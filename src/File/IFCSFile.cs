using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FCS.Property;

namespace FCS.File
{
    public abstract class IFCSFile
    {
        #region 读取文件
        #region 解析
        /// <summary>
        /// 读取文件流中的字节
        /// </summary>
        /// <param name="stream">可读取的流</param>
        /// <param name="fileBeginOffset">数据集的起点位置，相对于流起点的位置</param>
        /// <param name="readBegin">需要读取的数据的起点，相对于数据集起点</param>
        /// <param name="readEnd">需要读取的数据的终点，相对于数据集起点</param>
        /// <returns></returns>
        protected virtual byte[] ReadBytes(Stream stream, long fileBeginOffset, long readBegin, long readEnd)
        {
            if (stream.Length < fileBeginOffset + readEnd) throw new Exception("Stream length is not enough");
            long length = readEnd - readBegin + 1;
            byte[] bytes = new byte[length];
            stream.Seek(fileBeginOffset + readBegin, SeekOrigin.Begin);
            if (length > int.MaxValue)
            {
                long temp = length;
                while (temp > 0)
                {
                    byte[] tempbyte = new byte[(temp > int.MaxValue ? int.MaxValue : Convert.ToInt32(temp))];
                    if (stream.Read(tempbyte, 0, tempbyte.Length) != tempbyte.Length) throw new Exception("Stream read failed");
                    tempbyte.CopyTo(bytes, length - temp);
                    temp -= tempbyte.Length;
                }
            }
            else if (stream.Read(bytes, 0, Convert.ToInt32(length)) != length) throw new Exception("Stream read failed");
            return bytes;
        }
        /// <summary>
        /// 解析文本段，补充文本端，解析端等key-value形式的数据
        /// </summary>
        /// <param name="bytes">要解析的字节</param>
        /// <param name="keyValues">结果写入对象</param>
        /// <param name="spirtByte">分隔符</param>
        /// <param name="encoding">编码</param>
        protected virtual void AnalyseKeyValue(byte[] bytes, Dictionary<string, string> keyValues, byte spirtByte, Encoding encoding)
        {
            if (bytes == null || bytes.Length <= 0) throw new Exception("Data is null or empty,can't analyse to segment");
            if (keyValues == null) throw new Exception("Segment is null,can't edit");
            List<byte> key = new List<byte>();
            List<byte> value = new List<byte>();
            bool keyByte = false;
            byte lastByte = 0xff;//前一个字节
            for (long i = 0; i < bytes.LongLength; i++)
            {
                byte b = bytes[i];
                if (i == 0L)
                {
                    if (b != spirtByte) throw new Exception("Spirt byte error,can't analyse");//第一个字节不是分隔符，则表示解析失败，字节返回
                    keyByte = true;
                    continue;
                }
                if (b == spirtByte)
                {
                    keyByte = !keyByte;
                    if (lastByte == spirtByte)//如果前一个字节也是分隔字节，那么该字节是内容字节
                    {
                        if (keyByte) key.Add(spirtByte);
                        else value.Add(spirtByte);
                        lastByte = 0xff;
                        continue;
                    }
                    lastByte = b;
                    continue;
                }
                else if (lastByte == spirtByte && keyByte)
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
        }
        /// <summary>
        /// 使用UTF-8编码解析文本段，补充文本端，解析端等key-value形式的数据
        /// </summary>
        /// <param name="bytes">要解析的字节</param>
        /// <param name="keyValues">结果写入对象</param>
        /// <param name="spirtByte">分隔符</param>
        protected virtual void AnalyseUTF8KeyValue(byte[] bytes, Dictionary<string, string> keyValues, byte spirtByte)
        {
            AnalyseKeyValue(bytes, keyValues, spirtByte, Encoding.UTF8);
        }
        /// <summary>
        /// 读取并解析header段和文本段的分隔符
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileBeginOffset">数据集起始位置</param>
        /// <param name="parameter">参数</param>
        protected virtual void ReadHead(Stream stream, long fileBeginOffset, FCSFileParameter parameter)
        {
            byte[] headerBytes = new byte[58];
            stream.Seek(fileBeginOffset, SeekOrigin.Begin);
            if (stream.Read(headerBytes, 0, 58) == 58)
            {
                string headerString = Encoding.ASCII.GetString(headerBytes);
                parameter.Version = headerString.Substring(0, 6);
                parameter.TextBegin = Convert.ToInt64(headerString.Substring(10, 8));
                parameter.TextEnd = Convert.ToInt64(headerString.Substring(18, 8));
                parameter.DataBegin = Convert.ToInt64(headerString.Substring(26, 8));
                parameter.DataEnd = Convert.ToInt64(headerString.Substring(34, 8));
                parameter.AnalysisBegin = Convert.ToInt64(headerString.Substring(42, 8));
                parameter.AnalysisEnd = Convert.ToInt64(headerString.Substring(50, 8));
                if (stream.Length >= fileBeginOffset + parameter.TextBegin + 1)
                {
                    var spirt = ReadBytes(stream, fileBeginOffset, parameter.TextBegin, parameter.TextBegin);
                    if (spirt != null && spirt.Length == 1) parameter.SpirtByte = spirt[0];
                }
                else throw new Exception("Read Spirt byte failed,stream length is not enough");
            }
            else throw new Exception("Read head failed,stream length is not enough");
        }
        /// <summary>
        /// 从文本段填充文件参数
        /// </summary>
        /// <param name="keyValues">文本段字典</param>
        /// <param name="parameter">需要完善的参数</param>
        protected virtual void FillParameterFromTextSegment(Dictionary<string, string> keyValues, FCSFileParameter parameter)
        {
            if (keyValues == null) throw new Exception("Text segment is null,can't analyse and fill parameter");
            if (parameter == null) throw new Exception("FCS parameter is null,can't edit");
            if (keyValues.ContainsKey(Keys.BeginAnalysisKey) && long.TryParse(keyValues[Keys.BeginAnalysisKey], out long beginAnalysis)) parameter.AnalysisBegin = beginAnalysis;
            if (keyValues.ContainsKey(Keys.EndAnalysisKey) && long.TryParse(keyValues[Keys.EndAnalysisKey], out long endAnalysis)) parameter.AnalysisEnd = endAnalysis;
            if (keyValues.ContainsKey(Keys.BeginDataKey) && long.TryParse(keyValues[Keys.BeginDataKey], out long beginData)) parameter.DataBegin = beginData;
            if (keyValues.ContainsKey(Keys.EndDataKey) && long.TryParse(keyValues[Keys.EndDataKey], out long endData)) parameter.DataEnd = endData;
            if (keyValues.ContainsKey(Keys.BeginSTextKey) && long.TryParse(keyValues[Keys.BeginSTextKey], out long beginSText)) parameter.STextBegin = beginSText;
            if (keyValues.ContainsKey(Keys.EndSTextKey) && long.TryParse(keyValues[Keys.EndSTextKey], out long endSText)) parameter.STextEnd = endSText;
            if (keyValues.ContainsKey(Keys.ByteOrdKey)) parameter.ByteOrd = ByteOrderConvert.ConvertToEnum(keyValues[Keys.ByteOrdKey]);
            if (keyValues.ContainsKey(Keys.DataTypeKey) && keyValues[Keys.DataTypeKey].Length == 1) parameter.DataType = DataTypeConvert.ConvertToEnum(keyValues[Keys.DataTypeKey][0]);
            if (keyValues.ContainsKey(Keys.NextDataKey) && long.TryParse(keyValues[Keys.NextDataKey], out long nextData)) parameter.NextData = nextData;
            if (keyValues.ContainsKey(Keys.PARKey) && uint.TryParse(keyValues[Keys.PARKey], out uint par)) parameter.PAR = par;
            if (keyValues.ContainsKey(Keys.TOTKey) && ulong.TryParse(keyValues[Keys.TOTKey], out ulong tot)) parameter.TOT = tot;
        }
        /// <summary>
        /// 解析数据集的通道参数
        /// </summary>
        /// <param name="textSegment">文本段key-value集合</param>
        /// <param name="par">通道数量</param>
        /// <param name="defaultDataType">默认数据类型</param>
        protected virtual IList<Measurement> AnalyseParams(Dictionary<string, string> textSegment, uint par, DataType defaultDataType)
        {
            if (textSegment == null) throw new Exception("Text segment is null,can't analyse to measurement");
            if (par <= 0) throw new Exception("PAR can't be zero");
            var measurements = new List<Measurement>();
            for (uint i = 1; i <= par; i++)
            {
                Measurement param = new Measurement();
                var pnn = string.Format(Keys.PnNKey, i);
                if (textSegment.ContainsKey(pnn)) param.PnN = textSegment[pnn];
                var pnb = string.Format(Keys.PnBKey, i);
                if (textSegment.ContainsKey(pnb) && uint.TryParse(textSegment[pnb], out uint pnbo)) param.PnB = pnbo;
                var pne = string.Format(Keys.PnEKey, i);
                if (textSegment.ContainsKey(pne)) param.PnE = new Amplification(textSegment[pne]);
                var pnr = string.Format(Keys.PnRKey, i);
                if (textSegment.ContainsKey(pnr) && ulong.TryParse(textSegment[pnr], out ulong pnro)) param.PnR = pnro;
                var pnd = string.Format(Keys.PnDKey, i);
                if (textSegment.ContainsKey(pnd)) param.PnD = new RecommendsVisualizationScale(textSegment[pnd]);
                var pnf = string.Format(Keys.PnFKey, i);
                if (textSegment.ContainsKey(pnf)) param.PnF = textSegment[pnf];
                var png = string.Format(Keys.PnGKey, i);
                if (textSegment.ContainsKey(png) && double.TryParse(textSegment[png], out double pngo)) param.PnG = pngo;
                var pnl = string.Format(Keys.PnLKey, i);
                if (textSegment.ContainsKey(pnl)) param.PnL = textSegment[pnl];
                var pno = string.Format(Keys.PnOKey, i);
                if (textSegment.ContainsKey(pno) && uint.TryParse(textSegment[pno], out uint pnoo)) param.PnO = pnoo;
                var pns = string.Format(Keys.PnSKey, i);
                if (textSegment.ContainsKey(pns)) param.PnS = textSegment[pns];
                var pnt = string.Format(Keys.PnTKey, i);
                if (textSegment.ContainsKey(pnt)) param.PnT = textSegment[pnt];
                var pnv = string.Format(Keys.PnVKey, i);
                if (textSegment.ContainsKey(pnv) && double.TryParse(textSegment[pnv], out double pnvo)) param.PnV = pnvo;
                var pndatatype = string.Format(Keys.PnDataTypeKey, i);
                if (textSegment.ContainsKey(pndatatype) && textSegment[pndatatype].Length == 1) param.PnDATATYPE = DataTypeConvert.ConvertToEnum(textSegment[pndatatype][0]);
                else param.PnDATATYPE = defaultDataType;
                measurements.Add(param);
            }
            return measurements;
        }
        /// <summary>
        /// 解析数据段
        /// </summary>
        /// <param name="stream">可读取的流</param>
        /// <param name="fileBeginOffset">此数据集相对于文件起点的位置</param>
        /// <param name="dataBegin">数据段开始位置，相对于数据集起点位置</param>
        /// <param name="dataEnd">数据段结束位置，相对于数据集起点位置</param>
        /// <param name="measurements">通道集合</param>
        /// <param name="tot">每个通道的数据量</param>
        /// <param name="dataType">数据集的数据类型,整个数据集的默认数据类型</param>
        /// <param name="byteOrd">字节排序方式</param>
        protected virtual void AnalyseData(Stream stream, long fileBeginOffset, long dataBegin, long dataEnd, IList<Measurement> measurements, ulong tot, DataType dataType, ByteOrd byteOrd)
        {
            if (measurements == null) throw new Exception("Measurement list object is null");
            if (dataType == DataType.Unknown) throw new Exception("FCS datatype is not supported");
            if (stream.Length < (fileBeginOffset + dataEnd)) throw new Exception("Stream length is not enough");
            stream.Seek(fileBeginOffset + dataBegin, SeekOrigin.Begin);
            for (ulong i = 0; i < tot; i++)
            {
                foreach (var item in measurements)
                {
                    byte[] bytes;
                    int length = 0;
                    if (dataType == DataType.I) length = Convert.ToInt32(item.PnB / 8);
                    else if (dataType == DataType.F) length = 4;
                    else if (dataType == DataType.D) length = 8;
                    bytes = new byte[length];
                    if (length != stream.Read(bytes, 0, length)) throw new Exception("Read data segment failed,stream length is not enough");
                    item.AddOneValue(bytes, byteOrd);
                }
            }
        }
        #endregion

        /// <summary>
        /// 读取一个数据集
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="nextData">返回下一个数据集的起点</param>
        /// <param name="fileBeginOffset">数据集起始位,相对于流的起始位置</param>
        /// <returns></returns>
        public virtual FCS ReadDataset(Stream stream, out long nextData, long fileBeginOffset = 0)
        {
            if (!stream.CanRead || !stream.CanSeek) throw new Exception("Stream can't read or seek");
            if (fileBeginOffset > stream.Length) throw new Exception("Offset is too big");
            FCS fcs = new FCS();
            FCSFileParameter parameter = new FCSFileParameter();
            ReadHead(stream, fileBeginOffset, parameter);
            AnalyseUTF8KeyValue(ReadBytes(stream, fileBeginOffset, parameter.TextBegin, parameter.TextEnd), fcs.TextSegment, parameter.SpirtByte);
            FillParameterFromTextSegment(fcs.TextSegment, parameter);
            if (parameter.STextBegin != 0 && parameter.STextEnd != 0) AnalyseUTF8KeyValue(ReadBytes(stream, fileBeginOffset, parameter.STextBegin, parameter.STextEnd), fcs.TextSegment, parameter.SpirtByte);
            if (parameter.AnalysisBegin != 0 && parameter.AnalysisEnd != 0) AnalyseUTF8KeyValue(ReadBytes(stream, fileBeginOffset, parameter.AnalysisBegin, parameter.AnalysisEnd), fcs.AnalysisSegment, parameter.SpirtByte);
            fcs.Measurements = AnalyseParams(fcs.TextSegment, parameter.PAR, parameter.DataType);
            AnalyseData(stream, fileBeginOffset, parameter.DataBegin, parameter.DataEnd, fcs.Measurements, parameter.TOT, parameter.DataType, parameter.ByteOrd);
            nextData = parameter.NextData;
            return fcs;
        }
        /// <summary>
        /// 读取所有数据集
        /// </summary>
        /// <param name="stream">可读取的流</param>
        /// <returns></returns>
        public virtual IEnumerable<FCS> Read(Stream stream)
        {
            if (!stream.CanRead || !stream.CanSeek) throw new Exception("Stream can't read or seek");
            var temp = ReadDataset(stream, out long nextData, 0);
            if (temp == null || temp.Measurements == null) throw new Exception("Stream analyse failed");
            List<FCS> fcslist = new List<FCS> { temp };
            long nextfromfilebegin = 0;
            while (nextData != 0)
            {
                nextfromfilebegin += nextData;
                temp = ReadDataset(stream, out nextData, nextfromfilebegin);
                if (temp != null && temp.Measurements != null) fcslist.Add(temp);
                else break;
            }
            return fcslist;
        }
        #endregion

        #region 保存文件
        /// <summary>
        /// 保存一组数据集到文件
        /// </summary>
        /// <param name="stream">可写流</param>
        /// <param name="list">数据集数组</param>
        /// <returns></returns>
        public abstract bool Save(Stream stream, params FCS[] list);

        #endregion
    }

}
