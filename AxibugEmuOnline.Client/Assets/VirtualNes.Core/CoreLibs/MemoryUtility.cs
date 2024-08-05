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

        public static void memset(byte[] array, int offset, byte value, int length)
        {
            for (int i = offset; i < length; i++)
            {
                array[i] = value;
            }
        }
    }
}
