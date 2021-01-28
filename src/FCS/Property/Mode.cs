namespace FCS.Property
{
    /// <summary>
    /// 存储模式
    /// </summary>
    public enum Mode
    {
        Unknown,
        /// <summary>
        /// 列表模式
        /// </summary>
        L
    }

    public class ModeConvert
    {
        #region endian
        public const char List = 'L';
        #endregion
        public static Mode ConvertToEnum(char mode)
        {
            switch (mode)
            {
                case List:
                    return Mode.L;
                default:
                    return Mode.Unknown;
            }
        }

        public static Mode ConvertToEnum(string mode)
        {
            if (string.IsNullOrEmpty(mode) || mode.Length <= 0) return Mode.Unknown;
            switch (mode[0])
            {
                case List:
                    return Mode.L;
                default:
                    return Mode.Unknown;
            }
        }

        public static char ConvertToString(Mode fCSMode)
        {
            switch (fCSMode)
            {
                case Mode.L:
                    return List;
                default:
                    return default;
            }
        }
    }
}
