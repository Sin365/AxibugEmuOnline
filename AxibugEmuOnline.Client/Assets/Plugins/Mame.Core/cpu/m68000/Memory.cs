using System;

namespace cpu.m68000
{
    partial class MC68000
    {
        sbyte ReadValueB(int mode, int reg)
        {
            sbyte value;
            switch (mode)
            {
                case 0: // Dn
                    return D[reg].s8;
                case 1: // An
                    return A[reg].s8;
                case 2: // (An)
                    return ReadByte(A[reg].s32);
                case 3: // (An)+
                    value = ReadByte(A[reg].s32);
                    A[reg].s32 += reg == 7 ? 2 : 1;
                    return value;
                case 4: // -(An)
                    A[reg].s32 -= reg == 7 ? 2 : 1;
                    return ReadByte(A[reg].s32);
                case 5: // (d16,An)
                    value = ReadByte((A[reg].s32 + ReadOpWord(PC))); PC += 2;
                    return value;
                case 6: // (d8,An,Xn)
                    return ReadByte(A[reg].s32 + GetIndex());
                case 7:
                    switch (reg)
                    {
                        case 0: // (imm).W
                            value = ReadByte(ReadOpWord(PC)); PC += 2;
                            return value;
                        case 1: // (imm).L
                            value = ReadByte(ReadOpLong(PC)); PC += 4;
                            return value;
                        case 2: // (d16,PC)
                            value = ReadOpByte(PC + ReadOpWord(PC)); PC += 2;
                            return value;
                        case 3: // (d8,PC,Xn)
                            int pc = PC;
                            value = ReadOpByte((pc + GetIndex()));
                            return value;
                        case 4: // immediate
                            value = (sbyte)ReadOpWord(PC); PC += 2;
                            return value;
                        default:
                            throw new Exception("Invalid addressing mode!");
                    }
            }
            throw new Exception("Invalid addressing mode!");
        }

        short ReadValueW(int mode, int reg)
        {
            short value;
            switch (mode)
            {
                case 0: // Dn
                    return D[reg].s16;
                case 1: // An
                    return A[reg].s16;
                case 2: // (An)
                    return ReadWord(A[reg].s32);
                case 3: // (An)+
                    value = ReadWord(A[reg].s32);
                    A[reg].s32 += 2;
                    return value;
                case 4: // -(An)
                    A[reg].s32 -= 2;
                    return ReadWord(A[reg].s32);
                case 5: // (d16,An)
                    value = ReadWord((A[reg].s32 + ReadOpWord(PC))); PC += 2;
                    return value;
                case 6: // (d8,An,Xn)
                    return ReadWord(A[reg].s32 + GetIndex());
                case 7:
                    switch (reg)
                    {
                        case 0: // (imm).W
                            value = ReadWord(ReadOpWord(PC)); PC += 2;
                            return value;
                        case 1: // (imm).L
                            value = ReadWord(ReadOpLong(PC)); PC += 4;
                            return value;
                        case 2: // (d16,PC)
                            value = ReadOpWord(PC + ReadOpWord(PC)); PC += 2;
                            return value;
                        case 3: // (d8,PC,Xn)
                            int pc = PC;
                            value = ReadOpWord((pc + GetIndex()));
                            return value;
                        case 4: // immediate
                            value = ReadOpWord(PC); PC += 2;
                            return value;
                        default:
                            throw new Exception("Invalid addressing mode!");
                    }
            }
            throw new Exception("Invalid addressing mode!");
        }

        int ReadValueL(int mode, int reg)
        {
            int value;
            switch (mode)
            {
                case 0: // Dn
                    return D[reg].s32;
                case 1: // An
                    return A[reg].s32;
                case 2: // (An)
                    return ReadLong(A[reg].s32);
                case 3: // (An)+
                    value = ReadLong(A[reg].s32);
                    A[reg].s32 += 4;
                    return value;
                case 4: // -(An)
                    A[reg].s32 -= 4;
                    return ReadLong(A[reg].s32);
                case 5: // (d16,An)
                    value = ReadLong((A[reg].s32 + ReadOpWord(PC))); PC += 2;
                    return value;
                case 6: // (d8,An,Xn)
                    return ReadLong(A[reg].s32 + GetIndex());
                case 7:
                    switch (reg)
                    {
                        case 0: // (imm).W
                            value = ReadLong(ReadOpWord(PC)); PC += 2;
                            return value;
                        case 1: // (imm).L
                            value = ReadLong(ReadOpLong(PC)); PC += 4;
                            return value;
                        case 2: // (d16,PC)
                            value = ReadOpLong(PC + ReadOpWord(PC)); PC += 2;
                            return value;
                        case 3: // (d8,PC,Xn)
                            int pc = PC;
                            value = ReadOpLong((pc + GetIndex()));
                            return value;
                        case 4: // immediate
                            value = ReadOpLong(PC); PC += 4;
                            return value;
                        default:
                            throw new Exception("Invalid addressing mode!");
                    }
            }
            throw new Exception("Invalid addressing mode!");
        }

        sbyte PeekValueB(int mode, int reg)
        {
            sbyte value;
            switch (mode)
            {
                case 0: // Dn
                    return D[reg].s8;
                case 1: // An
                    return A[reg].s8;
                case 2: // (An)
                    return ReadByte(A[reg].s32);
                case 3: // (An)+
                    value = ReadByte(A[reg].s32);
                    return value;
                case 4: // -(An)
                    value = ReadByte(A[reg].s32 - (reg == 7 ? 2 : 1));
                    return value;
                case 5: // (d16,An)
                    value = ReadByte((A[reg].s32 + ReadOpWord(PC)));
                    return value;
                case 6: // (d8,An,Xn)
                    return ReadByte(A[reg].s32 + PeekIndex());
                case 7:
                    switch (reg)
                    {
                        case 0: // (imm).W
                            value = ReadByte(ReadOpWord(PC));
                            return value;
                        case 1: // (imm).L
                            value = ReadByte(ReadOpLong(PC));
                            return value;
                        case 2: // (d16,PC)
                            value = ReadByte(PC + ReadOpWord(PC));
                            return value;
                        case 3: // (d8,PC,Xn)
                            value = ReadByte((PC + PeekIndex()));
                            return value;
                        case 4: // immediate
                            return (sbyte)ReadOpWord(PC);
                        default:
                            throw new Exception("Invalid addressing mode!");
                    }
            }
            throw new Exception("Invalid addressing mode!");
        }

        short PeekValueW(int mode, int reg)
        {
            short value;
            switch (mode)
            {
                case 0: // Dn
                    return D[reg].s16;
                case 1: // An
                    return A[reg].s16;
                case 2: // (An)
                    return ReadWord(A[reg].s32);
                case 3: // (An)+
                    value = ReadWord(A[reg].s32);
                    return value;
                case 4: // -(An)
                    value = ReadWord(A[reg].s32 - 2);
                    return value;
                case 5: // (d16,An)
                    value = ReadWord((A[reg].s32 + ReadOpWord(PC)));
                    return value;
                case 6: // (d8,An,Xn)
                    return ReadWord(A[reg].s32 + PeekIndex());
                case 7:
                    switch (reg)
                    {
                        case 0: // (imm).W
                            value = ReadWord(ReadOpWord(PC));
                            return value;
                        case 1: // (imm).L
                            value = ReadWord(ReadOpLong(PC));
                            return value;
                        case 2: // (d16,PC)
                            value = ReadWord(PC + ReadOpWord(PC));
                            return value;
                        case 3: // (d8,PC,Xn)
                            value = ReadWord((PC + PeekIndex()));
                            return value;
                        case 4: // immediate
                            return ReadOpWord(PC);
                        default:
                            throw new Exception("Invalid addressing mode!");
                    }
            }
            throw new Exception("Invalid addressing mode!");
        }

        int PeekValueL(int mode, int reg)
        {
            int value;
            switch (mode)
            {
                case 0: // Dn
                    return D[reg].s32;
                case 1: // An
                    return A[reg].s32;
                case 2: // (An)
                    return ReadLong(A[reg].s32);
                case 3: // (An)+
                    value = ReadLong(A[reg].s32);
                    return value;
                case 4: // -(An)
                    value = ReadLong(A[reg].s32 - 4);
                    return value;
                case 5: // (d16,An)
                    value = ReadLong((A[reg].s32 + ReadOpWord(PC)));
                    return value;
                case 6: // (d8,An,Xn)
                    return ReadLong(A[reg].s32 + PeekIndex());
                case 7:
                    switch (reg)
                    {
                        case 0: // (imm).W
                            value = ReadLong(ReadOpWord(PC));
                            return value;
                        case 1: // (imm).L
                            value = ReadLong(ReadOpLong(PC));
                            return value;
                        case 2: // (d16,PC)
                            value = ReadLong(PC + ReadOpWord(PC));
                            return value;
                        case 3: // (d8,PC,Xn)
                            value = ReadLong((PC + PeekIndex()));
                            return value;
                        case 4: // immediate
                            return ReadOpLong(PC);
                        default:
                            throw new Exception("Invalid addressing mode!");
                    }
            }
            throw new Exception("Invalid addressing mode!");
        }

        int ReadAddress(int mode, int reg)
        {
            int addr;
            switch (mode)
            {
                case 0: throw new Exception("Invalid addressing mode!"); // Dn
                case 1: throw new Exception("Invalid addressing mode!"); // An
                case 2: return A[reg].s32; // (An)
                case 3: return A[reg].s32; // (An)+
                case 4: return A[reg].s32; // -(An)
                case 5: addr = A[reg].s32 + ReadOpWord(PC); PC += 2; return addr; // (d16,An)
                case 6: return A[reg].s32 + GetIndex(); // (d8,An,Xn)
                case 7:
                    switch (reg)
                    {
                        case 0: addr = ReadOpWord(PC); PC += 2; return addr; // (imm).w
                        case 1: addr = ReadOpLong(PC); PC += 4; return addr; // (imm).l
                        case 2: addr = PC; addr += ReadOpWord(PC); PC += 2; return addr; // (d16,PC)
                        case 3: addr = PC; addr += GetIndex(); return addr; // (d8,PC,Xn)
                        case 4: throw new Exception("Invalid addressing mode!"); // immediate
                    }
                    break;
            }
            throw new Exception("Invalid addressing mode!");
        }


        void WriteValueB(int mode, int reg, sbyte value)
        {
            switch (mode)
            {
                case 0x00: // Dn
                    D[reg].s8 = value;
                    return;
                case 0x01: // An
                    A[reg].s32 = value;
                    return;
                case 0x02: // (An)
                    WriteByte(A[reg].s32, value);
                    return;
                case 0x03: // (An)+
                    WriteByte(A[reg].s32, value);
                    A[reg].s32 += reg == 7 ? 2 : 1;
                    return;
                case 0x04: // -(An)
                    A[reg].s32 -= reg == 7 ? 2 : 1;
                    WriteByte(A[reg].s32, value);
                    return;
                case 0x05: // (d16,An)
                    WriteByte(A[reg].s32 + ReadOpWord(PC), value); PC += 2;
                    return;
                case 0x06: // (d8,An,Xn)
                    WriteByte(A[reg].s32 + GetIndex(), value);
                    return;
                case 0x07:
                    switch (reg)
                    {
                        case 0x00: // (imm).W
                            WriteByte(ReadOpWord(PC), value); PC += 2;
                            return;
                        case 0x01: // (imm).L
                            WriteByte(ReadOpLong(PC), value); PC += 4;
                            return;
                        case 0x02: // (d16,PC)
                            WriteByte(PC + ReadOpWord(PC), value); PC += 2;
                            return;
                        case 0x03: // (d8,PC,Xn)
                            int pc = PC;
                            WriteByte(pc + PeekIndex(), value);
                            PC += 2;
                            return;
                        default: throw new Exception("Invalid addressing mode!");
                    }
            }
        }

        void WriteValueW(int mode, int reg, short value)
        {
            switch (mode)
            {
                case 0x00: // Dn
                    D[reg].s16 = value;
                    return;
                case 0x01: // An
                    A[reg].s32 = value;
                    return;
                case 0x02: // (An)
                    WriteWord(A[reg].s32, value);
                    return;
                case 0x03: // (An)+
                    WriteWord(A[reg].s32, value);
                    A[reg].s32 += 2;
                    return;
                case 0x04: // -(An)
                    A[reg].s32 -= 2;
                    WriteWord(A[reg].s32, value);
                    return;
                case 0x05: // (d16,An)
                    WriteWord(A[reg].s32 + ReadOpWord(PC), value); PC += 2;
                    return;
                case 0x06: // (d8,An,Xn)
                    WriteWord(A[reg].s32 + GetIndex(), value);
                    return;
                case 0x07:
                    switch (reg)
                    {
                        case 0x00: // (imm).W
                            WriteWord(ReadOpWord(PC), value); PC += 2;
                            return;
                        case 0x01: // (imm).L
                            WriteWord(ReadOpLong(PC), value); PC += 4;
                            return;
                        case 0x02: // (d16,PC)
                            WriteWord(PC + ReadOpWord(PC), value); PC += 2;
                            return;
                        case 0x03: // (d8,PC,Xn)
                            int pc = PC;
                            WriteWord(pc + PeekIndex(), value);
                            PC += 2;
                            return;
                        default: throw new Exception("Invalid addressing mode!");
                    }
            }
        }

        void WriteValueL(int mode, int reg, int value)
        {
            switch (mode)
            {
                case 0x00: // Dn
                    D[reg].s32 = value;
                    return;
                case 0x01: // An
                    A[reg].s32 = value;
                    return;
                case 0x02: // (An)
                    WriteLong(A[reg].s32, value);
                    return;
                case 0x03: // (An)+
                    WriteLong(A[reg].s32, value);
                    A[reg].s32 += 4;
                    return;
                case 0x04: // -(An)
                    A[reg].s32 -= 4;
                    WriteLong(A[reg].s32, value);
                    return;
                case 0x05: // (d16,An)
                    WriteLong(A[reg].s32 + ReadOpWord(PC), value); PC += 2;
                    return;
                case 0x06: // (d8,An,Xn)
                    WriteLong(A[reg].s32 + GetIndex(), value);
                    return;
                case 0x07:
                    switch (reg)
                    {
                        case 0x00: // (imm).W
                            WriteLong(ReadOpWord(PC), value); PC += 2;
                            return;
                        case 0x01: // (imm).L
                            WriteLong(ReadOpLong(PC), value); PC += 4;
                            return;
                        case 0x02: // (d16,PC)
                            WriteLong(PC + ReadOpWord(PC), value); PC += 2;
                            return;
                        case 0x03: // (d8,PC,Xn)
                            int pc = PC;
                            WriteLong(pc + PeekIndex(), value);
                            PC += 2;
                            return;
                        default: throw new Exception("Invalid addressing mode!");
                    }
            }
        }

        int GetIndex()
        {
            //EmuLogger.Log("IN INDEX PORTION - NOT VERIFIED!!!");
            // TODO kid chameleon triggers this in startup sequence

            short extension = ReadOpWord(PC); PC += 2;

            int da = (extension >> 15) & 0x1;
            int reg = (extension >> 12) & 0x7;
            int size = (extension >> 11) & 0x1;
            int scale = (extension >> 9) & 0x3;
            sbyte displacement = (sbyte)extension;

            int indexReg;
            switch (scale)
            {
                case 0: indexReg = 1; break;
                case 1: indexReg = 2; break;
                case 2: indexReg = 4; break;
                default: indexReg = 8; break;
            }
            if (da == 0)
                indexReg *= size == 0 ? D[reg].s16 : D[reg].s32;
            else
                indexReg *= size == 0 ? A[reg].s16 : A[reg].s32;

            return displacement + indexReg;
        }

        int PeekIndex()
        {
            //EmuLogger.Log("IN INDEX PORTION - NOT VERIFIED!!!");

            short extension = ReadOpWord(PC);

            int da = (extension >> 15) & 0x1;
            int reg = (extension >> 12) & 0x7;
            int size = (extension >> 11) & 0x1;
            int scale = (extension >> 9) & 0x3;
            sbyte displacement = (sbyte)extension;

            int indexReg;
            switch (scale)
            {
                case 0: indexReg = 1; break;
                case 1: indexReg = 2; break;
                case 2: indexReg = 4; break;
                default: indexReg = 8; break;
            }
            if (da == 0)
                indexReg *= size == 0 ? D[reg].s16 : D[reg].s32;
            else
                indexReg *= size == 0 ? A[reg].s16 : A[reg].s32;

            return displacement + indexReg;
        }

    }
}