using cpu.z80;

namespace MAME.Core
{
    public unsafe partial class CPS
    {
        public static short short0, short1, short2;
        public static sbyte sbyte0, sbyte3;
        public static short short0_old, short1_old, short2_old;
        public static sbyte sbyte0_old, sbyte3_old;
        public static sbyte MCReadOpByte(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address <= 0x3fffff)
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
        public static sbyte MCReadByte(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address <= 0x3fffff)
            {
                if (address < Memory.mainromLength)
                {
                    result = (sbyte)Memory.mainrom[address];
                }
                else
                {
                    result = 0;
                }
            }
            else if (address >= 0x800000 && address <= 0x800007)
            {
                if (address == 0x800000)
                {
                    result = (sbyte)(short1 >> 8);
                }
                else if (address == 0x800001)
                {
                    result = (sbyte)(short1);
                }
                else
                {
                    result = -1;
                }
            }
            else if (address >= 0x800018 && address <= 0x80001f)
            {
                int offset = (address - 0x800018) / 2;
                result = (sbyte)cps1_dsw_r(offset);
            }
            else if (address >= 0x800020 && address <= 0x800021)
            {
                result = 0;
            }
            else if (address >= 0x800140 && address <= 0x80017f)
            {
                int offset = (address - 0x800140) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(cps1_cps_b_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)cps1_cps_b_r(offset);
                }
            }
            else if (address >= 0x900000 && address <= 0x92ffff)
            {
                result = (sbyte)gfxram[(address & 0x3ffff)];
            }
            else if (address >= 0xff0000 && address <= 0xffffff)
            {
                result = (sbyte)Memory.mainram[address & 0xffff];
            }
            return result;
        }
        public static short MCReadOpWord(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x3fffff)
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
            else if (address >= 0x900000 && address + 1 <= 0x92ffff)
            {
                result = (short)(gfxram[(address & 0x3ffff)] * 0x100 + gfxram[(address & 0x3ffff) + 1]);
            }
            else if (address >= 0xff0000 && address + 1 <= 0xffffff)
            {
                result = (short)(Memory.mainram[(address & 0xffff)] * 0x100 + Memory.mainram[(address & 0xffff) + 1]);
            }
            return result;
        }
        public static short MCReadWord(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x3fffff)
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
            else if (address >= 0x800000 && address + 1 <= 0x800007)
            {
                result = short1;// input_port_4_word_r
            }
            else if (address >= 0x800018 && address + 1 <= 0x80001f)
            {
                int offset = (address - 0x800018) / 2;
                result = (short)(((byte)(cps1_dsw_r(offset)) << 8) | (byte)cps1_dsw_r(offset));
            }
            else if (address >= 0x800020 && address + 1 <= 0x800021)
            {
                result = 0;
            }
            else if (address >= 0x800140 && address + 1 <= 0x80017f)
            {
                int offset = (address - 0x800140) / 2;
                result = (short)cps1_cps_b_r(offset);
            }
            else if (address >= 0x900000 && address + 1 <= 0x92ffff)
            {
                result = (short)(gfxram[(address & 0x3ffff)] * 0x100 + gfxram[(address & 0x3ffff) + 1]);
            }
            else if (address >= 0xff0000 && address + 1 <= 0xffffff)
            {
                result = (short)(Memory.mainram[(address & 0xffff)] * 0x100 + Memory.mainram[(address & 0xffff) + 1]);
            }
            return result;
        }
        public static int MCReadOpLong(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x3fffff)
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
            else if (address >= 0x900000 && address + 3 <= 0x92ffff)
            {
                result = (int)(gfxram[(address & 0x3ffff)] * 0x1000000 + gfxram[(address & 0x3ffff) + 1] * 0x10000 + gfxram[(address & 0x3ffff) + 2] * 0x100 + gfxram[(address & 0x3ffff) + 3]);
            }
            else if (address >= 0xff0000 && address + 3 <= 0xffffff)
            {
                result = (int)(Memory.mainram[(address & 0xffff)] * 0x1000000 + Memory.mainram[(address & 0xffff) + 1] * 0x10000 + Memory.mainram[(address & 0xffff) + 2] * 0x100 + Memory.mainram[(address & 0xffff) + 3]);
            }
            return result;
        }
        public static int MCReadLong(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x3fffff)
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
            else if (address >= 0x800000 && address + 3 <= 0x800007)
            {
                result = 0;
            }
            else if (address >= 0x800018 && address + 3 <= 0x80001f)
            {
                result = 0;
            }
            else if (address >= 0x800020 && address + 3 <= 0x800021)
            {
                result = 0;
            }
            else if (address >= 0x800140 && address + 3 <= 0x80017f)
            {
                int offset = (address - 0x800140) / 2;
                result = cps1_cps_b_r(offset) * 0x10000 + cps1_cps_b_r(offset + 1);
            }
            else if (address >= 0x900000 && address + 3 <= 0x92ffff)
            {
                result = (int)(gfxram[(address & 0x3ffff)] * 0x1000000 + gfxram[(address & 0x3ffff) + 1] * 0x10000 + gfxram[(address & 0x3ffff) + 2] * 0x100 + gfxram[(address & 0x3ffff) + 3]);
            }
            else if (address >= 0xff0000 && address + 3 <= 0xffffff)
            {
                result = (int)(Memory.mainram[(address & 0xffff)] * 0x1000000 + Memory.mainram[(address & 0xffff) + 1] * 0x10000 + Memory.mainram[(address & 0xffff) + 2] * 0x100 + Memory.mainram[(address & 0xffff) + 3]);
            }
            return result;
        }
        public static void MCWriteByte(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x800030 && address <= 0x800037)
            {
                if (address % 2 == 0)
                {
                    cps1_coinctrl_w((ushort)(value * 0x100));
                }
                else
                {
                    return;
                }
            }
            else if (address >= 0x800100 && address <= 0x80013f)
            {
                return;
            }
            else if (address >= 0x800140 && address <= 0x80017f)
            {
                return;
            }
            else if (address >= 0x800180 && address <= 0x800187)
            {
                Sound.soundlatch_w((ushort)value);
            }
            else if (address >= 0x800188 && address <= 0x80018f)
            {
                Sound.soundlatch2_w((ushort)value);
            }
            else if (address >= 0x900000 && address <= 0x92ffff)
            {
                gfxram[(address & 0x3ffff)] = (byte)(value);
                cps1_gfxram_w((address & 0x3ffff) / 2);
            }
            else if (address >= 0xff0000 && address <= 0xffffff)
            {
                Memory.mainram[(address & 0xffff)] = (byte)(value);
            }
            else
            {
                int i1 = 1;
            }
        }
        public static void MCWriteWord(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x800030 && address + 1 <= 0x800037)
            {
                return;
            }
            else if (address >= 0x800100 && address + 1 <= 0x80013f)
            {
                cps1_cps_a_w((address & 0x3f) / 2, (ushort)value);
            }
            else if (address >= 0x800140 && address + 1 <= 0x80017f)
            {
                cps1_cps_b_w((address & 0x3f) / 2, (ushort)value);
            }
            else if (address >= 0x800180 && address + 1 <= 0x800187)
            {
                Sound.soundlatch_w((ushort)value);
            }
            else if (address >= 0x800188 && address + 1 <= 0x80018f)
            {
                Sound.soundlatch2_w((ushort)value);
            }
            else if (address >= 0x900000 && address + 1 <= 0x92ffff)
            {
                gfxram[(address & 0x3ffff)] = (byte)(value >> 8);
                gfxram[(address & 0x3ffff) + 1] = (byte)value;
                cps1_gfxram_w((address & 0x3ffff) / 2);
            }
            else if (address >= 0xff0000 && address + 1 <= 0xffffff)
            {
                Memory.mainram[(address & 0xffff)] = (byte)(value >> 8);
                Memory.mainram[(address & 0xffff) + 1] = (byte)(value);
            }
            else
            {
                int i1 = 1;
            }
        }
        public static void MCWriteLong(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x800030 && address + 3 <= 0x800037)
            {
                return;
            }
            else if (address >= 0x800100 && address + 3 <= 0x80013f)
            {
                cps1_cps_a_w((address & 0x3f) / 2, (ushort)(value >> 16));
                cps1_cps_a_w(((address + 2) & 0x3f) / 2, (ushort)value);
            }
            else if (address >= 0x800140 && address + 3 <= 0x80017f)
            {
                cps1_cps_b_w((address & 0x3f) / 2, (ushort)(value >> 16));
                cps1_cps_b_w(((address + 2) & 0x3f) / 2, (ushort)value);
            }
            else if (address >= 0x800180 && address + 3 <= 0x800187)
            {
                return;
            }
            else if (address >= 0x800188 && address + 3 <= 0x80018f)
            {
                return;
            }
            else if (address >= 0x900000 && address + 3 <= 0x92ffff)
            {
                gfxram[(address & 0x3ffff)] = (byte)(value >> 24);
                gfxram[(address & 0x3ffff) + 1] = (byte)(value >> 16);
                gfxram[(address & 0x3ffff) + 2] = (byte)(value >> 8);
                gfxram[(address & 0x3ffff) + 3] = (byte)(value);
                cps1_gfxram_w((address & 0x3ffff) / 2);
                cps1_gfxram_w(((address + 2) & 0x3ffff) / 2);
            }
            else if (address >= 0xff0000 && address + 3 <= 0xffffff)
            {
                Memory.mainram[(address & 0xffff)] = (byte)(value >> 24);
                Memory.mainram[(address & 0xffff) + 1] = (byte)(value >> 16);
                Memory.mainram[(address & 0xffff) + 2] = (byte)(value >> 8);
                Memory.mainram[(address & 0xffff) + 3] = (byte)(value);
            }
            else
            {
                int i1 = 1;
            }
        }
        public unsafe static byte ZCReadOp(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = Memory.audiorom[address & 0x7fff];
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public unsafe static byte ZCReadMemory(ushort address)
        {
            byte result = 0;
            if (address < 0x8000)
            {
                result = Memory.audiorom[address & 0x7fff];
            }
            else if (address >= 0x8000 && address <= 0xbfff)
            {
                result = Memory.audiorom[basebanksnd + (address & 0x3fff)];
            }
            else if (address >= 0xd000 && address <= 0xd7ff)
            {
                result = Memory.audioram[address & 0x7ff];
            }
            else if (address == 0xf001)
            {
                result = YM2151.ym2151_status_port_0_r();
            }
            else if (address == 0xf002)
            {
                result = OKI6295.okim6295_status_0_r();
            }
            else if (address == 0xf008)
            {
                result = (byte)Sound.soundlatch_r();
            }
            else if (address == 0xf00a)
            {
                result = (byte)Sound.soundlatch2_r();
            }
            else
            {
                result = 0;
            }
            return result;
        }

        public static void ZCWriteMemory(ushort address, byte value)
        {
            if (address >= 0xd000 && address <= 0xd7ff)
            {
                Memory.audioram[address & 0x7ff] = value;
            }
            else if (address == 0xf000)
            {
                YM2151.ym2151_register_port_0_w(value);
            }
            else if (address == 0xf001)
            {
                YM2151.ym2151_data_port_0_w(value);
            }
            else if (address == 0xf002)
            {
                OKI6295.okim6295_data_0_w(value);
            }
            else if (address == 0xf004)
            {
                cps1_snd_bankswitch_w(value);
            }
            else if (address == 0xf006)
            {
                cps1_oki_pin7_w(value);
            }
            else
            {

            }
        }
        public static sbyte MQReadOpByte(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address <= 0x3fffff)
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
            else if (address >= 0x900000 && address <= 0x92ffff)
            {
                result = (sbyte)gfxram[(address & 0x3ffff)];
            }
            return result;
        }
        public static sbyte MQReadByte(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address <= 0x3fffff)
            {
                if (address < Memory.mainromLength)
                {
                    result = (sbyte)Memory.mainrom[address];
                }
                else
                {
                    result = 0;
                }
            }
            else if (address >= 0x800000 && address <= 0x800007)
            {
                if (address == 0x800000)//IN1
                {
                    result = (sbyte)(short1 >> 8);
                }
                else if (address == 0x800001)
                {
                    result = (sbyte)(short1);
                }
                else
                {
                    result = -1;
                }
            }
            else if (address >= 0x800018 && address <= 0x80001f)
            {
                int offset = (address - 0x800018) / 2;
                result = (sbyte)cps1_dsw_r(offset);
            }
            else if (address >= 0x800140 && address <= 0x80017f)
            {
                int offset = (address - 0x800140) / 2;
                result = (sbyte)cps1_cps_b_r(offset);
            }
            else if (address >= 0x900000 && address <= 0x92ffff)
            {
                result = (sbyte)gfxram[(address & 0x3ffff)];
            }
            else if (address >= 0xf00000 && address <= 0xf0ffff)
            {
                int offset = (address - 0xf00000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(qsound_rom_r(offset) >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)qsound_rom_r(offset);
                }
            }
            else if (address >= 0xf18000 && address <= 0xf19fff)
            {
                int offset = (address - 0xf18000) / 2;
                result = (sbyte)qsound_sharedram1_r(offset);
            }
            else if (address >= 0xf1c000 && address <= 0xf1c001)
            {
                result = (sbyte)short2;
            }
            else if (address >= 0xf1c002 && address <= 0xf1c003)
            {
                result = sbyte3;
            }
            else if (address >= 0xf1c006 && address <= 0xf1c007)
            {
                result = (sbyte)cps1_eeprom_port_r();
            }
            else if (address >= 0xf1e000 && address <= 0xf1ffff)
            {
                int offset = (address - 0xf1e000) / 2;
                result = (sbyte)qsound_sharedram2_r(offset);
            }
            else if (address >= 0xff0000 && address <= 0xffffff)
            {
                result = (sbyte)Memory.mainram[address & 0xffff];
            }
            return result;
        }
        public static short MQReadOpWord(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x3fffff)
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
            else if (address >= 0x900000 && address + 1 <= 0x92ffff)
            {
                result = (short)(gfxram[(address & 0x3ffff)] * 0x100 + gfxram[(address & 0x3ffff) + 1]);
            }
            else if (address >= 0xff0000 && address + 1 <= 0xffffff)
            {
                result = (short)(Memory.mainram[(address & 0xffff)] * 0x100 + Memory.mainram[(address & 0xffff) + 1]);
            }
            return result;
        }
        public static short MQReadWord(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x3fffff)
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
            else if (address >= 0x800000 && address + 1 <= 0x800007)
            {
                result = short1;//input_port_4_word_r
            }
            else if (address >= 0x800018 && address + 1 <= 0x80001f)
            {
                int offset = (address - 0x800018) / 2;
                result = cps1_dsw_r(offset);
            }
            else if (address >= 0x800140 && address + 1 <= 0x80017f)
            {
                int offset = (address - 0x800140) / 2;
                result = (short)cps1_cps_b_r(offset);
            }
            else if (address >= 0x900000 && address + 1 <= 0x92ffff)
            {
                result = (short)(gfxram[(address & 0x3ffff)] * 0x100 + gfxram[(address & 0x3ffff) + 1]);
            }
            else if (address >= 0xf00000 && address + 1 <= 0xf0ffff)
            {
                int offset = (address - 0xf00000) / 2;
                result = qsound_rom_r(offset);
            }
            else if (address >= 0xf18000 && address + 1 <= 0xf19fff)
            {
                int offset = (address - 0xf18000) / 2;
                result = qsound_sharedram1_r(offset);
            }
            else if (address >= 0xf1c000 && address + 1 <= 0xf1c001)
            {
                result = (short)((int)short2 & 0xff);
            }
            else if (address >= 0xf1c002 && address + 1 <= 0xf1c003)
            {
                result = (short)((int)sbyte3 & 0xff);
            }
            else if (address >= 0xf1c006 && address + 1 <= 0xf1c007)
            {
                result = (short)cps1_eeprom_port_r();
            }
            else if (address >= 0xf1e000 && address + 1 <= 0xf1ffff)
            {
                int offset = (address - 0xf1e000) / 2;
                result = qsound_sharedram2_r(offset);
            }
            else if (address >= 0xff0000 && address + 1 <= 0xffffff)
            {
                result = (short)(Memory.mainram[(address & 0xffff)] * 0x100 + Memory.mainram[(address & 0xffff) + 1]);
            }
            return result;
        }
        public static int MQReadOpLong(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x3fffff)
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
            else if (address >= 0x900000 && address + 3 <= 0x92ffff)
            {
                result = (int)(gfxram[(address & 0x3ffff)] * 0x1000000 + gfxram[(address & 0x3ffff) + 1] * 0x10000 + gfxram[(address & 0x3ffff) + 2] * 0x100 + gfxram[(address & 0x3ffff) + 3]);
            }
            else if (address >= 0xff0000 && address + 3 <= 0xffffff)
            {
                result = (int)(Memory.mainram[(address & 0xffff)] * 0x1000000 + Memory.mainram[(address & 0xffff) + 1] * 0x10000 + Memory.mainram[(address & 0xffff) + 2] * 0x100 + Memory.mainram[(address & 0xffff) + 3]);
            }
            return result;
        }
        public static int MQReadLong(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x3fffff)
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
            else if (address >= 0x800000 && address + 3 <= 0x800007)
            {
                result = -1;//short1
            }
            else if (address >= 0x800018 && address + 3 <= 0x80001f)
            {
                result = 0;//cps1_dsw_r
            }
            else if (address >= 0x800140 && address + 3 <= 0x80017f)
            {
                result = 0;//cps1_cps_b_r
            }
            else if (address >= 0x900000 && address + 3 <= 0x92ffff)
            {
                result = (int)(gfxram[(address & 0x3ffff)] * 0x1000000 + gfxram[(address & 0x3ffff) + 1] * 0x10000 + gfxram[(address & 0x3ffff) + 2] * 0x100 + gfxram[(address & 0x3ffff) + 3]);
            }
            else if (address >= 0xf00000 && address + 3 <= 0xf0ffff)
            {
                result = 0;//qsound_rom_r
            }
            else if (address >= 0xf18000 && address + 3 <= 0xf19fff)
            {
                result = 0;//qsound_sharedram1_r
            }
            else if (address >= 0xf1c000 && address + 3 <= 0xf1c001)
            {
                result = (int)short2 & 0xff;
            }
            else if (address >= 0xf1c002 && address + 3 <= 0xf1c003)
            {
                result = (int)sbyte3 & 0xff;
            }
            else if (address >= 0xf1c006 && address + 3 <= 0xf1c007)
            {
                result = 0;//cps1_eeprom_port_r();
            }
            else if (address >= 0xf1e000 && address + 3 <= 0xf1ffff)
            {
                result = 0;//qsound_sharedram2_r
            }
            else if (address >= 0xff0000 && address + 3 <= 0xffffff)
            {
                result = (int)(Memory.mainram[(address & 0xffff)] * 0x1000000 + Memory.mainram[(address & 0xffff) + 1] * 0x10000 + Memory.mainram[(address & 0xffff) + 2] * 0x100 + Memory.mainram[(address & 0xffff) + 3]);
            }
            return result;
        }
        public static void MQWriteByte(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x800030 && address <= 0x800037)
            {
                if (address % 2 == 0)
                {
                    cps1_coinctrl_w((ushort)(value * 0x100));
                }
                else
                {
                    int i11 = 1;
                }
            }
            else if (address >= 0x800100 && address <= 0x80013f)
            {
                int i11 = 1;//cps1_cps_a_w
            }
            else if (address >= 0x800140 && address <= 0x80017f)
            {
                int i11 = 1;//cps1_cps_b_w
            }
            else if (address >= 0x900000 && address <= 0x92ffff)
            {
                gfxram[(address & 0x3ffff)] = (byte)(value);
                cps1_gfxram_w((address & 0x3ffff) / 2);
            }
            else if (address >= 0xf18000 && address <= 0xf19fff)
            {
                int offset = (address - 0xf18000) / 2;
                if ((address & 1) == 1)
                {
                    qsound_sharedram1_w(offset, (byte)value);
                }
                else
                {
                    int i1 = 1;
                }
            }
            else if (address >= 0xf1c004 && address <= 0xf1c005)
            {
                int i11 = 1;//cpsq_coinctrl2_w
            }
            else if (address >= 0xf1c006 && address <= 0xf1c007)
            {
                if ((address & 1) == 1)
                {
                    cps1_eeprom_port_w(value);
                }
            }
            else if (address >= 0xf1e000 && address <= 0xf1ffff)
            {
                int offset = (address - 0xf1e000) / 2;
                if ((address & 1) == 1)
                {
                    qsound_sharedram2_w(offset, (byte)value);
                }
                else
                {
                    int i1 = 1;
                }
            }
            else if (address >= 0xff0000 && address <= 0xffffff)
            {
                Memory.mainram[(address & 0xffff)] = (byte)(value);
            }
            else
            {
                int i11 = 1;
            }
        }
        public static void MQWriteWord(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x800030 && address + 1 <= 0x800037)
            {
                if (address % 2 == 0)
                {
                    cps1_coinctrl_w((ushort)(value * 0x100));
                }
                else
                {
                    int i11 = 1;
                }
            }
            else if (address >= 0x800100 && address + 1 <= 0x80013f)
            {
                cps1_cps_a_w((address & 0x3f) / 2, (ushort)value);
            }
            else if (address >= 0x800140 && address + 1 <= 0x80017f)
            {
                cps1_cps_b_w((address & 0x3f) / 2, (ushort)value);
            }
            else if (address >= 0x900000 && address + 1 <= 0x92ffff)
            {
                gfxram[(address & 0x3ffff)] = (byte)(value >> 8);
                gfxram[(address & 0x3ffff) + 1] = (byte)value;
                cps1_gfxram_w((address & 0x3ffff) / 2);
            }
            else if (address >= 0xf18000 && address + 1 <= 0xf19fff)
            {
                qsound_sharedram1_w((address - 0xf18000) >> 1, (byte)value);
            }
            else if (address >= 0xf1c004 && address + 1 <= 0xf1c005)
            {
                cpsq_coinctrl2_w((ushort)value);
            }
            else if (address >= 0xf1c006 && address + 1 <= 0xf1c007)
            {
                cps1_eeprom_port_w(value);
            }
            else if (address >= 0xf1e000 && address + 1 <= 0xf1ffff)
            {
                qsound_sharedram2_w((address - 0xf1e000) >> 1, (byte)value);
            }
            else if (address >= 0xff0000 && address + 1 <= 0xffffff)
            {
                Memory.mainram[(address & 0xffff)] = (byte)(value >> 8);
                Memory.mainram[(address & 0xffff) + 1] = (byte)(value);
            }
            else
            {
                int i11 = 1;
            }
        }
        public static void MQWriteLong(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x800030 && address + 3 <= 0x800037)
            {
                return;
            }
            else if (address >= 0x800100 && address + 3 <= 0x80013f)
            {
                cps1_cps_a_w((address & 0x3f) / 2, (ushort)(value >> 16));
                cps1_cps_a_w(((address + 2) & 0x3f) / 2, (ushort)value);
            }
            else if (address >= 0x800140 && address + 3 <= 0x80017f)
            {
                return;//cps1_cps_b_w
            }
            else if (address >= 0x900000 && address + 3 <= 0x92ffff)
            {
                if (address == 0x904000)
                {
                    int i11 = 1;
                }
                gfxram[(address & 0x3ffff)] = (byte)(value >> 24);
                gfxram[(address & 0x3ffff) + 1] = (byte)(value >> 16);
                gfxram[(address & 0x3ffff) + 2] = (byte)(value >> 8);
                gfxram[(address & 0x3ffff) + 3] = (byte)(value);
                cps1_gfxram_w((address & 0x3ffff) / 2);
                cps1_gfxram_w(((address + 2) & 0x3ffff) / 2);
                if (address == 0x00914000 && value != 0)
                {
                    //int i11 = 1;
                }
            }
            else if (address >= 0xf18000 && address + 3 <= 0xf19fff)
            {
                int i11 = 1;//qsound_sharedram1_w
            }
            else if (address >= 0xf1c004 && address + 3 <= 0xf1c005)
            {
                int i11 = 1;//cpsq_coinctrl2_w
            }
            else if (address >= 0xf1c006 && address + 3 <= 0xf1c007)
            {
                int i11 = 1;//cps1_eeprom_port_w
            }
            else if (address >= 0xf1e000 && address + 3 <= 0xf1ffff)
            {
                int i11 = 1;//qsound_sharedram2_w
            }
            else if (address >= 0xff0000 && address + 3 <= 0xffffff)
            {
                Memory.mainram[(address & 0xffff)] = (byte)(value >> 24);
                Memory.mainram[(address & 0xffff) + 1] = (byte)(value >> 16);
                Memory.mainram[(address & 0xffff) + 2] = (byte)(value >> 8);
                Memory.mainram[(address & 0xffff) + 3] = (byte)(value);
            }
            else
            {
                int i11 = 1;
            }
        }
        public static sbyte MC2ReadOpByte(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address <= 0x3fffff)
            {
                if (address < mainromopLength)
                {
                    result = (sbyte)(mainromop[address]);
                }
                else
                {
                    result = 0;
                }
            }
            return result;
        }
        public static sbyte MC2ReadPcrelByte(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address <= 0x3fffff)
            {
                if (address < mainromopLength)
                {
                    result = (sbyte)mainromop[address];
                }
                else
                {
                    result = 0;

                }
            }
            else
            {
                result = MC2ReadByte(address);
            }
            return result;
        }
        public static sbyte MC2ReadByte(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address <= 0x3fffff)
            {
                if (address < Memory.mainromLength)
                {
                    result = (sbyte)Memory.mainrom[address];
                }
                else
                {
                    result = 0;
                }
            }
            else if (address >= 0x400000 && address <= 0x40000b)
            {
                int offset = (address - 0x400000) / 2;
                result = (sbyte)cps2_output[offset];
            }
            else if (address >= 0x618000 && address <= 0x619fff)
            {
                int offset = (address - 0x618000) / 2;
                result = (sbyte)qsound_sharedram1_r(offset);
            }
            else if (address >= 0x662000 && address <= 0x662001)
            {
                result = 0;
            }
            else if (address >= 0x662008 && address <= 0x662009)
            {
                result = 0;
            }
            else if (address >= 0x662020 && address <= 0x662021)
            {
                result = 0;
            }
            else if (address >= 0x660000 && address <= 0x663fff)
            {
                result = 0;
            }
            else if (address >= 0x664000 && address <= 0x664001)
            {
                result = 0;
            }
            else if (address >= 0x708000 && address <= 0x709fff)
            {
                int offset = (address - 0x708000) / 2;
                result = (sbyte)cps2_objram2_r(offset);
            }
            else if (address >= 0x70a000 && address <= 0x70bfff)
            {
                int offset = (address - 0x70a000) / 2;
                result = (sbyte)cps2_objram2_r(offset);
            }
            else if (address >= 0x70c000 && address <= 0x70dfff)
            {
                int offset = (address - 0x70c000) / 2;
                result = (sbyte)cps2_objram2_r(offset);
            }
            else if (address >= 0x70e000 && address <= 0x70ffff)
            {
                int offset = (address - 0x70e000) / 2;
                result = (sbyte)cps2_objram2_r(offset);
            }
            else if (address >= 0x800140 && address <= 0x80017f)
            {
                int offset = (address - 0x800140) / 2;
                result = (sbyte)cps1_cps_b_r(offset);
            }
            else if (address >= 0x804000 && address <= 0x804001)
            {
                result = (sbyte)short0;
            }
            else if (address >= 0x804010 && address <= 0x804011)
            {
                if (address == 0x804010)
                {
                    result = (sbyte)(short1 >> 8);
                }
                else if (address == 0x804011)
                {
                    result = (sbyte)short1;
                }
            }
            else if (address >= 0x804020 && address <= 0x804021)
            {
                if (address == 0x804020)
                {
                    result = (sbyte)(short2 >> 8);
                }
                else if (address == 0x804021)
                {
                    result = (sbyte)(short2 & (Eeprom.eeprom_bit_r() - 2));
                }
            }
            else if (address >= 0x804030 && address <= 0x804031)
            {
                if (address == 0x804030)
                {
                    result = (sbyte)(cps2_qsound_volume_r() >> 8);
                }
                else
                {
                    result = (sbyte)cps2_qsound_volume_r();
                }
            }
            else if (address >= 0x8040b0 && address <= 0x8040b3)
            {
                result = (sbyte)kludge_r();
            }
            else if (address >= 0x804140 && address <= 0x80417f)
            {
                int offset = (address - 0x804140) / 2;
                result = (sbyte)cps1_cps_b_r(offset);
            }
            else if (address >= 0x900000 && address <= 0x92ffff)
            {
                result = (sbyte)gfxram[(address & 0x3ffff)];
            }
            else if (address >= 0xff0000 && address <= 0xffffff)
            {
                result = (sbyte)Memory.mainram[address & 0xffff];
            }
            return result;
        }
        public static short MC2ReadOpWord(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x3fffff)
            {
                if (address + 1 < mainromopLength)
                {
                    result = (short)(mainromop[address] * 0x100 + mainromop[address + 1]);
                }
                else
                {
                    result = 0;
                }
            }
            return result;
        }
        public static short MC2ReadPcrelWord(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x3fffff)
            {
                if (address + 1 < mainromopLength)
                {
                    result = (short)(mainromop[address] * 0x100 + mainromop[address + 1]);
                }
                else
                {
                    result = 0;
                }
            }
            else
            {
                result = MC2ReadWord(address);
            }
            return result;
        }
        public static short MC2ReadWord(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address <= 0x3fffff)
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
            else if (address >= 0x400000 && address <= 0x40000b)
            {
                int offset = (address - 0x400000) / 2;
                result = (short)cps2_output[offset];
            }
            else if (address >= 0x618000 && address <= 0x619fff)
            {
                int offset = (address - 0x618000) / 2;
                result = qsound_sharedram1_r(offset);
            }
            else if (address >= 0x662000 && address <= 0x662001)
            {
                result = 0;
            }
            else if (address >= 0x662008 && address <= 0x662009)
            {
                result = 0;
            }
            else if (address >= 0x662020 && address <= 0x662021)
            {
                result = 0;
            }
            else if (address >= 0x660000 && address <= 0x663fff)
            {
                result = 0;
            }
            else if (address >= 0x664000 && address <= 0x664001)
            {
                result = 0;
            }
            else if (address >= 0x708000 && address <= 0x709fff)
            {
                int offset = (address - 0x708000) / 2;
                result = cps2_objram2_r(offset);
            }
            else if (address >= 0x70a000 && address <= 0x70bfff)
            {
                int offset = (address - 0x70a000) / 2;
                result = cps2_objram2_r(offset);
            }
            else if (address >= 0x70c000 && address <= 0x70dfff)
            {
                int offset = (address - 0x70c000) / 2;
                result = cps2_objram2_r(offset);
            }
            else if (address >= 0x70e000 && address <= 0x70ffff)
            {
                int offset = (address - 0x70e000) / 2;
                result = cps2_objram2_r(offset);
            }
            else if (address >= 0x800140 && address <= 0x80017f)
            {
                int offset = (address - 0x800140) / 2;
                result = (short)cps1_cps_b_r(offset);
            }
            else if (address >= 0x804000 && address <= 0x804001)
            {
                result = short0;
            }
            else if (address >= 0x804010 && address <= 0x804011)
            {
                result = short1;
            }
            else if (address >= 0x804020 && address <= 0x804021)
            {
                result = (short)(short2 & (Eeprom.eeprom_bit_r() - 2));
            }
            else if (address >= 0x804030 && address <= 0x804031)
            {
                result = cps2_qsound_volume_r();
            }
            else if (address >= 0x8040b0 && address <= 0x8040b3)
            {
                result = kludge_r();
            }
            else if (address >= 0x804140 && address <= 0x80417f)
            {
                int offset = (address - 0x804140) / 2;
                result = (short)cps1_cps_b_r(offset);
            }
            else if (address >= 0x900000 && address + 1 <= 0x92ffff)
            {
                result = (short)(gfxram[(address & 0x3ffff)] * 0x100 + gfxram[(address & 0x3ffff) + 1]);
            }
            else if (address >= 0xff0000 && address + 1 <= 0xffffff)
            {
                result = (short)(Memory.mainram[(address & 0xffff)] * 0x100 + Memory.mainram[(address & 0xffff) + 1]);
            }
            return result;
        }
        public static int MC2ReadOpLong(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x3fffff)
            {
                if (address + 3 < mainromopLength)
                {
                    result = (int)(mainromop[address] * 0x1000000 + mainromop[address + 1] * 0x10000 + mainromop[address + 2] * 0x100 + mainromop[address + 3]);
                }
                else
                {
                    result = 0;
                }
            }
            return result;
        }
        public static int MC2ReadPcrelLong(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x3fffff)
            {
                if (address + 3 < mainromopLength)
                {
                    result = (int)(mainromop[address] * 0x1000000 + mainromop[address + 1] * 0x10000 + mainromop[address + 2] * 0x100 + mainromop[address + 3]);
                }
                else
                {
                    result = 0;
                }
            }
            else
            {
                result = MC2ReadLong(address);
            }
            return result;
        }
        public static int MC2ReadLong(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x3fffff)
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
            else if (address >= 0x400000 && address <= 0x40000b)
            {
                result = 0;
                //int offset = (add - 0x400000) / 2;
                //return (short)CPS1.cps2_output[offset];
            }
            else if (address >= 0x618000 && address <= 0x619fff)
            {
                result = 0;
                //int offset = (add - 0x618000) / 2;
                //return CPS1.qsound_sharedram1_r(offset);
            }
            else if (address >= 0x662000 && address <= 0x662001)
            {
                result = 0;
            }
            else if (address >= 0x662008 && address <= 0x662009)
            {
                result = 0;
            }
            else if (address >= 0x662020 && address <= 0x662021)
            {
                result = 0;
            }
            else if (address >= 0x660000 && address <= 0x663fff)
            {
                result = 0;
            }
            else if (address >= 0x664000 && address <= 0x664001)
            {
                result = 0;
            }
            else if (address >= 0x708000 && address <= 0x709fff)
            {
                int offset = (address - 0x708000) / 2;
                result = (int)((ushort)cps2_objram2_r(offset) * 0x10000 + (ushort)cps2_objram2_r(offset + 1));
            }
            else if (address >= 0x70a000 && address <= 0x70bfff)
            {
                int offset = (address - 0x70a000) / 2;
                result = (int)((ushort)cps2_objram2_r(offset) * 0x10000 + (ushort)cps2_objram2_r(offset + 1));
            }
            else if (address >= 0x70c000 && address <= 0x70dfff)
            {
                int offset = (address - 0x70c000) / 2;
                result = (int)((ushort)cps2_objram2_r(offset) * 0x10000 + (ushort)cps2_objram2_r(offset + 1));
            }
            else if (address >= 0x70e000 && address <= 0x70ffff)
            {
                int offset = (address - 0x70e000) / 2;
                result = (int)((ushort)cps2_objram2_r(offset) * 0x10000 + (ushort)cps2_objram2_r(offset + 1));
            }
            else if (address >= 0x800140 && address <= 0x80017f)
            {
                result = 0;
                //int offset = (add - 0x800140) / 2;
                //return (short)cps1_cps_b_r(offset);
            }
            else if (address >= 0x804000 && address <= 0x804001)
            {
                result = 0;
                //return (int)sbyte0 & 0xff;
            }
            else if (address >= 0x804010 && address <= 0x804011)
            {
                result = -1;
                //return short1;
            }
            else if (address >= 0x804020 && address <= 0x804021)
            {
                result = 0;
                //return (int)sbyte2 & 0xff;
            }
            else if (address >= 0x804030 && address <= 0x804031)
            {
                result = 0;
                //return CPS1.cps2_qsound_volume_r();
            }
            else if (address >= 0x8040b0 && address <= 0x8040b3)
            {
                result = kludge_r();
            }
            else if (address >= 0x804140 && address <= 0x80417f)
            {
                result = 0;
                //int offset = (add - 0x804140) / 2;
                //return (short)CPS1.cps1_cps_b_r(offset);
            }
            else if (address >= 0x900000 && address + 3 <= 0x92ffff)
            {
                result = (int)(gfxram[(address & 0x3ffff)] * 0x1000000 + gfxram[(address & 0x3ffff) + 1] * 0x10000 + gfxram[(address & 0x3ffff) + 2] * 0x100 + gfxram[(address & 0x3ffff) + 3]);
            }
            else if (address >= 0xff0000 && address + 3 <= 0xffffff)
            {
                result = (int)(Memory.mainram[(address & 0xffff)] * 0x1000000 + Memory.mainram[(address & 0xffff) + 1] * 0x10000 + Memory.mainram[(address & 0xffff) + 2] * 0x100 + Memory.mainram[(address & 0xffff) + 3]);
            }
            return result;
        }
        public static void MC2WriteByte(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address <= 0x3fffff)
            {
                int i11 = 1;
            }
            if (address >= 0x400000 && address <= 0x40000b)
            {
                int offset = (address - 0x400000) / 2;
                cps2_output[offset] = (ushort)value;
            }
            else if (address >= 0x618000 && address <= 0x619fff)
            {
                int offset = (address - 0x618000) / 2;
                qsound_sharedram1_w(offset, (byte)value);
            }
            else if (address >= 0x662000 && address <= 0x662001)
            {
                int i11 = 1;
            }
            else if (address >= 0x662008 && address <= 0x662009)
            {
                int i11 = 1;
            }
            else if (address >= 0x662020 && address <= 0x662021)
            {
                int i11 = 1;
            }
            else if (address >= 0x660000 && address <= 0x663fff)
            {
                int i11 = 1;
            }
            else if (address >= 0x664000 && address <= 0x664001)
            {
                int i11 = 1;
            }
            else if (address >= 0x700000 && address <= 0x701fff)
            {
                int offset = (address - 0x700000) / 2;
                cps2_objram1_w(offset, (ushort)value);
            }
            else if (address >= 0x708000 && address <= 0x709fff)
            {
                int offset = (address - 0x708000) / 2;
                cps2_objram2_w(offset, (ushort)value);
            }
            else if (address >= 0x70a000 && address <= 0x70bfff)
            {
                int i1 = 1;
                //int offset = (add - 0x70a000) / 2;
                //cps2_objram2_w(offset, (ushort)value);
            }
            else if (address >= 0x70c000 && address <= 0x70dfff)
            {
                int i1 = 1;
                //int offset = (add - 0x70c000) / 2;
                //cps2_objram2_w(offset, (ushort)value);
            }
            else if (address >= 0x70e000 && address <= 0x70ffff)
            {
                int i1 = 1;
                //int offset = (add - 0x70e000) / 2;
                //cps2_objram2_w(offset, (ushort)value);
            }
            else if (address >= 0x800100 && address <= 0x80013f)
            {
                int i11 = 1;//cps1_cps_a_w
            }
            else if (address >= 0x800140 && address <= 0x80017f)
            {
                int i11 = 1;//cps1_cps_b_w
            }
            else if (address >= 0x804040 && address <= 0x804041)
            {
                if (address == 0x804040)
                {
                    cps2_eeprom_port_bh(value);
                }
                else if (address == 0x804041)
                {
                    cps2_eeprom_port_bl(value);
                }
            }
            else if (address >= 0x8040a0 && address <= 0x8040a1)
            {
                int i11 = 1;//nop
            }
            else if (address >= 0x8040e0 && address <= 0x8040e1)
            {
                cps2_objram_bank_w(value);
            }
            else if (address >= 0x804100 && address <= 0x80413f)
            {
                int i11 = 1;//cps1_cps_a_w
            }
            else if (address >= 0x804140 && address <= 0x80417f)
            {
                int i11 = 1;//cps1_cps_b_w
            }
            else if (address >= 0x900000 && address <= 0x92ffff)
            {
                gfxram[(address & 0x3ffff)] = (byte)(value);
                cps1_gfxram_w((address & 0x3ffff) / 2);
            }
            else if (address >= 0xff0000 && address <= 0xffffff)
            {
                Memory.mainram[(address & 0xffff)] = (byte)(value);
            }
            else
            {
                int i11 = 1;
            }
        }
        public static void MC2WriteWord(int address, short value)
        {
            address &= 0xffffff;
            if (address <= 0x3fffff)
            {
                int i11 = 1;
            }
            if (address >= 0x400000 && address + 1 <= 0x40000b)
            {
                int offset = (address - 0x400000) / 2;
                cps2_output[offset] = (ushort)value;
            }
            else if (address >= 0x618000 && address <= 0x619fff)
            {
                int offset = (address - 0x618000) / 2;
                qsound_sharedram1_w(offset, (byte)value);
            }
            else if (address >= 0x662000 && address <= 0x662001)
            {
                int i11 = 1;
            }
            else if (address >= 0x662008 && address <= 0x662009)
            {
                int i11 = 1;
            }
            else if (address >= 0x662020 && address <= 0x662021)
            {
                int i11 = 1;
            }
            else if (address >= 0x660000 && address <= 0x663fff)
            {
                int i11 = 1;
            }
            else if (address >= 0x664000 && address <= 0x664001)
            {
                int i11 = 1;
            }
            else if (address >= 0x700000 && address <= 0x701fff)
            {
                int offset = (address - 0x700000) / 2;
                cps2_objram1_w(offset, (ushort)value);
            }
            else if (address >= 0x708000 && address <= 0x709fff)
            {
                int offset = (address - 0x708000) / 2;
                cps2_objram2_w(offset, (ushort)value);
            }
            else if (address >= 0x70a000 && address <= 0x70bfff)
            {
                int offset = (address - 0x70a000) / 2;
                cps2_objram2_w(offset, (ushort)value);
            }
            else if (address >= 0x70c000 && address <= 0x70dfff)
            {
                int offset = (address - 0x70c000) / 2;
                cps2_objram2_w(offset, (ushort)value);
            }
            else if (address >= 0x70e000 && address <= 0x70ffff)
            {
                int offset = (address - 0x70e000) / 2;
                cps2_objram2_w(offset, (ushort)value);
            }
            else if (address >= 0x800100 && address <= 0x80013f)
            {
                cps1_cps_a_w((address & 0x3f) / 2, (ushort)value);
            }
            else if (address >= 0x800140 && address + 1 <= 0x80017f)
            {
                cps1_cps_b_w((address & 0x3f) / 2, (ushort)value);
            }
            else if (address >= 0x804040 && address <= 0x804041)
            {
                cps2_eeprom_port_w(value);
            }
            else if (address >= 0x8040a0 && address <= 0x8040a1)
            {
                int i11 = 1;//nop
            }
            else if (address >= 0x8040e0 && address <= 0x8040e1)
            {
                cps2_objram_bank_w(value);
            }
            else if (address >= 0x804100 && address + 1 <= 0x80413f)
            {
                cps1_cps_a_w((address & 0x3f) / 2, (ushort)value);
            }
            else if (address >= 0x804140 && address <= 0x80417f)
            {
                cps1_cps_b_w((address & 0x3f) / 2, (ushort)value);
            }
            else if (address >= 0x900000 && address + 1 <= 0x92ffff)
            {
                gfxram[address & 0x3ffff] = (byte)(value >> 8);
                gfxram[(address & 0x3ffff) + 1] = (byte)value;
                cps1_gfxram_w((address & 0x3ffff) / 2);
            }
            else if (address >= 0xff0000 && address + 1 <= 0xffffff)
            {
                Memory.mainram[address & 0xffff] = (byte)(value >> 8);
                Memory.mainram[(address & 0xffff) + 1] = (byte)value;
            }
            else
            {
                int i11 = 1;
            }
        }
        public static void MC2WriteLong(int address, int value)
        {
            address &= 0xffffff;
            if (address <= 0x3fffff)
            {
                int i11 = 1;
            }
            if (address >= 0x400000 && address + 3 <= 0x40000b)
            {
                int offset = (address - 0x400000) / 2;
                cps2_output[offset] = (ushort)(value >> 16);
                cps2_output[offset + 1] = (ushort)value;
            }
            else if (address >= 0x618000 && address <= 0x619fff)
            {
                int offset = (address - 0x618000) / 2;
                qsound_sharedram1_w(offset, (byte)(value >> 16));
                qsound_sharedram1_w(offset + 1, (byte)value);
            }
            else if (address >= 0x662000 && address <= 0x662001)
            {
                int i11 = 1;
            }
            else if (address >= 0x662008 && address <= 0x662009)
            {
                int i11 = 1;
            }
            else if (address >= 0x662020 && address <= 0x662021)
            {
                int i11 = 1;
            }
            else if (address >= 0x660000 && address <= 0x663fff)
            {
                int i11 = 1;
            }
            else if (address >= 0x664000 && address <= 0x664001)
            {
                int i11 = 1;
            }
            else if (address >= 0x700000 && address <= 0x701fff)
            {
                int offset = (address - 0x700000) / 2;
                cps2_objram1_w(offset, (ushort)(value >> 16));
                cps2_objram1_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x708000 && address <= 0x709fff)
            {
                int offset = (address - 0x708000) / 2;
                cps2_objram2_w(offset, (ushort)(value >> 16));
                cps2_objram2_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x70a000 && address <= 0x70bfff)
            {
                int offset = (address - 0x70a000) / 2;
                cps2_objram2_w(offset, (ushort)(value >> 16));
                cps2_objram2_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x70c000 && address <= 0x70dfff)
            {
                int offset = (address - 0x70c000) / 2;
                cps2_objram2_w(offset, (ushort)(value >> 16));
                cps2_objram2_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x70e000 && address <= 0x70ffff)
            {
                int offset = (address - 0x70e000) / 2;
                cps2_objram2_w(offset, (ushort)(value >> 16));
                cps2_objram2_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x800100 && address + 3 <= 0x80013f)
            {
                int offset = (address & 0x3f) / 2;
                cps1_cps_a_w(offset, (ushort)(value >> 16));
                cps1_cps_a_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x800140 && address <= 0x80017f)
            {
                int i11 = 1;
                //cps1_cps_b_w((add & 0x3f) / 2, (ushort)value);
            }
            else if (address >= 0x804040 && address <= 0x804041)
            {
                int i11 = 1;
                //cps2_eeprom_port_w(value);
            }
            else if (address >= 0x8040a0 && address <= 0x8040a1)
            {
                int i11 = 1;//nop
            }
            else if (address >= 0x8040e0 && address <= 0x8040e1)
            {
                int i11 = 1;
                //cps2_objram_bank_w(value);
            }
            else if (address >= 0x804100 && address <= 0x80413f)
            {
                int offset = (address & 0x3f) / 2;
                cps1_cps_a_w(offset, (ushort)(value >> 16));
                cps1_cps_a_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x804140 && address <= 0x80417f)
            {
                int i11 = 1;
                //cps1_cps_b_w((add & 0x3f) / 2, (ushort)value);
            }
            else if (address >= 0x900000 && address + 3 <= 0x92ffff)
            {
                gfxram[(address & 0x3ffff)] = (byte)(value >> 24);
                gfxram[(address & 0x3ffff) + 1] = (byte)(value >> 16);
                gfxram[(address & 0x3ffff) + 2] = (byte)(value >> 8);
                gfxram[(address & 0x3ffff) + 3] = (byte)(value);
                cps1_gfxram_w((address & 0x3ffff) / 2);
                cps1_gfxram_w(((address + 2) & 0x3ffff) / 2);
            }
            else if (address >= 0xff0000 && address + 3 <= 0xffffff)
            {
                Memory.mainram[(address & 0xffff)] = (byte)(value >> 24);
                Memory.mainram[(address & 0xffff) + 1] = (byte)(value >> 16);
                Memory.mainram[(address & 0xffff) + 2] = (byte)(value >> 8);
                Memory.mainram[(address & 0xffff) + 3] = (byte)(value);
            }
            else
            {
                int i11 = 1;
            }
        }
        public static byte ZQReadOp(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = audioromop[address & 0x7fff];
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public unsafe static byte ZQReadMemory(ushort address)
        {
            byte result = 0;
            if (address <= 0x7fff)
            {
                result = Memory.audiorom[address & 0x7fff];
            }
            else if (address >= 0x8000 && address <= 0xbfff)
            {
                result = Memory.audiorom[basebanksnd + (address & 0x3fff)];
            }
            else if (address >= 0xc000 && address <= 0xcfff)
            {
                result = qsound_sharedram1[address & 0xfff];
            }
            else if (address == 0xd007)
            {
                result = QSound.qsound_status_r();
            }
            else if (address >= 0xf000 && address <= 0xffff)
            {
                result = qsound_sharedram2[address & 0xfff];
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static void ZQWriteMemory(ushort address, byte value)
        {
            if (address >= 0xc000 && address <= 0xcfff)
            {
                qsound_sharedram1[address & 0xfff] = value;
            }
            else if (address == 0xd000)
            {
                QSound.qsound_data_h_w(value);
            }
            else if (address == 0xd001)
            {
                QSound.qsound_data_l_w(value);
            }
            else if (address == 0xd002)
            {
                QSound.qsound_cmd_w(value);
            }
            else if (address == 0xd003)
            {
                qsound_banksw_w(value);
            }
            else if (address >= 0xf000 && address <= 0xffff)
            {
                qsound_sharedram2[address & 0xfff] = value;
            }
            else
            {

            }
        }
        public static byte ZCReadHardware(ushort address)
        {
            return 0;
        }
        public static void ZCWriteHardware(ushort address, byte value)
        {

        }
        public static int ZIRQCallback()
        {
            return Cpuint.cpu_irq_callback(Z80A.zz1[0].cpunum, 0);
        }
    }
}
