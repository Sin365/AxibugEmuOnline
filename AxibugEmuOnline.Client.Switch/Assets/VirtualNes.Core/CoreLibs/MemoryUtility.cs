﻿using System;
using System.Runtime.CompilerServices;

namespace VirtualNes.Core
{
    public static class MemoryUtility
    {
        public static void ZEROMEMORY(byte[] array, int length)
        {
            Array.Clear(array, 0, array.Length);
        }
        public static void ZEROMEMORY(int[] array, int length)
        {
            Array.Clear(array, 0, array.Length);
        }

        public static void memset(byte[] array, byte value, int length)
        {
            memset(array, 0, value, length);
        }

        public unsafe static void memset(byte[] array, int offset, byte value, int length)
        {
            fixed (byte* ptr = array)
            {
                var offsetptr = ptr + offset;

                Unsafe.InitBlockUnaligned(offsetptr, value, (uint)length);
            }
        }

        public unsafe static void memset(uint[] array, int offset, byte value, int length)
        {
            fixed (uint* ptr = array)
            {
                var offsetptr = ptr + offset;
                for (int i = 0; i < length; i++)
                {
                    offsetptr[i] = value;
                }
            }
        }

        public unsafe static void memset(byte* ptr, int offset, byte value, int length)
        {
            var offsetptr = ptr + offset;
            for (int i = 0; i < length; i++)
            {
                offsetptr[i] = value;
            }
        }

        public unsafe static void memset(byte* ptr, byte value, int length)
        {
            memset(ptr, 0, value, length);
        }

        public unsafe static void memset(uint* ptr, int offset, uint value, int length)
        {
            var offsetptr = ptr + offset;
            for (int i = 0; i < length; i++)
            {
                offsetptr[i] = value;
            }
        }

        public unsafe static void memset(uint* ptr, uint value, int length)
        {
            memset(ptr, 0, value, length);
        }

        public static void memcpy(Array dst, Array src, int length)
        {
            Array.Copy(src, dst, length);
        }
    }
}
