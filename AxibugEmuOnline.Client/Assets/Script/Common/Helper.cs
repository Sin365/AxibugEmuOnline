using System;
using System.IO;
using System.IO.Compression;

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
    }
}
