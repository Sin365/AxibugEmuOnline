using System;
using System.Collections.Generic;
using System.IO;

namespace AxiIO
{
    public class CSharpIO : IAxiIO
    {
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

        public void file_WriteAllBytes(string filePath, byte[] data)
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
            FileStream streaming = System.IO.File.OpenRead(filePath);
            return streaming.Read(readToArr, 0, 4);
        }
    }
}