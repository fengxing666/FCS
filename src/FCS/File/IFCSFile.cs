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
        /// <param name="delimiterByte">分隔符</param>
        /// <param name="encoding">编码</param>
        protected virtual void AnalyseKeyValue(byte[] bytes, Dictionary<string, string> keyValues, byte delimiterByte, Encoding encoding)
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
                    if (b != delimiterByte) throw new Exception("Delimiter byte error,can't analyse");//第一个字节不是分隔符，则表示解析失败，字节返回
                    keyByte = true;
                    continue;
                }
                if (b == delimiterByte)
                {
                    keyByte = !keyByte;
                    if (lastByte == delimiterByte)//如果前一个字节也是分隔字节，那么该字节是内容字节
                    {
                        if (keyByte) key.Add(delimiterByte);
                        else value.Add(delimiterByte);
                        lastByte = 0xff;
                        continue;
                    }
                    lastByte = b;
                    continue;
                }
                else if (lastByte == delimiterByte && keyByte)
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
        /// <param name="delimiterByte">分隔符</param>
        protected virtual void AnalyseUTF8KeyValue(byte[] bytes, Dictionary<string, string> keyValues, byte delimiterByte)
        {
            AnalyseKeyValue(bytes, keyValues, delimiterByte, Encoding.UTF8);
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
                    var delimiterByte = ReadBytes(stream, fileBeginOffset, parameter.TextBegin, parameter.TextBegin);
                    if (delimiterByte != null && delimiterByte.Length == 1) parameter.DelimiterByte = delimiterByte[0];
                }
                else throw new Exception("Read Delimiter byte failed,stream length is not enough");
            }
            else throw new Exception("Read head failed,stream length is not enough");
        }
        /// <summary>
        /// 从文本段填充数据集参数
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
            if (keyValues.ContainsKey(Keys.DataTypeKey)) parameter.DataType = DataTypeConvert.ConvertToEnum(keyValues[Keys.DataTypeKey]);
            if (keyValues.ContainsKey(Keys.NextDataKey) && long.TryParse(keyValues[Keys.NextDataKey], out long nextData)) parameter.NextData = nextData;
            if (keyValues.ContainsKey(Keys.PARKey) && uint.TryParse(keyValues[Keys.PARKey], out uint par)) parameter.PAR = par;
            if (keyValues.ContainsKey(Keys.TOTKey) && uint.TryParse(keyValues[Keys.TOTKey], out uint tot)) parameter.TOT = tot;
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
            uint pnbtemp = 0;
            switch (defaultDataType)
            {
                case DataType.F:
                    pnbtemp = 32;
                    break;
                case DataType.D:
                    pnbtemp = 64;
                    break;
                default:
                    break;
            }
            var measurements = new List<Measurement>();
            for (uint i = 1; i <= par; i++)
            {
                Measurement param = new Measurement();
                var pnn = string.Format(Keys.PnNKey, i);
                if (textSegment.ContainsKey(pnn)) param.PnN = textSegment[pnn];
                if (pnbtemp == 0)
                {
                    var pnb = string.Format(Keys.PnBKey, i);
                    if (textSegment.ContainsKey(pnb) && uint.TryParse(textSegment[pnb], out uint pnbo))
                    {
                        if (pnbo % 8 != 0) throw new Exception("PnB value that are not divisible by 8 are not support");
                        param.PnB = pnbo;
                    }
                }
                else { param.PnB = pnbtemp; }
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
        protected virtual void AnalyseData(Stream stream, long fileBeginOffset, long dataBegin, long dataEnd, IList<Measurement> measurements, uint tot, DataType dataType, ByteOrd byteOrd)
        {
            if (measurements == null) throw new Exception("Measurement list object is null");
            if (dataType == DataType.Unknown) throw new Exception("FCS datatype is not supported");
            if (stream.Length < (fileBeginOffset + dataEnd)) throw new Exception("Stream length is not enough");
            stream.Seek(fileBeginOffset + dataBegin, SeekOrigin.Begin);
            for (uint i = 0; i < tot; i++)
            {
                foreach (var item in measurements)
                {
                    byte[] bytes = new byte[item.PnBByteLength];
                    if (item.PnBByteLength != stream.Read(bytes, 0, item.PnBByteLength)) throw new Exception("Read data segment failed,stream length is not enough");
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
            if (fileBeginOffset > stream.Length) throw new Exception("Offset is too big");
            FCS fcs = new FCS();
            FCSFileParameter parameter = new FCSFileParameter();
            ReadHead(stream, fileBeginOffset, parameter);
            AnalyseUTF8KeyValue(ReadBytes(stream, fileBeginOffset, parameter.TextBegin, parameter.TextEnd), fcs.TextSegment, parameter.DelimiterByte);
            FillParameterFromTextSegment(fcs.TextSegment, parameter);
            if (parameter.STextBegin != 0 && parameter.STextEnd != 0) AnalyseUTF8KeyValue(ReadBytes(stream, fileBeginOffset, parameter.STextBegin, parameter.STextEnd), fcs.TextSegment, parameter.DelimiterByte);
            if (parameter.AnalysisBegin != 0 && parameter.AnalysisEnd != 0) AnalyseUTF8KeyValue(ReadBytes(stream, fileBeginOffset, parameter.AnalysisBegin, parameter.AnalysisEnd), fcs.AnalysisSegment, parameter.DelimiterByte);
            fcs.Measurements = AnalyseParams(fcs.TextSegment, parameter.PAR, parameter.DataType);
            AnalyseData(stream, fileBeginOffset, parameter.DataBegin, parameter.DataEnd, fcs.Measurements, parameter.TOT, parameter.DataType, parameter.ByteOrd);
            nextData = parameter.NextData;
            return fcs;
        }
        #endregion

        #region 保存文件
        #region 分段保存和拆分
        /// <summary>
        /// 向流中写入字节
        /// </summary>
        /// <param name="stream">可写的流</param>
        /// <param name="fileBeginOffset">数据集起始位置，相对于文件的起始位</param>
        /// <param name="writeBegin">写入位置，相对于数据集的起始位</param>
        /// <param name="data">要写入的数据</param>
        protected virtual void Write(Stream stream, long fileBeginOffset, long writeBegin, byte[] data)
        {
            if (!stream.CanWrite || !stream.CanSeek) throw new Exception("Stream can't write or seek");
            if ((fileBeginOffset + writeBegin) < 0) throw new Exception("Can't write,offset must be greater than zero");
            if (data == null || data.Length <= 0) throw new Exception("Can't write,data is null or empty");
            stream.Seek(fileBeginOffset + writeBegin, SeekOrigin.Begin);
            stream.Write(data, 0, data.Length);
        }
        /// <summary>
        /// 文本段、补充文本段、分析段等字典转字节数组，带分隔符
        /// </summary>
        /// <param name="keyValues">字典</param>
        /// <param name="delimiterByte">分隔符</param>
        /// <returns></returns>
        protected virtual MemoryStream DictionaryToStream(Dictionary<string, string> keyValues, byte delimiterByte)
        {
            if (keyValues == null || keyValues.Count <= 0) throw new Exception("Dictionary is null or empty");
            MemoryStream stream = new MemoryStream();
            stream.WriteByte(delimiterByte);
            foreach (var item in keyValues)
            {
                if (string.IsNullOrEmpty(item.Key) || string.IsNullOrEmpty(item.Value)) throw new Exception("Dictionary key and value can't be null or empty");
                var temp = KeyValueToByteArray(item.Key, item.Value, delimiterByte);
                stream.Write(temp.ToArray(), 0, temp.Count);
            }
            return stream;
        }
        /// <summary>
        /// key value字符串转文本段字节
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="delimiterByte"></param>
        /// <returns></returns>
        protected virtual List<byte> KeyValueToByteArray(string key, string value, byte delimiterByte)
        {
            string delimiterChar = Encoding.UTF8.GetString(new byte[] { delimiterByte });
            string delimiterDoubleChar = string.Concat(delimiterChar, delimiterChar);
            List<byte> bytes = new List<byte>();
            if (key.Contains(delimiterChar)) key.Replace(delimiterChar, delimiterDoubleChar);
            if (value.Contains(delimiterChar)) value.Replace(delimiterChar, delimiterDoubleChar);
            var keys = Encoding.UTF8.GetBytes(key);
            var values = Encoding.UTF8.GetBytes(value);
            bytes.AddRange(keys);
            bytes.Add(delimiterByte);
            bytes.AddRange(values);
            bytes.Add(delimiterByte);
            return bytes;
        }

        /// <summary>
        /// 解析数据段，转成字节数组
        /// </summary>
        /// <param name="measurements"></param>
        /// <returns></returns>
        protected virtual MemoryStream DataSegmentToStream(IList<Measurement> measurements)
        {
            if (measurements == null || measurements.Count <= 0) throw new Exception("Measurement array is null or empty");
            long allMeasurementBitLength = 0;
            foreach (var item in measurements) allMeasurementBitLength += item.PnB / 8;
            uint startOffsetForOne = 0;
            MemoryStream stream = null;
            for (int j = 0; j < measurements.Count; j++)
            {
                var item = measurements[j];
                var bytelength = item.PnB / 8;
                if (stream == null) stream = new MemoryStream(new byte[allMeasurementBitLength * item.Values.Count]);
                for (int i = 0; i < item.Values.Count; i++)
                {
                    var v = item.Values[i];
                    byte[] bytes;
                    if (v is double d) bytes = BitConverter.GetBytes(d);
                    else if (v is float f) bytes = BitConverter.GetBytes(f);
                    else if (v is ulong ul) bytes = BitConverter.GetBytes(ul);
                    else if (v is uint ui) bytes = BitConverter.GetBytes(ui);
                    else if (v is ushort us) bytes = BitConverter.GetBytes(us);
                    else if (v is byte b) bytes = BitConverter.GetBytes(b);
                    else throw new Exception("Data type not supported");
                    if (bytes.Length > bytelength) throw new Exception("Data type error,byte array is too larger");
                    byte[] targetbytes = new byte[bytelength];
                    bytes.CopyTo(targetbytes, bytelength - bytes.Length);
                    stream.Seek(i * allMeasurementBitLength + startOffsetForOne, SeekOrigin.Begin);
                    stream.Write(targetbytes, 0, targetbytes.Length);
                }
                startOffsetForOne += bytelength;
            }
            return stream;
        }
        /// <summary>
        /// 重置文本段中的一些参数，根据通道参数
        /// </summary>
        /// <param name="measurements">通道集合</param>
        /// <param name="textSegment">文本段字典</param>
        protected abstract void ResetTextSegment(Dictionary<string, string> textSegment, IList<Measurement> measurements);

        /// <summary>
        /// 拆分文本段，分成补充文本段和文本段。用于文本段过长时，无法用八位数字表达文本段位置
        /// </summary>
        /// <param name="keyValues">需要拆分的文本段</param>
        /// <return>新的文本段和补充文本段,第一个是文本段，第二个是补充文本段</returns>
        protected virtual Dictionary<string, string>[] SplitTextSegment(Dictionary<string, string> keyValues, int measurementCount)
        {
            if (keyValues == null || keyValues.Count <= 0) throw new Exception("Split failed,dictionary is null or empty");
            if (measurementCount < 0) throw new Exception("Split error,measurement count is less then zero");
            List<string> textkeys = new List<string>
            {
                Keys.ByteOrdKey,
                Keys.CYTKey,
                Keys.DataTypeKey,
                Keys.PARKey,
                Keys.TOTKey
            };
            for (int i = 1; i <= measurementCount; i++)
            {
                textkeys.Add(string.Format(Keys.PnBKey, i));
                textkeys.Add(string.Format(Keys.PnEKey, i));
                textkeys.Add(string.Format(Keys.PnNKey, i));
                textkeys.Add(string.Format(Keys.PnRKey, i));
            }
            Dictionary<string, string> text = new Dictionary<string, string>();//新的文本段
            Dictionary<string, string> stext = new Dictionary<string, string>();//新的补充文本段
            foreach (var item in keyValues)
            {
                if (textkeys.Contains(item.Key)) text.Add(item.Key, item.Value);
                else stext.Add(item.Key, item.Value);
            }
            return new Dictionary<string, string>[] { text, stext };
        }

        /// <summary>
        /// 计算文本段里面记录的各个段的起止位字符串长度
        /// </summary>
        /// <param name="textLength">已知文本段长度</param>
        /// <param name="dataLength">已知数据段长度</param>
        /// <param name="stextLength">已知补充文本段长度</param>
        /// <param name="analysisLength">已知解析段长度</param>
        /// <param name="haveNext">是否有下一个数据集</param>
        /// <returns></returns>
        protected virtual long CalculationSegmentTextLength(long headLength, long textLength, long dataLength, long stextLength, long analysisLength, bool haveNext)
        {
            long length = 0;
            if (dataLength > 0)
            {
                var temp = headLength + textLength;
                length += Keys.BeginDataKey.Length;
                length += Keys.EndDataKey.Length;
                length += 4;//分隔符
                length += temp.ToString().Length;//文本段结束位即数据段起始位
                length += (temp + dataLength).ToString().Length;//数据段结束位
            }
            if (stextLength > 0)
            {
                var temp = headLength + textLength + dataLength;
                length += Keys.BeginSTextKey.Length;
                length += Keys.EndSTextKey.Length;
                length += 4;//分隔符
                length += temp.ToString().Length;
                length += (temp + stextLength).ToString().Length;
            }
            if (analysisLength > 0)
            {
                var temp = headLength + textLength + dataLength + stextLength;
                length += Keys.BeginAnalysisKey.Length;
                length += Keys.EndAnalysisKey.Length;
                length += 4;//分隔符
                length += temp.ToString().Length;
                length += (temp + analysisLength).ToString().Length;
            }
            if (haveNext)
            {
                var temp = headLength + textLength + dataLength + stextLength + analysisLength + 2;
                length += Keys.NextDataKey.Length;
                length += 2;
                length += temp.ToString().Length;
            }
            else
            {
                length += Keys.NextDataKey.Length;
                length += 3;
            }
            return length;
        }
        /// <summary>
        /// 创建一个空的没有数据的head段,只有版本号
        /// </summary>
        /// <returns></returns>
        protected abstract byte[] CreateEmptyHeadBytes();
        #endregion

        /// <summary>
        /// 保存一组数据集到文件
        /// </summary>
        /// <param name="stream">可写流</param>
        /// <param name="list">数据集数组</param>
        /// <returns></returns>
        public virtual void Save(Stream stream, params FCS[] list)
        {
            if (!stream.CanWrite || !stream.CanSeek) throw new Exception("Save failed,stream can't write or seek");
            if (list == null || list.Length <= 0) throw new Exception("Save failed,dataset is null or empty");
            long fileBeginOffset = 0;
            var datasetMaxIndex = list.Length - 1;
            for (int i = 0; i <= datasetMaxIndex; i++)
            {
                Save(stream, list[i], fileBeginOffset, i < datasetMaxIndex, out long nextOffset);
                fileBeginOffset += nextOffset;
            }
        }
        /// <summary>
        /// 保存一个数据集
        /// 头段、文本段、数据段、补充文本段、解析段依次保存
        /// </summary>
        /// <param name="stream">可写入的流</param>
        /// <param name="fcs">数据集</param>
        /// <param name="fileBeginOffset">写入的首位位置，相对于流的起始位</param>
        /// <param name="haveNext">是否有下一个数据集，如果有，该数据集的nextdata不能为0</param>
        /// <param name="nextOffset">下一个数据集的起始位置，相对于当前数据集的起始位置</param>
        /// <returns></returns>
        protected virtual void Save(Stream stream, FCS fcs, long fileBeginOffset, bool haveNext, out long nextOffset)
        {
            #region 重置一些基本参数
            ResetTextSegment(fcs.TextSegment, fcs.Measurements);
            if (fcs.TextSegment.ContainsKey(Keys.BeginAnalysisKey)) fcs.TextSegment.Remove(Keys.BeginAnalysisKey);//移除位置信息，后续计算
            if (fcs.TextSegment.ContainsKey(Keys.EndAnalysisKey)) fcs.TextSegment.Remove(Keys.EndAnalysisKey);
            if (fcs.TextSegment.ContainsKey(Keys.BeginSTextKey)) fcs.TextSegment.Remove(Keys.BeginSTextKey);
            if (fcs.TextSegment.ContainsKey(Keys.EndSTextKey)) fcs.TextSegment.Remove(Keys.EndSTextKey);
            if (fcs.TextSegment.ContainsKey(Keys.BeginDataKey)) fcs.TextSegment.Remove(Keys.BeginDataKey);
            if (fcs.TextSegment.ContainsKey(Keys.EndDataKey)) fcs.TextSegment.Remove(Keys.EndDataKey);
            if (fcs.TextSegment.ContainsKey(Keys.NextDataKey)) fcs.TextSegment.Remove(Keys.NextDataKey);
            #endregion

            byte[] headBytes = CreateEmptyHeadBytes();
            MemoryStream textStream = DictionaryToStream(fcs.TextSegment, Keys.DelimiterByte);//文本段
            MemoryStream stextStream = null;//补充文本段
            MemoryStream analysisStream = (fcs.AnalysisSegment != null && fcs.AnalysisSegment.Count > 0) ? DictionaryToStream(fcs.AnalysisSegment, Keys.DelimiterByte) : null;//分析段

            #region 计算各个段的长度
            long headLength = headBytes.Length;
            long bitLength = 0L;
            foreach (var item in fcs.Measurements) bitLength += item.PnB;
            long dataLength = (bitLength / 8) * Convert.ToInt32(fcs.TextSegment[Keys.TOTKey]);//数据段总长度
            long textLength = textStream.Length;
            long stextLength = 0L;
            long analysisLength = analysisStream == null ? 0L : analysisStream.Length;

            long beginendLength = 0L;
            long calculationLength = CalculationSegmentTextLength(headLength, textLength + beginendLength, dataLength, stextLength, analysisLength, haveNext);//记录各段位置的文本段长度
            while (calculationLength != beginendLength)
            {
                beginendLength = calculationLength;
                calculationLength = CalculationSegmentTextLength(headLength, textLength + beginendLength, dataLength, stextLength, analysisLength, haveNext);
            }
            if ((headLength + textLength + beginendLength) > 99999999)//文本段过大，需要拆分到补充文本段
            {
                var texts = SplitTextSegment(fcs.TextSegment, fcs.Measurements.Count);
                textStream = DictionaryToStream(texts[0], Keys.DelimiterByte);//文本段重新计算
                stextStream = DictionaryToStream(texts[1], Keys.DelimiterByte);//计算补充文本段
                textLength = textStream.Length;
                stextLength = stextStream.Length;
                beginendLength = 0L;
                calculationLength = CalculationSegmentTextLength(headLength, textLength + beginendLength, dataLength, stextLength, analysisLength, haveNext);
                while (calculationLength != beginendLength)
                {
                    beginendLength = calculationLength;
                    calculationLength = CalculationSegmentTextLength(headLength, textLength + beginendLength, dataLength, stextLength, analysisLength, haveNext);
                }
            }
            #endregion

            #region 保存数据
            MemoryStream segmentLocation = new MemoryStream();//记录各段位置的流，跟随文本段保存
            var textBegin = Encoding.UTF8.GetBytes(headLength.ToString());
            textBegin.CopyTo(headBytes, 18 - textBegin.Length);
            var textEnd = Encoding.UTF8.GetBytes((headLength + textLength + beginendLength - 1).ToString());
            textEnd.CopyTo(headBytes, 26 - textEnd.Length);

            #region 数据段位置
            var dataBegin = Encoding.UTF8.GetBytes((headLength + textLength + beginendLength).ToString());
            var dataEnd = Encoding.UTF8.GetBytes((headLength + textLength + beginendLength + dataLength - 1).ToString());
            if (dataEnd.Length <= 8)
            {
                dataBegin.CopyTo(headBytes, 34 - dataBegin.Length);
                dataEnd.CopyTo(headBytes, 42 - dataEnd.Length);
            }
            segmentLocation.WriteAll(Encoding.UTF8.GetBytes(Keys.BeginDataKey));
            segmentLocation.WriteByte(Keys.DelimiterByte);
            segmentLocation.WriteAll(dataBegin);
            segmentLocation.WriteByte(Keys.DelimiterByte);
            segmentLocation.WriteAll(Encoding.UTF8.GetBytes(Keys.EndDataKey));
            segmentLocation.WriteByte(Keys.DelimiterByte);
            segmentLocation.WriteAll(dataEnd);
            segmentLocation.WriteByte(Keys.DelimiterByte);
            #endregion
            if (stextLength > 0)//补充文本段位置
            {
                var stextBegin = (headLength + textLength + beginendLength + dataLength).ToString();
                var stextEnd = (headLength + textLength + beginendLength + dataLength + stextLength - 1).ToString();
                segmentLocation.WriteAll(KeyValueToByteArray(Keys.BeginSTextKey, stextBegin, Keys.DelimiterByte));
                segmentLocation.WriteAll(KeyValueToByteArray(Keys.EndSTextKey, stextEnd, Keys.DelimiterByte));
            }
            if (analysisLength > 0)//解析段位置
            {
                var analysisBegin = Encoding.UTF8.GetBytes((headLength + textLength + beginendLength + dataLength + stextLength).ToString());
                var analysisEnd = Encoding.UTF8.GetBytes((headLength + textLength + beginendLength + dataLength + stextLength + analysisLength - 1).ToString());
                if (analysisEnd.Length <= 8)
                {
                    analysisBegin.CopyTo(headBytes, 42 - analysisBegin.Length);
                    analysisEnd.CopyTo(headBytes, 50 - analysisEnd.Length);
                }
                segmentLocation.WriteAll(Encoding.UTF8.GetBytes(Keys.BeginAnalysisKey));
                segmentLocation.WriteByte(Keys.DelimiterByte);
                segmentLocation.WriteAll(analysisBegin);
                segmentLocation.WriteByte(Keys.DelimiterByte);
                segmentLocation.WriteAll(Encoding.UTF8.GetBytes(Keys.EndAnalysisKey));
                segmentLocation.WriteByte(Keys.DelimiterByte);
                segmentLocation.WriteAll(analysisEnd);
                segmentLocation.WriteByte(Keys.DelimiterByte);
            }
            if (haveNext)
            {
                nextOffset = headLength + textLength + beginendLength + dataLength + stextLength + analysisLength + 2;
                segmentLocation.WriteAll(KeyValueToByteArray(Keys.NextDataKey, nextOffset.ToString(), Keys.DelimiterByte));
            }
            else
            {
                nextOffset = 0;
                segmentLocation.WriteAll(KeyValueToByteArray(Keys.NextDataKey, "0", Keys.DelimiterByte));
            }
            //开始写入流并计算crc
            stream.Seek(fileBeginOffset, SeekOrigin.Begin);
            stream.Write(headBytes, 0, headBytes.Length);
            var crc = CRC16CCITT_Implementation.ComputeCrc(headBytes);
            textStream.WriteTo(stream);
            crc = CRC16CCITT_Implementation.ComputeCrc(textStream, crc);
            textStream.Dispose();
            segmentLocation.WriteTo(stream);
            crc = CRC16CCITT_Implementation.ComputeCrc(segmentLocation, crc);
            segmentLocation.Dispose();
            GC.Collect();
            var dataStream = DataSegmentToStream(fcs.Measurements);
            dataStream.WriteTo(stream);
            crc = CRC16CCITT_Implementation.ComputeCrc(dataStream, crc);
            dataStream.Dispose();
            if (stextStream != null)
            {
                stextStream.WriteTo(stream);
                crc = CRC16CCITT_Implementation.ComputeCrc(stextStream, crc);
                stextStream.Dispose();
            }
            if (analysisStream != null)
            {
                analysisStream.WriteTo(stream);
                crc = CRC16CCITT_Implementation.ComputeCrc(analysisStream, crc);
                analysisStream.Dispose();
            }
            stream.Write(BitConverter.GetBytes(crc), 0, 2);
            #endregion
        }
        #endregion
    }

}
