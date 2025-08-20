namespace MAME.Core
{
    public unsafe partial class CPS
    {
        public static sbyte MCReadByte_forgottn(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= 0x800052 && address <= 0x800055)
            {
                int offset = (address - 0x800052) / 2;
                result = (sbyte)((Inptport.input_port_read_direct(Inptport.analog_p0) - dial0) >> (8 * offset));
            }
            else if (address >= 0x80005a && address <= 0x80005d)
            {
                int offset = (address - 0x80005a) / 2;
                result = (sbyte)((Inptport.input_port_read_direct(Inptport.analog_p1) - dial1) >> (8 * offset));
            }
            else
            {
                result = MCReadByte(address);
            }
            return result;
        }
        public static short MCReadWord_forgottn(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= 0x800052 && address <= 0x800055)
            {
                int offset = (address - 0x800052) / 2;
                result = (short)(((Inptport.input_port_read_direct(Inptport.analog_p0) - dial0) >> (8 * offset)) & 0xff);
            }
            else if (address >= 0x80005a && address <= 0x80005d)
            {
                int offset = (address - 0x80005a) / 2;
                result = (short)(((Inptport.input_port_read_direct(Inptport.analog_p1) - dial1) >> (8 * offset)) & 0xff);
            }
            else
            {
                result = MCReadWord(address);
            }
            return result;
        }
        public static int MCReadLong_forgottn(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x800052 && address + 3 <= 0x800055)
            {
                result = (int)(((Inptport.input_port_read_direct(Inptport.analog_p0) - dial0) & 0xff) * 0x10000 + (((Inptport.input_port_read_direct(Inptport.analog_p0) - dial0) >> 8) & 0xff));
            }
            else if (address >= 0x80005a && address + 3 <= 0x80005d)
            {
                result = (int)(((Inptport.input_port_read_direct(Inptport.analog_p1) - dial1) & 0xff) * 0x10000 + (((Inptport.input_port_read_direct(Inptport.analog_p1) - dial1) >> 8) & 0xff));
            }
            else
            {
                result = MCReadLong(address);
            }
            return result;
        }
        public static void MCWriteByte_forgottn(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x800040 && address <= 0x800041)
            {
                dial0 = (int)Inptport.input_port_read_direct(Inptport.analog_p0);
            }
            else if (address >= 0x800048 && address <= 0x800049)
            {
                dial1 = (int)Inptport.input_port_read_direct(Inptport.analog_p1);
            }
            else
            {
                MCWriteByte(address, value);
            }
        }
        public static void MCWriteWord_forgottn(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x800040 && address <= 0x800041)
            {
                dial0 = (int)Inptport.input_port_read_direct(Inptport.analog_p0);
            }
            else if (address >= 0x800048 && address <= 0x800049)
            {
                dial1 = (int)Inptport.input_port_read_direct(Inptport.analog_p1);
            }
            else
            {
                MCWriteWord(address, value);
            }
        }
        public static void MCWriteLong_forgottn(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x800040 && address <= 0x800041)
            {
                int i1 = 1;
            }
            else if (address >= 0x800048 && address <= 0x800049)
            {
                int i1 = 1;
            }
            else
            {
                MCWriteLong(address, value);
            }
        }
        public static sbyte MCReadByte_sf2thndr(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= 0x8001c0 && address <= 0x8001ff)
            {
                int offset = (address - 0x8001c0) / 2;
                result = (sbyte)cps1_cps_b_r(offset);
            }
            else
            {
                result = MCReadByte(address);
            }
            return result;
        }
        public static short MCReadWord_sf2thndr(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= 0x8001c0 && address + 1 <= 0x8001ff)
            {
                int offset = (address - 0x8001c0) / 2;
                result = (short)cps1_cps_b_r(offset);
            }
            else
            {
                result = MCReadWord(address);
            }
            return result;
        }
        public static void MCWriteWord_sf2thndr(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x8001c0 && address + 1 <= 0x8001ff)
            {
                cps1_cps_b_w((address & 0x3f) / 2, (ushort)value);
            }
            else
            {
                MCWriteWord(address, value);
            }
        }
        public static short MCReadWord_sf2ceblp(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= 0x57a2b0 && address + 1 <= 0x57a2b1)
            {
                if (sf2ceblp_prot == 0x0)
                {
                    result = 0x1992;
                }
                else if (sf2ceblp_prot == 0x04)
                {
                    result = 0x0408;
                }
                else
                {
                    result = -1;
                }
            }
            else
            {
                result = MCReadWord(address);
            }
            return result;
        }
        public static void MCWriteWord_sf2ceblp(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x5762b0 && address + 1 <= 0x5762b1)
            {
                sf2ceblp_prot = value;
            }
            else
            {
                MCWriteWord(address, value);
            }
        }
        public static sbyte MCReadByte_sf2m3(int address)
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
            else if (address >= 0x800010 && address <= 0x800011)
            {
                if (address == 0x800010)
                {
                    result = (sbyte)(short1 >> 8);
                }
                else if (address == 0x800011)
                {
                    result = (sbyte)(short1);
                }
            }
            else if (address >= 0x800028 && address <= 0x80002f)
            {
                int offset = (address - 0x800028) / 2;
                result = (sbyte)cps1_dsw_r(offset);
            }
            else if (address >= 0x800140 && address <= 0x80017f)
            {
                int offset = (address - 0x800140) / 2;
                result = (sbyte)cps1_cps_b_r(offset);
            }
            else if (address >= 0x800186 && address <= 0x800187)
            {
                result = (sbyte)short2;
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
        public static short MCReadWord_sf2m3(int address)
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
            else if (address >= 0x800010 && address + 1 <= 0x800011)
            {
                result = short1;
            }
            else if (address >= 0x800028 && address + 1 <= 0x80002f)
            {
                int offset = (address - 0x800028) / 2;
                result = (short)(((byte)(cps1_dsw_r(offset)) << 8) | (byte)cps1_dsw_r(offset));
            }
            else if (address >= 0x800140 && address + 1 <= 0x80017f)
            {
                int offset = (address - 0x800140) / 2;
                result = (short)cps1_cps_b_r(offset);
            }
            else if (address >= 0x800186 && address + 1 <= 0x800187)
            {
                result = (short)((short2 << 8) | (byte)short2);
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
        public static int MCReadLong_sf2m3(int address)
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
            else if (address >= 0x800010 && address + 3 <= 0x800011)
            {
                result = 0;
            }
            else if (address >= 0x800028 && address + 3 <= 0x80002f)
            {
                result = 0;
            }
            else if (address >= 0x800140 && address + 3 <= 0x80017f)
            {
                int offset = (address - 0x800140) / 2;
                result = cps1_cps_b_r(offset) * 0x10000 + cps1_cps_b_r(offset + 1);
            }
            else if (address >= 0x800186 && address + 3 <= 0x800187)
            {
                result = 0;
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
        public static void MCWriteByte_sf2m3(int address, sbyte value)
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
            else if (address >= 0x800190 && address <= 0x800197)
            {
                Sound.soundlatch_w((ushort)value);
            }
            else if (address >= 0x800198 && address <= 0x80019f)
            {
                Sound.soundlatch2_w((ushort)value);
            }
            else if (address >= 0x8001a0 && address <= 0x8001c3)
            {
                return;
            }
            else if (address >= 0x8001c4 && address <= 0x8001c5)
            {
                return;
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
        public static void MCWriteWord_sf2m3(int address, short value)
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
            else if (address >= 0x800190 && address + 1 <= 0x800197)
            {
                Sound.soundlatch_w((ushort)value);
            }
            else if (address >= 0x800198 && address + 1 <= 0x80019f)
            {
                Sound.soundlatch2_w((ushort)value);
            }
            else if (address >= 0x8001a0 && address + 1 <= 0x8001c3)
            {
                cps1_cps_a_w((address - 0x8001a0) / 2, (ushort)value);
            }
            else if (address >= 0x8001c4 && address + 1 <= 0x8001c5)
            {
                sf2m3_layer_w((ushort)value);
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
        public static void MCWriteLong_sf2m3(int address, int value)
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
            else if (address >= 0x800190 && address + 3 <= 0x800197)
            {
                return;
            }
            else if (address >= 0x800198 && address + 3 <= 0x80019f)
            {
                return;
            }
            else if (address >= 0x8001a0 && address + 3 <= 0x8001c3)
            {
                cps1_cps_a_w((address - 0x8001a0) / 2, (ushort)(value >> 16));
                cps1_cps_a_w((address + 2 - 0x8001a0) / 2, (ushort)value);
            }
            else if (address >= 0x8001c4 && address + 3 <= 0x8001c5)
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
        public static sbyte MCReadByte_sf2m10(int address)
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
                result = (sbyte)cps1_cps_b_r(offset);
            }
            else if (address >= 0x900000 && address <= 0x92ffff)
            {
                result = (sbyte)gfxram[(address & 0x3ffff)];
            }
            else if (address >= 0xe00000 && address <= 0xefffff)
            {
                result = (sbyte)mainram2[address & 0xfffff];
            }
            else if (address >= 0xf1c000 && address <= 0xf1c001)
            {
                result = (sbyte)short2;
            }
            else if (address >= 0xfeff00 && address <= 0xfeffff)
            {
                result = (sbyte)mainram3[address & 0xff];
            }
            else if (address >= 0xff0000 && address <= 0xffffff)
            {
                result = (sbyte)Memory.mainram[address & 0xffff];
            }
            return result;
        }
        public static short MCReadWord_sf2m10(int address)
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
                result = short1;
            }
            else if (address >= 0x800018 && address + 1 <= 0x80001f)
            {
                int offset = (address - 0x800018) / 2;
                result = (short)(((ushort)(cps1_dsw_r(offset)) << 8) | (ushort)cps1_dsw_r(offset));
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
            else if (address >= 0xe00000 && address + 1 <= 0xefffff)
            {
                result = (short)(mainram2[(address & 0xfffff)] * 0x100 + mainram2[(address & 0xfffff) + 1]);
            }
            else if (address >= 0xf1c000 && address + 1 <= 0xf1c001)
            {
                result = (short)((short2 << 8) | (byte)short2);
            }
            else if (address >= 0xfeff00 && address + 1 <= 0xfeffff)
            {
                result = (short)(mainram3[(address & 0xff)] * 0x100 + mainram3[(address & 0xff) + 1]);
            }
            else if (address >= 0xff0000 && address + 1 <= 0xffffff)
            {
                result = (short)(Memory.mainram[(address & 0xffff)] * 0x100 + Memory.mainram[(address & 0xffff) + 1]);
            }
            return result;
        }
        public static int MCReadLong_sf2m10(int address)
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
            else if (address >= 0xe00000 && address + 3 <= 0xefffff)
            {
                result = (int)(mainram2[(address & 0xfffff)] * 0x1000000 + mainram2[(address & 0xfffff) + 1] * 0x10000 + mainram2[(address & 0xfffff) + 2] * 0x100 + mainram2[(address & 0xfffff) + 3]);
            }
            else if (address >= 0xf1c000 && address + 3 <= 0xf1c001)
            {
                result = 0;
            }
            else if (address >= 0xfeff00 && address + 3 <= 0xfeffff)
            {
                result = (int)(mainram3[(address & 0xff)] * 0x1000000 + mainram3[(address & 0xff) + 1] * 0x10000 + mainram3[(address & 0xff) + 2] * 0x100 + mainram3[(address & 0xff) + 3]);
            }
            else if (address >= 0xff0000 && address + 3 <= 0xffffff)
            {
                result = (int)(Memory.mainram[(address & 0xffff)] * 0x1000000 + Memory.mainram[(address & 0xffff) + 1] * 0x10000 + Memory.mainram[(address & 0xffff) + 2] * 0x100 + Memory.mainram[(address & 0xffff) + 3]);
            }
            return result;
        }
        public static void MCWriteByte_sf2m10(int address, sbyte value)
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
                cps1_cps_a_w(address & 0x3f, (byte)value);
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
            else if (address >= 0x8001a2 && address <= 0x8001b3)
            {
                cps1_cps_a_w(address - 0x8001a2, (byte)value);
            }
            else if (address >= 0x8001fe && address <= 0x8001ff)
            {
                return;
            }
            else if (address >= 0x900000 && address <= 0x92ffff)
            {
                gfxram[(address & 0x3ffff)] = (byte)(value);
                cps1_gfxram_w((address & 0x3ffff) / 2);
            }
            else if (address >= 0xe00000 && address <= 0xefffff)
            {
                mainram2[(address & 0xfffff)] = (byte)(value);
            }
            else if (address >= 0xfeff00 && address <= 0xfeffff)
            {
                mainram3[(address & 0xff)] = (byte)(value);
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
        public static void MCWriteWord_sf2m10(int address, short value)
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
            else if (address >= 0x8001a2 && address + 1 <= 0x8001b3)
            {
                cps1_cps_a_w((address - 0x8001a2) / 2, (ushort)value);
            }
            else if (address >= 0x8001fe && address + 1 <= 0x8001ff)
            {
                return;
            }
            else if (address >= 0x900000 && address + 1 <= 0x92ffff)
            {
                gfxram[(address & 0x3ffff)] = (byte)(value >> 8);
                gfxram[(address & 0x3ffff) + 1] = (byte)value;
                cps1_gfxram_w((address & 0x3ffff) / 2);
            }
            else if (address >= 0xe00000 && address + 1 <= 0xefffff)
            {
                mainram2[(address & 0xfffff)] = (byte)(value >> 8);
                mainram2[(address & 0xfffff) + 1] = (byte)(value);
            }
            else if (address >= 0xfeff00 && address + 1 <= 0xfeffff)
            {
                mainram3[(address & 0xff)] = (byte)(value >> 8);
                mainram3[(address & 0xff) + 1] = (byte)(value);
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
        public static void MCWriteLong_sf2m10(int address, int value)
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
            else if (address >= 0x8001a2 && address + 3 <= 0x8001b3)
            {
                cps1_cps_a_w((address - 0x8001a2) / 2, (ushort)(value >> 16));
                cps1_cps_a_w((address + 2 - 0x8001a2) / 2, (ushort)value);
            }
            else if (address >= 0x8001fe && address + 3 <= 0x8001ff)
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
            else if (address >= 0xe00000 && address + 3 <= 0xefffff)
            {
                mainram2[(address & 0xfffff)] = (byte)(value >> 24);
                mainram2[(address & 0xfffff) + 1] = (byte)(value >> 16);
                mainram2[(address & 0xfffff) + 2] = (byte)(value >> 8);
                mainram2[(address & 0xfffff) + 3] = (byte)(value);
            }
            else if (address >= 0xfeff00 && address + 3 <= 0xfeffff)
            {
                mainram3[(address & 0xff)] = (byte)(value >> 24);
                mainram3[(address & 0xff) + 1] = (byte)(value >> 16);
                mainram3[(address & 0xff) + 2] = (byte)(value >> 8);
                mainram3[(address & 0xff) + 3] = (byte)(value);
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
        public static short MCReadWord_sf2dongb(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address == 0x180000 || address == 0x1f7040)
            {
                result = 0x0210;
            }
            else
            {
                result = MCReadWord(address);
            }
            return result;
        }
        public static sbyte MCReadByte_dinohunt(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= 0xf18000 && address <= 0xf19fff)
            {
                result = -1;
            }
            else if (address >= 0xfc0000 && address <= 0xfc0001)
            {
                result = (sbyte)short2;
            }
            else
            {
                result = MCReadByte(address);
            }
            return result;
        }
        public static sbyte MCReadByte_pang3(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= 0x80017a && address <= 0x80017b)
            {
                result = (sbyte)cps1_eeprom_port_r();
            }
            else
            {
                result = MCReadByte(address);
            }
            return result;
        }
        public static short MCReadWord_pang3(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= 0x80017a && address + 1 <= 0x80017b)
            {
                result = (short)cps1_eeprom_port_r();
            }
            else
            {
                result = MCReadWord(address);
            }
            return result;
        }
        public static void MCWriteByte_pang3(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x80017a && address <= 0x80017b)
            {
                if ((address & 1) == 1)
                {
                    cps1_eeprom_port_w(value);
                }
            }
            else
            {
                MCWriteByte(address, value);
            }
        }
        public static void MCWriteWord_pang3(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x80017a && address + 1 <= 0x80017b)
            {
                cps1_eeprom_port_w(value);
            }
            else
            {
                MCWriteWord(address, value);
            }
        }
        public static sbyte MC2ReadByte_ecofghtr(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= 0x804000 && address <= 0x804001)
            {
                result = 0;
            }
            else
            {
                result = MC2ReadByte(address);
            }
            return result;
        }
        public static short MC2ReadWord_ecofghtr(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= 0x804000 && address + 1 <= 0x804001)
            {
                result = 0;
            }
            else
            {
                result = MC2ReadWord(address);
            }
            return result;
        }
        public static sbyte MC2ReadOpByte_dead(int address)
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
        public static sbyte MC2ReadByte_dead(int address)
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
            else if (address >= 0xff0000 && address <= 0xffffef)
            {
                result = (sbyte)Memory.mainram[address & 0xffff];
            }
            else if (address >= 0xfffff0 && address <= 0xfffffb)
            {
                int offset = (address - 0xfffff0) / 2;
                result = (sbyte)cps2_output[offset];
            }
            else if (address >= 0xfffffc && address <= 0xffffff)
            {
                result = 0;
            }
            return result;
        }
        public static short MC2ReadOpWord_dead(int address)
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
            return result;
        }
        public static short MC2ReadWord_dead(int address)
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
            else if (address >= 0xff0000 && address + 1 <= 0xffffef)
            {
                return (short)(Memory.mainram[(address & 0xffff)] * 0x100 + Memory.mainram[(address & 0xffff) + 1]);
            }
            else if (address >= 0xfffff0 && address + 1 <= 0xfffffb)
            {
                int offset = (address - 0xfffff0) / 2;
                result = (short)cps2_output[offset];
            }
            else if (address >= 0xfffffc && address + 1 <= 0xffffff)
            {
                result = 0;
            }
            return result;
        }
        public static int MC2ReadOpLong_dead(int address)
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
            return result;
        }
        public static int MC2ReadLong_dead(int address)
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
            else if (address >= 0x400000 && address + 3 <= 0x40000b)
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
            else if (address >= 0x708000 && address + 3 <= 0x709fff)
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
                result = 0;
                //return CPS1.kludge_r();
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
            else if (address >= 0xff0000 && address + 3 <= 0xffffef)
            {
                result = (int)(Memory.mainram[(address & 0xffff)] * 0x1000000 + Memory.mainram[(address & 0xffff) + 1] * 0x10000 + Memory.mainram[(address & 0xffff) + 2] * 0x100 + Memory.mainram[(address & 0xffff) + 3]);
            }
            else if (address >= 0xfffff0 && address + 3 <= 0xfffffb)
            {
                result = 0;
                //int offset = (address - 0xfffff0) / 2;
                //return (sbyte)cps2_output[offset];
            }
            else if (address >= 0xfffffc && address + 3 <= 0xffffff)
            {
                result = 0;
            }
            return result;
        }
        public static void MC2WriteByte_dead(int address, sbyte value)
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
            else if (address >= 0xff0000 && address <= 0xffffef)
            {
                Memory.mainram[(address & 0xffff)] = (byte)(value);
            }
            else if (address >= 0xfffff0 && address <= 0xfffffb)
            {
                int offset = (address - 0xfffff0) / 2;
                cps2_output[offset] = (ushort)value;
            }
            else if (address >= 0xfffffc && address <= 0xffffff)
            {
                int i11 = 1;
            }
            else
            {
                int i11 = 1;
            }
        }
        public static void MC2WriteWord_dead(int address, short value)
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
                int offset = (address - 0x70a000) / 2;
                cps2_objram2_w(offset, (ushort)value);
            }
            else if (address >= 0x70c000 && address <= 0x70dfff)
            {
                int i11 = 1;
                //int offset = (add - 0x70c000) / 2;
                //cps2_objram2_w(offset, (ushort)value);
            }
            else if (address >= 0x70e000 && address <= 0x70ffff)
            {
                int i11 = 1;
                //int offset = (add - 0x70e000) / 2;
                //cps2_objram2_w(offset, (ushort)value);
            }
            else if (address >= 0x800100 && address <= 0x80013f)
            {
                cps1_cps_a_w((address & 0x3f) / 2, (ushort)value);
            }
            else if (address >= 0x800140 && address <= 0x80017f)
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
            else if (address >= 0x804100 && address <= 0x80413f)
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
            else if (address >= 0xff0000 && address + 1 <= 0xffffef)
            {
                Memory.mainram[address & 0xffff] = (byte)(value >> 8);
                Memory.mainram[(address & 0xffff) + 1] = (byte)value;
            }
            else if (address >= 0xfffff0 && address + 1 <= 0xfffffb)
            {
                int offset = (address - 0xfffff0) / 2;
                cps2_output[offset] = (ushort)value;
            }
            else if (address >= 0xfffffc && address + 1 <= 0xffffff)
            {
                int i11 = 1;
            }
            else
            {
                int i11 = 1;
            }
        }
        public static void MC2WriteLong_dead(int address, int value)
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
            else if (address >= 0x618000 && address + 3 <= 0x619fff)
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
            else if (address >= 0x708000 && address + 3 <= 0x709fff)
            {
                int offset = (address - 0x708000) / 2;
                cps2_objram2_w(offset, (ushort)(value >> 16));
                cps2_objram2_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x70a000 && address + 3 <= 0x70bfff)
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
            else if (address >= 0x800100 && address <= 0x80013f)
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
            else if (address >= 0xff0000 && address + 3 <= 0xffffef)
            {
                Memory.mainram[(address & 0xffff)] = (byte)(value >> 24);
                Memory.mainram[(address & 0xffff) + 1] = (byte)(value >> 16);
                Memory.mainram[(address & 0xffff) + 2] = (byte)(value >> 8);
                Memory.mainram[(address & 0xffff) + 3] = (byte)(value);
            }
            else if (address >= 0xfffff0 && address + 3 <= 0xfffffb)
            {
                int offset = (address - 0xfffff0) / 2;
                cps2_output[offset] = (ushort)(value >> 16);
                cps2_output[offset + 1] = (ushort)value;
            }
            else if (address >= 0xfffffc && address + 3 <= 0xffffff)
            {
                int i11 = 1;
            }
            else
            {
                int i11 = 1;
            }
        }
    }
}
