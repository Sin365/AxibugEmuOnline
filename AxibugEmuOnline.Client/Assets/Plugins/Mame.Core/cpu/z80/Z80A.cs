using MAME.Core;
using System;
using System.Globalization;
using System.IO;

// This Z80 emulator is a modified version of Ben Ryves 'Brazil' emulator.
// It is MIT licensed.

namespace cpu.z80
{
    /// <summary>
    /// ZiLOG Z80A CPU Emulator
    /// </summary>
    public sealed partial class Z80A : cpuexec_data
    {
        public static Z80A[] zz1;
        public static int nZ80;
        private bool Interruptable;
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
        /// <summary>
        /// Creates an instance of the <see cref="Z80A"/> emulator class.
        /// </summary>
        public Z80A()
        {
            InitialiseTables();
            // Clear main registers
            PPC = 0;
            RegAF = 0x0040; RegBC = 0; RegDE = 0; RegHL = 0;
            // Clear alternate registers
            RegAltAF = 0; RegAltBC = 0; RegAltDE = 0; RegAltHL = 0;
            // Clear special purpose registers
            RegI = 0; RegR = 0; RegR2 = 0;
            RegIX.Word = 0xffff; RegIY.Word = 0xffff;
            RegSP.Word = 0; RegPC.Word = 0;
            RegWZ.Word = 0;
            IFF1 = IFF2 = false;
            Halted = false;
            InterruptMode = 0;
        }

        /// <summary>
        /// Reset the Z80 to its initial state
        /// </summary>
        public override void Reset()
        {
            ResetRegisters();
            ResetInterrupts();
        }

        public override void set_irq_line(int irqline, LineState state)
        {
            if (irqline == (int)LineState.INPUT_LINE_NMI)
            {
                if (NonMaskableInterrupt == false && state != LineState.CLEAR_LINE)
                    nonMaskableInterruptPending = true;
                NonMaskableInterrupt = (state != LineState.CLEAR_LINE);
            }
            else
            {
                Interrupt = (state != LineState.CLEAR_LINE);
            }
        }
        public override void cpunum_set_input_line_and_vector(int cpunum, int line, LineState state, int vector)
        {
            Atime time1;
            time1 = EmuTimer.get_current_time();
            bool b1 = false;
            foreach (irq irq1 in Cpuint.lirq)
            {
                if (irq1.cpunum == cpunum && irq1.line == line)
                {
                    if (Attotime.attotime_compare(irq1.time, time1) > 0)
                    {
                        b1 = true;
                        break;
                    }
                    else
                    {
                        int i1 = 1;
                    }
                }
            }
            if (b1)
            {
                int i1 = 1;
            }
            else
            {
                EmuTimer.timer_set_internal(EmuTimer.TIME_ACT.Cpuint_cpunum_empty_event_queue);
            }
        }

        // Memory Access 

        public Func<ushort, byte> ReadOp, ReadOpArg;
        public Func<ushort, byte> ReadMemory;
        public Action<ushort, byte> WriteMemory;


        public void UnregisterMemoryMapper()
        {
            ReadMemory = null;
            WriteMemory = null;
        }

        // Hardware I/O Port Access

        public Func<ushort, byte> ReadHardware;
        public Action<ushort, byte> WriteHardware;

        // State Save/Load

        public void SaveStateBinary(BinaryWriter writer)
        {
            writer.Write(PPC);
            writer.Write(RegisterAF);
            writer.Write(RegisterBC);
            writer.Write(RegisterDE);
            writer.Write(RegisterHL);
            writer.Write(RegisterShadowAF);
            writer.Write(RegisterShadowBC);
            writer.Write(RegisterShadowDE);
            writer.Write(RegisterShadowHL);
            writer.Write(RegisterIX);
            writer.Write(RegisterIY);
            writer.Write(RegisterSP);
            writer.Write(RegisterPC);
            writer.Write(RegisterWZ);
            writer.Write(RegisterI);
            writer.Write(RegisterR);
            writer.Write(RegisterR2);
            writer.Write(Interrupt);
            writer.Write(NonMaskableInterrupt);
            writer.Write(NonMaskableInterruptPending);
            writer.Write(InterruptMode);
            writer.Write(IFF1);
            writer.Write(IFF2);
            writer.Write(Halted);
            writer.Write(TotalExecutedCycles);
            writer.Write(PendingCycles);
        }
        public void LoadStateBinary(BinaryReader reader)
        {
            PPC = reader.ReadUInt16();
            RegisterAF = reader.ReadUInt16();
            RegisterBC = reader.ReadUInt16();
            RegisterDE = reader.ReadUInt16();
            RegisterHL = reader.ReadUInt16();
            RegisterShadowAF = reader.ReadUInt16();
            RegisterShadowBC = reader.ReadUInt16();
            RegisterShadowDE = reader.ReadUInt16();
            RegisterShadowHL = reader.ReadUInt16();
            RegisterIX = reader.ReadUInt16();
            RegisterIY = reader.ReadUInt16();
            RegisterSP = reader.ReadUInt16();
            RegisterPC = reader.ReadUInt16();
            RegisterWZ = reader.ReadUInt16();
            RegisterI = reader.ReadByte();
            RegisterR = reader.ReadByte();
            RegisterR2 = reader.ReadByte();
            Interrupt = reader.ReadBoolean();
            NonMaskableInterrupt = reader.ReadBoolean();
            NonMaskableInterruptPending = reader.ReadBoolean();
            InterruptMode = reader.ReadInt32();
            IFF1 = reader.ReadBoolean();
            IFF2 = reader.ReadBoolean();
            Halted = reader.ReadBoolean();
            TotalExecutedCycles = reader.ReadUInt64();
            PendingCycles = reader.ReadInt32();
        }
        public void SaveStateText(TextWriter writer)
        {
            writer.WriteLine("[Z80]");
            writer.WriteLine("AF {0:X4}", RegAF.Word);
            writer.WriteLine("BC {0:X4}", RegBC.Word);
            writer.WriteLine("DE {0:X4}", RegDE.Word);
            writer.WriteLine("HL {0:X4}", RegHL.Word);
            writer.WriteLine("ShadowAF {0:X4}", RegAltAF.Word);
            writer.WriteLine("ShadowBC {0:X4}", RegAltBC.Word);
            writer.WriteLine("ShadowDE {0:X4}", RegAltDE.Word);
            writer.WriteLine("ShadowHL {0:X4}", RegAltHL.Word);
            writer.WriteLine("I {0:X2}", RegI);
            writer.WriteLine("R {0:X2}", RegR);
            writer.WriteLine("IX {0:X4}", RegIX.Word);
            writer.WriteLine("IY {0:X4}", RegIY.Word);
            writer.WriteLine("SP {0:X4}", RegSP.Word);
            writer.WriteLine("PC {0:X4}", RegPC.Word);
            writer.WriteLine("IRQ {0}", interrupt);
            writer.WriteLine("NMI {0}", nonMaskableInterrupt);
            writer.WriteLine("NMIPending {0}", nonMaskableInterruptPending);
            writer.WriteLine("IM {0}", InterruptMode);
            writer.WriteLine("IFF1 {0}", IFF1);
            writer.WriteLine("IFF2 {0}", IFF2);
            writer.WriteLine("Halted {0}", Halted);
            writer.WriteLine("ExecutedCycles {0}", totalExecutedCycles);
            writer.WriteLine("PendingCycles {0}", pendingCycles);
            writer.WriteLine("[/Z80]");
            writer.WriteLine();
        }

        public void LoadStateText(TextReader reader)
        {
            while (true)
            {
                string[] args = reader.ReadLine().Split(' ');
                if (args[0].Trim() == "") continue;
                if (args[0] == "[/Z80]") break;
                if (args[0] == "AF")
                    RegAF.Word = ushort.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "BC")
                    RegBC.Word = ushort.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "DE")
                    RegDE.Word = ushort.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "HL")
                    RegHL.Word = ushort.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "ShadowAF")
                    RegAltAF.Word = ushort.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "ShadowBC")
                    RegAltBC.Word = ushort.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "ShadowDE")
                    RegAltDE.Word = ushort.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "ShadowHL")
                    RegAltHL.Word = ushort.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "I")
                    RegI = byte.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "R")
                    RegR = byte.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "IX")
                    RegIX.Word = ushort.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "IY")
                    RegIY.Word = ushort.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "SP")
                    RegSP.Word = ushort.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "PC")
                    RegPC.Word = ushort.Parse(args[1], NumberStyles.HexNumber);
                else if (args[0] == "IRQ")
                    interrupt = bool.Parse(args[1]);
                else if (args[0] == "NMI")
                    nonMaskableInterrupt = bool.Parse(args[1]);
                else if (args[0] == "NMIPending")
                    nonMaskableInterruptPending = bool.Parse(args[1]);
                else if (args[0] == "IM")
                    InterruptMode = int.Parse(args[1]);
                else if (args[0] == "IFF1")
                    IFF1 = bool.Parse(args[1]);
                else if (args[0] == "IFF2")
                    IFF2 = bool.Parse(args[1]);
                else if (args[0] == "Halted")
                    Halted = bool.Parse(args[1]);
                else if (args[0] == "ExecutedCycles")
                    totalExecutedCycles = ulong.Parse(args[1]);
                else if (args[0] == "PendingCycles")
                    pendingCycles = int.Parse(args[1]);

                else
                    EmuLogger.Log("Skipping unrecognized identifier " + args[0]);
            }
        }
    }
}