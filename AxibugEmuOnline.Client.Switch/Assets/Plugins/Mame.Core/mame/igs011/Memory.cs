namespace MAME.Core
{
    public unsafe partial class IGS011
    {
        public static byte bkey0, bkey1, bkey2, bkey3, bkey4;
        public static byte bkey0_old, bkey1_old, bkey2_old, bkey3_old, bkey4_old;
        public static sbyte sbyte0, sbyte1, sbyte2, sbytec;
        public static sbyte sbyte0_old, sbyte1_old, sbyte2_old, sbytec_old;
        public static sbyte MReadOpByte_drgnwrld(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= prot1_addr + 8 && address <= prot1_addr + 9)
            {
                if (address % 2 == 0)
                {
                    result = (sbyte)(igs011_prot1_r() >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)igs011_prot1_r();
                }
            }
            else if (address >= 0 && address <= 0x7ffff)
            {
                result = (sbyte)Memory.mainrom[address];
            }
            return result;
        }
        public static sbyte MReadByte_drgnwrld(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= prot1_addr + 8 && address <= prot1_addr + 9)
            {
                if (address % 2 == 0)
                {
                    result = (sbyte)(igs011_prot1_r() >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)igs011_prot1_r();
                }
            }
            else if (address >= 0 && address <= 0x7ffff)
            {
                result = (sbyte)Memory.mainrom[address];
            }
            else if (address >= 0x100000 && address <= 0x103fff)
            {
                int offset = address - 0x100000;
                result = (sbyte)Generic.generic_nvram[offset];
            }
            else if (address >= 0x200000 && address <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(priority_ram[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)priority_ram[offset];
                }
            }
            else if (address >= 0x400000 && address <= 0x401fff)
            {
                int offset = (address - 0x400000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(paletteram16[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)paletteram16[offset];
                }
            }
            else if (address >= 0x500000 && address <= 0x500001)
            {
                if (address == 0x500001)
                {
                    result = sbytec;
                }
            }
            else if (address >= 0x600000 && address <= 0x600001)
            {
                //if (address == 0x600001)
                {
                    result = (sbyte)OKI6295.okim6295_status_0_lsb_r();
                }
            }
            else if (address >= 0x800002 && address <= 0x800003)
            {
                /*if (address == 0x800002)
                {
                    int i1 = 1;
                }
                else*/
                if (address == 0x800003)
                {
                    result = (sbyte)drgnwrld_igs003_r();
                }
            }
            else if (address >= 0xa88000 && address <= 0xa88001)
            {
                result = (sbyte)igs_3_dips_r();
            }
            return result;
        }
        public static short MReadOpWord_drgnwrld(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= prot1_addr + 8 && address + 1 <= prot1_addr + 9)
            {
                result = (short)igs011_prot1_r();
            }
            else if (address >= 0 && address + 1 <= 0x7ffff)
            {
                result = (short)(Memory.mainrom[address] * 0x100 + Memory.mainrom[address + 1]);
            }
            return result;
        }
        public static short MReadWord_drgnwrld(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= prot1_addr + 8 && address + 1 <= prot1_addr + 9)
            {
                result = (short)igs011_prot1_r();
            }
            else if (address >= 0 && address + 1 <= 0x7ffff)
            {
                result = (short)(Memory.mainrom[address] * 0x100 + Memory.mainrom[address + 1]);
            }
            else if (address >= 0x100000 && address + 1 <= 0x103fff)
            {
                int offset = address - 0x100000;
                result = (short)(Generic.generic_nvram[offset] * 0x100 + Generic.generic_nvram[offset + 1]);
            }
            else if (address >= 0x200000 && address + 1 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                result = (short)priority_ram[offset];
            }
            else if (address >= 0x400000 && address + 1 <= 0x401fff)
            {
                int offset = (address - 0x400000) / 2;
                result = (short)paletteram16[offset];
            }
            else if (address >= 0x500000 && address + 1 <= 0x500001)
            {
                /*if (Video.screenstate.frame_number >= 60&&Video.screenstate.frame_number<=61)
                {
                    result = (short)(0xfe);
                }
                else*/
                {
                    result = (short)((byte)sbytec);
                }
            }
            else if (address >= 0x600000 && address + 1 <= 0x600001)
            {
                result = (short)OKI6295.okim6295_status_0_lsb_r();
            }
            else if (address >= 0x800002 && address + 1 <= 0x800003)
            {
                result = (short)drgnwrld_igs003_r();
            }
            else if (address >= 0xa88000 && address + 1 <= 0xa88001)
            {
                result = (short)igs_3_dips_r();
            }
            return result;
        }
        public static int MReadOpLong_drgnwrld(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0 && address + 3 <= 0x7ffff)
            {
                result = Memory.mainrom[address] * 0x1000000 + Memory.mainrom[address + 1] * 0x10000 + Memory.mainrom[address + 2] * 0x100 + Memory.mainrom[address + 3];
            }
            return result;
        }
        public static int MReadLong_drgnwrld(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0 && address + 3 <= 0x7ffff)
            {
                result = Memory.mainrom[address] * 0x1000000 + Memory.mainrom[address + 1] * 0x10000 + Memory.mainrom[address + 2] * 0x100 + Memory.mainrom[address + 3];
            }
            else if (address >= 0x100000 && address + 3 <= 0x103fff)
            {
                int offset = address - 0x100000;
                result = Generic.generic_nvram[offset] * 0x1000000 + Generic.generic_nvram[offset + 1] * 0x10000 + Generic.generic_nvram[offset + 2] * 0x100 + Generic.generic_nvram[offset + 3];
            }
            else if (address >= 0x200000 && address + 3 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                result = priority_ram[offset] * 0x10000 + priority_ram[offset + 1];
            }
            else if (address >= 0x400000 && address + 3 <= 0x401fff)
            {
                int offset = (address - 0x400000) / 2;
                result = paletteram16[offset] * 0x10000 + paletteram16[offset + 1];
            }
            else
            {
                int i1 = 1;
            }
            return result;
        }
        public static void MWriteByte_drgnwrld(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= prot1_addr && address <= prot1_addr + 7)
            {
                int offset = (int)(address - prot1_addr);
                igs011_prot1_w1(offset, (byte)value);
            }
            else if (address >= 0x100000 && address <= 0x103fff)
            {
                int offset = address - 0x100000;
                Generic.generic_nvram[offset] = (byte)value;
            }
            else if (address >= 0x200000 && address <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                if ((address & 1) == 0)
                {
                    priority_ram[offset] = (ushort)((value << 8) | (priority_ram[offset] & 0xff));
                }
                else if ((address & 1) == 1)
                {
                    priority_ram[offset] = (ushort)((priority_ram[offset] & 0xff00) | (byte)value);
                }
            }
            else if (address >= 0x400000 && address <= 0x401fff)
            {
                int offset = address - 0x400000;
                igs011_palette(offset, (byte)value);
            }
            else if (address >= 0x600000 && address <= 0x600001)
            {
                if (address == 0x600001)
                {
                    OKI6295.okim6295_data_0_lsb_w((byte)value);
                }
            }
            else if (address >= 0x700000 && address <= 0x700001)
            {
                if (address == 0x700001)
                {
                    igs_YM3812_control_port_0_w((byte)value);
                }
            }
            else if (address >= 0x700002 && address <= 0x700003)
            {
                if (address == 0x700003)
                {
                    igs_YM3812_write_port_0_w((byte)value);
                }
            }
            else if (address >= 0x800000 && address <= 0x800003)
            {
                int offset = address - 0x800000;
                drgnwrld_igs003_w(offset, (byte)value);
            }
            else if (address >= 0xa20000 && address <= 0xa20001)
            {
                int offset = address - 0xa20000;
                igs011_priority_w(offset, (byte)value);
            }
            else if (address >= 0xa40000 && address <= 0xa40001)
            {
                //igs_dips_w((ushort)value);
            }
            else if (address >= 0xa50000 && address <= 0xa50001)
            {
                int i1 = 1;
            }
            else if (address >= 0xa58000 && address <= 0xa58001)
            {
                int offset = address - 0xa58000;
                igs011_blit_x_w(offset, (byte)value);
            }
            else if (address >= 0xa58800 && address <= 0xa58801)
            {
                int offset = address - 0xa58800;
                igs011_blit_y_w(offset, (byte)value);
            }
            else if (address >= 0xa59000 && address <= 0xa59001)
            {
                int offset = address - 0xa59000;
                igs011_blit_w_w(offset, (byte)value);
            }
            else if (address >= 0xa59800 && address <= 0xa59801)
            {
                int offset = address - 0xa59800;
                igs011_blit_h_w(offset, (byte)value);
            }
            else if (address >= 0xa5a000 && address <= 0xa5a001)
            {
                int offset = address - 0xa5a000;
                igs011_blit_gfx_lo_w(offset, (byte)value);
            }
            else if (address >= 0xa5a800 && address <= 0xa5a801)
            {
                int offset = address - 0xa5a800;
                igs011_blit_gfx_hi_w(offset, (byte)value);
            }
            else if (address >= 0xa5b000 && address <= 0xa5b001)
            {
                igs011_blit_flags_w((ushort)value);
            }
            else if (address >= 0xa5b800 && address <= 0xa5b801)
            {
                int offset = address - 0xa5b800;
                igs011_blit_pen_w(offset, (byte)value);
            }
            else if (address >= 0xa5c000 && address <= 0xa5c001)
            {
                int offset = address - 0xa5c000;
                igs011_blit_depth_w(offset, (byte)value);
            }
        }
        public static void MWriteWord_drgnwrld(int address, short value)
        {
            address &= 0xffffff;
            if (address >= prot1_addr && address + 1 <= prot1_addr + 7)
            {
                int offset = (int)(address - prot1_addr);
                igs011_prot1_w(offset, (ushort)value);
            }
            else if (address >= 0x100000 && address + 1 <= 0x103fff)
            {
                int offset = address - 0x100000;
                Generic.generic_nvram[offset] = (byte)(value >> 8);
                Generic.generic_nvram[offset + 1] = (byte)value;
            }
            else if (address >= 0x200000 && address + 1 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                priority_ram[offset] = (ushort)value;
            }
            else if (address >= 0x400000 && address + 1 <= 0x401fff)
            {
                int offset = (address - 0x400000) / 2;
                igs011_palette(offset, (ushort)value);
            }
            else if (address >= 0x600000 && address + 1 <= 0x600001)
            {
                OKI6295.okim6295_data_0_lsb_w((byte)value);
            }
            else if (address >= 0x700000 && address + 1 <= 0x700001)
            {
                igs_YM3812_control_port_0_w((byte)value);
            }
            else if (address >= 0x700002 && address + 1 <= 0x700003)
            {
                igs_YM3812_write_port_0_w((byte)value);
            }
            else if (address >= 0x800000 && address + 1 <= 0x800003)
            {
                int offset = (address - 0x800000) / 2;
                drgnwrld_igs003_w(offset, (ushort)value);
            }
            else if (address >= 0xa20000 && address + 1 <= 0xa20001)
            {
                igs011_priority_w((ushort)value);
            }
            else if (address >= 0xa40000 && address + 1 <= 0xa40001)
            {
                igs_dips_w((ushort)value);
            }
            else if (address >= 0xa50000 && address + 1 <= 0xa50001)
            {
                igs011_prot_addr_w((ushort)value);
            }
            else if (address >= 0xa58000 && address + 1 <= 0xa58001)
            {
                igs011_blit_x_w((ushort)value);
            }
            else if (address >= 0xa58800 && address + 1 <= 0xa58801)
            {
                igs011_blit_y_w((ushort)value);
            }
            else if (address >= 0xa59000 && address + 1 <= 0xa59001)
            {
                igs011_blit_w_w((ushort)value);
            }
            else if (address >= 0xa59800 && address + 1 <= 0xa59801)
            {
                igs011_blit_h_w((ushort)value);
            }
            else if (address >= 0xa5a000 && address + 1 <= 0xa5a001)
            {
                igs011_blit_gfx_lo_w((ushort)value);
            }
            else if (address >= 0xa5a800 && address + 1 <= 0xa5a801)
            {
                igs011_blit_gfx_hi_w((ushort)value);
            }
            else if (address >= 0xa5b000 && address + 1 <= 0xa5b001)
            {
                igs011_blit_flags_w((ushort)value);
            }
            else if (address >= 0xa5b800 && address + 1 <= 0xa5b801)
            {
                igs011_blit_pen_w((ushort)value);
            }
            else if (address >= 0xa5c000 && address + 1 <= 0xa5c001)
            {
                igs011_blit_depth_w((ushort)value);
            }
        }
        public static void MWriteLong_drgnwrld(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x100000 && address + 3 <= 0x103fff)
            {
                int offset = address - 0x100000;
                Generic.generic_nvram[offset] = (byte)(value >> 24);
                Generic.generic_nvram[offset + 1] = (byte)(value >> 16);
                Generic.generic_nvram[offset + 2] = (byte)(value >> 8);
                Generic.generic_nvram[offset + 3] = (byte)value;
            }
            else if (address >= 0x200000 && address + 3 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                priority_ram[offset] = (ushort)(value >> 16);
                priority_ram[offset + 1] = (ushort)value;
            }
            else if (address >= 0x400000 && address + 3 <= 0x401fff)
            {
                int offset = (address - 0x400000) / 2;
                igs011_palette(offset, (ushort)(value >> 16));
                igs011_palette(offset + 1, (ushort)value);
            }
            else if (address >= 0x800000 && address + 3 <= 0x800003)
            {
                drgnwrld_igs003_w(0, (ushort)(value >> 16));
                drgnwrld_igs003_w(1, (ushort)value);
            }
        }
    }
}
