using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace StoicGooseUnity
{
    public static class StoicGooseUnityAxiMem
    {
        public static void Init() => AxiMemoryEx.Init();
        public static void FreeAllGCHandle() => AxiMemoryEx.FreeAllGCHandle();
    }
    internal unsafe static class AxiMemoryEx
    {
        static HashSet<GCHandle> GCHandles = new HashSet<GCHandle>();

        public static void Init()
        {
            FreeAllGCHandle();
            set_TempBuffer = new byte[0x100000];
        }

        public static void GetObjectPtr(this object srcObj, ref GCHandle handle, ref uint* ptr)
        {
            GetObjectPtr(srcObj, ref handle, out IntPtr intptr);
            ptr = (uint*)intptr;
        }

        public static void GetObjectPtr(this object srcObj, ref GCHandle handle, ref short* ptr)
        {
            GetObjectPtr(srcObj, ref handle, out IntPtr intptr);
            ptr = (short*)intptr;
        }
        public static void GetObjectPtr(this object srcObj, ref GCHandle handle, ref ushort* ptr)
        {
            GetObjectPtr(srcObj, ref handle, out IntPtr intptr);
            ptr = (ushort*)intptr;
        }
        public static void GetObjectPtr(this object srcObj, ref GCHandle handle, ref int* ptr)
        {
            GetObjectPtr(srcObj, ref handle, out IntPtr intptr);
            ptr = (int*)intptr;
        }
        public static void GetObjectPtr(this object srcObj, ref GCHandle handle, ref byte* ptr)
        {
            GetObjectPtr(srcObj, ref handle, out IntPtr intptr);
            ptr = (byte*)intptr;
        }

        public static void GetObjectPtr(this object srcObj, ref GCHandle handle, ref byte* ptr, out IntPtr intptr)
        {
            GetObjectPtr(srcObj, ref handle, out intptr);
            ptr = (byte*)intptr;
        }

        static void GetObjectPtr(this object srcObj, ref GCHandle handle, out IntPtr intptr)
        {
            ReleaseGCHandle(ref handle);
            handle = GCHandle.Alloc(srcObj, GCHandleType.Pinned);
            GCHandles.Add(handle);
            intptr = handle.AddrOfPinnedObject();
        }

        public static void ReleaseGCHandle(this ref GCHandle handle)
        {
            if (handle.IsAllocated)
                handle.Free();
            GCHandles.Remove(handle);
        }

        public static void FreeAllGCHandle()
        {
            foreach (var handle in GCHandles)
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
            GCHandles.Clear();
        }

        #region 指针化 TempBuffer
        static byte[] TempBuffer_src;
        static GCHandle TempBuffer_handle;
        public static byte* TempBuffer;
        public static byte[] set_TempBuffer
        {
            set
            {
                TempBuffer_handle.ReleaseGCHandle();
                if (value == null)
                    return;
                TempBuffer_src = value;
                TempBuffer_src.GetObjectPtr(ref TempBuffer_handle, ref TempBuffer);
            }
        }
        #endregion

        public static void Write(this BinaryWriter bw, byte* bufferPtr, int offset, int count)
        {
            // 使用指针复制数据到临时数组
            Buffer.MemoryCopy(bufferPtr + offset, TempBuffer, 0, count);
            // 使用BinaryWriter写入临时数组
            bw.Write(TempBuffer_src, 0, count);
        }
        public static void Write(this FileStream fs, byte* bufferPtr, int offset, int count)
        {
            // 使用指针复制数据到临时数组
            Buffer.MemoryCopy(bufferPtr + offset, TempBuffer, 0, count);
            // 使用BinaryWriter写入临时数组
            fs.Write(TempBuffer_src, 0, count);
        }
        public static int Read(this FileStream fs, byte* bufferPtr, int offset, int count)
        {
            // 使用BinaryWriter写入临时数组
            count = fs.Read(TempBuffer_src, offset, count);
            // 使用指针复制数据到临时数组
            Buffer.MemoryCopy(TempBuffer, bufferPtr + offset, 0, count);
            return count;
        }
    }

    internal unsafe static class AxiArray
    {

        public static void Copy(byte* src, int srcindex, byte* target, int targetindex, int count)
        {
            int singlesize = sizeof(byte);
            long totalBytesToCopy = count * singlesize;
            Buffer.MemoryCopy(&src[srcindex], &target[targetindex], totalBytesToCopy, totalBytesToCopy);
        }
        public static void Copy(short* src, int srcindex, short* target, int targetindex, int count)
        {
            int singlesize = sizeof(short);
            long totalBytesToCopy = count * singlesize;
            Buffer.MemoryCopy(&src[srcindex], &target[targetindex], totalBytesToCopy, totalBytesToCopy);
        }
        public static void Copy(ushort* src, int srcindex, ushort* target, int targetindex, int count)
        {
            int singlesize = sizeof(ushort);
            long totalBytesToCopy = count * singlesize;
            Buffer.MemoryCopy(&src[srcindex], &target[targetindex], totalBytesToCopy, totalBytesToCopy);
        }

        public static void Copy(byte* src, byte* target, int index, int count)
        {
            int singlesize = sizeof(byte);
            long totalBytesToCopy = count * singlesize;
            Buffer.MemoryCopy(&src[index], &target[index], totalBytesToCopy, totalBytesToCopy);
        }

        public static void Copy(ushort* src, ushort* target, int index, int count)
        {
            int singlesize = sizeof(ushort);
            long totalBytesToCopy = count * singlesize;
            Buffer.MemoryCopy(&src[index], &target[index], totalBytesToCopy, totalBytesToCopy);
        }
        public static void Copy(ushort* src, ushort* target, int count)
        {
            int singlesize = sizeof(ushort);
            long totalBytesToCopy = count * singlesize;
            Buffer.MemoryCopy(src, target, totalBytesToCopy, totalBytesToCopy);
        }
        public static void Copy(byte* src, byte* target, int count)
        {
            int singlesize = sizeof(byte);
            long totalBytesToCopy = count * singlesize;
            Buffer.MemoryCopy(src, target, totalBytesToCopy, totalBytesToCopy);
        }
        public static void Clear(byte* data, int index, int lenght)
        {
            for (int i = index; i < lenght; i++, index++)
                data[index] = 0;
        }
        public static void Clear(ushort* data, int index, int lenght)
        {
            for (int i = index; i < lenght; i++, index++)
                data[index] = 0;
        }
    }
}
