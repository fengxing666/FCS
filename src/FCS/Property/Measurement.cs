using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FCS.Property
{
    /// <summary>
    /// FCS参数必须属性
    /// </summary>
    public partial class Measurement
    {
        /// <summary>
        /// 名称 PnN
        /// </summary>
        public string PnN { get; set; }
        private uint _pnb;
        /// <summary>
        /// 数据位数 PnB
        /// </summary>
        public uint PnB
        {
            get { return _pnb; }
            set
            {
                _pnb = value;
                PnBByteLength = Convert.ToInt32(value / 8);
            }
        }
        public int PnBByteLength { get; private set; }
        /// <summary>
        /// 放大类型 PnE解析
        /// </summary>
        public Amplification PnE { get; set; }
        /// <summary>
        /// 范围 PnR 型号值得最大值
        /// </summary>
        public ulong PnR { get; set; }
        /// <summary>
        /// 用于记录的数据集合
        /// </summary>
        public IList Values { get; set; }
    }

    /// <summary>
    /// FCS参数可选属性
    /// </summary>
    public partial class Measurement
    {
        /// <summary>
        /// 参数 n 的建议可视化比例 解析 PnD
        /// </summary>
        public RecommendsVisualizationScale PnD { get; set; }
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
        public double PnV { get; set; } = double.NaN;
        /// <summary>
        /// 数据类型
        /// </summary>
        public DataType PnDATATYPE { get; set; }
    }

    /// <summary>
    /// 参数计算
    /// </summary>
    public partial class Measurement
    {
        /// <summary>
        /// 添加一个数据到列表
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <param name="byteOrd">排序方式,默认Little,跟随windows</param>
        public virtual void AddOneValue(byte[] bytes, int tot, ByteOrd byteOrd = ByteOrd.LittleEndian)
        {
            if (bytes == null || bytes.Length <= 0) return;
            if ((byteOrd == ByteOrd.BigEndian && BitConverter.IsLittleEndian) || (byteOrd == ByteOrd.LittleEndian && !BitConverter.IsLittleEndian)) bytes = bytes.Reverse().ToArray();
            switch (PnDATATYPE)
            {
                case DataType.I:
                    if (Values == null)
                    {
                        if (PnB <= 8) Values = new List<byte>(tot);
                        else if (PnB > 8 && PnB <= 16) Values = new List<ushort>(tot);
                        else if (PnB > 16 && PnB <= 32) Values = new List<uint>(tot);
                        else if (PnB > 32 && PnB <= 64) Values = new List<ulong>(tot);
                        else throw new Exception("Can't analyse data,PnB is too big");
                    }
                    if (PnB <= 8) Values.Add(BitMask(bytes[bytes.Length - 1]));
                    else if (PnB > 8 && PnB <= 16) Values.Add(BitMask(BitConverter.ToUInt16(bytes, bytes.Length > 2 ? (bytes.Length - 2) : 0)));
                    else if (PnB > 16 && PnB <= 32) Values.Add(BitMask(BitConverter.ToUInt32(bytes, bytes.Length > 4 ? (bytes.Length - 4) : 0)));
                    else if (PnB > 32 && PnB <= 64) Values.Add(BitMask(BitConverter.ToUInt64(bytes, bytes.Length > 8 ? (bytes.Length - 8) : 0)));
                    else throw new Exception("Can't analyse data,PnB is too big");
                    break;
                case DataType.F:
                    if (Values == null) Values = new List<float>(tot);
                    Values.Add(BitConverter.ToSingle(bytes, bytes.Length > 4 ? (bytes.Length - 4) : 0));
                    break;
                case DataType.D:
                    if (Values == null) Values = new List<double>(tot);
                    Values.Add(BitConverter.ToDouble(bytes, bytes.Length > 8 ? (bytes.Length - 8) : 0));
                    break;
                default:
                    throw new Exception("Can't analyse data,data type not supported");
            }
        }

        /// <summary>
        /// 位掩码计算-根据当前参数的最大值PnR范围和位数PnB,只用于datatype=i
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual byte BitMask(byte value)
        {
            if (PnR == 0) return value;
            return Convert.ToByte(value % PnR);
        }
        public virtual ushort BitMask(ushort value)
        {
            if (PnR == 0) return value;
            return Convert.ToUInt16(value % PnR);
        }
        public virtual uint BitMask(uint value)
        {
            if (PnR == 0) return value;
            return Convert.ToUInt32(value % PnR);
        }
        public virtual ulong BitMask(ulong value)
        {
            if (PnR == 0) return value;
            return value % PnR;
        }

        /// <summary>
        /// 对数放大 PnE,只用于datatype=i
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual double PnECalculation(ulong value)
        {
            if (PnR == 0 || (this.PnE.PowerNumber == 0 && this.PnE.ZeroValue == 0)) return value;
            return Math.Pow(10, this.PnE.PowerNumber * value / PnR) * (PnE.ZeroValue == 0d ? 1 : PnE.ZeroValue);
        }

        /// <summary>
        /// 线性放大 PnG 增益,3.2版本只用于datatype=i
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual double PnGCalculation(ulong value)
        {
            if (double.IsNaN(PnG) || PnG <= 0d || PnG == 1d) return value;
            return value / PnG;
        }
        public virtual double PnGCalculation(float value)
        {
            if (double.IsNaN(PnG) || PnG <= 0d || PnG == 1d) return value;
            return value / PnG;
        }
        public virtual double PnGCalculation(double value)
        {
            if (double.IsNaN(PnG) || PnG <= 0d || PnG == 1d) return value;
            return value / PnG;
        }

        /// <summary>
        /// 通道值转刻度值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual double ConvertChannelToScaleValue(object obj)
        {
            if (PnDATATYPE == DataType.I)
            {
                if (PnB <= 8)
                {
                    var item = Convert.ToByte(obj);
                    if (!double.IsNaN(PnG) && PnG > 0d && PnG != 1d) return PnGCalculation(item);
                    else return PnECalculation(item);
                }
                else if (PnB > 8 && PnB <= 16)
                {
                    var item = Convert.ToUInt16(obj);
                    if (!double.IsNaN(PnG) && PnG > 0d && PnG != 1d) return PnGCalculation(item);
                    else return PnECalculation(item);
                }
                else if (PnB > 16 && PnB <= 32)
                {
                    var item = Convert.ToUInt32(obj);
                    if (!double.IsNaN(PnG) && PnG > 0d && PnG != 1d) return PnGCalculation(item);
                    else return PnECalculation(item);
                }
                else if (PnB > 32 && PnB <= 64)
                {
                    var item = Convert.ToUInt64(obj);
                    if (!double.IsNaN(PnG) && PnG > 0d && PnG != 1d) return PnGCalculation(item);
                    else return PnECalculation(item);
                }
                else throw new Exception("Can't analyse data,PnB is too big");
            }
            else if (PnDATATYPE == DataType.F)
            {
                return PnGCalculation(Convert.ToSingle(obj));
            }
            else if (PnDATATYPE == DataType.D)
            {
                return PnGCalculation(Convert.ToDouble(obj));
            }
            else throw new Exception("Data type not supported");
        }

        /// <summary>
        /// 获取刻度值
        /// </summary>
        /// <returns></returns>
        public virtual IList<double> GetScaleValues()
        {
            var list = new List<double>();
            foreach (var obj in Values) list.Add(ConvertChannelToScaleValue(obj));
            return list;
        }
    }
}
