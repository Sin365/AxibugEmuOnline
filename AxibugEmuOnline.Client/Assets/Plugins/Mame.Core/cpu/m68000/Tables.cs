using MAME.Core;
using System;
using System.Runtime.InteropServices;

namespace cpu.m68000
{
    unsafe partial class MC68000
    {
        void InitMC68000Tables()
        {
            Init_MoveCyclesBW();
            Init_MoveCyclesL();
        }

        //static readonly int[,] MoveCyclesBW = new int[12, 9]
        //{
        //    { 4, 4, 8, 8, 8, 12, 14, 12, 16 },
        //    { 4, 4, 8, 8, 8, 12, 14, 12, 16 },
        //    { 8, 8, 12, 12, 12, 16, 18, 16, 20 },
        //    { 8, 8, 12, 12, 12, 16, 18, 16, 20 },
        //    { 10, 10, 14, 14, 14, 18, 20, 18, 22 },
        //    { 12, 12, 16, 16, 16, 20, 22, 20, 24 },
        //    { 14, 14, 18, 18, 18, 22, 24, 22, 26 },
        //    { 12, 12, 16, 16, 16, 20, 22, 20, 24 },
        //    { 16, 16, 20, 20, 20, 24, 26, 24, 28 },
        //    { 12, 12, 16, 16, 16, 20, 22, 20, 24 },
        //    { 14, 14, 18, 18, 18, 22, 24, 22, 26 },
        //    { 8, 8, 12, 12, 12, 16, 18, 16, 20 }
        //};

        //取值范例 Columns = 9 ; i* Columns + j

        const int MoveCyclesBW_len_1 = 12;
        const int MoveCyclesBW_Columns = 9;
        #region //指针化 MoveCyclesBW
        static byte[] MoveCyclesBW_src;
        static GCHandle MoveCyclesBW_handle;
        public static byte* MoveCyclesBW;
        public static int MoveCyclesBWLength;
        public static bool MoveCyclesBW_IsNull => MoveCyclesBW == null;
        public static byte[] MoveCyclesBW_set
        {
            set
            {
                MoveCyclesBW_handle.ReleaseGCHandle();
                MoveCyclesBW_src = value;
                MoveCyclesBWLength = value.Length;
                MoveCyclesBW_src.GetObjectPtr(ref MoveCyclesBW_handle, ref MoveCyclesBW);
            }
        }

        static void Init_MoveCyclesBW()
        {
            MoveCyclesBW_set = new byte[]
            {
                4, 4, 8, 8, 8, 12, 14, 12, 16,
                4, 4, 8, 8, 8, 12, 14, 12, 16,
                8, 8, 12, 12, 12, 16, 18, 16, 20,
                8, 8, 12, 12, 12, 16, 18, 16, 20,
                10, 10, 14, 14, 14, 18, 20, 18, 22,
                12, 12, 16, 16, 16, 20, 22, 20, 24,
                14, 14, 18, 18, 18, 22, 24, 22, 26,
                12, 12, 16, 16, 16, 20, 22, 20, 24,
                16, 16, 20, 20, 20, 24, 26, 24, 28,
                12, 12, 16, 16, 16, 20, 22, 20, 24,
                14, 14, 18, 18, 18, 22, 24, 22, 26,
                8, 8, 12, 12, 12, 16, 18, 16, 20
            };
        }

        #endregion



        //static readonly int[,] MoveCyclesL = new int[12, 9]
        //{
        //    { 4, 4, 12, 12, 12, 16, 18, 16, 20 },
        //    { 4, 4, 12, 12, 12, 16, 18, 16, 20 },
        //    { 12, 12, 20, 20, 20, 24, 26, 24, 28 },
        //    { 12, 12, 20, 20, 20, 24, 26, 24, 28 },
        //    { 14, 14, 22, 22, 22, 26, 28, 26, 30 },
        //    { 16, 16, 24, 24, 24, 28, 30, 28, 32 },
        //    { 18, 18, 26, 26, 26, 30, 32, 30, 34 },
        //    { 16, 16, 24, 24, 24, 28, 30, 28, 32 },
        //    { 20, 20, 28, 28, 28, 32, 34, 32, 36 },
        //    { 16, 16, 24, 24, 24, 28, 30, 28, 32 },
        //    { 18, 18, 26, 26, 26, 30, 32, 30, 34 },
        //    { 12, 12, 20, 20, 20, 24, 26, 24, 28 }
        //};


        const int MoveCyclesL_len_1 = 12;
        const int MoveCyclesL_Columns = 9;
        #region //指针化 MoveCyclesL
        static byte[] MoveCyclesL_src;
        static GCHandle MoveCyclesL_handle;
        public static byte* MoveCyclesL;
        public static int MoveCyclesLLength;
        public static bool MoveCyclesL_IsNull => MoveCyclesL == null;
        public static byte[] MoveCyclesL_set
        {
            set
            {
                MoveCyclesL_handle.ReleaseGCHandle();
                MoveCyclesL_src = value;
                MoveCyclesLLength = value.Length;
                MoveCyclesL_src.GetObjectPtr(ref MoveCyclesL_handle, ref MoveCyclesL);
            }
        }

        static void Init_MoveCyclesL()
        {
            MoveCyclesL_set = new byte[]
            {
                4, 4, 12, 12, 12, 16, 18, 16, 20 ,
                4, 4, 12, 12, 12, 16, 18, 16, 20 ,
                12, 12, 20, 20, 20, 24, 26, 24, 28 ,
                12, 12, 20, 20, 20, 24, 26, 24, 28 ,
                14, 14, 22, 22, 22, 26, 28, 26, 30 ,
                16, 16, 24, 24, 24, 28, 30, 28, 32 ,
                18, 18, 26, 26, 26, 30, 32, 30, 34 ,
                16, 16, 24, 24, 24, 28, 30, 28, 32 ,
                20, 20, 28, 28, 28, 32, 34, 32, 36 ,
                16, 16, 24, 24, 24, 28, 30, 28, 32 ,
                18, 18, 26, 26, 26, 30, 32, 30, 34 ,
                12, 12, 20, 20, 20, 24, 26, 24, 28
            };
        }
        #endregion


        static readonly int[,] EACyclesBW = new int[8, 8]
        {
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 4, 4, 4, 4, 4, 4, 4, 4 },
            { 4, 4, 4, 4, 4, 4, 4, 4 },
            { 6, 6, 6, 6, 6, 6, 6, 6 },
            { 8, 8, 8, 8, 8, 8, 8, 8 },
            { 10, 10, 10, 10, 10, 10, 10, 10 },
            { 8, 12, 8, 10, 4, 99, 99, 99 }
        };

        static readonly int[,] EACyclesL = new int[8, 8]
        {
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 8, 8, 8, 8, 8, 8, 8, 8 },
            { 8, 8, 8, 8, 8, 8, 8, 8 },
            { 10, 10, 10, 10, 10, 10, 10, 10 },
            { 12, 12, 12, 12, 12, 12, 12, 12 },
            { 14, 14, 14, 14, 14, 14, 14, 14 },
            { 12, 16, 12, 14, 8, 99, 99, 99 }
        };
        static readonly int[] CyclesException = new int[0x30]
        {
            0x04, 0x04, 0x32, 0x32, 0x22, 0x26, 0x28, 0x22,
            0x22, 0x22, 0x04, 0x04, 0x04, 0x04, 0x04, 0x2C,
            0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04,
            0x2C, 0x2C, 0x2C, 0x2C, 0x2C, 0x2C, 0x2C, 0x2C,
            0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22,
            0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22
        };
    }
}