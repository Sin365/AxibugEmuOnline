namespace VirtualNes.Core
{
    public struct REGSTAT : IStateBufferObject
    {
        public CPUSTAT cpureg;
        public PPUSTAT ppureg;



        public uint GetSize()
        {
            return cpureg.GetSize() + ppureg.GetSize();
        }

        public void SaveState(StateBuffer buffer)
        {
            cpureg.SaveState(buffer);
            ppureg.SaveState(buffer);
        }

        public void LoadState(StateReader buffer)
        {
            cpureg.LoadState(buffer);
            ppureg.LoadState(buffer);
        }
    }

    public struct CPUSTAT : IStateBufferObject
    {
        public ushort PC;
        public byte A;
        public byte X;
        public byte Y;
        public byte S;
        public byte P;
        public byte I; // Interrupt pending flag

        public byte FrameIRQ;
        public byte FrameIRQ_occur;
        public byte FrameIRQ_count;
        public byte FrameIRQ_type;
        public int FrameIRQ_cycles;
        public int DMA_cycles;

        public long emul_cycles;
        public long base_cycles;

        public uint GetSize()
        {
            return 32;
        }

        public void SaveState(StateBuffer buffer)
        {
            buffer.Write(PC);
            buffer.Write(A);
            buffer.Write(X);
            buffer.Write(Y);
            buffer.Write(S);
            buffer.Write(P);
            buffer.Write(I);
            buffer.Write(FrameIRQ);
            buffer.Write(FrameIRQ_occur);
            buffer.Write(FrameIRQ_count);
            buffer.Write(FrameIRQ_type);
            buffer.Write(FrameIRQ_cycles);
            buffer.Write(DMA_cycles);
            buffer.Write(emul_cycles);
            buffer.Write(base_cycles);
        }

        public void LoadState(StateReader buffer)
        {
            PC = buffer.Read_ushort();
            A = buffer.Read_byte();
            X = buffer.Read_byte();
            Y = buffer.Read_byte();
            S = buffer.Read_byte();
            P = buffer.Read_byte();
            I = buffer.Read_byte();
            FrameIRQ = buffer.Read_byte();
            FrameIRQ_occur = buffer.Read_byte();
            FrameIRQ_count = buffer.Read_byte();
            FrameIRQ_type = buffer.Read_byte();
            FrameIRQ_cycles = buffer.Read_int();
            DMA_cycles = buffer.Read_int();
            emul_cycles = buffer.Read_long();
            base_cycles = buffer.Read_long();
        }
    }

    public struct PPUSTAT : IStateBufferObject
    {
        public byte reg0;
        public byte reg1;
        public byte reg2;
        public byte reg3;
        public byte reg7;
        public byte toggle56;

        public ushort loopy_t;
        public ushort loopy_v;
        public ushort loopy_x;

        public uint GetSize()
        {
            return 12;
        }

        public void SaveState(StateBuffer buffer)
        {
            buffer.Write(reg0);
            buffer.Write(reg1);
            buffer.Write(reg2);
            buffer.Write(reg3);
            buffer.Write(reg7);
            buffer.Write(toggle56);
            buffer.Write(loopy_t);
            buffer.Write(loopy_v);
            buffer.Write(loopy_x);
        }

        public void LoadState(StateReader buffer)
        {
            reg0 = buffer.Read_byte();
            reg1 = buffer.Read_byte();
            reg2 = buffer.Read_byte();
            reg3 = buffer.Read_byte();
            reg7 = buffer.Read_byte();
            toggle56 = buffer.Read_byte();
            loopy_t = buffer.Read_ushort();
            loopy_v = buffer.Read_ushort();
            loopy_x = buffer.Read_ushort();
        }
    }
}
