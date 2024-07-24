using Codice.CM.Client.Differences;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

namespace VirtualNes.Core
{
    public static class CRC
    {
        const int CHAR_BIT = 8;
        const ulong CRCPOLY1 = 0x04C11DB7UL;
        const ulong CRCPOLY2 = 0xEDB88320UL;

        static bool m_Init;
        static bool m_InitRev;
        static ulong[] m_CrcTable = new ulong[byte.MaxValue + 1];
        static ulong[] m_CrcTableRev = new ulong[byte.MaxValue + 1];

        public static ulong Crc(int size, Span<byte> c)
        {
            if (!m_Init)
            {
                MakeTable();
                m_Init = true;
            }

            ulong r = 0xFFFFFFFFUL;
            int step = 0;
            while (--size >= 0)
            {
                r = (r << CHAR_BIT) ^ m_CrcTable[(byte)(r >> (32 - CHAR_BIT)) ^ c[step]];
                step++;
            }
            return ~r & 0xFFFFFFFFUL;
        }
        public static ulong CrcRev(int size, Span<byte> c)
        {
            if (!m_InitRev)
            {
                MakeTableRev();
                m_InitRev = true;
            }

            ulong r = 0xFFFFFFFFUL;
            int step = 0;
            while (--size >= 0)
            {
                r = (r >> CHAR_BIT) ^ m_CrcTableRev[(byte)r ^ c[step]];
                step++;
            }
            return r ^ 0xFFFFFFFFUL;
        }

        static void MakeTable()
        {
            int i, j;
            ulong r;

            for (i = 0; i <= byte.MaxValue; i++)
            {
                r = (ulong)i << (32 - CHAR_BIT);
                for (j = 0; j < CHAR_BIT; j++)
                {
                    if ((r & 0x80000000UL) > 0) r = (r << 1) ^ CRCPOLY1;
                    else r <<= 1;
                }
                m_CrcTable[i] = r & 0xFFFFFFFFUL;
            }

        }
        static void MakeTableRev()
        {
            int i, j;
            ulong r;

            for (i = 0; i <= byte.MaxValue; i++)
            {
                r = (ulong)i;
                for (j = 0; j < CHAR_BIT; j++)
                {
                    if ((r & 1) > 0) r = (r >> 1) ^ CRCPOLY2;
                    else r >>= 1;
                }
                m_CrcTableRev[i] = r;
            }
        }


    }
}
