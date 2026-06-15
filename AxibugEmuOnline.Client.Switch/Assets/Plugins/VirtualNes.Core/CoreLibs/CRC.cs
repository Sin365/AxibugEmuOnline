using System;

namespace VirtualNes.Core
{
    public static class CRC
    {
        const int CHAR_BIT = 8;
        const uint CRCPOLY1 = 0x04C11DB7U;
        const uint CRCPOLY2 = 0xEDB88320U;

        static bool m_Init;
        static bool m_InitRev;
        static uint[] m_CrcTable = new uint[byte.MaxValue + 1];
        static uint[] m_CrcTableRev = new uint[byte.MaxValue + 1];

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
        public static uint CrcRev(int size, Span<byte> c)
        {
            if (!m_InitRev)
            {
                MakeTableRev();
                m_InitRev = true;
            }

            uint r = 0xFFFFFFFFU;
            int step = 0;
            while (--size >= 0)
            {
                r = (r >> CHAR_BIT) ^ m_CrcTableRev[(byte)r ^ c[step]];
                step++;
            }
            return r ^ 0xFFFFFFFFU;
        }

        static void MakeTable()
        {
            int i, j;
            uint r;

            for (i = 0; i <= byte.MaxValue; i++)
            {
                r = (uint)i << (32 - CHAR_BIT);
                for (j = 0; j < CHAR_BIT; j++)
                {
                    if ((r & 0x80000000UL) > 0) r = (r << 1) ^ CRCPOLY1;
                    else r <<= 1;
                }
                m_CrcTable[i] = r & 0xFFFFFFFFU;
            }

        }
        static void MakeTableRev()
        {
            int i, j;
            uint r;

            for (i = 0; i <= byte.MaxValue; i++)
            {
                r = (uint)i;
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
