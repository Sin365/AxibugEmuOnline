using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace AxibugEmuOnline.Client.Common
{
    public static class Helper
    {
        public static long GetNowTimeStamp()
        {
            return GetTimeStamp(DateTime.Now);
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStamp(DateTime dt)
        {
            TimeSpan ts = dt - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }

        public static byte[] CompressByteArray(byte[] bytesToCompress)
        {
            using (var compressedMemoryStream = new MemoryStream())
            using (var gzipStream = new GZipStream(compressedMemoryStream, CompressionMode.Compress))
            {
                gzipStream.Write(bytesToCompress, 0, bytesToCompress.Length);
                gzipStream.Close();
                return compressedMemoryStream.ToArray();
            }
        }

        public static byte[] DecompressByteArray(byte[] compressedBytes)
        {
            using (var compressedMemoryStream = new MemoryStream(compressedBytes))
            using (var gzipStream = new GZipStream(compressedMemoryStream, CompressionMode.Decompress))
            using (var resultMemoryStream = new MemoryStream())
            {
                gzipStream.CopyTo(resultMemoryStream);
                return resultMemoryStream.ToArray();
            }
        }

        public static string FileMD5Hash(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = md5.ComputeHash(stream);
                    var sb = new StringBuilder(hash.Length * 2);
                    foreach (var b in hash)
                        sb.AppendFormat("{0:x2}", b);
                    return sb.ToString();
                }
            }
        }

        static byte[] FileMD5HashByte(byte[] data)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = new MemoryStream(data))
                {
                    return md5.ComputeHash(stream);
                }
            }
        }

        /// <summary>
        /// 单个文件hash
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string FileMD5Hash(byte[] data)
        {
            byte[] hash = FileMD5HashByte(data);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash)
                sb.AppendFormat("{0:x2}", b);
            return sb.ToString();
        }

        /// <summary>
        /// 多文件总hash (顺序不影响结果)
        /// </summary>
        /// <param name="dataList"></param>
        /// <returns></returns>
        public static string FileMD5Hash(List<byte[]> dataList)
        {
            string allhash = string.Empty;
            List<string> temp = new List<string>();
            for (int i = 0; i < dataList.Count; i++)
            {
                if (dataList[i] == null)
                    continue;
                temp.Add(FileMD5Hash(dataList[i]));
            }
            temp.Sort();
            var sb = new StringBuilder();
            for (int i = 0; i < temp.Count; i++)
            {
                sb.AppendLine(temp[i].ToString());
            }

            //这里用Ascll
            return FileMD5Hash(Encoding.ASCII.GetBytes(sb.ToString()));
        }
    }
}
