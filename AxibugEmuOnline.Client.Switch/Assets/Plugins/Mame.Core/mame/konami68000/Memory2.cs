namespace MAME.Core
{
    public unsafe partial class Konami68000
    {
        public static sbyte MReadOpByte_blswhstl(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address <= 0x07ffff)
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
            else if (address >= 0x204000 && address <= 0x207fff)
            {
                int offset = address - 0x204000;
                result = (sbyte)Memory.mainram[offset];
            }
            return result;
        }
        public static sbyte MReadByte_blswhstl(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address <= 0x07ffff)
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
            else if (address >= 0x180000 && address <= 0x183fff)
            {
                int offset = (address - 0x180000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(K052109_word_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)K052109_word_r(offset);
                }
            }
            else if (address >= 0x204000 && address <= 0x207fff)
            {
                int offset = address - 0x204000;
                result = (sbyte)Memory.mainram[offset];
            }
            else if (address >= 0x300000 && address <= 0x303fff)
            {
                int offset = (address - 0x300000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(K053245_scattered_word_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)K053245_scattered_word_r(offset);
                }
            }
            else if (address >= 0x400000 && address <= 0x400fff)
            {
                int offset = (address - 0x400000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(Generic.paletteram16[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)Generic.paletteram16[offset];
                }
            }
            else if (address >= 0x500000 && address <= 0x50003f)
            {
                int offset = (address - 0x500000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)K054000_lsb_r(offset);
                }
            }
            else if (address >= 0x680000 && address <= 0x68001f)
            {
                int offset = (address - 0x680000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(K053244_word_noA1_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)K053244_word_noA1_r(offset);
                }
            }
            else if (address >= 0x700000 && address <= 0x700001)
            {
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = sbyte1;
                }
            }
            else if (address >= 0x700002 && address <= 0x700003)
            {
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = sbyte2;
                }
            }
            else if (address >= 0x700004 && address <= 0x700005)
            {
                int offset = (address - 0x700004) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(blswhstl_coin_r() >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)blswhstl_coin_r();
                }
            }
            else if (address >= 0x700006 && address <= 0x700007)
            {
                int offset = (address - 0x700006) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(blswhstl_eeprom_r() >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)blswhstl_eeprom_r();
                }
            }
            else if (address >= 0x780600 && address <= 0x780603)
            {
                int offset = (address - 0x780600) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)blswhstl_sound_r(offset);
                }
            }
            return result;
        }
        public static short MReadOpWord_blswhstl(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x07ffff)
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
            else if (address >= 0x204000 && address + 1 <= 0x207fff)
            {
                int offset = address - 0x204000;
                result = (short)(Memory.mainram[offset] * 0x100 + Memory.mainram[offset + 1]);
            }
            return result;
        }
        public static short MReadWord_blswhstl(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x07ffff)
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
            else if (address >= 0x180000 && address + 1 <= 0x183fff)
            {
                int offset = (address - 0x180000) / 2;
                result = (short)K052109_word_r(offset);
            }
            else if (address >= 0x204000 && address + 1 <= 0x207fff)
            {
                int offset = address - 0x204000;
                result = (short)(Memory.mainram[offset] * 0x100 + Memory.mainram[offset + 1]);
            }
            else if (address >= 0x300000 && address + 1 <= 0x303fff)
            {
                int offset = (address - 0x300000) / 2;
                result = (short)K053245_scattered_word_r(offset);
            }
            else if (address >= 0x400000 && address + 1 <= 0x400fff)
            {
                int offset = (address - 0x400000) / 2;
                result = (short)Generic.paletteram16[offset];
            }
            else if (address >= 0x500000 && address + 1 <= 0x50003f)
            {
                int offset = (address - 0x500000) / 2;
                result = (short)K054000_lsb_r(offset);
            }
            else if (address >= 0x680000 && address + 1 <= 0x68001f)
            {
                int offset = (address - 0x680000) / 2;
                result = (short)K053244_word_noA1_r(offset);
            }
            else if (address >= 0x700000 && address + 1 <= 0x700001)
            {
                int offset = (address - 0x700000) / 2;
                result = (short)((byte)sbyte1);
            }
            else if (address >= 0x700002 && address + 1 <= 0x700003)
            {
                int offset = (address - 0x700002) / 2;
                result = (short)((byte)sbyte2);
            }
            else if (address >= 0x700004 && address + 1 <= 0x700005)
            {
                int offset = (address - 0x700004) / 2;
                result = (short)blswhstl_coin_r();
            }
            else if (address >= 0x700006 && address + 1 <= 0x700007)
            {
                int offset = (address - 0x700006) / 2;
                result = (short)blswhstl_eeprom_r();
            }
            else if (address >= 0x780600 && address + 1 <= 0x780603)
            {
                int offset = (address - 0x780600) / 2;
                result = (short)blswhstl_sound_r(offset);
            }
            return result;
        }
        public static int MReadOpLong_blswhstl(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x07ffff)
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
            else if (address >= 0x204000 && address + 3 <= 0x207fff)
            {
                int offset = address - 0x204000;
                result = (int)(Memory.mainram[offset] * 0x1000000 + Memory.mainram[offset + 1] * 0x10000 + Memory.mainram[offset + 2] * 0x100 + Memory.mainram[offset + 3]);
            }
            return result;
        }
        public static int MReadLong_blswhstl(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x07ffff)
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
            else if (address >= 0x180000 && address + 3 <= 0x183fff)
            {
                int offset = (address - 0x180000) / 2;
                result = (int)(K052109_word_r(offset) * 0x10000 + K052109_word_r(offset + 1));
            }
            else if (address >= 0x204000 && address + 3 <= 0x207fff)
            {
                int offset = address - 0x204000;
                result = (int)(Memory.mainram[offset] * 0x1000000 + Memory.mainram[offset + 1] * 0x10000 + Memory.mainram[offset + 2] * 0x100 + Memory.mainram[offset + 3]);
            }
            else if (address >= 0x300000 && address + 3 <= 0x303fff)
            {
                int offset = (address - 0x300000) / 2;
                result = (int)(K053245_scattered_word_r(offset) * 0x10000 + K053245_scattered_word_r(offset + 1));
            }
            else if (address >= 0x400000 && address + 3 <= 0x400fff)
            {
                int offset = (address - 0x400000) / 2;
                result = (int)(Generic.paletteram16[offset] * 0x10000 + Generic.paletteram16[offset + 1]); ;
            }
            else if (address >= 0x500000 && address + 3 <= 0x50003f)
            {
                int offset = (address - 0x500000) / 2;
                result = (int)(K054000_lsb_r(offset) * 0x10000 + K054000_lsb_r(offset + 1));
            }
            else if (address >= 0x680000 && address + 3 <= 0x68001f)
            {
                int offset = (address - 0x680000) / 2;
                result = (int)(K053244_word_noA1_r(offset) * 0x10000 + K053244_word_noA1_r(offset + 1));
            }
            else if (address >= 0x780600 && address + 3 <= 0x780603)
            {
                int offset = (address - 0x780600) / 2;
                result = (int)0;
            }
            else if (address >= 0x780600 && address + 3 <= 0x780601)
            {
                int offset = (address - 0x780600) / 2;
                result = (int)(blswhstl_sound_r(offset) * 0x10000 + blswhstl_sound_r(offset + 1));
            }
            return result;
        }
        public static void MWriteByte_blswhstl(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x180000 && address <= 0x183fff)
            {
                int offset = (address - 0x180000) / 2;
                if (address % 2 == 0)
                {
                    K052109_word_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    K052109_word_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x204000 && address <= 0x207fff)
            {
                int offset = address - 0x204000;
                Memory.mainram[offset] = (byte)value;
            }
            else if (address >= 0x300000 && address <= 0x303fff)
            {
                int offset = (address - 0x300000) / 2;
                if (address % 2 == 0)
                {
                    K053245_scattered_word_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    K053245_scattered_word_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x400000 && address <= 0x400fff)
            {
                int offset = (address - 0x400000) / 2;
                if (address % 2 == 0)
                {
                    Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x500000 && address <= 0x50003f)
            {
                int offset = (address - 0x500000) / 2;
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    K054000_lsb_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x680000 && address <= 0x68001f)
            {
                int offset = (address - 0x680000) / 2;
                if (address % 2 == 0)
                {
                    K053244_word_noA1_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    K053244_word_noA1_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x700200 && address <= 0x700201)
            {
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    blswhstl_eeprom_w2((byte)value);
                }
            }
            else if (address >= 0x700300 && address <= 0x700301)
            {
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    blswhstl_700300_w2((byte)value);
                }
            }
            else if (address >= 0x700400 && address <= 0x700401)
            {
                Generic.watchdog_reset16_w();
            }
            else if (address >= 0x780600 && address <= 0x780601)
            {
                int offset = (address - 0x780600) / 2;
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    K053260.k053260_0_lsb_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x780604 && address <= 0x780605)
            {
                ssriders_soundkludge_w();
            }
            else if (address >= 0x780700 && address <= 0x78071f)
            {
                int offset = (address - 0x780700) / 2;
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    K053251_lsb_w2(offset, (byte)value);
                }
            }
        }
        public static void MWriteWord_blswhstl(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x180000 && address + 1 <= 0x183fff)
            {
                int offset = (address - 0x180000) / 2;
                K052109_word_w(offset, (ushort)value);
            }
            else if (address >= 0x204000 && address + 1 <= 0x207fff)
            {
                int offset = address - 0x204000;
                Memory.mainram[offset] = (byte)(value >> 8);
                Memory.mainram[offset + 1] = (byte)value;
            }
            else if (address >= 0x300000 && address + 1 <= 0x303fff)
            {
                int offset = (address - 0x300000) / 2;
                K053245_scattered_word_w(offset, (ushort)value);
            }
            else if (address >= 0x400000 && address + 1 <= 0x400fff)
            {
                int offset = (address - 0x400000) / 2;
                Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w(offset, (ushort)value);
            }
            else if (address >= 0x500000 && address + 1 <= 0x50003f)
            {
                int offset = (address - 0x500000) / 2;
                K054000_lsb_w(offset, (ushort)value);
            }
            else if (address >= 0x680000 && address + 1 <= 0x68001f)
            {
                int offset = (address - 0x680000) / 2;
                K053244_word_noA1_w(offset, (ushort)value);
            }
            else if (address >= 0x700200 && address + 1 <= 0x700201)
            {
                blswhstl_eeprom_w((ushort)value);
            }
            else if (address >= 0x700300 && address + 1 <= 0x700301)
            {
                blswhstl_700300_w((ushort)value);
            }
            else if (address >= 0x700400 && address + 1 <= 0x700401)
            {
                Generic.watchdog_reset16_w();
            }
            else if (address >= 0x780600 && address + 1 <= 0x780601)
            {
                int offset = (address - 0x780600) / 2;
                K053260.k053260_0_lsb_w(offset, (ushort)value);
            }
            else if (address >= 0x780604 && address + 1 <= 0x780605)
            {
                ssriders_soundkludge_w();
            }
            else if (address >= 0x780700 && address + 1 <= 0x78071f)
            {
                int offset = (address - 0x780700) / 2;
                K053251_lsb_w(offset, (ushort)value);
            }
        }
        public static void MWriteLong_blswhstl(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x180000 && address + 3 <= 0x183fff)
            {
                int offset = (address - 0x180000) / 2;
                K052109_word_w(offset, (ushort)(value >> 16));
                K052109_word_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x204000 && address + 3 <= 0x207fff)
            {
                int offset = address - 0x204000;
                Memory.mainram[offset] = (byte)(value >> 24);
                Memory.mainram[offset + 1] = (byte)(value >> 16);
                Memory.mainram[offset + 2] = (byte)(value >> 8);
                Memory.mainram[offset + 3] = (byte)value;
            }
            else if (address >= 0x300000 && address + 3 <= 0x303fff)
            {
                int offset = (address - 0x300000) / 2;
                K053245_scattered_word_w(offset, (ushort)(value >> 16));
                K053245_scattered_word_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x400000 && address + 3 <= 0x400fff)
            {
                int offset = (address - 0x400000) / 2;
                Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w(offset, (ushort)(value >> 16));
                Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x500000 && address + 3 <= 0x50003f)
            {
                int offset = (address - 0x500000) / 2;
                K054000_lsb_w(offset, (ushort)(value >> 16));
                K054000_lsb_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x680000 && address + 3 <= 0x68001f)
            {
                int offset = (address - 0x680000) / 2;
                K053244_word_noA1_w(offset, (ushort)(value >> 16));
                K053244_word_noA1_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x780700 && address + 3 <= 0x78071f)
            {
                int offset = (address - 0x780700) / 2;
                K053251_lsb_w(offset, (ushort)(value >> 16));
                K053251_lsb_w(offset + 1, (ushort)value);
            }
        }
        public static sbyte MReadOpByte_glfgreat(int address)
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
            else if (address >= 0x100000 && address <= 0x103fff)
            {
                int offset = address - 0x100000;
                result = (sbyte)Memory.mainram[offset];
            }
            return result;
        }
        public static sbyte MReadByte_glfgreat(int address)
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
            else if (address >= 0x100000 && address <= 0x103fff)
            {
                int offset = address - 0x100000;
                result = (sbyte)Memory.mainram[offset];
            }
            else if (address >= 0x104000 && address <= 0x107fff)
            {
                int offset = (address - 0x104000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(K053245_scattered_word_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)K053245_scattered_word_r(offset);
                }
            }
            else if (address >= 0x108000 && address <= 0x108fff)
            {
                int offset = (address - 0x108000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(Generic.paletteram16[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)Generic.paletteram16[offset];
                }
            }
            else if (address >= 0x10c000 && address <= 0x10cfff)
            {
                int offset = (address - 0x10c000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(K053936_0_linectrl[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)K053936_0_linectrl[offset];
                }
            }
            else if (address >= 0x114000 && address <= 0x11401f)
            {
                int offset = (address - 0x114000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)K053244_lsb_r(offset);
                }
            }
            else if (address >= 0x120000 && address <= 0x120001)
            {
                if (address % 2 == 0)
                {
                    result = sbyte2;
                }
                else if (address % 2 == 1)
                {
                    result = sbyte1;
                }
            }
            else if (address >= 0x120002 && address <= 0x120003)
            {
                if (address % 2 == 0)
                {
                    result = sbyte4;
                }
                else if (address % 2 == 1)
                {
                    result = sbyte3;
                }
            }
            else if (address >= 0x120004 && address <= 0x120005)
            {
                if (address % 2 == 0)
                {
                    result = (sbyte)dsw3;
                }
                else if (address % 2 == 1)
                {
                    result = sbyte0;
                }
            }
            else if (address >= 0x120006 && address <= 0x120007)
            {
                if (address % 2 == 0)
                {
                    result = (sbyte)dsw2;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)dsw1;
                }
            }
            else if (address >= 0x121000 && address <= 0x121001)
            {
                int offset = (address - 0x121000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)glfgreat_ball_r();
                }
            }
            else if (address >= 0x125000 && address <= 0x125003)
            {
                int offset = (address - 0x125000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)glfgreat_sound_r1(offset);
                }
                else
                {
                    result = (sbyte)0;
                }
            }
            else if (address >= 0x200000 && address <= 0x207fff)
            {
                int offset = (address - 0x200000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(K052109_word_noA12_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)K052109_word_noA12_r(offset);
                }
            }
            else if (address >= 0x300000 && address <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(glfgreat_rom_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)glfgreat_rom_r(offset);
                }
            }
            return result;
        }
        public static short MReadOpWord_glfgreat(int address)
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
            else if (address >= 0x100000 && address + 1 <= 0x103fff)
            {
                int offset = address - 0x100000;
                result = (short)(Memory.mainram[offset] * 0x100 + Memory.mainram[offset + 1]);
            }
            return result;
        }
        public static short MReadWord_glfgreat(int address)
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
            else if (address >= 0x100000 && address + 1 <= 0x103fff)
            {
                int offset = address - 0x100000;
                result = (short)(Memory.mainram[offset] * 0x100 + Memory.mainram[offset + 1]);
            }
            else if (address >= 0x104000 && address + 1 <= 0x107fff)
            {
                int offset = (address - 0x104000) / 2;
                result = (short)K053245_scattered_word_r(offset);
            }
            else if (address >= 0x108000 && address + 1 <= 0x108fff)
            {
                int offset = (address - 0x108000) / 2;
                result = (short)Generic.paletteram16[offset];
            }
            else if (address >= 0x10c000 && address + 1 <= 0x10cfff)
            {
                int offset = (address - 0x10c000) / 2;
                result = (short)K053936_0_linectrl[offset];
            }
            else if (address >= 0x114000 && address + 1 <= 0x11401f)
            {
                int offset = (address - 0x114000) / 2;
                result = (short)K053244_lsb_r(offset);
            }
            else if (address >= 0x120000 && address + 1 <= 0x120001)
            {
                int offset = (address - 0x120000) / 2;
                result = (short)(((byte)sbyte2 << 8) | (byte)sbyte1);
            }
            else if (address >= 0x120002 && address + 1 <= 0x120003)
            {
                int offset = (address - 0x120002) / 2;
                result = (short)(((byte)sbyte4 << 8) | (byte)sbyte3);
            }
            else if (address >= 0x120004 && address + 1 <= 0x120005)
            {
                int offset = (address - 0x120004) / 2;
                result = (short)((dsw3 << 8) | (byte)sbyte0);
            }
            else if (address >= 0x120006 && address + 1 <= 0x120007)
            {
                int offset = (address - 0x120006) / 2;
                result = (short)((dsw2 << 8) | dsw1);
            }
            else if (address >= 0x121000 && address + 1 <= 0x121001)
            {
                int offset = (address - 0x121000) / 2;
                result = (short)glfgreat_ball_r();
            }
            else if (address >= 0x125000 && address + 1 <= 0x125003)
            {
                int offset = (address - 0x125000) / 2;
                result = (short)glfgreat_sound_r(offset);
            }
            else if (address >= 0x200000 && address + 1 <= 0x207fff)
            {
                int offset = (address - 0x200000) / 2;
                result = (short)K052109_word_noA12_r(offset);
            }
            else if (address >= 0x300000 && address + 1 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                result = (short)glfgreat_rom_r(offset);
            }
            return result;
        }
        public static int MReadOpLong_glfgreat(int address)
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
            else if (address >= 0x100000 && address + 3 <= 0x103fff)
            {
                int offset = address - 0x100000;
                result = (int)(Memory.mainram[offset] * 0x1000000 + Memory.mainram[offset + 1] * 0x10000 + Memory.mainram[offset + 2] * 0x100 + Memory.mainram[offset + 3]);
            }
            return result;
        }
        public static int MReadLong_glfgreat(int address)
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
            else if (address >= 0x100000 && address + 3 <= 0x103fff)
            {
                int offset = address - 0x100000;
                result = (int)(Memory.mainram[offset] * 0x1000000 + Memory.mainram[offset + 1] * 0x10000 + Memory.mainram[offset + 2] * 0x100 + Memory.mainram[offset + 3]);
            }
            else if (address >= 0x104000 && address + 3 <= 0x107fff)
            {
                int offset = (address - 0x104000) / 2;
                result = (int)(K053245_scattered_word_r(offset) * 0x10000 + K053245_scattered_word_r(offset + 1));
            }
            else if (address >= 0x108000 && address + 3 <= 0x108fff)
            {
                int offset = (address - 0x108000) / 2;
                result = (int)(Generic.paletteram16[offset] * 0x10000 + Generic.paletteram16[offset + 1]);
            }
            else if (address >= 0x10c000 && address + 3 <= 0x10cfff)
            {
                int offset = (address - 0x10c000) / 2;
                result = (int)(K053936_0_linectrl[offset] * 0x10000 + K053936_0_linectrl[offset + 1]);
            }
            else if (address >= 0x114000 && address + 3 <= 0x11401f)
            {
                int offset = (address - 0x114000) / 2;
                result = (int)(K053244_lsb_r(offset) * 0x10000 + K053244_lsb_r(offset + 1));
            }
            else if (address >= 0x125000 && address + 3 <= 0x125003)
            {
                int offset = (address - 0x125000) / 2;
                result = (int)(glfgreat_sound_r(offset) * 0x10000 + glfgreat_sound_r(offset + 1));
            }
            else if (address >= 0x200000 && address + 3 <= 0x207fff)
            {
                int offset = (address - 0x200000) / 2;
                result = (int)(K052109_word_noA12_r(offset) * 0x10000 + K052109_word_noA12_r(offset + 1));
            }
            else if (address >= 0x300000 && address + 3 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                result = (int)(glfgreat_rom_r(offset) * 0x10000 + glfgreat_rom_r(offset + 1));
            }
            return result;
        }
        public static void MWriteByte_glfgreat(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x100000 && address <= 0x103fff)
            {
                int offset = address - 0x100000;
                Memory.mainram[offset] = (byte)(value);
            }
            else if (address >= 0x104000 && address <= 0x107fff)
            {
                int offset = (address - 0x104000) / 2;
                if (address % 2 == 0)
                {
                    K053245_scattered_word_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    K053245_scattered_word_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x108000 && address <= 0x108fff)
            {
                int offset = (address - 0x108000) / 2;
                if (address % 2 == 0)
                {
                    Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x10c000 && address <= 0x10cfff)
            {
                int offset = (address - 0x10c000) / 2;
                if (address % 2 == 0)
                {
                    K053936_0_linectrl[offset] = (ushort)(((byte)value << 8) | (K053936_0_linectrl[offset] & 0xff));
                }
                else if (address % 2 == 1)
                {
                    K053936_0_linectrl[offset] = (ushort)((K053936_0_linectrl[offset] & 0xff00) | (byte)value);
                }
            }
            else if (address >= 0x110000 && address <= 0x11001f)
            {
                int offset = (address - 0x110000) / 2;
                if (address % 2 == 0)
                {
                    K053244_word_noA1_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    K053244_word_noA1_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x114000 && address <= 0x11401f)
            {
                int offset = (address - 0x114000) / 2;
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    K053244_lsb_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x118000 && address <= 0x11801f)
            {
                int offset = (address - 0x118000) / 2;
                if (address % 2 == 0)
                {
                    K053936_0_ctrl[offset] = (ushort)(((byte)value << 8) | (K053936_0_ctrl[offset] & 0xff));
                }
                else if (address % 2 == 1)
                {
                    K053936_0_ctrl[offset] = (ushort)((K053936_0_ctrl[offset] & 0xff00) | (byte)value);
                }
            }
            else if (address >= 0x11c000 && address <= 0x11c01f)
            {
                int offset = (address - 0x11c000) / 2;
                if (address % 2 == 0)
                {
                    K053251_msb_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {

                }
            }
            else if (address >= 0x122000 && address <= 0x122001)
            {
                int offset = (address - 0x122000) / 2;
                if (address % 2 == 0)
                {
                    glfgreat_122000_w1((byte)value);
                }
                else if (address % 2 == 1)
                {
                    glfgreat_122000_w2((byte)value);
                }
            }
            else if (address >= 0x124000 && address <= 0x124001)
            {
                Generic.watchdog_reset16_w();
            }
            else if (address >= 0x125000 && address <= 0x125003)
            {
                int offset = (address - 0x125000) / 2;
                if (address % 2 == 0)
                {
                    glfgreat_sound_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    glfgreat_sound_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x200000 && address <= 0x207fff)
            {
                int offset = (address - 0x200000) / 2;
                if (address % 2 == 0)
                {
                    K052109_word_noA12_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    K052109_word_noA12_w2(offset, (byte)value);
                }
            }
        }
        public static void MWriteWord_glfgreat(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x100000 && address + 1 <= 0x103fff)
            {
                int offset = address - 0x100000;
                Memory.mainram[offset] = (byte)(value >> 8);
                Memory.mainram[offset + 1] = (byte)value;
            }
            else if (address >= 0x104000 && address + 1 <= 0x107fff)
            {
                int offset = (address - 0x104000) / 2;
                K053245_scattered_word_w(offset, (ushort)value);
            }
            else if (address >= 0x108000 && address + 1 <= 0x108fff)
            {
                int offset = (address - 0x108000) / 2;
                Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w(offset, (ushort)value);
            }
            else if (address >= 0x10c000 && address + 1 <= 0x10cfff)
            {
                int offset = (address - 0x10c000) / 2;
                K053936_0_linectrl[offset] = (ushort)value;
            }
            else if (address >= 0x110000 && address + 1 <= 0x11001f)
            {
                int offset = (address - 0x110000) / 2;
                K053244_word_noA1_w(offset, (ushort)value);
            }
            else if (address >= 0x114000 && address + 1 <= 0x11401f)
            {
                int offset = (address - 0x114000) / 2;
                K053244_lsb_w(offset, (ushort)value);
            }
            else if (address >= 0x118000 && address + 1 <= 0x11801f)
            {
                int offset = (address - 0x118000) / 2;
                K053936_0_ctrl[offset] = (ushort)value;
            }
            else if (address >= 0x11c000 && address + 1 <= 0x11c01f)
            {
                int offset = (address - 0x11c000) / 2;
                K053251_msb_w(offset, (ushort)value);
            }
            else if (address >= 0x122000 && address + 1 <= 0x122001)
            {
                int offset = (address - 0x122000) / 2;
                glfgreat_122000_w((ushort)value);
            }
            else if (address >= 0x124000 && address + 1 <= 0x124001)
            {
                Generic.watchdog_reset16_w();
            }
            else if (address >= 0x125000 && address + 1 <= 0x125003)
            {
                int offset = (address - 0x125000) / 2;
                glfgreat_sound_w(offset, (ushort)value);
            }
            else if (address >= 0x200000 && address + 1 <= 0x207fff)
            {
                int offset = (address - 0x200000) / 2;
                K052109_word_noA12_w(offset, (ushort)value);
            }
        }
        public static void MWriteLong_glfgreat(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x100000 && address + 3 <= 0x103fff)
            {
                int offset = address - 0x100000;
                Memory.mainram[offset] = (byte)(value >> 24);
                Memory.mainram[offset + 1] = (byte)(value >> 16);
                Memory.mainram[offset + 2] = (byte)(value >> 8);
                Memory.mainram[offset + 3] = (byte)value;
            }
            else if (address >= 0x104000 && address + 3 <= 0x107fff)
            {
                int offset = (address - 0x104000) / 2;
                K053245_scattered_word_w(offset, (ushort)(value >> 16));
                K053245_scattered_word_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x108000 && address + 3 <= 0x108fff)
            {
                int offset = (address - 0x108000) / 2;
                Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w(offset, (ushort)(value >> 16));
                Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x10c000 && address + 3 <= 0x10cfff)
            {
                int offset = (address - 0x10c000) / 2;
                K053936_0_linectrl[offset] = (ushort)(value >> 16);
                K053936_0_linectrl[offset + 1] = (ushort)value;
            }
            else if (address >= 0x110000 && address + 3 <= 0x11001f)
            {
                int offset = (address - 0x110000) / 2;
                K053244_word_noA1_w(offset, (ushort)(value >> 16));
                K053244_word_noA1_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x114000 && address + 3 <= 0x11401f)
            {
                int offset = (address - 0x114000) / 2;
                K053244_lsb_w(offset, (ushort)(value >> 16));
                K053244_lsb_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x118000 && address + 3 <= 0x11801f)
            {
                int offset = (address - 0x118000) / 2;
                K053936_0_ctrl[offset] = (ushort)(value >> 16);
                K053936_0_ctrl[offset + 1] = (ushort)value;
            }
            else if (address >= 0x11c000 && address + 3 <= 0x11c01f)
            {
                int offset = (address - 0x11c000) / 2;
                K053251_msb_w(offset, (ushort)(value >> 16));
                K053251_msb_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x125000 && address + 3 <= 0x125003)
            {
                int offset = (address - 0x125000) / 2;
                glfgreat_sound_w(offset, (ushort)(value >> 16));
                glfgreat_sound_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x200000 && address + 3 <= 0x207fff)
            {
                int offset = (address - 0x200000) / 2;
                K052109_word_noA12_w(offset, (ushort)(value >> 16));
                K052109_word_noA12_w(offset + 1, (ushort)value);
            }
        }
        public static sbyte MReadOpByte_tmnt2(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address <= 0x0fffff)
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
            else if (address >= 0x104000 && address <= 0x107fff)
            {
                int offset = address - 0x104000;
                result = (sbyte)Memory.mainram[offset];
            }
            return result;
        }
        public static sbyte MReadByte_tmnt2(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address <= 0x0fffff)
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
            else if (address >= 0x104000 && address <= 0x107fff)
            {
                int offset = address - 0x104000;
                result = (sbyte)Memory.mainram[offset];
            }
            else if (address >= 0x140000 && address <= 0x140fff)
            {
                int offset = (address - 0x140000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(Generic.paletteram16[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)Generic.paletteram16[offset];
                }
            }
            else if (address >= 0x180000 && address <= 0x183fff)
            {
                int offset = (address - 0x180000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(Generic.spriteram16[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)Generic.spriteram16[offset];
                }
            }
            else if (address >= 0x1c0000 && address <= 0x1c0001)
            {
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = sbyte1;
                }
            }
            else if (address >= 0x1c0002 && address <= 0x1c0003)
            {
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = sbyte2;
                }
            }
            else if (address >= 0x1c0004 && address <= 0x1c0005)
            {
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = sbyte3;
                }
            }
            else if (address >= 0x1c0006 && address <= 0x1c0007)
            {
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = sbyte4;
                }
            }
            else if (address >= 0x1c0100 && address <= 0x1c0101)
            {
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = sbyte0;
                }
            }
            else if (address >= 0x1c0102 && address <= 0x1c0103)
            {
                if (address % 2 == 0)
                {
                    result = (sbyte)(ssriders_eeprom_r() >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)ssriders_eeprom_r();
                }
            }
            else if (address >= 0x1c0400 && address <= 0x1c0401)
            {
                Generic.watchdog_reset16_r();
            }
            else if (address >= 0x1c0500 && address <= 0x1c057f)
            {
                int offset = address - 0x1c0500;
                result = (sbyte)mainram2[offset];
            }
            else if (address >= 0x5a0000 && address <= 0x5a001f)
            {
                int offset = (address - 0x5a0000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(K053244_word_noA1_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)K053244_word_noA1_r(offset);
                }
            }
            else if (address >= 0x5c0600 && address <= 0x5c0603)
            {
                int offset = (address - 0x5c0600) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)0;
                }
                else
                {
                    result = (sbyte)tmnt2_sound_r(offset);
                }
            }
            else if (address >= 0x600000 && address <= 0x603fff)
            {
                int offset = (address - 0x600000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(K052109_word_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)K052109_word_r(offset);
                }
            }
            return result;
        }
        public static short MReadOpWord_tmnt2(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x0fffff)
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
            else if (address >= 0x104000 && address + 1 <= 0x107fff)
            {
                int offset = address - 0x104000;
                result = (short)(Memory.mainram[offset] * 0x100 + Memory.mainram[offset + 1]);
            }
            return result;
        }
        public static short MReadWord_tmnt2(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x0fffff)
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
            else if (address >= 0x104000 && address + 1 <= 0x107fff)
            {
                int offset = address - 0x104000;
                result = (short)(Memory.mainram[offset] * 0x100 + Memory.mainram[offset + 1]);
            }
            else if (address >= 0x140000 && address + 1 <= 0x140fff)
            {
                int offset = (address - 0x140000) / 2;
                result = (short)Generic.paletteram16[offset];
            }
            else if (address >= 0x180000 && address + 1 <= 0x183fff)
            {
                int offset = (address - 0x180000) / 2;
                result = (short)Generic.spriteram16[offset];
            }
            else if (address >= 0x1c0000 && address + 1 <= 0x1c0001)
            {
                int offset = (address - 0x1c0000) / 2;
                result = (short)((byte)sbyte1);
            }
            else if (address >= 0x1c0002 && address + 1 <= 0x1c0003)
            {
                int offset = (address - 0x1c0002) / 2;
                result = (short)((byte)sbyte2);
            }
            else if (address >= 0x1c0004 && address + 1 <= 0x1c0005)
            {
                int offset = (address - 0x1c0004) / 2;
                result = (short)((byte)sbyte3);
            }
            else if (address >= 0x1c0006 && address + 1 <= 0x1c0007)
            {
                int offset = (address - 0x1c0006) / 2;
                result = (short)((byte)sbyte4);
            }
            else if (address >= 0x1c0100 && address + 1 <= 0x1c0101)
            {
                int offset = (address - 0x1c0100) / 2;
                result = (short)((byte)sbyte0);
            }
            else if (address >= 0x1c0102 && address + 1 <= 0x1c0103)
            {
                int offset = (address - 0x1c0102) / 2;
                result = (short)ssriders_eeprom_r();
            }
            else if (address >= 0x1c0400 && address + 1 <= 0x1c0401)
            {
                Generic.watchdog_reset16_r();
            }
            else if (address >= 0x1c0500 && address + 1 <= 0x1c057f)
            {
                int offset = address - 0x1c0500;
                result = (short)(mainram2[offset] * 0x100 + mainram2[offset + 1]);
            }
            else if (address >= 0x5a0000 && address + 1 <= 0x5a001f)
            {
                int offset = (address - 0x5a0000) / 2;
                result = (short)K053244_word_noA1_r(offset);
            }
            else if (address >= 0x5c0600 && address + 1 <= 0x5c0603)
            {
                int offset = (address - 0x5c0600) / 2;
                result = (short)tmnt2_sound_r(offset);
            }
            else if (address >= 0x600000 && address + 1 <= 0x603fff)
            {
                int offset = (address - 0x600000) / 2;
                result = (short)K052109_word_r(offset);
            }
            return result;
        }
        public static int MReadOpLong_tmnt2(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x0fffff)
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
            else if (address >= 0x104000 && address + 3 <= 0x107fff)
            {
                int offset = address - 0x104000;
                result = (int)(Memory.mainram[offset] * 0x1000000 + Memory.mainram[offset + 1] * 0x10000 + Memory.mainram[offset + 2] * 0x100 + Memory.mainram[offset + 3]);
            }
            return result;
        }
        public static int MReadLong_tmnt2(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x0fffff)
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
            else if (address >= 0x104000 && address + 3 <= 0x107fff)
            {
                int offset = address - 0x104000;
                result = (int)(Memory.mainram[offset] * 0x1000000 + Memory.mainram[offset + 1] * 0x10000 + Memory.mainram[offset + 2] * 0x100 + Memory.mainram[offset + 3]);
            }
            else if (address >= 0x140000 && address + 3 <= 0x140fff)
            {
                int offset = (address - 0x140000) / 2;
                result = (int)(Generic.paletteram16[offset] * 0x10000 + Generic.paletteram16[offset + 1]);
            }
            else if (address >= 0x180000 && address + 3 <= 0x183fff)
            {
                int offset = (address - 0x180000) / 2;
                result = (int)(Generic.spriteram16[offset] * 0x10000 + Generic.spriteram16[offset + 1]);
            }
            else if (address >= 0x1c0500 && address + 3 <= 0x1c057f)
            {
                int offset = (address - 0x1c0500) / 2;
                result = (int)(mainram2[address - 0x1c0500] * 0x1000000 + mainram2[address - 0x1c0500 + 1] * 0x10000 + mainram2[address - 0x1c0500 + 2] * 0x100 + mainram2[address - 0x1c0500 + 3]);
            }
            else if (address >= 0x5a0000 && address + 3 <= 0x5a001f)
            {
                int offset = (address - 0x5a0000) / 2;
                result = (int)(K053244_word_noA1_r(offset) * 0x10000 + K053244_word_noA1_r(offset + 1));
            }
            else if (address >= 0x5c0600 && address + 3 <= 0x5c0603)
            {
                int offset = (address - 0x5c0600) / 2;
                result = (int)(tmnt2_sound_r(offset) * 0x10000 + tmnt2_sound_r(offset + 1));
            }
            else if (address >= 0x600000 && address + 3 <= 0x603fff)
            {
                int offset = (address - 0x600000) / 2;
                result = (int)(K052109_word_r(offset) * 0x10000 + K052109_word_r(offset + 1));
            }
            return result;
        }
        public static void MWriteByte_tmnt2(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x104000 && address <= 0x107fff)
            {
                int offset = address - 0x104000;
                Memory.mainram[offset] = (byte)(value);
            }
            else if (address >= 0x140000 && address <= 0x140fff)
            {
                int offset = (address - 0x140000) / 2;
                if (address % 2 == 0)
                {
                    Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x180000 && address <= 0x183fff)
            {
                int offset = (address - 0x180000) / 2;
                if (address % 2 == 0)
                {
                    K053245_scattered_word_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    K053245_scattered_word_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x1c0200 && address <= 0x1c0201)
            {
                int offset = (address - 0x1c0200) / 2;
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    ssriders_eeprom_w2((byte)value);
                }
            }
            else if (address >= 0x1c0300 && address <= 0x1c0301)
            {
                int offset = (address - 0x1c0300) / 2;
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    ssriders_1c0300_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x1c0400 && address <= 0x1c0401)
            {
                Generic.watchdog_reset16_w();
            }
            else if (address >= 0x1c0500 && address <= 0x1c057f)
            {
                int offset = address - 0x1c0500;
                mainram2[offset] = (byte)value;
            }
            else if (address >= 0x1c0800 && address <= 0x1c081f)
            {
                int offset = (address - 0x1c0800) / 2;
                if (address % 2 == 0)
                {
                    tmnt2_1c0800_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    tmnt2_1c0800_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x5a0000 && address <= 0x5a001f)
            {
                int offset = (address - 0x5a0000) / 2;
                if (address % 2 == 0)
                {
                    K053244_word_noA1_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    K053244_word_noA1_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x5c0600 && address <= 0x5c0601)
            {
                int offset = (address - 0x5c0600) / 2;
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    K053260.k053260_0_lsb_w(offset, (byte)value);
                }
            }
            else if (address >= 0x5c0604 && address <= 0x5c0605)
            {
                ssriders_soundkludge_w();
            }
            else if (address >= 0x5c0700 && address <= 0x5c071f)
            {
                int offset = (address - 0x5c0700) / 2;
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    K053251_lsb_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x600000 && address <= 0x603fff)
            {
                int offset = (address - 0x600000) / 2;
                if (address % 2 == 0)
                {
                    K052109_word_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    K052109_word_w2(offset, (byte)value);
                }
            }
        }
        public static void MWriteWord_tmnt2(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x104000 && address + 1 <= 0x107fff)
            {
                int offset = address - 0x104000;
                Memory.mainram[offset] = (byte)(value >> 8);
                Memory.mainram[offset + 1] = (byte)value;
            }
            else if (address >= 0x140000 && address + 1 <= 0x140fff)
            {
                int offset = (address - 0x140000) / 2;
                Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w(offset, (ushort)value);
            }
            else if (address >= 0x180000 && address + 1 <= 0x183fff)
            {
                int offset = (address - 0x180000) / 2;
                K053245_scattered_word_w(offset, (ushort)value);
            }
            else if (address >= 0x1c0200 && address + 1 <= 0x1c0201)
            {
                int offset = (address - 0x1c0200) / 2;
                ssriders_eeprom_w((ushort)value);
            }
            else if (address >= 0x1c0300 && address + 1 <= 0x1c0301)
            {
                int offset = (address - 0x1c0300) / 2;
                ssriders_1c0300_w(offset, (ushort)value);
            }
            else if (address >= 0x1c0400 && address + 1 <= 0x1c0401)
            {
                Generic.watchdog_reset16_w();
            }
            else if (address >= 0x1c0500 && address + 1 <= 0x1c057f)
            {
                int offset = address - 0x1c0500;
                mainram2[offset] = (byte)(value >> 8);
                mainram2[offset + 1] = (byte)value;
            }
            else if (address >= 0x1c0800 && address + 1 <= 0x1c081f)
            {
                int offset = (address - 0x1c0800) / 2;
                tmnt2_1c0800_w(offset, (ushort)value);
            }
            else if (address >= 0x5a0000 && address + 1 <= 0x5a001f)
            {
                int offset = (address - 0x5a0000) / 2;
                K053244_word_noA1_w(offset, (ushort)value);
            }
            else if (address >= 0x5c0600 && address + 1 <= 0x5c0601)
            {
                int offset = (address - 0x5c0600) / 2;
                K053260.k053260_0_lsb_w(offset, (ushort)value);
            }
            else if (address >= 0x5c0604 && address + 1 <= 0x5c0605)
            {
                ssriders_soundkludge_w();
            }
            else if (address >= 0x5c0700 && address + 1 <= 0x5c071f)
            {
                int offset = (address - 0x5c0700) / 2;
                K053251_lsb_w(offset, (ushort)value);
            }
            else if (address >= 0x600000 && address + 1 <= 0x603fff)
            {
                int offset = (address - 0x600000) / 2;
                K052109_word_w(offset, (ushort)value);
            }
        }
        public static void MWriteLong_tmnt2(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x104000 && address + 3 <= 0x107fff)
            {
                int offset = address - 0x104000;
                Memory.mainram[offset] = (byte)(value >> 24);
                Memory.mainram[offset + 1] = (byte)(value >> 16);
                Memory.mainram[offset + 2] = (byte)(value >> 8);
                Memory.mainram[offset + 3] = (byte)value;
            }
            else if (address >= 0x140000 && address + 3 <= 0x140fff)
            {
                int offset = (address - 0x140000) / 2;
                Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w(offset, (ushort)(value >> 16));
                Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x180000 && address + 3 <= 0x183fff)
            {
                int offset = (address - 0x180000) / 2;
                K053245_scattered_word_w(offset, (ushort)(value >> 16));
                K053245_scattered_word_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x1c0500 && address + 3 <= 0x1c057f)
            {
                int offset = address - 0x1c0500;
                mainram2[offset] = (byte)(value >> 24);
                mainram2[offset + 1] = (byte)(value >> 16);
                mainram2[offset + 2] = (byte)(value >> 8);
                mainram2[offset + 3] = (byte)value;
            }
            else if (address >= 0x1c0800 && address + 3 <= 0x1c081f)
            {
                int offset = (address - 0x1c0800) / 2;
                tmnt2_1c0800_w(offset, (ushort)(value >> 16));
                tmnt2_1c0800_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x5a0000 && address + 3 <= 0x5a001f)
            {
                int offset = (address - 0x5a0000) / 2;
                K053244_word_noA1_w(offset, (ushort)(value >> 16));
                K053244_word_noA1_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x5c0700 && address + 3 <= 0x5c071f)
            {
                int offset = (address - 0x5c0700) / 2;
                K053251_lsb_w(offset, (ushort)(value >> 16));
                K053251_lsb_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x600000 && address + 3 <= 0x603fff)
            {
                int offset = (address - 0x600000) / 2;
                K052109_word_w(offset, (ushort)(value >> 16));
                K052109_word_w(offset + 1, (ushort)value);
            }
        }
        public static sbyte MReadOpByte_thndrx2(int address)
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
            else if (address >= 0x100000 && address <= 0x103fff)
            {
                int offset = address - 0x100000;
                result = (sbyte)Memory.mainram[offset];
            }
            return result;
        }
        public static sbyte MReadByte_thndrx2(int address)
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
            else if (address >= 0x100000 && address <= 0x103fff)
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
            else if (address >= 0x400000 && address <= 0x400003)
            {
                int offset = (address - 0x400000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(punkshot_sound_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)punkshot_sound_r(offset);
                }
            }
            else if (address >= 0x500000 && address <= 0x50003f)
            {
                int offset = (address - 0x500000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)K054000_lsb_r(offset);
                }
            }
            else if (address >= 0x500200 && address <= 0x500201)
            {
                if (address % 2 == 0)
                {
                    result = (sbyte)(thndrx2_in0_r() >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)thndrx2_in0_r();
                }
            }
            else if (address >= 0x500202 && address <= 0x500203)
            {
                if (address % 2 == 0)
                {
                    result = (sbyte)(thndrx2_eeprom_r() >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)thndrx2_eeprom_r();
                }
            }
            else if (address >= 0x600000 && address <= 0x607fff)
            {
                int offset = (address - 0x600000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(K052109_word_noA12_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)K052109_word_noA12_r(offset);
                }
            }
            else if (address >= 0x700000 && address <= 0x700007)
            {
                int offset = address - 0x700000;
                result = (sbyte)K051937_r(offset);
            }
            else if (address >= 0x700400 && address <= 0x7007ff)
            {
                int offset = address - 0x700400;
                result = (sbyte)K051960_r(offset);
            }
            return result;
        }
        public static short MReadOpWord_thndrx2(int address)
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
            else if (address >= 0x100000 && address + 1 <= 0x103fff)
            {
                int offset = address - 0x100000;
                result = (short)(Memory.mainram[offset] * 0x100 + Memory.mainram[offset + 1]);
            }
            return result;
        }
        public static short MReadWord_thndrx2(int address)
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
            else if (address >= 0x100000 && address + 1 <= 0x103fff)
            {
                int offset = address - 0x100000;
                result = (short)(Memory.mainram[offset] * 0x100 + Memory.mainram[offset + 1]);
            }
            else if (address >= 0x200000 && address + 1 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                result = (short)Generic.paletteram16[offset];
            }
            else if (address >= 0x400000 && address + 1 <= 0x400003)
            {
                int offset = (address - 0x400000) / 2;
                result = (short)punkshot_sound_r(offset);
            }
            else if (address >= 0x500000 && address + 1 <= 0x50003f)
            {
                int offset = (address - 0x500000) / 2;
                result = (short)K054000_lsb_r(offset);
            }
            else if (address >= 0x500200 && address + 1 <= 0x500201)
            {
                result = (short)thndrx2_in0_r();
            }
            else if (address >= 0x500202 && address + 1 <= 0x500203)
            {
                result = (short)thndrx2_eeprom_r();
            }
            else if (address >= 0x600000 && address + 1 <= 0x607fff)
            {
                int offset = (address - 0x600000) / 2;
                result = (short)K052109_word_noA12_r(offset);
            }
            else if (address >= 0x700000 && address + 1 <= 0x700007)
            {
                int offset = address - 0x700000;
                result = (short)(K051937_r(offset) * 0x100 + K051937_r(offset + 1));
            }
            else if (address >= 0x700400 && address + 1 <= 0x7007ff)
            {
                int offset = address - 0x700400;
                result = (short)(K051960_r(offset) * 0x100 + K051960_r(offset + 1));
            }
            return result;
        }
        public static int MReadOpLong_thndrx2(int address)
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
            else if (address >= 0x100000 && address + 3 <= 0x103fff)
            {
                int offset = address - 0x100000;
                result = (int)(Memory.mainram[offset] * 0x1000000 + Memory.mainram[offset + 1] * 0x10000 + Memory.mainram[offset + 2] * 0x100 + Memory.mainram[offset + 3]);
            }
            return result;
        }
        public static int MReadLong_thndrx2(int address)
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
            else if (address >= 0x100000 && address + 3 <= 0x103fff)
            {
                int offset = address - 0x100000;
                result = (int)(Memory.mainram[offset] * 0x1000000 + Memory.mainram[offset + 1] * 0x10000 + Memory.mainram[offset + 2] * 0x100 + Memory.mainram[offset + 3]);
            }
            else if (address >= 0x200000 && address + 3 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                result = (int)(Generic.paletteram16[offset] * 0x10000 + Generic.paletteram16[offset + 1]);
            }
            else if (address >= 0x400000 && address + 3 <= 0x400003)
            {
                int offset = (address - 0x400000) / 2;
                result = (int)(punkshot_sound_r(offset) * 0x10000 + punkshot_sound_r(offset + 1));
            }
            else if (address >= 0x500000 && address + 3 <= 0x50003f)
            {
                int offset = (address - 0x500000) / 2;
                result = (int)(K054000_lsb_r(offset) * 0x10000 + K054000_lsb_r(offset + 1));
            }
            else if (address >= 0x600000 && address + 3 <= 0x607fff)
            {
                int offset = (address - 0x600000) / 2;
                result = (int)(K052109_word_noA12_r(offset) * 0x10000 + K052109_word_noA12_r(offset + 1));
            }
            else if (address >= 0x700000 && address + 3 <= 0x700007)
            {
                int offset = address - 0x700000;
                result = (short)(K051937_r(offset) * 0x1000000 + K051937_r(offset + 1) * 0x10000 + K051937_r(offset + 2) * 0x100 + K051937_r(offset + 3));
            }
            else if (address >= 0x700400 && address + 3 <= 0x7007ff)
            {
                int offset = address - 0x700400;
                result = (short)(K051960_r(offset) * 0x1000000 + K051960_r(offset + 1) * 0x10000 + K051960_r(offset + 2) * 0x100 + K051960_r(offset + 3));
            }
            return result;
        }
        public static void MWriteByte_thndrx2(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x100000 && address <= 0x103fff)
            {
                int offset = address - 0x100000;
                Memory.mainram[offset] = (byte)(value);
            }
            else if (address >= 0x200000 && address <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                if (address % 2 == 0)
                {
                    Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x300000 && address <= 0x30001f)
            {
                int offset = (address - 0x300000) / 2;
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    K053251_lsb_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x400000 && address <= 0x400001)
            {
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    K053260.k053260_0_lsb_w2(0, (byte)value);
                }
            }
            else if (address >= 0x500000 && address <= 0x50003f)
            {
                int offset = (address - 0x500000) / 2;
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    K054000_lsb_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x500100 && address <= 0x500101)
            {
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    thndrx2_eeprom_w2((byte)value);
                }
            }
            else if (address >= 0x500300 && address <= 0x500301)
            {
                int offset = (address - 0x500300) / 2;
                //NOP
            }
            else if (address >= 0x600000 && address <= 0x607fff)
            {
                int offset = (address - 0x600000) / 2;
                if (address % 2 == 0)
                {
                    K052109_word_noA12_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    K052109_word_noA12_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x700000 && address <= 0x700007)
            {
                int offset = address - 0x700000;
                K051937_w(offset, (byte)value);
            }
            else if (address >= 0x700400 && address <= 0x7007ff)
            {
                int offset = address - 0x700400;
                K051960_w(offset, (byte)value);
            }
        }
        public static void MWriteWord_thndrx2(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x100000 && address + 1 <= 0x103fff)
            {
                int offset = address - 0x100000;
                Memory.mainram[offset] = (byte)(value >> 8);
                Memory.mainram[offset + 1] = (byte)value;
            }
            else if (address >= 0x200000 && address + 1 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w(offset, (ushort)value);
            }
            else if (address >= 0x300000 && address + 1 <= 0x30001f)
            {
                int offset = (address - 0x300000) / 2;
                K053251_lsb_w(offset, (ushort)value);
            }
            else if (address >= 0x400000 && address + 1 <= 0x400001)
            {
                K053260.k053260_0_lsb_w(0, (ushort)value);
            }
            else if (address >= 0x500000 && address + 1 <= 0x50003f)
            {
                int offset = (address - 0x500000) / 2;
                K054000_lsb_w(offset, (ushort)value);
            }
            else if (address >= 0x500100 && address + 1 <= 0x500101)
            {
                int offset = (address - 0x500100) / 2;
                thndrx2_eeprom_w((ushort)value);
            }
            else if (address >= 0x500300 && address + 1 <= 0x500301)
            {
                int offset = (address - 0x500300) / 2;
                //NOP
            }
            else if (address >= 0x600000 && address + 1 <= 0x607fff)
            {
                int offset = (address - 0x600000) / 2;
                K052109_word_noA12_w(offset, (ushort)value);
            }
            else if (address >= 0x700000 && address + 1 <= 0x700007)
            {
                int offset = address - 0x700000;
                K051937_w(offset, (byte)(value >> 8));
                K051937_w(offset + 1, (byte)value);
            }
            else if (address >= 0x700400 && address + 1 <= 0x7007ff)
            {
                int offset = address - 0x700400;
                K051960_w(offset, (byte)(value >> 8));
                K051960_w(offset + 1, (byte)value);
            }
        }
        public static void MWriteLong_thndrx2(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x100000 && address + 3 <= 0x103fff)
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
                Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w(offset, (ushort)(value >> 16));
                Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x300000 && address + 3 <= 0x30001f)
            {
                int offset = (address - 0x300000) / 2;
                K053251_lsb_w(offset, (ushort)(value >> 16));
                K053251_lsb_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x500000 && address + 3 <= 0x50003f)
            {
                int offset = (address - 0x500000) / 2;
                K054000_lsb_w(offset, (ushort)(value >> 16));
                K054000_lsb_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x600000 && address + 3 <= 0x607fff)
            {
                int offset = (address - 0x600000) / 2;
                K052109_word_noA12_w(offset, (ushort)(value >> 16));
                K052109_word_noA12_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x700000 && address + 3 <= 0x700007)
            {
                int offset = address - 0x700000;
                K051937_w(offset, (byte)(value >> 24));
                K051937_w(offset + 1, (byte)(value >> 16));
                K051937_w(offset + 2, (byte)(value >> 8));
                K051937_w(offset + 3, (byte)value);
            }
            else if (address >= 0x700400 && address + 3 <= 0x7007ff)
            {
                int offset = address - 0x700400;
                K051960_w(offset, (byte)(value >> 24));
                K051960_w(offset + 1, (byte)(value >> 16));
                K051960_w(offset + 2, (byte)(value >> 8));
                K051960_w(offset + 3, (byte)value);
            }
        }
        public static sbyte MReadOpByte_prmrsocr(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address <= 0x07ffff)
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
            else if (address >= 0x100000 && address <= 0x103fff)
            {
                int offset = address - 0x100000;
                result = (sbyte)Memory.mainram[offset];
            }
            return result;
        }
        public static sbyte MReadByte_prmrsocr(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address <= 0x07ffff)
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
            else if (address >= 0x100000 && address <= 0x103fff)
            {
                int offset = address - 0x100000;
                result = (sbyte)Memory.mainram[offset];
            }
            else if (address >= 0x104000 && address <= 0x107fff)
            {
                int offset = (address - 0x104000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(K053245_scattered_word_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)K053245_scattered_word_r(offset);
                }
            }
            else if (address >= 0x108000 && address <= 0x108fff)
            {
                int offset = (address - 0x108000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(Generic.paletteram16[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)Generic.paletteram16[offset];
                }
            }
            else if (address >= 0x10c000 && address <= 0x10cfff)
            {
                int offset = (address - 0x10c000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(K053936_0_linectrl[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)K053936_0_linectrl[offset];
                }
            }
            else if (address >= 0x114000 && address <= 0x11401f)
            {
                int offset = (address - 0x114000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)K053244_lsb_r(offset);
                }
            }
            else if (address >= 0x120000 && address <= 0x120001)
            {
                if (address % 2 == 0)
                {
                    result = (sbyte)(prmrsocr_IN0_r() >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)prmrsocr_IN0_r();
                }
            }
            else if (address >= 0x120002 && address <= 0x120003)
            {
                if (address % 2 == 0)
                {
                    result = (sbyte)(prmrsocr_eeprom_r() >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)prmrsocr_eeprom_r();
                }
            }
            else if (address >= 0x121014 && address <= 0x121015)
            {
                if (address % 2 == 0)
                {
                    result = (sbyte)(prmrsocr_sound_r() >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)prmrsocr_sound_r();
                }
            }
            else if (address >= 0x200000 && address <= 0x207fff)
            {
                int offset = (address - 0x200000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(K052109_word_noA12_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)K052109_word_noA12_r(offset);
                }
            }
            else if (address >= 0x300000 && address <= 0x33ffff)
            {
                int offset = (address - 0x300000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)prmrsocr_rom_r1(offset);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)prmrsocr_rom_r2(offset);
                }
            }
            return result;
        }
        public static short MReadOpWord_prmrsocr(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x07ffff)
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
            else if (address >= 0x100000 && address + 1 <= 0x103fff)
            {
                int offset = address - 0x100000;
                result = (short)(Memory.mainram[offset] * 0x100 + Memory.mainram[offset + 1]);
            }
            return result;
        }
        public static short MReadWord_prmrsocr(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x07ffff)
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
            else if (address >= 0x100000 && address + 1 <= 0x103fff)
            {
                int offset = address - 0x100000;
                result = (short)(Memory.mainram[offset] * 0x100 + Memory.mainram[offset + 1]);
            }
            else if (address >= 0x104000 && address + 1 <= 0x107fff)
            {
                int offset = (address - 0x104000) / 2;
                result = (short)K053245_scattered_word_r(offset);
            }
            else if (address >= 0x108000 && address + 1 <= 0x108fff)
            {
                int offset = (address - 0x108000) / 2;
                result = (short)Generic.paletteram16[offset];
            }
            else if (address >= 0x10c000 && address + 1 <= 0x10cfff)
            {
                int offset = (address - 0x10c000) / 2;
                result = (short)K053936_0_linectrl[offset];
            }
            else if (address >= 0x114000 && address + 1 <= 0x11401f)
            {
                int offset = (address - 0x114000) / 2;
                result = (short)K053244_lsb_r(offset);
            }
            else if (address >= 0x120000 && address + 1 <= 0x120001)
            {
                int offset = (address - 0x120000) / 2;
                result = (short)prmrsocr_IN0_r();
            }
            else if (address >= 0x120002 && address + 1 <= 0x120003)
            {
                int offset = (address - 0x120002) / 2;
                result = (short)prmrsocr_eeprom_r();
            }
            else if (address >= 0x121014 && address + 1 <= 0x121015)
            {
                int offset = (address - 0x121014) / 2;
                result = (short)prmrsocr_sound_r();
            }
            else if (address >= 0x200000 && address + 1 <= 0x207fff)
            {
                int offset = (address - 0x200000) / 2;
                result = (short)K052109_word_noA12_r(offset);
            }
            else if (address >= 0x300000 && address + 1 <= 0x33ffff)
            {
                int offset = (address - 0x300000) / 2;
                result = (short)prmrsocr_rom_r(offset);
            }
            return result;
        }
        public static int MReadOpLong_prmrsocr(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x07ffff)
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
            else if (address >= 0x100000 && address + 3 <= 0x103fff)
            {
                int offset = address - 0x100000;
                result = (int)(Memory.mainram[offset] * 0x1000000 + Memory.mainram[offset + 1] * 0x10000 + Memory.mainram[offset + 2] * 0x100 + Memory.mainram[offset + 3]);
            }
            return result;
        }
        public static int MReadLong_prmrsocr(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x07ffff)
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
            else if (address >= 0x100000 && address + 3 <= 0x103fff)
            {
                int offset = address - 0x100000;
                result = (int)(Memory.mainram[offset] * 0x1000000 + Memory.mainram[offset + 1] * 0x10000 + Memory.mainram[offset + 2] * 0x100 + Memory.mainram[offset + 3]);
            }
            else if (address >= 0x104000 && address + 3 <= 0x107fff)
            {
                int offset = (address - 0x104000) / 2;
                result = (int)(K053245_scattered_word_r(offset) * 0x10000 + K053245_scattered_word_r(offset + 1));
            }
            else if (address >= 0x108000 && address + 3 <= 0x108fff)
            {
                int offset = (address - 0x108000) / 2;
                result = (int)(Generic.paletteram16[offset] * 0x10000 + Generic.paletteram16[offset + 1]);
            }
            else if (address >= 0x10c000 && address + 3 <= 0x10cfff)
            {
                int offset = (address - 0x10c000) / 2;
                result = (int)(K053936_0_linectrl[offset] * 0x10000 + K053936_0_linectrl[offset + 1]);
            }
            else if (address >= 0x114000 && address + 3 <= 0x11401f)
            {
                int offset = (address - 0x114000) / 2;
                result = (int)(K053244_lsb_r(offset) * 0x10000 + K053244_lsb_r(offset + 1));
            }
            else if (address >= 0x200000 && address + 3 <= 0x207fff)
            {
                int offset = (address - 0x200000) / 2;
                result = (int)(K052109_word_noA12_r(offset) * 0x10000 + K052109_word_noA12_r(offset + 1));
            }
            else if (address >= 0x300000 && address + 3 <= 0x33ffff)
            {
                int offset = (address - 0x300000) / 2;
                result = (int)(prmrsocr_rom_r(offset) * 0x10000 + prmrsocr_rom_r(offset + 1));
            }
            return result;
        }
        public static void MWriteByte_prmrsocr(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x100000 && address <= 0x103fff)
            {
                int offset = address - 0x100000;
                Memory.mainram[offset] = (byte)(value);
            }
            else if (address >= 0x104000 && address <= 0x107fff)
            {
                int offset = (address - 0x104000) / 2;
                if (address % 2 == 0)
                {
                    K053245_scattered_word_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    K053245_scattered_word_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x108000 && address <= 0x108fff)
            {
                int offset = (address - 0x108000) / 2;
                if (address % 2 == 0)
                {
                    Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x10c000 && address <= 0x10cfff)
            {
                int offset = (address - 0x10c000) / 2;
                if (address % 2 == 0)
                {
                    K053936_0_linectrl[offset] = (ushort)(((byte)value << 8) | (K053936_0_linectrl[offset] & 0xff));
                }
                else if (address % 2 == 1)
                {
                    K053936_0_linectrl[offset] = (ushort)((K053936_0_linectrl[offset] & 0xff00) | (byte)value);
                }
            }
            else if (address >= 0x110000 && address <= 0x11001f)
            {
                int offset = (address - 0x110000) / 2;
                if (address % 2 == 0)
                {
                    K053244_word_noA1_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    K053244_word_noA1_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x114000 && address <= 0x11401f)
            {
                int offset = (address - 0x114000) / 2;
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    K053244_lsb_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x118000 && address <= 0x11801f)
            {
                int offset = (address - 0x118000) / 2;
                if (address % 2 == 0)
                {
                    K053936_0_ctrl[offset] = (ushort)(((byte)value << 8) | (K053936_0_ctrl[offset] & 0xff));
                }
                else if (address % 2 == 1)
                {
                    K053936_0_ctrl[offset] = (ushort)((K053936_0_ctrl[offset] & 0xff00) | (byte)value);
                }
            }
            else if (address >= 0x11c000 && address <= 0x11c01f)
            {
                int offset = (address - 0x11c000) / 2;
                if (address % 2 == 0)
                {
                    K053251_msb_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {

                }
            }
            else if (address >= 0x12100c && address <= 0x12100f)
            {
                int offset = (address - 0x12100c) / 2;
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    prmrsocr_sound_cmd_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x122000 && address <= 0x122001)
            {
                if (address % 2 == 0)
                {
                    prmrsocr_eeprom_w1((byte)value);
                }
                else if (address % 2 == 1)
                {
                    prmrsocr_eeprom_w2((byte)value);
                }
            }
            else if (address >= 0x123000 && address <= 0x123001)
            {
                prmrsocr_sound_irq_w();
            }
            else if (address >= 0x200000 && address <= 0x207fff)
            {
                int offset = (address - 0x200000) / 2;
                if (address % 2 == 0)
                {
                    K052109_word_noA12_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    K052109_word_noA12_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x280000 && address <= 0x280001)
            {
                Generic.watchdog_reset16_w();
            }
        }
        public static void MWriteWord_prmrsocr(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x100000 && address + 1 <= 0x103fff)
            {
                int offset = address - 0x100000;
                Memory.mainram[offset] = (byte)(value >> 8);
                Memory.mainram[offset + 1] = (byte)value;
            }
            else if (address >= 0x104000 && address + 1 <= 0x107fff)
            {
                int offset = (address - 0x104000) / 2;
                K053245_scattered_word_w(offset, (ushort)value);
            }
            else if (address >= 0x108000 && address + 1 <= 0x108fff)
            {
                int offset = (address - 0x108000) / 2;
                Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w(offset, (ushort)value);
            }
            else if (address >= 0x10c000 && address + 1 <= 0x10cfff)
            {
                int offset = (address - 0x10c000) / 2;
                K053936_0_linectrl[offset] = (ushort)value;
            }
            else if (address >= 0x110000 && address + 1 <= 0x11001f)
            {
                int offset = (address - 0x110000) / 2;
                K053244_word_noA1_w(offset, (ushort)value);
            }
            else if (address >= 0x114000 && address + 1 <= 0x11401f)
            {
                int offset = (address - 0x114000) / 2;
                K053244_lsb_w(offset, (ushort)value);
            }
            else if (address >= 0x118000 && address + 1 <= 0x11801f)
            {
                int offset = (address - 0x118000) / 2;
                K053936_0_ctrl[offset] = (ushort)value;
            }
            else if (address >= 0x11c000 && address + 1 <= 0x11c01f)
            {
                int offset = (address - 0x11c000) / 2;
                K053251_msb_w(offset, (ushort)value);
            }
            else if (address >= 0x12100c && address + 1 <= 0x12100f)
            {
                int offset = (address - 0x12100c) / 2;
                prmrsocr_sound_cmd_w(offset, (ushort)value);
            }
            else if (address >= 0x122000 && address + 1 <= 0x122001)
            {
                prmrsocr_eeprom_w((ushort)value);
            }
            else if (address >= 0x123000 && address + 1 <= 0x123001)
            {
                prmrsocr_sound_irq_w();
            }
            else if (address >= 0x200000 && address + 1 <= 0x207fff)
            {
                int offset = (address - 0x200000) / 2;
                K052109_word_noA12_w(offset, (ushort)value);
            }
            else if (address >= 0x280000 && address + 1 <= 0x280001)
            {
                Generic.watchdog_reset16_w();
            }
        }
        public static void MWriteLong_prmrsocr(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x100000 && address + 3 <= 0x103fff)
            {
                int offset = address - 0x100000;
                Memory.mainram[offset] = (byte)(value >> 24);
                Memory.mainram[offset + 1] = (byte)(value >> 16);
                Memory.mainram[offset + 2] = (byte)(value >> 8);
                Memory.mainram[offset + 3] = (byte)value;
            }
            else if (address >= 0x104000 && address + 3 <= 0x107fff)
            {
                int offset = (address - 0x104000) / 2;
                K053245_scattered_word_w(offset, (ushort)(value >> 16));
                K053245_scattered_word_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x108000 && address + 3 <= 0x108fff)
            {
                int offset = (address - 0x108000) / 2;
                Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w(offset, (ushort)(value >> 16));
                Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x10c000 && address + 3 <= 0x10cfff)
            {
                int offset = (address - 0x10c000) / 2;
                K053936_0_linectrl[offset] = (ushort)(value >> 16);
                K053936_0_linectrl[offset + 1] = (ushort)value;
            }
            else if (address >= 0x110000 && address + 3 <= 0x11001f)
            {
                int offset = (address - 0x110000) / 2;
                K053244_word_noA1_w(offset, (ushort)(value >> 16));
                K053244_word_noA1_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x114000 && address + 3 <= 0x11401f)
            {
                int offset = (address - 0x114000) / 2;
                K053244_lsb_w(offset, (ushort)(value >> 16));
                K053244_lsb_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x118000 && address + 3 <= 0x11801f)
            {
                int offset = (address - 0x118000) / 2;
                K053936_0_ctrl[offset] = (ushort)(value >> 16);
                K053936_0_ctrl[offset + 1] = (ushort)value;
            }
            else if (address >= 0x11c000 && address + 3 <= 0x11c01f)
            {
                int offset = (address - 0x11c000) / 2;
                K053251_msb_w(offset, (ushort)(value >> 16));
                K053251_msb_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x12100c && address + 3 <= 0x12100f)
            {
                int offset = (address - 0x12100c) / 2;
                prmrsocr_sound_cmd_w(offset, (ushort)(value >> 16));
                prmrsocr_sound_cmd_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x200000 && address + 3 <= 0x207fff)
            {
                int offset = (address - 0x200000) / 2;
                K052109_word_noA12_w(offset, (ushort)(value >> 16));
                K052109_word_noA12_w(offset + 1, (ushort)value);
            }
        }
        public static byte ZReadOp_glfgreat(ushort address)
        {
            byte result = 0;
            if (address <= 0xefff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0xf000 && address <= 0xf7ff)
            {
                int offset = address - 0xf000;
                result = Memory.audioram[offset];
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static byte ZReadMemory_glfgreat(ushort address)
        {
            byte result = 0;
            if (address <= 0xefff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0xf000 && address <= 0xf7ff)
            {
                int offset = address - 0xf000;
                result = Memory.audioram[offset];
            }
            else if (address >= 0xf800 && address <= 0xf82f)
            {
                int offset = address - 0xf800;
                result = K053260.k053260_0_r(offset);
            }
            return result;
        }
        public static void ZWriteMemory_glfgreat(ushort address, byte value)
        {
            if (address >= 0xf000 && address <= 0xf7ff)
            {
                int offset = address - 0xf000;
                Memory.audioram[offset] = value;
            }
            else if (address >= 0xf800 && address <= 0xf82f)
            {
                int offset = address - 0xf800;
                K053260.k053260_0_w(offset, value);
            }
            else if (address == 0xfa00)
            {
                sound_arm_nmi_w();
            }
        }
        public static byte ZReadOp_thndrx2(ushort address)
        {
            byte result = 0;
            if (address <= 0xefff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0xf000 && address <= 0xf7ff)
            {
                int offset = address - 0xf000;
                result = Memory.audioram[offset];
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static byte ZReadMemory_thndrx2(ushort address)
        {
            byte result = 0;
            if (address <= 0xefff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0xf000 && address <= 0xf7ff)
            {
                int offset = address - 0xf000;
                result = Memory.audioram[offset];
            }
            else if (address == 0xf801 || address == 0xf811)
            {
                result = YM2151.ym2151_status_port_0_r();
            }
            else if (address >= 0xfc00 && address <= 0xfc2f)
            {
                int offset = address - 0xfc00;
                result = K053260.k053260_0_r(offset);
            }
            return result;
        }
        public static void ZWriteMemory_thndrx2(ushort address, byte value)
        {
            if (address >= 0xf000 && address <= 0xf7ff)
            {
                int offset = address - 0xf000;
                Memory.audioram[offset] = value;
            }
            else if (address == 0xf800 || address == 0xf810)
            {
                YM2151.ym2151_register_port_0_w(value);
            }
            else if (address == 0xf801 || address == 0xf811)
            {
                YM2151.ym2151_data_port_0_w(value);
            }
            else if (address == 0xfa00)
            {
                sound_arm_nmi_w();
            }
            else if (address >= 0xfc00 && address <= 0xfc2f)
            {
                int offset = address - 0xfc00;
                K053260.k053260_0_w(offset, value);
            }
        }
        public static byte ZReadOp_prmrsocr(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0x8000 && address <= 0xbfff)
            {
                int offset = address - 0x8000;
                result = Memory.audiorom[basebanksnd + offset];
            }
            else if (address >= 0xc000 && address <= 0xdfff)
            {
                int offset = address - 0xc000;
                result = Memory.audioram[offset];
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static byte ZReadMemory_prmrsocr(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0x8000 && address <= 0xbfff)
            {
                int offset = address - 0x8000;
                result = Memory.audiorom[basebanksnd + offset];
            }
            else if (address >= 0xc000 && address <= 0xdfff)
            {
                int offset = address - 0xc000;
                result = Memory.audioram[offset];
            }
            else if (address >= 0xe000 && address <= 0xe0ff)
            {
                int offset = address - 0xe000;
                result = K054539.k054539_0_r(offset);
            }
            else if (address >= 0xe100 && address <= 0xe12f)
            {
                int offset = address - 0xe100;
                result = k054539_0_ctrl_r(offset);
            }
            else if (address == 0xf002)
            {
                result = (byte)Sound.soundlatch_r();
            }
            else if (address == 0xf003)
            {
                result = (byte)Sound.soundlatch2_r();
            }
            return result;
        }
        public static void ZWriteMemory_prmrsocr(ushort address, byte value)
        {
            if (address >= 0x8000 && address <= 0xbfff)
            {
                int offset = address - 0x8000;
                Memory.audiorom[basebanksnd + offset] = value;
            }
            else if (address >= 0xc000 && address <= 0xdfff)
            {
                int offset = address - 0xc000;
                Memory.audioram[offset] = value;
            }
            else if (address >= 0xe000 && address <= 0xe0ff)
            {
                int offset = address - 0xe000;
                K054539.k054539_0_w(offset, value);
            }
            else if (address >= 0xe100 && address <= 0xe12f)
            {
                int offset = address - 0xe100;
                k054539_0_ctrl_w(offset, value);
            }
            else if (address == 0xf000)
            {
                Sound.soundlatch3_w((ushort)value);
            }
            else if (address == 0xf800)
            {
                prmrsocr_audio_bankswitch_w(value);
            }
        }
    }
}
