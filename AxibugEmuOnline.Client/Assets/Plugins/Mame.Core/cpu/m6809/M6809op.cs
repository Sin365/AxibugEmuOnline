using MAME.Core;

namespace cpu.m6809
{
    public partial class M6809
    {
        void illegal()
        {

        }
        void neg_di()
        {
            ushort r, t;
            t = DIRBYTE();
            r = (ushort)(-t);
            CLR_NZVC();
            SET_FLAGS8(0, t, r);
            WM(EA.LowWord, (byte)r);
        }
        void com_di()
        {
            byte t;
            t = DIRBYTE();
            t = (byte)(~t);
            CLR_NZV();
            SET_NZ8(t);
            SEC();
            WM(EA.LowWord, t);
        }
        void lsr_di()
        {
            byte t;
            t = DIRBYTE();
            CLR_NZC();
            CC |= (byte)(t & CC_C);
            t >>= 1;
            SET_Z8(t);
            WM(EA.LowWord, t);
        }
        void ror_di()
        {
            byte t, r;
            t = DIRBYTE();
            r = (byte)((CC & CC_C) << 7);
            CLR_NZC();
            CC |= (byte)(t & CC_C);
            r |= (byte)(t >> 1);
            SET_NZ8(r);
            WM(EA.LowWord, r);
        }
        void asr_di()
        {
            byte t;
            t = DIRBYTE();
            CLR_NZC();
            CC |= (byte)(t & CC_C);
            t = (byte)((t & 0x80) | (t >> 1));
            SET_NZ8(t);
            WM(EA.LowWord, t);
        }
        void asl_di()
        {
            ushort t, r;
            t = DIRBYTE();
            r = (ushort)(t << 1);
            CLR_NZVC();
            SET_FLAGS8((byte)t, t, r);
            WM(EA.LowWord, (byte)r);
        }
        void rol_di()
        {
            ushort t, r;
            t = DIRBYTE();
            r = (ushort)((CC & CC_C) | (t << 1));
            CLR_NZVC();
            SET_FLAGS8((byte)t, t, r);
            WM(EA.LowWord, (byte)r);
        }
        void dec_di()
        {
            byte t;
            t = DIRBYTE();
            --t;
            CLR_NZV();
            SET_FLAGS8D(t);
            WM(EA.LowWord, t);
        }
        void inc_di()
        {
            byte t;
            t = DIRBYTE();
            ++t;
            CLR_NZV();
            SET_FLAGS8I(t);
            WM(EA.LowWord, t);
        }
        void tst_di()
        {
            byte t;
            t = DIRBYTE();
            CLR_NZV();
            SET_NZ8(t);
        }
        void jmp_di()
        {
            DIRECT();
            PC.d = EA.d;
            //CHANGE_PC;
        }
        void clr_di()
        {
            DIRECT();
            RM(EA.LowWord);
            WM(EA.LowWord, 0);
            CLR_NZVC();
            SEZ();
        }
        void nop()
        {

        }
        void sync()
        {
            int_state |= M6809_SYNC;
            CHECK_IRQ_LINES();
            if ((int_state & M6809_SYNC) != 0)
                if (pendingCycles > 0) pendingCycles = 0;
        }
        void lbra()
        {
            EA = IMMWORD();
            PC.LowWord += EA.LowWord;
            //CHANGE_PC;
            if (EA.LowWord == 0xfffd)
                if (pendingCycles > 0)
                    pendingCycles = 0;
        }
        void lbsr()
        {
            EA = IMMWORD();
            PUSHWORD(PC);
            PC.LowWord += EA.LowWord;
            //CHANGE_PC;
        }
        void daa()
        {
            byte msn, lsn;
            ushort t, cf = 0;
            msn = (byte)(D.HighByte & 0xf0);
            lsn = (byte)(D.HighByte & 0x0f);
            if (lsn > 0x09 || (CC & CC_H) != 0)
                cf |= 0x06;
            if (msn > 0x80 && lsn > 0x09)
                cf |= 0x60;
            if (msn > 0x90 || (CC & CC_C) != 0)
                cf |= 0x60;
            t = (ushort)(cf + D.HighByte);
            CLR_NZV();
            SET_NZ8((byte)t);
            SET_C8(t);
            D.HighByte = (byte)t;
        }
        void orcc()
        {
            byte t;
            t = IMMBYTE();
            CC |= t;
            CHECK_IRQ_LINES();
        }
        void andcc()
        {
            byte t;
            t = IMMBYTE();
            CC &= t;
            CHECK_IRQ_LINES();
        }
        void sex()
        {
            ushort t;
            t = SIGNED(D.LowByte);
            D.LowWord = t;
            CLR_NZ();
            SET_NZ16(t);
        }
        void exg()
        {
            ushort t1, t2;
            byte tb;
            tb = IMMBYTE();
            if (((tb ^ (tb >> 4)) & 0x08) != 0)
            {
                t1 = t2 = 0xff;
            }
            else
            {
                switch (tb >> 4)
                {
                    case 0: t1 = D.LowWord; break;
                    case 1: t1 = X.LowWord; break;
                    case 2: t1 = Y.LowWord; break;
                    case 3: t1 = U.LowWord; break;
                    case 4: t1 = S.LowWord; break;
                    case 5: t1 = PC.LowWord; break;
                    case 8: t1 = D.HighByte; break;
                    case 9: t1 = D.LowByte; break;
                    case 10: t1 = CC; break;
                    case 11: t1 = DP.HighByte; break;
                    default: t1 = 0xff; break;
                }
                switch (tb & 15)
                {
                    case 0: t2 = D.LowWord; break;
                    case 1: t2 = X.LowWord; break;
                    case 2: t2 = Y.LowWord; break;
                    case 3: t2 = U.LowWord; break;
                    case 4: t2 = S.LowWord; break;
                    case 5: t2 = PC.LowWord; break;
                    case 8: t2 = D.HighByte; break;
                    case 9: t2 = D.LowByte; break;
                    case 10: t2 = CC; break;
                    case 11: t2 = DP.HighByte; break;
                    default: t2 = 0xff; break;
                }
            }
            switch (tb >> 4)
            {
                case 0: D.LowWord = t2; break;
                case 1: X.LowWord = t2; break;
                case 2: Y.LowWord = t2; break;
                case 3: U.LowWord = t2; break;
                case 4: S.LowWord = t2; break;
                case 5: PC.LowWord = t2; break;
                case 8: D.HighByte = (byte)t2; break;
                case 9: D.LowByte = (byte)t2; break;
                case 10: CC = (byte)t2; break;
                case 11: DP.HighByte = (byte)t2; break;
            }
            switch (tb & 15)
            {
                case 0: D.LowWord = t1; break;
                case 1: X.LowWord = t1; break;
                case 2: Y.LowWord = t1; break;
                case 3: U.LowWord = t1; break;
                case 4: S.LowWord = t1; break;
                case 5: PC.LowWord = t1; break;
                case 8: D.HighByte = (byte)t1; break;
                case 9: D.LowByte = (byte)t1; break;
                case 10: CC = (byte)t1; break;
                case 11: DP.HighByte = (byte)t1; break;
            }
        }
        void tfr()
        {
            byte tb;
            ushort t;
            tb = IMMBYTE();
            if (((tb ^ (tb >> 4)) & 0x08) != 0)
            {
                t = 0xff;
            }
            else
            {
                switch (tb >> 4)
                {
                    case 0: t = D.LowWord; break;
                    case 1: t = X.LowWord; break;
                    case 2: t = Y.LowWord; break;
                    case 3: t = U.LowWord; break;
                    case 4: t = S.LowWord; break;
                    case 5: t = PC.LowWord; break;
                    case 8: t = D.HighByte; break;
                    case 9: t = D.LowByte; break;
                    case 10: t = CC; break;
                    case 11: t = DP.HighByte; break;
                    default: t = 0xff; break;
                }
            }
            switch (tb & 15)
            {
                case 0: D.LowWord = t; break;
                case 1: X.LowWord = t; break;
                case 2: Y.LowWord = t; break;
                case 3: U.LowWord = t; break;
                case 4: S.LowWord = t; break;
                case 5: PC.LowWord = t; break;
                case 8: D.HighByte = (byte)t; break;
                case 9: D.LowByte = (byte)t; break;
                case 10: CC = (byte)t; break;
                case 11: DP.HighByte = (byte)t; break;
            }
        }
        void bra()
        {
            byte t;
            t = IMMBYTE();
            PC.LowWord += SIGNED(t);
            if (t == 0xfe)
                if (pendingCycles > 0)
                    pendingCycles = 0;
        }
        void brn()
        {
            byte t;
            t = IMMBYTE();
        }
        void lbrn()
        {
            EA = IMMWORD();
        }
        void bhi()
        {
            BRANCH((CC & (CC_Z | CC_C)) == 0);
        }
        void lbhi()
        {
            LBRANCH((CC & (CC_Z | CC_C)) == 0);
        }
        void bls()
        {
            BRANCH((CC & (CC_Z | CC_C)) != 0);
        }
        void lbls()
        {
            LBRANCH((CC & (CC_Z | CC_C)) != 0);
        }
        void bcc()
        {
            BRANCH((CC & CC_C) == 0);
        }
        void lbcc()
        {
            LBRANCH((CC & CC_C) == 0);
        }
        void bcs()
        {
            BRANCH((CC & CC_C) != 0);
        }
        void lbcs()
        {
            LBRANCH((CC & CC_C) != 0);
        }
        void bne()
        {
            BRANCH((CC & CC_Z) == 0);
        }
        void lbne()
        {
            LBRANCH((CC & CC_Z) == 0);
        }
        void beq()
        {
            BRANCH((CC & CC_Z) != 0);
        }
        void lbeq()
        {
            LBRANCH((CC & CC_Z) != 0);
        }
        void bvc()
        {
            BRANCH((CC & CC_V) == 0);
        }
        void lbvc()
        {
            LBRANCH((CC & CC_V) == 0);
        }
        void bvs()
        {
            BRANCH((CC & CC_V) != 0);
        }
        void lbvs()
        {
            LBRANCH((CC & CC_V) != 0);
        }
        void bpl()
        {
            BRANCH((CC & CC_N) == 0);
        }
        void lbpl()
        {
            LBRANCH((CC & CC_N) == 0);
        }
        void bmi()
        {
            BRANCH((CC & CC_N) != 0);
        }
        void lbmi()
        {
            LBRANCH((CC & CC_N) != 0);
        }
        void bge()
        {
            BRANCH(NXORV() == 0);
        }
        void lbge()
        {
            LBRANCH(NXORV() == 0);
        }
        void blt()
        {
            BRANCH(NXORV() != 0);
        }
        void lblt()
        {
            LBRANCH(NXORV() != 0);
        }
        void bgt()
        {
            BRANCH(!(NXORV() != 0 || (CC & CC_Z) != 0));
        }
        void lbgt()
        {
            LBRANCH(!(NXORV() != 0 || (CC & CC_Z) != 0));
        }
        void ble()
        {
            BRANCH(NXORV() != 0 || (CC & CC_Z) != 0);
        }
        void lble()
        {
            LBRANCH(NXORV() != 0 || (CC & CC_Z) != 0);
        }
        void leax()
        {
            fetch_effective_address();
            X.LowWord = EA.LowWord;
            CLR_Z();
            SET_Z(X.LowWord);
        }
        void leay()
        {
            fetch_effective_address();
            Y.LowWord = EA.LowWord;
            CLR_Z();
            SET_Z(Y.LowWord);
        }
        void leas()
        {
            fetch_effective_address();
            S.LowWord = EA.LowWord;
            int_state |= M6809_LDS;
        }
        void leau()
        {
            fetch_effective_address();
            U.LowWord = EA.LowWord;
        }
        void pshs()
        {
            byte t;
            t = IMMBYTE();
            if ((t & 0x80) != 0) { PUSHWORD(PC); pendingCycles -= 2; }
            if ((t & 0x40) != 0) { PUSHWORD(U); pendingCycles -= 2; }
            if ((t & 0x20) != 0) { PUSHWORD(Y); pendingCycles -= 2; }
            if ((t & 0x10) != 0) { PUSHWORD(X); pendingCycles -= 2; }
            if ((t & 0x08) != 0) { PUSHBYTE(DP.HighByte); pendingCycles -= 1; }
            if ((t & 0x04) != 0) { PUSHBYTE(D.LowByte); pendingCycles -= 1; }
            if ((t & 0x02) != 0) { PUSHBYTE(D.HighByte); pendingCycles -= 1; }
            if ((t & 0x01) != 0) { PUSHBYTE(CC); pendingCycles -= 1; }
        }
        void puls()
        {
            byte t;
            t = IMMBYTE();
            if ((t & 0x01) != 0) { CC = PULLBYTE(); pendingCycles -= 1; }
            if ((t & 0x02) != 0) { D.HighByte = PULLBYTE(); pendingCycles -= 1; }
            if ((t & 0x04) != 0) { D.LowByte = PULLBYTE(); pendingCycles -= 1; }
            if ((t & 0x08) != 0) { DP.HighByte = PULLBYTE(); pendingCycles -= 1; }
            if ((t & 0x10) != 0) { X.d = PULLWORD(); pendingCycles -= 2; }
            if ((t & 0x20) != 0) { Y.d = PULLWORD(); pendingCycles -= 2; }
            if ((t & 0x40) != 0) { U.d = PULLWORD(); pendingCycles -= 2; }
            if ((t & 0x80) != 0) { PC.d = PULLWORD(); pendingCycles -= 2; }
            if ((t & 0x01) != 0) { CHECK_IRQ_LINES(); }
        }
        void pshu()
        {
            byte t;
            t = IMMBYTE();
            if ((t & 0x80) != 0) { PSHUWORD(PC); pendingCycles -= 2; }
            if ((t & 0x40) != 0) { PSHUWORD(S); pendingCycles -= 2; }
            if ((t & 0x20) != 0) { PSHUWORD(Y); pendingCycles -= 2; }
            if ((t & 0x10) != 0) { PSHUWORD(X); pendingCycles -= 2; }
            if ((t & 0x08) != 0) { PSHUBYTE(DP.HighByte); pendingCycles -= 1; }
            if ((t & 0x04) != 0) { PSHUBYTE(D.LowByte); pendingCycles -= 1; }
            if ((t & 0x02) != 0) { PSHUBYTE(D.HighByte); pendingCycles -= 1; }
            if ((t & 0x01) != 0) { PSHUBYTE(CC); pendingCycles -= 1; }
        }
        void pulu()
        {
            byte t;
            t = IMMBYTE();
            if ((t & 0x01) != 0) { CC = PULUBYTE(); pendingCycles -= 1; }
            if ((t & 0x02) != 0) { D.HighByte = PULUBYTE(); pendingCycles -= 1; }
            if ((t & 0x04) != 0) { D.LowByte = PULUBYTE(); pendingCycles -= 1; }
            if ((t & 0x08) != 0) { DP.HighByte = PULUBYTE(); pendingCycles -= 1; }
            if ((t & 0x10) != 0) { X.d = PULUWORD(); pendingCycles -= 2; }
            if ((t & 0x20) != 0) { Y.d = PULUWORD(); pendingCycles -= 2; }
            if ((t & 0x40) != 0) { S.d = PULUWORD(); pendingCycles -= 2; }
            if ((t & 0x80) != 0) { PC.d = PULUWORD(); pendingCycles -= 2; }
            if ((t & 0x01) != 0) { CHECK_IRQ_LINES(); }
        }
        void rts()
        {
            PC.d = PULLWORD();
            //CHANGE_PC;
        }
        void abx()
        {
            X.LowWord += D.LowByte;
        }
        void rti()
        {
            byte t;
            CC = PULLBYTE();
            t = (byte)(CC & CC_E);
            if (t != 0)
            {
                pendingCycles -= 9;
                D.HighByte = PULLBYTE();
                D.LowByte = PULLBYTE();
                DP.HighByte = PULLBYTE();
                X.d = PULLWORD();
                Y.d = PULLWORD();
                U.d = PULLWORD();
            }
            PC.d = PULLWORD();
            //CHANGE_PC;
            CHECK_IRQ_LINES();
        }
        void cwai()
        {
            byte t;
            t = IMMBYTE();
            CC &= t;
            CC |= CC_E;
            PUSHWORD(PC);
            PUSHWORD(U);
            PUSHWORD(Y);
            PUSHWORD(X);
            PUSHBYTE(DP.HighByte);
            PUSHBYTE(D.LowByte);
            PUSHBYTE(D.HighByte);
            PUSHBYTE(CC);
            int_state |= M6809_CWAI;
            CHECK_IRQ_LINES();
            if ((int_state & M6809_CWAI) != 0)
                if (pendingCycles > 0)
                    pendingCycles = 0;
        }
        void mul()
        {
            ushort t;
            t = (ushort)(D.HighByte * D.LowByte);
            CLR_ZC();
            SET_Z16(t);
            if ((t & 0x80) != 0)
                SEC();
            D.LowWord = t;
        }
        void swi()
        {
            CC |= CC_E;
            PUSHWORD(PC);
            PUSHWORD(U);
            PUSHWORD(Y);
            PUSHWORD(X);
            PUSHBYTE(DP.HighByte);
            PUSHBYTE(D.LowByte);
            PUSHBYTE(D.HighByte);
            PUSHBYTE(CC);
            CC |= (byte)(CC_IF | CC_II);
            PC.d = RM16(0xfffa);
            //CHANGE_PC;
        }
        void swi2()
        {
            CC |= CC_E;
            PUSHWORD(PC);
            PUSHWORD(U);
            PUSHWORD(Y);
            PUSHWORD(X);
            PUSHBYTE(DP.HighByte);
            PUSHBYTE(D.LowByte);
            PUSHBYTE(D.HighByte);
            PUSHBYTE(CC);
            PC.d = RM16(0xfff4);
            //CHANGE_PC;
        }
        void swi3()
        {
            CC |= CC_E;
            PUSHWORD(PC);
            PUSHWORD(U);
            PUSHWORD(Y);
            PUSHWORD(X);
            PUSHBYTE(DP.HighByte);
            PUSHBYTE(D.LowByte);
            PUSHBYTE(D.HighByte);
            PUSHBYTE(CC);
            PC.d = RM16(0xfff2);
            //CHANGE_PC;
        }
        void nega()
        {
            ushort r;
            r = (ushort)(-D.HighByte);
            CLR_NZVC();
            SET_FLAGS8(0, D.HighByte, r);
            D.HighByte = (byte)r;
        }
        void coma()
        {
            D.HighByte = (byte)(~D.HighByte);
            CLR_NZV();
            SET_NZ8(D.HighByte);
            SEC();
        }
        void lsra()
        {
            CLR_NZC();
            CC |= (byte)(D.HighByte & CC_C);
            D.HighByte >>= 1;
            SET_Z8(D.HighByte);
        }
        void rora()
        {
            byte r;
            r = (byte)((CC & CC_C) << 7);
            CLR_NZC();
            CC |= (byte)(D.HighByte & CC_C);
            r |= (byte)(D.HighByte >> 1);
            SET_NZ8(r);
            D.HighByte = r;
        }
        void asra()
        {
            CLR_NZC();
            CC |= (byte)(D.HighByte & CC_C);
            D.HighByte = (byte)((D.HighByte & 0x80) | (D.HighByte >> 1));
            SET_NZ8(D.HighByte);
        }
        void asla()
        {
            ushort r;
            r = (ushort)(D.HighByte << 1);
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, D.HighByte, r);
            D.HighByte = (byte)r;
        }
        void rola()
        {
            ushort t, r;
            t = D.HighByte;
            r = (ushort)((CC & CC_C) | (t << 1));
            CLR_NZVC();
            SET_FLAGS8((byte)t, t, r);
            D.HighByte = (byte)r;
        }
        void deca()
        {
            --D.HighByte;
            CLR_NZV();
            SET_FLAGS8D(D.HighByte);
        }
        void inca()
        {
            ++D.HighByte;
            CLR_NZV();
            SET_FLAGS8I(D.HighByte);
        }
        void tsta()
        {
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        void clra()
        {
            D.HighByte = 0;
            CLR_NZVC();
            SEZ();
        }
        void negb()
        {
            ushort r;
            r = (ushort)(-D.LowByte);
            CLR_NZVC();
            SET_FLAGS8(0, D.LowByte, r);
            D.LowByte = (byte)r;
        }
        void comb()
        {
            D.LowByte = (byte)(~D.LowByte);
            CLR_NZV();
            SET_NZ8(D.LowByte);
            SEC();
        }
        void lsrb()
        {
            CLR_NZC();
            CC |= (byte)(D.LowByte & CC_C);
            D.LowByte >>= 1;
            SET_Z8(D.LowByte);
        }
        void rorb()
        {
            byte r;
            r = (byte)((CC & CC_C) << 7);
            CLR_NZC();
            CC |= (byte)(D.LowByte & CC_C);
            r |= (byte)(D.LowByte >> 1);
            SET_NZ8(r);
            D.LowByte = r;
        }
        void asrb()
        {
            CLR_NZC();
            CC |= (byte)(D.LowByte & CC_C);
            D.LowByte = (byte)((D.LowByte & 0x80) | (D.LowByte >> 1));
            SET_NZ8(D.LowByte);
        }
        void aslb()
        {
            ushort r;
            r = (ushort)(D.LowByte << 1);
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, D.LowByte, r);
            D.LowByte = (byte)r;
        }
        void rolb()
        {
            ushort t, r;
            t = D.LowByte;
            r = (ushort)(CC & CC_C);
            r |= (ushort)(t << 1);
            CLR_NZVC();
            SET_FLAGS8((byte)t, t, r);
            D.LowByte = (byte)r;
        }
        void decb()
        {
            --D.LowByte;
            CLR_NZV();
            SET_FLAGS8D(D.LowByte);
        }
        void incb()
        {
            ++D.LowByte;
            CLR_NZV();
            SET_FLAGS8I(D.LowByte);
        }
        void tstb()
        {
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        void clrb()
        {
            D.LowByte = 0;
            CLR_NZVC();
            SEZ();
        }
        void neg_ix()
        {
            ushort r, t;
            fetch_effective_address();
            t = RM(EA.LowWord);
            r = (ushort)(-t);
            CLR_NZVC();
            SET_FLAGS8(0, t, r);
            WM(EA.LowWord, (byte)r);
        }
        void com_ix()
        {
            byte t;
            fetch_effective_address();
            t = (byte)(~RM(EA.LowWord));
            CLR_NZV();
            SET_NZ8(t);
            SEC();
            WM(EA.LowWord, t);
        }
        void lsr_ix()
        {
            byte t;
            fetch_effective_address();
            t = RM(EA.LowWord);
            CLR_NZC();
            CC |= (byte)(t & CC_C);
            t >>= 1;
            SET_Z8(t);
            WM(EA.LowWord, t);
        }
        void ror_ix()
        {
            byte t, r;
            fetch_effective_address();
            t = RM(EA.LowWord);
            r = (byte)((CC & CC_C) << 7);
            CLR_NZC();
            CC |= (byte)(t & CC_C);
            r |= (byte)(t >> 1);
            SET_NZ8(r);
            WM(EA.LowWord, r);
        }
        void asr_ix()
        {
            byte t;
            fetch_effective_address();
            t = RM(EA.LowWord);
            CLR_NZC();
            CC |= (byte)(t & CC_C);
            t = (byte)((t & 0x80) | (t >> 1));
            SET_NZ8(t);
            WM(EA.LowWord, t);
        }
        void asl_ix()
        {
            ushort t, r;
            fetch_effective_address();
            t = RM(EA.LowWord);
            r = (ushort)(t << 1);
            CLR_NZVC();
            SET_FLAGS8((byte)t, t, r);
            WM(EA.LowWord, (byte)r);
        }
        void rol_ix()
        {
            ushort t, r;
            fetch_effective_address();
            t = RM(EA.LowWord);
            r = (ushort)(CC & CC_C);
            r |= (ushort)(t << 1);
            CLR_NZVC();
            SET_FLAGS8((byte)t, t, r);
            WM(EA.LowWord, (byte)r);
        }
        void dec_ix()
        {
            byte t;
            fetch_effective_address();
            t = (byte)(RM(EA.LowWord) - 1);
            CLR_NZV();
            SET_FLAGS8D(t);
            WM(EA.LowWord, t);
        }
        void inc_ix()
        {
            byte t;
            fetch_effective_address();
            t = (byte)(RM(EA.LowWord) + 1);
            CLR_NZV();
            SET_FLAGS8I(t);
            WM(EA.LowWord, t);
        }
        void tst_ix()
        {
            byte t;
            fetch_effective_address();
            t = RM(EA.LowWord);
            CLR_NZV();
            SET_NZ8(t);
        }
        void jmp_ix()
        {
            fetch_effective_address();
            PC.d = EA.d;
            //CHANGE_PC;
        }
        void clr_ix()
        {
            fetch_effective_address();
            RM(EA.LowWord);
            WM(EA.LowWord, 0);
            CLR_NZVC(); SEZ();
        }
        void neg_ex()
        {
            ushort r, t;
            t = EXTBYTE();
            r = (ushort)(-t);
            CLR_NZVC();
            SET_FLAGS8(0, t, r);
            WM(EA.LowWord, (byte)r);
        }
        void com_ex()
        {
            byte t;
            t = EXTBYTE();
            t = (byte)(~t);
            CLR_NZV();
            SET_NZ8(t);
            SEC();
            WM(EA.LowWord, t);
        }
        void lsr_ex()
        {
            byte t;
            t = EXTBYTE();
            CLR_NZC();
            CC |= (byte)(t & CC_C);
            t >>= 1;
            SET_Z8(t);
            WM(EA.LowWord, t);
        }
        void ror_ex()
        {
            byte t, r;
            t = EXTBYTE();
            r = (byte)((CC & CC_C) << 7);
            CLR_NZC();
            CC |= (byte)(t & CC_C);
            r |= (byte)(t >> 1);
            SET_NZ8(r);
            WM(EA.LowWord, r);
        }
        void asr_ex()
        {
            byte t;
            t = EXTBYTE();
            CLR_NZC();
            CC |= (byte)(t & CC_C);
            t = (byte)((t & 0x80) | (t >> 1));
            SET_NZ8(t);
            WM(EA.LowWord, t);
        }
        void asl_ex()
        {
            ushort t, r;
            t = EXTBYTE();
            r = (ushort)(t << 1);
            CLR_NZVC();
            SET_FLAGS8((byte)t, t, r);
            WM(EA.LowWord, (byte)r);
        }
        void rol_ex()
        {
            ushort t, r;
            t = EXTBYTE();
            r = (ushort)((CC & CC_C) | (t << 1));
            CLR_NZVC();
            SET_FLAGS8((byte)t, t, r);
            WM(EA.LowWord, (byte)r);
        }
        void dec_ex()
        {
            byte t;
            t = EXTBYTE();
            --t;
            CLR_NZV();
            SET_FLAGS8D(t);
            WM(EA.LowWord, t);
        }
        void inc_ex()
        {
            byte t;
            t = EXTBYTE();
            ++t;
            CLR_NZV();
            SET_FLAGS8I(t);
            WM(EA.LowWord, t);
        }
        void tst_ex()
        {
            byte t;
            t = EXTBYTE();
            CLR_NZV();
            SET_NZ8(t);
        }
        void jmp_ex()
        {
            EXTENDED();
            PC.d = EA.d;
            //CHANGE_PC;
        }
        void clr_ex()
        {
            EXTENDED();
            RM(EA.LowWord);
            WM(EA.LowWord, 0);
            CLR_NZVC();
            SEZ();
        }
        void suba_im()
        {
            ushort t, r;
            t = IMMBYTE();
            r = (ushort)(D.HighByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, t, r);
            D.HighByte = (byte)r;
        }
        void cmpa_im()
        {
            ushort t, r;
            int i1, i2, i3;
            t = IMMBYTE();
            r = (ushort)(D.HighByte - t);
            i1 = CC;
            CLR_NZVC();
            i2 = CC;
            SET_FLAGS8(D.HighByte, t, r);
            i3 = CC;
        }
        void sbca_im()
        {
            ushort t, r;
            t = IMMBYTE();
            r = (ushort)(D.HighByte - t - (CC & CC_C));
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, t, r);
            D.HighByte = (byte)r;
        }
        void subd_im()
        {
            uint r, d;
            Register b;
            b = IMMWORD();
            d = D.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
            D.LowWord = (ushort)r;
        }
        void cmpd_im()
        {
            uint r, d;
            Register b;
            b = IMMWORD();
            d = D.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
        }
        void cmpu_im()
        {
            uint r, d;
            Register b;
            b = IMMWORD();
            d = U.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
        }
        void anda_im()
        {
            byte t;
            t = IMMBYTE();
            D.HighByte &= t;
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        void bita_im()
        {
            byte t, r;
            t = IMMBYTE();
            r = (byte)(D.HighByte & t);
            CLR_NZV();
            SET_NZ8(r);
        }
        void lda_im()
        {
            D.HighByte = IMMBYTE();
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        void sta_im()
        {
            CLR_NZV();
            SET_NZ8(D.HighByte);
            IMM8();
            WM(EA.LowWord, D.HighByte);
        }
        void eora_im()
        {
            byte t;
            t = IMMBYTE();
            D.HighByte ^= t;
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        void adca_im()
        {
            ushort t, r;
            t = IMMBYTE();
            r = (ushort)(D.HighByte + t + (CC & CC_C));
            CLR_HNZVC();
            SET_FLAGS8(D.HighByte, t, r);
            SET_H(D.HighByte, (byte)t, (byte)r);
            D.HighByte = (byte)r;
        }
        void ora_im()
        {
            byte t;
            t = IMMBYTE();
            D.HighByte |= t;
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        void adda_im()
        {
            ushort t, r;
            t = IMMBYTE();
            r = (ushort)(D.HighByte + t);
            CLR_HNZVC();
            SET_FLAGS8(D.HighByte, t, r);
            SET_H(D.HighByte, (byte)t, (byte)r);
            D.HighByte = (byte)r;
        }
        void cmpx_im()
        {
            uint r, d;
            Register b;
            b = IMMWORD();
            d = X.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
        }
        void cmpy_im()
        {
            uint r, d;
            Register b;
            b = IMMWORD();
            d = Y.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
        }
        void cmps_im()
        {
            uint r, d;
            Register b;
            b = IMMWORD();
            d = S.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
        }
        void bsr()
        {
            byte t;
            t = IMMBYTE();
            PUSHWORD(PC);
            PC.LowWord += SIGNED(t);
            //CHANGE_PC;
        }
        void ldx_im()
        {
            X = IMMWORD();
            CLR_NZV();
            SET_NZ16(X.LowWord);
        }
        void ldy_im()
        {
            Y = IMMWORD();
            CLR_NZV();
            SET_NZ16(Y.LowWord);
        }
        void stx_im()
        {
            CLR_NZV();
            SET_NZ16(X.LowWord);
            IMM16();
            WM16(EA.LowWord, X);
        }
        void sty_im()
        {
            CLR_NZV();
            SET_NZ16(Y.LowWord);
            IMM16();
            WM16(EA.LowWord, Y);
        }
        void suba_di()
        {
            ushort t, r;
            t = DIRBYTE();
            r = (ushort)(D.HighByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, t, r);
            D.HighByte = (byte)r;
        }
        void cmpa_di()
        {
            ushort t, r;
            t = DIRBYTE();
            r = (ushort)(D.HighByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, t, r);
        }
        void sbca_di()
        {
            ushort t, r;
            t = DIRBYTE();
            r = (ushort)(D.HighByte - t - (CC & CC_C));
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, t, r);
            D.HighByte = (byte)r;
        }
        void subd_di()
        {
            uint r, d;
            Register b;
            b = DIRWORD();
            d = D.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
            D.LowWord = (ushort)r;
        }
        void cmpd_di()
        {
            uint r, d;
            Register b;
            b = DIRWORD();
            d = D.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
        }
        void cmpu_di()
        {
            uint r, d;
            Register b;
            b = DIRWORD();
            d = U.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16(U.LowWord, (ushort)b.d, r);
        }
        void anda_di()
        {
            byte t;
            t = DIRBYTE();
            D.HighByte &= t;
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        void bita_di()
        {
            byte t, r;
            t = DIRBYTE();
            r = (byte)(D.HighByte & t);
            CLR_NZV();
            SET_NZ8(r);
        }
        void lda_di()
        {
            D.HighByte = DIRBYTE();
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        void sta_di()
        {
            CLR_NZV();
            SET_NZ8(D.HighByte);
            DIRECT();
            WM(EA.LowWord, D.HighByte);
        }
        void eora_di()
        {
            byte t;
            t = DIRBYTE();
            D.HighByte ^= t;
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        void adca_di()
        {
            ushort t, r;
            t = DIRBYTE();
            r = (ushort)(D.HighByte + t + (CC & CC_C));
            CLR_HNZVC();
            SET_FLAGS8(D.HighByte, t, r);
            SET_H(D.HighByte, (byte)t, (byte)r);
            D.HighByte = (byte)r;
        }
        void ora_di()
        {
            byte t;
            t = DIRBYTE();
            D.HighByte |= t;
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        void adda_di()
        {
            ushort t, r;
            t = DIRBYTE();
            r = (ushort)(D.HighByte + t);
            CLR_HNZVC();
            SET_FLAGS8(D.HighByte, t, r);
            SET_H(D.HighByte, (byte)t, (byte)r);
            D.HighByte = (byte)r;
        }
        void cmpx_di()
        {
            uint r, d;
            Register b;
            b = DIRWORD();
            d = X.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
        }
        void cmpy_di()
        {
            uint r, d;
            Register b;
            b = DIRWORD();
            d = Y.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
        }
        void cmps_di()
        {
            uint r, d;
            Register b;
            b = DIRWORD();
            d = S.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
        }
        void jsr_di()
        {
            DIRECT();
            PUSHWORD(PC);
            PC.d = EA.d;
            //CHANGE_PC;
        }
        void ldx_di()
        {
            X = DIRWORD();
            CLR_NZV();
            SET_NZ16(X.LowWord);
        }
        void ldy_di()
        {
            Y = DIRWORD();
            CLR_NZV();
            SET_NZ16(Y.LowWord);
        }
        void stx_di()
        {
            CLR_NZV();
            SET_NZ16(X.LowWord);
            DIRECT();
            WM16(EA.LowWord, X);
        }
        void sty_di()
        {
            CLR_NZV();
            SET_NZ16(Y.LowWord);
            DIRECT();
            WM16(EA.LowWord, Y);
        }
        void suba_ix()
        {
            ushort t, r;
            fetch_effective_address();
            t = RM(EA.LowWord);
            r = (ushort)(D.HighByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, t, r);
            D.HighByte = (byte)r;
        }
        void cmpa_ix()
        {
            ushort t, r;
            fetch_effective_address();
            t = RM(EA.LowWord);
            r = (ushort)(D.HighByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, t, r);
        }
        void sbca_ix()
        {
            ushort t, r;
            fetch_effective_address();
            t = RM(EA.LowWord);
            r = (ushort)(D.HighByte - t - (CC & CC_C));
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, t, r);
            D.HighByte = (byte)r;
        }
        void subd_ix()
        {
            uint r, d;
            Register b;
            fetch_effective_address();
            b.d = RM16(EA.LowWord);
            d = D.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
            D.LowWord = (ushort)r;
        }
        void cmpd_ix()
        {
            uint r, d;
            Register b;
            fetch_effective_address();
            b.d = RM16(EA.LowWord);
            d = D.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
        }
        void cmpu_ix()
        {
            uint r;
            Register b = new Register();
            fetch_effective_address();
            b.d = RM16(EA.LowWord);
            r = U.LowWord - b.d;
            CLR_NZVC();
            SET_FLAGS16(U.LowWord, b.LowWord, r);
        }
        void anda_ix()
        {
            fetch_effective_address();
            D.HighByte &= RM(EA.LowWord);
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        void bita_ix()
        {
            byte r;
            fetch_effective_address();
            r = (byte)(D.HighByte & RM(EA.LowWord));
            CLR_NZV();
            SET_NZ8(r);
        }
        void lda_ix()
        {
            fetch_effective_address();
            D.HighByte = RM(EA.LowWord);
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        void sta_ix()
        {
            fetch_effective_address();
            CLR_NZV();
            SET_NZ8(D.HighByte);
            WM(EA.LowWord, D.HighByte);
        }
        void eora_ix()
        {
            fetch_effective_address();
            D.HighByte ^= RM(EA.LowWord);
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        void adca_ix()
        {
            ushort t, r;
            fetch_effective_address();
            t = RM(EA.LowWord);
            r = (ushort)(D.HighByte + t + (CC & CC_C));
            CLR_HNZVC();
            SET_FLAGS8(D.HighByte, t, r);
            SET_H(D.HighByte, (byte)t, (byte)r);
            D.HighByte = (byte)r;
        }
        void ora_ix()
        {
            fetch_effective_address();
            D.HighByte |= RM(EA.LowWord);
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        void adda_ix()
        {
            ushort t, r;
            fetch_effective_address();
            t = RM(EA.LowWord);
            r = (ushort)(D.HighByte + t);
            CLR_HNZVC();
            SET_FLAGS8(D.HighByte, t, r);
            SET_H(D.HighByte, (byte)t, (byte)r);
            D.HighByte = (byte)r;
        }
        void cmpx_ix()
        {
            uint r, d;
            Register b;
            fetch_effective_address();
            b.d = RM16(EA.LowWord);
            d = X.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
        }
        void cmpy_ix()
        {
            uint r, d;
            Register b;
            fetch_effective_address();
            b.d = RM16(EA.LowWord);
            d = Y.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
        }
        void cmps_ix()
        {
            uint r, d;
            Register b;
            fetch_effective_address();
            b.d = RM16(EA.LowWord);
            d = S.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
        }
        void jsr_ix()
        {
            fetch_effective_address();
            PUSHWORD(PC);
            PC.d = EA.d;
            //CHANGE_PC;
        }
        void ldx_ix()
        {
            fetch_effective_address();
            X.LowWord = RM16(EA.LowWord);
            CLR_NZV();
            SET_NZ16(X.LowWord);
        }
        void ldy_ix()
        {
            fetch_effective_address();
            Y.LowWord = RM16(EA.LowWord);
            CLR_NZV();
            SET_NZ16(Y.LowWord);
        }
        void stx_ix()
        {
            fetch_effective_address();
            CLR_NZV();
            SET_NZ16(X.LowWord);
            WM16(EA.LowWord, X);
        }
        void sty_ix()
        {
            fetch_effective_address();
            CLR_NZV();
            SET_NZ16(Y.LowWord);
            WM16(EA.LowWord, Y);
        }
        void suba_ex()
        {
            ushort t, r;
            t = EXTBYTE();
            r = (ushort)(D.HighByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, t, r);
            D.HighByte = (byte)r;
        }
        void cmpa_ex()
        {
            ushort t, r;
            t = EXTBYTE();
            r = (ushort)(D.HighByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, t, r);
        }
        void sbca_ex()
        {
            ushort t, r;
            t = EXTBYTE();
            r = (ushort)(D.HighByte - t - (CC & CC_C));
            CLR_NZVC();
            SET_FLAGS8(D.HighByte, t, r);
            D.HighByte = (byte)r;
        }
        void subd_ex()
        {
            uint r, d;
            Register b;
            b = EXTWORD();
            d = D.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
            D.LowWord = (ushort)r;
        }
        void cmpd_ex()
        {
            uint r, d;
            Register b;
            b = EXTWORD();
            d = D.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
        }
        void cmpu_ex()
        {
            uint r, d;
            Register b;
            b = EXTWORD();
            d = U.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
        }
        void anda_ex()
        {
            byte t;
            t = EXTBYTE();
            D.HighByte &= t;
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        void bita_ex()
        {
            byte t, r;
            t = EXTBYTE();
            r = (byte)(D.HighByte & t);
            CLR_NZV(); SET_NZ8(r);
        }
        void lda_ex()
        {
            D.HighByte = EXTBYTE();
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        void sta_ex()
        {
            CLR_NZV();
            SET_NZ8(D.HighByte);
            EXTENDED();
            WM(EA.LowWord, D.HighByte);
        }
        void eora_ex()
        {
            byte t;
            t = EXTBYTE();
            D.HighByte ^= t;
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        void adca_ex()
        {
            ushort t, r;
            t = EXTBYTE();
            r = (ushort)(D.HighByte + t + (CC & CC_C));
            CLR_HNZVC();
            SET_FLAGS8(D.HighByte, t, r);
            SET_H(D.HighByte, (byte)t, (byte)r);
            D.HighByte = (byte)r;
        }
        void ora_ex()
        {
            byte t;
            t = EXTBYTE();
            D.HighByte |= t;
            CLR_NZV();
            SET_NZ8(D.HighByte);
        }
        void adda_ex()
        {
            ushort t, r;
            t = EXTBYTE();
            r = (ushort)(D.HighByte + t);
            CLR_HNZVC();
            SET_FLAGS8(D.HighByte, t, r);
            SET_H(D.HighByte, (byte)t, (byte)r);
            D.HighByte = (byte)r;
        }
        void cmpx_ex()
        {
            uint r, d;
            Register b;
            b = EXTWORD();
            d = X.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
        }
        void cmpy_ex()
        {
            uint r, d;
            Register b;
            b = EXTWORD();
            d = Y.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
        }
        void cmps_ex()
        {
            uint r, d;
            Register b;
            b = EXTWORD();
            d = S.LowWord;
            r = d - b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
        }
        void jsr_ex()
        {
            EXTENDED();
            PUSHWORD(PC);
            PC.d = EA.d;
            //CHANGE_PC;
        }
        void ldx_ex()
        {
            X = EXTWORD();
            CLR_NZV();
            SET_NZ16(X.LowWord);
        }
        void ldy_ex()
        {
            Y = EXTWORD();
            CLR_NZV();
            SET_NZ16(Y.LowWord);
        }
        void stx_ex()
        {
            CLR_NZV();
            SET_NZ16(X.LowWord);
            EXTENDED();
            WM16(EA.LowWord, X);
        }
        void sty_ex()
        {
            CLR_NZV();
            SET_NZ16(Y.LowWord);
            EXTENDED();
            WM16(EA.LowWord, Y);
        }
        void subb_im()
        {
            ushort t, r;
            t = IMMBYTE();
            r = (ushort)(D.LowByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, t, r);
            D.LowByte = (byte)r;
        }
        void cmpb_im()
        {
            ushort t, r;
            t = IMMBYTE();
            r = (ushort)(D.LowByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, t, r);
        }
        void sbcb_im()
        {
            ushort t, r;
            t = IMMBYTE();
            r = (ushort)(D.LowByte - t - (CC & CC_C));
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, t, r);
            D.LowByte = (byte)r;
        }
        void addd_im()
        {
            uint r, d;
            Register b;
            b = IMMWORD();
            d = D.LowWord;
            r = d + b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
            D.LowWord = (ushort)r;
        }
        void andb_im()
        {
            byte t;
            t = IMMBYTE();
            D.LowByte &= t;
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        void bitb_im()
        {
            byte t, r;
            t = IMMBYTE();
            r = (byte)(D.LowByte & t);
            CLR_NZV();
            SET_NZ8(r);
        }
        void ldb_im()
        {
            D.LowByte = IMMBYTE();
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        void stb_im()
        {
            CLR_NZV();
            SET_NZ8(D.LowByte);
            IMM8();
            WM(EA.LowWord, D.LowByte);
        }
        void eorb_im()
        {
            byte t;
            t = IMMBYTE();
            D.LowByte ^= t;
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        void adcb_im()
        {
            ushort t, r;
            t = IMMBYTE();
            r = (ushort)(D.LowByte + t + (CC & CC_C));
            CLR_HNZVC();
            SET_FLAGS8(D.LowByte, t, r);
            SET_H(D.LowByte, (byte)t, (byte)r);
            D.LowByte = (byte)r;
        }
        void orb_im()
        {
            byte t;
            t = IMMBYTE();
            D.LowByte |= t;
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        void addb_im()
        {
            ushort t, r;
            t = IMMBYTE();
            r = (ushort)(D.LowByte + t);
            CLR_HNZVC();
            SET_FLAGS8(D.LowByte, t, r);
            SET_H(D.LowByte, (byte)t, (byte)r);
            D.LowByte = (byte)r;
        }
        void ldd_im()
        {
            D = IMMWORD();
            CLR_NZV();
            SET_NZ16(D.LowWord);
        }
        void std_im()
        {
            CLR_NZV();
            SET_NZ16(D.LowWord);
            IMM16();
            WM16(EA.LowWord, D);
        }
        void ldu_im()
        {
            U = IMMWORD();
            CLR_NZV();
            SET_NZ16(U.LowWord);
        }
        void lds_im()
        {
            S = IMMWORD();
            CLR_NZV();
            SET_NZ16(S.LowWord);
            int_state |= M6809_LDS;
        }
        void stu_im()
        {
            CLR_NZV();
            SET_NZ16(U.LowWord);
            IMM16();
            WM16(EA.LowWord, U);
        }
        void sts_im()
        {
            CLR_NZV();
            SET_NZ16(S.LowWord);
            IMM16();
            WM16(EA.LowWord, S);
        }
        void subb_di()
        {
            ushort t, r;
            t = DIRBYTE();
            r = (ushort)(D.LowByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, t, r);
            D.LowByte = (byte)r;
        }
        void cmpb_di()
        {
            ushort t, r;
            t = DIRBYTE();
            r = (ushort)(D.LowByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, t, r);
        }
        void sbcb_di()
        {
            ushort t, r;
            t = DIRBYTE();
            r = (ushort)(D.LowByte - t - (CC & CC_C));
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, t, r);
            D.LowByte = (byte)r;
        }
        void addd_di()
        {
            uint r, d;
            Register b;
            b = DIRWORD();
            d = D.LowWord;
            r = d + b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
            D.LowWord = (ushort)r;
        }
        void andb_di()
        {
            byte t;
            t = DIRBYTE();
            D.LowByte &= t;
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        void bitb_di()
        {
            byte t, r;
            t = DIRBYTE();
            r = (byte)(D.LowByte & t);
            CLR_NZV();
            SET_NZ8(r);
        }
        void ldb_di()
        {
            D.LowByte = DIRBYTE();
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        void stb_di()
        {
            CLR_NZV();
            SET_NZ8(D.LowByte);
            DIRECT();
            WM(EA.LowWord, D.LowByte);
        }
        void eorb_di()
        {
            byte t;
            t = DIRBYTE();
            D.LowByte ^= t;
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        void adcb_di()
        {
            ushort t, r;
            t = DIRBYTE();
            r = (ushort)(D.LowByte + t + (CC & CC_C));
            CLR_HNZVC();
            SET_FLAGS8(D.LowByte, t, r);
            SET_H(D.LowByte, (byte)t, (byte)r);
            D.LowByte = (byte)r;
        }
        void orb_di()
        {
            byte t;
            t = DIRBYTE();
            D.LowByte |= t;
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        void addb_di()
        {
            ushort t, r;
            t = DIRBYTE();
            r = (ushort)(D.LowByte + t);
            CLR_HNZVC();
            SET_FLAGS8(D.LowByte, t, r);
            SET_H(D.LowByte, (byte)t, (byte)r);
            D.LowByte = (byte)r;
        }
        void ldd_di()
        {
            D = DIRWORD();
            CLR_NZV();
            SET_NZ16(D.LowWord);
        }
        void std_di()
        {
            CLR_NZV();
            SET_NZ16(D.LowWord);
            DIRECT();
            WM16(EA.LowWord, D);
        }
        void ldu_di()
        {
            U = DIRWORD();
            CLR_NZV();
            SET_NZ16(U.LowWord);
        }
        void lds_di()
        {
            S = DIRWORD();
            CLR_NZV();
            SET_NZ16(S.LowWord);
            int_state |= M6809_LDS;
        }
        void stu_di()
        {
            CLR_NZV();
            SET_NZ16(U.LowWord);
            DIRECT();
            WM16(EA.LowWord, U);
        }
        void sts_di()
        {
            CLR_NZV();
            SET_NZ16(S.LowWord);
            DIRECT();
            WM16(EA.LowWord, S);
        }
        void subb_ix()
        {
            ushort t, r;
            fetch_effective_address();
            t = RM(EA.LowWord);
            r = (ushort)(D.LowByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, t, r);
            D.LowByte = (byte)r;
        }
        void cmpb_ix()
        {
            ushort t, r;
            fetch_effective_address();
            t = RM(EA.LowWord);
            r = (ushort)(D.LowByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, t, r);
        }
        void sbcb_ix()
        {
            ushort t, r;
            fetch_effective_address();
            t = RM(EA.LowWord);
            r = (ushort)(D.LowByte - t - (CC & CC_C));
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, t, r);
            D.LowByte = (byte)r;
        }
        void addd_ix()
        {
            uint r, d;
            Register b;
            fetch_effective_address();
            b.d = RM16(EA.LowWord);
            d = D.LowWord;
            r = d + b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
            D.LowWord = (ushort)r;
        }
        void andb_ix()
        {
            fetch_effective_address();
            D.LowByte &= RM(EA.LowWord);
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        void bitb_ix()
        {
            byte r;
            fetch_effective_address();
            r = (byte)(D.LowByte & RM(EA.LowWord));
            CLR_NZV();
            SET_NZ8(r);
        }
        void ldb_ix()
        {
            fetch_effective_address();
            D.LowByte = RM(EA.LowWord);
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        void stb_ix()
        {
            fetch_effective_address();
            CLR_NZV();
            SET_NZ8(D.LowByte);
            WM(EA.LowWord, D.LowByte);
        }
        void eorb_ix()
        {
            fetch_effective_address();
            D.LowByte ^= RM(EA.LowWord);
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        void adcb_ix()
        {
            ushort t, r;
            fetch_effective_address();
            t = RM(EA.LowWord);
            r = (ushort)(D.LowByte + t + (CC & CC_C));
            CLR_HNZVC();
            SET_FLAGS8(D.LowByte, t, r);
            SET_H(D.LowByte, (byte)t, (byte)r);
            D.LowByte = (byte)r;
        }
        void orb_ix()
        {
            fetch_effective_address();
            D.LowByte |= RM(EA.LowWord);
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        void addb_ix()
        {
            ushort t, r;
            fetch_effective_address();
            t = RM(EA.LowWord);
            r = (ushort)(D.LowByte + t);
            CLR_HNZVC();
            SET_FLAGS8(D.LowByte, t, r);
            SET_H(D.LowByte, (byte)t, (byte)r);
            D.LowByte = (byte)r;
        }
        void ldd_ix()
        {
            fetch_effective_address();
            D.LowWord = RM16(EA.LowWord);
            CLR_NZV();
            SET_NZ16(D.LowWord);
        }
        void std_ix()
        {
            fetch_effective_address();
            CLR_NZV();
            SET_NZ16(D.LowWord);
            WM16(EA.LowWord, D);
        }
        void ldu_ix()
        {
            fetch_effective_address();
            U.LowWord = RM16(EA.LowWord);
            CLR_NZV();
            SET_NZ16(U.LowWord);
        }
        void lds_ix()
        {
            fetch_effective_address();
            S.LowWord = RM16(EA.LowWord);
            CLR_NZV();
            SET_NZ16(S.LowWord);
            int_state |= M6809_LDS;
        }
        void stu_ix()
        {
            fetch_effective_address();
            CLR_NZV();
            SET_NZ16(U.LowWord);
            WM16(EA.LowWord, U);
        }
        void sts_ix()
        {
            fetch_effective_address();
            CLR_NZV();
            SET_NZ16(S.LowWord);
            WM16(EA.LowWord, S);
        }
        void subb_ex()
        {
            ushort t, r;
            t = EXTBYTE();
            r = (ushort)(D.LowByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, t, r);
            D.LowByte = (byte)r;
        }
        void cmpb_ex()
        {
            ushort t, r;
            t = EXTBYTE();
            r = (ushort)(D.LowByte - t);
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, t, r);
        }
        void sbcb_ex()
        {
            ushort t, r;
            t = EXTBYTE();
            r = (ushort)(D.LowByte - t - (CC & CC_C));
            CLR_NZVC();
            SET_FLAGS8(D.LowByte, t, r);
            D.LowByte = (byte)r;
        }
        void addd_ex()
        {
            uint r, d;
            Register b;
            b = EXTWORD();
            d = D.LowWord;
            r = d + b.d;
            CLR_NZVC();
            SET_FLAGS16((ushort)d, (ushort)b.d, r);
            D.LowWord = (ushort)r;
        }
        void andb_ex()
        {
            byte t;
            t = EXTBYTE();
            D.LowByte &= t;
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        void bitb_ex()
        {
            byte t, r;
            t = EXTBYTE();
            r = (byte)(D.LowByte & t);
            CLR_NZV();
            SET_NZ8(r);
        }
        void ldb_ex()
        {
            D.LowByte = EXTBYTE();
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        void stb_ex()
        {
            CLR_NZV();
            SET_NZ8(D.LowByte);
            EXTENDED();
            WM(EA.LowWord, D.LowByte);
        }
        void eorb_ex()
        {
            byte t;
            t = EXTBYTE();
            D.LowByte ^= t;
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        void adcb_ex()
        {
            ushort t, r;
            t = EXTBYTE();
            r = (ushort)(D.LowByte + t + (CC & CC_C));
            CLR_HNZVC();
            SET_FLAGS8(D.LowByte, t, r);
            SET_H(D.LowByte, (byte)t, (byte)r);
            D.LowByte = (byte)r;
        }
        void orb_ex()
        {
            byte t;
            t = EXTBYTE();
            D.LowByte |= t;
            CLR_NZV();
            SET_NZ8(D.LowByte);
        }
        void addb_ex()
        {
            ushort t, r;
            t = EXTBYTE();
            r = (ushort)(D.LowByte + t);
            CLR_HNZVC();
            SET_FLAGS8(D.LowByte, t, r);
            SET_H(D.LowByte, (byte)t, (byte)r);
            D.LowByte = (byte)r;
        }
        void ldd_ex()
        {
            D = EXTWORD();
            CLR_NZV();
            SET_NZ16(D.LowWord);
        }
        void std_ex()
        {
            CLR_NZV();
            SET_NZ16(D.LowWord);
            EXTENDED();
            WM16(EA.LowWord, D);
        }
        void ldu_ex()
        {
            U = EXTWORD();
            CLR_NZV();
            SET_NZ16(U.LowWord);
        }
        void lds_ex()
        {
            S = EXTWORD();
            CLR_NZV();
            SET_NZ16(S.LowWord);
            int_state |= M6809_LDS;
        }
        void stu_ex()
        {
            CLR_NZV();
            SET_NZ16(U.LowWord);
            EXTENDED();
            WM16(EA.LowWord, U);
        }
        void sts_ex()
        {
            CLR_NZV();
            SET_NZ16(S.LowWord);
            EXTENDED();
            WM16(EA.LowWord, S);
        }
        void pref10()
        {
            byte ireg2 = ReadOp(PC.LowWord);
            PC.LowWord++;
            switch (ireg2)
            {
                case 0x21: lbrn(); pendingCycles -= 5; break;
                case 0x22: lbhi(); pendingCycles -= 5; break;
                case 0x23: lbls(); pendingCycles -= 5; break;
                case 0x24: lbcc(); pendingCycles -= 5; break;
                case 0x25: lbcs(); pendingCycles -= 5; break;
                case 0x26: lbne(); pendingCycles -= 5; break;
                case 0x27: lbeq(); pendingCycles -= 5; break;
                case 0x28: lbvc(); pendingCycles -= 5; break;
                case 0x29: lbvs(); pendingCycles -= 5; break;
                case 0x2a: lbpl(); pendingCycles -= 5; break;
                case 0x2b: lbmi(); pendingCycles -= 5; break;
                case 0x2c: lbge(); pendingCycles -= 5; break;
                case 0x2d: lblt(); pendingCycles -= 5; break;
                case 0x2e: lbgt(); pendingCycles -= 5; break;
                case 0x2f: lble(); pendingCycles -= 5; break;

                case 0x3f: swi2(); pendingCycles -= 20; break;

                case 0x83: cmpd_im(); pendingCycles -= 5; break;
                case 0x8c: cmpy_im(); pendingCycles -= 5; break;
                case 0x8e: ldy_im(); pendingCycles -= 4; break;
                case 0x8f: sty_im(); pendingCycles -= 4; break;

                case 0x93: cmpd_di(); pendingCycles -= 7; break;
                case 0x9c: cmpy_di(); pendingCycles -= 7; break;
                case 0x9e: ldy_di(); pendingCycles -= 6; break;
                case 0x9f: sty_di(); pendingCycles -= 6; break;

                case 0xa3: cmpd_ix(); pendingCycles -= 7; break;
                case 0xac: cmpy_ix(); pendingCycles -= 7; break;
                case 0xae: ldy_ix(); pendingCycles -= 6; break;
                case 0xaf: sty_ix(); pendingCycles -= 6; break;

                case 0xb3: cmpd_ex(); pendingCycles -= 8; break;
                case 0xbc: cmpy_ex(); pendingCycles -= 8; break;
                case 0xbe: ldy_ex(); pendingCycles -= 7; break;
                case 0xbf: sty_ex(); pendingCycles -= 7; break;

                case 0xce: lds_im(); pendingCycles -= 4; break;
                case 0xcf: sts_im(); pendingCycles -= 4; break;

                case 0xde: lds_di(); pendingCycles -= 6; break;
                case 0xdf: sts_di(); pendingCycles -= 6; break;

                case 0xee: lds_ix(); pendingCycles -= 6; break;
                case 0xef: sts_ix(); pendingCycles -= 6; break;

                case 0xfe: lds_ex(); pendingCycles -= 7; break;
                case 0xff: sts_ex(); pendingCycles -= 7; break;

                default: illegal(); break;
            }
        }
        void pref11()
        {
            byte ireg2 = ReadOp(PC.LowWord);
            PC.LowWord++;
            switch (ireg2)
            {
                case 0x3f: swi3(); pendingCycles -= 20; break;

                case 0x83: cmpu_im(); pendingCycles -= 5; break;
                case 0x8c: cmps_im(); pendingCycles -= 5; break;

                case 0x93: cmpu_di(); pendingCycles -= 7; break;
                case 0x9c: cmps_di(); pendingCycles -= 7; break;

                case 0xa3: cmpu_ix(); pendingCycles -= 7; break;
                case 0xac: cmps_ix(); pendingCycles -= 7; break;

                case 0xb3: cmpu_ex(); pendingCycles -= 8; break;
                case 0xbc: cmps_ex(); pendingCycles -= 8; break;

                default: illegal(); break;
            }
        }
    }
}
