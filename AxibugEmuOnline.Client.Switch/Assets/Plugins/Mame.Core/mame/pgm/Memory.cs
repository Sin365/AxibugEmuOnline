namespace MAME.Core
{
    public unsafe partial class PGM
    {
        public static short short0, short1, short2, short3, short4, short5, short6;
        public static short short0_old, short1_old, short2_old, short3_old, short4_old, short5_old, short6_old;
        public static sbyte MReadOpByte(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= 0 && address <= 0x1ffff)
            {
                result = (sbyte)(mainbiosrom[address]);
            }
            else if (address >= 0x100000 && address <= 0x3fffff)
            {
                if (address < 0x100000 + Memory.mainromLength)
                {
                    result = (sbyte)(Memory.mainrom[address - 0x100000]);
                }
                else
                {
                    result = 0;
                }
            }
            /*else if (address >= 0x800000 && address <= 0x81ffff)
            {
                result = (sbyte)Memory.mainram[address - 0x800000];
            }*/
            return result;
        }
        public static sbyte MReadByte(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= 0 && address <= 0x1ffff)
            {
                result = (sbyte)(mainbiosrom[address]);
            }
            else if (address >= 0x100000 && address <= 0x3fffff)
            {
                if (address < 0x100000 + Memory.mainromLength)
                {
                    result = (sbyte)(Memory.mainrom[address - 0x100000]);
                }
                else
                {
                    result = 0;
                }
            }
            else if (address >= 0x800000 && address <= 0x81ffff)
            {
                result = (sbyte)Memory.mainram[address - 0x800000];
            }
            else if (address >= 0x900000 && address <= 0x903fff)
            {
                result = (sbyte)pgm_bg_videoram[address - 0x900000];
            }
            else if (address >= 0x904000 && address <= 0x905fff)
            {
                result = (sbyte)pgm_tx_videoram[address - 0x904000];
            }
            else if (address >= 0x907000 && address <= 0x9077ff)
            {
                result = (sbyte)pgm_rowscrollram[address - 0x907000];
            }
            else if (address >= 0xa00000 && address <= 0xa011ff)
            {
                int offset = (address - 0xa00000) / 2;
                if ((address % 2) == 0)
                {
                    result = (sbyte)(Generic.paletteram16[offset] >> 8);
                }
                else if ((address % 2) == 1)
                {
                    result = (sbyte)(Generic.paletteram16[offset]);
                }
            }
            else if (address >= 0xb00000 && address <= 0xb0ffff)
            {
                result = (sbyte)pgm_videoregs[address - 0xb00000];
            }
            else if (address >= 0xc00002 && address <= 0xc00003)
            {
                result = (sbyte)Sound.latched_value[0];
            }
            else if (address >= 0xc00004 && address <= 0xc00005)
            {
                result = (sbyte)Sound.latched_value[1];
            }
            else if (address >= 0xc00006 && address <= 0xc00007)
            {
                result = (sbyte)PGM.pgm_calendar_r();
            }
            else if (address >= 0xc0000c && address <= 0xc0000d)
            {
                result = (sbyte)Sound.latched_value[2];
            }
            else if (address >= 0xc08000 && address <= 0xc08001)
            {
                result = (sbyte)short0;
            }
            else if (address >= 0xc08002 && address <= 0xc08003)
            {
                result = (sbyte)short1;
            }
            else if (address >= 0xc08004 && address <= 0xc08005)
            {
                result = (sbyte)short2;
            }
            else if (address >= 0xc08006 && address <= 0xc08007)
            {
                result = (sbyte)short3;
            }
            else if (address >= 0xc10000 && address <= 0xc1ffff)
            {
                result = (sbyte)z80_ram_r(address - 0xc10000);
            }
            return result;
        }
        public static short MReadOpWord(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= 0 && address + 1 <= 0x1ffff)
            {
                result = (short)(mainbiosrom[address] * 0x100 + mainbiosrom[address + 1]);
            }
            else if (address >= 0x100000 && address + 1 <= 0x3fffff)
            {
                if (address + 1 < 0x100000 + Memory.mainromLength)
                {
                    result = (short)(Memory.mainrom[address - 0x100000] * 0x100 + Memory.mainrom[address - 0x100000 + 1]);
                }
                else
                {
                    result = 0;
                }
            }
            else if (address >= 0x800000 && address + 1 <= 0x81ffff)
            {
                result = (short)(Memory.mainram[address - 0x800000] * 0x100 + Memory.mainram[address - 0x800000 + 1]);
            }
            return result;
        }
        public static short MReadWord(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= 0 && address + 1 <= 0x1ffff)
            {
                result = (short)(mainbiosrom[address] * 0x100 + mainbiosrom[address + 1]);
            }
            else if (address >= 0x100000 && address + 1 <= 0x3fffff)
            {
                if (address + 1 < 0x100000 + Memory.mainromLength)
                {
                    result = (short)(Memory.mainrom[address - 0x100000] * 0x100 + Memory.mainrom[address - 0x100000 + 1]);
                }
                else
                {
                    result = 0;
                }
            }
            else if (address >= 0x800000 && address + 1 <= 0x81ffff)
            {
                result = (short)(Memory.mainram[address - 0x800000] * 0x100 + Memory.mainram[address - 0x800000 + 1]);
            }
            else if (address >= 0x900000 && address + 1 <= 0x903fff)
            {
                result = (short)(pgm_bg_videoram[address - 0x900000] * 0x100 + pgm_bg_videoram[address - 0x900000 + 1]);
            }
            else if (address >= 0x904000 && address + 1 <= 0x905fff)
            {
                result = (short)(pgm_tx_videoram[address - 0x904000] * 0x100 + pgm_tx_videoram[address - 0x904000 + 1]);
            }
            else if (address >= 0x907000 && address + 1 <= 0x9077ff)
            {
                result = (short)(pgm_rowscrollram[address - 0x907000] * 0x100 + pgm_rowscrollram[address - 0x907000 + 1]);
            }
            else if (address >= 0xa00000 && address + 1 <= 0xa011ff)
            {
                int offset = (address - 0xa0000) / 2;
                result = (short)Generic.paletteram16[offset];
            }
            else if (address >= 0xb00000 && address + 1 <= 0xb0ffff)
            {
                result = (short)(pgm_videoregs[address - 0xb00000] * 0x100 + pgm_videoregs[address - 0xb00000 + 1]);
            }
            else if (address >= 0xc00002 && address + 1 <= 0xc00003)
            {
                result = (short)Sound.latched_value[0];
            }
            else if (address >= 0xc00004 && address + 1 <= 0xc00005)
            {
                result = (short)Sound.latched_value[1];
            }
            else if (address >= 0xc00006 && address + 1 <= 0xc00007)
            {
                result = (short)PGM.pgm_calendar_r();
            }
            else if (address >= 0xc0000c && address + 1 <= 0xc0000d)
            {
                result = (short)Sound.latched_value[2];
            }
            else if (address >= 0xc08000 && address + 1 <= 0xc08001)
            {
                result = short0;
            }
            else if (address >= 0xc08002 && address + 1 <= 0xc08003)
            {
                result = short1;
            }
            else if (address >= 0xc08004 && address + 1 <= 0xc08005)
            {
                result = short2;
            }
            else if (address >= 0xc08006 && address + 1 <= 0xc08007)
            {
                result = short3;
            }
            else if (address >= 0xc10000 && address + 1 <= 0xc1ffff)
            {
                result = (short)(z80_ram_r(address - 0xc10000) * 0x100 + z80_ram_r(address - 0xc10000 + 1));
            }
            return result;
        }
        public static int MReadOpLong(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0 && address + 3 <= 0x1ffff)
            {
                result = mainbiosrom[address] * 0x1000000 + mainbiosrom[address + 1] * 0x10000 + mainbiosrom[address + 2] * 0x100 + mainbiosrom[address + 3];
            }
            else if (address >= 0x100000 && address + 3 <= 0x3fffff)
            {
                if (address + 3 < 0x100000 + Memory.mainromLength)
                {
                    result = Memory.mainrom[address - 0x100000] * 0x1000000 + Memory.mainrom[address - 0x100000 + 1] * 0x10000 + Memory.mainrom[address - 0x100000 + 2] * 0x100 + Memory.mainrom[address - 0x100000 + 3];
                }
                else
                {
                    result = 0;
                }
            }
            else if (address >= 0x800000 && address + 3 <= 0x81ffff)
            {
                result = Memory.mainram[address - 0x800000] * 0x1000000 + Memory.mainram[address - 0x800000 + 1] * 0x10000 + Memory.mainram[address - 0x800000 + 2] * 0x100 + Memory.mainram[address - 0x800000 + 3];
            }
            return result;
        }
        public static int MReadLong(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0 && address + 1 <= 0x1ffff)
            {
                result = mainbiosrom[address] * 0x1000000 + mainbiosrom[address + 1] * 0x10000 + mainbiosrom[address + 2] * 0x100 + mainbiosrom[address + 3];
            }
            else if (address >= 0x100000 && address + 3 <= 0x3fffff)
            {
                if (address + 3 < 0x100000 + Memory.mainromLength)
                {
                    result = Memory.mainrom[address - 0x100000] * 0x1000000 + Memory.mainrom[address - 0x100000 + 1] * 0x10000 + Memory.mainrom[address - 0x100000 + 2] * 0x100 + Memory.mainrom[address - 0x100000 + 3];
                }
                else
                {
                    result = 0;
                }
            }
            else if (address >= 0x800000 && address + 3 <= 0x81ffff)
            {
                result = Memory.mainram[address - 0x800000] * 0x1000000 + Memory.mainram[address - 0x800000 + 1] * 0x10000 + Memory.mainram[address - 0x800000 + 2] * 0x100 + Memory.mainram[address - 0x800000 + 3];
            }
            else if (address >= 0x900000 && address + 3 <= 0x903fff)
            {
                result = pgm_bg_videoram[address - 0x900000] * 0x1000000 + pgm_bg_videoram[address - 0x900000 + 1] * 0x10000 + pgm_bg_videoram[address - 0x900000 + 2] * 0x100 + pgm_bg_videoram[address - 0x900000 + 3];
            }
            else if (address >= 0x904000 && address + 3 <= 0x905fff)
            {
                result = pgm_tx_videoram[address - 0x904000] * 0x1000000 + pgm_tx_videoram[address - 0x904000 + 1] * 0x10000 + pgm_tx_videoram[address - 0x904000 + 2] * 0x100 + pgm_tx_videoram[address - 0x904000 + 3];
            }
            else if (address >= 0x907000 && address + 3 <= 0x9077ff)
            {
                result = pgm_rowscrollram[address - 0x907000] * 0x1000000 + pgm_rowscrollram[address - 0x907000 + 1] * 0x10000 + pgm_rowscrollram[address - 0x907000 + 2] * 0x100 + pgm_rowscrollram[address - 0x907000 + 3];
            }
            else if (address >= 0xa00000 && address + 3 <= 0xa011ff)
            {
                int offset = (address - 0xa00000) / 2;
                result = Generic.paletteram16[offset] * 0x10000 + Generic.paletteram16[offset + 1];
            }
            else if (address >= 0xb00000 && address + 3 <= 0xb0ffff)
            {
                result = pgm_videoregs[address - 0xb00000] * 0x1000000 + pgm_videoregs[address - 0xb00000 + 1] * 0x10000 + pgm_videoregs[address - 0xb00000 + 2] * 0x100 + pgm_videoregs[address - 0xb00000 + 3];
            }
            /*else if (address >= 0xc00002 && address + 3 <= 0xc00003)
            {
                result = (short)Sound.ulatched_value[0];
            }
            else if (address >= 0xc00004 && address + 3 <= 0xc00005)
            {
                result = (short)Sound.ulatched_value[1];
            }
            else if (address >= 0xc00006 && address + 3 <= 0xc00007)
            {
                result = (short)Pgm.pgm_calendar_r();
            }
            else if (address >= 0xc0000c && address + 3 <= 0xc0000d)
            {
                result = (short)Sound.ulatched_value[2];
            }*/
            else if (address >= 0xc08000 && address + 3 <= 0xc08003)
            {
                result = (int)(((ushort)short0 << 16) | (ushort)short1);
            }
            else if (address >= 0xc08002 && address + 3 <= 0xc08005)
            {
                result = short1;
            }
            else if (address >= 0xc08004 && address + 3 <= 0xc08007)
            {
                result = short2;
            }
            else if (address >= 0xc08006 && address + 3 <= 0xc08009)
            {
                result = short3;
            }
            else if (address >= 0xc10000 && address + 3 <= 0xc1ffff)
            {
                result = z80_ram_r(address - 0xc10000) * 0x1000000 + z80_ram_r(address - 0xc10000 + 1) * 0x10000 + z80_ram_r(address - 0xc10000 + 2) * 0x100 + z80_ram_r(address - 0xc10000 + 3);
            }
            return result;
        }
        public static void MWriteByte(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x700006 && address <= 0x700007)
            {
                //NOP;
            }
            else if (address >= 0x800000 && address <= 0x81ffff)
            {
                int offset = address - 0x800000;
                Memory.mainram[offset] = (byte)value;
            }
            else if (address >= 0x900000 && address <= 0x903fff)
            {
                int offset = address - 0x900000;
                pgm_bg_videoram_w(offset, (byte)value);
            }
            else if (address >= 0x904000 && address <= 0x905fff)
            {
                int offset = address - 0x904000;
                pgm_tx_videoram_w(offset, (byte)value);
            }
            else if (address >= 0x907000 && address <= 0x9077ff)
            {
                int offset = address - 0x907000;
                pgm_rowscrollram[offset] = (byte)value;
            }
            else if (address >= 0xa00000 && address <= 0xa011ff)
            {
                int offset = (address - 0xa00000) / 2;
                if ((address % 2) == 0)
                {
                    Generic.paletteram16[offset] = (ushort)(((byte)value << 8) | (Generic.paletteram16[offset] & 0xff));
                }
                else if ((address % 2) == 1)
                {
                    Generic.paletteram16[offset] = (ushort)((Generic.paletteram16[offset] & 0xff00) | (byte)value);
                }
                Generic.paletteram16_xRRRRRGGGGGBBBBB_word_w(offset);
            }
            else if (address >= 0xb00000 && address <= 0xb0ffff)
            {
                int offset = address - 0xb00000;
                pgm_videoregs[offset] = (byte)value;
            }
            else if (address >= 0xc00002 && address <= 0xc00003)
            {
                if (address == 0xc00003)
                {
                    m68k_l1_w((byte)value);
                }
            }
            else if (address >= 0xc00004 && address <= 0xc00005)
            {
                Sound.soundlatch2_w((ushort)value);
            }
            else if (address >= 0xc00006 && address <= 0xc00007)
            {
                if (address == 0xc00006)
                {
                    int i1 = 1;
                }
                else if (address == 0xc00007)
                {
                    pgm_calendar_w((ushort)value);
                }
            }
            else if (address >= 0xc00008 && address <= 0xc00009)
            {
                z80_reset_w((ushort)value);
            }
            else if (address >= 0xc0000a && address <= 0xc0000b)
            {
                z80_ctrl_w();
            }
            else if (address >= 0xc0000c && address <= 0xc0000d)
            {
                Sound.soundlatch3_w((ushort)value);
            }
            else if (address >= 0xc10000 && address <= 0xc1ffff)
            {
                int offset = address - 0xc10000;
                z80_ram_w(offset, (byte)value);
            }
            else
            {
                int i1 = 1;
            }
        }
        public static void MWriteWord(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x700006 && address + 1 <= 0x700007)
            {
                //NOP;
            }
            else if (address >= 0x800000 && address + 1 <= 0x81ffff)
            {
                int offset = address - 0x800000;
                Memory.mainram[offset] = (byte)(value >> 8);
                Memory.mainram[offset + 1] = (byte)value;
            }
            else if (address >= 0x900000 && address + 1 <= 0x903fff)
            {
                int offset = (address - 0x900000) / 2;
                pgm_bg_videoram_w(offset, (ushort)value);
            }
            else if (address >= 0x904000 && address + 1 <= 0x905fff)
            {
                int offset = (address - 0x904000) / 2;
                pgm_tx_videoram_w(offset, (ushort)value);
            }
            else if (address >= 0x907000 && address + 1 <= 0x9077ff)
            {
                int offset = (address - 0x907000) / 2;
                pgm_rowscrollram[offset * 2] = (byte)(value >> 8);
                pgm_rowscrollram[offset * 2 + 1] = (byte)value;
            }
            else if (address >= 0xa00000 && address + 1 <= 0xa011ff)
            {
                int offset = (address - 0xa00000) / 2;
                Generic.paletteram16[offset] = (ushort)value;
                Generic.paletteram16_xRRRRRGGGGGBBBBB_word_w(offset);
            }
            else if (address >= 0xb00000 && address + 1 <= 0xb0ffff)
            {
                int offset = (address - 0xb00000) / 2;
                pgm_videoregs[offset * 2] = (byte)(value >> 8);
                pgm_videoregs[offset * 2 + 1] = (byte)value;
            }
            else if (address >= 0xc00002 && address + 1 <= 0xc00003)
            {
                m68k_l1_w((ushort)value);
            }
            else if (address >= 0xc00004 && address + 1 <= 0xc00005)
            {
                Sound.soundlatch2_w((ushort)value);
            }
            else if (address >= 0xc00006 && address + 1 <= 0xc00007)
            {
                pgm_calendar_w((ushort)value);
            }
            else if (address >= 0xc00008 && address + 1 <= 0xc00009)
            {
                z80_reset_w((ushort)value);
            }
            else if (address >= 0xc0000a && address + 1 <= 0xc0000b)
            {
                z80_ctrl_w();
            }
            else if (address >= 0xc0000c && address + 1 <= 0xc0000d)
            {
                Sound.soundlatch3_w((ushort)value);
            }
            else if (address >= 0xc10000 && address + 1 <= 0xc1ffff)
            {
                int offset = address - 0xc10000;
                z80_ram_w(offset, (byte)(value >> 8));
                z80_ram_w(offset + 1, (byte)value);
            }
            else
            {
                int i1 = 1;
            }
        }
        public static void MWriteLong(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x700006 && address + 3 <= 0x700007)
            {
                //NOP;
            }
            else if (address >= 0x800000 && address + 3 <= 0x81ffff)
            {
                int offset = address - 0x800000;
                Memory.mainram[offset] = (byte)(value >> 24);
                Memory.mainram[offset + 1] = (byte)(value >> 16);
                Memory.mainram[offset + 2] = (byte)(value >> 8);
                Memory.mainram[offset + 3] = (byte)value;
            }
            else if (address >= 0x900000 && address + 3 <= 0x903fff)
            {
                int offset = (address - 0x900000) / 2;
                pgm_bg_videoram_w(offset, (ushort)(value >> 16));
                pgm_bg_videoram_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x904000 && address + 3 <= 0x905fff)
            {
                int offset = (address - 0x904000) / 2;
                pgm_tx_videoram_w(offset, (ushort)(value >> 16));
                pgm_tx_videoram_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x907000 && address + 3 <= 0x9077ff)
            {
                int offset = (address - 0x907000) / 2;
                pgm_rowscrollram[offset * 2] = (byte)(value >> 24);
                pgm_rowscrollram[offset * 2 + 1] = (byte)(value >> 16);
                pgm_rowscrollram[offset * 2 + 2] = (byte)(value >> 8);
                pgm_rowscrollram[offset * 2 + 3] = (byte)value;
            }
            else if (address >= 0xa00000 && address + 3 <= 0xa011ff)
            {
                int offset = (address - 0xa00000) / 2;
                Generic.paletteram16[offset] = (ushort)(value >> 16);
                Generic.paletteram16[offset + 1] = (ushort)value;
                Generic.paletteram16_xRRRRRGGGGGBBBBB_word_w(offset);
                Generic.paletteram16_xRRRRRGGGGGBBBBB_word_w(offset + 1);
            }
            else if (address >= 0xb00000 && address + 3 <= 0xb0ffff)
            {
                int offset = (address - 0xb00000) / 2;
                pgm_videoregs[offset * 2] = (byte)(value >> 24);
                pgm_videoregs[offset * 2 + 1] = (byte)(value >> 16);
                pgm_videoregs[offset * 2 + 2] = (byte)(value >> 8);
                pgm_videoregs[offset * 2 + 3] = (byte)value;
            }
            /*else if (address >= 0xc00002 && address + 3 <= 0xc00003)
            {
                m68k_l1_w((ushort)value);
            }
            else if (address >= 0xc00004 && address + 3 <= 0xc00005)
            {
                Sound.soundlatch2_w((ushort)value);
            }
            else if (address >= 0xc00006 && address + 3 <= 0xc00007)
            {
                pgm_calendar_w((ushort)value);
            }
            else if (address >= 0xc00008 && address + 3 <= 0xc00009)
            {
                z80_reset_w((ushort)value);
            }
            else if (address >= 0xc0000a && address + 3 <= 0xc0000b)
            {
                z80_ctrl_w();
            }
            else if (address >= 0xc0000c && address + 3 <= 0xc0000d)
            {
                Sound.soundlatch3_w((ushort)value);
            }*/
            else if (address >= 0xc10000 && address + 3 <= 0xc1ffff)
            {
                int offset = address - 0xc10000;
                z80_ram_w(offset, (byte)(value >> 24));
                z80_ram_w(offset + 1, (byte)(value >> 16));
                z80_ram_w(offset + 2, (byte)(value >> 8));
                z80_ram_w(offset + 3, (byte)value);
            }
            else
            {
                int i1 = 1;
            }
        }
        public static byte ZReadMemory(ushort address)
        {
            byte result = Memory.audioram[address];
            return result;
        }
        public static void ZWriteMemory(ushort address, byte value)
        {
            Memory.audioram[address] = value;
        }
        public static byte ZReadHardware(ushort address)
        {
            byte result = 0;
            if (address >= 0x8000 && address <= 0x8003)
            {
                int offset = address - 0x8000;
                result = ICS2115.ics2115_r(offset);
            }
            else if (address >= 0x8100 && address <= 0x81ff)
            {
                result = (byte)Sound.soundlatch3_r();
            }
            else if (address >= 0x8200 && address <= 0x82ff)
            {
                result = (byte)Sound.soundlatch_r();
            }
            else if (address >= 0x8400 && address <= 0x84ff)
            {
                result = (byte)Sound.soundlatch2_r();
            }
            return result;
        }
        public static void ZWriteHardware(ushort address, byte value)
        {
            if (address >= 0x8000 && address <= 0x8003)
            {
                int offset = address - 0x8000;
                ICS2115.ics2115_w(offset, value);
            }
            else if (address >= 0x8100 && address <= 0x81ff)
            {
                z80_l3_w(value);
            }
            else if (address >= 0x8200 && address <= 0x82ff)
            {
                Sound.soundlatch_w((ushort)value);
            }
            else if (address >= 0x8400 && address <= 0x84ff)
            {
                Sound.soundlatch2_w((ushort)value);
            }
        }
        public static int ZIRQCallback()
        {
            return 0;
        }
    }
}
