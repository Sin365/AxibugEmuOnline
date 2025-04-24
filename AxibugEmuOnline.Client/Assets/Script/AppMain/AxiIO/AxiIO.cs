using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
    };

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

    }
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

        public void file_WriteAllBytes(string filePath, byte[] data)
        {
            AxiNS.instance.io.FileToSaveWithCreate(filePath, data);
        }

        public void file_WriteAllBytes(string filePath, MemoryStream ms)
        {
            AxiNS.instance.io.FileToSaveWithCreate(filePath,ms);
        }
    }
    public static class AxiIO
    {
        static IAxiIO m_io;
        public static IAxiIO io
        {
            get
            {
                if (m_io == null)
                {
                    if (UnityEngine.Application.platform == RuntimePlatform.Switch)
                        m_io = new NintendoSwitchIO();
                    else
                        m_io = new CSharpIO();
                }
                return m_io;
            }
        }
    }
    public static class File
    {
        internal static void Delete(string filePath)
        {
            AxiIO.io.file_Delete(filePath);
        }

        internal static bool Exists(string filePath)
        {
            return AxiIO.io.file_Exists(filePath);
        }

        internal static byte[] ReadAllBytes(string filePath)
        {
            return AxiIO.io.file_ReadAllBytes(filePath);
        }

        internal static void WriteAllBytes(string path, byte[] data)
        {
            AxiIO.io.file_WriteAllBytes(path, data);
        }

        internal static void WriteAllBytesFromStream(string path, MemoryStream ms)
        {
            AxiIO.io.file_WriteAllBytes(path, ms);
        }
    }
    public static class Directory
    {
        public static bool Exists(string dirpath)
        {
            return AxiIO.io.dir_Exists(dirpath);
        }

        public static void CreateDirectory(string dirpath)
        {
            AxiIO.io.dir_CreateDirectory(dirpath);
        }

        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            return AxiIO.io.dir_EnumerateFiles(path, searchPattern);
        }

        internal static void Delete(string cacheDirPath, bool v)
        {
            AxiIO.io.dir_Delete(cacheDirPath, v);
        }
    }
}