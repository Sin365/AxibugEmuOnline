using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;

namespace MAME.Core
{
    [StructLayout(LayoutKind.Explicit)]
    [Serializable()]
    public struct Register
    {
        [FieldOffset(0)]
        public uint d;
        [FieldOffset(0)]
        public int sd;

        [FieldOffset(0)]
        public ushort LowWord;
        [FieldOffset(2)]
        public ushort HighWord;

        [FieldOffset(0)]
        public byte LowByte;
        [FieldOffset(1)]
        public byte HighByte;
        [FieldOffset(2)]
        public byte HighByte2;
        [FieldOffset(3)]
        public byte HighByte3;

        public override string ToString()
        {
            return String.Format("{0:X8}", d);
        }
    }
    public enum LineState
    {
        CLEAR_LINE = 0,
        ASSERT_LINE,
        HOLD_LINE,
        PULSE_LINE,
        INTERNAL_CLEAR_LINE = 100 + CLEAR_LINE,
        INTERNAL_ASSERT_LINE = 100 + ASSERT_LINE,
        MAX_INPUT_LINES = 32 + 3,
        INPUT_LINE_NMI = MAX_INPUT_LINES - 3,
        INPUT_LINE_RESET = MAX_INPUT_LINES - 2,
        INPUT_LINE_HALT = MAX_INPUT_LINES - 1,
    }
    public class irq
    {
        public int cpunum;
        public int line;
        public LineState state;
        public int vector;
        public Atime time;
        public irq()
        {

        }
        public void Init(int _cpunum, int _line, LineState _state, int _vector, Atime _time)
        {
            cpunum = _cpunum;
            line = _line;
            state = _state;
            vector = _vector;
            time = _time;
        }
    }
    public class vec
    {
        public int vector;
        public Atime time;
        public vec()
        {

        }
        public vec(int _vector, Atime _time)
        {
            vector = _vector;
            time = _time;
        }
    }
    public class Cpuint
    {
        public static int[,] interrupt_vector;
        public static byte[,] input_line_state;
        public static int[,] input_line_vector;
        public static int[,] input_event_index;
        //public static int[, ,] input_state;
        public static List<irq> lirq;
        public static List<vec> lvec;
        public static void cpuint_init()
        {
            int i, j;
            lirq = new List<irq>();
            lvec = new List<vec>();
            interrupt_vector = new int[8, 35];
            input_line_state = new byte[8, 35];
            input_line_vector = new int[8, 35];
            input_event_index = new int[8, 35];
            //input_state = new int[8, 35, 32];
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 35; j++)
                {
                    input_line_state[i, j] = 0;
                    interrupt_vector[i, j] = input_line_vector[i, j] = 0xff;
                    input_event_index[i, j] = 0;
                }
            }
        }
        public static void cpuint_reset()
        {
            int i, j;
            lirq = new List<irq>();
            lvec = new List<vec>();
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 35; j++)
                {
                    interrupt_vector[i, j] = 0xff;
                    input_event_index[i, j] = 0;
                }
            }
        }
        public static void cps1_irq_handler_mus(int irq)
        {
            cpunum_set_input_line(1, 0, (irq != 0) ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
        }
        public static void namcos1_sound_interrupt(int irq)
        {
            cpunum_set_input_line(2, 1, (irq != 0) ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
        }
        public static void cpunum_set_input_line(int cpunum, int line, LineState state)
        {
            int vector = (line >= 0 && line < 35) ? interrupt_vector[cpunum, line] : 0xff;
            irq _irq = ObjectPoolAuto.Acquire<irq>();
            _irq.Init(cpunum, line, state, vector, EmuTimer.get_current_time());
            lirq.Add(_irq);
            //lirq.Add(new irq(cpunum, line, state, vector, EmuTimer.get_current_time()));
            Cpuexec.cpu[cpunum].cpunum_set_input_line_and_vector(cpunum, line, state, vector);
        }
        public static void cpunum_set_input_line_vector(int cpunum, int line, int vector)
        {
            if (cpunum < Cpuexec.ncpu && line >= 0 && line < (int)LineState.MAX_INPUT_LINES)
            {
                interrupt_vector[cpunum, line] = vector;
                return;
            }
        }
        public static void cpunum_set_input_line_and_vector2(int cpunum, int line, LineState state, int vector)
        {
            if (line >= 0 && line < 35)
            {
                irq _irq = ObjectPoolAuto.Acquire<irq>();
                _irq.Init(cpunum, line, state, vector, EmuTimer.get_current_time());
                lirq.Add(_irq);
                //lirq.Add(new irq(cpunum, line, state, vector, EmuTimer.get_current_time()));
                EmuTimer.timer_set_internal(EmuTimer.TIME_ACT.Cpuint_cpunum_empty_event_queue);
            }
        }
        public static void cpunum_empty_event_queue()
        {
            //List<irq> lsirq = new List<irq>();
            List<irq> lsirq = ObjectPoolAuto.AcquireList<irq>();
            if (lirq.Count == 0)
            {
                int i1 = 1;
            }
            foreach (irq irq1 in lirq)
            {
                if (Attotime.attotime_compare(irq1.time, EmuTimer.global_basetime) <= 0)
                {
                    input_line_state[irq1.cpunum, irq1.line] = (byte)irq1.state;
                    input_line_vector[irq1.cpunum, irq1.line] = irq1.vector;
                    if (irq1.line == (int)LineState.INPUT_LINE_RESET)
                    {
                        if (irq1.state == LineState.ASSERT_LINE)
                        {
                            Cpuexec.cpunum_suspend(irq1.cpunum, Cpuexec.SUSPEND_REASON_RESET, 1);
                        }
                        else
                        {
                            if ((irq1.state == LineState.CLEAR_LINE && Cpuexec.cpunum_is_suspended(irq1.cpunum, Cpuexec.SUSPEND_REASON_RESET)) || irq1.state == LineState.PULSE_LINE)
                            {
                                Cpuexec.cpu[irq1.cpunum].Reset();
                            }
                            Cpuexec.cpunum_resume(irq1.cpunum, Cpuexec.SUSPEND_REASON_RESET);
                        }
                    }
                    else if (irq1.line == (int)LineState.INPUT_LINE_HALT)
                    {
                        if (irq1.state == LineState.ASSERT_LINE)
                        {
                            Cpuexec.cpunum_suspend(irq1.cpunum, Cpuexec.SUSPEND_REASON_HALT, 1);
                        }
                        else if (irq1.state == LineState.CLEAR_LINE)
                        {
                            Cpuexec.cpunum_resume(irq1.cpunum, Cpuexec.SUSPEND_REASON_HALT);
                        }
                    }
                    else
                    {
                        switch (irq1.state)
                        {
                            case LineState.PULSE_LINE:
                                Cpuexec.cpu[irq1.cpunum].set_irq_line(irq1.line, LineState.ASSERT_LINE);
                                Cpuexec.cpu[irq1.cpunum].set_irq_line(irq1.line, LineState.CLEAR_LINE);
                                break;
                            case LineState.HOLD_LINE:
                            case LineState.ASSERT_LINE:
                                Cpuexec.cpu[irq1.cpunum].set_irq_line(irq1.line, LineState.ASSERT_LINE);
                                break;
                            case LineState.CLEAR_LINE:
                                Cpuexec.cpu[irq1.cpunum].set_irq_line(irq1.line, LineState.CLEAR_LINE);
                                break;
                        }
                        if (irq1.state != LineState.CLEAR_LINE)
                        {
                            Cpuexec.cpu_triggerint(irq1.cpunum);
                        }
                    }
                    lsirq.Add(irq1);
                }
            }
            foreach (irq irq1 in lsirq)
            {
                input_event_index[irq1.cpunum, irq1.line] = 0;
                ObjectPoolAuto.Release(irq1);
                lirq.Remove(irq1);
            }
            if (lirq.Count > 0)
            {
                int i1 = 1;
            }
            ObjectPoolAuto.Release(lsirq);
        }
        public static int cpu_irq_callback(int cpunum, int line)
        {
            int vector = input_line_vector[cpunum, line];
            if (input_line_state[cpunum, line] == (byte)LineState.HOLD_LINE)
            {
                Cpuexec.cpu[cpunum].set_irq_line(line, LineState.CLEAR_LINE);
                input_line_state[cpunum, line] = (byte)LineState.CLEAR_LINE;
            }
            return vector;
        }
        public static int cpu_0_irq_callback(int line)
        {
            return cpu_irq_callback(0, line);
        }
        public static int cpu_1_irq_callback(int line)
        {
            return cpu_irq_callback(1, line);
        }
        public static int cpu_2_irq_callback(int line)
        {
            return cpu_irq_callback(2, line);
        }
        public static int cpu_3_irq_callback(int line)
        {
            return cpu_irq_callback(3, line);
        }
        public static void SaveStateBinary_v(BinaryWriter writer)
        {
            int i, n;
            n = lvec.Count;
            writer.Write(n);
            for (i = 0; i < n; i++)
            {
                writer.Write(lvec[i].vector);
                writer.Write(lvec[i].time.seconds);
                writer.Write(lvec[i].time.attoseconds);
            }
            for (i = n; i < 16; i++)
            {
                writer.Write(0);
                writer.Write(0);
                writer.Write((long)0);
            }
        }
        public static void LoadStateBinary_v(BinaryReader reader)
        {
            int i, n;
            n = reader.ReadInt32();
            lvec = new List<vec>();
            for (i = 0; i < n; i++)
            {
                lvec.Add(new vec());
                lvec[i].vector = reader.ReadInt32();
                lvec[i].time.seconds = reader.ReadInt32();
                lvec[i].time.attoseconds = reader.ReadInt64();
            }
            for (i = n; i < 16; i++)
            {
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt64();
            }
        }
        public static void SaveStateBinary(BinaryWriter writer)
        {
            int i, j, n;
            n = lirq.Count;
            writer.Write(n);
            for (i = 0; i < n; i++)
            {
                writer.Write(lirq[i].cpunum);
                writer.Write(lirq[i].line);
                writer.Write((int)lirq[i].state);
                writer.Write(lirq[i].vector);
                writer.Write(lirq[i].time.seconds);
                writer.Write(lirq[i].time.attoseconds);
            }
            for (i = n; i < 16; i++)
            {
                writer.Write(0);
                writer.Write(0);
                writer.Write(0);
                writer.Write(0);
                writer.Write(0);
                writer.Write((long)0);
            }
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 35; j++)
                {
                    writer.Write(interrupt_vector[i, j]);
                }
            }
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 35; j++)
                {
                    writer.Write(input_line_state[i, j]);
                }
            }
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 35; j++)
                {
                    writer.Write(input_line_vector[i, j]);
                }
            }
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 35; j++)
                {
                    writer.Write(input_event_index[i, j]);
                }
            }
        }
        public static void LoadStateBinary(BinaryReader reader)
        {
            int i, j, n;
            n = reader.ReadInt32();
            lirq = new List<irq>();
            for (i = 0; i < n; i++)
            {
                irq _irq = ObjectPoolAuto.Acquire<irq>();
                lirq.Add(_irq);
                //lirq.Add(new irq());
                lirq[i].cpunum = reader.ReadInt32();
                lirq[i].line = reader.ReadInt32();
                lirq[i].state = (LineState)reader.ReadInt32();
                lirq[i].vector = reader.ReadInt32();
                lirq[i].time.seconds = reader.ReadInt32();
                lirq[i].time.attoseconds = reader.ReadInt64();
            }
            for (i = n; i < 16; i++)
            {
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt64();
            }
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 35; j++)
                {
                    interrupt_vector[i, j] = reader.ReadInt32();
                }
            }
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 35; j++)
                {
                    input_line_state[i, j] = reader.ReadByte();
                }
            }
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 35; j++)
                {
                    input_line_vector[i, j] = reader.ReadInt32();
                }
            }
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 35; j++)
                {
                    input_event_index[i, j] = reader.ReadInt32();
                }
            }
        }
    }
}
