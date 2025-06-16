using System;
using System.Collections.Generic;

namespace AxiIO
{
    public interface IAxiIO
    {
        bool dir_Exists(string dirpath);
        void dir_CreateDirectory(string dirpath);
        IEnumerable<string> dir_EnumerateFiles(string path, string searchPattern);
        void dir_Delete(string path, bool recursive);
        byte[] file_ReadAllBytes(string filePath);
        bool file_Exists(string filePath);
        void file_Delete(string filePath);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="data"></param>
        /// <param name="immediatelyCommit">是否立即Commit到物理存储（目前只有NS对本参数有效）</param>
        void file_WriteAllBytes(string filePath, byte[] data, bool immediatelyCommit = true);
        void file_WriteAllBytes(string filePath, System.IO.MemoryStream ms);
        int file_ReadBytesToArr(string filePath, byte[] readToArr, int start, int len);
        string[] dir_GetDirectories(string path);
        string[] dir_GetFiles(string path);
		void Ping();
	};
}