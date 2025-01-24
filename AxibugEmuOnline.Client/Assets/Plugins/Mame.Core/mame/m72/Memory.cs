namespace MAME.Core
{
    public unsafe partial class M72
    {
        public static ushort ushort0, ushort1, dsw;
        public static ushort ushort0_old, ushort1_old;
        public static byte NReadOpByte(int address)
        {
            address &= 0xfffff;
            byte result = 0;
            if (address >= 0 && address <= 0xfffff)
            {
                result = Memory.mainrom[address];
            }
            return result;
        }
        public static byte NReadByte_m72(int address)
        {
            address &= 0xfffff;
            byte result = 0;
            if (address >= 0 && address <= 0x7ffff)
            {
                result = Memory.mainrom[address];
            }
            else if (address >= 0xa0000 && address <= 0xa3fff)
            {
                result = Memory.mainram[address - 0xa0000];
            }
            else if (address >= 0xc0000 && address <= 0xc03ff)
            {
                int offset = (address - 0xc0000) / 2;
                result = (byte)Generic.spriteram16[offset];
            }
            else if (address >= 0xc8000 && address <= 0xc8bff)
            {
                int offset = (address - 0xc8000) / 2;
                result = (byte)m72_palette1_r(offset);
            }
            else if (address >= 0xcc000 && address <= 0xccbff)
            {
                int offset = (address - 0xcc000) / 2;
                result = (byte)m72_palette2_r(offset);
            }
            else if (address >= 0xd0000 && address <= 0xd3fff)
            {
                int offset = address - 0xd0000;
                result = m72_videoram1[offset];
            }
            else if (address >= 0xd8000 && address <= 0xdbfff)
            {
                int offset = address - 0xd8000;
                result = m72_videoram2[offset];
            }
            else if (address >= 0xe0000 && address <= 0xeffff)
            {
                int offset = address - 0xe0000;
                result = soundram_r(offset);
            }
            else if (address >= 0xffff0 && address <= 0xfffff)
            {
                result = Memory.mainrom[address & 0xfffff];
            }
            return result;
        }
        public static ushort NReadWord_m72(int address)
        {
            address &= 0xfffff;
            ushort result = 0;
            if (address >= 0 && address + 1 <= 0x7ffff)
            {
                result = (ushort)(Memory.mainrom[address] + Memory.mainrom[address + 1] * 0x100);
            }
            else if (address >= 0xa0000 && address + 1 <= 0xa3fff)
            {
                result = (ushort)(Memory.mainram[address - 0xa0000] + Memory.mainram[address - 0xa0000 + 1] * 0x100);
            }
            else if (address >= 0xc0000 && address + 1 <= 0xc03ff)
            {
                int offset = (address - 0xc0000) / 2;
                result = Generic.spriteram16[offset];
            }
            else if (address >= 0xc8000 && address + 1 <= 0xc8bff)
            {
                int offset = (address - 0xc8000) / 2;
                result = m72_palette1_r(offset);
            }
            else if (address >= 0xcc000 && address + 1 <= 0xccbff)
            {
                int offset = (address - 0xcc000) / 2;
                result = m72_palette2_r(offset);
            }
            else if (address >= 0xd0000 && address + 1 <= 0xd3fff)
            {
                int offset = address - 0xd0000;
                result = (ushort)(m72_videoram1[offset] + m72_videoram1[offset + 1] * 0x100);
            }
            else if (address >= 0xd8000 && address + 1 <= 0xdbfff)
            {
                int offset = address - 0xd8000;
                result = (ushort)(m72_videoram2[offset] + m72_videoram2[offset + 1] * 0x100);
            }
            else if (address >= 0xe0000 && address + 1 <= 0xeffff)
            {
                int offset = (address - 0xe0000) / 2;
                result = soundram_r2(offset);
            }
            else if (address >= 0xffff0 && address + 1 <= 0xfffff)
            {
                result = (ushort)(Memory.mainrom[address] + Memory.mainrom[address + 1] * 0x100);
            }
            return result;
        }
        public static void NWriteByte_m72(int address, byte value)
        {
            address &= 0xfffff;
            if (address >= 0xa0000 && address <= 0xa3fff)
            {
                Memory.mainram[address - 0xa0000] = value;
            }
            else if (address >= 0xc0000 && address <= 0xc03ff)
            {
                int offset = (address - 0xc0000) / 2;
                Generic.spriteram16[offset] = value;
            }
            else if (address >= 0xc8000 && address <= 0xc8bff)
            {
                int offset = (address - 0xc8000) / 2;
                m72_palette1_w(offset, value);
            }
            else if (address >= 0xcc000 && address <= 0xccbff)
            {
                int offset = (address - 0xcc000) / 2;
                m72_palette2_w(offset, value);
            }
            else if (address >= 0xd0000 && address <= 0xd3fff)
            {
                int offset = address - 0xd0000;
                m72_videoram1_w(offset, value);
            }
            else if (address >= 0xd8000 && address <= 0xdbfff)
            {
                int offset = address - 0xd8000;
                m72_videoram2_w(offset, value);
            }
            else if (address >= 0xe0000 && address <= 0xeffff)
            {
                int offset = address - 0xe0000;
                soundram_w(offset, value);
            }
        }
        public static void NWriteWord_m72(int address, ushort value)
        {
            address &= 0xfffff;
            if (address >= 0xa0000 && address + 1 <= 0xa3fff)
            {
                Memory.mainram[address - 0xa0000] = (byte)value;
                Memory.mainram[address - 0xa0000 + 1] = (byte)(value >> 8);
            }
            else if (address >= 0xc0000 && address + 1 <= 0xc03ff)
            {
                int offset = (address - 0xc0000) / 2;
                Generic.spriteram16[offset] = value;
            }
            else if (address >= 0xc8000 && address + 1 <= 0xc8bff)
            {
                int offset = (address - 0xc8000) / 2;
                m72_palette1_w(offset, value);
            }
            else if (address >= 0xcc000 && address + 1 <= 0xccbff)
            {
                int offset = (address - 0xcc000) / 2;
                m72_palette2_w(offset, value);
            }
            else if (address >= 0xd0000 && address + 1 <= 0xd3fff)
            {
                int offset = (address - 0xd0000) / 2;
                m72_videoram1_w(offset, value);
            }
            else if (address >= 0xd8000 && address + 1 <= 0xdbfff)
            {
                int offset = (address - 0xd8000) / 2;
                m72_videoram2_w(offset, value);
            }
            else if (address >= 0xe0000 && address + 1 <= 0xeffff)
            {
                int offset = (address - 0xe0000) / 2;
                soundram_w(offset, value);
            }
        }
        public static byte NReadIOByte(int address)
        {
            byte result = 0;
            if (address >= 0x00 && address <= 0x01)
            {
                result = (byte)ushort0;
            }
            else if (address >= 0x02 && address <= 0x03)
            {
                result = (byte)ushort1;
            }
            else if (address >= 0x04 && address <= 0x05)
            {
                result = (byte)dsw;
            }
            return result;
        }
        public static ushort NReadIOWord(int address)
        {
            ushort result = 0;
            if (address >= 0x00 && address + 1 <= 0x01)
            {
                result = ushort0;
            }
            else if (address >= 0x02 && address + 1 <= 0x03)
            {
                result = ushort1;
            }
            else if (address >= 0x04 && address + 1 <= 0x05)
            {
                result = dsw;
            }
            return result;
        }
        public static void NWriteIOByte_m72(int address, byte value)
        {
            if (address >= 0x00 && address <= 0x01)
            {
                m72_sound_command_w(0, value);
            }
            else if (address >= 0x02 && address <= 0x03)
            {
                m72_port02_w(value);
            }
            else if (address >= 0x04 && address <= 0x05)
            {
                m72_dmaon_w(value);
            }
            else if (address >= 0x06 && address <= 0x07)
            {
                m72_irq_line_w(value);
            }
            else if (address >= 0x40 && address <= 0x43)
            {

            }
            else if (address >= 0x80 && address <= 0x81)
            {
                m72_scrolly1_w(value);
            }
            else if (address >= 0x82 && address <= 0x83)
            {
                m72_scrollx1_w(value);
            }
            else if (address >= 0x84 && address <= 0x85)
            {
                m72_scrolly2_w(value);
            }
            else if (address >= 0x86 && address <= 0x87)
            {
                m72_scrollx2_w(value);
            }
        }
        public static void NWriteIOWord_m72(int address, ushort value)
        {
            if (address >= 0x00 && address + 1 <= 0x01)
            {
                m72_sound_command_w(0, value);
            }
            else if (address >= 0x02 && address + 1 <= 0x03)
            {
                m72_port02_w(value);
            }
            else if (address >= 0x04 && address + 1 <= 0x05)
            {
                m72_dmaon_w(value);
            }
            else if (address >= 0x06 && address + 1 <= 0x07)
            {
                m72_irq_line_w(value);
            }
            else if (address >= 0x40 && address + 1 <= 0x43)
            {

            }
            else if (address >= 0x80 && address + 1 <= 0x81)
            {
                m72_scrolly1_w(value);
            }
            else if (address >= 0x82 && address + 1 <= 0x83)
            {
                m72_scrollx1_w(value);
            }
            else if (address >= 0x84 && address + 1 <= 0x85)
            {
                m72_scrolly2_w(value);
            }
            else if (address >= 0x86 && address + 1 <= 0x87)
            {
                m72_scrollx2_w(value);
            }
        }
        public static byte NReadByte_kengo(int address)
        {
            address &= 0xfffff;
            byte result = 0;
            if (address >= 0 && address <= 0x7ffff)
            {
                result = Memory.mainrom[address];
            }
            else if (address >= 0x80000 && address <= 0x83fff)
            {
                int offset = address - 0x80000;
                result = m72_videoram1[offset];
            }
            else if (address >= 0x84000 && address <= 0x87fff)
            {
                int offset = address - 0x84000;
                result = m72_videoram2[offset];
            }
            else if (address >= 0xa0000 && address <= 0xa0bff)
            {
                int offset = (address - 0xa0000) / 2;
                result = (byte)m72_palette1_r(offset);
            }
            else if (address >= 0xa8000 && address <= 0xa8bff)
            {
                int offset = (address - 0xa8000) / 2;
                result = (byte)m72_palette2_r(offset);
            }
            else if (address >= 0xc0000 && address <= 0xc03ff)
            {
                int offset = (address - 0xc0000) / 2;
                result = (byte)Generic.spriteram16[offset];
            }
            else if (address >= 0xe0000 && address <= 0xe3fff)
            {
                result = Memory.mainram[address - 0xe0000];
            }
            else if (address >= 0xffff0 && address <= 0xfffff)
            {
                result = Memory.mainrom[address & 0xfffff];
            }
            return result;
        }
        public static ushort NReadWord_kengo(int address)
        {
            address &= 0xfffff;
            ushort result = 0;
            if (address >= 0 && address + 1 <= 0x7ffff)
            {
                result = (ushort)(Memory.mainrom[address] + Memory.mainrom[address + 1] * 0x100);
            }
            else if (address >= 0x80000 && address + 1 <= 0x83fff)
            {
                int offset = address - 0x80000;
                result = (ushort)(m72_videoram1[offset] + m72_videoram1[offset + 1] * 0x100);
            }
            else if (address >= 0x84000 && address + 1 <= 0x87fff)
            {
                int offset = address - 0x84000;
                result = (ushort)(m72_videoram2[offset] + m72_videoram2[offset + 1] * 0x100);
            }
            else if (address >= 0xa0000 && address + 1 <= 0xa0bff)
            {
                int offset = (address - 0xa0000) / 2;
                result = m72_palette1_r(offset);
            }
            else if (address >= 0xa8000 && address + 1 <= 0xa8bff)
            {
                int offset = (address - 0xa8000) / 2;
                result = m72_palette2_r(offset);
            }
            else if (address >= 0xc0000 && address + 1 <= 0xc03ff)
            {
                int offset = (address - 0xc0000) / 2;
                result = Generic.spriteram16[offset];
            }
            else if (address >= 0xe0000 && address + 1 <= 0xe3fff)
            {
                result = (ushort)(Memory.mainram[address - 0xe0000] + Memory.mainram[address - 0xe0000 + 1] * 0x100);
            }
            else if (address >= 0xffff0 && address + 1 <= 0xfffff)
            {
                result = (ushort)(Memory.mainrom[address] + Memory.mainrom[address + 1] * 0x100);
            }
            return result;
        }
        public static void NWriteByte_kengo(int address, byte value)
        {
            address &= 0xfffff;
            if (address >= 0x80000 && address <= 0x83fff)
            {
                int offset = address - 0x80000;
                m72_videoram1_w(offset, value);
            }
            else if (address >= 0x84000 && address <= 0x87fff)
            {
                int offset = address - 0x84000;
                m72_videoram2_w(offset, value);
            }
            else if (address >= 0xa0000 && address <= 0xa0bff)
            {
                int offset = (address - 0xa0000) / 2;
                m72_palette1_w(offset, value);
            }
            else if (address >= 0xa8000 && address <= 0xa8bff)
            {
                int offset = (address - 0xa8000) / 2;
                m72_palette2_w(offset, value);
            }
            else if (address >= 0xb0000 && address <= 0xb0001)
            {
                m72_irq_line_w(value);
            }
            else if (address >= 0xbc000 && address <= 0xbc001)
            {
                m72_dmaon_w(value);
            }
            else if (address >= 0xc0000 && address <= 0xc03ff)
            {
                int offset = (address - 0xc0000) / 2;
                Generic.spriteram16[offset] = value;
            }
            else if (address >= 0xe0000 && address <= 0xe3fff)
            {
                Memory.mainram[address - 0xe0000] = value;
            }
        }
        public static void NWriteWord_kengo(int address, ushort value)
        {
            address &= 0xfffff;
            if (address >= 0x80000 && address + 1 <= 0x83fff)
            {
                int offset = (address - 0x80000) / 2;
                m72_videoram1_w(offset, value);
            }
            else if (address >= 0x84000 && address + 1 <= 0x87fff)
            {
                int offset = (address - 0x84000) / 2;
                m72_videoram2_w(offset, value);
            }
            else if (address >= 0xa0000 && address + 1 <= 0xa0bff)
            {
                int offset = (address - 0xa0000) / 2;
                m72_palette1_w(offset, value);
            }
            else if (address >= 0xa8000 && address + 1 <= 0xa8bff)
            {
                int offset = (address - 0xa8000) / 2;
                m72_palette2_w(offset, value);
            }
            else if (address >= 0xb0000 && address + 1 <= 0xb0001)
            {
                m72_irq_line_w(value);
            }
            else if (address >= 0xbc000 && address + 1 <= 0xbc001)
            {
                m72_dmaon_w(value);
            }
            else if (address >= 0xc0000 && address + 1 <= 0xc03ff)
            {
                int offset = (address - 0xc0000) / 2;
                Generic.spriteram16[offset] = value;
            }
            else if (address >= 0xe0000 && address + 1 <= 0xe3fff)
            {
                Memory.mainram[address - 0xe0000] = (byte)value;
                Memory.mainram[address - 0xe0000 + 1] = (byte)(value >> 8);
            }
        }
        public static void NWriteIOByte_kengo(int address, byte value)
        {
            if (address >= 0x00 && address <= 0x01)
            {
                m72_sound_command_w(0, value);
            }
            else if (address >= 0x02 && address <= 0x03)
            {
                rtype2_port02_w(value);
            }
            else if (address >= 0x80 && address <= 0x81)
            {
                m72_scrolly1_w(value);
            }
            else if (address >= 0x82 && address <= 0x83)
            {
                m72_scrollx1_w(value);
            }
            else if (address >= 0x84 && address <= 0x85)
            {
                m72_scrolly2_w(value);
            }
            else if (address >= 0x86 && address <= 0x87)
            {
                m72_scrollx2_w(value);
            }
            else
            {
                int i1 = 1;
            }
        }
        public static void NWriteIOWord_kengo(int address, ushort value)
        {
            if (address >= 0x00 && address + 1 <= 0x01)
            {
                m72_sound_command_w(0, value);
            }
            else if (address >= 0x02 && address + 1 <= 0x03)
            {
                rtype2_port02_w(value);
            }
            else if (address >= 0x80 && address + 1 <= 0x81)
            {
                m72_scrolly1_w(value);
            }
            else if (address >= 0x82 && address + 1 <= 0x83)
            {
                m72_scrollx1_w(value);
            }
            else if (address >= 0x84 && address + 1 <= 0x85)
            {
                m72_scrolly2_w(value);
            }
            else if (address >= 0x86 && address + 1 <= 0x87)
            {
                m72_scrollx2_w(value);
            }
            else
            {
                int i1 = 1;
            }
        }
        public static byte ZReadOp(ushort address)
        {
            byte result = 0;
            if (address >= 0 && address <= 0xffff)
            {
                result = Memory.audiorom[address];
            }
            return result;
        }
        public static byte ZReadMemory_ram(ushort address)
        {
            byte result = 0;
            if (address >= 0 && address <= 0xffff)
            {
                result = Memory.audioram[address];
            }
            return result;
        }
        public static byte ZReadMemory_rom(ushort address)
        {
            byte result = 0;
            if (address >= 0 && address <= 0xefff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0xf000 && address <= 0xffff)
            {
                result = Memory.audioram[address - 0xf000];
            }
            return result;
        }
        public static void ZWriteMemory_ram(ushort address, byte value)
        {
            if (address >= 0x0000 && address <= 0xffff)
            {
                Memory.audioram[address] = value;
            }
        }
        public static void ZWriteMemory_rom(ushort address, byte value)
        {
            if (address >= 0xf000 && address <= 0xffff)
            {
                Memory.audioram[address - 0xf000] = value;
            }
        }
        public static byte ZReadHardware(ushort address)
        {
            byte result = 0;
            address &= 0xff;
            if (address == 0x01)
            {
                result = YM2151.ym2151_status_port_0_r();
            }
            else if (address == 0x02)
            {
                result = (byte)Sound.soundlatch_r();
            }
            else if (address == 0x84)
            {
                result = m72_sample_r();
            }

            return result;
        }
        public static byte ZReadHardware_rtype2(ushort address)
        {
            byte result = 0;
            address &= 0xff;
            if (address == 0x01)
            {
                result = YM2151.ym2151_status_port_0_r();
            }
            else if (address == 0x80)
            {
                result = (byte)Sound.soundlatch_r();
            }
            else if (address == 0x84)
            {
                result = m72_sample_r();
            }

            return result;
        }
        public static void ZWriteHardware(ushort address, byte value)
        {
            address &= 0xff;
            if (address == 0x00)
            {
                YM2151.ym2151_register_port_0_w(value);
            }
            else if (address == 0x01)
            {
                YM2151.ym2151_data_port_0_w(value);
            }
            else if (address == 0x06)
            {
                m72_sound_irq_ack_w(0, value);
            }
            else if (address == 0x82)
            {
                m72_sample_w(value);
            }
        }
        public static void ZWriteHardware_rtype2(ushort address, byte value)
        {
            address &= 0xff;
            if (address == 0x00)
            {
                YM2151.ym2151_register_port_0_w(value);
            }
            else if (address == 0x01)
            {
                YM2151.ym2151_data_port_0_w(value);
            }
            else if (address >= 0x80 && address <= 0x81)
            {
                int offset = address - 0x80;
                rtype2_sample_addr_w(offset, value);
            }
            else if (address == 0x82)
            {
                m72_sample_w(value);
            }
            else if (address == 0x83)
            {
                m72_sound_irq_ack_w(0, value);
            }
        }
        public static int ZIRQCallback()
        {
            return Cpuint.cpu_irq_callback(1, 0);
        }
    }
}
