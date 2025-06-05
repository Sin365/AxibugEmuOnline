using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace AxibugEmuOnline.Server.Common
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
            using (var compressedMemoryStream = new System.IO.MemoryStream())
            using (var gzipStream = new GZipStream(compressedMemoryStream, CompressionMode.Compress))
            {
                gzipStream.Write(bytesToCompress, 0, bytesToCompress.Length);
                gzipStream.Close();
                return compressedMemoryStream.ToArray();
            }
        }

        public static byte[] DecompressByteArray(byte[] compressedBytes)
        {
            using (var compressedMemoryStream = new System.IO.MemoryStream(compressedBytes))
            using (var gzipStream = new GZipStream(compressedMemoryStream, CompressionMode.Decompress))
            using (var resultMemoryStream = new System.IO.MemoryStream())
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
    }
}
