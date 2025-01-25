using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace MAME.Core
{
    public unsafe class Memory
    {
        //public static byte[] mainrom, audiorom, mainram, audioram;


        #region //指针化mainrom
        static byte[] mainrom_src;
        static GCHandle mainrom_handle;
        public static byte* mainrom;
        public static int mainromLength;
        public static bool mainrom_IsNull => mainrom == null;
        public static void Set_mainrom(byte[] data) { mainrom_set = data; }
        public static byte[] mainrom_set
        {
            set
            {
                mainrom_handle.ReleaseGCHandle();
                mainrom_src = value;
                mainromLength = value.Length;
                mainrom_src.GetObjectPtr(ref mainrom_handle, ref mainrom);
            }
        }
        #endregion

        #region //指针化audiorom
        static byte[] audiorom_src;
        static GCHandle audiorom_handle;
        public static byte* audiorom;
        public static int audioromLength;
        public static bool audiorom_IsNull => audiorom == null;
        public static void Set_audiorom(byte[] data) { audiorom_set = data; }
        public static byte[] audiorom_set
        {
            set
            {
                audiorom_handle.ReleaseGCHandle();
                audiorom_src = value;
                audioromLength = value.Length;
                audiorom_src.GetObjectPtr(ref audiorom_handle, ref audiorom);
            }
        }
        #endregion

        #region //指针化mainram
        static byte[] mainram_src;
        static GCHandle mainram_handle;
        public static byte* mainram;
        public static int mainramLength;
        public static bool mainram_IsNull => mainram == null;
        public static void Set_mainram(byte[] data) { mainram_set = data; }
        public static byte[] mainram_set
        {
            set
            {
                mainram_handle.ReleaseGCHandle();
                mainram_src = value;
                mainramLength = value.Length;
                mainram_src.GetObjectPtr(ref mainram_handle, ref mainram);
            }
        }
        #endregion

        #region //指针化audioram
        static byte[] audioram_src;
        static GCHandle audioram_handle;
        public static byte* audioram;
        public static int audioramLength;
        public static bool audioram_IsNull => audioram == null;
        public static void Set_audioram(byte[] data) { audioram_set = data; }
        public static byte[] audioram_set
        {
            set
            {
                audioram_handle.ReleaseGCHandle();
                audioram_src = value;
                audioramLength = value.Length;
                audioram_src.GetObjectPtr(ref audioram_handle, ref audioram);
            }
        }
        #endregion


        public static void memory_reset()
        {
            switch (Machine.sBoard)
            {
                case "CPS-1":
                case "CPS-1(QSound)":
                    CPS.sbyte0 = -1;
                    CPS.short1 = -1;
                    CPS.short2 = -1;
                    CPS.sbyte3 = -1;
                    break;
                case "CPS2":
                    CPS.short0 = -1;
                    CPS.short1 = -1;
                    CPS.short2 = -1;
                    break;
                case "Data East":
                    Dataeast.byte1 = 0xff;
                    Dataeast.byte2 = 0xff;
                    break;
                case "Tehkan":
                    Tehkan.byte0 = 0;
                    Tehkan.byte1 = 0;
                    Tehkan.byte2 = 0;
                    break;
                case "Neo Geo":
                    Neogeo.short0 = unchecked((short)0xff00);
                    Neogeo.short1 = -1;
                    Neogeo.short2 = -1;
                    Neogeo.short3 = 0x3f;
                    Neogeo.short4 = unchecked((short)0xffc0);
                    Neogeo.short5 = unchecked((short)0xffff);
                    Neogeo.short6 = 0x03;
                    break;
                case "Namco System 1":
                    Namcos1.byte0 = 0xff;
                    Namcos1.byte1 = 0xff;
                    Namcos1.byte2 = 0xff;
                    switch (Machine.sName)
                    {
                        case "faceoff":
                            Namcos1.byte00 = 0xff;
                            Namcos1.byte01 = 0xff;
                            Namcos1.byte02 = 0xff;
                            Namcos1.byte03 = 0xff;
                            break;
                    }
                    break;
                case "IGS011":
                    switch (Machine.sName)
                    {
                        case "drgnwrld":
                        case "drgnwrldv30":
                        case "drgnwrldv21":
                        case "drgnwrldv21j":
                        case "drgnwrldv20j":
                        case "drgnwrldv10c":
                        case "drgnwrldv11h":
                        case "drgnwrldv40k":
                            IGS011.sbyte0 = -1;
                            IGS011.sbyte1 = -1;
                            IGS011.sbyte2 = -1;
                            IGS011.sbytec = -1;
                            break;
                        case "lhb":
                        case "lhbv33c":
                        case "dbc":
                        case "ryukobou":
                            IGS011.bkey0 = 0xff;
                            IGS011.bkey1 = 0xff;
                            IGS011.bkey2 = 0xff;
                            IGS011.bkey3 = 0xff;
                            IGS011.bkey4 = 0xff;
                            IGS011.sbytec = -1;
                            break;
                    }
                    break;
                case "PGM":
                    PGM.short0 = -1;
                    PGM.short1 = -1;
                    PGM.short2 = -1;
                    PGM.short3 = 0xff;
                    PGM.short4 = 0;
                    break;
                case "M72":
                    M72.ushort0 = 0xffff;
                    M72.ushort1 = 0xffff;
                    break;
                case "M92":
                    M92.ushort0 = 0xffff;
                    M92.ushort1 = 0xff7f;
                    M92.ushort2 = 0xffff;
                    break;
                case "Taito":
                    Taito.sbyte0 = unchecked((sbyte)0xf3);
                    Taito.sbyte1 = -1;
                    Taito.sbyte2 = -1;
                    switch (Machine.sName)
                    {
                        case "tokio":
                        case "tokioo":
                        case "tokiou":
                        case "tokiob":
                            Taito.sbyte0 = unchecked((sbyte)0xd3);
                            break;
                        case "sboblbobl":
                            Taito.sbyte0 = unchecked((sbyte)0x73);
                            break;
                        case "opwolf":
                            Taito.sbyte0 = unchecked((sbyte)0xfc);
                            break;
                        case "opwolfp":
                            Taito.sbyte2 = 0x3e;
                            Taito.sbyte3 = 0;
                            break;
                    }
                    break;
                case "Taito B":
                    Taitob.sbyte0 = -1;
                    Taitob.sbyte1 = -1;
                    Taitob.sbyte2 = -1;
                    Taitob.sbyte3 = -1;
                    Taitob.sbyte4 = -1;
                    Taitob.sbyte5 = -1;
                    break;
                case "Konami 68000":
                    Konami68000.sbyte0 = -1;
                    Konami68000.sbyte1 = -1;
                    Konami68000.sbyte2 = -1;
                    Konami68000.sbyte3 = -1;
                    Konami68000.sbyte4 = -1;
                    switch (Machine.sName)
                    {
                        case "lgtnfght":
                        case "lgtnfghta":
                        case "lgtnfghtu":
                        case "trigon":
                            Konami68000.sbyte0 = unchecked((sbyte)0xfb);
                            break;
                        case "prmrsocr":
                        case "prmrsocrj":
                            Konami68000.sbyte0 = unchecked((sbyte)0xef);
                            Konami68000.bytee = 0xfe;
                            break;
                    }
                    break;
                case "Capcom":
                    switch (Machine.sName)
                    {
                        case "gng":
                        case "gnga":
                        case "gngbl":
                        case "gngprot":
                        case "gngblita":
                        case "gngc":
                        case "gngt":
                        case "makaimur":
                        case "makaimurc":
                        case "makaimurg":
                        case "diamond":
                            Capcom.bytes = 0xff;
                            Capcom.byte1 = 0xff;
                            Capcom.byte2 = 0xff;
                            break;
                        case "sf":
                            Capcom.short0 = -1;
                            Capcom.short1 = -1;
                            break;
                        case "sfua":
                        case "sfj":
                            Capcom.short1 = -1;
                            Capcom.short2 = -1;
                            Capcom.shortc = -1;
                            break;
                        case "sfjan":
                        case "sfan":
                        case "sfp":
                            Capcom.short0 = -1;
                            Capcom.shortc = -1;
                            Capcom.sbyte1 = 0;
                            Capcom.sbyte2 = 0;
                            Capcom.sbyte3 = 0;
                            Capcom.sbyte4 = 0;
                            break;
                    }
                    break;
            }
        }
        public static void memory_reset2()
        {
            switch (Machine.sBoard)
            {
                case "CPS-1":
                case "CPS-1(QSound)":
                    CPS.sbyte0_old = 0;
                    CPS.short1_old = 0;
                    CPS.short2_old = 0;
                    CPS.sbyte3_old = 0;
                    break;
                case "CPS2":
                    CPS.short0_old = 0;
                    CPS.short1_old = 0;
                    CPS.short2_old = 0;
                    break;
                case "Data East":
                    Dataeast.byte1_old = 0;
                    Dataeast.byte2_old = 0;
                    break;
                case "Tehkan":
                    Tehkan.byte0_old = 0;
                    Tehkan.byte1_old = 0;
                    Tehkan.byte2_old = 0;
                    break;
                case "Neo Geo":
                    Neogeo.short0_old = 0;
                    Neogeo.short1_old = 0;
                    Neogeo.short2_old = 0;
                    Neogeo.short3_old = 0;
                    Neogeo.short4_old = 0;
                    break;
                case "Namco System 1":
                    Namcos1.byte0_old = 0;
                    Namcos1.byte1_old = 0;
                    Namcos1.byte2_old = 0;
                    break;
                case "IGS011":
                    IGS011.sbyte0_old = 0;
                    IGS011.sbyte1_old = 0;
                    IGS011.sbyte2_old = 0;
                    IGS011.sbytec_old = 0;
                    break;
                case "PGM":
                    PGM.short0_old = 0;
                    PGM.short1_old = 0;
                    PGM.short2_old = 0;
                    PGM.short3_old = 0;
                    PGM.short4_old = 0;
                    break;
                case "M72":
                    M72.ushort0_old = 0;
                    M72.ushort1_old = 0;
                    break;
                case "M92":
                    M92.ushort0_old = 0;
                    M92.ushort1_old = 0;
                    M92.ushort2_old = 0;
                    break;
                case "Taito":
                    Taito.sbyte0_old = 0;
                    Taito.sbyte1_old = 0;
                    Taito.sbyte2_old = 0;
                    Taito.sbyte3_old = 0;
                    break;
                case "Taito B":
                    Taitob.dswb_old = 0;
                    Taitob.sbyte0_old = 0;
                    Taitob.sbyte1_old = 0;
                    Taitob.sbyte2_old = 0;
                    Taitob.sbyte3_old = 0;
                    Taitob.sbyte4_old = 0;
                    Taitob.sbyte5_old = 0;
                    break;
                case "Konami 68000":
                    Konami68000.sbyte0_old = 0;
                    Konami68000.sbyte1_old = 0;
                    Konami68000.sbyte2_old = 0;
                    Konami68000.sbyte3_old = 0;
                    Konami68000.sbyte4_old = 0;
                    switch (Machine.sName)
                    {
                        case "prmrsocr":
                        case "prmrsocrj":
                            Konami68000.bytee = 0;
                            break;
                    }
                    break;
                case "Capcom":
                    switch (Machine.sName)
                    {
                        case "gng":
                        case "gnga":
                        case "gngbl":
                        case "gngprot":
                        case "gngblita":
                        case "gngc":
                        case "gngt":
                        case "makaimur":
                        case "makaimurc":
                        case "makaimurg":
                        case "diamond":
                            Capcom.bytes_old = 0;
                            Capcom.byte1_old = 0;
                            Capcom.byte2_old = 0;
                            break;
                        case "sf":
                            Capcom.short0_old = 0;
                            Capcom.short1_old = 0;
                            Capcom.shorts_old = 0;
                            break;
                        case "sfua":
                        case "sfj":
                            Capcom.short1_old = 0;
                            Capcom.short2_old = 0;
                            Capcom.shortc_old = 0;
                            break;
                        case "sfjan":
                        case "sfan":
                        case "sfp":
                            Capcom.short0_old = 0;
                            Capcom.shortc_old = 0;
                            Capcom.sbyte1_old = 0;
                            Capcom.sbyte2_old = 0;
                            Capcom.sbyte3_old = 0;
                            Capcom.sbyte4_old = 0;
                            break;
                    }
                    break;
            }
        }

    }

    public unsafe static class AxiMemoryEx
    {
        static HashSet<GCHandle> GCHandles = new HashSet<GCHandle>();

        public static void Init()
        {
            FreeAllGCHandle();
            set_TempBuffer = new byte[0x40000];
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
            int singlesize = sizeof(byte);
            long totalBytesToCopy = count * singlesize;
            Buffer.MemoryCopy(&bufferPtr[offset], TempBuffer, totalBytesToCopy, totalBytesToCopy);
            bw.Write(TempBuffer_src, 0, count);
        }
        public static void Write(this FileStream fs, byte* bufferPtr, int offset, int count)
        {
            int singlesize = sizeof(byte);
            long totalBytesToCopy = count * singlesize;
            Buffer.MemoryCopy(&bufferPtr[offset], TempBuffer, totalBytesToCopy, totalBytesToCopy);
            fs.Write(TempBuffer_src, 0, count);
        }
        public static int Read(this FileStream fs, byte* bufferPtr, int offset, int count)
        {
            count = fs.Read(TempBuffer_src, offset, count);
            Buffer.MemoryCopy(TempBuffer, bufferPtr + offset, 0, count);
            return count;
        }
    }

    public unsafe static class AxiArray
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