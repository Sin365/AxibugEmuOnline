using System;

namespace cpu.m68000
{
    partial class MC68000
    {
        void ADD0()
        {
            int Dreg = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            switch (size)
            {
                case 0: // byte
                    {
                        sbyte value = ReadValueB(mode, reg);
                        int result = D[Dreg].s8 + value;
                        int uresult = D[Dreg].u8 + (byte)value;
                        X = C = (uresult & 0x100) != 0;
                        V = result > sbyte.MaxValue || result < sbyte.MinValue;
                        N = (result & 0x80) != 0;
                        Z = (result & 0xff) == 0;
                        D[Dreg].s8 = (sbyte)result;
                        pendingCycles -= 4 + EACyclesBW[mode, reg];
                        return;
                    }
                case 1: // word
                    {
                        short value = ReadValueW(mode, reg);
                        int result = D[Dreg].s16 + value;
                        int uresult = D[Dreg].u16 + (ushort)value;
                        X = C = (uresult & 0x10000) != 0;
                        V = result > short.MaxValue || result < short.MinValue;
                        N = (result & 0x8000) != 0;
                        Z = (result & 0xffff) == 0;
                        D[Dreg].s16 = (short)result;
                        pendingCycles -= 4 + EACyclesBW[mode, reg];
                        return;
                    }
                case 2: // long
                    {
                        int value = ReadValueL(mode, reg);
                        long result = (long)D[Dreg].s32 + (long)value;
                        ulong uresult = (ulong)D[Dreg].u32 + ((ulong)(uint)value);
                        X = C = (uresult & 0x100000000) != 0;
                        V = result > int.MaxValue || result < int.MinValue;
                        N = (result & 0x80000000) != 0;
                        Z = (uint)result == 0;
                        D[Dreg].s32 = (int)result;
                        if (mode == 0 || mode == 1 || (mode == 7 && reg == 4))
                        {
                            pendingCycles -= 8 + EACyclesL[mode, reg];
                        }
                        else
                        {
                            pendingCycles -= 6 + EACyclesL[mode, reg];
                        }
                        return;
                    }
            }
        }

        void ADD1()
        {
            int Dreg = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            switch (size)
            {
                case 0: // byte
                    {
                        sbyte value = PeekValueB(mode, reg);
                        int result = value + D[Dreg].s8;
                        int uresult = (byte)value + D[Dreg].u8;
                        X = C = (uresult & 0x100) != 0;
                        V = result > sbyte.MaxValue || result < sbyte.MinValue;
                        N = (result & 0x80) != 0;
                        Z = (result & 0xff) == 0;
                        WriteValueB(mode, reg, (sbyte)result);
                        pendingCycles -= 8 + EACyclesBW[mode, reg];
                        return;
                    }
                case 1: // word
                    {
                        short value = PeekValueW(mode, reg);
                        int result = value + D[Dreg].s16;
                        int uresult = (ushort)value + D[Dreg].u16;
                        X = C = (uresult & 0x10000) != 0;
                        V = result > short.MaxValue || result < short.MinValue;
                        N = (result & 0x8000) != 0;
                        Z = (result & 0xffff) == 0;
                        WriteValueW(mode, reg, (short)result);
                        pendingCycles -= 8 + EACyclesBW[mode, reg];
                        return;
                    }
                case 2: // long
                    {
                        int value = PeekValueL(mode, reg);
                        long result = (long)value + (long)D[Dreg].s32;
                        ulong uresult = ((ulong)(uint)value) + (ulong)D[Dreg].u32;
                        X = C = (uresult & 0x100000000) != 0;
                        V = result > int.MaxValue || result < int.MinValue;
                        N = (result & 0x80000000) != 0;
                        Z = ((uint)result == 0);
                        WriteValueL(mode, reg, (int)result);
                        pendingCycles -= 12 + EACyclesL[mode, reg];
                        return;
                    }
            }
        }


        void ADDI()
        {
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            switch (size)
            {
                case 0: // byte
                    {
                        int immed = (sbyte)ReadOpWord(PC); PC += 2;
                        sbyte value = PeekValueB(mode, reg);
                        int result = value + immed;
                        int uresult = (byte)value + (byte)immed;
                        X = C = (uresult & 0x100) != 0;
                        V = result > sbyte.MaxValue || result < sbyte.MinValue;
                        N = (result & 0x80) != 0;
                        Z = (result & 0xff) == 0;
                        WriteValueB(mode, reg, (sbyte)result);
                        if (mode == 0) pendingCycles -= 8;
                        else pendingCycles -= 12 + EACyclesBW[mode, reg];
                        return;
                    }
                case 1: // word
                    {
                        int immed = ReadOpWord(PC); PC += 2;
                        short value = PeekValueW(mode, reg);
                        int result = value + immed;
                        int uresult = (ushort)value + (ushort)immed;
                        X = C = (uresult & 0x10000) != 0;
                        V = result > short.MaxValue || result < short.MinValue;
                        N = (result & 0x8000) != 0;
                        Z = (result & 0xffff) == 0;
                        WriteValueW(mode, reg, (short)result);
                        if (mode == 0) pendingCycles -= 8;
                        else pendingCycles -= 12 + EACyclesBW[mode, reg];
                        return;
                    }
                case 2: // long
                    {
                        int immed = ReadOpLong(PC); PC += 4;
                        int value = PeekValueL(mode, reg);
                        long result = (long)value + (long)immed;
                        ulong uresult = ((ulong)(uint)value) + ((ulong)(uint)immed);
                        X = C = (uresult & 0x100000000) != 0;
                        V = result > int.MaxValue || result < int.MinValue;
                        N = (result & 0x80000000) != 0;
                        Z = ((uint)result == 0);
                        WriteValueL(mode, reg, (int)result);
                        if (mode == 0)
                            pendingCycles -= 16;
                        else
                            pendingCycles -= 20 + EACyclesL[mode, reg];
                        return;
                    }
            }
        }


        void ADDQ()
        {
            int data = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            data = data == 0 ? 8 : data; // range is 1-8; 0 represents 8

            switch (size)
            {
                case 0: // byte
                    {
                        if (mode == 1) throw new Exception("ADDQ.B on address reg is invalid");
                        sbyte value = PeekValueB(mode, reg);
                        int result = value + data;
                        int uresult = (byte)value + data;
                        N = (result & 0x80) != 0;
                        Z = result == 0;
                        V = result > sbyte.MaxValue || result < sbyte.MinValue;
                        C = X = (uresult & 0x100) != 0;
                        WriteValueB(mode, reg, (sbyte)result);
                        if (mode == 0) pendingCycles -= 4;
                        else pendingCycles -= 8 + EACyclesBW[mode, reg];
                        return;
                    }
                case 1: // word
                    {
                        if (mode == 1)
                        {
                            int value = PeekValueL(mode, reg);
                            WriteValueL(mode, reg, value + data);
                        }
                        else
                        {
                            short value = PeekValueW(mode, reg);
                            int result = value + data;
                            int uresult = (ushort)value + data;
                            N = (result & 0x8000) != 0;
                            Z = result == 0;
                            V = result > short.MaxValue || result < short.MinValue;
                            C = X = (uresult & 0x10000) != 0;
                            WriteValueW(mode, reg, (short)result);
                        }
                        if (mode <= 1)
                            pendingCycles -= 4;
                        else
                            pendingCycles -= 8 + EACyclesBW[mode, reg];
                        return;
                    }
                default: // long
                    {
                        int value = PeekValueL(mode, reg);
                        long result = (long)value + (long)data;
                        ulong uresult = ((ulong)(uint)value) + ((ulong)(uint)data);
                        if (mode != 1)
                        {
                            N = (result & 0x80000000) != 0;
                            Z = (result == 0);
                            V = result > int.MaxValue || result < int.MinValue;
                            C = X = (uresult & 0x100000000) != 0;
                        }
                        WriteValueL(mode, reg, (int)result);
                        if (mode <= 1)
                            pendingCycles -= 8;
                        else
                            pendingCycles -= 12 + EACyclesL[mode, reg];
                        return;
                    }
            }
        }


        void ADDA()
        {
            int aReg = (op >> 9) & 7;
            int size = (op >> 8) & 1;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            if (size == 0) // word
            {
                int value = ReadValueW(mode, reg);
                A[aReg].s32 += value;
                pendingCycles -= 8 + EACyclesBW[mode, reg];
            }
            else
            { // long
                int value = ReadValueL(mode, reg);
                A[aReg].s32 += value;
                if (mode == 0 || mode == 1 || (mode == 7 && reg == 4))
                    pendingCycles -= 8 + EACyclesL[mode, reg];
                else
                    pendingCycles -= 6 + EACyclesL[mode, reg];
            }
        }


        void SUB0()
        {
            int dReg = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            switch (size)
            {
                case 0: // byte
                    {
                        sbyte a = D[dReg].s8;
                        sbyte b = ReadValueB(mode, reg);
                        int result = a - b;
                        X = C = ((a < b) ^ ((a ^ b) >= 0) == false);
                        V = result > sbyte.MaxValue || result < sbyte.MinValue;
                        N = (result & 0x80) != 0;
                        Z = result == 0;
                        D[dReg].s8 = (sbyte)result;
                        pendingCycles -= 4 + EACyclesBW[mode, reg];
                        return;
                    }
                case 1: // word
                    {
                        short a = D[dReg].s16;
                        short b = ReadValueW(mode, reg);
                        int result = a - b;
                        X = C = ((a < b) ^ ((a ^ b) >= 0) == false);
                        V = result > short.MaxValue || result < short.MinValue;
                        N = (result & 0x8000) != 0;
                        Z = result == 0;
                        D[dReg].s16 = (short)result;
                        pendingCycles -= 4 + EACyclesBW[mode, reg];
                        return;
                    }
                case 2: // long
                    {
                        int a = D[dReg].s32;
                        int b = ReadValueL(mode, reg);
                        long result = (long)a - (long)b;
                        X = C = ((a < b) ^ ((a ^ b) >= 0) == false);
                        V = result > int.MaxValue || result < int.MinValue;
                        N = (result & 0x80000000) != 0;
                        Z = result == 0;
                        D[dReg].s32 = (int)result;
                        if (mode == 0 || mode == 1 || (mode == 7 && reg == 4))
                        {
                            pendingCycles -= 8 + EACyclesL[mode, reg];
                        }
                        else
                        {
                            pendingCycles -= 6 + EACyclesL[mode, reg];
                        }
                        return;
                    }
            }
        }

        void SUB1()
        {
            int dReg = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            switch (size)
            {
                case 0: // byte
                    {
                        sbyte a = PeekValueB(mode, reg);
                        sbyte b = D[dReg].s8;
                        int result = a - b;
                        X = C = ((a < b) ^ ((a ^ b) >= 0) == false);
                        V = result > sbyte.MaxValue || result < sbyte.MinValue;
                        N = (result & 0x80) != 0;
                        Z = result == 0;
                        WriteValueB(mode, reg, (sbyte)result);
                        pendingCycles -= 8 + EACyclesBW[mode, reg];
                        return;
                    }
                case 1: // word
                    {
                        short a = PeekValueW(mode, reg);
                        short b = D[dReg].s16;
                        int result = a - b;
                        X = C = ((a < b) ^ ((a ^ b) >= 0) == false);
                        V = result > short.MaxValue || result < short.MinValue;
                        N = (result & 0x8000) != 0;
                        Z = result == 0;
                        WriteValueW(mode, reg, (short)result);
                        pendingCycles -= 8 + EACyclesBW[mode, reg];
                        return;
                    }
                case 2: // long
                    {
                        int a = PeekValueL(mode, reg);
                        int b = D[dReg].s32;
                        long result = (long)a - (long)b;
                        X = C = ((a < b) ^ ((a ^ b) >= 0) == false);
                        V = result > int.MaxValue || result < int.MinValue;
                        N = (result & 0x80000000) != 0;
                        Z = ((uint)result == 0);
                        WriteValueL(mode, reg, (int)result);
                        pendingCycles -= 12 + EACyclesL[mode, reg];
                        return;
                    }
            }
        }


        void SUBI()
        {
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            switch (size)
            {
                case 0: // byte
                    {
                        sbyte b = (sbyte)ReadOpWord(PC); PC += 2;
                        sbyte a = PeekValueB(mode, reg);
                        int result = a - b;
                        X = C = ((a < b) ^ ((a ^ b) >= 0) == false);
                        V = result > sbyte.MaxValue || result < sbyte.MinValue;
                        N = (result & 0x80) != 0;
                        Z = result == 0;
                        WriteValueB(mode, reg, (sbyte)result);
                        if (mode == 0) pendingCycles -= 8;
                        else pendingCycles -= 12 + EACyclesBW[mode, reg];
                        return;
                    }
                case 1: // word
                    {
                        short b = ReadOpWord(PC); PC += 2;
                        short a = PeekValueW(mode, reg);
                        int result = a - b;
                        X = C = ((a < b) ^ ((a ^ b) >= 0) == false);
                        V = result > short.MaxValue || result < short.MinValue;
                        N = (result & 0x8000) != 0;
                        Z = result == 0;
                        WriteValueW(mode, reg, (short)result);
                        if (mode == 0) pendingCycles -= 8;
                        else pendingCycles -= 12 + EACyclesBW[mode, reg];
                        return;
                    }
                case 2: // long
                    {
                        int b = ReadOpLong(PC); PC += 4;
                        int a = PeekValueL(mode, reg);
                        long result = (long)a - (long)b;
                        X = C = ((a < b) ^ ((a ^ b) >= 0) == false);
                        V = result > int.MaxValue || result < int.MinValue;
                        N = (result & 0x80000000) != 0;
                        Z = ((uint)result == 0);
                        WriteValueL(mode, reg, (int)result);
                        if (mode == 0)
                            pendingCycles -= 16;
                        else
                            pendingCycles -= 20 + EACyclesL[mode, reg];
                        return;
                    }
            }
        }


        void SUBQ()
        {
            int data = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            data = data == 0 ? 8 : data; // range is 1-8; 0 represents 8

            switch (size)
            {
                case 0: // byte
                    {
                        if (mode == 1) throw new Exception("SUBQ.B on address reg is invalid");
                        sbyte value = PeekValueB(mode, reg);
                        int result = value - data;
                        N = (result & 0x80) != 0;
                        Z = result == 0;
                        V = result > sbyte.MaxValue || result < sbyte.MinValue;
                        C = X = ((value < data) ^ ((value ^ data) >= 0) == false);
                        WriteValueB(mode, reg, (sbyte)result);
                        if (mode == 0) pendingCycles -= 4;
                        else pendingCycles -= 8 + EACyclesBW[mode, reg];
                        return;
                    }
                case 1: // word
                    {
                        if (mode == 1)
                        {
                            int value = PeekValueL(mode, reg);
                            WriteValueL(mode, reg, value - data);
                        }
                        else
                        {
                            short value = PeekValueW(mode, reg);
                            int result = value - data;
                            N = (result & 0x8000) != 0;
                            Z = result == 0;
                            V = result > short.MaxValue || result < short.MinValue;
                            C = X = ((value < data) ^ ((value ^ data) >= 0) == false);
                            WriteValueW(mode, reg, (short)result);
                        }
                        if (mode == 0)
                            pendingCycles -= 4;
                        else if (mode == 1)
                            pendingCycles -= 8;
                        else
                            pendingCycles -= 8 + EACyclesBW[mode, reg];
                        return;
                    }
                default: // long
                    {
                        int value = PeekValueL(mode, reg);
                        long result = (long)value - (long)data;
                        if (mode != 1)
                        {
                            N = (result & 0x80000000) != 0;
                            Z = (result == 0);
                            V = result > int.MaxValue || result < int.MinValue;
                            C = X = ((value < data) ^ ((value ^ data) >= 0) == false);
                        }
                        WriteValueL(mode, reg, (int)result);
                        if (mode <= 1) pendingCycles -= 8;
                        else pendingCycles -= 12 + EACyclesL[mode, reg];
                        return;
                    }
            }
        }


        void SUBA()
        {
            int aReg = (op >> 9) & 7;
            int size = (op >> 8) & 1;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            if (size == 0) // word
            {
                int value = ReadValueW(mode, reg);
                A[aReg].s32 -= value;
                pendingCycles -= 8 + EACyclesBW[mode, reg];
            }
            else
            { // long
                int value = ReadValueL(mode, reg);
                A[aReg].s32 -= value;
                if (mode == 0 || mode == 1 || (mode == 7 && reg == 4))
                    pendingCycles -= 8 + EACyclesL[mode, reg];
                else
                    pendingCycles -= 6 + EACyclesL[mode, reg];
            }
        }


        void NEG()
        {
            int size = (op >> 6) & 0x03;
            int mode = (op >> 3) & 0x07;
            int reg = op & 0x07;

            if (mode == 1) throw new Exception("NEG on address reg is invalid");

            switch (size)
            {
                case 0: // Byte
                    {
                        sbyte value = PeekValueB(mode, reg);
                        int result = 0 - value;
                        N = (result & 0x80) != 0;
                        Z = result == 0;
                        V = result > sbyte.MaxValue || result < sbyte.MinValue;
                        C = X = ((0 < value) ^ ((0 ^ value) >= 0) == false);
                        WriteValueB(mode, reg, (sbyte)result);
                        if (mode == 0) pendingCycles -= 4;
                        else pendingCycles -= 8 + EACyclesBW[mode, reg];
                        return;
                    }
                case 1: // Word
                    {
                        short value = PeekValueW(mode, reg);
                        int result = 0 - value;
                        N = (result & 0x8000) != 0;
                        Z = result == 0;
                        V = result > short.MaxValue || result < short.MinValue;
                        C = X = ((0 < value) ^ ((0 ^ value) >= 0) == false);
                        WriteValueW(mode, reg, (short)result);
                        if (mode == 0) pendingCycles -= 4;
                        else pendingCycles -= 8 + EACyclesBW[mode, reg];
                        return;
                    }
                case 2: // Long
                    {
                        int value = PeekValueL(mode, reg);
                        long result = 0 - value;
                        N = (result & 0x80000000) != 0;
                        Z = result == 0;
                        V = result > int.MaxValue || result < int.MinValue;
                        C = X = ((0 < value) ^ ((0 ^ value) >= 0) == false);
                        WriteValueL(mode, reg, (int)result);
                        if (mode == 0)
                            pendingCycles -= 6;
                        else
                            pendingCycles -= 12 + EACyclesL[mode, reg];
                        return;
                    }
            }
        }


        void NBCD()
        {
            int mode = (op >> 3) & 0x07;
            int reg = op & 0x07;
            byte result = (byte)PeekValueB(mode, reg);
            result = (byte)((0x9a - result - (X ? 1 : 0)) & 0xff);
            if (result != 0x9a)
            {
                V = (((~result) & 0x80) != 0);
                if ((result & 0x0f) == 0x0a)
                {
                    result = (byte)((result & 0xf0) + 0x10);
                }
                V &= ((result & 0x80) != 0);
                WriteValueB(mode, reg, (sbyte)result);
                Z &= (result == 0);
                C = true;
                X = true;
            }
            else
            {
                V = false;
                C = false;
                X = false;
            }
            N = ((result & 0x80) != 0);
            pendingCycles -= (mode == 0) ? 6 : 8 + EACyclesBW[mode, reg];
        }


        void ILLEGAL()
        {
            TrapVector2(4);
            pendingCycles -= 4;
        }


        void ILL()
        {
            TrapVector2(4);
        }


        void STOP()
        {
            if (s)
            {
                short new_sr = ReadOpWord(PC); PC += 2;
                stopped = true;
                SR = new_sr;
                pendingCycles = 0;
            }
            else
            {
                TrapVector2(8);
            }
            pendingCycles -= 4;
        }


        void TRAPV()
        {
            if (!V)
            {
                pendingCycles -= 4;
            }
            else
            {
                TrapVector(7);
                pendingCycles -= 4;
            }
        }


        void CHK()
        {
            int dreg = (op >> 9) & 0x07;
            int boundMode = (op >> 3) & 0x07;
            int boundReg = op & 0x07;
            short src, bound;
            src = D[dreg].s16;
            bound = ReadValueW(boundMode, boundReg);
            Z = (src == 0);
            V = false;
            C = false;
            if (src >= 0 && src <= bound)
            {
                pendingCycles -= 10 + EACyclesBW[boundMode, boundReg];
            }
            else
            {
                N = (src < 0);
                TrapVector(6);
                pendingCycles -= 10 + EACyclesBW[boundMode, boundReg];
            }
        }


        void NEGX()
        {
            int size = (op >> 6) & 0x03;
            int mode = (op >> 3) & 0x07;
            int reg = op & 0x07;

            if (mode == 1)
            {
                throw new Exception("NEG on address reg is invalid");
            }
            switch (size)
            {
                case 0: // Byte
                    {
                        sbyte value = PeekValueB(mode, reg);
                        int result = 0 - value - (X ? 1 : 0);
                        N = (result & 0x80) != 0;
                        Z &= (result == 0);
                        V = result > sbyte.MaxValue || result < sbyte.MinValue;
                        C = X = ((0 < value) ^ ((0 ^ value) >= 0) == false);
                        WriteValueB(mode, reg, (sbyte)result);
                        if (mode == 0) pendingCycles -= 4;
                        else pendingCycles -= 8 + EACyclesBW[mode, reg];
                        return;
                    }
                case 1: // Word
                    {
                        short value = PeekValueW(mode, reg);
                        int result = 0 - value - (X ? 1 : 0);
                        N = (result & 0x8000) != 0;
                        Z &= (result == 0);
                        V = result > short.MaxValue || result < short.MinValue;
                        C = X = ((0 < value) ^ ((0 ^ value) >= 0) == false);
                        WriteValueW(mode, reg, (short)result);
                        if (mode == 0) pendingCycles -= 4;
                        else pendingCycles -= 8 + EACyclesBW[mode, reg];
                        return;
                    }
                case 2: // Long
                    {
                        int value = PeekValueL(mode, reg);
                        long result = 0 - value - (X ? 1 : 0);
                        N = (result & 0x80000000) != 0;
                        Z &= (result == 0);
                        V = result > int.MaxValue || result < int.MinValue;
                        C = X = ((0 < value) ^ ((0 ^ value) >= 0) == false);
                        WriteValueL(mode, reg, (int)result);
                        if (mode == 0) pendingCycles -= 6;
                        else pendingCycles -= 12 + EACyclesL[mode, reg];
                        return;
                    }
            }
        }


        void SBCD0()
        {
            int dstReg = (op >> 9) & 0x07;
            int srcReg = op & 0x07;
            uint dst = D[dstReg].u32;
            uint src = D[srcReg].u32;
            uint res;
            res = (uint)((dst & 0x0f) - (src & 0x0f) - (X ? 1 : 0));
            V = false;
            if (res > 9)
            {
                res -= 6;
            }
            res += (dst & 0xf0) - (src & 0xf0);
            if (res > 0x99)
            {
                res += 0xa0;
                X = C = true;
                N = true;
            }
            else
            {
                N = X = C = false;
            }
            res = res & 0xff;
            Z &= (res == 0);
            D[dstReg].u32 = (D[dstReg].u32 & 0xffffff00) | res;
            pendingCycles -= 6;
        }


        void SBCD1()
        {
            int dstReg = (op >> 9) & 0x07;
            int srcReg = op & 0x07;
            uint src, dst, res;
            if (srcReg == 7)
            {
                A[srcReg].u32 -= 2;
            }
            else
            {
                A[srcReg].u32--;
            }
            if (dstReg == 7)
            {
                A[dstReg].u32 -= 2;
            }
            else
            {
                A[dstReg].u32--;
            }
            src = (uint)ReadByte(A[srcReg].s32);
            dst = (uint)ReadByte(A[dstReg].s32);
            res = (uint)((dst & 0x0f) - (src & 0x0f) - (X ? 1 : 0));
            V = false;
            if (res > 9)
            {
                res -= 6;
            }
            res += (dst & 0xf0) - (src & 0xf0);
            if (res > 0x99)
            {
                res += 0xa0;
                X = C = true;
                N = true;
            }
            else
            {
                N = X = C = false;
            }
            res = res & 0xff;
            Z &= (res == 0);
            WriteByte(A[dstReg].s32, (sbyte)res);
            pendingCycles -= 18;
        }


        void ABCD0()
        {
            int dstReg = (op >> 9) & 0x07;
            int srcReg = op & 0x07;
            uint src, dst, res;
            src = D[srcReg].u32;
            dst = D[dstReg].u32;
            res = (uint)((src & 0x0f) + (dst & 0x0f) + (X ? 1 : 0));
            V = (((~res) & 0x80) != 0);
            if (res > 9)
            {
                res += 6;
            }
            res += (src & 0xf0) + (dst & 0xf0);
            X = C = (res > 0x99);
            if (C)
            {
                res -= 0xa0;
            }
            V &= ((res & 0x80) != 0);
            N = ((res & 0x80) != 0);
            res = res & 0xff;
            Z &= (res == 0);
            D[dstReg].u32 = (((D[dstReg].u32) & 0xffffff00) | res);
            pendingCycles -= 6;
        }


        void ABCD1()
        {
            int dstReg = (op >> 9) & 0x07;
            int srcReg = op & 0x07;
            uint src, dst, res;
            if (srcReg == 7)
            {
                A[srcReg].u32 -= 2;
            }
            else
            {
                A[srcReg].u32--;
            }
            if (dstReg == 7)
            {
                A[dstReg].u32 -= 2;
            }
            else
            {
                A[dstReg].u32--;
            }
            src = (uint)ReadByte(A[srcReg].s32);
            dst = (uint)ReadByte(A[dstReg].s32);
            res = (uint)((src & 0x0f) + (dst & 0x0f) + (X ? 1 : 0));
            V = (((~res) & 0x80) != 0);
            if (res > 9)
            {
                res += 6;
            }
            res += (src & 0xf0) + (dst & 0xf0);
            X = C = (res > 0x99);
            if (C)
            {
                res -= 0xa0;
            }
            V &= ((res & 0x80) != 0);
            N = ((res & 0x80) != 0);
            res = res & 0xff;
            Z &= (res == 0);
            WriteByte(A[dstReg].s32, (sbyte)res);
            pendingCycles -= 18;
        }


        void EXGdd()
        {
            int reg_a = (op >> 9) & 0x07;
            int reg_b = op & 0x07;
            uint tmp;
            tmp = D[reg_a].u32;
            D[reg_a].u32 = D[reg_b].u32;
            D[reg_b].u32 = tmp;
            pendingCycles -= 6;
        }


        void EXGaa()
        {
            int reg_a = (op >> 9) & 0x07;
            int reg_b = op & 0x07;
            uint tmp;
            tmp = A[reg_a].u32;
            A[reg_a].u32 = A[reg_b].u32;
            A[reg_b].u32 = tmp;
            pendingCycles -= 6;
        }


        void EXGda()
        {
            int reg_a = (op >> 9) & 0x07;
            int reg_b = op & 0x07;
            uint tmp;
            tmp = D[reg_a].u32;
            D[reg_a].u32 = A[reg_b].u32;
            A[reg_b].u32 = tmp;
            pendingCycles -= 6;
        }


        void ADDX0()
        {
            int dstReg = (op >> 9) & 0x07;
            int size = (op >> 6) & 0x03;
            int srcReg = op & 0x07;
            switch (size)
            {
                case 0:
                    {
                        uint src = D[srcReg].u32 & 0xff;
                        uint dst = D[dstReg].u32 & 0xff;
                        uint res;
                        res = (uint)(dst + src + (X ? 1 : 0));
                        N = ((res & 0x80) != 0);
                        V = (((src ^ res & (dst ^ res)) & 0x80) != 0);
                        X = C = ((res & 0x100) != 0);
                        res = res & 0xff;
                        Z &= (res == 0);
                        D[dstReg].u32 = (D[dstReg].u32 & 0xffffff00) | res;
                        pendingCycles -= 4;
                        return;
                    }
                case 1:
                    {
                        uint src = D[srcReg].u32 & 0xffff;
                        uint dst = D[dstReg].u32 & 0xffff;
                        uint res;
                        res = (uint)(dst + src + (X ? 1 : 0));
                        N = ((res & 0x8000) != 0);
                        V = ((((src ^ res) & (dst ^ res)) & 0x8000) != 0);
                        X = C = ((res & 0x10000) != 0);
                        res = res & 0xffff;
                        Z &= (res == 0);
                        D[dstReg].u32 = (D[dstReg].u32 & 0xffff0000) | res;
                        pendingCycles -= 4;
                        return;
                    }
                case 2:
                    {
                        uint src = D[srcReg].u32;
                        uint dst = D[dstReg].u32;
                        uint res;
                        res = (uint)(dst + src + (X ? 1 : 0));
                        N = ((res & 0x80000000) != 0);
                        V = ((((src ^ res) & (dst ^ res)) & 0x80000000) != 0);
                        X = C = ((((src & dst) | (~res & (src | dst))) & 0x80000000) != 0);
                        Z &= (res == 0);
                        D[dstReg].u32 = res;
                        pendingCycles -= 8;
                        return;
                    }
            }
        }


        void ADDX1()
        {
            int dstReg = (op >> 9) & 0x07;
            int size = (op >> 6) & 0x03;
            int srcReg = op & 0x07;
            switch (size)
            {
                case 0:
                    {
                        if (srcReg == 7)
                        {
                            A[srcReg].u32 -= 2;
                        }
                        else
                        {
                            A[srcReg].u32--;
                        }
                        if (dstReg == 7)
                        {
                            A[dstReg].u32 -= 2;
                        }
                        else
                        {
                            A[dstReg].u32--;
                        }
                        uint src = (uint)ReadByte(A[srcReg].s32);
                        uint dst = (uint)ReadByte(A[dstReg].s32);
                        uint res;
                        res = (uint)(dst + src + (X ? 1 : 0));
                        N = ((res & 0x80) != 0);
                        V = ((((src ^ res) & (dst ^ res)) & 0x80) != 0);
                        X = C = ((res & 0x100) != 0);
                        res = res & 0xff;
                        Z &= (res == 0);
                        WriteByte(A[dstReg].s32, (sbyte)res);
                        pendingCycles -= 18;
                        return;
                    }
                case 1:
                    {
                        A[srcReg].u32 -= 2;
                        uint src = (uint)ReadWord(A[srcReg].s32);
                        A[dstReg].u32 -= 2;
                        uint dst = (uint)ReadWord(A[dstReg].s32);
                        uint res;
                        res = (uint)(dst + src + (X ? 1 : 0));
                        N = ((res & 0x8000) != 0);
                        V = ((((src ^ res) & (dst ^ res)) & 0x8000) != 0);
                        X = C = ((res & 0x10000) != 0);
                        res = res & 0xffff;
                        Z &= (res == 0);
                        WriteWord(A[dstReg].s32, (short)res);
                        pendingCycles -= 18;
                        return;
                    }
                case 2:
                    {
                        A[srcReg].u32 -= 4;
                        uint src = (uint)ReadLong(A[srcReg].s32);
                        A[dstReg].u32 -= 4;
                        uint dst = (uint)ReadWord(A[dstReg].s32);
                        uint res;
                        res = (uint)(dst + src + (X ? 1 : 0));
                        N = ((res & 0x80000000) != 0);
                        V = (((((src ^ res) & (dst ^ res)) >> 24) & 0x80) != 0);
                        X = C = (((((src & dst) | (~res & (src | dst))) >> 23) & 0x100) != 0);
                        Z &= (res == 0);
                        WriteLong(A[dstReg].s32, (int)res);
                        pendingCycles -= 30;
                        return;
                    }
            }
        }


        void SUBX0()
        {
            int dstReg = (op >> 9) & 0x07;
            int size = (op >> 6) & 0x03;
            int srcReg = op & 0x07;
            switch (size)
            {
                case 0:
                    {
                        uint src = D[srcReg].u32 & 0xff;
                        uint dst = D[dstReg].u32 & 0xff;
                        uint res;
                        res = (uint)(dst - src - (X ? 1 : 0));
                        N = ((res & 0x80) != 0);
                        X = C = ((res & 0x100) != 0);
                        V = ((((src ^ dst) & (res ^ dst)) & 0x80) != 0);
                        res = res & 0xff;
                        Z &= (res == 0);
                        D[dstReg].u32 = (D[dstReg].u32 & 0xffffff00) | res;
                        pendingCycles -= 4;
                        return;
                    }
                case 1:
                    {
                        uint src = D[srcReg].u32 & 0xffff;
                        uint dst = D[dstReg].u32 & 0xffff;
                        uint res;
                        res = (uint)(dst - src - (X ? 1 : 0));
                        N = ((res & 0x8000) != 0);
                        X = C = ((res & 0x10000) != 0);
                        V = ((((src ^ dst) & (res ^ dst)) & 0x8000) != 0);
                        res = res & 0xffff;
                        Z &= (res == 0);
                        D[dstReg].u32 = (D[dstReg].u32 & 0xffff0000) | res;
                        pendingCycles -= 4;
                        return;
                    }
                case 2:
                    {
                        uint src = D[srcReg].u32;
                        uint dst = D[dstReg].u32;
                        uint res;
                        res = (uint)(dst - src - (X ? 1 : 0));
                        N = ((res & 0x80000000) != 0);
                        X = C = (((((src & res) | (~dst & (src | res))) >> 23) & 0x100) != 0);
                        V = (((((src ^ dst) & (res ^ dst)) >> 24) & 0x80) != 0);
                        Z &= (res == 0);
                        D[dstReg].u32 = res;
                        pendingCycles -= 8;
                        return;
                    }
            }
        }


        void SUBX1()
        {
            int dstReg = (op >> 9) & 0x07;
            int size = (op >> 6) & 0x03;
            int srcReg = op & 0x07;
            switch (size)
            {
                case 0:
                    {
                        if (srcReg == 7)
                        {
                            A[srcReg].u32 -= 2;
                        }
                        else
                        {
                            A[srcReg].u32--;
                        }
                        if (dstReg == 7)
                        {
                            A[dstReg].u32 -= 2;
                        }
                        else
                        {
                            A[dstReg].u32--;
                        }
                        uint src = (uint)ReadByte(A[srcReg].s32);
                        uint dst = (uint)ReadByte(A[dstReg].s32);
                        uint res;
                        res = (uint)(dst - src - (X ? 1 : 0));
                        N = ((res & 0x80) != 0);
                        X = C = ((res & 0x100) != 0);
                        V = ((((src ^ dst) & (res ^ dst)) & 0x80) != 0);
                        res = res & 0xff;
                        Z &= (res == 0);
                        WriteByte(A[dstReg].s32, (sbyte)res);
                        pendingCycles -= 18;
                        return;
                    }
                case 1:
                    {
                        A[srcReg].u32 -= 2;
                        uint src = (uint)ReadWord(A[srcReg].s32);
                        A[dstReg].u32 -= 2;
                        uint dst = (uint)ReadWord(A[dstReg].s32);
                        uint res;
                        res = (uint)(dst - src - (X ? 1 : 0));
                        N = ((res & 0x8000) != 0);
                        X = C = ((res & 0x10000) != 0);
                        V = ((((src ^ dst) & (res ^ dst)) & 0x8000) != 0);
                        res = res & 0xffff;
                        Z &= (res == 0);
                        WriteWord(A[dstReg].s32, (short)res);
                        pendingCycles -= 18;
                        return;
                    }
                case 2:
                    {
                        A[srcReg].u32 -= 4;
                        uint src = (uint)ReadLong(A[srcReg].s32);
                        A[dstReg].u32 -= 4;
                        uint dst = (uint)ReadWord(A[dstReg].s32);
                        uint res;
                        res = (uint)(dst - src - (X ? 1 : 0));
                        N = ((res & 0x80000000) != 0);
                        X = C = (((((src & res) | (~dst & (src | res))) >> 23) & 0x100) != 0);
                        V = (((((src ^ dst) & (res ^ dst)) >> 24) & 0x80) != 0);
                        Z &= (res == 0);
                        WriteLong(A[dstReg].s32, (int)res);
                        pendingCycles -= 30;
                        return;
                    }
            }
        }


        void CMP()
        {
            int dReg = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            switch (size)
            {
                case 0: // byte
                    {
                        sbyte a = D[dReg].s8;
                        sbyte b = ReadValueB(mode, reg);
                        int result = a - b;
                        N = (result & 0x80) != 0;
                        Z = result == 0;
                        V = result > sbyte.MaxValue || result < sbyte.MinValue;
                        C = ((a < b) ^ ((a ^ b) >= 0) == false);
                        pendingCycles -= 4 + EACyclesBW[mode, reg];
                        return;
                    }
                case 1: // word
                    {
                        short a = D[dReg].s16;
                        short b = ReadValueW(mode, reg);
                        int result = a - b;
                        N = (result & 0x8000) != 0;
                        Z = result == 0;
                        V = result > short.MaxValue || result < short.MinValue;
                        C = ((a < b) ^ ((a ^ b) >= 0) == false);
                        pendingCycles -= 4 + EACyclesBW[mode, reg];
                        return;
                    }
                case 2: // long
                    {
                        int a = D[dReg].s32;
                        int b = ReadValueL(mode, reg);
                        long result = (long)a - (long)b;
                        N = (result & 0x80000000) != 0;
                        Z = (uint)result == 0;
                        V = result > int.MaxValue || result < int.MinValue;
                        C = ((a < b) ^ ((a ^ b) >= 0) == false);
                        pendingCycles -= 6 + EACyclesL[mode, reg];
                        return;
                    }
            }
        }


        void CMPA()
        {
            int aReg = (op >> 9) & 7;
            int size = (op >> 8) & 1;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            switch (size)
            {
                case 0: // word
                    {
                        int a = A[aReg].s32;
                        short b = ReadValueW(mode, reg);
                        long result = a - b;
                        N = (result & 0x80000000) != 0;
                        Z = result == 0;
                        V = result > int.MaxValue || result < int.MinValue;
                        C = ((a < b) ^ ((a ^ b) >= 0) == false);
                        pendingCycles -= 6 + EACyclesBW[mode, reg];
                        return;
                    }
                case 1: // long
                    {
                        int a = A[aReg].s32;
                        int b = ReadValueL(mode, reg);
                        long result = a - b;
                        N = (result & 0x80000000) != 0;
                        Z = result == 0;
                        V = result > int.MaxValue || result < int.MinValue;
                        C = ((a < b) ^ ((a ^ b) >= 0) == false);
                        pendingCycles -= 6 + EACyclesL[mode, reg];
                        return;
                    }
            }
        }


        void CMPM()
        {
            int axReg = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int ayReg = (op >> 0) & 7;

            switch (size)
            {
                case 0: // byte
                    {
                        sbyte a = ReadByte(A[axReg].s32); A[axReg].s32 += 1; // Does A7 stay word aligned???
                        sbyte b = ReadByte(A[ayReg].s32); A[ayReg].s32 += 1;
                        int result = a - b;
                        N = (result & 0x80) != 0;
                        Z = (result & 0xff) == 0;
                        V = result > sbyte.MaxValue || result < sbyte.MinValue;
                        C = ((a < b) ^ ((a ^ b) >= 0) == false);
                        pendingCycles -= 12;
                        return;
                    }
                case 1: // word
                    {
                        short a = ReadWord(A[axReg].s32); A[axReg].s32 += 2;
                        short b = ReadWord(A[ayReg].s32); A[ayReg].s32 += 2;
                        int result = a - b;
                        N = (result & 0x8000) != 0;
                        Z = (result & 0xffff) == 0;
                        V = result > short.MaxValue || result < short.MinValue;
                        C = ((a < b) ^ ((a ^ b) >= 0) == false);
                        pendingCycles -= 12;
                        return;
                    }
                case 2: // long
                    {
                        int a = ReadLong(A[axReg].s32); A[axReg].s32 += 4;
                        int b = ReadLong(A[ayReg].s32); A[ayReg].s32 += 4;
                        long result = a - b;
                        N = (result & 0x80000000) != 0;
                        Z = (uint)result == 0;
                        V = result > int.MaxValue || result < int.MinValue;
                        C = ((a < b) ^ ((a ^ b) >= 0) == false);
                        pendingCycles -= 20;
                        return;
                    }
            }
        }


        void CMPI()
        {
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            switch (size)
            {
                case 0: // byte
                    {
                        sbyte b = (sbyte)ReadOpWord(PC); PC += 2;
                        sbyte a = ReadValueB(mode, reg);
                        int result = a - b;
                        N = (result & 0x80) != 0;
                        Z = result == 0;
                        V = result > sbyte.MaxValue || result < sbyte.MinValue;
                        C = ((a < b) ^ ((a ^ b) >= 0) == false);
                        if (mode == 0) pendingCycles -= 8;
                        else pendingCycles -= 8 + EACyclesBW[mode, reg];
                        return;
                    }
                case 1: // word
                    {
                        short b = ReadOpWord(PC); PC += 2;
                        short a = ReadValueW(mode, reg);
                        int result = a - b;
                        N = (result & 0x8000) != 0;
                        Z = result == 0;
                        V = result > short.MaxValue || result < short.MinValue;
                        C = ((a < b) ^ ((a ^ b) >= 0) == false);
                        if (mode == 0) pendingCycles -= 8;
                        else pendingCycles -= 8 + EACyclesBW[mode, reg];
                        return;
                    }
                case 2: // long
                    {
                        int b = ReadOpLong(PC); PC += 4;
                        int a = ReadValueL(mode, reg);
                        long result = a - b;
                        N = (result & 0x80000000) != 0;
                        Z = result == 0;
                        V = result > int.MaxValue || result < int.MinValue;
                        C = ((a < b) ^ ((a ^ b) >= 0) == false);
                        if (mode == 0) pendingCycles -= 14;
                        else pendingCycles -= 12 + EACyclesL[mode, reg];
                        return;
                    }
            }
        }


        void MULU()
        {
            int dreg = (op >> 9) & 7;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            uint result = (uint)(D[dreg].u16 * (ushort)ReadValueW(mode, reg));
            D[dreg].u32 = result;

            V = false;
            C = false;
            N = (result & 0x80000000) != 0;
            Z = result == 0;

            pendingCycles -= 54 + EACyclesBW[mode, reg];
        }


        void MULS()
        {
            int dreg = (op >> 9) & 7;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            int result = D[dreg].s16 * ReadValueW(mode, reg);
            D[dreg].s32 = result;

            V = false;
            C = false;
            N = (result & 0x80000000) != 0;
            Z = result == 0;

            pendingCycles -= 54 + EACyclesBW[mode, reg];
        }


        void DIVU()
        {
            int dreg = (op >> 9) & 7;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            uint source = (ushort)ReadValueW(mode, reg);
            uint dest = D[dreg].u32;

            if (source == 0)
            {
                TrapVector(5);
            }
            else
            {
                uint quotient = dest / source;
                uint remainder = dest % source;
                if (quotient < 0x10000)
                {
                    Z = quotient == 0;
                    N = (quotient & 0x8000) != 0;
                    V = false;
                    C = false;
                    D[dreg].u32 = (quotient & 0xFFFF) | (remainder << 16);
                }
                else
                {
                    V = true;
                }
            }
            pendingCycles -= 140 + EACyclesBW[mode, reg];
        }


        void DIVS()
        {
            int dreg = (op >> 9) & 7;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            int source = ReadValueW(mode, reg);
            int dest = D[dreg].s32;

            if (source == 0)
            {
                TrapVector(5);
            }
            else
            {
                int quotient = dest / source;
                int remainder = dest % source;
                if (quotient == (short)quotient)
                {
                    Z = quotient == 0;
                    N = (quotient & 0x8000) != 0;
                    V = false;
                    C = false;
                    D[dreg].s32 = (quotient & 0xFFFF) | (remainder << 16);
                }
                else
                {
                    V = true;
                }
            }
            pendingCycles -= 158 + EACyclesBW[mode, reg];
        }

    }
}
