#undef DPCM_SYNCCLOCK

using System;
using UnityEngine.UI;

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
        private R6502 R = new R6502();
        private byte[] ZN_Table = new byte[256];

        public CPU(NES parent)
        {
            nes = parent;
            m_bClockProcess = false;

        }

        public void Dispose() { }

        internal long EXEC(int request_cycles)
        {
            byte opcode = 0;
            int OLD_cycles = TOTAL_cycles;
            int exec_cycles = 0;
            byte nmi_request = 0, irq_request = 0;

            ushort EA = 0;
            ushort ET = 0;
            ushort WT = 0;
            byte DT = 0;

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
                    case 0x69:
                        MR_IM(ref DT); ADC(ref WT, ref DT);
                        ADD_CYCLE(2, ref exec_cycles);
                        break;
                    case 0x65:
                        MR_ZP(ref EA, ref DT); ADC(ref WT, ref DT);
                        ADD_CYCLE(3, ref exec_cycles);
                        break;
                    case 0x75:
                        MR_ZX(ref DT, ref EA); ADC(ref WT, ref DT);
                        ADD_CYCLE(4, ref exec_cycles);
                        break;
                    case 0x6D:
                        MR_AB(ref EA, ref DT); ADC(ref WT, ref DT);
                        ADD_CYCLE(4, ref exec_cycles);
                        break;
                    case 0x7D:
                        MR_AX(ref ET, ref EA, ref DT); ADC(ref WT, ref DT); CHECK_EA(ref EA, ref ET, ref exec_cycles);
                        ADD_CYCLE(4, ref exec_cycles);
                        break;
                    case 0x79:
                        MR_AY(ref ET, ref EA, ref DT); ADC(ref WT, ref DT); CHECK_EA(ref EA, ref ET, ref exec_cycles);
                        ADD_CYCLE(4, ref exec_cycles);
                        break;
                    case 0x61:
                        MR_IX(ref DT, ref EA); ADC(ref WT, ref DT);
                        ADD_CYCLE(6, ref exec_cycles);
                        break;
                    case 0x71:
                        MR_IY(ref DT, ref ET, ref EA); ADC(ref WT, ref DT); CHECK_EA(ref EA, ref ET, ref exec_cycles);
                        ADD_CYCLE(4, ref exec_cycles);
                        break;
                    case 0xE9:
                        MR_IM(ref DT); SBC(ref WT, ref DT);
                        ADD_CYCLE(2, ref exec_cycles);
                        break;
                    case 0xE5:
                        MR_ZP(ref EA, ref DT); SBC(ref WT, ref DT);
                        ADD_CYCLE(3, ref exec_cycles);
                        break;
                    case 0xF5:
                        MR_ZX(ref DT, ref EA); SBC(ref WT, ref DT);
                        ADD_CYCLE(4, ref exec_cycles);
                        break;
                    case 0xED:
                        MR_AB(ref EA, ref DT); SBC(ref WT, ref DT);
                        ADD_CYCLE(4, ref exec_cycles);
                        break;
                    case 0xFD:
                        MR_AX(ref ET, ref EA, ref DT); SBC(ref WT, ref DT); CHECK_EA(ref EA, ref ET, ref exec_cycles);
                        ADD_CYCLE(4, ref exec_cycles);
                        break;
                    case 0xF9: // SBC $????,Y
                        MR_AY(ref ET, ref EA, ref DT); SBC(ref WT, ref DT); CHECK_EA(ref EA, ref ET, ref exec_cycles);
                        ADD_CYCLE(4, ref exec_cycles);
                        break;
                    case 0xE1: // SBC ($??,X)
                        MR_IX(ref DT, ref EA); SBC(ref WT, ref DT);
                        ADD_CYCLE(6, ref exec_cycles);
                        break;
                    case 0xF1: // SBC ($??),Y
                        MR_IY(ref DT, ref ET, ref EA); SBC(ref WT, ref DT); CHECK_EA(ref EA, ref ET, ref exec_cycles);
                        ADD_CYCLE(5, ref exec_cycles);
                        break;
                    case 0xC6: // DEC $??
                        MR_ZP(ref EA, ref DT); DEC(ref DT); MW_ZP(EA, DT);
                        ADD_CYCLE(5, ref exec_cycles);
                        break;
                    case 0xD6: // DEC $??,X
                        MR_ZX(ref DT, ref EA); DEC(ref DT); MW_ZP(EA, DT);
                        ADD_CYCLE(6, ref exec_cycles);
                        break;
                    case 0xCE: // DEC $????
                        MR_AB(ref EA, ref DT); DEC(ref DT); MW_EA(EA, DT);
                        ADD_CYCLE(6, ref exec_cycles);
                        break;
                }
            }
        _execute_exit:
#if !DPCM_SYNCCLOCK
            apu.SyncDPCM(TOTAL_cycles - OLD_cycles);
#endif
            return TOTAL_cycles - OLD_cycles;
        }

        internal void SetClockProcess(bool bEnable)
        {
            m_bClockProcess = bEnable;
        }

        internal byte OP6502(ushort addr)
        {
            return MMU.CPU_MEM_BANK[addr >> 13][addr & 0x1FFF];
        }

        internal ushort OP6502W(ushort addr)
        {
            var bytePage = MMU.CPU_MEM_BANK[addr >> 13];

            return BitConverter.ToUInt16(bytePage, addr & 0x1FFF);
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

        private void MR_IM(ref byte DT)
        {
            DT = OP6502(R.PC++);
        }

        private void MR_ZP(ref ushort EA, ref byte DT)
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

        private void ADC(ref ushort WT, ref byte DT)
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

        private void ADD_CYCLE(int V, ref int exec_cycles)
        {
            exec_cycles += V;
        }

        private void MR_ZX(ref byte DT, ref ushort EA)
        {
            DT = OP6502(R.PC++);
            EA = (ushort)(DT + R.X);
            DT = ZPRD(EA);
        }

        private void MR_AB(ref ushort EA, ref byte DT)
        {
            EA = OP6502W(R.PC);
            R.PC += 2;
            DT = RD6502(EA);
        }

        private void MR_AX(ref ushort ET, ref ushort EA, ref byte DT)
        {
            ET = OP6502W(R.PC);
            R.PC += 2;
            EA = (byte)(ET + R.X);
            DT = RD6502(EA);
        }

        private void CHECK_EA(ref ushort EA, ref ushort ET, ref int exec_cycles)
        {
            if ((ET & 0xFF00) != (EA & 0xFF00)) ADD_CYCLE(1, ref exec_cycles);
        }

        private void MR_AY(ref ushort ET, ref ushort EA, ref byte DT)
        {
            ET = OP6502W(R.PC);
            R.PC += 2;
            EA = (ushort)(ET + R.Y);
            DT = RD6502(EA);
        }

        private void MR_IX(ref byte DT, ref ushort EA)
        {
            DT = OP6502(R.PC++);
            EA = ZPRDW(DT + R.X);
            DT = RD6502(EA);
        }

        private void MR_IY(ref byte DT, ref ushort ET, ref ushort EA)
        {
            DT = OP6502(R.PC++);
            ET = ZPRDW(DT);
            EA = (ushort)(ET + R.Y);
            DT = RD6502(EA);
        }

        private void SBC(ref ushort WT, ref byte DT)
        {
            WT = (ushort)(R.A - DT - (~R.P & C_FLAG));
            bool f = ((R.A ^ DT) & (R.A ^ WT) & (0x80)) != 0;
            TST_FLAG(f, V_FLAG);
            TST_FLAG(WT < 0x100, C_FLAG);
            R.A = (byte)WT;
            SET_ZN_FLAG(R.A);
        }

        private void DEC(ref byte DT)
        {
            DT--;
            SET_ZN_FLAG(DT);
        }

        private void MW_ZP(ushort EA, byte DT)
        {
            ZPWR(EA, DT);
        }

        private void ZPWR(ushort a, byte v)
        {
            MMU.RAM[a] = v;
        }

        private void MW_EA(ushort EA, byte DT)
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