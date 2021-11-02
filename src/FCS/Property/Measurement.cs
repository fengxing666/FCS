using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace FCS.Property
{
    public partial class Measurement : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    /// <summary>
    /// FCS参数必须属性
    /// </summary>
    public partial class Measurement
    {
        private string name;
        /// <summary>
        /// 名称 PnN
        /// </summary>
        public string Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }

        private uint bitNumber;
        /// <summary>
        /// 数据位数 PnB
        /// </summary>
        public uint BitNumber
        {
            get { return bitNumber; }
            set
            {
                bitNumber = value;
                ByteNumber = Convert.ToInt32(value / 8);
                OnPropertyChanged("BitNumber");
                OnPropertyChanged("ByteNumber");
            }
        }
        public int ByteNumber { get; private set; }

        private Amplification amplification;
        /// <summary>
        /// 放大类型 PnE解析
        /// </summary>
        public Amplification Amplification { get { return amplification; } set { amplification = value; OnPropertyChanged("Amplification"); } }

        private ulong range;
        /// <summary>
        /// 范围 PnR 型号值得最大值
        /// </summary>
        public ulong Range { get { return range; } set { range = value; OnPropertyChanged("Range"); } }

        private IList values;
        /// <summary>
        /// 用于记录的数据集合
        /// </summary>
        public IList Values { get { return values; } set { values = value; OnPropertyChanged("Values"); } }
    }

    /// <summary>
    /// FCS参数可选属性
    /// </summary>
    public partial class Measurement
    {
        private SuggestedVisualizationScale suggestedVisualizationScale;
        /// <summary>
        /// 参数 n 的建议可视化比例 解析 PnD
        /// </summary>
        public SuggestedVisualizationScale SuggestedVisualizationScale { get { return suggestedVisualizationScale; } set { suggestedVisualizationScale = value; OnPropertyChanged("SuggestedVisualizationScale"); } }

        private string opticalFilter;
        /// <summary>
        /// 参数 n 的光学滤波器的名称
        /// </summary>
        public string OpticalFilter { get { return opticalFilter; } set { opticalFilter = value; OnPropertyChanged("OpticalFilter"); } }

        private double gain = double.NaN;
        /// <summary>
        /// 用于获取参数 n 的放大器增益
        /// </summary>
        public double Gain { get { return gain; } set { gain = value; OnPropertyChanged("Gain"); } }

        private string wavelength;
        /// <summary>
        /// 参数 n 的激发波长
        /// </summary>
        public string Wavelength { get { return wavelength; } set { wavelength = value; OnPropertyChanged("Wavelength"); } }

        private uint power;
        /// <summary>
        /// 参数 n 的激发功率
        /// </summary>
        public uint Power { get { return power; } set { power = value; OnPropertyChanged("Power"); } }

        private string longName;
        /// <summary>
        /// 用于参数 n 的名称
        /// </summary>
        public string LongName { get { return longName; } set { longName = value; OnPropertyChanged("LongName"); } }

        private string detector;
        /// <summary>
        /// 参数 n 的探测器类型
        /// </summary>
        public string Detector { get { return detector; } set { detector = value; OnPropertyChanged("Detector"); } }

        private double voltage = double.NaN;
        /// <summary>
        /// 参数 n 的探测器电压
        /// </summary>
        public double Voltage { get { return voltage; } set { voltage = value; OnPropertyChanged("Voltage"); } }

        private DataType dataType;
        /// <summary>
        /// 数据类型
        /// </summary>
        public DataType DataType { get { return dataType; } set { dataType = value; OnPropertyChanged("DataType"); } }
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
            switch (DataType)
            {
                case DataType.I:
                    if (Values == null)
                    {
                        if (BitNumber <= 8) Values = new List<byte>(tot);
                        else if (BitNumber > 8 && BitNumber <= 16) Values = new List<ushort>(tot);
                        else if (BitNumber > 16 && BitNumber <= 32) Values = new List<uint>(tot);
                        else if (BitNumber > 32 && BitNumber <= 64) Values = new List<ulong>(tot);
                        else throw new Exception("Can't analyse data,PnB is too big");
                    }
                    if (BitNumber <= 8) Values.Add(BitMask(bytes[bytes.Length - 1]));
                    else if (BitNumber > 8 && BitNumber <= 16) Values.Add(BitMask(BitConverter.ToUInt16(bytes, bytes.Length > 2 ? (bytes.Length - 2) : 0)));
                    else if (BitNumber > 16 && BitNumber <= 32) Values.Add(BitMask(BitConverter.ToUInt32(bytes, bytes.Length > 4 ? (bytes.Length - 4) : 0)));
                    else if (BitNumber > 32 && BitNumber <= 64) Values.Add(BitMask(BitConverter.ToUInt64(bytes, bytes.Length > 8 ? (bytes.Length - 8) : 0)));
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
            if (Range == 0) return value;
            return Convert.ToByte(value % Range);
        }
        public virtual ushort BitMask(ushort value)
        {
            if (Range == 0) return value;
            return Convert.ToUInt16(value % Range);
        }
        public virtual uint BitMask(uint value)
        {
            if (Range == 0) return value;
            return Convert.ToUInt32(value % Range);
        }
        public virtual ulong BitMask(ulong value)
        {
            if (Range == 0) return value;
            return value % Range;
        }

        /// <summary>
        /// 对数放大 PnE,只用于datatype=i
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual double PnECalculation(ulong value)
        {
            if (Range == 0 || (this.Amplification.PowerNumber == 0 && this.Amplification.ZeroValue == 0)) return value;
            return Math.Pow(10, this.Amplification.PowerNumber * value / Range) * (Amplification.ZeroValue == 0d ? 1 : Amplification.ZeroValue);
        }

        /// <summary>
        /// 线性放大 PnG 增益,3.2版本只用于datatype=i
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual double PnGCalculation(ulong value)
        {
            if (double.IsNaN(Gain) || Gain <= 0d || Gain == 1d) return value;
            return value / Gain;
        }
        public virtual double PnGCalculation(float value)
        {
            if (double.IsNaN(Gain) || Gain <= 0d || Gain == 1d) return value;
            return value / Gain;
        }
        public virtual double PnGCalculation(double value)
        {
            if (double.IsNaN(Gain) || Gain <= 0d || Gain == 1d) return value;
            return value / Gain;
        }

        /// <summary>
        /// 通道值转刻度值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual double ConvertChannelToScaleValue(object obj)
        {
            if (DataType == DataType.I)
            {
                if (BitNumber <= 8)
                {
                    var item = Convert.ToByte(obj);
                    if (!double.IsNaN(Gain) && Gain > 0d && Gain != 1d) return PnGCalculation(item);
                    else return PnECalculation(item);
                }
                else if (BitNumber > 8 && BitNumber <= 16)
                {
                    var item = Convert.ToUInt16(obj);
                    if (!double.IsNaN(Gain) && Gain > 0d && Gain != 1d) return PnGCalculation(item);
                    else return PnECalculation(item);
                }
                else if (BitNumber > 16 && BitNumber <= 32)
                {
                    var item = Convert.ToUInt32(obj);
                    if (!double.IsNaN(Gain) && Gain > 0d && Gain != 1d) return PnGCalculation(item);
                    else return PnECalculation(item);
                }
                else if (BitNumber > 32 && BitNumber <= 64)
                {
                    var item = Convert.ToUInt64(obj);
                    if (!double.IsNaN(Gain) && Gain > 0d && Gain != 1d) return PnGCalculation(item);
                    else return PnECalculation(item);
                }
                else throw new Exception("Can't analyse data,PnB is too big");
            }
            else if (DataType == DataType.F)
            {
                return PnGCalculation(Convert.ToSingle(obj));
            }
            else if (DataType == DataType.D)
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
