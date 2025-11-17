using System;
using System.Text;

namespace cpu.m68000
{
    unsafe partial class MC68000
    {
        void MOVE()
        {
            int size = ((op >> 12) & 0x03);
            int dstMode = ((op >> 6) & 0x07);
            int dstReg = ((op >> 9) & 0x07);
            int srcMode = ((op >> 3) & 0x07);
            int srcReg = (op & 0x07);

            int value = 0;
            switch (size)
            {
                case 1: // Byte
                    value = ReadValueB(srcMode, srcReg);
                    WriteValueB(dstMode, dstReg, (sbyte)value);
                    pendingCycles -= MoveCyclesBW[srcMode + (srcMode == 7 ? srcReg : 0), dstMode + (dstMode == 7 ? dstReg : 0)];
                    N = (value & 0x80) != 0;
                    break;
                case 3: // Word
                    value = ReadValueW(srcMode, srcReg);
                    WriteValueW(dstMode, dstReg, (short)value);
                    pendingCycles -= MoveCyclesBW[srcMode + (srcMode == 7 ? srcReg : 0), dstMode + (dstMode == 7 ? dstReg : 0)];
                    N = (value & 0x8000) != 0;
                    break;
                case 2: // Long
                    value = ReadValueL(srcMode, srcReg);
                    WriteValueL(dstMode, dstReg, value);
                    pendingCycles -= MoveCyclesL[srcMode + (srcMode == 7 ? srcReg : 0), dstMode + (dstMode == 7 ? dstReg : 0)];
                    N = (value & 0x80000000) != 0;
                    break;
            }

            V = false;
            C = false;
            Z = (value == 0);
        }


        void MOVEA()
        {
            int size = ((op >> 12) & 0x03);
            int dstReg = ((op >> 9) & 0x07);
            int srcMode = ((op >> 3) & 0x07);
            int srcReg = (op & 0x07);

            if (size == 3) // Word
            {
                A[dstReg].s32 = ReadValueW(srcMode, srcReg);
                switch (srcMode)
                {
                    case 0: pendingCycles -= 4; break;
                    case 1: pendingCycles -= 4; break;
                    case 2: pendingCycles -= 8; break;
                    case 3: pendingCycles -= 8; break;
                    case 4: pendingCycles -= 10; break;
                    case 5: pendingCycles -= 12; break;
                    case 6: pendingCycles -= 14; break;
                    case 7:
                        switch (srcReg)
                        {
                            case 0: pendingCycles -= 12; break;
                            case 1: pendingCycles -= 16; break;
                            case 2: pendingCycles -= 12; break;
                            case 3: pendingCycles -= 14; break;
                            case 4: pendingCycles -= 8; break;
                            default: throw new InvalidOperationException();
                        }
                        break;
                }
            }
            else
            { // Long
                A[dstReg].s32 = ReadValueL(srcMode, srcReg);
                switch (srcMode)
                {
                    case 0: pendingCycles -= 4; break;
                    case 1: pendingCycles -= 4; break;
                    case 2: pendingCycles -= 12; break;
                    case 3: pendingCycles -= 12; break;
                    case 4: pendingCycles -= 14; break;
                    case 5: pendingCycles -= 16; break;
                    case 6: pendingCycles -= 18; break;
                    case 7:
                        switch (srcReg)
                        {
                            case 0: pendingCycles -= 16; break;
                            case 1: pendingCycles -= 20; break;
                            case 2: pendingCycles -= 16; break;
                            case 3: pendingCycles -= 18; break;
                            case 4: pendingCycles -= 12; break;
                            default: throw new InvalidOperationException();
                        }
                        break;
                }
            }
        }


        void MOVEP()
        {
            int dReg = ((op >> 9) & 0x07);
            int dir = ((op >> 7) & 0x01);
            int size = ((op >> 6) & 0x01);
            int aReg = (op & 0x07);
            if (dir == 0 && size == 0)
            {
                int ea;
                ea = A[aReg].s32 + ReadOpWord(PC); PC += 2;
                D[dReg].u32 = (D[dReg].u32 & 0xffff0000) | (ushort)(((byte)ReadByte(ea) << 8) + (byte)ReadByte(ea + 2));
                pendingCycles -= 16;
            }
            else if (dir == 0 && size == 1)
            {
                int ea;
                ea = A[aReg].s32 + ReadOpWord(PC); PC += 2;
                D[dReg].u32 = (uint)(((byte)ReadByte(ea) << 24) + ((byte)ReadByte(ea + 2) << 16) + ((byte)ReadByte(ea + 4) << 8) + (byte)ReadByte(ea + 6));
                pendingCycles -= 24;
            }
            else if (dir == 1 && size == 0)
            {
                uint src;
                int ea;
                ea = A[aReg].s32 + ReadOpWord(PC); PC += 2;
                src = D[dReg].u32;
                WriteByte(ea, (sbyte)((src >> 8) & 0xff));
                WriteByte(ea + 2, (sbyte)(src & 0xff));
                pendingCycles -= 16;
            }
            else if (dir == 1 && size == 1)
            {
                uint src;
                int ea;
                ea = A[aReg].s32 + ReadOpWord(PC); PC += 2;
                src = D[dReg].u32;
                WriteByte(ea, (sbyte)((src >> 24) & 0xff));
                WriteByte(ea + 2, (sbyte)((src >> 16) & 0xff));
                WriteByte(ea + 4, (sbyte)((src >> 8) & 0xff));
                WriteByte(ea + 6, (sbyte)(src & 0xff));
                pendingCycles -= 24;
            }
        }


        void MOVEQ()
        {
            int value = (sbyte)op; // 8-bit data payload is sign-extended to 32-bits.
            N = (value & 0x80) != 0;
            Z = (value == 0);
            V = false;
            C = false;
            D[(op >> 9) & 7].s32 = value;
            pendingCycles -= 4;
        }


        static string DisassembleRegisterList0(ushort registers)
        {
            var str = new StringBuilder();
            int count = 0;
            for (int i = 0; i < 8; i++)
            {
                if ((registers & 0x8000) != 0)
                {
                    if (count > 0) str.Append(",");
                    str.Append("D" + i);
                    count++;
                }
                registers <<= 1;
            }
            for (int i = 0; i < 8; i++)
            {
                if ((registers & 0x8000) != 0)
                {
                    if (count > 0) str.Append(",");
                    str.Append("A" + i);
                    count++;
                }
                registers <<= 1;
            }
            return str.ToString();
        }

        static string DisassembleRegisterList1(ushort registers)
        {
            var str = new StringBuilder();
            int count = 0;
            for (int i = 0; i < 8; i++)
            {
                if ((registers & 1) != 0)
                {
                    if (count > 0) str.Append(",");
                    str.Append("D" + i);
                    count++;
                }
                registers >>= 1;
            }
            for (int i = 0; i < 8; i++)
            {
                if ((registers & 1) != 0)
                {
                    if (count > 0) str.Append(",");
                    str.Append("A" + i);
                    count++;
                }
                registers >>= 1;
            }
            return str.ToString();
        }

        void MOVEM0()
        {
            // Move register to memory
            int size = (op >> 6) & 1;
            int dstMode = (op >> 3) & 7;
            int dstReg = (op >> 0) & 7;

            ushort registers = (ushort)ReadOpWord(PC); PC += 2;
            int address = ReadAddress(dstMode, dstReg);
            int regCount = 0;

            if (size == 0)
            {
                // word-assign
                if (dstMode == 4) // decrement address
                {
                    for (int i = 7; i >= 0; i--)
                    {
                        if ((registers & 1) == 1)
                        {
                            address -= 2;
                            WriteWord(address, A[i].s16);
                            regCount++;
                        }
                        registers >>= 1;
                    }
                    for (int i = 7; i >= 0; i--)
                    {
                        if ((registers & 1) == 1)
                        {
                            address -= 2;
                            WriteWord(address, D[i].s16);
                            regCount++;
                        }
                        registers >>= 1;
                    }
                    A[dstReg].s32 = address;
                }
                else
                { // increment address
                    for (int i = 0; i <= 7; i++)
                    {
                        if ((registers & 1) == 1)
                        {
                            WriteWord(address, D[i].s16);
                            address += 2;
                            regCount++;
                        }
                        registers >>= 1;
                    }
                    for (int i = 0; i <= 7; i++)
                    {
                        if ((registers & 1) == 1)
                        {
                            WriteWord(address, A[i].s16);
                            address += 2;
                            regCount++;
                        }
                        registers >>= 1;
                    }
                }
                pendingCycles -= regCount * 4;
            }
            else
            {
                // long-assign
                if (dstMode == 4) // decrement address
                {
                    for (int i = 7; i >= 0; i--)
                    {
                        if ((registers & 1) == 1)
                        {
                            address -= 4;
                            WriteLong(address, A[i].s32);
                            regCount++;
                        }
                        registers >>= 1;
                    }
                    for (int i = 7; i >= 0; i--)
                    {
                        if ((registers & 1) == 1)
                        {
                            address -= 4;
                            WriteLong(address, D[i].s32);
                            regCount++;
                        }
                        registers >>= 1;
                    }
                    A[dstReg].s32 = address;
                }
                else
                { // increment address
                    for (int i = 0; i <= 7; i++)
                    {
                        if ((registers & 1) == 1)
                        {
                            WriteLong(address, D[i].s32);
                            address += 4;
                            regCount++;
                        }
                        registers >>= 1;
                    }
                    for (int i = 0; i <= 7; i++)
                    {
                        if ((registers & 1) == 1)
                        {
                            WriteLong(address, A[i].s32);
                            address += 4;
                            regCount++;
                        }
                        registers >>= 1;
                    }
                }
                pendingCycles -= regCount * 8;
            }

            switch (dstMode)
            {
                case 2: pendingCycles -= 8; break;
                case 3: pendingCycles -= 8; break;
                case 4: pendingCycles -= 8; break;
                case 5: pendingCycles -= 12; break;
                case 6: pendingCycles -= 14; break;
                case 7:
                    switch (dstReg)
                    {
                        case 0: pendingCycles -= 12; break;
                        case 1: pendingCycles -= 16; break;
                    }
                    break;
            }
        }


        void MOVEM1()
        {
            // Move memory to register
            int size = (op >> 6) & 1;
            int srcMode = (op >> 3) & 7;
            int srcReg = (op >> 0) & 7;

            ushort registers = (ushort)ReadOpWord(PC); PC += 2;
            int address = ReadAddress(srcMode, srcReg);
            int regCount = 0;

            if (size == 0)
            {
                // word-assign
                for (int i = 0; i < 8; i++)
                {
                    if ((registers & 1) == 1)
                    {
                        if (srcMode == 7 && (srcReg == 2 || srcReg == 3))
                        {
                            D[i].s32 = ReadPcrelWord(address);
                        }
                        else
                        {
                            D[i].s32 = ReadWord(address);
                        }
                        address += 2;
                        regCount++;
                    }
                    registers >>= 1;
                }
                for (int i = 0; i < 8; i++)
                {
                    if ((registers & 1) == 1)
                    {
                        if (srcMode == 7 && (srcReg == 2 || srcReg == 3))
                        {
                            A[i].s32 = ReadPcrelWord(address);
                        }
                        else
                        {
                            A[i].s32 = ReadWord(address);
                        }
                        address += 2;
                        regCount++;
                    }
                    registers >>= 1;
                }
                pendingCycles -= regCount * 4;
                if (srcMode == 3)
                    A[srcReg].s32 = address;
            }
            else
            {
                // long-assign
                for (int i = 0; i < 8; i++)
                {
                    if ((registers & 1) == 1)
                    {
                        if (srcMode == 7 && (srcReg == 2 || srcReg == 3))
                        {
                            D[i].s32 = ReadPcrelLong(address);
                        }
                        else
                        {
                            D[i].s32 = ReadLong(address);
                        }
                        address += 4;
                        regCount++;
                    }
                    registers >>= 1;
                }
                for (int i = 0; i < 8; i++)
                {
                    if ((registers & 1) == 1)
                    {
                        if (srcMode == 7 && (srcReg == 2 || srcReg == 3))
                        {
                            A[i].s32 = ReadPcrelLong(address);
                        }
                        else
                        {
                            A[i].s32 = ReadLong(address);
                        }
                        address += 4;
                        regCount++;
                    }
                    registers >>= 1;
                }
                pendingCycles -= regCount * 8;
                if (srcMode == 3)
                    A[srcReg].s32 = address;
            }

            switch (srcMode)
            {
                case 2: pendingCycles -= 12; break;
                case 3: pendingCycles -= 12; break;
                case 4: pendingCycles -= 12; break;
                case 5: pendingCycles -= 16; break;
                case 6: pendingCycles -= 18; break;
                case 7:
                    switch (srcReg)
                    {
                        case 0: pendingCycles -= 16; break;
                        case 1: pendingCycles -= 20; break;
                        case 2: pendingCycles -= 16; break;
                        case 3: pendingCycles -= 18; break;
                    }
                    break;
            }
        }

        void LEA()
        {
            int mode = (op >> 3) & 7;
            int sReg = (op >> 0) & 7;
            int dReg = (op >> 9) & 7;

            A[dReg].u32 = (uint)ReadAddress(mode, sReg);
            switch (mode)
            {
                case 2: pendingCycles -= 4; break;
                case 5: pendingCycles -= 8; break;
                case 6: pendingCycles -= 12; break;
                case 7:
                    switch (sReg)
                    {
                        case 0: pendingCycles -= 8; break;
                        case 1: pendingCycles -= 12; break;
                        case 2: pendingCycles -= 8; break;
                        case 3: pendingCycles -= 12; break;
                    }
                    break;
            }
        }


        void CLR()
        {
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            switch (size)
            {
                case 0: WriteValueB(mode, reg, 0); pendingCycles -= mode == 0 ? 4 : 8 + EACyclesBW[mode, reg]; break;
                case 1:
                    WriteValueW(mode, reg, 0);
                    pendingCycles -= mode == 0 ? 4 : 8 + EACyclesBW[mode, reg]; break;
                case 2: WriteValueL(mode, reg, 0); pendingCycles -= mode == 0 ? 6 : 12 + EACyclesL[mode, reg]; break;
            }

            N = V = C = false;
            Z = true;
        }


        void EXT()
        {
            int size = (op >> 6) & 1;
            int reg = op & 7;

            switch (size)
            {
                case 0: // ext.w
                    D[reg].s16 = D[reg].s8;
                    N = (D[reg].s16 & 0x8000) != 0;
                    Z = (D[reg].s16 == 0);
                    break;
                case 1: // ext.l
                    D[reg].s32 = D[reg].s16;
                    N = (D[reg].s32 & 0x80000000) != 0;
                    Z = (D[reg].s32 == 0);
                    break;
            }

            V = false;
            C = false;
            pendingCycles -= 4;
        }


        void PEA()
        {
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;
            int ea = ReadAddress(mode, reg);

            A[7].s32 -= 4;
            WriteLong(A[7].s32, ea);

            switch (mode)
            {
                case 2: pendingCycles -= 12; break;
                case 5: pendingCycles -= 16; break;
                case 6: pendingCycles -= 20; break;
                case 7:
                    switch (reg)
                    {
                        case 0: pendingCycles -= 16; break;
                        case 1: pendingCycles -= 20; break;
                        case 2: pendingCycles -= 16; break;
                        case 3: pendingCycles -= 20; break;
                    }
                    break;
            }
        }

    }
}
