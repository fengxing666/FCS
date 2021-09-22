using FCS.Property;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FCS
{
    /// <summary>
    /// 输出对象
    /// </summary>
    public class FCS
    {
        /// <summary>
        /// 文本段数据,含补充文本段
        /// </summary>
        public Dictionary<string, string> TextSegment { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// 分析段数据
        /// </summary>
        public Dictionary<string, string> AnalysisSegment { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// 参数集合
        /// </summary>
        public IList<Measurement> Measurements { get; set; }
        /// <summary>
        /// 补偿
        /// </summary>
        public Compensation Compensation { get; set; }
    }
}
