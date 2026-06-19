using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxiIO
{
    public class NSwitchIO : IAxiIO
    {
        public NSwitchIO()
        {
            Debug.Log($"NSwitchIO Init");
            AxiNS.instance.Init();
        }
        public void Ping()
        {
            throw new NotImplementedException();
        }
        public void dir_CreateDirectory(string dirpath)
        {
            AxiNS.instance.io.CreateDir(dirpath);
        }
        public void dir_Delete(string path, bool recursive)
        {
            if (recursive)
                AxiNS.instance.io.DeletePathDirRecursively(path);
            else
                AxiNS.instance.io.DeletePathDir(path);
        }
        public IEnumerable<string> dir_EnumerateFiles(string path, string searchPattern)
        {
            return AxiNS.instance.io.EnumerateFiles(path, searchPattern);
        }
        public bool dir_Exists(string dirpath)
        {
            if (string.IsNullOrWhiteSpace(dirpath))
                return false;
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
            if (string.IsNullOrWhiteSpace(filePath))
                return false;
            bool result = AxiNS.instance.io.CheckPathExists(filePath);
            return result;
        }
        public byte[] file_ReadAllBytes(string filePath)
        {
            return AxiNS.instance.io.LoadSwitchDataFile(filePath);
        }
        public int file_ReadBytesToArr(string filePath, byte[] readToArr, int start, int len)
        {
            byte[] bytes = file_ReadAllBytes(filePath);
            if (bytes == null)
                return 0;
            if (readToArr == null)
                throw new ArgumentNullException(nameof(readToArr));
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (len < 0)
                throw new ArgumentOutOfRangeException(nameof(len));

            int availableInBuffer = Math.Max(0, readToArr.Length - start);
            int templen = Math.Min(len, Math.Min(bytes.Length, availableInBuffer));
            if (templen <= 0)
                return 0;
            Array.Copy(bytes, 0, readToArr, start, templen);
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
            AxiNS.instance.io.FileToSaveWithCreate(filePath, data, immediatelyCommit);
        }
        public void file_WriteAllBytes(string filePath, System.IO.MemoryStream ms)
        {
            AxiNS.instance.io.FileToSaveWithCreate(filePath, ms);
        }
    }
}