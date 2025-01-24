namespace MAME.Core
{
    public unsafe partial class M92
    {
        public static ushort ushort0, ushort1, ushort2, dsw;
        public static ushort ushort0_old, ushort1_old, ushort2_old;
        public static byte N0ReadOpByte(int address)
        {
            address &= 0xfffff;
            byte result = 0;
            if (address >= 0 && address <= 0xfffff)
            {
                result = Memory.mainrom[address];
            }
            return result;
        }
        public static byte N0ReadByte_m92(int address)
        {
            address &= 0xfffff;
            byte result = 0;
            if (address >= 0 && address <= 0x9ffff)
            {
                result = Memory.mainrom[address];
            }
            else if (address >= 0xa0000 && address <= 0xbffff)
            {
                int offset = address - 0xa0000;
                result = Memory.mainrom[bankaddress + offset];
            }
            else if (address >= 0xc0000 && address <= 0xcffff)
            {
                result = Memory.mainrom[address];
            }
            else if (address >= 0xd0000 && address <= 0xdffff)
            {
                int offset = (address - 0xd0000) / 2;
                if (address % 2 == 0)
                {
                    result = (byte)(m92_vram_data[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (byte)m92_vram_data[offset];
                }
            }
            else if (address >= 0xe0000 && address <= 0xeffff)
            {
                int offset = address - 0xe0000;
                result = Memory.mainram[offset];
            }
            else if (address >= 0xf8000 && address <= 0xf87ff)
            {
                int offset = (address - 0xf8000) / 2;
                result = (byte)Generic.spriteram16[offset];
            }
            else if (address >= 0xf8800 && address <= 0xf8fff)
            {
                int offset = (address - 0xf8800) / 2;
                result = (byte)m92_paletteram_r(offset);
            }
            else if (address >= 0xffff0 && address <= 0xfffff)
            {
                result = Memory.mainrom[address];
            }
            return result;
        }
        public static ushort N0ReadWord_m92(int address)
        {
            address &= 0xfffff;
            ushort result = 0;
            if (address >= 0 && address + 1 <= 0x9ffff)
            {
                result = (ushort)(Memory.mainrom[address] + Memory.mainrom[address + 1] * 0x100);
            }
            else if (address >= 0xa0000 && address + 1 <= 0xbffff)
            {
                int offset = address - 0xa0000;
                result = (ushort)(Memory.mainrom[bankaddress + offset] + Memory.mainrom[bankaddress + offset + 1] * 0x100);
            }
            else if (address >= 0xc0000 && address + 1 <= 0xcffff)
            {
                result = (ushort)(Memory.mainrom[address] + Memory.mainrom[address + 1] * 0x100);
            }
            else if (address >= 0xd0000 && address + 1 <= 0xdffff)
            {
                int offset = (address - 0xd0000) / 2;
                result = m92_vram_data[offset];
            }
            else if (address >= 0xe0000 && address + 1 <= 0xeffff)
            {
                int offset = address - 0xe0000;
                result = (ushort)(Memory.mainram[offset] + Memory.mainram[offset + 1] * 0x100);
            }
            else if (address >= 0xf8000 && address + 1 <= 0xf87ff)
            {
                int offset = (address - 0xf8000) / 2;
                result = Generic.spriteram16[offset];
            }
            else if (address >= 0xf8800 && address + 1 <= 0xf8fff)
            {
                int offset = (address - 0xf8800) / 2;
                result = m92_paletteram_r(offset);
            }
            else if (address >= 0xffff0 && address + 1 <= 0xfffff)
            {
                result = (ushort)(Memory.mainrom[address] + Memory.mainrom[address + 1] * 0x100);
            }
            return result;
        }
        public static void N0WriteByte_m92(int address, byte value)
        {
            address &= 0xfffff;
            if (address >= 0xd0000 && address <= 0xdffff)
            {
                int offset = (address - 0xd0000) / 2;
                if (address % 2 == 0)
                {
                    m92_vram_data[offset] = (ushort)((value << 8) | (m92_vram_data[offset] & 0xff));
                }
                else if (address % 2 == 1)
                {
                    m92_vram_data[offset] = (ushort)((m92_vram_data[offset] & 0xff00) | value);
                }
                m92_vram_w(offset);
            }
            else if (address >= 0xe0000 && address <= 0xeffff)
            {
                int offset = address - 0xe0000;
                Memory.mainram[offset] = value;
            }
            else if (address >= 0xf8000 && address <= 0xf87ff)
            {
                int offset = (address - 0xf8000) / 2;
                Generic.spriteram16[offset] = value;
            }
            else if (address >= 0xf8800 && address <= 0xf8fff)
            {
                int offset = (address - 0xf8800) / 2;
                m92_paletteram_w(offset, value);
            }
            else if (address >= 0xf9000 && address <= 0xf900f)
            {
                int offset = (address - 0xf9000) / 2;
                if (address % 2 == 0)
                {
                    m92_spritecontrol_w1(offset, value);
                }
                else if (address % 2 == 1)
                {
                    m92_spritecontrol_w2(offset, value);
                }
            }
            else if (address >= 0xf9800 && address <= 0xf9801)
            {
                if (address % 2 == 1)
                {
                    m92_videocontrol_w(value);
                }
            }
        }
        public static void N0WriteWord_m92(int address, ushort value)
        {
            address &= 0xfffff;
            if (address >= 0xd0000 && address + 1 <= 0xdffff)
            {
                int offset = (address - 0xd0000) / 2;
                m92_vram_data[offset] = value;
                m92_vram_w(offset);
            }
            else if (address >= 0xe0000 && address + 1 <= 0xeffff)
            {
                int offset = address - 0xe0000;
                Memory.mainram[offset] = (byte)value;
                Memory.mainram[offset + 1] = (byte)(value >> 8);
            }
            else if (address >= 0xf8000 && address + 1 <= 0xf87ff)
            {
                int offset = (address - 0xf8000) / 2;
                Generic.spriteram16[offset] = value;
            }
            else if (address >= 0xf8800 && address + 1 <= 0xf8fff)
            {
                int offset = (address - 0xf8800) / 2;
                m92_paletteram_w(offset, value);
            }
            else if (address >= 0xf9000 && address + 1 <= 0xf900f)
            {
                int offset = (address - 0xf9000) / 2;
                m92_spritecontrol_w(offset, value);
            }
            else if (address >= 0xf9800 && address + 1 <= 0xf9801)
            {
                m92_videocontrol_w((byte)value);
            }
        }
        public static byte N0ReadIOByte_m92(int address)
        {
            byte result = 0;
            if (address >= 0x00 && address <= 0x01)
            {
                if (address == 0x00)
                {
                    result = (byte)ushort0;
                }
                else if (address == 0x01)
                {
                    result = (byte)(ushort0 >> 8);
                }
            }
            else if (address >= 0x02 && address <= 0x03)
            {
                result = (byte)ushort1;
            }
            else if (address >= 0x04 && address <= 0x05)
            {
                result = (byte)dsw;
            }
            else if (address >= 0x06 && address <= 0x07)
            {
                result = (byte)ushort2;
            }
            else if (address >= 0x08 && address <= 0x09)
            {
                result = (byte)m92_sound_status_r();
            }
            return result;
        }
        public static ushort N0ReadIOWord_m92(int address)
        {
            ushort result = 0;
            if (address >= 0x00 && address + 1 <= 0x01)
            {
                result = ushort0;
            }
            else if (address >= 0x02 && address + 1 <= 0x03)
            {
                result = ushort1;
                result = (ushort)(result | (m92_sprite_busy_r() * 0x80));
            }
            else if (address >= 0x04 && address + 1 <= 0x05)
            {
                result = dsw;
            }
            else if (address >= 0x06 && address + 1 <= 0x07)
            {
                result = ushort2;
            }
            else if (address >= 0x08 && address + 1 <= 0x09)
            {
                result = m92_sound_status_r();
            }
            return result;
        }
        public static void N0WriteIOByte_m92(int address, byte value)
        {
            if (address >= 0x00 && address <= 0x01)
            {
                m92_soundlatch_w(value);
            }
            else if (address >= 0x02 && address <= 0x03)
            {
                m92_coincounter_w(value);
            }
            else if (address >= 0x20 && address <= 0x21)
            {
                m92_bankswitch_w(value);
            }
            else if (address >= 0x40 && address <= 0x43)
            {

            }
            else if (address >= 0x80 && address <= 0x87)
            {
                int offset = (address - 0x80) / 2;
                if (address % 2 == 0)
                {
                    m92_pf1_control_w2(offset, value);
                }
                else if (address % 2 == 1)
                {
                    m92_pf1_control_w1(offset, value);
                }
            }
            else if (address >= 0x88 && address <= 0x8f)
            {
                int offset = (address - 0x88) / 2;
                if (address % 2 == 0)
                {
                    m92_pf2_control_w2(offset, value);
                }
                else if (address % 2 == 1)
                {
                    m92_pf2_control_w1(offset, value);
                }
            }
            else if (address >= 0x90 && address <= 0x97)
            {
                int offset = (address - 0x90) / 2;
                if (address % 2 == 0)
                {
                    m92_pf3_control_w2(offset, value);
                }
                else if (address % 2 == 1)
                {
                    m92_pf3_control_w1(offset, value);
                }
            }
            else if (address >= 0x98 && address <= 0x9f)
            {
                int offset = (address - 0x98) / 2;
                if (address % 2 == 0)
                {
                    m92_master_control_w2(offset, value);
                }
                else if (address % 2 == 1)
                {
                    m92_master_control_w1(offset, value);
                }
            }
        }
        public static void N0WriteIOWord_m92(int address, ushort value)
        {
            if (address >= 0x00 && address + 1 <= 0x01)
            {
                m92_soundlatch_w(value);
            }
            else if (address >= 0x02 && address + 1 <= 0x03)
            {
                m92_coincounter_w((byte)value);
            }
            else if (address >= 0x20 && address + 1 <= 0x21)
            {
                m92_bankswitch_w((byte)value);
            }
            else if (address >= 0x40 && address + 1 <= 0x43)
            {

            }
            else if (address >= 0x80 && address + 1 <= 0x87)
            {
                int offset = (address - 0x80) / 2;
                m92_pf1_control_w(offset, value);
            }
            else if (address >= 0x88 && address + 1 <= 0x8f)
            {
                int offset = (address - 0x88) / 2;
                m92_pf2_control_w(offset, value);
            }
            else if (address >= 0x90 && address + 1 <= 0x97)
            {
                int offset = (address - 0x90) / 2;
                m92_pf3_control_w(offset, value);
            }
            else if (address >= 0x98 && address + 1 <= 0x9f)
            {
                int offset = (address - 0x98) / 2;
                m92_master_control_w(offset, value);
            }
        }
        public static byte N1ReadOpByte(int address)
        {
            address &= 0xfffff;
            byte result = 0;
            if (address >= 0 && address <= 0x1ffff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0xffff0 && address <= 0xfffff)
            {
                int offset = address - 0xe0000;
                result = Memory.audiorom[offset];
            }
            return result;
        }
        public static byte N1ReadByte(int address)
        {
            address &= 0xfffff;
            byte result = 0;
            if (address >= 0 && address <= 0x1ffff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0xa0000 && address <= 0xa3fff)
            {
                int offset = address - 0xa0000;
                result = Memory.audioram[offset];
            }
            else if (address >= 0xa8000 && address <= 0xa803f)
            {
                int offset = (address - 0xa8000) / 2;
                result = (byte)Iremga20.irem_ga20_r(offset);
            }
            else if (address >= 0xa8042 && address <= 0xa8043)
            {
                result = YM2151.ym2151_status_port_0_r();
            }
            else if (address >= 0xa8044 && address <= 0xa8045)
            {
                result = (byte)m92_soundlatch_r();
            }
            else if (address >= 0xffff0 && address <= 0xfffff)
            {
                int offset = address - 0xe0000;
                result = Memory.mainrom[offset];
            }
            return result;
        }
        public static ushort N1ReadWord(int address)
        {
            address &= 0xfffff;
            ushort result = 0;
            if (address >= 0 && address + 1 <= 0x1ffff)
            {
                result = (ushort)(Memory.audiorom[address] + Memory.audiorom[address + 1] * 0x100);
            }
            else if (address >= 0xa0000 && address + 1 <= 0xa3fff)
            {
                int offset = address - 0xa0000;
                result = (ushort)(Memory.audioram[offset] + Memory.audioram[offset + 1] * 0x100);
            }
            else if (address >= 0xa8000 && address + 1 <= 0xa803f)
            {
                int offset = (address - 0xa8000) / 2;
                result = Iremga20.irem_ga20_r(offset);
            }
            else if (address >= 0xa8042 && address + 1 <= 0xa8043)
            {
                result = YM2151.ym2151_status_port_0_r();
            }
            else if (address >= 0xa8044 && address + 1 <= 0xa8045)
            {
                result = m92_soundlatch_r();
            }
            else if (address >= 0xffff0 && address + 1 <= 0xfffff)
            {
                int offset = address - 0xe0000;
                result = (ushort)(Memory.mainrom[offset] + Memory.mainrom[offset + 1] * 0x100);
            }
            return result;
        }
        public static void N1WriteByte(int address, byte value)
        {
            address &= 0xfffff;
            if (address >= 0x9ff00 && address <= 0x9ffff)
            {

            }
            else if (address >= 0xa0000 && address <= 0xa3fff)
            {
                int offset = address - 0xa0000;
                Memory.audioram[offset] = value;
            }
            else if (address >= 0xa8000 && address <= 0xa803f)
            {
                int offset = (address - 0xa8000) / 2;
                Iremga20.irem_ga20_w(offset, value);
            }
            else if (address >= 0xa8040 && address <= 0xa8041)
            {
                YM2151.ym2151_register_port_0_w(value);
            }
            else if (address >= 0xa8042 && address <= 0xa8043)
            {
                YM2151.ym2151_data_port_0_w(value);
            }
            else if (address >= 0xa8044 && address <= 0xa8045)
            {
                m92_sound_irq_ack_w();
            }
            else if (address >= 0xa8046 && address <= 0xa8047)
            {
                m92_sound_status_w(value);
            }
        }
        public static void N1WriteWord(int address, ushort value)
        {
            address &= 0xfffff;
            if (address >= 0x9ff00 && address + 1 <= 0x9ffff)
            {

            }
            else if (address >= 0xa0000 && address + 1 <= 0xa3fff)
            {
                int offset = address - 0xa0000;
                Memory.audioram[offset] = (byte)value;
                Memory.audioram[offset + 1] = (byte)(value >> 8);
            }
            else if (address >= 0xa8000 && address + 1 <= 0xa803f)
            {
                int offset = (address - 0xa8000) / 2;
                Iremga20.irem_ga20_w(offset, value);
            }
            else if (address >= 0xa8040 && address + 1 <= 0xa8041)
            {
                YM2151.ym2151_register_port_0_w((byte)value);
            }
            else if (address >= 0xa8042 && address + 1 <= 0xa8043)
            {
                YM2151.ym2151_data_port_0_w((byte)value);
            }
            else if (address >= 0xa8044 && address + 1 <= 0xa8045)
            {
                m92_sound_irq_ack_w();
            }
            else if (address >= 0xa8046 && address + 1 <= 0xa8047)
            {
                m92_sound_status_w(value);
            }
        }
    }
}
