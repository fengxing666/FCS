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
        Unknown = 0,
        /// <summary>
        /// 二进制
        /// </summary>
        I = 1,
        /// <summary>
        /// 单精度浮点
        /// </summary>
        F = 2,
        /// <summary>
        /// 双精度浮点
        /// </summary>
        D = 3
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

        public static DataType ConvertToEnum(string type)
        {
            if (string.IsNullOrEmpty(type) || type.Length <= 0) return DataType.Unknown;
            switch (type[0])
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

        public static char ConvertToChar(DataType fCSDataType)
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
        public static string ConvertToString(DataType fCSDataType)
        {
            switch (fCSDataType)
            {
                case DataType.I:
                    return "I";
                case DataType.F:
                    return "F";
                case DataType.D:
                    return "D";
                default:
                    return default;
            }
        }
    }
}


