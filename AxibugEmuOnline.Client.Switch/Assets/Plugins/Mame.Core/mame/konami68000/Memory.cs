using cpu.z80;

namespace MAME.Core
{
    public unsafe partial class Konami68000
    {
        public static sbyte sbyte0, sbyte1, sbyte2, sbyte3, sbyte4;
        public static sbyte sbyte0_old, sbyte1_old, sbyte2_old, sbyte3_old, sbyte4_old;
        public static byte bytee_old;
        public static sbyte MReadOpByte_cuebrick(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address <= 0x01ffff)
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
            else if (address >= 0x040000 && address <= 0x043fff)
            {
                int offset = address - 0x040000;
                result = (sbyte)Memory.mainram[offset];
            }
            else if (address >= 0x060000 && address <= 0x063fff)
            {
                int offset = address - 0x060000;
                result = (sbyte)mainram2[offset];
            }
            return result;
        }
        public static sbyte MReadByte_cuebrick(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address <= 0x01ffff)
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
            else if (address >= 0x040000 && address <= 0x043fff)
            {
                int offset = address - 0x040000;
                result = (sbyte)Memory.mainram[offset];
            }
            else if (address >= 0x060000 && address <= 0x063fff)
            {
                int offset = address - 0x060000;
                result = (sbyte)mainram2[offset];
            }
            else if (address >= 0x080000 && address <= 0x080fff)
            {
                int offset = (address - 0x080000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(Generic.paletteram16[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)Generic.paletteram16[offset];
                }
            }
            else if (address >= 0x0a0000 && address <= 0x0a0001)
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
            else if (address >= 0x0a0002 && address <= 0x0a0003)
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
            else if (address >= 0x0a0004 && address <= 0x0a0005)
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
            else if (address >= 0x0a0010 && address <= 0x0a0011)
            {
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)dsw1;
                }
            }
            else if (address >= 0x0a0012 && address <= 0x0a0013)
            {
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)dsw2;
                }
            }
            else if (address >= 0x0a0018 && address <= 0x0a0019)
            {
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)dsw3;
                }
            }
            else if (address >= 0x0b0000 && address <= 0x0b03ff)
            {
                int offset = (address - 0x0b0000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)cuebrick_nv_r1(offset);
                }
                else
                {
                    result = (sbyte)cuebrick_nv_r2(offset);
                }
            }
            else if (address >= 0x0c0000 && address <= 0x0c0003)
            {
                if (address % 2 == 0)
                {
                    result = (sbyte)cuebrick_snd_r1();
                }
                else
                {
                    result = (sbyte)0;
                }
            }
            else if (address >= 0x100000 && address <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(K052109_word_noA12_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)K052109_word_noA12_r(offset);
                }
            }
            else if (address >= 0x140000 && address <= 0x140007)
            {
                int offset = address - 0x140000;
                result = (sbyte)K051937_r(offset);
            }
            else if (address >= 0x140400 && address <= 0x1407ff)
            {
                int offset = address - 0x140400;
                result = (sbyte)K051960_r(offset);
            }
            return result;
        }
        public static short MReadOpWord_cuebrick(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x01ffff)
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
            else if (address >= 0x040000 && address + 1 <= 0x043fff)
            {
                int offset = address - 0x040000;
                result = (short)(Memory.mainram[offset] * 0x100 + Memory.mainram[offset + 1]);
            }
            else if (address >= 0x060000 && address + 1 <= 0x063fff)
            {
                int offset = address - 0x060000;
                result = (short)(mainram2[offset] * 0x100 + mainram2[offset + 1]);
            }
            return result;
        }
        public static short MReadWord_cuebrick(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x01ffff)
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
            else if (address >= 0x040000 && address + 1 <= 0x043fff)
            {
                int offset = address - 0x040000;
                result = (short)(Memory.mainram[offset] * 0x100 + Memory.mainram[offset + 1]);
            }
            else if (address >= 0x060000 && address + 1 <= 0x063fff)
            {
                int offset = address - 0x060000;
                result = (short)(mainram2[offset] * 0x100 + mainram2[offset + 1]);
            }
            else if (address >= 0x080000 && address + 1 <= 0x080fff)
            {
                int offset = (address - 0x080000) / 2;
                result = (short)Generic.paletteram16[offset];
            }
            else if (address >= 0x0a0000 && address + 1 <= 0x0a0001)
            {
                result = (short)sbyte0;
            }
            else if (address >= 0x0a0002 && address + 1 <= 0x0a0003)
            {
                result = (short)sbyte1;
            }
            else if (address >= 0x0a0004 && address + 1 <= 0x0a0005)
            {
                result = (short)sbyte2;
            }
            else if (address >= 0x0a0010 && address + 1 <= 0x0a0011)
            {
                result = (short)dsw1;
            }
            else if (address >= 0x0a0012 && address + 1 <= 0x0a0013)
            {
                result = (short)dsw2;
            }
            else if (address >= 0x0a0018 && address + 1 <= 0x0a0019)
            {
                result = (short)dsw3;
            }
            else if (address >= 0x0b0000 && address + 1 <= 0x0b03ff)
            {
                int offset = (address - 0x0b0000) / 2;
                result = (short)cuebrick_nv_r(offset);
            }
            else if (address >= 0x0c0000 && address + 1 <= 0x0c0003)
            {
                result = (short)cuebrick_snd_r();
            }
            else if (address >= 0x100000 && address + 1 <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                result = (short)K052109_word_noA12_r(offset);
            }
            else if (address >= 0x140000 && address + 1 <= 0x140007)
            {
                int offset = address - 0x140000;
                result = (short)(K051937_r(offset) * 0x100 + K051937_r(offset + 1));
            }
            else if (address >= 0x140400 && address + 1 <= 0x1407ff)
            {
                int offset = address - 0x140400;
                result = (short)(K051960_r(offset) * 0x100 + K051960_r(offset + 1));
            }
            return result;
        }
        public static int MReadOpLong_cuebrick(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x01ffff)
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
            else if (address >= 0x040000 && address + 3 <= 0x043fff)
            {
                int offset = address - 0x040000;
                result = (int)(Memory.mainram[offset] * 0x1000000 + Memory.mainram[offset + 1] * 0x10000 + Memory.mainram[offset + 2] * 0x100 + Memory.mainram[offset + 3]);
            }
            else if (address >= 0x060000 && address + 3 <= 0x063fff)
            {
                int offset = address - 0x060000;
                result = (int)(mainram2[offset] * 0x1000000 + mainram2[offset + 1] * 0x10000 + mainram2[offset + 2] * 0x100 + mainram2[offset + 3]);
            }
            return result;
        }
        public static int MReadLong_cuebrick(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x01ffff)
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
            else if (address >= 0x040000 && address + 3 <= 0x043fff)
            {
                int offset = address - 0x040000;
                result = (int)(Memory.mainram[offset] * 0x1000000 + Memory.mainram[offset + 1] * 0x10000 + Memory.mainram[offset + 2] * 0x100 + Memory.mainram[offset + 3]);
            }
            else if (address >= 0x060000 && address + 3 <= 0x063fff)
            {
                int offset = address - 0x060000;
                result = (int)(mainram2[offset] * 0x1000000 + mainram2[offset + 1] * 0x10000 + mainram2[offset + 2] * 0x100 + mainram2[offset + 3]);
            }
            else if (address >= 0x080000 && address + 3 <= 0x080fff)
            {
                int offset = (address - 0x080000) / 2;
                result = (int)(Generic.paletteram16[offset] * 0x10000 + Generic.paletteram16[offset + 1]);
            }
            else if (address >= 0x0b0000 && address + 3 <= 0x0b03ff)
            {
                int offset = (address - 0x0b0000) / 2;
                result = (int)(cuebrick_nv_r(offset) * 0x10000 + cuebrick_nv_r(offset + 1));
            }
            else if (address >= 0x0c0000 && address + 3 <= 0x0c0003)
            {
                result = (int)0;//cuebrick_snd_r();
            }
            else if (address >= 0x100000 && address + 3 <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                result = (int)(K052109_word_noA12_r(offset) * 0x10000 + K052109_word_noA12_r(offset + 1));
            }
            else if (address >= 0x140000 && address + 3 <= 0x140007)
            {
                int offset = address - 0x140000;
                result = (short)(K051937_r(offset) * 0x1000000 + K051937_r(offset + 1) * 0x10000 + K051937_r(offset + 2) * 0x100 + K051937_r(offset + 3));
            }
            else if (address >= 0x140400 && address + 3 <= 0x1407ff)
            {
                int offset = address - 0x140400;
                result = (short)(K051960_r(offset) * 0x1000000 + K051960_r(offset + 1) * 0x10000 + K051960_r(offset + 2) * 0x100 + K051960_r(offset + 3));
            }
            return result;
        }
        public static void MWriteByte_cuebrick(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x040000 && address <= 0x043fff)
            {
                int offset = address - 0x040000;
                Memory.mainram[offset] = (byte)value;
            }
            else if (address >= 0x060000 && address <= 0x063fff)
            {
                int offset = address - 0x060000;
                mainram2[offset] = (byte)value;
            }
            else if (address >= 0x080000 && address <= 0x080fff)
            {
                int offset = (address - 0x080000) / 2;
                if (address % 2 == 0)
                {
                    tmnt_paletteram_word_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    tmnt_paletteram_word_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x0a0000 && address <= 0x0a0001)
            {
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    tmnt_0a0000_w2((byte)value);
                }
            }
            else if (address >= 0x0a0008 && address <= 0x0a0009)
            {
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    tmnt_sound_command_w2((byte)value);
                }
            }
            else if (address >= 0x0a0010 && address <= 0x0a0011)
            {
                Generic.watchdog_reset16_w();
            }
            else if (address >= 0x0b0000 && address <= 0x0b03ff)
            {
                int offset = (address - 0x0b0000) / 2;
                if (address % 2 == 0)
                {
                    cuebrick_nv_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    cuebrick_nv_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x0b0400 && address <= 0x0b0401)
            {
                int offset = (address - 0x0b0400) / 2;
                if (address % 2 == 0)
                {
                    cuebrick_nvbank_w1((byte)value);
                }
                else if (address % 2 == 1)
                {
                    cuebrick_nvbank_w2((byte)value);
                }
            }
            else if (address >= 0x0c0000 && address <= 0x0c0003)
            {
                int offset = (address - 0x0c0000) / 2;
                if (address % 2 == 0)
                {
                    cuebrick_snd_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    cuebrick_snd_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x100000 && address <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                if (address % 2 == 0)
                {
                    K052109_word_noA12_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    K052109_word_noA12_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x140000 && address <= 0x140007)
            {
                int offset = address - 0x140000;
                K051937_w(offset, (byte)value);
            }
            else if (address >= 0x140400 && address <= 0x1407ff)
            {
                int offset = address - 0x140400;
                K051960_w(offset, (byte)value);
            }
        }
        public static void MWriteWord_cuebrick(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x040000 && address + 1 <= 0x043fff)
            {
                int offset = address - 0x040000;
                Memory.mainram[offset] = (byte)(value >> 8);
                Memory.mainram[offset + 1] = (byte)value;
            }
            else if (address >= 0x060000 && address + 1 <= 0x063fff)
            {
                int offset = address - 0x060000;
                mainram2[offset] = (byte)(value >> 8);
                mainram2[offset + 1] = (byte)value;
            }
            else if (address >= 0x080000 && address + 1 <= 0x080fff)
            {
                int offset = (address - 0x080000) / 2;
                tmnt_paletteram_word_w(offset, (ushort)value);
            }
            else if (address >= 0x0a0000 && address + 1 <= 0x0a0001)
            {
                tmnt_0a0000_w((ushort)value);
            }
            else if (address >= 0x0a0008 && address + 1 <= 0x0a0009)
            {
                tmnt_sound_command_w((ushort)value);
            }
            else if (address >= 0x0a0010 && address + 1 <= 0x0a0011)
            {
                Generic.watchdog_reset16_w();
            }
            else if (address >= 0x0b0000 && address + 1 <= 0x0b03ff)
            {
                int offset = (address - 0x0b0000) / 2;
                cuebrick_nv_w(offset, (ushort)value);
            }
            else if (address >= 0x0b0400 && address + 1 <= 0x0b0401)
            {
                int offset = (address - 0x0b0400) / 2;
                cuebrick_nvbank_w((ushort)value);
            }
            else if (address >= 0x0c0000 && address + 1 <= 0x0c0003)
            {
                int offset = (address - 0x0c0000) / 2;
                cuebrick_snd_w(offset, (ushort)value);
            }
            else if (address >= 0x100000 && address + 1 <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                K052109_word_noA12_w(offset, (ushort)value);
            }
            else if (address >= 0x140000 && address + 1 <= 0x140007)
            {
                int offset = address - 0x140000;
                K051937_w(offset, (byte)(value >> 8));
                K051937_w(offset + 1, (byte)value);
            }
            else if (address >= 0x140400 && address + 1 <= 0x1407ff)
            {
                int offset = address - 0x140400;
                K051960_w(offset, (byte)(value >> 8));
                K051960_w(offset + 1, (byte)value);
            }
        }
        public static void MWriteLong_cuebrick(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x040000 && address + 3 <= 0x043fff)
            {
                int offset = address - 0x040000;
                Memory.mainram[offset] = (byte)(value >> 24);
                Memory.mainram[offset + 1] = (byte)(value >> 16);
                Memory.mainram[offset + 2] = (byte)(value >> 8);
                Memory.mainram[offset + 3] = (byte)value;
            }
            else if (address >= 0x060000 && address + 3 <= 0x063fff)
            {
                int offset = address - 0x060000;
                mainram2[offset] = (byte)(value >> 24);
                mainram2[offset + 1] = (byte)(value >> 16);
                mainram2[offset + 2] = (byte)(value >> 8);
                mainram2[offset + 3] = (byte)value;
            }
            else if (address >= 0x080000 && address + 3 <= 0x080fff)
            {
                int offset = (address - 0x080000) / 2;
                tmnt_paletteram_word_w(offset, (ushort)(value >> 16));
                tmnt_paletteram_word_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x0b0000 && address + 3 <= 0x0b03ff)
            {
                int offset = (address - 0x0b0000) / 2;
                cuebrick_nv_w(offset, (ushort)(value >> 16));
                cuebrick_nv_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x0c0000 && address + 3 <= 0x0c0003)
            {
                int offset = (address - 0x0c0000) / 2;
                cuebrick_snd_w(offset, (ushort)(value >> 16));
                cuebrick_snd_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x100000 && address + 3 <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                K052109_word_noA12_w(offset, (ushort)(value >> 16));
                K052109_word_noA12_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x140000 && address + 3 <= 0x140007)
            {
                int offset = address - 0x140000;
                K051937_w(offset, (byte)(value >> 24));
                K051937_w(offset + 1, (byte)(value >> 16));
                K051937_w(offset + 2, (byte)(value >> 8));
                K051937_w(offset + 3, (byte)value);
            }
            else if (address >= 0x140400 && address + 3 <= 0x1407ff)
            {
                int offset = address - 0x140400;
                K051960_w(offset, (byte)(value >> 24));
                K051960_w(offset + 1, (byte)(value >> 16));
                K051960_w(offset + 2, (byte)(value >> 8));
                K051960_w(offset + 3, (byte)value);
            }
        }
        public static sbyte MReadOpByte_mia(int address)
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
            else if (address >= 0x040000 && address <= 0x043fff)
            {
                int offset = address - 0x040000;
                result = (sbyte)Memory.mainram[offset];
            }
            else if (address >= 0x060000 && address <= 0x063fff)
            {
                int offset = address - 0x060000;
                result = (sbyte)mainram2[offset];
            }
            return result;
        }
        public static sbyte MReadByte_mia(int address)
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
            else if (address >= 0x040000 && address <= 0x043fff)
            {
                int offset = address - 0x040000;
                result = (sbyte)Memory.mainram[offset];
            }
            else if (address >= 0x060000 && address <= 0x063fff)
            {
                int offset = address - 0x060000;
                result = (sbyte)mainram2[offset];
            }
            else if (address >= 0x080000 && address <= 0x080fff)
            {
                int offset = (address - 0x080000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(Generic.paletteram16[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)Generic.paletteram16[offset];
                }
            }
            else if (address >= 0x0a0000 && address <= 0x0a0001)
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
            else if (address >= 0x0a0002 && address <= 0x0a0003)
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
            else if (address >= 0x0a0004 && address <= 0x0a0005)
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
            else if (address >= 0x0a0010 && address <= 0x0a0011)
            {
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)dsw1;
                }
            }
            else if (address >= 0x0a0012 && address <= 0x0a0013)
            {
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)dsw2;
                }
            }
            else if (address >= 0x0a0018 && address <= 0x0a0019)
            {
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)dsw3;
                }
            }
            else if (address >= 0x100000 && address <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(K052109_word_noA12_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)K052109_word_noA12_r(offset);
                }
            }
            else if (address >= 0x140000 && address <= 0x140007)
            {
                int offset = address - 0x140000;
                result = (sbyte)K051937_r(offset);
            }
            else if (address >= 0x140400 && address <= 0x1407ff)
            {
                int offset = address - 0x140400;
                result = (sbyte)K051960_r(offset);
            }
            return result;
        }
        public static short MReadOpWord_mia(int address)
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
            else if (address >= 0x040000 && address + 1 <= 0x043fff)
            {
                int offset = address - 0x040000;
                result = (short)(Memory.mainram[offset] * 0x100 + Memory.mainram[offset + 1]);
            }
            else if (address >= 0x060000 && address + 1 <= 0x063fff)
            {
                int offset = address - 0x060000;
                result = (short)(mainram2[offset] * 0x100 + mainram2[offset + 1]);
            }
            return result;
        }
        public static short MReadWord_mia(int address)
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
            else if (address >= 0x040000 && address + 1 <= 0x043fff)
            {
                int offset = address - 0x040000;
                result = (short)(Memory.mainram[offset] * 0x100 + Memory.mainram[offset + 1]);
            }
            else if (address >= 0x060000 && address + 1 <= 0x063fff)
            {
                int offset = address - 0x060000;
                result = (short)(mainram2[offset] * 0x100 + mainram2[offset + 1]);
            }
            else if (address >= 0x080000 && address + 1 <= 0x080fff)
            {
                int offset = (address - 0x080000) / 2;
                result = (short)Generic.paletteram16[offset];
            }
            else if (address >= 0x0a0000 && address + 1 <= 0x0a0001)
            {
                result = (short)sbyte0;
            }
            else if (address >= 0x0a0002 && address + 1 <= 0x0a0003)
            {
                result = (short)sbyte1;
            }
            else if (address >= 0x0a0004 && address + 1 <= 0x0a0005)
            {
                result = (short)sbyte2;
            }
            else if (address >= 0x0a0010 && address + 1 <= 0x0a0011)
            {
                result = (short)dsw1;
            }
            else if (address >= 0x0a0012 && address + 1 <= 0x0a0013)
            {
                result = (short)dsw2;
            }
            else if (address >= 0x0a0018 && address + 1 <= 0x0a0019)
            {
                result = (short)dsw3;
            }
            else if (address >= 0x100000 && address + 1 <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                result = (short)K052109_word_noA12_r(offset);
            }
            else if (address >= 0x140000 && address + 1 <= 0x140007)
            {
                int offset = address - 0x140000;
                result = (short)(K051937_r(offset) * 0x100 + K051937_r(offset + 1));
            }
            else if (address >= 0x140400 && address + 1 <= 0x1407ff)
            {
                int offset = address - 0x140400;
                result = (short)(K051960_r(offset) * 0x100 + K051960_r(offset + 1));
            }
            return result;
        }
        public static int MReadOpLong_mia(int address)
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
            else if (address >= 0x040000 && address + 3 <= 0x043fff)
            {
                int offset = address - 0x040000;
                result = (int)(Memory.mainram[offset] * 0x1000000 + Memory.mainram[offset + 1] * 0x10000 + Memory.mainram[offset + 2] * 0x100 + Memory.mainram[offset + 3]);
            }
            else if (address >= 0x060000 && address + 3 <= 0x063fff)
            {
                int offset = address - 0x060000;
                result = (int)(mainram2[offset] * 0x1000000 + mainram2[offset + 1] * 0x10000 + mainram2[offset + 2] * 0x100 + mainram2[offset + 3]);
            }
            return result;
        }
        public static int MReadLong_mia(int address)
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
            else if (address >= 0x040000 && address + 3 <= 0x043fff)
            {
                int offset = address - 0x040000;
                result = (int)(Memory.mainram[offset] * 0x1000000 + Memory.mainram[offset + 1] * 0x10000 + Memory.mainram[offset + 2] * 0x100 + Memory.mainram[offset + 3]);
            }
            else if (address >= 0x060000 && address + 3 <= 0x063fff)
            {
                int offset = address - 0x060000;
                result = (int)(mainram2[offset] * 0x1000000 + mainram2[offset + 1] * 0x10000 + mainram2[offset + 2] * 0x100 + mainram2[offset + 3]);
            }
            else if (address >= 0x080000 && address + 3 <= 0x080fff)
            {
                int offset = (address - 0x080000) / 2;
                result = (int)(Generic.paletteram16[offset] * 0x10000 + Generic.paletteram16[offset + 1]);
            }
            else if (address >= 0x100000 && address + 3 <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                result = (int)(K052109_word_noA12_r(offset) * 0x10000 + K052109_word_noA12_r(offset + 1));
            }
            else if (address >= 0x140000 && address + 3 <= 0x140007)
            {
                int offset = address - 0x140000;
                result = (short)(K051937_r(offset) * 0x1000000 + K051937_r(offset + 1) * 0x10000 + K051937_r(offset + 2) * 0x100 + K051937_r(offset + 3));
            }
            else if (address >= 0x140400 && address + 3 <= 0x1407ff)
            {
                int offset = address - 0x140400;
                result = (short)(K051960_r(offset) * 0x1000000 + K051960_r(offset + 1) * 0x10000 + K051960_r(offset + 2) * 0x100 + K051960_r(offset + 3));
            }
            return result;
        }
        public static void MWriteByte_mia(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x040000 && address <= 0x043fff)
            {
                int offset = address - 0x040000;
                Memory.mainram[offset] = (byte)value;
            }
            else if (address >= 0x060000 && address <= 0x063fff)
            {
                int offset = address - 0x060000;
                mainram2[offset] = (byte)value;
            }
            else if (address >= 0x080000 && address <= 0x080fff)
            {
                int offset = (address - 0x080000) / 2;
                if (address % 2 == 0)
                {
                    tmnt_paletteram_word_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    tmnt_paletteram_word_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x0a0000 && address <= 0x0a0001)
            {
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    tmnt_0a0000_w2((byte)value);
                }
            }
            else if (address >= 0x0a0008 && address <= 0x0a0009)
            {
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    tmnt_sound_command_w2((byte)value);
                }
            }
            else if (address >= 0x0a0010 && address <= 0x0a0011)
            {
                Generic.watchdog_reset16_w();
            }
            else if (address >= 0x100000 && address <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                if (address % 2 == 0)
                {
                    K052109_word_noA12_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    K052109_word_noA12_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x140000 && address <= 0x140007)
            {
                int offset = address - 0x140000;
                K051937_w(offset, (byte)value);
            }
            else if (address >= 0x140400 && address <= 0x1407ff)
            {
                int offset = address - 0x140400;
                K051960_w(offset, (byte)value);
            }
        }
        public static void MWriteWord_mia(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x040000 && address + 1 <= 0x043fff)
            {
                int offset = address - 0x040000;
                Memory.mainram[offset] = (byte)(value >> 8);
                Memory.mainram[offset + 1] = (byte)value;
            }
            else if (address >= 0x060000 && address + 1 <= 0x063fff)
            {
                int offset = address - 0x060000;
                mainram2[offset] = (byte)(value >> 8);
                mainram2[offset + 1] = (byte)value;
            }
            else if (address >= 0x080000 && address + 1 <= 0x080fff)
            {
                int offset = (address - 0x080000) / 2;
                tmnt_paletteram_word_w(offset, (ushort)value);
            }
            else if (address >= 0x0a0000 && address + 1 <= 0x0a0001)
            {
                tmnt_0a0000_w((ushort)value);
            }
            else if (address >= 0x0a0008 && address + 1 <= 0x0a0009)
            {
                tmnt_sound_command_w((ushort)value);
            }
            else if (address >= 0x0a0010 && address + 1 <= 0x0a0011)
            {
                Generic.watchdog_reset16_w();
            }
            else if (address >= 0x100000 && address + 1 <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                K052109_word_noA12_w(offset, (ushort)value);
            }
            else if (address >= 0x140000 && address + 1 <= 0x140007)
            {
                int offset = address - 0x140000;
                K051937_w(offset, (byte)(value >> 8));
                K051937_w(offset + 1, (byte)value);
            }
            else if (address >= 0x140400 && address + 1 <= 0x1407ff)
            {
                int offset = address - 0x140400;
                K051960_w(offset, (byte)(value >> 8));
                K051960_w(offset + 1, (byte)value);
            }
        }
        public static void MWriteLong_mia(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x040000 && address + 3 <= 0x043fff)
            {
                int offset = address - 0x040000;
                Memory.mainram[offset] = (byte)(value >> 24);
                Memory.mainram[offset + 1] = (byte)(value >> 16);
                Memory.mainram[offset + 2] = (byte)(value >> 8);
                Memory.mainram[offset + 3] = (byte)value;
            }
            else if (address >= 0x060000 && address + 3 <= 0x063fff)
            {
                int offset = address - 0x060000;
                mainram2[offset] = (byte)(value >> 24);
                mainram2[offset + 1] = (byte)(value >> 16);
                mainram2[offset + 2] = (byte)(value >> 8);
                mainram2[offset + 3] = (byte)value;
            }
            else if (address >= 0x080000 && address + 3 <= 0x080fff)
            {
                int offset = (address - 0x080000) / 2;
                tmnt_paletteram_word_w(offset, (ushort)(value >> 16));
                tmnt_paletteram_word_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x100000 && address + 3 <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                K052109_word_noA12_w(offset, (ushort)(value >> 16));
                K052109_word_noA12_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x140000 && address + 3 <= 0x140007)
            {
                int offset = address - 0x140000;
                K051937_w(offset, (byte)(value >> 24));
                K051937_w(offset + 1, (byte)(value >> 16));
                K051937_w(offset + 2, (byte)(value >> 8));
                K051937_w(offset + 3, (byte)value);
            }
            else if (address >= 0x140400 && address + 3 <= 0x1407ff)
            {
                int offset = address - 0x140400;
                K051960_w(offset, (byte)(value >> 24));
                K051960_w(offset + 1, (byte)(value >> 16));
                K051960_w(offset + 2, (byte)(value >> 8));
                K051960_w(offset + 3, (byte)value);
            }
        }
        public static sbyte MReadOpByte_tmnt(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address <= 0x05ffff)
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
            else if (address >= 0x060000 && address <= 0x063fff)
            {
                int offset = address - 0x060000;
                result = (sbyte)Memory.mainram[offset];
            }
            return result;
        }
        public static sbyte MReadByte_tmnt(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address <= 0x05ffff)
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
            else if (address >= 0x060000 && address <= 0x063fff)
            {
                int offset = address - 0x060000;
                result = (sbyte)Memory.mainram[offset];
            }
            else if (address >= 0x080000 && address <= 0x080fff)
            {
                int offset = (address - 0x080000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(Generic.paletteram16[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)Generic.paletteram16[offset];
                }
            }
            else if (address >= 0x0a0000 && address <= 0x0a0001)
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
            else if (address >= 0x0a0002 && address <= 0x0a0003)
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
            else if (address >= 0x0a0004 && address <= 0x0a0005)
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
            else if (address >= 0x0a0006 && address <= 0x0a0007)
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
            else if (address >= 0x0a0010 && address <= 0x0a0011)
            {
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)dsw1;
                }
            }
            else if (address >= 0x0a0012 && address <= 0x0a0013)
            {
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)dsw2;
                }
            }
            else if (address >= 0x0a0014 && address <= 0x0a0015)
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
            else if (address >= 0x0a0018 && address <= 0x0a0019)
            {
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)dsw3;
                }
            }
            else if (address >= 0x100000 && address <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(K052109_word_noA12_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)K052109_word_noA12_r(offset);
                }
            }
            else if (address >= 0x140000 && address <= 0x140007)
            {
                int offset = address - 0x140000;
                result = (sbyte)K051937_r(offset);
            }
            else if (address >= 0x140400 && address <= 0x1407ff)
            {
                int offset = address - 0x140400;
                result = (sbyte)K051960_r(offset);
            }
            return result;
        }
        public static short MReadOpWord_tmnt(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x05ffff)
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
            else if (address >= 0x060000 && address + 1 <= 0x063fff)
            {
                int offset = address - 0x060000;
                result = (short)(Memory.mainram[offset] * 0x100 + Memory.mainram[offset + 1]);
            }
            return result;
        }
        public static short MReadWord_tmnt(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x05ffff)
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
            else if (address >= 0x060000 && address + 1 <= 0x063fff)
            {
                int offset = address - 0x060000;
                result = (short)(Memory.mainram[offset] * 0x100 + Memory.mainram[offset + 1]);
            }
            else if (address >= 0x080000 && address + 1 <= 0x080fff)
            {
                int offset = (address - 0x080000) / 2;
                result = (short)Generic.paletteram16[offset];
            }
            else if (address >= 0x0a0000 && address + 1 <= 0x0a0001)
            {
                result = (short)sbyte0;
            }
            else if (address >= 0x0a0002 && address + 1 <= 0x0a0003)
            {
                result = (short)sbyte1;
            }
            else if (address >= 0x0a0004 && address + 1 <= 0x0a0005)
            {
                result = (short)sbyte2;
            }
            else if (address >= 0x0a0006 && address + 1 <= 0x0a0007)
            {
                result = (short)sbyte3;
            }
            else if (address >= 0x0a0010 && address + 1 <= 0x0a0011)
            {
                result = (short)dsw1;
            }
            else if (address >= 0x0a0012 && address + 1 <= 0x0a0013)
            {
                result = (short)dsw2;
            }
            else if (address >= 0x0a0014 && address + 1 <= 0x0a0015)
            {
                result = (short)sbyte4;
            }
            else if (address >= 0x0a0018 && address + 1 <= 0x0a0019)
            {
                result = (short)dsw3;
            }
            else if (address >= 0x100000 && address + 1 <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                result = (short)K052109_word_noA12_r(offset);
            }
            else if (address >= 0x140000 && address + 1 <= 0x140007)
            {
                int offset = address - 0x140000;
                result = (short)(K051937_r(offset) * 0x100 + K051937_r(offset + 1));
            }
            else if (address >= 0x140400 && address + 1 <= 0x1407ff)
            {
                int offset = address - 0x140400;
                result = (short)(K051960_r(offset) * 0x100 + K051960_r(offset + 1));
            }
            return result;
        }
        public static int MReadOpLong_tmnt(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x05ffff)
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
            else if (address >= 0x060000 && address + 3 <= 0x063fff)
            {
                int offset = address - 0x060000;
                result = (int)(Memory.mainram[offset] * 0x1000000 + Memory.mainram[offset + 1] * 0x10000 + Memory.mainram[offset + 2] * 0x100 + Memory.mainram[offset + 3]);
            }
            return result;
        }
        public static int MReadLong_tmnt(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x05ffff)
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
            else if (address >= 0x060000 && address + 3 <= 0x063fff)
            {
                int offset = address - 0x060000;
                result = (int)(Memory.mainram[offset] * 0x1000000 + Memory.mainram[offset + 1] * 0x10000 + Memory.mainram[offset + 2] * 0x100 + Memory.mainram[offset + 3]);
            }
            else if (address >= 0x080000 && address + 3 <= 0x080fff)
            {
                int offset = (address - 0x080000) / 2;
                result = (int)(Generic.paletteram16[offset] * 0x10000 + Generic.paletteram16[offset + 1]);
            }
            else if (address >= 0x100000 && address + 3 <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                result = (int)(K052109_word_noA12_r(offset) * 0x10000 + K052109_word_noA12_r(offset + 1));
            }
            else if (address >= 0x140000 && address + 3 <= 0x140007)
            {
                int offset = address - 0x140000;
                result = (int)(K051937_r(offset) * 0x1000000 + K051937_r(offset + 1) * 0x10000 + K051937_r(offset + 2) * 0x100 + K051937_r(offset + 3));
            }
            else if (address >= 0x140400 && address + 3 <= 0x1407ff)
            {
                int offset = address - 0x140400;
                result = (int)(K051960_r(offset) * 0x1000000 + K051960_r(offset + 1) * 0x10000 + K051960_r(offset + 2) * 0x100 + K051960_r(offset + 3));
            }
            return result;
        }
        public static void MWriteByte_tmnt(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x060000 && address <= 0x063fff)
            {
                int offset = address - 0x060000;
                Memory.mainram[offset] = (byte)value;
            }
            else if (address >= 0x080000 && address <= 0x080fff)
            {
                int offset = (address - 0x080000) / 2;
                if (address % 2 == 0)
                {
                    tmnt_paletteram_word_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    tmnt_paletteram_word_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x0a0000 && address <= 0x0a0001)
            {
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    tmnt_0a0000_w2((byte)value);
                }
            }
            else if (address >= 0x0a0008 && address <= 0x0a0009)
            {
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    tmnt_sound_command_w2((byte)value);
                }
            }
            else if (address >= 0x0a0010 && address <= 0x0a0011)
            {
                Generic.watchdog_reset16_w();
            }
            else if (address >= 0x0c0000 && address <= 0x0c0001)
            {
                tmnt_priority_w2((byte)value);
            }
            else if (address >= 0x100000 && address <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                if (address % 2 == 0)
                {
                    K052109_word_noA12_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    K052109_word_noA12_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x140000 && address <= 0x140007)
            {
                int offset = address - 0x140000;
                K051937_w(offset, (byte)value);
            }
            else if (address >= 0x140400 && address <= 0x1407ff)
            {
                int offset = address - 0x140400;
                K051960_w(offset, (byte)value);
            }
        }
        public static void MWriteWord_tmnt(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x060000 && address + 1 <= 0x063fff)
            {
                int offset = address - 0x060000;
                Memory.mainram[offset] = (byte)(value >> 8);
                Memory.mainram[offset + 1] = (byte)value;
            }
            else if (address >= 0x080000 && address + 1 <= 0x080fff)
            {
                int offset = (address - 0x080000) / 2;
                tmnt_paletteram_word_w(offset, (ushort)value);
            }
            else if (address >= 0x0a0000 && address + 1 <= 0x0a0001)
            {
                tmnt_0a0000_w((ushort)value);
            }
            else if (address >= 0x0a0008 && address + 1 <= 0x0a0009)
            {
                tmnt_sound_command_w((ushort)value);
            }
            else if (address >= 0x0a0010 && address + 1 <= 0x0a0011)
            {
                Generic.watchdog_reset16_w();
            }
            else if (address >= 0x0c0000 && address + 1 <= 0x0c0001)
            {
                tmnt_priority_w((ushort)value);
            }
            else if (address >= 0x100000 && address + 1 <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                K052109_word_noA12_w(offset, (ushort)value);
            }
            else if (address >= 0x140000 && address + 1 <= 0x140007)
            {
                int offset = address - 0x140000;
                K051937_w(offset, (byte)(value >> 8));
                K051937_w(offset + 1, (byte)value);
            }
            else if (address >= 0x140400 && address + 1 <= 0x1407ff)
            {
                int offset = address - 0x140400;
                K051960_w(offset, (byte)(value >> 8));
                K051960_w(offset + 1, (byte)value);
            }
        }
        public static void MWriteLong_tmnt(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x060000 && address + 3 <= 0x063fff)
            {
                int offset = address - 0x060000;
                Memory.mainram[offset] = (byte)(value >> 24);
                Memory.mainram[offset + 1] = (byte)(value >> 16);
                Memory.mainram[offset + 2] = (byte)(value >> 8);
                Memory.mainram[offset + 3] = (byte)value;
            }
            else if (address >= 0x080000 && address + 3 <= 0x080fff)
            {
                int offset = (address - 0x080000) / 2;
                tmnt_paletteram_word_w(offset, (ushort)(value >> 16));
                tmnt_paletteram_word_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x100000 && address + 3 <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                K052109_word_noA12_w(offset, (ushort)(value >> 16));
                K052109_word_noA12_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x140000 && address + 3 <= 0x140007)
            {
                int offset = address - 0x140000;
                K051937_w(offset, (byte)(value >> 24));
                K051937_w(offset + 1, (byte)(value >> 16));
                K051937_w(offset + 2, (byte)(value >> 8));
                K051937_w(offset + 3, (byte)value);
            }
            else if (address >= 0x140400 && address + 3 <= 0x1407ff)
            {
                int offset = address - 0x140400;
                K051960_w(offset, (byte)(value >> 24));
                K051960_w(offset + 1, (byte)(value >> 16));
                K051960_w(offset + 2, (byte)(value >> 8));
                K051960_w(offset + 3, (byte)value);
            }
        }
        public static sbyte MReadOpByte_punkshot(int address)
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
            else if (address >= 0x080000 && address <= 0x083fff)
            {
                int offset = address - 0x080000;
                result = (sbyte)Memory.mainram[offset];
            }
            return result;
        }
        public static sbyte MReadByte_punkshot(int address)
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
            else if (address >= 0x080000 && address <= 0x083fff)
            {
                int offset = address - 0x080000;
                result = (sbyte)Memory.mainram[offset];
            }
            else if (address >= 0x090000 && address <= 0x090fff)
            {
                int offset = (address - 0x090000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(Generic.paletteram16[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)Generic.paletteram16[offset];
                }
            }
            else if (address >= 0x0a0000 && address <= 0x0a0001)
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
            else if (address >= 0x0a0002 && address <= 0x0a0003)
            {
                if (address % 2 == 0)
                {
                    result = (sbyte)dsw3;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)sbyte0;
                }
            }
            else if (address >= 0x0a0004 && address <= 0x0a0005)
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
            else if (address >= 0x0a0006 && address <= 0x0a0007)
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
            else if (address >= 0x0a0040 && address <= 0x0a0043)
            {
                int offset = (address - 0x0a0040) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)punkshot_sound_r(offset);
                }
            }
            else if (address >= 0x100000 && address <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(K052109_word_noA12_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)K052109_word_noA12_r(offset);
                }
            }
            else if (address >= 0x110000 && address <= 0x110007)
            {
                int offset = address - 0x110000;
                result = (sbyte)K051937_r(offset);
            }
            else if (address >= 0x110400 && address <= 0x1107ff)
            {
                int offset = address - 0x110400;
                result = (sbyte)K051960_r(offset);
            }
            else if (address >= 0xfffffc && address <= 0xffffff)
            {
                result = (sbyte)punkshot_kludge_r1();
            }
            return result;
        }
        public static short MReadOpWord_punkshot(int address)
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
            else if (address >= 0x080000 && address + 1 <= 0x083fff)
            {
                int offset = address - 0x080000;
                result = (short)(Memory.mainram[offset] * 0x100 + Memory.mainram[offset + 1]);
            }
            return result;
        }
        public static short MReadWord_punkshot(int address)
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
            else if (address >= 0x080000 && address + 1 <= 0x083fff)
            {
                int offset = address - 0x080000;
                result = (short)(Memory.mainram[offset] * 0x100 + Memory.mainram[offset + 1]);
            }
            else if (address >= 0x090000 && address + 1 <= 0x090fff)
            {
                int offset = (address - 0x090000) / 2;
                result = (short)Generic.paletteram16[offset];
            }
            else if (address >= 0x0a0000 && address + 1 <= 0x0a0001)
            {
                result = (short)((dsw2 << 8) | dsw1);
            }
            else if (address >= 0x0a0002 && address + 1 <= 0x0a0003)
            {
                result = (short)((dsw3 << 8) | (byte)sbyte0);
            }
            else if (address >= 0x0a0004 && address + 1 <= 0x0a0005)
            {
                result = (short)(((byte)sbyte4 << 8) | (byte)sbyte3);
            }
            else if (address >= 0x0a0006 && address + 1 <= 0x0a0007)
            {
                result = (short)(((byte)sbyte2 << 8) | (byte)sbyte1);
            }
            else if (address >= 0x0a0040 && address + 1 <= 0x0a0043)
            {
                int offset = (address - 0x0a0040) / 2;
                result = (short)punkshot_sound_r(offset);
            }
            else if (address >= 0x100000 && address + 1 <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                result = (short)K052109_word_noA12_r(offset);
            }
            else if (address >= 0x110000 && address + 1 <= 0x110007)
            {
                int offset = address - 0x110000;
                result = (short)(K051937_r(offset) * 0x100 + K051937_r(offset + 1));
            }
            else if (address >= 0x110400 && address + 1 <= 0x1107ff)
            {
                int offset = address - 0x110400;
                result = (short)(K051960_r(offset) * 0x100 + K051960_r(offset + 1));
            }
            else if (address >= 0xfffffc && address + 1 <= 0xffffff)
            {
                result = (short)punkshot_kludge_r();
            }
            return result;
        }
        public static int MReadOpLong_punkshot(int address)
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
            else if (address >= 0x080000 && address + 3 <= 0x083fff)
            {
                int offset = address - 0x080000;
                result = (int)(Memory.mainram[offset] * 0x1000000 + Memory.mainram[offset + 1] * 0x10000 + Memory.mainram[offset + 2] * 0x100 + Memory.mainram[offset + 3]);
            }
            return result;
        }
        public static int MReadLong_punkshot(int address)
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
            else if (address >= 0x080000 && address + 3 <= 0x083fff)
            {
                int offset = address - 0x080000;
                result = (int)(Memory.mainram[offset] * 0x1000000 + Memory.mainram[offset + 1] * 0x10000 + Memory.mainram[offset + 2] * 0x100 + Memory.mainram[offset + 3]);
            }
            else if (address >= 0x090000 && address + 3 <= 0x090fff)
            {
                int offset = (address - 0x090000) / 2;
                result = (int)(Generic.paletteram16[offset] * 0x10000 + Generic.paletteram16[offset + 1]);
            }
            else if (address >= 0x0a0040 && address + 3 <= 0x0a0043)
            {
                int offset = (address - 0x0a0040) / 2;
                result = (int)(punkshot_sound_r(offset) * 0x10000 + punkshot_sound_r(offset + 1));
            }
            else if (address >= 0x100000 && address + 3 <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                result = (int)(K052109_word_noA12_r(offset) * 0x10000 + K052109_word_noA12_r(offset + 1));
            }
            else if (address >= 0x110000 && address + 3 <= 0x110007)
            {
                int offset = address - 0x110000;
                result = (int)(K051937_r(offset) * 0x1000000 + K051937_r(offset + 1) * 0x10000 + K051937_r(offset + 2) * 0x100 + K051937_r(offset + 3));
            }
            else if (address >= 0x110400 && address + 3 <= 0x1107ff)
            {
                int offset = address - 0x110400;
                result = (int)(K051960_r(offset) * 0x1000000 + K051960_r(offset + 1) * 0x10000 + K051960_r(offset + 2) * 0x100 + K051960_r(offset + 3));
            }
            else if (address >= 0xfffffc && address + 3 <= 0xffffff)
            {
                result = (int)punkshot_kludge_r();
            }
            return result;
        }
        public static void MWriteByte_punkshot(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x080000 && address <= 0x083fff)
            {
                int offset = address - 0x080000;
                Memory.mainram[offset] = (byte)value;
            }
            else if (address >= 0x090000 && address <= 0x090fff)
            {
                int offset = (address - 0x090000) / 2;
                if (address % 2 == 0)
                {
                    Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x0a0020 && address <= 0x0a0021)
            {
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    punkshot_0a0020_w2((byte)value);
                }
            }
            else if (address >= 0x0a0040 && address <= 0x0a0041)
            {
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    K053260.k053260_0_lsb_w2(0, (byte)value);
                }
            }
            else if (address >= 0x0a0060 && address <= 0x0a007f)
            {
                int offset = (address - 0x0a0060) / 2;
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    K053251_lsb_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x0a0080 && address <= 0x0a0081)
            {
                Generic.watchdog_reset16_w();
            }
            else if (address >= 0x100000 && address <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                if (address % 2 == 0)
                {
                    punkshot_K052109_word_noA12_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    punkshot_K052109_word_noA12_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x110000 && address <= 0x110007)
            {
                int offset = address - 0x110000;
                K051937_w(offset, (byte)value);
            }
            else if (address >= 0x110400 && address <= 0x1107ff)
            {
                int offset = address - 0x110400;
                K051960_w(offset, (byte)value);
            }
        }
        public static void MWriteWord_punkshot(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x080000 && address + 1 <= 0x083fff)
            {
                int offset = address - 0x080000;
                Memory.mainram[offset] = (byte)(value >> 8);
                Memory.mainram[offset + 1] = (byte)value;
            }
            else if (address >= 0x090000 && address + 1 <= 0x090fff)
            {
                int offset = (address - 0x090000) / 2;
                Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w(offset, (ushort)value);
            }
            else if (address >= 0x0a0020 && address + 1 <= 0x0a0021)
            {
                punkshot_0a0020_w((ushort)value);
            }
            else if (address >= 0x0a0040 && address + 1 <= 0x0a0041)
            {
                K053260.k053260_0_lsb_w(0, (ushort)value);
            }
            else if (address >= 0x0a0060 && address + 1 <= 0x0a007f)
            {
                int offset = (address - 0x0a0060) / 2;
                K053251_lsb_w(offset, (ushort)value);
            }
            else if (address >= 0x0a0080 && address + 1 <= 0x0a0081)
            {
                Generic.watchdog_reset16_w();
            }
            else if (address >= 0x100000 && address + 1 <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                punkshot_K052109_word_noA12_w(offset, (ushort)value);
            }
            else if (address >= 0x110000 && address + 1 <= 0x110007)
            {
                int offset = address - 0x110000;
                K051937_w(offset, (byte)(value >> 8));
                K051937_w(offset + 1, (byte)value);
            }
            else if (address >= 0x110400 && address + 1 <= 0x1107ff)
            {
                int offset = address - 0x110400;
                K051960_w(offset, (byte)(value >> 8));
                K051960_w(offset + 1, (byte)value);
            }
        }
        public static void MWriteLong_punkshot(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x080000 && address + 3 <= 0x083fff)
            {
                int offset = address - 0x080000;
                Memory.mainram[offset] = (byte)(value >> 24);
                Memory.mainram[offset + 1] = (byte)(value >> 16);
                Memory.mainram[offset + 2] = (byte)(value >> 8);
                Memory.mainram[offset + 3] = (byte)value;
            }
            else if (address >= 0x090000 && address + 3 <= 0x090fff)
            {
                int offset = (address - 0x090000) / 2;
                Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w(offset, (ushort)(value >> 16));
                Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x0a0060 && address + 3 <= 0x0a007f)
            {
                int offset = (address - 0x0a0060) / 2;
                K053251_lsb_w(offset, (ushort)(value >> 16));
                K053251_lsb_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x100000 && address + 3 <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                punkshot_K052109_word_noA12_w(offset, (ushort)(value >> 16));
                punkshot_K052109_word_noA12_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x110000 && address + 3 <= 0x110007)
            {
                int offset = address - 0x110000;
                K051937_w(offset, (byte)(value >> 24));
                K051937_w(offset + 1, (byte)(value >> 16));
                K051937_w(offset + 2, (byte)(value >> 8));
                K051937_w(offset + 3, (byte)value);
            }
            else if (address >= 0x110400 && address + 3 <= 0x1107ff)
            {
                int offset = address - 0x110400;
                K051960_w(offset, (byte)(value >> 24));
                K051960_w(offset + 1, (byte)(value >> 16));
                K051960_w(offset + 2, (byte)(value >> 8));
                K051960_w(offset + 3, (byte)value);
            }
        }
        public static sbyte MReadOpByte_lgtnfght(int address)
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
            else if (address >= 0x090000 && address <= 0x093fff)
            {
                int offset = address - 0x090000;
                result = (sbyte)Memory.mainram[offset];
            }
            return result;
        }
        public static sbyte MReadByte_lgtnfght(int address)
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
            else if (address >= 0x080000 && address <= 0x080fff)
            {
                int offset = (address - 0x080000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(Generic.paletteram16[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)Generic.paletteram16[offset];
                }
            }
            else if (address >= 0x090000 && address <= 0x093fff)
            {
                int offset = address - 0x090000;
                result = (sbyte)Memory.mainram[offset];
            }
            else if (address >= 0x0a0000 && address <= 0x0a0001)
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
            else if (address >= 0x0a0002 && address <= 0x0a0003)
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
            else if (address >= 0x0a0004 && address <= 0x0a0005)
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
            else if (address >= 0x0a0006 && address <= 0x0a0007)
            {
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)dsw1;
                }
            }
            else if (address >= 0x0a0008 && address <= 0x0a0009)
            {
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)dsw2;
                }
            }
            else if (address >= 0x0a0010 && address <= 0x0a0011)
            {
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)dsw3;
                }
            }
            else if (address >= 0x0a0020 && address <= 0x0a0023)
            {
                int offset = (address - 0x0a0020) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(punkshot_sound_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)punkshot_sound_r(offset);
                }
            }
            else if (address >= 0x0b0000 && address <= 0x0b3fff)
            {
                int offset = (address - 0x0b0000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(K053245_scattered_word_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)K053245_scattered_word_r(offset);
                }
            }
            else if (address >= 0x0c0000 && address <= 0x0c001f)
            {
                int offset = (address - 0x0c0000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(K053244_word_noA1_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)K053244_word_noA1_r(offset);
                }
            }
            else if (address >= 0x100000 && address <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(K052109_word_noA12_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)K052109_word_noA12_r(offset);
                }
            }
            return result;
        }
        public static short MReadOpWord_lgtnfght(int address)
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
            else if (address >= 0x090000 && address + 1 <= 0x093fff)
            {
                int offset = address - 0x090000;
                result = (short)(Memory.mainram[offset] * 0x100 + Memory.mainram[offset + 1]);
            }
            return result;
        }
        public static short MReadWord_lgtnfght(int address)
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
            else if (address >= 0x080000 && address + 1 <= 0x080fff)
            {
                int offset = (address - 0x080000) / 2;
                result = (short)Generic.paletteram16[offset];
            }
            else if (address >= 0x090000 && address + 1 <= 0x093fff)
            {
                int offset = address - 0x090000;
                result = (short)(Memory.mainram[offset] * 0x100 + Memory.mainram[offset + 1]);
            }
            else if (address >= 0x0a0000 && address + 1 <= 0x0a0001)
            {
                int offset = (address - 0x0a0000) / 2;
                result = (short)sbyte0;
            }
            else if (address >= 0x0a0002 && address + 1 <= 0x0a0003)
            {
                int offset = (address - 0x0a0002) / 2;
                result = (short)sbyte1;
            }
            else if (address >= 0x0a0004 && address + 1 <= 0x0a0005)
            {
                int offset = (address - 0x0a0004) / 2;
                result = (short)sbyte2;
            }
            else if (address >= 0x0a0006 && address + 1 <= 0x0a0007)
            {
                int offset = (address - 0x0a0006) / 2;
                result = (short)dsw1;
            }
            else if (address >= 0x0a0008 && address + 1 <= 0x0a0009)
            {
                int offset = (address - 0x0a0008) / 2;
                result = (short)dsw2;
            }
            else if (address >= 0x0a0010 && address + 1 <= 0x0a0011)
            {
                int offset = (address - 0x0a0010) / 2;
                result = (short)dsw3;
            }
            else if (address >= 0x0a0020 && address + 1 <= 0x0a0023)
            {
                int offset = (address - 0x0a0020) / 2;
                result = (short)punkshot_sound_r(offset);
            }
            else if (address >= 0x0b0000 && address + 1 <= 0x0b3fff)
            {
                int offset = (address - 0x0b0000) / 2;
                result = (short)K053245_scattered_word_r(offset);
            }
            else if (address >= 0x0c0000 && address + 1 <= 0x0c001f)
            {
                int offset = (address - 0x0c0000) / 2;
                result = (short)K053244_word_noA1_r(offset);
            }
            else if (address >= 0x100000 && address + 1 <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                result = (short)K052109_word_noA12_r(offset);
            }
            return result;
        }
        public static int MReadOpLong_lgtnfght(int address)
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
            else if (address >= 0x090000 && address + 3 <= 0x093fff)
            {
                int offset = address - 0x090000;
                result = (int)(Memory.mainram[offset] * 0x1000000 + Memory.mainram[offset + 1] * 0x10000 + Memory.mainram[offset + 2] * 0x100 + Memory.mainram[offset + 3]);
            }
            return result;
        }
        public static int MReadLong_lgtnfght(int address)
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
            else if (address >= 0x080000 && address + 3 <= 0x080fff)
            {
                int offset = (address - 0x080000) / 2;
                result = (int)(Generic.paletteram16[offset] * 0x10000 + Generic.paletteram16[offset + 1]);
            }
            else if (address >= 0x090000 && address + 3 <= 0x093fff)
            {
                int offset = address - 0x090000;
                result = (int)(Memory.mainram[offset] * 0x1000000 + Memory.mainram[offset + 1] * 0x10000 + Memory.mainram[offset + 2] * 0x100 + Memory.mainram[offset + 3]);
            }
            else if (address >= 0x0a0020 && address + 3 <= 0x0a0023)
            {
                int offset = (address - 0x0a0020) / 2;
                result = (int)(punkshot_sound_r(offset) * 0x10000 + punkshot_sound_r(offset + 1));
            }
            else if (address >= 0x0b0000 && address + 3 <= 0x0b3fff)
            {
                int offset = (address - 0x0b0000) / 2;
                result = (int)(K053245_scattered_word_r(offset) * 0x10000 + K053245_scattered_word_r(offset + 1));
            }
            else if (address >= 0x0c0000 && address + 3 <= 0x0c001f)
            {
                int offset = (address - 0x0c0000) / 2;
                result = (int)(K053244_word_noA1_r(offset) * 0x10000 + K053244_word_noA1_r(offset + 1));
            }
            else if (address >= 0x100000 && address + 3 <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                result = (int)(K052109_word_noA12_r(offset) * 0x10000 + K052109_word_noA12_r(offset + 1));
            }
            return result;
        }
        public static void MWriteByte_lgtnfght(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x080000 && address <= 0x080fff)
            {
                int offset = (address - 0x080000) / 2;
                if (address % 2 == 0)
                {
                    Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x090000 && address <= 0x093fff)
            {
                int offset = address - 0x090000;
                Memory.mainram[offset] = (byte)value;
            }
            else if (address >= 0x0a0018 && address <= 0x0a0019)
            {
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    lgtnfght_0a0018_w2((byte)value);
                }
            }
            else if (address >= 0x0a0020 && address <= 0x0a0021)
            {
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    K053260.k053260_0_lsb_w2(0, (byte)value);
                }
            }
            else if (address >= 0x0a0028 && address <= 0x0a0029)
            {
                Generic.watchdog_reset16_w();
            }
            else if (address >= 0x0b0000 && address <= 0x0b3fff)
            {
                int offset = (address - 0x0b0000) / 2;
                if (address % 2 == 0)
                {
                    K053245_scattered_word_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    K053245_scattered_word_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x0c0000 && address <= 0x0c001f)
            {
                int offset = (address - 0x0c0000) / 2;
                if (address % 2 == 0)
                {
                    K053244_word_noA1_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    K053244_word_noA1_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x0e0000 && address <= 0x0e001f)
            {
                int offset = (address - 0x0e0000) / 2;
                if (address % 2 == 0)
                {

                }
                else if (address % 2 == 1)
                {
                    K053251_lsb_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x100000 && address <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
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
        public static void MWriteWord_lgtnfght(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x080000 && address + 1 <= 0x080fff)
            {
                int offset = (address - 0x080000) / 2;
                Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w(offset, (ushort)value);
            }
            else if (address >= 0x090000 && address + 1 <= 0x093fff)
            {
                int offset = address - 0x090000;
                Memory.mainram[offset] = (byte)(value >> 8);
                Memory.mainram[offset + 1] = (byte)value;
            }
            else if (address >= 0x0a0018 && address + 1 <= 0x0a0019)
            {
                lgtnfght_0a0018_w((ushort)value);
            }
            else if (address >= 0x0a0020 && address + 1 <= 0x0a0021)
            {
                K053260.k053260_0_lsb_w(0, (ushort)value);
            }
            else if (address >= 0x0a0028 && address + 1 <= 0x0a0029)
            {
                Generic.watchdog_reset16_w();
            }
            else if (address >= 0x0b0000 && address + 1 <= 0x0b3fff)
            {
                int offset = (address - 0x0b0000) / 2;
                K053245_scattered_word_w(offset, (ushort)value);
            }
            else if (address >= 0x0c0000 && address + 1 <= 0x0c001f)
            {
                int offset = (address - 0x0c0000) / 2;
                K053244_word_noA1_w(offset, (ushort)value);
            }
            else if (address >= 0x0e0000 && address + 1 <= 0x0e001f)
            {
                int offset = (address - 0x0e0000) / 2;
                K053251_lsb_w(offset, (ushort)value);
            }
            else if (address >= 0x100000 && address + 1 <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                K052109_word_noA12_w(offset, (ushort)value);
            }
        }
        public static void MWriteLong_lgtnfght(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x080000 && address + 3 <= 0x080fff)
            {
                int offset = (address - 0x080000) / 2;
                Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w(offset, (ushort)(value >> 16));
                Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x090000 && address + 3 <= 0x093fff)
            {
                int offset = address - 0x090000;
                Memory.mainram[offset] = (byte)(value >> 24);
                Memory.mainram[offset + 1] = (byte)(value >> 16);
                Memory.mainram[offset + 2] = (byte)(value >> 8);
                Memory.mainram[offset + 3] = (byte)value;
            }
            else if (address >= 0x0b0000 && address + 3 <= 0x0b3fff)
            {
                int offset = (address - 0x0b0000) / 2;
                K053245_scattered_word_w(offset, (ushort)(value >> 16));
                K053245_scattered_word_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x0c0000 && address + 3 <= 0x0c001f)
            {
                int offset = (address - 0x0c0000) / 2;
                K053244_word_noA1_w(offset, (ushort)(value >> 16));
                K053244_word_noA1_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x0e0000 && address + 3 <= 0x0e001f)
            {
                int offset = (address - 0x0e0000) / 2;
                K053251_lsb_w(offset, (ushort)(value >> 16));
                K053251_lsb_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x100000 && address + 3 <= 0x107fff)
            {
                int offset = (address - 0x100000) / 2;
                K052109_word_noA12_w(offset, (ushort)(value >> 16));
                K052109_word_noA12_w(offset + 1, (ushort)value);
            }
        }
        public static sbyte MReadOpByte_ssriders(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address <= 0x0bffff)
            {
                result = (sbyte)Memory.mainrom[address];
            }
            else if (address >= 0x104000 && address <= 0x107fff)
            {
                result = (sbyte)Memory.mainram[address - 0x104000];
            }
            return result;
        }
        public static sbyte MReadByte_ssriders(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address <= 0x0bffff)
            {
                result = (sbyte)Memory.mainrom[address];
            }
            else if (address >= 0x104000 && address <= 0x107fff)
            {
                result = (sbyte)Memory.mainram[address - 0x104000];
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
                    result = (sbyte)(K053245_scattered_word_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)K053245_scattered_word_r(offset);
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
                result = (sbyte)Generic.watchdog_reset16_r();
            }
            else if (address >= 0x1c0500 && address <= 0x1c057f)
            {
                result = (sbyte)mainram2[address - 0x1c0500];
            }
            else if (address >= 0x1c0800 && address <= 0x1c0801)
            {
                if (address % 2 == 0)
                {
                    result = (sbyte)(ssriders_protection_r() >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)ssriders_protection_r();
                }
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
                    result = (sbyte)(punkshot_sound_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)punkshot_sound_r(offset);
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
        public static short MReadOpWord_ssriders(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x0bffff)
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
                result = (short)(Memory.mainram[address - 0x104000] * 0x100 + Memory.mainram[address - 0x104000 + 1]);
            }
            return result;
        }
        public static short MReadWord_ssriders(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x0bffff)
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
                result = (short)(Memory.mainram[address - 0x104000] * 0x100 + Memory.mainram[address - 0x104000 + 1]);
            }
            else if (address >= 0x140000 && address + 1 <= 0x140fff)
            {
                int offset = (address - 0x140000) / 2;
                result = (short)Generic.paletteram16[offset];
            }
            else if (address >= 0x180000 && address + 1 <= 0x183fff)
            {
                int offset = (address - 0x180000) / 2;
                result = (short)K053245_scattered_word_r(offset);
            }
            else if (address >= 0x1c0000 && address + 1 <= 0x1c0001)
            {
                result = (short)sbyte1;
            }
            else if (address >= 0x1c0002 && address + 1 <= 0x1c0003)
            {
                result = (short)sbyte2;
            }
            else if (address >= 0x1c0004 && address + 1 <= 0x1c0005)
            {
                result = (short)sbyte3;
            }
            else if (address >= 0x1c0006 && address + 1 <= 0x1c0007)
            {
                result = (short)sbyte4;
            }
            else if (address >= 0x1c0100 && address + 1 <= 0x1c0101)
            {
                result = (short)sbyte0;
            }
            else if (address >= 0x1c0102 && address + 1 <= 0x1c0103)
            {
                result = (short)ssriders_eeprom_r();
            }
            else if (address >= 0x1c0400 && address + 1 <= 0x1c0401)
            {
                result = (short)Generic.watchdog_reset16_r(); ;
            }
            else if (address >= 0x1c0500 && address + 1 <= 0x1c057f)
            {
                result = (short)(mainram2[address - 0x1c0500] * 0x100 + mainram2[address - 0x1c0500 + 1]);
            }
            else if (address >= 0x1c0800 && address + 1 <= 0x1c0801)
            {
                result = (short)ssriders_protection_r();
            }
            else if (address >= 0x5a0000 && address + 1 <= 0x5a001f)
            {
                int offset = (address - 0x5a0000) / 2;
                result = (short)K053244_word_noA1_r(offset);
            }
            else if (address >= 0x5c0600 && address + 1 <= 0x5c0603)
            {
                int offset = (address - 0x5c0600) / 2;
                result = (short)punkshot_sound_r(offset);
            }
            else if (address >= 0x600000 && address + 1 <= 0x603fff)
            {
                int offset = (address - 0x600000) / 2;
                result = (short)K052109_word_r(offset);
            }
            return result;
        }
        public static int MReadOpLong_ssriders(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x0bffff)
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
                result = (int)(Memory.mainram[address - 0x104000] * 0x1000000 + Memory.mainram[address - 0x104000 + 1] * 0x10000 + Memory.mainram[address - 0x104000 + 2] * 0x100 + Memory.mainram[address - 0x104000 + 3]);
            }
            return result;
        }
        public static int MReadLong_ssriders(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x0bffff)
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
                result = (int)(Memory.mainram[address - 0x104000] * 0x1000000 + Memory.mainram[address - 0x104000 + 1] * 0x10000 + Memory.mainram[address - 0x104000 + 2] * 0x100 + Memory.mainram[address - 0x104000 + 3]);
            }
            else if (address >= 0x140000 && address + 3 <= 0x140fff)
            {
                int offset = (address - 0x140000) / 2;
                result = (int)(Generic.paletteram16[offset] * 0x10000 + Generic.paletteram16[offset + 1]);
            }
            else if (address >= 0x180000 && address + 3 <= 0x183fff)
            {
                int offset = (address - 0x180000) / 2;
                result = (int)(K053245_scattered_word_r(offset) * 0x10000 + K053245_scattered_word_r(offset + 1));
            }
            else if (address >= 0x1c0500 && address + 3 <= 0x1c057f)
            {
                result = (int)(mainram2[address - 0x1c0500] * 0x1000000 + mainram2[address - 0x1c0500 + 1] * 0x10000 + mainram2[address - 0x1c0500 + 2] * 0x100 + mainram2[address - 0x1c0500 + 3]);
            }
            else if (address >= 0x5a0000 && address + 3 <= 0x5a001f)
            {
                int offset = (address - 0x5a0000) / 2;
                result = (int)(K053244_word_noA1_r(offset) * 0x10000 + K053244_word_noA1_r(offset));
            }
            else if (address >= 0x5c0600 && address + 3 <= 0x5c0603)
            {
                int offset = (address - 0x5c0600) / 2;
                result = (int)(punkshot_sound_r(offset) * 0x10000 + punkshot_sound_r(offset + 1));
            }
            else if (address >= 0x600000 && address + 3 <= 0x603fff)
            {
                int offset = (address - 0x600000) / 2;
                result = (int)(K052109_word_r(offset) * 0x10000 + K052109_word_r(offset + 1));
            }
            return result;
        }
        public static void MWriteByte_ssriders(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x104000 && address <= 0x107fff)
            {
                int offset = address - 0x104000;
                Memory.mainram[offset] = (byte)value;
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
            else if (address >= 0x1c0800 && address <= 0x1c0803)
            {
                int offset = (address - 0x1c0800) / 2;
                ssriders_protection_w(offset);
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
        public static void MWriteWord_ssriders(int address, short value)
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
                int offset = (address - 0x1c0400) / 2;
                Generic.watchdog_reset16_w();
            }
            else if (address >= 0x1c0500 && address + 1 <= 0x1c057f)
            {
                int offset = address - 0x1c0500;
                mainram2[offset] = (byte)(value >> 8);
                mainram2[offset + 1] = (byte)value;
            }
            else if (address >= 0x1c0800 && address + 1 <= 0x1c0803)
            {
                int offset = (address - 0x1c0800) / 2;
                ssriders_protection_w(offset);
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
        public static void MWriteLong_ssriders(int address, int value)
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
            else if (address >= 0x1c0800 && address + 3 <= 0x1c0803)
            {
                int offset = (address - 0x1c0800) / 2;
                //ssriders_protection_w(offset);
                ssriders_protection_w(offset + 1);
            }
            else if (address >= 0x5a0000 && address + 3 <= 0x5a001f)
            {
                int offset = (address - 0x5a0000) / 2;
                K053260.k053260_0_lsb_w(offset, (ushort)(value >> 16));
                K053260.k053260_0_lsb_w(offset + 1, (ushort)value);
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
        public static byte ZReadOp_mia(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0x8000 && address <= 0x87ff)
            {
                int offset = address - 0x8000;
                result = Memory.audioram[offset];
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static byte ZReadMemory_mia(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0x8000 && address <= 0x87ff)
            {
                int offset = address - 0x8000;
                result = Memory.audioram[offset];
            }
            else if (address == 0xa000)
            {
                result = (byte)Sound.soundlatch_r();
            }
            else if (address >= 0xb000 && address <= 0xb00d)
            {
                int offset = address - 0xb000;
                result = K007232.k007232_read_port_0_r(offset);
            }
            else if (address == 0xc001)
            {
                result = YM2151.ym2151_status_port_0_r();
            }
            return result;
        }
        public static void ZWriteMemory_mia(ushort address, byte value)
        {
            if (address >= 0x8000 && address <= 0x87ff)
            {
                int offset = address - 0x8000;
                Memory.audioram[offset] = value;
            }
            else if (address >= 0xb000 && address <= 0xb00d)
            {
                int offset = address - 0xb000;
                K007232.k007232_write_port_0_w(offset, value);
            }
            else if (address == 0xc000)
            {
                YM2151.ym2151_register_port_0_w(value);
            }
            else if (address == 0xc001)
            {
                YM2151.ym2151_data_port_0_w(value);
            }
        }
        public static byte ZReadOp_tmnt(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0x8000 && address <= 0x87ff)
            {
                int offset = address - 0x8000;
                result = Memory.audioram[offset];
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static byte ZReadMemory_tmnt(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0x8000 && address <= 0x87ff)
            {
                int offset = address - 0x8000;
                result = Memory.audioram[offset];
            }
            else if (address == 0x9000)
            {
                result = tmnt_sres_r();
            }
            else if (address == 0xa000)
            {
                result = (byte)Sound.soundlatch_r();
            }
            else if (address >= 0xb000 && address <= 0xb00d)
            {
                int offset = address - 0xb000;
                result = K007232.k007232_read_port_0_r(offset);
            }
            else if (address == 0xc001)
            {
                result = YM2151.ym2151_status_port_0_r();
            }
            else if (address == 0xf000)
            {
                result = Upd7759.upd7759_0_busy_r();
            }
            return result;
        }
        public static void ZWriteMemory_tmnt(ushort address, byte value)
        {
            if (address >= 0x8000 && address <= 0x87ff)
            {
                int offset = address - 0x8000;
                Memory.audioram[offset] = value;
            }
            else if (address == 0x9000)
            {
                tmnt_sres_w(value);
            }
            else if (address >= 0xb000 && address <= 0xb00d)
            {
                int offset = address - 0xb000;
                K007232.k007232_write_port_0_w(offset, value);
            }
            else if (address == 0xc000)
            {
                YM2151.ym2151_register_port_0_w(value);
            }
            else if (address == 0xc001)
            {
                YM2151.ym2151_data_port_0_w(value);
            }
            else if (address == 0xd000)
            {
                Upd7759.upd7759_0_port_w(value);
            }
            else if (address == 0xe000)
            {
                Upd7759.upd7759_0_start_w(value);
            }
        }
        public static byte ZReadOp_punkshot(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
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
        public static byte ZReadMemory_punkshot(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0xf000 && address <= 0xf7ff)
            {
                int offset = address - 0xf000;
                result = Memory.audioram[offset];
            }
            else if (address == 0xf801)
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
        public static void ZWriteMemory_punkshot(ushort address, byte value)
        {
            if (address >= 0xf000 && address <= 0xf7ff)
            {
                int offset = address - 0xf000;
                Memory.audioram[offset] = value;
            }
            else if (address == 0xf800)
            {
                YM2151.ym2151_register_port_0_w(value);
            }
            else if (address == 0xf801)
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
        public static byte ZReadOp_lgtnfght(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0x8000 && address <= 0x87ff)
            {
                int offset = address - 0x8000;
                result = Memory.audioram[offset];
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static byte ZReadMemory_lgtnfght(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0x8000 && address <= 0x87ff)
            {
                int offset = address - 0x8000;
                result = Memory.audioram[offset];
            }
            else if (address == 0xa001)
            {
                result = YM2151.ym2151_status_port_0_r();
            }
            else if (address >= 0xc000 && address <= 0xc02f)
            {
                int offset = address - 0xc000;
                result = K053260.k053260_0_r(offset);
            }
            return result;
        }
        public static void ZWriteMemory_lgtnfght(ushort address, byte value)
        {
            if (address >= 0x8000 && address <= 0x87ff)
            {
                int offset = address - 0x8000;
                Memory.audioram[offset] = value;
            }
            else if (address == 0xa000)
            {
                YM2151.ym2151_register_port_0_w(value);
            }
            else if (address == 0xa001)
            {
                YM2151.ym2151_data_port_0_w(value);
            }
            else if (address >= 0xc000 && address <= 0xc02f)
            {
                int offset = address - 0xc000;
                K053260.k053260_0_w(offset, value);
            }
        }
        public static byte ZReadOp_ssriders(ushort address)
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
        public static byte ZReadMemory_ssriders(ushort address)
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
            else if (address == 0xf801)
            {
                result = YM2151.ym2151_status_port_0_r();
            }
            else if (address >= 0xfa00 && address <= 0xfa2f)
            {
                int offset = address - 0xfa00;
                result = K053260.k053260_0_r(offset);
            }
            return result;
        }
        public static void ZWriteMemory_ssriders(ushort address, byte value)
        {
            if (address >= 0xf000 && address <= 0xf7ff)
            {
                int offset = address - 0xf000;
                Memory.audioram[offset] = value;
            }
            else if (address == 0xf800)
            {
                YM2151.ym2151_register_port_0_w(value);
            }
            else if (address == 0xf801)
            {
                YM2151.ym2151_data_port_0_w(value);
            }
            else if (address >= 0xfa00 && address <= 0xfa2f)
            {
                int offset = address - 0xfa00;
                K053260.k053260_0_w(offset, value);
            }
            else if (address == 0xfc00)
            {
                sound_arm_nmi_w();
            }
        }
        public static byte ZReadHardware(ushort address)
        {
            return 0;
        }
        public static void ZWriteHardware(ushort address, byte value)
        {

        }
        public static int ZIRQCallback()
        {
            return Cpuint.cpu_irq_callback(Z80A.zz1[0].cpunum, 0);
        }
    }
}
