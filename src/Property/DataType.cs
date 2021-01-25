namespace FCS.Property
{
    /// <summary>
    /// 数据格式
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// 未知的
        /// </summary>
        Unknown,
        /// <summary>
        /// 二进制
        /// </summary>
        I,
        /// <summary>
        /// 单精度浮点
        /// </summary>
        F,
        /// <summary>
        /// 双精度浮点
        /// </summary>
        D
    }

    public class DataTypeConvert
    {
        #region endian
        public const char Integers = 'I';
        public const char Float = 'F';
        public const char Double = 'D';
        #endregion
        public static DataType ConvertToEnum(char type)
        {
            switch (type)
            {
                case Integers:
                    return DataType.I;
                case Float:
                    return DataType.F;
                case Double:
                    return DataType.D;
                default:
                    return DataType.Unknown;
            }
        }

        public static char ConvertToString(DataType fCSDataType)
        {
            switch (fCSDataType)
            {
                case DataType.I:
                    return Integers;
                case DataType.F:
                    return Float;
                case DataType.D:
                    return Double;
                default:
                    return default;
            }
        }
    }
}


