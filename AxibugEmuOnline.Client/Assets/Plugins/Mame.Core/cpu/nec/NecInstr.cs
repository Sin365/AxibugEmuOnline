namespace cpu.nec
{

    public static unsafe class NecEx
    {
        public static void i_add_br8(this Nec nec)
        {
            int ModRM;
            byte src, dst;
            nec.DEF_br8(out ModRM, out src, out dst);
            nec.ADDB(ref src, ref dst);
            nec.PutbackRMByte(ModRM, dst);
            nec.CLKM(ModRM, 2, 2, 2, 16, 16, 7);
        }
        public static void i_add_wr16(this Nec nec)
        {
            int ModRM;
            ushort src, dst;
            nec.DEF_wr16(out ModRM, out src, out dst);
            nec.ADDW(ref src, ref dst);
            nec.PutbackRMWord(ModRM, dst);
            nec.CLKR(ModRM, 24, 24, 11, 24, 16, 7, 2, Nec.EA);
        }
        public static void i_add_r8b(this Nec nec)
        {
            int ModRM;
            byte src, dst;
            nec.DEF_r8b(out ModRM, out src, out dst);
            nec.ADDB(ref src, ref dst);
            nec.I.regs.b[nec.mod_RM.regb[ModRM]] = dst;
            nec.CLKM(ModRM, 2, 2, 2, 11, 11, 6);
        }
        public static void i_add_r16w(this Nec nec)
        {
            int ModRM;
            ushort src, dst;
            nec.DEF_r16w(out ModRM, out src, out dst);
            nec.ADDW(ref src, ref dst);
            //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = dst;
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
            nec.CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, Nec.EA);
        }
        public static void i_add_ald8(this Nec nec)
        {
            byte src, dst;
            nec.DEF_ald8(out src, out dst);
            nec.ADDB(ref src, ref dst);
            nec.I.regs.b[0] = dst;
            nec.CLKS(4, 4, 2);
        }
        public static void i_add_axd16(this Nec nec)
        {
            ushort src, dst;
            nec.DEF_axd16(out src, out dst);
            nec.ADDW(ref src, ref dst);
            //nec.I.regs.w[0] = dst;
            nec.I.regs.b[0] = (byte)(dst % 0x100);
            nec.I.regs.b[1] = (byte)(dst / 0x100);
            nec.CLKS(4, 4, 2);
        }
        public static void i_push_es(this Nec nec)
        {
            nec.PUSH(nec.I.sregs[0]);
            nec.CLKS(12, 8, 3);
        }
        public static void i_pop_es(this Nec nec)
        {
            nec.POP(ref nec.I.sregs[0]);
            nec.CLKS(12, 8, 5);
        }
        public static void i_or_br8(this Nec nec)
        {
            int ModRM;
            byte src, dst;
            nec.DEF_br8(out ModRM, out src, out dst);
            nec.ORB(ref src, ref dst);
            nec.PutbackRMByte(ModRM, dst);
            nec.CLKM(ModRM, 2, 2, 2, 16, 16, 7);
        }
        public static void i_or_wr16(this Nec nec)
        {
            int ModRM;
            ushort src, dst;
            nec.DEF_wr16(out ModRM, out src, out dst);
            nec.ORW(ref src, ref dst);
            nec.PutbackRMWord(ModRM, dst);
            nec.CLKR(ModRM, 24, 24, 11, 24, 16, 7, 2, Nec.EA);
        }
        public static void i_or_r8b(this Nec nec)
        {
            int ModRM;
            byte src, dst;
            nec.DEF_r8b(out ModRM, out src, out dst);
            nec.ORB(ref src, ref dst);
            nec.I.regs.b[nec.mod_RM.regb[ModRM]] = dst;
            nec.CLKM(ModRM, 2, 2, 2, 11, 11, 6);
        }
        public static void i_or_r16w(this Nec nec)
        {
            int ModRM;
            ushort src, dst;
            nec.DEF_r16w(out ModRM, out src, out dst);
            nec.ORW(ref src, ref dst);
            //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = dst;
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
            nec.CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, Nec.EA);
        }
        public static void i_or_ald8(this Nec nec)
        {
            byte src, dst;
            nec.DEF_ald8(out src, out dst);
            nec.ORB(ref src, ref dst);
            nec.I.regs.b[0] = dst;
            nec.CLKS(4, 4, 2);
        }
        public static void i_or_axd16(this Nec nec)
        {
            ushort src, dst;
            nec.DEF_axd16(out src, out dst);
            nec.ORW(ref src, ref dst);
            //nec.I.regs.w[0] = dst;
            nec.I.regs.b[0] = (byte)(dst % 0x100);
            nec.I.regs.b[1] = (byte)(dst / 0x100);
            nec.CLKS(4, 4, 2);
        }
        public static void i_push_cs(this Nec nec)
        {
            nec.PUSH(nec.I.sregs[1]);
            nec.CLKS(12, 8, 3);
        }
        public static void i_pre_nec(this Nec nec)
        {
            int ModRM = 0, tmp = 0, tmp2 = 0;
            switch (nec.FETCH())
            {
                case 0x10: nec.BITOP_BYTE(ref ModRM, ref tmp); nec.CLKS(3, 3, 4); tmp2 = nec.I.regs.b[2] & 0x7; nec.I.ZeroVal = (uint)(((tmp & (1 << tmp2)) != 0) ? 1 : 0); nec.I.CarryVal = nec.I.OverVal = 0; break; /* Test */
                case 0x11: nec.BITOP_WORD(ref ModRM, ref tmp); nec.CLKS(3, 3, 4); tmp2 = nec.I.regs.b[2] & 0xf; nec.I.ZeroVal = (uint)(((tmp & (1 << tmp2)) != 0) ? 1 : 0); nec.I.CarryVal = nec.I.OverVal = 0; break; /* Test */
                case 0x12: nec.BITOP_BYTE(ref ModRM, ref tmp); nec.CLKS(5, 5, 4); tmp2 = nec.I.regs.b[2] & 0x7; tmp &= ~(1 << tmp2); nec.PutbackRMByte(ModRM, (byte)tmp); break; /* Clr */
                case 0x13: nec.BITOP_WORD(ref ModRM, ref tmp); nec.CLKS(5, 5, 4); tmp2 = nec.I.regs.b[2] & 0xf; tmp &= ~(1 << tmp2); nec.PutbackRMWord(ModRM, (ushort)tmp); break; /* Clr */
                case 0x14: nec.BITOP_BYTE(ref ModRM, ref tmp); nec.CLKS(4, 4, 4); tmp2 = nec.I.regs.b[2] & 0x7; tmp |= (1 << tmp2); nec.PutbackRMByte(ModRM, (byte)tmp); break; /* Set */
                case 0x15: nec.BITOP_WORD(ref ModRM, ref tmp); nec.CLKS(4, 4, 4); tmp2 = nec.I.regs.b[2] & 0xf; tmp |= (1 << tmp2); nec.PutbackRMWord(ModRM, (ushort)tmp); break; /* Set */
                case 0x16: nec.BITOP_BYTE(ref ModRM, ref tmp); nec.CLKS(4, 4, 4); tmp2 = nec.I.regs.b[2] & 0x7; nec.BIT_NOT(ref tmp, ref tmp2); nec.PutbackRMByte(ModRM, (byte)tmp); break; /* Not */
                case 0x17: nec.BITOP_WORD(ref ModRM, ref tmp); nec.CLKS(4, 4, 4); tmp2 = nec.I.regs.b[2] & 0xf; nec.BIT_NOT(ref tmp, ref tmp2); nec.PutbackRMWord(ModRM, (ushort)tmp); break; /* Not */

                case 0x18: nec.BITOP_BYTE(ref ModRM, ref tmp); nec.CLKS(4, 4, 4); tmp2 = (nec.FETCH()) & 0x7; nec.I.ZeroVal = (uint)(((tmp & (1 << tmp2)) != 0) ? 1 : 0); nec.I.CarryVal = nec.I.OverVal = 0; break; /* Test */
                case 0x19: nec.BITOP_WORD(ref ModRM, ref tmp); nec.CLKS(4, 4, 4); tmp2 = (nec.FETCH()) & 0xf; nec.I.ZeroVal = (uint)(((tmp & (1 << tmp2)) != 0) ? 1 : 0); nec.I.CarryVal = nec.I.OverVal = 0; break; /* Test */
                case 0x1a: nec.BITOP_BYTE(ref ModRM, ref tmp); nec.CLKS(6, 6, 4); tmp2 = (nec.FETCH()) & 0x7; tmp &= ~(1 << tmp2); nec.PutbackRMByte(ModRM, (byte)tmp); break; /* Clr */
                case 0x1b: nec.BITOP_WORD(ref ModRM, ref tmp); nec.CLKS(6, 6, 4); tmp2 = (nec.FETCH()) & 0xf; tmp &= ~(1 << tmp2); nec.PutbackRMWord(ModRM, (ushort)tmp); break; /* Clr */
                case 0x1c: nec.BITOP_BYTE(ref ModRM, ref tmp); nec.CLKS(5, 5, 4); tmp2 = (nec.FETCH()) & 0x7; tmp |= (1 << tmp2); nec.PutbackRMByte(ModRM, (byte)tmp); break; /* Set */
                case 0x1d: nec.BITOP_WORD(ref ModRM, ref tmp); nec.CLKS(5, 5, 4); tmp2 = (nec.FETCH()) & 0xf; tmp |= (1 << tmp2); nec.PutbackRMWord(ModRM, (ushort)tmp); break; /* Set */
                case 0x1e: nec.BITOP_BYTE(ref ModRM, ref tmp); nec.CLKS(5, 5, 4); tmp2 = (nec.FETCH()) & 0x7; nec.BIT_NOT(ref tmp, ref tmp2); nec.PutbackRMByte(ModRM, (byte)tmp); break; /* Not */
                case 0x1f: nec.BITOP_WORD(ref ModRM, ref tmp); nec.CLKS(5, 5, 4); tmp2 = (nec.FETCH()) & 0xf; nec.BIT_NOT(ref tmp, ref tmp2); nec.PutbackRMWord(ModRM, (ushort)tmp); break; /* Not */

                case 0x20: nec.ADD4S(ref tmp, ref tmp2); nec.CLKS(7, 7, 2); break;
                case 0x22: nec.SUB4S(ref tmp, ref tmp2); nec.CLKS(7, 7, 2); break;
                case 0x26: nec.CMP4S(ref tmp, ref tmp2); nec.CLKS(7, 7, 2); break;
                case 0x28: ModRM = nec.FETCH(); tmp = nec.GetRMByte(ModRM); tmp <<= 4; tmp |= nec.I.regs.b[0] & 0xf; nec.I.regs.b[0] = (byte)((nec.I.regs.b[0] & 0xf0) | ((tmp >> 8) & 0xf)); tmp &= 0xff; nec.PutbackRMByte(ModRM, (byte)tmp); nec.CLKM(ModRM, 13, 13, 9, 28, 28, 15); break;
                case 0x2a: ModRM = nec.FETCH(); tmp = nec.GetRMByte(ModRM); tmp2 = (nec.I.regs.b[0] & 0xf) << 4; nec.I.regs.b[0] = (byte)((nec.I.regs.b[0] & 0xf0) | (tmp & 0xf)); tmp = tmp2 | (tmp >> 4); nec.PutbackRMByte(ModRM, (byte)tmp); nec.CLKM(ModRM, 17, 17, 13, 32, 32, 19); break;
                case 0x31: ModRM = nec.FETCH(); ModRM = 0; break;
                case 0x33: ModRM = nec.FETCH(); ModRM = 0; break;
                case 0x92: nec.CLK(2); break; /* V25/35 FINT */
                case 0xe0: ModRM = nec.FETCH(); ModRM = 0; break;
                case 0xf0: ModRM = nec.FETCH(); ModRM = 0; break;
                case 0xff: ModRM = nec.FETCH(); ModRM = 0; break;
                default: break;
            }
        }
        public static void i_adc_br8(this Nec nec)
        {
            int ModRM;
            byte src, dst;
            nec.DEF_br8(out ModRM, out src, out dst);
            src += (byte)(nec.CF() ? 1 : 0);
            nec.ADDB(ref src, ref dst);
            nec.PutbackRMByte(ModRM, dst);
            nec.CLKM(ModRM, 2, 2, 2, 16, 16, 7);
        }
        public static void i_adc_wr16(this Nec nec)
        {
            int ModRM;
            ushort src, dst;
            nec.DEF_wr16(out ModRM, out src, out dst);
            src += (ushort)(nec.CF() ? 1 : 0);
            nec.ADDW(ref src, ref dst);
            nec.PutbackRMWord(ModRM, dst);
            nec.CLKR(ModRM, 24, 24, 11, 24, 16, 7, 2, Nec.EA);
        }
        public static void i_adc_r8b(this Nec nec)
        {
            int ModRM;
            byte src, dst;
            nec.DEF_r8b(out ModRM, out src, out dst);
            src += (byte)(nec.CF() ? 1 : 0);
            nec.ADDB(ref src, ref dst);
            nec.I.regs.b[nec.mod_RM.regb[ModRM]] = dst;
            nec.CLKM(ModRM, 2, 2, 2, 11, 11, 6);
        }
        public static void i_adc_r16w(this Nec nec)
        {
            int ModRM;
            ushort src, dst;
            nec.DEF_r16w(out ModRM, out src, out dst);
            src += (ushort)(nec.CF() ? 1 : 0);
            nec.ADDW(ref src, ref dst);
            //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = dst;
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
            nec.CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, Nec.EA);
        }
        public static void i_adc_ald8(this Nec nec)
        {
            byte src, dst;
            nec.DEF_ald8(out src, out dst);
            src += (byte)(nec.CF() ? 1 : 0);
            nec.ADDB(ref src, ref dst);
            nec.I.regs.b[0] = dst;
            nec.CLKS(4, 4, 2);
        }
        public static void i_adc_axd16(this Nec nec)
        {
            ushort src, dst;
            nec.DEF_axd16(out src, out dst);
            src += (ushort)(nec.CF() ? 1 : 0);
            nec.ADDW(ref src, ref dst);
            //nec.I.regs.w[0] = dst;
            nec.I.regs.b[0] = (byte)(dst % 0x100);
            nec.I.regs.b[1] = (byte)(dst / 0x100);
            nec.CLKS(4, 4, 2);
        }
        public static void i_push_ss(this Nec nec)
        {
            nec.PUSH(nec.I.sregs[2]);
            nec.CLKS(12, 8, 3);
        }
        public static void i_pop_ss(this Nec nec)
        {
            nec.POP(ref nec.I.sregs[2]);
            nec.CLKS(12, 8, 5);
            nec.I.no_interrupt = 1;
        }
        public static void i_sbb_br8(this Nec nec)
        {
            int ModRM;
            byte src, dst;
            nec.DEF_br8(out ModRM, out src, out dst);
            src += (byte)(nec.CF() ? 1 : 0);
            nec.SUBB(ref src, ref dst);
            nec.PutbackRMByte(ModRM, dst);
            nec.CLKM(ModRM, 2, 2, 2, 16, 16, 7);
        }
        public static void i_sbb_wr16(this Nec nec)
        {
            int ModRM;
            ushort src, dst;
            nec.DEF_wr16(out ModRM, out src, out dst);
            src += (ushort)(nec.CF() ? 1 : 0);
            nec.SUBW(ref src, ref dst);
            nec.PutbackRMWord(ModRM, dst);
            nec.CLKR(ModRM, 24, 24, 11, 24, 16, 7, 2, Nec.EA);
        }
        public static void i_sbb_r8b(this Nec nec)
        {
            int ModRM;
            byte src, dst;
            nec.DEF_r8b(out ModRM, out src, out dst);
            src += (byte)(nec.CF() ? 1 : 0);
            nec.SUBB(ref src, ref dst);
            nec.I.regs.b[nec.mod_RM.regb[ModRM]] = dst;
            nec.CLKM(ModRM, 2, 2, 2, 11, 11, 6);
        }
        public static void i_sbb_r16w(this Nec nec)
        {
            int ModRM;
            ushort src, dst;
            nec.DEF_r16w(out ModRM, out src, out dst);
            src += (ushort)(nec.CF() ? 1 : 0);
            nec.SUBW(ref src, ref dst);
            //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = dst;
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
            nec.CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, Nec.EA);
        }
        public static void i_sbb_ald8(this Nec nec)
        {
            byte src, dst;
            nec.DEF_ald8(out src, out dst);
            src += (byte)(nec.CF() ? 1 : 0);
            nec.SUBB(ref src, ref dst);
            nec.I.regs.b[0] = dst;
            nec.CLKS(4, 4, 2);
        }
        public static void i_sbb_axd16(this Nec nec)
        {
            ushort src, dst;
            nec.DEF_axd16(out src, out dst);
            src += (ushort)(nec.CF() ? 1 : 0);
            nec.SUBW(ref src, ref dst);
            //nec.I.regs.w[0] = dst;
            nec.I.regs.b[0] = (byte)(dst % 0x100);
            nec.I.regs.b[1] = (byte)(dst / 0x100);
            nec.CLKS(4, 4, 2);
        }
        public static void i_push_ds(this Nec nec)
        {
            nec.PUSH(nec.I.sregs[3]);
            nec.CLKS(12, 8, 3);
        }
        public static void i_pop_ds(this Nec nec)
        {
            nec.POP(ref nec.I.sregs[3]);
            nec.CLKS(12, 8, 5);
        }
        public static void i_and_br8(this Nec nec)
        {
            int ModRM;
            byte src, dst;
            nec.DEF_br8(out ModRM, out src, out dst);
            nec.ANDB(ref src, ref dst);
            nec.PutbackRMByte(ModRM, dst);
            nec.CLKM(ModRM, 2, 2, 2, 16, 16, 7);
        }
        public static void i_and_wr16(this Nec nec)
        {
            int ModRM;
            ushort src, dst;
            nec.DEF_wr16(out ModRM, out src, out dst);
            nec.ANDW(ref src, ref dst);
            nec.PutbackRMWord(ModRM, dst);
            nec.CLKR(ModRM, 24, 24, 11, 24, 16, 7, 2, Nec.EA);
        }
        public static void i_and_r8b(this Nec nec)
        {
            int ModRM;
            byte src, dst;
            nec.DEF_r8b(out ModRM, out src, out dst);
            nec.ANDB(ref src, ref dst);
            nec.I.regs.b[nec.mod_RM.regb[ModRM]] = dst;
            nec.CLKM(ModRM, 2, 2, 2, 11, 11, 6);
        }
        public static void i_and_r16w(this Nec nec)
        {
            int ModRM;
            ushort src, dst;
            nec.DEF_r16w(out ModRM, out src, out dst);
            nec.ANDW(ref src, ref dst);
            //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = dst;
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
            nec.CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, Nec.EA);
        }
        public static void i_and_ald8(this Nec nec)
        {
            byte src, dst;
            nec.DEF_ald8(out src, out dst);
            nec.ANDB(ref src, ref dst);
            nec.I.regs.b[0] = dst;
            nec.CLKS(4, 4, 2);
        }
        public static void i_and_axd16(this Nec nec)
        {
            ushort src, dst;
            nec.DEF_axd16(out src, out dst);
            nec.ANDW(ref src, ref dst);
            //nec.I.regs.w[0] = dst;
            nec.I.regs.b[0] = (byte)(dst % 0x100);
            nec.I.regs.b[1] = (byte)(dst / 0x100);
            nec.CLKS(4, 4, 2);
        }
        public unsafe static void i_es(this Nec nec)
        {
            Nec.seg_prefix = 1;
            Nec.prefix_base = nec.I.sregs[0] << 4;
            nec.CLK(2);
            nec.nec_instruction[nec.fetchop()](nec);
            //DoInstructionOpCode(nec.fetchop());
            Nec.seg_prefix = 0;
        }
        public static void i_daa(this Nec nec)
        {
            nec.ADJ4(6, 0x60);
            nec.CLKS(3, 3, 2);
        }
        public static void i_sub_br8(this Nec nec)
        {
            int ModRM;
            byte src, dst;
            nec.DEF_br8(out ModRM, out src, out dst);
            nec.SUBB(ref src, ref dst);
            nec.PutbackRMByte(ModRM, dst);
            nec.CLKM(ModRM, 2, 2, 2, 16, 16, 7);
        }
        public static void i_sub_wr16(this Nec nec)
        {
            int ModRM;
            ushort src, dst;
            nec.DEF_wr16(out ModRM, out src, out dst);
            nec.SUBW(ref src, ref dst);
            nec.PutbackRMWord(ModRM, dst);
            nec.CLKR(ModRM, 24, 24, 11, 24, 16, 7, 2, Nec.EA);
        }
        public static void i_sub_r8b(this Nec nec)
        {
            int ModRM;
            byte src, dst;
            nec.DEF_r8b(out ModRM, out src, out dst);
            nec.SUBB(ref src, ref dst);
            nec.I.regs.b[nec.mod_RM.regb[ModRM]] = dst;
            nec.CLKM(ModRM, 2, 2, 2, 11, 11, 6);
        }
        public static void i_sub_r16w(this Nec nec)
        {
            int ModRM;
            ushort src, dst;
            nec.DEF_r16w(out ModRM, out src, out dst);
            nec.SUBW(ref src, ref dst);
            //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = dst;
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
            nec.CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, Nec.EA);
        }
        public static void i_sub_ald8(this Nec nec)
        {
            byte src, dst;
            nec.DEF_ald8(out src, out dst);
            nec.SUBB(ref src, ref dst);
            nec.I.regs.b[0] = dst;
            nec.CLKS(4, 4, 2);
        }
        public static void i_sub_axd16(this Nec nec)
        {
            ushort src, dst;
            nec.DEF_axd16(out src, out dst);
            nec.SUBW(ref src, ref dst);
            //nec.I.regs.w[0] = dst;
            nec.I.regs.b[0] = (byte)(dst % 0x100);
            nec.I.regs.b[1] = (byte)(dst / 0x100);
            nec.CLKS(4, 4, 2);
        }
        public static void i_cs(this Nec nec)
        {
            Nec.seg_prefix = 1;
            Nec.prefix_base = nec.I.sregs[1] << 4;
            nec.CLK(2);
            nec.nec_instruction[nec.fetchop()](nec);
            //DoInstructionOpCode(nec.fetchop());
            Nec.seg_prefix = 0;
        }
        public static void i_das(this Nec nec)
        {
            nec.ADJ4(-6, -0x60);
            nec.CLKS(3, 3, 2);
        }
        public static void i_xor_br8(this Nec nec)
        {
            int ModRM;
            byte src, dst;
            nec.DEF_br8(out ModRM, out src, out dst);
            nec.XORB(ref src, ref dst);
            nec.PutbackRMByte(ModRM, dst);
            nec.CLKM(ModRM, 2, 2, 2, 16, 16, 7);
        }
        public static void i_xor_wr16(this Nec nec)
        {
            int ModRM;
            ushort src, dst;
            nec.DEF_wr16(out ModRM, out src, out dst);
            nec.XORW(ref src, ref dst);
            nec.PutbackRMWord(ModRM, dst);
            nec.CLKR(ModRM, 24, 24, 11, 24, 16, 7, 2, Nec.EA);
        }
        public static void i_xor_r8b(this Nec nec)
        {
            int ModRM;
            byte src, dst;
            nec.DEF_r8b(out ModRM, out src, out dst);
            nec.XORB(ref src, ref dst);
            nec.I.regs.b[nec.mod_RM.regb[ModRM]] = dst;
            nec.CLKM(ModRM, 2, 2, 2, 11, 11, 6);
        }
        public static void i_xor_r16w(this Nec nec)
        {
            int ModRM;
            ushort src, dst;
            nec.DEF_r16w(out ModRM, out src, out dst);
            nec.XORW(ref src, ref dst);
            //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = dst;
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
            nec.CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, Nec.EA);
        }
        public static void i_xor_ald8(this Nec nec)
        {
            byte src, dst;
            nec.DEF_ald8(out src, out dst);
            nec.XORB(ref src, ref dst);
            nec.I.regs.b[0] = dst;
            nec.CLKS(4, 4, 2);
        }
        public static void i_xor_axd16(this Nec nec)
        {
            ushort src, dst;
            nec.DEF_axd16(out src, out dst);
            nec.XORW(ref src, ref dst);
            //nec.I.regs.w[0] = dst;
            nec.I.regs.b[0] = (byte)(dst % 0x100);
            nec.I.regs.b[1] = (byte)(dst / 0x100);
            nec.CLKS(4, 4, 2);
        }
        public static void i_ss(this Nec nec)
        {
            Nec.seg_prefix = 1;
            Nec.prefix_base = nec.I.sregs[2] << 4;
            nec.CLK(2);
            nec.nec_instruction[nec.fetchop()](nec);
            //DoInstructionOpCode(nec.fetchop());
            Nec.seg_prefix = 0;
        }
        public static void i_aaa(this Nec nec)
        {
            nec.ADJB(6, (nec.I.regs.b[0] > 0xf9) ? 2 : 1);
            nec.CLKS(7, 7, 4);
        }
        public static void i_cmp_br8(this Nec nec)
        {
            int ModRM;
            byte src, dst;
            nec.DEF_br8(out ModRM, out src, out dst);
            nec.SUBB(ref src, ref dst);
            nec.CLKM(ModRM, 2, 2, 2, 11, 11, 6);
        }
        public static void i_cmp_wr16(this Nec nec)
        {
            int ModRM;
            ushort src, dst;
            nec.DEF_wr16(out ModRM, out src, out dst);
            nec.SUBW(ref src, ref dst);
            nec.CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, Nec.EA);
        }
        public static void i_cmp_r8b(this Nec nec)
        {
            int ModRM;
            byte src, dst;
            nec.DEF_r8b(out ModRM, out src, out dst);
            nec.SUBB(ref src, ref dst);
            nec.CLKM(ModRM, 2, 2, 2, 11, 11, 6);
        }
        public static void i_cmp_r16w(this Nec nec)
        {
            int ModRM;
            ushort src, dst;
            nec.DEF_r16w(out ModRM, out src, out dst);
            nec.SUBW(ref src, ref dst);
            nec.CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, Nec.EA);
        }
        public static void i_cmp_ald8(this Nec nec)
        {
            byte src, dst;
            nec.DEF_ald8(out src, out dst);
            nec.SUBB(ref src, ref dst);
            nec.CLKS(4, 4, 2);
        }
        public static void i_cmp_axd16(this Nec nec)
        {
            ushort src, dst;
            nec.DEF_axd16(out src, out dst);
            nec.SUBW(ref src, ref dst);
            nec.CLKS(4, 4, 2);
        }
        public static void i_ds(this Nec nec)
        {
            Nec.seg_prefix = 1;
            Nec.prefix_base = nec.I.sregs[3] << 4;
            nec.CLK(2);
            nec.nec_instruction[nec.fetchop()](nec);
            //DoInstructionOpCode(nec.fetchop());
            Nec.seg_prefix = 0;
        }
        public static void i_aas(this Nec nec)
        {
            nec.ADJB(-6, (nec.I.regs.b[0] < 6) ? -2 : -1);
            nec.CLKS(7, 7, 4);
        }
        public static void i_inc_ax(this Nec nec)
        {
            nec.IncWordReg(0);
            nec.CLK(2);
        }
        public static void i_inc_cx(this Nec nec)
        {
            nec.IncWordReg(1);
            nec.CLK(2);
        }
        public static void i_inc_dx(this Nec nec)
        {
            nec.IncWordReg(2);
            nec.CLK(2);
        }
        public static void i_inc_bx(this Nec nec)
        {
            nec.IncWordReg(3);
            nec.CLK(2);
        }
        public static void i_inc_sp(this Nec nec)
        {
            nec.IncWordReg(4);
            nec.CLK(2);
        }
        public static void i_inc_bp(this Nec nec)
        {
            nec.IncWordReg(5);
            nec.CLK(2);
        }
        public static void i_inc_si(this Nec nec)
        {
            nec.IncWordReg(6);
            nec.CLK(2);
        }
        public static void i_inc_di(this Nec nec)
        {
            nec.IncWordReg(7);
            nec.CLK(2);
        }
        public static void i_dec_ax(this Nec nec)
        {
            nec.DecWordReg(0);
            nec.CLK(2);
        }
        public static void i_dec_cx(this Nec nec)
        {
            nec.DecWordReg(1);
            nec.CLK(2);
        }
        public static void i_dec_dx(this Nec nec)
        {
            nec.DecWordReg(2);
            nec.CLK(2);
        }
        public static void i_dec_bx(this Nec nec)
        {
            nec.DecWordReg(3);
            nec.CLK(2);
        }
        public static void i_dec_sp(this Nec nec)
        {
            nec.DecWordReg(4);
            nec.CLK(2);
        }
        public static void i_dec_bp(this Nec nec)
        {
            nec.DecWordReg(5);
            nec.CLK(2);
        }
        public static void i_dec_si(this Nec nec)
        {
            nec.DecWordReg(6);
            nec.CLK(2);
        }
        public static void i_dec_di(this Nec nec)
        {
            nec.DecWordReg(7);
            nec.CLK(2);
        }
        public static void i_push_ax(this Nec nec)
        {
            //PUSH(nec.I.regs.w[0]);
            nec.PUSH((ushort)(nec.I.regs.b[0] + nec.I.regs.b[1] * 0x100));
            nec.CLKS(12, 8, 3);
        }
        public static void i_push_cx(this Nec nec)
        {
            //PUSH(nec.I.regs.w[1]);
            nec.PUSH((ushort)(nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100));
            nec.CLKS(12, 8, 3);
        }
        public static void i_push_dx(this Nec nec)
        {
            //PUSH(nec.I.regs.w[2]);
            nec.PUSH((ushort)(nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100));
            nec.CLKS(12, 8, 3);
        }
        public static void i_push_bx(this Nec nec)
        {
            //PUSH(nec.I.regs.w[3]);
            nec.PUSH((ushort)(nec.I.regs.b[6] + nec.I.regs.b[7] * 0x100));
            nec.CLKS(12, 8, 3);
        }
        public static void i_push_sp(this Nec nec)
        {
            //PUSH(nec.I.regs.w[4]);
            nec.PUSH((ushort)(nec.I.regs.b[8] + nec.I.regs.b[9] * 0x100));
            nec.CLKS(12, 8, 3);
        }
        public static void i_push_bp(this Nec nec)
        {
            //PUSH(nec.I.regs.w[5]);
            nec.PUSH((ushort)(nec.I.regs.b[10] + nec.I.regs.b[11] * 0x100));
            nec.CLKS(12, 8, 3);
        }
        public static void i_push_si(this Nec nec)
        {
            //PUSH(nec.I.regs.w[6]);
            nec.PUSH((ushort)(nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100));
            nec.CLKS(12, 8, 3);
        }
        public static void i_push_di(this Nec nec)
        {
            //PUSH(nec.I.regs.w[7]);
            nec.PUSH((ushort)(nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100));
            nec.CLKS(12, 8, 3);
        }
        public static void i_pop_ax(this Nec nec)
        {
            //POP(ref nec.I.regs.w[0]);
            nec.POPW(0);
            nec.CLKS(12, 8, 5);
        }
        public static void i_pop_cx(this Nec nec)
        {
            //POP(ref nec.I.regs.w[1]);
            nec.POPW(1);
            nec.CLKS(12, 8, 5);
        }
        public static void i_pop_dx(this Nec nec)
        {
            //POP(ref nec.I.regs.w[2]);
            nec.POPW(2);
            nec.CLKS(12, 8, 5);
        }
        public static void i_pop_bx(this Nec nec)
        {
            //POP(ref nec.I.regs.w[3]);
            nec.POPW(3);
            nec.CLKS(12, 8, 5);
        }
        public static void i_pop_sp(this Nec nec)
        {
            //POP(ref nec.I.regs.w[4]);
            nec.POPW(4);
            nec.CLKS(12, 8, 5);
        }
        public static void i_pop_bp(this Nec nec)
        {
            //POP(ref nec.I.regs.w[5]);
            nec.POPW(5);
            nec.CLKS(12, 8, 5);
        }
        public static void i_pop_si(this Nec nec)
        {
            //POP(ref nec.I.regs.w[6]);
            nec.POPW(6);
            nec.CLKS(12, 8, 5);
        }
        public static void i_pop_di(this Nec nec)
        {
            //POP(ref nec.I.regs.w[7]);
            nec.POPW(7);
            nec.CLKS(12, 8, 5);
        }
        public static void i_pusha(this Nec nec)
        {
            ushort tmp = (ushort)(nec.I.regs.b[8] + nec.I.regs.b[9] * 0x100);// nec.I.regs.w[4];
            /*PUSH(nec.I.regs.w[0]);
            nec.PUSH(nec.I.regs.w[1]);
            nec.PUSH(nec.I.regs.w[2]);
            nec.PUSH(nec.I.regs.w[3]);*/
            nec.PUSH((ushort)(nec.I.regs.b[0] + nec.I.regs.b[1] * 0x100));
            nec.PUSH((ushort)(nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100));
            nec.PUSH((ushort)(nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100));
            nec.PUSH((ushort)(nec.I.regs.b[6] + nec.I.regs.b[7] * 0x100));
            nec.PUSH(tmp);
            /*PUSH(nec.I.regs.w[5]);
            nec.PUSH(nec.I.regs.w[6]);
            nec.PUSH(nec.I.regs.w[7]);*/
            nec.PUSH((ushort)(nec.I.regs.b[10] + nec.I.regs.b[11] * 0x100));
            nec.PUSH((ushort)(nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100));
            nec.PUSH((ushort)(nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100));
            nec.CLKS(67, 35, 20);
        }
        public static void i_popa(this Nec nec)
        {
            ushort tmp = 0;
            /*POP(ref nec.I.regs.w[7]);
            nec.POP(ref nec.I.regs.w[6]);
            nec.POP(ref nec.I.regs.w[5]);*/
            nec.POPW(7);
            nec.POPW(6);
            nec.POPW(5);
            nec.POP(ref tmp);
            /*POP(ref nec.I.regs.w[3]);
            nec.POP(ref nec.I.regs.w[2]);
            nec.POP(ref nec.I.regs.w[1]);
            nec.POP(ref nec.I.regs.w[0]);*/
            nec.POPW(3);
            nec.POPW(2);
            nec.POPW(1);
            nec.POPW(0);
            nec.CLKS(75, 43, 22);
        }
        public static void i_chkind(this Nec nec)
        {
            int low, high, tmp;
            int ModRM;
            ModRM = nec.GetModRM();
            low = nec.GetRMWord(ModRM);
            high = nec.GetnextRMWord();
            tmp = nec.RegWord(ModRM);
            if (tmp < low || tmp > high)
            {
                nec.nec_interrupt(5, false);
            }
            nec.pendingCycles -= 20;
        }
        public static void i_brkn(this Nec nec)
        {
            nec.nec_interrupt(nec.FETCH(), true);
            nec.CLKS(50, 50, 24);
        }
        public static void i_repnc(this Nec nec)
        {
            int next = nec.fetchop();
            ushort c = (ushort)(nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100);// nec.I.regs.w[1];
            switch (next)
            { /* Segments */
                case 0x26: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[0] << 4); next = nec.fetchop(); nec.CLK(2); break;
                case 0x2e: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[1] << 4); next = nec.fetchop(); nec.CLK(2); break;
                case 0x36: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[2] << 4); next = nec.fetchop(); nec.CLK(2); break;
                case 0x3e: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[3] << 4); next = nec.fetchop(); nec.CLK(2); break;
            }
            switch (next)
            {
                case 0x6c: nec.CLK(2); if (c != 0) do { i_insb(nec); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); /*nec.I.regs.w[1] = c;*/ break;
                case 0x6d: nec.CLK(2); if (c != 0) do { i_insw(nec); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0x6e: nec.CLK(2); if (c != 0) do { i_outsb(nec); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0x6f: nec.CLK(2); if (c != 0) do { i_outsw(nec); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa4: nec.CLK(2); if (c != 0) do { i_movsb(nec); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa5: nec.CLK(2); if (c != 0) do { i_movsw(nec); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa6: nec.CLK(2); if (c != 0) do { i_cmpsb(nec); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa7: nec.CLK(2); if (c != 0) do { i_cmpsw(nec); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xaa: nec.CLK(2); if (c != 0) do { i_stosb(nec); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xab: nec.CLK(2); if (c != 0) do { i_stosw(nec); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xac: nec.CLK(2); if (c != 0) do { i_lodsb(nec); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xad: nec.CLK(2); if (c != 0) do { i_lodsw(nec); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xae: nec.CLK(2); if (c != 0) do { i_scasb(nec); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xaf: nec.CLK(2); if (c != 0) do { i_scasw(nec); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                default:
                    nec.nec_instruction[next](nec);
                    //DoInstructionOpCode(next);
                    break;
            }
            Nec.seg_prefix = 0;
        }
        public static void i_repc(this Nec nec)
        {
            int next = nec.fetchop();
            ushort c = (ushort)(nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100);// nec.I.regs.w[1];
            switch (next)
            { /* Segments */
                case 0x26: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[0] << 4); next = nec.fetchop(); nec.CLK(2); break;
                case 0x2e: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[1] << 4); next = nec.fetchop(); nec.CLK(2); break;
                case 0x36: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[2] << 4); next = nec.fetchop(); nec.CLK(2); break;
                case 0x3e: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[3] << 4); next = nec.fetchop(); nec.CLK(2); break;
            }
            switch (next)
            {
                case 0x6c: nec.CLK(2); if (c != 0) do { i_insb(nec); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100);/*nec.I.regs.w[1] = c;*/ break;
                case 0x6d: nec.CLK(2); if (c != 0) do { i_insw(nec); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0x6e: nec.CLK(2); if (c != 0) do { i_outsb(nec); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0x6f: nec.CLK(2); if (c != 0) do { i_outsw(nec); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa4: nec.CLK(2); if (c != 0) do { i_movsb(nec); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa5: nec.CLK(2); if (c != 0) do { i_movsw(nec); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa6: nec.CLK(2); if (c != 0) do { i_cmpsb(nec); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa7: nec.CLK(2); if (c != 0) do { i_cmpsw(nec); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xaa: nec.CLK(2); if (c != 0) do { i_stosb(nec); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xab: nec.CLK(2); if (c != 0) do { i_stosw(nec); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xac: nec.CLK(2); if (c != 0) do { i_lodsb(nec); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xad: nec.CLK(2); if (c != 0) do { i_lodsw(nec); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xae: nec.CLK(2); if (c != 0) do { i_scasb(nec); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xaf: nec.CLK(2); if (c != 0) do { i_scasw(nec); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                default:
                    nec.nec_instruction[next](nec);
                    //DoInstructionOpCode(next);
                    break;
            }
            Nec.seg_prefix = 0;
        }
        public static void i_push_d16(this Nec nec)
        {
            int tmp;
            tmp = nec.FETCHWORD();
            nec.PUSH((ushort)tmp);
            //nec.CLKW(12, 12, 5, 12, 8, 5, nec.I.regs.w[4]);
            nec.CLKW(12, 12, 5, 12, 8, 5, nec.I.regs.b[8] + nec.I.regs.b[9] * 0x100);
        }
        public static void i_imul_d16(this Nec nec)
        {
            int tmp;
            int ModRM;
            ushort src, dst;
            nec.DEF_r16w(out ModRM, out src, out dst);
            tmp = nec.FETCHWORD();
            dst = (ushort)((int)((short)src) * (int)((short)tmp));
            nec.I.CarryVal = nec.I.OverVal = (uint)(((((int)dst) >> 15 != 0) && (((int)dst) >> 15 != -1)) ? 1 : 0);
            //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = (ushort)dst;
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
            nec.pendingCycles -= (ModRM >= 0xc0) ? 38 : 47;
        }
        public static void i_push_d8(this Nec nec)
        {
            int tmp = (ushort)((short)((sbyte)nec.FETCH()));
            nec.PUSH((ushort)tmp);
            //nec.CLKW(11, 11, 5, 11, 7, 3, nec.I.regs.w[4]);
            nec.CLKW(11, 11, 5, 11, 7, 3, nec.I.regs.b[8] + nec.I.regs.b[9] * 0x100);
        }
        public static void i_imul_d8(this Nec nec)
        {
            int src2;
            int ModRM;
            ushort src, dst;
            nec.DEF_r16w(out ModRM, out src, out dst);
            src2 = (ushort)((short)((sbyte)nec.FETCH()));
            dst = (ushort)((int)((short)src) * (int)((short)src2));
            nec.I.CarryVal = nec.I.OverVal = (uint)(((((int)dst) >> 15 != 0) && (((int)dst) >> 15 != -1)) ? 1 : 0);
            //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = (ushort)dst;
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
            nec.pendingCycles -= (ModRM >= 0xc0) ? 31 : 39;
        }
        public static void i_insb(this Nec nec)
        {
            //PutMemB(0, nec.I.regs.w[7], ReadIOByte(nec.I.regs.w[2]));
            nec.PutMemB(0, nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100, nec.ReadIOByte(nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100));
            //nec.I.regs.w[7] += (ushort)(-2 * (nec.I.DF ? 1 : 0) + 1);
            ushort w7 = (ushort)(nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100);
            w7 += (ushort)(-2 * (nec.I.DF ? 1 : 0) + 1);
            nec.I.regs.b[14] = (byte)(w7 % 0x100);
            nec.I.regs.b[15] = (byte)(w7 / 0x100);
            nec.CLK(8);
        }
        public static void i_insw(this Nec nec)
        {
            //PutMemW(0, nec.I.regs.w[7], nec.ReadIOWord(nec.I.regs.w[2]));
            nec.PutMemW(0, nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100, nec.ReadIOWord(nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100));
            //nec.I.regs.w[7] += (ushort)(-4 * (nec.I.DF ? 1 : 0) + 2);
            ushort w7 = (ushort)(nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100);
            w7 += (ushort)(-4 * (nec.I.DF ? 1 : 0) + 2);
            nec.I.regs.b[14] = (byte)(w7 % 0x100);
            nec.I.regs.b[15] = (byte)(w7 / 0x100);
            nec.CLKS(18, 10, 8);
        }
        public static void i_outsb(this Nec nec)
        {
            //WriteIOByte(nec.I.regs.w[2], nec.GetMemB(3, nec.I.regs.w[6]));
            nec.WriteIOByte(nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100, nec.GetMemB(3, nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100));
            //nec.I.regs.w[6] += (ushort)(-2 * (nec.I.DF ? 1 : 0) + 1);
            ushort w6 = (ushort)(nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100);
            w6 += (ushort)(-2 * (nec.I.DF ? 1 : 0) + 1);
            nec.I.regs.b[12] = (byte)(w6 % 0x100);
            nec.I.regs.b[13] = (byte)(w6 / 0x100);
            nec.CLK(8);
        }
        public static void i_outsw(this Nec nec)
        {
            //WriteIOWord(nec.I.regs.w[2], nec.GetMemW(3, nec.I.regs.w[6]));
            nec.WriteIOWord(nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100, nec.GetMemW(3, nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100));
            //nec.I.regs.w[6] += (ushort)(-4 * (nec.I.DF ? 1 : 0) + 2);
            ushort w6 = (ushort)(nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100);
            w6 += (ushort)(-4 * (nec.I.DF ? 1 : 0) + 2);
            nec.I.regs.b[12] = (byte)(w6 % 0x100);
            nec.I.regs.b[13] = (byte)(w6 / 0x100);
            nec.CLKS(18, 10, 8);
        }
        public static void i_jo(this Nec nec)
        {
            bool b1 = nec.OF();
            nec.JMP(b1);
            if (!b1)
            {
                nec.CLKS(4, 4, 3);
            }
        }
        public static void i_jno(this Nec nec)
        {
            bool b1 = !nec.OF();
            nec.JMP(b1);
            if (!b1)
            {
                nec.CLKS(4, 4, 3);
            }
        }
        public static void i_jc(this Nec nec)
        {
            bool b1 = nec.CF();
            nec.JMP(b1);
            if (!b1)
            {
                nec.CLKS(4, 4, 3);
            }
        }
        public static void i_jnc(this Nec nec)
        {
            bool b1 = !nec.CF();
            nec.JMP(b1);
            if (!b1)
            {
                nec.CLKS(4, 4, 3);
            }
        }
        public static void i_jz(this Nec nec)
        {
            bool b1 = nec.ZF();
            nec.JMP(b1);
            if (!b1)
            {
                nec.CLKS(4, 4, 3);
            }
        }
        public static void i_jnz(this Nec nec)
        {
            bool b1 = !nec.ZF();
            nec.JMP(b1);
            if (!b1)
            {
                nec.CLKS(4, 4, 3);
            }
        }
        public static void i_jce(this Nec nec)
        {
            bool b1 = nec.CF() || nec.ZF();
            nec.JMP(b1);
            if (!b1)
            {
                nec.CLKS(4, 4, 3);
            }
        }
        public static void i_jnce(this Nec nec)
        {
            bool b1 = !(nec.CF() || nec.ZF());
            nec.JMP(b1);
            if (!b1)
            {
                nec.CLKS(4, 4, 3);
            }
        }
        public static void i_js(this Nec nec)
        {
            bool b1 = nec.SF();
            nec.JMP(b1);
            if (!b1)
            {
                nec.CLKS(4, 4, 3);
            }
        }
        public static void i_jns(this Nec nec)
        {
            bool b1 = !nec.SF();
            nec.JMP(b1);
            if (!b1)
            {
                nec.CLKS(4, 4, 3);
            }
        }
        public static void i_jp(this Nec nec)
        {
            bool b1 = nec.PF();
            nec.JMP(b1);
            if (!b1)
            {
                nec.CLKS(4, 4, 3);
            }
        }
        public static void i_jnp(this Nec nec)
        {
            bool b1 = !nec.PF();
            nec.JMP(b1);
            if (!b1)
            {
                nec.CLKS(4, 4, 3);
            }
        }
        public static void i_jl(this Nec nec)
        {
            bool b1 = (nec.SF() != nec.OF()) && (!nec.ZF());
            nec.JMP(b1);
            if (!b1)
            {
                nec.CLKS(4, 4, 3);
            }
        }
        public static void i_jnl(this Nec nec)
        {
            bool b1 = (nec.ZF()) || (nec.SF() == nec.OF());
            nec.JMP(b1);
            if (!b1)
            {
                nec.CLKS(4, 4, 3);
            }
        }
        public static void i_jle(this Nec nec)
        {
            bool b1 = (nec.ZF()) || (nec.SF() != nec.OF());
            nec.JMP(b1);
            if (!b1)
            {
                nec.CLKS(4, 4, 3);
            }
        }
        public static void i_jnle(this Nec nec)
        {
            bool b1 = (nec.SF() == nec.OF()) && (!nec.ZF());
            nec.JMP(b1);
            if (!b1)
            {
                nec.CLKS(4, 4, 3);
            }
        }
        public static void i_80pre(this Nec nec)
        {
            int ModRM;
            byte src, dst;
            ModRM = nec.GetModRM();
            dst = nec.GetRMByte(ModRM);
            src = nec.FETCH();
            if (ModRM >= 0xc0)
            {
                nec.CLKS(4, 4, 2);
            }
            else if ((ModRM & 0x38) == 0x38)
            {
                nec.CLKS(13, 13, 6);
            }
            else
            {
                nec.CLKS(18, 18, 7);
            }
            switch (ModRM & 0x38)
            {
                case 0x00: nec.ADDB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
                case 0x08: nec.ORB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
                case 0x10: src += (byte)(nec.CF() ? 1 : 0); nec.ADDB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
                case 0x18: src += (byte)(nec.CF() ? 1 : 0); nec.SUBB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
                case 0x20: nec.ANDB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
                case 0x28: nec.SUBB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
                case 0x30: nec.XORB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
                case 0x38: nec.SUBB(ref src, ref dst); break;
            }
        }
        public static void i_81pre(this Nec nec)
        {
            int ModRM;
            ushort src, dst;
            ModRM = nec.GetModRM();
            dst = nec.GetRMWord(ModRM);
            src = nec.FETCH();
            src += (ushort)(nec.FETCH() << 8);
            if (ModRM >= 0xc0)
            {
                nec.CLKS(4, 4, 2);
            }
            else if ((ModRM & 0x38) == 0x38)
            {
                nec.CLKW(17, 17, 8, 17, 13, 6, Nec.EA);
            }
            else
            {
                nec.CLKW(26, 26, 11, 26, 18, 7, Nec.EA);
            }
            switch (ModRM & 0x38)
            {
                case 0x00: nec.ADDW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
                case 0x08: nec.ORW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
                case 0x10: src += (ushort)(nec.CF() ? 1 : 0); nec.ADDW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
                case 0x18: src += (ushort)(nec.CF() ? 1 : 0); nec.SUBW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
                case 0x20: nec.ANDW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
                case 0x28: nec.SUBW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
                case 0x30: nec.XORW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
                case 0x38: nec.SUBW(ref src, ref dst); break;
            }
        }
        public static void i_82pre(this Nec nec)
        {
            int ModRM;
            byte src, dst;
            ModRM = nec.GetModRM();
            dst = nec.GetRMByte(ModRM);
            src = (byte)((sbyte)nec.FETCH());
            if (ModRM >= 0xc0)
            {
                nec.CLKS(4, 4, 2);
            }
            else if ((ModRM & 0x38) == 0x38)
            {
                nec.CLKS(13, 13, 6);
            }
            else
            {
                nec.CLKS(18, 18, 7);
            }
            switch (ModRM & 0x38)
            {
                case 0x00: nec.ADDB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
                case 0x08: nec.ORB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
                case 0x10: src += (byte)(nec.CF() ? 1 : 0); nec.ADDB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
                case 0x18: src += (byte)(nec.CF() ? 1 : 0); nec.SUBB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
                case 0x20: nec.ANDB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
                case 0x28: nec.SUBB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
                case 0x30: nec.XORB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
                case 0x38: nec.SUBB(ref src, ref dst); break;
            }
        }
        public static void i_83pre(this Nec nec)
        {
            int ModRM;
            ushort src, dst;
            ModRM = nec.GetModRM();
            dst = nec.GetRMWord(ModRM);
            src = (ushort)((short)((sbyte)nec.FETCH()));
            if (ModRM >= 0xc0)
            {
                nec.CLKS(4, 4, 2);
            }
            else if ((ModRM & 0x38) == 0x38)
            {
                nec.CLKW(17, 17, 8, 17, 13, 6, Nec.EA);
            }
            else
            {
                nec.CLKW(26, 26, 11, 26, 18, 7, Nec.EA);
            }
            switch (ModRM & 0x38)
            {
                case 0x00: nec.ADDW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
                case 0x08: nec.ORW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
                case 0x10: src += (ushort)(nec.CF() ? 1 : 0); nec.ADDW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
                case 0x18: src += (ushort)(nec.CF() ? 1 : 0); nec.SUBW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
                case 0x20: nec.ANDW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
                case 0x28: nec.SUBW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
                case 0x30: nec.XORW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
                case 0x38: nec.SUBW(ref src, ref dst); break;
            }
        }
        public static void i_test_br8(this Nec nec)
        {
            int ModRM;
            byte src, dst;
            nec.DEF_br8(out ModRM, out src, out dst);
            nec.ANDB(ref src, ref dst);
            nec.CLKM(ModRM, 2, 2, 2, 10, 10, 6);
        }
        public static void i_test_wr16(this Nec nec)
        {
            int ModRM;
            ushort src, dst;
            nec.DEF_wr16(out ModRM, out src, out dst);
            nec.ANDW(ref src, ref dst);
            nec.CLKR(ModRM, 14, 14, 8, 14, 10, 6, 2, Nec.EA);
        }
        public static void i_xchg_br8(this Nec nec)
        {
            int ModRM;
            byte src, dst;
            nec.DEF_br8(out ModRM, out src, out dst);
            nec.I.regs.b[nec.mod_RM.regb[ModRM]] = dst;
            nec.PutbackRMByte(ModRM, src);
            nec.CLKM(ModRM, 3, 3, 3, 16, 18, 8);
        }
        public static void i_xchg_wr16(this Nec nec)
        {
            int ModRM;
            ushort src, dst;
            nec.DEF_wr16(out ModRM, out src, out dst);
            //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = dst;
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
            nec.PutbackRMWord(ModRM, src);
            nec.CLKR(ModRM, 24, 24, 12, 24, 16, 8, 3, Nec.EA);
        }
        public static void i_mov_br8(this Nec nec)
        {
            int ModRM;
            byte src;
            ModRM = nec.GetModRM();
            src = nec.I.regs.b[nec.mod_RM.regb[ModRM]];
            nec.PutRMByte(ModRM, src);
            nec.CLKM(ModRM, 2, 2, 2, 9, 9, 3);
        }
        public static void i_mov_wr16(this Nec nec)
        {
            int ModRM;
            ushort src;
            ModRM = nec.GetModRM();
            //src = nec.I.regs.w[nec.mod_RM.regw[ModRM]];
            src = (ushort)(nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] + nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] * 0x100);
            nec.PutRMWord(ModRM, src);
            nec.CLKR(ModRM, 13, 13, 5, 13, 9, 3, 2, Nec.EA);
        }
        public static void i_mov_r8b(this Nec nec)
        {
            int ModRM;
            byte src;
            ModRM = nec.GetModRM();
            src = nec.GetRMByte(ModRM);
            nec.I.regs.b[nec.mod_RM.regb[ModRM]] = src;
            nec.CLKM(ModRM, 2, 2, 2, 11, 11, 5);
        }
        public static void i_mov_r16w(this Nec nec)
        {
            int ModRM;
            ushort src;
            ModRM = nec.GetModRM();
            src = nec.GetRMWord(ModRM);
            //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = src;
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(src % 0x100);
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(src / 0x100);
            nec.CLKR(ModRM, 15, 15, 7, 15, 11, 5, 2, Nec.EA);
        }
        public static void i_mov_wsreg(this Nec nec)
        {
            int ModRM;
            ModRM = nec.GetModRM();
            nec.PutRMWord(ModRM, nec.I.sregs[(ModRM & 0x38) >> 3]);
            nec.CLKR(ModRM, 14, 14, 5, 14, 10, 3, 2, Nec.EA);
        }
        public static void i_lea(this Nec nec)
        {
            int ModRM = nec.FETCH();
            nec.GetEA[ModRM]();
            //DoNecnec.GetEAOpCode(ModRM);


            //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = EO;
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(Nec.EO % 0x100);
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(Nec.EO / 0x100);
            nec.CLKS(4, 4, 2);
        }
        public static void i_mov_sregw(this Nec nec)
        {
            int ModRM;
            ushort src;
            ModRM = nec.GetModRM();
            src = nec.GetRMWord(ModRM);
            nec.CLKR(ModRM, 15, 15, 7, 15, 11, 5, 2, Nec.EA);
            switch (ModRM & 0x38)
            {
                case 0x00: nec.I.sregs[0] = src; break; /* mov es,ew */
                case 0x08: nec.I.sregs[1] = src; break; /* mov cs,ew */
                case 0x10: nec.I.sregs[2] = src; break; /* mov ss,ew */
                case 0x18: nec.I.sregs[3] = src; break; /* mov ds,ew */
                default: break;
            }
            nec.I.no_interrupt = 1;
        }
        public static void i_popw(this Nec nec)
        {
            int ModRM;
            ushort tmp = 0;
            ModRM = nec.GetModRM();
            nec.POP(ref tmp);
            nec.PutRMWord(ModRM, tmp);
            nec.pendingCycles -= 21;
        }
        public static void i_nop(this Nec nec)
        {
            nec.CLK(3);
            if (nec.I.no_interrupt == 0 && nec.pendingCycles > 0 && (nec.I.pending_irq == 0) && (nec.PEEKOP((uint)((nec.I.sregs[1] << 4) + nec.I.ip))) == 0xeb && (nec.PEEK((uint)((nec.I.sregs[1] << 4) + nec.I.ip + 1))) == 0xfd)
                nec.pendingCycles %= 15;
        }
        public static void i_xchg_axcx(this Nec nec)
        {
            nec.XchgAWReg(1);
            nec.CLK(3);
        }
        public static void i_xchg_axdx(this Nec nec)
        {
            nec.XchgAWReg(2);
            nec.CLK(3);
        }
        public static void i_xchg_axbx(this Nec nec)
        {
            nec.XchgAWReg(3);
            nec.CLK(3);
        }
        public static void i_xchg_axsp(this Nec nec)
        {
            nec.XchgAWReg(4);
            nec.CLK(3);
        }
        public static void i_xchg_axbp(this Nec nec)
        {
            nec.XchgAWReg(5);
            nec.CLK(3);
        }
        public static void i_xchg_axsi(this Nec nec)
        {
            nec.XchgAWReg(6);
            nec.CLK(3);
        }
        public static void i_xchg_axdi(this Nec nec)
        {
            nec.XchgAWReg(7);
            nec.CLK(3);
        }
        public static void i_cbw(this Nec nec)
        {
            nec.I.regs.b[1] = (byte)(((nec.I.regs.b[0] & 0x80) != 0) ? 0xff : 0);
            nec.CLK(2);
        }
        public static void i_cwd(this Nec nec)
        {
            //nec.I.regs.w[2] = (ushort)(((nec.I.regs.b[1] & 0x80) != 0) ? 0xffff : 0);
            ushort w2 = (ushort)(((nec.I.regs.b[1] & 0x80) != 0) ? 0xffff : 0);
            nec.I.regs.b[4] = (byte)(w2 % 0x100);
            nec.I.regs.b[5] = (byte)(w2 / 0x100);
            nec.CLK(4);
        }
        public static void i_call_far(this Nec nec)
        {
            ushort tmp, tmp2;
            tmp = nec.FETCHWORD();
            tmp2 = nec.FETCHWORD();
            nec.PUSH(nec.I.sregs[1]);
            nec.PUSH(nec.I.ip);
            nec.I.ip = (ushort)tmp;
            nec.I.sregs[1] = (ushort)tmp2;
            //CHANGE_PC;
            nec.CLKW(29, 29, 13, 29, 21, 9, nec.I.regs.b[8] + nec.I.regs.b[9] * 0x100);
        }
        public static void i_wait(this Nec nec)
        {
            if (!nec.I.poll_state)
            {
                nec.I.ip--;
            }
            nec.CLK(5);
        }
        public static void i_pushf(this Nec nec)
        {
            ushort tmp = nec.CompressFlags();
            nec.PUSH(tmp);
            nec.CLKS(12, 8, 3);
        }
        public static void i_popf(this Nec nec)
        {
            ushort tmp = 0;
            nec.POP(ref tmp);
            nec.ExpandFlags(tmp);
            nec.CLKS(12, 8, 5);
            if (nec.I.TF)
            {
                nec.nec_trap();
            }
        }
        public static void i_sahf(this Nec nec)
        {
            ushort tmp = (ushort)((nec.CompressFlags() & 0xff00) | (nec.I.regs.b[1] & 0xd5));
            nec.ExpandFlags(tmp);
            nec.CLKS(3, 3, 2);
        }
        public static void i_lahf(this Nec nec)
        {
            nec.I.regs.b[1] = (byte)(nec.CompressFlags() & 0xff);
            nec.CLKS(3, 3, 2);
        }
        public static void i_mov_aldisp(this Nec nec)
        {
            ushort addr;
            addr = nec.FETCHWORD();
            nec.I.regs.b[0] = nec.GetMemB(3, addr);
            nec.CLKS(10, 10, 5);
        }
        public static void i_mov_axdisp(this Nec nec)
        {
            ushort addr;
            addr = nec.FETCHWORD();
            //nec.I.regs.w[0] = nec.GetMemW(3, addr);
            ushort w0 = nec.GetMemW(3, addr);
            nec.I.regs.b[0] = (byte)(w0 % 0x100);
            nec.I.regs.b[1] = (byte)(w0 / 0x100);
            nec.CLKW(14, 14, 7, 14, 10, 5, addr);
        }
        public static void i_mov_dispal(this Nec nec)
        {
            ushort addr;
            addr = nec.FETCHWORD();
            nec.PutMemB(3, addr, nec.I.regs.b[0]);
            nec.CLKS(9, 9, 3);
        }
        public static void i_mov_dispax(this Nec nec)
        {
            ushort addr;
            addr = nec.FETCHWORD();
            nec.PutMemW(3, addr, (ushort)(nec.I.regs.b[0] + nec.I.regs.b[1] * 0x100));
            nec.CLKW(13, 13, 5, 13, 9, 3, addr);
        }
        public static void i_movsb(this Nec nec)
        {
            byte tmp = nec.GetMemB(3, nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100);
            nec.PutMemB(0, nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100, tmp);
            //nec.I.regs.w[7] += (ushort)(-2 * (nec.I.DF ? 1 : 0) + 1);
            //nec.I.regs.w[6] += (ushort)(-2 * (nec.I.DF ? 1 : 0) + 1);
            ushort w7 = (ushort)(nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100 + (-2 * (nec.I.DF ? 1 : 0) + 1));
            nec.I.regs.b[14] = (byte)(w7 % 0x100);
            nec.I.regs.b[15] = (byte)(w7 / 0x100);
            ushort w6 = (ushort)(nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100 + (-2 * (nec.I.DF ? 1 : 0) + 1));
            nec.I.regs.b[12] = (byte)(w6 % 0x100);
            nec.I.regs.b[13] = (byte)(w6 / 0x100);
            nec.CLKS(8, 8, 6);
        }
        public static void i_movsw(this Nec nec)
        {
            ushort tmp = nec.GetMemW(3, nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100);
            nec.PutMemW(0, nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100, tmp);
            //nec.I.regs.w[7] += (ushort)(-4 * (nec.I.DF ? 1 : 0) + 2);
            //nec.I.regs.w[6] += (ushort)(-4 * (nec.I.DF ? 1 : 0) + 2);
            ushort w7 = (ushort)(nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100 + (-4 * (nec.I.DF ? 1 : 0) + 2));
            nec.I.regs.b[14] = (byte)(w7 % 0x100);
            nec.I.regs.b[15] = (byte)(w7 / 0x100);
            ushort w6 = (ushort)(nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100 + (-4 * (nec.I.DF ? 1 : 0) + 2));
            nec.I.regs.b[12] = (byte)(w6 % 0x100);
            nec.I.regs.b[13] = (byte)(w6 / 0x100);
            nec.CLKS(16, 16, 10);
        }
        public static void i_cmpsb(this Nec nec)
        {
            byte src = nec.GetMemB(0, nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100);
            byte dst = nec.GetMemB(3, nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100);
            nec.SUBB(ref src, ref dst);
            //nec.I.regs.w[7] += (ushort)(-2 * (nec.I.DF ? 1 : 0) + 1);
            //nec.I.regs.w[6] += (ushort)(-2 * (nec.I.DF ? 1 : 0) + 1);
            ushort w7 = (ushort)(nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100 + (-2 * (nec.I.DF ? 1 : 0) + 1));
            nec.I.regs.b[14] = (byte)(w7 % 0x100);
            nec.I.regs.b[15] = (byte)(w7 / 0x100);
            ushort w6 = (ushort)(nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100 + (-2 * (nec.I.DF ? 1 : 0) + 1));
            nec.I.regs.b[12] = (byte)(w6 % 0x100);
            nec.I.regs.b[13] = (byte)(w6 / 0x100);
            nec.CLKS(14, 14, 14);
        }
        public static void i_cmpsw(this Nec nec)
        {
            ushort src = nec.GetMemW(0, nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100);
            ushort dst = nec.GetMemW(3, nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100);
            nec.SUBW(ref src, ref dst);
            //nec.I.regs.w[7] += (ushort)(-4 * (nec.I.DF ? 1 : 0) + 2);
            //nec.I.regs.w[6] += (ushort)(-4 * (nec.I.DF ? 1 : 0) + 2);
            ushort w7 = (ushort)(nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100 + (-4 * (nec.I.DF ? 1 : 0) + 2));
            nec.I.regs.b[14] = (byte)(w7 % 0x100);
            nec.I.regs.b[15] = (byte)(w7 / 0x100);
            ushort w6 = (ushort)(nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100 + (-4 * (nec.I.DF ? 1 : 0) + 2));
            nec.I.regs.b[12] = (byte)(w6 % 0x100);
            nec.I.regs.b[13] = (byte)(w6 / 0x100);
            nec.CLKS(14, 14, 14);
        }
        public static void i_test_ald8(this Nec nec)
        {
            byte src, dst;
            nec.DEF_ald8(out src, out dst);
            nec.ANDB(ref src, ref dst);
            nec.CLKS(4, 4, 2);
        }
        public static void i_test_axd16(this Nec nec)
        {
            ushort src, dst;
            nec.DEF_axd16(out src, out dst);
            nec.ANDW(ref src, ref dst);
            nec.CLKS(4, 4, 2);
        }
        public static void i_stosb(this Nec nec)
        {
            nec.PutMemB(0, nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100, nec.I.regs.b[0]);
            //nec.I.regs.w[7] += (ushort)(-2 * (nec.I.DF ? 1 : 0) + 1);
            ushort w7 = (ushort)(nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100 + (-2 * (nec.I.DF ? 1 : 0) + 1));
            nec.I.regs.b[14] = (byte)(w7 % 0x100);
            nec.I.regs.b[15] = (byte)(w7 / 0x100);
            nec.CLKS(4, 4, 3);
        }
        public static void i_stosw(this Nec nec)
        {
            nec.PutMemW(0, nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100, (ushort)(nec.I.regs.b[0] + nec.I.regs.b[1] * 0x100));
            //nec.I.regs.w[7] += (ushort)(-4 * (nec.I.DF ? 1 : 0) + 2);
            ushort w7 = (ushort)(nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100 + (-4 * (nec.I.DF ? 1 : 0) + 2));
            nec.I.regs.b[14] = (byte)(w7 % 0x100);
            nec.I.regs.b[15] = (byte)(w7 / 0x100);
            nec.CLKW(8, 8, 5, 8, 4, 3, nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100);
        }
        public static void i_lodsb(this Nec nec)
        {
            nec.I.regs.b[0] = nec.GetMemB(3, nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100);
            //nec.I.regs.w[6] += (ushort)(-2 * (nec.I.DF ? 1 : 0) + 1);
            ushort w6 = (ushort)(nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100 + (-2 * (nec.I.DF ? 1 : 0) + 1));
            nec.I.regs.b[12] = (byte)(w6 % 0x100);
            nec.I.regs.b[13] = (byte)(w6 / 0x100);
            nec.CLKS(4, 4, 3);
        }
        public static void i_lodsw(this Nec nec)
        {
            ushort w0 = nec.GetMemW(3, nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100);
            nec.I.regs.b[0] = (byte)(w0 % 0x100);
            nec.I.regs.b[1] = (byte)(w0 / 0x100);
            //nec.I.regs.w[0] = nec.GetMemW(3, nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100);
            //nec.I.regs.w[6] += (ushort)(-4 * (nec.I.DF ? 1 : 0) + 2);
            ushort w6 = (ushort)(nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100 + (-4 * (nec.I.DF ? 1 : 0) + 2));
            nec.I.regs.b[12] = (byte)(w6 % 0x100);
            nec.I.regs.b[13] = (byte)(w6 / 0x100);
            nec.CLKW(8, 8, 5, 8, 4, 3, nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100);
        }
        public static void i_scasb(this Nec nec)
        {
            byte src = nec.GetMemB(0, nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100);
            byte dst = nec.I.regs.b[0];
            nec.SUBB(ref src, ref dst);
            //nec.I.regs.w[7] += (ushort)(-2 * (nec.I.DF ? 1 : 0) + 1);
            ushort w7 = (ushort)(nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100 + (-2 * (nec.I.DF ? 1 : 0) + 1));
            nec.I.regs.b[14] = (byte)(w7 % 0x100);
            nec.I.regs.b[15] = (byte)(w7 / 0x100);
            nec.CLKS(4, 4, 3);
        }
        public static void i_scasw(this Nec nec)
        {
            ushort src = nec.GetMemW(0, nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100);
            ushort dst = (ushort)(nec.I.regs.b[0] + nec.I.regs.b[1] * 0x100);
            nec.SUBW(ref src, ref dst);
            //nec.I.regs.w[7] += (ushort)(-4 * (nec.I.DF ? 1 : 0) + 2);
            ushort w7 = (ushort)(nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100 + (-4 * (nec.I.DF ? 1 : 0) + 2));
            nec.I.regs.b[14] = (byte)(w7 % 0x100);
            nec.I.regs.b[15] = (byte)(w7 / 0x100);
            nec.CLKW(8, 8, 5, 8, 4, 3, nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100);
        }
        public static void i_mov_ald8(this Nec nec)
        {
            nec.I.regs.b[0] = nec.FETCH();
            nec.CLKS(4, 4, 2);
        }
        public static void i_mov_cld8(this Nec nec)
        {
            nec.I.regs.b[2] = nec.FETCH();
            nec.CLKS(4, 4, 2);
        }
        public static void i_mov_dld8(this Nec nec)
        {
            nec.I.regs.b[4] = nec.FETCH();
            nec.CLKS(4, 4, 2);
        }
        public static void i_mov_bld8(this Nec nec)
        {
            nec.I.regs.b[6] = nec.FETCH();
            nec.CLKS(4, 4, 2);
        }
        public static void i_mov_ahd8(this Nec nec)
        {
            nec.I.regs.b[1] = nec.FETCH();
            nec.CLKS(4, 4, 2);
        }
        public static void i_mov_chd8(this Nec nec)
        {
            nec.I.regs.b[3] = nec.FETCH();
            nec.CLKS(4, 4, 2);
        }
        public static void i_mov_dhd8(this Nec nec)
        {
            nec.I.regs.b[5] = nec.FETCH();
            nec.CLKS(4, 4, 2);
        }
        public static void i_mov_bhd8(this Nec nec)
        {
            nec.I.regs.b[7] = nec.FETCH();
            nec.CLKS(4, 4, 2);
        }
        public static void i_mov_axd16(this Nec nec)
        {
            nec.I.regs.b[0] = nec.FETCH();
            nec.I.regs.b[1] = nec.FETCH();
            nec.CLKS(4, 4, 2);
        }
        public static void i_mov_cxd16(this Nec nec)
        {
            nec.I.regs.b[2] = nec.FETCH();
            nec.I.regs.b[3] = nec.FETCH();
            nec.CLKS(4, 4, 2);
        }
        public static void i_mov_dxd16(this Nec nec)
        {
            nec.I.regs.b[4] = nec.FETCH();
            nec.I.regs.b[5] = nec.FETCH();
            nec.CLKS(4, 4, 2);
        }
        public static void i_mov_bxd16(this Nec nec)
        {
            nec.I.regs.b[6] = nec.FETCH();
            nec.I.regs.b[7] = nec.FETCH();
            nec.CLKS(4, 4, 2);
        }
        public static void i_mov_spd16(this Nec nec)
        {
            nec.I.regs.b[8] = nec.FETCH();
            nec.I.regs.b[9] = nec.FETCH();
            nec.CLKS(4, 4, 2);
        }
        public static void i_mov_bpd16(this Nec nec)
        {
            nec.I.regs.b[10] = nec.FETCH();
            nec.I.regs.b[11] = nec.FETCH();
            nec.CLKS(4, 4, 2);
        }
        public static void i_mov_sid16(this Nec nec)
        {
            nec.I.regs.b[12] = nec.FETCH();
            nec.I.regs.b[13] = nec.FETCH();
            nec.CLKS(4, 4, 2);
        }
        public static void i_mov_did16(this Nec nec)
        {
            nec.I.regs.b[14] = nec.FETCH();
            nec.I.regs.b[15] = nec.FETCH();
            nec.CLKS(4, 4, 2);
        }
        public static void i_rotshft_bd8(this Nec nec)
        {
            int ModRM;
            int src, dst;
            byte c;
            ModRM = nec.GetModRM();
            src = nec.GetRMByte(ModRM);
            dst = src;
            c = nec.FETCH();
            nec.CLKM(ModRM, 7, 7, 2, 19, 19, 6);
            if (c != 0)
            {
                switch (ModRM & 0x38)
                {
                    case 0x00: do {  nec.ROL_BYTE(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMByte(ModRM, (byte)dst); break;
                    case 0x08: do { nec.ROR_BYTE(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMByte(ModRM, (byte)dst); break;
                    case 0x10: do { nec.ROLC_BYTE(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMByte(ModRM, (byte)dst); break;
                    case 0x18: do { nec.RORC_BYTE(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMByte(ModRM, (byte)dst); break;
                    case 0x20: nec.SHL_BYTE(c, ref dst, ModRM); break;
                    case 0x28: nec.SHR_BYTE(c, ref dst, ModRM); break;
                    case 0x30: break;
                    case 0x38: nec.SHRA_BYTE(c, ref dst, ModRM); break;
                }
            }
        }
        public static void i_rotshft_wd8(this Nec nec)
        {
            int ModRM;
            int src, dst;
            byte c;
            ModRM = nec.GetModRM();
            src = nec.GetRMWord(ModRM);
            dst = src;
            c = nec.FETCH();
            nec.CLKM(ModRM, 7, 7, 2, 27, 19, 6);
            if (c != 0)
            {
                switch (ModRM & 0x38)
                {
                    case 0x00: do { nec.ROL_WORD(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMWord(ModRM, (ushort)dst); break;
                    case 0x08: do { nec.ROR_WORD(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMWord(ModRM, (ushort)dst); break;
                    case 0x10: do { nec.ROLC_WORD(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMWord(ModRM, (ushort)dst); break;
                    case 0x18: do { nec.RORC_WORD(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMWord(ModRM, (ushort)dst); break;
                    case 0x20: nec.SHL_WORD(c, ref dst, ModRM); break;
                    case 0x28: nec.SHR_WORD(c, ref dst, ModRM); break;
                    case 0x30: break;
                    case 0x38: nec.SHRA_WORD(c, ref dst, ModRM); break;
                }
            }
        }
        public static void i_ret_d16(this Nec nec)
        {
            ushort count = nec.FETCH();
            count += (ushort)(nec.FETCH() << 8);
            nec.POP(ref nec.I.ip);
            //nec.I.regs.w[4] += count;
            ushort w4 = (ushort)(nec.I.regs.b[8] + nec.I.regs.b[9] * 0x100 + count);
            nec.I.regs.b[8] = (byte)(w4 % 0x100);
            nec.I.regs.b[9] = (byte)(w4 / 0x100);
            //CHANGE_PC;
            nec.CLKS(24, 24, 10);
        }
        public static void i_ret(this Nec nec)
        {
            nec.POP(ref nec.I.ip);
            //CHANGE_PC;
            nec.CLKS(19, 19, 10);
        }
        public static void i_les_dw(this Nec nec)
        {
            int ModRM;
            ModRM = nec.GetModRM();
            ushort tmp = nec.GetRMWord(ModRM);
            //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = tmp;
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(tmp % 0x100);
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(tmp / 0x100);
            nec.I.sregs[0] = nec.GetnextRMWord();
            nec.CLKW(26, 26, 14, 26, 18, 10, Nec.EA);
        }
        public static void i_lds_dw(this Nec nec)
        {
            int ModRM;
            ModRM = nec.GetModRM();
            ushort tmp = nec.GetRMWord(ModRM);
            //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = tmp;
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(tmp % 0x100);
            nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(tmp / 0x100);
            nec.I.sregs[3] = nec.GetnextRMWord();
            nec.CLKW(26, 26, 14, 26, 18, 10, Nec.EA);
        }
        public static void i_mov_bd8(this Nec nec)
        {
            int ModRM;
            ModRM = nec.GetModRM();
            nec.PutImmRMByte(ModRM);
            nec.pendingCycles -= (ModRM >= 0xc0) ? 4 : 11;
        }
        public static void i_mov_wd16(this Nec nec)
        {
            int ModRM;
            ModRM = nec.GetModRM();
            nec.PutImmRMWord(ModRM);
            nec.pendingCycles -= (ModRM >= 0xc0) ? 4 : 15;
        }
        public static void i_enter(this Nec nec)
        {
            ushort nb = nec.FETCH();
            int i, level;
            nec.pendingCycles -= 23;
            nb += (ushort)(nec.FETCH() << 8);
            level = nec.FETCH();
            nec.PUSH((ushort)(nec.I.regs.b[10] + nec.I.regs.b[11] * 0x100));
            //nec.I.regs.w[5] = nec.I.regs.w[4];
            nec.I.regs.b[10] = nec.I.regs.b[8];
            nec.I.regs.b[11] = nec.I.regs.b[9];
            //nec.I.regs.w[4] -= nb;
            ushort w4 = (ushort)(nec.I.regs.b[8] + nec.I.regs.b[9] * 0x100 - nb);
            nec.I.regs.b[8] = (byte)(w4 % 0x100);
            nec.I.regs.b[9] = (byte)(w4 / 0x100);
            for (i = 1; i < level; i++)
            {
                nec.PUSH(nec.GetMemW(2, nec.I.regs.b[10] + nec.I.regs.b[11] * 0x100 - i * 2));
                nec.pendingCycles -= 16;
            }
            if (level != 0)
            {
                nec.PUSH((ushort)(nec.I.regs.b[10] + nec.I.regs.b[11] * 0x100));
            }
        }
        public static void i_leave(this Nec nec)
        {
            //nec.I.regs.w[4] = nec.I.regs.w[5];
            nec.I.regs.b[8] = nec.I.regs.b[10];
            nec.I.regs.b[9] = nec.I.regs.b[11];
            //POP(ref nec.I.regs.w[5]);
            nec.POPW(5);
            nec.pendingCycles -= 8;
        }
        public static void i_retf_d16(this Nec nec)
        {
            ushort count = nec.FETCH();
            count += (ushort)(nec.FETCH() << 8);
            nec.POP(ref nec.I.ip);
            nec.POP(ref nec.I.sregs[1]);
            //nec.I.regs.w[4] += count;
            ushort w4 = (ushort)(nec.I.regs.b[8] + nec.I.regs.b[9] * 0x100 + count);
            nec.I.regs.b[8] = (byte)(w4 % 0x100);
            nec.I.regs.b[9] = (byte)(w4 / 0x100);
            //CHANGE_PC;
            nec.CLKS(32, 32, 16);
        }
        public static void i_retf(this Nec nec)
        {
            nec.POP(ref nec.I.ip);
            nec.POP(ref nec.I.sregs[1]);
            //CHANGE_PC;
            nec.CLKS(29, 29, 16);
        }
        public static void i_int3(this Nec nec)
        {
            nec.nec_interrupt(3, false);
            nec.CLKS(50, 50, 24);
        }
        public static void i_int(this Nec nec)
        {
            nec.nec_interrupt(nec.FETCH(), false);
            nec.CLKS(50, 50, 24);
        }
        public static void i_into(this Nec nec)
        {
            if (nec.OF())
            {
                nec.nec_interrupt(4, false);
                nec.CLKS(52, 52, 26);
            }
            else
            {
                nec.CLK(3);
            }
        }
        public static void i_iret(this Nec nec)
        {
            nec.POP(ref nec.I.ip);
            nec.POP(ref nec.I.sregs[1]);
            nec.i_popf();
            nec.I.MF = true;
            //CHANGE_PC;
            nec.CLKS(39, 39, 19);
        }
        public static void i_rotshft_b(this Nec nec)
        {
            int ModRM;
            int src, dst;
            ModRM = nec.GetModRM();
            src = nec.GetRMByte(ModRM);
            dst = src;
            nec.CLKM(ModRM, 6, 6, 2, 16, 16, 7);
            switch (ModRM & 0x38)
            {
                case 0x00:  nec.ROL_BYTE(ref dst); nec.PutbackRMByte(ModRM, (byte)dst); nec.I.OverVal = (uint)((src ^ dst) & 0x80); break;
                case 0x08: nec.ROR_BYTE(ref dst); nec.PutbackRMByte(ModRM, (byte)dst); nec.I.OverVal = (uint)((src ^ dst) & 0x80); break;
                case 0x10: nec.ROLC_BYTE(ref dst); nec.PutbackRMByte(ModRM, (byte)dst); nec.I.OverVal = (uint)((src ^ dst) & 0x80); break;
                case 0x18: nec.RORC_BYTE(ref dst); nec.PutbackRMByte(ModRM, (byte)dst); nec.I.OverVal = (uint)((src ^ dst) & 0x80); break;
                case 0x20: nec.SHL_BYTE(1, ref dst, ModRM); nec.I.OverVal = (uint)((src ^ dst) & 0x80); break;
                case 0x28: nec.SHR_BYTE(1, ref dst, ModRM); nec.I.OverVal = (uint)((src ^ dst) & 0x80); break;
                case 0x30: break;
                case 0x38: nec.SHRA_BYTE(1, ref dst, ModRM); nec.I.OverVal = 0; break;
            }
        }
        public static void i_rotshft_w(this Nec nec)
        {
            int ModRM;
            int src, dst;
            ModRM = nec.GetModRM();
            src = nec.GetRMWord(ModRM);
            dst = src;
            nec.CLKM(ModRM, 6, 6, 2, 24, 16, 7);
            switch (ModRM & 0x38)
            {
                case 0x00: nec.ROL_WORD(ref dst); nec.PutbackRMWord(ModRM, (ushort)dst); nec.I.OverVal = (uint)((src ^ dst) & 0x8000); break;
                case 0x08: nec.ROR_WORD(ref dst); nec.PutbackRMWord(ModRM, (ushort)dst); nec.I.OverVal = (uint)((src ^ dst) & 0x8000); break;
                case 0x10: nec.ROLC_WORD(ref dst); nec.PutbackRMWord(ModRM, (ushort)dst); nec.I.OverVal = (uint)((src ^ dst) & 0x8000); break;
                case 0x18: nec.RORC_WORD(ref dst); nec.PutbackRMWord(ModRM, (ushort)dst); nec.I.OverVal = (uint)((src ^ dst) & 0x8000); break;
                case 0x20: nec.SHL_WORD(1, ref dst, ModRM); nec.I.OverVal = (uint)((src ^ dst) & 0x8000); break;
                case 0x28: nec.SHR_WORD(1, ref dst, ModRM); nec.I.OverVal = (uint)((src ^ dst) & 0x8000); break;
                case 0x30: break;
                case 0x38: nec.SHRA_WORD(1, ref dst, ModRM); nec.I.OverVal = 0; break;
            }
        }
        public static void i_rotshft_bcl(this Nec nec)
        {
            int ModRM;
            int src, dst;
            byte c;
            ModRM = nec.GetModRM();
            src = nec.GetRMByte(ModRM);
            dst = src;
            c = nec.I.regs.b[2];
            nec.CLKM(ModRM, 7, 7, 2, 19, 19, 6);
            if (c != 0)
            {
                switch (ModRM & 0x38)
                {
                    case 0x00: do {  nec.ROL_BYTE(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMByte(ModRM, (byte)dst); break;
                    case 0x08: do { nec.ROR_BYTE(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMByte(ModRM, (byte)dst); break;
                    case 0x10: do { nec.ROLC_BYTE(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMByte(ModRM, (byte)dst); break;
                    case 0x18: do { nec.RORC_BYTE(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMByte(ModRM, (byte)dst); break;
                    case 0x20: nec.SHL_BYTE(c, ref dst, ModRM); break;
                    case 0x28: nec.SHR_BYTE(c, ref dst, ModRM); break;
                    case 0x30: break;
                    case 0x38: nec.SHRA_BYTE(c, ref dst, ModRM); break;
                }
            }
        }
        public static void i_rotshft_wcl(this Nec nec)
        {
            int ModRM;
            int src, dst;
            byte c;
            ModRM = nec.GetModRM();
            src = nec.GetRMWord(ModRM);
            dst = src;
            c = nec.I.regs.b[2];
            nec.CLKM(ModRM, 7, 7, 2, 27, 19, 6);
            if (c != 0)
            {
                switch (ModRM & 0x38)
                {
                    case 0x00: do { nec.ROL_WORD(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMWord(ModRM, (ushort)dst); break;
                    case 0x08: do { nec.ROR_WORD(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMWord(ModRM, (ushort)dst); break;
                    case 0x10: do { nec.ROLC_WORD(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMWord(ModRM, (ushort)dst); break;
                    case 0x18: do { nec.RORC_WORD(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMWord(ModRM, (ushort)dst); break;
                    case 0x20: nec.SHL_WORD(c, ref dst, ModRM); break;
                    case 0x28: nec.SHR_WORD(c, ref dst, ModRM); break;
                    case 0x30: break;
                    case 0x38: nec.SHRA_WORD(c, ref dst, ModRM); break;
                }
            }
        }
        public static void i_aam(this Nec nec)
        {
            byte mult = nec.FETCH();
            mult = 0;
            nec.I.regs.b[1] = (byte)(nec.I.regs.b[0] / 10);
            nec.I.regs.b[0] %= 10;
            nec.SetSZPF_Word(nec.I.regs.b[0] + nec.I.regs.b[1] * 0x100);
            nec.CLKS(15, 15, 12);
        }
        public static void i_aad(this Nec nec)
        {
            byte mult = nec.FETCH();
            mult = 0;
            nec.I.regs.b[0] = (byte)(nec.I.regs.b[1] * 10 + nec.I.regs.b[0]);
            nec.I.regs.b[1] = 0;
            nec.SetSZPF_Byte(nec.I.regs.b[0]);
            nec.CLKS(7, 7, 8);
        }
        public static void i_setalc(this Nec nec)
        {
            nec.I.regs.b[0] = (byte)(nec.CF() ? 0xff : 0x00);
            nec.pendingCycles -= 3;
        }
        public static void i_trans(this Nec nec)
        {
            int dest = (nec.I.regs.b[6] + nec.I.regs.b[7] * 0x100 + nec.I.regs.b[0]) & 0xffff;
            nec.I.regs.b[0] = nec.GetMemB(3, dest);
            nec.CLKS(9, 9, 5);
        }
        public static void i_fpo(this Nec nec)
        {
            int ModRM;
            ModRM = nec.GetModRM();
            nec.pendingCycles -= 2;
        }
        public static void i_loopne(this Nec nec)
        {
            sbyte disp = (sbyte)nec.FETCH();
            //nec.I.regs.w[1]--;
            ushort w1 = (ushort)(nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100 - 1);
            nec.I.regs.b[2] = (byte)(w1 % 0x100);
            nec.I.regs.b[3] = (byte)(w1 / 0x100);
            if (!nec.ZF() && (nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100 != 0))
            {
                nec.I.ip = (ushort)(nec.I.ip + disp);
                nec.CLKS(14, 14, 6);
            }
            else
            {
                nec.CLKS(5, 5, 3);
            }
        }
        public static void i_loope(this Nec nec)
        {
            sbyte disp = (sbyte)nec.FETCH();
            //nec.I.regs.w[1]--;
            ushort w1 = (ushort)(nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100 - 1);
            nec.I.regs.b[2] = (byte)(w1 % 0x100);
            nec.I.regs.b[3] = (byte)(w1 / 0x100);
            if (nec.ZF() && (nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100 != 0))
            {
                nec.I.ip = (ushort)(nec.I.ip + disp);
                nec.CLKS(14, 14, 6);
            }
            else
            {
                nec.CLKS(5, 5, 3);
            }
        }
        public static void i_loop(this Nec nec)
        {
            sbyte disp = (sbyte)nec.FETCH();
            //nec.I.regs.w[1]--;
            ushort w1 = (ushort)(nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100 - 1);
            nec.I.regs.b[2] = (byte)(w1 % 0x100);
            nec.I.regs.b[3] = (byte)(w1 / 0x100);
            if (nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100 != 0)
            {
                nec.I.ip = (ushort)(nec.I.ip + disp);
                nec.CLKS(13, 13, 6);
            }
            else
            {
                nec.CLKS(5, 5, 3);
            }
        }
        public static void i_jcxz(this Nec nec)
        {
            sbyte disp = (sbyte)nec.FETCH();
            if (nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100 == 0)
            {
                nec.I.ip = (ushort)(nec.I.ip + disp);
                nec.CLKS(13, 13, 6);
            }
            else
            {
                nec.CLKS(5, 5, 3);
            }
        }
        public static void i_inal(this Nec nec)
        {
            byte port = nec.FETCH();
            nec.I.regs.b[0] = nec.ReadIOByte(port);
            nec.CLKS(9, 9, 5);
        }
        public static void i_inax(this Nec nec)
        {
            byte port = nec.FETCH();
            //nec.I.regs.w[0] = nec.ReadIOWord(port);
            ushort w0 = nec.ReadIOWord(port);
            nec.I.regs.b[0] = (byte)(w0 % 0x100);
            nec.I.regs.b[1] = (byte)(w0 / 0x100);
            nec.CLKW(13, 13, 7, 13, 9, 5, port);
        }
        public static void i_outal(this Nec nec)
        {
            byte port = nec.FETCH();
            nec.WriteIOByte(port, nec.I.regs.b[0]);
            nec.CLKS(8, 8, 3);
        }
        public static void i_outax(this Nec nec)
        {
            byte port = nec.FETCH();
            //WriteIOWord(port, nec.I.regs.w[0]);
            nec.WriteIOWord(port, (ushort)(nec.I.regs.b[0] + nec.I.regs.b[1] * 0x100));
            nec.CLKW(12, 12, 5, 12, 8, 3, port);
        }
        public static void i_call_d16(this Nec nec)
        {
            ushort tmp;
            tmp = nec.FETCHWORD();
            nec.PUSH(nec.I.ip);
            nec.I.ip = (ushort)(nec.I.ip + (short)tmp);
            //CHANGE_PC;
            nec.pendingCycles -= 24;
        }
        public static void i_jmp_d16(this Nec nec)
        {
            ushort tmp;
            tmp = nec.FETCHWORD();
            nec.I.ip = (ushort)(nec.I.ip + (short)tmp);
            //CHANGE_PC;
            nec.pendingCycles -= 15;
        }
        public static void i_jmp_far(this Nec nec)
        {
            ushort tmp, tmp1;
            tmp = nec.FETCHWORD();
            tmp1 = nec.FETCHWORD();
            nec.I.sregs[1] = (ushort)tmp1;
            nec.I.ip = (ushort)tmp;
            //CHANGE_PC;
            nec.pendingCycles -= 27;
        }
        public static void i_jmp_d8(this Nec nec)
        {
            int tmp = (int)((sbyte)nec.FETCH());
            nec.pendingCycles -= 12;
            if (tmp == -2 && nec.I.no_interrupt == 0 && (nec.I.pending_irq == 0) && nec.pendingCycles > 0)
            {
                nec.pendingCycles %= 12;
            }
            nec.I.ip = (ushort)(nec.I.ip + tmp);
        }
        public static void i_inaldx(this Nec nec)
        {
            //nec.I.regs.b[0] = ReadIOByte(nec.I.regs.w[2]);
            nec.I.regs.b[0] = nec.ReadIOByte(nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100);
            nec.CLKS(8, 8, 5);
        }
        public static void i_inaxdx(this Nec nec)
        {
            //nec.I.regs.w[0] = nec.ReadIOWord(nec.I.regs.w[2]);
            ushort w0 = nec.ReadIOWord(nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100);
            nec.I.regs.b[0] = (byte)(w0 % 0x100);
            nec.I.regs.b[1] = (byte)(w0 / 0x100);
            nec.CLKW(12, 12, 7, 12, 8, 5, nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100);
        }
        public static void i_outdxal(this Nec nec)
        {
            //WriteIOByte(nec.I.regs.w[2], nec.I.regs.b[0]);
            nec.WriteIOByte(nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100, nec.I.regs.b[0]);
            nec.CLKS(8, 8, 3);
        }
        public static void i_outdxax(this Nec nec)
        {
            //WriteIOWord(nec.I.regs.w[2], nec.I.regs.w[0]);
            //nec.CLKW(12, 12, 5, 12, 8, 3, nec.I.regs.w[2]);
            nec.WriteIOWord(nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100, (ushort)(nec.I.regs.b[0] + nec.I.regs.b[1] * 0x100));
            nec.CLKW(12, 12, 5, 12, 8, 3, nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100);
        }
        public static void i_lock(this Nec nec)
        {
            nec.I.no_interrupt = 1;
            nec.CLK(2);
        }
        public static void i_repne(this Nec nec)
        {
            byte next = nec.fetchop();
            ushort c = (ushort)(nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100);//nec.I.regs.w[1];
            switch (next)
            {
                case 0x26: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[0] << 4); next = nec.fetchop(); nec.CLK(2); break;
                case 0x2e: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[1] << 4); next = nec.fetchop(); nec.CLK(2); break;
                case 0x36: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[2] << 4); next = nec.fetchop(); nec.CLK(2); break;
                case 0x3e: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[3] << 4); next = nec.fetchop(); nec.CLK(2); break;
            }
            switch (next)
            {
                case 0x6c: nec.CLK(2); if (c != 0) do { i_insb(nec); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100);/*nec.I.regs.w[1] = c;*/ break;
                case 0x6d: nec.CLK(2); if (c != 0) do { i_insw(nec); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0x6e: nec.CLK(2); if (c != 0) do { i_outsb(nec); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0x6f: nec.CLK(2); if (c != 0) do { i_outsw(nec); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa4: nec.CLK(2); if (c != 0) do { i_movsb(nec); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa5: nec.CLK(2); if (c != 0) do { i_movsw(nec); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa6: nec.CLK(2); if (c != 0) do { i_cmpsb(nec); c--; } while (c > 0 && nec.ZF() == false); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100);break;
                case 0xa7: nec.CLK(2); if (c != 0) do { i_cmpsw(nec); c--; } while (c > 0 && nec.ZF() == false); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xaa: nec.CLK(2); if (c != 0) do { i_stosb(nec); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xab: nec.CLK(2); if (c != 0) do { i_stosw(nec); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xac: nec.CLK(2); if (c != 0) do { i_lodsb(nec); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xad: nec.CLK(2); if (c != 0) do { i_lodsw(nec); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xae: nec.CLK(2); if (c != 0) do { i_scasb(nec); c--; } while (c > 0 && nec.ZF() == false); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xaf: nec.CLK(2); if (c != 0) do { i_scasw(nec); c--; } while (c > 0 && nec.ZF() == false); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                default:
                    nec.nec_instruction[next](nec);
                    //DoInstructionOpCode(next);
                    break;
            }
            Nec.seg_prefix = 0;
        }
        public static void i_repe(this Nec nec)
        {
            byte next = nec.fetchop();
            ushort c = (ushort)(nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100);// nec.I.regs.w[1];
            switch (next)
            {
                case 0x26: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[0] << 4); next = nec.fetchop(); nec.CLK(2); break;
                case 0x2e: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[1] << 4); next = nec.fetchop(); nec.CLK(2); break;
                case 0x36: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[2] << 4); next = nec.fetchop(); nec.CLK(2); break;
                case 0x3e: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[3] << 4); next = nec.fetchop(); nec.CLK(2); break;
            }
            switch (next)
            {
                case 0x6c: nec.CLK(2); if (c != 0) do { i_insb(nec); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100);/*nec.I.regs.w[1] = c;*/ break;
                case 0x6d: nec.CLK(2); if (c != 0) do { i_insw(nec); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0x6e: nec.CLK(2); if (c != 0) do { i_outsb(nec); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0x6f: nec.CLK(2); if (c != 0) do { i_outsw(nec); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa4: nec.CLK(2); if (c != 0) do { i_movsb(nec); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa5: nec.CLK(2); if (c != 0) do { i_movsw(nec); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa6: nec.CLK(2); if (c != 0) do { i_cmpsb(nec); c--; } while (c > 0 && nec.ZF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xa7: nec.CLK(2); if (c != 0) do { i_cmpsw(nec); c--; } while (c > 0 && nec.ZF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xaa: nec.CLK(2); if (c != 0) do { i_stosb(nec); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xab: nec.CLK(2); if (c != 0) do { i_stosw(nec); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xac: nec.CLK(2); if (c != 0) do { i_lodsb(nec); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xad: nec.CLK(2); if (c != 0) do { i_lodsw(nec); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xae: nec.CLK(2); if (c != 0) do { i_scasb(nec); c--; } while (c > 0 && nec.ZF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                case 0xaf: nec.CLK(2); if (c != 0) do { i_scasw(nec); c--; } while (c > 0 && nec.ZF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
                default:
                    nec.nec_instruction[next](nec);
                    //DoInstructionOpCode(next);
                    break;
            }
            Nec.seg_prefix = 0;
        }
        public static void i_hlt(this Nec nec)
        {
            nec.pendingCycles = 0;
        }
        public static void i_cmc(this Nec nec)
        {
            nec.I.CarryVal = (uint)(nec.CF() ? 0 : 1);
            nec.CLK(2);
        }
        public static void i_f6pre(this Nec nec)
        {
            int ModRM;
            uint tmp;
            uint uresult, uresult2;
            int result, result2;
            ModRM = nec.GetModRM();
            tmp = nec.GetRMByte(ModRM);
            switch (ModRM & 0x38)
            {
                case 0x00: tmp &= nec.FETCH(); nec.I.CarryVal = nec.I.OverVal = 0; nec.SetSZPF_Byte((int)tmp); nec.pendingCycles -= (ModRM >= 0xc0) ? 4 : 11; break;
                case 0x08: break;
                case 0x10: nec.PutbackRMByte(ModRM, (byte)(~tmp)); nec.pendingCycles -= (ModRM >= 0xc0) ? 2 : 16; break;
                case 0x18: nec.I.CarryVal = (uint)((tmp != 0) ? 1 : 0); tmp = (~tmp) + 1; nec.SetSZPF_Byte((int)tmp); nec.PutbackRMByte(ModRM, (byte)(tmp & 0xff)); nec.pendingCycles -= (ModRM >= 0xc0) ? 2 : 16; break;
                case 0x20:
                    uresult = nec.I.regs.b[0] * tmp;
                    //nec.I.regs.w[0] = (ushort)uresult;
                    nec.I.regs.b[0] = (byte)((ushort)uresult % 0x100);
                    nec.I.regs.b[1] = (byte)((ushort)uresult / 0x100);
                    nec.I.CarryVal = nec.I.OverVal = (uint)((nec.I.regs.b[1] != 0) ? 1 : 0);
                    nec.pendingCycles -= (ModRM >= 0xc0) ? 30 : 36;
                    break;
                case 0x28:
                    result = (short)((sbyte)nec.I.regs.b[0]) * (short)((sbyte)tmp);
                    //nec.I.regs.w[0] = (ushort)result;
                    nec.I.regs.b[0] = (byte)((ushort)result % 0x100);
                    nec.I.regs.b[1] = (byte)((ushort)result / 0x100);
                    nec.I.CarryVal = nec.I.OverVal = (uint)((nec.I.regs.b[1] != 0) ? 1 : 0);
                    nec.pendingCycles -= (ModRM >= 0xc0) ? 30 : 36;
                    break;
                case 0x30:
                    if (tmp != 0)
                    {
                        bool b1;
                        nec.DIVUB((int)tmp, out b1);
                        if (b1)
                        {
                            break;
                        }
                    }
                    else
                    {
                        nec.nec_interrupt(0, false);
                    }
                    nec.pendingCycles -= (ModRM >= 0xc0) ? 43 : 53;
                    break;
                case 0x38:
                    if (tmp != 0)
                    {
                        bool b1;
                        nec.DIVB((int)tmp, out b1);
                        if (b1)
                        {
                            break;
                        }
                    }
                    else
                    {
                        nec.nec_interrupt(0, false);
                    }
                    nec.pendingCycles -= (ModRM >= 0xc0) ? 43 : 53;
                    break;
            }
        }
        public static void i_f7pre(this Nec nec)
        {
            int ModRM;
            uint tmp, tmp2;
            uint uresult, uresult2;
            int result, result2;
            ModRM = nec.GetModRM();
            tmp = nec.GetRMWord(ModRM);
            switch (ModRM & 0x38)
            {
                case 0x00: tmp2 = nec.FETCHWORD(); tmp &= tmp2; nec.I.CarryVal = nec.I.OverVal = 0; nec.SetSZPF_Word((int)tmp); nec.pendingCycles -= (ModRM >= 0xc0) ? 4 : 11; break;
                case 0x08: break;
                case 0x10: nec.PutbackRMWord(ModRM, (ushort)(~tmp)); nec.pendingCycles -= (ModRM >= 0xc0) ? 2 : 16; break;
                case 0x18: nec.I.CarryVal = (uint)((tmp != 0) ? 1 : 0); tmp = (~tmp) + 1; nec.SetSZPF_Word((int)tmp); nec.PutbackRMWord(ModRM, (ushort)(tmp & 0xffff)); nec.pendingCycles -= (ModRM >= 0xc0) ? 2 : 16; break;
                case 0x20:
                    uresult = (uint)((nec.I.regs.b[0] + nec.I.regs.b[1] * 0x100) * tmp);
                    //nec.I.regs.w[0] = (ushort)(uresult & 0xffff);
                    //nec.I.regs.w[2] = (ushort)(uresult >> 16);
                    nec.I.regs.b[0] = (byte)((ushort)(uresult & 0xffff) % 0x100);
                    nec.I.regs.b[1] = (byte)((ushort)(uresult & 0xffff) / 0x100);
                    nec.I.regs.b[4] = (byte)((ushort)(uresult >> 16) % 0x100);
                    nec.I.regs.b[5] = (byte)((ushort)(uresult >> 16) / 0x100);
                    nec.I.CarryVal = nec.I.OverVal = (uint)(((nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100) != 0) ? 1 : 0);
                    nec.pendingCycles -= (ModRM >= 0xc0) ? 30 : 36;
                    break;
                case 0x28:
                    result = (int)((short)(nec.I.regs.b[0] + nec.I.regs.b[1] * 0x100)) * (int)((short)tmp);
                    //nec.I.regs.w[0] = (ushort)(result & 0xffff);
                    //nec.I.regs.w[2] = (ushort)(result >> 16);
                    nec.I.regs.b[0] = (byte)((ushort)(result & 0xffff) % 0x100);
                    nec.I.regs.b[1] = (byte)((ushort)(result & 0xffff) / 0x100);
                    nec.I.regs.b[4] = (byte)((ushort)(result >> 16) % 0x100);
                    nec.I.regs.b[5] = (byte)((ushort)(result >> 16) / 0x100);
                    nec.I.CarryVal = nec.I.OverVal = (uint)(((nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100) != 0) ? 1 : 0);
                    nec.pendingCycles -= (ModRM >= 0xc0) ? 30 : 36;
                    break;
                case 0x30:
                    if (tmp != 0)
                    {
                        bool b1;
                        nec.DIVUW((int)tmp, out b1);
                        if (b1)
                        {
                            break;
                        }
                    }
                    else
                    {
                        nec.nec_interrupt(0, false);
                    }
                    nec.pendingCycles -= (ModRM >= 0xc0) ? 43 : 53;
                    break;
                case 0x38:
                    if (tmp != 0)
                    {
                        bool b1;
                        nec.DIVW((int)tmp, out b1);
                        if (b1)
                        {
                            break;
                        }
                    }
                    else
                    {
                        nec.nec_interrupt(0, false);
                    }
                    nec.pendingCycles -= (ModRM >= 0xc0) ? 43 : 53;
                    break;
            }
        }
        public static void i_clc(this Nec nec)
        {
            nec.I.CarryVal = 0;
            nec.CLK(2);
        }
        public static void i_stc(this Nec nec)
        {
            nec.I.CarryVal = 1;
            nec.CLK(2);
        }
        public static void i_di(this Nec nec)
        {
            nec.I.IF = false;
            nec.CLK(2);
        }
        public static void i_ei(this Nec nec)
        {
            nec.I.IF = true;
            nec.CLK(2);
        }
        public static void i_cld(this Nec nec)
        {
            nec.I.DF = false;
            nec.CLK(2);
        }
        public static void i_std(this Nec nec)
        {
            nec.I.DF = true;
            nec.CLK(2);
        }
        //void i_fepre()
        //{
        //    int ModRM;
        //    byte tmp, tmp1;
        //    ModRM = nec.GetModRM();
        //    tmp = nec.GetRMByte(ModRM);
        //    switch (ModRM & 0x38)
        //    {
        //        case 0x00:
        //            tmp1 = (byte)(tmp + 1);
        //            nec.I.OverVal = (uint)((tmp == 0x7f) ? 1 : 0);
        //            SetAF(tmp1, tmp, 1);
        //            nec.SetSZPF_Byte(tmp1);
        //            nec.PutbackRMByte(ModRM, (byte)tmp1);
        //            nec.CLKM(ModRM, 2, 2, 2, 16, 16, 7);
        //            break;
        //        case 0x08: 
        //            tmp1 = (byte)(tmp - 1);
        //            nec.I.OverVal = (uint)((tmp == 0x80) ? 1 : 0);
        //            SetAF(tmp1, tmp, 1); nec.SetSZPF_Byte(tmp1);
        //            nec.PutbackRMByte(ModRM, (byte)tmp1);
        //            nec.CLKM(ModRM, 2, 2, 2, 16, 16, 7);
        //            break;
        //        default: break;
        //    }
        //}
        const int _i_fepre_ccount = 131586;  // (2 << 16) | (2 << 8) | 2
        const int _i_fepre_mcount = 1052679; // (16 << 16) | (16 << 8) | 7
        //手动内联了一些
        public static void i_fepre(this Nec nec)
        {
            int ModRM;
            byte tmp, tmp1;
            //ModRM = nec.GetModRM();
            ModRM = nec.ReadOpArg(((nec.I.sregs[1] << 4) + nec.I.ip++) ^ 0);
            //tmp = nec.GetRMByte(ModRM);
            tmp = ((ModRM) >= 0xc0 ? nec.I.regs.b[nec.mod_RM.RMb[ModRM]] : nec.ReadByte(
                nec.GetEA[ModRM]()
                //DoNecnec.GetEAOpCode(ModRM)
                ));
            switch (ModRM & 0x38)
            {
                case 0x00:
                    {
                        tmp1 = (byte)(tmp + 1);
                        nec.I.OverVal = (uint)((tmp == 0x7f) ? 1 : 0);

                        //SetAF(tmp1, tmp, 1);
                        nec.I.AuxVal = (uint)(((tmp1) ^ ((tmp) ^ (1))) & 0x10);

                        //SetSZPF_Byte(tmp1);
                        nec.I.ZeroVal = nec.I.ParityVal = (uint)((sbyte)tmp1);
                        nec.I.SignVal = (int)nec.I.ZeroVal;

                        //PutbackRMByte(ModRM, (byte)tmp1);
                        if (ModRM >= 0xc0)
                        {
                            nec.I.regs.b[nec.mod_RM.RMb[ModRM]] = tmp1;
                        }
                        else
                        {
                            nec.WriteByte(Nec.EA, tmp1);
                        }

                        //nec.CLKM(ModRM, 2, 2, 2, 16, 16, 7);

                        //计算也可以简化
                        //int ccount = (2 << 16) | (2 << 8) | 2, mcount = (16 << 16) | (16 << 8) | 7;
                        //nec.pendingCycles -= (ModRM >= 0xc0) ? ((ccount >> nec.chip_type) & 0x7f) : ((mcount >> nec.chip_type) & 0x7f);

                        //简化为读取常量
                        nec.pendingCycles -= (ModRM >= 0xc0) ? ((_i_fepre_ccount >> nec.chip_type) & 0x7f) : ((_i_fepre_mcount >> nec.chip_type) & 0x7f);
                    }
                    break;
                case 0x08:
                    {
                        tmp1 = (byte)(tmp - 1);
                        nec.I.OverVal = (uint)((tmp == 0x80) ? 1 : 0);
                        //SetAF(tmp1, tmp, 1); 
                        nec.I.AuxVal = (uint)(((tmp1) ^ ((tmp) ^ (1))) & 0x10);

                        //SetSZPF_Byte(tmp1);
                        nec.I.ZeroVal = nec.I.ParityVal = (uint)((sbyte)tmp1);
                        nec.I.SignVal = (int)nec.I.ZeroVal;

                        //PutbackRMByte(ModRM, (byte)tmp1);

                        if (ModRM >= 0xc0)
                        {
                            nec.I.regs.b[nec.mod_RM.RMb[ModRM]] = tmp1;
                        }
                        else
                        {
                            nec.WriteByte(Nec.EA, tmp1);
                        }

                        //nec.CLKM(ModRM, 2, 2, 2, 16, 16, 7);

                        //计算也可以简化
                        //int ccount = (2 << 16) | (2 << 8) | 2, mcount = (16 << 16) | (16 << 8) | 7;
                        //nec.pendingCycles -= (ModRM >= 0xc0) ? ((ccount >> nec.chip_type) & 0x7f) : ((mcount >> nec.chip_type) & 0x7f);

                        //简化为读取常量
                        nec.pendingCycles -= (ModRM >= 0xc0) ? ((_i_fepre_ccount >> nec.chip_type) & 0x7f) : ((_i_fepre_mcount >> nec.chip_type) & 0x7f);
                    }
                    break;
                default: break;
            }
        }
        public static void i_ffpre(this Nec nec)
        {
            int ModRM;
            ushort tmp, tmp1;
            ModRM = nec.GetModRM();
            tmp = nec.GetRMWord(ModRM);
            switch (ModRM & 0x38)
            {
                case 0x00: tmp1 = (ushort)(tmp + 1); nec.I.OverVal = (uint)((tmp == 0x7fff) ? 1 : 0); nec.SetAF(tmp1, tmp, 1); nec.SetSZPF_Word(tmp1); nec.PutbackRMWord(ModRM, (ushort)tmp1); nec.CLKM(ModRM, 2, 2, 2, 24, 16, 7); break;
                case 0x08: tmp1 = (ushort)(tmp - 1); nec.I.OverVal = (uint)((tmp == 0x8000) ? 1 : 0); nec.SetAF(tmp1, tmp, 1); nec.SetSZPF_Word(tmp1); nec.PutbackRMWord(ModRM, (ushort)tmp1); nec.CLKM(ModRM, 2, 2, 2, 24, 16, 7); break;
                case 0x10:
                    nec.PUSH(nec.I.ip);
                    nec.I.ip = (ushort)tmp;
                    //CHANGE_PC;
                    nec.pendingCycles -= (ModRM >= 0xc0) ? 16 : 20;
                    break;
                case 0x18:
                    tmp1 = nec.I.sregs[1];
                    nec.I.sregs[1] = nec.GetnextRMWord();
                    nec.PUSH(tmp1);
                    nec.PUSH(nec.I.ip);
                    nec.I.ip = tmp;
                    //CHANGE_PC;
                    nec.pendingCycles -= (ModRM >= 0xc0) ? 16 : 26;
                    break;
                case 0x20:
                    nec.I.ip = tmp;
                    //CHANGE_PC;
                    nec.pendingCycles -= 13;
                    break;
                case 0x28:
                    nec.I.ip = tmp;
                    nec.I.sregs[1] = nec.GetnextRMWord();
                    //CHANGE_PC;
                    nec.pendingCycles -= 15;
                    break;
                case 0x30: nec.PUSH(tmp); nec.pendingCycles -= 4; break;
                default: break;
            }
        }
        public static void i_invalid(this Nec nec)
        {
            nec.pendingCycles -= 10;
        }
    }

    //partial class Nec
    //{
    //    void i_add_br8()
    //    {
    //        int ModRM;
    //        byte src, dst;
    //        nec.DEF_br8(out ModRM, out src, out dst);
    //        nec.ADDB(ref src, ref dst);
    //        nec.PutbackRMByte(ModRM, dst);
    //        nec.CLKM(ModRM, 2, 2, 2, 16, 16, 7);
    //    }
    //    void i_add_wr16()
    //    {
    //        int ModRM;
    //        ushort src, dst;
    //        nec.DEF_wr16(out ModRM, out src, out dst);
    //        nec.ADDW(ref src, ref dst);
    //        nec.PutbackRMWord(ModRM, dst);
    //        nec.CLKR(ModRM, 24, 24, 11, 24, 16, 7, 2, Nec.EA);
    //    }
    //    void i_add_r8b()
    //    {
    //        int ModRM;
    //        byte src, dst;
    //        nec.DEF_r8b(out ModRM, out src, out dst);
    //        nec.ADDB(ref src, ref dst);
    //        nec.I.regs.b[nec.mod_RM.regb[ModRM]] = dst;
    //        nec.CLKM(ModRM, 2, 2, 2, 11, 11, 6);
    //    }
    //    void i_add_r16w()
    //    {
    //        int ModRM;
    //        ushort src, dst;
    //        nec.DEF_r16w(out ModRM, out src, out dst);
    //        nec.ADDW(ref src, ref dst);
    //        //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = dst;
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
    //        nec.CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, Nec.EA);
    //    }
    //    void i_add_ald8()
    //    {
    //        byte src, dst;
    //        nec.DEF_ald8(out src, out dst);
    //        nec.ADDB(ref src, ref dst);
    //        nec.I.regs.b[0] = dst;
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_add_axd16()
    //    {
    //        ushort src, dst;
    //        nec.DEF_axd16(out src, out dst);
    //        nec.ADDW(ref src, ref dst);
    //        //nec.I.regs.w[0] = dst;
    //        nec.I.regs.b[0] = (byte)(dst % 0x100);
    //        nec.I.regs.b[1] = (byte)(dst / 0x100);
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_push_es()
    //    {
    //        nec.PUSH(nec.I.sregs[0]);
    //        nec.CLKS(12, 8, 3);
    //    }
    //    void i_pop_es()
    //    {
    //        nec.POP(ref nec.I.sregs[0]);
    //        nec.CLKS(12, 8, 5);
    //    }
    //    void i_or_br8()
    //    {
    //        int ModRM;
    //        byte src, dst;
    //        nec.DEF_br8(out ModRM, out src, out dst);
    //        nec.ORB(ref src, ref dst);
    //        nec.PutbackRMByte(ModRM, dst);
    //        nec.CLKM(ModRM, 2, 2, 2, 16, 16, 7);
    //    }
    //    void i_or_wr16()
    //    {
    //        int ModRM;
    //        ushort src, dst;
    //        nec.DEF_wr16(out ModRM, out src, out dst);
    //        nec.ORW(ref src, ref dst);
    //        nec.PutbackRMWord(ModRM, dst);
    //        nec.CLKR(ModRM, 24, 24, 11, 24, 16, 7, 2, Nec.EA);
    //    }
    //    void i_or_r8b()
    //    {
    //        int ModRM;
    //        byte src, dst;
    //        nec.DEF_r8b(out ModRM, out src, out dst);
    //        nec.ORB(ref src, ref dst);
    //        nec.I.regs.b[nec.mod_RM.regb[ModRM]] = dst;
    //        nec.CLKM(ModRM, 2, 2, 2, 11, 11, 6);
    //    }
    //    void i_or_r16w()
    //    {
    //        int ModRM;
    //        ushort src, dst;
    //        nec.DEF_r16w(out ModRM, out src, out dst);
    //        nec.ORW(ref src, ref dst);
    //        //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = dst;
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
    //        nec.CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, Nec.EA);
    //    }
    //    void i_or_ald8()
    //    {
    //        byte src, dst;
    //        nec.DEF_ald8(out src, out dst);
    //        nec.ORB(ref src, ref dst);
    //        nec.I.regs.b[0] = dst;
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_or_axd16()
    //    {
    //        ushort src, dst;
    //        nec.DEF_axd16(out src, out dst);
    //        nec.ORW(ref src, ref dst);
    //        //nec.I.regs.w[0] = dst;
    //        nec.I.regs.b[0] = (byte)(dst % 0x100);
    //        nec.I.regs.b[1] = (byte)(dst / 0x100);
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_push_cs()
    //    {
    //        nec.PUSH(nec.I.sregs[1]);
    //        nec.CLKS(12, 8, 3);
    //    }
    //    void i_pre_nec()
    //    {
    //        int ModRM = 0, tmp = 0, tmp2 = 0;
    //        switch (nec.FETCH())
    //        {
    //            case 0x10: nec.BITOP_BYTE(ref ModRM, ref tmp); nec.CLKS(3, 3, 4); tmp2 = nec.I.regs.b[2] & 0x7; nec.I.ZeroVal = (uint)(((tmp & (1 << tmp2)) != 0) ? 1 : 0); nec.I.CarryVal = nec.I.OverVal = 0; break; /* Test */
    //            case 0x11: nec.BITOP_WORD(ref ModRM, ref tmp); nec.CLKS(3, 3, 4); tmp2 = nec.I.regs.b[2] & 0xf; nec.I.ZeroVal = (uint)(((tmp & (1 << tmp2)) != 0) ? 1 : 0); nec.I.CarryVal = nec.I.OverVal = 0; break; /* Test */
    //            case 0x12: nec.BITOP_BYTE(ref ModRM, ref tmp); nec.CLKS(5, 5, 4); tmp2 = nec.I.regs.b[2] & 0x7; tmp &= ~(1 << tmp2); nec.PutbackRMByte(ModRM, (byte)tmp); break; /* Clr */
    //            case 0x13: nec.BITOP_WORD(ref ModRM, ref tmp); nec.CLKS(5, 5, 4); tmp2 = nec.I.regs.b[2] & 0xf; tmp &= ~(1 << tmp2); nec.PutbackRMWord(ModRM, (ushort)tmp); break; /* Clr */
    //            case 0x14: nec.BITOP_BYTE(ref ModRM, ref tmp); nec.CLKS(4, 4, 4); tmp2 = nec.I.regs.b[2] & 0x7; tmp |= (1 << tmp2); nec.PutbackRMByte(ModRM, (byte)tmp); break; /* Set */
    //            case 0x15: nec.BITOP_WORD(ref ModRM, ref tmp); nec.CLKS(4, 4, 4); tmp2 = nec.I.regs.b[2] & 0xf; tmp |= (1 << tmp2); nec.PutbackRMWord(ModRM, (ushort)tmp); break; /* Set */
    //            case 0x16: nec.BITOP_BYTE(ref ModRM, ref tmp); nec.CLKS(4, 4, 4); tmp2 = nec.I.regs.b[2] & 0x7; nec.BIT_NOT(ref tmp, ref tmp2); nec.PutbackRMByte(ModRM, (byte)tmp); break; /* Not */
    //            case 0x17: nec.BITOP_WORD(ref ModRM, ref tmp); nec.CLKS(4, 4, 4); tmp2 = nec.I.regs.b[2] & 0xf; nec.BIT_NOT(ref tmp, ref tmp2); nec.PutbackRMWord(ModRM, (ushort)tmp); break; /* Not */

    //            case 0x18: nec.BITOP_BYTE(ref ModRM, ref tmp); nec.CLKS(4, 4, 4); tmp2 = (nec.FETCH()) & 0x7; nec.I.ZeroVal = (uint)(((tmp & (1 << tmp2)) != 0) ? 1 : 0); nec.I.CarryVal = nec.I.OverVal = 0; break; /* Test */
    //            case 0x19: nec.BITOP_WORD(ref ModRM, ref tmp); nec.CLKS(4, 4, 4); tmp2 = (nec.FETCH()) & 0xf; nec.I.ZeroVal = (uint)(((tmp & (1 << tmp2)) != 0) ? 1 : 0); nec.I.CarryVal = nec.I.OverVal = 0; break; /* Test */
    //            case 0x1a: nec.BITOP_BYTE(ref ModRM, ref tmp); nec.CLKS(6, 6, 4); tmp2 = (nec.FETCH()) & 0x7; tmp &= ~(1 << tmp2); nec.PutbackRMByte(ModRM, (byte)tmp); break; /* Clr */
    //            case 0x1b: nec.BITOP_WORD(ref ModRM, ref tmp); nec.CLKS(6, 6, 4); tmp2 = (nec.FETCH()) & 0xf; tmp &= ~(1 << tmp2); nec.PutbackRMWord(ModRM, (ushort)tmp); break; /* Clr */
    //            case 0x1c: nec.BITOP_BYTE(ref ModRM, ref tmp); nec.CLKS(5, 5, 4); tmp2 = (nec.FETCH()) & 0x7; tmp |= (1 << tmp2); nec.PutbackRMByte(ModRM, (byte)tmp); break; /* Set */
    //            case 0x1d: nec.BITOP_WORD(ref ModRM, ref tmp); nec.CLKS(5, 5, 4); tmp2 = (nec.FETCH()) & 0xf; tmp |= (1 << tmp2); nec.PutbackRMWord(ModRM, (ushort)tmp); break; /* Set */
    //            case 0x1e: nec.BITOP_BYTE(ref ModRM, ref tmp); nec.CLKS(5, 5, 4); tmp2 = (nec.FETCH()) & 0x7; nec.BIT_NOT(ref tmp, ref tmp2); nec.PutbackRMByte(ModRM, (byte)tmp); break; /* Not */
    //            case 0x1f: nec.BITOP_WORD(ref ModRM, ref tmp); nec.CLKS(5, 5, 4); tmp2 = (nec.FETCH()) & 0xf; nec.BIT_NOT(ref tmp, ref tmp2); nec.PutbackRMWord(ModRM, (ushort)tmp); break; /* Not */

    //            case 0x20: nec.ADD4S(ref tmp, ref tmp2); nec.CLKS(7, 7, 2); break;
    //            case 0x22: nec.SUB4S(ref tmp, ref tmp2); nec.CLKS(7, 7, 2); break;
    //            case 0x26: nec.CMP4S(ref tmp, ref tmp2); nec.CLKS(7, 7, 2); break;
    //            case 0x28: ModRM = nec.FETCH(); tmp = nec.GetRMByte(ModRM); tmp <<= 4; tmp |= nec.I.regs.b[0] & 0xf; nec.I.regs.b[0] = (byte)((nec.I.regs.b[0] & 0xf0) | ((tmp >> 8) & 0xf)); tmp &= 0xff; nec.PutbackRMByte(ModRM, (byte)tmp); nec.CLKM(ModRM, 13, 13, 9, 28, 28, 15); break;
    //            case 0x2a: ModRM = nec.FETCH(); tmp = nec.GetRMByte(ModRM); tmp2 = (nec.I.regs.b[0] & 0xf) << 4; nec.I.regs.b[0] = (byte)((nec.I.regs.b[0] & 0xf0) | (tmp & 0xf)); tmp = tmp2 | (tmp >> 4); nec.PutbackRMByte(ModRM, (byte)tmp); nec.CLKM(ModRM, 17, 17, 13, 32, 32, 19); break;
    //            case 0x31: ModRM = nec.FETCH(); ModRM = 0; break;
    //            case 0x33: ModRM = nec.FETCH(); ModRM = 0; break;
    //            case 0x92: nec.CLK(2); break; /* V25/35 FINT */
    //            case 0xe0: ModRM = nec.FETCH(); ModRM = 0; break;
    //            case 0xf0: ModRM = nec.FETCH(); ModRM = 0; break;
    //            case 0xff: ModRM = nec.FETCH(); ModRM = 0; break;
    //            default: break;
    //        }
    //    }
    //    void i_adc_br8()
    //    {
    //        int ModRM;
    //        byte src, dst;
    //        nec.DEF_br8(out ModRM, out src, out dst);
    //        src += (byte)(nec.CF() ? 1 : 0);
    //        nec.ADDB(ref src, ref dst);
    //        nec.PutbackRMByte(ModRM, dst);
    //        nec.CLKM(ModRM, 2, 2, 2, 16, 16, 7);
    //    }
    //    void i_adc_wr16()
    //    {
    //        int ModRM;
    //        ushort src, dst;
    //        nec.DEF_wr16(out ModRM, out src, out dst);
    //        src += (ushort)(nec.CF() ? 1 : 0);
    //        nec.ADDW(ref src, ref dst);
    //        nec.PutbackRMWord(ModRM, dst);
    //        nec.CLKR(ModRM, 24, 24, 11, 24, 16, 7, 2, Nec.EA);
    //    }
    //    void i_adc_r8b()
    //    {
    //        int ModRM;
    //        byte src, dst;
    //        nec.DEF_r8b(out ModRM, out src, out dst);
    //        src += (byte)(nec.CF() ? 1 : 0);
    //        nec.ADDB(ref src, ref dst);
    //        nec.I.regs.b[nec.mod_RM.regb[ModRM]] = dst;
    //        nec.CLKM(ModRM, 2, 2, 2, 11, 11, 6);
    //    }
    //    void i_adc_r16w()
    //    {
    //        int ModRM;
    //        ushort src, dst;
    //        nec.DEF_r16w(out ModRM, out src, out dst);
    //        src += (ushort)(nec.CF() ? 1 : 0);
    //        nec.ADDW(ref src, ref dst);
    //        //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = dst;
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
    //        nec.CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, Nec.EA);
    //    }
    //    void i_adc_ald8()
    //    {
    //        byte src, dst;
    //        nec.DEF_ald8(out src, out dst);
    //        src += (byte)(nec.CF() ? 1 : 0);
    //        nec.ADDB(ref src, ref dst);
    //        nec.I.regs.b[0] = dst;
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_adc_axd16()
    //    {
    //        ushort src, dst;
    //        nec.DEF_axd16(out src, out dst);
    //        src += (ushort)(nec.CF() ? 1 : 0);
    //        nec.ADDW(ref src, ref dst);
    //        //nec.I.regs.w[0] = dst;
    //        nec.I.regs.b[0] = (byte)(dst % 0x100);
    //        nec.I.regs.b[1] = (byte)(dst / 0x100);
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_push_ss()
    //    {
    //        nec.PUSH(nec.I.sregs[2]);
    //        nec.CLKS(12, 8, 3);
    //    }
    //    void i_pop_ss()
    //    {
    //        nec.POP(ref nec.I.sregs[2]);
    //        nec.CLKS(12, 8, 5);
    //        nec.I.no_interrupt = 1;
    //    }
    //    void i_sbb_br8()
    //    {
    //        int ModRM;
    //        byte src, dst;
    //        nec.DEF_br8(out ModRM, out src, out dst);
    //        src += (byte)(nec.CF() ? 1 : 0);
    //        nec.SUBB(ref src, ref dst);
    //        nec.PutbackRMByte(ModRM, dst);
    //        nec.CLKM(ModRM, 2, 2, 2, 16, 16, 7);
    //    }
    //    void i_sbb_wr16()
    //    {
    //        int ModRM;
    //        ushort src, dst;
    //        nec.DEF_wr16(out ModRM, out src, out dst);
    //        src += (ushort)(nec.CF() ? 1 : 0);
    //        nec.SUBW(ref src, ref dst);
    //        nec.PutbackRMWord(ModRM, dst);
    //        nec.CLKR(ModRM, 24, 24, 11, 24, 16, 7, 2, Nec.EA);
    //    }
    //    void i_sbb_r8b()
    //    {
    //        int ModRM;
    //        byte src, dst;
    //        nec.DEF_r8b(out ModRM, out src, out dst);
    //        src += (byte)(nec.CF() ? 1 : 0);
    //        nec.SUBB(ref src, ref dst);
    //        nec.I.regs.b[nec.mod_RM.regb[ModRM]] = dst;
    //        nec.CLKM(ModRM, 2, 2, 2, 11, 11, 6);
    //    }
    //    void i_sbb_r16w()
    //    {
    //        int ModRM;
    //        ushort src, dst;
    //        nec.DEF_r16w(out ModRM, out src, out dst);
    //        src += (ushort)(nec.CF() ? 1 : 0);
    //        nec.SUBW(ref src, ref dst);
    //        //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = dst;
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
    //        nec.CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, Nec.EA);
    //    }
    //    void i_sbb_ald8()
    //    {
    //        byte src, dst;
    //        nec.DEF_ald8(out src, out dst);
    //        src += (byte)(nec.CF() ? 1 : 0);
    //        nec.SUBB(ref src, ref dst);
    //        nec.I.regs.b[0] = dst;
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_sbb_axd16()
    //    {
    //        ushort src, dst;
    //        nec.DEF_axd16(out src, out dst);
    //        src += (ushort)(nec.CF() ? 1 : 0);
    //        nec.SUBW(ref src, ref dst);
    //        //nec.I.regs.w[0] = dst;
    //        nec.I.regs.b[0] = (byte)(dst % 0x100);
    //        nec.I.regs.b[1] = (byte)(dst / 0x100);
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_push_ds()
    //    {
    //        nec.PUSH(nec.I.sregs[3]);
    //        nec.CLKS(12, 8, 3);
    //    }
    //    void i_pop_ds()
    //    {
    //        nec.POP(ref nec.I.sregs[3]);
    //        nec.CLKS(12, 8, 5);
    //    }
    //    void i_and_br8()
    //    {
    //        int ModRM;
    //        byte src, dst;
    //        nec.DEF_br8(out ModRM, out src, out dst);
    //        nec.ANDB(ref src, ref dst);
    //        nec.PutbackRMByte(ModRM, dst);
    //        nec.CLKM(ModRM, 2, 2, 2, 16, 16, 7);
    //    }
    //    void i_and_wr16()
    //    {
    //        int ModRM;
    //        ushort src, dst;
    //        nec.DEF_wr16(out ModRM, out src, out dst);
    //        nec.ANDW(ref src, ref dst);
    //        nec.PutbackRMWord(ModRM, dst);
    //        nec.CLKR(ModRM, 24, 24, 11, 24, 16, 7, 2, Nec.EA);
    //    }
    //    void i_and_r8b()
    //    {
    //        int ModRM;
    //        byte src, dst;
    //        nec.DEF_r8b(out ModRM, out src, out dst);
    //        nec.ANDB(ref src, ref dst);
    //        nec.I.regs.b[nec.mod_RM.regb[ModRM]] = dst;
    //        nec.CLKM(ModRM, 2, 2, 2, 11, 11, 6);
    //    }
    //    void i_and_r16w()
    //    {
    //        int ModRM;
    //        ushort src, dst;
    //        nec.DEF_r16w(out ModRM, out src, out dst);
    //        nec.ANDW(ref src, ref dst);
    //        //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = dst;
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
    //        nec.CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, Nec.EA);
    //    }
    //    void i_and_ald8()
    //    {
    //        byte src, dst;
    //        nec.DEF_ald8(out src, out dst);
    //        nec.ANDB(ref src, ref dst);
    //        nec.I.regs.b[0] = dst;
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_and_axd16()
    //    {
    //        ushort src, dst;
    //        nec.DEF_axd16(out src, out dst);
    //        nec.ANDW(ref src, ref dst);
    //        //nec.I.regs.w[0] = dst;
    //        nec.I.regs.b[0] = (byte)(dst % 0x100);
    //        nec.I.regs.b[1] = (byte)(dst / 0x100);
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_es()
    //    {
    //        Nec.seg_prefix = 1;
    //        Nec.prefix_base = nec.I.sregs[0] << 4;
    //        nec.CLK(2);
    //        nec.nec_instruction[nec.fetchop()]();
    //        //DoInstructionOpCode(nec.fetchop());
    //        Nec.seg_prefix = 0;
    //    }
    //    void i_daa()
    //    {
    //        ADJ4(6, 0x60);
    //        nec.CLKS(3, 3, 2);
    //    }
    //    void i_sub_br8()
    //    {
    //        int ModRM;
    //        byte src, dst;
    //        nec.DEF_br8(out ModRM, out src, out dst);
    //        nec.SUBB(ref src, ref dst);
    //        nec.PutbackRMByte(ModRM, dst);
    //        nec.CLKM(ModRM, 2, 2, 2, 16, 16, 7);
    //    }
    //    void i_sub_wr16()
    //    {
    //        int ModRM;
    //        ushort src, dst;
    //        nec.DEF_wr16(out ModRM, out src, out dst);
    //        nec.SUBW(ref src, ref dst);
    //        nec.PutbackRMWord(ModRM, dst);
    //        nec.CLKR(ModRM, 24, 24, 11, 24, 16, 7, 2, Nec.EA);
    //    }
    //    void i_sub_r8b()
    //    {
    //        int ModRM;
    //        byte src, dst;
    //        nec.DEF_r8b(out ModRM, out src, out dst);
    //        nec.SUBB(ref src, ref dst);
    //        nec.I.regs.b[nec.mod_RM.regb[ModRM]] = dst;
    //        nec.CLKM(ModRM, 2, 2, 2, 11, 11, 6);
    //    }
    //    void i_sub_r16w()
    //    {
    //        int ModRM;
    //        ushort src, dst;
    //        nec.DEF_r16w(out ModRM, out src, out dst);
    //        nec.SUBW(ref src, ref dst);
    //        //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = dst;
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
    //        nec.CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, Nec.EA);
    //    }
    //    void i_sub_ald8()
    //    {
    //        byte src, dst;
    //        nec.DEF_ald8(out src, out dst);
    //        nec.SUBB(ref src, ref dst);
    //        nec.I.regs.b[0] = dst;
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_sub_axd16()
    //    {
    //        ushort src, dst;
    //        nec.DEF_axd16(out src, out dst);
    //        nec.SUBW(ref src, ref dst);
    //        //nec.I.regs.w[0] = dst;
    //        nec.I.regs.b[0] = (byte)(dst % 0x100);
    //        nec.I.regs.b[1] = (byte)(dst / 0x100);
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_cs()
    //    {
    //        Nec.seg_prefix = 1;
    //        Nec.prefix_base = nec.I.sregs[1] << 4;
    //        nec.CLK(2);
    //        nec.nec_instruction[nec.fetchop()]();
    //        //DoInstructionOpCode(nec.fetchop());
    //        Nec.seg_prefix = 0;
    //    }
    //    void i_das()
    //    {
    //        ADJ4(-6, -0x60);
    //        nec.CLKS(3, 3, 2);
    //    }
    //    void i_xor_br8()
    //    {
    //        int ModRM;
    //        byte src, dst;
    //        nec.DEF_br8(out ModRM, out src, out dst);
    //        nec.XORB(ref src, ref dst);
    //        nec.PutbackRMByte(ModRM, dst);
    //        nec.CLKM(ModRM, 2, 2, 2, 16, 16, 7);
    //    }
    //    void i_xor_wr16()
    //    {
    //        int ModRM;
    //        ushort src, dst;
    //        nec.DEF_wr16(out ModRM, out src, out dst);
    //        nec.XORW(ref src, ref dst);
    //        nec.PutbackRMWord(ModRM, dst);
    //        nec.CLKR(ModRM, 24, 24, 11, 24, 16, 7, 2, Nec.EA);
    //    }
    //    void i_xor_r8b()
    //    {
    //        int ModRM;
    //        byte src, dst;
    //        nec.DEF_r8b(out ModRM, out src, out dst);
    //        nec.XORB(ref src, ref dst);
    //        nec.I.regs.b[nec.mod_RM.regb[ModRM]] = dst;
    //        nec.CLKM(ModRM, 2, 2, 2, 11, 11, 6);
    //    }
    //    void i_xor_r16w()
    //    {
    //        int ModRM;
    //        ushort src, dst;
    //        nec.DEF_r16w(out ModRM, out src, out dst);
    //        nec.XORW(ref src, ref dst);
    //        //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = dst;
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
    //        nec.CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, Nec.EA);
    //    }
    //    void i_xor_ald8()
    //    {
    //        byte src, dst;
    //        nec.DEF_ald8(out src, out dst);
    //        nec.XORB(ref src, ref dst);
    //        nec.I.regs.b[0] = dst;
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_xor_axd16()
    //    {
    //        ushort src, dst;
    //        nec.DEF_axd16(out src, out dst);
    //        nec.XORW(ref src, ref dst);
    //        //nec.I.regs.w[0] = dst;
    //        nec.I.regs.b[0] = (byte)(dst % 0x100);
    //        nec.I.regs.b[1] = (byte)(dst / 0x100);
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_ss()
    //    {
    //        Nec.seg_prefix = 1;
    //        Nec.prefix_base = nec.I.sregs[2] << 4;
    //        nec.CLK(2);
    //        nec.nec_instruction[nec.fetchop()]();
    //        //DoInstructionOpCode(nec.fetchop());
    //        Nec.seg_prefix = 0;
    //    }
    //    void i_aaa()
    //    {
    //        nec.ADJB(6, (nec.I.regs.b[0] > 0xf9) ? 2 : 1);
    //        nec.CLKS(7, 7, 4);
    //    }
    //    void i_cmp_br8()
    //    {
    //        int ModRM;
    //        byte src, dst;
    //        nec.DEF_br8(out ModRM, out src, out dst);
    //        nec.SUBB(ref src, ref dst);
    //        nec.CLKM(ModRM, 2, 2, 2, 11, 11, 6);
    //    }
    //    void i_cmp_wr16()
    //    {
    //        int ModRM;
    //        ushort src, dst;
    //        nec.DEF_wr16(out ModRM, out src, out dst);
    //        nec.SUBW(ref src, ref dst);
    //        nec.CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, Nec.EA);
    //    }
    //    void i_cmp_r8b()
    //    {
    //        int ModRM;
    //        byte src, dst;
    //        nec.DEF_r8b(out ModRM, out src, out dst);
    //        nec.SUBB(ref src, ref dst);
    //        nec.CLKM(ModRM, 2, 2, 2, 11, 11, 6);
    //    }
    //    void i_cmp_r16w()
    //    {
    //        int ModRM;
    //        ushort src, dst;
    //        nec.DEF_r16w(out ModRM, out src, out dst);
    //        nec.SUBW(ref src, ref dst);
    //        nec.CLKR(ModRM, 15, 15, 8, 15, 11, 6, 2, Nec.EA);
    //    }
    //    void i_cmp_ald8()
    //    {
    //        byte src, dst;
    //        nec.DEF_ald8(out src, out dst);
    //        nec.SUBB(ref src, ref dst);
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_cmp_axd16()
    //    {
    //        ushort src, dst;
    //        nec.DEF_axd16(out src, out dst);
    //        nec.SUBW(ref src, ref dst);
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_ds()
    //    {
    //        Nec.seg_prefix = 1;
    //        Nec.prefix_base = nec.I.sregs[3] << 4;
    //        nec.CLK(2);
    //        nec.nec_instruction[nec.fetchop()]();
    //        //DoInstructionOpCode(nec.fetchop());
    //        Nec.seg_prefix = 0;
    //    }
    //    void i_aas()
    //    {
    //        nec.ADJB(-6, (nec.I.regs.b[0] < 6) ? -2 : -1);
    //        nec.CLKS(7, 7, 4);
    //    }
    //    void i_inc_ax()
    //    {
    //        nec.IncWordReg(0);
    //        nec.CLK(2);
    //    }
    //    void i_inc_cx()
    //    {
    //        nec.IncWordReg(1);
    //        nec.CLK(2);
    //    }
    //    void i_inc_dx()
    //    {
    //        nec.IncWordReg(2);
    //        nec.CLK(2);
    //    }
    //    void i_inc_bx()
    //    {
    //        nec.IncWordReg(3);
    //        nec.CLK(2);
    //    }
    //    void i_inc_sp()
    //    {
    //        nec.IncWordReg(4);
    //        nec.CLK(2);
    //    }
    //    void i_inc_bp()
    //    {
    //        nec.IncWordReg(5);
    //        nec.CLK(2);
    //    }
    //    void i_inc_si()
    //    {
    //        nec.IncWordReg(6);
    //        nec.CLK(2);
    //    }
    //    void i_inc_di()
    //    {
    //        nec.IncWordReg(7);
    //        nec.CLK(2);
    //    }
    //    void i_dec_ax()
    //    {
    //        nec.DecWordReg(0);
    //        nec.CLK(2);
    //    }
    //    void i_dec_cx()
    //    {
    //        nec.DecWordReg(1);
    //        nec.CLK(2);
    //    }
    //    void i_dec_dx()
    //    {
    //        nec.DecWordReg(2);
    //        nec.CLK(2);
    //    }
    //    void i_dec_bx()
    //    {
    //        nec.DecWordReg(3);
    //        nec.CLK(2);
    //    }
    //    void i_dec_sp()
    //    {
    //        nec.DecWordReg(4);
    //        nec.CLK(2);
    //    }
    //    void i_dec_bp()
    //    {
    //        nec.DecWordReg(5);
    //        nec.CLK(2);
    //    }
    //    void i_dec_si()
    //    {
    //        nec.DecWordReg(6);
    //        nec.CLK(2);
    //    }
    //    void i_dec_di()
    //    {
    //        nec.DecWordReg(7);
    //        nec.CLK(2);
    //    }
    //    void i_push_ax()
    //    {
    //        //PUSH(nec.I.regs.w[0]);
    //        nec.PUSH((ushort)(nec.I.regs.b[0] + nec.I.regs.b[1] * 0x100));
    //        nec.CLKS(12, 8, 3);
    //    }
    //    void i_push_cx()
    //    {
    //        //PUSH(nec.I.regs.w[1]);
    //        nec.PUSH((ushort)(nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100));
    //        nec.CLKS(12, 8, 3);
    //    }
    //    void i_push_dx()
    //    {
    //        //PUSH(nec.I.regs.w[2]);
    //        nec.PUSH((ushort)(nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100));
    //        nec.CLKS(12, 8, 3);
    //    }
    //    void i_push_bx()
    //    {
    //        //PUSH(nec.I.regs.w[3]);
    //        nec.PUSH((ushort)(nec.I.regs.b[6] + nec.I.regs.b[7] * 0x100));
    //        nec.CLKS(12, 8, 3);
    //    }
    //    void i_push_sp()
    //    {
    //        //PUSH(nec.I.regs.w[4]);
    //        nec.PUSH((ushort)(nec.I.regs.b[8] + nec.I.regs.b[9] * 0x100));
    //        nec.CLKS(12, 8, 3);
    //    }
    //    void i_push_bp()
    //    {
    //        //PUSH(nec.I.regs.w[5]);
    //        nec.PUSH((ushort)(nec.I.regs.b[10] + nec.I.regs.b[11] * 0x100));
    //        nec.CLKS(12, 8, 3);
    //    }
    //    void i_push_si()
    //    {
    //        //PUSH(nec.I.regs.w[6]);
    //        nec.PUSH((ushort)(nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100));
    //        nec.CLKS(12, 8, 3);
    //    }
    //    void i_push_di()
    //    {
    //        //PUSH(nec.I.regs.w[7]);
    //        nec.PUSH((ushort)(nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100));
    //        nec.CLKS(12, 8, 3);
    //    }
    //    void i_pop_ax()
    //    {
    //        //POP(ref nec.I.regs.w[0]);
    //        nec.POPW(0);
    //        nec.CLKS(12, 8, 5);
    //    }
    //    void i_pop_cx()
    //    {
    //        //POP(ref nec.I.regs.w[1]);
    //        nec.POPW(1);
    //        nec.CLKS(12, 8, 5);
    //    }
    //    void i_pop_dx()
    //    {
    //        //POP(ref nec.I.regs.w[2]);
    //        nec.POPW(2);
    //        nec.CLKS(12, 8, 5);
    //    }
    //    void i_pop_bx()
    //    {
    //        //POP(ref nec.I.regs.w[3]);
    //        nec.POPW(3);
    //        nec.CLKS(12, 8, 5);
    //    }
    //    void i_pop_sp()
    //    {
    //        //POP(ref nec.I.regs.w[4]);
    //        nec.POPW(4);
    //        nec.CLKS(12, 8, 5);
    //    }
    //    void i_pop_bp()
    //    {
    //        //POP(ref nec.I.regs.w[5]);
    //        nec.POPW(5);
    //        nec.CLKS(12, 8, 5);
    //    }
    //    void i_pop_si()
    //    {
    //        //POP(ref nec.I.regs.w[6]);
    //        nec.POPW(6);
    //        nec.CLKS(12, 8, 5);
    //    }
    //    void i_pop_di()
    //    {
    //        //POP(ref nec.I.regs.w[7]);
    //        nec.POPW(7);
    //        nec.CLKS(12, 8, 5);
    //    }
    //    void i_pusha()
    //    {
    //        ushort tmp = (ushort)(nec.I.regs.b[8] + nec.I.regs.b[9] * 0x100);// nec.I.regs.w[4];
    //        /*PUSH(nec.I.regs.w[0]);
    //        nec.PUSH(nec.I.regs.w[1]);
    //        nec.PUSH(nec.I.regs.w[2]);
    //        nec.PUSH(nec.I.regs.w[3]);*/
    //        nec.PUSH((ushort)(nec.I.regs.b[0] + nec.I.regs.b[1] * 0x100));
    //        nec.PUSH((ushort)(nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100));
    //        nec.PUSH((ushort)(nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100));
    //        nec.PUSH((ushort)(nec.I.regs.b[6] + nec.I.regs.b[7] * 0x100));
    //        nec.PUSH(tmp);
    //        /*PUSH(nec.I.regs.w[5]);
    //        nec.PUSH(nec.I.regs.w[6]);
    //        nec.PUSH(nec.I.regs.w[7]);*/
    //        nec.PUSH((ushort)(nec.I.regs.b[10] + nec.I.regs.b[11] * 0x100));
    //        nec.PUSH((ushort)(nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100));
    //        nec.PUSH((ushort)(nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100));
    //        nec.CLKS(67, 35, 20);
    //    }
    //    void i_popa()
    //    {
    //        ushort tmp = 0;
    //        /*POP(ref nec.I.regs.w[7]);
    //        nec.POP(ref nec.I.regs.w[6]);
    //        nec.POP(ref nec.I.regs.w[5]);*/
    //        nec.POPW(7);
    //        nec.POPW(6);
    //        nec.POPW(5);
    //        nec.POP(ref tmp);
    //        /*POP(ref nec.I.regs.w[3]);
    //        nec.POP(ref nec.I.regs.w[2]);
    //        nec.POP(ref nec.I.regs.w[1]);
    //        nec.POP(ref nec.I.regs.w[0]);*/
    //        nec.POPW(3);
    //        nec.POPW(2);
    //        nec.POPW(1);
    //        nec.POPW(0);
    //        nec.CLKS(75, 43, 22);
    //    }
    //    void i_chkind()
    //    {
    //        int low, high, tmp;
    //        int ModRM;
    //        ModRM = nec.GetModRM();
    //        low = nec.GetRMWord(ModRM);
    //        high = nec.GetnextRMWord();
    //        tmp = nec.RegWord(ModRM);
    //        if (tmp < low || tmp > high)
    //        {
    //            nec.nec_interrupt(5, false);
    //        }
    //        nec.pendingCycles -= 20;
    //    }
    //    void i_brkn()
    //    {
    //        nec.nec_interrupt(nec.FETCH(), true);
    //        nec.CLKS(50, 50, 24);
    //    }
    //    void i_repnc()
    //    {
    //        int next = nec.fetchop();
    //        ushort c = (ushort)(nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100);// nec.I.regs.w[1];
    //        switch (next)
    //        { /* Segments */
    //            case 0x26: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[0] << 4); next = nec.fetchop(); nec.CLK(2); break;
    //            case 0x2e: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[1] << 4); next = nec.fetchop(); nec.CLK(2); break;
    //            case 0x36: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[2] << 4); next = nec.fetchop(); nec.CLK(2); break;
    //            case 0x3e: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[3] << 4); next = nec.fetchop(); nec.CLK(2); break;
    //        }
    //        switch (next)
    //        {
    //            case 0x6c: nec.CLK(2); if (c != 0) do { i_insb(); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); /*nec.I.regs.w[1] = c;*/ break;
    //            case 0x6d: nec.CLK(2); if (c != 0) do { i_insw(); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0x6e: nec.CLK(2); if (c != 0) do { i_outsb(); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0x6f: nec.CLK(2); if (c != 0) do { i_outsw(); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xa4: nec.CLK(2); if (c != 0) do { i_movsb(); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xa5: nec.CLK(2); if (c != 0) do { i_movsw(); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xa6: nec.CLK(2); if (c != 0) do { i_cmpsb(); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xa7: nec.CLK(2); if (c != 0) do { i_cmpsw(); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xaa: nec.CLK(2); if (c != 0) do { i_stosb(); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xab: nec.CLK(2); if (c != 0) do { i_stosw(); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xac: nec.CLK(2); if (c != 0) do { i_lodsb(); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xad: nec.CLK(2); if (c != 0) do { i_lodsw(); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xae: nec.CLK(2); if (c != 0) do { i_scasb(); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xaf: nec.CLK(2); if (c != 0) do { i_scasw(); c--; } while (c > 0 && !nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            default:
    //                nec.nec_instruction[next]();
    //                //DoInstructionOpCode(next);
    //                break;
    //        }
    //        Nec.seg_prefix = 0;
    //    }
    //    void i_repc()
    //    {
    //        int next = nec.fetchop();
    //        ushort c = (ushort)(nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100);// nec.I.regs.w[1];
    //        switch (next)
    //        { /* Segments */
    //            case 0x26: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[0] << 4); next = nec.fetchop(); nec.CLK(2); break;
    //            case 0x2e: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[1] << 4); next = nec.fetchop(); nec.CLK(2); break;
    //            case 0x36: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[2] << 4); next = nec.fetchop(); nec.CLK(2); break;
    //            case 0x3e: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[3] << 4); next = nec.fetchop(); nec.CLK(2); break;
    //        }
    //        switch (next)
    //        {
    //            case 0x6c: nec.CLK(2); if (c != 0) do { i_insb(); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100);/*nec.I.regs.w[1] = c;*/ break;
    //            case 0x6d: nec.CLK(2); if (c != 0) do { i_insw(); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0x6e: nec.CLK(2); if (c != 0) do { i_outsb(); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0x6f: nec.CLK(2); if (c != 0) do { i_outsw(); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xa4: nec.CLK(2); if (c != 0) do { i_movsb(); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xa5: nec.CLK(2); if (c != 0) do { i_movsw(); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xa6: nec.CLK(2); if (c != 0) do { i_cmpsb(); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xa7: nec.CLK(2); if (c != 0) do { i_cmpsw(); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xaa: nec.CLK(2); if (c != 0) do { i_stosb(); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xab: nec.CLK(2); if (c != 0) do { i_stosw(); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xac: nec.CLK(2); if (c != 0) do { i_lodsb(); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xad: nec.CLK(2); if (c != 0) do { i_lodsw(); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xae: nec.CLK(2); if (c != 0) do { i_scasb(); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xaf: nec.CLK(2); if (c != 0) do { i_scasw(); c--; } while (c > 0 && nec.CF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            default:
    //                nec.nec_instruction[next]();
    //                //DoInstructionOpCode(next);
    //                break;
    //        }
    //        Nec.seg_prefix = 0;
    //    }
    //    void i_push_d16()
    //    {
    //        int tmp;
    //        tmp = nec.FETCHWORD();
    //        nec.PUSH((ushort)tmp);
    //        //nec.CLKW(12, 12, 5, 12, 8, 5, nec.I.regs.w[4]);
    //        nec.CLKW(12, 12, 5, 12, 8, 5, nec.I.regs.b[8] + nec.I.regs.b[9] * 0x100);
    //    }
    //    void i_imul_d16()
    //    {
    //        int tmp;
    //        int ModRM;
    //        ushort src, dst;
    //        nec.DEF_r16w(out ModRM, out src, out dst);
    //        tmp = nec.FETCHWORD();
    //        dst = (ushort)((int)((short)src) * (int)((short)tmp));
    //        nec.I.CarryVal = nec.I.OverVal = (uint)(((((int)dst) >> 15 != 0) && (((int)dst) >> 15 != -1)) ? 1 : 0);
    //        //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = (ushort)dst;
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
    //        nec.pendingCycles -= (ModRM >= 0xc0) ? 38 : 47;
    //    }
    //    void i_push_d8()
    //    {
    //        int tmp = (ushort)((short)((sbyte)nec.FETCH()));
    //        nec.PUSH((ushort)tmp);
    //        //nec.CLKW(11, 11, 5, 11, 7, 3, nec.I.regs.w[4]);
    //        nec.CLKW(11, 11, 5, 11, 7, 3, nec.I.regs.b[8] + nec.I.regs.b[9] * 0x100);
    //    }
    //    void i_imul_d8()
    //    {
    //        int src2;
    //        int ModRM;
    //        ushort src, dst;
    //        nec.DEF_r16w(out ModRM, out src, out dst);
    //        src2 = (ushort)((short)((sbyte)nec.FETCH()));
    //        dst = (ushort)((int)((short)src) * (int)((short)src2));
    //        nec.I.CarryVal = nec.I.OverVal = (uint)(((((int)dst) >> 15 != 0) && (((int)dst) >> 15 != -1)) ? 1 : 0);
    //        //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = (ushort)dst;
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
    //        nec.pendingCycles -= (ModRM >= 0xc0) ? 31 : 39;
    //    }
    //    void i_insb()
    //    {
    //        //PutMemB(0, nec.I.regs.w[7], ReadIOByte(nec.I.regs.w[2]));
    //        nec.PutMemB(0, nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100, ReadIOByte(nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100));
    //        //nec.I.regs.w[7] += (ushort)(-2 * (nec.I.DF ? 1 : 0) + 1);
    //        ushort w7 = (ushort)(nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100);
    //        w7 += (ushort)(-2 * (nec.I.DF ? 1 : 0) + 1);
    //        nec.I.regs.b[14] = (byte)(w7 % 0x100);
    //        nec.I.regs.b[15] = (byte)(w7 / 0x100);
    //        nec.CLK(8);
    //    }
    //    void i_insw()
    //    {
    //        //PutMemW(0, nec.I.regs.w[7], nec.ReadIOWord(nec.I.regs.w[2]));
    //        nec.PutMemW(0, nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100, nec.ReadIOWord(nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100));
    //        //nec.I.regs.w[7] += (ushort)(-4 * (nec.I.DF ? 1 : 0) + 2);
    //        ushort w7 = (ushort)(nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100);
    //        w7 += (ushort)(-4 * (nec.I.DF ? 1 : 0) + 2);
    //        nec.I.regs.b[14] = (byte)(w7 % 0x100);
    //        nec.I.regs.b[15] = (byte)(w7 / 0x100);
    //        nec.CLKS(18, 10, 8);
    //    }
    //    void i_outsb()
    //    {
    //        //WriteIOByte(nec.I.regs.w[2], nec.GetMemB(3, nec.I.regs.w[6]));
    //        WriteIOByte(nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100, nec.GetMemB(3, nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100));
    //        //nec.I.regs.w[6] += (ushort)(-2 * (nec.I.DF ? 1 : 0) + 1);
    //        ushort w6 = (ushort)(nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100);
    //        w6 += (ushort)(-2 * (nec.I.DF ? 1 : 0) + 1);
    //        nec.I.regs.b[12] = (byte)(w6 % 0x100);
    //        nec.I.regs.b[13] = (byte)(w6 / 0x100);
    //        nec.CLK(8);
    //    }
    //    void i_outsw()
    //    {
    //        //WriteIOWord(nec.I.regs.w[2], nec.GetMemW(3, nec.I.regs.w[6]));
    //        nec.WriteIOWord(nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100, nec.GetMemW(3, nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100));
    //        //nec.I.regs.w[6] += (ushort)(-4 * (nec.I.DF ? 1 : 0) + 2);
    //        ushort w6 = (ushort)(nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100);
    //        w6 += (ushort)(-4 * (nec.I.DF ? 1 : 0) + 2);
    //        nec.I.regs.b[12] = (byte)(w6 % 0x100);
    //        nec.I.regs.b[13] = (byte)(w6 / 0x100);
    //        nec.CLKS(18, 10, 8);
    //    }
    //    void i_jo()
    //    {
    //        bool b1 = nec.OF();
    //        nec.JMP(b1);
    //        if (!b1)
    //        {
    //            nec.CLKS(4, 4, 3);
    //        }
    //    }
    //    void i_jno()
    //    {
    //        bool b1 = !nec.OF();
    //        nec.JMP(b1);
    //        if (!b1)
    //        {
    //            nec.CLKS(4, 4, 3);
    //        }
    //    }
    //    void i_jc()
    //    {
    //        bool b1 = nec.CF();
    //        nec.JMP(b1);
    //        if (!b1)
    //        {
    //            nec.CLKS(4, 4, 3);
    //        }
    //    }
    //    void i_jnc()
    //    {
    //        bool b1 = !nec.CF();
    //        nec.JMP(b1);
    //        if (!b1)
    //        {
    //            nec.CLKS(4, 4, 3);
    //        }
    //    }
    //    void i_jz()
    //    {
    //        bool b1 = nec.ZF();
    //        nec.JMP(b1);
    //        if (!b1)
    //        {
    //            nec.CLKS(4, 4, 3);
    //        }
    //    }
    //    void i_jnz()
    //    {
    //        bool b1 = !nec.ZF();
    //        nec.JMP(b1);
    //        if (!b1)
    //        {
    //            nec.CLKS(4, 4, 3);
    //        }
    //    }
    //    void i_jce()
    //    {
    //        bool b1 = nec.CF() || nec.ZF();
    //        nec.JMP(b1);
    //        if (!b1)
    //        {
    //            nec.CLKS(4, 4, 3);
    //        }
    //    }
    //    void i_jnce()
    //    {
    //        bool b1 = !(nec.CF() || nec.ZF());
    //        nec.JMP(b1);
    //        if (!b1)
    //        {
    //            nec.CLKS(4, 4, 3);
    //        }
    //    }
    //    void i_js()
    //    {
    //        bool b1 = nec.SF();
    //        nec.JMP(b1);
    //        if (!b1)
    //        {
    //            nec.CLKS(4, 4, 3);
    //        }
    //    }
    //    void i_jns()
    //    {
    //        bool b1 = !nec.SF();
    //        nec.JMP(b1);
    //        if (!b1)
    //        {
    //            nec.CLKS(4, 4, 3);
    //        }
    //    }
    //    void i_jp()
    //    {
    //        bool b1 = PF();
    //        nec.JMP(b1);
    //        if (!b1)
    //        {
    //            nec.CLKS(4, 4, 3);
    //        }
    //    }
    //    void i_jnp()
    //    {
    //        bool b1 = !PF();
    //        nec.JMP(b1);
    //        if (!b1)
    //        {
    //            nec.CLKS(4, 4, 3);
    //        }
    //    }
    //    void i_jl()
    //    {
    //        bool b1 = (nec.SF() != nec.OF()) && (!nec.ZF());
    //        nec.JMP(b1);
    //        if (!b1)
    //        {
    //            nec.CLKS(4, 4, 3);
    //        }
    //    }
    //    void i_jnl()
    //    {
    //        bool b1 = (nec.ZF()) || (nec.SF() == nec.OF());
    //        nec.JMP(b1);
    //        if (!b1)
    //        {
    //            nec.CLKS(4, 4, 3);
    //        }
    //    }
    //    void i_jle()
    //    {
    //        bool b1 = (nec.ZF()) || (nec.SF() != nec.OF());
    //        nec.JMP(b1);
    //        if (!b1)
    //        {
    //            nec.CLKS(4, 4, 3);
    //        }
    //    }
    //    void i_jnle()
    //    {
    //        bool b1 = (nec.SF() == nec.OF()) && (!nec.ZF());
    //        nec.JMP(b1);
    //        if (!b1)
    //        {
    //            nec.CLKS(4, 4, 3);
    //        }
    //    }
    //    void i_80pre()
    //    {
    //        int ModRM;
    //        byte src, dst;
    //        ModRM = nec.GetModRM();
    //        dst = nec.GetRMByte(ModRM);
    //        src = nec.FETCH();
    //        if (ModRM >= 0xc0)
    //        {
    //            nec.CLKS(4, 4, 2);
    //        }
    //        else if ((ModRM & 0x38) == 0x38)
    //        {
    //            nec.CLKS(13, 13, 6);
    //        }
    //        else
    //        {
    //            nec.CLKS(18, 18, 7);
    //        }
    //        switch (ModRM & 0x38)
    //        {
    //            case 0x00: nec.ADDB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
    //            case 0x08: nec.ORB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
    //            case 0x10: src += (byte)(nec.CF() ? 1 : 0); nec.ADDB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
    //            case 0x18: src += (byte)(nec.CF() ? 1 : 0); nec.SUBB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
    //            case 0x20: nec.ANDB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
    //            case 0x28: nec.SUBB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
    //            case 0x30: nec.XORB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
    //            case 0x38: nec.SUBB(ref src, ref dst); break;
    //        }
    //    }
    //    void i_81pre()
    //    {
    //        int ModRM;
    //        ushort src, dst;
    //        ModRM = nec.GetModRM();
    //        dst = nec.GetRMWord(ModRM);
    //        src = nec.FETCH();
    //        src += (ushort)(nec.FETCH() << 8);
    //        if (ModRM >= 0xc0)
    //        {
    //            nec.CLKS(4, 4, 2);
    //        }
    //        else if ((ModRM & 0x38) == 0x38)
    //        {
    //            nec.CLKW(17, 17, 8, 17, 13, 6, Nec.EA);
    //        }
    //        else
    //        {
    //            nec.CLKW(26, 26, 11, 26, 18, 7, Nec.EA);
    //        }
    //        switch (ModRM & 0x38)
    //        {
    //            case 0x00: nec.ADDW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
    //            case 0x08: nec.ORW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
    //            case 0x10: src += (ushort)(nec.CF() ? 1 : 0); nec.ADDW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
    //            case 0x18: src += (ushort)(nec.CF() ? 1 : 0); nec.SUBW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
    //            case 0x20: nec.ANDW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
    //            case 0x28: nec.SUBW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
    //            case 0x30: nec.XORW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
    //            case 0x38: nec.SUBW(ref src, ref dst); break;
    //        }
    //    }
    //    void i_82pre()
    //    {
    //        int ModRM;
    //        byte src, dst;
    //        ModRM = nec.GetModRM();
    //        dst = nec.GetRMByte(ModRM);
    //        src = (byte)((sbyte)nec.FETCH());
    //        if (ModRM >= 0xc0)
    //        {
    //            nec.CLKS(4, 4, 2);
    //        }
    //        else if ((ModRM & 0x38) == 0x38)
    //        {
    //            nec.CLKS(13, 13, 6);
    //        }
    //        else
    //        {
    //            nec.CLKS(18, 18, 7);
    //        }
    //        switch (ModRM & 0x38)
    //        {
    //            case 0x00: nec.ADDB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
    //            case 0x08: nec.ORB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
    //            case 0x10: src += (byte)(nec.CF() ? 1 : 0); nec.ADDB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
    //            case 0x18: src += (byte)(nec.CF() ? 1 : 0); nec.SUBB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
    //            case 0x20: nec.ANDB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
    //            case 0x28: nec.SUBB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
    //            case 0x30: nec.XORB(ref src, ref dst); nec.PutbackRMByte(ModRM, dst); break;
    //            case 0x38: nec.SUBB(ref src, ref dst); break;
    //        }
    //    }
    //    void i_83pre()
    //    {
    //        int ModRM;
    //        ushort src, dst;
    //        ModRM = nec.GetModRM();
    //        dst = nec.GetRMWord(ModRM);
    //        src = (ushort)((short)((sbyte)nec.FETCH()));
    //        if (ModRM >= 0xc0)
    //        {
    //            nec.CLKS(4, 4, 2);
    //        }
    //        else if ((ModRM & 0x38) == 0x38)
    //        {
    //            nec.CLKW(17, 17, 8, 17, 13, 6, Nec.EA);
    //        }
    //        else
    //        {
    //            nec.CLKW(26, 26, 11, 26, 18, 7, Nec.EA);
    //        }
    //        switch (ModRM & 0x38)
    //        {
    //            case 0x00: nec.ADDW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
    //            case 0x08: nec.ORW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
    //            case 0x10: src += (ushort)(nec.CF() ? 1 : 0); nec.ADDW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
    //            case 0x18: src += (ushort)(nec.CF() ? 1 : 0); nec.SUBW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
    //            case 0x20: nec.ANDW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
    //            case 0x28: nec.SUBW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
    //            case 0x30: nec.XORW(ref src, ref dst); nec.PutbackRMWord(ModRM, dst); break;
    //            case 0x38: nec.SUBW(ref src, ref dst); break;
    //        }
    //    }
    //    void i_test_br8()
    //    {
    //        int ModRM;
    //        byte src, dst;
    //        nec.DEF_br8(out ModRM, out src, out dst);
    //        nec.ANDB(ref src, ref dst);
    //        nec.CLKM(ModRM, 2, 2, 2, 10, 10, 6);
    //    }
    //    void i_test_wr16()
    //    {
    //        int ModRM;
    //        ushort src, dst;
    //        nec.DEF_wr16(out ModRM, out src, out dst);
    //        nec.ANDW(ref src, ref dst);
    //        nec.CLKR(ModRM, 14, 14, 8, 14, 10, 6, 2, Nec.EA);
    //    }
    //    void i_xchg_br8()
    //    {
    //        int ModRM;
    //        byte src, dst;
    //        nec.DEF_br8(out ModRM, out src, out dst);
    //        nec.I.regs.b[nec.mod_RM.regb[ModRM]] = dst;
    //        nec.PutbackRMByte(ModRM, src);
    //        nec.CLKM(ModRM, 3, 3, 3, 16, 18, 8);
    //    }
    //    void i_xchg_wr16()
    //    {
    //        int ModRM;
    //        ushort src, dst;
    //        nec.DEF_wr16(out ModRM, out src, out dst);
    //        //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = dst;
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(dst % 0x100);
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(dst / 0x100);
    //        nec.PutbackRMWord(ModRM, src);
    //        nec.CLKR(ModRM, 24, 24, 12, 24, 16, 8, 3, Nec.EA);
    //    }
    //    void i_mov_br8()
    //    {
    //        int ModRM;
    //        byte src;
    //        ModRM = nec.GetModRM();
    //        src = nec.I.regs.b[nec.mod_RM.regb[ModRM]];
    //        PutRMByte(ModRM, src);
    //        nec.CLKM(ModRM, 2, 2, 2, 9, 9, 3);
    //    }
    //    void i_mov_wr16()
    //    {
    //        int ModRM;
    //        ushort src;
    //        ModRM = nec.GetModRM();
    //        //src = nec.I.regs.w[nec.mod_RM.regw[ModRM]];
    //        src = (ushort)(nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] + nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] * 0x100);
    //        PutRMWord(ModRM, src);
    //        nec.CLKR(ModRM, 13, 13, 5, 13, 9, 3, 2, Nec.EA);
    //    }
    //    void i_mov_r8b()
    //    {
    //        int ModRM;
    //        byte src;
    //        ModRM = nec.GetModRM();
    //        src = nec.GetRMByte(ModRM);
    //        nec.I.regs.b[nec.mod_RM.regb[ModRM]] = src;
    //        nec.CLKM(ModRM, 2, 2, 2, 11, 11, 5);
    //    }
    //    void i_mov_r16w()
    //    {
    //        int ModRM;
    //        ushort src;
    //        ModRM = nec.GetModRM();
    //        src = nec.GetRMWord(ModRM);
    //        //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = src;
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(src % 0x100);
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(src / 0x100);
    //        nec.CLKR(ModRM, 15, 15, 7, 15, 11, 5, 2, Nec.EA);
    //    }
    //    void i_mov_wsreg()
    //    {
    //        int ModRM;
    //        ModRM = nec.GetModRM();
    //        PutRMWord(ModRM, nec.I.sregs[(ModRM & 0x38) >> 3]);
    //        nec.CLKR(ModRM, 14, 14, 5, 14, 10, 3, 2, Nec.EA);
    //    }
    //    void i_lea()
    //    {
    //        int ModRM = nec.FETCH();
    //        nec.GetEA[ModRM]();
    //        //DoNecnec.GetEAOpCode(ModRM);


    //        //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = EO;
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(EO % 0x100);
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(EO / 0x100);
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_mov_sregw()
    //    {
    //        int ModRM;
    //        ushort src;
    //        ModRM = nec.GetModRM();
    //        src = nec.GetRMWord(ModRM);
    //        nec.CLKR(ModRM, 15, 15, 7, 15, 11, 5, 2, Nec.EA);
    //        switch (ModRM & 0x38)
    //        {
    //            case 0x00: nec.I.sregs[0] = src; break; /* mov es,ew */
    //            case 0x08: nec.I.sregs[1] = src; break; /* mov cs,ew */
    //            case 0x10: nec.I.sregs[2] = src; break; /* mov ss,ew */
    //            case 0x18: nec.I.sregs[3] = src; break; /* mov ds,ew */
    //            default: break;
    //        }
    //        nec.I.no_interrupt = 1;
    //    }
    //    void i_popw()
    //    {
    //        int ModRM;
    //        ushort tmp = 0;
    //        ModRM = nec.GetModRM();
    //        nec.POP(ref tmp);
    //        PutRMWord(ModRM, tmp);
    //        nec.pendingCycles -= 21;
    //    }
    //    void i_nop()
    //    {
    //        nec.CLK(3);
    //        if (nec.I.no_interrupt == 0 && nec.pendingCycles > 0 && (nec.I.pending_irq == 0) && (PEEKOP((uint)((nec.I.sregs[1] << 4) + nec.I.ip))) == 0xeb && (PEEK((uint)((nec.I.sregs[1] << 4) + nec.I.ip + 1))) == 0xfd)
    //            nec.pendingCycles %= 15;
    //    }
    //    void i_xchg_axcx()
    //    {
    //        nec.XchgAWReg(1);
    //        nec.CLK(3);
    //    }
    //    void i_xchg_axdx()
    //    {
    //        nec.XchgAWReg(2);
    //        nec.CLK(3);
    //    }
    //    void i_xchg_axbx()
    //    {
    //        nec.XchgAWReg(3);
    //        nec.CLK(3);
    //    }
    //    void i_xchg_axsp()
    //    {
    //        nec.XchgAWReg(4);
    //        nec.CLK(3);
    //    }
    //    void i_xchg_axbp()
    //    {
    //        nec.XchgAWReg(5);
    //        nec.CLK(3);
    //    }
    //    void i_xchg_axsi()
    //    {
    //        nec.XchgAWReg(6);
    //        nec.CLK(3);
    //    }
    //    void i_xchg_axdi()
    //    {
    //        nec.XchgAWReg(7);
    //        nec.CLK(3);
    //    }
    //    void i_cbw()
    //    {
    //        nec.I.regs.b[1] = (byte)(((nec.I.regs.b[0] & 0x80) != 0) ? 0xff : 0);
    //        nec.CLK(2);
    //    }
    //    void i_cwd()
    //    {
    //        //nec.I.regs.w[2] = (ushort)(((nec.I.regs.b[1] & 0x80) != 0) ? 0xffff : 0);
    //        ushort w2 = (ushort)(((nec.I.regs.b[1] & 0x80) != 0) ? 0xffff : 0);
    //        nec.I.regs.b[4] = (byte)(w2 % 0x100);
    //        nec.I.regs.b[5] = (byte)(w2 / 0x100);
    //        nec.CLK(4);
    //    }
    //    void i_call_far()
    //    {
    //        ushort tmp, tmp2;
    //        tmp = nec.FETCHWORD();
    //        tmp2 = nec.FETCHWORD();
    //        nec.PUSH(nec.I.sregs[1]);
    //        nec.PUSH(nec.I.ip);
    //        nec.I.ip = (ushort)tmp;
    //        nec.I.sregs[1] = (ushort)tmp2;
    //        //CHANGE_PC;
    //        nec.CLKW(29, 29, 13, 29, 21, 9, nec.I.regs.b[8] + nec.I.regs.b[9] * 0x100);
    //    }
    //    void i_wait()
    //    {
    //        if (!nec.I.poll_state)
    //        {
    //            nec.I.ip--;
    //        }
    //        nec.CLK(5);
    //    }
    //    void i_pushf()
    //    {
    //        ushort tmp = nec.CompressFlags();
    //        nec.PUSH(tmp);
    //        nec.CLKS(12, 8, 3);
    //    }
    //    void nec.i_popf()
    //    {
    //        ushort tmp = 0;
    //        nec.POP(ref tmp);
    //        nec.ExpandFlags(tmp);
    //        nec.CLKS(12, 8, 5);
    //        if (nec.I.TF)
    //        {
    //            nec_trap();
    //        }
    //    }
    //    void i_sahf()
    //    {
    //        ushort tmp = (ushort)((nec.CompressFlags() & 0xff00) | (nec.I.regs.b[1] & 0xd5));
    //        nec.ExpandFlags(tmp);
    //        nec.CLKS(3, 3, 2);
    //    }
    //    void i_lahf()
    //    {
    //        nec.I.regs.b[1] = (byte)(nec.CompressFlags() & 0xff);
    //        nec.CLKS(3, 3, 2);
    //    }
    //    void i_mov_aldisp()
    //    {
    //        ushort addr;
    //        addr = nec.FETCHWORD();
    //        nec.I.regs.b[0] = nec.GetMemB(3, addr);
    //        nec.CLKS(10, 10, 5);
    //    }
    //    void i_mov_axdisp()
    //    {
    //        ushort addr;
    //        addr = nec.FETCHWORD();
    //        //nec.I.regs.w[0] = nec.GetMemW(3, addr);
    //        ushort w0 = nec.GetMemW(3, addr);
    //        nec.I.regs.b[0] = (byte)(w0 % 0x100);
    //        nec.I.regs.b[1] = (byte)(w0 / 0x100);
    //        nec.CLKW(14, 14, 7, 14, 10, 5, addr);
    //    }
    //    void i_mov_dispal()
    //    {
    //        ushort addr;
    //        addr = nec.FETCHWORD();
    //        nec.PutMemB(3, addr, nec.I.regs.b[0]);
    //        nec.CLKS(9, 9, 3);
    //    }
    //    void i_mov_dispax()
    //    {
    //        ushort addr;
    //        addr = nec.FETCHWORD();
    //        nec.PutMemW(3, addr, (ushort)(nec.I.regs.b[0] + nec.I.regs.b[1] * 0x100));
    //        nec.CLKW(13, 13, 5, 13, 9, 3, addr);
    //    }
    //    void i_movsb()
    //    {
    //        byte tmp = nec.GetMemB(3, nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100);
    //        nec.PutMemB(0, nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100, tmp);
    //        //nec.I.regs.w[7] += (ushort)(-2 * (nec.I.DF ? 1 : 0) + 1);
    //        //nec.I.regs.w[6] += (ushort)(-2 * (nec.I.DF ? 1 : 0) + 1);
    //        ushort w7 = (ushort)(nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100 + (-2 * (nec.I.DF ? 1 : 0) + 1));
    //        nec.I.regs.b[14] = (byte)(w7 % 0x100);
    //        nec.I.regs.b[15] = (byte)(w7 / 0x100);
    //        ushort w6 = (ushort)(nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100 + (-2 * (nec.I.DF ? 1 : 0) + 1));
    //        nec.I.regs.b[12] = (byte)(w6 % 0x100);
    //        nec.I.regs.b[13] = (byte)(w6 / 0x100);
    //        nec.CLKS(8, 8, 6);
    //    }
    //    void i_movsw()
    //    {
    //        ushort tmp = nec.GetMemW(3, nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100);
    //        nec.PutMemW(0, nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100, tmp);
    //        //nec.I.regs.w[7] += (ushort)(-4 * (nec.I.DF ? 1 : 0) + 2);
    //        //nec.I.regs.w[6] += (ushort)(-4 * (nec.I.DF ? 1 : 0) + 2);
    //        ushort w7 = (ushort)(nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100 + (-4 * (nec.I.DF ? 1 : 0) + 2));
    //        nec.I.regs.b[14] = (byte)(w7 % 0x100);
    //        nec.I.regs.b[15] = (byte)(w7 / 0x100);
    //        ushort w6 = (ushort)(nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100 + (-4 * (nec.I.DF ? 1 : 0) + 2));
    //        nec.I.regs.b[12] = (byte)(w6 % 0x100);
    //        nec.I.regs.b[13] = (byte)(w6 / 0x100);
    //        nec.CLKS(16, 16, 10);
    //    }
    //    void i_cmpsb()
    //    {
    //        byte src = nec.GetMemB(0, nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100);
    //        byte dst = nec.GetMemB(3, nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100);
    //        nec.SUBB(ref src, ref dst);
    //        //nec.I.regs.w[7] += (ushort)(-2 * (nec.I.DF ? 1 : 0) + 1);
    //        //nec.I.regs.w[6] += (ushort)(-2 * (nec.I.DF ? 1 : 0) + 1);
    //        ushort w7 = (ushort)(nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100 + (-2 * (nec.I.DF ? 1 : 0) + 1));
    //        nec.I.regs.b[14] = (byte)(w7 % 0x100);
    //        nec.I.regs.b[15] = (byte)(w7 / 0x100);
    //        ushort w6 = (ushort)(nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100 + (-2 * (nec.I.DF ? 1 : 0) + 1));
    //        nec.I.regs.b[12] = (byte)(w6 % 0x100);
    //        nec.I.regs.b[13] = (byte)(w6 / 0x100);
    //        nec.CLKS(14, 14, 14);
    //    }
    //    void i_cmpsw()
    //    {
    //        ushort src = nec.GetMemW(0, nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100);
    //        ushort dst = nec.GetMemW(3, nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100);
    //        nec.SUBW(ref src, ref dst);
    //        //nec.I.regs.w[7] += (ushort)(-4 * (nec.I.DF ? 1 : 0) + 2);
    //        //nec.I.regs.w[6] += (ushort)(-4 * (nec.I.DF ? 1 : 0) + 2);
    //        ushort w7 = (ushort)(nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100 + (-4 * (nec.I.DF ? 1 : 0) + 2));
    //        nec.I.regs.b[14] = (byte)(w7 % 0x100);
    //        nec.I.regs.b[15] = (byte)(w7 / 0x100);
    //        ushort w6 = (ushort)(nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100 + (-4 * (nec.I.DF ? 1 : 0) + 2));
    //        nec.I.regs.b[12] = (byte)(w6 % 0x100);
    //        nec.I.regs.b[13] = (byte)(w6 / 0x100);
    //        nec.CLKS(14, 14, 14);
    //    }
    //    void i_test_ald8()
    //    {
    //        byte src, dst;
    //        nec.DEF_ald8(out src, out dst);
    //        nec.ANDB(ref src, ref dst);
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_test_axd16()
    //    {
    //        ushort src, dst;
    //        nec.DEF_axd16(out src, out dst);
    //        nec.ANDW(ref src, ref dst);
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_stosb()
    //    {
    //        nec.PutMemB(0, nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100, nec.I.regs.b[0]);
    //        //nec.I.regs.w[7] += (ushort)(-2 * (nec.I.DF ? 1 : 0) + 1);
    //        ushort w7 = (ushort)(nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100 + (-2 * (nec.I.DF ? 1 : 0) + 1));
    //        nec.I.regs.b[14] = (byte)(w7 % 0x100);
    //        nec.I.regs.b[15] = (byte)(w7 / 0x100);
    //        nec.CLKS(4, 4, 3);
    //    }
    //    void i_stosw()
    //    {
    //        nec.PutMemW(0, nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100, (ushort)(nec.I.regs.b[0] + nec.I.regs.b[1] * 0x100));
    //        //nec.I.regs.w[7] += (ushort)(-4 * (nec.I.DF ? 1 : 0) + 2);
    //        ushort w7 = (ushort)(nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100 + (-4 * (nec.I.DF ? 1 : 0) + 2));
    //        nec.I.regs.b[14] = (byte)(w7 % 0x100);
    //        nec.I.regs.b[15] = (byte)(w7 / 0x100);
    //        nec.CLKW(8, 8, 5, 8, 4, 3, nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100);
    //    }
    //    void i_lodsb()
    //    {
    //        nec.I.regs.b[0] = nec.GetMemB(3, nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100);
    //        //nec.I.regs.w[6] += (ushort)(-2 * (nec.I.DF ? 1 : 0) + 1);
    //        ushort w6 = (ushort)(nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100 + (-2 * (nec.I.DF ? 1 : 0) + 1));
    //        nec.I.regs.b[12] = (byte)(w6 % 0x100);
    //        nec.I.regs.b[13] = (byte)(w6 / 0x100);
    //        nec.CLKS(4, 4, 3);
    //    }
    //    void i_lodsw()
    //    {
    //        ushort w0 = nec.GetMemW(3, nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100);
    //        nec.I.regs.b[0] = (byte)(w0 % 0x100);
    //        nec.I.regs.b[1] = (byte)(w0 / 0x100);
    //        //nec.I.regs.w[0] = nec.GetMemW(3, nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100);
    //        //nec.I.regs.w[6] += (ushort)(-4 * (nec.I.DF ? 1 : 0) + 2);
    //        ushort w6 = (ushort)(nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100 + (-4 * (nec.I.DF ? 1 : 0) + 2));
    //        nec.I.regs.b[12] = (byte)(w6 % 0x100);
    //        nec.I.regs.b[13] = (byte)(w6 / 0x100);
    //        nec.CLKW(8, 8, 5, 8, 4, 3, nec.I.regs.b[12] + nec.I.regs.b[13] * 0x100);
    //    }
    //    void i_scasb()
    //    {
    //        byte src = nec.GetMemB(0, nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100);
    //        byte dst = nec.I.regs.b[0];
    //        nec.SUBB(ref src, ref dst);
    //        //nec.I.regs.w[7] += (ushort)(-2 * (nec.I.DF ? 1 : 0) + 1);
    //        ushort w7 = (ushort)(nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100 + (-2 * (nec.I.DF ? 1 : 0) + 1));
    //        nec.I.regs.b[14] = (byte)(w7 % 0x100);
    //        nec.I.regs.b[15] = (byte)(w7 / 0x100);
    //        nec.CLKS(4, 4, 3);
    //    }
    //    void i_scasw()
    //    {
    //        ushort src = nec.GetMemW(0, nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100);
    //        ushort dst = (ushort)(nec.I.regs.b[0] + nec.I.regs.b[1] * 0x100);
    //        nec.SUBW(ref src, ref dst);
    //        //nec.I.regs.w[7] += (ushort)(-4 * (nec.I.DF ? 1 : 0) + 2);
    //        ushort w7 = (ushort)(nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100 + (-4 * (nec.I.DF ? 1 : 0) + 2));
    //        nec.I.regs.b[14] = (byte)(w7 % 0x100);
    //        nec.I.regs.b[15] = (byte)(w7 / 0x100);
    //        nec.CLKW(8, 8, 5, 8, 4, 3, nec.I.regs.b[14] + nec.I.regs.b[15] * 0x100);
    //    }
    //    void i_mov_ald8()
    //    {
    //        nec.I.regs.b[0] = nec.FETCH();
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_mov_cld8()
    //    {
    //        nec.I.regs.b[2] = nec.FETCH();
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_mov_dld8()
    //    {
    //        nec.I.regs.b[4] = nec.FETCH();
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_mov_bld8()
    //    {
    //        nec.I.regs.b[6] = nec.FETCH();
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_mov_ahd8()
    //    {
    //        nec.I.regs.b[1] = nec.FETCH();
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_mov_chd8()
    //    {
    //        nec.I.regs.b[3] = nec.FETCH();
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_mov_dhd8()
    //    {
    //        nec.I.regs.b[5] = nec.FETCH();
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_mov_bhd8()
    //    {
    //        nec.I.regs.b[7] = nec.FETCH();
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_mov_axd16()
    //    {
    //        nec.I.regs.b[0] = nec.FETCH();
    //        nec.I.regs.b[1] = nec.FETCH();
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_mov_cxd16()
    //    {
    //        nec.I.regs.b[2] = nec.FETCH();
    //        nec.I.regs.b[3] = nec.FETCH();
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_mov_dxd16()
    //    {
    //        nec.I.regs.b[4] = nec.FETCH();
    //        nec.I.regs.b[5] = nec.FETCH();
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_mov_bxd16()
    //    {
    //        nec.I.regs.b[6] = nec.FETCH();
    //        nec.I.regs.b[7] = nec.FETCH();
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_mov_spd16()
    //    {
    //        nec.I.regs.b[8] = nec.FETCH();
    //        nec.I.regs.b[9] = nec.FETCH();
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_mov_bpd16()
    //    {
    //        nec.I.regs.b[10] = nec.FETCH();
    //        nec.I.regs.b[11] = nec.FETCH();
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_mov_sid16()
    //    {
    //        nec.I.regs.b[12] = nec.FETCH();
    //        nec.I.regs.b[13] = nec.FETCH();
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_mov_did16()
    //    {
    //        nec.I.regs.b[14] = nec.FETCH();
    //        nec.I.regs.b[15] = nec.FETCH();
    //        nec.CLKS(4, 4, 2);
    //    }
    //    void i_rotshft_bd8()
    //    {
    //        int ModRM;
    //        int src, dst;
    //        byte c;
    //        ModRM = nec.GetModRM();
    //        src = nec.GetRMByte(ModRM);
    //        dst = src;
    //        c = nec.FETCH();
    //        nec.CLKM(ModRM, 7, 7, 2, 19, 19, 6);
    //        if (c != 0)
    //        {
    //            switch (ModRM & 0x38)
    //            {
    //                case 0x00: do {  nec.ROL_BYTE(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMByte(ModRM, (byte)dst); break;
    //                case 0x08: do { nec.ROR_BYTE(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMByte(ModRM, (byte)dst); break;
    //                case 0x10: do { nec.ROLC_BYTE(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMByte(ModRM, (byte)dst); break;
    //                case 0x18: do { nec.RORC_BYTE(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMByte(ModRM, (byte)dst); break;
    //                case 0x20: nec.SHL_BYTE(c, ref dst, ModRM); break;
    //                case 0x28: nec.SHR_BYTE(c, ref dst, ModRM); break;
    //                case 0x30: break;
    //                case 0x38: nec.SHRA_BYTE(c, ref dst, ModRM); break;
    //            }
    //        }
    //    }
    //    void i_rotshft_wd8()
    //    {
    //        int ModRM;
    //        int src, dst;
    //        byte c;
    //        ModRM = nec.GetModRM();
    //        src = nec.GetRMWord(ModRM);
    //        dst = src;
    //        c = nec.FETCH();
    //        nec.CLKM(ModRM, 7, 7, 2, 27, 19, 6);
    //        if (c != 0)
    //        {
    //            switch (ModRM & 0x38)
    //            {
    //                case 0x00: do { nec.ROL_WORD(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMWord(ModRM, (ushort)dst); break;
    //                case 0x08: do { nec.ROR_WORD(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMWord(ModRM, (ushort)dst); break;
    //                case 0x10: do { nec.ROLC_WORD(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMWord(ModRM, (ushort)dst); break;
    //                case 0x18: do { nec.RORC_WORD(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMWord(ModRM, (ushort)dst); break;
    //                case 0x20: nec.SHL_WORD(c, ref dst, ModRM); break;
    //                case 0x28: nec.SHR_WORD(c, ref dst, ModRM); break;
    //                case 0x30: break;
    //                case 0x38: nec.SHRA_WORD(c, ref dst, ModRM); break;
    //            }
    //        }
    //    }
    //    void i_ret_d16()
    //    {
    //        ushort count = nec.FETCH();
    //        count += (ushort)(nec.FETCH() << 8);
    //        nec.POP(ref nec.I.ip);
    //        //nec.I.regs.w[4] += count;
    //        ushort w4 = (ushort)(nec.I.regs.b[8] + nec.I.regs.b[9] * 0x100 + count);
    //        nec.I.regs.b[8] = (byte)(w4 % 0x100);
    //        nec.I.regs.b[9] = (byte)(w4 / 0x100);
    //        //CHANGE_PC;
    //        nec.CLKS(24, 24, 10);
    //    }
    //    void i_ret()
    //    {
    //        nec.POP(ref nec.I.ip);
    //        //CHANGE_PC;
    //        nec.CLKS(19, 19, 10);
    //    }
    //    void i_les_dw()
    //    {
    //        int ModRM;
    //        ModRM = nec.GetModRM();
    //        ushort tmp = nec.GetRMWord(ModRM);
    //        //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = tmp;
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(tmp % 0x100);
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(tmp / 0x100);
    //        nec.I.sregs[0] = nec.GetnextRMWord();
    //        nec.CLKW(26, 26, 14, 26, 18, 10, Nec.EA);
    //    }
    //    void i_lds_dw()
    //    {
    //        int ModRM;
    //        ModRM = nec.GetModRM();
    //        ushort tmp = nec.GetRMWord(ModRM);
    //        //nec.I.regs.w[nec.mod_RM.regw[ModRM]] = tmp;
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2] = (byte)(tmp % 0x100);
    //        nec.I.regs.b[nec.mod_RM.regw[ModRM] * 2 + 1] = (byte)(tmp / 0x100);
    //        nec.I.sregs[3] = nec.GetnextRMWord();
    //        nec.CLKW(26, 26, 14, 26, 18, 10, Nec.EA);
    //    }
    //    void i_mov_bd8()
    //    {
    //        int ModRM;
    //        ModRM = nec.GetModRM();
    //        PutImmRMByte(ModRM);
    //        nec.pendingCycles -= (ModRM >= 0xc0) ? 4 : 11;
    //    }
    //    void i_mov_wd16()
    //    {
    //        int ModRM;
    //        ModRM = nec.GetModRM();
    //        PutImmRMWord(ModRM);
    //        nec.pendingCycles -= (ModRM >= 0xc0) ? 4 : 15;
    //    }
    //    void i_enter()
    //    {
    //        ushort nb = nec.FETCH();
    //        int i, level;
    //        nec.pendingCycles -= 23;
    //        nb += (ushort)(nec.FETCH() << 8);
    //        level = nec.FETCH();
    //        nec.PUSH((ushort)(nec.I.regs.b[10] + nec.I.regs.b[11] * 0x100));
    //        //nec.I.regs.w[5] = nec.I.regs.w[4];
    //        nec.I.regs.b[10] = nec.I.regs.b[8];
    //        nec.I.regs.b[11] = nec.I.regs.b[9];
    //        //nec.I.regs.w[4] -= nb;
    //        ushort w4 = (ushort)(nec.I.regs.b[8] + nec.I.regs.b[9] * 0x100 - nb);
    //        nec.I.regs.b[8] = (byte)(w4 % 0x100);
    //        nec.I.regs.b[9] = (byte)(w4 / 0x100);
    //        for (i = 1; i < level; i++)
    //        {
    //            nec.PUSH(GetMemW(2, nec.I.regs.b[10] + nec.I.regs.b[11] * 0x100 - i * 2));
    //            nec.pendingCycles -= 16;
    //        }
    //        if (level != 0)
    //        {
    //            nec.PUSH((ushort)(nec.I.regs.b[10] + nec.I.regs.b[11] * 0x100));
    //        }
    //    }
    //    void i_leave()
    //    {
    //        //nec.I.regs.w[4] = nec.I.regs.w[5];
    //        nec.I.regs.b[8] = nec.I.regs.b[10];
    //        nec.I.regs.b[9] = nec.I.regs.b[11];
    //        //POP(ref nec.I.regs.w[5]);
    //        nec.POPW(5);
    //        nec.pendingCycles -= 8;
    //    }
    //    void i_retf_d16()
    //    {
    //        ushort count = nec.FETCH();
    //        count += (ushort)(nec.FETCH() << 8);
    //        nec.POP(ref nec.I.ip);
    //        nec.POP(ref nec.I.sregs[1]);
    //        //nec.I.regs.w[4] += count;
    //        ushort w4 = (ushort)(nec.I.regs.b[8] + nec.I.regs.b[9] * 0x100 + count);
    //        nec.I.regs.b[8] = (byte)(w4 % 0x100);
    //        nec.I.regs.b[9] = (byte)(w4 / 0x100);
    //        //CHANGE_PC;
    //        nec.CLKS(32, 32, 16);
    //    }
    //    void i_retf()
    //    {
    //        nec.POP(ref nec.I.ip);
    //        nec.POP(ref nec.I.sregs[1]);
    //        //CHANGE_PC;
    //        nec.CLKS(29, 29, 16);
    //    }
    //    void i_int3()
    //    {
    //        nec.nec_interrupt(3, false);
    //        nec.CLKS(50, 50, 24);
    //    }
    //    void i_int()
    //    {
    //        nec.nec_interrupt(nec.FETCH(), false);
    //        nec.CLKS(50, 50, 24);
    //    }
    //    void i_into()
    //    {
    //        if (nec.OF())
    //        {
    //            nec.nec_interrupt(4, false);
    //            nec.CLKS(52, 52, 26);
    //        }
    //        else
    //        {
    //            nec.CLK(3);
    //        }
    //    }
    //    void i_iret()
    //    {
    //        nec.POP(ref nec.I.ip);
    //        nec.POP(ref nec.I.sregs[1]);
    //        nec.i_popf();
    //        nec.I.MF = true;
    //        //CHANGE_PC;
    //        nec.CLKS(39, 39, 19);
    //    }
    //    void i_rotshft_b()
    //    {
    //        int ModRM;
    //        int src, dst;
    //        ModRM = nec.GetModRM();
    //        src = nec.GetRMByte(ModRM);
    //        dst = src;
    //        nec.CLKM(ModRM, 6, 6, 2, 16, 16, 7);
    //        switch (ModRM & 0x38)
    //        {
    //            case 0x00:  nec.ROL_BYTE(ref dst); nec.PutbackRMByte(ModRM, (byte)dst); nec.I.OverVal = (uint)((src ^ dst) & 0x80); break;
    //            case 0x08: nec.ROR_BYTE(ref dst); nec.PutbackRMByte(ModRM, (byte)dst); nec.I.OverVal = (uint)((src ^ dst) & 0x80); break;
    //            case 0x10: nec.ROLC_BYTE(ref dst); nec.PutbackRMByte(ModRM, (byte)dst); nec.I.OverVal = (uint)((src ^ dst) & 0x80); break;
    //            case 0x18: nec.RORC_BYTE(ref dst); nec.PutbackRMByte(ModRM, (byte)dst); nec.I.OverVal = (uint)((src ^ dst) & 0x80); break;
    //            case 0x20: nec.SHL_BYTE(1, ref dst, ModRM); nec.I.OverVal = (uint)((src ^ dst) & 0x80); break;
    //            case 0x28: nec.SHR_BYTE(1, ref dst, ModRM); nec.I.OverVal = (uint)((src ^ dst) & 0x80); break;
    //            case 0x30: break;
    //            case 0x38: nec.SHRA_BYTE(1, ref dst, ModRM); nec.I.OverVal = 0; break;
    //        }
    //    }
    //    void i_rotshft_w()
    //    {
    //        int ModRM;
    //        int src, dst;
    //        ModRM = nec.GetModRM();
    //        src = nec.GetRMWord(ModRM);
    //        dst = src;
    //        nec.CLKM(ModRM, 6, 6, 2, 24, 16, 7);
    //        switch (ModRM & 0x38)
    //        {
    //            case 0x00: nec.ROL_WORD(ref dst); nec.PutbackRMWord(ModRM, (ushort)dst); nec.I.OverVal = (uint)((src ^ dst) & 0x8000); break;
    //            case 0x08: nec.ROR_WORD(ref dst); nec.PutbackRMWord(ModRM, (ushort)dst); nec.I.OverVal = (uint)((src ^ dst) & 0x8000); break;
    //            case 0x10: nec.ROLC_WORD(ref dst); nec.PutbackRMWord(ModRM, (ushort)dst); nec.I.OverVal = (uint)((src ^ dst) & 0x8000); break;
    //            case 0x18: nec.RORC_WORD(ref dst); nec.PutbackRMWord(ModRM, (ushort)dst); nec.I.OverVal = (uint)((src ^ dst) & 0x8000); break;
    //            case 0x20: nec.SHL_WORD(1, ref dst, ModRM); nec.I.OverVal = (uint)((src ^ dst) & 0x8000); break;
    //            case 0x28: nec.SHR_WORD(1, ref dst, ModRM); nec.I.OverVal = (uint)((src ^ dst) & 0x8000); break;
    //            case 0x30: break;
    //            case 0x38: nec.SHRA_WORD(1, ref dst, ModRM); nec.I.OverVal = 0; break;
    //        }
    //    }
    //    void i_rotshft_bcl()
    //    {
    //        int ModRM;
    //        int src, dst;
    //        byte c;
    //        ModRM = nec.GetModRM();
    //        src = nec.GetRMByte(ModRM);
    //        dst = src;
    //        c = nec.I.regs.b[2];
    //        nec.CLKM(ModRM, 7, 7, 2, 19, 19, 6);
    //        if (c != 0)
    //        {
    //            switch (ModRM & 0x38)
    //            {
    //                case 0x00: do {  nec.ROL_BYTE(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMByte(ModRM, (byte)dst); break;
    //                case 0x08: do { nec.ROR_BYTE(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMByte(ModRM, (byte)dst); break;
    //                case 0x10: do { nec.ROLC_BYTE(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMByte(ModRM, (byte)dst); break;
    //                case 0x18: do { nec.RORC_BYTE(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMByte(ModRM, (byte)dst); break;
    //                case 0x20: nec.SHL_BYTE(c, ref dst, ModRM); break;
    //                case 0x28: nec.SHR_BYTE(c, ref dst, ModRM); break;
    //                case 0x30: break;
    //                case 0x38: nec.SHRA_BYTE(c, ref dst, ModRM); break;
    //            }
    //        }
    //    }
    //    void i_rotshft_wcl()
    //    {
    //        int ModRM;
    //        int src, dst;
    //        byte c;
    //        ModRM = nec.GetModRM();
    //        src = nec.GetRMWord(ModRM);
    //        dst = src;
    //        c = nec.I.regs.b[2];
    //        nec.CLKM(ModRM, 7, 7, 2, 27, 19, 6);
    //        if (c != 0)
    //        {
    //            switch (ModRM & 0x38)
    //            {
    //                case 0x00: do { nec.ROL_WORD(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMWord(ModRM, (ushort)dst); break;
    //                case 0x08: do { nec.ROR_WORD(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMWord(ModRM, (ushort)dst); break;
    //                case 0x10: do { nec.ROLC_WORD(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMWord(ModRM, (ushort)dst); break;
    //                case 0x18: do { nec.RORC_WORD(ref dst); c--; nec.CLK(1); } while (c > 0); nec.PutbackRMWord(ModRM, (ushort)dst); break;
    //                case 0x20: nec.SHL_WORD(c, ref dst, ModRM); break;
    //                case 0x28: nec.SHR_WORD(c, ref dst, ModRM); break;
    //                case 0x30: break;
    //                case 0x38: nec.SHRA_WORD(c, ref dst, ModRM); break;
    //            }
    //        }
    //    }
    //    void i_aam()
    //    {
    //        byte mult = nec.FETCH();
    //        mult = 0;
    //        nec.I.regs.b[1] = (byte)(nec.I.regs.b[0] / 10);
    //        nec.I.regs.b[0] %= 10;
    //        nec.SetSZPF_Word(nec.I.regs.b[0] + nec.I.regs.b[1] * 0x100);
    //        nec.CLKS(15, 15, 12);
    //    }
    //    void i_aad()
    //    {
    //        byte mult = nec.FETCH();
    //        mult = 0;
    //        nec.I.regs.b[0] = (byte)(nec.I.regs.b[1] * 10 + nec.I.regs.b[0]);
    //        nec.I.regs.b[1] = 0;
    //        nec.SetSZPF_Byte(nec.I.regs.b[0]);
    //        nec.CLKS(7, 7, 8);
    //    }
    //    void i_setalc()
    //    {
    //        nec.I.regs.b[0] = (byte)(nec.CF() ? 0xff : 0x00);
    //        nec.pendingCycles -= 3;
    //    }
    //    void i_trans()
    //    {
    //        int dest = (nec.I.regs.b[6] + nec.I.regs.b[7] * 0x100 + nec.I.regs.b[0]) & 0xffff;
    //        nec.I.regs.b[0] = nec.GetMemB(3, dest);
    //        nec.CLKS(9, 9, 5);
    //    }
    //    void i_fpo()
    //    {
    //        int ModRM;
    //        ModRM = nec.GetModRM();
    //        nec.pendingCycles -= 2;
    //    }
    //    void i_loopne()
    //    {
    //        sbyte disp = (sbyte)nec.FETCH();
    //        //nec.I.regs.w[1]--;
    //        ushort w1 = (ushort)(nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100 - 1);
    //        nec.I.regs.b[2] = (byte)(w1 % 0x100);
    //        nec.I.regs.b[3] = (byte)(w1 / 0x100);
    //        if (!nec.ZF() && (nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100 != 0))
    //        {
    //            nec.I.ip = (ushort)(nec.I.ip + disp);
    //            nec.CLKS(14, 14, 6);
    //        }
    //        else
    //        {
    //            nec.CLKS(5, 5, 3);
    //        }
    //    }
    //    void i_loope()
    //    {
    //        sbyte disp = (sbyte)nec.FETCH();
    //        //nec.I.regs.w[1]--;
    //        ushort w1 = (ushort)(nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100 - 1);
    //        nec.I.regs.b[2] = (byte)(w1 % 0x100);
    //        nec.I.regs.b[3] = (byte)(w1 / 0x100);
    //        if (nec.ZF() && (nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100 != 0))
    //        {
    //            nec.I.ip = (ushort)(nec.I.ip + disp);
    //            nec.CLKS(14, 14, 6);
    //        }
    //        else
    //        {
    //            nec.CLKS(5, 5, 3);
    //        }
    //    }
    //    void i_loop()
    //    {
    //        sbyte disp = (sbyte)nec.FETCH();
    //        //nec.I.regs.w[1]--;
    //        ushort w1 = (ushort)(nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100 - 1);
    //        nec.I.regs.b[2] = (byte)(w1 % 0x100);
    //        nec.I.regs.b[3] = (byte)(w1 / 0x100);
    //        if (nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100 != 0)
    //        {
    //            nec.I.ip = (ushort)(nec.I.ip + disp);
    //            nec.CLKS(13, 13, 6);
    //        }
    //        else
    //        {
    //            nec.CLKS(5, 5, 3);
    //        }
    //    }
    //    void i_jcxz()
    //    {
    //        sbyte disp = (sbyte)nec.FETCH();
    //        if (nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100 == 0)
    //        {
    //            nec.I.ip = (ushort)(nec.I.ip + disp);
    //            nec.CLKS(13, 13, 6);
    //        }
    //        else
    //        {
    //            nec.CLKS(5, 5, 3);
    //        }
    //    }
    //    void i_inal()
    //    {
    //        byte port = nec.FETCH();
    //        nec.I.regs.b[0] = ReadIOByte(port);
    //        nec.CLKS(9, 9, 5);
    //    }
    //    void i_inax()
    //    {
    //        byte port = nec.FETCH();
    //        //nec.I.regs.w[0] = nec.ReadIOWord(port);
    //        ushort w0 = nec.ReadIOWord(port);
    //        nec.I.regs.b[0] = (byte)(w0 % 0x100);
    //        nec.I.regs.b[1] = (byte)(w0 / 0x100);
    //        nec.CLKW(13, 13, 7, 13, 9, 5, port);
    //    }
    //    void i_outal()
    //    {
    //        byte port = nec.FETCH();
    //        WriteIOByte(port, nec.I.regs.b[0]);
    //        nec.CLKS(8, 8, 3);
    //    }
    //    void i_outax()
    //    {
    //        byte port = nec.FETCH();
    //        //WriteIOWord(port, nec.I.regs.w[0]);
    //        nec.WriteIOWord(port, (ushort)(nec.I.regs.b[0] + nec.I.regs.b[1] * 0x100));
    //        nec.CLKW(12, 12, 5, 12, 8, 3, port);
    //    }
    //    void i_call_d16()
    //    {
    //        ushort tmp;
    //        tmp = nec.FETCHWORD();
    //        nec.PUSH(nec.I.ip);
    //        nec.I.ip = (ushort)(nec.I.ip + (short)tmp);
    //        //CHANGE_PC;
    //        nec.pendingCycles -= 24;
    //    }
    //    void i_nec.JMP_d16()
    //    {
    //        ushort tmp;
    //        tmp = nec.FETCHWORD();
    //        nec.I.ip = (ushort)(nec.I.ip + (short)tmp);
    //        //CHANGE_PC;
    //        nec.pendingCycles -= 15;
    //    }
    //    void i_nec.JMP_far()
    //    {
    //        ushort tmp, tmp1;
    //        tmp = nec.FETCHWORD();
    //        tmp1 = nec.FETCHWORD();
    //        nec.I.sregs[1] = (ushort)tmp1;
    //        nec.I.ip = (ushort)tmp;
    //        //CHANGE_PC;
    //        nec.pendingCycles -= 27;
    //    }
    //    void i_nec.JMP_d8()
    //    {
    //        int tmp = (int)((sbyte)nec.FETCH());
    //        nec.pendingCycles -= 12;
    //        if (tmp == -2 && nec.I.no_interrupt == 0 && (nec.I.pending_irq == 0) && nec.pendingCycles > 0)
    //        {
    //            nec.pendingCycles %= 12;
    //        }
    //        nec.I.ip = (ushort)(nec.I.ip + tmp);
    //    }
    //    void i_inaldx()
    //    {
    //        //nec.I.regs.b[0] = ReadIOByte(nec.I.regs.w[2]);
    //        nec.I.regs.b[0] = ReadIOByte(nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100);
    //        nec.CLKS(8, 8, 5);
    //    }
    //    void i_inaxdx()
    //    {
    //        //nec.I.regs.w[0] = nec.ReadIOWord(nec.I.regs.w[2]);
    //        ushort w0 = nec.ReadIOWord(nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100);
    //        nec.I.regs.b[0] = (byte)(w0 % 0x100);
    //        nec.I.regs.b[1] = (byte)(w0 / 0x100);
    //        nec.CLKW(12, 12, 7, 12, 8, 5, nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100);
    //    }
    //    void i_outdxal()
    //    {
    //        //WriteIOByte(nec.I.regs.w[2], nec.I.regs.b[0]);
    //        WriteIOByte(nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100, nec.I.regs.b[0]);
    //        nec.CLKS(8, 8, 3);
    //    }
    //    void i_outdxax()
    //    {
    //        //WriteIOWord(nec.I.regs.w[2], nec.I.regs.w[0]);
    //        //nec.CLKW(12, 12, 5, 12, 8, 3, nec.I.regs.w[2]);
    //        nec.WriteIOWord(nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100, (ushort)(nec.I.regs.b[0] + nec.I.regs.b[1] * 0x100));
    //        nec.CLKW(12, 12, 5, 12, 8, 3, nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100);
    //    }
    //    void i_lock()
    //    {
    //        nec.I.no_interrupt = 1;
    //        nec.CLK(2);
    //    }
    //    void i_repne()
    //    {
    //        byte next = nec.fetchop();
    //        ushort c = (ushort)(nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100);//nec.I.regs.w[1];
    //        switch (next)
    //        {
    //            case 0x26: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[0] << 4); next = nec.fetchop(); nec.CLK(2); break;
    //            case 0x2e: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[1] << 4); next = nec.fetchop(); nec.CLK(2); break;
    //            case 0x36: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[2] << 4); next = nec.fetchop(); nec.CLK(2); break;
    //            case 0x3e: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[3] << 4); next = nec.fetchop(); nec.CLK(2); break;
    //        }
    //        switch (next)
    //        {
    //            case 0x6c: nec.CLK(2); if (c != 0) do { i_insb(); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100);/*nec.I.regs.w[1] = c;*/ break;
    //            case 0x6d: nec.CLK(2); if (c != 0) do { i_insw(); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0x6e: nec.CLK(2); if (c != 0) do { i_outsb(); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0x6f: nec.CLK(2); if (c != 0) do { i_outsw(); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xa4: nec.CLK(2); if (c != 0) do { i_movsb(); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xa5: nec.CLK(2); if (c != 0) do { i_movsw(); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xa6: nec.CLK(2); if (c != 0) do { i_cmpsb(); c--; } while (c > 0 && nec.ZF() == false); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xa7: nec.CLK(2); if (c != 0) do { i_cmpsw(); c--; } while (c > 0 && nec.ZF() == false); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xaa: nec.CLK(2); if (c != 0) do { i_stosb(); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xab: nec.CLK(2); if (c != 0) do { i_stosw(); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xac: nec.CLK(2); if (c != 0) do { i_lodsb(); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xad: nec.CLK(2); if (c != 0) do { i_lodsw(); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xae: nec.CLK(2); if (c != 0) do { i_scasb(); c--; } while (c > 0 && nec.ZF() == false); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xaf: nec.CLK(2); if (c != 0) do { i_scasw(); c--; } while (c > 0 && nec.ZF() == false); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            default:
    //                nec.nec_instruction[next]();
    //                //DoInstructionOpCode(next);
    //                break;
    //        }
    //        Nec.seg_prefix = 0;
    //    }
    //    void i_repe()
    //    {
    //        byte next = nec.fetchop();
    //        ushort c = (ushort)(nec.I.regs.b[2] + nec.I.regs.b[3] * 0x100);// nec.I.regs.w[1];
    //        switch (next)
    //        {
    //            case 0x26: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[0] << 4); next = nec.fetchop(); nec.CLK(2); break;
    //            case 0x2e: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[1] << 4); next = nec.fetchop(); nec.CLK(2); break;
    //            case 0x36: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[2] << 4); next = nec.fetchop(); nec.CLK(2); break;
    //            case 0x3e: Nec.seg_prefix = 1; Nec.prefix_base = (nec.I.sregs[3] << 4); next = nec.fetchop(); nec.CLK(2); break;
    //        }
    //        switch (next)
    //        {
    //            case 0x6c: nec.CLK(2); if (c != 0) do { i_insb(); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100);/*nec.I.regs.w[1] = c;*/ break;
    //            case 0x6d: nec.CLK(2); if (c != 0) do { i_insw(); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0x6e: nec.CLK(2); if (c != 0) do { i_outsb(); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0x6f: nec.CLK(2); if (c != 0) do { i_outsw(); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xa4: nec.CLK(2); if (c != 0) do { i_movsb(); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xa5: nec.CLK(2); if (c != 0) do { i_movsw(); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xa6: nec.CLK(2); if (c != 0) do { i_cmpsb(); c--; } while (c > 0 && nec.ZF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xa7: nec.CLK(2); if (c != 0) do { i_cmpsw(); c--; } while (c > 0 && nec.ZF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xaa: nec.CLK(2); if (c != 0) do { i_stosb(); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xab: nec.CLK(2); if (c != 0) do { i_stosw(); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xac: nec.CLK(2); if (c != 0) do { i_lodsb(); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xad: nec.CLK(2); if (c != 0) do { i_lodsw(); c--; } while (c > 0); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xae: nec.CLK(2); if (c != 0) do { i_scasb(); c--; } while (c > 0 && nec.ZF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            case 0xaf: nec.CLK(2); if (c != 0) do { i_scasw(); c--; } while (c > 0 && nec.ZF()); nec.I.regs.b[2] = (byte)(c % 0x100); nec.I.regs.b[3] = (byte)(c / 0x100); break;
    //            default:
    //                nec.nec_instruction[next]();
    //                //DoInstructionOpCode(next);
    //                break;
    //        }
    //        Nec.seg_prefix = 0;
    //    }
    //    void i_hlt()
    //    {
    //        nec.pendingCycles = 0;
    //    }
    //    void i_cmc()
    //    {
    //        nec.I.CarryVal = (uint)(nec.CF() ? 0 : 1);
    //        nec.CLK(2);
    //    }
    //    void i_f6pre()
    //    {
    //        int ModRM;
    //        uint tmp;
    //        uint uresult, uresult2;
    //        int result, result2;
    //        ModRM = nec.GetModRM();
    //        tmp = nec.GetRMByte(ModRM);
    //        switch (ModRM & 0x38)
    //        {
    //            case 0x00: tmp &= nec.FETCH(); nec.I.CarryVal = nec.I.OverVal = 0; nec.SetSZPF_Byte((int)tmp); nec.pendingCycles -= (ModRM >= 0xc0) ? 4 : 11; break;
    //            case 0x08: break;
    //            case 0x10: nec.PutbackRMByte(ModRM, (byte)(~tmp)); nec.pendingCycles -= (ModRM >= 0xc0) ? 2 : 16; break;
    //            case 0x18: nec.I.CarryVal = (uint)((tmp != 0) ? 1 : 0); tmp = (~tmp) + 1; nec.SetSZPF_Byte((int)tmp); nec.PutbackRMByte(ModRM, (byte)(tmp & 0xff)); nec.pendingCycles -= (ModRM >= 0xc0) ? 2 : 16; break;
    //            case 0x20:
    //                uresult = nec.I.regs.b[0] * tmp;
    //                //nec.I.regs.w[0] = (ushort)uresult;
    //                nec.I.regs.b[0] = (byte)((ushort)uresult % 0x100);
    //                nec.I.regs.b[1] = (byte)((ushort)uresult / 0x100);
    //                nec.I.CarryVal = nec.I.OverVal = (uint)((nec.I.regs.b[1] != 0) ? 1 : 0);
    //                nec.pendingCycles -= (ModRM >= 0xc0) ? 30 : 36;
    //                break;
    //            case 0x28:
    //                result = (short)((sbyte)nec.I.regs.b[0]) * (short)((sbyte)tmp);
    //                //nec.I.regs.w[0] = (ushort)result;
    //                nec.I.regs.b[0] = (byte)((ushort)result % 0x100);
    //                nec.I.regs.b[1] = (byte)((ushort)result / 0x100);
    //                nec.I.CarryVal = nec.I.OverVal = (uint)((nec.I.regs.b[1] != 0) ? 1 : 0);
    //                nec.pendingCycles -= (ModRM >= 0xc0) ? 30 : 36;
    //                break;
    //            case 0x30:
    //                if (tmp != 0)
    //                {
    //                    bool b1;
    //                    nec.DIVUB((int)tmp, out b1);
    //                    if (b1)
    //                    {
    //                        break;
    //                    }
    //                }
    //                else
    //                {
    //                    nec.nec_interrupt(0, false);
    //                }
    //                nec.pendingCycles -= (ModRM >= 0xc0) ? 43 : 53;
    //                break;
    //            case 0x38:
    //                if (tmp != 0)
    //                {
    //                    bool b1;
    //                    DIVB((int)tmp, out b1);
    //                    if (b1)
    //                    {
    //                        break;
    //                    }
    //                }
    //                else
    //                {
    //                    nec.nec_interrupt(0, false);
    //                }
    //                nec.pendingCycles -= (ModRM >= 0xc0) ? 43 : 53;
    //                break;
    //        }
    //    }
    //    void i_f7pre()
    //    {
    //        int ModRM;
    //        uint tmp, tmp2;
    //        uint uresult, uresult2;
    //        int result, result2;
    //        ModRM = nec.GetModRM();
    //        tmp = nec.GetRMWord(ModRM);
    //        switch (ModRM & 0x38)
    //        {
    //            case 0x00: tmp2 = nec.FETCHWORD(); tmp &= tmp2; nec.I.CarryVal = nec.I.OverVal = 0; nec.SetSZPF_Word((int)tmp); nec.pendingCycles -= (ModRM >= 0xc0) ? 4 : 11; break;
    //            case 0x08: break;
    //            case 0x10: nec.PutbackRMWord(ModRM, (ushort)(~tmp)); nec.pendingCycles -= (ModRM >= 0xc0) ? 2 : 16; break;
    //            case 0x18: nec.I.CarryVal = (uint)((tmp != 0) ? 1 : 0); tmp = (~tmp) + 1; nec.SetSZPF_Word((int)tmp); nec.PutbackRMWord(ModRM, (ushort)(tmp & 0xffff)); nec.pendingCycles -= (ModRM >= 0xc0) ? 2 : 16; break;
    //            case 0x20:
    //                uresult = (uint)((nec.I.regs.b[0] + nec.I.regs.b[1] * 0x100) * tmp);
    //                //nec.I.regs.w[0] = (ushort)(uresult & 0xffff);
    //                //nec.I.regs.w[2] = (ushort)(uresult >> 16);
    //                nec.I.regs.b[0] = (byte)((ushort)(uresult & 0xffff) % 0x100);
    //                nec.I.regs.b[1] = (byte)((ushort)(uresult & 0xffff) / 0x100);
    //                nec.I.regs.b[4] = (byte)((ushort)(uresult >> 16) % 0x100);
    //                nec.I.regs.b[5] = (byte)((ushort)(uresult >> 16) / 0x100);
    //                nec.I.CarryVal = nec.I.OverVal = (uint)(((nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100) != 0) ? 1 : 0);
    //                nec.pendingCycles -= (ModRM >= 0xc0) ? 30 : 36;
    //                break;
    //            case 0x28:
    //                result = (int)((short)(nec.I.regs.b[0] + nec.I.regs.b[1] * 0x100)) * (int)((short)tmp);
    //                //nec.I.regs.w[0] = (ushort)(result & 0xffff);
    //                //nec.I.regs.w[2] = (ushort)(result >> 16);
    //                nec.I.regs.b[0] = (byte)((ushort)(result & 0xffff) % 0x100);
    //                nec.I.regs.b[1] = (byte)((ushort)(result & 0xffff) / 0x100);
    //                nec.I.regs.b[4] = (byte)((ushort)(result >> 16) % 0x100);
    //                nec.I.regs.b[5] = (byte)((ushort)(result >> 16) / 0x100);
    //                nec.I.CarryVal = nec.I.OverVal = (uint)(((nec.I.regs.b[4] + nec.I.regs.b[5] * 0x100) != 0) ? 1 : 0);
    //                nec.pendingCycles -= (ModRM >= 0xc0) ? 30 : 36;
    //                break;
    //            case 0x30:
    //                if (tmp != 0)
    //                {
    //                    bool b1;
    //                    DIVUW((int)tmp, out b1);
    //                    if (b1)
    //                    {
    //                        break;
    //                    }
    //                }
    //                else
    //                {
    //                    nec.nec_interrupt(0, false);
    //                }
    //                nec.pendingCycles -= (ModRM >= 0xc0) ? 43 : 53;
    //                break;
    //            case 0x38:
    //                if (tmp != 0)
    //                {
    //                    bool b1;
    //                    DIVW((int)tmp, out b1);
    //                    if (b1)
    //                    {
    //                        break;
    //                    }
    //                }
    //                else
    //                {
    //                    nec.nec_interrupt(0, false);
    //                }
    //                nec.pendingCycles -= (ModRM >= 0xc0) ? 43 : 53;
    //                break;
    //        }
    //    }
    //    void i_clc()
    //    {
    //        nec.I.CarryVal = 0;
    //        nec.CLK(2);
    //    }
    //    void i_stc()
    //    {
    //        nec.I.CarryVal = 1;
    //        nec.CLK(2);
    //    }
    //    void i_di()
    //    {
    //        nec.I.IF = false;
    //        nec.CLK(2);
    //    }
    //    void i_ei()
    //    {
    //        nec.I.IF = true;
    //        nec.CLK(2);
    //    }
    //    void i_cld()
    //    {
    //        nec.I.DF = false;
    //        nec.CLK(2);
    //    }
    //    void i_std()
    //    {
    //        nec.I.DF = true;
    //        nec.CLK(2);
    //    }
    //    //void i_fepre()
    //    //{
    //    //    int ModRM;
    //    //    byte tmp, tmp1;
    //    //    ModRM = nec.GetModRM();
    //    //    tmp = nec.GetRMByte(ModRM);
    //    //    switch (ModRM & 0x38)
    //    //    {
    //    //        case 0x00:
    //    //            tmp1 = (byte)(tmp + 1);
    //    //            nec.I.OverVal = (uint)((tmp == 0x7f) ? 1 : 0);
    //    //            SetAF(tmp1, tmp, 1);
    //    //            nec.SetSZPF_Byte(tmp1);
    //    //            nec.PutbackRMByte(ModRM, (byte)tmp1);
    //    //            nec.CLKM(ModRM, 2, 2, 2, 16, 16, 7);
    //    //            break;
    //    //        case 0x08: 
    //    //            tmp1 = (byte)(tmp - 1);
    //    //            nec.I.OverVal = (uint)((tmp == 0x80) ? 1 : 0);
    //    //            SetAF(tmp1, tmp, 1); nec.SetSZPF_Byte(tmp1);
    //    //            nec.PutbackRMByte(ModRM, (byte)tmp1);
    //    //            nec.CLKM(ModRM, 2, 2, 2, 16, 16, 7);
    //    //            break;
    //    //        default: break;
    //    //    }
    //    //}
    //    const int _i_fepre_ccount = 131586;  // (2 << 16) | (2 << 8) | 2
    //    const int _i_fepre_mcount = 1052679; // (16 << 16) | (16 << 8) | 7
    //    //手动内联了一些
    //    void i_fepre()
    //    {
    //        int ModRM;
    //        byte tmp, tmp1;
    //        //ModRM = nec.GetModRM();
    //        ModRM = nec.ReadOpArg(((nec.I.sregs[1] << 4) + nec.I.ip++) ^ 0);
    //        //tmp = nec.GetRMByte(ModRM);
    //        tmp = ((ModRM) >= 0xc0 ? nec.I.regs.b[nec.mod_RM.RMb[ModRM]] : ReadByte(
    //            nec.GetEA[ModRM]()
    //            //DoNecnec.GetEAOpCode(ModRM)
    //            ));
    //        switch (ModRM & 0x38)
    //        {
    //            case 0x00:
    //                {
    //                    tmp1 = (byte)(tmp + 1);
    //                    nec.I.OverVal = (uint)((tmp == 0x7f) ? 1 : 0);

    //                    //SetAF(tmp1, tmp, 1);
    //                    nec.I.AuxVal = (uint)(((tmp1) ^ ((tmp) ^ (1))) & 0x10);

    //                    //SetSZPF_Byte(tmp1);
    //                    nec.I.ZeroVal = nec.I.ParityVal = (uint)((sbyte)tmp1);
    //                    nec.I.SignVal = (int)I.ZeroVal;

    //                    //PutbackRMByte(ModRM, (byte)tmp1);
    //                    if (ModRM >= 0xc0)
    //                    {
    //                        nec.I.regs.b[nec.mod_RM.RMb[ModRM]] = tmp1;
    //                    }
    //                    else
    //                    {
    //                        nec.WriteByte(Nec.EA, tmp1);
    //                    }

    //                    //nec.CLKM(ModRM, 2, 2, 2, 16, 16, 7);

    //                    //计算也可以简化
    //                    //int ccount = (2 << 16) | (2 << 8) | 2, mcount = (16 << 16) | (16 << 8) | 7;
    //                    //nec.pendingCycles -= (ModRM >= 0xc0) ? ((ccount >> nec.chip_type) & 0x7f) : ((mcount >> nec.chip_type) & 0x7f);

    //                    //简化为读取常量
    //                    nec.pendingCycles -= (ModRM >= 0xc0) ? ((_i_fepre_ccount >> nec.chip_type) & 0x7f) : ((_i_fepre_mcount >> nec.chip_type) & 0x7f);
    //                }
    //                break;
    //            case 0x08:
    //                {
    //                    tmp1 = (byte)(tmp - 1);
    //                    nec.I.OverVal = (uint)((tmp == 0x80) ? 1 : 0);
    //                    //SetAF(tmp1, tmp, 1); 
    //                    nec.I.AuxVal = (uint)(((tmp1) ^ ((tmp) ^ (1))) & 0x10);

    //                    //SetSZPF_Byte(tmp1);
    //                    nec.I.ZeroVal = nec.I.ParityVal = (uint)((sbyte)tmp1);
    //                    nec.I.SignVal = (int)I.ZeroVal;

    //                    //PutbackRMByte(ModRM, (byte)tmp1);

    //                    if (ModRM >= 0xc0)
    //                    {
    //                        nec.I.regs.b[nec.mod_RM.RMb[ModRM]] = tmp1;
    //                    }
    //                    else
    //                    {
    //                        nec.WriteByte(Nec.EA, tmp1);
    //                    }

    //                    //nec.CLKM(ModRM, 2, 2, 2, 16, 16, 7);

    //                    //计算也可以简化
    //                    //int ccount = (2 << 16) | (2 << 8) | 2, mcount = (16 << 16) | (16 << 8) | 7;
    //                    //nec.pendingCycles -= (ModRM >= 0xc0) ? ((ccount >> nec.chip_type) & 0x7f) : ((mcount >> nec.chip_type) & 0x7f);

    //                    //简化为读取常量
    //                    nec.pendingCycles -= (ModRM >= 0xc0) ? ((_i_fepre_ccount >> nec.chip_type) & 0x7f) : ((_i_fepre_mcount >> nec.chip_type) & 0x7f);
    //                }
    //                break;
    //            default: break;
    //        }
    //    }
    //    void i_ffpre()
    //    {
    //        int ModRM;
    //        ushort tmp, tmp1;
    //        ModRM = nec.GetModRM();
    //        tmp = nec.GetRMWord(ModRM);
    //        switch (ModRM & 0x38)
    //        {
    //            case 0x00: tmp1 = (ushort)(tmp + 1); nec.I.OverVal = (uint)((tmp == 0x7fff) ? 1 : 0); SetAF(tmp1, tmp, 1); nec.SetSZPF_Word(tmp1); nec.PutbackRMWord(ModRM, (ushort)tmp1); nec.CLKM(ModRM, 2, 2, 2, 24, 16, 7); break;
    //            case 0x08: tmp1 = (ushort)(tmp - 1); nec.I.OverVal = (uint)((tmp == 0x8000) ? 1 : 0); SetAF(tmp1, tmp, 1); nec.SetSZPF_Word(tmp1); nec.PutbackRMWord(ModRM, (ushort)tmp1); nec.CLKM(ModRM, 2, 2, 2, 24, 16, 7); break;
    //            case 0x10:
    //                nec.PUSH(nec.I.ip);
    //                nec.I.ip = (ushort)tmp;
    //                //CHANGE_PC;
    //                nec.pendingCycles -= (ModRM >= 0xc0) ? 16 : 20;
    //                break;
    //            case 0x18:
    //                tmp1 = nec.I.sregs[1];
    //                nec.I.sregs[1] = nec.GetnextRMWord();
    //                nec.PUSH(tmp1);
    //                nec.PUSH(nec.I.ip);
    //                nec.I.ip = tmp;
    //                //CHANGE_PC;
    //                nec.pendingCycles -= (ModRM >= 0xc0) ? 16 : 26;
    //                break;
    //            case 0x20:
    //                nec.I.ip = tmp;
    //                //CHANGE_PC;
    //                nec.pendingCycles -= 13;
    //                break;
    //            case 0x28:
    //                nec.I.ip = tmp;
    //                nec.I.sregs[1] = nec.GetnextRMWord();
    //                //CHANGE_PC;
    //                nec.pendingCycles -= 15;
    //                break;
    //            case 0x30: nec.PUSH(tmp); nec.pendingCycles -= 4; break;
    //            default: break;
    //        }
    //    }
    //    void i_invalid()
    //    {
    //        nec.pendingCycles -= 10;
    //    }
    //}
}
