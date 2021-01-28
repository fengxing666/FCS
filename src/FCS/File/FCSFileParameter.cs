using FCS.Property;

namespace FCS.File
{

    /// <summary>
    /// 数据集的重要参数，用于文件解析等操作
    /// </summary>
    public class FCSFileParameter
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 文本段起始位
        /// </summary>
        public long TextBegin { get; set; }
        /// <summary>
        /// 文本段结束位
        /// </summary>
        public long TextEnd { get; set; }
        /// <summary>
        /// 补充文本端开始位
        /// </summary>
        public long STextBegin { get; set; }
        /// <summary>
        /// 补充文本端结束位
        /// </summary>
        public long STextEnd { get; set; }
        /// <summary>
        /// 数据端起始位
        /// </summary>
        public long DataBegin { get; set; }
        /// <summary>
        /// 数据段结束位
        /// </summary>
        public long DataEnd { get; set; }
        /// <summary>
        /// 解析段起始位
        /// </summary>
        public long AnalysisBegin { get; set; }
        /// <summary>
        /// 解释段结束位
        /// </summary>
        public long AnalysisEnd { get; set; }
        /// <summary>
        /// 文本段分隔符
        /// </summary>
        public byte DelimiterByte { get; set; }
        /// <summary>
        /// 字节顺序
        /// </summary>
        public ByteOrd ByteOrd { get; set; }
        /// <summary>
        /// 数据类型
        /// </summary>
        public DataType DataType { get; set; }
        /// <summary>
        /// 参数个数
        /// </summary>
        public uint PAR { get; set; }
        /// <summary>
        /// 数据量
        /// </summary>
        public uint TOT { get; set; }
        /// <summary>
        /// 下一个数据集的起始位
        /// </summary>
        public long NextData { get; set; }
    }
}
