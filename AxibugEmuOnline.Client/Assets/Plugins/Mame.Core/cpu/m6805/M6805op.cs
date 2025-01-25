using MAME.Core;

namespace cpu.m6805
{
    public partial class M6805
    {
        protected void illegal()
        {

        }


        /* $00/$02/$04/$06/$08/$0A/$0C/$0E BRSET direct,relative ---- */
        protected void brset(byte bit)
        {
            byte t = 0, r = 0;
            DIRBYTE(ref r);
            IMMBYTE(ref t);
            CLC();
            if ((r & bit) != 0)
            {
                SEC();
                pc.LowWord += (ushort)SIGNED(t);
                if (t == 0xfd)
                {
                    /* speed up busy loops */
                    if (pendingCycles > 0)
                        pendingCycles = 0;
                }
            }
        }

        /* $01/$03/$05/$07/$09/$0B/$0D/$0F BRCLR direct,relative ---- */
        protected void brclr(byte bit)
        {
            byte t = 0, r = 0;
            DIRBYTE(ref r);
            IMMBYTE(ref t);
            SEC();
            if ((r & bit) == 0)
            {
                CLC();
                pc.LowWord += (ushort)SIGNED(t);
                if (t == 0xfd)
                {
                    /* speed up busy loops */
                    if (pendingCycles > 0)
                        pendingCycles = 0;
                }
            }
        }

        /* $10/$12/$14/$16/$18/$1A/$1C/$1E BSET direct ---- */
        protected void bset(byte bit)
        {
            byte t = 0, r;
            DIRBYTE(ref t);
            r = (byte)(t | bit);
            WriteMemory((ushort)ea.d, r);
        }

        /* $11/$13/$15/$17/$19/$1B/$1D/$1F BCLR direct ---- */
        protected void bclr(byte bit)
        {
            byte t = 0, r;
            DIRBYTE(ref t);
            r = (byte)(t & (~bit));
            WriteMemory((ushort)ea.d, r);
        }

        /* $20 BRA relative ---- */
        protected void bra()
        {
            byte t = 0;
            IMMBYTE(ref t);
            pc.LowWord += (ushort)SIGNED(t);
            if (t == 0xfe)
            {
                /* speed up busy loops */
                if (pendingCycles > 0)
                    pendingCycles = 0;
            }
        }

        /* $21 BRN relative ---- */
        protected void brn()
        {
            byte t = 0;
            IMMBYTE(ref t);
        }

        /* $22 BHI relative ---- */
        protected void bhi()
        {
            BRANCH((cc & (CFLAG | ZFLAG)) == 0);
        }

        /* $23 BLS relative ---- */
        protected void bls()
        {
            BRANCH((cc & (CFLAG | ZFLAG)) != 0);
        }

        /* $24 BCC relative ---- */
        protected void bcc()
        {
            BRANCH((cc & CFLAG) == 0);
        }

        /* $25 BCS relative ---- */
        protected void bcs()
        {
            BRANCH((cc & CFLAG) != 0);
        }

        /* $26 BNE relative ---- */
        protected void bne()
        {
            BRANCH((cc & ZFLAG) == 0);
        }

        /* $27 BEQ relative ---- */
        protected void beq()
        {
            BRANCH((cc & ZFLAG) != 0);
        }

        /* $28 BHCC relative ---- */
        protected void bhcc()
        {
            BRANCH((cc & HFLAG) == 0);
        }

        /* $29 BHCS relative ---- */
        protected void bhcs()
        {
            BRANCH((cc & HFLAG) != 0);
        }

        /* $2a BPL relative ---- */
        protected void bpl()
        {
            BRANCH((cc & NFLAG) == 0);
        }

        /* $2b BMI relative ---- */
        protected void bmi()
        {
            BRANCH((cc & NFLAG) != 0);
        }

        /* $2c BMC relative ---- */
        protected void bmc()
        {
            BRANCH((cc & IFLAG) == 0);
        }

        /* $2d BMS relative ---- */
        protected void bms()
        {
            BRANCH((cc & IFLAG) != 0);
        }

        /* $2e BIL relative ---- */
        protected void bil()
        {
            if (subtype == SUBTYPE_HD63705)
            {
                BRANCH(nmi_state != (int)LineState.CLEAR_LINE);
            }
            else
            {
                BRANCH(irq_state[0] != (int)LineState.CLEAR_LINE);
            }
        }

        /* $2f BIH relative ---- */
        protected void bih()
        {
            if (subtype == SUBTYPE_HD63705)
            {
                BRANCH(nmi_state == (int)LineState.CLEAR_LINE);
            }
            else
            {
                BRANCH(irq_state[0] == (int)LineState.CLEAR_LINE);
            }
        }

        /* $30 NEG direct -*** */
        protected void neg_di()
        {
            byte t = 0;
            ushort r;
            DIRBYTE(ref t); r = (ushort)-t;
            CLR_NZC(); SET_FLAGS8(0, t, r);
            WriteMemory((ushort)ea.d, (byte)r);
        }

        /* $31 ILLEGAL */

        /* $32 ILLEGAL */

        /* $33 COM direct -**1 */
        protected void com_di()
        {
            byte t = 0;
            DIRBYTE(ref t); t = (byte)~t;
            CLR_NZ(); SET_NZ8(t); SEC();
            WriteMemory((ushort)ea.d, t);
        }

        /* $34 LSR direct -0** */
        protected void lsr_di()
        {
            byte t = 0;
            DIRBYTE(ref t);
            CLR_NZC();
            cc |= (byte)(t & 0x01);
            t >>= 1;
            SET_Z8(t);
            WriteMemory((ushort)ea.d, t);
        }

        /* $35 ILLEGAL */

        /* $36 ROR direct -*** */
        protected void ror_di()
        {
            byte t = 0, r;
            DIRBYTE(ref t);
            r = (byte)((cc & 0x01) << 7);
            CLR_NZC();
            cc |= (byte)(t & 0x01);
            r |= (byte)(t >> 1);
            SET_NZ8(r);
            WriteMemory((ushort)ea.d, r);
        }

        /* $37 ASR direct ?*** */
        protected void asr_di()
        {
            byte t = 0;
            DIRBYTE(ref t);
            CLR_NZC(); cc |= (byte)(t & 0x01);
            t >>= 1; t |= (byte)((t & 0x40) << 1);
            SET_NZ8(t);
            WriteMemory((ushort)ea.d, t);
        }

        /* $38 LSL direct ?*** */
        protected void lsl_di()
        {
            byte t = 0;
            ushort r;
            DIRBYTE(ref t);
            r = (ushort)(t << 1);
            CLR_NZC();
            SET_FLAGS8(t, t, r);
            WriteMemory((ushort)ea.d, (byte)r);
        }

        /* $39 ROL direct -*** */
        protected void rol_di()
        {
            byte b = 0;
            ushort t = 0, r;
            DIRBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(cc & 0x01);
            r |= (ushort)(t << 1);
            CLR_NZC();
            SET_FLAGS8((byte)t, (byte)t, r);
            WriteMemory((ushort)ea.d, (byte)r);
        }

        /* $3a DEC direct -**- */
        protected void dec_di()
        {
            byte t = 0;
            DIRBYTE(ref t);
            --t;
            CLR_NZ(); SET_FLAGS8D(t);
            WriteMemory((ushort)ea.d, t);
        }

        /* $3b ILLEGAL */

        /* $3c INC direct -**- */
        protected void inc_di()
        {
            byte t = 0;
            DIRBYTE(ref t);
            ++t;
            CLR_NZ(); SET_FLAGS8I(t);
            WriteMemory((ushort)ea.d, t);
        }

        /* $3d TST direct -**- */
        protected void tst_di()
        {
            byte t = 0;
            DIRBYTE(ref t);
            CLR_NZ(); SET_NZ8(t);
        }

        /* $3e ILLEGAL */

        /* $3f CLR direct -0100 */
        protected void clr_di()
        {
            DIRECT();
            CLR_NZC(); SEZ();
            WriteMemory((ushort)ea.d, 0);
        }

        /* $40 NEGA inherent ?*** */
        protected void nega()
        {
            ushort r;
            r = (ushort)-a;
            CLR_NZC(); SET_FLAGS8(0, a, r);
            a = (byte)r;
        }

        /* $41 ILLEGAL */

        /* $42 ILLEGAL */

        /* $43 COMA inherent -**1 */
        protected void coma()
        {
            a = (byte)~a;
            CLR_NZ();
            SET_NZ8(a);
            SEC();
        }

        /* $44 LSRA inherent -0** */
        protected void lsra()
        {
            CLR_NZC();
            cc |= (byte)(a & 0x01);
            a >>= 1;
            SET_Z8(a);
        }

        /* $45 ILLEGAL */

        /* $46 RORA inherent -*** */
        protected void rora()
        {
            byte r;
            r = (byte)((cc & 0x01) << 7);
            CLR_NZC();
            cc |= (byte)(a & 0x01);
            r |= (byte)(a >> 1);
            SET_NZ8(r);
            a = r;
        }

        /* $47 ASRA inherent ?*** */
        protected void asra()
        {
            CLR_NZC();
            cc |= (byte)(a & 0x01);
            a = (byte)((a & 0x80) | (a >> 1));
            SET_NZ8(a);
        }

        /* $48 LSLA inherent ?*** */
        protected void lsla()
        {
            ushort r;
            r = (ushort)(a << 1);
            CLR_NZC();
            SET_FLAGS8(a, a, r);
            a = (byte)r;
        }

        /* $49 ROLA inherent -*** */
        protected void rola()
        {
            ushort t, r;
            t = a;
            r = (ushort)(cc & 0x01);
            r |= (ushort)(t << 1);
            CLR_NZC();
            SET_FLAGS8((byte)t, (byte)t, r);
            a = (byte)r;
        }

        /* $4a DECA inherent -**- */
        protected void deca()
        {
            --a;
            CLR_NZ();
            SET_FLAGS8D(a);
        }

        /* $4b ILLEGAL */

        /* $4c INCA inherent -**- */
        protected void inca()
        {
            ++a;
            CLR_NZ();
            SET_FLAGS8I(a);
        }

        /* $4d TSTA inherent -**- */
        protected void tsta()
        {
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $4e ILLEGAL */

        /* $4f CLRA inherent -010 */
        protected void clra()
        {
            a = 0;
            CLR_NZ();
            SEZ();
        }

        /* $50 NEGX inherent ?*** */
        protected void negx()
        {
            ushort r;
            r = (ushort)-x;
            CLR_NZC();
            SET_FLAGS8(0, x, r);
            x = (byte)r;
        }

        /* $51 ILLEGAL */

        /* $52 ILLEGAL */

        /* $53 COMX inherent -**1 */
        protected void comx()
        {
            x = (byte)~x;
            CLR_NZ();
            SET_NZ8(x);
            SEC();
        }

        /* $54 LSRX inherent -0** */
        protected void lsrx()
        {
            CLR_NZC();
            cc |= (byte)(x & 0x01);
            x >>= 1;
            SET_Z8(x);
        }

        /* $55 ILLEGAL */

        /* $56 RORX inherent -*** */
        protected void rorx()
        {
            byte r;
            r = (byte)((cc & 0x01) << 7);
            CLR_NZC();
            cc |= (byte)(x & 0x01);
            r |= (byte)(x >> 1);
            SET_NZ8(r);
            x = r;
        }

        /* $57 ASRX inherent ?*** */
        protected void asrx()
        {
            CLR_NZC();
            cc |= (byte)(x & 0x01);
            x = (byte)((x & 0x80) | (x >> 1));
            SET_NZ8(x);
        }

        /* $58 ASLX inherent ?*** */
        protected void aslx()
        {
            ushort r;
            r = (ushort)(x << 1);
            CLR_NZC();
            SET_FLAGS8(x, x, r);
            x = (byte)r;
        }

        /* $59 ROLX inherent -*** */
        protected void rolx()
        {
            ushort t, r;
            t = x;
            r = (ushort)(cc & 0x01);
            r |= (ushort)(t << 1);
            CLR_NZC();
            SET_FLAGS8((byte)t, (byte)t, r);
            x = (byte)r;
        }

        /* $5a DECX inherent -**- */
        protected void decx()
        {
            --x;
            CLR_NZ();
            SET_FLAGS8D(x);
        }

        /* $5b ILLEGAL */

        /* $5c INCX inherent -**- */
        protected void incx()
        {
            ++x;
            CLR_NZ();
            SET_FLAGS8I(x);
        }

        /* $5d TSTX inherent -**- */
        protected void tstx()
        {
            CLR_NZ();
            SET_NZ8(x);
        }

        /* $5e ILLEGAL */

        /* $5f CLRX inherent -010 */
        protected void clrx()
        {
            x = 0;
            CLR_NZC();
            SEZ();
        }

        /* $60 NEG indexed, 1 byte offset -*** */
        protected void neg_ix1()
        {
            byte t = 0;
            ushort r;
            IDX1BYTE(ref t); r = (ushort)(-t);
            CLR_NZC(); SET_FLAGS8(0, t, r);
            WriteMemory((ushort)ea.d, (byte)r);
        }

        /* $61 ILLEGAL */

        /* $62 ILLEGAL */

        /* $63 COM indexed, 1 byte offset -**1 */
        protected void com_ix1()
        {
            byte t = 0;
            IDX1BYTE(ref t); t = (byte)~t;
            CLR_NZ(); SET_NZ8(t); SEC();
            WriteMemory((ushort)ea.d, t);
        }

        /* $64 LSR indexed, 1 byte offset -0** */
        protected void lsr_ix1()
        {
            byte t = 0;
            IDX1BYTE(ref t);
            CLR_NZC();
            cc |= (byte)(t & 0x01);
            t >>= 1;
            SET_Z8(t);
            WriteMemory((ushort)ea.d, t);
        }

        /* $65 ILLEGAL */

        /* $66 ROR indexed, 1 byte offset -*** */
        protected void ror_ix1()
        {
            byte t = 0, r;
            IDX1BYTE(ref t);
            r = (byte)((cc & 0x01) << 7);
            CLR_NZC();
            cc |= (byte)(t & 0x01);
            r |= (byte)(t >> 1);
            SET_NZ8(r);
            WriteMemory((ushort)ea.d, r);
        }

        /* $67 ASR indexed, 1 byte offset ?*** */
        protected void asr_ix1()
        {
            byte t = 0;
            IDX1BYTE(ref t);
            CLR_NZC(); cc |= (byte)(t & 0x01);
            t >>= 1; t |= (byte)((t & 0x40) << 1);
            SET_NZ8(t);
            WriteMemory((ushort)ea.d, t);
        }

        /* $68 LSL indexed, 1 byte offset ?*** */
        protected void lsl_ix1()
        {
            byte t = 0;
            ushort r;
            IDX1BYTE(ref t);
            r = (ushort)(t << 1);
            CLR_NZC();
            SET_FLAGS8(t, t, r);
            WriteMemory((ushort)ea.d, (byte)r);
        }

        /* $69 ROL indexed, 1 byte offset -*** */
        protected void rol_ix1()
        {
            byte b = 0;
            ushort t, r;
            IDX1BYTE(ref b);
            t = (ushort)b;
            r = (ushort)(cc & 0x01);
            r |= (ushort)(t << 1);
            CLR_NZC();
            SET_FLAGS8((byte)t, (byte)t, r);
            WriteMemory((ushort)ea.d, (byte)r);
        }

        /* $6a DEC indexed, 1 byte offset -**- */
        protected void dec_ix1()
        {
            byte t = 0;
            IDX1BYTE(ref t);
            --t;
            CLR_NZ(); SET_FLAGS8D(t);
            WriteMemory((ushort)ea.d, t);
        }

        /* $6b ILLEGAL */

        /* $6c INC indexed, 1 byte offset -**- */
        protected void inc_ix1()
        {
            byte t = 0;
            IDX1BYTE(ref t);
            ++t;
            CLR_NZ(); SET_FLAGS8I(t);
            WriteMemory((ushort)ea.d, t);
        }

        /* $6d TST indexed, 1 byte offset -**- */
        protected void tst_ix1()
        {
            byte t = 0;
            IDX1BYTE(ref t);
            CLR_NZ(); SET_NZ8(t);
        }

        /* $6e ILLEGAL */

        /* $6f CLR indexed, 1 byte offset -0100 */
        protected void clr_ix1()
        {
            INDEXED1();
            CLR_NZC(); SEZ();
            WriteMemory((ushort)ea.d, 0);
        }

        /* $70 NEG indexed -*** */
        protected void neg_ix()
        {
            byte t = 0;
            ushort r;
            IDXBYTE(ref t); r = (ushort)-t;
            CLR_NZC(); SET_FLAGS8(0, t, r);
            WriteMemory((ushort)ea.d, (byte)r);
        }

        /* $71 ILLEGAL */

        /* $72 ILLEGAL */

        /* $73 COM indexed -**1 */
        protected void com_ix()
        {
            byte t = 0;
            IDXBYTE(ref t); t = (byte)~t;
            CLR_NZ(); SET_NZ8(t); SEC();
            WriteMemory((ushort)ea.d, t);
        }

        /* $74 LSR indexed -0** */
        protected void lsr_ix()
        {
            byte t = 0;
            IDXBYTE(ref t);
            CLR_NZC();
            cc |= (byte)(t & 0x01);
            t >>= 1;
            SET_Z8(t);
            WriteMemory((ushort)ea.d, t);
        }

        /* $75 ILLEGAL */

        /* $76 ROR indexed -*** */
        protected void ror_ix()
        {
            byte t = 0, r;
            IDXBYTE(ref t);
            r = (byte)((cc & 0x01) << 7);
            CLR_NZC();
            cc |= (byte)(t & 0x01);
            r |= (byte)(t >> 1);
            SET_NZ8(r);
            WriteMemory((ushort)ea.d, r);
        }

        /* $77 ASR indexed ?*** */
        protected void asr_ix()
        {
            byte t = 0;
            IDXBYTE(ref t);
            CLR_NZC();
            cc |= (byte)(t & 0x01);
            t = (byte)((t & 0x80) | (t >> 1));
            SET_NZ8(t);
            WriteMemory((ushort)ea.d, t);
        }

        /* $78 LSL indexed ?*** */
        protected void lsl_ix()
        {
            byte t = 0;
            ushort r;
            IDXBYTE(ref t); r = (ushort)(t << 1);
            CLR_NZC(); SET_FLAGS8(t, t, r);
            WriteMemory((ushort)ea.d, (byte)r);
        }

        /* $79 ROL indexed -*** */
        protected void rol_ix()
        {
            byte b = 0;
            ushort t = 0, r;
            IDXBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(cc & 0x01);
            r |= (ushort)(t << 1);
            CLR_NZC();
            SET_FLAGS8((byte)t, (byte)t, r);
            WriteMemory((ushort)ea.d, (byte)r);
        }

        /* $7a DEC indexed -**- */
        protected void dec_ix()
        {
            byte t = 0;
            IDXBYTE(ref t);
            --t;
            CLR_NZ(); SET_FLAGS8D(t);
            WriteMemory((ushort)ea.d, t);
        }

        /* $7b ILLEGAL */

        /* $7c INC indexed -**- */
        protected void inc_ix()
        {
            byte t = 0;
            IDXBYTE(ref t);
            ++t;
            CLR_NZ(); SET_FLAGS8I(t);
            WriteMemory((ushort)ea.d, t);
        }

        /* $7d TST indexed -**- */
        protected void tst_ix()
        {
            byte t = 0;
            IDXBYTE(ref t);
            CLR_NZ(); SET_NZ8(t);
        }

        /* $7e ILLEGAL */

        /* $7f CLR indexed -0100 */
        protected void clr_ix()
        {
            INDEXED();
            CLR_NZC(); SEZ();
            WriteMemory((ushort)ea.d, 0);
        }

        /* $80 RTI inherent #### */
        protected void rti()
        {
            PULLBYTE(ref cc);
            PULLBYTE(ref a);
            PULLBYTE(ref x);
            PULLWORD(ref pc);
            //change_pc(PC);
        }

        /* $81 RTS inherent ---- */
        protected void rts()
        {
            PULLWORD(ref pc);
            //change_pc(PC);
        }

        /* $82 ILLEGAL */

        /* $83 SWI absolute indirect ---- */
        protected void swi()
        {
            PUSHWORD(ref pc);
            PUSHBYTE(ref x);
            PUSHBYTE(ref a);
            PUSHBYTE(ref cc);
            SEI();
            if (subtype == SUBTYPE_HD63705)
            {
                RM16(0x1ffa, ref pc);
            }
            else
            {
                RM16(0xfffc, ref pc);
            }
            //change_pc(PC);
        }

        /* $84 ILLEGAL */

        /* $85 ILLEGAL */

        /* $86 ILLEGAL */

        /* $87 ILLEGAL */

        /* $88 ILLEGAL */

        /* $89 ILLEGAL */

        /* $8A ILLEGAL */

        /* $8B ILLEGAL */

        /* $8C ILLEGAL */

        /* $8D ILLEGAL */

        /* $8E ILLEGAL */

        /* $8F ILLEGAL */

        /* $90 ILLEGAL */

        /* $91 ILLEGAL */

        /* $92 ILLEGAL */

        /* $93 ILLEGAL */

        /* $94 ILLEGAL */

        /* $95 ILLEGAL */

        /* $96 ILLEGAL */

        /* $97 TAX inherent ---- */
        protected void tax()
        {
            x = a;
        }

        /* $98 CLC */

        /* $99 SEC */

        /* $9A CLI */

        /* $9B SEI */

        /* $9C RSP inherent ---- */
        protected void rsp()
        {
            s.LowWord = (byte)sp_mask;
        }

        /* $9D NOP inherent ---- */
        protected void nop()
        {
        }

        /* $9E ILLEGAL */

        /* $9F TXA inherent ---- */
        protected void txa()
        {
            a = x;
        }

        /* $a0 SUBA immediate ?*** */
        protected void suba_im()
        {
            byte b = 0;
            ushort t, r;
            IMMBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a - t);
            CLR_NZC();
            SET_FLAGS8(a, (byte)t, r);
            a = (byte)r;
        }

        /* $a1 CMPA immediate ?*** */
        protected void cmpa_im()
        {
            byte b = 0;
            ushort t, r;
            IMMBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a - t);
            CLR_NZC();
            SET_FLAGS8(a, (byte)t, r);
        }

        /* $a2 SBCA immediate ?*** */
        protected void sbca_im()
        {
            byte b = 0;
            ushort t, r;
            IMMBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a - t - (cc & 0x01));
            CLR_NZC();
            SET_FLAGS8(a, (byte)t, r);
            a = (byte)r;
        }

        /* $a3 CPX immediate -*** */
        protected void cpx_im()
        {
            byte b = 0;
            ushort t, r;
            IMMBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(x - t);
            CLR_NZC();
            SET_FLAGS8(x, (byte)t, r);
        }

        /* $a4 ANDA immediate -**- */
        protected void anda_im()
        {
            byte t = 0;
            IMMBYTE(ref t);
            a &= t;
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $a5 BITA immediate -**- */
        protected void bita_im()
        {
            byte t = 0, r;
            IMMBYTE(ref t);
            r = (byte)(a & t);
            CLR_NZ();
            SET_NZ8(r);
        }

        /* $a6 LDA immediate -**- */
        protected void lda_im()
        {
            IMMBYTE(ref a);
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $a7 ILLEGAL */

        /* $a8 EORA immediate -**- */
        protected void eora_im()
        {
            byte t = 0;
            IMMBYTE(ref t);
            a ^= t;
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $a9 ADCA immediate **** */
        protected void adca_im()
        {
            byte b = 0;
            ushort t, r;
            IMMBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a + t + (cc & 0x01));
            CLR_HNZC();
            SET_FLAGS8(a, (byte)t, r);
            SET_H(a, (byte)t, (byte)r);
            a = (byte)r;
        }

        /* $aa ORA immediate -**- */
        protected void ora_im()
        {
            byte t = 0;
            IMMBYTE(ref t);
            a |= t;
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $ab ADDA immediate **** */
        protected void adda_im()
        {
            byte b = 0;
            ushort t, r;
            IMMBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a + t);
            CLR_HNZC();
            SET_FLAGS8(a, (byte)t, r);
            SET_H(a, (byte)t, (byte)r);
            a = (byte)r;
        }

        /* $ac ILLEGAL */

        /* $ad BSR ---- */
        protected void bsr()
        {
            byte t = 0;
            IMMBYTE(ref t);
            PUSHWORD(ref pc);
            pc.LowWord += (ushort)SIGNED(t);
        }

        /* $ae LDX immediate -**- */
        protected void ldx_im()
        {
            IMMBYTE(ref x);
            CLR_NZ();
            SET_NZ8(x);
        }

        /* $af ILLEGAL */

        /* $b0 SUBA direct ?*** */
        protected void suba_di()
        {
            byte b = 0;
            ushort t, r;
            DIRBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a - t);
            CLR_NZC();
            SET_FLAGS8(a, (byte)t, r);
            a = (byte)r;
        }

        /* $b1 CMPA direct ?*** */
        protected void cmpa_di()
        {
            byte b = 0;
            ushort t, r;
            DIRBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a - t);
            CLR_NZC();
            SET_FLAGS8(a, (byte)t, r);
        }

        /* $b2 SBCA direct ?*** */
        protected void sbca_di()
        {
            byte b = 0;
            ushort t, r;
            DIRBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a - t - (cc & 0x01));
            CLR_NZC();
            SET_FLAGS8(a, (byte)t, r);
            a = (byte)r;
        }

        /* $b3 CPX direct -*** */
        protected void cpx_di()
        {
            byte b = 0;
            ushort t, r;
            DIRBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(x - t);
            CLR_NZC();
            SET_FLAGS8(x, (byte)t, r);
        }

        /* $b4 ANDA direct -**- */
        protected void anda_di()
        {
            byte t = 0;
            DIRBYTE(ref t);
            a &= t;
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $b5 BITA direct -**- */
        protected void bita_di()
        {
            byte t = 0, r;
            DIRBYTE(ref t);
            r = (byte)(a & t);
            CLR_NZ();
            SET_NZ8(r);
        }

        /* $b6 LDA direct -**- */
        protected void lda_di()
        {
            DIRBYTE(ref a);
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $b7 STA direct -**- */
        protected void sta_di()
        {
            CLR_NZ();
            SET_NZ8(a);
            DIRECT();
            WriteMemory((ushort)ea.d, a);
        }

        /* $b8 EORA direct -**- */
        protected void eora_di()
        {
            byte t = 0;
            DIRBYTE(ref t);
            a ^= t;
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $b9 ADCA direct **** */
        protected void adca_di()
        {
            byte b = 0;
            ushort t, r;
            DIRBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a + t + (cc & 0x01));
            CLR_HNZC();
            SET_FLAGS8(a, (byte)t, r);
            SET_H(a, (byte)t, (byte)r);
            a = (byte)r;
        }

        /* $ba ORA direct -**- */
        protected void ora_di()
        {
            byte t = 0;
            DIRBYTE(ref t);
            a |= t;
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $bb ADDA direct **** */
        protected void adda_di()
        {
            byte b = 0;
            ushort t, r;
            DIRBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a + t);
            CLR_HNZC();
            SET_FLAGS8(a, (byte)t, r);
            SET_H(a, (byte)t, (byte)r);
            a = (byte)r;
        }

        /* $bc JMP direct -*** */
        protected void jmp_di()
        {
            DIRECT();
            pc.LowWord = ea.LowWord;
            //change_pc(PC);
        }

        /* $bd JSR direct ---- */
        protected void jsr_di()
        {
            DIRECT();
            PUSHWORD(ref pc);
            pc.LowWord = ea.LowWord;
            //change_pc(PC);
        }

        /* $be LDX direct -**- */
        protected void ldx_di()
        {
            DIRBYTE(ref x);
            CLR_NZ();
            SET_NZ8(x);
        }

        /* $bf STX direct -**- */
        protected void stx_di()
        {
            CLR_NZ();
            SET_NZ8(x);
            DIRECT();
            WriteMemory((ushort)ea.d, x);
        }

        /* $c0 SUBA extended ?*** */
        protected void suba_ex()
        {
            byte b = 0;
            ushort t, r;
            EXTBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a - t);
            CLR_NZC();
            SET_FLAGS8(a, (byte)t, r);
            a = (byte)r;
        }

        /* $c1 CMPA extended ?*** */
        protected void cmpa_ex()
        {
            byte b = 0;
            ushort t, r;
            EXTBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a - t);
            CLR_NZC();
            SET_FLAGS8(a, (byte)t, r);
        }

        /* $c2 SBCA extended ?*** */
        protected void sbca_ex()
        {
            byte b = 0;
            ushort t, r;
            EXTBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a - t - (cc & 0x01));
            CLR_NZC();
            SET_FLAGS8(a, (byte)t, r);
            a = (byte)r;
        }

        /* $c3 CPX extended -*** */
        protected void cpx_ex()
        {
            byte b = 0;
            ushort t, r;
            EXTBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(x - t);
            CLR_NZC();
            SET_FLAGS8(x, (byte)t, r);
        }

        /* $c4 ANDA extended -**- */
        protected void anda_ex()
        {
            byte t = 0;
            EXTBYTE(ref t);
            a &= t;
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $c5 BITA extended -**- */
        protected void bita_ex()
        {
            byte t = 0, r;
            EXTBYTE(ref t);
            r = (byte)(a & t);
            CLR_NZ();
            SET_NZ8(r);
        }

        /* $c6 LDA extended -**- */
        protected void lda_ex()
        {
            EXTBYTE(ref a);
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $c7 STA extended -**- */
        protected void sta_ex()
        {
            CLR_NZ();
            SET_NZ8(a);
            EXTENDED();
            WriteMemory((ushort)ea.d, a);
        }

        /* $c8 EORA extended -**- */
        protected void eora_ex()
        {
            byte t = 0;
            EXTBYTE(ref t);
            a ^= t;
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $c9 ADCA extended **** */
        protected void adca_ex()
        {
            byte b = 0;
            ushort t, r;
            EXTBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a + t + (cc & 0x01));
            CLR_HNZC();
            SET_FLAGS8(a, (byte)t, r);
            SET_H(a, (byte)t, (byte)r);
            a = (byte)r;
        }

        /* $ca ORA extended -**- */
        protected void ora_ex()
        {
            byte t = 0;
            EXTBYTE(ref t);
            a |= t;
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $cb ADDA extended **** */
        protected void adda_ex()
        {
            byte b = 0;
            ushort t, r;
            EXTBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a + t);
            CLR_HNZC();
            SET_FLAGS8(a, (byte)t, r);
            SET_H(a, (byte)t, (byte)r);
            a = (byte)r;
        }

        /* $cc JMP extended -*** */
        protected void jmp_ex()
        {
            EXTENDED();
            pc.LowWord = ea.LowWord;
            //change_pc(PC);
        }

        /* $cd JSR extended ---- */
        protected void jsr_ex()
        {
            EXTENDED();
            PUSHWORD(ref pc);
            pc.LowWord = ea.LowWord;
            //change_pc(PC);
        }

        /* $ce LDX extended -**- */
        protected void ldx_ex()
        {
            EXTBYTE(ref x);
            CLR_NZ();
            SET_NZ8(x);
        }

        /* $cf STX extended -**- */
        protected void stx_ex()
        {
            CLR_NZ();
            SET_NZ8(x);
            EXTENDED();
            WriteMemory((ushort)ea.d, x);
        }

        /* $d0 SUBA indexed, 2 byte offset ?*** */
        protected void suba_ix2()
        {
            byte b = 0;
            ushort t, r;
            IDX2BYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a - t);
            CLR_NZC();
            SET_FLAGS8(a, (byte)t, r);
            a = (byte)r;
        }

        /* $d1 CMPA indexed, 2 byte offset ?*** */
        protected void cmpa_ix2()
        {
            byte b = 0;
            ushort t, r;
            IDX2BYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a - t);
            CLR_NZC();
            SET_FLAGS8(a, (byte)t, r);
        }

        /* $d2 SBCA indexed, 2 byte offset ?*** */
        protected void sbca_ix2()
        {
            byte b = 0;
            ushort t, r;
            IDX2BYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a - t - (cc & 0x01));
            CLR_NZC();
            SET_FLAGS8(a, (byte)t, r);
            a = (byte)r;
        }

        /* $d3 CPX indexed, 2 byte offset -*** */
        protected void cpx_ix2()
        {
            byte b = 0;
            ushort t, r;
            IDX2BYTE(ref b);
            t = (ushort)b;
            r = (ushort)(x - t);
            CLR_NZC();
            SET_FLAGS8(x, (byte)t, r);
        }

        /* $d4 ANDA indexed, 2 byte offset -**- */
        protected void anda_ix2()
        {
            byte t = 0;
            IDX2BYTE(ref t);
            a &= t;
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $d5 BITA indexed, 2 byte offset -**- */
        protected void bita_ix2()
        {
            byte t = 0, r;
            IDX2BYTE(ref t);
            r = (byte)(a & t);
            CLR_NZ();
            SET_NZ8(r);
        }

        /* $d6 LDA indexed, 2 byte offset -**- */
        protected void lda_ix2()
        {
            IDX2BYTE(ref a);
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $d7 STA indexed, 2 byte offset -**- */
        protected void sta_ix2()
        {
            CLR_NZ();
            SET_NZ8(a);
            INDEXED2();
            WriteMemory((ushort)ea.d, a);
        }

        /* $d8 EORA indexed, 2 byte offset -**- */
        protected void eora_ix2()
        {
            byte t = 0;
            IDX2BYTE(ref t);
            a ^= t;
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $d9 ADCA indexed, 2 byte offset **** */
        protected void adca_ix2()
        {
            byte b = 0;
            ushort t, r;
            IDX2BYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a + t + (cc & 0x01));
            CLR_HNZC();
            SET_FLAGS8(a, (byte)t, r);
            SET_H(a, (byte)t, (byte)r);
            a = (byte)r;
        }

        /* $da ORA indexed, 2 byte offset -**- */
        protected void ora_ix2()
        {
            byte t = 0;
            IDX2BYTE(ref t);
            a |= t;
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $db ADDA indexed, 2 byte offset **** */
        protected void adda_ix2()
        {
            byte b = 0;
            ushort t, r;
            IDX2BYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a + t);
            CLR_HNZC();
            SET_FLAGS8(a, (byte)t, r);
            SET_H(a, (byte)t, (byte)r);
            a = (byte)r;
        }

        /* $dc JMP indexed, 2 byte offset -*** */
        protected void jmp_ix2()
        {
            INDEXED2();
            pc.LowWord = ea.LowWord;
            //change_pc(PC);
        }

        /* $dd JSR indexed, 2 byte offset ---- */
        protected void jsr_ix2()
        {
            INDEXED2();
            PUSHWORD(ref pc);
            pc.LowWord = ea.LowWord;
            //change_pc(PC);
        }

        /* $de LDX indexed, 2 byte offset -**- */
        protected void ldx_ix2()
        {
            IDX2BYTE(ref x);
            CLR_NZ();
            SET_NZ8(x);
        }

        /* $df STX indexed, 2 byte offset -**- */
        protected void stx_ix2()
        {
            CLR_NZ();
            SET_NZ8(x);
            INDEXED2();
            WriteMemory((ushort)ea.d, x);
        }

        /* $e0 SUBA indexed, 1 byte offset ?*** */
        protected void suba_ix1()
        {
            byte b = 0;
            ushort t, r;
            IDX1BYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a - t);
            CLR_NZC();
            SET_FLAGS8(a, (byte)t, r);
            a = (byte)r;
        }

        /* $e1 CMPA indexed, 1 byte offset ?*** */
        protected void cmpa_ix1()
        {
            byte b = 0;
            ushort t, r;
            IDX1BYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a - t);
            CLR_NZC();
            SET_FLAGS8(a, (byte)t, r);
        }

        /* $e2 SBCA indexed, 1 byte offset ?*** */
        protected void sbca_ix1()
        {
            byte b = 0;
            ushort t, r;
            IDX1BYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a - t - (cc & 0x01));
            CLR_NZC();
            SET_FLAGS8(a, (byte)t, r);
            a = (byte)r;
        }

        /* $e3 CPX indexed, 1 byte offset -*** */
        protected void cpx_ix1()
        {
            byte b = 0;
            ushort t, r;
            IDX1BYTE(ref b);
            t = (ushort)b;
            r = (ushort)(x - t);
            CLR_NZC();
            SET_FLAGS8(x, (byte)t, r);
        }

        /* $e4 ANDA indexed, 1 byte offset -**- */
        protected void anda_ix1()
        {
            byte t = 0;
            IDX1BYTE(ref t);
            a &= t;
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $e5 BITA indexed, 1 byte offset -**- */
        protected void bita_ix1()
        {
            byte t = 0, r;
            IDX1BYTE(ref t);
            r = (byte)(a & t);
            CLR_NZ();
            SET_NZ8(r);
        }

        /* $e6 LDA indexed, 1 byte offset -**- */
        protected void lda_ix1()
        {
            IDX1BYTE(ref a);
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $e7 STA indexed, 1 byte offset -**- */
        protected void sta_ix1()
        {
            CLR_NZ();
            SET_NZ8(a);
            INDEXED1();
            WriteMemory((ushort)ea.d, a);
        }

        /* $e8 EORA indexed, 1 byte offset -**- */
        protected void eora_ix1()
        {
            byte t = 0;
            IDX1BYTE(ref t);
            a ^= t;
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $e9 ADCA indexed, 1 byte offset **** */
        protected void adca_ix1()
        {
            byte b = 0;
            ushort t, r;
            IDX1BYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a + t + (cc & 0x01));
            CLR_HNZC();
            SET_FLAGS8(a, (byte)t, r);
            SET_H(a, (byte)t, (byte)r);
            a = (byte)r;
        }

        /* $ea ORA indexed, 1 byte offset -**- */
        protected void ora_ix1()
        {
            byte t = 0;
            IDX1BYTE(ref t);
            a |= t;
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $eb ADDA indexed, 1 byte offset **** */
        protected void adda_ix1()
        {
            byte b = 0;
            ushort t, r;
            IDX1BYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a + t);
            CLR_HNZC();
            SET_FLAGS8(a, (byte)t, r);
            SET_H(a, (byte)t, (byte)r);
            a = (byte)r;
        }

        /* $ec JMP indexed, 1 byte offset -*** */
        protected void jmp_ix1()
        {
            INDEXED1();
            pc.LowWord = ea.LowWord;
            //change_pc(PC);
        }

        /* $ed JSR indexed, 1 byte offset ---- */
        protected void jsr_ix1()
        {
            INDEXED1();
            PUSHWORD(ref pc);
            pc.LowWord = ea.LowWord;
            //change_pc(PC);
        }

        /* $ee LDX indexed, 1 byte offset -**- */
        protected void ldx_ix1()
        {
            IDX1BYTE(ref x);
            CLR_NZ();
            SET_NZ8(x);
        }

        /* $ef STX indexed, 1 byte offset -**- */
        protected void stx_ix1()
        {
            CLR_NZ();
            SET_NZ8(x);
            INDEXED1();
            WriteMemory((ushort)ea.d, x);
        }

        /* $f0 SUBA indexed ?*** */
        protected void suba_ix()
        {
            byte b = 0;
            ushort t, r;
            IDXBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a - t);
            CLR_NZC();
            SET_FLAGS8(a, (byte)t, r);
            a = (byte)r;
        }

        /* $f1 CMPA indexed ?*** */
        protected void cmpa_ix()
        {
            byte b = 0;
            ushort t, r;
            IDXBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a - t);
            CLR_NZC();
            SET_FLAGS8(a, (byte)t, r);
        }

        /* $f2 SBCA indexed ?*** */
        protected void sbca_ix()
        {
            byte b = 0;
            ushort t, r;
            IDXBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a - t - (cc & 0x01));
            CLR_NZC();
            SET_FLAGS8(a, (byte)t, r);
            a = (byte)r;
        }

        /* $f3 CPX indexed -*** */
        protected void cpx_ix()
        {
            byte b = 0;
            ushort t, r;
            IDXBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(x - t);
            CLR_NZC();
            SET_FLAGS8(x, (byte)t, r);
        }

        /* $f4 ANDA indexed -**- */
        protected void anda_ix()
        {
            byte t = 0;
            IDXBYTE(ref t);
            a &= t;
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $f5 BITA indexed -**- */
        protected void bita_ix()
        {
            byte t = 0, r;
            IDXBYTE(ref t);
            r = (byte)(a & t);
            CLR_NZ();
            SET_NZ8(r);
        }

        /* $f6 LDA indexed -**- */
        protected void lda_ix()
        {
            IDXBYTE(ref a);
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $f7 STA indexed -**- */
        protected void sta_ix()
        {
            CLR_NZ();
            SET_NZ8(a);
            INDEXED();
            WriteMemory((ushort)ea.d, a);
        }

        /* $f8 EORA indexed -**- */
        protected void eora_ix()
        {
            byte t = 0;
            IDXBYTE(ref t);
            a ^= t;
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $f9 ADCA indexed **** */
        protected void adca_ix()
        {
            byte b = 0;
            ushort t, r;
            IDXBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a + t + (cc & 0x01));
            CLR_HNZC();
            SET_FLAGS8(a, (byte)t, r);
            SET_H(a, (byte)t, (byte)r);
            a = (byte)r;
        }

        /* $fa ORA indexed -**- */
        protected void ora_ix()
        {
            byte t = 0;
            IDXBYTE(ref t);
            a |= t;
            CLR_NZ();
            SET_NZ8(a);
        }

        /* $fb ADDA indexed **** */
        protected void adda_ix()
        {
            byte b = 0;
            ushort t, r;
            IDXBYTE(ref b);
            t = (ushort)b;
            r = (ushort)(a + t);
            CLR_HNZC();
            SET_FLAGS8(a, (byte)t, r);
            SET_H(a, (byte)t, (byte)r);
            a = (byte)r;
        }

        /* $fc JMP indexed -*** */
        protected void jmp_ix()
        {
            INDEXED();
            pc.LowWord = ea.LowWord;
            //change_pc(PC);
        }

        /* $fd JSR indexed ---- */
        protected void jsr_ix()
        {
            INDEXED();
            PUSHWORD(ref pc);
            pc.LowWord = ea.LowWord;
            //change_pc(PC);
        }

        /* $fe LDX indexed -**- */
        protected void ldx_ix()
        {
            IDXBYTE(ref x);
            CLR_NZ();
            SET_NZ8(x);
        }

        /* $ff STX indexed -**- */
        protected void stx_ix()
        {
            CLR_NZ();
            SET_NZ8(x);
            INDEXED();
            WriteMemory((ushort)ea.d, x);
        }
    }
}