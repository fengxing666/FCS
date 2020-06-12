using System;
using System.Collections.Generic;
using System.Text;

namespace FCS.Property
{
    /// <summary>
    /// 数据排序方式
    /// </summary>
    public enum FCSByteOrd
    {
        /// <summary>
        /// 未知的
        /// </summary>
        Unknown,
        LittleEndian,
        BigEndian
    }
    public class FCSByteOrderConvert
    {
        #region endian
        public const string BigEndian = "4,3,2,1";
        public const string LittleEndian = "1,2,3,4";
        #endregion
        public static FCSByteOrd ConvertToEnum(string endian)
        {
            return endian switch
            {
                BigEndian => FCSByteOrd.BigEndian,
                LittleEndian => FCSByteOrd.LittleEndian,
                _ => FCSByteOrd.LittleEndian,
            };
        }

        public static string ConvertToString(FCSByteOrd fCSByteOrd)
        {
            return fCSByteOrd switch
            {
                FCSByteOrd.LittleEndian => LittleEndian,
                FCSByteOrd.BigEndian => BigEndian,
                _ => string.Empty,
            };
        }
    }
}
