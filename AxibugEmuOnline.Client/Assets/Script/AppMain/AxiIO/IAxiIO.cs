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
        void file_WriteAllBytes(string filePath, byte[] data);
        void file_WriteAllBytes(string filePath, System.IO.MemoryStream ms);
        int file_ReadBytesToArr(string filePath, byte[] readToArr, int start, int len);
    };
}