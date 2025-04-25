using System;
using System.Collections.Generic;
using System.IO;

namespace AxiIO
{
    public class NintendoSwitchIO : IAxiIO
    {
        public NintendoSwitchIO()
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
            throw new NotImplementedException();
        }

        public bool dir_Exists(string dirpath)
        {
            return AxiNS.instance.io.CheckPathExists(dirpath);
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

        public void file_WriteAllBytes(string filePath, byte[] data)
        {
            AxiNS.instance.io.FileToSaveWithCreate(filePath, data);
        }

        public void file_WriteAllBytes(string filePath, MemoryStream ms)
        {
            AxiNS.instance.io.FileToSaveWithCreate(filePath, ms);
        }
    }
}