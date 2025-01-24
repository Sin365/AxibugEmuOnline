using MAME.Core;

namespace cpu.m6800
{
    public partial class M6800
    {
        protected void illegal()
        {

        }
        protected void trap()
        {
            //logerror("M6808: illegal opcode: address %04X, op %02X\n",PC,(int) M_RDOP_ARG(PC)&0xFF);
            TAKE_TRAP();
        }
        protected void nop()
        {
        }
        protected void lsrd()
        {
            ushort t;
            CLR_NZC();
            t = D.LowWord;
            cc |= (byte)(t & 0x0001);
            t >>= 1;
            SET_Z16(t);
            D.LowWord = t;
        }
        protected void asld()
        {
            int r;
            ushort t;
            t = D.LowWord;
            r = t << 1;
            CLR_NZVC();
            SET_FLAGS16((uint)t, (uint)t, (uint)r);
            D.LowWord = (ushort)r;
        }
        protected void tap()
        {
            cc = D.HighByte;
            ONE_MORE_INSN();
            CHECK_IRQ_LINES();
        }
        protected void tpa()
        {
            D.HighByte = cc;
        }
        protected void inx()
        {
            ++X.LowWord;
            CLR_Z();
            SET_Z16(X.LowWord);
        }
        protected void dex()
        {
            --X.LowWord;
            CLR_Z();
            SET_Z16(X.LowWord);
        }
        protected void clv()
        {
            CLV();
        }
        protected void sev()
        {
            SEV();
        }
        protected void clc()
        {
            CLC();
        }
        protected void sec()
        {
            SEC();
        }
        protected void cli()
        {
            CLI();
            ONE_MORE_INSN();
            CHECK_IRQ_LINES();
        }
        protected void sei()
        {
            SEI();
            ONE_MORE_INSN();
            CHECK_IRQ_LINES();
        }
        protected void sba()
        {
            ushort t;
            t = (ushort)(D.HighByte - D.LowByte);
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, D.LowByte, t);
            D.HighByte = (byte)t;
        }
        protected void cba()
        {
            ushort t;
            t = (ushort)(D.HighByte - D.LowByte);
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, D.LowByte, t);
        }
        protected void undoc1()
        {
            X.LowWord += ReadMemory((ushort)(S.LowWord + 1));
        }
        protected void undoc2()
        {
            X.LowWord += ReadMemory((ushort)(S.LowWord + 1));
        }
        protected void tab()
        {
            D.LowByte = D.HighByte;
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        protected void tba()
        {
            D.HighByte = D.LowByte;
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        protected void xgdx()
        {
            ushort t = X.LowWord;
            X.LowWord = D.LowWord;
            D.LowWord = t;
        }
        protected void daa()
        {
            byte msn, lsn;
            ushort t, cf = 0;
            msn = (byte)(D.HighByte & 0xf0);
            lsn = (byte)(D.HighByte & 0x0f);
            if (lsn > 0x09 || (cc & 0x20) != 0)
                cf |= 0x06;
            if (msn > 0x80 && lsn > 0x09)
                cf |= 0x60;
            if (msn > 0x90 || (cc & 0x01) != 0)
                cf |= 0x60;
            t = (ushort)(cf + D.HighByte);
            CLR_NZV();
            SET_NZ8((byte)t);
            SET_C8(t);
            D.HighByte = (byte)t;
        }
        protected void slp()
        {
            wai_state |= M6800_SLP;
            EAT_CYCLES();
        }
        protected void aba()
        {
            ushort t;
            t = (ushort)(D.HighByte + D.LowByte);
            CLR_HNZVC();
            SET_FLAGS8(D.HighByte, D.LowByte, t);
            SET_H(D.HighByte, D.LowByte, t);
            D.HighByte = (byte)t;
        }
        protected void bra()
        {
            byte t;
            t = IMMBYTE();
            PC.LowWord += (ushort)SIGNED(t);
            if (t == 0xfe)
                EAT_CYCLES();
        }
        protected void brn()
        {
            byte t;
            t = IMMBYTE();
        }
        protected void bhi()
        {
            BRANCH((cc & 0x05) == 0);
        }
        protected void bls()
        {
            BRANCH((cc & 0x05) != 0);
        }
        protected void bcc()
        {
            BRANCH((cc & 0x01) == 0);
        }
        protected void bcs()
        {
            BRANCH((cc & 0x01) != 0);
        }
        protected void bne()
        {
            BRANCH((cc & 0x04) == 0);
        }
        protected void beq()
        {
            BRANCH((cc & 0x04) != 0);
        }
        protected void bvc()
        {
            BRANCH((cc & 0x02) == 0);
        }
        protected void bvs()
        {
            BRANCH((cc & 0x02) != 0);
        }
        protected void bpl()
        {
            BRANCH((cc & 0x08) == 0);
        }
        protected void bmi()
        {
            BRANCH((cc & 0x08) != 0);
        }
        protected void bge()
        {
            BRANCH(NXORV() == 0);
        }
        protected void blt()
        {
            BRANCH(NXORV() != 0);
        }
        protected void bgt()
        {
            BRANCH(!(NXORV() != 0 || (cc & 0x04) != 0));
        }
        protected void ble()
        {
            BRANCH(NXORV() != 0 || (cc & 0x04) != 0);
        }
        protected void tsx()
        {
            X.LowWord = (ushort)(S.LowWord + 1);
        }
        protected void ins()
        {
            ++S.LowWord;
        }
        protected void pula()
        {
            D.HighByte = PULLBYTE();
        }
        protected void pulb()
        {
            D.LowByte = PULLBYTE();
        }
        protected void des()
        {
            --S.LowWord;
        }
        protected void txs()
        {
            S.LowWord = (ushort)(X.LowWord - 1);
        }
        protected void psha()
        {
            PUSHBYTE(D.HighByte);
        }
        protected void pshb()
        {
            PUSHBYTE(D.LowByte);
        }
        protected void pulx()
        {
            X = PULLWORD();
        }
        protected void rts()
        {
            PC = PULLWORD();
            //CHANGE_PC();
        }
        protected void abx()
        {
            X.LowWord += D.LowByte;
        }
        protected void rti()
        {
            cc = PULLBYTE();
            D.LowByte = PULLBYTE();
            D.HighByte = PULLBYTE();
            X = PULLWORD();
            PC = PULLWORD();
            //CHANGE_PC();
            CHECK_IRQ_LINES();
        }
        protected void pshx()
        {
            PUSHWORD(X);
        }
        protected void mul()
        {
            ushort t;
            t = (ushort)(D.HighByte * D.LowByte);
            CLR_C();
            if ((t & 0x80) != 0)
                SEC();
            D.LowWord = t;
        }
        protected void wai()
        {
            wai_state |= M6800_WAI;
            PUSHWORD(PC);
            PUSHWORD(X);
            PUSHBYTE(D.HighByte);
            PUSHBYTE(D.LowByte);
            PUSHBYTE(cc);
            CHECK_IRQ_LINES();
            if ((wai_state & M6800_WAI) != 0)
                EAT_CYCLES();
        }
        protected void swi()
        {
            PUSHWORD(PC);
            PUSHWORD(X);
            PUSHBYTE(D.HighByte);
            PUSHBYTE(D.LowByte);
            PUSHBYTE(cc);
            SEI();
            PC.d = RM16(0xfffa);
            //CHANGE_PC();
        }
        protected void nega()
        {
            ushort r;
            r = (ushort)(-D.HighByte);
            CLR_NZVC();
            SET_FLAGS8(0, D.HighByte, r);
            D.HighByte = (byte)r;
        }
        protected void coma()
        {
            D.HighByte = (byte)(~D.HighByte);
            CLR_NZV();
            SET_NZ8(D.HighByte);
            SEC();
        }
        protected void lsra()
        {
            CLR_NZC();
            cc |= (byte)(D.HighByte & 0x01);
            D.HighByte >>= 1;
            SET_Z8(D.HighByte);
        }
        protected void rora()
        {
            byte r;
            r = (byte)((cc & 0x01) << 7);
            CLR_NZC();
            cc |= (byte)(D.HighByte & 0x01);
            r |= (byte)(D.HighByte >> 1);
            SET_NZ8(r);
            D.HighByte = r;
        }
        protected void asra()
        {
            CLR_NZC();
            cc |= (byte)(D.HighByte & 0x01);
            D.HighByte >>= 1;
            D.HighByte |= (byte)((D.HighByte & 0x40) << 1);
            SET_NZ8(D.HighByte);
        }
        protected void asla()
        {
            ushort r;
            r = (ushort)(D.HighByte << 1);
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, D.HighByte, r);
            D.HighByte = (byte)r;
        }
        protected void rola()
        {
            ushort t, r;
            t = D.HighByte;
            r = (ushort)(cc & 0x01);
            r |= (ushort)(t << 1);
            CLR_NZVC();
            SET_FLAGS8(t, t, r);
            D.HighByte = (byte)r;
        }
        protected void deca()
        {
            --D.HighByte;
            CLR_NZV();
            SET_FLAGS8D(D.HighByte);
        }
        protected void inca()
        {
            ++D.HighByte;
            CLR_NZV();
            SET_FLAGS8I(D.HighByte);
        }
        protected void tsta()
        {
            CLR_NZVC();
            SET_NZ8(D.HighByte);
        }
        protected void clra()
        {
            D.HighByte = 0;
            CLR_NZVC();
            SEZ();
        }
        protected void negb()
        {
            ushort r;
            r = (ushort)(-D.LowByte);
            CLR_NZVC();
            SET_FLAGS8(0, D.LowByte, r);
            D.LowByte = (byte)r;
        }
        protected void comb()
        {
            D.LowByte = (byte)(~D.LowByte);
            CLR_NZV();
            SET_NZ8(D.LowByte);
            SEC();
        }
        protected void lsrb()
        {
            CLR_NZC();
            cc |= (byte)(D.LowByte & 0x01);
            D.LowByte >>= 1;
            SET_Z8(D.LowByte);
        }
        protected void rorb()
        {
            byte r;
            r = (byte)((cc & 0x01) << 7);
            CLR_NZC();
            cc |= (byte)(D.LowByte & 0x01);
            r |= (byte)(D.LowByte >> 1);
            SET_NZ8(r);
            D.LowByte = r;
        }
        protected void asrb()
        {
            CLR_NZC();
            cc |= (byte)(D.LowByte & 0x01);
            D.LowByte >>= 1;
            D.LowByte |= (byte)((D.LowByte & 0x40) << 1);
            SET_NZ8(D.LowByte);
        }
        protected void aslb()
        {
            ushort r;
            r = (ushort)(D.LowByte << 1);
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, D.LowByte, r);
            D.LowByte = (byte)r;
        }
        protected void rolb()
        {
            ushort t, r;
            t = D.LowByte;
            r = (ushort)(cc & 0x01);
            r |= (ushort)(t << 1);
            CLR_NZVC();
            SET_FLAGS8(t, t, r);
            D.LowByte = (byte)r;
        }
        protected void decb()
        {
            --D.LowByte;
            CLR_NZV();
            SET_FLAGS8D(D.LowByte);
        }
        protected void incb()
        {
            ++D.LowByte;
            CLR_NZV();
            SET_FLAGS8I(D.LowByte);
        }
        protected void tstb()
        {
            CLR_NZVC();
            SET_NZ8(D.LowByte);
        }
        protected void clrb()
        {
            D.LowByte = 0;
            CLR_NZVC();
            SEZ();
        }
        protected void neg_ix()
        {
            ushort r, t;
            t = (ushort)IDXBYTE();
            r = (ushort)(-t);
            CLR_NZVC();
            SET_FLAGS8(0, t, r);
            WriteMemory(EA.LowWord, (byte)r);
        }
        protected void aim_ix()
        {
            byte t, r;
            t = IMMBYTE();
            r = IDXBYTE();
            r &= t;
            CLR_NZV();
            SET_NZ8(r);
            WriteMemory(EA.LowWord, r);
        }
        protected void oim_ix()
        {
            byte t, r;
            t = IMMBYTE();
            r = IDXBYTE();
            r |= t;
            CLR_NZV();
            SET_NZ8(r);
            WriteMemory(EA.LowWord, r);
        }
        protected void com_ix()
        {
            byte t;
            t = IDXBYTE();
            t = (byte)(~t);
            CLR_NZV();
            SET_NZ8(t);
            SEC();
            WriteMemory(EA.LowWord, t);
        }
        protected void lsr_ix()
        {
            byte t;
            t = IDXBYTE();
            CLR_NZC();
            cc |= (byte)(t & 0x01);
            t >>= 1;
            SET_Z8(t);
            WriteMemory(EA.LowWord, t);
        }
        protected void eim_ix()
        {
            byte t, r;
            t = IMMBYTE();
            r = IDXBYTE();
            r ^= t;
            CLR_NZV();
            SET_NZ8(r);
            WriteMemory(EA.LowWord, r);
        }
        protected void ror_ix()
        {
            byte t, r;
            t = IDXBYTE();
            r = (byte)((cc & 0x01) << 7);
            CLR_NZC();
            cc |= (byte)(t & 0x01);
            r |= (byte)(t >> 1);
            SET_NZ8(r);
            WriteMemory(EA.LowWord, r);
        }
        protected void asr_ix()
        {
            byte t;
            t = IDXBYTE();
            CLR_NZC();
            cc |= (byte)(t & 0x01);
            t >>= 1;
            t |= (byte)((t & 0x40) << 1);
            SET_NZ8(t);
            WriteMemory(EA.LowWord, t);
        }
        protected void asl_ix()
        {
            ushort t, r;
            t = IDXBYTE();
            r = (ushort)(t << 1);
            CLR_NZVC();
            SET_FLAGS8(t, t, r);
            WriteMemory(EA.LowWord, (byte)r);
        }
        protected void rol_ix()
        {
            ushort t, r;
            t = IDXBYTE();
            r = (ushort)(cc & 0x01);
            r |= (ushort)(t << 1);
            CLR_NZVC();
            SET_FLAGS8(t, t, r);
            WriteMemory(EA.LowWord, (byte)r);
        }
        protected void dec_ix()
        {
            byte t;
            t = IDXBYTE();
            --t;
            CLR_NZV();
            SET_FLAGS8D(t);
            WriteMemory(EA.LowWord, t);
        }
        protected void tim_ix()
        {
            byte t, r;
            t = IMMBYTE();
            r = IDXBYTE();
            r &= t;
            CLR_NZV();
            SET_NZ8(r);
        }
        protected void inc_ix()
        {
            byte t;
            t = IDXBYTE();
            ++t;
            CLR_NZV();
            SET_FLAGS8I(t);
            WriteMemory(EA.LowWord, t);
        }
        protected void tst_ix()
        {
            byte t;
            t = IDXBYTE();
            CLR_NZVC();
            SET_NZ8(t);
        }
        protected void jmp_ix()
        {
            INDEXED();
            PC.LowWord = EA.LowWord;
            //CHANGE_PC();
        }
        protected void clr_ix()
        {
            INDEXED();
            WriteMemory(EA.LowWord, 0);
            CLR_NZVC();
            SEZ();
        }
        protected void neg_ex()
        {
            ushort r, t;
            t = EXTBYTE();
            r = (ushort)(-t);
            CLR_NZVC();
            SET_FLAGS8(0, t, r);
            WriteMemory(EA.LowWord, (byte)r);
        }
        protected void aim_di()
        {
            byte t, r;
            t = IMMBYTE();
            r = DIRBYTE();
            r &= t;
            CLR_NZV();
            SET_NZ8(r);
            WriteMemory(EA.LowWord, r);
        }
        protected void oim_di()
        {
            byte t, r;
            t = IMMBYTE();
            r = DIRBYTE();
            r |= t;
            CLR_NZV();
            SET_NZ8(r);
            WriteMemory(EA.LowWord, r);
        }
        protected void com_ex()
        {
            byte t;
            t = EXTBYTE();
            t = (byte)(~t);
            CLR_NZV();
            SET_NZ8(t);
            SEC();
            WriteMemory(EA.LowWord, t);
        }
        protected void lsr_ex()
        {
            byte t;
            t = EXTBYTE();
            CLR_NZC();
            cc |= (byte)(t & 0x01);
            t >>= 1;
            SET_Z8(t);
            WriteMemory(EA.LowWord, t);
        }
        protected void eim_di()
        {
            byte t, r;
            t = IMMBYTE();
            r = DIRBYTE();
            r ^= t;
            CLR_NZV();
            SET_NZ8(r);
            WriteMemory(EA.LowWord, r);
        }
        protected void ror_ex()
        {
            byte t, r;
            t = EXTBYTE();
            r = (byte)((cc & 0x01) << 7);
            CLR_NZC();
            cc |= (byte)(t & 0x01);
            r |= (byte)(t >> 1);
            SET_NZ8(r);
            WriteMemory(EA.LowWord, r);
        }
        protected void asr_ex()
        {
            byte t;
            t = EXTBYTE();
            CLR_NZC();
            cc |= (byte)(t & 0x01);
            t >>= 1;
            t |= (byte)((t & 0x40) << 1);
            SET_NZ8(t);
            WriteMemory(EA.LowWord, t);
        }
        protected void asl_ex()
        {
            ushort t, r;
            t = EXTBYTE();
            r = (ushort)(t << 1);
            CLR_NZVC();
            SET_FLAGS8(t, t, r);
            WriteMemory(EA.LowWord, (byte)r);
        }
        protected void rol_ex()
        {
            ushort t, r;
            t = EXTBYTE();
            r = (ushort)(cc & 0x01);
            r |= (ushort)(t << 1);
            CLR_NZVC();
            SET_FLAGS8(t, t, r);
            WriteMemory(EA.LowWord, (byte)r);
        }
        protected void dec_ex()
        {
            byte t;
            t = EXTBYTE();
            --t;
            CLR_NZV();
            SET_FLAGS8D(t);
            WriteMemory(EA.LowWord, (byte)t);
        }
        protected void tim_di()
        {
            byte t, r;
            t = IMMBYTE();
            r = DIRBYTE();
            r &= t;
            CLR_NZV();
            SET_NZ8(r);
        }
        protected void inc_ex()
        {
            byte t;
            t = EXTBYTE();
            ++t;
            CLR_NZV();
            SET_FLAGS8I(t);
            WriteMemory(EA.LowWord, t);
        }
        protected void tst_ex()
        {
            byte t;
            t = EXTBYTE();
            CLR_NZVC();
            SET_NZ8(t);
        }
        protected void jmp_ex()
        {
            EXTENDED();
            PC.LowWord = EA.LowWord;
            //CHANGE_PC();
        }
        protected void clr_ex()
        {
            EXTENDED();
            WriteMemory(EA.LowWord, 0);
            CLR_NZVC();
            SEZ();
        }
        protected void suba_im()
        {
            ushort t, r;
            t = IMMBYTE();
            r = (ushort)(D.HighByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, t, r);
            D.HighByte = (byte)r;
        }
        protected void cmpa_im()
        {
            ushort t, r;
            t = IMMBYTE();
            r = (ushort)(D.HighByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, t, r);
        }
        protected void sbca_im()
        {
            ushort t, r;
            t = IMMBYTE();
            r = (ushort)(D.HighByte - t - (cc & 0x01));
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, t, r);
            D.HighByte = (byte)r;
        }
        protected void subd_im()
        {
            uint r, d;
            Register b;
            b = IMMWORD();
            d = D.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16(d, b.d, r);
            D.LowWord = (ushort)r;
        }
        protected void anda_im()
        {
            byte t;
            t = IMMBYTE();
            D.HighByte &= t;
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        protected void bita_im()
        {
            byte t, r;
            t = IMMBYTE();
            r = (byte)(D.HighByte & t);
            CLR_NZV();
            SET_NZ8(r);
        }
        protected void lda_im()
        {
            D.HighByte = IMMBYTE();
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        protected void sta_im()
        {
            CLR_NZV();
            SET_NZ8(D.HighByte);
            IMM8();
            WriteMemory(EA.LowWord, D.HighByte);
        }
        protected void eora_im()
        {
            byte t;
            t = IMMBYTE();
            D.HighByte ^= t;
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        protected void adca_im()
        {
            ushort t, r;
            t = IMMBYTE();
            r = (ushort)(D.HighByte + t + (cc & 0x01));
            CLR_HNZVC();
            SET_FLAGS8(D.HighByte, t, r);
            SET_H(D.HighByte, t, r);
            D.HighByte = (byte)r;
        }
        protected void ora_im()
        {
            byte t;
            t = IMMBYTE();
            D.HighByte |= t;
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        protected void adda_im()
        {
            ushort t, r;
            t = IMMBYTE();
            r = (ushort)(D.HighByte + t);
            CLR_HNZVC();
            SET_FLAGS8(D.HighByte, t, r);
            SET_H(D.HighByte, t, r);
            D.HighByte = (byte)r;
        }
        protected void cmpx_im()
        {
            uint r, d;
            Register b;
            b = IMMWORD();
            d = X.LowWord;
            r = d - b.d;
            CLR_NZV();
            SET_NZ16((ushort)r);
            SET_V16(d, b.d, r);
        }
        protected void cpx_im()
        {
            uint r, d;
            Register b;
            b = IMMWORD();
            d = X.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16(d, b.d, r);
        }
        protected void bsr()
        {
            byte t;
            t = IMMBYTE();
            PUSHWORD(PC);
            PC.LowWord += (ushort)SIGNED(t);
            //CHANGE_PC();
        }
        protected void lds_im()
        {
            S = IMMWORD();
            CLR_NZV();
            SET_NZ16(S.LowWord);
        }
        protected void sts_im()
        {
            CLR_NZV();
            SET_NZ16(S.LowWord);
            IMM16();
            WM16(EA.LowWord, S);
        }
        protected void suba_di()
        {
            ushort t, r;
            t = DIRBYTE();
            r = (ushort)(D.HighByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, t, r);
            D.HighByte = (byte)r;
        }
        protected void cmpa_di()
        {
            ushort t, r;
            t = DIRBYTE();
            r = (ushort)(D.HighByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, t, r);
        }
        protected void sbca_di()
        {
            ushort t, r;
            t = DIRBYTE();
            r = (ushort)(D.HighByte - t - (cc & 0x01));
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, t, r);
            D.HighByte = (byte)r;
        }
        protected void subd_di()
        {
            uint r, d;
            Register b;
            b = DIRWORD();
            d = D.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16(d, b.d, r);
            D.LowWord = (ushort)r;
        }
        protected void anda_di()
        {
            byte t;
            t = DIRBYTE();
            D.HighByte &= t;
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        protected void bita_di()
        {
            byte t, r;
            t = DIRBYTE();
            r = (byte)(D.HighByte & t);
            CLR_NZV();
            SET_NZ8(r);
        }
        protected void lda_di()
        {
            D.HighByte = DIRBYTE();
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        protected void sta_di()
        {
            CLR_NZV();
            SET_NZ8(D.HighByte);
            DIRECT();
            WriteMemory(EA.LowWord, D.HighByte);
        }
        protected void eora_di()
        {
            byte t;
            t = DIRBYTE();
            D.HighByte ^= t;
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        protected void adca_di()
        {
            ushort t, r;
            t = DIRBYTE();
            r = (ushort)(D.HighByte + t + (cc & 0x01));
            CLR_HNZVC();
            SET_FLAGS8(D.HighByte, t, r);
            SET_H(D.HighByte, t, r);
            D.HighByte = (byte)r;
        }
        protected void ora_di()
        {
            byte t;
            t = DIRBYTE();
            D.HighByte |= t;
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        protected void adda_di()
        {
            ushort t, r;
            t = DIRBYTE();
            r = (ushort)(D.HighByte + t);
            CLR_HNZVC();
            SET_FLAGS8(D.HighByte, t, r);
            SET_H(D.HighByte, t, r);
            D.HighByte = (byte)r;
        }
        protected void cmpx_di()
        {
            uint r, d;
            Register b;
            b = DIRWORD();
            d = X.LowWord;
            r = d - b.d;
            CLR_NZV();
            SET_NZ16((ushort)r);
            SET_V16(d, b.d, r);
        }
        protected void cpx_di()
        {
            uint r, d;
            Register b;
            b = DIRWORD();
            d = X.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16(d, b.d, r);
        }
        protected void jsr_di()
        {
            DIRECT();
            PUSHWORD(PC);
            PC.LowWord = EA.LowWord;
            //CHANGE_PC();
        }
        protected void lds_di()
        {
            S = DIRWORD();
            CLR_NZV();
            SET_NZ16(S.LowWord);
        }
        protected void sts_di()
        {
            CLR_NZV();
            SET_NZ16(S.LowWord);
            DIRECT();
            WM16(EA.LowWord, S);
        }
        protected void suba_ix()
        {
            ushort t, r;
            t = IDXBYTE();
            r = (ushort)(D.HighByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, t, r);
            D.HighByte = (byte)r;
        }
        protected void cmpa_ix()
        {
            ushort t, r;
            t = IDXBYTE();
            r = (ushort)(D.HighByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, t, r);
        }
        protected void sbca_ix()
        {
            ushort t, r;
            t = IDXBYTE();
            r = (ushort)(D.HighByte - t - (cc & 0x01));
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, t, r);
            D.HighByte = (byte)r;
        }
        protected void subd_ix()
        {
            uint r, d;
            Register b;
            b = IDXWORD();
            d = D.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16(d, b.d, r);
            D.LowWord = (ushort)r;
        }
        protected void anda_ix()
        {
            byte t;
            t = IDXBYTE();
            D.HighByte &= t;
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        protected void bita_ix()
        {
            byte t, r;
            t = IDXBYTE();
            r = (byte)(D.HighByte & t);
            CLR_NZV();
            SET_NZ8(r);
        }
        protected void lda_ix()
        {
            D.HighByte = IDXBYTE();
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        protected void sta_ix()
        {
            CLR_NZV();
            SET_NZ8(D.HighByte);
            INDEXED();
            WriteMemory(EA.LowWord, D.HighByte);
        }
        protected void eora_ix()
        {
            byte t;
            t = IDXBYTE();
            D.HighByte ^= t;
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        protected void adca_ix()
        {
            ushort t, r;
            t = IDXBYTE();
            r = (ushort)(D.HighByte + t + (cc & 0x01));
            CLR_HNZVC();
            SET_FLAGS8(D.HighByte, t, r);
            SET_H(D.HighByte, t, r);
            D.HighByte = (byte)r;
        }
        protected void ora_ix()
        {
            byte t;
            t = IDXBYTE();
            D.HighByte |= t;
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        protected void adda_ix()
        {
            ushort t, r;
            t = IDXBYTE();
            r = (ushort)(D.HighByte + t);
            CLR_HNZVC();
            SET_FLAGS8(D.HighByte, t, r);
            SET_H(D.HighByte, t, r);
            D.HighByte = (byte)r;
        }
        protected void cmpx_ix()
        {
            uint r, d;
            Register b;
            b = IDXWORD();
            d = X.LowWord;
            r = d - b.d;
            CLR_NZV();
            SET_NZ16((ushort)r);
            SET_V16(d, b.d, r);
        }
        protected void cpx_ix()
        {
            uint r, d;
            Register b;
            b = IDXWORD();
            d = X.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16(d, b.d, r);
        }
        protected void jsr_ix()
        {
            INDEXED();
            PUSHWORD(PC);
            PC.LowWord = EA.LowWord;
            //CHANGE_PC();
        }
        protected void lds_ix()
        {
            S = IDXWORD();
            CLR_NZV();
            SET_NZ16(S.LowWord);
        }
        protected void sts_ix()
        {
            CLR_NZV();
            SET_NZ16(S.LowWord);
            INDEXED();
            WM16(EA.LowWord, S);
        }
        protected void suba_ex()
        {
            ushort t, r;
            t = EXTBYTE();
            r = (ushort)(D.HighByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, t, r);
            D.HighByte = (byte)r;
        }
        protected void cmpa_ex()
        {
            ushort t, r;
            t = EXTBYTE();
            r = (ushort)(D.HighByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, t, r);
        }
        protected void sbca_ex()
        {
            ushort t, r;
            t = EXTBYTE();
            r = (ushort)(D.HighByte - t - (cc & 0x01));
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, t, r);
            D.HighByte = (byte)r;
        }
        protected void subd_ex()
        {
            uint r, d;
            Register b;
            b = EXTWORD();
            d = D.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16(d, b.d, r);
            D.LowWord = (ushort)r;
        }
        protected void anda_ex()
        {
            byte t;
            t = EXTBYTE();
            D.HighByte &= t;
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        protected void bita_ex()
        {
            byte t, r;
            t = EXTBYTE();
            r = (byte)(D.HighByte & t);
            CLR_NZV();
            SET_NZ8(r);
        }
        protected void lda_ex()
        {
            D.HighByte = EXTBYTE();
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        protected void sta_ex()
        {
            CLR_NZV();
            SET_NZ8(D.HighByte);
            EXTENDED();
            WriteMemory(EA.LowWord, D.HighByte);
        }
        protected void eora_ex()
        {
            byte t;
            t = EXTBYTE();
            D.HighByte ^= t;
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        protected void adca_ex()
        {
            ushort t, r;
            t = EXTBYTE();
            r = (ushort)(D.HighByte + t + (cc & 0x01));
            CLR_HNZVC();
            SET_FLAGS8(D.HighByte, t, r); SET_H(D.HighByte, t, r);
            D.HighByte = (byte)r;
        }
        protected void ora_ex()
        {
            byte t;
            t = EXTBYTE();
            D.HighByte |= t;
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        protected void adda_ex()
        {
            ushort t, r;
            t = EXTBYTE();
            r = (ushort)(D.HighByte + t);
            CLR_HNZVC();
            SET_FLAGS8(D.HighByte, t, r); SET_H(D.HighByte, t, r);
            D.HighByte = (byte)r;
        }
        protected void cmpx_ex()
        {
            uint r, d;
            Register b;
            b = EXTWORD();
            d = X.LowWord;
            r = d - b.d;
            CLR_NZV();
            SET_NZ16((ushort)r);
            SET_V16(d, b.d, r);
        }
        protected void cpx_ex()
        {
            uint r, d;
            Register b;
            b = EXTWORD();
            d = X.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16(d, b.d, r);
        }
        protected void jsr_ex()
        {
            EXTENDED();
            PUSHWORD(PC);
            PC.LowWord = EA.LowWord;
            //CHANGE_PC();
        }
        protected void lds_ex()
        {
            S = EXTWORD();
            CLR_NZV();
            SET_NZ16(S.LowWord);
        }
        protected void sts_ex()
        {
            CLR_NZV();
            SET_NZ16(S.LowWord);
            EXTENDED();
            WM16(EA.LowWord, S);
        }
        protected void subb_im()
        {
            ushort t, r;
            t = IMMBYTE();
            r = (ushort)(D.LowByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, t, r);
            D.LowByte = (byte)r;
        }
        protected void cmpb_im()
        {
            ushort t, r;
            t = IMMBYTE();
            r = (ushort)(D.LowByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, t, r);
        }
        protected void sbcb_im()
        {
            ushort t, r;
            t = IMMBYTE();
            r = (ushort)(D.LowByte - t - (cc & 0x01));
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, t, r);
            D.LowByte = (byte)r;
        }
        protected void addd_im()
        {
            uint r, d;
            Register b;
            b = IMMWORD();
            d = D.LowWord;
            r = d + b.d;
            CLR_NZVC();
            SET_FLAGS16(d, b.d, r);
            D.LowWord = (ushort)r;
        }
        protected void andb_im()
        {
            byte t;
            t = IMMBYTE();
            D.LowByte &= t;
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        protected void bitb_im()
        {
            byte t, r;
            t = IMMBYTE();
            r = (byte)(D.LowByte & t);
            CLR_NZV();
            SET_NZ8(r);
        }
        protected void ldb_im()
        {
            D.LowByte = IMMBYTE();
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        protected void stb_im()
        {
            CLR_NZV();
            SET_NZ8(D.LowByte);
            IMM8();
            WriteMemory(EA.LowWord, D.LowByte);
        }
        protected void eorb_im()
        {
            byte t;
            t = IMMBYTE();
            D.LowByte ^= t;
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        protected void adcb_im()
        {
            ushort t, r;
            t = IMMBYTE();
            r = (ushort)(D.LowByte + t + (cc & 0x01));
            CLR_HNZVC();
            SET_FLAGS8(D.LowByte, t, r);
            SET_H(D.LowByte, t, r);
            D.LowByte = (byte)r;
        }
        protected void orb_im()
        {
            byte t;
            t = IMMBYTE();
            D.LowByte |= t;
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        protected void addb_im()
        {
            ushort t, r;
            t = IMMBYTE();
            r = (ushort)(D.LowByte + t);
            CLR_HNZVC();
            SET_FLAGS8(D.LowByte, t, r);
            SET_H(D.LowByte, t, r);
            D.LowByte = (byte)r;
        }
        protected void ldd_im()
        {
            D = IMMWORD();
            CLR_NZV();
            SET_NZ16(D.LowWord);
        }
        protected void std_im()
        {
            IMM16();
            CLR_NZV();
            SET_NZ16(D.LowWord);
            WM16(EA.LowWord, D);
        }
        protected void ldx_im()
        {
            X = IMMWORD();
            CLR_NZV();
            SET_NZ16(X.LowWord);
        }
        protected void stx_im()
        {
            CLR_NZV();
            SET_NZ16(X.LowWord);
            IMM16();
            WM16(EA.LowWord, X);
        }
        protected void subb_di()
        {
            ushort t, r;
            t = DIRBYTE();
            r = (ushort)(D.LowByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, t, r);
            D.LowByte = (byte)r;
        }
        protected void cmpb_di()
        {
            ushort t, r;
            t = DIRBYTE();
            r = (ushort)(D.LowByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, t, r);
        }
        protected void sbcb_di()
        {
            ushort t, r;
            t = DIRBYTE();
            r = (ushort)(D.LowByte - t - (cc & 0x01));
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, t, r);
            D.LowByte = (byte)r;
        }
        protected void addd_di()
        {
            uint r, d;
            Register b;
            b = DIRWORD();
            d = D.LowWord;
            r = d + b.d;
            CLR_NZVC();
            SET_FLAGS16(d, b.d, r);
            D.LowWord = (ushort)r;
        }
        protected void andb_di()
        {
            byte t;
            t = DIRBYTE();
            D.LowByte &= t;
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        protected void bitb_di()
        {
            byte t, r;
            t = DIRBYTE();
            r = (byte)(D.LowByte & t);
            CLR_NZV();
            SET_NZ8(r);
        }
        protected void ldb_di()
        {
            D.LowByte = DIRBYTE();
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        protected void stb_di()
        {
            CLR_NZV();
            SET_NZ8(D.LowByte);
            DIRECT();
            WriteMemory(EA.LowWord, D.LowByte);
        }
        protected void eorb_di()
        {
            byte t;
            t = DIRBYTE();
            D.LowByte ^= t;
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        protected void adcb_di()
        {
            ushort t, r;
            t = DIRBYTE();
            r = (ushort)(D.LowByte + t + (cc & 0x01));
            CLR_HNZVC();
            SET_FLAGS8(D.LowByte, t, r);
            SET_H(D.LowByte, t, r);
            D.LowByte = (byte)r;
        }
        protected void orb_di()
        {
            byte t;
            t = DIRBYTE();
            D.LowByte |= t;
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        protected void addb_di()
        {
            ushort t, r;
            t = DIRBYTE();
            r = (ushort)(D.LowByte + t);
            CLR_HNZVC();
            SET_FLAGS8(D.LowByte, t, r);
            SET_H(D.LowByte, t, r);
            D.LowByte = (byte)r;
        }
        protected void ldd_di()
        {
            D = DIRWORD();
            CLR_NZV();
            SET_NZ16(D.LowWord);
        }
        protected void std_di()
        {
            DIRECT();
            CLR_NZV();
            SET_NZ16(D.LowWord);
            WM16(EA.LowWord, D);
        }
        protected void ldx_di()
        {
            X = DIRWORD();
            CLR_NZV();
            SET_NZ16(X.LowWord);
        }
        protected void stx_di()
        {
            CLR_NZV();
            SET_NZ16(X.LowWord);
            DIRECT();
            WM16(EA.LowWord, X);
        }
        protected void subb_ix()
        {
            ushort t, r;
            t = IDXBYTE();
            r = (ushort)(D.LowByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, t, r);
            D.LowByte = (byte)r;
        }
        protected void cmpb_ix()
        {
            ushort t, r;
            t = IDXBYTE();
            r = (ushort)(D.LowByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, t, r);
        }
        protected void sbcb_ix()
        {
            ushort t, r;
            t = IDXBYTE();
            r = (ushort)(D.LowByte - t - (cc & 0x01));
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, t, r);
            D.LowByte = (byte)r;
        }
        protected void addd_ix()
        {
            uint r, d;
            Register b;
            b = IDXWORD();
            d = D.LowWord;
            r = d + b.d;
            CLR_NZVC();
            SET_FLAGS16(d, b.d, r);
            D.LowWord = (ushort)r;
        }
        protected void andb_ix()
        {
            byte t;
            t = IDXBYTE();
            D.LowByte &= t;
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        protected void bitb_ix()
        {
            byte t, r;
            t = IDXBYTE();
            r = (byte)(D.LowByte & t);
            CLR_NZV();
            SET_NZ8(r);
        }
        protected void ldb_ix()
        {
            D.LowByte = IDXBYTE();
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        protected void stb_ix()
        {
            CLR_NZV();
            SET_NZ8(D.LowByte);
            INDEXED();
            WriteMemory(EA.LowWord, D.LowByte);
        }
        protected void eorb_ix()
        {
            byte t;
            t = IDXBYTE();
            D.LowByte ^= t;
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        protected void adcb_ix()
        {
            ushort t, r;
            t = IDXBYTE();
            r = (ushort)(D.LowByte + t + (cc & 0x01));
            CLR_HNZVC();
            SET_FLAGS8(D.LowByte, t, r);
            SET_H(D.LowByte, t, r);
            D.LowByte = (byte)r;
        }
        protected void orb_ix()
        {
            byte t;
            t = IDXBYTE();
            D.LowByte |= t;
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        protected void addb_ix()
        {
            ushort t, r;
            t = IDXBYTE();
            r = (ushort)(D.LowByte + t);
            CLR_HNZVC();
            SET_FLAGS8(D.LowByte, t, r);
            SET_H(D.LowByte, t, r);
            D.LowByte = (byte)r;
        }
        protected void ldd_ix()
        {
            D = IDXWORD();
            CLR_NZV();
            SET_NZ16(D.LowWord);
        }
        protected void adcx_im()
        {
            ushort t, r;
            t = IMMBYTE();
            r = (ushort)(X.LowWord + t + (cc & 0x01));
            CLR_HNZVC();
            SET_FLAGS8(X.LowWord, t, r); SET_H(X.LowWord, t, r);
            X.LowWord = r;
        }
        protected void std_ix()
        {
            INDEXED();
            CLR_NZV();
            SET_NZ16(D.LowWord);
            WM16(EA.LowWord, D);
        }
        protected void ldx_ix()
        {
            X = IDXWORD();
            CLR_NZV();
            SET_NZ16(X.LowWord);
        }
        protected void stx_ix()
        {
            CLR_NZV();
            SET_NZ16(X.LowWord);
            INDEXED();
            WM16(EA.LowWord, X);
        }
        protected void subb_ex()
        {
            ushort t, r;
            t = EXTBYTE();
            r = (ushort)(D.LowByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, t, r);
            D.LowByte = (byte)r;
        }
        protected void cmpb_ex()
        {
            ushort t, r;
            t = EXTBYTE();
            r = (ushort)(D.LowByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, t, r);
        }
        protected void sbcb_ex()
        {
            ushort t, r;
            t = EXTBYTE();
            r = (ushort)(D.LowByte - t - (cc & 0x01));
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, t, r);
            D.LowByte = (byte)r;
        }
        protected void addd_ex()
        {
            uint r, d;
            Register b;
            b = EXTWORD();
            d = D.LowWord;
            r = d + b.d;
            CLR_NZVC();
            SET_FLAGS16(d, b.d, r);
            D.LowWord = (ushort)r;
        }
        protected void andb_ex()
        {
            byte t;
            t = EXTBYTE();
            D.LowByte &= t;
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        protected void bitb_ex()
        {
            byte t, r;
            t = EXTBYTE();
            r = (byte)(D.LowByte & t);
            CLR_NZV();
            SET_NZ8(r);
        }
        protected void ldb_ex()
        {
            D.LowByte = EXTBYTE();
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        protected void stb_ex()
        {
            CLR_NZV();
            SET_NZ8(D.LowByte);
            EXTENDED();
            WriteMemory(EA.LowWord, D.LowByte);
        }
        protected void eorb_ex()
        {
            byte t;
            t = EXTBYTE();
            D.LowByte ^= t;
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        protected void adcb_ex()
        {
            ushort t, r;
            t = EXTBYTE();
            r = (ushort)(D.LowByte + t + (cc & 0x01));
            CLR_HNZVC();
            SET_FLAGS8(D.LowByte, t, r); SET_H(D.LowByte, t, r);
            D.LowByte = (byte)r;
        }
        protected void orb_ex()
        {
            byte t;
            t = EXTBYTE();
            D.LowByte |= t;
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        protected void addb_ex()
        {
            ushort t, r;
            t = EXTBYTE();
            r = (ushort)(D.LowByte + t);
            CLR_HNZVC();
            SET_FLAGS8(D.LowByte, t, r);
            SET_H(D.LowByte, t, r);
            D.LowByte = (byte)r;
        }
        protected void ldd_ex()
        {
            D = EXTWORD();
            CLR_NZV();
            SET_NZ16(D.LowWord);
        }
        protected void addx_ex()
        {
            uint r, d;
            Register b;
            b = EXTWORD();
            d = X.LowWord;
            r = d + b.d;
            CLR_NZVC();
            SET_FLAGS16(d, b.d, r);
            X.LowWord = (ushort)r;
        }
        protected void std_ex()
        {
            EXTENDED();
            CLR_NZV();
            SET_NZ16(D.LowWord);
            WM16(EA.LowWord, D);
        }
        protected void ldx_ex()
        {
            X = EXTWORD();
            CLR_NZV();
            SET_NZ16(X.LowWord);
        }
        protected void stx_ex()
        {
            CLR_NZV();
            SET_NZ16(X.LowWord);
            EXTENDED();
            WM16(EA.LowWord, X);
        }
        /*protected void CLV()
        {
            cc &= 0xfd;
        }
        protected void SEV()
        {

        }*/
    }
}
