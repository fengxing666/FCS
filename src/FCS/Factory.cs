using FCS.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FCS
{
    public class Factory
    {
        #region server
        private static IFCSFile FCS32Server { get; } = new FCSFile3_2();
        private static IFCSFile FCS31Server { get; } = new FCSFile3_1();
        private static IFCSFile FCS30Server { get; } = new FCSFile3_0();

        #endregion

        #region 读取文件
        /// <summary>
        /// 读取fcs文件，返回fcs对象集合
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="notReadDataSegment">是否不读取数据段</param>
        /// <returns></returns>
        public static IList<FCS> ReadFile(string filePath, bool notReadDataSegment = false)
        {
            var fileInfo = new FileInfo(filePath);
            using (var fileStream = fileInfo.OpenRead())
            {
                return Read(fileStream, notReadDataSegment);
            }
        }
        /// <summary>
        /// 读取fcs文件，返回fcs对象集合
        /// </summary>
        /// <param name="stream">可读取的流</param>
        /// <param name="notReadDataSegment">是否不读取数据段</param>
        /// <returns></returns>
        public static IList<FCS> Read(Stream stream, bool notReadDataSegment = false)
        {
            var first = ReadOneDataset(stream, out long next, 0, notReadDataSegment);
            List<FCS> list = new List<FCS>() { first };
            long nextfromfilebegin = 0;
            while (next != 0)
            {
                nextfromfilebegin += next;
                var temp = ReadOneDataset(stream, out next, nextfromfilebegin, notReadDataSegment);
                if (temp != null && temp.Measurements != null) list.Add(temp);
                else break;
            }
            return list;
        }

        /// <summary>
        /// 读取fcs文件的一个数据集
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="nextData">下一个数据集的起点，相对于此方法返回的数据集起点</param>
        /// <param name="fileBeginOffset">相对于文件起点的位置</param>
        /// <param name="notReadDataSegment">是否不读取数据段</param>
        /// <returns></returns>
        public static FCS ReadFileOneDataset(string filePath, out long nextData, long fileBeginOffset = 0, bool notReadDataSegment = false)
        {
            var fileInfo = new FileInfo(filePath);
            using (var fileStream = fileInfo.OpenRead())
            {
                return ReadOneDataset(fileStream, out nextData, fileBeginOffset, notReadDataSegment);
            }
        }
        /// <summary>
        /// 读取一个fcs数据集
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="nextData">下一个数据集的起点，相对于此方法返回的数据集起点</param>
        /// <param name="fileBeginOffset">相对于流起点的位置</param>
        /// <param name="notReadDataSegment">是否不读取数据段</param>
        /// <returns></returns>
        public static FCS ReadOneDataset(Stream stream, out long nextData, long fileBeginOffset = 0, bool notReadDataSegment = false)
        {
            if (!stream.CanRead || !stream.CanSeek) throw new Exception("Stream can't read or seek");
            var version = ReadVersion(stream, fileBeginOffset);
            switch (version)
            {
                case "FCS3.2":
                    return FCS32Server.ReadDataset(stream, out nextData, fileBeginOffset, notReadDataSegment);
                case "FCS3.1":
                    return FCS31Server.ReadDataset(stream, out nextData, fileBeginOffset, notReadDataSegment);
                case "FCS3.0":
                    return FCS30Server.ReadDataset(stream, out nextData, fileBeginOffset, notReadDataSegment);
                default:
                    throw new Exception("Version not supported");
            }
        }

        /// <summary>
        /// 读取版本号
        /// </summary>
        /// <param name="stream">可读取的流</param>
        /// <returns></returns>
        protected static string ReadVersion(Stream stream, long offset)
        {
            if (stream.Length < (6 + offset)) throw new Exception("Offset is too big");
            byte[] bytes = new byte[6];
            stream.Seek(offset, SeekOrigin.Begin);
            if (stream.Read(bytes, 0, 6) != 6) throw new Exception("Version read failed");
            return Encoding.ASCII.GetString(bytes).ToUpper();
        }
        #endregion

        #region 保存文件
        /// <summary>
        /// 保存到3.2版本
        /// </summary>
        /// <param name="filePath">保存的文件路径</param>
        /// <param name="list">fcs对象集合</param>
        /// <returns>是否保存成功</returns>
        public static void SaveToFCS32(string filePath, params FCS[] list)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                SaveToFCS32(stream, list);
            }
        }
        /// <summary>
        /// 保存到3.2版本
        /// </summary>
        /// <param name="stream">可保存的流</param>
        /// <param name="list">fcs对象集合</param>
        /// <returns>是否保存成功</returns>
        public static void SaveToFCS32(Stream stream, params FCS[] list)
        {
            FCS32Server.Save(stream, list);
        }

        /// <summary>
        /// 保存到3.1版本
        /// </summary>
        /// <param name="filePath">保存的文件路径</param>
        /// <param name="list">fcs对象集合</param>
        /// <returns>是否保存成功</returns>
        public static void SaveToFCS31(string filePath, params FCS[] list)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                SaveToFCS31(stream, list);
            }
        }
        /// <summary>
        /// 保存到3.1版本
        /// </summary>
        /// <param name="stream">可保存的流</param>
        /// <param name="list">fcs对象集合</param>
        /// <returns>是否保存成功</returns>
        public static void SaveToFCS31(Stream stream, params FCS[] list)
        {
            FCS31Server.Save(stream, list);
        }

        /// <summary>
        /// 保存到3.0版本
        /// </summary>
        /// <param name="filePath">保存的文件路径</param>
        /// <param name="list">fcs对象集合</param>
        /// <returns>是否保存成功</returns>
        public static void SaveToFCS30(string filePath, params FCS[] list)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                SaveToFCS30(stream, list);
            }
        }
        /// <summary>
        /// 保存到3.0版本
        /// </summary>
        /// <param name="stream">可保存的流</param>
        /// <param name="list">fcs对象集合</param>
        /// <returns>是否保存成功</returns>
        public static void SaveToFCS30(Stream stream, params FCS[] list)
        {
            FCS30Server.Save(stream, list);
        }
        #endregion
    }
}
