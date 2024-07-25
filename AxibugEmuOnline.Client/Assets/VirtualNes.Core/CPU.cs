#undef DPCM_SYNCCLOCK

using Codice.CM.Client.Differences;
using System;
using System.Runtime.CompilerServices;

namespace VirtualNes.Core
{
    public class CPU
    {
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
        private R6502 R;
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

                if (R.Int_Pending != 0)
                {
                    if ((R.Int_Pending & NMI_FLAG) != 0)
                    {
                        nmi_request = 0xFF;
                        byte temp = unchecked((byte)(~NMI_FLAG));
                        R.Int_Pending &= temp;
                    }
                    else if ((R.Int_Pending & IRQ_MASK) != 0)
                    {
                        byte temp = unchecked((byte)(~IRQ_TRIGGER2));
                        R.Int_Pending &= temp;
                        if (
                            ((R.P & I_FLAG) == 0)
                            &&
                            (opcode != 0x40)
                            )
                        {
                            irq_request = 0xFF;
                            temp = unchecked((byte)(~IRQ_TRIGGER));
                            R.Int_Pending &= temp;
                        }
                    }
                }

                switch (opcode)
                {
                    case 0x69:
                        MR_IM(ref DT, ref R); ADC(ref WT, ref DT, ref R);
                        ADD_CYCLE(2, ref exec_cycles);
                        break;
                    case 0x65:
                        MR_ZP(ref EA, ref DT, ref R); ADC(ref WT, ref DT, ref R);
                        ADD_CYCLE(3, ref exec_cycles);
                        break;
                    case 0x75:
                        MR_ZX(ref DT, ref EA, ref R); ADC(ref WT, ref DT, ref R);
                        ADD_CYCLE(4, ref exec_cycles);
                        break;
                    case 0x6D:
                        MR_AB(ref EA, ref DT, ref R);ADC(ref WT, ref DT, ref R);
                        ADD_CYCLE(4, ref exec_cycles);
                        break;
                    case 0x7D:
                        
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

        private void MR_IM(ref byte DT, ref R6502 R)
        {
            DT = OP6502(R.PC++);
        }

        private void MR_ZP(ref ushort EA, ref byte DT, ref R6502 R)
        {
            EA = OP6502(R.PC++);
            DT = ZPRD(ref EA);
        }

        private byte ZPRD(ref ushort A)
        {
            return MMU.RAM[A];
        }

        private void ADC(ref ushort WT, ref byte DT, ref R6502 R)
        {
            WT = (ushort)(R.A + DT + (R.P & C_FLAG));
            TST_FLAG(WT > 0xFF, C_FLAG, ref R);
            var temp = ((~(R.A ^ DT)) & (R.A ^ WT) & 0x80);
            TST_FLAG(temp != 0, V_FLAG, ref R);
            R.A = (byte)WT;
            SET_ZN_FLAG(R.A, ref R);
        }

        private void TST_FLAG(bool F, byte V, ref R6502 R)
        {
            byte temp = (byte)~V;
            R.P &= temp;

            if (F) R.P |= V;
        }

        private void SET_ZN_FLAG(byte A, ref R6502 R)
        {
            byte temp = unchecked((byte)(~(Z_FLAG | N_FLAG)));
            R.P &= temp;
            R.P |= ZN_Table[A];
        }

        private void ADD_CYCLE(int V, ref int exec_cycles)
        {
            exec_cycles += V;
        }

        private void MR_ZX(ref byte DT, ref ushort EA, ref R6502 R)
        {
            DT = OP6502(R.PC++);
            EA = (ushort)(DT + R.X);
            DT = ZPRD(ref EA);
        }

        private void MR_AB(ref ushort EA, ref byte DT, ref R6502 R)
        {
            EA = OP6502W(R.PC);
            R.PC += 2;
            DT = RD6502(EA);
        }

        internal void ClrIRQ(byte mask)
        {
            byte temp = (byte)~mask;
            R.Int_Pending &= temp;
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

    public struct R6502
    {
        public ushort PC;
        public byte A;
        public byte P;
        public byte X;
        public byte Y;
        public byte S;

        public byte Int_Pending;
    }
}