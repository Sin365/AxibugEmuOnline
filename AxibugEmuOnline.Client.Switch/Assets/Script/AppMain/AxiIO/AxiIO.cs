using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxiIO
{
    public static class AxiIO
    {
        static IAxiIO m_io;
        #region debug用
        public static bool m_bDebugStepBreak = false;
        static int m_StepBreakCount = 0;
        public static void SetDebugStep(int step)
        {
            m_bDebugStepBreak = true;
            m_StepBreakCount = Math.Max(0, step);
            Debug.Log("[AxiIO]设置步进中断数" + step);
        }

        public static bool CheckCanStep(
            string path, string method = "")
        {
            if (!m_bDebugStepBreak)
            {
                return true;
            }
            string temp = $"调用 {method} do->{path}";
            if (m_StepBreakCount < 1)
            {
                Debug.Log("[AxiIoDbg]步进中断:" + temp + "");
                return false;
            }
            Debug.Log("[AxiIoDbg]步进进行" + "[" + m_StepBreakCount + "]" + ":" + temp + "");
            m_StepBreakCount--;
            return true;
        }

        public static void ClearDbgStep()
        {
            m_bDebugStepBreak = false;
        }
        #endregion
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


#if UNITY_SWITCH
                //WTF 至今不明，很诡异，在NS上如果每次IO行为有打印，则“完美”不会崩溃。如果没有打印，则IO时可能会崩溃。
                // 这明明是单线程程序，亦或者NS文件系统内部是不同线程某种线程安全？亦或者是 需要等待某种结束，或者访问频率？
                // 总之，保留在每一个IO行为都有打印，就可以确保，不触发2168-0002
                var stack = new System.Diagnostics.StackTrace(1, false);
                var caller = stack.GetFrame(0)?.GetMethod()?.Name ?? "Unknown";
                var callerParent = stack.GetFrame(1)?.GetMethod()?.Name ?? "Unknown";
                Debug.Log(
                    $"[NS专有打印] 步进中断: 来自{callerParent}>{caller}"
                );
#endif
                return m_io;
            }
        }
    }
    public static class File
    {
        internal static void Delete(string filePath)
        {
            if (AxiIO.m_bDebugStepBreak && !AxiIO.CheckCanStep(filePath, System.Reflection.MethodBase.GetCurrentMethod().Name)) return;
            AxiIO.io.file_Delete(filePath);
        }

        internal static bool Exists(string filePath)
        {
            if (AxiIO.m_bDebugStepBreak && !AxiIO.CheckCanStep(filePath, System.Reflection.MethodBase.GetCurrentMethod().Name)) return default;
            return AxiIO.io.file_Exists(filePath);
        }

        internal static byte[] ReadAllBytes(string filePath)
        {
            if (AxiIO.m_bDebugStepBreak && !AxiIO.CheckCanStep(filePath, System.Reflection.MethodBase.GetCurrentMethod().Name)) return default;
            return AxiIO.io.file_ReadAllBytes(filePath);
        }
        internal static int ReadBytesToArr(string filePath, byte[] readToArr, int start, int len)
        {
            if (AxiIO.m_bDebugStepBreak && !AxiIO.CheckCanStep(filePath, System.Reflection.MethodBase.GetCurrentMethod().Name)) return default;
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
            if (AxiIO.m_bDebugStepBreak && !AxiIO.CheckCanStep(path, System.Reflection.MethodBase.GetCurrentMethod().Name)) return;
            AxiIO.io.file_WriteAllBytes(path, data, ImmediatelyCommit);
        }

        internal static void WriteAllBytesFromStream(string path, System.IO.MemoryStream ms)
        {
            if (AxiIO.m_bDebugStepBreak && !AxiIO.CheckCanStep(path, System.Reflection.MethodBase.GetCurrentMethod().Name)) return;
            AxiIO.io.file_WriteAllBytes(path, ms);
        }
    }

    public static class Directory
    {
        public static bool Exists(string dirpath)
        {
            if (AxiIO.m_bDebugStepBreak && !AxiIO.CheckCanStep(dirpath, System.Reflection.MethodBase.GetCurrentMethod().Name)) return default;
            return AxiIO.io.dir_Exists(dirpath);
        }

        public static void CreateDirectory(string dirpath)
        {
            if (AxiIO.m_bDebugStepBreak && !AxiIO.CheckCanStep(dirpath, System.Reflection.MethodBase.GetCurrentMethod().Name)) return;
            AxiIO.io.dir_CreateDirectory(dirpath);
        }

        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            if (AxiIO.m_bDebugStepBreak && !AxiIO.CheckCanStep(path, System.Reflection.MethodBase.GetCurrentMethod().Name)) return default;
            return AxiIO.io.dir_EnumerateFiles(path, searchPattern);
        }

        public static string[] GetDirectories(string path)
        {
            if (AxiIO.m_bDebugStepBreak && !AxiIO.CheckCanStep(path, System.Reflection.MethodBase.GetCurrentMethod().Name)) return default;
            return AxiIO.io.dir_GetDirectories(path);
        }

        public static string[] GetFiles(string path)
        {
            if (AxiIO.m_bDebugStepBreak && !AxiIO.CheckCanStep(path, System.Reflection.MethodBase.GetCurrentMethod().Name)) return default;
            return AxiIO.io.dir_GetFiles(path);
        }

        internal static void Delete(string cacheDirPath, bool v)
        {
            if (AxiIO.m_bDebugStepBreak && !AxiIO.CheckCanStep(cacheDirPath, System.Reflection.MethodBase.GetCurrentMethod().Name)) return;
            AxiIO.io.dir_Delete(cacheDirPath, v);
        }
    }
}