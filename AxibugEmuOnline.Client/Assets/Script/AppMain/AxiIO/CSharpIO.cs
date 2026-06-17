using System;
using System.Collections.Generic;
using System.IO;

namespace AxiIO
{
    public class CSharpIO : IAxiIO
	{
		public void Ping()
		{
			throw new NotImplementedException();
		}
		public void dir_CreateDirectory(string dirpath)
        {
            System.IO.Directory.CreateDirectory(dirpath);
        }

        public void dir_Delete(string path, bool recursive)
        {
            System.IO.Directory.Delete(path, recursive);
        }

        public IEnumerable<string> dir_EnumerateFiles(string path, string searchPattern)
        {
            return System.IO.Directory.EnumerateFiles(path, searchPattern);
        }

        public bool dir_Exists(string dirpath)
        {
            return System.IO.Directory.Exists(dirpath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="data"></param>
        /// <param name="immediatelyCommit">是否立即Commit到物理存储（C#原生这里不需要）</param>
        public void file_WriteAllBytes(string filePath, byte[] data, bool immediatelyCommit = true)
        {
            System.IO.File.WriteAllBytes(filePath, data);
        }

        public void file_WriteAllBytes(string filePath, MemoryStream ms)
        {
            System.IO.File.WriteAllBytes(filePath, ms.ToArray());
        }

        public void file_Delete(string filePath)
        {
            System.IO.File.Delete(filePath);
        }

        public bool file_Exists(string filePath)
        {
            return System.IO.File.Exists(filePath);
        }

        public byte[] file_ReadAllBytes(string filePath)
        {
            return System.IO.File.ReadAllBytes(filePath);
        }

        public int file_ReadBytesToArr(string filePath, byte[] readToArr, int start, int len)
        {
            using (FileStream streaming = System.IO.File.OpenRead(filePath))
            {
                if (readToArr == null)
                    throw new ArgumentNullException(nameof(readToArr));
                if (start < 0)
                    throw new ArgumentOutOfRangeException(nameof(start));
                if (len < 0)
                    throw new ArgumentOutOfRangeException(nameof(len));

                int available = Math.Max(0, readToArr.Length - start);
                int toRead = Math.Min(len, available);
                if (toRead == 0)
                    return 0;

                int readlen = streaming.Read(readToArr, start, toRead);
                return readlen;
            }
        }

        public string[] dir_GetDirectories(string path)
        {
            return System.IO.Directory.GetDirectories(path);
        }

        public string[] dir_GetFiles(string path)
        {
            return System.IO.Directory.GetFiles(path);
        }

	}
}