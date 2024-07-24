namespace VirtualNes.Core
{
    public class CPU
    {
        protected NES m_nes;

        public CPU(NES parent)
        {
            m_nes = parent;
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
        ushort PC;
        byte A;
        byte P;
        byte X;
        byte Y;
        byte S;

        byte Int_Pending;
    }
}