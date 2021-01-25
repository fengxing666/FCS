using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FCS
{
    public class Factory
    {
        #region 读取文件
        /// <summary>
        /// 读取fcs文件，返回fcs对象集合
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static IEnumerable<FCS> ReadFile(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            using (var fileStream = fileInfo.OpenRead())
            {
                return Read(fileStream);
            }
        }
        /// <summary>
        /// 读取fcs文件，返回fcs对象集合
        /// </summary>
        /// <param name="stream">可读取的流</param>
        /// <returns></returns>
        public static IEnumerable<FCS> Read(Stream stream)
        {
            if (!stream.CanRead || !stream.CanSeek) throw new Exception("Stream can't read or seek");
            var version = ReadVersion(stream, 0);
            switch (version)
            {
                case "FCS3.2":
                    return File.FCSFile3_2.FCSFileServer.Read(stream);
                default:
                    throw new Exception("Version not supported");
            }
        }

        /// <summary>
        /// 读取fcs文件的一个数据集
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="nextData">下一个数据集的起点，相对于此方法返回的数据集起点</param>
        /// <param name="fileBeginOffset">相对于文件起点的位置</param>
        /// <returns></returns>
        public static FCS ReadFileOneDataset(string filePath, out long nextData, long fileBeginOffset = 0)
        {
            var fileInfo = new FileInfo(filePath);
            using (var fileStream = fileInfo.OpenRead())
            {
                return ReadOneDataset(fileStream, out nextData, fileBeginOffset);
            }
        }
        /// <summary>
        /// 读取一个fcs数据集
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="nextData">下一个数据集的起点，相对于此方法返回的数据集起点</param>
        /// <param name="fileBeginOffset">相对于流起点的位置</param>
        /// <returns></returns>
        public static FCS ReadOneDataset(Stream stream, out long nextData, long fileBeginOffset = 0)
        {
            if (!stream.CanRead || !stream.CanSeek) throw new Exception("Stream can't read or seek");
            var version = ReadVersion(stream, fileBeginOffset);
            switch (version)
            {
                case "FCS3.2":
                    return File.FCSFile3_2.FCSFileServer.ReadDataset(stream, out nextData, fileBeginOffset);
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
            if (!stream.CanRead || !stream.CanSeek) throw new Exception("Stream can't read or seek");
            if (stream.Length < (6 + offset)) throw new Exception("Offset is too big");
            byte[] bytes = new byte[6];
            stream.Seek(offset, SeekOrigin.Begin);
            if (stream.Read(bytes, 0, 6) != 6) throw new Exception("Version read failed");
            return Encoding.ASCII.GetString(bytes).ToUpper();
        }
        #endregion

        #region 保存文件



        #endregion
    }
}
