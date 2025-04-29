using System;
using System.Collections.Generic;

namespace AxiIO
{
    public class NSwitchIO : IAxiIO
    {
        public NSwitchIO()
        {
            AxiNS.instance.Init();
        }
        public void dir_CreateDirectory(string dirpath)
        {
            AxiNS.instance.io.CreateDir(dirpath);
        }

        public void dir_Delete(string path, bool recursive)
        {
            AxiNS.instance.io.DeletePathFile(path);
        }

        public IEnumerable<string> dir_EnumerateFiles(string path, string searchPattern)
        {
            return AxiNS.instance.io.EnumerateFiles(path, searchPattern);
        }

        public bool dir_Exists(string dirpath)
        {
            return AxiNS.instance.io.CheckPathExists(dirpath);
        }

        public string[] dir_GetDirectories(string path)
        {
            if (!AxiNS.instance.io.GetDirectoryDirs(path, out string[] result))
            {
                return new string[0];
            }
            return result;
        }

        public string[] dir_GetFiles(string path)
        {
            if (!AxiNS.instance.io.GetDirectoryFiles(path, out string[] result))
            {
                return new string[0];
            }
            return result;
        }

        public void file_Delete(string filePath)
        {
            AxiNS.instance.io.DeletePathFile(filePath);
        }

        public bool file_Exists(string filePath)
        {
            return AxiNS.instance.io.CheckPathExists(filePath);
        }

        public byte[] file_ReadAllBytes(string filePath)
        {
            return AxiNS.instance.io.LoadSwitchDataFile(filePath);
        }

        public int file_ReadBytesToArr(string filePath, byte[] readToArr, int start, int len)
        {
            byte[] bytes = file_ReadAllBytes(filePath);
            int templen = Math.Min(len, bytes.Length);
            Array.Copy(readToArr, readToArr, len);
            return templen;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="data"></param>
        /// <param name="immediatelyCommit">是否立即Commit到物理存储</param>
        public void file_WriteAllBytes(string filePath, byte[] data, bool immediatelyCommit = true)
        {
            AxiNS.instance.io.FileToSaveWithCreate(filePath, data);
        }

        public void file_WriteAllBytes(string filePath, System.IO.MemoryStream ms)
        {
            AxiNS.instance.io.FileToSaveWithCreate(filePath, ms);
        }
    }
}