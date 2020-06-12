using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FCS.Property;

namespace FCS
{
    public abstract class IFCS
    {
        #region property

        #region 文件必须
        /// <summary>
        /// 文件版本
        /// </summary>
        public string Version { get; protected set; }
        /// <summary>
        /// 文本段起始位置
        /// </summary>
        protected long TextBegin { get; set; }
        /// <summary>
        /// 文本段结束位置
        /// </summary>
        protected long TextEnd { get; set; }
        /// <summary>
        /// 数据段起始位置
        /// </summary>
        protected long DataBegin { get; set; }
        /// <summary>
        /// 数据段结束位置
        /// </summary>
        protected long DataEnd { get; set; }
        /// <summary>
        /// 分析段起始位置
        /// </summary>
        protected long AnalysisBegin { get; set; }
        /// <summary>
        /// 分析段结束位置
        /// </summary>
        protected long AnalysisEnd { get; set; }
        /// <summary>
        /// 补充文本段起始位置
        /// </summary>
        protected long STextBegin { get; set; }
        /// <summary>
        /// 补充文本段结束位置
        /// </summary>
        protected long STextEnd { get; set; }
        /// <summary>
        /// 数据采集计算机的字节顺序
        /// </summary>
        protected FCSByteOrd ByteOrd { get; set; } = FCSByteOrd.Unknown;
        /// <summary>
        /// DATA 段的数据类型 (ASCII, integer, floating point)
        /// </summary>
        protected FCSDataType DataType { get; set; } = FCSDataType.Unknown;
        /// <summary>
        /// 数据模式 (list mode – 推介, histogram – 废弃)
        /// </summary>
        protected FCSMode Mode { get; set; } = FCSMode.Unknown;
        /// <summary>
        /// 一个细胞中的参数个数（Number of parameters in an event.）
        /// </summary>
        public uint PAR { get; private set; }
        /// <summary>
        /// 数据集中总细胞个数
        /// </summary>
        public ulong TOT { get; protected set; }
        /// <summary>
        /// 下一个数据集
        /// </summary>
        public long NextData { get; protected set; }
        #endregion

        /// <summary>
        /// 文本段数据,含补充文本段
        /// </summary>
        public Dictionary<string, string> TextSegment { get; set; }
        /// <summary>
        /// 分析段数据
        /// </summary>
        public Dictionary<string, string> AnalysisSegment { get; set; }
        /// <summary>
        /// 数据段数据
        /// </summary>
        public IEnumerable<IList> DataSegment { get; set; }
        /// <summary>
        /// 参数集合
        /// </summary>
        public IList<Param> Params { get; set; }

        /// <summary>
        /// header段起始的位置
        /// </summary>
        protected long BeginOffSet { get; set; }
        /// <summary>
        /// 间隔字节
        /// </summary>
        protected byte SpirtByte { get; set; }
        #endregion

        public IFCS(FileStream fileStream, long offset = 0)
        {
            BeginOffSet = offset;
            ReadHeader(fileStream);
            ReadText(fileStream);
            AnalysisParams(TextSegment);
            ReadData(fileStream);
        }

        /// <summary>
        /// 读取并解析header段
        /// </summary>
        /// <param name="fileStream"></param>
        protected virtual void ReadHeader(FileStream fileStream)
        {
            byte[] headerBytes = new byte[58];
            fileStream.Seek(BeginOffSet, SeekOrigin.Begin);
            if (fileStream.Read(headerBytes, 0, 58) == 58)
            {
                string headerString = Encoding.ASCII.GetString(headerBytes);
                Version = headerString.Substring(0, 6);
                TextBegin = Convert.ToInt64(headerString.Substring(10, 8));
                TextEnd = Convert.ToInt64(headerString.Substring(18, 8));
                DataBegin = Convert.ToInt64(headerString.Substring(26, 8));
                DataEnd = Convert.ToInt64(headerString.Substring(34, 8));
                AnalysisBegin = Convert.ToInt64(headerString.Substring(42, 8));
                AnalysisEnd = Convert.ToInt64(headerString.Substring(50, 8));
            }
            if (fileStream.Length >= BeginOffSet + TextBegin + 1)
            {
                var spirt = ReadBytes(fileStream, TextBegin, TextBegin);
                if (spirt != null && spirt.Length == 1)
                    SpirtByte = spirt[0];
            }
        }

        /// <summary>
        /// 读取并解析文本段、补充文本段、分析段
        /// </summary>
        /// <param name="fileStream"></param>
        protected virtual void ReadText(FileStream fileStream)
        {
            TextSegment = ReadKeyValue(ReadBytes(fileStream, TextBegin, TextEnd));
            AnalysisTextSegmentMust(TextSegment);
            if (TextSegment == null) TextSegment = new Dictionary<string, string>();
            if (STextBegin != 0 && STextEnd != 0)
            {
                var stext = ReadKeyValue(ReadBytes(fileStream, STextBegin, STextEnd));
                if (stext != null)
                    foreach (var item in stext)
                        TextSegment[item.Key] = item.Value;
            }
            if (AnalysisBegin != 0 && AnalysisEnd != 0)
            {
                AnalysisSegment = ReadKeyValue(ReadBytes(fileStream, AnalysisBegin, AnalysisEnd));
            }
        }

        /// <summary>
        /// 解析文本段中必须的字段
        /// </summary>
        /// <param name="keyValues"></param>
        private void AnalysisTextSegmentMust(Dictionary<string, string> keyValues)
        {
            if (keyValues == null) return;
            if (keyValues.ContainsKey(Keys.BeginAnalysisKey) && keyValues[Keys.BeginAnalysisKey] != null) AnalysisBegin = Convert.ToInt64(keyValues[Keys.BeginAnalysisKey]);
            if (keyValues.ContainsKey(Keys.EndAnalysisKey) && TextSegment[Keys.EndAnalysisKey] != null) AnalysisEnd = Convert.ToInt64(keyValues[Keys.EndAnalysisKey]);
            if (keyValues.ContainsKey(Keys.BeginDataKey) && TextSegment[Keys.BeginDataKey] != null) DataBegin = Convert.ToInt64(keyValues[Keys.BeginDataKey]);
            if (keyValues.ContainsKey(Keys.EndDataKey) && TextSegment[Keys.EndDataKey] != null) DataEnd = Convert.ToInt64(keyValues[Keys.EndDataKey]);
            if (keyValues.ContainsKey(Keys.BeginSTextKey) && TextSegment[Keys.BeginSTextKey] != null) STextBegin = Convert.ToInt64(keyValues[Keys.BeginSTextKey]);
            if (keyValues.ContainsKey(Keys.EndSTextKey) && TextSegment[Keys.EndSTextKey] != null) STextEnd = Convert.ToInt64(keyValues[Keys.EndSTextKey]);
            if (keyValues.ContainsKey(Keys.ByteOrdKey)) ByteOrd = FCSByteOrderConvert.ConvertToEnum(keyValues[Keys.ByteOrdKey]);
            if (keyValues.ContainsKey(Keys.DataTypeKey)) DataType = FCSDataTypeConvert.ConvertToEnum(keyValues[Keys.DataTypeKey]);
            if (keyValues.ContainsKey(Keys.ModeKey)) Mode = FCSModeConvert.ConvertToEnum(keyValues[Keys.ModeKey]);
            if (keyValues.ContainsKey(Keys.NextDataKey) && keyValues[Keys.NextDataKey] != null) NextData = Convert.ToInt64(keyValues[Keys.NextDataKey]);
            if (keyValues.ContainsKey(Keys.PARKey) && keyValues[Keys.PARKey] != null) PAR = Convert.ToUInt32(keyValues[Keys.PARKey]);
            if (keyValues.ContainsKey(Keys.TOTKey) && keyValues[Keys.TOTKey] != null) TOT = Convert.ToUInt64(keyValues[Keys.TOTKey]);
        }

        /// <summary>
        /// 解析参数属性
        /// </summary>
        /// <param name="keyValues"></param>
        private void AnalysisParams(Dictionary<string, string> keyValues)
        {
            var paramlist = new List<Param>();
            for (uint i = 1; i <= PAR; i++)
            {
                Param param = new Param();
                var pnn = string.Format(Keys.PnNKey, i);
                if (keyValues.ContainsKey(pnn)) param.PnN = keyValues[pnn];
                var pnb = string.Format(Keys.PnBKey, i);
                if (keyValues.ContainsKey(pnb) && uint.TryParse(keyValues[pnb], out uint pnbo)) param.PnB = pnbo;
                var pne = string.Format(Keys.PnEKey, i);
                if (keyValues.ContainsKey(pne)) param.PnE = keyValues[pne];
                var pnr = string.Format(Keys.PnRKey, i);
                if (keyValues.ContainsKey(pnr) && uint.TryParse(keyValues[pnr], out uint pnro)) param.PnR = pnro;
                var pnd = string.Format(Keys.PnDKey, i);
                if (keyValues.ContainsKey(pnd)) param.PnD = keyValues[pnd];
                var pnf = string.Format(Keys.PnFKey, i);
                if (keyValues.ContainsKey(pnf)) param.PnF = keyValues[pnf];
                var png = string.Format(Keys.PnGKey, i);
                if (keyValues.ContainsKey(png) && double.TryParse(keyValues[png], out double pngo)) param.PnG = pngo;
                var pnl = string.Format(Keys.PnLKey, i);
                if (keyValues.ContainsKey(pnl)) param.PnL = keyValues[pnl];
                var pno = string.Format(Keys.PnOKey, i);
                if (keyValues.ContainsKey(pno) && uint.TryParse(keyValues[pno], out uint pnoo)) param.PnO = pnoo;
                var pnp = string.Format(Keys.PnPKey, i);
                if (keyValues.ContainsKey(pnp) && uint.TryParse(keyValues[pnp], out uint pnpo)) param.PnP = pnpo;
                var pns = string.Format(Keys.PnSKey, i);
                if (keyValues.ContainsKey(pns)) param.PnS = keyValues[pns];
                var pnt = string.Format(Keys.PnTKey, i);
                if (keyValues.ContainsKey(pnt)) param.PnT = keyValues[pnt];
                var pnv = string.Format(Keys.PnVKey, i);
                if (keyValues.ContainsKey(pnv) && double.TryParse(keyValues[pnv], out double pnvo)) param.PnV = pnvo;
                paramlist.Add(param);
            }
            Params = paramlist;
        }

        /// <summary>
        /// 读取数据段并解析
        /// </summary>
        /// <param name="fileStream"></param>
        protected virtual void ReadData(FileStream fileStream)
        {
            var bytes = ReadBytes(fileStream, DataBegin, DataEnd);
            DataSegment = AnalysisData(bytes, DataType, Mode, ByteOrd, Params, TOT);
        }

        #region 解析数据段
        /// <summary>
        /// 解析数据段
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="dataType"></param>
        /// <param name="mode"></param>
        /// <param name="byteOrd"></param>
        /// <param name="paramList"></param>
        /// <param name="tot"></param>
        /// <returns></returns>
        protected virtual IEnumerable<IList> AnalysisData(byte[] bytes, FCSDataType dataType, FCSMode mode, FCSByteOrd byteOrd, IList<Param> paramList, ulong tot)
        {
            return mode switch
            {
                FCSMode.Unknown => null,
                FCSMode.L => AnalysisListData(bytes, dataType, byteOrd, paramList, tot),
                _ => null,
            };
        }
        /// <summary>
        /// 解析列表数据段
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="dataType"></param>
        /// <param name="byteOrd"></param>
        /// <param name="paramList"></param>
        /// <param name="tot"></param>
        /// <returns></returns>
        private IEnumerable<IList> AnalysisListData(byte[] bytes, FCSDataType dataType, FCSByteOrd byteOrd, IList<Param> paramList, ulong tot)
        {
            return dataType switch
            {
                FCSDataType.Unknown => null,
                FCSDataType.I => AnalysisIntegerListData(bytes, byteOrd, paramList, tot),
                FCSDataType.F => AnalysisFloatListData(bytes, byteOrd, paramList, tot),
                FCSDataType.D => AnalysisDoubleListData(bytes, byteOrd, paramList, tot),
                _ => null,
            };
        }
        /// <summary>
        /// 解析列表整数数据段
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="byteOrd"></param>
        /// <param name="paramList"></param>
        /// <param name="tot"></param>
        /// <returns></returns>
        protected virtual IEnumerable<IList> AnalysisIntegerListData(byte[] bytes, FCSByteOrd byteOrd, IList<Param> paramList, ulong tot)
        {
            List<List<double>> datas = new List<List<double>>();
            if (paramList.FirstOrDefault(p => p.PnB % 8 != 0) != null)
            {
                ulong index = 0;
                for (ulong i = 0; i < tot; i++)
                {
                    List<double> itemdatas = new List<double>();
                    foreach (var param in paramList)
                    {
                        var byteindex = index / 8;
                        var bitindex = Convert.ToInt32(index % 8);
                        List<byte> bs = new List<byte>();
                        for (int x = 0; x < param.PnB; x += 8)
                        {
                            bs.Add(bytes[byteindex]);
                            byteindex++;
                        }
                        if (bs.Count > 0)
                        {
                            bs[0] = Convert.ToByte((bs[0] << bitindex) >> bitindex);
                            if ((index + param.PnB) % 8 != 0)
                            {
                                bs[bs.Count - 1] = Convert.ToByte((bs[0] >> (8 - bitindex)) << (8 - bitindex));
                            }
                            var temp = BitConverter.ToUInt64(bs.ToArray());
                            if ((index + param.PnB) % 8 != 0) temp /= Convert.ToUInt16(Math.Pow(2, 8 - bitindex));
                            double v;
                            if (double.IsNaN(param.PnG))
                                v = param.AmplificationCalculation(param.BitMask(temp));//PnB PnR位掩码计算,PnE放大计算
                            else
                                v = param.LineEnlargeCalculation(param.BitMask(temp));//PnB PnR位掩码计算,PnG放大计算
                            itemdatas.Add(v);
                        }
                        else itemdatas.Add(0);
                        index += param.PnB;
                    }
                    datas.Add(itemdatas);
                }
            }
            else
            {
                var bytelength = Convert.ToUInt64(paramList.Sum(p => p.PnB / 8));
                var pcount = Convert.ToUInt32(paramList.Count());
                for (ulong i = 0; i < tot; i++)
                {
                    List<double> itemdatas = new List<double>();
                    uint byteindex = 0;
                    foreach (var param in paramList)
                    {
                        var index = i * bytelength + byteindex;
                        List<byte> bs = new List<byte>();
                        for (ulong j = 0; j < param.PnB / 8; j++)
                        {
                            bs.Add(bytes[j + index]);
                        }
                        switch (byteOrd)
                        {
                            case FCSByteOrd.BigEndian:
                                bs.Reverse();
                                break;
                            case FCSByteOrd.Unknown:
                            case FCSByteOrd.LittleEndian:
                            default:
                                break;
                        }
                        double v;
                        if (double.IsNaN(param.PnG))
                            v = param.AmplificationCalculation(param.BitMask(BitConverter.ToUInt64(bs.ToArray())));//PnB PnR位掩码计算,PnE放大计算
                        else
                            v = param.LineEnlargeCalculation(param.BitMask(BitConverter.ToUInt64(bs.ToArray())));//PnB PnR位掩码计算,PnG放大计算
                        itemdatas.Add(v);
                        byteindex += param.PnB / 8;
                    }
                    datas.Add(itemdatas);
                }
            }
            return datas;
        }
        /// <summary>
        /// 解析列表单精度浮点数据段
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="byteOrd"></param>
        /// <param name="paramList"></param>
        /// <param name="tot"></param>
        /// <returns></returns>
        protected virtual IEnumerable<IList> AnalysisFloatListData(byte[] bytes, FCSByteOrd byteOrd, IList<Param> paramList, ulong tot)
        {
            List<List<float>> datas = new List<List<float>>();
            var pcount = Convert.ToUInt32(paramList.Count());
            for (ulong i = 0; i < tot; i++)
            {
                List<float> itemdatas = new List<float>();
                uint j = 0;
                foreach (var param in paramList)
                {
                    var index = (i * pcount + j) * 4;
                    float f = 0;
                    switch (byteOrd)
                    {
                        case FCSByteOrd.Unknown:
                        case FCSByteOrd.LittleEndian:
                            f = BitConverter.ToSingle(new byte[] { bytes[index], bytes[index + 1], bytes[index + 2], bytes[index + 3] });
                            break;
                        case FCSByteOrd.BigEndian:
                            f = BitConverter.ToSingle(new byte[] { bytes[index + 3], bytes[index + 2], bytes[index + 1], bytes[index] });
                            break;
                        default:
                            break;
                    }
                    if (!double.IsNaN(param.PnG)) f = Convert.ToSingle(f / param.PnG);//增益                    
                    itemdatas.Add(f);
                    j++;
                }
                datas.Add(itemdatas);
            }
            return datas;
        }
        /// <summary>
        /// 解析列表双精度浮点数据段
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="byteOrd"></param>
        /// <param name="paramList"></param>
        /// <param name="tot"></param>
        /// <returns></returns>
        protected virtual IEnumerable<IList> AnalysisDoubleListData(byte[] bytes, FCSByteOrd byteOrd, IList<Param> paramList, ulong tot)
        {
            List<List<double>> datas = new List<List<double>>();
            var pcount = Convert.ToUInt32(paramList.Count());
            for (ulong i = 0; i < tot; i++)
            {
                List<double> itemdatas = new List<double>();
                uint j = 0;
                foreach (var param in paramList)
                {
                    var index = (i * pcount + j) * 8;
                    double d = 0;
                    switch (byteOrd)
                    {
                        case FCSByteOrd.Unknown:
                        case FCSByteOrd.LittleEndian:
                            d = BitConverter.ToDouble(new byte[] { bytes[index], bytes[index + 1], bytes[index + 2], bytes[index + 3], bytes[index + 4], bytes[index + 5], bytes[index + 6], bytes[index + 7] });
                            break;
                        case FCSByteOrd.BigEndian:
                            d = BitConverter.ToDouble(new byte[] { bytes[index + 7], bytes[index + 6], bytes[index + 5], bytes[index + 4], bytes[index + 3], bytes[index + 2], bytes[index + 1], bytes[index] });
                            break;
                        default:
                            break;
                    }
                    if (!double.IsNaN(param.PnG)) d /= param.PnG;//增益
                    itemdatas.Add(d);
                    j++;
                }
                datas.Add(itemdatas);
            }
            return datas;
        }
        #endregion

        #region 读取操作
        /// <summary>
        /// 解析fcs文件的key/value形式的段，包括文本段，补充文本段，分析段
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        protected virtual Dictionary<string, string> ReadKeyValue(byte[] bytes)
        {
            return ReadKeyValue(bytes, Encoding.UTF8);
        }
        /// <summary>
        /// 解析fcs文件的key/value形式的段，包括文本段，补充文本段，分析段
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="encoding">value值编码格式</param>
        protected virtual Dictionary<string, string> ReadKeyValue(byte[] bytes, Encoding encoding)
        {
            if (bytes == null || bytes.Length <= 0 || encoding == null) return null;
            List<byte> key = new List<byte>();
            List<byte> value = new List<byte>();
            bool keyByte = false;
            byte lastByte = 0xff;//前一个字节
            var keyValues = new Dictionary<string, string>();
            for (long i = 0; i < bytes.LongLength; i++)
            {
                byte b = bytes[i];
                if (i == 0L)
                {
                    if (b != SpirtByte) return null;
                    keyByte = true;
                    continue;
                }
                if (b == SpirtByte)
                {
                    keyByte = !keyByte;
                    if (lastByte == SpirtByte)//如果前一个字节也是分隔字节，那么该字节是内容字节
                    {
                        if (keyByte) key.Add(SpirtByte);
                        else value.Add(SpirtByte);
                        lastByte = 0xff;
                        continue;
                    }
                    lastByte = b;
                    continue;
                }
                else if (lastByte == SpirtByte && keyByte)
                {
                    keyValues.Add(Encoding.ASCII.GetString(key.ToArray()).ToUpper(), encoding.GetString(value.ToArray()));
                    key.Clear();
                    value.Clear();
                }
                lastByte = b;
                if (keyByte) key.Add(b);
                else value.Add(b);
            }
            keyValues.Add(Encoding.ASCII.GetString(key.ToArray()).ToUpper(), encoding.GetString(value.ToArray()));
            return keyValues;
        }
        /// <summary>
        /// 读取文件流中的字节
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="readBegin"></param>
        /// <param name="readEnd"></param>
        /// <returns></returns>
        protected virtual byte[] ReadBytes(FileStream fileStream, long readBegin, long readEnd)
        {
            if (fileStream.Length < readEnd) return null;
            long length = readEnd - readBegin + 1;
            byte[] bytes = new byte[length];
            fileStream.Seek(BeginOffSet + readBegin, SeekOrigin.Begin);
            if (length > int.MaxValue)
            {
                long temp = length;
                while (temp > 0)
                {
                    byte[] tempbyte = new byte[(temp > int.MaxValue ? int.MaxValue : Convert.ToInt32(temp))];
                    if (fileStream.Read(tempbyte, 0, tempbyte.Length) != tempbyte.Length) return null;
                    tempbyte.CopyTo(bytes, length - temp);
                    temp -= tempbyte.Length;
                }
            }
            else if (fileStream.Read(bytes, 0, Convert.ToInt32(length)) != length) return null;
            return bytes;
        }
        #endregion
    }




}
