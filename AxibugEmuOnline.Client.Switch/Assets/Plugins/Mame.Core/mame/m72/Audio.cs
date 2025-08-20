namespace MAME.Core
{
    public unsafe partial class M72
    {
        public static int setvector_param;
        public static byte irqvector;
        public static int sample_addr;
        public static void setvector_callback()
        {
            switch (setvector_param)
            {
                case 0:
                    irqvector = 0xff;
                    break;
                case 1:
                    irqvector &= 0xef;
                    break;
                case 2:
                    irqvector |= 0x10;
                    break;
                case 3:
                    irqvector &= 0xdf;
                    break;
                case 4:
                    irqvector |= 0x20;
                    break;
            }
            Cpuint.interrupt_vector[1, 0] = irqvector;
            if (irqvector == 0xff)
            {
                Cpuint.cpunum_set_input_line(1, 0, LineState.CLEAR_LINE);
            }
            else
            {
                Cpuint.cpunum_set_input_line(1, 0, LineState.ASSERT_LINE);
            }
        }
        public static void machine_reset_m72_sound()
        {
            setvector_param = 0;
            setvector_callback();
        }
        public static void m72_ym2151_irq_handler(int irq)
        {
            if (irq != 0)
            {
                Cpuint.lvec.Add(new vec(1, EmuTimer.get_current_time()));
                setvector_param = 1;
                EmuTimer.emu_timer timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.setvector, true);
                EmuTimer.timer_adjust_periodic(timer, Attotime.ATTOTIME_ZERO, Attotime.ATTOTIME_NEVER);
            }
            else
            {
                Cpuint.lvec.Add(new vec(2, EmuTimer.get_current_time()));
                setvector_param = 2;
                EmuTimer.emu_timer timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.setvector, true);
                EmuTimer.timer_adjust_periodic(timer, Attotime.ATTOTIME_ZERO, Attotime.ATTOTIME_NEVER);
            }
        }
        public static void m72_sound_command_w(int offset, ushort data)
        {
            //if (ACCESSING_BITS_0_7)
            {
                Sound.soundlatch_w(data);
                Cpuint.lvec.Add(new vec(3, EmuTimer.get_current_time()));
                setvector_param = 3;
                EmuTimer.emu_timer timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.setvector, true);
                EmuTimer.timer_adjust_periodic(timer, Attotime.ATTOTIME_ZERO, Attotime.ATTOTIME_NEVER);
            }
        }
        public static void m72_sound_command_byte_w(int offset, byte data)
        {
            Sound.soundlatch_w(data);
            Cpuint.lvec.Add(new vec(3, EmuTimer.get_current_time()));
            setvector_param = 3;
            EmuTimer.emu_timer timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.setvector, true);
            EmuTimer.timer_adjust_periodic(timer, Attotime.ATTOTIME_ZERO, Attotime.ATTOTIME_NEVER);
        }
        public static void m72_sound_irq_ack_w(int offset, byte data)
        {
            Cpuint.lvec.Add(new vec(4, EmuTimer.get_current_time()));
            setvector_param = 4;
            EmuTimer.emu_timer timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.setvector, true);
            EmuTimer.timer_adjust_periodic(timer, Attotime.ATTOTIME_ZERO, Attotime.ATTOTIME_NEVER);
        }
        public static void m72_set_sample_start(int start)
        {
            sample_addr = start;
        }
        public static void rtype2_sample_addr_w(int offset, byte data)
        {
            sample_addr >>= 5;
            if (offset == 1)
            {
                sample_addr = (sample_addr & 0x00ff) | ((data << 8) & 0xff00);
            }
            else
            {
                sample_addr = (sample_addr & 0xff00) | ((data << 0) & 0x00ff);
            }
            sample_addr <<= 5;
        }
        public static byte m72_sample_r()
        {
            return samplesrom[sample_addr];
        }
        public static void m72_sample_w(byte data)
        {
            DAC.dac_signed_data_w(0, data);
            sample_addr = (sample_addr + 1) & (samplesromLength - 1);
        }
    }
}
