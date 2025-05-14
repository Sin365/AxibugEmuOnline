﻿using System.Collections.Generic;

namespace AxiIO
{

    public static class AxiIO
    {
        static IAxiIO m_io;
        public static IAxiIO io
        {
            get
            {
                if (m_io == null)
                {
#if UNITY_SWITCH && !UNITY_EDITOR
                        m_io = new NSwitchIO();
#else
                    m_io = new CSharpIO();
#endif
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
        internal static int ReadBytesToArr(string filePath, byte[] readToArr, int start, int len)
        {
            return AxiIO.io.file_ReadBytesToArr(filePath, readToArr, start, len);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="ImmediatelyCommit">是否立即Commit到物理存储（目前只有NS对本参数有效）</param>
        internal static void WriteAllBytes(string path, byte[] data, bool ImmediatelyCommit = true)
        {
            AxiIO.io.file_WriteAllBytes(path, data, ImmediatelyCommit);
        }

        internal static void WriteAllBytesFromStream(string path, System.IO.MemoryStream ms)
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

        public static string[] GetDirectories(string path)
        {
            return AxiIO.io.dir_GetDirectories(path);
        }

        public static string[] GetFiles(string path)
        {
            return AxiIO.io.dir_GetFiles(path);
        }

        internal static void Delete(string cacheDirPath, bool v)
        {
            AxiIO.io.dir_Delete(cacheDirPath, v);
        }
    }
}