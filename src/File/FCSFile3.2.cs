using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FCS.File
{
    public class FCSFile3_2 : IFCSFile
    {
        public static FCSFile3_2 FCSFileServer { get; } = new FCSFile3_2();

        /// <summary>
        /// 保存一组数据集到文件
        /// </summary>
        /// <param name="stream">可写流</param>
        /// <param name="list">数据集数组</param>
        /// <returns></returns>
        public override bool Save(Stream stream, params FCS[] list)
        {
            if (!stream.CanWrite || !stream.CanSeek) throw new Exception("Save failed,stream can't write or seek");
            if (list == null || list.Length <= 0) throw new Exception("Save failed,dataset is null or empty");

            return true;
        }
    }
}
