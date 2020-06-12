using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FCS
{
    public class Factory
    {
        /// <summary>
        /// 读取fcs文件，返回fcs对象
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static IEnumerable<IFCS> ReadFCSFile(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            using var fileStream = fileInfo.OpenRead();
            List<IFCS> list = new List<IFCS>();
            var fcs = ReadOneFCS(fileStream, 0);
            if (fcs != null)
            {
                list.Add(fcs);
                while (fcs != null && fcs.NextData != 0)
                {
                    fcs = ReadOneFCS(fileStream, fcs.NextData);
                    if (fcs != null) list.Add(fcs);
                }
            }
            fileStream.Close();
            return list;
        }

        /// <summary>
        /// 读取一个fcs数据集
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private static IFCS ReadOneFCS(FileStream fileStream, long offset)
        {
            var version = ReadVersion(fileStream, offset);
            IFCS fcs = version switch
            {
                "FCS3.1" => new FCS3_1(fileStream),
                "FCS3.0" => new FCS3_0(fileStream),
                _ => null,
            };
            return fcs;
        }

        /// <summary>
        /// 读取版本号
        /// </summary>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        private static string ReadVersion(FileStream fileStream, long offset)
        {
            if (fileStream.Length < 6) return null;
            byte[] bytes = new byte[6];
            fileStream.Seek(offset, SeekOrigin.Begin);
            if (fileStream.Read(bytes, 0, 6) != 6) return null;
            return Encoding.ASCII.GetString(bytes).ToUpper();
        }
    }
}
