using MAME.Core;
using System;

namespace cpu.m6800
{
    public partial class M6800 : cpuexec_data
    {
        public static M6800 m1;
        public static Action action_rx, action_tx;
        public Action[] insn, m6800_insn, hd63701_insn, m6803_insn;
        public Register PPC, PC;
        public Register S, X, D, EA;
        public byte[] cycles;
        public byte cc, wai_state, ic_eddge;
        public byte[] irq_state = new byte[2];
        public byte nmi_state;
        public int extra_cycles;
        public delegate int irq_delegate(int i);
        public irq_delegate irq_callback;
        public byte port1_ddr, port2_ddr, port3_ddr, port4_ddr, port1_data, port2_data, port3_data, port4_data;
        public byte tcsr, pending_tcsr, irq2, ram_ctrl;
        public Register counter, output_compare, timer_over;
        public ushort input_capture;
        public int clock;
        public byte trcsr, rmcr, rdr, tdr, rsr, tsr;
        public int rxbits, txbits, trcsr_read, tx;
        public M6800_TX_STATE txstate;
        public EmuTimer.emu_timer m6800_rx_timer, m6800_tx_timer;
        private byte TCSR_OLVL = 0x01, TCSR_IEDG = 0x02, TCSR_ETOI = 0x04, TCSR_EOCI = 0x08, TCSR_EICI = 0x10, TCSR_TOF = 0x20, TCSR_OCF = 0x40, TCSR_ICF = 0x80;
        protected byte M6800_WAI = 8, M6800_SLP = 0x10;
        private const byte M6800_IRQ_LINE = 0, M6800_TIN_LINE = 1;
        private ushort M6803_DDR1 = 0x00, M6803_DDR2 = 0x01, M6803_DDR3 = 0x04, M6803_DDR4 = 0x05, M6803_PORT1 = 0x100, M6803_PORT2 = 0x101, M6803_PORT3 = 0x102, M6803_PORT4 = 0x103;
        private byte M6800_RMCR_SS_MASK = 0x03, M6800_RMCR_SS_4096 = 0x03, M6800_RMCR_SS_1024 = 0x02, M6800_RMCR_SS_128 = 0x01, M6800_RMCR_SS_16 = 0x00, M6800_RMCR_CC_MASK = 0x0c;
        private byte M6800_TRCSR_RDRF = 0x80, M6800_TRCSR_ORFE = 0x40, M6800_TRCSR_TDRE = 0x20, M6800_TRCSR_RIE = 0x10, M6800_TRCSR_RE = 0x08, M6800_TRCSR_TIE = 0x04, M6800_TRCSR_TE = 0x02, M6800_TRCSR_WU = 0x01, M6800_PORT2_IO4 = 0x10, M6800_PORT2_IO3 = 0x08;
        private static int[] M6800_RMCR_SS = new int[] { 16, 128, 1024, 4096 };
        public enum M6800_TX_STATE
        {
            INIT = 0,
            READY
        }
        private byte CLEAR_LINE = 0, INPUT_LINE_NMI = 32;
        protected ulong totalExecutedCycles;
        protected int pendingCycles;
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
        public uint timer_next;
        public readonly static byte[] flags8i = new byte[256]	 /* increment */
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
        private readonly static byte[] flags8d = new byte[256] /* decrement */
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
        private readonly static byte[] cycles_6800 = new byte[]
{
		/* 0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F */
	/*0*/ 99, 2,99,99,99,99, 2, 2, 4, 4, 2, 2, 2, 2, 2, 2,
	/*1*/  2, 2,99,99,99,99, 2, 2,99, 2,99, 2,99,99,99,99,
	/*2*/  4,99, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
	/*3*/  4, 4, 4, 4, 4, 4, 4, 4,99, 5,99,10,99,99, 9,12,
	/*4*/  2,99,99, 2, 2,99, 2, 2, 2, 2, 2,99, 2, 2,99, 2,
	/*5*/  2,99,99, 2, 2,99, 2, 2, 2, 2, 2,99, 2, 2,99, 2,
	/*6*/  7,99,99, 7, 7,99, 7, 7, 7, 7, 7,99, 7, 7, 4, 7,
	/*7*/  6,99,99, 6, 6,99, 6, 6, 6, 6, 6,99, 6, 6, 3, 6,
	/*8*/  2, 2, 2,99, 2, 2, 2,99, 2, 2, 2, 2, 3, 8, 3,99,
	/*9*/  3, 3, 3,99, 3, 3, 3, 4, 3, 3, 3, 3, 4,99, 4, 5,
	/*A*/  5, 5, 5,99, 5, 5, 5, 6, 5, 5, 5, 5, 6, 8, 6, 7,
	/*B*/  4, 4, 4,99, 4, 4, 4, 5, 4, 4, 4, 4, 5, 9, 5, 6,
	/*C*/  2, 2, 2,99, 2, 2, 2,99, 2, 2, 2, 2,99,99, 3,99,
	/*D*/  3, 3, 3,99, 3, 3, 3, 4, 3, 3, 3, 3,99,99, 4, 5,
	/*E*/  5, 5, 5,99, 5, 5, 5, 6, 5, 5, 5, 5,99,99, 6, 7,
	/*F*/  4, 4, 4,99, 4, 4, 4, 5, 4, 4, 4, 4,99,99, 5, 6
};
        protected byte[] cycles_6803 =
{
		/* 0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F */
	/*0*/ 99, 2,99,99, 3, 3, 2, 2, 3, 3, 2, 2, 2, 2, 2, 2,
	/*1*/  2, 2,99,99,99,99, 2, 2,99, 2,99, 2,99,99,99,99,
	/*2*/  3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
	/*3*/  3, 3, 4, 4, 3, 3, 3, 3, 5, 5, 3,10, 4,10, 9,12,
	/*4*/  2,99,99, 2, 2,99, 2, 2, 2, 2, 2,99, 2, 2,99, 2,
	/*5*/  2,99,99, 2, 2,99, 2, 2, 2, 2, 2,99, 2, 2,99, 2,
	/*6*/  6,99,99, 6, 6,99, 6, 6, 6, 6, 6,99, 6, 6, 3, 6,
	/*7*/  6,99,99, 6, 6,99, 6, 6, 6, 6, 6,99, 6, 6, 3, 6,
	/*8*/  2, 2, 2, 4, 2, 2, 2,99, 2, 2, 2, 2, 4, 6, 3,99,
	/*9*/  3, 3, 3, 5, 3, 3, 3, 3, 3, 3, 3, 3, 5, 5, 4, 4,
	/*A*/  4, 4, 4, 6, 4, 4, 4, 4, 4, 4, 4, 4, 6, 6, 5, 5,
	/*B*/  4, 4, 4, 6, 4, 4, 4, 4, 4, 4, 4, 4, 6, 6, 5, 5,
	/*C*/  2, 2, 2, 4, 2, 2, 2,99, 2, 2, 2, 2, 3,99, 3,99,
	/*D*/  3, 3, 3, 5, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4,
	/*E*/  4, 4, 4, 6, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5,
	/*F*/  4, 4, 4, 6, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5
};
        private byte[] cycles_63701 =
{
		/* 0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F */
	/*0*/ 99, 1,99,99, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
	/*1*/  1, 1,99,99,99,99, 1, 1, 2, 2, 4, 1,99,99,99,99,
	/*2*/  3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
	/*3*/  1, 1, 3, 3, 1, 1, 4, 4, 4, 5, 1,10, 5, 7, 9,12,
	/*4*/  1,99,99, 1, 1,99, 1, 1, 1, 1, 1,99, 1, 1,99, 1,
	/*5*/  1,99,99, 1, 1,99, 1, 1, 1, 1, 1,99, 1, 1,99, 1,
	/*6*/  6, 7, 7, 6, 6, 7, 6, 6, 6, 6, 6, 5, 6, 4, 3, 5,
	/*7*/  6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 4, 6, 4, 3, 5,
	/*8*/  2, 2, 2, 3, 2, 2, 2,99, 2, 2, 2, 2, 3, 5, 3,99,
	/*9*/  3, 3, 3, 4, 3, 3, 3, 3, 3, 3, 3, 3, 4, 5, 4, 4,
	/*A*/  4, 4, 4, 5, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5,
	/*B*/  4, 4, 4, 5, 4, 4, 4, 4, 4, 4, 4, 4, 5, 6, 5, 5,
	/*C*/  2, 2, 2, 3, 2, 2, 2,99, 2, 2, 2, 2, 3,99, 3,99,
	/*D*/  3, 3, 3, 4, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4,
	/*E*/  4, 4, 4, 5, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5,
	/*F*/  4, 4, 4, 5, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5
};

        public Func<ushort, byte> ReadOp, ReadOpArg;
        public Func<ushort, byte> ReadMemory;
        public Action<ushort, byte> WriteMemory;
        public Func<ushort, byte> ReadIO;
        public Action<ushort, byte> WriteIO;

        public M6800()
        {
            m6800_insn = new Action[256]
            {
                illegal,nop,    illegal,illegal,illegal,illegal,tap,    tpa,
                inx,    dex,    clv,    sev,    clc,    sec,    cli,    sei,
                sba,    cba,    illegal,illegal,illegal,illegal,tab,    tba,
                illegal,daa,    illegal,aba,    illegal,illegal,illegal,illegal,
                bra,    brn,    bhi,    bls,    bcc,    bcs,    bne,    beq,
                bvc,    bvs,    bpl,    bmi,    bge,    blt,    bgt,    ble,
                tsx,    ins,    pula,   pulb,   des,    txs,    psha,   pshb,
                illegal,rts,    illegal,rti,    illegal,illegal,wai,    swi,
                nega,   illegal,illegal,coma,   lsra,   illegal,rora,   asra,
                asla,   rola,   deca,   illegal,inca,   tsta,   illegal,clra,
                negb,   illegal,illegal,comb,   lsrb,   illegal,rorb,   asrb,
                aslb,   rolb,   decb,   illegal,incb,   tstb,   illegal,clrb,
                neg_ix, illegal,illegal,com_ix, lsr_ix, illegal,ror_ix, asr_ix,
                asl_ix, rol_ix, dec_ix, illegal,inc_ix, tst_ix, jmp_ix, clr_ix,
                neg_ex, illegal,illegal,com_ex, lsr_ex, illegal,ror_ex, asr_ex,
                asl_ex, rol_ex, dec_ex, illegal,inc_ex, tst_ex, jmp_ex, clr_ex,
                suba_im,cmpa_im,sbca_im,illegal,anda_im,bita_im,lda_im, sta_im,
                eora_im,adca_im,ora_im, adda_im,cmpx_im,bsr,    lds_im, sts_im,
                suba_di,cmpa_di,sbca_di,illegal,anda_di,bita_di,lda_di, sta_di,
                eora_di,adca_di,ora_di, adda_di,cmpx_di,jsr_di, lds_di, sts_di,
                suba_ix,cmpa_ix,sbca_ix,illegal,anda_ix,bita_ix,lda_ix, sta_ix,
                eora_ix,adca_ix,ora_ix, adda_ix,cmpx_ix,jsr_ix, lds_ix, sts_ix,
                suba_ex,cmpa_ex,sbca_ex,illegal,anda_ex,bita_ex,lda_ex, sta_ex,
                eora_ex,adca_ex,ora_ex, adda_ex,cmpx_ex,jsr_ex, lds_ex, sts_ex,
                subb_im,cmpb_im,sbcb_im,illegal,andb_im,bitb_im,ldb_im, stb_im,
                eorb_im,adcb_im,orb_im, addb_im,illegal,illegal,ldx_im, stx_im,
                subb_di,cmpb_di,sbcb_di,illegal,andb_di,bitb_di,ldb_di, stb_di,
                eorb_di,adcb_di,orb_di, addb_di,illegal,illegal,ldx_di, stx_di,
                subb_ix,cmpb_ix,sbcb_ix,illegal,andb_ix,bitb_ix,ldb_ix, stb_ix,
                eorb_ix,adcb_ix,orb_ix, addb_ix,illegal,illegal,ldx_ix, stx_ix,
                subb_ex,cmpb_ex,sbcb_ex,illegal,andb_ex,bitb_ex,ldb_ex, stb_ex,
                eorb_ex,adcb_ex,orb_ex, addb_ex,illegal,illegal,ldx_ex, stx_ex
            };
            hd63701_insn = new Action[]
            {
                trap,   nop,    trap    ,trap   ,lsrd,  asld,   tap,    tpa,
                inx,    dex,    clv,    sev,    clc,    sec,    cli,    sei,
                sba,    cba,    undoc1, undoc2, trap    ,trap   ,tab,   tba,
                xgdx,   daa,    slp     ,aba,   trap    ,trap   ,trap   ,trap   ,
                bra,    brn,    bhi,    bls,    bcc,    bcs,    bne,    beq,
                bvc,    bvs,    bpl,    bmi,    bge,    blt,    bgt,    ble,
                tsx,    ins,    pula,   pulb,   des,    txs,    psha,   pshb,
                pulx,   rts,    abx,    rti,    pshx,   mul,    wai,    swi,
                nega,   trap    ,trap   ,coma,  lsra,   trap    ,rora,  asra,
                asla,   rola,   deca,   trap    ,inca,  tsta,   trap    ,clra,
                negb,   trap    ,trap   ,comb,  lsrb,   trap    ,rorb,  asrb,
                aslb,   rolb,   decb,   trap    ,incb,  tstb,   trap    ,clrb,
                neg_ix, aim_ix, oim_ix, com_ix, lsr_ix, eim_ix, ror_ix, asr_ix,
                asl_ix, rol_ix, dec_ix, tim_ix, inc_ix, tst_ix, jmp_ix, clr_ix,
                neg_ex, aim_di, oim_di, com_ex, lsr_ex, eim_di, ror_ex, asr_ex,
                asl_ex, rol_ex, dec_ex, tim_di, inc_ex, tst_ex, jmp_ex, clr_ex,
                suba_im,cmpa_im,sbca_im,subd_im,anda_im,bita_im,lda_im, sta_im,
                eora_im,adca_im,ora_im, adda_im,cpx_im ,bsr,    lds_im, sts_im,
                suba_di,cmpa_di,sbca_di,subd_di,anda_di,bita_di,lda_di, sta_di,
                eora_di,adca_di,ora_di, adda_di,cpx_di ,jsr_di, lds_di, sts_di,
                suba_ix,cmpa_ix,sbca_ix,subd_ix,anda_ix,bita_ix,lda_ix, sta_ix,
                eora_ix,adca_ix,ora_ix, adda_ix,cpx_ix ,jsr_ix, lds_ix, sts_ix,
                suba_ex,cmpa_ex,sbca_ex,subd_ex,anda_ex,bita_ex,lda_ex, sta_ex,
                eora_ex,adca_ex,ora_ex, adda_ex,cpx_ex ,jsr_ex, lds_ex, sts_ex,
                subb_im,cmpb_im,sbcb_im,addd_im,andb_im,bitb_im,ldb_im, stb_im,
                eorb_im,adcb_im,orb_im, addb_im,ldd_im, std_im, ldx_im, stx_im,
                subb_di,cmpb_di,sbcb_di,addd_di,andb_di,bitb_di,ldb_di, stb_di,
                eorb_di,adcb_di,orb_di, addb_di,ldd_di, std_di, ldx_di, stx_di,
                subb_ix,cmpb_ix,sbcb_ix,addd_ix,andb_ix,bitb_ix,ldb_ix, stb_ix,
                eorb_ix,adcb_ix,orb_ix, addb_ix,ldd_ix, std_ix, ldx_ix, stx_ix,
                subb_ex,cmpb_ex,sbcb_ex,addd_ex,andb_ex,bitb_ex,ldb_ex, stb_ex,
                eorb_ex,adcb_ex,orb_ex, addb_ex,ldd_ex, std_ex, ldx_ex, stx_ex
            };
            insn = hd63701_insn;
            cycles = cycles_63701;
            clock = 1536000;
            irq_callback = null;
            //m6800_rx_timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.M6800_action_rx, false);
            EmuTimer.timer_alloc_common(ref m6800_rx_timer, EmuTimer.TIME_ACT.M6800_action_rx, false);
            //m6800_tx_timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.M6800_action_tx, false);
            EmuTimer.timer_alloc_common(ref m6800_tx_timer, EmuTimer.TIME_ACT.M6800_action_tx, false);
        }
        public override void Reset()
        {
            m6800_reset();
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
            WriteMemory(S.LowWord, b);
            --S.LowWord;
        }
        private void PUSHWORD(Register w)
        {
            WriteMemory(S.LowWord, w.LowByte);
            --S.LowWord;
            WriteMemory(S.LowWord, w.HighByte);
            --S.LowWord;
        }
        private byte PULLBYTE()
        {
            S.LowWord++;
            return ReadMemory(S.LowWord);
        }
        private Register PULLWORD()
        {
            Register w = new Register();
            S.LowWord++;
            w.d = (uint)(ReadMemory(S.LowWord) << 8);
            S.LowWord++;
            w.d |= ReadMemory(S.LowWord);
            return w;
        }
        private void MODIFIED_tcsr()
        {
            irq2 = (byte)((tcsr & (tcsr << 3)) & (TCSR_ICF | TCSR_OCF | TCSR_TOF));
        }
        private void SET_TIMER_EVENT()
        {
            timer_next = (output_compare.d - counter.d < timer_over.d - counter.d) ? output_compare.d : timer_over.d;
        }
        protected void CLEANUP_conters()
        {
            output_compare.HighWord -= counter.HighWord;
            timer_over.LowWord -= counter.HighWord;
            counter.HighWord = 0;
            SET_TIMER_EVENT();
        }
        private void MODIFIED_counters()
        {
            output_compare.HighWord = (output_compare.LowWord >= counter.LowWord) ? counter.HighWord : (ushort)(counter.HighWord + 1);
            SET_TIMER_EVENT();
        }
        private void TAKE_ICI()
        {
            ENTER_INTERRUPT(0xfff6);
        }
        private void TAKE_OCI()
        {
            ENTER_INTERRUPT(0xfff4);
        }
        private void TAKE_TOI()
        {
            ENTER_INTERRUPT(0xfff2);
        }
        private void TAKE_SCI()
        {
            ENTER_INTERRUPT(0xfff0);
        }
        private void TAKE_TRAP()
        {
            ENTER_INTERRUPT(0xffee);
        }
        private void ONE_MORE_INSN()
        {
            byte ireg;
            PPC = PC;
            //debugger_instruction_hook(Machine, PCD);
            ireg = ReadOp(PC.LowWord);
            PC.LowWord++;
            insn[ireg]();
            INCREMENT_COUNTER(cycles[ireg]);
        }
        private void CHECK_IRQ_LINES()
        {
            if ((cc & 0x10) == 0)
            {
                if (irq_state[0] != (byte)LineState.CLEAR_LINE)
                {
                    ENTER_INTERRUPT(0xfff8);
                    if (irq_callback != null)
                    {
                        irq_callback(0);
                    }
                }
                else
                {
                    m6800_check_irq2();
                }
            }
        }
        private void CLR_HNZVC()
        {
            cc &= 0xd0;
        }
        private void CLR_NZV()
        {
            cc &= 0xf1;
        }
        private void CLR_HNZC()
        {
            cc &= 0xd2;
        }
        private void CLR_NZVC()
        {
            cc &= 0xf0;
        }
        private void CLR_Z()
        {
            cc &= 0xfb;
        }
        private void CLR_NZC()
        {
            cc &= 0xf2;
        }
        private void CLR_ZC()
        {
            cc &= 0xfa;
        }
        private void CLR_C()
        {
            cc &= 0xfe;
        }
        private void SET_Z(byte a)
        {
            if (a == 0)
            {
                SEZ();
            }
        }
        private void SET_Z(ushort a)
        {
            if (a == 0)
            {
                SEZ();
            }
        }
        private void SET_Z8(byte a)
        {
            SET_Z(a);
        }
        private void SET_Z16(ushort a)
        {
            SET_Z(a);
        }
        private void SET_N8(byte a)
        {
            cc |= (byte)(((a) & 0x80) >> 4);
        }
        private void SET_N16(ushort a)
        {
            cc |= (byte)(((a) & 0x8000) >> 12);
        }
        private void SET_H(ushort a, ushort b, ushort r)
        {
            cc |= (byte)((((a) ^ (b) ^ (r)) & 0x10) << 1);
        }
        private void SET_C8(ushort a)
        {
            cc |= (byte)(((a) & 0x100) >> 8);
        }
        private void SET_C16(uint a)
        {
            cc |= (byte)(((a) & 0x10000) >> 16);
        }
        private void SET_V8(ushort a, ushort b, ushort r)
        {
            cc |= (byte)((((a) ^ (b) ^ (r) ^ ((r) >> 1)) & 0x80) >> 6);
        }
        private void SET_V16(uint a, uint b, uint r)
        {
            cc |= (byte)((((a) ^ (b) ^ (r) ^ ((r) >> 1)) & 0x8000) >> 14);
        }
        private void SET_FLAGS8I(byte a)
        {
            cc |= flags8i[(a) & 0xff];
        }
        private void SET_FLAGS8D(byte a)
        {
            cc |= flags8d[(a) & 0xff];
        }
        private void SET_NZ8(byte a)
        {
            SET_N8(a);
            SET_Z8(a);
        }
        private void SET_NZ16(ushort a)
        {
            SET_N16(a);
            SET_Z16(a);
        }
        private void SET_FLAGS8(ushort a, ushort b, ushort r)
        {
            SET_N8((byte)r);
            SET_Z8((byte)r);
            SET_V8(a, b, r);
            SET_C8(r);
        }
        private void SET_FLAGS16(uint a, uint b, uint r)
        {
            SET_N16((ushort)r);
            SET_Z16((ushort)r);
            SET_V16(a, b, r);
            SET_C16(r);
        }
        private short SIGNED(byte b)
        {
            return (short)((b & 0x80) != 0 ? b | 0xff00 : b);
        }
        private void DIRECT()
        {
            EA.d = IMMBYTE();
        }
        private void IMM8()
        {
            EA.LowWord = PC.LowWord++;
        }
        private void IMM16()
        {
            EA.LowWord = PC.LowWord;
            PC.LowWord += 2;
        }
        private void EXTENDED()
        {
            EA = IMMWORD();
        }
        private void INDEXED()
        {
            EA.LowWord = (ushort)(X.LowWord + ReadOpArg(PC.LowWord));
            PC.LowWord++;
        }
        protected void SEC()
        {
            cc |= 0x01;
        }
        protected void CLC()
        {
            cc &= 0xfe;
        }
        protected void SEZ()
        {
            cc |= 0x04;
        }
        protected void CLZ()
        {
            cc &= 0xfb;
        }
        protected void SEN()
        {
            cc |= 0x08;
        }
        protected void CLN()
        {
            cc &= 0xf7;
        }
        protected void SEV()
        {
            cc |= 0x02;
        }
        protected void CLV()
        {
            cc &= 0xfd;
        }
        protected void SEH()
        {
            cc |= 0x20;
        }
        protected void CLH()
        {
            cc &= 0xdf;
        }
        protected void SEI()
        {
            cc |= 0x10;
        }
        protected void CLI()
        {
            cc &= unchecked((byte)(~0x10));
        }
        protected void INCREMENT_COUNTER(int amount)
        {
            pendingCycles -= amount;
            counter.d += (uint)amount;
            if (counter.d >= timer_next)
                check_timer_event();
        }
        protected void EAT_CYCLES()
        {
            int cycles_to_eat;
            cycles_to_eat = (int)(timer_next - counter.d);
            if (cycles_to_eat > pendingCycles)
                cycles_to_eat = pendingCycles;
            if (cycles_to_eat > 0)
            {
                INCREMENT_COUNTER(cycles_to_eat);
            }
        }
        private byte DIRBYTE()
        {
            DIRECT();
            return ReadMemory(EA.LowWord);
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
            return ReadMemory(EA.LowWord);
        }
        private Register EXTWORD()
        {
            Register w = new Register();
            EXTENDED();
            w.LowWord = RM16(EA.LowWord);
            return w;
        }
        private byte IDXBYTE()
        {
            INDEXED();
            return ReadMemory(EA.LowWord);
        }
        private Register IDXWORD()
        {
            Register w = new Register();
            INDEXED();
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
        private byte NXORV()
        {
            return (byte)((cc & 0x08) ^ ((cc & 0x02) << 2));
        }
        private ushort RM16(ushort Addr)
        {
            ushort result = (ushort)(ReadMemory(Addr) << 8);
            return (ushort)(result | ReadMemory((ushort)(Addr + 1)));
        }
        private void WM16(ushort Addr, Register p)
        {
            WriteMemory(Addr, p.HighByte);
            WriteMemory((ushort)(Addr + 1), p.LowByte);
        }
        private void ENTER_INTERRUPT(ushort irq_vector)
        {
            if ((wai_state & (M6800_WAI | M6800_SLP)) != 0)
            {
                if ((wai_state & M6800_WAI) != 0)
                    extra_cycles += 4;
                wai_state &= (byte)(~(M6800_WAI | M6800_SLP));
            }
            else
            {
                PUSHWORD(PC);
                PUSHWORD(X);
                PUSHBYTE(D.HighByte);
                PUSHBYTE(D.LowByte);
                PUSHBYTE(cc);
                extra_cycles += 12;
            }
            SEI();
            PC.d = RM16(irq_vector);
        }
        private void m6800_check_irq2()
        {
            if ((tcsr & (TCSR_EICI | TCSR_ICF)) == (TCSR_EICI | TCSR_ICF))
            {
                TAKE_ICI();
                //if( m6800.irq_callback )
                //	(void)(*m6800.irq_callback)(M6800_TIN_LINE);
            }
            else if ((tcsr & (TCSR_EOCI | TCSR_OCF)) == (TCSR_EOCI | TCSR_OCF))
            {
                TAKE_OCI();
            }
            else if ((tcsr & (TCSR_ETOI | TCSR_TOF)) == (TCSR_ETOI | TCSR_TOF))
            {
                TAKE_TOI();
            }
            else if (((trcsr & (M6800_TRCSR_RIE | M6800_TRCSR_RDRF)) == (M6800_TRCSR_RIE | M6800_TRCSR_RDRF)) ||
                     ((trcsr & (M6800_TRCSR_RIE | M6800_TRCSR_ORFE)) == (M6800_TRCSR_RIE | M6800_TRCSR_ORFE)) ||
                     ((trcsr & (M6800_TRCSR_TIE | M6800_TRCSR_TDRE)) == (M6800_TRCSR_TIE | M6800_TRCSR_TDRE)))
            {
                TAKE_SCI();
            }
        }
        private void check_timer_event()
        {
            if (counter.d >= output_compare.d)
            {
                output_compare.HighWord++;
                tcsr |= TCSR_OCF;
                pending_tcsr |= TCSR_OCF;
                MODIFIED_tcsr();
                if (((cc & 0x10) == 0) && ((tcsr & TCSR_EOCI) != 0))
                    TAKE_OCI();
            }
            if (counter.d >= timer_over.d)
            {
                timer_over.LowWord++;
                tcsr |= TCSR_TOF;
                pending_tcsr |= TCSR_TOF;
                MODIFIED_tcsr();
                if (((cc & 0x10) == 0) && (tcsr & TCSR_ETOI) != 0)
                    TAKE_TOI();
            }
            SET_TIMER_EVENT();
        }
        private void m6800_tx(int value)
        {
            port2_data = (byte)((port2_data & 0xef) | (value << 4));
            if (port2_ddr == 0xff)
                WriteIO(M6803_PORT2, port2_data);
            else
                WriteIO(M6803_PORT2, (byte)((port2_data & port2_ddr) | (ReadIO(M6803_PORT2) & (port2_ddr ^ 0xff))));
        }
        private int m6800_rx()
        {
            return (ReadIO(M6803_PORT2) & M6800_PORT2_IO3) >> 3;
        }
        public void m6800_tx_tick()
        {
            if ((trcsr & M6800_TRCSR_TE) != 0)
            {
                port2_ddr |= M6800_PORT2_IO4;
                switch (txstate)
                {
                    case M6800_TX_STATE.INIT:
                        tx = 1;
                        txbits++;

                        if (txbits == 10)
                        {
                            txstate = M6800_TX_STATE.READY;
                            txbits = 0;
                        }
                        break;

                    case M6800_TX_STATE.READY:
                        switch (txbits)
                        {
                            case 0:
                                if ((trcsr & M6800_TRCSR_TDRE) != 0)
                                {
                                    tx = 1;
                                }
                                else
                                {
                                    tsr = tdr;
                                    trcsr |= M6800_TRCSR_TDRE;
                                    tx = 0;
                                    txbits++;
                                }
                                break;

                            case 9:
                                // send stop bit '1'
                                tx = 1;
                                CHECK_IRQ_LINES();
                                txbits = 0;
                                break;

                            default:
                                tx = tsr & 0x01;
                                tsr >>= 1;
                                txbits++;
                                break;
                        }
                        break;
                }
            }
            m6800_tx(tx);
        }
        public void m6800_rx_tick()
        {
            if ((trcsr & M6800_TRCSR_RE) != 0)
            {
                if ((trcsr & M6800_TRCSR_WU) != 0)
                {
                    if (m6800_rx() == 1)
                    {
                        rxbits++;
                        if (rxbits == 10)
                        {
                            trcsr &= (byte)(~M6800_TRCSR_WU);
                            rxbits = 0;
                        }
                    }
                    else
                    {
                        rxbits = 0;
                    }
                }
                else
                {
                    switch (rxbits)
                    {
                        case 0:
                            if (m6800_rx() == 0)
                            {
                                rxbits++;
                            }
                            break;
                        case 9:
                            if (m6800_rx() == 1)
                            {
                                if ((trcsr & M6800_TRCSR_RDRF) != 0)
                                {
                                    trcsr |= M6800_TRCSR_ORFE;
                                    CHECK_IRQ_LINES();
                                }
                                else
                                {
                                    if ((trcsr & M6800_TRCSR_ORFE) == 0)
                                    {
                                        rdr = rsr;
                                        trcsr |= M6800_TRCSR_RDRF;
                                        CHECK_IRQ_LINES();
                                    }
                                }
                            }
                            else
                            {
                                if ((trcsr & M6800_TRCSR_ORFE) == 0)
                                {
                                    // transfer unframed data into receive register
                                    rdr = rsr;
                                }
                                trcsr |= (byte)M6800_TRCSR_ORFE;
                                trcsr &= (byte)(~M6800_TRCSR_RDRF);
                                CHECK_IRQ_LINES();
                            }
                            rxbits = 0;
                            break;
                        default:
                            rsr >>= 1;
                            rsr |= (byte)(m6800_rx() << 7);
                            rxbits++;
                            break;
                    }
                }
            }
        }
        private void m6800_reset()
        {
            SEI();				/* IRQ disabled */
            PC.LowWord = RM16(0xfffe);
            wai_state = 0;
            nmi_state = 0;
            irq_state[0] = 0;
            irq_state[1] = 0;
            ic_eddge = 0;
            port1_ddr = 0x00;
            port2_ddr = 0x00;
            tcsr = 0x00;
            pending_tcsr = 0x00;
            irq2 = 0;
            counter.d = 0x0000;
            output_compare.d = 0xffff;
            timer_over.d = 0xffff;
            ram_ctrl |= 0x40;
            trcsr = M6800_TRCSR_TDRE;
            rmcr = 0;
            EmuTimer.timer_enable(m6800_rx_timer, false);
            EmuTimer.timer_enable(m6800_tx_timer, false);
            txstate = M6800_TX_STATE.INIT;
            txbits = rxbits = 0;
            trcsr_read = 0;
        }
        public override void set_irq_line(int irqline, LineState state)
        {
            if (irqline == INPUT_LINE_NMI)
            {
                if (nmi_state == (byte)state)
                {
                    return;
                }
                nmi_state = (byte)state;
                if (state == LineState.CLEAR_LINE)
                {
                    return;
                }
                ENTER_INTERRUPT(0xfffc);
            }
            else
            {
                int eddge;
                if (irq_state[irqline] == (byte)state)
                {
                    return;
                }
                irq_state[irqline] = (byte)state;
                switch (irqline)
                {
                    case M6800_IRQ_LINE:
                        if (state == LineState.CLEAR_LINE)
                        {
                            return;
                        }
                        break;
                    case M6800_TIN_LINE:
                        eddge = (state == LineState.CLEAR_LINE) ? 2 : 0;
                        if (((tcsr & TCSR_IEDG) ^ (state == LineState.CLEAR_LINE ? TCSR_IEDG : 0)) == 0)
                        {
                            return;
                        }
                        /* active edge in */
                        tcsr |= TCSR_ICF;
                        pending_tcsr |= TCSR_ICF;
                        input_capture = counter.LowWord;
                        MODIFIED_tcsr();
                        if ((cc & 0x10) == 0)
                        {
                            m6800_check_irq2();
                        }
                        break;
                    default:
                        return;
                }
                CHECK_IRQ_LINES(); /* HJB 990417 */
            }
        }
        public override void cpunum_set_input_line_and_vector(int cpunum, int line, LineState state, int vector)
        {
            EmuTimer.timer_set_internal(EmuTimer.TIME_ACT.Cpuint_cpunum_empty_event_queue);
        }
        public override int ExecuteCycles(int cycles)
        {
            byte ireg;
            pendingCycles = cycles;
            CLEANUP_conters();
            INCREMENT_COUNTER(extra_cycles);
            extra_cycles = 0;
            do
            {
                int prevCycles = pendingCycles;
                if ((wai_state & (M6800_WAI | M6800_SLP)) != 0)
                {
                    EAT_CYCLES();
                }
                else
                {
                    PPC = PC;
                    //debugger_instruction_hook(Machine, PCD);                    
                    ireg = ReadOp(PC.LowWord);
                    PC.LowWord++;
                    insn[ireg]();
                    INCREMENT_COUNTER(this.cycles[ireg]);
                    int delta = prevCycles - pendingCycles;
                    totalExecutedCycles += (ulong)delta;
                }
            } while (pendingCycles > 0);
            INCREMENT_COUNTER(extra_cycles);
            extra_cycles = 0;
            return cycles - pendingCycles;
        }
        public byte hd63701_internal_registers_r(int offset)
        {
            return m6803_internal_registers_r(offset);
        }
        public void hd63701_internal_registers_w(int offset, byte data)
        {
            m6803_internal_registers_w(offset, data);
        }
        private byte m6803_internal_registers_r(int offset)
        {
            switch (offset)
            {
                case 0x00:
                    return port1_ddr;
                case 0x01:
                    return port2_ddr;
                case 0x02:
                    return (byte)((ReadIO(0x100) & (port1_ddr ^ 0xff)) | (port1_data & port1_ddr));
                case 0x03:
                    return (byte)((ReadIO(0x101) & (port2_ddr ^ 0xff)) | (port2_data & port2_ddr));
                case 0x04:
                    return port3_ddr;
                case 0x05:
                    return port4_ddr;
                case 0x06:
                    return (byte)((ReadIO(0x102) & (port3_ddr ^ 0xff)) | (port3_data & port3_ddr));
                case 0x07:
                    return (byte)((ReadIO(0x103) & (port4_ddr ^ 0xff)) | (port4_data & port4_ddr));
                case 0x08:
                    pending_tcsr = 0;
                    return tcsr;
                case 0x09:
                    if ((pending_tcsr & TCSR_TOF) == 0)
                    {
                        tcsr &= (byte)(~TCSR_TOF);
                        MODIFIED_tcsr();
                    }
                    return counter.HighByte;
                case 0x0a:
                    return counter.LowByte;
                case 0x0b:
                    if ((pending_tcsr & TCSR_OCF) == 0)
                    {
                        tcsr &= (byte)(~TCSR_OCF);
                        MODIFIED_tcsr();
                    }
                    return output_compare.HighByte;
                case 0x0c:
                    if ((pending_tcsr & TCSR_OCF) == 0)
                    {
                        tcsr &= (byte)(~TCSR_OCF);
                        MODIFIED_tcsr();
                    }
                    return output_compare.LowByte;
                case 0x0d:
                    if ((pending_tcsr & TCSR_ICF) == 0)
                    {
                        tcsr &= (byte)(~TCSR_ICF);
                        MODIFIED_tcsr();
                    }
                    return (byte)((input_capture >> 0) & 0xff);
                case 0x0e:
                    return (byte)((input_capture >> 8) & 0xff);
                case 0x0f:
                    //logerror("CPU #%d PC %04x: warning - read from unsupported register %02x\n",cpu_getactivecpu(),activecpu_get_pc(),offset);
                    return 0;
                case 0x10:
                    return rmcr;
                case 0x11:
                    trcsr_read = 1;
                    return trcsr;
                case 0x12:
                    if (trcsr_read != 0)
                    {
                        trcsr_read = 0;
                        trcsr = (byte)(trcsr & 0x3f);
                    }
                    return rdr;
                case 0x13:
                    return tdr;
                case 0x14:
                    //logerror("CPU #%d PC %04x: read RAM control register\n",cpu_getactivecpu(),activecpu_get_pc());
                    return ram_ctrl;
                case 0x15:
                case 0x16:
                case 0x17:
                case 0x18:
                case 0x19:
                case 0x1a:
                case 0x1b:
                case 0x1c:
                case 0x1d:
                case 0x1e:
                case 0x1f:
                default:
                    //logerror("CPU #%d PC %04x: warning - read from reserved internal register %02x\n",cpu_getactivecpu(),activecpu_get_pc(),offset);
                    return 0;
            }
        }
        private void m6803_internal_registers_w(int offset, byte data)
        {
            int latch09 = 0;
            switch (offset)
            {
                case 0x00:
                    if (port1_ddr != data)
                    {
                        port1_ddr = data;
                        if (port1_ddr == 0xff)
                            WriteIO(0x100, port1_data);
                        else
                            WriteIO(0x100, (byte)((port1_data & port1_ddr) | (ReadIO(0x100) & (port1_ddr ^ 0xff))));
                    }
                    break;
                case 0x01:
                    if (port2_ddr != data)
                    {
                        port2_ddr = data;
                        if (port2_ddr == 0xff)
                            WriteIO(0x101, port2_data);
                        else
                            WriteIO(0x101, (byte)((port2_data & port2_ddr) | (ReadIO(0x101) & (port2_ddr ^ 0xff))));

                        if ((port2_ddr & 2) != 0)
                        {
                            //logerror("CPU #%d PC %04x: warning - port 2 bit 1 set as output (OLVL) - not supported\n", cpu_getactivecpu(), activecpu_get_pc());
                        }
                    }
                    break;
                case 0x02:
                    port1_data = data;
                    if (port1_ddr == 0xff)
                        WriteIO(0x100, port1_data);
                    else
                        WriteIO(0x100, (byte)((port1_data & port1_ddr) | (ReadIO(0x100) & (port1_ddr ^ 0xff))));
                    break;
                case 0x03:
                    if ((trcsr & M6800_TRCSR_TE) != 0)
                    {
                        port2_data = (byte)((data & 0xef) | (tx << 4));
                    }
                    else
                    {
                        port2_data = data;
                    }
                    if (port2_ddr == 0xff)
                        WriteIO(0x101, port2_data);
                    else
                        WriteIO(0x101, (byte)((port2_data & port2_ddr) | (ReadIO(0x101) & (port2_ddr ^ 0xff))));
                    break;
                case 0x04:
                    if (port3_ddr != data)
                    {
                        port3_ddr = data;
                        if (port3_ddr == 0xff)
                            WriteIO(0x102, port3_data);
                        else
                            WriteIO(0x102, (byte)((port3_data & port3_ddr) | (ReadIO(0x102) & (port3_ddr ^ 0xff))));
                    }
                    break;
                case 0x05:
                    if (port4_ddr != data)
                    {
                        port4_ddr = data;
                        if (port4_ddr == 0xff)
                            WriteIO(0x103, port4_data);
                        else
                            WriteIO(0x103, (byte)((port4_data & port4_ddr) | (ReadIO(0x103) & (port4_ddr ^ 0xff))));
                    }
                    break;
                case 0x06:
                    port3_data = data;
                    if (port3_ddr == 0xff)
                        WriteIO(0x102, port3_data);
                    else
                        WriteIO(0x102, (byte)((port3_data & port3_ddr) | (ReadIO(0x102) & (port3_ddr ^ 0xff))));
                    break;
                case 0x07:
                    port4_data = data;
                    if (port4_ddr == 0xff)
                        WriteIO(0x103, port4_data);
                    else
                        WriteIO(0x103, (byte)((port4_data & port4_ddr) | (ReadIO(0x103) & (port4_ddr ^ 0xff))));
                    break;
                case 0x08:
                    tcsr = data;
                    pending_tcsr &= tcsr;
                    MODIFIED_tcsr();
                    if ((cc & 0x10) == 0)
                        m6800_check_irq2();
                    break;
                case 0x09:
                    latch09 = data & 0xff;	/* 6301 only */
                    counter.LowWord = 0xfff8;
                    timer_over.LowWord = counter.HighWord;
                    MODIFIED_counters();
                    break;
                case 0x0a:	/* 6301 only */
                    counter.LowWord = (ushort)((latch09 << 8) | (data & 0xff));
                    timer_over.LowWord = counter.HighWord;
                    MODIFIED_counters();
                    break;
                case 0x0b:
                    if (output_compare.HighByte != data)
                    {
                        output_compare.HighByte = data;
                        MODIFIED_counters();
                    }
                    break;
                case 0x0c:
                    if (output_compare.LowByte != data)
                    {
                        output_compare.LowByte = data;
                        MODIFIED_counters();
                    }
                    break;
                case 0x0d:
                case 0x0e:
                case 0x12:
                    //logerror("CPU #%d PC %04x: warning - write %02x to read only internal register %02x\n",cpu_getactivecpu(),activecpu_get_pc(),data,offset);
                    break;
                case 0x0f:
                    //logerror("CPU #%d PC %04x: warning - write %02x to unsupported internal register %02x\n",cpu_getactivecpu(),activecpu_get_pc(),data,offset);
                    break;
                case 0x10:
                    rmcr = (byte)(data & 0x0f);
                    switch ((rmcr & M6800_RMCR_CC_MASK) >> 2)
                    {
                        case 0:
                        case 3: // not implemented
                            EmuTimer.timer_enable(m6800_rx_timer, false);
                            EmuTimer.timer_enable(m6800_tx_timer, false);
                            break;

                        case 1:
                        case 2:
                            {
                                int divisor = M6800_RMCR_SS[rmcr & M6800_RMCR_SS_MASK];
                                EmuTimer.timer_adjust_periodic(m6800_rx_timer, Attotime.ATTOTIME_ZERO, new Atime(0, (long)(1e18 / (clock / divisor))));
                                EmuTimer.timer_adjust_periodic(m6800_tx_timer, Attotime.ATTOTIME_ZERO, new Atime(0, (long)(1e18 / (clock / divisor))));
                            }
                            break;
                    }
                    break;
                case 0x11:
                    if ((data & M6800_TRCSR_TE) != 0 && (trcsr & M6800_TRCSR_TE) == 0)
                    {
                        txstate = 0;
                    }
                    trcsr = (byte)((trcsr & 0xe0) | (data & 0x1f));
                    break;
                case 0x13:
                    if (trcsr_read != 0)
                    {
                        trcsr_read = (int)M6800_TX_STATE.INIT;
                        trcsr &= (byte)(~M6800_TRCSR_TDRE);
                    }
                    tdr = data;
                    break;
                case 0x14:
                    //logerror("CPU #%d PC %04x: write %02x to RAM control register\n",cpu_getactivecpu(),activecpu_get_pc(),data);
                    ram_ctrl = data;
                    break;
                case 0x15:
                case 0x16:
                case 0x17:
                case 0x18:
                case 0x19:
                case 0x1a:
                case 0x1b:
                case 0x1c:
                case 0x1d:
                case 0x1e:
                case 0x1f:
                default:
                    //logerror("CPU #%d PC %04x: warning - write %02x to reserved internal register %02x\n",cpu_getactivecpu(),activecpu_get_pc(),data,offset);
                    break;
            }
        }
        public void SaveStateBinary(System.IO.BinaryWriter writer)
        {
            writer.Write(PPC.LowWord);
            writer.Write(PC.LowWord);
            writer.Write(S.LowWord);
            writer.Write(X.LowWord);
            writer.Write(D.LowWord);
            writer.Write(cc);
            writer.Write(wai_state);
            writer.Write(nmi_state);
            writer.Write(irq_state[0]);
            writer.Write(irq_state[1]);
            writer.Write(ic_eddge);
            writer.Write(port1_ddr);
            writer.Write(port2_ddr);
            writer.Write(port3_ddr);
            writer.Write(port4_ddr);
            writer.Write(port1_data);
            writer.Write(port2_data);
            writer.Write(port3_data);
            writer.Write(port4_data);
            writer.Write(tcsr);
            writer.Write(pending_tcsr);
            writer.Write(irq2);
            writer.Write(ram_ctrl);
            writer.Write(counter.d);
            writer.Write(output_compare.d);
            writer.Write(input_capture);
            writer.Write(timer_over.d);
            writer.Write(clock);
            writer.Write(trcsr);
            writer.Write(rmcr);
            writer.Write(rdr);
            writer.Write(tdr);
            writer.Write(rsr);
            writer.Write(tsr);
            writer.Write(rxbits);
            writer.Write(txbits);
            writer.Write((int)txstate);
            writer.Write(trcsr_read);
            writer.Write(tx);
            writer.Write(TotalExecutedCycles);
            writer.Write(PendingCycles);
        }
        public void LoadStateBinary(System.IO.BinaryReader reader)
        {
            PPC.LowWord = reader.ReadUInt16();
            PC.LowWord = reader.ReadUInt16();
            S.LowWord = reader.ReadUInt16();
            X.LowWord = reader.ReadUInt16();
            D.LowWord = reader.ReadUInt16();
            cc = reader.ReadByte();
            wai_state = reader.ReadByte();
            nmi_state = reader.ReadByte();
            irq_state[0] = reader.ReadByte();
            irq_state[1] = reader.ReadByte();
            ic_eddge = reader.ReadByte();
            port1_ddr = reader.ReadByte();
            port2_ddr = reader.ReadByte();
            port3_ddr = reader.ReadByte();
            port4_ddr = reader.ReadByte();
            port1_data = reader.ReadByte();
            port2_data = reader.ReadByte();
            port3_data = reader.ReadByte();
            port4_data = reader.ReadByte();
            tcsr = reader.ReadByte();
            pending_tcsr = reader.ReadByte();
            irq2 = reader.ReadByte();
            ram_ctrl = reader.ReadByte();
            counter.d = reader.ReadUInt32();
            output_compare.d = reader.ReadUInt32();
            input_capture = reader.ReadUInt16();
            timer_over.d = reader.ReadUInt32();
            clock = reader.ReadInt32();
            trcsr = reader.ReadByte();
            rmcr = reader.ReadByte();
            rdr = reader.ReadByte();
            tdr = reader.ReadByte();
            rsr = reader.ReadByte();
            tsr = reader.ReadByte();
            rxbits = reader.ReadInt32();
            txbits = reader.ReadInt32();
            txstate = (M6800.M6800_TX_STATE)reader.ReadInt32();
            trcsr_read = reader.ReadInt32();
            tx = reader.ReadInt32();
            TotalExecutedCycles = reader.ReadUInt64();
            PendingCycles = reader.ReadInt32();
        }
    }
    public class M6801 : M6800
    {
        public M6801()
        {
            m6803_insn = new Action[256]{
                illegal,nop,    illegal,illegal,lsrd,   asld,   tap,    tpa,
                inx,    dex,    CLV,    SEV,    CLC,    SEC,    cli,    sei,
                sba,    cba,    illegal,illegal,illegal,illegal,tab,    tba,
                illegal,daa,    illegal,aba,    illegal,illegal,illegal,illegal,
                bra,    brn,    bhi,    bls,    bcc,    bcs,    bne,    beq,
                bvc,    bvs,    bpl,    bmi,    bge,    blt,    bgt,    ble,
                tsx,    ins,    pula,   pulb,   des,    txs,    psha,   pshb,
                pulx,   rts,    abx,    rti,    pshx,   mul,    wai,    swi,
                nega,   illegal,illegal,coma,   lsra,   illegal,rora,   asra,
                asla,   rola,   deca,   illegal,inca,   tsta,   illegal,clra,
                negb,   illegal,illegal,comb,   lsrb,   illegal,rorb,   asrb,
                aslb,   rolb,   decb,   illegal,incb,   tstb,   illegal,clrb,
                neg_ix, illegal,illegal,com_ix, lsr_ix, illegal,ror_ix, asr_ix,
                asl_ix, rol_ix, dec_ix, illegal,inc_ix, tst_ix, jmp_ix, clr_ix,
                neg_ex, illegal,illegal,com_ex, lsr_ex, illegal,ror_ex, asr_ex,
                asl_ex, rol_ex, dec_ex, illegal,inc_ex, tst_ex, jmp_ex, clr_ex,
                suba_im,cmpa_im,sbca_im,subd_im,anda_im,bita_im,lda_im, sta_im,
                eora_im,adca_im,ora_im, adda_im,cpx_im, bsr,    lds_im, sts_im,
                suba_di,cmpa_di,sbca_di,subd_di,anda_di,bita_di,lda_di, sta_di,
                eora_di,adca_di,ora_di, adda_di,cpx_di, jsr_di, lds_di, sts_di,
                suba_ix,cmpa_ix,sbca_ix,subd_ix,anda_ix,bita_ix,lda_ix, sta_ix,
                eora_ix,adca_ix,ora_ix, adda_ix,cpx_ix, jsr_ix, lds_ix, sts_ix,
                suba_ex,cmpa_ex,sbca_ex,subd_ex,anda_ex,bita_ex,lda_ex, sta_ex,
                eora_ex,adca_ex,ora_ex, adda_ex,cpx_ex, jsr_ex, lds_ex, sts_ex,
                subb_im,cmpb_im,sbcb_im,addd_im,andb_im,bitb_im,ldb_im, stb_im,
                eorb_im,adcb_im,orb_im, addb_im,ldd_im, std_im, ldx_im, stx_im,
                subb_di,cmpb_di,sbcb_di,addd_di,andb_di,bitb_di,ldb_di, stb_di,
                eorb_di,adcb_di,orb_di, addb_di,ldd_di, std_di, ldx_di, stx_di,
                subb_ix,cmpb_ix,sbcb_ix,addd_ix,andb_ix,bitb_ix,ldb_ix, stb_ix,
                eorb_ix,adcb_ix,orb_ix, addb_ix,ldd_ix, std_ix, ldx_ix, stx_ix,
                subb_ex,cmpb_ex,sbcb_ex,addd_ex,andb_ex,bitb_ex,ldb_ex, stb_ex,
                eorb_ex,adcb_ex,orb_ex, addb_ex,ldd_ex, std_ex, ldx_ex, stx_ex
            };
            clock = 1000000;
            irq_callback = Cpuint.cpu_3_irq_callback;
            //m6800_rx_timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.M6800_action_rx, false);
            EmuTimer.timer_alloc_common(ref m6800_rx_timer, EmuTimer.TIME_ACT.M6800_action_rx, false);
            //m6800_tx_timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.M6800_action_tx, false);
            EmuTimer.timer_alloc_common(ref m6800_tx_timer, EmuTimer.TIME_ACT.M6800_action_tx, false);
        }
        public override int ExecuteCycles(int cycles)
        {
            return m6801_execute(cycles);
        }
        public int m6801_execute(int cycles)
        {
            byte ireg;
            pendingCycles = cycles;
            CLEANUP_conters();
            INCREMENT_COUNTER(extra_cycles);
            extra_cycles = 0;
            do
            {
                int prevCycles = pendingCycles;
                if ((wai_state & M6800_WAI) != 0)
                {
                    EAT_CYCLES();
                }
                else
                {
                    PPC = PC;
                    //debugger_instruction_hook(Machine, PCD);
                    ireg = ReadOp(PC.LowWord);
                    PC.LowWord++;
                    m6803_insn[ireg]();
                    INCREMENT_COUNTER(cycles_6803[ireg]);
                    int delta = prevCycles - pendingCycles;
                    totalExecutedCycles += (ulong)delta;
                }
            } while (pendingCycles > 0);
            INCREMENT_COUNTER(extra_cycles);
            extra_cycles = 0;
            return cycles - pendingCycles;
        }
    }
}
