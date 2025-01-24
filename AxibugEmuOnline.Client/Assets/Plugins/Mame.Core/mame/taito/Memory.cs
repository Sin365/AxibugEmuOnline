using cpu.z80;

namespace MAME.Core
{
    public unsafe partial class Taito
    {
        public static sbyte sbyte0, sbyte1, sbyte2, sbyte3, sbyte4, sbyte5;
        public static sbyte sbyte0_old, sbyte1_old, sbyte2_old, sbyte3_old, sbyte4_old, sbyte5_old;
        public static int p1x_accum_old, p1x_previous_old, p1y_accum_old, p1y_previous_old;
        public static byte Z0ReadMemory_tokio(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = Memory.mainrom[address];
            }
            else if (address >= 0x8000 && address <= 0xbfff)
            {
                int offset = address - 0x8000;
                result = Memory.mainrom[basebankmain + offset];
            }
            else if (address >= 0xc000 && address <= 0xdcff)
            {
                int offset = address - 0xc000;
                result = videoram[offset];
            }
            else if (address >= 0xdd00 && address <= 0xdfff)
            {
                int offset = address - 0xdd00;
                result = bublbobl_objectram[offset];
            }
            else if (address >= 0xe000 && address <= 0xf7ff)
            {
                int offset = address - 0xe000;
                result = Memory.mainram[offset];
            }
            else if (address == 0xfa03)
            {
                result = dsw0;
            }
            else if (address == 0xfa04)
            {
                result = dsw1;
            }
            else if (address == 0xfa05)
            {
                result = (byte)sbyte0;
            }
            else if (address == 0xfa06)
            {
                result = (byte)sbyte1;
            }
            else if (address == 0xfa07)
            {
                result = (byte)sbyte2;
            }
            else if (address == 0xfc00)
            {
                result = 0;
            }
            else if (address == 0xfe00)
            {
                result = tokio_mcu_r();
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static byte Z0ReadMemory_tokiob(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = Memory.mainrom[address];
            }
            else if (address >= 0x8000 && address <= 0xbfff)
            {
                int offset = address - 0x8000;
                result = Memory.mainrom[basebankmain + offset];
            }
            else if (address >= 0xc000 && address <= 0xdcff)
            {
                int offset = address - 0xc000;
                result = videoram[offset];
            }
            else if (address >= 0xdd00 && address <= 0xdfff)
            {
                int offset = address - 0xdd00;
                result = bublbobl_objectram[offset];
            }
            else if (address >= 0xe000 && address <= 0xf7ff)
            {
                int offset = address - 0xe000;
                result = Memory.mainram[offset];
            }
            else if (address == 0xfa03)
            {
                result = dsw0;
            }
            else if (address == 0xfa04)
            {
                result = dsw1;
            }
            else if (address == 0xfa05)
            {
                result = (byte)sbyte0;
            }
            else if (address == 0xfa06)
            {
                result = (byte)sbyte1;
            }
            else if (address == 0xfa07)
            {
                result = (byte)sbyte2;
            }
            else if (address == 0xfc00)
            {
                result = 0;
            }
            else if (address == 0xfe00)
            {
                result = tokiob_mcu_r();
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static byte Z0ReadMemory_bootleg(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = Memory.mainrom[address];
            }
            else if (address >= 0x8000 && address <= 0xbfff)
            {
                int offset = address - 0x8000;
                result = Memory.mainrom[basebankmain + offset];
            }
            else if (address >= 0xc000 && address <= 0xdcff)
            {
                int offset = address - 0xc000;
                result = videoram[offset];
            }
            else if (address >= 0xdd00 && address <= 0xdfff)
            {
                int offset = address - 0xdd00;
                result = bublbobl_objectram[offset];
            }
            else if (address >= 0xe000 && address <= 0xf7ff)
            {
                int offset = address - 0xe000;
                result = Memory.mainram[offset];
            }
            else if (address >= 0xfc00 && address <= 0xfcff)
            {
                int offset = address - 0xfc00;
                result = mainram2[offset];
            }
            else if (address >= 0xfd00 && address <= 0xfdff)
            {
                int offset = address - 0xfd00;
                result = mainram3[offset];
            }
            else if (address >= 0xfe00 && address <= 0xfe03)
            {
                int offset = address - 0xfe00;
                result = boblbobl_ic43_a_r(offset);
            }
            else if (address >= 0xfe80 && address <= 0xfe83)
            {
                int offset = address - 0xfe80;
                result = boblbobl_ic43_b_r(offset);
            }
            else if (address == 0xff00)
            {
                result = dsw0;
            }
            else if (address == 0xff01)
            {
                result = dsw1;
            }
            else if (address == 0xff02)
            {
                result = (byte)sbyte0;
            }
            else if (address == 0xff03)
            {
                result = (byte)sbyte1;
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static void Z0WriteMemory_tokio(ushort address, byte value)
        {
            if (address >= 0x0000 && address <= 0x7fff)
            {
                Memory.audiorom[address] = value;
            }
            else if (address >= 0x8000 && address <= 0xbfff)
            {
                int offset = address - 0x8000;
                Memory.mainrom[basebankmain + offset] = value;
            }
            else if (address >= 0xc000 && address <= 0xdcff)
            {
                int offset = address - 0xc000;
                videoram[offset] = value;
            }
            else if (address >= 0xdd00 && address <= 0xdfff)
            {
                int offset = address - 0xdd00;
                bublbobl_objectram[offset] = value;
            }
            else if (address >= 0xe000 && address <= 0xf7ff)
            {
                int offset = address - 0xe000;
                Memory.mainram[offset] = value;
            }
            else if (address >= 0xf800 && address <= 0xf9ff)
            {
                int offset = address - 0xf800;
                Generic.paletteram_RRRRGGGGBBBBxxxx_be_w(offset, value);
            }
            else if (address == 0xfa00)
            {
                Generic.watchdog_reset_w();
            }
            else if (address == 0xfa80)
            {
                tokio_bankswitch_w(value);
            }
            else if (address == 0xfb00)
            {
                tokio_videoctrl_w(value);
            }
            else if (address == 0xfb80)
            {
                bublbobl_nmitrigger_w();
            }
            else if (address == 0xfc00)
            {
                bublbobl_sound_command_w(value);
            }
            else if (address == 0xfe00)
            {

            }
        }
        public static void Z0WriteMemory_bootleg(ushort address, byte value)
        {
            if (address >= 0x0000 && address <= 0x7fff)
            {
                Memory.audiorom[address] = value;
            }
            else if (address >= 0x8000 && address <= 0xbfff)
            {
                int offset = address - 0x8000;
                Memory.mainrom[basebankmain + offset] = value;
            }
            else if (address >= 0xc000 && address <= 0xdcff)
            {
                int offset = address - 0xc000;
                videoram[offset] = value;
            }
            else if (address >= 0xdd00 && address <= 0xdfff)
            {
                int offset = address - 0xdd00;
                bublbobl_objectram[offset] = value;
            }
            else if (address >= 0xe000 && address <= 0xf7ff)
            {
                int offset = address - 0xe000;
                Memory.mainram[offset] = value;
            }
            else if (address >= 0xf800 && address <= 0xf9ff)
            {
                int offset = address - 0xf800;
                Generic.paletteram_RRRRGGGGBBBBxxxx_be_w(offset, value);
            }
            else if (address == 0xfa00)
            {
                bublbobl_sound_command_w(value);
            }
            else if (address == 0xfa03)
            {

            }
            else if (address == 0xfa80)
            {

            }
            else if (address == 0xfb40)
            {
                bublbobl_bankswitch_w(value);
            }
            else if (address >= 0xfc00 && address <= 0xfcff)
            {
                int offset = address - 0xfc00;
                mainram2[offset] = value;
            }
            else if (address >= 0xfd00 && address <= 0xfdff)
            {
                int offset = address - 0xfd00;
                mainram3[offset] = value;
            }
            else if (address >= 0xfe00 && address <= 0xfe03)
            {
                int offset = address - 0xfe00;
                boblbobl_ic43_a_w(offset);
            }
            else if (address >= 0xfe80 && address <= 0xfe83)
            {
                int offset = address - 0xfe80;
                boblbobl_ic43_b_w(offset, value);
            }
            else if (address == 0xff94)
            {

            }
            else if (address == 0xff98)
            {

            }
        }
        public static byte Z1ReadOp_tokio(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = slaverom[address];
            }
            else if (address >= 0x8000 && address <= 0x97ff)
            {
                int offset = address - 0x8000;
                result = Memory.mainram[offset];
            }
            return result;
        }
        public static byte Z1ReadMemory_tokio(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = slaverom[address];
            }
            else if (address >= 0x8000 && address <= 0x97ff)
            {
                int offset = address - 0x8000;
                result = Memory.mainram[offset];
            }
            return result;
        }
        public static void Z1WriteMemory_tokio(ushort address, byte value)
        {
            if (address <= 0x7fff)
            {
                slaverom[address] = value;
            }
            else if (address >= 0x8000 && address <= 0x97ff)
            {
                int offset = address - 0x8000;
                Memory.mainram[offset] = value;
            }
        }
        public static byte Z2ReadMemory_tokio(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0x8000 && address <= 0x8fff)
            {
                int offset = address - 0x8000;
                result = Memory.audioram[offset];
            }
            else if (address == 0x9000)
            {
                result = (byte)Sound.soundlatch_r();
            }
            else if (address == 0x9800)
            {
                result = 0;
            }
            else if (address == 0xb000)
            {
                result = YM2203.ym2203_status_port_0_r();
            }
            else if (address == 0xb001)
            {
                result = YM2203.ym2203_read_port_0_r();
            }
            else if (address >= 0xe000 && address <= 0xffff)
            {
                result = Memory.audiorom[address];
            }
            return result;
        }
        public static void Z2WriteMemory_tokio(ushort address, byte value)
        {
            if (address <= 0x7fff)
            {
                Memory.audiorom[address] = value;
            }
            else if (address >= 0x8000 && address <= 0x8fff)
            {
                int offset = address - 0x8000;
                Memory.audioram[offset] = value;
            }
            else if (address == 0x9000)
            {

            }
            else if (address == 0xa000)
            {
                bublbobl_sh_nmi_disable_w();
            }
            else if (address == 0xa800)
            {
                bublbobl_sh_nmi_enable_w();
            }
            else if (address == 0xb000)
            {
                YM2203.ym2203_control_port_0_w(value);
            }
            else if (address == 0xb001)
            {
                YM2203.ym2203_write_port_0_w(value);
            }
            else if (address >= 0xe000 && address <= 0xffff)
            {
                Memory.audiorom[address] = value;
            }
        }
        public static byte Z0ReadOp_bublbobl(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = Memory.mainrom[address];
            }
            else if (address >= 0x8000 && address <= 0xbfff)
            {
                int offset = address - 0x8000;
                result = Memory.mainrom[basebankmain + offset];
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static byte Z0ReadMemory_bublbobl(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = Memory.mainrom[address];
            }
            else if (address >= 0x8000 && address <= 0xbfff)
            {
                int offset = address - 0x8000;
                result = Memory.mainrom[basebankmain + offset];
            }
            else if (address >= 0xc000 && address <= 0xdcff)
            {
                int offset = address - 0xc000;
                result = videoram[offset];
            }
            else if (address >= 0xdd00 && address <= 0xdfff)
            {
                int offset = address - 0xdd00;
                result = bublbobl_objectram[offset];
            }
            else if (address >= 0xe000 && address <= 0xf7ff)
            {
                int offset = address - 0xe000;
                result = Memory.mainram[offset];
            }
            else if (address >= 0xfc00 && address <= 0xffff)
            {
                int offset = address - 0xfc00;
                result = bublbobl_mcu_sharedram[offset];
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static void Z0WriteMemory_bublbobl(ushort address, byte value)
        {
            if (address >= 0x0000 && address <= 0x7fff)
            {
                Memory.audiorom[address] = value;
            }
            else if (address >= 0x8000 && address <= 0xbfff)
            {
                int offset = address - 0x8000;
                Memory.mainrom[basebankmain + offset] = value;
            }
            else if (address >= 0xc000 && address <= 0xdcff)
            {
                int offset = address - 0xc000;
                videoram[offset] = value;
            }
            else if (address >= 0xdd00 && address <= 0xdfff)
            {
                int offset = address - 0xdd00;
                bublbobl_objectram[offset] = value;
            }
            else if (address >= 0xe000 && address <= 0xf7ff)
            {
                int offset = address - 0xe000;
                Memory.mainram[offset] = value;
            }
            else if (address >= 0xf800 && address <= 0xf9ff)
            {
                int offset = address - 0xf800;
                Generic.paletteram_RRRRGGGGBBBBxxxx_be_w(offset, value);
            }
            else if (address == 0xfa00)
            {
                bublbobl_sound_command_w(value);
            }
            else if (address == 0xfa80)
            {
                Watchdog.watchdog_reset();
            }
            else if (address == 0xfb40)
            {
                bublbobl_bankswitch_w(value);
            }
            else if (address >= 0xfc00 && address <= 0xffff)
            {
                int offset = address - 0xfc00;
                bublbobl_mcu_sharedram[offset] = value;
            }
        }
        public static byte Z0ReadHardware(ushort address)
        {
            return 0;
        }
        public static void Z0WriteHardware(ushort address, byte value)
        {

        }
        public static int Z0IRQCallback()
        {
            return Cpuint.cpu_irq_callback(Z80A.zz1[0].cpunum, 0);
        }
        public static byte Z1ReadOp_bublbobl(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = slaverom[address];
            }
            else if (address >= 0xe000 && address <= 0xf7ff)
            {
                int offset = address - 0xe000;
                result = Memory.mainram[offset];
            }
            return result;
        }
        public static byte Z1ReadMemory_bublbobl(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = slaverom[address];
            }
            else if (address >= 0xe000 && address <= 0xf7ff)
            {
                int offset = address - 0xe000;
                result = Memory.mainram[offset];
            }
            return result;
        }
        public static void Z1WriteMemory_bublbobl(ushort address, byte value)
        {
            if (address <= 0x7fff)
            {
                slaverom[address] = value;
            }
            else if (address >= 0xe000 && address <= 0xf7ff)
            {
                int offset = address - 0xe000;
                Memory.mainram[offset] = value;
            }
        }
        public static byte Z1ReadHardware(ushort address)
        {
            byte result = 0;
            address &= 0xff;
            return result;
        }
        public static void Z1WriteHardware(ushort address, byte value)
        {
            address &= 0xff;

        }
        public static int Z1IRQCallback()
        {
            return Cpuint.cpu_irq_callback(Z80A.zz1[1].cpunum, 0);
        }
        public static byte Z2ReadOp_bublbobl(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0x8000 && address <= 0x8fff)
            {
                int offset = address - 0x8000;
                result = Memory.audioram[offset];
            }
            return result;
        }
        public static byte Z2ReadMemory_bublbobl(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0x8000 && address <= 0x8fff)
            {
                int offset = address - 0x8000;
                result = Memory.audioram[offset];
            }
            else if (address == 0x9000)
            {
                result = YM2203.ym2203_status_port_0_r();
            }
            else if (address == 0x9001)
            {
                result = YM2203.ym2203_read_port_0_r();
            }
            else if (address == 0xa000)
            {
                result = YM3812.ym3526_status_port_0_r();
            }
            else if (address == 0xb000)
            {
                result = (byte)Sound.soundlatch_r();
            }
            else if (address >= 0xe000 && address <= 0xffff)
            {
                result = Memory.audiorom[address];
            }
            return result;
        }
        public static void Z2WriteMemory_bublbobl(ushort address, byte value)
        {
            if (address <= 0x7fff)
            {
                Memory.audiorom[address] = value;
            }
            else if (address >= 0x8000 && address <= 0x8fff)
            {
                int offset = address - 0x8000;
                Memory.audioram[offset] = value;
            }
            else if (address == 0x9000)
            {
                YM2203.ym2203_control_port_0_w(value);
            }
            else if (address == 0x9001)
            {
                YM2203.ym2203_write_port_0_w(value);
            }
            else if (address == 0xa000)
            {
                YM3812.ym3526_control_port_0_w(value);
            }
            else if (address == 0xa001)
            {
                YM3812.ym3526_write_port_0_w(value);
            }
            else if (address == 0xb001)
            {
                bublbobl_sh_nmi_enable_w();
            }
            else if (address == 0xb002)
            {
                bublbobl_sh_nmi_disable_w();
            }
            else if (address >= 0xe000 && address <= 0xffff)
            {
                Memory.audiorom[address] = value;
            }
        }
        public static byte Z2ReadHardware(ushort address)
        {
            byte result = 0;
            address &= 0xff;
            return result;
        }
        public static void Z2WriteHardware(ushort address, byte value)
        {
            address &= 0xff;
        }
        public static int Z2IRQCallback()
        {
            return Cpuint.cpu_irq_callback(Z80A.zz1[2].cpunum, 0);
        }
        public static byte MReadOp_bublbobl(ushort address)
        {
            byte result = 0;
            if (address >= 0x0040 && address <= 0x00ff)
            {
                int offset = address - 0x0040;
                result = mcuram[offset];
            }
            else if (address >= 0xf000 && address <= 0xffff)
            {
                result = mcurom[address];
            }
            return result;
        }
        public static byte MReadMemory_bublbobl(ushort address)
        {
            byte result = 0;
            if (address == 0x0000)
            {
                result = bublbobl_mcu_ddr1_r();
            }
            else if (address == 0x0001)
            {
                result = bublbobl_mcu_ddr2_r();
            }
            else if (address == 0x0002)
            {
                result = bublbobl_mcu_port1_r();
            }
            else if (address == 0x0003)
            {
                result = bublbobl_mcu_port2_r();
            }
            else if (address == 0x0004)
            {
                result = bublbobl_mcu_ddr3_r();
            }
            else if (address == 0x0005)
            {
                result = bublbobl_mcu_ddr4_r();
            }
            else if (address == 0x0006)
            {
                result = bublbobl_mcu_port3_r();
            }
            else if (address == 0x0007)
            {
                result = bublbobl_mcu_port4_r();
            }
            else if (address >= 0x0040 && address <= 0x00ff)
            {
                int offset = address - 0x0040;
                result = mcuram[offset];
            }
            else if (address >= 0xf000 && address <= 0xffff)
            {
                result = mcurom[address];
            }
            return result;
        }
        public static void MWriteMemory_bublbobl(ushort address, byte value)
        {
            if (address == 0x0000)
            {
                bublbobl_mcu_ddr1_w(value);
            }
            else if (address == 0x0001)
            {
                bublbobl_mcu_ddr2_w(value);
            }
            else if (address == 0x0002)
            {
                bublbobl_mcu_port1_w(value);
            }
            else if (address == 0x0003)
            {
                bublbobl_mcu_port2_w(value);
            }
            else if (address == 0x0004)
            {
                bublbobl_mcu_ddr3_w(value);
            }
            else if (address == 0x0005)
            {
                bublbobl_mcu_ddr4_w(value);
            }
            else if (address == 0x0006)
            {
                bublbobl_mcu_port3_w(value);
            }
            else if (address == 0x0007)
            {
                bublbobl_mcu_port4_w(value);
            }
            else if (address >= 0x0040 && address <= 0x00ff)
            {
                int offset = address - 0x0040;
                mcuram[offset] = value;
            }
            else if (address >= 0xf000 && address <= 0xffff)
            {
                mcurom[address] = value;
            }
        }
        public static byte MReadOp_bootleg(ushort address)
        {
            byte result = 0;
            address &= 0x7ff;
            if (address >= 0x010 && address <= 0x07f)
            {
                result = mcuram[address];
            }
            else if (address >= 0x080 && address <= 0x7ff)
            {
                result = mcurom[address];
            }
            return result;
        }
        public static byte MReadMemory_bootleg(ushort address)
        {
            byte result = 0;
            address &= 0x7ff;
            if (address == 0x000)
            {
                result = bublbobl_68705_portA_r();
            }
            else if (address == 0x001)
            {
                result = bublbobl_68705_portB_r();
            }
            else if (address == 0x002)
            {
                result = (byte)sbyte0;
            }
            else if (address >= 0x010 && address <= 0x07f)
            {
                result = mcuram[address];
            }
            else if (address >= 0x080 && address <= 0x7ff)
            {
                result = mcurom[address];
            }
            return result;
        }
        public static void MWriteMemory_bootleg(ushort address, byte value)
        {
            address &= 0x7ff;
            if (address == 0x000)
            {
                bublbobl_68705_portA_w(value);
            }
            else if (address == 0x001)
            {
                bublbobl_68705_portB_w(value);
            }
            else if (address == 0x004)
            {
                bublbobl_68705_ddrA_w(value);
            }
            else if (address == 0x005)
            {
                bublbobl_68705_ddrB_w(value);
            }
            else if (address == 0x006)
            {

            }
            else if (address >= 0x010 && address <= 0x07f)
            {
                mcuram[address] = value;
            }
            else if (address >= 0x080 && address <= 0x7ff)
            {
                mcurom[address] = value;
            }
        }
        public static sbyte MReadOpByte_opwolf(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address <= 0x03ffff)
            {
                if (address < Memory.mainromLength)
                {
                    result = (sbyte)(Memory.mainrom[address]);
                }
                else
                {
                    result = 0;
                }
            }
            return result;
        }
        public static sbyte MReadByte_opwolf(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            int add1;
            if (address <= 0x03ffff)
            {
                if (address < Memory.mainromLength)
                {
                    result = (sbyte)(Memory.mainrom[address]);
                }
                else
                {
                    result = 0;
                }
            }
            else if (address >= 0x0f0000 && address <= 0x0fffff)
            {
                add1 = address & 0xfff;
                if (add1 >= 0 && add1 <= 0x7ff)
                {
                    int offset = add1 / 2;
                    if (add1 % 2 == 0)
                    {
                        result = (sbyte)(opwolf_cchip_data_r(offset) >> 8);
                    }
                    else if (add1 % 2 == 1)
                    {
                        result = (sbyte)opwolf_cchip_data_r(offset);
                    }
                }
                else if (add1 >= 0x802 && add1 <= 0x803)
                {
                    if (add1 == 0x802)
                    {
                        result = 0;
                    }
                    else if (add1 == 0x803)
                    {
                        result = (sbyte)opwolf_cchip_status_r();
                    }
                }
            }
            else if (address >= 0x100000 && address <= 0x107fff)
            {
                int offset = address - 0x100000;
                result = (sbyte)Memory.mainram[offset];
            }
            else if (address >= 0x200000 && address <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(Generic.paletteram16[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)Generic.paletteram16[offset];
                }
            }
            else if (address >= 0x380000 && address <= 0x380003)
            {
                int offset = (address - 0x380000) / 2;
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)opwolf_dsw_r(offset);
                }
            }
            else if (address >= 0x3a0000 && address <= 0x3a0003)
            {
                int offset = (address - 0x3a0000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(opwolf_lightgun_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)opwolf_lightgun_r(offset);
                }
            }
            else if (address >= 0x3e0000 && address <= 0x3e0001)
            {
                result = 0;
            }
            else if (address >= 0x3e0002 && address <= 0x3e0003)
            {
                if (address % 2 == 0)
                {
                    result = (sbyte)(Taitosnd.taitosound_comm16_msb_r() >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)Taitosnd.taitosound_comm16_msb_r();
                }
            }
            else if (address >= 0xc00000 && address <= 0xc0ffff)
            {
                int offset = (address - 0xc00000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(PC080SN_word_0_r(offset) >> 8);
                }
                else
                {
                    result = (sbyte)PC080SN_word_0_r(offset);
                }
            }
            else if (address >= 0xd00000 && address <= 0xd03fff)
            {
                int offset = (address - 0xd00000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(PC090OJ_word_0_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)PC090OJ_word_0_r(offset);
                }
            }
            return result;
        }
        public static short MReadOpWord_opwolf(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x03ffff)
            {
                if (address + 1 < Memory.mainromLength)
                {
                    result = (short)(Memory.mainrom[address] * 0x100 + Memory.mainrom[address + 1]);
                }
                else
                {
                    result = 0;
                }
            }
            return result;
        }
        public static short MReadWord_opwolf(int address)
        {
            address &= 0xffffff;
            short result = 0;
            int add1;
            if (address <= 0x03ffff)
            {
                if (address + 1 < Memory.mainromLength)
                {
                    result = (short)(Memory.mainrom[address] * 0x100 + Memory.mainrom[address + 1]);
                }
                else
                {
                    result = 0;
                }
            }
            else if (address >= 0x0f0000 && address <= 0x0fffff)
            {
                add1 = address & 0xfff;
                if (add1 >= 0 && add1 <= 0x7ff)
                {
                    int offset = add1 / 2;
                    result = (short)opwolf_cchip_data_r(offset);
                }
                else if (add1 >= 0x802 && add1 <= 0x803)
                {
                    result = (short)opwolf_cchip_status_r();
                }
            }
            else if (address >= 0x100000 && address + 1 <= 0x107fff)
            {
                int offset = address - 0x100000;
                result = (short)(Memory.mainram[offset] * 0x100 + Memory.mainram[offset + 1]);
            }
            else if (address >= 0x200000 && address + 1 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                result = (short)Generic.paletteram16[offset];
            }
            else if (address >= 0x380000 && address + 1 <= 0x380003)
            {
                int offset = (address - 0x380000) / 2;
                result = (short)opwolf_dsw_r(offset);
            }
            else if (address >= 0x3a0000 && address + 1 <= 0x3a0003)
            {
                int offset = (address - 0x3a0000) / 2;
                result = (short)opwolf_lightgun_r(offset);
            }
            else if (address >= 0x3e0000 && address + 1 <= 0x3e0001)
            {
                result = 0;
            }
            else if (address >= 0x3e0002 && address + 1 <= 0x3e0003)
            {
                result = (short)Taitosnd.taitosound_comm16_msb_r();
            }
            else if (address >= 0xc00000 && address + 1 <= 0xc0ffff)
            {
                int offset = (address - 0xc00000) / 2;
                result = (short)PC080SN_word_0_r(offset);
            }
            else if (address >= 0xd00000 && address + 1 <= 0xd03fff)
            {
                int offset = (address - 0xd00000) / 2;
                result = (short)PC090OJ_word_0_r(offset);
            }
            return result;
        }
        public static int MReadOpLong_opwolf(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x03ffff)
            {
                if (address + 3 < Memory.mainromLength)
                {
                    result = (int)(Memory.mainrom[address] * 0x1000000 + Memory.mainrom[address + 1] * 0x10000 + Memory.mainrom[address + 2] * 0x100 + Memory.mainrom[address + 3]);
                }
                else
                {
                    result = 0;
                }
            }
            return result;
        }
        public static int MReadLong_opwolf(int address)
        {
            address &= 0xffffff;
            int result = 0;
            int add1;
            if (address <= 0x03ffff)
            {
                if (address + 3 < Memory.mainromLength)
                {
                    int offset = (address - 0x000000) / 2;
                    result = (int)(Memory.mainrom[address] * 0x1000000 + Memory.mainrom[address + 1] * 0x10000 + Memory.mainrom[address + 2] * 0x100 + Memory.mainrom[address + 3]);
                }
                else
                {
                    result = 0;
                }
            }
            else if (address >= 0x0f0000 && address <= 0x0fffff)
            {
                add1 = address & 0xfff;
                if (add1 >= 0 && add1 <= 0x7ff)
                {
                    int offset = add1 / 2;
                    result = (int)(opwolf_cchip_data_r(offset) * 0x10000 + opwolf_cchip_data_r(offset + 1));
                }
            }
            else if (address >= 0x100000 && address + 3 <= 0x107fff)
            {
                int offset = address - 0x100000;
                result = (int)(Memory.mainram[offset] * 0x1000000 + Memory.mainram[offset + 1] * 0x10000 + Memory.mainram[offset + 2] * 0x100 + Memory.mainram[offset + 3]);
            }
            else if (address >= 0x200000 && address + 3 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                result = (int)(Generic.paletteram16[offset] * 0x10000 + Generic.paletteram16[offset + 1]);
            }
            else if (address >= 0x380000 && address + 3 <= 0x380003)
            {
                int offset = (address - 0x380000) / 2;
                result = (int)(opwolf_dsw_r(offset) * 0x10000 + opwolf_dsw_r(offset + 1));
            }
            else if (address >= 0x3a0000 && address + 3 <= 0x3a0003)
            {
                int offset = (address - 0x3a0000) / 2;
                result = (int)(opwolf_lightgun_r(offset) * 0x10000 + opwolf_lightgun_r(offset + 1));
            }
            else if (address >= 0xc00000 && address + 3 <= 0xc0ffff)
            {
                int offset = (address - 0xc00000) / 2;
                result = (int)(PC080SN_word_0_r(offset) * 0x10000 + PC080SN_word_0_r(offset + 1));
            }
            else if (address >= 0xd00000 && address + 3 <= 0xd03fff)
            {
                int offset = (address - 0xd00000) / 2;
                result = (int)(PC090OJ_word_0_r(offset) * 0x10000 + PC090OJ_word_0_r(offset + 1));
            }
            return result;
        }
        public static void MWriteByte_opwolf(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x000000 && address <= 0x03ffff)
            {
                if (address < Memory.mainromLength)
                {
                    Memory.mainrom[address] = (byte)value;
                }
            }
            else if (address >= 0x0ff000 && address <= 0x0ff7ff)
            {
                int offset = (address - 0x0ff000) / 2;
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    opwolf_cchip_data_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x0ff802 && address <= 0x0ff803)
            {
                opwolf_cchip_status_w();
            }
            else if (address >= 0x0ffc00 && address <= 0x0ffc01)
            {
                if (address == 0x0ffc01)
                {
                    opwolf_cchip_bank_w((byte)value);
                }
            }
            else if (address >= 0x100000 && address <= 0x107fff)
            {
                int offset = address - 0x100000;
                Memory.mainram[offset] = (byte)value;
            }
            else if (address >= 0x200000 && address <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                if (address % 2 == 0)
                {
                    Generic.paletteram16_xxxxRRRRGGGGBBBB_word_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    Generic.paletteram16_xxxxRRRRGGGGBBBB_word_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x380000 && address <= 0x380003)
            {
                int offset = (address - 0x380000) / 2;
                if (address % 2 == 1)
                {
                    opwolf_spritectrl_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x3c0000 && address <= 0x3c0001)
            {
                int i1 = 1;
            }
            else if (address >= 0x3e0000 && address <= 0x3e0001)
            {
                if (address % 2 == 0)
                {
                    Taitosnd.taitosound_port16_msb_w1((byte)value);
                }
            }
            else if (address >= 0x3e0002 && address <= 0x3e0003)
            {
                if (address % 2 == 0)
                {
                    Taitosnd.taitosound_comm16_msb_w1((byte)value);
                }
            }
            else if (address >= 0xc00000 && address <= 0xc0ffff)
            {
                int offset = (address - 0xc00000) / 2;
                if (address % 2 == 0)
                {
                    PC080SN_word_0_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    PC080SN_word_0_w2(offset, (byte)value);
                }
            }
            else if (address >= 0xc10000 && address <= 0xc1ffff)
            {
                int offset = address - 0xc10000;
                mainram2[offset] = (byte)value;
            }
            else if (address >= 0xc20000 && address <= 0xc20003)
            {
                int offset = (address - 0xc20000) / 2;
                if (address % 2 == 0)
                {
                    PC080SN_yscroll_word_0_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    PC080SN_yscroll_word_0_w2(offset, (byte)value);
                }
            }
            else if (address >= 0xc40000 && address <= 0xc40003)
            {
                int offset = (address - 0xc40000) / 2;
                if (address % 2 == 0)
                {
                    PC080SN_xscroll_word_0_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    PC080SN_xscroll_word_0_w2(offset, (byte)value);
                }
            }
            else if (address >= 0xc50000 && address <= 0xc50003)
            {
                int offset = (address - 0xc50000) / 2;
                if (address % 2 == 0)
                {
                    PC080SN_ctrl_word_0_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    PC080SN_ctrl_word_0_w2(offset, (byte)value);
                }
            }
            else if (address >= 0xd00000 && address <= 0xd03fff)
            {
                int offset = (address - 0xd00000) / 2;
                if (address % 2 == 0)
                {
                    PC090OJ_word_0_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    PC090OJ_word_0_w2(offset, (byte)value);
                }
            }
        }
        public static void MWriteWord_opwolf(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x000000 && address + 1 <= 0x03ffff)
            {
                if (address < Memory.mainromLength)
                {
                    Memory.mainrom[address] = (byte)(value >> 8);
                    Memory.mainrom[address + 1] = (byte)value;
                }
            }
            else if (address >= 0x0ff000 && address + 1 <= 0x0ff7ff)
            {
                int offset = (address - 0x0ff000) / 2;
                opwolf_cchip_data_w(offset, (ushort)value);
            }
            else if (address >= 0x0ff802 && address + 1 <= 0x0ff803)
            {
                opwolf_cchip_status_w();
            }
            else if (address >= 0x0ffc00 && address + 1 <= 0x0ffc01)
            {
                opwolf_cchip_bank_w((byte)value);
            }
            else if (address >= 0x100000 && address + 1 <= 0x107fff)
            {
                int offset = address - 0x100000;
                Memory.mainram[offset] = (byte)(value >> 8);
                Memory.mainram[offset + 1] = (byte)value;
            }
            else if (address >= 0x200000 && address + 1 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                Generic.paletteram16_xxxxRRRRGGGGBBBB_word_w(offset, (ushort)value);
            }
            else if (address >= 0x380000 && address + 1 <= 0x380003)
            {
                int offset = (address - 0x380000) / 2;
                opwolf_spritectrl_w(offset, (ushort)value);
            }
            else if (address >= 0x3c0000 && address + 1 <= 0x3c0001)
            {
                int i1 = 1;
            }
            else if (address >= 0x3e0000 && address + 1 <= 0x3e0001)
            {
                Taitosnd.taitosound_port16_msb_w((ushort)value);
            }
            else if (address >= 0x3e0002 && address + 1 <= 0x3e0003)
            {
                Taitosnd.taitosound_comm16_msb_w((ushort)value);
            }
            else if (address >= 0xc00000 && address + 1 <= 0xc0ffff)
            {
                int offset = (address - 0xc00000) / 2;
                PC080SN_word_0_w(offset, (ushort)value);
            }
            else if (address >= 0xc10000 && address + 1 <= 0xc1ffff)
            {
                int offset = address - 0xc10000;
                mainram2[offset] = (byte)(value >> 8);
                mainram2[offset + 1] = (byte)value;
            }
            else if (address >= 0xc20000 && address + 1 <= 0xc20003)
            {
                int offset = (address - 0xc20000) / 2;
                PC080SN_yscroll_word_0_w(offset, (ushort)value);
            }
            else if (address >= 0xc40000 && address + 1 <= 0xc40003)
            {
                int offset = (address - 0xc40000) / 2;
                PC080SN_xscroll_word_0_w(offset, (ushort)value);
            }
            else if (address >= 0xc50000 && address + 1 <= 0xc50003)
            {
                int offset = (address - 0xc50000) / 2;
                PC080SN_ctrl_word_0_w(offset, (ushort)value);
            }
            else if (address >= 0xd00000 && address + 1 <= 0xd03fff)
            {
                int offset = (address - 0xd00000) / 2;
                PC090OJ_word_0_w(offset, (ushort)value);
            }
        }
        public static void MWriteLong_opwolf(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x000000 && address + 3 <= 0x03ffff)
            {
                if (address < Memory.mainromLength)
                {
                    Memory.mainrom[address] = (byte)(value >> 24);
                    Memory.mainrom[address + 1] = (byte)(value >> 16);
                    Memory.mainrom[address + 2] = (byte)(value >> 8);
                    Memory.mainrom[address + 3] = (byte)value;
                }
            }
            else if (address >= 0x0ff000 && address + 3 <= 0x0ff7ff)
            {
                int offset = (address - 0x0ff000) / 2;
                opwolf_cchip_data_w(offset, (ushort)(value >> 16));
                opwolf_cchip_data_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x100000 && address + 3 <= 0x107fff)
            {
                int offset = address - 0x100000;
                Memory.mainram[offset] = (byte)(value >> 24);
                Memory.mainram[offset + 1] = (byte)(value >> 16);
                Memory.mainram[offset + 2] = (byte)(value >> 8);
                Memory.mainram[offset + 3] = (byte)value;
            }
            else if (address >= 0x200000 && address + 3 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                Generic.paletteram16_xxxxRRRRGGGGBBBB_word_w(offset, (ushort)(value >> 16));
                Generic.paletteram16_xxxxRRRRGGGGBBBB_word_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x380000 && address + 3 <= 0x380003)
            {
                int i1 = 1;
            }
            else if (address >= 0xc00000 && address + 3 <= 0xc0ffff)
            {
                int offset = (address - 0xc00000) / 2;
                PC080SN_word_0_w(offset, (ushort)(value >> 16));
                PC080SN_word_0_w(offset + 1, (ushort)value);
            }
            else if (address >= 0xc10000 && address + 3 <= 0xc1ffff)
            {
                int offset = address - 0xc10000;
                mainram2[offset] = (byte)(value >> 24);
                mainram2[offset + 1] = (byte)(value >> 16);
                mainram2[offset + 2] = (byte)(value >> 8);
                mainram2[offset + 3] = (byte)value;
            }
            else if (address >= 0xd00000 && address + 3 <= 0xd03fff)
            {
                int offset = (address - 0xd00000) / 2;
                PC090OJ_word_0_w(offset, (ushort)(value >> 16));
                PC090OJ_word_0_w(offset + 1, (ushort)value);
            }
        }
        public static sbyte MReadByte_opwolfb(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address <= 0x03ffff)
            {
                if (address < Memory.mainromLength)
                {
                    result = (sbyte)(Memory.mainrom[address]);
                }
                else
                {
                    result = 0;
                }
            }
            else if (address >= 0x0f0008 && address <= 0x0f000b)
            {
                int offset = (address - 0x0f0008) / 2;
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)opwolf_in_r(offset);
                }
            }
            else if (address >= 0x0ff000 && address <= 0x0fffff)
            {
                int offset = (address - 0x0ff000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(cchip_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)cchip_r(offset);
                }
            }
            else if (address >= 0x100000 && address <= 0x107fff)
            {
                int offset = address - 0x100000;
                result = (sbyte)Memory.mainram[offset];
            }
            else if (address >= 0x200000 && address <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(Generic.paletteram16[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)Generic.paletteram16[offset];
                }
            }
            else if (address >= 0x380000 && address <= 0x380003)
            {
                int offset = (address - 0x380000) / 2;
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)opwolf_dsw_r(offset);
                }
            }
            else if (address >= 0x3a0000 && address <= 0x3a0003)
            {
                int offset = (address - 0x3a0000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(opwolf_lightgun_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)opwolf_lightgun_r(offset);
                }
            }
            else if (address >= 0x3e0000 && address <= 0x3e0001)
            {
                result = 0;
            }
            else if (address >= 0x3e0002 && address <= 0x3e0003)
            {
                if (address % 2 == 0)
                {
                    result = (sbyte)(Taitosnd.taitosound_comm16_msb_r() >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)Taitosnd.taitosound_comm16_msb_r();
                }
            }
            else if (address >= 0xc00000 && address <= 0xc0ffff)
            {
                int offset = (address - 0xc00000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(PC080SN_word_0_r(offset) >> 8);
                }
                else
                {
                    result = (sbyte)PC080SN_word_0_r(offset);
                }
            }
            else if (address >= 0xd00000 && address <= 0xd03fff)
            {
                int offset = (address - 0xd00000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(PC090OJ_word_0_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)PC090OJ_word_0_r(offset);
                }
            }
            return result;
        }
        public static short MReadOpWord_opwolfb(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x03ffff)
            {
                if (address + 1 < Memory.mainromLength)
                {
                    result = (short)(Memory.mainrom[address] * 0x100 + Memory.mainrom[address + 1]);
                }
                else
                {
                    result = 0;
                }
            }
            return result;
        }
        public static short MReadWord_opwolfb(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x03ffff)
            {
                if (address + 1 < Memory.mainromLength)
                {
                    result = (short)(Memory.mainrom[address] * 0x100 + Memory.mainrom[address + 1]);
                }
                else
                {
                    result = 0;
                }
            }
            else if (address >= 0x0f0008 && address + 1 <= 0x0f000b)
            {
                int offset = (address - 0x0f0008) / 2;
                result = (short)opwolf_in_r(offset);
            }
            else if (address >= 0x0ff000 && address + 1 <= 0x0fffff)
            {
                int offset = (address - 0x0ff000) / 2;
                result = (short)cchip_r(offset);
            }
            else if (address >= 0x100000 && address + 1 <= 0x107fff)
            {
                int offset = address - 0x100000;
                result = (short)(Memory.mainram[offset] * 0x100 + Memory.mainram[offset + 1]);
            }
            else if (address >= 0x200000 && address + 1 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                result = (short)Generic.paletteram16[offset];
            }
            else if (address >= 0x380000 && address + 1 <= 0x380003)
            {
                int offset = (address - 0x380000) / 2;
                result = (short)opwolf_dsw_r(offset);
            }
            else if (address >= 0x3a0000 && address + 1 <= 0x3a0003)
            {
                int offset = (address - 0x3a0000) / 2;
                result = (short)opwolf_lightgun_r(offset);
            }
            else if (address >= 0x3e0000 && address + 1 <= 0x3e0001)
            {
                result = 0;
            }
            else if (address >= 0x3e0002 && address + 1 <= 0x3e0003)
            {
                result = (short)Taitosnd.taitosound_comm16_msb_r();
            }
            else if (address >= 0xc00000 && address + 1 <= 0xc0ffff)
            {
                int offset = (address - 0xc00000) / 2;
                result = (short)PC080SN_word_0_r(offset);
            }
            else if (address >= 0xd00000 && address + 1 <= 0xd03fff)
            {
                int offset = (address - 0xd00000) / 2;
                result = (short)PC090OJ_word_0_r(offset);
            }
            return result;
        }
        public static int MReadOpLong_opwolfb(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x03ffff)
            {
                if (address + 3 < Memory.mainromLength)
                {
                    result = (int)(Memory.mainrom[address] * 0x1000000 + Memory.mainrom[address + 1] * 0x10000 + Memory.mainrom[address + 2] * 0x100 + Memory.mainrom[address + 3]);
                }
                else
                {
                    result = 0;
                }
            }
            return result;
        }
        public static int MReadLong_opwolfb(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x03ffff)
            {
                if (address + 3 < Memory.mainromLength)
                {
                    int offset = (address - 0x000000) / 2;
                    result = (int)(Memory.mainrom[address] * 0x1000000 + Memory.mainrom[address + 1] * 0x10000 + Memory.mainrom[address + 2] * 0x100 + Memory.mainrom[address + 3]);
                }
                else
                {
                    result = 0;
                }
            }
            else if (address >= 0x0f0008 && address + 3 <= 0x0f000b)
            {
                int offset = (address - 0x0f0008) / 2;
                result = (int)(opwolf_in_r(offset) * 0x10000 + opwolf_in_r(offset + 1));
            }
            else if (address >= 0x0ff000 && address + 3 <= 0x0fffff)
            {
                int offset = (address - 0x0ff000) / 2;
                result = (int)(cchip_r(offset) * 0x10000 + cchip_r(offset + 1));
            }
            else if (address >= 0x100000 && address + 3 <= 0x107fff)
            {
                int offset = address - 0x100000;
                result = (int)(Memory.mainram[offset] * 0x1000000 + Memory.mainram[offset + 1] * 0x10000 + Memory.mainram[offset + 2] * 0x100 + Memory.mainram[offset + 3]);
            }
            else if (address >= 0x200000 && address + 3 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                result = (int)(Generic.paletteram16[offset] * 0x10000 + Generic.paletteram16[offset + 1]);
            }
            else if (address >= 0x380000 && address + 3 <= 0x380003)
            {
                int offset = (address - 0x380000) / 2;
                result = (int)(opwolf_dsw_r(offset) * 0x10000 + opwolf_dsw_r(offset + 1));
            }
            else if (address >= 0x3a0000 && address + 3 <= 0x3a0003)
            {
                int offset = (address - 0x3a0000) / 2;
                result = (int)(opwolf_lightgun_r(offset) * 0x10000 + opwolf_lightgun_r(offset + 1));
            }
            else if (address >= 0xc00000 && address + 3 <= 0xc0ffff)
            {
                int offset = (address - 0xc00000) / 2;
                result = (int)(PC080SN_word_0_r(offset) * 0x10000 + PC080SN_word_0_r(offset + 1));
            }
            else if (address >= 0xd00000 && address + 3 <= 0xd03fff)
            {
                int offset = (address - 0xd00000) / 2;
                result = (int)(PC090OJ_word_0_r(offset) * 0x10000 + PC090OJ_word_0_r(offset + 1));
            }
            return result;
        }
        public static void MWriteByte_opwolfb(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x000000 && address <= 0x03ffff)
            {
                if (address < Memory.mainromLength)
                {
                    Memory.mainrom[address] = (byte)value;
                }
            }
            else if (address >= 0x0ff000 && address <= 0x0fffff)
            {
                int offset = (address - 0x0ff000) / 2;
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    cchip_w(offset, (byte)value);
                }
            }
            else if (address >= 0x100000 && address <= 0x107fff)
            {
                int offset = address - 0x100000;
                Memory.mainram[offset] = (byte)value;
            }
            else if (address >= 0x200000 && address <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                if (address % 2 == 0)
                {
                    Generic.paletteram16_xxxxRRRRGGGGBBBB_word_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    Generic.paletteram16_xxxxRRRRGGGGBBBB_word_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x380000 && address <= 0x380003)
            {
                int offset = (address - 0x380000) / 2;
                if (address % 2 == 1)
                {
                    opwolf_spritectrl_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x3c0000 && address <= 0x3c0001)
            {
                int i1 = 1;
            }
            else if (address >= 0x3e0000 && address <= 0x3e0001)
            {
                if (address % 2 == 0)
                {
                    Taitosnd.taitosound_port16_msb_w1((byte)value);
                }
            }
            else if (address >= 0x3e0002 && address <= 0x3e0003)
            {
                if (address % 2 == 0)
                {
                    Taitosnd.taitosound_comm16_msb_w1((byte)value);
                }
            }
            else if (address >= 0xc00000 && address <= 0xc0ffff)
            {
                int offset = (address - 0xc00000) / 2;
                if (address % 2 == 0)
                {
                    PC080SN_word_0_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    PC080SN_word_0_w2(offset, (byte)value);
                }
            }
            else if (address >= 0xc10000 && address <= 0xc1ffff)
            {
                int offset = address - 0xc10000;
                mainram2[offset] = (byte)value;
            }
            else if (address >= 0xc20000 && address <= 0xc20003)
            {
                int offset = (address - 0xc20000) / 2;
                if (address % 2 == 0)
                {
                    PC080SN_yscroll_word_0_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    PC080SN_yscroll_word_0_w2(offset, (byte)value);
                }
            }
            else if (address >= 0xc40000 && address <= 0xc40003)
            {
                int offset = (address - 0xc40000) / 2;
                if (address % 2 == 0)
                {
                    PC080SN_xscroll_word_0_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    PC080SN_xscroll_word_0_w2(offset, (byte)value);
                }
            }
            else if (address >= 0xc50000 && address <= 0xc50003)
            {
                int offset = (address - 0xc50000) / 2;
                if (address % 2 == 0)
                {
                    PC080SN_ctrl_word_0_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    PC080SN_ctrl_word_0_w2(offset, (byte)value);
                }
            }
            else if (address >= 0xd00000 && address <= 0xd03fff)
            {
                int offset = (address - 0xd00000) / 2;
                if (address % 2 == 0)
                {
                    PC090OJ_word_0_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    PC090OJ_word_0_w2(offset, (byte)value);
                }
            }
        }
        public static void MWriteWord_opwolfb(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x000000 && address + 1 <= 0x03ffff)
            {
                if (address < Memory.mainromLength)
                {
                    Memory.mainrom[address] = (byte)(value >> 8);
                    Memory.mainrom[address + 1] = (byte)value;
                }
            }
            else if (address >= 0x0ff000 && address + 1 <= 0x0fffff)
            {
                int offset = (address - 0x0ff000) / 2;
                cchip_w(offset, (byte)value);
            }
            else if (address >= 0x100000 && address + 1 <= 0x107fff)
            {
                int offset = address - 0x100000;
                Memory.mainram[offset] = (byte)(value >> 8);
                Memory.mainram[offset + 1] = (byte)value;
            }
            else if (address >= 0x200000 && address + 1 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                Generic.paletteram16_xxxxRRRRGGGGBBBB_word_w(offset, (ushort)value);
            }
            else if (address >= 0x380000 && address + 1 <= 0x380003)
            {
                int offset = (address - 0x380000) / 2;
                opwolf_spritectrl_w(offset, (ushort)value);
            }
            else if (address >= 0x3c0000 && address + 1 <= 0x3c0001)
            {
                int i1 = 1;
            }
            else if (address >= 0x3e0000 && address + 1 <= 0x3e0001)
            {
                Taitosnd.taitosound_port16_msb_w((ushort)value);
            }
            else if (address >= 0x3e0002 && address + 1 <= 0x3e0003)
            {
                Taitosnd.taitosound_comm16_msb_w((ushort)value);
            }
            else if (address >= 0xc00000 && address + 1 <= 0xc0ffff)
            {
                int offset = (address - 0xc00000) / 2;
                PC080SN_word_0_w(offset, (ushort)value);
            }
            else if (address >= 0xc10000 && address + 1 <= 0xc1ffff)
            {
                int offset = address - 0xc10000;
                mainram2[offset] = (byte)(value >> 8);
                mainram2[offset + 1] = (byte)value;
            }
            else if (address >= 0xc20000 && address + 1 <= 0xc20003)
            {
                int offset = (address - 0xc20000) / 2;
                PC080SN_yscroll_word_0_w(offset, (ushort)value);
            }
            else if (address >= 0xc40000 && address + 1 <= 0xc40003)
            {
                int offset = (address - 0xc40000) / 2;
                PC080SN_xscroll_word_0_w(offset, (ushort)value);
            }
            else if (address >= 0xc50000 && address + 1 <= 0xc50003)
            {
                int offset = (address - 0xc50000) / 2;
                PC080SN_ctrl_word_0_w(offset, (ushort)value);
            }
            else if (address >= 0xd00000 && address + 1 <= 0xd03fff)
            {
                int offset = (address - 0xd00000) / 2;
                PC090OJ_word_0_w(offset, (ushort)value);
            }
        }
        public static void MWriteLong_opwolfb(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x000000 && address + 3 <= 0x03ffff)
            {
                if (address < Memory.mainromLength)
                {
                    Memory.mainrom[address] = (byte)(value >> 24);
                    Memory.mainrom[address + 1] = (byte)(value >> 16);
                    Memory.mainrom[address + 2] = (byte)(value >> 8);
                    Memory.mainrom[address + 3] = (byte)value;
                }
            }
            else if (address >= 0x0ff000 && address + 3 <= 0x0fffff)
            {
                int offset = (address - 0x0ff000) / 2;
                cchip_w(offset, (byte)(value >> 16));
                cchip_w(offset + 1, (byte)value);
            }
            else if (address >= 0x100000 && address + 3 <= 0x107fff)
            {
                int offset = address - 0x100000;
                Memory.mainram[offset] = (byte)(value >> 24);
                Memory.mainram[offset + 1] = (byte)(value >> 16);
                Memory.mainram[offset + 2] = (byte)(value >> 8);
                Memory.mainram[offset + 3] = (byte)value;
            }
            else if (address >= 0x200000 && address + 3 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                Generic.paletteram16_xxxxRRRRGGGGBBBB_word_w(offset, (ushort)(value >> 16));
                Generic.paletteram16_xxxxRRRRGGGGBBBB_word_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x380000 && address + 3 <= 0x380003)
            {
                int i1 = 1;
            }
            else if (address >= 0xc00000 && address + 3 <= 0xc0ffff)
            {
                int offset = (address - 0xc00000) / 2;
                PC080SN_word_0_w(offset, (ushort)(value >> 16));
                PC080SN_word_0_w(offset + 1, (ushort)value);
            }
            else if (address >= 0xc10000 && address + 3 <= 0xc1ffff)
            {
                int offset = address - 0xc10000;
                mainram2[offset] = (byte)(value >> 24);
                mainram2[offset + 1] = (byte)(value >> 16);
                mainram2[offset + 2] = (byte)(value >> 8);
                mainram2[offset + 3] = (byte)value;
            }
            else if (address >= 0xd00000 && address + 3 <= 0xd03fff)
            {
                int offset = (address - 0xd00000) / 2;
                PC090OJ_word_0_w(offset, (ushort)(value >> 16));
                PC090OJ_word_0_w(offset + 1, (ushort)value);
            }
        }
        public static sbyte MReadByte_opwolfp(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address <= 0x03ffff)
            {
                if (address < Memory.mainromLength)
                {
                    result = (sbyte)(Memory.mainrom[address]);
                }
                else
                {
                    result = 0;
                }
            }
            else if (address >= 0x100000 && address <= 0x107fff)
            {
                int offset = address - 0x100000;
                result = (sbyte)Memory.mainram[offset];
            }
            else if (address >= 0x200000 && address <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(Generic.paletteram16[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)Generic.paletteram16[offset];
                }
            }
            else if (address >= 0x380000 && address <= 0x380003)
            {
                int offset = (address - 0x380000) / 2;
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)opwolf_dsw_r(offset);
                }
            }
            else if (address >= 0x3a0000 && address <= 0x3a0003)
            {
                int offset = (address - 0x3a0000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(opwolf_lightgun_r_p(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)opwolf_lightgun_r_p(offset);
                }
            }
            else if (address >= 0x3e0000 && address <= 0x3e0001)
            {
                result = 0;
            }
            else if (address >= 0x3e0002 && address <= 0x3e0003)
            {
                if (address % 2 == 0)
                {
                    result = (sbyte)(Taitosnd.taitosound_comm16_msb_r() >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)Taitosnd.taitosound_comm16_msb_r();
                }
            }
            else if (address >= 0xc00000 && address <= 0xc0ffff)
            {
                int offset = (address - 0xc00000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(PC080SN_word_0_r(offset) >> 8);
                }
                else
                {
                    result = (sbyte)PC080SN_word_0_r(offset);
                }
            }
            else if (address >= 0xd00000 && address <= 0xd03fff)
            {
                int offset = (address - 0xd00000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(PC090OJ_word_0_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)PC090OJ_word_0_r(offset);
                }
            }
            return result;
        }
        public static short MReadWord_opwolfp(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x03ffff)
            {
                if (address + 1 < Memory.mainromLength)
                {
                    result = (short)(Memory.mainrom[address] * 0x100 + Memory.mainrom[address + 1]);
                }
                else
                {
                    result = 0;
                }
            }
            else if (address >= 0x100000 && address + 1 <= 0x107fff)
            {
                int offset = address - 0x100000;
                result = (short)(Memory.mainram[offset] * 0x100 + Memory.mainram[offset + 1]);
            }
            else if (address >= 0x200000 && address + 1 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                result = (short)Generic.paletteram16[offset];
            }
            else if (address >= 0x380000 && address + 1 <= 0x380003)
            {
                int offset = (address - 0x380000) / 2;
                result = (short)opwolf_dsw_r(offset);
            }
            else if (address >= 0x3a0000 && address + 1 <= 0x3a0003)
            {
                int offset = (address - 0x3a0000) / 2;
                result = (short)opwolf_lightgun_r_p(offset);
            }
            else if (address >= 0x3e0000 && address + 1 <= 0x3e0001)
            {
                result = 0;
            }
            else if (address >= 0x3e0002 && address + 1 <= 0x3e0003)
            {
                result = (short)Taitosnd.taitosound_comm16_msb_r();
            }
            else if (address >= 0xc00000 && address + 1 <= 0xc0ffff)
            {
                int offset = (address - 0xc00000) / 2;
                result = (short)PC080SN_word_0_r(offset);
            }
            else if (address >= 0xd00000 && address + 1 <= 0xd03fff)
            {
                int offset = (address - 0xd00000) / 2;
                result = (short)PC090OJ_word_0_r(offset);
            }
            return result;
        }
        public static int MReadLong_opwolfp(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x03ffff)
            {
                if (address + 3 < Memory.mainromLength)
                {
                    int offset = (address - 0x000000) / 2;
                    result = (int)(Memory.mainrom[address] * 0x1000000 + Memory.mainrom[address + 1] * 0x10000 + Memory.mainrom[address + 2] * 0x100 + Memory.mainrom[address + 3]);
                }
                else
                {
                    result = 0;
                }
            }
            else if (address >= 0x100000 && address + 3 <= 0x107fff)
            {
                int offset = address - 0x100000;
                result = (int)(Memory.mainram[offset] * 0x1000000 + Memory.mainram[offset + 1] * 0x10000 + Memory.mainram[offset + 2] * 0x100 + Memory.mainram[offset + 3]);
            }
            else if (address >= 0x200000 && address + 3 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                result = (int)(Generic.paletteram16[offset] * 0x10000 + Generic.paletteram16[offset + 1]);
            }
            else if (address >= 0x380000 && address + 3 <= 0x380003)
            {
                int offset = (address - 0x380000) / 2;
                result = (int)(opwolf_dsw_r(offset) * 0x10000 + opwolf_dsw_r(offset + 1));
            }
            else if (address >= 0x3a0000 && address + 3 <= 0x3a0003)
            {
                int offset = (address - 0x3a0000) / 2;
                result = (int)(opwolf_lightgun_r(offset) * 0x10000 + opwolf_lightgun_r(offset + 1));
            }
            else if (address >= 0xc00000 && address + 3 <= 0xc0ffff)
            {
                int offset = (address - 0xc00000) / 2;
                result = (int)(PC080SN_word_0_r(offset) * 0x10000 + PC080SN_word_0_r(offset + 1));
            }
            else if (address >= 0xd00000 && address + 3 <= 0xd03fff)
            {
                int offset = (address - 0xd00000) / 2;
                result = (int)(PC090OJ_word_0_r(offset) * 0x10000 + PC090OJ_word_0_r(offset + 1));
            }
            return result;
        }
        public static void MWriteByte_opwolfp(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x000000 && address <= 0x03ffff)
            {
                if (address < Memory.mainromLength)
                {
                    Memory.mainrom[address] = (byte)value;
                }
            }
            else if (address >= 0x100000 && address <= 0x107fff)
            {
                int offset = address - 0x100000;
                Memory.mainram[offset] = (byte)value;
            }
            else if (address >= 0x200000 && address <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                if (address % 2 == 0)
                {
                    Generic.paletteram16_xxxxRRRRGGGGBBBB_word_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    Generic.paletteram16_xxxxRRRRGGGGBBBB_word_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x380000 && address <= 0x380003)
            {
                int offset = (address - 0x380000) / 2;
                if (address % 2 == 1)
                {
                    opwolf_spritectrl_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x3c0000 && address <= 0x3c0001)
            {
                int i1 = 1;
            }
            else if (address >= 0x3e0000 && address <= 0x3e0001)
            {
                if (address % 2 == 0)
                {
                    Taitosnd.taitosound_port16_msb_w1((byte)value);
                }
            }
            else if (address >= 0x3e0002 && address <= 0x3e0003)
            {
                if (address % 2 == 0)
                {
                    Taitosnd.taitosound_comm16_msb_w1((byte)value);
                }
            }
            else if (address >= 0xc00000 && address <= 0xc0ffff)
            {
                int offset = (address - 0xc00000) / 2;
                if (address % 2 == 0)
                {
                    PC080SN_word_0_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    PC080SN_word_0_w2(offset, (byte)value);
                }
            }
            else if (address >= 0xc10000 && address <= 0xc1ffff)
            {
                int offset = address - 0xc10000;
                mainram2[offset] = (byte)value;
            }
            else if (address >= 0xc20000 && address <= 0xc20003)
            {
                int offset = (address - 0xc20000) / 2;
                if (address % 2 == 0)
                {
                    PC080SN_yscroll_word_0_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    PC080SN_yscroll_word_0_w2(offset, (byte)value);
                }
            }
            else if (address >= 0xc40000 && address <= 0xc40003)
            {
                int offset = (address - 0xc40000) / 2;
                if (address % 2 == 0)
                {
                    PC080SN_xscroll_word_0_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    PC080SN_xscroll_word_0_w2(offset, (byte)value);
                }
            }
            else if (address >= 0xc50000 && address <= 0xc50003)
            {
                int offset = (address - 0xc50000) / 2;
                if (address % 2 == 0)
                {
                    PC080SN_ctrl_word_0_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    PC080SN_ctrl_word_0_w2(offset, (byte)value);
                }
            }
            else if (address >= 0xd00000 && address <= 0xd03fff)
            {
                int offset = (address - 0xd00000) / 2;
                if (address % 2 == 0)
                {
                    PC090OJ_word_0_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    PC090OJ_word_0_w2(offset, (byte)value);
                }
            }
        }
        public static void MWriteWord_opwolfp(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x000000 && address + 1 <= 0x03ffff)
            {
                if (address < Memory.mainromLength)
                {
                    Memory.mainrom[address] = (byte)(value >> 8);
                    Memory.mainrom[address + 1] = (byte)value;
                }
            }
            else if (address >= 0x100000 && address + 1 <= 0x107fff)
            {
                int offset = address - 0x100000;
                Memory.mainram[offset] = (byte)(value >> 8);
                Memory.mainram[offset + 1] = (byte)value;
            }
            else if (address >= 0x200000 && address + 1 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                Generic.paletteram16_xxxxRRRRGGGGBBBB_word_w(offset, (ushort)value);
            }
            else if (address >= 0x380000 && address + 1 <= 0x380003)
            {
                int offset = (address - 0x380000) / 2;
                opwolf_spritectrl_w(offset, (ushort)value);
            }
            else if (address >= 0x3c0000 && address + 1 <= 0x3c0001)
            {
                int i1 = 1;
            }
            else if (address >= 0x3e0000 && address + 1 <= 0x3e0001)
            {
                Taitosnd.taitosound_port16_msb_w((ushort)value);
            }
            else if (address >= 0x3e0002 && address + 1 <= 0x3e0003)
            {
                Taitosnd.taitosound_comm16_msb_w((ushort)value);
            }
            else if (address >= 0xc00000 && address + 1 <= 0xc0ffff)
            {
                int offset = (address - 0xc00000) / 2;
                PC080SN_word_0_w(offset, (ushort)value);
            }
            else if (address >= 0xc10000 && address + 1 <= 0xc1ffff)
            {
                int offset = address - 0xc10000;
                mainram2[offset] = (byte)(value >> 8);
                mainram2[offset + 1] = (byte)value;
            }
            else if (address >= 0xc20000 && address + 1 <= 0xc20003)
            {
                int offset = (address - 0xc20000) / 2;
                PC080SN_yscroll_word_0_w(offset, (ushort)value);
            }
            else if (address >= 0xc40000 && address + 1 <= 0xc40003)
            {
                int offset = (address - 0xc40000) / 2;
                PC080SN_xscroll_word_0_w(offset, (ushort)value);
            }
            else if (address >= 0xc50000 && address + 1 <= 0xc50003)
            {
                int offset = (address - 0xc50000) / 2;
                PC080SN_ctrl_word_0_w(offset, (ushort)value);
            }
            else if (address >= 0xd00000 && address + 1 <= 0xd03fff)
            {
                int offset = (address - 0xd00000) / 2;
                PC090OJ_word_0_w(offset, (ushort)value);
            }
        }
        public static void MWriteLong_opwolfp(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x000000 && address + 3 <= 0x03ffff)
            {
                if (address < Memory.mainromLength)
                {
                    Memory.mainrom[address] = (byte)(value >> 24);
                    Memory.mainrom[address + 1] = (byte)(value >> 16);
                    Memory.mainrom[address + 2] = (byte)(value >> 8);
                    Memory.mainrom[address + 3] = (byte)value;
                }
            }
            else if (address >= 0x100000 && address + 3 <= 0x107fff)
            {
                int offset = address - 0x100000;
                Memory.mainram[offset] = (byte)(value >> 24);
                Memory.mainram[offset + 1] = (byte)(value >> 16);
                Memory.mainram[offset + 2] = (byte)(value >> 8);
                Memory.mainram[offset + 3] = (byte)value;
            }
            else if (address >= 0x200000 && address + 3 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                Generic.paletteram16_xxxxRRRRGGGGBBBB_word_w(offset, (ushort)(value >> 16));
                Generic.paletteram16_xxxxRRRRGGGGBBBB_word_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x380000 && address + 3 <= 0x380003)
            {
                int i1 = 1;
            }
            else if (address >= 0xc00000 && address + 3 <= 0xc0ffff)
            {
                int offset = (address - 0xc00000) / 2;
                PC080SN_word_0_w(offset, (ushort)(value >> 16));
                PC080SN_word_0_w(offset + 1, (ushort)value);
            }
            else if (address >= 0xc10000 && address + 3 <= 0xc1ffff)
            {
                int offset = address - 0xc10000;
                mainram2[offset] = (byte)(value >> 24);
                mainram2[offset + 1] = (byte)(value >> 16);
                mainram2[offset + 2] = (byte)(value >> 8);
                mainram2[offset + 3] = (byte)value;
            }
            else if (address >= 0xd00000 && address + 3 <= 0xd03fff)
            {
                int offset = (address - 0xd00000) / 2;
                PC090OJ_word_0_w(offset, (ushort)(value >> 16));
                PC090OJ_word_0_w(offset + 1, (ushort)value);
            }
        }
        public static byte ZReadOp_opwolf(ushort address)
        {
            byte result = 0;
            if (address <= 0x3fff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0x4000 && address <= 0x7fff)
            {
                int offset = address - 0x4000;
                result = Memory.audiorom[basebanksnd + offset];
            }
            return result;
        }
        public static byte ZReadMemory_opwolf(ushort address)
        {
            byte result = 0;
            if (address <= 0x3fff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0x4000 && address <= 0x7fff)
            {
                int offset = address - 0x4000;
                result = Memory.audiorom[basebanksnd + offset];
            }
            else if (address >= 0x8000 && address <= 0x8fff)
            {
                int offset = address - 0x8000;
                result = Memory.audioram[offset];
            }
            else if (address == 0x9001)
            {
                result = YM2151.ym2151_status_port_0_r();
            }
            else if (address >= 0x9002 && address <= 0x9100)
            {
                int offset = address - 0x9002;
                result = Memory.audioram[offset];
            }
            else if (address == 0xa001)
            {
                result = Taitosnd.taitosound_slave_comm_r();
            }
            return result;
        }
        public static void ZWriteMemory_opwolf(ushort address, byte value)
        {
            if (address <= 0x3fff)
            {
                Memory.audiorom[address] = value;
            }
            else if (address >= 0x4000 && address <= 0x7fff)
            {
                int offset = address - 0x4000;
                Memory.audiorom[basebanksnd + offset] = value;
            }
            else if (address >= 0x8000 && address <= 0x8fff)
            {
                int offset = address - 0x8000;
                Memory.audioram[offset] = value;
            }
            else if (address == 0x9000)
            {
                YM2151.ym2151_register_port_0_w(value);
            }
            else if (address == 0x9001)
            {
                YM2151.ym2151_data_port_0_w(value);
            }
            else if (address == 0xa000)
            {
                Taitosnd.taitosound_slave_port_w(value);
            }
            else if (address == 0xa001)
            {
                Taitosnd.taitosound_slave_comm_w(value);
            }
            else if (address >= 0xb000 && address <= 0xb006)
            {
                int offset = address - 0xb000;
                opwolf_adpcm_b_w(offset, value);
            }
            else if (address >= 0xc000 && address <= 0xc006)
            {
                int offset = address - 0xc000;
                opwolf_adpcm_c_w(offset, value);
            }
            else if (address == 0xd000)
            {
                opwolf_adpcm_d_w();
            }
            else if (address == 0xe000)
            {
                opwolf_adpcm_e_w();
            }
        }
        public static byte ZReadOp_opwolf_sub(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = subrom[address];
            }
            return result;
        }
        public static byte ZReadMemory_opwolf_sub(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = subrom[address];
            }
            else if (address == 0x8800)
            {
                result = z80_input1_r();
            }
            else if (address == 0x9800)
            {
                result = z80_input2_r();
            }
            else if (address >= 0xc000 && address <= 0xc7ff)
            {
                int offset = address - 0xc000;
                result = cchip_ram[offset];
            }
            return result;
        }
        public static void ZWriteMemory_opwolf_sub(ushort address, byte value)
        {
            if (address <= 0x7fff)
            {
                subrom[address] = value;
            }
            else if (address == 0x8000)
            {
                int offset = address - 0x8000;
                Memory.audioram[offset] = value;
            }
            else if (address == 0x9000)
            {
                int i1 = 1;
            }
            else if (address == 0xa000)
            {
                int i1 = 1;
            }
            else if (address >= 0xc000 && address <= 0xc7ff)
            {
                int offset = address - 0xc000;
                cchip_ram[offset] = value;
            }
        }
        public static byte MReadHardware(ushort address)
        {
            byte result = 0;
            address &= 0xff;
            if (address == 0x01)
            {
                result = (byte)Sound.soundlatch_r();
            }
            return result;
        }
        public static void MWriteHardware(ushort address, byte value)
        {
            address &= 0xff;
        }
        public static int MIRQCallback()
        {
            return 0;
        }
    }
}
