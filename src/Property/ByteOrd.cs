namespace FCS.Property
{
    /// <summary>
    /// 数据字节排序方式
    /// </summary>
    public enum ByteOrd
    {
        /// <summary>
        /// 未知的
        /// </summary>
        Unknown,
        LittleEndian,
        BigEndian
    }
    public class ByteOrderConvert
    {
        #region endian
        public const string BigEndian = "4,3,2,1";
        public const string LittleEndian = "1,2,3,4";
        #endregion
        public static ByteOrd ConvertToEnum(string endian)
        {
            switch (endian)
            {
                case BigEndian:
                    return ByteOrd.BigEndian;
                case LittleEndian:
                    return ByteOrd.LittleEndian;
                default:
                    return ByteOrd.LittleEndian;
            }
        }

        public static string ConvertToString(ByteOrd fCSByteOrd)
        {
            switch (fCSByteOrd)
            {
                case ByteOrd.LittleEndian:
                    return LittleEndian;
                case ByteOrd.BigEndian:
                    return BigEndian;
                default:
                    return string.Empty;
            }
        }
    }
}
