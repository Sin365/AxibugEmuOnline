namespace cpu.nec
{
    partial class Nec
    {
        void i_add_br8()
        {
            int ModRM;
            byte src, dst;
            DEF_br8(out ModRM, out src, out dst);
            ADDB(ref src, ref dst);
            PutbackRMByte(ModRM, dst);
            CLKM(ModRM, 2, 2, 2, 16, 16, 7);
        }
        void i_add_wr16()
        {
            int ModRM;
            ushort src, dst;
            DEF_wr16(out ModRM, out src, out dst);
            ADDW(ref src, ref dst);
            PutbackRMWord(ModRM, dst);
            CLKR(ModRM, 24, 24, 11, 24, 16, 7, 2, EA);
        }
        void i_add_r8b()
        {
            int ModRM;
            byte src, dst;
            DEF_r8b(out ModRM, out src, out dst);
            ADDB(ref src, ref dst);
            I.regs.b[mod_RM.regb[ModRM]] = dst;
            CLKM(ModRM, 2, 2, 2, 11, 11, 6);
        }
        void i_add_r16w()
        {
            int ModRM;
            ushort src, dst;
            DEF_r16w(out ModRM, out src, out dst);
            ADDW(ref src, ref dst);
            //I.regs.w[mod_RM.regw[ModRM]] = dst;
            I.regs.b[mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
            I.regs.b[mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
            CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, EA);
        }
        void i_add_ald8()
        {
            byte src, dst;
            DEF_ald8(out src, out dst);
            ADDB(ref src, ref dst);
            I.regs.b[0] = dst;
            CLKS(4, 4, 2);
        }
        void i_add_axd16()
        {
            ushort src, dst;
            DEF_axd16(out src, out dst);
            ADDW(ref src, ref dst);
            //I.regs.w[0] = dst;
            I.regs.b[0] = (byte)(dst % 0x100);
            I.regs.b[1] = (byte)(dst / 0x100);
            CLKS(4, 4, 2);
        }
        void i_push_es()
        {
            PUSH(I.sregs[0]);
            CLKS(12, 8, 3);
        }
        void i_pop_es()
        {
            POP(ref I.sregs[0]);
            CLKS(12, 8, 5);
        }
        void i_or_br8()
        {
            int ModRM;
            byte src, dst;
            DEF_br8(out ModRM, out src, out dst);
            ORB(ref src, ref dst);
            PutbackRMByte(ModRM, dst);
            CLKM(ModRM, 2, 2, 2, 16, 16, 7);
        }
        void i_or_wr16()
        {
            int ModRM;
            ushort src, dst;
            DEF_wr16(out ModRM, out src, out dst);
            ORW(ref src, ref dst);
            PutbackRMWord(ModRM, dst);
            CLKR(ModRM, 24, 24, 11, 24, 16, 7, 2, EA);
        }
        void i_or_r8b()
        {
            int ModRM;
            byte src, dst;
            DEF_r8b(out ModRM, out src, out dst);
            ORB(ref src, ref dst);
            I.regs.b[mod_RM.regb[ModRM]] = dst;
            CLKM(ModRM, 2, 2, 2, 11, 11, 6);
        }
        void i_or_r16w()
        {
            int ModRM;
            ushort src, dst;
            DEF_r16w(out ModRM, out src, out dst);
            ORW(ref src, ref dst);
            //I.regs.w[mod_RM.regw[ModRM]] = dst;
            I.regs.b[mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
            I.regs.b[mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
            CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, EA);
        }
        void i_or_ald8()
        {
            byte src, dst;
            DEF_ald8(out src, out dst);
            ORB(ref src, ref dst);
            I.regs.b[0] = dst;
            CLKS(4, 4, 2);
        }
        void i_or_axd16()
        {
            ushort src, dst;
            DEF_axd16(out src, out dst);
            ORW(ref src, ref dst);
            //I.regs.w[0] = dst;
            I.regs.b[0] = (byte)(dst % 0x100);
            I.regs.b[1] = (byte)(dst / 0x100);
            CLKS(4, 4, 2);
        }
        void i_push_cs()
        {
            PUSH(I.sregs[1]);
            CLKS(12, 8, 3);
        }
        void i_pre_nec()
        {
            int ModRM = 0, tmp = 0, tmp2 = 0;
            switch (FETCH())
            {
                case 0x10: BITOP_BYTE(ref ModRM, ref tmp); CLKS(3, 3, 4); tmp2 = I.regs.b[2] & 0x7; I.ZeroVal = (uint)(((tmp & (1 << tmp2)) != 0) ? 1 : 0); I.CarryVal = I.OverVal = 0; break; /* Test */
                case 0x11: BITOP_WORD(ref ModRM, ref tmp); CLKS(3, 3, 4); tmp2 = I.regs.b[2] & 0xf; I.ZeroVal = (uint)(((tmp & (1 << tmp2)) != 0) ? 1 : 0); I.CarryVal = I.OverVal = 0; break; /* Test */
                case 0x12: BITOP_BYTE(ref ModRM, ref tmp); CLKS(5, 5, 4); tmp2 = I.regs.b[2] & 0x7; tmp &= ~(1 << tmp2); PutbackRMByte(ModRM, (byte)tmp); break; /* Clr */
                case 0x13: BITOP_WORD(ref ModRM, ref tmp); CLKS(5, 5, 4); tmp2 = I.regs.b[2] & 0xf; tmp &= ~(1 << tmp2); PutbackRMWord(ModRM, (ushort)tmp); break; /* Clr */
                case 0x14: BITOP_BYTE(ref ModRM, ref tmp); CLKS(4, 4, 4); tmp2 = I.regs.b[2] & 0x7; tmp |= (1 << tmp2); PutbackRMByte(ModRM, (byte)tmp); break; /* Set */
                case 0x15: BITOP_WORD(ref ModRM, ref tmp); CLKS(4, 4, 4); tmp2 = I.regs.b[2] & 0xf; tmp |= (1 << tmp2); PutbackRMWord(ModRM, (ushort)tmp); break; /* Set */
                case 0x16: BITOP_BYTE(ref ModRM, ref tmp); CLKS(4, 4, 4); tmp2 = I.regs.b[2] & 0x7; BIT_NOT(ref tmp, ref tmp2); PutbackRMByte(ModRM, (byte)tmp); break; /* Not */
                case 0x17: BITOP_WORD(ref ModRM, ref tmp); CLKS(4, 4, 4); tmp2 = I.regs.b[2] & 0xf; BIT_NOT(ref tmp, ref tmp2); PutbackRMWord(ModRM, (ushort)tmp); break; /* Not */

                case 0x18: BITOP_BYTE(ref ModRM, ref tmp); CLKS(4, 4, 4); tmp2 = (FETCH()) & 0x7; I.ZeroVal = (uint)(((tmp & (1 << tmp2)) != 0) ? 1 : 0); I.CarryVal = I.OverVal = 0; break; /* Test */
                case 0x19: BITOP_WORD(ref ModRM, ref tmp); CLKS(4, 4, 4); tmp2 = (FETCH()) & 0xf; I.ZeroVal = (uint)(((tmp & (1 << tmp2)) != 0) ? 1 : 0); I.CarryVal = I.OverVal = 0; break; /* Test */
                case 0x1a: BITOP_BYTE(ref ModRM, ref tmp); CLKS(6, 6, 4); tmp2 = (FETCH()) & 0x7; tmp &= ~(1 << tmp2); PutbackRMByte(ModRM, (byte)tmp); break; /* Clr */
                case 0x1b: BITOP_WORD(ref ModRM, ref tmp); CLKS(6, 6, 4); tmp2 = (FETCH()) & 0xf; tmp &= ~(1 << tmp2); PutbackRMWord(ModRM, (ushort)tmp); break; /* Clr */
                case 0x1c: BITOP_BYTE(ref ModRM, ref tmp); CLKS(5, 5, 4); tmp2 = (FETCH()) & 0x7; tmp |= (1 << tmp2); PutbackRMByte(ModRM, (byte)tmp); break; /* Set */
                case 0x1d: BITOP_WORD(ref ModRM, ref tmp); CLKS(5, 5, 4); tmp2 = (FETCH()) & 0xf; tmp |= (1 << tmp2); PutbackRMWord(ModRM, (ushort)tmp); break; /* Set */
                case 0x1e: BITOP_BYTE(ref ModRM, ref tmp); CLKS(5, 5, 4); tmp2 = (FETCH()) & 0x7; BIT_NOT(ref tmp, ref tmp2); PutbackRMByte(ModRM, (byte)tmp); break; /* Not */
                case 0x1f: BITOP_WORD(ref ModRM, ref tmp); CLKS(5, 5, 4); tmp2 = (FETCH()) & 0xf; BIT_NOT(ref tmp, ref tmp2); PutbackRMWord(ModRM, (ushort)tmp); break; /* Not */

                case 0x20: ADD4S(ref tmp, ref tmp2); CLKS(7, 7, 2); break;
                case 0x22: SUB4S(ref tmp, ref tmp2); CLKS(7, 7, 2); break;
                case 0x26: CMP4S(ref tmp, ref tmp2); CLKS(7, 7, 2); break;
                case 0x28: ModRM = FETCH(); tmp = GetRMByte(ModRM); tmp <<= 4; tmp |= I.regs.b[0] & 0xf; I.regs.b[0] = (byte)((I.regs.b[0] & 0xf0) | ((tmp >> 8) & 0xf)); tmp &= 0xff; PutbackRMByte(ModRM, (byte)tmp); CLKM(ModRM, 13, 13, 9, 28, 28, 15); break;
                case 0x2a: ModRM = FETCH(); tmp = GetRMByte(ModRM); tmp2 = (I.regs.b[0] & 0xf) << 4; I.regs.b[0] = (byte)((I.regs.b[0] & 0xf0) | (tmp & 0xf)); tmp = tmp2 | (tmp >> 4); PutbackRMByte(ModRM, (byte)tmp); CLKM(ModRM, 17, 17, 13, 32, 32, 19); break;
                case 0x31: ModRM = FETCH(); ModRM = 0; break;
                case 0x33: ModRM = FETCH(); ModRM = 0; break;
                case 0x92: CLK(2); break; /* V25/35 FINT */
                case 0xe0: ModRM = FETCH(); ModRM = 0; break;
                case 0xf0: ModRM = FETCH(); ModRM = 0; break;
                case 0xff: ModRM = FETCH(); ModRM = 0; break;
                default: break;
            }
        }
        void i_adc_br8()
        {
            int ModRM;
            byte src, dst;
            DEF_br8(out ModRM, out src, out dst);
            src += (byte)(CF() ? 1 : 0);
            ADDB(ref src, ref dst);
            PutbackRMByte(ModRM, dst);
            CLKM(ModRM, 2, 2, 2, 16, 16, 7);
        }
        void i_adc_wr16()
        {
            int ModRM;
            ushort src, dst;
            DEF_wr16(out ModRM, out src, out dst);
            src += (ushort)(CF() ? 1 : 0);
            ADDW(ref src, ref dst);
            PutbackRMWord(ModRM, dst);
            CLKR(ModRM, 24, 24, 11, 24, 16, 7, 2, EA);
        }
        void i_adc_r8b()
        {
            int ModRM;
            byte src, dst;
            DEF_r8b(out ModRM, out src, out dst);
            src += (byte)(CF() ? 1 : 0);
            ADDB(ref src, ref dst);
            I.regs.b[mod_RM.regb[ModRM]] = dst;
            CLKM(ModRM, 2, 2, 2, 11, 11, 6);
        }
        void i_adc_r16w()
        {
            int ModRM;
            ushort src, dst;
            DEF_r16w(out ModRM, out src, out dst);
            src += (ushort)(CF() ? 1 : 0);
            ADDW(ref src, ref dst);
            //I.regs.w[mod_RM.regw[ModRM]] = dst;
            I.regs.b[mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
            I.regs.b[mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
            CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, EA);
        }
        void i_adc_ald8()
        {
            byte src, dst;
            DEF_ald8(out src, out dst);
            src += (byte)(CF() ? 1 : 0);
            ADDB(ref src, ref dst);
            I.regs.b[0] = dst;
            CLKS(4, 4, 2);
        }
        void i_adc_axd16()
        {
            ushort src, dst;
            DEF_axd16(out src, out dst);
            src += (ushort)(CF() ? 1 : 0);
            ADDW(ref src, ref dst);
            //I.regs.w[0] = dst;
            I.regs.b[0] = (byte)(dst % 0x100);
            I.regs.b[1] = (byte)(dst / 0x100);
            CLKS(4, 4, 2);
        }
        void i_push_ss()
        {
            PUSH(I.sregs[2]);
            CLKS(12, 8, 3);
        }
        void i_pop_ss()
        {
            POP(ref I.sregs[2]);
            CLKS(12, 8, 5);
            I.no_interrupt = 1;
        }
        void i_sbb_br8()
        {
            int ModRM;
            byte src, dst;
            DEF_br8(out ModRM, out src, out dst);
            src += (byte)(CF() ? 1 : 0);
            SUBB(ref src, ref dst);
            PutbackRMByte(ModRM, dst);
            CLKM(ModRM, 2, 2, 2, 16, 16, 7);
        }
        void i_sbb_wr16()
        {
            int ModRM;
            ushort src, dst;
            DEF_wr16(out ModRM, out src, out dst);
            src += (ushort)(CF() ? 1 : 0);
            SUBW(ref src, ref dst);
            PutbackRMWord(ModRM, dst);
            CLKR(ModRM, 24, 24, 11, 24, 16, 7, 2, EA);
        }
        void i_sbb_r8b()
        {
            int ModRM;
            byte src, dst;
            DEF_r8b(out ModRM, out src, out dst);
            src += (byte)(CF() ? 1 : 0);
            SUBB(ref src, ref dst);
            I.regs.b[mod_RM.regb[ModRM]] = dst;
            CLKM(ModRM, 2, 2, 2, 11, 11, 6);
        }
        void i_sbb_r16w()
        {
            int ModRM;
            ushort src, dst;
            DEF_r16w(out ModRM, out src, out dst);
            src += (ushort)(CF() ? 1 : 0);
            SUBW(ref src, ref dst);
            //I.regs.w[mod_RM.regw[ModRM]] = dst;
            I.regs.b[mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
            I.regs.b[mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
            CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, EA);
        }
        void i_sbb_ald8()
        {
            byte src, dst;
            DEF_ald8(out src, out dst);
            src += (byte)(CF() ? 1 : 0);
            SUBB(ref src, ref dst);
            I.regs.b[0] = dst;
            CLKS(4, 4, 2);
        }
        void i_sbb_axd16()
        {
            ushort src, dst;
            DEF_axd16(out src, out dst);
            src += (ushort)(CF() ? 1 : 0);
            SUBW(ref src, ref dst);
            //I.regs.w[0] = dst;
            I.regs.b[0] = (byte)(dst % 0x100);
            I.regs.b[1] = (byte)(dst / 0x100);
            CLKS(4, 4, 2);
        }
        void i_push_ds()
        {
            PUSH(I.sregs[3]);
            CLKS(12, 8, 3);
        }
        void i_pop_ds()
        {
            POP(ref I.sregs[3]);
            CLKS(12, 8, 5);
        }
        void i_and_br8()
        {
            int ModRM;
            byte src, dst;
            DEF_br8(out ModRM, out src, out dst);
            ANDB(ref src, ref dst);
            PutbackRMByte(ModRM, dst);
            CLKM(ModRM, 2, 2, 2, 16, 16, 7);
        }
        void i_and_wr16()
        {
            int ModRM;
            ushort src, dst;
            DEF_wr16(out ModRM, out src, out dst);
            ANDW(ref src, ref dst);
            PutbackRMWord(ModRM, dst);
            CLKR(ModRM, 24, 24, 11, 24, 16, 7, 2, EA);
        }
        void i_and_r8b()
        {
            int ModRM;
            byte src, dst;
            DEF_r8b(out ModRM, out src, out dst);
            ANDB(ref src, ref dst);
            I.regs.b[mod_RM.regb[ModRM]] = dst;
            CLKM(ModRM, 2, 2, 2, 11, 11, 6);
        }
        void i_and_r16w()
        {
            int ModRM;
            ushort src, dst;
            DEF_r16w(out ModRM, out src, out dst);
            ANDW(ref src, ref dst);
            //I.regs.w[mod_RM.regw[ModRM]] = dst;
            I.regs.b[mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
            I.regs.b[mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
            CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, EA);
        }
        void i_and_ald8()
        {
            byte src, dst;
            DEF_ald8(out src, out dst);
            ANDB(ref src, ref dst);
            I.regs.b[0] = dst;
            CLKS(4, 4, 2);
        }
        void i_and_axd16()
        {
            ushort src, dst;
            DEF_axd16(out src, out dst);
            ANDW(ref src, ref dst);
            //I.regs.w[0] = dst;
            I.regs.b[0] = (byte)(dst % 0x100);
            I.regs.b[1] = (byte)(dst / 0x100);
            CLKS(4, 4, 2);
        }
        void i_es()
        {
            seg_prefix = 1;
            prefix_base = I.sregs[0] << 4;
            CLK(2);
            nec_instruction[fetchop()]();
            seg_prefix = 0;
        }
        void i_daa()
        {
            ADJ4(6, 0x60);
            CLKS(3, 3, 2);
        }
        void i_sub_br8()
        {
            int ModRM;
            byte src, dst;
            DEF_br8(out ModRM, out src, out dst);
            SUBB(ref src, ref dst);
            PutbackRMByte(ModRM, dst);
            CLKM(ModRM, 2, 2, 2, 16, 16, 7);
        }
        void i_sub_wr16()
        {
            int ModRM;
            ushort src, dst;
            DEF_wr16(out ModRM, out src, out dst);
            SUBW(ref src, ref dst);
            PutbackRMWord(ModRM, dst);
            CLKR(ModRM, 24, 24, 11, 24, 16, 7, 2, EA);
        }
        void i_sub_r8b()
        {
            int ModRM;
            byte src, dst;
            DEF_r8b(out ModRM, out src, out dst);
            SUBB(ref src, ref dst);
            I.regs.b[mod_RM.regb[ModRM]] = dst;
            CLKM(ModRM, 2, 2, 2, 11, 11, 6);
        }
        void i_sub_r16w()
        {
            int ModRM;
            ushort src, dst;
            DEF_r16w(out ModRM, out src, out dst);
            SUBW(ref src, ref dst);
            //I.regs.w[mod_RM.regw[ModRM]] = dst;
            I.regs.b[mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
            I.regs.b[mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
            CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, EA);
        }
        void i_sub_ald8()
        {
            byte src, dst;
            DEF_ald8(out src, out dst);
            SUBB(ref src, ref dst);
            I.regs.b[0] = dst;
            CLKS(4, 4, 2);
        }
        void i_sub_axd16()
        {
            ushort src, dst;
            DEF_axd16(out src, out dst);
            SUBW(ref src, ref dst);
            //I.regs.w[0] = dst;
            I.regs.b[0] = (byte)(dst % 0x100);
            I.regs.b[1] = (byte)(dst / 0x100);
            CLKS(4, 4, 2);
        }
        void i_cs()
        {
            seg_prefix = 1;
            prefix_base = I.sregs[1] << 4;
            CLK(2);
            nec_instruction[fetchop()]();
            seg_prefix = 0;
        }
        void i_das()
        {
            ADJ4(-6, -0x60);
            CLKS(3, 3, 2);
        }
        void i_xor_br8()
        {
            int ModRM;
            byte src, dst;
            DEF_br8(out ModRM, out src, out dst);
            XORB(ref src, ref dst);
            PutbackRMByte(ModRM, dst);
            CLKM(ModRM, 2, 2, 2, 16, 16, 7);
        }
        void i_xor_wr16()
        {
            int ModRM;
            ushort src, dst;
            DEF_wr16(out ModRM, out src, out dst);
            XORW(ref src, ref dst);
            PutbackRMWord(ModRM, dst);
            CLKR(ModRM, 24, 24, 11, 24, 16, 7, 2, EA);
        }
        void i_xor_r8b()
        {
            int ModRM;
            byte src, dst;
            DEF_r8b(out ModRM, out src, out dst);
            XORB(ref src, ref dst);
            I.regs.b[mod_RM.regb[ModRM]] = dst;
            CLKM(ModRM, 2, 2, 2, 11, 11, 6);
        }
        void i_xor_r16w()
        {
            int ModRM;
            ushort src, dst;
            DEF_r16w(out ModRM, out src, out dst);
            XORW(ref src, ref dst);
            //I.regs.w[mod_RM.regw[ModRM]] = dst;
            I.regs.b[mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
            I.regs.b[mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
            CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, EA);
        }
        void i_xor_ald8()
        {
            byte src, dst;
            DEF_ald8(out src, out dst);
            XORB(ref src, ref dst);
            I.regs.b[0] = dst;
            CLKS(4, 4, 2);
        }
        void i_xor_axd16()
        {
            ushort src, dst;
            DEF_axd16(out src, out dst);
            XORW(ref src, ref dst);
            //I.regs.w[0] = dst;
            I.regs.b[0] = (byte)(dst % 0x100);
            I.regs.b[1] = (byte)(dst / 0x100);
            CLKS(4, 4, 2);
        }
        void i_ss()
        {
            seg_prefix = 1;
            prefix_base = I.sregs[2] << 4;
            CLK(2);
            nec_instruction[fetchop()]();
            seg_prefix = 0;
        }
        void i_aaa()
        {
            ADJB(6, (I.regs.b[0] > 0xf9) ? 2 : 1);
            CLKS(7, 7, 4);
        }
        void i_cmp_br8()
        {
            int ModRM;
            byte src, dst;
            DEF_br8(out ModRM, out src, out dst);
            SUBB(ref src, ref dst);
            CLKM(ModRM, 2, 2, 2, 11, 11, 6);
        }
        void i_cmp_wr16()
        {
            int ModRM;
            ushort src, dst;
            DEF_wr16(out ModRM, out src, out dst);
            SUBW(ref src, ref dst);
            CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, EA);
        }
        void i_cmp_r8b()
        {
            int ModRM;
            byte src, dst;
            DEF_r8b(out ModRM, out src, out dst);
            SUBB(ref src, ref dst);
            CLKM(ModRM, 2, 2, 2, 11, 11, 6);
        }
        void i_cmp_r16w()
        {
            int ModRM;
            ushort src, dst;
            DEF_r16w(out ModRM, out src, out dst);
            SUBW(ref src, ref dst);
            CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, EA);
        }
        void i_cmp_ald8()
        {
            byte src, dst;
            DEF_ald8(out src, out dst);
            SUBB(ref src, ref dst);
            CLKS(4, 4, 2);
        }
        void i_cmp_axd16()
        {
            ushort src, dst;
            DEF_axd16(out src, out dst);
            SUBW(ref src, ref dst);
            CLKS(4, 4, 2);
        }
        void i_ds()
        {
            seg_prefix = 1;
            prefix_base = I.sregs[3] << 4;
            CLK(2);
            nec_instruction[fetchop()]();
            seg_prefix = 0;
        }
        void i_aas()
        {
            ADJB(-6, (I.regs.b[0] < 6) ? -2 : -1);
            CLKS(7, 7, 4);
        }
        void i_inc_ax()
        {
            IncWordReg(0);
            CLK(2);
        }
        void i_inc_cx()
        {
            IncWordReg(1);
            CLK(2);
        }
        void i_inc_dx()
        {
            IncWordReg(2);
            CLK(2);
        }
        void i_inc_bx()
        {
            IncWordReg(3);
            CLK(2);
        }
        void i_inc_sp()
        {
            IncWordReg(4);
            CLK(2);
        }
        void i_inc_bp()
        {
            IncWordReg(5);
            CLK(2);
        }
        void i_inc_si()
        {
            IncWordReg(6);
            CLK(2);
        }
        void i_inc_di()
        {
            IncWordReg(7);
            CLK(2);
        }
        void i_dec_ax()
        {
            DecWordReg(0);
            CLK(2);
        }
        void i_dec_cx()
        {
            DecWordReg(1);
            CLK(2);
        }
        void i_dec_dx()
        {
            DecWordReg(2);
            CLK(2);
        }
        void i_dec_bx()
        {
            DecWordReg(3);
            CLK(2);
        }
        void i_dec_sp()
        {
            DecWordReg(4);
            CLK(2);
        }
        void i_dec_bp()
        {
            DecWordReg(5);
            CLK(2);
        }
        void i_dec_si()
        {
            DecWordReg(6);
            CLK(2);
        }
        void i_dec_di()
        {
            DecWordReg(7);
            CLK(2);
        }
        void i_push_ax()
        {
            //PUSH(I.regs.w[0]);
            PUSH((ushort)(I.regs.b[0] + I.regs.b[1] * 0x100));
            CLKS(12, 8, 3);
        }
        void i_push_cx()
        {
            //PUSH(I.regs.w[1]);
            PUSH((ushort)(I.regs.b[2] + I.regs.b[3] * 0x100));
            CLKS(12, 8, 3);
        }
        void i_push_dx()
        {
            //PUSH(I.regs.w[2]);
            PUSH((ushort)(I.regs.b[4] + I.regs.b[5] * 0x100));
            CLKS(12, 8, 3);
        }
        void i_push_bx()
        {
            //PUSH(I.regs.w[3]);
            PUSH((ushort)(I.regs.b[6] + I.regs.b[7] * 0x100));
            CLKS(12, 8, 3);
        }
        void i_push_sp()
        {
            //PUSH(I.regs.w[4]);
            PUSH((ushort)(I.regs.b[8] + I.regs.b[9] * 0x100));
            CLKS(12, 8, 3);
        }
        void i_push_bp()
        {
            //PUSH(I.regs.w[5]);
            PUSH((ushort)(I.regs.b[10] + I.regs.b[11] * 0x100));
            CLKS(12, 8, 3);
        }
        void i_push_si()
        {
            //PUSH(I.regs.w[6]);
            PUSH((ushort)(I.regs.b[12] + I.regs.b[13] * 0x100));
            CLKS(12, 8, 3);
        }
        void i_push_di()
        {
            //PUSH(I.regs.w[7]);
            PUSH((ushort)(I.regs.b[14] + I.regs.b[15] * 0x100));
            CLKS(12, 8, 3);
        }
        void i_pop_ax()
        {
            //POP(ref I.regs.w[0]);
            POPW(0);
            CLKS(12, 8, 5);
        }
        void i_pop_cx()
        {
            //POP(ref I.regs.w[1]);
            POPW(1);
            CLKS(12, 8, 5);
        }
        void i_pop_dx()
        {
            //POP(ref I.regs.w[2]);
            POPW(2);
            CLKS(12, 8, 5);
        }
        void i_pop_bx()
        {
            //POP(ref I.regs.w[3]);
            POPW(3);
            CLKS(12, 8, 5);
        }
        void i_pop_sp()
        {
            //POP(ref I.regs.w[4]);
            POPW(4);
            CLKS(12, 8, 5);
        }
        void i_pop_bp()
        {
            //POP(ref I.regs.w[5]);
            POPW(5);
            CLKS(12, 8, 5);
        }
        void i_pop_si()
        {
            //POP(ref I.regs.w[6]);
            POPW(6);
            CLKS(12, 8, 5);
        }
        void i_pop_di()
        {
            //POP(ref I.regs.w[7]);
            POPW(7);
            CLKS(12, 8, 5);
        }
        void i_pusha()
        {
            ushort tmp = (ushort)(I.regs.b[8] + I.regs.b[9] * 0x100);// I.regs.w[4];
            /*PUSH(I.regs.w[0]);
            PUSH(I.regs.w[1]);
            PUSH(I.regs.w[2]);
            PUSH(I.regs.w[3]);*/
            PUSH((ushort)(I.regs.b[0] + I.regs.b[1] * 0x100));
            PUSH((ushort)(I.regs.b[2] + I.regs.b[3] * 0x100));
            PUSH((ushort)(I.regs.b[4] + I.regs.b[5] * 0x100));
            PUSH((ushort)(I.regs.b[6] + I.regs.b[7] * 0x100));
            PUSH(tmp);
            /*PUSH(I.regs.w[5]);
            PUSH(I.regs.w[6]);
            PUSH(I.regs.w[7]);*/
            PUSH((ushort)(I.regs.b[10] + I.regs.b[11] * 0x100));
            PUSH((ushort)(I.regs.b[12] + I.regs.b[13] * 0x100));
            PUSH((ushort)(I.regs.b[14] + I.regs.b[15] * 0x100));
            CLKS(67, 35, 20);
        }
        void i_popa()
        {
            ushort tmp = 0;
            /*POP(ref I.regs.w[7]);
            POP(ref I.regs.w[6]);
            POP(ref I.regs.w[5]);*/
            POPW(7);
            POPW(6);
            POPW(5);
            POP(ref tmp);
            /*POP(ref I.regs.w[3]);
            POP(ref I.regs.w[2]);
            POP(ref I.regs.w[1]);
            POP(ref I.regs.w[0]);*/
            POPW(3);
            POPW(2);
            POPW(1);
            POPW(0);
            CLKS(75, 43, 22);
        }
        void i_chkind()
        {
            int low, high, tmp;
            int ModRM;
            ModRM = GetModRM();
            low = GetRMWord(ModRM);
            high = GetnextRMWord();
            tmp = RegWord(ModRM);
            if (tmp < low || tmp > high)
            {
                nec_interrupt(5, false);
            }
            pendingCycles -= 20;
        }
        void i_brkn()
        {
            nec_interrupt(FETCH(), true);
            CLKS(50, 50, 24);
        }
        void i_repnc()
        {
            int next = fetchop();
            ushort c = (ushort)(I.regs.b[2] + I.regs.b[3] * 0x100);// I.regs.w[1];
            switch (next)
            { /* Segments */
                case 0x26: seg_prefix = 1; prefix_base = (I.sregs[0] << 4); next = fetchop(); CLK(2); break;
                case 0x2e: seg_prefix = 1; prefix_base = (I.sregs[1] << 4); next = fetchop(); CLK(2); break;
                case 0x36: seg_prefix = 1; prefix_base = (I.sregs[2] << 4); next = fetchop(); CLK(2); break;
                case 0x3e: seg_prefix = 1; prefix_base = (I.sregs[3] << 4); next = fetchop(); CLK(2); break;
            }
            switch (next)
            {
                case 0x6c: CLK(2); if (c != 0) do { i_insb(); c--; } while (c > 0 && !CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); /*I.regs.w[1] = c;*/ break;
                case 0x6d: CLK(2); if (c != 0) do { i_insw(); c--; } while (c > 0 && !CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0x6e: CLK(2); if (c != 0) do { i_outsb(); c--; } while (c > 0 && !CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0x6f: CLK(2); if (c != 0) do { i_outsw(); c--; } while (c > 0 && !CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa4: CLK(2); if (c != 0) do { i_movsb(); c--; } while (c > 0 && !CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa5: CLK(2); if (c != 0) do { i_movsw(); c--; } while (c > 0 && !CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa6: CLK(2); if (c != 0) do { i_cmpsb(); c--; } while (c > 0 && !CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa7: CLK(2); if (c != 0) do { i_cmpsw(); c--; } while (c > 0 && !CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xaa: CLK(2); if (c != 0) do { i_stosb(); c--; } while (c > 0 && !CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xab: CLK(2); if (c != 0) do { i_stosw(); c--; } while (c > 0 && !CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xac: CLK(2); if (c != 0) do { i_lodsb(); c--; } while (c > 0 && !CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xad: CLK(2); if (c != 0) do { i_lodsw(); c--; } while (c > 0 && !CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xae: CLK(2); if (c != 0) do { i_scasb(); c--; } while (c > 0 && !CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xaf: CLK(2); if (c != 0) do { i_scasw(); c--; } while (c > 0 && !CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                default: nec_instruction[next](); break;
            }
            seg_prefix = 0;
        }
        void i_repc()
        {
            int next = fetchop();
            ushort c = (ushort)(I.regs.b[2] + I.regs.b[3] * 0x100);// I.regs.w[1];
            switch (next)
            { /* Segments */
                case 0x26: seg_prefix = 1; prefix_base = (I.sregs[0] << 4); next = fetchop(); CLK(2); break;
                case 0x2e: seg_prefix = 1; prefix_base = (I.sregs[1] << 4); next = fetchop(); CLK(2); break;
                case 0x36: seg_prefix = 1; prefix_base = (I.sregs[2] << 4); next = fetchop(); CLK(2); break;
                case 0x3e: seg_prefix = 1; prefix_base = (I.sregs[3] << 4); next = fetchop(); CLK(2); break;
            }
            switch (next)
            {
                case 0x6c: CLK(2); if (c != 0) do { i_insb(); c--; } while (c > 0 && CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100);/*I.regs.w[1] = c;*/ break;
                case 0x6d: CLK(2); if (c != 0) do { i_insw(); c--; } while (c > 0 && CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0x6e: CLK(2); if (c != 0) do { i_outsb(); c--; } while (c > 0 && CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0x6f: CLK(2); if (c != 0) do { i_outsw(); c--; } while (c > 0 && CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa4: CLK(2); if (c != 0) do { i_movsb(); c--; } while (c > 0 && CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa5: CLK(2); if (c != 0) do { i_movsw(); c--; } while (c > 0 && CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa6: CLK(2); if (c != 0) do { i_cmpsb(); c--; } while (c > 0 && CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa7: CLK(2); if (c != 0) do { i_cmpsw(); c--; } while (c > 0 && CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xaa: CLK(2); if (c != 0) do { i_stosb(); c--; } while (c > 0 && CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xab: CLK(2); if (c != 0) do { i_stosw(); c--; } while (c > 0 && CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xac: CLK(2); if (c != 0) do { i_lodsb(); c--; } while (c > 0 && CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xad: CLK(2); if (c != 0) do { i_lodsw(); c--; } while (c > 0 && CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xae: CLK(2); if (c != 0) do { i_scasb(); c--; } while (c > 0 && CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xaf: CLK(2); if (c != 0) do { i_scasw(); c--; } while (c > 0 && CF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                default: nec_instruction[next](); break;
            }
            seg_prefix = 0;
        }
        void i_push_d16()
        {
            int tmp;
            tmp = FETCHWORD();
            PUSH((ushort)tmp);
            //CLKW(12, 12, 5, 12, 8, 5, I.regs.w[4]);
            CLKW(12, 12, 5, 12, 8, 5, I.regs.b[8] + I.regs.b[9] * 0x100);
        }
        void i_imul_d16()
        {
            int tmp;
            int ModRM;
            ushort src, dst;
            DEF_r16w(out ModRM, out src, out dst);
            tmp = FETCHWORD();
            dst = (ushort)((int)((short)src) * (int)((short)tmp));
            I.CarryVal = I.OverVal = (uint)(((((int)dst) >> 15 != 0) && (((int)dst) >> 15 != -1)) ? 1 : 0);
            //I.regs.w[mod_RM.regw[ModRM]] = (ushort)dst;
            I.regs.b[mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
            I.regs.b[mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
            pendingCycles -= (ModRM >= 0xc0) ? 38 : 47;
        }
        void i_push_d8()
        {
            int tmp = (ushort)((short)((sbyte)FETCH()));
            PUSH((ushort)tmp);
            //CLKW(11, 11, 5, 11, 7, 3, I.regs.w[4]);
            CLKW(11, 11, 5, 11, 7, 3, I.regs.b[8] + I.regs.b[9] * 0x100);
        }
        void i_imul_d8()
        {
            int src2;
            int ModRM;
            ushort src, dst;
            DEF_r16w(out ModRM, out src, out dst);
            src2 = (ushort)((short)((sbyte)FETCH()));
            dst = (ushort)((int)((short)src) * (int)((short)src2));
            I.CarryVal = I.OverVal = (uint)(((((int)dst) >> 15 != 0) && (((int)dst) >> 15 != -1)) ? 1 : 0);
            //I.regs.w[mod_RM.regw[ModRM]] = (ushort)dst;
            I.regs.b[mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
            I.regs.b[mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
            pendingCycles -= (ModRM >= 0xc0) ? 31 : 39;
        }
        void i_insb()
        {
            //PutMemB(0, I.regs.w[7], ReadIOByte(I.regs.w[2]));
            PutMemB(0, I.regs.b[14] + I.regs.b[15] * 0x100, ReadIOByte(I.regs.b[4] + I.regs.b[5] * 0x100));
            //I.regs.w[7] += (ushort)(-2 * (I.DF ? 1 : 0) + 1);
            ushort w7 = (ushort)(I.regs.b[14] + I.regs.b[15] * 0x100);
            w7 += (ushort)(-2 * (I.DF ? 1 : 0) + 1);
            I.regs.b[14] = (byte)(w7 % 0x100);
            I.regs.b[15] = (byte)(w7 / 0x100);
            CLK(8);
        }
        void i_insw()
        {
            //PutMemW(0, I.regs.w[7], ReadIOWord(I.regs.w[2]));
            PutMemW(0, I.regs.b[14] + I.regs.b[15] * 0x100, ReadIOWord(I.regs.b[4] + I.regs.b[5] * 0x100));
            //I.regs.w[7] += (ushort)(-4 * (I.DF ? 1 : 0) + 2);
            ushort w7 = (ushort)(I.regs.b[14] + I.regs.b[15] * 0x100);
            w7 += (ushort)(-4 * (I.DF ? 1 : 0) + 2);
            I.regs.b[14] = (byte)(w7 % 0x100);
            I.regs.b[15] = (byte)(w7 / 0x100);
            CLKS(18, 10, 8);
        }
        void i_outsb()
        {
            //WriteIOByte(I.regs.w[2], GetMemB(3, I.regs.w[6]));
            WriteIOByte(I.regs.b[4] + I.regs.b[5] * 0x100, GetMemB(3, I.regs.b[12] + I.regs.b[13] * 0x100));
            //I.regs.w[6] += (ushort)(-2 * (I.DF ? 1 : 0) + 1);
            ushort w6 = (ushort)(I.regs.b[12] + I.regs.b[13] * 0x100);
            w6 += (ushort)(-2 * (I.DF ? 1 : 0) + 1);
            I.regs.b[12] = (byte)(w6 % 0x100);
            I.regs.b[13] = (byte)(w6 / 0x100);
            CLK(8);
        }
        void i_outsw()
        {
            //WriteIOWord(I.regs.w[2], GetMemW(3, I.regs.w[6]));
            WriteIOWord(I.regs.b[4] + I.regs.b[5] * 0x100, GetMemW(3, I.regs.b[12] + I.regs.b[13] * 0x100));
            //I.regs.w[6] += (ushort)(-4 * (I.DF ? 1 : 0) + 2);
            ushort w6 = (ushort)(I.regs.b[12] + I.regs.b[13] * 0x100);
            w6 += (ushort)(-4 * (I.DF ? 1 : 0) + 2);
            I.regs.b[12] = (byte)(w6 % 0x100);
            I.regs.b[13] = (byte)(w6 / 0x100);
            CLKS(18, 10, 8);
        }
        void i_jo()
        {
            bool b1 = OF();
            JMP(b1);
            if (!b1)
            {
                CLKS(4, 4, 3);
            }
        }
        void i_jno()
        {
            bool b1 = !OF();
            JMP(b1);
            if (!b1)
            {
                CLKS(4, 4, 3);
            }
        }
        void i_jc()
        {
            bool b1 = CF();
            JMP(b1);
            if (!b1)
            {
                CLKS(4, 4, 3);
            }
        }
        void i_jnc()
        {
            bool b1 = !CF();
            JMP(b1);
            if (!b1)
            {
                CLKS(4, 4, 3);
            }
        }
        void i_jz()
        {
            bool b1 = ZF();
            JMP(b1);
            if (!b1)
            {
                CLKS(4, 4, 3);
            }
        }
        void i_jnz()
        {
            bool b1 = !ZF();
            JMP(b1);
            if (!b1)
            {
                CLKS(4, 4, 3);
            }
        }
        void i_jce()
        {
            bool b1 = CF() || ZF();
            JMP(b1);
            if (!b1)
            {
                CLKS(4, 4, 3);
            }
        }
        void i_jnce()
        {
            bool b1 = !(CF() || ZF());
            JMP(b1);
            if (!b1)
            {
                CLKS(4, 4, 3);
            }
        }
        void i_js()
        {
            bool b1 = SF();
            JMP(b1);
            if (!b1)
            {
                CLKS(4, 4, 3);
            }
        }
        void i_jns()
        {
            bool b1 = !SF();
            JMP(b1);
            if (!b1)
            {
                CLKS(4, 4, 3);
            }
        }
        void i_jp()
        {
            bool b1 = PF();
            JMP(b1);
            if (!b1)
            {
                CLKS(4, 4, 3);
            }
        }
        void i_jnp()
        {
            bool b1 = !PF();
            JMP(b1);
            if (!b1)
            {
                CLKS(4, 4, 3);
            }
        }
        void i_jl()
        {
            bool b1 = (SF() != OF()) && (!ZF());
            JMP(b1);
            if (!b1)
            {
                CLKS(4, 4, 3);
            }
        }
        void i_jnl()
        {
            bool b1 = (ZF()) || (SF() == OF());
            JMP(b1);
            if (!b1)
            {
                CLKS(4, 4, 3);
            }
        }
        void i_jle()
        {
            bool b1 = (ZF()) || (SF() != OF());
            JMP(b1);
            if (!b1)
            {
                CLKS(4, 4, 3);
            }
        }
        void i_jnle()
        {
            bool b1 = (SF() == OF()) && (!ZF());
            JMP(b1);
            if (!b1)
            {
                CLKS(4, 4, 3);
            }
        }
        void i_80pre()
        {
            int ModRM;
            byte src, dst;
            ModRM = GetModRM();
            dst = GetRMByte(ModRM);
            src = FETCH();
            if (ModRM >= 0xc0)
            {
                CLKS(4, 4, 2);
            }
            else if ((ModRM & 0x38) == 0x38)
            {
                CLKS(13, 13, 6);
            }
            else
            {
                CLKS(18, 18, 7);
            }
            switch (ModRM & 0x38)
            {
                case 0x00: ADDB(ref src, ref dst); PutbackRMByte(ModRM, dst); break;
                case 0x08: ORB(ref src, ref dst); PutbackRMByte(ModRM, dst); break;
                case 0x10: src += (byte)(CF() ? 1 : 0); ADDB(ref src, ref dst); PutbackRMByte(ModRM, dst); break;
                case 0x18: src += (byte)(CF() ? 1 : 0); SUBB(ref src, ref dst); PutbackRMByte(ModRM, dst); break;
                case 0x20: ANDB(ref src, ref dst); PutbackRMByte(ModRM, dst); break;
                case 0x28: SUBB(ref src, ref dst); PutbackRMByte(ModRM, dst); break;
                case 0x30: XORB(ref src, ref dst); PutbackRMByte(ModRM, dst); break;
                case 0x38: SUBB(ref src, ref dst); break;
            }
        }
        void i_81pre()
        {
            int ModRM;
            ushort src, dst;
            ModRM = GetModRM();
            dst = GetRMWord(ModRM);
            src = FETCH();
            src += (ushort)(FETCH() << 8);
            if (ModRM >= 0xc0)
            {
                CLKS(4, 4, 2);
            }
            else if ((ModRM & 0x38) == 0x38)
            {
                CLKW(17, 17, 8, 17, 13, 6, EA);
            }
            else
            {
                CLKW(26, 26, 11, 26, 18, 7, EA);
            }
            switch (ModRM & 0x38)
            {
                case 0x00: ADDW(ref src, ref dst); PutbackRMWord(ModRM, dst); break;
                case 0x08: ORW(ref src, ref dst); PutbackRMWord(ModRM, dst); break;
                case 0x10: src += (ushort)(CF() ? 1 : 0); ADDW(ref src, ref dst); PutbackRMWord(ModRM, dst); break;
                case 0x18: src += (ushort)(CF() ? 1 : 0); SUBW(ref src, ref dst); PutbackRMWord(ModRM, dst); break;
                case 0x20: ANDW(ref src, ref dst); PutbackRMWord(ModRM, dst); break;
                case 0x28: SUBW(ref src, ref dst); PutbackRMWord(ModRM, dst); break;
                case 0x30: XORW(ref src, ref dst); PutbackRMWord(ModRM, dst); break;
                case 0x38: SUBW(ref src, ref dst); break;
            }
        }
        void i_82pre()
        {
            int ModRM;
            byte src, dst;
            ModRM = GetModRM();
            dst = GetRMByte(ModRM);
            src = (byte)((sbyte)FETCH());
            if (ModRM >= 0xc0)
            {
                CLKS(4, 4, 2);
            }
            else if ((ModRM & 0x38) == 0x38)
            {
                CLKS(13, 13, 6);
            }
            else
            {
                CLKS(18, 18, 7);
            }
            switch (ModRM & 0x38)
            {
                case 0x00: ADDB(ref src, ref dst); PutbackRMByte(ModRM, dst); break;
                case 0x08: ORB(ref src, ref dst); PutbackRMByte(ModRM, dst); break;
                case 0x10: src += (byte)(CF() ? 1 : 0); ADDB(ref src, ref dst); PutbackRMByte(ModRM, dst); break;
                case 0x18: src += (byte)(CF() ? 1 : 0); SUBB(ref src, ref dst); PutbackRMByte(ModRM, dst); break;
                case 0x20: ANDB(ref src, ref dst); PutbackRMByte(ModRM, dst); break;
                case 0x28: SUBB(ref src, ref dst); PutbackRMByte(ModRM, dst); break;
                case 0x30: XORB(ref src, ref dst); PutbackRMByte(ModRM, dst); break;
                case 0x38: SUBB(ref src, ref dst); break;
            }
        }
        void i_83pre()
        {
            int ModRM;
            ushort src, dst;
            ModRM = GetModRM();
            dst = GetRMWord(ModRM);
            src = (ushort)((short)((sbyte)FETCH()));
            if (ModRM >= 0xc0)
            {
                CLKS(4, 4, 2);
            }
            else if ((ModRM & 0x38) == 0x38)
            {
                CLKW(17, 17, 8, 17, 13, 6, EA);
            }
            else
            {
                CLKW(26, 26, 11, 26, 18, 7, EA);
            }
            switch (ModRM & 0x38)
            {
                case 0x00: ADDW(ref src, ref dst); PutbackRMWord(ModRM, dst); break;
                case 0x08: ORW(ref src, ref dst); PutbackRMWord(ModRM, dst); break;
                case 0x10: src += (ushort)(CF() ? 1 : 0); ADDW(ref src, ref dst); PutbackRMWord(ModRM, dst); break;
                case 0x18: src += (ushort)(CF() ? 1 : 0); SUBW(ref src, ref dst); PutbackRMWord(ModRM, dst); break;
                case 0x20: ANDW(ref src, ref dst); PutbackRMWord(ModRM, dst); break;
                case 0x28: SUBW(ref src, ref dst); PutbackRMWord(ModRM, dst); break;
                case 0x30: XORW(ref src, ref dst); PutbackRMWord(ModRM, dst); break;
                case 0x38: SUBW(ref src, ref dst); break;
            }
        }
        void i_test_br8()
        {
            int ModRM;
            byte src, dst;
            DEF_br8(out ModRM, out src, out dst);
            ANDB(ref src, ref dst);
            CLKM(ModRM, 2, 2, 2, 10, 10, 6);
        }
        void i_test_wr16()
        {
            int ModRM;
            ushort src, dst;
            DEF_wr16(out ModRM, out src, out dst);
            ANDW(ref src, ref dst);
            CLKR(ModRM, 14, 14, 8, 14, 10, 6, 2, EA);
        }
        void i_xchg_br8()
        {
            int ModRM;
            byte src, dst;
            DEF_br8(out ModRM, out src, out dst);
            I.regs.b[mod_RM.regb[ModRM]] = dst;
            PutbackRMByte(ModRM, src);
            CLKM(ModRM, 3, 3, 3, 16, 18, 8);
        }
        void i_xchg_wr16()
        {
            int ModRM;
            ushort src, dst;
            DEF_wr16(out ModRM, out src, out dst);
            //I.regs.w[mod_RM.regw[ModRM]] = dst;
            I.regs.b[mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
            I.regs.b[mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
            PutbackRMWord(ModRM, src);
            CLKR(ModRM, 24, 24, 12, 24, 16, 8, 3, EA);
        }
        void i_mov_br8()
        {
            int ModRM;
            byte src;
            ModRM = GetModRM();
            src = I.regs.b[mod_RM.regb[ModRM]];
            PutRMByte(ModRM, src);
            CLKM(ModRM, 2, 2, 2, 9, 9, 3);
        }
        void i_mov_wr16()
        {
            int ModRM;
            ushort src;
            ModRM = GetModRM();
            //src = I.regs.w[mod_RM.regw[ModRM]];
            src = (ushort)(I.regs.b[mod_RM.regw[ModRM] * 2] + I.regs.b[mod_RM.regw[ModRM] * 2 + 1] * 0x100);
            PutRMWord(ModRM, src);
            CLKR(ModRM, 13, 13, 5, 13, 9, 3, 2, EA);
        }
        void i_mov_r8b()
        {
            int ModRM;
            byte src;
            ModRM = GetModRM();
            src = GetRMByte(ModRM);
            I.regs.b[mod_RM.regb[ModRM]] = src;
            CLKM(ModRM, 2, 2, 2, 11, 11, 5);
        }
        void i_mov_r16w()
        {
            int ModRM;
            ushort src;
            ModRM = GetModRM();
            src = GetRMWord(ModRM);
            //I.regs.w[mod_RM.regw[ModRM]] = src;
            I.regs.b[mod_RM.regw[ModRM] * 2] = (byte)(src % 0x100);
            I.regs.b[mod_RM.regw[ModRM] * 2 + 1] = (byte)(src / 0x100);
            CLKR(ModRM, 15, 15, 7, 15, 11, 5, 2, EA);
        }
        void i_mov_wsreg()
        {
            int ModRM;
            ModRM = GetModRM();
            PutRMWord(ModRM, I.sregs[(ModRM & 0x38) >> 3]);
            CLKR(ModRM, 14, 14, 5, 14, 10, 3, 2, EA);
        }
        void i_lea()
        {
            int ModRM = FETCH();
            GetEA[ModRM]();
            //I.regs.w[mod_RM.regw[ModRM]] = EO;
            I.regs.b[mod_RM.regw[ModRM] * 2] = (byte)(EO % 0x100);
            I.regs.b[mod_RM.regw[ModRM] * 2 + 1] = (byte)(EO / 0x100);
            CLKS(4, 4, 2);
        }
        void i_mov_sregw()
        {
            int ModRM;
            ushort src;
            ModRM = GetModRM();
            src = GetRMWord(ModRM);
            CLKR(ModRM, 15, 15, 7, 15, 11, 5, 2, EA);
            switch (ModRM & 0x38)
            {
                case 0x00: I.sregs[0] = src; break; /* mov es,ew */
                case 0x08: I.sregs[1] = src; break; /* mov cs,ew */
                case 0x10: I.sregs[2] = src; break; /* mov ss,ew */
                case 0x18: I.sregs[3] = src; break; /* mov ds,ew */
                default: break;
            }
            I.no_interrupt = 1;
        }
        void i_popw()
        {
            int ModRM;
            ushort tmp = 0;
            ModRM = GetModRM();
            POP(ref tmp);
            PutRMWord(ModRM, tmp);
            pendingCycles -= 21;
        }
        void i_nop()
        {
            CLK(3);
            if (I.no_interrupt == 0 && pendingCycles > 0 && (I.pending_irq == 0) && (PEEKOP((uint)((I.sregs[1] << 4) + I.ip))) == 0xeb && (PEEK((uint)((I.sregs[1] << 4) + I.ip + 1))) == 0xfd)
                pendingCycles %= 15;
        }
        void i_xchg_axcx()
        {
            XchgAWReg(1);
            CLK(3);
        }
        void i_xchg_axdx()
        {
            XchgAWReg(2);
            CLK(3);
        }
        void i_xchg_axbx()
        {
            XchgAWReg(3);
            CLK(3);
        }
        void i_xchg_axsp()
        {
            XchgAWReg(4);
            CLK(3);
        }
        void i_xchg_axbp()
        {
            XchgAWReg(5);
            CLK(3);
        }
        void i_xchg_axsi()
        {
            XchgAWReg(6);
            CLK(3);
        }
        void i_xchg_axdi()
        {
            XchgAWReg(7);
            CLK(3);
        }
        void i_cbw()
        {
            I.regs.b[1] = (byte)(((I.regs.b[0] & 0x80) != 0) ? 0xff : 0);
            CLK(2);
        }
        void i_cwd()
        {
            //I.regs.w[2] = (ushort)(((I.regs.b[1] & 0x80) != 0) ? 0xffff : 0);
            ushort w2 = (ushort)(((I.regs.b[1] & 0x80) != 0) ? 0xffff : 0);
            I.regs.b[4] = (byte)(w2 % 0x100);
            I.regs.b[5] = (byte)(w2 / 0x100);
            CLK(4);
        }
        void i_call_far()
        {
            ushort tmp, tmp2;
            tmp = FETCHWORD();
            tmp2 = FETCHWORD();
            PUSH(I.sregs[1]);
            PUSH(I.ip);
            I.ip = (ushort)tmp;
            I.sregs[1] = (ushort)tmp2;
            //CHANGE_PC;
            CLKW(29, 29, 13, 29, 21, 9, I.regs.b[8] + I.regs.b[9] * 0x100);
        }
        void i_wait()
        {
            if (!I.poll_state)
            {
                I.ip--;
            }
            CLK(5);
        }
        void i_pushf()
        {
            ushort tmp = CompressFlags();
            PUSH(tmp);
            CLKS(12, 8, 3);
        }
        void i_popf()
        {
            ushort tmp = 0;
            POP(ref tmp);
            ExpandFlags(tmp);
            CLKS(12, 8, 5);
            if (I.TF)
            {
                nec_trap();
            }
        }
        void i_sahf()
        {
            ushort tmp = (ushort)((CompressFlags() & 0xff00) | (I.regs.b[1] & 0xd5));
            ExpandFlags(tmp);
            CLKS(3, 3, 2);
        }
        void i_lahf()
        {
            I.regs.b[1] = (byte)(CompressFlags() & 0xff);
            CLKS(3, 3, 2);
        }
        void i_mov_aldisp()
        {
            ushort addr;
            addr = FETCHWORD();
            I.regs.b[0] = GetMemB(3, addr);
            CLKS(10, 10, 5);
        }
        void i_mov_axdisp()
        {
            ushort addr;
            addr = FETCHWORD();
            //I.regs.w[0] = GetMemW(3, addr);
            ushort w0 = GetMemW(3, addr);
            I.regs.b[0] = (byte)(w0 % 0x100);
            I.regs.b[1] = (byte)(w0 / 0x100);
            CLKW(14, 14, 7, 14, 10, 5, addr);
        }
        void i_mov_dispal()
        {
            ushort addr;
            addr = FETCHWORD();
            PutMemB(3, addr, I.regs.b[0]);
            CLKS(9, 9, 3);
        }
        void i_mov_dispax()
        {
            ushort addr;
            addr = FETCHWORD();
            PutMemW(3, addr, (ushort)(I.regs.b[0] + I.regs.b[1] * 0x100));
            CLKW(13, 13, 5, 13, 9, 3, addr);
        }
        void i_movsb()
        {
            byte tmp = GetMemB(3, I.regs.b[12] + I.regs.b[13] * 0x100);
            PutMemB(0, I.regs.b[14] + I.regs.b[15] * 0x100, tmp);
            //I.regs.w[7] += (ushort)(-2 * (I.DF ? 1 : 0) + 1);
            //I.regs.w[6] += (ushort)(-2 * (I.DF ? 1 : 0) + 1);
            ushort w7 = (ushort)(I.regs.b[14] + I.regs.b[15] * 0x100 + (-2 * (I.DF ? 1 : 0) + 1));
            I.regs.b[14] = (byte)(w7 % 0x100);
            I.regs.b[15] = (byte)(w7 / 0x100);
            ushort w6 = (ushort)(I.regs.b[12] + I.regs.b[13] * 0x100 + (-2 * (I.DF ? 1 : 0) + 1));
            I.regs.b[12] = (byte)(w6 % 0x100);
            I.regs.b[13] = (byte)(w6 / 0x100);
            CLKS(8, 8, 6);
        }
        void i_movsw()
        {
            ushort tmp = GetMemW(3, I.regs.b[12] + I.regs.b[13] * 0x100);
            PutMemW(0, I.regs.b[14] + I.regs.b[15] * 0x100, tmp);
            //I.regs.w[7] += (ushort)(-4 * (I.DF ? 1 : 0) + 2);
            //I.regs.w[6] += (ushort)(-4 * (I.DF ? 1 : 0) + 2);
            ushort w7 = (ushort)(I.regs.b[14] + I.regs.b[15] * 0x100 + (-4 * (I.DF ? 1 : 0) + 2));
            I.regs.b[14] = (byte)(w7 % 0x100);
            I.regs.b[15] = (byte)(w7 / 0x100);
            ushort w6 = (ushort)(I.regs.b[12] + I.regs.b[13] * 0x100 + (-4 * (I.DF ? 1 : 0) + 2));
            I.regs.b[12] = (byte)(w6 % 0x100);
            I.regs.b[13] = (byte)(w6 / 0x100);
            CLKS(16, 16, 10);
        }
        void i_cmpsb()
        {
            byte src = GetMemB(0, I.regs.b[14] + I.regs.b[15] * 0x100);
            byte dst = GetMemB(3, I.regs.b[12] + I.regs.b[13] * 0x100);
            SUBB(ref src, ref dst);
            //I.regs.w[7] += (ushort)(-2 * (I.DF ? 1 : 0) + 1);
            //I.regs.w[6] += (ushort)(-2 * (I.DF ? 1 : 0) + 1);
            ushort w7 = (ushort)(I.regs.b[14] + I.regs.b[15] * 0x100 + (-2 * (I.DF ? 1 : 0) + 1));
            I.regs.b[14] = (byte)(w7 % 0x100);
            I.regs.b[15] = (byte)(w7 / 0x100);
            ushort w6 = (ushort)(I.regs.b[12] + I.regs.b[13] * 0x100 + (-2 * (I.DF ? 1 : 0) + 1));
            I.regs.b[12] = (byte)(w6 % 0x100);
            I.regs.b[13] = (byte)(w6 / 0x100);
            CLKS(14, 14, 14);
        }
        void i_cmpsw()
        {
            ushort src = GetMemW(0, I.regs.b[14] + I.regs.b[15] * 0x100);
            ushort dst = GetMemW(3, I.regs.b[12] + I.regs.b[13] * 0x100);
            SUBW(ref src, ref dst);
            //I.regs.w[7] += (ushort)(-4 * (I.DF ? 1 : 0) + 2);
            //I.regs.w[6] += (ushort)(-4 * (I.DF ? 1 : 0) + 2);
            ushort w7 = (ushort)(I.regs.b[14] + I.regs.b[15] * 0x100 + (-4 * (I.DF ? 1 : 0) + 2));
            I.regs.b[14] = (byte)(w7 % 0x100);
            I.regs.b[15] = (byte)(w7 / 0x100);
            ushort w6 = (ushort)(I.regs.b[12] + I.regs.b[13] * 0x100 + (-4 * (I.DF ? 1 : 0) + 2));
            I.regs.b[12] = (byte)(w6 % 0x100);
            I.regs.b[13] = (byte)(w6 / 0x100);
            CLKS(14, 14, 14);
        }
        void i_test_ald8()
        {
            byte src, dst;
            DEF_ald8(out src, out dst);
            ANDB(ref src, ref dst);
            CLKS(4, 4, 2);
        }
        void i_test_axd16()
        {
            ushort src, dst;
            DEF_axd16(out src, out dst);
            ANDW(ref src, ref dst);
            CLKS(4, 4, 2);
        }
        void i_stosb()
        {
            PutMemB(0, I.regs.b[14] + I.regs.b[15] * 0x100, I.regs.b[0]);
            //I.regs.w[7] += (ushort)(-2 * (I.DF ? 1 : 0) + 1);
            ushort w7 = (ushort)(I.regs.b[14] + I.regs.b[15] * 0x100 + (-2 * (I.DF ? 1 : 0) + 1));
            I.regs.b[14] = (byte)(w7 % 0x100);
            I.regs.b[15] = (byte)(w7 / 0x100);
            CLKS(4, 4, 3);
        }
        void i_stosw()
        {
            PutMemW(0, I.regs.b[14] + I.regs.b[15] * 0x100, (ushort)(I.regs.b[0] + I.regs.b[1] * 0x100));
            //I.regs.w[7] += (ushort)(-4 * (I.DF ? 1 : 0) + 2);
            ushort w7 = (ushort)(I.regs.b[14] + I.regs.b[15] * 0x100 + (-4 * (I.DF ? 1 : 0) + 2));
            I.regs.b[14] = (byte)(w7 % 0x100);
            I.regs.b[15] = (byte)(w7 / 0x100);
            CLKW(8, 8, 5, 8, 4, 3, I.regs.b[14] + I.regs.b[15] * 0x100);
        }
        void i_lodsb()
        {
            I.regs.b[0] = GetMemB(3, I.regs.b[12] + I.regs.b[13] * 0x100);
            //I.regs.w[6] += (ushort)(-2 * (I.DF ? 1 : 0) + 1);
            ushort w6 = (ushort)(I.regs.b[12] + I.regs.b[13] * 0x100 + (-2 * (I.DF ? 1 : 0) + 1));
            I.regs.b[12] = (byte)(w6 % 0x100);
            I.regs.b[13] = (byte)(w6 / 0x100);
            CLKS(4, 4, 3);
        }
        void i_lodsw()
        {
            ushort w0 = GetMemW(3, I.regs.b[12] + I.regs.b[13] * 0x100);
            I.regs.b[0] = (byte)(w0 % 0x100);
            I.regs.b[1] = (byte)(w0 / 0x100);
            //I.regs.w[0] = GetMemW(3, I.regs.b[12] + I.regs.b[13] * 0x100);
            //I.regs.w[6] += (ushort)(-4 * (I.DF ? 1 : 0) + 2);
            ushort w6 = (ushort)(I.regs.b[12] + I.regs.b[13] * 0x100 + (-4 * (I.DF ? 1 : 0) + 2));
            I.regs.b[12] = (byte)(w6 % 0x100);
            I.regs.b[13] = (byte)(w6 / 0x100);
            CLKW(8, 8, 5, 8, 4, 3, I.regs.b[12] + I.regs.b[13] * 0x100);
        }
        void i_scasb()
        {
            byte src = GetMemB(0, I.regs.b[14] + I.regs.b[15] * 0x100);
            byte dst = I.regs.b[0];
            SUBB(ref src, ref dst);
            //I.regs.w[7] += (ushort)(-2 * (I.DF ? 1 : 0) + 1);
            ushort w7 = (ushort)(I.regs.b[14] + I.regs.b[15] * 0x100 + (-2 * (I.DF ? 1 : 0) + 1));
            I.regs.b[14] = (byte)(w7 % 0x100);
            I.regs.b[15] = (byte)(w7 / 0x100);
            CLKS(4, 4, 3);
        }
        void i_scasw()
        {
            ushort src = GetMemW(0, I.regs.b[14] + I.regs.b[15] * 0x100);
            ushort dst = (ushort)(I.regs.b[0] + I.regs.b[1] * 0x100);
            SUBW(ref src, ref dst);
            //I.regs.w[7] += (ushort)(-4 * (I.DF ? 1 : 0) + 2);
            ushort w7 = (ushort)(I.regs.b[14] + I.regs.b[15] * 0x100 + (-4 * (I.DF ? 1 : 0) + 2));
            I.regs.b[14] = (byte)(w7 % 0x100);
            I.regs.b[15] = (byte)(w7 / 0x100);
            CLKW(8, 8, 5, 8, 4, 3, I.regs.b[14] + I.regs.b[15] * 0x100);
        }
        void i_mov_ald8()
        {
            I.regs.b[0] = FETCH();
            CLKS(4, 4, 2);
        }
        void i_mov_cld8()
        {
            I.regs.b[2] = FETCH();
            CLKS(4, 4, 2);
        }
        void i_mov_dld8()
        {
            I.regs.b[4] = FETCH();
            CLKS(4, 4, 2);
        }
        void i_mov_bld8()
        {
            I.regs.b[6] = FETCH();
            CLKS(4, 4, 2);
        }
        void i_mov_ahd8()
        {
            I.regs.b[1] = FETCH();
            CLKS(4, 4, 2);
        }
        void i_mov_chd8()
        {
            I.regs.b[3] = FETCH();
            CLKS(4, 4, 2);
        }
        void i_mov_dhd8()
        {
            I.regs.b[5] = FETCH();
            CLKS(4, 4, 2);
        }
        void i_mov_bhd8()
        {
            I.regs.b[7] = FETCH();
            CLKS(4, 4, 2);
        }
        void i_mov_axd16()
        {
            I.regs.b[0] = FETCH();
            I.regs.b[1] = FETCH();
            CLKS(4, 4, 2);
        }
        void i_mov_cxd16()
        {
            I.regs.b[2] = FETCH();
            I.regs.b[3] = FETCH();
            CLKS(4, 4, 2);
        }
        void i_mov_dxd16()
        {
            I.regs.b[4] = FETCH();
            I.regs.b[5] = FETCH();
            CLKS(4, 4, 2);
        }
        void i_mov_bxd16()
        {
            I.regs.b[6] = FETCH();
            I.regs.b[7] = FETCH();
            CLKS(4, 4, 2);
        }
        void i_mov_spd16()
        {
            I.regs.b[8] = FETCH();
            I.regs.b[9] = FETCH();
            CLKS(4, 4, 2);
        }
        void i_mov_bpd16()
        {
            I.regs.b[10] = FETCH();
            I.regs.b[11] = FETCH();
            CLKS(4, 4, 2);
        }
        void i_mov_sid16()
        {
            I.regs.b[12] = FETCH();
            I.regs.b[13] = FETCH();
            CLKS(4, 4, 2);
        }
        void i_mov_did16()
        {
            I.regs.b[14] = FETCH();
            I.regs.b[15] = FETCH();
            CLKS(4, 4, 2);
        }
        void i_rotshft_bd8()
        {
            int ModRM;
            int src, dst;
            byte c;
            ModRM = GetModRM();
            src = GetRMByte(ModRM);
            dst = src;
            c = FETCH();
            CLKM(ModRM, 7, 7, 2, 19, 19, 6);
            if (c != 0)
            {
                switch (ModRM & 0x38)
                {
                    case 0x00: do { ROL_BYTE(ref dst); c--; CLK(1); } while (c > 0); PutbackRMByte(ModRM, (byte)dst); break;
                    case 0x08: do { ROR_BYTE(ref dst); c--; CLK(1); } while (c > 0); PutbackRMByte(ModRM, (byte)dst); break;
                    case 0x10: do { ROLC_BYTE(ref dst); c--; CLK(1); } while (c > 0); PutbackRMByte(ModRM, (byte)dst); break;
                    case 0x18: do { RORC_BYTE(ref dst); c--; CLK(1); } while (c > 0); PutbackRMByte(ModRM, (byte)dst); break;
                    case 0x20: SHL_BYTE(c, ref dst, ModRM); break;
                    case 0x28: SHR_BYTE(c, ref dst, ModRM); break;
                    case 0x30: break;
                    case 0x38: SHRA_BYTE(c, ref dst, ModRM); break;
                }
            }
        }
        void i_rotshft_wd8()
        {
            int ModRM;
            int src, dst;
            byte c;
            ModRM = GetModRM();
            src = GetRMWord(ModRM);
            dst = src;
            c = FETCH();
            CLKM(ModRM, 7, 7, 2, 27, 19, 6);
            if (c != 0)
            {
                switch (ModRM & 0x38)
                {
                    case 0x00: do { ROL_WORD(ref dst); c--; CLK(1); } while (c > 0); PutbackRMWord(ModRM, (ushort)dst); break;
                    case 0x08: do { ROR_WORD(ref dst); c--; CLK(1); } while (c > 0); PutbackRMWord(ModRM, (ushort)dst); break;
                    case 0x10: do { ROLC_WORD(ref dst); c--; CLK(1); } while (c > 0); PutbackRMWord(ModRM, (ushort)dst); break;
                    case 0x18: do { RORC_WORD(ref dst); c--; CLK(1); } while (c > 0); PutbackRMWord(ModRM, (ushort)dst); break;
                    case 0x20: SHL_WORD(c, ref dst, ModRM); break;
                    case 0x28: SHR_WORD(c, ref dst, ModRM); break;
                    case 0x30: break;
                    case 0x38: SHRA_WORD(c, ref dst, ModRM); break;
                }
            }
        }
        void i_ret_d16()
        {
            ushort count = FETCH();
            count += (ushort)(FETCH() << 8);
            POP(ref I.ip);
            //I.regs.w[4] += count;
            ushort w4 = (ushort)(I.regs.b[8] + I.regs.b[9] * 0x100 + count);
            I.regs.b[8] = (byte)(w4 % 0x100);
            I.regs.b[9] = (byte)(w4 / 0x100);
            //CHANGE_PC;
            CLKS(24, 24, 10);
        }
        void i_ret()
        {
            POP(ref I.ip);
            //CHANGE_PC;
            CLKS(19, 19, 10);
        }
        void i_les_dw()
        {
            int ModRM;
            ModRM = GetModRM();
            ushort tmp = GetRMWord(ModRM);
            //I.regs.w[mod_RM.regw[ModRM]] = tmp;
            I.regs.b[mod_RM.regw[ModRM] * 2] = (byte)(tmp % 0x100);
            I.regs.b[mod_RM.regw[ModRM] * 2 + 1] = (byte)(tmp / 0x100);
            I.sregs[0] = GetnextRMWord();
            CLKW(26, 26, 14, 26, 18, 10, EA);
        }
        void i_lds_dw()
        {
            int ModRM;
            ModRM = GetModRM();
            ushort tmp = GetRMWord(ModRM);
            //I.regs.w[mod_RM.regw[ModRM]] = tmp;
            I.regs.b[mod_RM.regw[ModRM] * 2] = (byte)(tmp % 0x100);
            I.regs.b[mod_RM.regw[ModRM] * 2 + 1] = (byte)(tmp / 0x100);
            I.sregs[3] = GetnextRMWord();
            CLKW(26, 26, 14, 26, 18, 10, EA);
        }
        void i_mov_bd8()
        {
            int ModRM;
            ModRM = GetModRM();
            PutImmRMByte(ModRM);
            pendingCycles -= (ModRM >= 0xc0) ? 4 : 11;
        }
        void i_mov_wd16()
        {
            int ModRM;
            ModRM = GetModRM();
            PutImmRMWord(ModRM);
            pendingCycles -= (ModRM >= 0xc0) ? 4 : 15;
        }
        void i_enter()
        {
            ushort nb = FETCH();
            int i, level;
            pendingCycles -= 23;
            nb += (ushort)(FETCH() << 8);
            level = FETCH();
            PUSH((ushort)(I.regs.b[10] + I.regs.b[11] * 0x100));
            //I.regs.w[5] = I.regs.w[4];
            I.regs.b[10] = I.regs.b[8];
            I.regs.b[11] = I.regs.b[9];
            //I.regs.w[4] -= nb;
            ushort w4 = (ushort)(I.regs.b[8] + I.regs.b[9] * 0x100 - nb);
            I.regs.b[8] = (byte)(w4 % 0x100);
            I.regs.b[9] = (byte)(w4 / 0x100);
            for (i = 1; i < level; i++)
            {
                PUSH(GetMemW(2, I.regs.b[10] + I.regs.b[11] * 0x100 - i * 2));
                pendingCycles -= 16;
            }
            if (level != 0)
            {
                PUSH((ushort)(I.regs.b[10] + I.regs.b[11] * 0x100));
            }
        }
        void i_leave()
        {
            //I.regs.w[4] = I.regs.w[5];
            I.regs.b[8] = I.regs.b[10];
            I.regs.b[9] = I.regs.b[11];
            //POP(ref I.regs.w[5]);
            POPW(5);
            pendingCycles -= 8;
        }
        void i_retf_d16()
        {
            ushort count = FETCH();
            count += (ushort)(FETCH() << 8);
            POP(ref I.ip);
            POP(ref I.sregs[1]);
            //I.regs.w[4] += count;
            ushort w4 = (ushort)(I.regs.b[8] + I.regs.b[9] * 0x100 + count);
            I.regs.b[8] = (byte)(w4 % 0x100);
            I.regs.b[9] = (byte)(w4 / 0x100);
            //CHANGE_PC;
            CLKS(32, 32, 16);
        }
        void i_retf()
        {
            POP(ref I.ip);
            POP(ref I.sregs[1]);
            //CHANGE_PC;
            CLKS(29, 29, 16);
        }
        void i_int3()
        {
            nec_interrupt(3, false);
            CLKS(50, 50, 24);
        }
        void i_int()
        {
            nec_interrupt(FETCH(), false);
            CLKS(50, 50, 24);
        }
        void i_into()
        {
            if (OF())
            {
                nec_interrupt(4, false);
                CLKS(52, 52, 26);
            }
            else
            {
                CLK(3);
            }
        }
        void i_iret()
        {
            POP(ref I.ip);
            POP(ref I.sregs[1]);
            i_popf();
            I.MF = true;
            //CHANGE_PC;
            CLKS(39, 39, 19);
        }
        void i_rotshft_b()
        {
            int ModRM;
            int src, dst;
            ModRM = GetModRM();
            src = GetRMByte(ModRM);
            dst = src;
            CLKM(ModRM, 6, 6, 2, 16, 16, 7);
            switch (ModRM & 0x38)
            {
                case 0x00: ROL_BYTE(ref dst); PutbackRMByte(ModRM, (byte)dst); I.OverVal = (uint)((src ^ dst) & 0x80); break;
                case 0x08: ROR_BYTE(ref dst); PutbackRMByte(ModRM, (byte)dst); I.OverVal = (uint)((src ^ dst) & 0x80); break;
                case 0x10: ROLC_BYTE(ref dst); PutbackRMByte(ModRM, (byte)dst); I.OverVal = (uint)((src ^ dst) & 0x80); break;
                case 0x18: RORC_BYTE(ref dst); PutbackRMByte(ModRM, (byte)dst); I.OverVal = (uint)((src ^ dst) & 0x80); break;
                case 0x20: SHL_BYTE(1, ref dst, ModRM); I.OverVal = (uint)((src ^ dst) & 0x80); break;
                case 0x28: SHR_BYTE(1, ref dst, ModRM); I.OverVal = (uint)((src ^ dst) & 0x80); break;
                case 0x30: break;
                case 0x38: SHRA_BYTE(1, ref dst, ModRM); I.OverVal = 0; break;
            }
        }
        void i_rotshft_w()
        {
            int ModRM;
            int src, dst;
            ModRM = GetModRM();
            src = GetRMWord(ModRM);
            dst = src;
            CLKM(ModRM, 6, 6, 2, 24, 16, 7);
            switch (ModRM & 0x38)
            {
                case 0x00: ROL_WORD(ref dst); PutbackRMWord(ModRM, (ushort)dst); I.OverVal = (uint)((src ^ dst) & 0x8000); break;
                case 0x08: ROR_WORD(ref dst); PutbackRMWord(ModRM, (ushort)dst); I.OverVal = (uint)((src ^ dst) & 0x8000); break;
                case 0x10: ROLC_WORD(ref dst); PutbackRMWord(ModRM, (ushort)dst); I.OverVal = (uint)((src ^ dst) & 0x8000); break;
                case 0x18: RORC_WORD(ref dst); PutbackRMWord(ModRM, (ushort)dst); I.OverVal = (uint)((src ^ dst) & 0x8000); break;
                case 0x20: SHL_WORD(1, ref dst, ModRM); I.OverVal = (uint)((src ^ dst) & 0x8000); break;
                case 0x28: SHR_WORD(1, ref dst, ModRM); I.OverVal = (uint)((src ^ dst) & 0x8000); break;
                case 0x30: break;
                case 0x38: SHRA_WORD(1, ref dst, ModRM); I.OverVal = 0; break;
            }
        }
        void i_rotshft_bcl()
        {
            int ModRM;
            int src, dst;
            byte c;
            ModRM = GetModRM();
            src = GetRMByte(ModRM);
            dst = src;
            c = I.regs.b[2];
            CLKM(ModRM, 7, 7, 2, 19, 19, 6);
            if (c != 0)
            {
                switch (ModRM & 0x38)
                {
                    case 0x00: do { ROL_BYTE(ref dst); c--; CLK(1); } while (c > 0); PutbackRMByte(ModRM, (byte)dst); break;
                    case 0x08: do { ROR_BYTE(ref dst); c--; CLK(1); } while (c > 0); PutbackRMByte(ModRM, (byte)dst); break;
                    case 0x10: do { ROLC_BYTE(ref dst); c--; CLK(1); } while (c > 0); PutbackRMByte(ModRM, (byte)dst); break;
                    case 0x18: do { RORC_BYTE(ref dst); c--; CLK(1); } while (c > 0); PutbackRMByte(ModRM, (byte)dst); break;
                    case 0x20: SHL_BYTE(c, ref dst, ModRM); break;
                    case 0x28: SHR_BYTE(c, ref dst, ModRM); break;
                    case 0x30: break;
                    case 0x38: SHRA_BYTE(c, ref dst, ModRM); break;
                }
            }
        }
        void i_rotshft_wcl()
        {
            int ModRM;
            int src, dst;
            byte c;
            ModRM = GetModRM();
            src = GetRMWord(ModRM);
            dst = src;
            c = I.regs.b[2];
            CLKM(ModRM, 7, 7, 2, 27, 19, 6);
            if (c != 0)
            {
                switch (ModRM & 0x38)
                {
                    case 0x00: do { ROL_WORD(ref dst); c--; CLK(1); } while (c > 0); PutbackRMWord(ModRM, (ushort)dst); break;
                    case 0x08: do { ROR_WORD(ref dst); c--; CLK(1); } while (c > 0); PutbackRMWord(ModRM, (ushort)dst); break;
                    case 0x10: do { ROLC_WORD(ref dst); c--; CLK(1); } while (c > 0); PutbackRMWord(ModRM, (ushort)dst); break;
                    case 0x18: do { RORC_WORD(ref dst); c--; CLK(1); } while (c > 0); PutbackRMWord(ModRM, (ushort)dst); break;
                    case 0x20: SHL_WORD(c, ref dst, ModRM); break;
                    case 0x28: SHR_WORD(c, ref dst, ModRM); break;
                    case 0x30: break;
                    case 0x38: SHRA_WORD(c, ref dst, ModRM); break;
                }
            }
        }
        void i_aam()
        {
            byte mult = FETCH();
            mult = 0;
            I.regs.b[1] = (byte)(I.regs.b[0] / 10);
            I.regs.b[0] %= 10;
            SetSZPF_Word(I.regs.b[0] + I.regs.b[1] * 0x100);
            CLKS(15, 15, 12);
        }
        void i_aad()
        {
            byte mult = FETCH();
            mult = 0;
            I.regs.b[0] = (byte)(I.regs.b[1] * 10 + I.regs.b[0]);
            I.regs.b[1] = 0;
            SetSZPF_Byte(I.regs.b[0]);
            CLKS(7, 7, 8);
        }
        void i_setalc()
        {
            I.regs.b[0] = (byte)(CF() ? 0xff : 0x00);
            pendingCycles -= 3;
        }
        void i_trans()
        {
            int dest = (I.regs.b[6] + I.regs.b[7] * 0x100 + I.regs.b[0]) & 0xffff;
            I.regs.b[0] = GetMemB(3, dest);
            CLKS(9, 9, 5);
        }
        void i_fpo()
        {
            int ModRM;
            ModRM = GetModRM();
            pendingCycles -= 2;
        }
        void i_loopne()
        {
            sbyte disp = (sbyte)FETCH();
            //I.regs.w[1]--;
            ushort w1 = (ushort)(I.regs.b[2] + I.regs.b[3] * 0x100 - 1);
            I.regs.b[2] = (byte)(w1 % 0x100);
            I.regs.b[3] = (byte)(w1 / 0x100);
            if (!ZF() && (I.regs.b[2] + I.regs.b[3] * 0x100 != 0))
            {
                I.ip = (ushort)(I.ip + disp);
                CLKS(14, 14, 6);
            }
            else
            {
                CLKS(5, 5, 3);
            }
        }
        void i_loope()
        {
            sbyte disp = (sbyte)FETCH();
            //I.regs.w[1]--;
            ushort w1 = (ushort)(I.regs.b[2] + I.regs.b[3] * 0x100 - 1);
            I.regs.b[2] = (byte)(w1 % 0x100);
            I.regs.b[3] = (byte)(w1 / 0x100);
            if (ZF() && (I.regs.b[2] + I.regs.b[3] * 0x100 != 0))
            {
                I.ip = (ushort)(I.ip + disp);
                CLKS(14, 14, 6);
            }
            else
            {
                CLKS(5, 5, 3);
            }
        }
        void i_loop()
        {
            sbyte disp = (sbyte)FETCH();
            //I.regs.w[1]--;
            ushort w1 = (ushort)(I.regs.b[2] + I.regs.b[3] * 0x100 - 1);
            I.regs.b[2] = (byte)(w1 % 0x100);
            I.regs.b[3] = (byte)(w1 / 0x100);
            if (I.regs.b[2] + I.regs.b[3] * 0x100 != 0)
            {
                I.ip = (ushort)(I.ip + disp);
                CLKS(13, 13, 6);
            }
            else
            {
                CLKS(5, 5, 3);
            }
        }
        void i_jcxz()
        {
            sbyte disp = (sbyte)FETCH();
            if (I.regs.b[2] + I.regs.b[3] * 0x100 == 0)
            {
                I.ip = (ushort)(I.ip + disp);
                CLKS(13, 13, 6);
            }
            else
            {
                CLKS(5, 5, 3);
            }
        }
        void i_inal()
        {
            byte port = FETCH();
            I.regs.b[0] = ReadIOByte(port);
            CLKS(9, 9, 5);
        }
        void i_inax()
        {
            byte port = FETCH();
            //I.regs.w[0] = ReadIOWord(port);
            ushort w0 = ReadIOWord(port);
            I.regs.b[0] = (byte)(w0 % 0x100);
            I.regs.b[1] = (byte)(w0 / 0x100);
            CLKW(13, 13, 7, 13, 9, 5, port);
        }
        void i_outal()
        {
            byte port = FETCH();
            WriteIOByte(port, I.regs.b[0]);
            CLKS(8, 8, 3);
        }
        void i_outax()
        {
            byte port = FETCH();
            //WriteIOWord(port, I.regs.w[0]);
            WriteIOWord(port, (ushort)(I.regs.b[0] + I.regs.b[1] * 0x100));
            CLKW(12, 12, 5, 12, 8, 3, port);
        }
        void i_call_d16()
        {
            ushort tmp;
            tmp = FETCHWORD();
            PUSH(I.ip);
            I.ip = (ushort)(I.ip + (short)tmp);
            //CHANGE_PC;
            pendingCycles -= 24;
        }
        void i_jmp_d16()
        {
            ushort tmp;
            tmp = FETCHWORD();
            I.ip = (ushort)(I.ip + (short)tmp);
            //CHANGE_PC;
            pendingCycles -= 15;
        }
        void i_jmp_far()
        {
            ushort tmp, tmp1;
            tmp = FETCHWORD();
            tmp1 = FETCHWORD();
            I.sregs[1] = (ushort)tmp1;
            I.ip = (ushort)tmp;
            //CHANGE_PC;
            pendingCycles -= 27;
        }
        void i_jmp_d8()
        {
            int tmp = (int)((sbyte)FETCH());
            pendingCycles -= 12;
            if (tmp == -2 && I.no_interrupt == 0 && (I.pending_irq == 0) && pendingCycles > 0)
            {
                pendingCycles %= 12;
            }
            I.ip = (ushort)(I.ip + tmp);
        }
        void i_inaldx()
        {
            //I.regs.b[0] = ReadIOByte(I.regs.w[2]);
            I.regs.b[0] = ReadIOByte(I.regs.b[4] + I.regs.b[5] * 0x100);
            CLKS(8, 8, 5);
        }
        void i_inaxdx()
        {
            //I.regs.w[0] = ReadIOWord(I.regs.w[2]);
            ushort w0 = ReadIOWord(I.regs.b[4] + I.regs.b[5] * 0x100);
            I.regs.b[0] = (byte)(w0 % 0x100);
            I.regs.b[1] = (byte)(w0 / 0x100);
            CLKW(12, 12, 7, 12, 8, 5, I.regs.b[4] + I.regs.b[5] * 0x100);
        }
        void i_outdxal()
        {
            //WriteIOByte(I.regs.w[2], I.regs.b[0]);
            WriteIOByte(I.regs.b[4] + I.regs.b[5] * 0x100, I.regs.b[0]);
            CLKS(8, 8, 3);
        }
        void i_outdxax()
        {
            //WriteIOWord(I.regs.w[2], I.regs.w[0]);
            //CLKW(12, 12, 5, 12, 8, 3, I.regs.w[2]);
            WriteIOWord(I.regs.b[4] + I.regs.b[5] * 0x100, (ushort)(I.regs.b[0] + I.regs.b[1] * 0x100));
            CLKW(12, 12, 5, 12, 8, 3, I.regs.b[4] + I.regs.b[5] * 0x100);
        }
        void i_lock()
        {
            I.no_interrupt = 1;
            CLK(2);
        }
        void i_repne()
        {
            byte next = fetchop();
            ushort c = (ushort)(I.regs.b[2] + I.regs.b[3] * 0x100);//I.regs.w[1];
            switch (next)
            {
                case 0x26: seg_prefix = 1; prefix_base = (I.sregs[0] << 4); next = fetchop(); CLK(2); break;
                case 0x2e: seg_prefix = 1; prefix_base = (I.sregs[1] << 4); next = fetchop(); CLK(2); break;
                case 0x36: seg_prefix = 1; prefix_base = (I.sregs[2] << 4); next = fetchop(); CLK(2); break;
                case 0x3e: seg_prefix = 1; prefix_base = (I.sregs[3] << 4); next = fetchop(); CLK(2); break;
            }
            switch (next)
            {
                case 0x6c: CLK(2); if (c != 0) do { i_insb(); c--; } while (c > 0); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100);/*I.regs.w[1] = c;*/ break;
                case 0x6d: CLK(2); if (c != 0) do { i_insw(); c--; } while (c > 0); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0x6e: CLK(2); if (c != 0) do { i_outsb(); c--; } while (c > 0); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0x6f: CLK(2); if (c != 0) do { i_outsw(); c--; } while (c > 0); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa4: CLK(2); if (c != 0) do { i_movsb(); c--; } while (c > 0); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa5: CLK(2); if (c != 0) do { i_movsw(); c--; } while (c > 0); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa6: CLK(2); if (c != 0) do { i_cmpsb(); c--; } while (c > 0 && ZF() == false); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa7: CLK(2); if (c != 0) do { i_cmpsw(); c--; } while (c > 0 && ZF() == false); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xaa: CLK(2); if (c != 0) do { i_stosb(); c--; } while (c > 0); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xab: CLK(2); if (c != 0) do { i_stosw(); c--; } while (c > 0); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xac: CLK(2); if (c != 0) do { i_lodsb(); c--; } while (c > 0); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xad: CLK(2); if (c != 0) do { i_lodsw(); c--; } while (c > 0); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xae: CLK(2); if (c != 0) do { i_scasb(); c--; } while (c > 0 && ZF() == false); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xaf: CLK(2); if (c != 0) do { i_scasw(); c--; } while (c > 0 && ZF() == false); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                default: nec_instruction[next](); break;
            }
            seg_prefix = 0;
        }
        void i_repe()
        {
            byte next = fetchop();
            ushort c = (ushort)(I.regs.b[2] + I.regs.b[3] * 0x100);// I.regs.w[1];
            switch (next)
            {
                case 0x26: seg_prefix = 1; prefix_base = (I.sregs[0] << 4); next = fetchop(); CLK(2); break;
                case 0x2e: seg_prefix = 1; prefix_base = (I.sregs[1] << 4); next = fetchop(); CLK(2); break;
                case 0x36: seg_prefix = 1; prefix_base = (I.sregs[2] << 4); next = fetchop(); CLK(2); break;
                case 0x3e: seg_prefix = 1; prefix_base = (I.sregs[3] << 4); next = fetchop(); CLK(2); break;
            }
            switch (next)
            {
                case 0x6c: CLK(2); if (c != 0) do { i_insb(); c--; } while (c > 0); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100);/*I.regs.w[1] = c;*/ break;
                case 0x6d: CLK(2); if (c != 0) do { i_insw(); c--; } while (c > 0); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0x6e: CLK(2); if (c != 0) do { i_outsb(); c--; } while (c > 0); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0x6f: CLK(2); if (c != 0) do { i_outsw(); c--; } while (c > 0); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa4: CLK(2); if (c != 0) do { i_movsb(); c--; } while (c > 0); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa5: CLK(2); if (c != 0) do { i_movsw(); c--; } while (c > 0); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa6: CLK(2); if (c != 0) do { i_cmpsb(); c--; } while (c > 0 && ZF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa7: CLK(2); if (c != 0) do { i_cmpsw(); c--; } while (c > 0 && ZF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xaa: CLK(2); if (c != 0) do { i_stosb(); c--; } while (c > 0); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xab: CLK(2); if (c != 0) do { i_stosw(); c--; } while (c > 0); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xac: CLK(2); if (c != 0) do { i_lodsb(); c--; } while (c > 0); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xad: CLK(2); if (c != 0) do { i_lodsw(); c--; } while (c > 0); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xae: CLK(2); if (c != 0) do { i_scasb(); c--; } while (c > 0 && ZF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xaf: CLK(2); if (c != 0) do { i_scasw(); c--; } while (c > 0 && ZF()); I.regs.b[2] = (byte)(c % 0x100); I.regs.b[3] = (byte)(c / 0x100); break;
                default: nec_instruction[next](); break;
            }
            seg_prefix = 0;
        }
        void i_hlt()
        {
            pendingCycles = 0;
        }
        void i_cmc()
        {
            I.CarryVal = (uint)(CF() ? 0 : 1);
            CLK(2);
        }
        void i_f6pre()
        {
            int ModRM;
            uint tmp;
            uint uresult, uresult2;
            int result, result2;
            ModRM = GetModRM();
            tmp = GetRMByte(ModRM);
            switch (ModRM & 0x38)
            {
                case 0x00: tmp &= FETCH(); I.CarryVal = I.OverVal = 0; SetSZPF_Byte((int)tmp); pendingCycles -= (ModRM >= 0xc0) ? 4 : 11; break;
                case 0x08: break;
                case 0x10: PutbackRMByte(ModRM, (byte)(~tmp)); pendingCycles -= (ModRM >= 0xc0) ? 2 : 16; break;
                case 0x18: I.CarryVal = (uint)((tmp != 0) ? 1 : 0); tmp = (~tmp) + 1; SetSZPF_Byte((int)tmp); PutbackRMByte(ModRM, (byte)(tmp & 0xff)); pendingCycles -= (ModRM >= 0xc0) ? 2 : 16; break;
                case 0x20:
                    uresult = I.regs.b[0] * tmp;
                    //I.regs.w[0] = (ushort)uresult;
                    I.regs.b[0] = (byte)((ushort)uresult % 0x100);
                    I.regs.b[1] = (byte)((ushort)uresult / 0x100);
                    I.CarryVal = I.OverVal = (uint)((I.regs.b[1] != 0) ? 1 : 0);
                    pendingCycles -= (ModRM >= 0xc0) ? 30 : 36;
                    break;
                case 0x28:
                    result = (short)((sbyte)I.regs.b[0]) * (short)((sbyte)tmp);
                    //I.regs.w[0] = (ushort)result;
                    I.regs.b[0] = (byte)((ushort)result % 0x100);
                    I.regs.b[1] = (byte)((ushort)result / 0x100);
                    I.CarryVal = I.OverVal = (uint)((I.regs.b[1] != 0) ? 1 : 0);
                    pendingCycles -= (ModRM >= 0xc0) ? 30 : 36;
                    break;
                case 0x30:
                    if (tmp != 0)
                    {
                        bool b1;
                        DIVUB((int)tmp, out b1);
                        if (b1)
                        {
                            break;
                        }
                    }
                    else
                    {
                        nec_interrupt(0, false);
                    }
                    pendingCycles -= (ModRM >= 0xc0) ? 43 : 53;
                    break;
                case 0x38:
                    if (tmp != 0)
                    {
                        bool b1;
                        DIVB((int)tmp, out b1);
                        if (b1)
                        {
                            break;
                        }
                    }
                    else
                    {
                        nec_interrupt(0, false);
                    }
                    pendingCycles -= (ModRM >= 0xc0) ? 43 : 53;
                    break;
            }
        }
        void i_f7pre()
        {
            int ModRM;
            uint tmp, tmp2;
            uint uresult, uresult2;
            int result, result2;
            ModRM = GetModRM();
            tmp = GetRMWord(ModRM);
            switch (ModRM & 0x38)
            {
                case 0x00: tmp2 = FETCHWORD(); tmp &= tmp2; I.CarryVal = I.OverVal = 0; SetSZPF_Word((int)tmp); pendingCycles -= (ModRM >= 0xc0) ? 4 : 11; break;
                case 0x08: break;
                case 0x10: PutbackRMWord(ModRM, (ushort)(~tmp)); pendingCycles -= (ModRM >= 0xc0) ? 2 : 16; break;
                case 0x18: I.CarryVal = (uint)((tmp != 0) ? 1 : 0); tmp = (~tmp) + 1; SetSZPF_Word((int)tmp); PutbackRMWord(ModRM, (ushort)(tmp & 0xffff)); pendingCycles -= (ModRM >= 0xc0) ? 2 : 16; break;
                case 0x20:
                    uresult = (uint)((I.regs.b[0] + I.regs.b[1] * 0x100) * tmp);
                    //I.regs.w[0] = (ushort)(uresult & 0xffff);
                    //I.regs.w[2] = (ushort)(uresult >> 16);
                    I.regs.b[0] = (byte)((ushort)(uresult & 0xffff) % 0x100);
                    I.regs.b[1] = (byte)((ushort)(uresult & 0xffff) / 0x100);
                    I.regs.b[4] = (byte)((ushort)(uresult >> 16) % 0x100);
                    I.regs.b[5] = (byte)((ushort)(uresult >> 16) / 0x100);
                    I.CarryVal = I.OverVal = (uint)(((I.regs.b[4] + I.regs.b[5] * 0x100) != 0) ? 1 : 0);
                    pendingCycles -= (ModRM >= 0xc0) ? 30 : 36;
                    break;
                case 0x28:
                    result = (int)((short)(I.regs.b[0] + I.regs.b[1] * 0x100)) * (int)((short)tmp);
                    //I.regs.w[0] = (ushort)(result & 0xffff);
                    //I.regs.w[2] = (ushort)(result >> 16);
                    I.regs.b[0] = (byte)((ushort)(result & 0xffff) % 0x100);
                    I.regs.b[1] = (byte)((ushort)(result & 0xffff) / 0x100);
                    I.regs.b[4] = (byte)((ushort)(result >> 16) % 0x100);
                    I.regs.b[5] = (byte)((ushort)(result >> 16) / 0x100);
                    I.CarryVal = I.OverVal = (uint)(((I.regs.b[4] + I.regs.b[5] * 0x100) != 0) ? 1 : 0);
                    pendingCycles -= (ModRM >= 0xc0) ? 30 : 36;
                    break;
                case 0x30:
                    if (tmp != 0)
                    {
                        bool b1;
                        DIVUW((int)tmp, out b1);
                        if (b1)
                        {
                            break;
                        }
                    }
                    else
                    {
                        nec_interrupt(0, false);
                    }
                    pendingCycles -= (ModRM >= 0xc0) ? 43 : 53;
                    break;
                case 0x38:
                    if (tmp != 0)
                    {
                        bool b1;
                        DIVW((int)tmp, out b1);
                        if (b1)
                        {
                            break;
                        }
                    }
                    else
                    {
                        nec_interrupt(0, false);
                    }
                    pendingCycles -= (ModRM >= 0xc0) ? 43 : 53;
                    break;
            }
        }
        void i_clc()
        {
            I.CarryVal = 0;
            CLK(2);
        }
        void i_stc()
        {
            I.CarryVal = 1;
            CLK(2);
        }
        void i_di()
        {
            I.IF = false;
            CLK(2);
        }
        void i_ei()
        {
            I.IF = true;
            CLK(2);
        }
        void i_cld()
        {
            I.DF = false;
            CLK(2);
        }
        void i_std()
        {
            I.DF = true;
            CLK(2);
        }
        void i_fepre()
        {
            int ModRM;
            byte tmp, tmp1;
            ModRM = GetModRM();
            tmp = GetRMByte(ModRM);
            switch (ModRM & 0x38)
            {
                case 0x00: tmp1 = (byte)(tmp + 1); I.OverVal = (uint)((tmp == 0x7f) ? 1 : 0); SetAF(tmp1, tmp, 1); SetSZPF_Byte(tmp1); PutbackRMByte(ModRM, (byte)tmp1); CLKM(ModRM, 2, 2, 2, 16, 16, 7); break;
                case 0x08: tmp1 = (byte)(tmp - 1); I.OverVal = (uint)((tmp == 0x80) ? 1 : 0); SetAF(tmp1, tmp, 1); SetSZPF_Byte(tmp1); PutbackRMByte(ModRM, (byte)tmp1); CLKM(ModRM, 2, 2, 2, 16, 16, 7); break;
                default: break;
            }
        }
        void i_ffpre()
        {
            int ModRM;
            ushort tmp, tmp1;
            ModRM = GetModRM();
            tmp = GetRMWord(ModRM);
            switch (ModRM & 0x38)
            {
                case 0x00: tmp1 = (ushort)(tmp + 1); I.OverVal = (uint)((tmp == 0x7fff) ? 1 : 0); SetAF(tmp1, tmp, 1); SetSZPF_Word(tmp1); PutbackRMWord(ModRM, (ushort)tmp1); CLKM(ModRM, 2, 2, 2, 24, 16, 7); break;
                case 0x08: tmp1 = (ushort)(tmp - 1); I.OverVal = (uint)((tmp == 0x8000) ? 1 : 0); SetAF(tmp1, tmp, 1); SetSZPF_Word(tmp1); PutbackRMWord(ModRM, (ushort)tmp1); CLKM(ModRM, 2, 2, 2, 24, 16, 7); break;
                case 0x10:
                    PUSH(I.ip);
                    I.ip = (ushort)tmp;
                    //CHANGE_PC;
                    pendingCycles -= (ModRM >= 0xc0) ? 16 : 20;
                    break;
                case 0x18:
                    tmp1 = I.sregs[1];
                    I.sregs[1] = GetnextRMWord();
                    PUSH(tmp1);
                    PUSH(I.ip);
                    I.ip = tmp;
                    //CHANGE_PC;
                    pendingCycles -= (ModRM >= 0xc0) ? 16 : 26;
                    break;
                case 0x20:
                    I.ip = tmp;
                    //CHANGE_PC;
                    pendingCycles -= 13;
                    break;
                case 0x28:
                    I.ip = tmp;
                    I.sregs[1] = GetnextRMWord();
                    //CHANGE_PC;
                    pendingCycles -= 15;
                    break;
                case 0x30: PUSH(tmp); pendingCycles -= 4; break;
                default: break;
            }
        }
        void i_invalid()
        {
            pendingCycles -= 10;
        }
    }
}
