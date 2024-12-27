using System;
using System.Runtime.InteropServices;
using System.Text;

namespace AxiReplay
{
    [StructLayout(LayoutKind.Explicit, Size = 44)]
    public struct ReplayHandler
    {
        [FieldOffset(0)]
        public int Format;
        [FieldOffset(sizeof(int) * 1)]
        public int RomID;
        [FieldOffset(sizeof(int) * 2)]
        public int RomType;
        [FieldOffset(sizeof(int) * 3)]
        public int DataOffset;
        [FieldOffset(sizeof(int) * 4)]
        public int TitleOffset;
        [FieldOffset(sizeof(int) * 5)]
        public int NoteOffset;
        [FieldOffset(sizeof(int) * 6)]
        public int AllFrame;
        [FieldOffset(sizeof(int) * 7)]
        public int AllTime;
        [FieldOffset(sizeof(int) * 8)]
        public int SingleLenght;
        [FieldOffset(sizeof(int) * 9)]
        public long CreateTime;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ReplayStep
    {
        [FieldOffset(0)]
        public UInt64 All64Data;
        [FieldOffset(0)]
        public Int32 FrameStartID;
        [FieldOffset(4)]
        public UInt64 InPut;
    }

    public static class ReplayData
    {
        public static int HandlerLenght = sizeof(int) * 9 + sizeof(long);
        public enum ReplayFormat : byte
        {
            None = 0,
            FM32IPBYTE,
            FM32IP16,
            FM32IP32,
            FM32IP64,
        }
        public static void GetStringByteData(string str, out byte[] data, out int lenghtWithEnd, Encoding encoding)
        {
            data = encoding.GetBytes(str);
            lenghtWithEnd = data.Length + 1;
        }

        public static byte[] GetHandlerData(ReplayHandler replayhandler)
        {
            int size = Marshal.SizeOf(typeof(ReplayHandler));
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(replayhandler, ptr, false);
                Marshal.Copy(ptr, arr, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return arr;
        }

        public static ReplayHandler GetReplayHandlerFromData(byte[] data)
        {
            if (data == null || data.Length < ReplayData.HandlerLenght)
            {
                throw new ArgumentException("Invalid data length or null data.");
            }

            IntPtr ptr = Marshal.AllocHGlobal(ReplayData.HandlerLenght);
            try
            {
                // 将byte数组的内容复制到非托管内存中  
                Marshal.Copy(data, 0, ptr, ReplayData.HandlerLenght);
                // 从非托管内存将内容转换回ReplayHandler结构体  
                return (ReplayHandler)Marshal.PtrToStructure(ptr, typeof(ReplayHandler));
            }
            finally
            {
                // 释放非托管内存  
                Marshal.FreeHGlobal(ptr);
            }
        }
    }
}
