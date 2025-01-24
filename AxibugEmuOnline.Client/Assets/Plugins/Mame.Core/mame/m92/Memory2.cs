namespace MAME.Core
{
    public unsafe partial class M92
    {
        public static byte N0ReadByte_lethalth(int address)
        {
            address &= 0xfffff;
            byte result = 0;
            if (address >= 0 && address <= 0x7ffff)
            {
                result = Memory.mainrom[address];
            }
            else if (address >= 0x80000 && address <= 0x8ffff)
            {
                int offset = (address - 0x80000) / 2;
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
        public static ushort N0ReadWord_lethalth(int address)
        {
            address &= 0xfffff;
            ushort result = 0;
            if (address >= 0 && address + 1 <= 0x7ffff)
            {
                result = (ushort)(Memory.mainrom[address] + Memory.mainrom[address + 1] * 0x100);
            }
            else if (address >= 0x80000 && address + 1 <= 0x8ffff)
            {
                int offset = (address - 0x80000) / 2;
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
        public static void N0WriteByte_lethalth(int address, byte value)
        {
            address &= 0xfffff;
            if (address >= 0x80000 && address <= 0x8ffff)
            {
                int offset = (address - 0x80000) / 2;
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
        public static void N0WriteWord_lethalth(int address, ushort value)
        {
            address &= 0xfffff;
            if (address >= 0x80000 && address + 1 <= 0x8ffff)
            {
                int offset = (address - 0x80000) / 2;
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
        public static void N0WriteIOByte_lethalth(int address, byte value)
        {
            if (address >= 0x00 && address <= 0x01)
            {
                m92_soundlatch_w(value);
            }
            else if (address >= 0x02 && address <= 0x03)
            {
                m92_coincounter_w(value);
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
        public static void N0WriteIOWord_lethalth(int address, ushort value)
        {
            if (address >= 0x00 && address + 1 <= 0x01)
            {
                m92_soundlatch_w(value);
            }
            else if (address >= 0x02 && address + 1 <= 0x03)
            {
                m92_coincounter_w((byte)value);
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
        public static byte N0ReadByte_majtitl2(int address)
        {
            address &= 0xfffff;
            byte result = 0;
            if (address >= 0xf0000 && address <= 0xf3fff)
            {
                int offset = (address - 0xf0000) / 2;
                result = m92_eeprom_r(offset);
            }
            else
            {
                result = N0ReadByte_m92(address);
            }
            return result;
        }
        public static ushort N0ReadWord_majtitl2(int address)
        {
            address &= 0xfffff;
            ushort result = 0;
            if (address >= 0xf0000 && address + 1 <= 0xf3fff)
            {
                int offset = (address - 0xf0000) / 2;
                result = m92_eeprom_r2(offset);
            }
            else
            {
                result = N0ReadWord_m92(address);
            }
            return result;
        }
        public static void N0WriteByte_majtitl2(int address, byte value)
        {
            address &= 0xfffff;
            if (address >= 0xf0000 && address <= 0xf3fff)
            {
                int offset = (address - 0xf0000) / 2;
                m92_eeprom_w(offset, value);
            }
            else
            {
                N0WriteByte_m92(address, value);
            }
        }
        public static void N0WriteWord_majtitl2(int address, ushort value)
        {
            address &= 0xfffff;
            if (address >= 0xf0000 && address + 1 <= 0xf3fff)
            {
                int offset = (address - 0xf0000) / 2;
                m92_eeprom_w(offset, (byte)value);
            }
            else
            {
                N0WriteWord_m92(address, value);
            }
        }
    }
}
