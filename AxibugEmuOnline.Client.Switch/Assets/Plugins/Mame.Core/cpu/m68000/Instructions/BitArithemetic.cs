using System;

namespace cpu.m68000
{
    partial class MC68000
    {
        void AND0() // AND <ea>, Dn
        {
            int dstReg = (op >> 9) & 0x07;
            int size = (op >> 6) & 0x03;
            int srcMode = (op >> 3) & 0x07;
            int srcReg = op & 0x07;

            V = false;
            C = false;

            switch (size)
            {
                case 0: // Byte
                    D[dstReg].s8 &= ReadValueB(srcMode, srcReg);
                    pendingCycles -= (srcMode == 0) ? 4 : 4 + EACyclesBW[srcMode, srcReg];
                    N = (D[dstReg].s8 & 0x80) != 0;
                    Z = (D[dstReg].s8 == 0);
                    return;
                case 1: // Word
                    D[dstReg].s16 &= ReadValueW(srcMode, srcReg);
                    pendingCycles -= (srcMode == 0) ? 4 : 4 + EACyclesBW[srcMode, srcReg];
                    N = (D[dstReg].s16 & 0x8000) != 0;
                    Z = (D[dstReg].s16 == 0);
                    return;
                case 2: // Long
                    D[dstReg].s32 &= ReadValueL(srcMode, srcReg);
                    if (srcMode == 0 || (srcMode == 7 && srcReg == 4))
                    {
                        pendingCycles -= 8 + EACyclesL[srcMode, srcReg];
                    }
                    else
                    {
                        pendingCycles -= 6 + EACyclesL[srcMode, srcReg];
                    }
                    N = (D[dstReg].s32 & 0x80000000) != 0;
                    Z = (D[dstReg].s32 == 0);
                    return;
            }
        }


        void AND1() // AND Dn, <ea>
        {
            int srcReg = (op >> 9) & 0x07;
            int size = (op >> 6) & 0x03;
            int dstMode = (op >> 3) & 0x07;
            int dstReg = op & 0x07;

            V = false;
            C = false;

            switch (size)
            {
                case 0: // Byte
                    {
                        sbyte dest = PeekValueB(dstMode, dstReg);
                        sbyte value = (sbyte)(dest & D[srcReg].s8);
                        WriteValueB(dstMode, dstReg, value);
                        pendingCycles -= (dstMode == 0) ? 4 : 8 + EACyclesBW[dstMode, dstReg];
                        N = (value & 0x80) != 0;
                        Z = (value == 0);
                        return;
                    }
                case 1: // Word
                    {
                        short dest = PeekValueW(dstMode, dstReg);
                        short value = (short)(dest & D[srcReg].s16);
                        WriteValueW(dstMode, dstReg, value);
                        pendingCycles -= (dstMode == 0) ? 4 : 8 + EACyclesBW[dstMode, dstReg];
                        N = (value & 0x8000) != 0;
                        Z = (value == 0);
                        return;
                    }
                case 2: // Long
                    {
                        int dest = PeekValueL(dstMode, dstReg);
                        int value = dest & D[srcReg].s32;
                        WriteValueL(dstMode, dstReg, value);
                        pendingCycles -= (dstMode == 0) ? 8 : 12 + EACyclesL[dstMode, dstReg];
                        N = (value & 0x80000000) != 0;
                        Z = (value == 0);
                        return;
                    }
            }
        }


        void ANDI() // ANDI #<data>, <ea>
        {
            int size = (op >> 6) & 0x03;
            int dstMode = (op >> 3) & 0x07;
            int dstReg = op & 0x07;

            V = false;
            C = false;

            switch (size)
            {
                case 0: // Byte
                    {
                        sbyte imm = (sbyte)ReadOpWord(PC); PC += 2;
                        sbyte arg = PeekValueB(dstMode, dstReg);
                        sbyte result = (sbyte)(imm & arg);
                        WriteValueB(dstMode, dstReg, result);
                        pendingCycles -= (dstMode == 0) ? 8 : 12 + EACyclesBW[dstMode, dstReg];
                        N = (result & 0x80) != 0;
                        Z = (result == 0);
                        return;
                    }
                case 1: // Word
                    {
                        short imm = ReadOpWord(PC); PC += 2;
                        short arg = PeekValueW(dstMode, dstReg);
                        short result = (short)(imm & arg);
                        WriteValueW(dstMode, dstReg, result);
                        pendingCycles -= (dstMode == 0) ? 8 : 12 + EACyclesBW[dstMode, dstReg];
                        N = (result & 0x8000) != 0;
                        Z = (result == 0);
                        return;
                    }
                case 2: // Long
                    {
                        int imm = ReadOpLong(PC); PC += 4;
                        int arg = PeekValueL(dstMode, dstReg);
                        int result = imm & arg;
                        WriteValueL(dstMode, dstReg, result);
                        pendingCycles -= (dstMode == 0) ? 14 : 20 + EACyclesL[dstMode, dstReg];
                        N = (result & 0x80000000) != 0;
                        Z = (result == 0);
                        return;
                    }
            }
        }


        void ANDI_CCR() //m68k_op_andi_16_toc         , 0xffff, 0x023c, { 20}
        {
            short value;
            value = ReadOpWord(PC); PC += 2;
            CCR = (short)(CCR & value);
            pendingCycles -= 20;
        }


        void EOR() // EOR Dn, <ea>
        {
            int srcReg = (op >> 9) & 0x07;
            int size = (op >> 6) & 0x03;
            int dstMode = (op >> 3) & 0x07;
            int dstReg = op & 0x07;

            V = false;
            C = false;

            switch (size)
            {
                case 0: // Byte
                    {
                        sbyte dest = PeekValueB(dstMode, dstReg);
                        sbyte value = (sbyte)(dest ^ D[srcReg].s8);
                        WriteValueB(dstMode, dstReg, value);
                        pendingCycles -= (dstMode == 0) ? 4 : 8 + EACyclesBW[dstMode, dstReg];
                        N = (value & 0x80) != 0;
                        Z = (value == 0);
                        return;
                    }
                case 1: // Word
                    {
                        short dest = PeekValueW(dstMode, dstReg);
                        short value = (short)(dest ^ D[srcReg].s16);
                        WriteValueW(dstMode, dstReg, value);
                        pendingCycles -= (dstMode == 0) ? 4 : 8 + EACyclesBW[dstMode, dstReg];
                        N = (value & 0x8000) != 0;
                        Z = (value == 0);
                        return;
                    }
                case 2: // Long
                    {
                        int dest = PeekValueL(dstMode, dstReg);
                        int value = dest ^ D[srcReg].s32;
                        WriteValueL(dstMode, dstReg, value);
                        pendingCycles -= (dstMode == 0) ? 8 : 12 + EACyclesL[dstMode, dstReg];
                        N = (value & 0x80000000) != 0;
                        Z = (value == 0);
                        return;
                    }
            }
        }


        void EORI()
        {
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            V = false;
            C = false;

            switch (size)
            {
                case 0: // byte
                    {
                        sbyte immed = (sbyte)ReadOpWord(PC); PC += 2;
                        sbyte value = (sbyte)(PeekValueB(mode, reg) ^ immed);
                        WriteValueB(mode, reg, value);
                        N = (value & 0x80) != 0;
                        Z = value == 0;
                        pendingCycles -= mode == 0 ? 8 : 12 + EACyclesBW[mode, reg];
                        return;
                    }
                case 1: // word
                    {
                        short immed = ReadOpWord(PC); PC += 2;
                        short value = (short)(PeekValueW(mode, reg) ^ immed);
                        WriteValueW(mode, reg, value);
                        N = (value & 0x8000) != 0;
                        Z = value == 0;
                        pendingCycles -= mode == 0 ? 8 : 12 + EACyclesBW[mode, reg];
                        return;
                    }
                case 2: // long
                    {
                        int immed = ReadOpLong(PC); PC += 4;
                        int value = PeekValueL(mode, reg) ^ immed;
                        WriteValueL(mode, reg, value);
                        N = (value & 0x80000000) != 0;
                        Z = value == 0;
                        pendingCycles -= mode == 0 ? 16 : 20 + EACyclesL[mode, reg];
                        return;
                    }
            }
        }


        void EORI_CCR()//m68k_op_eori_16_toc         , 0xffff, 0x0a3c, { 20}
        {
            //m68ki_set_ccr(m68ki_get_ccr() ^ m68ki_read_imm_16());
            short value;
            value = ReadOpWord(PC); PC += 2;
            CCR = (short)(CCR ^ value);
            pendingCycles -= 20;
        }


        void OR0() // OR <ea>, Dn
        {
            int dstReg = (op >> 9) & 0x07;
            int size = (op >> 6) & 0x03;
            int srcMode = (op >> 3) & 0x07;
            int srcReg = op & 0x07;

            V = false;
            C = false;

            switch (size)
            {
                case 0: // Byte
                    D[dstReg].s8 |= ReadValueB(srcMode, srcReg);
                    pendingCycles -= (srcMode == 0) ? 4 : 4 + EACyclesBW[srcMode, srcReg];
                    N = (D[dstReg].s8 & 0x80) != 0;
                    Z = (D[dstReg].s8 == 0);
                    return;
                case 1: // Word
                    D[dstReg].s16 |= ReadValueW(srcMode, srcReg);
                    pendingCycles -= (srcMode == 0) ? 4 : 4 + EACyclesBW[srcMode, srcReg];
                    N = (D[dstReg].s16 & 0x8000) != 0;
                    Z = (D[dstReg].s16 == 0);
                    return;
                case 2: // Long
                    D[dstReg].s32 |= ReadValueL(srcMode, srcReg);
                    if (srcMode == 0 || (srcMode == 7 && srcReg == 4))
                    {
                        pendingCycles -= 8 + EACyclesL[srcMode, srcReg];
                    }
                    else
                    {
                        pendingCycles -= 6 + EACyclesL[srcMode, srcReg];
                    }
                    N = (D[dstReg].s32 & 0x80000000) != 0;
                    Z = (D[dstReg].s32 == 0);
                    return;
            }
        }


        void OR1() // OR Dn, <ea>
        {
            int srcReg = (op >> 9) & 0x07;
            int size = (op >> 6) & 0x03;
            int dstMode = (op >> 3) & 0x07;
            int dstReg = op & 0x07;

            V = false;
            C = false;

            switch (size)
            {
                case 0: // Byte
                    {
                        sbyte dest = PeekValueB(dstMode, dstReg);
                        sbyte value = (sbyte)(dest | D[srcReg].s8);
                        WriteValueB(dstMode, dstReg, value);
                        pendingCycles -= (dstMode == 0) ? 4 : 8 + EACyclesBW[dstMode, dstReg];
                        N = (value & 0x80) != 0;
                        Z = (value == 0);
                        return;
                    }
                case 1: // Word
                    {
                        short dest = PeekValueW(dstMode, dstReg);
                        short value = (short)(dest | D[srcReg].s16);
                        WriteValueW(dstMode, dstReg, value);
                        pendingCycles -= (dstMode == 0) ? 4 : 8 + EACyclesBW[dstMode, dstReg];
                        N = (value & 0x8000) != 0;
                        Z = (value == 0);
                        return;
                    }
                case 2: // Long
                    {
                        int dest = PeekValueL(dstMode, dstReg);
                        int value = dest | D[srcReg].s32;
                        WriteValueL(dstMode, dstReg, value);
                        pendingCycles -= (dstMode == 0) ? 8 : 12 + EACyclesL[dstMode, dstReg];
                        N = (value & 0x80000000) != 0;
                        Z = (value == 0);
                        return;
                    }
            }
        }


        void ORI()
        {
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg = (op >> 0) & 7;

            V = false;
            C = false;

            switch (size)
            {
                case 0: // byte
                    {
                        sbyte immed = (sbyte)ReadOpWord(PC); PC += 2;
                        sbyte value = (sbyte)(PeekValueB(mode, reg) | immed);
                        WriteValueB(mode, reg, value);
                        N = (value & 0x80) != 0;
                        Z = value == 0;
                        pendingCycles -= mode == 0 ? 8 : 12 + EACyclesBW[mode, reg];
                        return;
                    }
                case 1: // word
                    {
                        short immed = ReadOpWord(PC); PC += 2;
                        short value = (short)(PeekValueW(mode, reg) | immed);
                        WriteValueW(mode, reg, value);
                        N = (value & 0x8000) != 0;
                        Z = value == 0;
                        pendingCycles -= mode == 0 ? 8 : 12 + EACyclesBW[mode, reg];
                        return;
                    }
                case 2: // long
                    {
                        int immed = ReadOpLong(PC); PC += 4;
                        int value = PeekValueL(mode, reg) | immed;
                        WriteValueL(mode, reg, value);
                        N = (value & 0x80000000) != 0;
                        Z = value == 0;
                        pendingCycles -= mode == 0 ? 16 : 20 + EACyclesL[mode, reg];
                        return;
                    }
            }
        }


        void ORI_CCR()//m68k_op_ori_16_toc          , 0xffff, 0x003c, { 20}
        {
            short value;
            value = ReadOpWord(PC); PC += 2;
            CCR = (short)(CCR | value);
            pendingCycles -= 20;
        }


        void NOT()
        {
            int size = (op >> 6) & 0x03;
            int mode = (op >> 3) & 0x07;
            int reg = op & 0x07;

            V = false;
            C = false;

            switch (size)
            {
                case 0: // Byte
                    {
                        sbyte value = PeekValueB(mode, reg);
                        value = (sbyte)~value;
                        WriteValueB(mode, reg, value);
                        pendingCycles -= (mode == 0) ? 4 : 8 + EACyclesBW[mode, reg];
                        N = (value & 0x80) != 0;
                        Z = (value == 0);
                        return;
                    }
                case 1: // Word
                    {
                        short value = PeekValueW(mode, reg);
                        value = (short)~value;
                        WriteValueW(mode, reg, value);
                        pendingCycles -= (mode == 0) ? 4 : 8 + EACyclesBW[mode, reg];
                        N = (value & 0x8000) != 0;
                        Z = (value == 0);
                        return;
                    }
                case 2: // Long
                    {
                        int value = PeekValueL(mode, reg);
                        value = ~value;
                        WriteValueL(mode, reg, value);
                        pendingCycles -= (mode == 0) ? 6 : 12 + EACyclesL[mode, reg];//8:12
                        N = (value & 0x80000000) != 0;
                        Z = (value == 0);
                        return;
                    }
            }
        }


        void LSLd()
        {
            int rot = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int m = (op >> 5) & 1;
            int reg = op & 7;

            if (m == 0 && rot == 0) rot = 8;
            else if (m == 1) rot = D[rot].s32 & 63;

            V = false;
            C = false;

            switch (size)
            {
                case 0: // byte
                    for (int i = 0; i < rot; i++)
                    {
                        C = X = (D[reg].u8 & 0x80) != 0;
                        D[reg].u8 <<= 1;
                    }
                    N = (D[reg].s8 & 0x80) != 0;
                    Z = D[reg].u8 == 0;
                    pendingCycles -= 6 + (rot * 2);
                    return;
                case 1: // word
                    for (int i = 0; i < rot; i++)
                    {
                        C = X = (D[reg].u16 & 0x8000) != 0;
                        D[reg].u16 <<= 1;
                    }
                    N = (D[reg].s16 & 0x8000) != 0;
                    Z = D[reg].u16 == 0;
                    pendingCycles -= 6 + (rot * 2);
                    return;
                case 2: // long
                    for (int i = 0; i < rot; i++)
                    {
                        C = X = (D[reg].u32 & 0x80000000) != 0;
                        D[reg].u32 <<= 1;
                    }
                    N = (D[reg].s32 & 0x80000000) != 0;
                    Z = D[reg].u32 == 0;
                    pendingCycles -= 8 + (rot * 2);
                    return;
            }
        }


        void LSLd0()
        {
            //m68k_op_lsl_16_ai           , 0xfff8, 0xe3d0, { 12}
            //m68k_op_lsl_16_pi           , 0xfff8, 0xe3d8, { 12}
            //m68k_op_lsl_16_pd           , 0xfff8, 0xe3e0, { 14}
            //m68k_op_lsl_16_di           , 0xfff8, 0xe3e8, { 16}
            //m68k_op_lsl_16_ix           , 0xfff8, 0xe3f0, { 18}
            //m68k_op_lsl_16_aw           , 0xffff, 0xe3f8, { 16}
            //m68k_op_lsl_16_al           , 0xffff, 0xe3f9, { 20}
            int mode = (op >> 3) & 0x07;
            int reg = op & 0x07;
            int src;
            ushort res;
            src = PeekValueW(mode, reg);
            res = (ushort)(src << 1);
            WriteValueW(mode, reg, (short)res);
            N = ((res & 0x8000) != 0);
            Z = (res == 0);
            X = C = ((res & 0x8000) != 0);
            V = false;
            pendingCycles -= 8 + EACyclesBW[mode, reg];
        }


        void LSRd()
        {
            int rot = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int m = (op >> 5) & 1;
            int reg = op & 7;

            if (m == 0 && rot == 0) rot = 8;
            else if (m == 1) rot = D[rot].s32 & 63;

            V = false;
            C = false;

            switch (size)
            {
                case 0: // byte
                    for (int i = 0; i < rot; i++)
                    {
                        C = X = (D[reg].u8 & 1) != 0;
                        D[reg].u8 >>= 1;
                    }
                    N = (D[reg].s8 & 0x80) != 0;
                    Z = D[reg].u8 == 0;
                    pendingCycles -= 6 + (rot * 2);
                    return;
                case 1: // word
                    for (int i = 0; i < rot; i++)
                    {
                        C = X = (D[reg].u16 & 1) != 0;
                        D[reg].u16 >>= 1;
                    }
                    N = (D[reg].s16 & 0x8000) != 0;
                    Z = D[reg].u16 == 0;
                    pendingCycles -= 6 + (rot * 2);
                    return;
                case 2: // long
                    for (int i = 0; i < rot; i++)
                    {
                        C = X = (D[reg].u32 & 1) != 0;
                        D[reg].u32 >>= 1;
                    }
                    N = (D[reg].s32 & 0x80000000) != 0;
                    Z = D[reg].u32 == 0;
                    pendingCycles -= 8 + (rot * 2);
                    return;
            }
        }


        void LSRd0()
        {
            //m68k_op_lsr_16_ai           , 0xfff8, 0xe2d0, { 12}
            //m68k_op_lsr_16_pi           , 0xfff8, 0xe2d8, { 12}
            //m68k_op_lsr_16_pd           , 0xfff8, 0xe2e0, { 14}
            //m68k_op_lsr_16_di           , 0xfff8, 0xe2e8, { 16}
            //m68k_op_lsr_16_ix           , 0xfff8, 0xe2f0, { 18}
            //m68k_op_lsr_16_aw           , 0xffff, 0xe2f8, { 16}
            //m68k_op_lsr_16_al           , 0xffff, 0xe2f9, { 20}
            int mode = (op >> 3) & 0x07;
            int reg = op & 0x07;
            int src;
            ushort res;
            src = PeekValueW(mode, reg);
            res = (ushort)(src >> 1);
            WriteValueW(mode, reg, (short)res);
            N = false;
            Z = (res == 0);
            //C = X = ((src & 0x10000) != 0);
            C = X = ((src & 1) != 0);
            V = false;
            pendingCycles -= 8 + EACyclesBW[mode, reg];
        }


        void ASLd()
        {
            int rot = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int m = (op >> 5) & 1;
            int reg = op & 7;

            if (m == 0 && rot == 0) rot = 8;
            else if (m == 1) rot = D[rot].s32 & 63;

            V = false;
            C = false;

            switch (size)
            {
                case 0: // byte
                    for (int i = 0; i < rot; i++)
                    {
                        bool msb = D[reg].s8 < 0;
                        C = X = (D[reg].u8 & 0x80) != 0;
                        D[reg].s8 <<= 1;
                        V |= (D[reg].s8 < 0) != msb;
                    }
                    N = (D[reg].s8 & 0x80) != 0;
                    Z = D[reg].u8 == 0;
                    pendingCycles -= 6 + (rot * 2);
                    return;
                case 1: // word
                    for (int i = 0; i < rot; i++)
                    {
                        bool msb = D[reg].s16 < 0;
                        C = X = (D[reg].u16 & 0x8000) != 0;
                        D[reg].s16 <<= 1;
                        V |= (D[reg].s16 < 0) != msb;
                    }
                    N = (D[reg].s16 & 0x8000) != 0;
                    Z = D[reg].u16 == 0;
                    pendingCycles -= 6 + (rot * 2);
                    return;
                case 2: // long
                    for (int i = 0; i < rot; i++)
                    {
                        bool msb = D[reg].s32 < 0;
                        C = X = (D[reg].u32 & 0x80000000) != 0;
                        D[reg].s32 <<= 1;
                        V |= (D[reg].s32 < 0) != msb;
                    }
                    N = (D[reg].s32 & 0x80000000) != 0;
                    Z = D[reg].u32 == 0;
                    pendingCycles -= 8 + (rot * 2);
                    return;
            }
        }


        void ASLd0()
        {
            //m68k_op_asl_16_ai           , 0xfff8, 0xe1d0, { 12}
            //m68k_op_asl_16_pi           , 0xfff8, 0xe1d8, { 12}
            //m68k_op_asl_16_pd           , 0xfff8, 0xe1e0, { 14}
            //m68k_op_asl_16_di           , 0xfff8, 0xe1e8, { 16}
            //m68k_op_asl_16_ix           , 0xfff8, 0xe1f0, { 18}
            //m68k_op_asl_16_aw           , 0xffff, 0xe1f8, { 16}
            //m68k_op_asl_16_al           , 0xffff, 0xe1f9, { 20}
            int mode = (op >> 3) & 0x07;
            int reg = op & 0x07;
            int src;
            ushort res;
            src = PeekValueW(mode, reg);
            res = (ushort)(src << 1);
            WriteValueW(mode, reg, (short)res);
            N = ((res & 0x8000) != 0);
            Z = (res == 0);
            X = C = ((src & 0x8000) != 0);
            src &= 0xc000;
            V = !(src == 0 || src == 0xc000);
            pendingCycles -= 8 + EACyclesBW[mode, reg];
        }


        void ASRd()
        {
            int rot = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int m = (op >> 5) & 1;
            int reg = op & 7;

            if (m == 0 && rot == 0) rot = 8;
            else if (m == 1) rot = D[rot].s32 & 63;

            V = false;
            C = false;

            switch (size)
            {
                case 0: // byte
                    for (int i = 0; i < rot; i++)
                    {
                        bool msb = D[reg].s8 < 0;
                        C = X = (D[reg].u8 & 1) != 0;
                        D[reg].s8 >>= 1;
                        V |= (D[reg].s8 < 0) != msb;
                    }
                    N = (D[reg].s8 & 0x80) != 0;
                    Z = D[reg].u8 == 0;
                    pendingCycles -= 6 + (rot * 2);
                    return;
                case 1: // word
                    for (int i = 0; i < rot; i++)
                    {
                        bool msb = D[reg].s16 < 0;
                        C = X = (D[reg].u16 & 1) != 0;
                        D[reg].s16 >>= 1;
                        V |= (D[reg].s16 < 0) != msb;
                    }
                    N = (D[reg].s16 & 0x8000) != 0;
                    Z = D[reg].u16 == 0;
                    pendingCycles -= 6 + (rot * 2);
                    return;
                case 2: // long
                    for (int i = 0; i < rot; i++)
                    {
                        bool msb = D[reg].s32 < 0;
                        C = X = (D[reg].u32 & 1) != 0;
                        D[reg].s32 >>= 1;
                        V |= (D[reg].s32 < 0) != msb;
                    }
                    N = (D[reg].s32 & 0x80000000) != 0;
                    Z = D[reg].u32 == 0;
                    pendingCycles -= 8 + (rot * 2);
                    return;
            }
        }


        void ASRd0()
        {
            //m68k_op_asr_16_ai           , 0xfff8, 0xe0d0, { 12}
            //m68k_op_asr_16_pi           , 0xfff8, 0xe0d8, { 12}
            //m68k_op_asr_16_pd           , 0xfff8, 0xe0e0, { 14}
            //m68k_op_asr_16_di           , 0xfff8, 0xe0e8, { 16}
            //m68k_op_asr_16_ix           , 0xfff8, 0xe0f0, { 18}
            //m68k_op_asr_16_aw           , 0xffff, 0xe0f8, { 16}
            //m68k_op_asr_16_al           , 0xffff, 0xe0f9, { 20}
            int mode = (op >> 3) & 0x07;
            int reg = op & 0x07;
            int src;
            ushort res;
            src = PeekValueW(mode, reg);
            res = (ushort)(src >> 1);
            if ((src & 0x8000) != 0)
            {
                res |= 0x8000;
            }
            WriteValueW(mode, reg, (short)res);
            N = ((res & 0x8000) != 0);
            Z = (res == 0);
            V = false;
            C = X = ((src & 0x01) != 0);
            pendingCycles -= 8 + EACyclesBW[mode, reg];
        }


        void ROLd()
        {
            int rot = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int m = (op >> 5) & 1;
            int reg = op & 7;

            if (m == 0 && rot == 0) rot = 8;
            else if (m == 1) rot = D[rot].s32 & 63;

            V = false;
            C = false;

            switch (size)
            {
                case 0: // byte
                    for (int i = 0; i < rot; i++)
                    {
                        C = (D[reg].u8 & 0x80) != 0;
                        D[reg].u8 = (byte)((D[reg].u8 << 1) | (D[reg].u8 >> 7));
                    }
                    N = (D[reg].s8 & 0x80) != 0;
                    Z = D[reg].u8 == 0;
                    pendingCycles -= 6 + (rot * 2);
                    return;
                case 1: // word
                    for (int i = 0; i < rot; i++)
                    {
                        C = (D[reg].u16 & 0x8000) != 0;
                        D[reg].u16 = (ushort)((D[reg].u16 << 1) | (D[reg].u16 >> 15));
                    }
                    N = (D[reg].s16 & 0x8000) != 0;
                    Z = D[reg].u16 == 0;
                    pendingCycles -= 6 + (rot * 2);
                    return;
                case 2: // long
                    for (int i = 0; i < rot; i++)
                    {
                        C = (D[reg].u32 & 0x80000000) != 0;
                        D[reg].u32 = ((D[reg].u32 << 1) | (D[reg].u32 >> 31));
                    }
                    N = (D[reg].s32 & 0x80000000) != 0;
                    Z = D[reg].u32 == 0;
                    pendingCycles -= 8 + (rot * 2);
                    return;
            }
        }


        void ROLd0()
        {
            //m68k_op_rol_16_ai           , 0xfff8, 0xe7d0, { 12}
            //m68k_op_rol_16_pi           , 0xfff8, 0xe7d8, { 12}
            //m68k_op_rol_16_pd           , 0xfff8, 0xe7e0, { 14}
            //m68k_op_rol_16_di           , 0xfff8, 0xe7e8, { 16}
            //m68k_op_rol_16_ix           , 0xfff8, 0xe7f0, { 18}
            //m68k_op_rol_16_aw           , 0xffff, 0xe7f8, { 16}
            //m68k_op_rol_16_al           , 0xffff, 0xe7f9, { 20}
            int mode = (op >> 3) & 0x07;
            int reg = op & 0x07;
            int src;
            uint res;
            src = PeekValueW(mode, reg);
            res = (uint)(((src << 1) | (src >> 15)) & 0xffff);
            WriteValueW(mode, reg, (short)res);
            N = ((res & 0x8000) != 0);
            Z = (res == 0);
            C = ((res & 0x8000) != 0);
            V = false;
            pendingCycles -= 8 + EACyclesBW[mode, reg];
        }


        void RORd()
        {
            int rot = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int m = (op >> 5) & 1;
            int reg = op & 7;

            if (m == 0 && rot == 0) rot = 8;
            else if (m == 1) rot = D[rot].s32 & 63;

            V = false;
            C = false;

            switch (size)
            {
                case 0: // byte
                    for (int i = 0; i < rot; i++)
                    {
                        C = (D[reg].u8 & 1) != 0;
                        D[reg].u8 = (byte)((D[reg].u8 >> 1) | (D[reg].u8 << 7));
                    }
                    N = (D[reg].s8 & 0x80) != 0;
                    Z = D[reg].u8 == 0;
                    pendingCycles -= 6 + (rot * 2);
                    return;
                case 1: // word
                    for (int i = 0; i < rot; i++)
                    {
                        C = (D[reg].u16 & 1) != 0;
                        D[reg].u16 = (ushort)((D[reg].u16 >> 1) | (D[reg].u16 << 15));
                    }
                    N = (D[reg].s16 & 0x8000) != 0;
                    Z = D[reg].u16 == 0;
                    pendingCycles -= 6 + (rot * 2);
                    return;
                case 2: // long
                    for (int i = 0; i < rot; i++)
                    {
                        C = (D[reg].u32 & 1) != 0;
                        D[reg].u32 = ((D[reg].u32 >> 1) | (D[reg].u32 << 31));
                    }
                    N = (D[reg].s32 & 0x80000000) != 0;
                    Z = D[reg].u32 == 0;
                    pendingCycles -= 8 + (rot * 2);
                    return;
            }
        }


        void RORd0()
        {
            //m68k_op_ror_16_ai           , 0xfff8, 0xe6d0, { 12}
            //m68k_op_ror_16_pi           , 0xfff8, 0xe6d8, { 12}
            //m68k_op_ror_16_pd           , 0xfff8, 0xe6e0, { 14}
            //m68k_op_ror_16_di           , 0xfff8, 0xe6e8, { 16}
            //m68k_op_ror_16_ix           , 0xfff8, 0xe6f0, { 18}
            //m68k_op_ror_16_aw           , 0xffff, 0xe6f8, { 16}
            //m68k_op_ror_16_al           , 0xffff, 0xe6f9, { 20}
            int mode = (op >> 3) & 0x07;
            int reg = op & 0x07;
            int src;
            uint res;
            src = PeekValueW(mode, reg);
            res = (uint)(((src >> 1) | (src << 15)) & 0xffff);
            WriteValueW(mode, reg, (short)res);
            N = ((res & 0x8000) != 0);
            Z = (res == 0);
            C = ((res & 0x01) != 0);
            V = false;
            pendingCycles -= 8 + EACyclesBW[mode, reg];
        }


        void ROXLd()
        {
            int rot = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int m = (op >> 5) & 1;
            int reg = op & 7;

            if (m == 0 && rot == 0) rot = 8;
            else if (m == 1) rot = D[rot].s32 & 63;

            C = X;
            V = false;

            switch (size)
            {
                case 0: // byte
                    for (int i = 0; i < rot; i++)
                    {
                        C = (D[reg].u8 & 0x80) != 0;
                        D[reg].u8 = (byte)((D[reg].u8 << 1) | (X ? 1 : 0));
                        X = C;
                    }
                    N = (D[reg].s8 & 0x80) != 0;
                    Z = D[reg].s8 == 0;
                    pendingCycles -= 6 + (rot * 2);
                    return;
                case 1: // word
                    for (int i = 0; i < rot; i++)
                    {
                        C = (D[reg].u16 & 0x8000) != 0;
                        D[reg].u16 = (ushort)((D[reg].u16 << 1) | (X ? 1 : 0));
                        X = C;
                    }
                    N = (D[reg].s16 & 0x8000) != 0;
                    Z = D[reg].s16 == 0;
                    pendingCycles -= 6 + (rot * 2);
                    return;
                case 2: // long
                    for (int i = 0; i < rot; i++)
                    {
                        C = (D[reg].s32 & 0x80000000) != 0;
                        D[reg].s32 = ((D[reg].s32 << 1) | (X ? 1 : 0));
                        X = C;
                    }
                    N = (D[reg].s32 & 0x80000000) != 0;
                    Z = D[reg].s32 == 0;
                    pendingCycles -= 8 + (rot * 2);
                    return;
            }
        }


        void ROXLd0()
        {
            //m68k_op_roxl_16_ai          , 0xfff8, 0xe5d0, { 12}
            //m68k_op_roxl_16_pi          , 0xfff8, 0xe5d8, { 12}
            //m68k_op_roxl_16_pd          , 0xfff8, 0xe5e0, { 14}
            //m68k_op_roxl_16_di          , 0xfff8, 0xe5e8, { 16}
            //m68k_op_roxl_16_ix          , 0xfff8, 0xe5f0, { 18}
            //m68k_op_roxl_16_aw          , 0xffff, 0xe5f8, { 16}
            //m68k_op_roxl_16_al          , 0xffff, 0xe5f9, { 20}
            int mode = (op >> 3) & 0x07;
            int reg = op & 0x07;
            ushort src;
            uint src2;
            uint res;
            src = (ushort)PeekValueW(mode, reg);
            src2 = (uint)(src | (X ? 0x10000 : 0));
            res = ((src2 << 1) | (src2 >> 16));
            C = X = (((res >> 8) & 0x100) != 0);
            WriteValueW(mode, reg, (short)res);
            N = ((res & 0x8000) != 0);
            Z = (res == 0);
            V = false;
            pendingCycles -= 8 + EACyclesBW[mode, reg];
        }



        void ROXRd()
        {
            int rot = (op >> 9) & 7;
            int size = (op >> 6) & 3;
            int m = (op >> 5) & 1;
            int reg = op & 7;

            if (m == 0 && rot == 0) rot = 8;
            else if (m == 1) rot = D[rot].s32 & 63;

            C = X;
            V = false;

            switch (size)
            {
                case 0: // byte
                    for (int i = 0; i < rot; i++)
                    {
                        C = (D[reg].u8 & 1) != 0;
                        D[reg].u8 = (byte)((D[reg].u8 >> 1) | (X ? 0x80 : 0));
                        X = C;
                    }
                    N = (D[reg].s8 & 0x80) != 0;
                    Z = D[reg].s8 == 0;
                    pendingCycles -= 6 + (rot * 2);
                    return;
                case 1: // word
                    for (int i = 0; i < rot; i++)
                    {
                        C = (D[reg].u16 & 1) != 0;
                        D[reg].u16 = (ushort)((D[reg].u16 >> 1) | (X ? 0x8000 : 0));
                        X = C;
                    }
                    N = (D[reg].s16 & 0x8000) != 0;
                    Z = D[reg].s16 == 0;
                    pendingCycles -= 6 + (rot * 2);
                    return;
                case 2: // long
                    for (int i = 0; i < rot; i++)
                    {
                        C = (D[reg].s32 & 1) != 0;
                        D[reg].u32 = ((D[reg].u32 >> 1) | (X ? 0x80000000 : 0));
                        X = C;
                    }
                    N = (D[reg].s32 & 0x80000000) != 0;
                    Z = D[reg].s32 == 0;
                    pendingCycles -= 8 + (rot * 2);
                    return;
            }
        }

        void ROXRd0()
        {
            //m68k_op_roxr_16_ai          , 0xfff8, 0xe4d0, { 12}
            //m68k_op_roxr_16_pi          , 0xfff8, 0xe4d8, { 12}
            //m68k_op_roxr_16_pd          , 0xfff8, 0xe4e0, { 14}
            //m68k_op_roxr_16_di          , 0xfff8, 0xe4e8, { 16}
            //m68k_op_roxr_16_ix          , 0xfff8, 0xe4f0, { 18}
            //m68k_op_roxr_16_aw          , 0xffff, 0xe4f8, { 16}
            //m68k_op_roxr_16_al          , 0xffff, 0xe4f9, { 20}
            int mode = (op >> 3) & 0x07;
            int reg = op & 0x07;
            ushort src;
            uint src2;
            uint res;
            src = (ushort)PeekValueW(mode, reg);
            src2 = ((uint)src | (uint)(X ? 0x10000 : 0));
            res = ((src2 >> 1) | (src2 << 16));
            C = X = ((res & 0x10000) != 0);
            res = res & 0xffff;
            WriteValueW(mode, reg, (short)res);
            N = ((res & 0x8000) != 0);
            Z = (res == 0);
            V = false;
            pendingCycles -= 8 + EACyclesBW[mode, reg];
        }


        void SWAP()
        {
            int reg = op & 7;
            D[reg].u32 = (D[reg].u32 << 16) | (D[reg].u32 >> 16);
            V = C = false;
            Z = D[reg].u32 == 0;
            N = (D[reg].s32 & 0x80000000) != 0;
            pendingCycles -= 4;
        }

    }
}
