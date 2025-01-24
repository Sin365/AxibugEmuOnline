using MAME.Core;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace cpu.m68000
{
    public sealed partial class MC68000 : cpuexec_data
    {
        public static MC68000 m1;
        // Machine State
        public Register[] D = new Register[8];
        public Register[] A = new Register[8];
        public int PC, PPC;
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

        // Status Registers
        public int int_cycles;
        public int InterruptMaskLevel;
        bool s, m;
        public int usp, ssp;
        public bool stopped;

        /// <summary>Machine/Interrupt mode</summary>
        public bool M { get { return m; } set { m = value; } } // TODO probably have some switch logic maybe

        public void SetS(bool b1)
        {
            s = b1;
        }
        public void m68k_set_irq(int interrupt)
        {
            Interrupt = interrupt;
            m68ki_check_interrupts();
        }

        /// <summary>Supervisor/User mode</summary>
        public bool S
        {
            get
            {
                return s;
            }
            set
            {
                if (value == s)
                    return;
                if (value == true) // entering supervisor mode
                {
                    //EmuLogger.Log("&^&^&^&^& ENTER SUPERVISOR MODE");
                    usp = A[7].s32;
                    A[7].s32 = ssp;
                    s = true;
                }
                else
                { // exiting supervisor mode
                    //EmuLogger.Log("&^&^&^&^& LEAVE SUPERVISOR MODE");
                    ssp = A[7].s32;
                    A[7].s32 = usp;
                    s = false;
                }
            }
        }

        /// <summary>Extend Flag</summary>
        public bool X;
        /// <summary>Negative Flag</summary>
        public bool N;
        /// <summary>Zero Flag</summary>
        public bool Z;
        /// <summary>Overflow Flag</summary>
        public bool V;
        /// <summary>Carry Flag</summary>
        public bool C;



        /// <summary>Status Register</summary>
        public short SR
        {
            get
            {
                short value = 0;
                if (C) value |= 0x0001;
                if (V) value |= 0x0002;
                if (Z) value |= 0x0004;
                if (N) value |= 0x0008;
                if (X) value |= 0x0010;
                if (M) value |= 0x1000;
                if (S) value |= 0x2000;
                value |= (short)((InterruptMaskLevel & 7) << 8);
                return value;
            }
            set
            {
                C = (value & 0x0001) != 0;
                V = (value & 0x0002) != 0;
                Z = (value & 0x0004) != 0;
                N = (value & 0x0008) != 0;
                X = (value & 0x0010) != 0;
                M = (value & 0x1000) != 0;
                S = (value & 0x2000) != 0;
                InterruptMaskLevel = (value >> 8) & 7;
                //m68ki_check_interrupts();
            }
        }
        public short CCR
        {
            get
            {
                short value = 0;
                if (C) value |= 0x0001;
                if (V) value |= 0x0002;
                if (Z) value |= 0x0004;
                if (N) value |= 0x0008;
                if (X) value |= 0x0010;
                return value;
            }
            set
            {
                C = (value & 0x0001) != 0;
                V = (value & 0x0002) != 0;
                Z = (value & 0x0004) != 0;
                N = (value & 0x0008) != 0;
                X = (value & 0x0010) != 0;
            }
        }
        public int Interrupt;// { get; set; }

        // Memory Access        
        public Func<int, sbyte> ReadOpByte, ReadByte;
        public Func<int, short> ReadOpWord, ReadPcrelWord, ReadWord;
        public Func<int, int> ReadOpLong, ReadPcrelLong, ReadLong;

        public Action<int, sbyte> WriteByte;
        public Action<int, short> WriteWord;
        public Action<int, int> WriteLong;


        public MC68000()
        {
            //倒是已经废弃
            //BuildOpcodeTable();
        }


        public override void Reset()
        {
            Pulse_Reset();
        }
        public override void set_irq_line(int irqline, LineState state)
        {
            if (irqline == (int)LineState.INPUT_LINE_NMI)
                irqline = 7;
            switch (state)
            {
                case LineState.CLEAR_LINE:
                    m68k_set_irq(0);
                    break;
                case LineState.ASSERT_LINE:
                    m68k_set_irq(irqline);
                    break;
                default:
                    m68k_set_irq(irqline);
                    break;
            }
        }
        public override void cpunum_set_input_line_and_vector(int cpunum, int line, LineState state, int vector)
        {
            EmuTimer.timer_set_internal(EmuTimer.TIME_ACT.Cpuint_cpunum_empty_event_queue);
        }
        public void Pulse_Reset()
        {
            stopped = false;
            pendingCycles = 0;
            S = true;
            M = false;
            InterruptMaskLevel = 7;
            Interrupt = 0;
            A[7].s32 = ReadOpLong(0);
            PC = ReadOpLong(4);
        }

        //public Action[] Opcodes = new Action[0x10000];
        public ushort op;

        public void Step()
        {
            //EmuLogger.Log(Disassemble(PC));

            op = (ushort)ReadOpWord(PC); PC += 2;
            //Opcodes[op]();
            DoOpCode(op);
        }

        public override int ExecuteCycles(int cycles)
        {
            if (!stopped)
            {
                pendingCycles = cycles;
                int ran;
                pendingCycles -= int_cycles;
                int_cycles = 0;
                do
                {
                    int prevCycles = pendingCycles;
                    PPC = PC;
                    op = (ushort)ReadOpWord(PC); PC += 2;
                    //Opcodes[op]();
                    DoOpCode(op);
                    m68ki_check_interrupts();
                    int delta = prevCycles - pendingCycles;
                    totalExecutedCycles += (ulong)delta;
                }
                while (pendingCycles > 0);
                pendingCycles -= int_cycles;
                int_cycles = 0;
                ran = cycles - pendingCycles;
                return ran;
            }
            pendingCycles = 0;
            int_cycles = 0;
            return cycles;
        }
        public void m68ki_check_interrupts()
        {
            if (Interrupt > 0 && (Interrupt > InterruptMaskLevel || Interrupt > 7))
            {
                stopped = false;
                //int vector = Cpuint.cpu_irq_callback(cpunum, Interrupt);
                short sr = (short)SR;                  // capture current SR.
                S = true;                               // switch to supervisor mode, if not already in it.
                A[7].s32 -= 4;                          // Push PC on stack
                WriteLong(A[7].s32, PC);
                A[7].s32 -= 2;                          // Push SR on stack
                WriteWord(A[7].s32, sr);
                PC = ReadLong((24 + Interrupt) * 4);    // Jump to interrupt vector
                InterruptMaskLevel = Interrupt;         // Set interrupt mask to level currently being entered
                Interrupt = 0;                          // "ack" interrupt. Note: this is wrong.
                int_cycles += 0x2c;
            }
        }

        //public string State()
        //{
        //    string a = Disassemble(PC).ToString().PadRight(64);
        //    //string a = string.Format("{0:X6}: {1:X4}", PC, ReadWord(PC)).PadRight(64);
        //    string b = string.Format("D0:{0:X8} D1:{1:X8} D2:{2:X8} D3:{3:X8} D4:{4:X8} D5:{5:X8} D6:{6:X8} D7:{7:X8} ", D[0].u32, D[1].u32, D[2].u32, D[3].u32, D[4].u32, D[5].u32, D[6].u32, D[7].u32);
        //    string c = string.Format("A0:{0:X8} A1:{1:X8} A2:{2:X8} A3:{3:X8} A4:{4:X8} A5:{5:X8} A6:{6:X8} A7:{7:X8} ", A[0].u32, A[1].u32, A[2].u32, A[3].u32, A[4].u32, A[5].u32, A[6].u32, A[7].u32);
        //    string d = string.Format("SR:{0:X4} Pending {1}", SR, pendingCycles);
        //    return a + b + c + d;
        //}

        public void SaveStateBinary(BinaryWriter writer)
        {
            int i;
            for (i = 0; i < 0x08; i++)
            {
                writer.Write(MC68000.m1.D[i].u32);
            }
            for (i = 0; i < 0x08; i++)
            {
                writer.Write(MC68000.m1.A[i].u32);
            }
            writer.Write(MC68000.m1.PPC);
            writer.Write(MC68000.m1.PC);
            writer.Write(MC68000.m1.S);
            writer.Write(MC68000.m1.M);
            writer.Write(MC68000.m1.X);
            writer.Write(MC68000.m1.N);
            writer.Write(MC68000.m1.Z);
            writer.Write(MC68000.m1.V);
            writer.Write(MC68000.m1.C);
            writer.Write(MC68000.m1.InterruptMaskLevel);
            writer.Write(MC68000.m1.Interrupt);
            writer.Write(MC68000.m1.int_cycles);
            writer.Write(MC68000.m1.usp);
            writer.Write(MC68000.m1.ssp);
            writer.Write(MC68000.m1.stopped);
            writer.Write(MC68000.m1.TotalExecutedCycles);
            writer.Write(MC68000.m1.PendingCycles);
        }
        public void LoadStateBinary(BinaryReader reader)
        {
            int i;
            for (i = 0; i < 0x08; i++)
            {
                MC68000.m1.D[i].u32 = reader.ReadUInt32();
            }
            for (i = 0; i < 0x08; i++)
            {
                MC68000.m1.A[i].u32 = reader.ReadUInt32();
            }
            MC68000.m1.PPC = reader.ReadInt32();
            MC68000.m1.PC = reader.ReadInt32();
            MC68000.m1.SetS(reader.ReadBoolean());
            MC68000.m1.M = reader.ReadBoolean();
            MC68000.m1.X = reader.ReadBoolean();
            MC68000.m1.N = reader.ReadBoolean();
            MC68000.m1.Z = reader.ReadBoolean();
            MC68000.m1.V = reader.ReadBoolean();
            MC68000.m1.C = reader.ReadBoolean();
            MC68000.m1.InterruptMaskLevel = reader.ReadInt32();
            MC68000.m1.Interrupt = reader.ReadInt32();
            MC68000.m1.int_cycles = reader.ReadInt32();
            MC68000.m1.usp = reader.ReadInt32();
            MC68000.m1.ssp = reader.ReadInt32();
            MC68000.m1.stopped = reader.ReadBoolean();
            MC68000.m1.TotalExecutedCycles = reader.ReadUInt64();
            MC68000.m1.PendingCycles = reader.ReadInt32();
        }
        public void SaveStateText(TextWriter writer, string id)
        {
            writer.WriteLine("[{0}]", id);
            writer.WriteLine("D0 {0:X8}", D[0].s32);
            writer.WriteLine("D1 {0:X8}", D[1].s32);
            writer.WriteLine("D2 {0:X8}", D[2].s32);
            writer.WriteLine("D3 {0:X8}", D[3].s32);
            writer.WriteLine("D4 {0:X8}", D[4].s32);
            writer.WriteLine("D5 {0:X8}", D[5].s32);
            writer.WriteLine("D6 {0:X8}", D[6].s32);
            writer.WriteLine("D7 {0:X8}", D[7].s32);
            writer.WriteLine();

            writer.WriteLine("A0 {0:X8}", A[0].s32);
            writer.WriteLine("A1 {0:X8}", A[1].s32);
            writer.WriteLine("A2 {0:X8}", A[2].s32);
            writer.WriteLine("A3 {0:X8}", A[3].s32);
            writer.WriteLine("A4 {0:X8}", A[4].s32);
            writer.WriteLine("A5 {0:X8}", A[5].s32);
            writer.WriteLine("A6 {0:X8}", A[6].s32);
            writer.WriteLine("A7 {0:X8}", A[7].s32);
            writer.WriteLine();

            writer.WriteLine("PC {0:X6}", PC);
            writer.WriteLine("InterruptMaskLevel {0}", InterruptMaskLevel);
            writer.WriteLine("USP {0:X8}", usp);
            writer.WriteLine("SSP {0:X8}", ssp);
            writer.WriteLine("S {0}", s);
            writer.WriteLine("M {0}", m);
            writer.WriteLine();

            writer.WriteLine("TotalExecutedCycles {0}", totalExecutedCycles);
            writer.WriteLine("PendingCycles {0}", pendingCycles);

            writer.WriteLine("[/{0}]", id);
        }

        public void LoadStateText(TextReader reader, string id)
        {
            while (true)
            {
                string[] args = reader.ReadLine().Split(' ');
                if (args[0].Trim() == "") continue;
                if (args[0] == "[/" + id + "]") break;
                else if (args[0] == "D0") D[0].s32 = int.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "D1") D[1].s32 = int.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "D2") D[2].s32 = int.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "D3") D[3].s32 = int.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "D4") D[4].s32 = int.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "D5") D[5].s32 = int.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "D6") D[6].s32 = int.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "D7") D[7].s32 = int.Parse(args[1], NumberStyles.HexNumber);

                else if (args[0] == "A0") A[0].s32 = int.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "A1") A[1].s32 = int.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "A2") A[2].s32 = int.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "A3") A[3].s32 = int.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "A4") A[4].s32 = int.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "A5") A[5].s32 = int.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "A6") A[6].s32 = int.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "A7") A[7].s32 = int.Parse(args[1], NumberStyles.HexNumber);

                else if (args[0] == "PC") PC = int.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "InterruptMaskLevel") InterruptMaskLevel = int.Parse(args[1]);
                else if (args[0] == "USP") usp = int.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "SSP") ssp = int.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "S") s = bool.Parse(args[1]);
                else if (args[0] == "M") m = bool.Parse(args[1]);

                else if (args[0] == "TotalExecutedCycles") totalExecutedCycles = ulong.Parse(args[1]);
                else if (args[0] == "PendingCycles") pendingCycles = int.Parse(args[1]);

                else
                {
                    //EmuLogger.Log("Skipping unrecognized identifier " + args[0]);
                }
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Register
    {
        [FieldOffset(0)]
        public uint u32;
        [FieldOffset(0)]
        public int s32;

        [FieldOffset(0)]
        public ushort u16;
        [FieldOffset(0)]
        public short s16;

        [FieldOffset(0)]
        public byte u8;
        [FieldOffset(0)]
        public sbyte s8;

        public override string ToString()
        {
            return String.Format("{0:X8}", u32);
        }
    }
}
