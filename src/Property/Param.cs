using System;
using System.Collections.Generic;
using System.Text;

namespace FCS.Property
{
    /// <summary>
    /// FCS参数必须属性
    /// </summary>
    public partial class Param
    {
        /// <summary>
        /// 名称 PnN
        /// </summary>
        public string PnN { get; set; }
        /// <summary>
        /// 数据位数 PnB
        /// </summary>
        public uint PnB { get; set; }
        private string pne;
        /// <summary>
        /// 放大类型 PnE
        /// </summary>
        public string PnE
        {
            get { return pne; }
            set
            {
                pne = value;
                AmplificationValue = new Amplification(value);
            }
        }
        /// <summary>
        /// 放大类型 PnE解析
        /// </summary>
        public Amplification AmplificationValue { get; private set; }
        /// <summary>
        /// 范围 PnR 型号值得最大值
        /// </summary>
        public uint PnR { get; set; }
    }

    /// <summary>
    /// FCS参数可选属性
    /// </summary>
    public partial class Param
    {
        private string pnd;
        /// <summary>
        /// 参数 n 的建议可视化比例
        /// </summary>
        public string PnD
        {
            get { return pnd; }
            set
            {
                pnd = value;
                RecommendsVisualizationScaleValue = new RecommendsVisualizationScale(value);
            }
        }
        /// <summary>
        /// 参数 n 的建议可视化比例 解析
        /// </summary>
        public RecommendsVisualizationScale RecommendsVisualizationScaleValue { get; set; }
        /// <summary>
        /// 参数 n 的光学滤波器的名称
        /// </summary>
        public string PnF { get; set; }
        /// <summary>
        /// 用于获取参数 n 的放大器增益
        /// </summary>
        public double PnG { get; set; } = double.NaN;
        /// <summary>
        /// 参数 n 的激发波长
        /// </summary>
        public string PnL { get; set; }
        /// <summary>
        /// 参数 n 的激发功率
        /// </summary>
        public uint PnO { get; set; }
        /// <summary>
        /// 参数 n 收集的发射光的百分比
        /// </summary>
        public uint PnP { get; set; }
        /// <summary>
        /// 用于参数 n 的名称
        /// </summary>
        public string PnS { get; set; }
        /// <summary>
        /// 参数 n 的探测器类型
        /// </summary>
        public string PnT { get; set; }
        /// <summary>
        /// 参数 n 的探测器电压
        /// </summary>
        public double PnV { get; set; }
    }

    public partial class Param
    {
        /// <summary>
        /// 位掩码计算-根据当前参数的最大值PnR范围和位数PnB
        /// 只有datatype=i时需要
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ulong BitMask(ulong value)
        {
            if (PnR == 0) return value;
            return value % PnR;
        }

        /// <summary>
        /// 对数放大 PnE
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public double AmplificationCalculation(double value)
        {
            if (PnR == 0 || this.AmplificationValue.PowerNumber == 0 && this.AmplificationValue.ZeroValue == 0) return value;
            return Math.Pow(10, this.AmplificationValue.PowerNumber * value / PnR) * AmplificationValue.ZeroValue == 0 ? 1 : AmplificationValue.ZeroValue;
        }

        /// <summary>
        /// 线性放大 PnG 增益
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public double LineEnlargeCalculation(double value)
        {
            if (double.IsNaN(PnG)) return value;
            return value / PnG;
        }
    }

}
