using System;
using System.Collections.Generic;
using System.Text;

namespace FCS.Property
{
    /// <summary>
    /// 数据格式
    /// </summary>
    public enum FCSDataType
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

    public class FCSDataTypeConvert
    {
        #region endian
        public const string Integers = "I";
        public const string Float = "F";
        public const string Double = "D";
        #endregion
        public static FCSDataType ConvertToEnum(string type)
        {
            return type switch
            {
                Integers => FCSDataType.I,
                Float => FCSDataType.F,
                Double => FCSDataType.D,
                _ => FCSDataType.Unknown,
            };
        }

        public static string ConvertToString(FCSDataType fCSDataType)
        {
            return fCSDataType switch
            {
                FCSDataType.I => Integers,
                FCSDataType.F => Float,
                FCSDataType.D => Double,
                _ => string.Empty,
            };
        }
    }
}
