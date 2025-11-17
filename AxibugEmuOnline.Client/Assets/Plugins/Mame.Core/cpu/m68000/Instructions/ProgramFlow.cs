using System;

namespace cpu.m68000
{
    unsafe partial class MC68000
    {
        bool TestCondition(int condition)
        {
            switch (condition)
            {
                case 0x00: return true;     // True
                case 0x01: return false;    // False
                case 0x02: return !C && !Z; // High (Unsigned)
                case 0x03: return C || Z;   // Less or Same (Unsigned)
                case 0x04: return !C;       // Carry Clear (High or Same)
                case 0x05: return C;        // Carry Set (Lower)
                case 0x06: return !Z;       // Not Equal
                case 0x07: return Z;        // Equal
                case 0x08: return !V;       // Overflow Clear
                case 0x09: return V;        // Overflow Set
                case 0x0A: return !N;       // Plus (Positive)
                case 0x0B: return N;        // Minus (Negative)
                case 0x0C: return N && V || !N && !V;             // Greater or Equal
                case 0x0D: return N && !V || !N && V;             // Less Than
                case 0x0E: return N && V && !Z || !N && !V && !Z; // Greater Than
                case 0x0F: return Z || N && !V || !N && V;        // Less or Equal
                default:
                    throw new Exception("Invalid condition " + condition);
            }
        }

        string DisassembleCondition(int condition)
        {
            switch (condition)
            {
                case 0x00: return "t";  // True
                case 0x01: return "f";  // False
                case 0x02: return "hi"; // High (Unsigned)
                case 0x03: return "ls"; // Less or Same (Unsigned)
                case 0x04: return "cc"; // Carry Clear (High or Same)
                case 0x05: return "cs"; // Carry Set (Lower)
                case 0x06: return "ne"; // Not Equal
                case 0x07: return "eq"; // Equal
                case 0x08: return "vc"; // Overflow Clear
                case 0x09: return "vs"; // Overflow Set
                case 0x0A: return "pl"; // Plus (Positive)
                case 0x0B: return "mi"; // Minus (Negative)
                case 0x0C: return "ge"; // Greater or Equal
                case 0x0D: return "lt"; // Less Than
                case 0x0E: return "gt"; // Greater Than
                case 0x0F: return "le"; // Less or Equal
                default: return "??"; // Invalid condition
            }
        }

        void Bcc() // Branch on condition
        {
            sbyte displacement8 = (sbyte)op;
            int cond = (op >> 8) & 0x0F;

            if (TestCondition(cond) == true)
            {
                if (displacement8 != 0)
                {
                    // use opcode-embedded displacement
                    PC += displacement8;
                    pendingCycles -= 10;
                }
                else
                {
                    // use extension word displacement
                    PC += ReadOpWord(PC);
                    pendingCycles -= 10;
                }
            }
            else
            { // false
                if (displacement8 != 0)
                    pendingCycles -= 8;
                else
                {
                    PC += 2;
                    pendingCycles -= 12;
                }
            }
        }


        void BRA()
        {
            sbyte displacement8 = (sbyte)op;

            if (displacement8 != 0)
                PC += displacement8;
            else
                PC += ReadOpWord(PC);
            if (PPC == PC)
            {
                pendingCycles = 0;
            }
            pendingCycles -= 10;
        }


        void BSR()
        {
            sbyte displacement8 = (sbyte)op;

            A[7].s32 -= 4;
            if (displacement8 != 0)
            {
                // use embedded displacement
                WriteLong(A[7].s32, PC);
                PC += displacement8;
            }
            else
            {
                // use extension word displacement
                WriteLong(A[7].s32, PC + 2);
                PC += ReadOpWord(PC);
            }
            pendingCycles -= 18;
        }


        void DBcc()
        {
            if (TestCondition((op >> 8) & 0x0F) == true)
            {
                PC += 2; // condition met, break out of loop
                pendingCycles -= 12;
            }
            else
            {
                int reg = op & 7;
                D[reg].u16--;

                if (D[reg].u16 == 0xFFFF)
                {
                    PC += 2; // counter underflowed, break out of loop
                    pendingCycles -= 14;
                }
                else
                {
                    PC += ReadOpWord(PC); // condition false and counter not exhausted, so branch.
                    pendingCycles -= 10;
                }
            }
        }


        void RTS()
        {
            PC = ReadLong(A[7].s32);
            A[7].s32 += 4;
            pendingCycles -= 16;
        }


        void RTR()
        {
            short value = ReadWord(A[7].s32);
            A[7].s32 += 2;
            CCR = value;
            PC = ReadLong(A[7].s32);
            A[7].s32 += 4;
            pendingCycles -= 20;
        }

        void RESET()
        {
            if (s)
            {
                pendingCycles -= 132;
            }
            else
            {
                TrapVector2(8);
            }
        }

        void RTE()
        {
            short newSR = ReadWord(A[7].s32);
            A[7].s32 += 2;
            PC = ReadLong(A[7].s32);
            A[7].s32 += 4;
            SR = newSR;
            pendingCycles -= 20;
        }

        void TAS()
        {
            int mode = (op >> 3) & 0x07;
            int reg = op & 0x07;
            byte result;
            //result = (byte)ReadValueB(mode, reg);
            result = (byte)PeekValueB(mode, reg);
            Z = (result == 0);
            N = ((result & 0x80) != 0);
            V = false;
            C = false;
            /*if (mode == 0)
            {
                //D[reg].u8 = (byte)(result | 0x80);
            }*/
            WriteValueB(mode, reg, (sbyte)(result | 0x80));
            pendingCycles -= (mode == 0) ? 4 : 14 + EACyclesBW[mode, reg];
        }

        void TST()
        {
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            int value;
            switch (size)
            {
                case 0: value = ReadValueB(mode, reg); pendingCycles -= 4 + EACyclesBW[mode, reg]; N = (value & 0x80) != 0; break;
                case 1: value = ReadValueW(mode, reg); pendingCycles -= 4 + EACyclesBW[mode, reg]; N = (value & 0x8000) != 0; break;
                default: value = ReadValueL(mode, reg); pendingCycles -= 4 + EACyclesL[mode, reg]; N = (value & 0x80000000) != 0; break;
            }
            V = false;
            C = false;
            Z = (value == 0);
        }

        void BTSTi()
        {
            int bit = ReadOpWord(PC); PC += 2;
            int mode = (op >> 3) & 7;
            int reg = op & 7;

            if (mode == 0)
            {
                bit &= 31;
                int mask = 1 << bit;
                Z = (D[reg].s32 & mask) == 0;
                pendingCycles -= 10;
            }
            else
            {
                bit &= 7;
                int mask = 1 << bit;
                Z = (ReadValueB(mode, reg) & mask) == 0;
                pendingCycles -= 8 + EACyclesBW[mode, reg];
            }
        }

        void BTSTr()
        {
            int dReg = (op >> 9) & 7;
            int mode = (op >> 3) & 7;
            int reg = op & 7;
            int bit = D[dReg].s32;

            if (mode == 0)
            {
                bit &= 31;
                int mask = 1 << bit;
                Z = (D[reg].s32 & mask) == 0;
                pendingCycles -= 6;
            }
            else
            {
                bit &= 7;
                int mask = 1 << bit;
                Z = (ReadValueB(mode, reg) & mask) == 0;
                pendingCycles -= 4 + EACyclesBW[mode, reg];
            }
        }

        void BCHGi()
        {
            int bit = ReadOpWord(PC); PC += 2;
            int mode = (op >> 3) & 7;
            int reg = op & 7;

            if (mode == 0)
            {
                bit &= 31;
                int mask = 1 << bit;
                Z = (D[reg].s32 & mask) == 0;
                D[reg].s32 ^= mask;
                pendingCycles -= 12;
            }
            else
            {
                bit &= 7;
                int mask = 1 << bit;
                sbyte value = PeekValueB(mode, reg);
                Z = (value & mask) == 0;
                value ^= (sbyte)mask;
                WriteValueB(mode, reg, value);
                pendingCycles -= 12 + EACyclesBW[mode, reg];
            }
        }


        void BCHGr()
        {
            int dReg = (op >> 9) & 7;
            int mode = (op >> 3) & 7;
            int reg = op & 7;
            int bit = D[dReg].s32;

            if (mode == 0)
            {
                bit &= 31;
                int mask = 1 << bit;
                Z = (D[reg].s32 & mask) == 0;
                D[reg].s32 ^= mask;
                pendingCycles -= 8;
            }
            else
            {
                bit &= 7;
                int mask = 1 << bit;
                sbyte value = PeekValueB(mode, reg);
                Z = (value & mask) == 0;
                value ^= (sbyte)mask;
                WriteValueB(mode, reg, value);
                pendingCycles -= 8 + EACyclesBW[mode, reg];
            }
        }

        void BCLRi()
        {
            int bit = ReadOpWord(PC); PC += 2;
            int mode = (op >> 3) & 7;
            int reg = op & 7;

            if (mode == 0)
            {
                bit &= 31;
                int mask = 1 << bit;
                Z = (D[reg].s32 & mask) == 0;
                D[reg].s32 &= ~mask;
                pendingCycles -= 14;
            }
            else
            {
                bit &= 7;
                int mask = 1 << bit;
                sbyte value = PeekValueB(mode, reg);
                Z = (value & mask) == 0;
                value &= (sbyte)~mask;
                WriteValueB(mode, reg, value);
                pendingCycles -= 12 + EACyclesBW[mode, reg];
            }
        }

        void BCLRr()
        {
            int dReg = (op >> 9) & 7;
            int mode = (op >> 3) & 7;
            int reg = op & 7;
            int bit = D[dReg].s32;

            if (mode == 0)
            {
                bit &= 31;
                int mask = 1 << bit;
                Z = (D[reg].s32 & mask) == 0;
                D[reg].s32 &= ~mask;
                pendingCycles -= 10;
            }
            else
            {
                bit &= 7;
                int mask = 1 << bit;
                sbyte value = PeekValueB(mode, reg);
                Z = (value & mask) == 0;
                value &= (sbyte)~mask;
                WriteValueB(mode, reg, value);
                pendingCycles -= 8 + EACyclesBW[mode, reg];
            }
        }


        void BSETi()
        {
            int bit = ReadOpWord(PC); PC += 2;
            int mode = (op >> 3) & 7;
            int reg = op & 7;

            if (mode == 0)
            {
                bit &= 31;
                int mask = 1 << bit;
                Z = (D[reg].s32 & mask) == 0;
                D[reg].s32 |= mask;
                pendingCycles -= 12;
            }
            else
            {
                bit &= 7;
                int mask = 1 << bit;
                sbyte value = PeekValueB(mode, reg);
                Z = (value & mask) == 0;
                value |= (sbyte)mask;
                WriteValueB(mode, reg, value);
                pendingCycles -= 12 + EACyclesBW[mode, reg];
            }
        }


        void BSETr()
        {
            int dReg = (op >> 9) & 7;
            int mode = (op >> 3) & 7;
            int reg = op & 7;
            int bit = D[dReg].s32;

            if (mode == 0)
            {
                bit &= 31;
                int mask = 1 << bit;
                Z = (D[reg].s32 & mask) == 0;
                D[reg].s32 |= mask;
                pendingCycles -= 8;
            }
            else
            {
                bit &= 7;
                int mask = 1 << bit;
                sbyte value = PeekValueB(mode, reg);
                Z = (value & mask) == 0;
                value |= (sbyte)mask;
                WriteValueB(mode, reg, value);
                pendingCycles -= 8 + EACyclesBW[mode, reg];
            }
        }


        void JMP()
        {
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;
            PC = ReadAddress(mode, reg);
            if (PPC == PC)
            {
                pendingCycles = 0;
            }
            switch (mode)
            {
                case 2: pendingCycles -= 8; break;
                case 5: pendingCycles -= 10; break;
                case 6: pendingCycles -= 14; break;
                case 7:
                    switch (reg)
                    {
                        case 0: pendingCycles -= 10; break;
                        case 1: pendingCycles -= 12; break;
                        case 2: pendingCycles -= 10; break;
                        case 3: pendingCycles -= 14; break;
                    }
                    break;
            }
        }


        void JSR()
        {
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;
            int addr = ReadAddress(mode, reg);

            A[7].s32 -= 4;
            WriteLong(A[7].s32, PC);
            PC = addr;

            switch (mode)
            {
                case 2: pendingCycles -= 16; break;
                case 5: pendingCycles -= 18; break;
                case 6: pendingCycles -= 22; break;
                case 7:
                    switch (reg)
                    {
                        case 0: pendingCycles -= 18; break;
                        case 1: pendingCycles -= 20; break;
                        case 2: pendingCycles -= 18; break;
                        case 3: pendingCycles -= 22; break;
                    }
                    break;
            }
        }


        void LINK()
        {
            int reg = op & 7;
            A[7].s32 -= 4;
            short offset = ReadOpWord(PC); PC += 2;
            WriteLong(A[7].s32, A[reg].s32);
            A[reg].s32 = A[7].s32;
            A[7].s32 += offset;
            pendingCycles -= 16;
        }


        void UNLK()
        {
            int reg = op & 7;
            A[7].s32 = A[reg].s32;
            A[reg].s32 = ReadLong(A[7].s32);
            A[7].s32 += 4;
            pendingCycles -= 12;
        }


        void NOP()
        {
            pendingCycles -= 4;
        }


        void Scc() // Set on condition
        {
            int cond = (op >> 8) & 0x0F;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            if (TestCondition(cond) == true)
            {
                WriteValueB(mode, reg, -1);
                if (mode == 0) pendingCycles -= 6;
                else pendingCycles -= 8 + EACyclesBW[mode, reg];
            }
            else
            {
                WriteValueB(mode, reg, 0);
                if (mode == 0)
                    pendingCycles -= 4;
                else
                    pendingCycles -= 8 + EACyclesBW[mode, reg];
            }
        }

    }
}
