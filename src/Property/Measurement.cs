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
        /// <summary>
        /// 数据位数 PnB
        /// </summary>
        public uint PnB { get; set; }
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
        public double PnV { get; set; }
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
        /// <param name="byteOrd">排序方式</param>
        public virtual void AddOneValue(byte[] bytes, ByteOrd byteOrd)
        {
            if (bytes == null || bytes.Length <= 0) return;
            if ((byteOrd == ByteOrd.BigEndian && BitConverter.IsLittleEndian) || (byteOrd == ByteOrd.LittleEndian && !BitConverter.IsLittleEndian)) bytes = bytes.Reverse().ToArray();
            switch (PnDATATYPE)
            {
                case DataType.Unknown:
                    break;
                case DataType.I:
                    if (Values == null)
                    {
                        if (PnB <= 8) Values = new List<byte>();
                        else if (PnB > 8 && PnB <= 16) Values = new List<ushort>();
                        else if (PnB > 16 && PnB <= 32) Values = new List<uint>();
                        else if (PnB > 32 && PnB <= 64) Values = new List<ulong>();
                        else throw new Exception("Can't analyse data,PnB is too big");
                    }
                    if (PnB <= 8) Values.Add(BitMask(bytes[bytes.Length - 1]));
                    else if (PnB > 8 && PnB <= 16) Values.Add(BitMask(BitConverter.ToUInt16(bytes, bytes.Length > 2 ? (bytes.Length - 2) : 0)));
                    else if (PnB > 16 && PnB <= 32) Values.Add(BitMask(BitConverter.ToUInt32(bytes, bytes.Length > 4 ? (bytes.Length - 4) : 0)));
                    else if (PnB > 32 && PnB <= 64) Values.Add(BitMask(BitConverter.ToUInt64(bytes, bytes.Length > 8 ? (bytes.Length - 8) : 0)));
                    else throw new Exception("Can't analyse data,PnB is too big");
                    break;
                case DataType.F:
                    if (Values == null) Values = new List<float>();
                    Values.Add(BitConverter.ToSingle(bytes, bytes.Length > 4 ? (bytes.Length - 4) : 0));
                    break;
                case DataType.D:
                    if (Values == null) Values = new List<double>();
                    Values.Add(BitConverter.ToDouble(bytes, bytes.Length > 8 ? (bytes.Length - 8) : 0));
                    break;
                default:
                    break;
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
        /// 线性放大 PnG 增益,只用于datatype=i
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual double PnGCalculation(ulong value)
        {
            if (double.IsNaN(PnG) || PnG == 0d) return value;
            return value / PnG;
        }

        /// <summary>
        /// 获取放大前的刻度值
        /// </summary>
        /// <returns></returns>
        public virtual IList<double> GetScaleValues()
        {
            if (PnDATATYPE == DataType.I)
            {
                List<double> list = new List<double>();
                if (PnB <= 8)
                {
                    foreach (byte item in Values)
                    {
                        if (PnG != 0d) list.Add(PnGCalculation(item));
                        else list.Add(PnECalculation(item));
                    }
                }
                else if (PnB > 8 && PnB <= 16)
                {
                    foreach (ushort item in Values)
                    {
                        if (PnG != 0d) list.Add(PnGCalculation(item));
                        else list.Add(PnECalculation(item));
                    }
                }
                else if (PnB > 16 && PnB <= 32)
                {
                    foreach (uint item in Values)
                    {
                        if (PnG != 0d) list.Add(PnGCalculation(item));
                        else list.Add(PnECalculation(item));
                    }
                }
                else if (PnB > 32 && PnB <= 64)
                {
                    foreach (ulong item in Values)
                    {
                        if (PnG != 0d) list.Add(PnGCalculation(item));
                        else list.Add(PnECalculation(item));
                    }
                }
                else throw new Exception("Can't analyse data,PnB is too big");
                return list;
            }
            else if (PnDATATYPE == DataType.F)
            {
                List<double> list = new List<double>();
                foreach (float item in Values) list.Add(item);
                return list;
            }
            else if (PnDATATYPE == DataType.D)
            {
                List<double> list = new List<double>();
                foreach (double item in Values) list.Add(item);
                return list;
            }
            else throw new Exception("Data type not supported");
        }

    }
}
