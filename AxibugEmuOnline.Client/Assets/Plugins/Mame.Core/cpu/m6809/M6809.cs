using MAME.Core;
using System;
using System.IO;

namespace cpu.m6809
{
    public partial class M6809 : cpuexec_data
    {
        public static M6809[] mm1;
        public Action[] insn;
        public Register PC, PPC, D, DP, U, S, X, Y, EA;
        public byte CC, ireg;
        public LineState[] irq_state = new LineState[2];
        public int extra_cycles; /* cycles used up by interrupts */
        public byte int_state;
        public LineState nmi_state;
        private byte CC_C = 0x01, CC_V = 0x02, CC_Z = 0x04, CC_N = 0x08, CC_II = 0x10, CC_H = 0x20, CC_IF = 0x40, CC_E = 0x80;
        private byte M6809_CWAI = 8, M6809_SYNC = 16, M6809_LDS = 32;
        private byte M6809_IRQ_LINE = 0, M6809_FIRQ_LINE = 1;
        public Func<ushort, byte> ReadOp, ReadOpArg;
        public Func<ushort, byte> RM;
        public Action<ushort, byte> WM;
        public Func<ushort, byte> ReadIO;
        public Action<ushort, byte> WriteIO;
        public delegate int irq_delegate(int irqline);
        public irq_delegate irq_callback;
        private ulong totalExecutedCycles;
        private int pendingCycles;
        public override ulong TotalExecutedCycles
        {
            get
            {
                return totalExecutedCycles;
            }
            set
            {
                totalExecutedCycles = value;
            }
        }
        public override int PendingCycles
        {
            get
            {
                return pendingCycles;
            }
            set
            {
                pendingCycles = value;
            }
        }
        public byte[] flags8i = new byte[256]
{
0x04,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
0x0a,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,
0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,
0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,
0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,
0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,
0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,
0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,
0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08
};
        public byte[] flags8d = new byte[256]
{
0x04,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x02,
0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,
0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,
0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,
0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,
0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,
0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,
0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,
0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08,0x08
};
        private byte[] cycles_6809 = new byte[]
        {
            0x06,0x06,0x02,0x06,0x06,0x02,0x06,0x06,0x06,0x06,0x06,0x02,0x06,0x06,0x03,0x06,
            0x00,0x00,0x02,0x04,0x02,0x02,0x05,0x09,0x02,0x02,0x03,0x02,0x03,0x02,0x08,0x06,
            0x03,0x03,0x03,0x03,0x03,0x03,0x03,0x03,0x03,0x03,0x03,0x03,0x03,0x03,0x03,0x03,
            0x04,0x04,0x04,0x04,0x05,0x05,0x05,0x05,0x02,0x05,0x03,0x06,0x14,0x0b,0x02,0x13,
            0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x02,
            0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x02,
            0x06,0x02,0x02,0x06,0x06,0x02,0x06,0x06,0x06,0x06,0x06,0x02,0x06,0x06,0x03,0x06,
            0x07,0x02,0x02,0x07,0x07,0x02,0x07,0x07,0x07,0x07,0x07,0x02,0x07,0x07,0x04,0x07,
            0x02,0x02,0x02,0x04,0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x04,0x07,0x03,0x02,
            0x04,0x04,0x04,0x06,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x06,0x07,0x05,0x05,
            0x04,0x04,0x04,0x06,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x06,0x07,0x05,0x05,
            0x05,0x05,0x05,0x07,0x05,0x05,0x05,0x05,0x05,0x05,0x05,0x05,0x07,0x08,0x06,0x06,
            0x02,0x02,0x02,0x04,0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x03,0x02,0x03,0x03,
            0x04,0x04,0x04,0x06,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x05,0x05,0x05,0x05,
            0x04,0x04,0x04,0x06,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x04,0x05,0x05,0x05,0x05,
            0x05,0x05,0x05,0x07,0x05,0x05,0x05,0x05,0x05,0x05,0x05,0x05,0x06,0x06,0x06,0x06
        };
        public M6809()
        {
            insn = new Action[]{
                neg_di,neg_di,illegal,com_di,lsr_di,illegal,ror_di,asr_di,
                asl_di,rol_di,dec_di,illegal,inc_di,tst_di,jmp_di,clr_di,
                pref10,pref11,nop,sync,illegal,illegal,lbra,lbsr,
                illegal,daa,orcc,illegal,andcc,sex,exg,tfr,
                bra,brn,bhi,bls,bcc,bcs,bne,beq,
                bvc,bvs,bpl,bmi,bge,blt,bgt,ble,
                leax,leay,leas,leau,pshs,puls,pshu,pulu,
                illegal,rts,abx,rti,cwai,mul,illegal,swi,
                nega,illegal,illegal,coma,lsra,illegal,rora,asra,
                asla,rola,deca,illegal,inca,tsta,illegal,clra,
                negb,illegal,illegal,comb,lsrb,illegal,rorb,asrb,
                aslb,rolb,decb,illegal,incb,tstb,illegal,clrb,
                neg_ix,illegal,illegal,com_ix,lsr_ix,illegal,ror_ix,asr_ix,
                asl_ix,rol_ix,dec_ix,illegal,inc_ix,tst_ix,jmp_ix,clr_ix,
                neg_ex,illegal,illegal,com_ex,lsr_ex,illegal,ror_ex,asr_ex,
                asl_ex,rol_ex,dec_ex,illegal,inc_ex,tst_ex,jmp_ex,clr_ex,
                suba_im,cmpa_im,sbca_im,subd_im,anda_im,bita_im,lda_im,sta_im,
                eora_im,adca_im,ora_im,adda_im,cmpx_im,bsr,ldx_im,stx_im,
                suba_di,cmpa_di,sbca_di,subd_di,anda_di,bita_di,lda_di,sta_di,
                eora_di,adca_di,ora_di,adda_di,cmpx_di,jsr_di,ldx_di,stx_di,
                suba_ix,cmpa_ix,sbca_ix,subd_ix,anda_ix,bita_ix,lda_ix,sta_ix,
                eora_ix,adca_ix,ora_ix,adda_ix,cmpx_ix,jsr_ix,ldx_ix,stx_ix,
                suba_ex,cmpa_ex,sbca_ex,subd_ex,anda_ex,bita_ex,lda_ex,sta_ex,
                eora_ex,adca_ex,ora_ex,adda_ex,cmpx_ex,jsr_ex,ldx_ex,stx_ex,
                subb_im,cmpb_im,sbcb_im,addd_im,andb_im,bitb_im,ldb_im,stb_im,
                eorb_im,adcb_im,orb_im,addb_im,ldd_im,std_im,ldu_im,stu_im,
                subb_di,cmpb_di,sbcb_di,addd_di,andb_di,bitb_di,ldb_di,stb_di,
                eorb_di,adcb_di,orb_di,addb_di,ldd_di,std_di,ldu_di,stu_di,
                subb_ix,cmpb_ix,sbcb_ix,addd_ix,andb_ix,bitb_ix,ldb_ix,stb_ix,
                eorb_ix,adcb_ix,orb_ix,addb_ix,ldd_ix,std_ix,ldu_ix,stu_ix,
                subb_ex,cmpb_ex,sbcb_ex,addd_ex,andb_ex,bitb_ex,ldb_ex,stb_ex,
                eorb_ex,adcb_ex,orb_ex,addb_ex,ldd_ex,std_ex,ldu_ex,stu_ex
            };
        }
        public override void Reset()
        {
            m6809_reset();
        }
        private void CHECK_IRQ_LINES()
        {
            if (irq_state[M6809_IRQ_LINE] != LineState.CLEAR_LINE || irq_state[M6809_FIRQ_LINE] != LineState.CLEAR_LINE)
                int_state &= (byte)(~M6809_SYNC);
            if (irq_state[M6809_FIRQ_LINE] != LineState.CLEAR_LINE && (CC & CC_IF) == 0)
            {
                if ((int_state & M6809_CWAI) != 0)
                {
                    int_state &= (byte)(~M6809_CWAI);
                    extra_cycles += 7;
                }
                else
                {
                    CC &= (byte)(~CC_E);
                    PUSHWORD(PC);
                    PUSHBYTE(CC);
                    extra_cycles += 10;
                }
                CC |= (byte)(CC_IF | CC_II);
                PC.LowWord = RM16(0xfff6);
                if (irq_callback != null)
                {
                    irq_callback(M6809_FIRQ_LINE);
                }
            }
            else if (irq_state[M6809_IRQ_LINE] != LineState.CLEAR_LINE && (CC & CC_II) == 0)
            {

                if ((int_state & M6809_CWAI) != 0)
                {
                    int_state &= (byte)(~M6809_CWAI);
                    extra_cycles += 7;
                }
                else
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
                    extra_cycles += 19;
                }
                CC |= CC_II;
                PC.LowWord = RM16(0xfff8);
                if (irq_callback != null)
                {
                    irq_callback(M6809_IRQ_LINE);
                }
            }
        }
        private byte IMMBYTE()
        {
            byte b = ReadOpArg(PC.LowWord);
            PC.LowWord++;
            return b;
        }
        private Register IMMWORD()
        {
            Register w = new Register();
            w.d = (uint)((ReadOpArg(PC.LowWord) << 8) | ReadOpArg((ushort)((PC.LowWord + 1) & 0xffff)));
            PC.LowWord += 2;
            return w;
        }
        private void PUSHBYTE(byte b)
        {
            --S.LowWord;
            WM(S.LowWord, b);
        }
        private void PUSHWORD(Register w)
        {
            --S.LowWord;
            WM(S.LowWord, w.LowByte);
            --S.LowWord;
            WM(S.LowWord, w.HighByte);
        }
        private byte PULLBYTE()
        {
            byte b;
            b = RM(S.LowWord);
            S.LowWord++;
            return b;
        }
        private ushort PULLWORD()
        {
            ushort w;
            w = (ushort)(RM(S.LowWord) << 8);
            S.LowWord++;
            w |= RM(S.LowWord);
            S.LowWord++;
            return w;
        }
        private void PSHUBYTE(byte b)
        {
            --U.LowWord; WM(U.LowWord, b);
        }
        private void PSHUWORD(Register w)
        {
            --U.LowWord;
            WM(U.LowWord, w.LowByte);
            --U.LowWord;
            WM(U.LowWord, w.HighByte);
        }
        private byte PULUBYTE()
        {
            byte b;
            b = RM(U.LowWord);
            U.LowWord++;
            return b;
        }
        private ushort PULUWORD()
        {
            ushort w;
            w = (ushort)(RM(U.LowWord) << 8);
            U.LowWord++;
            w |= RM(U.LowWord);
            U.LowWord++;
            return w;
        }
        private void CLR_HNZVC()
        {
            CC &= (byte)(~(CC_H | CC_N | CC_Z | CC_V | CC_C));
        }
        private void CLR_NZV()
        {
            CC &= (byte)(~(CC_N | CC_Z | CC_V));
        }
        private void CLR_NZ()
        {
            CC &= (byte)(~(CC_N | CC_Z));
        }
        private void CLR_HNZC()
        {
            CC &= (byte)(~(CC_H | CC_N | CC_Z | CC_C));
        }
        private void CLR_NZVC()
        {
            CC &= (byte)(~(CC_N | CC_Z | CC_V | CC_C));
        }
        private void CLR_Z()
        {
            CC &= (byte)(~(CC_Z));
        }
        private void CLR_NZC()
        {
            CC &= (byte)(~(CC_N | CC_Z | CC_C));
        }
        private void CLR_ZC()
        {
            CC &= (byte)(~(CC_Z | CC_C));
        }
        private void SET_Z(uint a)
        {
            if (a == 0)
            {
                SEZ();
            }
        }
        private void SET_Z8(byte a)
        {
            if (a == 0)
            {
                SEZ();
            }
        }
        private void SET_Z16(ushort a)
        {
            if (a == 0)
            {
                SEZ();
            }
        }
        private void SET_N8(byte a)
        {
            CC |= (byte)((a & 0x80) >> 4);
        }
        private void SET_N16(ushort a)
        {
            CC |= (byte)((a & 0x8000) >> 12);
        }
        private void SET_H(byte a, byte b, byte r)
        {
            CC |= (byte)(((a ^ b ^ r) & 0x10) << 1);
        }
        private void SET_C8(ushort a)
        {
            CC |= (byte)((a & 0x100) >> 8);
        }
        private void SET_C16(uint a)
        {
            CC |= (byte)((a & 0x10000) >> 16);
        }
        private void SET_V8(byte a, ushort b, ushort r)
        {
            CC |= (byte)(((a ^ b ^ r ^ (r >> 1)) & 0x80) >> 6);
        }
        private void SET_V16(ushort a, ushort b, uint r)
        {
            CC |= (byte)(((a ^ b ^ r ^ (r >> 1)) & 0x8000) >> 14);
        }
        private void SET_FLAGS8I(byte a)
        {
            CC |= flags8i[(a) & 0xff];
        }
        private void SET_FLAGS8D(byte a)
        {
            CC |= flags8d[(a) & 0xff];
        }
        private void SET_NZ8(byte a)
        {
            SET_N8(a);
            SET_Z(a);
        }
        private void SET_NZ16(ushort a)
        {
            SET_N16(a);
            SET_Z(a);
        }
        private void SET_FLAGS8(byte a, ushort b, ushort r)
        {
            SET_N8((byte)r);
            SET_Z8((byte)r);
            SET_V8(a, b, r);
            SET_C8(r);
        }
        private void SET_FLAGS16(ushort a, ushort b, uint r)
        {
            SET_N16((ushort)r);
            SET_Z16((ushort)r);
            SET_V16(a, b, r);
            SET_C16(r);
        }
        private ushort SIGNED(byte b)
        {
            return (ushort)((b & 0x80) != 0 ? b | 0xff00 : b);
        }
        private void DIRECT()
        {
            EA.d = DP.d;
            EA.LowByte = IMMBYTE();
        }
        private void IMM8()
        {
            EA.d = PC.d;
            PC.LowWord++;
        }
        private void IMM16()
        {
            EA.d = PC.d;
            PC.LowWord += 2;
        }
        private void EXTENDED()
        {
            EA = IMMWORD();
        }
        private void SEC()
        {
            CC |= CC_C;
        }
        private void CLC()
        {
            CC &= (byte)(~CC_C);
        }
        private void SEZ()
        {
            CC |= CC_Z;
        }
        private void CLZ()
        {
            CC &= (byte)(~CC_Z);
        }
        private void SEN()
        {
            CC |= CC_N;
        }
        private void CLN()
        {
            CC &= (byte)(~CC_N);
        }
        private void SEV()
        {
            CC |= CC_V;
        }
        private void CLV()
        {
            CC &= (byte)(~CC_V);
        }
        private void SEH()
        {
            CC |= CC_H;
        }
        private void CLH()
        {
            CC &= (byte)(~CC_H);
        }
        private byte DIRBYTE()
        {
            DIRECT();
            return RM(EA.LowWord);
        }
        private Register DIRWORD()
        {
            Register w = new Register();
            DIRECT();
            w.LowWord = RM16(EA.LowWord);
            return w;
        }
        private byte EXTBYTE()
        {
            EXTENDED();
            return RM(EA.LowWord);
        }
        private Register EXTWORD()
        {
            Register w = new Register();
            EXTENDED();
            w.LowWord = RM16(EA.LowWord);
            return w;
        }
        private void BRANCH(bool f)
        {
            byte t = IMMBYTE();
            if (f)
            {
                PC.LowWord += (ushort)SIGNED(t);
            }
        }
        private void LBRANCH(bool f)
        {
            Register t = IMMWORD();
            if (f)
            {
                pendingCycles -= 1;
                PC.LowWord += t.LowWord;
            }
        }
        private byte NXORV()
        {
            return (byte)((CC & CC_N) ^ ((CC & CC_V) << 2));
        }
        private ushort RM16(ushort Addr)
        {
            ushort result = (ushort)(RM(Addr) << 8);
            return (ushort)(result | RM((ushort)((Addr + 1) & 0xffff)));
        }
        private void WM16(ushort Addr, Register p)
        {
            WM(Addr, p.HighByte);
            WM((ushort)((Addr + 1) & 0xffff), p.LowByte);
        }
        private void m6809_reset()
        {
            int_state = 0;
            nmi_state = LineState.CLEAR_LINE;
            irq_state[0] = LineState.CLEAR_LINE;
            irq_state[1] = LineState.CLEAR_LINE;
            DP.d = 0;			/* Reset direct page register */
            CC |= CC_II;        /* IRQ disabled */
            CC |= CC_IF;        /* FIRQ disabled */
            PC.LowWord = RM16(0xfffe);
        }
        public override void set_irq_line(int irqline, LineState state)
        {
            if (irqline == (int)LineState.INPUT_LINE_NMI)
            {
                if (nmi_state == state)
                    return;
                nmi_state = state;
                if (state == LineState.CLEAR_LINE)
                    return;
                if ((int_state & M6809_LDS) == 0)
                    return;
                int_state &= (byte)(~M6809_SYNC);
                if ((int_state & M6809_CWAI) != 0)
                {
                    int_state &= (byte)(~M6809_CWAI);
                    extra_cycles += 7;
                }
                else
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
                    extra_cycles += 19;
                }
                CC |= (byte)(CC_IF | CC_II);
                PC.LowWord = RM16(0xfffc);
            }
            else if (irqline < 2)
            {
                irq_state[irqline] = state;
                if (state == LineState.CLEAR_LINE)
                    return;
                CHECK_IRQ_LINES();
            }
        }
        public override void cpunum_set_input_line_and_vector(int cpunum, int line, LineState state, int vector)
        {
            EmuTimer.timer_set_internal(EmuTimer.TIME_ACT.Cpuint_cpunum_empty_event_queue);
        }
        public override int ExecuteCycles(int cycles)
        {
            pendingCycles = cycles - extra_cycles;
            extra_cycles = 0;
            if ((int_state & (M6809_CWAI | M6809_SYNC)) != 0)
            {
                pendingCycles = 0;
            }
            else
            {
                do
                {
                    int prevCycles = pendingCycles;
                    PPC = PC;
                    ireg = ReadOp(PC.LowWord);
                    PC.LowWord++;
                    insn[ireg]();
                    pendingCycles -= cycles_6809[ireg];
                    int delta = prevCycles - pendingCycles;
                    totalExecutedCycles += (ulong)delta;
                } while (pendingCycles > 0);

                pendingCycles -= extra_cycles;
                extra_cycles = 0;
            }
            return cycles - pendingCycles;
        }
        private void fetch_effective_address()
        {
            byte postbyte = ReadOpArg(PC.LowWord);
            PC.LowWord++;
            switch (postbyte)
            {
                case 0x00: EA.LowWord = X.LowWord; pendingCycles -= 1; break;
                case 0x01: EA.LowWord = (ushort)(X.LowWord + 1); pendingCycles -= 1; break;
                case 0x02: EA.LowWord = (ushort)(X.LowWord + 2); pendingCycles -= 1; break;
                case 0x03: EA.LowWord = (ushort)(X.LowWord + 3); pendingCycles -= 1; break;
                case 0x04: EA.LowWord = (ushort)(X.LowWord + 4); pendingCycles -= 1; break;
                case 0x05: EA.LowWord = (ushort)(X.LowWord + 5); pendingCycles -= 1; break;
                case 0x06: EA.LowWord = (ushort)(X.LowWord + 6); pendingCycles -= 1; break;
                case 0x07: EA.LowWord = (ushort)(X.LowWord + 7); pendingCycles -= 1; break;
                case 0x08: EA.LowWord = (ushort)(X.LowWord + 8); pendingCycles -= 1; break;
                case 0x09: EA.LowWord = (ushort)(X.LowWord + 9); pendingCycles -= 1; break;
                case 0x0a: EA.LowWord = (ushort)(X.LowWord + 10); pendingCycles -= 1; break;
                case 0x0b: EA.LowWord = (ushort)(X.LowWord + 11); pendingCycles -= 1; break;
                case 0x0c: EA.LowWord = (ushort)(X.LowWord + 12); pendingCycles -= 1; break;
                case 0x0d: EA.LowWord = (ushort)(X.LowWord + 13); pendingCycles -= 1; break;
                case 0x0e: EA.LowWord = (ushort)(X.LowWord + 14); pendingCycles -= 1; break;
                case 0x0f: EA.LowWord = (ushort)(X.LowWord + 15); pendingCycles -= 1; break;

                case 0x10: EA.LowWord = (ushort)(X.LowWord - 16); pendingCycles -= 1; break;
                case 0x11: EA.LowWord = (ushort)(X.LowWord - 15); pendingCycles -= 1; break;
                case 0x12: EA.LowWord = (ushort)(X.LowWord - 14); pendingCycles -= 1; break;
                case 0x13: EA.LowWord = (ushort)(X.LowWord - 13); pendingCycles -= 1; break;
                case 0x14: EA.LowWord = (ushort)(X.LowWord - 12); pendingCycles -= 1; break;
                case 0x15: EA.LowWord = (ushort)(X.LowWord - 11); pendingCycles -= 1; break;
                case 0x16: EA.LowWord = (ushort)(X.LowWord - 10); pendingCycles -= 1; break;
                case 0x17: EA.LowWord = (ushort)(X.LowWord - 9); pendingCycles -= 1; break;
                case 0x18: EA.LowWord = (ushort)(X.LowWord - 8); pendingCycles -= 1; break;
                case 0x19: EA.LowWord = (ushort)(X.LowWord - 7); pendingCycles -= 1; break;
                case 0x1a: EA.LowWord = (ushort)(X.LowWord - 6); pendingCycles -= 1; break;
                case 0x1b: EA.LowWord = (ushort)(X.LowWord - 5); pendingCycles -= 1; break;
                case 0x1c: EA.LowWord = (ushort)(X.LowWord - 4); pendingCycles -= 1; break;
                case 0x1d: EA.LowWord = (ushort)(X.LowWord - 3); pendingCycles -= 1; break;
                case 0x1e: EA.LowWord = (ushort)(X.LowWord - 2); pendingCycles -= 1; break;
                case 0x1f: EA.LowWord = (ushort)(X.LowWord - 1); pendingCycles -= 1; break;

                case 0x20: EA.LowWord = Y.LowWord; pendingCycles -= 1; break;
                case 0x21: EA.LowWord = (ushort)(Y.LowWord + 1); pendingCycles -= 1; break;
                case 0x22: EA.LowWord = (ushort)(Y.LowWord + 2); pendingCycles -= 1; break;
                case 0x23: EA.LowWord = (ushort)(Y.LowWord + 3); pendingCycles -= 1; break;
                case 0x24: EA.LowWord = (ushort)(Y.LowWord + 4); pendingCycles -= 1; break;
                case 0x25: EA.LowWord = (ushort)(Y.LowWord + 5); pendingCycles -= 1; break;
                case 0x26: EA.LowWord = (ushort)(Y.LowWord + 6); pendingCycles -= 1; break;
                case 0x27: EA.LowWord = (ushort)(Y.LowWord + 7); pendingCycles -= 1; break;
                case 0x28: EA.LowWord = (ushort)(Y.LowWord + 8); pendingCycles -= 1; break;
                case 0x29: EA.LowWord = (ushort)(Y.LowWord + 9); pendingCycles -= 1; break;
                case 0x2a: EA.LowWord = (ushort)(Y.LowWord + 10); pendingCycles -= 1; break;
                case 0x2b: EA.LowWord = (ushort)(Y.LowWord + 11); pendingCycles -= 1; break;
                case 0x2c: EA.LowWord = (ushort)(Y.LowWord + 12); pendingCycles -= 1; break;
                case 0x2d: EA.LowWord = (ushort)(Y.LowWord + 13); pendingCycles -= 1; break;
                case 0x2e: EA.LowWord = (ushort)(Y.LowWord + 14); pendingCycles -= 1; break;
                case 0x2f: EA.LowWord = (ushort)(Y.LowWord + 15); pendingCycles -= 1; break;

                case 0x30: EA.LowWord = (ushort)(Y.LowWord - 16); pendingCycles -= 1; break;
                case 0x31: EA.LowWord = (ushort)(Y.LowWord - 15); pendingCycles -= 1; break;
                case 0x32: EA.LowWord = (ushort)(Y.LowWord - 14); pendingCycles -= 1; break;
                case 0x33: EA.LowWord = (ushort)(Y.LowWord - 13); pendingCycles -= 1; break;
                case 0x34: EA.LowWord = (ushort)(Y.LowWord - 12); pendingCycles -= 1; break;
                case 0x35: EA.LowWord = (ushort)(Y.LowWord - 11); pendingCycles -= 1; break;
                case 0x36: EA.LowWord = (ushort)(Y.LowWord - 10); pendingCycles -= 1; break;
                case 0x37: EA.LowWord = (ushort)(Y.LowWord - 9); pendingCycles -= 1; break;
                case 0x38: EA.LowWord = (ushort)(Y.LowWord - 8); pendingCycles -= 1; break;
                case 0x39: EA.LowWord = (ushort)(Y.LowWord - 7); pendingCycles -= 1; break;
                case 0x3a: EA.LowWord = (ushort)(Y.LowWord - 6); pendingCycles -= 1; break;
                case 0x3b: EA.LowWord = (ushort)(Y.LowWord - 5); pendingCycles -= 1; break;
                case 0x3c: EA.LowWord = (ushort)(Y.LowWord - 4); pendingCycles -= 1; break;
                case 0x3d: EA.LowWord = (ushort)(Y.LowWord - 3); pendingCycles -= 1; break;
                case 0x3e: EA.LowWord = (ushort)(Y.LowWord - 2); pendingCycles -= 1; break;
                case 0x3f: EA.LowWord = (ushort)(Y.LowWord - 1); pendingCycles -= 1; break;

                case 0x40: EA.LowWord = U.LowWord; pendingCycles -= 1; break;
                case 0x41: EA.LowWord = (ushort)(U.LowWord + 1); pendingCycles -= 1; break;
                case 0x42: EA.LowWord = (ushort)(U.LowWord + 2); pendingCycles -= 1; break;
                case 0x43: EA.LowWord = (ushort)(U.LowWord + 3); pendingCycles -= 1; break;
                case 0x44: EA.LowWord = (ushort)(U.LowWord + 4); pendingCycles -= 1; break;
                case 0x45: EA.LowWord = (ushort)(U.LowWord + 5); pendingCycles -= 1; break;
                case 0x46: EA.LowWord = (ushort)(U.LowWord + 6); pendingCycles -= 1; break;
                case 0x47: EA.LowWord = (ushort)(U.LowWord + 7); pendingCycles -= 1; break;
                case 0x48: EA.LowWord = (ushort)(U.LowWord + 8); pendingCycles -= 1; break;
                case 0x49: EA.LowWord = (ushort)(U.LowWord + 9); pendingCycles -= 1; break;
                case 0x4a: EA.LowWord = (ushort)(U.LowWord + 10); pendingCycles -= 1; break;
                case 0x4b: EA.LowWord = (ushort)(U.LowWord + 11); pendingCycles -= 1; break;
                case 0x4c: EA.LowWord = (ushort)(U.LowWord + 12); pendingCycles -= 1; break;
                case 0x4d: EA.LowWord = (ushort)(U.LowWord + 13); pendingCycles -= 1; break;
                case 0x4e: EA.LowWord = (ushort)(U.LowWord + 14); pendingCycles -= 1; break;
                case 0x4f: EA.LowWord = (ushort)(U.LowWord + 15); pendingCycles -= 1; break;

                case 0x50: EA.LowWord = (ushort)(U.LowWord - 16); pendingCycles -= 1; break;
                case 0x51: EA.LowWord = (ushort)(U.LowWord - 15); pendingCycles -= 1; break;
                case 0x52: EA.LowWord = (ushort)(U.LowWord - 14); pendingCycles -= 1; break;
                case 0x53: EA.LowWord = (ushort)(U.LowWord - 13); pendingCycles -= 1; break;
                case 0x54: EA.LowWord = (ushort)(U.LowWord - 12); pendingCycles -= 1; break;
                case 0x55: EA.LowWord = (ushort)(U.LowWord - 11); pendingCycles -= 1; break;
                case 0x56: EA.LowWord = (ushort)(U.LowWord - 10); pendingCycles -= 1; break;
                case 0x57: EA.LowWord = (ushort)(U.LowWord - 9); pendingCycles -= 1; break;
                case 0x58: EA.LowWord = (ushort)(U.LowWord - 8); pendingCycles -= 1; break;
                case 0x59: EA.LowWord = (ushort)(U.LowWord - 7); pendingCycles -= 1; break;
                case 0x5a: EA.LowWord = (ushort)(U.LowWord - 6); pendingCycles -= 1; break;
                case 0x5b: EA.LowWord = (ushort)(U.LowWord - 5); pendingCycles -= 1; break;
                case 0x5c: EA.LowWord = (ushort)(U.LowWord - 4); pendingCycles -= 1; break;
                case 0x5d: EA.LowWord = (ushort)(U.LowWord - 3); pendingCycles -= 1; break;
                case 0x5e: EA.LowWord = (ushort)(U.LowWord - 2); pendingCycles -= 1; break;
                case 0x5f: EA.LowWord = (ushort)(U.LowWord - 1); pendingCycles -= 1; break;

                case 0x60: EA.LowWord = S.LowWord; pendingCycles -= 1; break;
                case 0x61: EA.LowWord = (ushort)(S.LowWord + 1); pendingCycles -= 1; break;
                case 0x62: EA.LowWord = (ushort)(S.LowWord + 2); pendingCycles -= 1; break;
                case 0x63: EA.LowWord = (ushort)(S.LowWord + 3); pendingCycles -= 1; break;
                case 0x64: EA.LowWord = (ushort)(S.LowWord + 4); pendingCycles -= 1; break;
                case 0x65: EA.LowWord = (ushort)(S.LowWord + 5); pendingCycles -= 1; break;
                case 0x66: EA.LowWord = (ushort)(S.LowWord + 6); pendingCycles -= 1; break;
                case 0x67: EA.LowWord = (ushort)(S.LowWord + 7); pendingCycles -= 1; break;
                case 0x68: EA.LowWord = (ushort)(S.LowWord + 8); pendingCycles -= 1; break;
                case 0x69: EA.LowWord = (ushort)(S.LowWord + 9); pendingCycles -= 1; break;
                case 0x6a: EA.LowWord = (ushort)(S.LowWord + 10); pendingCycles -= 1; break;
                case 0x6b: EA.LowWord = (ushort)(S.LowWord + 11); pendingCycles -= 1; break;
                case 0x6c: EA.LowWord = (ushort)(S.LowWord + 12); pendingCycles -= 1; break;
                case 0x6d: EA.LowWord = (ushort)(S.LowWord + 13); pendingCycles -= 1; break;
                case 0x6e: EA.LowWord = (ushort)(S.LowWord + 14); pendingCycles -= 1; break;
                case 0x6f: EA.LowWord = (ushort)(S.LowWord + 15); pendingCycles -= 1; break;

                case 0x70: EA.LowWord = (ushort)(S.LowWord - 16); pendingCycles -= 1; break;
                case 0x71: EA.LowWord = (ushort)(S.LowWord - 15); pendingCycles -= 1; break;
                case 0x72: EA.LowWord = (ushort)(S.LowWord - 14); pendingCycles -= 1; break;
                case 0x73: EA.LowWord = (ushort)(S.LowWord - 13); pendingCycles -= 1; break;
                case 0x74: EA.LowWord = (ushort)(S.LowWord - 12); pendingCycles -= 1; break;
                case 0x75: EA.LowWord = (ushort)(S.LowWord - 11); pendingCycles -= 1; break;
                case 0x76: EA.LowWord = (ushort)(S.LowWord - 10); pendingCycles -= 1; break;
                case 0x77: EA.LowWord = (ushort)(S.LowWord - 9); pendingCycles -= 1; break;
                case 0x78: EA.LowWord = (ushort)(S.LowWord - 8); pendingCycles -= 1; break;
                case 0x79: EA.LowWord = (ushort)(S.LowWord - 7); pendingCycles -= 1; break;
                case 0x7a: EA.LowWord = (ushort)(S.LowWord - 6); pendingCycles -= 1; break;
                case 0x7b: EA.LowWord = (ushort)(S.LowWord - 5); pendingCycles -= 1; break;
                case 0x7c: EA.LowWord = (ushort)(S.LowWord - 4); pendingCycles -= 1; break;
                case 0x7d: EA.LowWord = (ushort)(S.LowWord - 3); pendingCycles -= 1; break;
                case 0x7e: EA.LowWord = (ushort)(S.LowWord - 2); pendingCycles -= 1; break;
                case 0x7f: EA.LowWord = (ushort)(S.LowWord - 1); pendingCycles -= 1; break;

                case 0x80: EA.LowWord = X.LowWord; X.LowWord++; pendingCycles -= 2; break;
                case 0x81: EA.LowWord = X.LowWord; X.LowWord += 2; pendingCycles -= 3; break;
                case 0x82: X.LowWord--; EA.LowWord = X.LowWord; pendingCycles -= 2; break;
                case 0x83: X.LowWord -= 2; EA.LowWord = X.LowWord; pendingCycles -= 3; break;
                case 0x84: EA.LowWord = X.LowWord; break;
                case 0x85: EA.LowWord = (ushort)(X.LowWord + SIGNED(D.LowByte)); pendingCycles -= 1; break;
                case 0x86: EA.LowWord = (ushort)(X.LowWord + SIGNED(D.HighByte)); pendingCycles -= 1; break;
                case 0x87: EA.LowWord = 0; break; /*   ILLEGAL*/
                case 0x88: EA.LowWord = IMMBYTE(); EA.LowWord = (ushort)(X.LowWord + SIGNED(EA.LowByte)); pendingCycles -= 1; break; /* this is a hack to make Vectrex work. It should be m6809_ICount-=1. Dunno where the cycle was lost :( */
                case 0x89: EA = IMMWORD(); EA.LowWord += X.LowWord; pendingCycles -= 4; break;
                case 0x8a: EA.LowWord = 0; break; /*   ILLEGAL*/
                case 0x8b: EA.LowWord = (ushort)(X.LowWord + D.LowWord); pendingCycles -= 4; break;
                case 0x8c: EA.LowWord = IMMBYTE(); EA.LowWord = (ushort)(PC.LowWord + SIGNED(EA.LowByte)); pendingCycles -= 1; break;
                case 0x8d: EA = IMMWORD(); EA.LowWord += PC.LowWord; pendingCycles -= 5; break;
                case 0x8e: EA.LowWord = 0; break; /*   ILLEGAL*/
                case 0x8f: EA = IMMWORD(); pendingCycles -= 5; break;

                case 0x90: EA.LowWord = X.LowWord; X.LowWord++; EA.d = RM16(EA.LowWord); pendingCycles -= 5; break; /* Indirect ,R+ not in my specs */
                case 0x91: EA.LowWord = X.LowWord; X.LowWord += 2; EA.d = RM16(EA.LowWord); pendingCycles -= 6; break;
                case 0x92: X.LowWord--; EA.LowWord = X.LowWord; EA.d = RM16(EA.LowWord); pendingCycles -= 5; break;
                case 0x93: X.LowWord -= 2; EA.LowWord = X.LowWord; EA.d = RM16(EA.LowWord); pendingCycles -= 6; break;
                case 0x94: EA.LowWord = X.LowWord; EA.d = RM16(EA.LowWord); pendingCycles -= 3; break;
                case 0x95: EA.LowWord = (ushort)(X.LowWord + SIGNED(D.LowByte)); EA.d = RM16(EA.LowWord); pendingCycles -= 4; break;
                case 0x96: EA.LowWord = (ushort)(X.LowWord + SIGNED(D.HighByte)); EA.d = RM16(EA.LowWord); pendingCycles -= 4; break;
                case 0x97: EA.LowWord = 0; break; /*   ILLEGAL*/
                case 0x98: EA.LowWord = IMMBYTE(); EA.LowWord = (ushort)(X.LowWord + SIGNED(EA.LowByte)); EA.d = RM16(EA.LowWord); pendingCycles -= 4; break;
                case 0x99: EA = IMMWORD(); EA.LowWord += X.LowWord; EA.d = RM16(EA.LowWord); pendingCycles -= 7; break;
                case 0x9a: EA.LowWord = 0; break; /*   ILLEGAL*/
                case 0x9b: EA.LowWord = (ushort)(X.LowWord + D.LowWord); EA.d = RM16(EA.LowWord); pendingCycles -= 7; break;
                case 0x9c: EA.LowWord = IMMBYTE(); EA.LowWord = (ushort)(PC.LowWord + SIGNED(EA.LowByte)); EA.d = RM16(EA.LowWord); pendingCycles -= 4; break;
                case 0x9d: EA = IMMWORD(); EA.LowWord += PC.LowWord; EA.d = RM16(EA.LowWord); pendingCycles -= 8; break;
                case 0x9e: EA.LowWord = 0; break; /*   ILLEGAL*/
                case 0x9f: EA = IMMWORD(); EA.d = RM16(EA.LowWord); pendingCycles -= 8; break;

                case 0xa0: EA.LowWord = Y.LowWord; Y.LowWord++; pendingCycles -= 2; break;
                case 0xa1: EA.LowWord = Y.LowWord; Y.LowWord += 2; pendingCycles -= 3; break;
                case 0xa2: Y.LowWord--; EA.LowWord = Y.LowWord; pendingCycles -= 2; break;
                case 0xa3: Y.LowWord -= 2; EA.LowWord = Y.LowWord; pendingCycles -= 3; break;
                case 0xa4: EA.LowWord = Y.LowWord; break;
                case 0xa5: EA.LowWord = (ushort)(Y.LowWord + SIGNED(D.LowByte)); pendingCycles -= 1; break;
                case 0xa6: EA.LowWord = (ushort)(Y.LowWord + SIGNED(D.HighByte)); pendingCycles -= 1; break;
                case 0xa7: EA.LowWord = 0; break; /*   ILLEGAL*/
                case 0xa8: EA.LowWord = IMMBYTE(); EA.LowWord = (ushort)(Y.LowWord + SIGNED(EA.LowByte)); pendingCycles -= 1; break;
                case 0xa9: EA = IMMWORD(); EA.LowWord += Y.LowWord; pendingCycles -= 4; break;
                case 0xaa: EA.LowWord = 0; break; /*   ILLEGAL*/
                case 0xab: EA.LowWord = (ushort)(Y.LowWord + D.LowWord); pendingCycles -= 4; break;
                case 0xac: EA.LowWord = IMMBYTE(); EA.LowWord = (ushort)(PC.LowWord + SIGNED(EA.LowByte)); pendingCycles -= 1; break;
                case 0xad: EA = IMMWORD(); EA.LowWord += PC.LowWord; pendingCycles -= 5; break;
                case 0xae: EA.LowWord = 0; break; /*   ILLEGAL*/
                case 0xaf: EA = IMMWORD(); pendingCycles -= 5; break;

                case 0xb0: EA.LowWord = Y.LowWord; Y.LowWord++; EA.LowWord = RM16(EA.LowWord); pendingCycles -= 5; break;
                case 0xb1: EA.LowWord = Y.LowWord; Y.LowWord += 2; EA.LowWord = RM16(EA.LowWord); pendingCycles -= 6; break;
                case 0xb2: Y.LowWord--; EA.LowWord = Y.LowWord; EA.LowWord = RM16(EA.LowWord); pendingCycles -= 5; break;
                case 0xb3: Y.LowWord -= 2; EA.LowWord = Y.LowWord; EA.LowWord = RM16(EA.LowWord); pendingCycles -= 6; break;
                case 0xb4: EA.LowWord = Y.LowWord; EA.LowWord = RM16(EA.LowWord); pendingCycles -= 3; break;
                case 0xb5: EA.LowWord = (ushort)(Y.LowWord + SIGNED(D.LowByte)); EA.LowWord = RM16(EA.LowWord); pendingCycles -= 4; break;
                case 0xb6: EA.LowWord = (ushort)(Y.LowWord + SIGNED(D.HighByte)); EA.LowWord = RM16(EA.LowWord); pendingCycles -= 4; break;
                case 0xb7: EA.LowWord = 0; break; /*   ILLEGAL*/
                case 0xb8: EA.LowWord = IMMBYTE(); EA.LowWord = (ushort)(Y.LowWord + SIGNED(EA.LowByte)); EA.LowWord = RM16(EA.LowWord); pendingCycles -= 4; break;
                case 0xb9: EA = IMMWORD(); EA.LowWord += Y.LowWord; EA.LowWord = RM16(EA.LowWord); pendingCycles -= 7; break;
                case 0xba: EA.LowWord = 0; break; /*   ILLEGAL*/
                case 0xbb: EA.LowWord = (ushort)(Y.LowWord + D.LowWord); EA.d = RM16(EA.LowWord); pendingCycles -= 7; break;
                case 0xbc: EA.LowWord = IMMBYTE(); EA.LowWord = (ushort)(PC.LowWord + SIGNED(EA.LowByte)); EA.d = RM16(EA.LowWord); pendingCycles -= 4; break;
                case 0xbd: EA = IMMWORD(); EA.LowWord += PC.LowWord; EA.d = RM16(EA.LowWord); pendingCycles -= 8; break;
                case 0xbe: EA.LowWord = 0; break; /*   ILLEGAL*/
                case 0xbf: EA = IMMWORD(); EA.d = RM16(EA.LowWord); pendingCycles -= 8; break;

                case 0xc0: EA.LowWord = U.LowWord; U.LowWord++; pendingCycles -= 2; break;
                case 0xc1: EA.LowWord = U.LowWord; U.LowWord += 2; pendingCycles -= 3; break;
                case 0xc2: U.LowWord--; EA.LowWord = U.LowWord; pendingCycles -= 2; break;
                case 0xc3: U.LowWord -= 2; EA.LowWord = U.LowWord; pendingCycles -= 3; break;
                case 0xc4: EA.LowWord = U.LowWord; break;
                case 0xc5: EA.LowWord = (ushort)(U.LowWord + SIGNED(D.LowByte)); pendingCycles -= 1; break;
                case 0xc6: EA.LowWord = (ushort)(U.LowWord + SIGNED(D.HighByte)); pendingCycles -= 1; break;
                case 0xc7: EA.LowWord = 0; break; /*ILLEGAL*/
                case 0xc8: EA.LowWord = IMMBYTE(); EA.LowWord = (ushort)(U.LowWord + SIGNED(EA.LowByte)); pendingCycles -= 1; break;
                case 0xc9: EA = IMMWORD(); EA.LowWord += U.LowWord; pendingCycles -= 4; break;
                case 0xca: EA.LowWord = 0; break; /*ILLEGAL*/
                case 0xcb: EA.LowWord = (ushort)(U.LowWord + D.LowWord); pendingCycles -= 4; break;
                case 0xcc: EA.LowWord = IMMBYTE(); EA.LowWord = (ushort)(PC.LowWord + SIGNED(EA.LowByte)); pendingCycles -= 1; break;
                case 0xcd: EA = IMMWORD(); EA.LowWord += PC.LowWord; pendingCycles -= 5; break;
                case 0xce: EA.LowWord = 0; break; /*ILLEGAL*/
                case 0xcf: EA = IMMWORD(); pendingCycles -= 5; break;

                case 0xd0: EA.LowWord = U.LowWord; U.LowWord++; EA.d = RM16(EA.LowWord); pendingCycles -= 5; break;
                case 0xd1: EA.LowWord = U.LowWord; U.LowWord += 2; EA.d = RM16(EA.LowWord); pendingCycles -= 6; break;
                case 0xd2: U.LowWord--; EA.LowWord = U.LowWord; EA.d = RM16(EA.LowWord); pendingCycles -= 5; break;
                case 0xd3: U.LowWord -= 2; EA.LowWord = U.LowWord; EA.d = RM16(EA.LowWord); pendingCycles -= 6; break;
                case 0xd4: EA.LowWord = U.LowWord; EA.d = RM16(EA.LowWord); pendingCycles -= 3; break;
                case 0xd5: EA.LowWord = (ushort)(U.LowWord + SIGNED(D.LowByte)); EA.d = RM16(EA.LowWord); pendingCycles -= 4; break;
                case 0xd6: EA.LowWord = (ushort)(U.LowWord + SIGNED(D.HighByte)); EA.d = RM16(EA.LowWord); pendingCycles -= 4; break;
                case 0xd7: EA.LowWord = 0; break; /*ILLEGAL*/
                case 0xd8: EA.LowWord = IMMBYTE(); EA.LowWord = (ushort)(U.LowWord + SIGNED(EA.LowByte)); EA.d = RM16(EA.LowWord); pendingCycles -= 4; break;
                case 0xd9: EA = IMMWORD(); EA.LowWord += U.LowWord; EA.d = RM16(EA.LowWord); pendingCycles -= 7; break;
                case 0xda: EA.LowWord = 0; break; /*ILLEGAL*/
                case 0xdb: EA.LowWord = (ushort)(U.LowWord + D.LowWord); EA.d = RM16(EA.LowWord); pendingCycles -= 7; break;
                case 0xdc: EA.LowWord = IMMBYTE(); EA.LowWord = (ushort)(PC.LowWord + SIGNED(EA.LowByte)); EA.d = RM16(EA.LowWord); pendingCycles -= 4; break;
                case 0xdd: EA = IMMWORD(); EA.LowWord += PC.LowWord; EA.d = RM16(EA.LowWord); pendingCycles -= 8; break;
                case 0xde: EA.LowWord = 0; break; /*ILLEGAL*/
                case 0xdf: EA = IMMWORD(); EA.d = RM16(EA.LowWord); pendingCycles -= 8; break;

                case 0xe0: EA.LowWord = S.LowWord; S.LowWord++; pendingCycles -= 2; break;
                case 0xe1: EA.LowWord = S.LowWord; S.LowWord += 2; pendingCycles -= 3; break;
                case 0xe2: S.LowWord--; EA.LowWord = S.LowWord; pendingCycles -= 2; break;
                case 0xe3: S.LowWord -= 2; EA.LowWord = S.LowWord; pendingCycles -= 3; break;
                case 0xe4: EA.LowWord = S.LowWord; break;
                case 0xe5: EA.LowWord = (ushort)(S.LowWord + SIGNED(D.LowByte)); pendingCycles -= 1; break;
                case 0xe6: EA.LowWord = (ushort)(S.LowWord + SIGNED(D.HighByte)); pendingCycles -= 1; break;
                case 0xe7: EA.LowWord = 0; break; /*ILLEGAL*/
                case 0xe8: EA.LowWord = IMMBYTE(); EA.LowWord = (ushort)(S.LowWord + SIGNED(EA.LowByte)); pendingCycles -= 1; break;
                case 0xe9: EA = IMMWORD(); EA.LowWord += S.LowWord; pendingCycles -= 4; break;
                case 0xea: EA.LowWord = 0; break; /*ILLEGAL*/
                case 0xeb: EA.LowWord = (ushort)(S.LowWord + D.LowWord); pendingCycles -= 4; break;
                case 0xec: EA.LowWord = IMMBYTE(); EA.LowWord = (ushort)(PC.LowWord + SIGNED(EA.LowByte)); pendingCycles -= 1; break;
                case 0xed: EA = IMMWORD(); EA.LowWord += PC.LowWord; pendingCycles -= 5; break;
                case 0xee: EA.LowWord = 0; break;  /*ILLEGAL*/
                case 0xef: EA = IMMWORD(); pendingCycles -= 5; break;

                case 0xf0: EA.LowWord = S.LowWord; S.LowWord++; EA.d = RM16(EA.LowWord); pendingCycles -= 5; break;
                case 0xf1: EA.LowWord = S.LowWord; S.LowWord += 2; EA.d = RM16(EA.LowWord); pendingCycles -= 6; break;
                case 0xf2: S.LowWord--; EA.LowWord = S.LowWord; EA.d = RM16(EA.LowWord); pendingCycles -= 5; break;
                case 0xf3: S.LowWord -= 2; EA.LowWord = S.LowWord; EA.d = RM16(EA.LowWord); pendingCycles -= 6; break;
                case 0xf4: EA.LowWord = S.LowWord; EA.d = RM16(EA.LowWord); pendingCycles -= 3; break;
                case 0xf5: EA.LowWord = (ushort)(S.LowWord + SIGNED(D.LowByte)); EA.d = RM16(EA.LowWord); pendingCycles -= 4; break;
                case 0xf6: EA.LowWord = (ushort)(S.LowWord + SIGNED(D.HighByte)); EA.d = RM16(EA.LowWord); pendingCycles -= 4; break;
                case 0xf7: EA.LowWord = 0; break; /*ILLEGAL*/
                case 0xf8: EA.LowWord = IMMBYTE(); EA.LowWord = (ushort)(S.LowWord + SIGNED(EA.LowByte)); EA.d = RM16(EA.LowWord); pendingCycles -= 4; break;
                case 0xf9: EA = IMMWORD(); EA.LowWord += S.LowWord; EA.d = RM16(EA.LowWord); pendingCycles -= 7; break;
                case 0xfa: EA.LowWord = 0; break; /*ILLEGAL*/
                case 0xfb: EA.LowWord = (ushort)(S.LowWord + D.LowWord); EA.d = RM16(EA.LowWord); pendingCycles -= 7; break;
                case 0xfc: EA.LowWord = IMMBYTE(); EA.LowWord = (ushort)(PC.LowWord + SIGNED(EA.LowByte)); EA.d = RM16(EA.LowWord); pendingCycles -= 4; break;
                case 0xfd: EA = IMMWORD(); EA.LowWord += PC.LowWord; EA.d = RM16(EA.LowWord); pendingCycles -= 8; break;
                case 0xfe: EA.LowWord = 0; break; /*ILLEGAL*/
                case 0xff: EA = IMMWORD(); EA.d = RM16(EA.LowWord); pendingCycles -= 8; break;
            }
        }
        public void SaveStateBinary(BinaryWriter writer)
        {
            writer.Write(PC.LowWord);
            writer.Write(PPC.LowWord);
            writer.Write(D.LowWord);
            writer.Write(DP.LowWord);
            writer.Write(U.LowWord);
            writer.Write(S.LowWord);
            writer.Write(X.LowWord);
            writer.Write(Y.LowWord);
            writer.Write(CC);
            writer.Write((byte)irq_state[0]);
            writer.Write((byte)irq_state[1]);
            writer.Write(int_state);
            writer.Write((byte)nmi_state);
            writer.Write(TotalExecutedCycles);
            writer.Write(PendingCycles);
        }
        public void LoadStateBinary(BinaryReader reader)
        {
            PC.LowWord = reader.ReadUInt16();
            PPC.LowWord = reader.ReadUInt16();
            D.LowWord = reader.ReadUInt16();
            DP.LowWord = reader.ReadUInt16();
            U.LowWord = reader.ReadUInt16();
            S.LowWord = reader.ReadUInt16();
            X.LowWord = reader.ReadUInt16();
            Y.LowWord = reader.ReadUInt16();
            CC = reader.ReadByte();
            irq_state[0] = (LineState)reader.ReadByte();
            irq_state[1] = (LineState)reader.ReadByte();
            int_state = reader.ReadByte();
            nmi_state = (LineState)reader.ReadByte();
            TotalExecutedCycles = reader.ReadUInt64();
            PendingCycles = reader.ReadInt32();
        }
    }
}
