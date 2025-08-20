
namespace MAME.Core
{
    public class YM3812
    {
        public static EmuTimer.emu_timer[] timer;
        public delegate void ym3812_delegate(LineState linestate);
        public static ym3812_delegate ym3812handler;
        public static void IRQHandler_3812(int irq)
        {
            if (ym3812handler != null)
            {
                ym3812handler(irq != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
            }
        }
        public static void timer_callback_3812_0()
        {
            FMOpl.ym3812_timer_over(0);
        }
        public static void timer_callback_3812_1()
        {
            FMOpl.ym3812_timer_over(1);
        }
        private static void TimerHandler_3812(int c, Atime period)
        {
            if (Attotime.attotime_compare(period, Attotime.ATTOTIME_ZERO) == 0)
            {
                EmuTimer.timer_enable(timer[c], false);
            }
            else
            {
                EmuTimer.timer_adjust_periodic(timer[c], period, Attotime.ATTOTIME_NEVER);
            }
        }
        public static void _stream_update_3812(int interval)
        {
            Sound.ym3812stream.stream_update();
        }
        public static void ym3812_start(int clock)
        {
            FMOpl.tl_tab = new int[0x1800];
            FMOpl.sin_tab = new uint[0x1000];
            timer = new EmuTimer.emu_timer[2];
            int rate = clock / 72;
            switch (Machine.sName)
            {
                case "pcktgal":
                case "pcktgalb":
                case "pcktgal2":
                case "pcktgal2j":
                case "spool3":
                case "spool3i":
                    ym3812handler = null;
                    break;
                case "starfigh":
                    ym3812handler = null;
                    break;
                case "drgnwrld":
                case "drgnwrldv30":
                case "drgnwrldv21":
                case "drgnwrldv21j":
                case "drgnwrldv20j":
                case "drgnwrldv10c":
                case "drgnwrldv11h":
                case "drgnwrldv40k":
                    ym3812handler = null;
                    break;
                default:
                    ym3812handler = null;
                    break;
            }
            FMOpl.ym3812_init(0, clock, rate);
            FMOpl.ym3812_set_timer_handler(TimerHandler_3812);
            FMOpl.ym3812_set_irq_handler(IRQHandler_3812);
            FMOpl.ym3812_set_update_handler(_stream_update_3812);
            timer[0] = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.YM3812_timer_callback_3812_0, false);
            timer[1] = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.YM3812_timer_callback_3812_1, false);
        }
        public static void ym3812_control_port_0_w(byte data)
        {
            FMOpl.ym3812_write(0, data);
        }
        public static void ym3812_write_port_0_w(byte data)
        {
            FMOpl.ym3812_write(1, data);
        }
        public static void SaveStateBinary(System.IO.BinaryWriter writer)
        {
            int i, j;
            for (i = 0; i < 9; i++)
            {
                writer.Write(FMOpl.YM3812.P_CH[i].block_fnum);
                writer.Write(FMOpl.YM3812.P_CH[i].kcode);
                for (j = 0; j < 2; j++)
                {
                    writer.Write(FMOpl.YM3812.P_CH[i].SLOT[j].ar);
                    writer.Write(FMOpl.YM3812.P_CH[i].SLOT[j].dr);
                    writer.Write(FMOpl.YM3812.P_CH[i].SLOT[j].rr);
                    writer.Write(FMOpl.YM3812.P_CH[i].SLOT[j].KSR);
                    writer.Write(FMOpl.YM3812.P_CH[i].SLOT[j].ksl);
                    writer.Write(FMOpl.YM3812.P_CH[i].SLOT[j].ksr);
                    writer.Write(FMOpl.YM3812.P_CH[i].SLOT[j].mul);
                    writer.Write(FMOpl.YM3812.P_CH[i].SLOT[j].Cnt);
                    writer.Write(FMOpl.YM3812.P_CH[i].SLOT[j].FB);
                    writer.Write(FMOpl.YM3812.P_CH[i].SLOT[j].op1_out[0]);
                    writer.Write(FMOpl.YM3812.P_CH[i].SLOT[j].op1_out[1]);
                    writer.Write(FMOpl.YM3812.P_CH[i].SLOT[j].CON);
                    writer.Write(FMOpl.YM3812.P_CH[i].SLOT[j].eg_type);
                    writer.Write(FMOpl.YM3812.P_CH[i].SLOT[j].state);
                    writer.Write(FMOpl.YM3812.P_CH[i].SLOT[j].TL);
                    writer.Write(FMOpl.YM3812.P_CH[i].SLOT[j].volume);
                    writer.Write(FMOpl.YM3812.P_CH[i].SLOT[j].sl);
                    writer.Write(FMOpl.YM3812.P_CH[i].SLOT[j].key);
                    writer.Write(FMOpl.YM3812.P_CH[i].SLOT[j].AMmask);
                    writer.Write(FMOpl.YM3812.P_CH[i].SLOT[j].vib);
                    writer.Write(FMOpl.YM3812.P_CH[i].SLOT[j].wavetable);
                }
            }
            writer.Write(FMOpl.YM3812.eg_cnt);
            writer.Write(FMOpl.YM3812.eg_timer);
            writer.Write(FMOpl.YM3812.rhythm);
            writer.Write(FMOpl.YM3812.lfo_am_depth);
            writer.Write(FMOpl.YM3812.lfo_pm_depth_range);
            writer.Write(FMOpl.YM3812.lfo_am_cnt);
            writer.Write(FMOpl.YM3812.lfo_pm_cnt);
            writer.Write(FMOpl.YM3812.noise_rng);
            writer.Write(FMOpl.YM3812.noise_p);
            writer.Write(FMOpl.YM3812.wavesel);
            for (i = 0; i < 2; i++)
            {
                writer.Write(FMOpl.YM3812.T[i]);
            }
            for (i = 0; i < 2; i++)
            {
                writer.Write(FMOpl.YM3812.st[i]);
            }
            writer.Write(FMOpl.YM3812.address);
            writer.Write(FMOpl.YM3812.status);
            writer.Write(FMOpl.YM3812.statusmask);
            writer.Write(FMOpl.YM3812.mode);
        }
        public static void LoadStateBinary(System.IO.BinaryReader reader)
        {
            int i, j;
            for (i = 0; i < 9; i++)
            {
                FMOpl.YM3812.P_CH[i].block_fnum = reader.ReadUInt32();
                FMOpl.YM3812.P_CH[i].kcode = reader.ReadByte();
                for (j = 0; j < 2; j++)
                {
                    FMOpl.YM3812.P_CH[i].SLOT[j].ar = reader.ReadUInt32();
                    FMOpl.YM3812.P_CH[i].SLOT[j].dr = reader.ReadUInt32();
                    FMOpl.YM3812.P_CH[i].SLOT[j].rr = reader.ReadUInt32();
                    FMOpl.YM3812.P_CH[i].SLOT[j].KSR = reader.ReadByte();
                    FMOpl.YM3812.P_CH[i].SLOT[j].ksl = reader.ReadByte();
                    FMOpl.YM3812.P_CH[i].SLOT[j].ksr = reader.ReadByte();
                    FMOpl.YM3812.P_CH[i].SLOT[j].mul = reader.ReadByte();
                    FMOpl.YM3812.P_CH[i].SLOT[j].Cnt = reader.ReadUInt32();
                    FMOpl.YM3812.P_CH[i].SLOT[j].FB = reader.ReadByte();
                    FMOpl.YM3812.P_CH[i].SLOT[j].op1_out[0] = reader.ReadInt32();
                    FMOpl.YM3812.P_CH[i].SLOT[j].op1_out[1] = reader.ReadInt32();
                    FMOpl.YM3812.P_CH[i].SLOT[j].CON = reader.ReadByte();
                    FMOpl.YM3812.P_CH[i].SLOT[j].eg_type = reader.ReadByte();
                    FMOpl.YM3812.P_CH[i].SLOT[j].state = reader.ReadByte();
                    FMOpl.YM3812.P_CH[i].SLOT[j].TL = reader.ReadUInt32();
                    FMOpl.YM3812.P_CH[i].SLOT[j].volume = reader.ReadInt32();
                    FMOpl.YM3812.P_CH[i].SLOT[j].sl = reader.ReadUInt32();
                    FMOpl.YM3812.P_CH[i].SLOT[j].key = reader.ReadUInt32();
                    FMOpl.YM3812.P_CH[i].SLOT[j].AMmask = reader.ReadUInt32();
                    FMOpl.YM3812.P_CH[i].SLOT[j].vib = reader.ReadByte();
                    FMOpl.YM3812.P_CH[i].SLOT[j].wavetable = reader.ReadUInt16();
                }
            }
            FMOpl.YM3812.eg_cnt = reader.ReadUInt32();
            FMOpl.YM3812.eg_timer = reader.ReadUInt32();
            FMOpl.YM3812.rhythm = reader.ReadByte();
            FMOpl.YM3812.lfo_am_depth = reader.ReadByte();
            FMOpl.YM3812.lfo_pm_depth_range = reader.ReadByte();
            FMOpl.YM3812.lfo_am_cnt = reader.ReadUInt32();
            FMOpl.YM3812.lfo_pm_cnt = reader.ReadUInt32();
            FMOpl.YM3812.noise_rng = reader.ReadUInt32();
            FMOpl.YM3812.noise_p = reader.ReadUInt32();
            FMOpl.YM3812.wavesel = reader.ReadByte();
            for (i = 0; i < 2; i++)
            {
                FMOpl.YM3812.T[i] = reader.ReadUInt32();
            }
            for (i = 0; i < 2; i++)
            {
                FMOpl.YM3812.st[i] = reader.ReadByte();
            }
            FMOpl.YM3812.address = reader.ReadByte();
            FMOpl.YM3812.status = reader.ReadByte();
            FMOpl.YM3812.statusmask = reader.ReadByte();
            FMOpl.YM3812.mode = reader.ReadByte();
        }
        public static void IRQHandler_3526(int irq)
        {
            if (ym3812handler != null)
            {
                ym3812handler(irq != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
            }
        }
        public static void timer_callback_3526_0()
        {
            FMOpl.ym3526_timer_over(0);
        }
        public static void timer_callback_3526_1()
        {
            FMOpl.ym3812_timer_over(1);
        }
        public static void TimerHandler_3526(int c, Atime period)
        {
            if (Attotime.attotime_compare(period, Attotime.ATTOTIME_ZERO) == 0)
            {
                EmuTimer.timer_enable(timer[c], false);
            }
            else
            {
                EmuTimer.timer_adjust_periodic(timer[c], period, Attotime.ATTOTIME_NEVER);
            }
        }
        public static void _stream_update_3526(int interval)
        {
            Sound.ym3526stream.stream_update();
        }
        public static void ym3526_start(int clock)
        {
            int rate = clock / 72;
            FMOpl.YM3526 = FMOpl.ym3526_init(0, clock, rate);
            timer = new EmuTimer.emu_timer[2];
            FMOpl.ym3526_set_timer_handler(TimerHandler_3526);
            FMOpl.ym3526_set_irq_handler(IRQHandler_3526);
            FMOpl.ym3526_set_update_handler(_stream_update_3526);
            timer[0] = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.YM3812_timer_callback_3526_0, false);
            timer[1] = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.YM3812_timer_callback_3526_1, false);
        }
        public static void ym3526_control_port_0_w(byte data)
        {
            FMOpl.ym3526_write(0, data);
        }
        public static void ym3526_write_port_0_w(byte data)
        {
            FMOpl.ym3526_write(1, data);
        }
        public static byte ym3526_status_port_0_r()
        {
            return FMOpl.ym3526_read(0);
        }
        public static byte ym3526_read_port_0_r()
        {
            return FMOpl.ym3526_read(1);
        }
        public static void ym3526_control_port_1_w(byte data)
        {
            FMOpl.ym3526_write(0, data);
        }
        public static void ym3526_write_port_1_w(byte data)
        {
            FMOpl.ym3526_write(1, data);
        }
        public static byte ym3526_status_port_1_r()
        {
            return FMOpl.ym3526_read(0);
        }
        public static byte ym3526_read_port_1_r()
        {
            return FMOpl.ym3526_read(1);
        }
        public static void SaveStateBinary_YM3526(System.IO.BinaryWriter writer)
        {
            int i, j;
            for (i = 0; i < 9; i++)
            {
                writer.Write(FMOpl.YM3526.P_CH[i].block_fnum);
                writer.Write(FMOpl.YM3526.P_CH[i].kcode);
                for (j = 0; j < 2; j++)
                {
                    writer.Write(FMOpl.YM3526.P_CH[i].SLOT[j].ar);
                    writer.Write(FMOpl.YM3526.P_CH[i].SLOT[j].dr);
                    writer.Write(FMOpl.YM3526.P_CH[i].SLOT[j].rr);
                    writer.Write(FMOpl.YM3526.P_CH[i].SLOT[j].KSR);
                    writer.Write(FMOpl.YM3526.P_CH[i].SLOT[j].ksl);
                    writer.Write(FMOpl.YM3526.P_CH[i].SLOT[j].ksr);
                    writer.Write(FMOpl.YM3526.P_CH[i].SLOT[j].mul);
                    writer.Write(FMOpl.YM3526.P_CH[i].SLOT[j].Cnt);
                    writer.Write(FMOpl.YM3526.P_CH[i].SLOT[j].FB);
                    writer.Write(FMOpl.YM3526.P_CH[i].SLOT[j].op1_out[0]);
                    writer.Write(FMOpl.YM3526.P_CH[i].SLOT[j].op1_out[1]);
                    writer.Write(FMOpl.YM3526.P_CH[i].SLOT[j].CON);
                    writer.Write(FMOpl.YM3526.P_CH[i].SLOT[j].eg_type);
                    writer.Write(FMOpl.YM3526.P_CH[i].SLOT[j].state);
                    writer.Write(FMOpl.YM3526.P_CH[i].SLOT[j].TL);
                    writer.Write(FMOpl.YM3526.P_CH[i].SLOT[j].volume);
                    writer.Write(FMOpl.YM3526.P_CH[i].SLOT[j].sl);
                    writer.Write(FMOpl.YM3526.P_CH[i].SLOT[j].key);
                    writer.Write(FMOpl.YM3526.P_CH[i].SLOT[j].AMmask);
                    writer.Write(FMOpl.YM3526.P_CH[i].SLOT[j].vib);
                    writer.Write(FMOpl.YM3526.P_CH[i].SLOT[j].wavetable);
                }
            }
            writer.Write(FMOpl.YM3526.eg_cnt);
            writer.Write(FMOpl.YM3526.eg_timer);
            writer.Write(FMOpl.YM3526.rhythm);
            writer.Write(FMOpl.YM3526.lfo_am_depth);
            writer.Write(FMOpl.YM3526.lfo_pm_depth_range);
            writer.Write(FMOpl.YM3526.lfo_am_cnt);
            writer.Write(FMOpl.YM3526.lfo_pm_cnt);
            writer.Write(FMOpl.YM3526.noise_rng);
            writer.Write(FMOpl.YM3526.noise_p);
            writer.Write(FMOpl.YM3526.wavesel);
            for (i = 0; i < 2; i++)
            {
                writer.Write(FMOpl.YM3526.T[i]);
            }
            for (i = 0; i < 2; i++)
            {
                writer.Write(FMOpl.YM3526.st[i]);
            }
            writer.Write(FMOpl.YM3526.address);
            writer.Write(FMOpl.YM3526.status);
            writer.Write(FMOpl.YM3526.statusmask);
            writer.Write(FMOpl.YM3526.mode);
        }
        public static void LoadStateBinary_YM3526(System.IO.BinaryReader reader)
        {
            int i, j;
            for (i = 0; i < 9; i++)
            {
                FMOpl.YM3526.P_CH[i].block_fnum = reader.ReadUInt32();
                FMOpl.YM3526.P_CH[i].kcode = reader.ReadByte();
                for (j = 0; j < 2; j++)
                {
                    FMOpl.YM3526.P_CH[i].SLOT[j].ar = reader.ReadUInt32();
                    FMOpl.YM3526.P_CH[i].SLOT[j].dr = reader.ReadUInt32();
                    FMOpl.YM3526.P_CH[i].SLOT[j].rr = reader.ReadUInt32();
                    FMOpl.YM3526.P_CH[i].SLOT[j].KSR = reader.ReadByte();
                    FMOpl.YM3526.P_CH[i].SLOT[j].ksl = reader.ReadByte();
                    FMOpl.YM3526.P_CH[i].SLOT[j].ksr = reader.ReadByte();
                    FMOpl.YM3526.P_CH[i].SLOT[j].mul = reader.ReadByte();
                    FMOpl.YM3526.P_CH[i].SLOT[j].Cnt = reader.ReadUInt32();
                    FMOpl.YM3526.P_CH[i].SLOT[j].FB = reader.ReadByte();
                    FMOpl.YM3526.P_CH[i].SLOT[j].op1_out[0] = reader.ReadInt32();
                    FMOpl.YM3526.P_CH[i].SLOT[j].op1_out[1] = reader.ReadInt32();
                    FMOpl.YM3526.P_CH[i].SLOT[j].CON = reader.ReadByte();
                    FMOpl.YM3526.P_CH[i].SLOT[j].eg_type = reader.ReadByte();
                    FMOpl.YM3526.P_CH[i].SLOT[j].state = reader.ReadByte();
                    FMOpl.YM3526.P_CH[i].SLOT[j].TL = reader.ReadUInt32();
                    FMOpl.YM3526.P_CH[i].SLOT[j].volume = reader.ReadInt32();
                    FMOpl.YM3526.P_CH[i].SLOT[j].sl = reader.ReadUInt32();
                    FMOpl.YM3526.P_CH[i].SLOT[j].key = reader.ReadUInt32();
                    FMOpl.YM3526.P_CH[i].SLOT[j].AMmask = reader.ReadUInt32();
                    FMOpl.YM3526.P_CH[i].SLOT[j].vib = reader.ReadByte();
                    FMOpl.YM3526.P_CH[i].SLOT[j].wavetable = reader.ReadUInt16();
                }
            }
            FMOpl.YM3526.eg_cnt = reader.ReadUInt32();
            FMOpl.YM3526.eg_timer = reader.ReadUInt32();
            FMOpl.YM3526.rhythm = reader.ReadByte();
            FMOpl.YM3526.lfo_am_depth = reader.ReadByte();
            FMOpl.YM3526.lfo_pm_depth_range = reader.ReadByte();
            FMOpl.YM3526.lfo_am_cnt = reader.ReadUInt32();
            FMOpl.YM3526.lfo_pm_cnt = reader.ReadUInt32();
            FMOpl.YM3526.noise_rng = reader.ReadUInt32();
            FMOpl.YM3526.noise_p = reader.ReadUInt32();
            FMOpl.YM3526.wavesel = reader.ReadByte();
            for (i = 0; i < 2; i++)
            {
                FMOpl.YM3526.T[i] = reader.ReadUInt32();
            }
            for (i = 0; i < 2; i++)
            {
                FMOpl.YM3526.st[i] = reader.ReadByte();
            }
            FMOpl.YM3526.address = reader.ReadByte();
            FMOpl.YM3526.status = reader.ReadByte();
            FMOpl.YM3526.statusmask = reader.ReadByte();
            FMOpl.YM3526.mode = reader.ReadByte();
        }
    }
}
