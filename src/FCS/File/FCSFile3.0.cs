using FCS.Property;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FCS.File
{
    public class FCSFile3_0 : IFCSFile
    {
        #region 解析
        /// <summary>
        /// 从文本段填充数据集参数
        /// </summary>
        /// <param name="keyValues">文本段字典</param>
        /// <param name="parameter">需要完善的参数</param>
        protected override void FillFileParameterFromTextSegment(Dictionary<string, string> keyValues, FCSFileParameter parameter)
        {
            base.FillFileParameterFromTextSegment(keyValues, parameter);
            if (keyValues.ContainsKey(Keys.ModeKey) && ModeConvert.ConvertToEnum(keyValues[Keys.ModeKey]) != Mode.L) throw new Exception("Can't analyse,mode must be L");
            if (keyValues.ContainsKey(Keys.ByteOrdKey) && ByteOrderConvert.ConvertToEnum(keyValues[Keys.ByteOrdKey]) == ByteOrd.Unknown) throw new Exception("Can't analyse,byteord not supported");
            if (keyValues.ContainsKey(Keys.UnicodeKey) && !"UTF-8".Equals(keyValues[Keys.UnicodeKey].ToUpper()) && !"UTF8".Equals(keyValues[Keys.UnicodeKey].ToUpper())) throw new Exception("Can't analyse,unicode must be utf-8");
        }

        /// <summary>
        /// 解析数据集的通道参数
        /// </summary>
        /// <param name="textSegment">文本段key-value集合</param>
        /// <param name="par">通道数量</param>
        /// <param name="defaultDataType">默认数据类型</param>
        protected override IList<Measurement> AnalyseMeasurements(Dictionary<string, string> textSegment, uint par, DataType defaultDataType)
        {
            var measurements = base.AnalyseMeasurements(textSegment, par, defaultDataType);
            foreach (var item in measurements) item.DataType = defaultDataType;
            return measurements;
        }

        /// <summary>
        /// 解析补偿
        /// </summary>
        /// <param name="fcs"></param>
        protected override void AnalyseCompensation(FCS fcs)
        {
            if (fcs.TextSegment.ContainsKey(Keys.COMPKey)) fcs.Compensation = new Compensation(fcs.TextSegment[Keys.COMPKey]);
        }
        #endregion

        #region 保存
        protected override byte[] CreateEmptyHeadBytes()
        {
            return new byte[] { 0x46, 0x43, 0x53, 0x33, 0x2e, 0x30, 0x20, 0x20, 0x20, 0x20
                    , 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x30
                    , 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x30
                    , 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x30
                    , 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x30
                    , 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x30
                    , 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x30};
        }
        /// <summary>
        /// 重置文本段中的一些参数，根据通道参数
        /// </summary>
        /// <param name="textSegment">文本段字典</param>
        /// <param name="measurements">通道集合</param>
        protected override void ResetTextSegment(Dictionary<string, string> textSegment, IList<Measurement> measurements)
        {
            if (textSegment == null) throw new Exception("Dictionary can't be null");
            if (measurements == null || measurements.Count <= 0 || measurements.FirstOrDefault(p => p.Values == null || p.Values.Count <= 0) != null) throw new Exception("Measurement array and every measurement's values can't be null or empty");
            var counts = measurements.Select(p => p.Values.Count).Distinct().ToArray();
            if (counts.Length > 1) throw new Exception("Error,every measurement's values count mast same");
            else textSegment[Keys.TOTKey] = counts[0].ToString();//tot数量
            textSegment[Keys.ByteOrdKey] = BitConverter.IsLittleEndian ? ByteOrderConvert.LittleEndian : ByteOrderConvert.BigEndian;//windows系统默认
            textSegment[Keys.ModeKey] = "L";//保留字段，3.2版本已经取消，都是List类型
            textSegment[Keys.PARKey] = measurements.Count.ToString();//通道数量
            var datatypes = measurements.Select(p => p.DataType).Distinct().ToArray();
            DataType defaultDataType = DataType.Unknown;//取出默认数据类型
            if (datatypes.Contains(DataType.D)) defaultDataType = DataType.D;
            else if (datatypes.Contains(DataType.F)) defaultDataType = DataType.F;
            else defaultDataType = DataType.I;
            textSegment[Keys.DataTypeKey] = DataTypeConvert.ConvertToString(defaultDataType);
            for (int i = 1; i <= measurements.Count; i++)
            {
                var measurement = measurements[i - 1];
                if (defaultDataType != DataType.I)//如果是浮点型
                {
                    textSegment[string.Format(Keys.PnBKey, i)] = defaultDataType == DataType.D ? "64" : "32";
                    textSegment[string.Format(Keys.PnEKey, i)] = "0,0";
                }
                else//如果是整数型
                {
                    if (measurement.Values[0] is ulong ul) textSegment[string.Format(Keys.PnBKey, i)] = "64";
                    else if (measurement.Values[0] is uint ui) textSegment[string.Format(Keys.PnBKey, i)] = "32";
                    else if (measurement.Values[0] is ushort us) textSegment[string.Format(Keys.PnBKey, i)] = "16";
                    else if (measurement.Values[0] is byte ub) textSegment[string.Format(Keys.PnBKey, i)] = "8";
                    else throw new Exception("Measurement value's data type not supported");
                    textSegment[string.Format(Keys.PnEKey, i)] = measurement.Amplification.ToString();
                }
                string pngkey = string.Format(Keys.PnGKey, i);
                if (!double.IsNaN(measurement.Gain) && measurement.Gain > 0) textSegment[pngkey] = measurement.Gain.ToString();
                else textSegment[pngkey] = "1";

                string pndatatypekey = string.Format(Keys.PnDataTypeKey, i);
                if (defaultDataType != measurement.DataType) textSegment[pndatatypekey] = DataTypeConvert.ConvertToString(measurement.DataType);//PnDataType不是默认值
                else if (textSegment.ContainsKey(pndatatypekey)) textSegment.Remove(pndatatypekey);//PnDataType是默认值
                if (string.IsNullOrEmpty(measurement.Name)) throw new Exception("Measurment name can't be null or empty");
                textSegment[string.Format(Keys.PnNKey, i)] = measurement.Name;

                textSegment[string.Format(Keys.PnRKey, i)] = measurement.Range.ToString();

                string pndkey = string.Format(Keys.PnDKey, i);
                if (measurement.SuggestedVisualizationScale.Type != SuggestedVisualizationScaleType.Unknown) textSegment[pndkey] = measurement.SuggestedVisualizationScale.ToString();
                else if (textSegment.ContainsKey(pndkey)) textSegment.Remove(pndkey);

                string pnfkey = string.Format(Keys.PnFKey, i);
                if (!string.IsNullOrEmpty(measurement.OpticalFilter)) textSegment[pnfkey] = measurement.OpticalFilter;
                else if (textSegment.ContainsKey(pnfkey)) textSegment.Remove(pnfkey);

                string pnlkey = string.Format(Keys.PnLKey, i);
                if (!string.IsNullOrEmpty(measurement.Wavelength)) textSegment[pnlkey] = measurement.Wavelength;
                else if (textSegment.ContainsKey(pnlkey)) textSegment.Remove(pnlkey);

                string pnokey = string.Format(Keys.PnOKey, i);
                if (measurement.Power != 0) textSegment[pnokey] = measurement.Power.ToString();
                else if (textSegment.ContainsKey(pnokey)) textSegment.Remove(pnokey);

                string pnskey = string.Format(Keys.PnSKey, i);
                if (!string.IsNullOrEmpty(measurement.LongName)) textSegment[pnskey] = measurement.LongName;
                else if (textSegment.ContainsKey(pnskey)) textSegment.Remove(pnskey);

                string pntkey = string.Format(Keys.PnTKey, i);
                if (!string.IsNullOrEmpty(measurement.Detector)) textSegment[pntkey] = measurement.Detector;
                else if (textSegment.ContainsKey(pntkey)) textSegment.Remove(pntkey);

                string pnvkey = string.Format(Keys.PnVKey, i);
                if (!double.IsNaN(measurement.Voltage) && measurement.Voltage > 0) textSegment[pnvkey] = measurement.Voltage.ToString();
                else if (textSegment.ContainsKey(pnvkey)) textSegment.Remove(pnvkey);
            }
        }
        /// <summary>
        /// 重置补偿
        /// </summary>
        /// <param name="fcs"></param>
        protected override void ResetCompensation(FCS fcs)
        {
            if (fcs == null) return;
            var temp = fcs.Compensation.ToString();
            if (string.IsNullOrEmpty(temp)) return;
            fcs.TextSegment[Keys.COMPKey] = temp;
        }
        #endregion
    }
}
