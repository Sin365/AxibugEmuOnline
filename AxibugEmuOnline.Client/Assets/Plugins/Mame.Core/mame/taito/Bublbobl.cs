namespace MAME.Core
{
    public partial class Taito
    {
        public static byte dsw0, dsw1;
        public static int bublbobl_video_enable, tokio_prot_count;
        public static int address, latch;
        public static int sound_nmi_enable, pending_nmi;
        public static byte ddr1, ddr2, ddr3, ddr4;
        public static byte port1_in, port2_in, port3_in, port4_in;
        public static byte port1_out, port2_out, port3_out, port4_out;
        public static byte portA_in, portA_out, ddrA, portB_in, portB_out, ddrB;
        public static int ic43_a, ic43_b;
        public readonly static byte[] tokio_prot_data = new byte[]
        {
            0x6c,
            0x7f,0x5f,0x7f,0x6f,0x5f,0x77,0x5f,0x7f,0x5f,0x7f,0x5f,0x7f,0x5b,0x7f,0x5f,0x7f,
            0x5f,0x77,0x59,0x7f,0x5e,0x7e,0x5f,0x6d,0x57,0x7f,0x5d,0x7d,0x5f,0x7e,0x5f,0x7f,
            0x5d,0x7d,0x5f,0x7e,0x5e,0x79,0x5f,0x7f,0x5f,0x7f,0x5d,0x7f,0x5f,0x7b,0x5d,0x7e,
            0x5f,0x7f,0x5d,0x7d,0x5f,0x7e,0x5e,0x7e,0x5f,0x7d,0x5f,0x7f,0x5f,0x7e,0x7f,0x5f,
            0x01,0x00,0x02,0x01,0x01,0x01,0x03,0x00,0x05,0x02,0x04,0x01,0x03,0x00,0x05,0x01,
            0x02,0x03,0x00,0x04,0x04,0x01,0x02,0x00,0x05,0x03,0x02,0x01,0x04,0x05,0x00,0x03,
            0x00,0x05,0x02,0x01,0x03,0x04,0x05,0x00,0x01,0x04,0x04,0x02,0x01,0x04,0x01,0x00,
            0x03,0x01,0x02,0x05,0x00,0x03,0x00,0x01,0x02,0x00,0x03,0x04,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x01,0x02,0x00,0x00,0x00,0x00,0x01,0x02,0x00,0x00,0x00,
            0x01,0x02,0x01,0x00,0x00,0x00,0x02,0x00,0x01,0x00,0x00,0x00,0x00,0x00,0x01,0x01,
            0x00,0x00,0x00,0x00,0x02,0x00,0x01,0x02,0x00,0x01,0x01,0x00,0x00,0x02,0x01,0x00,
            0x00,0x00,0x00,0x00,0x02,0x00,0x00,0x01
        };
        public static void bublbobl_bankswitch_w(byte data)
        {
            basebankmain = 0x10000 + 0x4000 * ((data ^ 4) & 7);
            Cpuint.cpunum_set_input_line(1, (int)LineState.INPUT_LINE_RESET, (data & 0x10) != 0 ? LineState.CLEAR_LINE : LineState.ASSERT_LINE);
            if (Cpuexec.ncpu == 4)
            {
                Cpuint.cpunum_set_input_line(3, (int)LineState.INPUT_LINE_RESET, (data & 0x20) != 0 ? LineState.CLEAR_LINE : LineState.ASSERT_LINE);
            }
            bublbobl_video_enable = data & 0x40;
            Generic.flip_screen_set(data & 0x80);
        }
        public static void tokio_bankswitch_w(byte data)
        {
            basebankmain = 0x10000 + 0x4000 * (data & 7);
        }
        public static void tokio_videoctrl_w(byte data)
        {
            Generic.flip_screen_set(data & 0x80);
        }
        public static void bublbobl_nmitrigger_w()
        {
            Cpuint.cpunum_set_input_line(1, (int)LineState.INPUT_LINE_NMI, LineState.PULSE_LINE);
        }
        public static byte tokio_mcu_r()
        {
            tokio_prot_count %= tokio_prot_data.Length;
            return tokio_prot_data[tokio_prot_count++];
        }
        public static byte tokiob_mcu_r()
        {
            return 0xbf; /* ad-hoc value set to pass initial testing */
        }
        public static void nmi_callback()
        {
            if (sound_nmi_enable != 0)
            {
                Cpuint.cpunum_set_input_line(2, (int)LineState.INPUT_LINE_NMI, LineState.PULSE_LINE);
            }
            else
            {
                pending_nmi = 1;
            }
        }
        public static void bublbobl_sound_command_w(byte data)
        {
            Sound.soundlatch_w((ushort)data);
            EmuTimer.timer_set_internal(EmuTimer.TIME_ACT.Taito_nmi_callback);
        }
        public static void bublbobl_sh_nmi_disable_w()
        {
            sound_nmi_enable = 0;
        }
        public static void bublbobl_sh_nmi_enable_w()
        {
            sound_nmi_enable = 1;
            if (pending_nmi != 0)
            {
                Cpuint.cpunum_set_input_line(2, (int)LineState.INPUT_LINE_NMI, LineState.PULSE_LINE);
                pending_nmi = 0;
            }
        }
        public static byte bublbobl_mcu_ddr1_r()
        {
            return ddr1;
        }
        public static void bublbobl_mcu_ddr1_w(byte data)
        {
            ddr1 = data;
        }
        public static byte bublbobl_mcu_ddr2_r()
        {
            return ddr2;
        }
        public static void bublbobl_mcu_ddr2_w(byte data)
        {
            ddr2 = data;
        }
        public static byte bublbobl_mcu_ddr3_r()
        {
            return ddr3;
        }
        public static void bublbobl_mcu_ddr3_w(byte data)
        {
            ddr3 = data;
        }
        public static byte bublbobl_mcu_ddr4_r()
        {
            return ddr4;
        }
        public static void bublbobl_mcu_ddr4_w(byte data)
        {
            ddr4 = data;
        }
        public static byte bublbobl_mcu_port1_r()
        {
            port1_in = (byte)sbyte0;
            return (byte)((port1_out & ddr1) | (port1_in & ~ddr1));
        }
        public static void bublbobl_mcu_port1_w(byte data)
        {
            //coin_lockout_global_w(~data & 0x10);
            if ((port1_out & 0x40) != 0 && (~data & 0x40) != 0)
            {
                Cpuint.cpunum_set_input_line_vector(0, 0, bublbobl_mcu_sharedram[0]);
                Cpuint.cpunum_set_input_line(0, 0, LineState.HOLD_LINE);
            }
            port1_out = data;
        }
        public static byte bublbobl_mcu_port2_r()
        {
            return (byte)((port2_out & ddr2) | (port2_in & ~ddr2));
        }
        static byte[] bublbobl_mcu_port2_w_ports = new byte[] { dsw0, dsw1, (byte)sbyte1, (byte)sbyte2 };
        public static void bublbobl_mcu_port2_w(byte data)
        {
            //使用外部定义减少GC压力
            bublbobl_mcu_port2_w_ports[0] = dsw0;
            bublbobl_mcu_port2_w_ports[1] = dsw1;
            bublbobl_mcu_port2_w_ports[2] = (byte)sbyte1;
            bublbobl_mcu_port2_w_ports[3] = (byte)sbyte2;
            //byte[] ports = new byte[] { dsw0, dsw1, (byte)sbyte1, (byte)sbyte2 };
            if ((~port2_out & 0x10) != 0 && (data & 0x10) != 0)
            {
                int address = port4_out | ((data & 0x0f) << 8);
                if ((port1_out & 0x80) != 0)
                {
                    if ((address & 0x0800) == 0x0000)
                    {
                        port3_in = bublbobl_mcu_port2_w_ports[address & 3];
                    }
                    else if ((address & 0x0c00) == 0x0c00)
                    {
                        port3_in = bublbobl_mcu_sharedram[address & 0x03ff];
                    }
                }
                else
                {
                    if ((address & 0x0c00) == 0x0c00)
                    {
                        bublbobl_mcu_sharedram[address & 0x03ff] = port3_out;
                    }
                }
            }
            port2_out = data;
        }
        public static byte bublbobl_mcu_port3_r()
        {
            return (byte)((port3_out & ddr3) | (port3_in & ~ddr3));
        }
        public static void bublbobl_mcu_port3_w(byte data)
        {
            port3_out = data;
        }
        public static byte bublbobl_mcu_port4_r()
        {
            return (byte)((port4_out & ddr4) | (port4_in & ~ddr4));
        }
        public static void bublbobl_mcu_port4_w(byte data)
        {
            port4_out = data;
        }
        public static byte boblbobl_ic43_a_r(int offset)
        {
            if (offset == 0)
            {
                return (byte)(ic43_a << 4);
            }
            else
            {
                return 0;
            }
        }
        public static void boblbobl_ic43_a_w(int offset)
        {
            int res = 0;
            switch (offset)
            {
                case 0:
                    if ((~ic43_a & 8) != 0)
                    {
                        res ^= 1;
                    }
                    if ((~ic43_a & 1) != 0)
                    {
                        res ^= 2;
                    }
                    if ((~ic43_a & 1) != 0)
                    {
                        res ^= 4;
                    }
                    if ((~ic43_a & 2) != 0)
                    {
                        res ^= 4;
                    }
                    if ((~ic43_a & 4) != 0)
                    {
                        res ^= 8;
                    }
                    break;
                case 1:
                    if ((~ic43_a & 8) != 0)
                    {
                        res ^= 1;
                    }
                    if ((~ic43_a & 2) != 0)
                    {
                        res ^= 1;
                    }
                    if ((~ic43_a & 8) != 0)
                    {
                        res ^= 2;
                    }
                    if ((~ic43_a & 1) != 0)
                    {
                        res ^= 4;
                    }
                    if ((~ic43_a & 4) != 0)
                    {
                        res ^= 8;
                    }
                    break;
                case 2:
                    if ((~ic43_a & 4) != 0)
                    {
                        res ^= 1;
                    }
                    if ((~ic43_a & 8) != 0)
                    {
                        res ^= 2;
                    }
                    if ((~ic43_a & 2) != 0)
                    {
                        res ^= 4;
                    }
                    if ((~ic43_a & 1) != 0)
                    {
                        res ^= 8;
                    }
                    if ((~ic43_a & 4) != 0)
                    {
                        res ^= 8;
                    }
                    break;
                case 3:
                    if ((~ic43_a & 2) != 0)
                    {
                        res ^= 1;
                    }
                    if ((~ic43_a & 4) != 0)
                    {
                        res ^= 2;
                    }
                    if ((~ic43_a & 8) != 0)
                    {
                        res ^= 2;
                    }
                    if ((~ic43_a & 8) != 0)
                    {
                        res ^= 4;
                    }
                    if ((~ic43_a & 1) != 0)
                    {
                        res ^= 8;
                    }
                    break;
            }
            ic43_a = res;
        }
        readonly static int[] boblbobl_ic43_b_w_xor = new int[] { 4, 1, 8, 2 };
        public static void boblbobl_ic43_b_w(int offset, byte data)
        {
            ic43_b = (data >> 4) ^ boblbobl_ic43_b_w_xor[offset];
        }
        public static byte boblbobl_ic43_b_r(int offset)
        {
            if (offset == 0)
            {
                return (byte)(ic43_b << 4);
            }
            else
            {
                return 0xff;
            }
        }
        public static void bublbobl_m68705_interrupt()
        {
            if ((Cpuexec.iloops & 1) != 0)
            {
                Cpuint.cpunum_set_input_line(3, 0, LineState.CLEAR_LINE);
            }
            else
            {
                Cpuint.cpunum_set_input_line(3, 0, LineState.ASSERT_LINE);
            }
        }
        public static byte bublbobl_68705_portA_r()
        {
            return (byte)((portA_out & ddrA) | (portA_in & ~ddrA));
        }
        public static void bublbobl_68705_portA_w(byte data)
        {
            portA_out = data;
        }
        public static void bublbobl_68705_ddrA_w(byte data)
        {
            ddrA = data;
        }
        public static byte bublbobl_68705_portB_r()
        {
            return (byte)((portB_out & ddrB) | (portB_in & ~ddrB));
        }
        static byte[] bublbobl_68705_portB_w_ports = new byte[] { dsw0, dsw1, (byte)sbyte1, (byte)sbyte2 };
        public static void bublbobl_68705_portB_w(byte data)
        {

            //使用外部定义减少GC压力
            bublbobl_68705_portB_w_ports[0] = dsw0;
            bublbobl_68705_portB_w_ports[1] = dsw1;
            bublbobl_68705_portB_w_ports[2] = (byte)sbyte1;
            bublbobl_68705_portB_w_ports[3] = (byte)sbyte2;
            //byte[] ports = new byte[] { dsw0, dsw1, (byte)sbyte1, (byte)sbyte2 };
            if (((ddrB & 0x01) != 0) && ((~data & 0x01) != 0) && ((portB_out & 0x01) != 0))
            {
                portA_in = (byte)latch;
            }
            if (((ddrB & 0x02) != 0) && ((data & 0x02) != 0) && ((~portB_out & 0x02) != 0))
            {
                address = (address & 0xff00) | portA_out;
            }
            if (((ddrB & 0x04) != 0) && ((data & 0x04) != 0) && ((~portB_out & 0x04) != 0))
            {
                address = (address & 0x00ff) | ((portA_out & 0x0f) << 8);
            }
            if (((ddrB & 0x10) != 0) && ((~data & 0x10) != 0) && ((portB_out & 0x10) != 0))
            {
                if ((data & 0x08) != 0)
                {
                    if ((address & 0x0800) == 0x0000)
                    {
                        latch = bublbobl_68705_portB_w_ports[address & 3];
                    }
                    else if ((address & 0x0c00) == 0x0c00)
                    {
                        latch = bublbobl_mcu_sharedram[address & 0x03ff];
                    }
                    else
                    {

                    }
                }
                else
                {
                    if ((address & 0x0c00) == 0x0c00)
                    {
                        bublbobl_mcu_sharedram[address & 0x03ff] = portA_out;
                    }
                    else
                    {

                    }
                }
            }
            if (((ddrB & 0x20) != 0) && ((~data & 0x20) != 0) && ((portB_out & 0x20) != 0))
            {
                bublbobl_mcu_sharedram[0x7c] = 0;
                Cpuint.cpunum_set_input_line_vector(0, 0, bublbobl_mcu_sharedram[0]);
                Cpuint.cpunum_set_input_line(0, 0, LineState.HOLD_LINE);
            }
            if (((ddrB & 0x40) != 0) && ((~data & 0x40) != 0) && ((portB_out & 0x40) != 0))
            {

            }
            if (((ddrB & 0x80) != 0) && ((~data & 0x80) != 0) && ((portB_out & 0x80) != 0))
            {

            }
            portB_out = data;
        }
        public static void bublbobl_68705_ddrB_w(byte data)
        {
            ddrB = data;
        }
    }
}
