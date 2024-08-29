#undef DPCM_SYNCCLOCK

using System;

namespace VirtualNes.Core
{
    public class CPU
    {
        private static int nmicount;

        // 6502 status flags
        public const byte C_FLAG = 0x01;        // 1: Carry
        public const byte Z_FLAG = 0x02;    // 1: Zero
        public const byte I_FLAG = 0x04;    // 1: Irq disabled
        public const byte D_FLAG = 0x08;    // 1: Decimal mode flag (NES unused)
        public const byte B_FLAG = 0x10;    // 1: Break
        public const byte R_FLAG = 0x20;    // 1: Reserved (Always 1)
        public const byte V_FLAG = 0x40;   // 1: Overflow
        public const byte N_FLAG = 0x80;    // 1: Negative

        // Interrupt
        public const byte NMI_FLAG = 0x01;
        public const byte IRQ_FLAG = 0x02;

        public const byte IRQ_FRAMEIRQ = 0x04;
        public const byte IRQ_DPCM = 0x08;
        public const byte IRQ_MAPPER = 0x10;
        public const byte IRQ_MAPPER2 = 0x20;
        public const byte IRQ_TRIGGER = 0x40;		// one shot(媽IRQ())
        public const byte IRQ_TRIGGER2 = 0x80;      // one shot(媽IRQ_NotPending())

        public static readonly byte IRQ_MASK = unchecked((byte)(~(NMI_FLAG | IRQ_FLAG)));

        // Vector
        public const ushort NMI_VECTOR = 0xFFFA;
        public const ushort RES_VECTOR = 0xFFFC;
        public const ushort IRQ_VECTOR = 0xFFFE;

        private NES nes;
        private bool m_bClockProcess;
        private int TOTAL_cycles;
        private int DMA_cycles;
        private Mapper mapper;
        private APU apu;
        internal R6502 R = new R6502();
        private byte[] ZN_Table = new byte[256];
        private ArrayRef<byte> STACK;

        public CPU(NES parent)
        {
            nes = parent;
            m_bClockProcess = false;

        }

        public void Dispose() { }

        ushort EA = 0;
        ushort ET = 0;
        ushort WT = 0;
        byte DT = 0;
        int exec_cycles = 0;

        internal long EXEC(int request_cycles)
        {
            byte opcode = 0;
            int OLD_cycles = TOTAL_cycles;
            byte nmi_request = 0, irq_request = 0;
            bool bClockProcess = m_bClockProcess;

            exec_cycles = 0;
            EA = 0;
            ET = 0;
            WT = 0;
            DT = 0;

            while (request_cycles > 0)
            {
                exec_cycles = 0;
                if (DMA_cycles > 0)
                {
                    if (request_cycles <= DMA_cycles)
                    {
                        DMA_cycles -= request_cycles;
                        TOTAL_cycles += request_cycles;

                        mapper.Clock(request_cycles);
#if DPCM_SYNCCLOCK
                        apu.SyncDPCM(request_cycles);
#endif
                        if (m_bClockProcess)
                        {
                            nes.Clock(request_cycles);
                        }

                        goto _execute_exit;
                    }
                    else
                    {
                        exec_cycles += DMA_cycles;
                        DMA_cycles = 0;
                    }
                }

                nmi_request = irq_request = 0;
                opcode = OP6502(R.PC++);

                if (R.INT_pending != 0)
                {
                    if ((R.INT_pending & NMI_FLAG) != 0)
                    {
                        nmi_request = 0xFF;
                        byte temp = unchecked((byte)(~NMI_FLAG));
                        R.INT_pending &= temp;
                    }
                    else if ((R.INT_pending & IRQ_MASK) != 0)
                    {
                        byte temp = unchecked((byte)(~IRQ_TRIGGER2));
                        R.INT_pending &= temp;
                        if (
                            ((R.P & I_FLAG) == 0)
                            &&
                            (opcode != 0x40)
                            )
                        {
                            irq_request = 0xFF;
                            temp = unchecked((byte)(~IRQ_TRIGGER));
                            R.INT_pending &= temp;
                        }
                    }
                }

                switch (opcode)
                {
                    case 0x69: // ADC #$??
                        MR_IM(); ADC();
                        ADD_CYCLE(2);
                        break;
                    case 0x65: // ADC $??
                        MR_ZP(); ADC();
                        ADD_CYCLE(3);
                        break;
                    case 0x75: // ADC $??,X
                        MR_ZX(); ADC();
                        ADD_CYCLE(4);
                        break;
                    case 0x6D: // ADC $????
                        MR_AB(); ADC();
                        ADD_CYCLE(4);
                        break;
                    case 0x7D: // ADC $????,X
                        MR_AX(); ADC(); CHECK_EA();
                        ADD_CYCLE(4);
                        break;
                    case 0x79: // ADC $????,Y
                        MR_AY(); ADC(); CHECK_EA();
                        ADD_CYCLE(4);
                        break;
                    case 0x61: // ADC ($??,X)
                        MR_IX(); ADC();
                        ADD_CYCLE(6);
                        break;
                    case 0x71: // ADC ($??),Y
                        MR_IY(); ADC(); CHECK_EA();
                        ADD_CYCLE(4);
                        break;

                    case 0xE9: // SBC #$??
                        MR_IM(); SBC();
                        ADD_CYCLE(2);
                        break;
                    case 0xE5: // SBC $??
                        MR_ZP(); SBC();
                        ADD_CYCLE(3);
                        break;
                    case 0xF5: // SBC $??,X
                        MR_ZX(); SBC();
                        ADD_CYCLE(4);
                        break;
                    case 0xED: // SBC $????
                        MR_AB(); SBC();
                        ADD_CYCLE(4);
                        break;
                    case 0xFD: // SBC $????,X
                        MR_AX(); SBC(); CHECK_EA();
                        ADD_CYCLE(4);
                        break;
                    case 0xF9: // SBC $????,Y
                        MR_AY(); SBC(); CHECK_EA();
                        ADD_CYCLE(4);
                        break;
                    case 0xE1: // SBC ($??,X)
                        MR_IX(); SBC();
                        ADD_CYCLE(6);
                        break;
                    case 0xF1: // SBC ($??),Y
                        MR_IY(); SBC(); CHECK_EA();
                        ADD_CYCLE(5);
                        break;

                    case 0xC6: // DEC $??
                        MR_ZP(); DEC(); MW_ZP();
                        ADD_CYCLE(5);
                        break;
                    case 0xD6: // DEC $??,X
                        MR_ZX(); DEC(); MW_ZP();
                        ADD_CYCLE(6);
                        break;
                    case 0xCE: // DEC $????
                        MR_AB(); DEC(); MW_EA();
                        ADD_CYCLE(6);
                        break;
                    case 0xDE: // DEC $????,X
                        MR_AX(); DEC(); MW_EA();
                        ADD_CYCLE(7);
                        break;

                    case 0xCA: // DEX
                        DEX();
                        ADD_CYCLE(2);
                        break;
                    case 0x88: // DEY
                        DEY();
                        ADD_CYCLE(2);
                        break;

                    case 0xE6: // INC $??
                        MR_ZP(); INC(); MW_ZP();
                        ADD_CYCLE(5);
                        break;
                    case 0xF6: // INC $??,X
                        MR_ZX(); INC(); MW_ZP();
                        ADD_CYCLE(6);
                        break;
                    case 0xEE: // INC $????
                        MR_AB(); INC(); MW_EA();
                        ADD_CYCLE(6);
                        break;
                    case 0xFE: // INC $????,X
                        MR_AX(); INC(); MW_EA();
                        ADD_CYCLE(7);
                        break;

                    case 0xE8: // INX
                        INX();
                        ADD_CYCLE(2);
                        break;
                    case 0xC8: // INY
                        INY();
                        ADD_CYCLE(2);
                        break;

                    case 0x29: // AND #$??
                        MR_IM(); AND();
                        ADD_CYCLE(2);
                        break;
                    case 0x25: // AND $??
                        MR_ZP(); AND();
                        ADD_CYCLE(3);
                        break;
                    case 0x35: // AND $??,X
                        MR_ZX(); AND();
                        ADD_CYCLE(4);
                        break;
                    case 0x2D: // AND $????
                        MR_AB(); AND();
                        ADD_CYCLE(4);
                        break;
                    case 0x3D: // AND $????,X
                        MR_AX(); AND(); CHECK_EA();
                        ADD_CYCLE(4);
                        break;
                    case 0x39: // AND $????,Y
                        MR_AY(); AND(); CHECK_EA();
                        ADD_CYCLE(4);
                        break;
                    case 0x21: // AND ($??,X)
                        MR_IX(); AND();
                        ADD_CYCLE(6);
                        break;
                    case 0x31: // AND ($??),Y
                        MR_IY(); AND(); CHECK_EA();
                        ADD_CYCLE(5);
                        break;

                    case 0x0A: // ASL A
                        ASL_A();
                        ADD_CYCLE(2);
                        break;
                    case 0x06: // ASL $??
                        MR_ZP(); ASL(); MW_ZP();
                        ADD_CYCLE(5);
                        break;
                    case 0x16: // ASL $??,X
                        MR_ZX(); ASL(); MW_ZP();
                        ADD_CYCLE(6);
                        break;
                    case 0x0E: // ASL $????
                        MR_AB(); ASL(); MW_EA();
                        ADD_CYCLE(6);
                        break;
                    case 0x1E: // ASL $????,X
                        MR_AX(); ASL(); MW_EA();
                        ADD_CYCLE(7);
                        break;

                    case 0x24: // BIT $??
                        MR_ZP(); BIT();
                        ADD_CYCLE(3);
                        break;
                    case 0x2C: // BIT $????
                        MR_AB(); BIT();
                        ADD_CYCLE(4);
                        break;

                    case 0x49: // EOR #$??
                        MR_IM(); EOR();
                        ADD_CYCLE(2);
                        break;
                    case 0x45: // EOR $??
                        MR_ZP(); EOR();
                        ADD_CYCLE(3);
                        break;
                    case 0x55: // EOR $??,X
                        MR_ZX(); EOR();
                        ADD_CYCLE(4);
                        break;
                    case 0x4D: // EOR $????
                        MR_AB(); EOR();
                        ADD_CYCLE(4);
                        break;
                    case 0x5D: // EOR $????,X
                        MR_AX(); EOR(); CHECK_EA();
                        ADD_CYCLE(4);
                        break;
                    case 0x59: // EOR $????,Y
                        MR_AY(); EOR(); CHECK_EA();
                        ADD_CYCLE(4);
                        break;
                    case 0x41: // EOR ($??,X)
                        MR_IX(); EOR();
                        ADD_CYCLE(6);
                        break;
                    case 0x51: // EOR ($??),Y
                        MR_IY(); EOR(); CHECK_EA();
                        ADD_CYCLE(5);
                        break;

                    case 0x4A: // LSR A
                        LSR_A();
                        ADD_CYCLE(2);
                        break;
                    case 0x46: // LSR $??
                        MR_ZP(); LSR(); MW_ZP();
                        ADD_CYCLE(5);
                        break;
                    case 0x56: // LSR $??,X
                        MR_ZX(); LSR(); MW_ZP();
                        ADD_CYCLE(6);
                        break;
                    case 0x4E: // LSR $????
                        MR_AB(); LSR(); MW_EA();
                        ADD_CYCLE(6);
                        break;
                    case 0x5E: // LSR $????,X
                        MR_AX(); LSR(); MW_EA();
                        ADD_CYCLE(7);
                        break;

                    case 0x09: // ORA #$??
                        MR_IM(); ORA();
                        ADD_CYCLE(2);
                        break;
                    case 0x05: // ORA $??
                        MR_ZP(); ORA();
                        ADD_CYCLE(3);
                        break;
                    case 0x15: // ORA $??,X
                        MR_ZX(); ORA();
                        ADD_CYCLE(4);
                        break;
                    case 0x0D: // ORA $????
                        MR_AB(); ORA();
                        ADD_CYCLE(4);
                        break;
                    case 0x1D: // ORA $????,X
                        MR_AX(); ORA(); CHECK_EA();
                        ADD_CYCLE(4);
                        break;
                    case 0x19: // ORA $????,Y
                        MR_AY(); ORA(); CHECK_EA();
                        ADD_CYCLE(4);
                        break;
                    case 0x01: // ORA ($??,X)
                        MR_IX(); ORA();
                        ADD_CYCLE(6);
                        break;
                    case 0x11: // ORA ($??),Y
                        MR_IY(); ORA(); CHECK_EA();
                        ADD_CYCLE(5);
                        break;

                    case 0x2A: // ROL A
                        ROL_A();
                        ADD_CYCLE(2);
                        break;
                    case 0x26: // ROL $??
                        MR_ZP(); ROL(); MW_ZP();
                        ADD_CYCLE(5);
                        break;
                    case 0x36: // ROL $??,X
                        MR_ZX(); ROL(); MW_ZP();
                        ADD_CYCLE(6);
                        break;
                    case 0x2E: // ROL $????
                        MR_AB(); ROL(); MW_EA();
                        ADD_CYCLE(6);
                        break;
                    case 0x3E: // ROL $????,X
                        MR_AX(); ROL(); MW_EA();
                        ADD_CYCLE(7);
                        break;

                    case 0x6A: // ROR A
                        ROR_A();
                        ADD_CYCLE(2);
                        break;
                    case 0x66: // ROR $??
                        MR_ZP(); ROR(); MW_ZP();
                        ADD_CYCLE(5);
                        break;
                    case 0x76: // ROR $??,X
                        MR_ZX(); ROR(); MW_ZP();
                        ADD_CYCLE(6);
                        break;
                    case 0x6E: // ROR $????
                        MR_AB(); ROR(); MW_EA();
                        ADD_CYCLE(6);
                        break;
                    case 0x7E: // ROR $????,X
                        MR_AX(); ROR(); MW_EA();
                        ADD_CYCLE(7);
                        break;

                    case 0xA9: // LDA #$??
                        MR_IM(); LDA();
                        ADD_CYCLE(2);
                        break;
                    case 0xA5: // LDA $??
                        MR_ZP(); LDA();
                        ADD_CYCLE(3);
                        break;
                    case 0xB5: // LDA $??,X
                        MR_ZX(); LDA();
                        ADD_CYCLE(4);
                        break;
                    case 0xAD: // LDA $????
                        MR_AB(); LDA();
                        ADD_CYCLE(4);
                        break;
                    case 0xBD: // LDA $????,X
                        MR_AX(); LDA(); CHECK_EA();
                        ADD_CYCLE(4);
                        break;
                    case 0xB9: // LDA $????,Y
                        MR_AY(); LDA(); CHECK_EA();
                        ADD_CYCLE(4);
                        break;
                    case 0xA1: // LDA ($??,X)
                        MR_IX(); LDA();
                        ADD_CYCLE(6);
                        break;
                    case 0xB1: // LDA ($??),Y
                        MR_IY(); LDA(); CHECK_EA();
                        ADD_CYCLE(5);
                        break;

                    case 0xA2: // LDX #$??
                        MR_IM(); LDX();
                        ADD_CYCLE(2);
                        break;
                    case 0xA6: // LDX $??
                        MR_ZP(); LDX();
                        ADD_CYCLE(3);
                        break;
                    case 0xB6: // LDX $??,Y
                        MR_ZY(); LDX();
                        ADD_CYCLE(4);
                        break;
                    case 0xAE: // LDX $????
                        MR_AB(); LDX();
                        ADD_CYCLE(4);
                        break;
                    case 0xBE: // LDX $????,Y
                        MR_AY(); LDX(); CHECK_EA();
                        ADD_CYCLE(4);
                        break;

                    case 0xA0: // LDY #$??
                        MR_IM(); LDY();
                        ADD_CYCLE(2);
                        break;
                    case 0xA4: // LDY $??
                        MR_ZP(); LDY();
                        ADD_CYCLE(3);
                        break;
                    case 0xB4: // LDY $??,X
                        MR_ZX(); LDY();
                        ADD_CYCLE(4);
                        break;
                    case 0xAC: // LDY $????
                        MR_AB(); LDY();
                        ADD_CYCLE(4);
                        break;
                    case 0xBC: // LDY $????,X
                        MR_AX(); LDY(); CHECK_EA();
                        ADD_CYCLE(4);
                        break;

                    case 0x85: // STA $??
                        EA_ZP(); STA(); MW_ZP();
                        ADD_CYCLE(3);
                        break;
                    case 0x95: // STA $??,X
                        EA_ZX(); STA(); MW_ZP();
                        ADD_CYCLE(4);
                        break;
                    case 0x8D: // STA $????
                        EA_AB(); STA(); MW_EA();
                        ADD_CYCLE(4);
                        break;
                    case 0x9D: // STA $????,X
                        EA_AX(); STA(); MW_EA();
                        ADD_CYCLE(5);
                        break;
                    case 0x99: // STA $????,Y
                        EA_AY(); STA(); MW_EA();
                        ADD_CYCLE(5);
                        break;
                    case 0x81: // STA ($??,X)
                        EA_IX(); STA(); MW_EA();
                        ADD_CYCLE(6);
                        break;
                    case 0x91: // STA ($??),Y
                        EA_IY(); STA(); MW_EA();
                        ADD_CYCLE(6);
                        break;

                    case 0x86: // STX $??
                        EA_ZP(); STX(); MW_ZP();
                        ADD_CYCLE(3);
                        break;
                    case 0x96: // STX $??,Y
                        EA_ZY(); STX(); MW_ZP();
                        ADD_CYCLE(4);
                        break;
                    case 0x8E: // STX $????
                        EA_AB(); STX(); MW_EA();
                        ADD_CYCLE(4);
                        break;

                    case 0x84: // STY $??
                        EA_ZP(); STY(); MW_ZP();
                        ADD_CYCLE(3);
                        break;
                    case 0x94: // STY $??,X
                        EA_ZX(); STY(); MW_ZP();
                        ADD_CYCLE(4);
                        break;
                    case 0x8C: // STY $????
                        EA_AB(); STY(); MW_EA();
                        ADD_CYCLE(4);
                        break;

                    case 0xAA: // TAX
                        TAX();
                        ADD_CYCLE(2);
                        break;
                    case 0x8A: // TXA
                        TXA();
                        ADD_CYCLE(2);
                        break;
                    case 0xA8: // TAY
                        TAY();
                        ADD_CYCLE(2);
                        break;
                    case 0x98: // TYA
                        TYA();
                        ADD_CYCLE(2);
                        break;
                    case 0xBA: // TSX
                        TSX();
                        ADD_CYCLE(2);
                        break;
                    case 0x9A: // TXS
                        TXS();
                        ADD_CYCLE(2);
                        break;

                    case 0xC9: // CMP #$??
                        MR_IM(); CMP_();
                        ADD_CYCLE(2);
                        break;
                    case 0xC5: // CMP $??
                        MR_ZP(); CMP_();
                        ADD_CYCLE(3);
                        break;
                    case 0xD5: // CMP $??,X
                        MR_ZX(); CMP_();
                        ADD_CYCLE(4);
                        break;
                    case 0xCD: // CMP $????
                        MR_AB(); CMP_();
                        ADD_CYCLE(4);
                        break;
                    case 0xDD: // CMP $????,X
                        MR_AX(); CMP_(); CHECK_EA();
                        ADD_CYCLE(4);
                        break;
                    case 0xD9: // CMP $????,Y
                        MR_AY(); CMP_(); CHECK_EA();
                        ADD_CYCLE(4);
                        break;
                    case 0xC1: // CMP ($??,X)
                        MR_IX(); CMP_();
                        ADD_CYCLE(6);
                        break;
                    case 0xD1: // CMP ($??),Y
                        MR_IY(); CMP_(); CHECK_EA();
                        ADD_CYCLE(5);
                        break;

                    case 0xE0: // CPX #$??
                        MR_IM(); CPX();
                        ADD_CYCLE(2);
                        break;
                    case 0xE4: // CPX $??
                        MR_ZP(); CPX();
                        ADD_CYCLE(3);
                        break;
                    case 0xEC: // CPX $????
                        MR_AB(); CPX();
                        ADD_CYCLE(4);
                        break;

                    case 0xC0: // CPY #$??
                        MR_IM(); CPY();
                        ADD_CYCLE(2);
                        break;
                    case 0xC4: // CPY $??
                        MR_ZP(); CPY();
                        ADD_CYCLE(3);
                        break;
                    case 0xCC: // CPY $????
                        MR_AB(); CPY();
                        ADD_CYCLE(4);
                        break;

                    case 0x90: // BCC
                        MR_IM(); BCC();
                        ADD_CYCLE(2);
                        break;
                    case 0xB0: // BCS
                        MR_IM(); BCS();
                        ADD_CYCLE(2);
                        break;
                    case 0xF0: // BEQ
                        MR_IM(); BEQ();
                        ADD_CYCLE(2);
                        break;
                    case 0x30: // BMI
                        MR_IM(); BMI();
                        ADD_CYCLE(2);
                        break;
                    case 0xD0: // BNE
                        MR_IM(); BNE();
                        ADD_CYCLE(2);
                        break;
                    case 0x10: // BPL
                        MR_IM(); BPL();
                        ADD_CYCLE(2);
                        break;
                    case 0x50: // BVC
                        MR_IM(); BVC();
                        ADD_CYCLE(2);
                        break;
                    case 0x70: // BVS
                        MR_IM(); BVS();
                        ADD_CYCLE(2);
                        break;

                    case 0x4C: // JMP $????
                        JMP();
                        ADD_CYCLE(3);
                        break;
                    case 0x6C: // JMP ($????)
                        JMP_ID();
                        ADD_CYCLE(5);
                        break;

                    case 0x20: // JSR
                        JSR();
                        ADD_CYCLE(6);
                        break;

                    case 0x40: // RTI
                        RTI();
                        ADD_CYCLE(6);
                        break;
                    case 0x60: // RTS
                        RTS();
                        ADD_CYCLE(6);
                        break;

                    // フラグ制御系
                    case 0x18: // CLC
                        CLC();
                        ADD_CYCLE(2);
                        break;
                    case 0xD8: // CLD
                        CLD();
                        ADD_CYCLE(2);
                        break;
                    case 0x58: // CLI
                        CLI();
                        ADD_CYCLE(2);
                        break;
                    case 0xB8: // CLV
                        CLV();
                        ADD_CYCLE(2);
                        break;

                    case 0x38: // SEC
                        SEC();
                        ADD_CYCLE(2);
                        break;
                    case 0xF8: // SED
                        SED();
                        ADD_CYCLE(2);
                        break;
                    case 0x78: // SEI
                        SEI();
                        ADD_CYCLE(2);
                        break;

                    // スタック系
                    case 0x48: // PHA
                        PUSH(R.A);
                        ADD_CYCLE(3);
                        break;
                    case 0x08: // PHP
                        PUSH((byte)(R.P | B_FLAG));
                        ADD_CYCLE(3);
                        break;
                    case 0x68: // PLA (N-----Z-)
                        R.A = POP();
                        SET_ZN_FLAG(R.A);
                        ADD_CYCLE(4);
                        break;
                    case 0x28: // PLP
                        R.P = (byte)(POP() | R_FLAG);
                        ADD_CYCLE(4);
                        break;

                    // その他
                    case 0x00: // BRK
                        BRK();
                        ADD_CYCLE(7);
                        break;

                    case 0xEA: // NOP
                        ADD_CYCLE(2);
                        break;

                    // 未公開命令群
                    case 0x0B: // ANC #$??
                    case 0x2B: // ANC #$??
                        MR_IM(); ANC();
                        ADD_CYCLE(2);
                        break;

                    case 0x8B: // ANE #$??
                        MR_IM(); ANE();
                        ADD_CYCLE(2);
                        break;

                    case 0x6B: // ARR #$??
                        MR_IM(); ARR();
                        ADD_CYCLE(2);
                        break;

                    case 0x4B: // ASR #$??
                        MR_IM(); ASR();
                        ADD_CYCLE(2);
                        break;

                    case 0xC7: // DCP $??
                        MR_ZP(); DCP(); MW_ZP();
                        ADD_CYCLE(5);
                        break;
                    case 0xD7: // DCP $??,X
                        MR_ZX(); DCP(); MW_ZP();
                        ADD_CYCLE(6);
                        break;
                    case 0xCF: // DCP $????
                        MR_AB(); DCP(); MW_EA();
                        ADD_CYCLE(6);
                        break;
                    case 0xDF: // DCP $????,X
                        MR_AX(); DCP(); MW_EA();
                        ADD_CYCLE(7);
                        break;
                    case 0xDB: // DCP $????,Y
                        MR_AY(); DCP(); MW_EA();
                        ADD_CYCLE(7);
                        break;
                    case 0xC3: // DCP ($??,X)
                        MR_IX(); DCP(); MW_EA();
                        ADD_CYCLE(8);
                        break;
                    case 0xD3: // DCP ($??),Y
                        MR_IY(); DCP(); MW_EA();
                        ADD_CYCLE(8);
                        break;

                    case 0xE7: // ISB $??
                        MR_ZP(); ISB(); MW_ZP();
                        ADD_CYCLE(5);
                        break;
                    case 0xF7: // ISB $??,X
                        MR_ZX(); ISB(); MW_ZP();
                        ADD_CYCLE(5);
                        break;
                    case 0xEF: // ISB $????
                        MR_AB(); ISB(); MW_EA();
                        ADD_CYCLE(5);
                        break;
                    case 0xFF: // ISB $????,X
                        MR_AX(); ISB(); MW_EA();
                        ADD_CYCLE(5);
                        break;
                    case 0xFB: // ISB $????,Y
                        MR_AY(); ISB(); MW_EA();
                        ADD_CYCLE(5);
                        break;
                    case 0xE3: // ISB ($??,X)
                        MR_IX(); ISB(); MW_EA();
                        ADD_CYCLE(5);
                        break;
                    case 0xF3: // ISB ($??),Y
                        MR_IY(); ISB(); MW_EA();
                        ADD_CYCLE(5);
                        break;

                    case 0xBB: // LAS $????,Y
                        MR_AY(); LAS(); CHECK_EA();
                        ADD_CYCLE(4);
                        break;


                    case 0xA7: // LAX $??
                        MR_ZP(); LAX();
                        ADD_CYCLE(3);
                        break;
                    case 0xB7: // LAX $??,Y
                        MR_ZY(); LAX();
                        ADD_CYCLE(4);
                        break;
                    case 0xAF: // LAX $????
                        MR_AB(); LAX();
                        ADD_CYCLE(4);
                        break;
                    case 0xBF: // LAX $????,Y
                        MR_AY(); LAX(); CHECK_EA();
                        ADD_CYCLE(4);
                        break;
                    case 0xA3: // LAX ($??,X)
                        MR_IX(); LAX();
                        ADD_CYCLE(6);
                        break;
                    case 0xB3: // LAX ($??),Y
                        MR_IY(); LAX(); CHECK_EA();
                        ADD_CYCLE(5);
                        break;

                    case 0xAB: // LXA #$??
                        MR_IM(); LXA();
                        ADD_CYCLE(2);
                        break;

                    case 0x27: // RLA $??
                        MR_ZP(); RLA(); MW_ZP();
                        ADD_CYCLE(5);
                        break;
                    case 0x37: // RLA $??,X
                        MR_ZX(); RLA(); MW_ZP();
                        ADD_CYCLE(6);
                        break;
                    case 0x2F: // RLA $????
                        MR_AB(); RLA(); MW_EA();
                        ADD_CYCLE(6);
                        break;
                    case 0x3F: // RLA $????,X
                        MR_AX(); RLA(); MW_EA();
                        ADD_CYCLE(7);
                        break;
                    case 0x3B: // RLA $????,Y
                        MR_AY(); RLA(); MW_EA();
                        ADD_CYCLE(7);
                        break;
                    case 0x23: // RLA ($??,X)
                        MR_IX(); RLA(); MW_EA();
                        ADD_CYCLE(8);
                        break;
                    case 0x33: // RLA ($??),Y
                        MR_IY(); RLA(); MW_EA();
                        ADD_CYCLE(8);
                        break;

                    case 0x67: // RRA $??
                        MR_ZP(); RRA(); MW_ZP();
                        ADD_CYCLE(5);
                        break;
                    case 0x77: // RRA $??,X
                        MR_ZX(); RRA(); MW_ZP();
                        ADD_CYCLE(6);
                        break;
                    case 0x6F: // RRA $????
                        MR_AB(); RRA(); MW_EA();
                        ADD_CYCLE(6);
                        break;
                    case 0x7F: // RRA $????,X
                        MR_AX(); RRA(); MW_EA();
                        ADD_CYCLE(7);
                        break;
                    case 0x7B: // RRA $????,Y
                        MR_AY(); RRA(); MW_EA();
                        ADD_CYCLE(7);
                        break;
                    case 0x63: // RRA ($??,X)
                        MR_IX(); RRA(); MW_EA();
                        ADD_CYCLE(8);
                        break;
                    case 0x73: // RRA ($??),Y
                        MR_IY(); RRA(); MW_EA();
                        ADD_CYCLE(8);
                        break;

                    case 0x87: // SAX $??
                        MR_ZP(); SAX(); MW_ZP();
                        ADD_CYCLE(3);
                        break;
                    case 0x97: // SAX $??,Y
                        MR_ZY(); SAX(); MW_ZP();
                        ADD_CYCLE(4);
                        break;
                    case 0x8F: // SAX $????
                        MR_AB(); SAX(); MW_EA();
                        ADD_CYCLE(4);
                        break;
                    case 0x83: // SAX ($??,X)
                        MR_IX(); SAX(); MW_EA();
                        ADD_CYCLE(6);
                        break;

                    case 0xCB: // SBX #$??
                        MR_IM(); SBX();
                        ADD_CYCLE(2);
                        break;

                    case 0x9F: // SHA $????,Y
                        MR_AY(); SHA(); MW_EA();
                        ADD_CYCLE(5);
                        break;
                    case 0x93: // SHA ($??),Y
                        MR_IY(); SHA(); MW_EA();
                        ADD_CYCLE(6);
                        break;

                    case 0x9B: // SHS $????,Y
                        MR_AY(); SHS(); MW_EA();
                        ADD_CYCLE(5);
                        break;

                    case 0x9E: // SHX $????,Y
                        MR_AY(); SHX(); MW_EA();
                        ADD_CYCLE(5);
                        break;

                    case 0x9C: // SHY $????,X
                        MR_AX(); SHY(); MW_EA();
                        ADD_CYCLE(5);
                        break;

                    case 0x07: // SLO $??
                        MR_ZP(); SLO(); MW_ZP();
                        ADD_CYCLE(5);
                        break;
                    case 0x17: // SLO $??,X
                        MR_ZX(); SLO(); MW_ZP();
                        ADD_CYCLE(6);
                        break;
                    case 0x0F: // SLO $????
                        MR_AB(); SLO(); MW_EA();
                        ADD_CYCLE(6);
                        break;
                    case 0x1F: // SLO $????,X
                        MR_AX(); SLO(); MW_EA();
                        ADD_CYCLE(7);
                        break;
                    case 0x1B: // SLO $????,Y
                        MR_AY(); SLO(); MW_EA();
                        ADD_CYCLE(7);
                        break;
                    case 0x03: // SLO ($??,X)
                        MR_IX(); SLO(); MW_EA();
                        ADD_CYCLE(8);
                        break;
                    case 0x13: // SLO ($??),Y
                        MR_IY(); SLO(); MW_EA();
                        ADD_CYCLE(8);
                        break;

                    case 0x47: // SRE $??
                        MR_ZP(); SRE(); MW_ZP();
                        ADD_CYCLE(5);
                        break;
                    case 0x57: // SRE $??,X
                        MR_ZX(); SRE(); MW_ZP();
                        ADD_CYCLE(6);
                        break;
                    case 0x4F: // SRE $????
                        MR_AB(); SRE(); MW_EA();
                        ADD_CYCLE(6);
                        break;
                    case 0x5F: // SRE $????,X
                        MR_AX(); SRE(); MW_EA();
                        ADD_CYCLE(7);
                        break;
                    case 0x5B: // SRE $????,Y
                        MR_AY(); SRE(); MW_EA();
                        ADD_CYCLE(7);
                        break;
                    case 0x43: // SRE ($??,X)
                        MR_IX(); SRE(); MW_EA();
                        ADD_CYCLE(8);
                        break;
                    case 0x53: // SRE ($??),Y
                        MR_IY(); SRE(); MW_EA();
                        ADD_CYCLE(8);
                        break;

                    case 0xEB: // SBC #$?? (Unofficial)
                        MR_IM(); SBC();
                        ADD_CYCLE(2);
                        break;

                    case 0x1A: // NOP (Unofficial)
                    case 0x3A: // NOP (Unofficial)
                    case 0x5A: // NOP (Unofficial)
                    case 0x7A: // NOP (Unofficial)
                    case 0xDA: // NOP (Unofficial)
                    case 0xFA: // NOP (Unofficial)
                        ADD_CYCLE(2);
                        break;
                    case 0x80: // DOP (CYCLES 2)
                    case 0x82: // DOP (CYCLES 2)
                    case 0x89: // DOP (CYCLES 2)
                    case 0xC2: // DOP (CYCLES 2)
                    case 0xE2: // DOP (CYCLES 2)
                        R.PC++;
                        ADD_CYCLE(2);
                        break;
                    case 0x04: // DOP (CYCLES 3)
                    case 0x44: // DOP (CYCLES 3)
                    case 0x64: // DOP (CYCLES 3)
                        R.PC++;
                        ADD_CYCLE(3);
                        break;
                    case 0x14: // DOP (CYCLES 4)
                    case 0x34: // DOP (CYCLES 4)
                    case 0x54: // DOP (CYCLES 4)
                    case 0x74: // DOP (CYCLES 4)
                    case 0xD4: // DOP (CYCLES 4)
                    case 0xF4: // DOP (CYCLES 4)
                        R.PC++;
                        ADD_CYCLE(4);
                        break;
                    case 0x0C: // TOP
                    case 0x1C: // TOP
                    case 0x3C: // TOP
                    case 0x5C: // TOP
                    case 0x7C: // TOP
                    case 0xDC: // TOP
                    case 0xFC: // TOP
                        R.PC += 2;
                        ADD_CYCLE(4);
                        break;

                    case 0x02:  /* JAM */
                    case 0x12:  /* JAM */
                    case 0x22:  /* JAM */
                    case 0x32:  /* JAM */
                    case 0x42:  /* JAM */
                    case 0x52:  /* JAM */
                    case 0x62:  /* JAM */
                    case 0x72:  /* JAM */
                    case 0x92:  /* JAM */
                    case 0xB2:  /* JAM */
                    case 0xD2:  /* JAM */
                    case 0xF2:  /* JAM */
                    default:
                        if (!Supporter.Config.emulator.bIllegalOp)
                        {
                            throw new Exception("IllegalOp");
                        }
                        else
                        {
                            R.PC--;
                            ADD_CYCLE(4);
                        }
                        break;
                        //			default:
                        //				__assume(0);
                }

                if (nmi_request != 0)
                {
                    _NMI();
                }
                else
                if (irq_request != 0)
                {
                    _IRQ();
                }

                request_cycles -= exec_cycles;
                TOTAL_cycles += exec_cycles;

                mapper.Clock(exec_cycles);
#if DPCM_SYNCCLOCK
		        apu->SyncDPCM( exec_cycles );
#endif
                if (bClockProcess)
                {
                    nes.Clock(exec_cycles);
                }
            }
        _execute_exit:
#if !DPCM_SYNCCLOCK
            apu.SyncDPCM(TOTAL_cycles - OLD_cycles);
#endif
            return TOTAL_cycles - OLD_cycles;
        }
        private void _IRQ()
        {
            PUSH((byte)(R.PC >> 8));
            PUSH((byte)(R.PC & 0xFF));
            CLR_FLAG(B_FLAG);
            PUSH(R.P);
            SET_FLAG(I_FLAG);
            R.PC = RD6502W(IRQ_VECTOR);
            exec_cycles += 7;
        }


        private ushort RD6502W(ushort addr)
        {
            if (addr < 0x2000)
            {
                // RAM (Mirror $0800, $1000, $1800)
                return BitConverter.ToUInt16(MMU.RAM, addr & 0x07FF);
            }
            else if (addr < 0x8000)
            {
                // Others
                return (ushort)(nes.Read(addr) + nes.Read((ushort)(addr + 1)) * 0x100);
            }

            var temp = MMU.CPU_MEM_BANK[addr >> 13];
            shortTemp[0] = temp[addr & 0x1FFF];
            shortTemp[1] = temp[(addr & 0x1FFF) + 1];
            return BitConverter.ToUInt16(shortTemp, 0);
        }

        private void SET_FLAG(byte V)
        {
            R.P |= (V);
        }

        private void CLR_FLAG(byte V)
        {
            var temp = (byte)(~V);
            R.P &= temp;
        }

        private void _NMI()
        {
            PUSH((byte)(R.PC >> 8));
            PUSH((byte)(R.PC & 0xFF));
            CLR_FLAG(B_FLAG);
            PUSH(R.P);
            SET_FLAG(I_FLAG);
            R.PC = RD6502W(NMI_VECTOR);
            exec_cycles += 7;
        }

        private void SRE()
        {
            TST_FLAG((DT & 0x01) != 0, C_FLAG);
            DT >>= 1;
            R.A ^= DT;
            SET_ZN_FLAG(R.A);
        }

        private void SLO()
        {
            TST_FLAG((DT & 0x80) != 0, C_FLAG);
            DT <<= 1;
            R.A |= DT;
            SET_ZN_FLAG(R.A);
        }

        private void SHY()
        {
            DT = (byte)(R.Y & ((EA >> 8) + 1));
        }

        private void SHX()
        {
            DT = (byte)(R.X & ((EA >> 8) + 1));
        }

        private void SHS()
        {
            R.S = (byte)(R.A & R.X);
            DT = (byte)(R.S & ((EA >> 8) + 1));
        }

        private void SHA()
        {
            DT = (byte)(R.A & R.X & ((EA >> 8) + 1));
        }

        private void SBX()
        {
            WT = (ushort)((R.A & R.X) - DT);
            TST_FLAG(WT < 0x100, C_FLAG);
            R.X = (byte)(WT & 0xFF);
            SET_ZN_FLAG(R.X);
        }

        private void SAX()
        {
            DT = (byte)(R.A & R.X);
        }

        private void RRA()
        {
            if ((R.P & C_FLAG) != 0)
            {
                TST_FLAG((DT & 0x01) != 0, C_FLAG);
                DT = (byte)((DT >> 1) | 0x80);
            }
            else
            {
                TST_FLAG((DT & 0x01) != 0, C_FLAG);
                DT >>= 1;
            }
            ADC();
        }

        private void RLA()
        {
            if ((R.P & C_FLAG) != 0)
            {
                TST_FLAG((DT & 0x80) != 0, C_FLAG);
                DT = (byte)((DT << 1) | 1);
            }
            else
            {
                TST_FLAG((DT & 0x80) != 0, C_FLAG);
                DT <<= 1;
            }
            R.A &= DT;
            SET_ZN_FLAG(R.A);
        }

        private void LXA()
        {
            R.A = R.X = (byte)((R.A | 0xEE) & DT);
            SET_ZN_FLAG(R.A);
        }

        private void LAX()
        {
            R.A = DT;
            R.X = R.A;
            SET_ZN_FLAG(R.A);
        }

        private void LAS()
        {
            R.A = R.X = R.S = (byte)(R.S & DT);
            SET_ZN_FLAG(R.A);
        }

        private void ISB()
        {
            DT++;
            SBC();
        }

        private void DCP()
        {
            DT--;
            CMP_();
        }

        private void ASR()
        {
            DT &= R.A;
            TST_FLAG((DT & 0x01) != 0, C_FLAG);
            R.A = (byte)(DT >> 1);
            SET_ZN_FLAG(R.A);
        }

        private void ARR()
        {
            DT &= R.A;
            R.A = (byte)((DT >> 1) | ((R.P & C_FLAG) << 7));
            SET_ZN_FLAG(R.A);
            TST_FLAG((R.A & 0x40) != 0, C_FLAG);
            TST_FLAG(((R.A >> 6) ^ (R.A >> 5)) != 0, V_FLAG);
        }

        private void ANE()
        {
            R.A = (byte)((R.A | 0xEE) & R.X & DT);
            SET_ZN_FLAG(R.A);
        }

        private void ANC()
        {
            R.A &= DT;
            SET_ZN_FLAG(R.A);
            TST_FLAG((R.P & N_FLAG) != 0, C_FLAG);
        }

        private void BRK()
        {
            R.PC++;
            PUSH((byte)(R.PC >> 8));
            PUSH((byte)(R.PC & 0xFF));
            SET_FLAG(B_FLAG);
            PUSH(R.P);
            SET_FLAG(I_FLAG);
            R.PC = RD6502W(IRQ_VECTOR);
        }

        private byte POP()
        {
            return STACK[(++R.S) & 0xFF];
        }

        private void PUSH(byte V)
        {
            STACK[(R.S--) & 0xFF] = V;
        }

        private void SEI()
        {
            R.P |= I_FLAG;
        }

        private void SED()
        {
            R.P |= D_FLAG;
        }

        private void SEC()
        {
            R.P |= C_FLAG;
        }

        private void CLV()
        {
            var temp = unchecked((byte)(~V_FLAG));
            R.P &= temp;
        }

        private void CLI()
        {
            var temp = unchecked((byte)(~I_FLAG));
            R.P &= temp;
        }

        private void CLD()
        {
            var temp = unchecked((byte)(~D_FLAG));
            R.P &= temp;
        }

        private void CLC()
        {
            var temp = unchecked((byte)(~C_FLAG));
            R.P &= temp;
        }

        private void RTS()
        {
            R.PC = POP();
            R.PC |= (ushort)(POP() * 0x0100);
            R.PC++;
        }

        private void RTI()
        {
            R.P = (byte)(POP() | R_FLAG);
            R.PC = POP();
            R.PC |= (ushort)(POP() * 0x0100);
        }

        private void JSR()
        {
            EA = OP6502W(R.PC);
            R.PC++;
            PUSH((byte)(R.PC >> 8));
            PUSH((byte)(R.PC & 0xFF));
            R.PC = EA;
        }

        private void JMP_ID()
        {
            WT = OP6502W(R.PC);
            EA = RD6502(WT);
            WT = (ushort)((WT & 0xFF00) | ((WT + 1) & 0x00FF));
            R.PC = (ushort)(EA + RD6502(WT) * 0x100);
        }

        private void JMP()
        {
            R.PC = OP6502W(R.PC);
        }

        private void BVS()
        {
            if ((R.P & V_FLAG) != 0)
                REL_JUMP();
        }

        private void REL_JUMP()
        {
            ET = R.PC;
            EA = (ushort)(R.PC + (sbyte)DT);
            R.PC = EA;
            ADD_CYCLE(1);
            CHECK_EA();
        }

        private void BVC()
        {
            if ((R.P & V_FLAG) == 0) REL_JUMP();
        }

        private void BPL()
        {
            if ((R.P & N_FLAG) == 0) REL_JUMP();
        }

        private void BNE()
        {
            if ((R.P & Z_FLAG) == 0) REL_JUMP();
        }

        private void BMI()
        {
            if ((R.P & N_FLAG) != 0) REL_JUMP();
        }

        private void BEQ()
        {
            if ((R.P & Z_FLAG) != 0) REL_JUMP();
        }

        private void BCS()
        {
            if ((R.P & C_FLAG) != 0) REL_JUMP();
        }

        private void BCC()
        {
            if ((R.P & C_FLAG) == 0) REL_JUMP();
        }

        private void CPY()
        {
            WT = (ushort)(R.Y - DT);
            TST_FLAG((WT & 0x8000) == 0, C_FLAG);
            SET_ZN_FLAG((byte)WT);
        }

        private void CPX()
        {
            WT = (ushort)(R.X - DT);
            TST_FLAG((WT & 0x8000) == 0, C_FLAG);
            SET_ZN_FLAG((byte)WT);
        }

        private void CMP_()
        {
            WT = (ushort)(R.A - DT);
            TST_FLAG((WT & 0x8000) == 0, C_FLAG);
            SET_ZN_FLAG((byte)WT);
        }

        private void TXS()
        {
            R.S = R.X;
        }

        private void TSX()
        {
            R.X = R.S; SET_ZN_FLAG(R.X);
        }

        private void TYA()
        {
            R.A = R.Y; SET_ZN_FLAG(R.A);
        }

        private void TAY()
        {
            R.Y = R.A; SET_ZN_FLAG(R.Y);
        }

        private void TXA()
        {
            R.A = R.X; SET_ZN_FLAG(R.A);
        }

        private void TAX()
        {
            R.X = R.A; SET_ZN_FLAG(R.X);
        }

        private void STY()
        {
            DT = R.Y;
        }

        private void EA_ZY()
        {
            DT = OP6502(R.PC++);
            EA = (byte)(DT + R.Y);
        }

        private void STX()
        {
            DT = R.X;
        }

        private void EA_IY()
        {
            DT = OP6502(R.PC++);
            ET = ZPRDW(DT);
            EA = (ushort)(ET + R.Y);
        }

        private void EA_IX()
        {
            DT = OP6502(R.PC++);
            EA = ZPRDW(DT + R.X);
        }

        private void EA_AY()
        {
            ET = OP6502W(R.PC);
            R.PC += 2;
            EA = (ushort)(ET + R.Y);
        }

        private void EA_AX()
        {
            ET = OP6502W(R.PC);
            R.PC += 2;
            EA = (ushort)(ET + R.X);
        }

        private void EA_AB()
        {
            EA = OP6502W(R.PC);
            R.PC += 2;
        }

        private void EA_ZX()
        {
            DT = OP6502(R.PC++);
            EA = (byte)(DT + R.X);
        }

        private void STA()
        {
            DT = R.A;
        }

        private void EA_ZP()
        {
            EA = OP6502(R.PC++);
        }

        private void LDY()
        {
            R.Y = DT; SET_ZN_FLAG(R.Y);
        }

        private void MR_ZY()
        {
            DT = OP6502(R.PC++);
            EA = (byte)(DT + R.Y);
            DT = ZPRD(EA);
        }

        private void LDX()
        {
            R.X = DT; SET_ZN_FLAG(R.X);
        }

        private void LDA()
        {
            R.A = DT; SET_ZN_FLAG(R.A);
        }

        private void ROR()
        {
            if ((R.P & C_FLAG) != 0)
            {
                TST_FLAG((DT & 0x01) != 0, C_FLAG);
                DT = (byte)((DT >> 1) | 0x80);
            }
            else
            {
                TST_FLAG((DT & 0x01) != 0, C_FLAG);
                DT >>= 1;
            }
            SET_ZN_FLAG(DT);
        }

        private void ROR_A()
        {
            if ((R.P & C_FLAG) != 0)
            {
                TST_FLAG((R.A & 0x01) != 0, C_FLAG);
                R.A = (byte)((R.A >> 1) | 0x80);
            }
            else
            {
                TST_FLAG((R.A & 0x01) != 0, C_FLAG);
                R.A >>= 1;
            }
            SET_ZN_FLAG(R.A);
        }

        private void ROL()
        {
            if ((R.P & C_FLAG) != 0)
            {
                TST_FLAG((DT & 0x80) != 0, C_FLAG);
                DT = (byte)((DT << 1) | 0x01);
            }
            else
            {
                TST_FLAG((DT & 0x80) != 0, C_FLAG);
                DT <<= 1;
            }
            SET_ZN_FLAG(DT);
        }

        private void ROL_A()
        {
            if ((R.P & C_FLAG) != 0)
            {
                TST_FLAG((R.A & 0x80) != 0, C_FLAG);
                R.A = (byte)((R.A << 1) | 0x01);
            }
            else
            {
                TST_FLAG((R.A & 0x80) != 0, C_FLAG);
                R.A <<= 1;
            }
            SET_ZN_FLAG(R.A);
        }

        private void ORA()
        {
            R.A |= DT;
            SET_ZN_FLAG(R.A);
        }

        private void LSR_A()
        {
            TST_FLAG((R.A & 0x01) != 0, C_FLAG);
            R.A >>= 1;
            SET_ZN_FLAG(R.A);
        }

        private void LSR()
        {
            TST_FLAG((DT & 0x01) != 0, C_FLAG);
            DT >>= 1;
            SET_ZN_FLAG(DT);
        }

        private void EOR()
        {
            R.A ^= DT;
            SET_ZN_FLAG(R.A);
        }

        internal void SetClockProcess(bool bEnable)
        {
            m_bClockProcess = bEnable;
        }

        internal byte OP6502(ushort addr)
        {
            return MMU.CPU_MEM_BANK[addr >> 13][addr & 0x1FFF];
        }

        private byte[] shortTemp = new byte[2];
        internal ushort OP6502W(ushort addr)
        {
            var bytePage = MMU.CPU_MEM_BANK[addr >> 13];
            var spanByte = bytePage;
            shortTemp[0] = spanByte[addr & 0x1FFF];
            shortTemp[1] = spanByte[(addr & 0x1FFF) + 1];
            return BitConverter.ToUInt16(shortTemp, 0);
        }

        internal byte RD6502(ushort addr)
        {
            if (addr < 0x2000)
            {
                // RAM (Mirror $0800, $1000, $1800)
                return MMU.RAM[addr & 0x07FF];
            }
            else if (addr < 0x8000)
            {
                // Others
                return nes.Read(addr);
            }
            else
            {
                // Dummy access
                mapper.Read(addr, MMU.CPU_MEM_BANK[addr >> 13][addr & 0x1FFF]);
            }

            // Quick bank read
            return MMU.CPU_MEM_BANK[addr >> 13][addr & 0x1FFF];
        }

        private void AND()
        {
            R.A &= DT;
            SET_ZN_FLAG(R.A);
        }

        private void MR_IM()
        {
            DT = OP6502(R.PC++);
        }

        private void BIT()
        {
            TST_FLAG((DT & R.A) == 0, Z_FLAG);
            TST_FLAG((DT & 0x80) != 0, N_FLAG);
            TST_FLAG((DT & 0x40) != 0, V_FLAG);
        }

        private void MR_ZP()
        {
            EA = OP6502(R.PC++);
            DT = ZPRD(EA);
        }

        private byte ZPRD(ushort A)
        {
            return MMU.RAM[A];
        }

        private ushort ZPRDW(int A)
        {
            ushort ram1 = MMU.RAM[A];
            ushort ram2 = MMU.RAM[A + 1];
            ram2 <<= 8;
            return (ushort)(ram1 + ram2);
        }

        private void ADC()
        {
            WT = (ushort)(R.A + DT + (R.P & C_FLAG));
            TST_FLAG(WT > 0xFF, C_FLAG);
            var temp = ((~(R.A ^ DT)) & (R.A ^ WT) & 0x80);
            TST_FLAG(temp != 0, V_FLAG);
            R.A = (byte)WT;
            SET_ZN_FLAG(R.A);
        }

        private void TST_FLAG(bool F, byte V)
        {
            byte temp = (byte)~V;
            R.P &= temp;

            if (F) R.P |= V;
        }

        private void SET_ZN_FLAG(byte A)
        {
            byte temp = unchecked((byte)(~(Z_FLAG | N_FLAG)));
            R.P &= temp;
            R.P |= ZN_Table[A];
        }

        private void ADD_CYCLE(int V)
        {
            exec_cycles += V;
        }

        private void MR_ZX()
        {
            DT = OP6502(R.PC++);
            EA = (ushort)(DT + R.X);
            DT = ZPRD(EA);
        }

        private void MR_AB()
        {
            EA = OP6502W(R.PC);
            R.PC += 2;
            DT = RD6502(EA);
        }

        private void MR_AX()
        {
            ET = OP6502W(R.PC);
            R.PC += 2;
            EA = (ushort)(ET + R.X);
            DT = RD6502(EA);
        }

        private void CHECK_EA()
        {
            if ((ET & 0xFF00) != (EA & 0xFF00)) ADD_CYCLE(1);
        }

        private void MR_AY()
        {
            ET = OP6502W(R.PC);
            R.PC += 2;
            EA = (ushort)(ET + R.Y);
            DT = RD6502(EA);
        }

        private void MR_IX()
        {
            DT = OP6502(R.PC++);
            EA = ZPRDW(DT + R.X);
            DT = RD6502(EA);
        }

        private void MR_IY()
        {
            DT = OP6502(R.PC++);
            ET = ZPRDW(DT);
            EA = (ushort)(ET + R.Y);
            DT = RD6502(EA);
        }
        private void ASL_A()
        {
            TST_FLAG((R.A & 0x80) != 0, C_FLAG);
            R.A <<= 1;
            SET_ZN_FLAG(R.A);
        }

        private void ASL()
        {
            TST_FLAG((DT & 0x80) != 0, C_FLAG);
            DT <<= 1;
            SET_ZN_FLAG(DT);
        }

        private void SBC()
        {
            WT = (ushort)(R.A - DT - (~R.P & C_FLAG));
            bool f = ((R.A ^ DT) & (R.A ^ WT) & (0x80)) != 0;
            TST_FLAG(f, V_FLAG);
            TST_FLAG(WT < 0x100, C_FLAG);
            R.A = (byte)WT;
            SET_ZN_FLAG(R.A);
        }

        private void DEC()
        {
            DT--;
            SET_ZN_FLAG(DT);
        }

        private void DEX()
        {
            R.X--;
            SET_ZN_FLAG(R.X);
        }

        private void DEY()
        {
            R.Y--;
            SET_ZN_FLAG(R.Y);
        }

        private void INC()
        {
            DT++;
            SET_ZN_FLAG(DT);
        }

        private void INX()
        {
            R.X++;
            SET_ZN_FLAG(R.X);
        }

        private void INY()
        {
            R.Y++;
            SET_ZN_FLAG(R.Y);
        }

        private void MW_ZP()
        {
            ZPWR(EA, DT);
        }

        private void ZPWR(ushort a, byte v)
        {
            MMU.RAM[a] = v;
        }

        private void MW_EA()
        {
            WR6502(EA, DT);
        }

        internal void ClrIRQ(byte mask)
        {
            byte temp = (byte)~mask;
            R.INT_pending &= temp;
        }

        internal void WR6502(ushort addr, byte data)
        {
            if (addr < 0x2000)
            {
                // RAM (Mirror $0800, $1000, $1800)
                MMU.RAM[addr & 0x07FF] = data;
            }
            else
            {
                // Others
                nes.Write(addr, data);
            }
        }

        internal void NMI()
        {
            R.INT_pending |= NMI_FLAG;
            nmicount = 0;
        }

        internal void SetIRQ(byte mask)
        {
            R.INT_pending |= mask;
        }

        internal int GetTotalCycles()
        {
            return TOTAL_cycles;
        }

        internal void DMA(int cycles)
        {
            DMA_cycles += cycles;
        }

        internal void Reset()
        {
            apu = nes.apu;
            mapper = nes.mapper;

            R.A = 0x00;
            R.X = 0x00;
            R.Y = 0x00;
            R.S = 0xFF;
            R.P = Z_FLAG | R_FLAG;
            R.PC = RD6502W(RES_VECTOR);

            R.INT_pending = 0;

            TOTAL_cycles = 0;
            DMA_cycles = 0;

            // STACK quick access
            STACK = new ArrayRef<byte>(MMU.RAM, 0x0100, MMU.RAM.Length - 0x100);

            // Zero/Negative FLAG
            ZN_Table[0] = Z_FLAG;
            for (int i = 1; i < 256; i++)
                ZN_Table[i] = (byte)((i & 0x80) != 0 ? N_FLAG : 0);
        }

        internal void GetContext(ref R6502 r)
        {
            r = R;
        }

        internal int GetDmaCycles()
        {
            return DMA_cycles;
        }
    }

    public enum StatusFlag6502 : int
    {
        C_FLAG = 0x01,
        Z_FLAG = 0x02,
        I_FLAG = 0x04,
        D_FLAG = 0x08,
        B_FLAG = 0x10,
        R_FLAG = 0x20,
        V_FLAG = 0x40,
        N_FLAG = 0x80
    }

    public enum Interrupt : int
    {
        NMI_FLAG = 0x01,
        IRQ_FLAG = 0x02,
        IRQ_FRAMEIRQ = 0x04,
        IRQ_DPCM = 0x08,
        IRQ_MAPPER = 0x10,
        IRQ_MAPPER2 = 0x20,
        IRQ_TRIGGER = 0x40,
        IRQ_TRIGGER2 = 0x80,
        IRQ_MASK = (~(NMI_FLAG | IRQ_FLAG)),
    }

    public enum Vector : int
    {
        NMI_VECTOR = 0xFFFA,
        RES_VECTOR = 0xFFFC,
        IRQ_VECTOR = 0xFFFE
    }

    public class R6502
    {
        public ushort PC;
        public byte A;
        public byte P;
        public byte X;
        public byte Y;
        public byte S;

        public byte INT_pending;
    }
}