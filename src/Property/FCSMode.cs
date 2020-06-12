using System;
using System.Collections.Generic;
using System.Text;

namespace FCS.Property
{
    /// <summary>
    /// 存储模式
    /// </summary>
    public enum FCSMode
    {
        Unknown,
        /// <summary>
        /// 列表
        /// </summary>
        L
    }

    public class FCSModeConvert
    {
        #region endian
        public const string List = "L";
        #endregion
        public static FCSMode ConvertToEnum(string mode)
        {
            return mode switch
            {
                List => FCSMode.L,
                _ => FCSMode.Unknown,
            };
        }

        public static string ConvertToString(FCSMode fCSMode)
        {
            return fCSMode switch
            {
                FCSMode.L => List,
                _ => string.Empty,
            };
        }
    }
}
