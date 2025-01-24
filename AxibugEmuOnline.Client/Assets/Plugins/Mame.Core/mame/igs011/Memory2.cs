namespace MAME.Core
{
    public unsafe partial class IGS011
    {
        public static sbyte MReadByte_drgnwrld_igs012(int address)
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
            else if (address >= 0 && address <= 0x1ffff)
            {
                int address2 = address & ~0x1c000;
                if (address2 >= 0x001610 && address2 <= 0x00161f)
                {
                    if (address2 % 2 == 1)
                    {
                        result = (sbyte)igs012_prot_r();
                    }
                }
                else if (address2 >= 0x001660 && address2 <= 0x00166f)
                {
                    if (address2 % 2 == 1)
                    {
                        result = (sbyte)igs012_prot_r();
                    }
                }
                else if (address >= 0x00d4c0 && address <= 0x00d4ff)
                {
                    if (address2 % 2 == 0)
                    {
                        result = (sbyte)(drgnwrldv20j_igs011_prot2_r() >> 8);
                    }
                }
                else
                {
                    result = MReadByte_drgnwrld(address);
                }
            }
            else
            {
                result = MReadByte_drgnwrld(address);
            }
            return result;
        }
        public static short MReadWord_drgnwrld_igs012(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= prot1_addr + 8 && address + 1 <= prot1_addr + 9)
            {
                result = (short)igs011_prot1_r();
            }
            else if (address >= 0 && address + 1 <= 0x1ffff)
            {
                int address2 = address & ~0x1c000;
                if (address2 >= 0x001610 && address2 + 1 <= 0x00161f)
                {
                    result = (short)igs012_prot_r();
                }
                else if (address2 >= 0x001660 && address2 + 1 <= 0x00166f)
                {
                    result = (short)igs012_prot_r();
                }
                else if (address >= 0x00d4c0 && address + 1 <= 0x00d4ff)
                {
                    result = (short)drgnwrldv20j_igs011_prot2_r();
                }
                else
                {
                    result = MReadWord_drgnwrld(address);
                }
            }
            else
            {
                result = MReadWord_drgnwrld(address);
            }
            return result;
        }
        public static int MReadLong_drgnwrld_igs012(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0 && address + 3 <= 0x1ffff)
            {
                int address2 = address & ~0x1c000;
                if (address2 >= 0x001610 && address2 + 3 <= 0x00161f)
                {
                    int i1 = 1;
                }
                else if (address2 >= 0x001660 && address2 + 3 <= 0x00166f)
                {
                    int i1 = 1;
                }
                else if (address >= 0x00d4c0 && address + 3 <= 0x00d4ff)
                {
                    int i1 = 1;
                }
                else
                {
                    result = MReadLong_drgnwrld(address);
                }
            }
            else
            {
                result = MReadLong_drgnwrld(address);
            }
            return result;
        }
        public static void MWriteByte_drgnwrld_igs012(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= prot1_addr && address <= prot1_addr + 7)
            {
                int offset = (int)(address - prot1_addr);
                igs011_prot1_w1(offset, (byte)value);
            }
            else if (address >= 0 && address <= 0x1ffff)
            {
                int address2 = address & ~0x1c000;
                if (address2 >= 0x001600 && address2 <= 0x00160f)
                {
                    igs012_prot_swap_w((byte)value);
                }
                else if (address2 >= 0x001620 && address2 <= 0x00162f)
                {
                    igs012_prot_dec_inc_w((byte)value);
                }
                else if (address2 >= 0x001630 && address2 <= 0x00163f)
                {
                    igs012_prot_inc_w((byte)value);
                }
                else if (address2 >= 0x001640 && address2 <= 0x00164f)
                {
                    igs012_prot_copy_w((byte)value);
                }
                else if (address2 >= 0x001650 && address2 <= 0x00165f)
                {
                    igs012_prot_dec_copy_w((byte)value);
                }
                else if (address2 >= 0x001670 && address2 <= 0x00167f)
                {
                    igs012_prot_mode_w((byte)value);
                }
                else if (address >= 0x00d400 && address <= 0x00d43f)
                {
                    igs011_prot2_dec_w();
                }
                else if (address >= 0x00d440 && address <= 0x00d47f)
                {
                    drgnwrld_igs011_prot2_swap_w();
                }
                else if (address >= 0x00d480 && address <= 0x00d4bf)
                {
                    igs011_prot2_reset_w();
                }
                else
                {
                    MWriteByte_drgnwrld(address, value);
                }
            }
            else if (address >= 0x902000 && address <= 0x902fff)
            {
                igs012_prot_reset_w();
            }
            else
            {
                MWriteByte_drgnwrld(address, value);
            }
        }
        public static void MWriteWord_drgnwrld_igs012(int address, short value)
        {
            address &= 0xffffff;
            if (address >= prot1_addr && address + 1 <= prot1_addr + 7)
            {
                int offset = (int)(address - prot1_addr);
                igs011_prot1_w(offset, (ushort)value);
            }
            else if (address >= 0 && address <= 0x1ffff)
            {
                int address2 = address & ~0x1c000;
                if (address2 >= 0x001600 && address2 + 1 <= 0x00160f)
                {
                    igs012_prot_swap_w((ushort)value);
                }
                else if (address2 >= 0x001620 && address2 + 1 <= 0x00162f)
                {
                    igs012_prot_dec_inc_w((ushort)value);
                }
                else if (address2 >= 0x001630 && address2 + 1 <= 0x00163f)
                {
                    igs012_prot_inc_w((ushort)value);
                }
                else if (address2 >= 0x001640 && address2 + 1 <= 0x00164f)
                {
                    igs012_prot_copy_w((ushort)value);
                }
                else if (address2 >= 0x001650 && address2 + 1 <= 0x00165f)
                {
                    igs012_prot_dec_copy_w((ushort)value);
                }
                else if (address2 >= 0x001670 && address2 + 1 <= 0x00167f)
                {
                    igs012_prot_mode_w((ushort)value);
                }
                else if (address >= 0x00d400 && address + 1 <= 0x00d43f)
                {
                    igs011_prot2_dec_w();
                }
                else if (address >= 0x00d440 && address + 1 <= 0x00d47f)
                {
                    drgnwrld_igs011_prot2_swap_w();
                }
                else if (address >= 0x00d480 && address + 1 <= 0x00d4bf)
                {
                    igs011_prot2_reset_w();
                }
                else
                {
                    MWriteWord_drgnwrld(address, value);
                }
            }
            else if (address >= 0x902000 && address + 1 <= 0x902fff)
            {
                igs012_prot_reset_w();
            }
            else
            {
                MWriteWord_drgnwrld(address, value);
            }
        }
        public static void MWriteLong_drgnwrld_igs012(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0 && address <= 0x1ffff)
            {
                int address2 = address & ~0x1c000;
                if (address2 >= 0x001600 && address2 + 3 <= 0x00160f)
                {
                    int i1 = 1;
                }
                else if (address2 >= 0x001620 && address2 + 3 <= 0x00162f)
                {
                    int i1 = 1;
                }
                else if (address2 >= 0x001630 && address2 + 3 <= 0x00163f)
                {
                    int i1 = 1;
                }
                else if (address2 >= 0x001640 && address2 + 3 <= 0x00164f)
                {
                    int i1 = 1;
                }
                else if (address2 >= 0x001650 && address2 + 3 <= 0x00165f)
                {
                    int i1 = 1;
                }
                else if (address2 >= 0x001670 && address2 + 3 <= 0x00167f)
                {
                    int i1 = 1;
                }
                else if (address >= 0x00d400 && address + 3 <= 0x00d43f)
                {
                    int i1 = 1;
                }
                else if (address >= 0x00d440 && address + 3 <= 0x00d47f)
                {
                    int i1 = 1;
                }
                else if (address >= 0x00d480 && address + 3 <= 0x00d4bf)
                {
                    int i1 = 1;
                }
                else
                {
                    MWriteLong_drgnwrld(address, value);
                }
            }
            else if (address >= 0x902000 && address + 3 <= 0x902fff)
            {
                int i1 = 1;
            }
            else
            {
                MWriteLong_drgnwrld(address, value);
            }
        }
        public static short MReadWord_drgnwrld_igs012_drgnwrldv21(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= prot1_addr + 8 && address + 1 <= prot1_addr + 9)
            {
                result = (short)igs011_prot1_r();
            }
            else if (address >= 0 && address + 1 <= 0x1ffff)
            {
                int address2 = address & ~0x1c000;
                if (address2 >= 0x001610 && address2 + 1 <= 0x00161f)
                {
                    result = (short)igs012_prot_r();
                }
                else if (address2 >= 0x001660 && address2 + 1 <= 0x00166f)
                {
                    result = (short)igs012_prot_r();
                }
                else if (address >= 0x00d4c0 && address + 1 <= 0x00d4ff)
                {
                    result = (short)drgnwrldv21_igs011_prot2_r();
                }
                else
                {
                    result = MReadWord_drgnwrld(address);
                }
            }
            else
            {
                result = MReadWord_drgnwrld(address);
            }
            return result;
        }
        public static sbyte MReadByte_lhb(int address)
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
            else if (address >= 0x010600 && address <= 0x0107ff)
            {
                if (address % 2 == 0)
                {
                    result = (sbyte)(lhb_igs011_prot2_r() >> 8);
                }
            }
            else if (address >= 0x000000 && address <= 0x07ffff)
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
            else if (address >= 0x300000 && address <= 0x3fffff)
            {
                int offset = address - 0x300000;
                if (address % 2 == 0)
                {
                    result = (sbyte)igs011_layers_r1(offset / 2);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)igs011_layers_r2(offset / 2);
                }
            }
            else if (address >= 0x600000 && address <= 0x600001)
            {
                //if (address == 0x600001)
                {
                    result = (sbyte)OKI6295.okim6295_status_0_lsb_r();
                }
            }
            else if (address >= 0x700000 && address <= 0x700001)
            {
                if (address == 0x700001)
                {
                    result = sbytec;
                }
            }
            else if (address >= 0x700002 && address <= 0x700005)
            {
                int offset = (address - 0x700002) / 2;
                if (address % 2 == 0)
                {
                    int i1 = 1;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)lhb_inputs_r(offset);
                }
            }
            else if (address >= 0x888000 && address <= 0x888001)
            {
                if (address % 2 == 0)
                {
                    result = 0;
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)igs_5_dips_r();
                }
            }
            return result;
        }
        public static short MReadOpWord_lhb(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= prot1_addr + 8 && address + 1 <= prot1_addr + 9)
            {
                result = (short)igs011_prot1_r();
            }
            else if (address >= 0x010600 && address + 1 <= 0x0107ff)
            {
                result = (short)lhb_igs011_prot2_r();
            }
            else if (address >= 0 && address + 1 <= 0x7ffff)
            {
                result = (short)(Memory.mainrom[address] * 0x100 + Memory.mainrom[address + 1]);
            }
            return result;
        }
        public static short MReadWord_lhb(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= prot1_addr + 8 && address + 1 <= prot1_addr + 9)
            {
                result = (short)igs011_prot1_r();
            }
            else if (address >= 0x010600 && address + 1 <= 0x0107ff)
            {
                result = (short)lhb_igs011_prot2_r();
            }
            else if (address >= 0x000000 && address + 1 <= 0x07ffff)
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
            else if (address >= 0x300000 && address + 1 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                result = (short)igs011_layers_r(offset);
            }
            else if (address >= 0x600000 && address + 1 <= 0x600001)
            {
                result = (short)OKI6295.okim6295_status_0_lsb_r();
            }
            else if (address >= 0x700000 && address + 1 <= 0x700001)
            {
                result = (short)((byte)sbytec);
            }
            else if (address >= 0x700002 && address + 1 <= 0x700005)
            {
                int offset = (address - 0x700002) / 2;
                result = (short)lhb_inputs_r(offset);
            }
            else if (address >= 0x888000 && address + 1 <= 0x888001)
            {
                result = (short)((byte)igs_5_dips_r());
            }
            return result;
        }
        public static int MReadLong_lhb(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x000000 && address + 3 <= 0x07ffff)
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
            else if (address >= 0x300000 && address + 3 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                result = igs011_layers_r(offset) * 0x10000 + igs011_layers_r(offset + 1);
            }
            else if (address >= 0x700002 && address + 3 <= 0x700005)
            {
                int offset = (address - 0x700002) / 2;
                result = lhb_inputs_r(offset) * 0x10000 + lhb_inputs_r(offset + 1);
            }
            return result;
        }
        public static void MWriteByte_lhb(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= prot1_addr && address <= prot1_addr + 7)
            {
                int offset = (int)(address - prot1_addr);
                igs011_prot1_w1(offset, (byte)value);
            }
            else if (address >= 0x010000 && address <= 0x010001)
            {
                if (address % 2 == 0)
                {
                    lhb_okibank_w((byte)value);
                }
            }
            else if (address >= 0x010200 && address <= 0x0103ff)
            {
                igs011_prot2_inc_w();
            }
            else if (address >= 0x010400 && address <= 0x0105ff)
            {
                int offset = (address - 0x010400) / 2;
                lhb_igs011_prot2_swap_w(offset);
            }
            else if (address >= 0x100000 && address <= 0x103fff)
            {
                int offset = address - 0x100000;
                Generic.generic_nvram[offset] = (byte)value;
            }
            else if (address >= 0x200000 && address <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                if (address % 2 == 0)
                {
                    priority_ram[offset] = (ushort)((value << 8) | (priority_ram[offset] & 0xff));
                }
                else if (address % 2 == 1)
                {
                    priority_ram[offset] = (ushort)((priority_ram[offset] & 0xff00) | (byte)value);
                }
            }
            else if (address >= 0x300000 && address <= 0x3fffff)
            {
                int offset = address - 0x300000;
                igs011_layers_w(offset, (byte)value);
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
            else if (address >= 0x700002 && address <= 0x700003)
            {
                int offset = address - 0x700002;
                lhb_inputs_w(offset, (byte)value);
            }
            else if (address >= 0x820000 && address <= 0x820001)
            {
                int offset = address - 0x820000;
                igs011_priority_w(offset, (byte)value);
            }
            else if (address >= 0x838000 && address <= 0x838001)
            {
                int offset = address - 0x838000;
                lhb_irq_enable_w(offset, (byte)value);
            }
            else if (address >= 0x840000 && address <= 0x840001)
            {
                int offset = address - 0x840000;
                igs_dips_w(offset, (byte)value);
            }
            else if (address >= 0x850000 && address <= 0x850001)
            {
                igs011_prot_addr_w((ushort)value);
            }
            else if (address >= 0x858000 && address <= 0x858001)
            {
                int offset = address - 0x858000;
                igs011_blit_x_w(offset, (byte)value);
            }
            else if (address >= 0x858800 && address <= 0x858801)
            {
                int offset = address - 0x858800;
                igs011_blit_y_w(offset, (byte)value);
            }
            else if (address >= 0x859000 && address <= 0x859001)
            {
                int offset = address - 0x859000;
                igs011_blit_w_w(offset, (byte)value);
            }
            else if (address >= 0x859800 && address <= 0x859801)
            {
                int offset = address - 0x859800;
                igs011_blit_h_w(offset, (byte)value);
            }
            else if (address >= 0x85a000 && address <= 0x85a001)
            {
                int offset = address - 0x85a000;
                igs011_blit_gfx_lo_w(offset, (byte)value);
            }
            else if (address >= 0x85a800 && address <= 0x85a801)
            {
                int offset = address - 0x85a800;
                igs011_blit_gfx_hi_w(offset, (byte)value);
            }
            else if (address >= 0x85b000 && address <= 0x85b001)
            {
                int i1 = 1;
                //igs011_blit_flags_w((byte)value);
            }
            else if (address >= 0x85b800 && address <= 0x85b801)
            {
                int offset = address - 0x85b800;
                igs011_blit_pen_w(offset, (byte)value);
            }
            else if (address >= 0x85c000 && address <= 0x85c001)
            {
                int offset = address - 0x85c000;
                igs011_blit_depth_w(offset, (byte)value);
            }
        }
        public static void MWriteWord_lhb(int address, short value)
        {
            address &= 0xffffff;
            if (address >= prot1_addr && address + 1 <= prot1_addr + 7)
            {
                int offset = (int)(address - prot1_addr);
                igs011_prot1_w(offset, (ushort)value);
            }
            else if (address >= 0x010000 && address + 1 <= 0x010001)
            {
                lhb_okibank_w((ushort)value);
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
            else if (address >= 0x300000 && address + 1 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                igs011_layers_w(offset, (ushort)value);
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
            else if (address >= 0x700002 && address + 1 <= 0x700003)
            {
                lhb_inputs_w((ushort)value);
            }
            else if (address >= 0x820000 && address + 1 <= 0x820001)
            {
                igs011_priority_w((ushort)value);
            }
            else if (address >= 0x838000 && address + 1 <= 0x838001)
            {
                lhb_irq_enable_w((ushort)value);
            }
            else if (address >= 0x840000 && address + 1 <= 0x840001)
            {
                igs_dips_w((ushort)value);
            }
            else if (address >= 0x850000 && address <= 0x850001)
            {
                igs011_prot_addr_w((ushort)value);
            }
            else if (address >= 0x858000 && address + 1 <= 0x858001)
            {
                igs011_blit_x_w((ushort)value);
            }
            else if (address >= 0x858800 && address + 1 <= 0x858801)
            {
                igs011_blit_y_w((ushort)value);
            }
            else if (address >= 0x859000 && address + 1 <= 0x859001)
            {
                igs011_blit_w_w((ushort)value);
            }
            else if (address >= 0x859800 && address + 1 <= 0x859801)
            {
                igs011_blit_h_w((ushort)value);
            }
            else if (address >= 0x85a000 && address + 1 <= 0x85a001)
            {
                igs011_blit_gfx_lo_w((ushort)value);
            }
            else if (address >= 0x85a800 && address + 1 <= 0x85a801)
            {
                igs011_blit_gfx_hi_w((ushort)value);
            }
            else if (address >= 0x85b000 && address + 1 <= 0x85b001)
            {
                igs011_blit_flags_w((ushort)value);
            }
            else if (address >= 0x85b800 && address + 1 <= 0x85b801)
            {
                igs011_blit_pen_w((ushort)value);
            }
            else if (address >= 0x85c000 && address + 1 <= 0x85c001)
            {
                igs011_blit_depth_w((ushort)value);
            }
        }
        public static void MWriteLong_lhb(int address, int value)
        {
            address &= 0xffffff;
            if (address >= prot1_addr && address + 3 <= prot1_addr + 7)
            {
                int i1 = 1;
            }
            else if (address >= 0x100000 && address + 3 <= 0x103fff)
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
            else if (address >= 0x300000 && address + 3 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                igs011_layers_w(offset, (ushort)(value >> 16));
                igs011_layers_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x400000 && address + 3 <= 0x401fff)
            {
                int offset = (address - 0x400000) / 2;
                igs011_palette(offset, (ushort)(value >> 16));
                igs011_palette(offset + 1, (ushort)value);
            }
        }
        public static short MReadWord_dbc(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= prot1_addr + 8 && address + 1 <= prot1_addr + 9)
            {
                result = (short)igs011_prot1_r();
            }
            else if (address >= 0x010600 && address + 1 <= 0x0107ff)
            {
                result = (short)dbc_igs011_prot2_r();
            }
            else if (address >= 0x000000 && address + 1 <= 0x07ffff)
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
            else if (address >= 0x300000 && address + 1 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                result = (short)igs011_layers_r(offset);
            }
            else if (address >= 0x600000 && address + 1 <= 0x600001)
            {
                result = (short)OKI6295.okim6295_status_0_lsb_r();
            }
            else if (address >= 0x700000 && address + 1 <= 0x700001)
            {
                result = (short)((byte)sbytec);
            }
            else if (address >= 0x700002 && address + 1 <= 0x700005)
            {
                int offset = (address - 0x700002) / 2;
                result = (short)lhb_inputs_r(offset);
            }
            else if (address >= 0x888000 && address + 1 <= 0x888001)
            {
                result = (short)((byte)igs_5_dips_r());
            }
            return result;
        }
        public static short MReadWord_ryukobou(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= prot1_addr + 8 && address + 1 <= prot1_addr + 9)
            {
                result = (short)igs011_prot1_r();
            }
            else if (address >= 0x010600 && address + 1 <= 0x0107ff)
            {
                result = (short)ryukobou_igs011_prot2_r();
            }
            else if (address >= 0x000000 && address + 1 <= 0x07ffff)
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
            else if (address >= 0x300000 && address + 1 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                result = (short)igs011_layers_r(offset);
            }
            else if (address >= 0x600000 && address + 1 <= 0x600001)
            {
                result = (short)OKI6295.okim6295_status_0_lsb_r();
            }
            else if (address >= 0x700000 && address + 1 <= 0x700001)
            {
                result = (short)((byte)sbytec);
            }
            else if (address >= 0x700002 && address + 1 <= 0x700005)
            {
                int offset = (address - 0x700002) / 2;
                result = (short)lhb_inputs_r(offset);
            }
            else if (address >= 0x888000 && address + 1 <= 0x888001)
            {
                result = (short)((byte)igs_5_dips_r());
            }
            return result;
        }
        public static sbyte MReadOpByte_lhb2(int address)
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
            else if (address >= 0x020400 && address <= 0x0205ff)
            {
                if (address % 2 == 1)
                {
                    result = (sbyte)lhb2_igs011_prot2_r();
                }
            }
            else if (address >= 0x000000 && address <= 0x07ffff)
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
        public static sbyte MReadByte_lhb2(int address)
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
            else if (address >= 0x020400 && address <= 0x0205ff)
            {
                if (address % 2 == 1)
                {
                    result = (sbyte)lhb2_igs011_prot2_r();
                }
            }
            else if (address >= 0x000000 && address <= 0x07ffff)
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
                result = (sbyte)Generic.generic_nvram[offset];
            }
            else if (address >= 0x200000 && address <= 0x200001)
            {
                result = (sbyte)OKI6295.okim6295_status_0_lsb_r();
            }
            else if (address >= 0x208002 && address <= 0x208003)
            {
                if (address % 2 == 1)
                {
                    result = (sbyte)lhb2_igs003_r();
                }
            }
            else if (address >= 0x20c000 && address <= 0x20cfff)
            {
                int offset = (address - 0x20c000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(priority_ram[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)priority_ram[offset];
                }
            }
            else if (address >= 0x210000 && address <= 0x211fff)
            {
                int offset = (address - 0x210000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(paletteram16[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)paletteram16[offset];
                }
            }
            else if (address >= 0x214000 && address <= 0x214001)
            {
                if (address % 2 == 1)
                {
                    result = sbytec;
                }
            }
            else if (address >= 0x300000 && address <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)igs011_layers_r1(offset / 2);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)igs011_layers_r2(offset / 2);
                }
            }
            else if (address >= 0xa88000 && address <= 0xa88001)
            {
                result = (sbyte)igs_3_dips_r();
            }
            return result;
        }
        public static short MReadOpWord_lhb2(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= prot1_addr + 8 && address + 1 <= prot1_addr + 9)
            {
                result = (short)igs011_prot1_r();
            }
            else if (address >= 0x020400 && address + 1 <= 0x0205ff)
            {
                result = (short)lhb2_igs011_prot2_r();
            }
            else if (address >= 0x000000 && address + 1 <= 0x07ffff)
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
        public static short MReadWord_lhb2(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= prot1_addr + 8 && address + 1 <= prot1_addr + 9)
            {
                result = (short)igs011_prot1_r();
            }
            else if (address >= 0x020400 && address + 1 <= 0x0205ff)
            {
                result = (short)lhb2_igs011_prot2_r();
            }
            else if (address >= 0x000000 && address + 1 <= 0x07ffff)
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
                result = (short)(Generic.generic_nvram[offset] * 0x100 + Generic.generic_nvram[offset + 1]);
            }
            else if (address >= 0x200000 && address + 1 <= 0x200001)
            {
                result = (short)OKI6295.okim6295_status_0_lsb_r();
            }
            else if (address >= 0x208002 && address + 1 <= 0x208003)
            {
                result = (short)lhb2_igs003_r();
            }
            else if (address >= 0x20c000 && address + 1 <= 0x20cfff)
            {
                int offset = (address - 0x20c000) / 2;
                result = (short)priority_ram[offset];
            }
            else if (address >= 0x210000 && address + 1 <= 0x211fff)
            {
                int offset = (address - 0x210000) / 2;
                result = (short)paletteram16[offset];
            }
            else if (address >= 0x214000 && address + 1 <= 0x214001)
            {
                result = (short)((byte)sbytec);
            }
            else if (address >= 0x300000 && address + 1 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                result = (short)igs011_layers_r(offset);
            }
            else if (address >= 0xa88000 && address + 1 <= 0xa88001)
            {
                result = (short)igs_3_dips_r();
            }
            return result;
        }
        public static int MReadOpLong_lhb2(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x000000 && address + 3 <= 0x07ffff)
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
        public static int MReadLong_lhb2(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x000000 && address + 3 <= 0x07ffff)
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
                result = Generic.generic_nvram[offset] * 0x1000000 + Generic.generic_nvram[offset + 1] * 0x10000 + Generic.generic_nvram[offset + 2] * 0x100 + Generic.generic_nvram[offset + 3];
            }
            else if (address >= 0x20c000 && address + 3 <= 0x20cfff)
            {
                int offset = (address - 0x20c000) / 2;
                result = priority_ram[offset] * 0x10000 + priority_ram[offset + 1];
            }
            else if (address >= 0x210000 && address + 3 <= 0x211fff)
            {
                int offset = (address - 0x210000) / 2;
                result = paletteram16[offset] * 0x10000 + paletteram16[offset + 1];
            }
            else if (address >= 0x300000 && address + 3 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                result = igs011_layers_r(offset) * 0x10000 + igs011_layers_r(offset + 1);
            }
            return result;
        }
        public static void MWriteByte_lhb2(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= prot1_addr && address <= prot1_addr + 7)
            {
                int offset = (int)(address - prot1_addr);
                igs011_prot1_w1(offset, (byte)value);
            }
            else if (address >= 0x020000 && address <= 0x0201ff)
            {
                igs011_prot2_inc_w();
            }
            else if (address >= 0x020200 && address <= 0x0203ff)
            {
                int offset = (address - 0x020200) / 2;
                lhb_igs011_prot2_swap_w(offset);
            }
            else if (address >= 0x020600 && address <= 0x0207ff)
            {
                igs011_prot2_reset_w();
            }
            else if (address >= 0x100000 && address <= 0x103fff)
            {
                int offset = address - 0x100000;
                Generic.generic_nvram[offset] = (byte)value;
            }
            else if (address >= 0x200000 && address <= 0x200001)
            {
                if (address % 2 == 1)
                {
                    OKI6295.okim6295_data_0_lsb_w((byte)value);
                }
            }
            else if (address >= 0x204000 && address <= 0x204003)
            {
                int offset = (address - 0x204000) / 2;
                if (address % 2 == 1)
                {
                    YM2413.ym2413_write(offset, value);
                }
            }
            else if (address >= 0x208000 && address <= 0x208003)
            {
                int offset = (address - 0x208000) / 2;
                if (address % 2 == 0)
                {
                    lhb2_igs003_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    lhb2_igs003_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x20c000 && address <= 0x20cfff)
            {
                int offset = (address - 0x20c000) / 2;
                if (address % 2 == 0)
                {
                    priority_ram[offset] = (ushort)((value << 8) | (priority_ram[offset] & 0xff));
                }
                else if (address % 2 == 1)
                {
                    priority_ram[offset] = (ushort)((priority_ram[offset] & 0xff00) | (byte)value);
                }
            }
            else if (address >= 0x210000 && address <= 0x211fff)
            {
                int offset = address - 0x210000;
                igs011_palette(offset, (byte)value);
            }
            else if (address >= 0x300000 && address <= 0x3fffff)
            {
                int offset = address - 0x300000;
                igs011_layers_w(offset, (byte)value);
            }
            else if (address >= 0xa20000 && address <= 0xa20001)
            {
                int offset = address - 0xa20000;
                igs011_priority_w(offset, (byte)value);
            }
            else if (address >= 0xa40000 && address <= 0xa40001)
            {
                int offset = address - 0xa40000;
                igs_dips_w(offset, (byte)value);
            }
            else if (address >= 0xa50000 && address <= 0xa50001)
            {
                igs011_prot_addr_w((ushort)value);
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
        public static void MWriteWord_lhb2(int address, short value)
        {
            address &= 0xffffff;
            if (address >= prot1_addr && address + 1 <= prot1_addr + 7)
            {
                int offset = (int)(address - prot1_addr);
                igs011_prot1_w(offset, (ushort)value);
            }
            else if (address >= 0x020000 && address + 1 <= 0x0201ff)
            {
                igs011_prot2_inc_w();
            }
            else if (address >= 0x020200 && address + 1 <= 0x0203ff)
            {
                int offset = (address - 0x020200) / 2;
                lhb_igs011_prot2_swap_w(offset);
            }
            else if (address >= 0x020600 && address + 1 <= 0x0207ff)
            {
                igs011_prot2_reset_w();
            }
            else if (address >= 0x100000 && address + 1 <= 0x103fff)
            {
                int offset = address - 0x100000;
                Generic.generic_nvram[offset] = (byte)(value >> 8);
                Generic.generic_nvram[offset + 1] = (byte)value;
            }
            else if (address >= 0x200000 && address + 1 <= 0x200001)
            {
                OKI6295.okim6295_data_0_lsb_w((byte)value);
            }
            else if (address >= 0x204000 && address + 1 <= 0x204003)
            {
                int offset = (address - 0x204000) / 2;
                YM2413.ym2413_write(offset, value);
            }
            else if (address >= 0x208000 && address + 1 <= 0x208003)
            {
                int offset = (address - 0x208000) / 2;
                lhb2_igs003_w(offset, (ushort)value);
            }
            else if (address >= 0x20c000 && address + 1 <= 0x20cfff)
            {
                int offset = (address - 0x20c000) / 2;
                priority_ram[offset] = (ushort)value;
            }
            else if (address >= 0x210000 && address + 1 <= 0x211fff)
            {
                int offset = (address - 0x210000) / 2;
                igs011_palette(offset, (ushort)value);
            }
            else if (address >= 0x300000 && address + 1 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                igs011_layers_w(offset, (ushort)value);
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
        public static void MWriteLong_lhb2(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x100000 && address + +3 <= 0x103fff)
            {
                int offset = address - 0x100000;
                Generic.generic_nvram[offset] = (byte)(value >> 24);
                Generic.generic_nvram[offset + 1] = (byte)(value >> 16);
                Generic.generic_nvram[offset + 2] = (byte)(value >> 8);
                Generic.generic_nvram[offset + 3] = (byte)value;
            }
            else if (address >= 0x20c000 && address + 3 <= 0x20cfff)
            {
                int offset = (address - 0x20c000) / 2;
                priority_ram[offset] = (ushort)(value >> 16);
                priority_ram[offset + 1] = (ushort)value;
            }
            else if (address >= 0x210000 && address + 3 <= 0x211fff)
            {
                int offset = (address - 0x210000) / 2;
                igs011_palette(offset, (ushort)(value >> 16));
                igs011_palette(offset + 1, (ushort)value);
            }
            else if (address >= 0x300000 && address + 3 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                igs011_layers_w(offset, (ushort)(value >> 16));
                igs011_layers_w(offset + 1, (ushort)value);
            }
        }
        public static sbyte MReadOpByte_xymg(int address)
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
            else if (address >= 0x000000 && address <= 0x07ffff)
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
        public static sbyte MReadByte_xymg(int address)
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
            else if (address >= 0x010600 && address <= 0x0107ff)
            {
                if (address % 2 == 0)
                {
                    result = (sbyte)(lhb2_igs011_prot2_r() >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)lhb2_igs011_prot2_r();
                }
            }
            else if (address >= 0x000000 && address <= 0x07ffff)
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
            else if (address >= 0x1f0000 && address <= 0x1f3fff)
            {
                int offset = address - 0x1f0000;
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
            else if (address >= 0x300000 && address <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)igs011_layers_r1(offset / 2);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)igs011_layers_r2(offset / 2);
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
            else if (address >= 0x600000 && address <= 0x600001)
            {
                result = (sbyte)OKI6295.okim6295_status_0_lsb_r();
            }
            else if (address >= 0x700002 && address <= 0x700003)
            {
                result = (sbyte)xymg_igs003_r();
            }
            else if (address >= 0x888000 && address <= 0x888001)
            {
                result = (sbyte)igs_3_dips_r();
            }
            return result;
        }
        public static short MReadOpWord_xymg(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= prot1_addr + 8 && address + 1 <= prot1_addr + 9)
            {
                result = (short)igs011_prot1_r();
            }
            else if (address >= 0x000000 && address + 1 <= 0x07ffff)
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
        public static short MReadWord_xymg(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= prot1_addr + 8 && address + 1 <= prot1_addr + 9)
            {
                result = (short)igs011_prot1_r();
            }
            else if (address >= 0x010600 && address + 1 <= 0x0107ff)
            {
                result = (short)igs011_prot1_r();
            }
            else if (address >= 0x000000 && address + 1 <= 0x07ffff)
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
            else if (address >= 0x1f0000 && address + 1 <= 0x1f3fff)
            {
                int offset = address - 0x1f0000;
                result = (short)(Generic.generic_nvram[offset] * 0x100 + Generic.generic_nvram[offset + 1]);
            }
            else if (address >= 0x200000 && address + 1 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                result = (short)priority_ram[offset];
            }
            else if (address >= 0x300000 && address + 1 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                result = (short)igs011_layers_r(offset);
            }
            else if (address >= 0x400000 && address + 1 <= 0x401fff)
            {
                int offset = (address - 0x400000) / 2;
                result = (short)paletteram16[offset];
            }
            else if (address >= 0x600000 && address + 1 <= 0x600001)
            {
                result = (short)OKI6295.okim6295_status_0_lsb_r();
            }
            else if (address >= 0x700002 && address + 1 <= 0x700003)
            {
                result = (short)xymg_igs003_r();
            }
            else if (address >= 0x888000 && address + 1 <= 0x888001)
            {
                result = (short)igs_3_dips_r();
            }
            return result;
        }
        public static int MReadOpLong_xymg(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address <= 0x010001)
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
        public static int MReadLong_xymg(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x000000 && address + 3 <= 0x07ffff)
            {
                if (address + 3 < Memory.mainromLength)
                {
                    int offset = (address - 0x010000) / 2;
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
            else if (address >= 0x1f0000 && address + 3 <= 0x1f3fff)
            {
                int offset = address - 0x1f0000;
                result = Generic.generic_nvram[offset] * 0x1000000 + Generic.generic_nvram[offset + 1] * 0x10000 + Generic.generic_nvram[offset + 2] * 0x100 + Generic.generic_nvram[offset + 3];
            }
            else if (address >= 0x200000 && address + 3 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                result = priority_ram[offset] * 0x10000 + priority_ram[offset + 1];
            }
            else if (address >= 0x300000 && address + 3 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                result = igs011_layers_r(offset) * 0x10000 + igs011_layers_r(offset + 1);
            }
            else if (address >= 0x400000 && address + 3 <= 0x401fff)
            {
                int offset = (address - 0x400000) / 2;
                result = paletteram16[offset] * 0x10000 + paletteram16[offset + 1];
            }
            return result;
        }
        public static void MWriteByte_xymg(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= prot1_addr && address <= prot1_addr + 7)
            {
                int offset = (int)(address - prot1_addr);
                igs011_prot1_w1(offset, (byte)value);
            }
            else if (address >= 0x010000 && address <= 0x010001)
            {
                if (address % 2 == 0)
                {
                    lhb_okibank_w((byte)value);
                }
            }
            else if (address >= 0x010200 && address <= 0x0103ff)
            {
                igs011_prot2_inc_w();
            }
            else if (address >= 0x010400 && address <= 0x0105ff)
            {
                int offset = (address - 0x010400) / 2;
                lhb_igs011_prot2_swap_w(offset);
            }
            else if (address >= 0x100000 && address <= 0x103fff)
            {
                int offset = address - 0x100000;
                Memory.mainram[offset] = (byte)(value);
            }
            else if (address >= 0x1f0000 && address <= 0x1f3fff)
            {
                int offset = address - 0x1f0000;
                Generic.generic_nvram[offset] = (byte)value;
            }
            else if (address >= 0x200000 && address <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                if (address % 2 == 0)
                {
                    priority_ram[offset] = (ushort)((value << 8) | (priority_ram[offset] & 0xff));
                }
                else if (address % 2 == 1)
                {
                    priority_ram[offset] = (ushort)((priority_ram[offset] & 0xff00) | (byte)value);
                }
            }
            else if (address >= 0x300000 && address <= 0x3fffff)
            {
                int offset = address - 0x300000;
                igs011_layers_w(offset, (byte)value);
            }
            else if (address >= 0x400000 && address <= 0x401fff)
            {
                int offset = address - 0x400000;
                igs011_palette(offset, (byte)value);
            }
            else if (address >= 0x600000 && address <= 0x600001)
            {
                if (address % 2 == 1)
                {
                    OKI6295.okim6295_data_0_lsb_w((byte)value);
                }
            }
            else if (address >= 0x700000 && address <= 0x700003)
            {
                int offset = (address - 0x700000) / 2;
                xymg_igs003_w(offset, (ushort)value);
            }
            else if (address >= 0x820000 && address <= 0x820001)
            {
                int offset = address - 0x820000;
                igs011_priority_w(offset, (byte)value);
            }
            else if (address >= 0x840000 && address <= 0x840001)
            {
                igs_dips_w((ushort)value);
            }
            else if (address >= 0x850000 && address <= 0x850001)
            {
                igs011_prot_addr_w((ushort)value);
            }
            else if (address >= 0x858000 && address <= 0x858001)
            {
                int offset = address - 0x858000;
                igs011_blit_x_w(offset, (byte)value);
            }
            else if (address >= 0x858800 && address <= 0x858801)
            {
                int offset = address - 0x858800;
                igs011_blit_y_w(offset, (byte)value);
            }
            else if (address >= 0x859000 && address <= 0x859001)
            {
                int offset = address - 0x859000;
                igs011_blit_w_w(offset, (byte)value);
            }
            else if (address >= 0x859800 && address <= 0x859801)
            {
                int offset = address - 0x859800;
                igs011_blit_h_w(offset, (byte)value);
            }
            else if (address >= 0x85a000 && address <= 0x85a001)
            {
                int offset = address - 0x85a000;
                igs011_blit_gfx_lo_w(offset, (byte)value);
            }
            else if (address >= 0x85a800 && address <= 0x85a801)
            {
                int offset = address - 0x85a800;
                igs011_blit_gfx_hi_w(offset, (byte)value);
            }
            else if (address >= 0x85b000 && address <= 0x85b001)
            {
                igs011_blit_flags_w((ushort)value);
            }
            else if (address >= 0x85b800 && address <= 0x85b801)
            {
                int offset = address - 0x85b800;
                igs011_blit_pen_w(offset, (byte)value);
            }
            else if (address >= 0x85c000 && address <= 0x85c001)
            {
                int offset = address - 0x85c000;
                igs011_blit_depth_w(offset, (byte)value);
            }
        }
        public static void MWriteWord_xymg(int address, short value)
        {
            address &= 0xffffff;
            if (address >= prot1_addr && address + 1 <= prot1_addr + 7)
            {
                int offset = (int)(address - prot1_addr);
                igs011_prot1_w(offset, (ushort)value);
            }
            else if (address >= 0x010000 && address + 1 <= 0x010001)
            {
                lhb_okibank_w((ushort)value);
            }
            else if (address >= 0x010200 && address + 1 <= 0x0103ff)
            {
                igs011_prot2_inc_w();
            }
            else if (address >= 0x010400 && address + 1 <= 0x0105ff)
            {
                int offset = (address - 0x010400) / 2;
                lhb_igs011_prot2_swap_w(offset);
            }
            else if (address >= 0x100000 && address + 1 <= 0x103fff)
            {
                int offset = address - 0x100000;
                Memory.mainram[offset] = (byte)(value >> 8);
                Memory.mainram[offset + 1] = (byte)value;
            }
            else if (address >= 0x1f0000 && address + 1 <= 0x1f3fff)
            {
                int offset = address - 0x1f0000;
                Generic.generic_nvram[offset] = (byte)(value >> 8);
                Generic.generic_nvram[offset + 1] = (byte)value;
            }
            else if (address >= 0x200000 && address + 1 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                priority_ram[offset] = (ushort)value;
            }
            else if (address >= 0x300000 && address + 1 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                igs011_layers_w(offset, (ushort)value);
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
            else if (address >= 0x700000 && address + 1 <= 0x700003)
            {
                int offset = (address - 0x700000) / 2;
                xymg_igs003_w(offset, (ushort)value);
            }
            else if (address >= 0x820000 && address + 1 <= 0x820001)
            {
                igs011_priority_w((ushort)value);
            }
            else if (address >= 0x840000 && address + 1 <= 0x840001)
            {
                igs_dips_w((ushort)value);
            }
            else if (address >= 0x850000 && address + 1 <= 0x850001)
            {
                igs011_prot_addr_w((ushort)value);
            }
            else if (address >= 0x858000 && address + 1 <= 0x858001)
            {
                igs011_blit_x_w((ushort)value);
            }
            else if (address >= 0x858800 && address + 1 <= 0x858801)
            {
                igs011_blit_y_w((ushort)value);
            }
            else if (address >= 0x859000 && address + 1 <= 0x859001)
            {
                igs011_blit_w_w((ushort)value);
            }
            else if (address >= 0x859800 && address + 1 <= 0x859801)
            {
                igs011_blit_h_w((ushort)value);
            }
            else if (address >= 0x85a000 && address + 1 <= 0x85a001)
            {
                igs011_blit_gfx_lo_w((ushort)value);
            }
            else if (address >= 0x85a800 && address + 1 <= 0x85a801)
            {
                igs011_blit_gfx_hi_w((ushort)value);
            }
            else if (address >= 0x85b000 && address + 1 <= 0x85b001)
            {
                igs011_blit_flags_w((ushort)value);
            }
            else if (address >= 0x85b800 && address + 1 <= 0x85b801)
            {
                igs011_blit_pen_w((ushort)value);
            }
            else if (address >= 0x85c000 && address + 1 <= 0x85c001)
            {
                igs011_blit_depth_w((ushort)value);
            }
        }
        public static void MWriteLong_xymg(int address, int value)
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
            else if (address >= 0x1f0000 && address + 3 <= 0x1f3fff)
            {
                int offset = address - 0x1f0000;
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
            else if (address >= 0x300000 && address + 3 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                igs011_layers_w(offset, (ushort)(value >> 16));
                igs011_layers_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x400000 && address + 3 <= 0x401fff)
            {
                int offset = (address - 0x400000) / 2;
                igs011_palette(offset, (ushort)(value >> 16));
                igs011_palette(offset + 1, (ushort)value);
            }
        }
        public static sbyte MReadOpByte_wlcc(int address)
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
            else if (address >= 0x000000 && address <= 0x07ffff)
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
        public static sbyte MReadByte_wlcc(int address)
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
            else if (address >= 0x518800 && address <= 0x5189ff)
            {
                result = (sbyte)igs011_prot2_reset_r();
            }
            else if (address >= 0x519000 && address <= 0x5195ff)
            {
                if (address % 2 == 0)
                {
                    result = (sbyte)(lhb2_igs011_prot2_r() >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)lhb2_igs011_prot2_r();
                }
            }
            else if (address >= 0x000000 && address <= 0x07ffff)
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
            else if (address >= 0x300000 && address <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)igs011_layers_r1(offset / 2);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)igs011_layers_r2(offset / 2);
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
            else if (address >= 0x520000 && address <= 0x520001)
            {
                if (address % 2 == 1)
                {
                    result = sbytec;
                }
            }
            else if (address >= 0x600000 && address <= 0x600001)
            {
                result = (sbyte)OKI6295.okim6295_status_0_lsb_r();
            }
            else if (address >= 0x800002 && address <= 0x800003)
            {
                if (address % 2 == 1)
                {
                    result = (sbyte)wlcc_igs003_r();
                }
            }
            else if (address >= 0xa88000 && address <= 0xa88001)
            {
                result = (sbyte)igs_4_dips_r();
            }
            return result;
        }
        public static short MReadOpWord_wlcc(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= prot1_addr + 8 && address + 1 <= prot1_addr + 9)
            {
                result = (short)igs011_prot1_r();
            }
            else if (address >= 0x000000 && address + 1 <= 0x07ffff)
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
        public static short MReadWord_wlcc(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= 0x518800 && address + 1 <= 0x5189ff)
            {
                result = (short)igs011_prot2_reset_r();
            }
            else if (address >= 0x519000 && address + 1 <= 0x5195ff)
            {
                result = (short)lhb2_igs011_prot2_r();
            }
            else if (address >= 0x000000 && address + 1 <= 0x07ffff)
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
                result = (short)(Generic.generic_nvram[offset] * 0x100 + Generic.generic_nvram[offset + 1]);
            }
            else if (address >= 0x200000 && address + 1 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                result = (short)priority_ram[offset];
            }
            else if (address >= 0x300000 && address + 1 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                result = (short)igs011_layers_r(offset);
            }
            else if (address >= 0x400000 && address + 1 <= 0x401fff)
            {
                int offset = (address - 0x400000) / 2;
                result = (short)paletteram16[offset];
            }
            else if (address >= 0x520000 && address + 1 <= 0x520001)
            {
                int offset = (address - 0x520000) / 2;
                result = (short)((byte)sbytec);
            }
            else if (address >= 0x600000 && address + 1 <= 0x600001)
            {
                int offset = (address - 0x600000) / 2;
                result = (short)OKI6295.okim6295_status_0_lsb_r();
            }
            else if (address >= 0x800002 && address + 1 <= 0x800003)
            {
                int offset = (address - 0x800002) / 2;
                result = (short)wlcc_igs003_r();
            }
            else if (address >= 0xa88000 && address + 1 <= 0xa88001)
            {
                int offset = (address - 0xa88000) / 2;
                result = (short)igs_4_dips_r();
            }
            return result;
        }
        public static int MReadOpLong_wlcc(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x000000 && address + 3 <= 0x07ffff)
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
        public static int MReadLong_wlcc(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x000000 && address + 3 <= 0x07ffff)
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
                int offset = (address - 0x100000) / 2;
                result = (int)(Generic.generic_nvram[offset] * 0x1000000 + Generic.generic_nvram[offset + 1] * 0x10000 + Generic.generic_nvram[offset + 2] * 0x100 + Generic.generic_nvram[offset + 3]);
            }
            else if (address >= 0x200000 && address + 3 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                result = (int)(priority_ram[offset] * 0x10000 + priority_ram[offset + 1]);
            }
            else if (address >= 0x300000 && address + 3 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                result = igs011_layers_r(offset) * 0x10000 + igs011_layers_r(offset + 1);
            }
            else if (address >= 0x400000 && address + 3 <= 0x401fff)
            {
                int offset = (address - 0x400000) / 2;
                result = (int)(paletteram16[offset] * 0x10000 + paletteram16[offset + 1]);
            }
            return result;
        }
        public static void MWriteByte_wlcc(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= prot1_addr && address <= prot1_addr + 7)
            {
                int offset = (int)(address - prot1_addr);
                igs011_prot1_w1(offset, (byte)value);
            }
            else if (address >= 0x518000 && address <= 0x5181ff)
            {
                igs011_prot2_inc_w();
            }
            else if (address >= 0x518200 && address <= 0x5183ff)
            {
                int offset = (address - 0x518200) / 2;
                wlcc_igs011_prot2_swap_w(offset);
            }
            else if (address >= 0x100000 && address <= 0x103fff)
            {
                int offset = address - 0x100000;
                Generic.generic_nvram[offset] = (byte)value;
            }
            else if (address >= 0x200000 && address <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                if (address % 2 == 0)
                {
                    priority_ram[offset] = (ushort)((value << 8) | (priority_ram[offset] & 0xff));
                }
                else if (address % 2 == 1)
                {
                    priority_ram[offset] = (ushort)((priority_ram[offset] & 0xff00) | (byte)value);
                }
            }
            else if (address >= 0x300000 && address <= 0x3fffff)
            {
                int offset = address - 0x300000;
                igs011_layers_w(offset, (byte)value);
            }
            else if (address >= 0x400000 && address <= 0x401fff)
            {
                int offset = address - 0x400000;
                igs011_palette(offset, (byte)value);
            }
            else if (address >= 0x600000 && address <= 0x600001)
            {
                if (address % 2 == 1)
                {
                    OKI6295.okim6295_data_0_lsb_w((byte)value);
                }
            }
            else if (address >= 0x800000 && address <= 0x800003)
            {
                int offset = (address - 0x800000) / 2;
                if (address % 2 == 0)
                {
                    wlcc_igs003_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    wlcc_igs003_w2(offset, (byte)value);
                }
            }
            else if (address >= 0xa20000 && address <= 0xa20001)
            {
                int offset = address - 0xa20000;
                igs011_priority_w(offset, (byte)value);
            }
            else if (address >= 0xa40000 && address <= 0xa40001)
            {
                igs_dips_w((ushort)value);
            }
            else if (address >= 0xa50000 && address <= 0xa50001)
            {
                igs011_prot_addr_w((ushort)value);
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
        public static void MWriteWord_wlcc(int address, short value)
        {
            address &= 0xffffff;
            if (address >= prot1_addr && address + 1 <= prot1_addr + 7)
            {
                int offset = (int)(address - prot1_addr);
                igs011_prot1_w(offset, (ushort)value);
            }
            else if (address >= 0x518000 && address + 1 <= 0x5181ff)
            {
                igs011_prot2_inc_w();
            }
            else if (address >= 0x518200 && address + 1 <= 0x5183ff)
            {
                int offset = (address - 0x518200) / 2;
                wlcc_igs011_prot2_swap_w(offset);
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
            else if (address >= 0x300000 && address + 1 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                igs011_layers_w(offset, (ushort)value);
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
            else if (address >= 0x800000 && address + 1 <= 0x800003)
            {
                int offset = (address - 0x800000) / 2;
                wlcc_igs003_w(offset, (ushort)value);
            }
            else if (address >= 0xa20000 && address + 1 <= 0xa20001)
            {
                igs011_priority_w((ushort)value);
            }
            else if (address >= 0xa40000 && address + 1 <= 0xa40001)
            {
                int offset = (address - 0xa40000) / 2;
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
        public static void MWriteLong_wlcc(int address, int value)
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
            else if (address >= 0x300000 && address + 3 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                igs011_layers_w(offset, (ushort)(value >> 16));
                igs011_layers_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x400000 && address + 3 <= 0x401fff)
            {
                int offset = (address - 0x400000) / 2;
                igs011_palette(offset, (ushort)(value >> 16));
                igs011_palette(offset + 1, (ushort)value);
            }
        }
        public static sbyte MReadOpByte_vbowl(int address)
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
            else if (address >= 0x000000 && address <= 0x07ffff)
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
        public static sbyte MReadByte_vbowl(int address)
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
            else if (address >= 0 && address <= 0x1ffff)
            {
                int address2 = address & ~0x1c000;
                if (address2 >= 0x001610 && address2 <= 0x00161f)
                {
                    if (address2 % 2 == 1)
                    {
                        result = (sbyte)igs012_prot_r();
                    }
                }
                else if (address2 >= 0x001660 && address2 <= 0x00166f)
                {
                    if (address2 % 2 == 1)
                    {
                        result = (sbyte)igs012_prot_r();
                    }
                }
                else if (address >= 0x00d4c0 && address <= 0x00d4ff)
                {
                    if (address2 % 2 == 0)
                    {
                        result = (sbyte)(drgnwrldv20j_igs011_prot2_r() >> 8);
                    }
                }
            }
            else if (address >= 0x50f600 && address <= 0x50f7ff)
            {
                int offset = (address - 0x50f600) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(vbowl_igs011_prot2_r() >> 8);
                }
            }
            else if (address >= 0x000000 && address <= 0x07ffff)
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
            else if (address >= 0x300000 && address <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)igs011_layers_r1(offset / 2);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)igs011_layers_r2(offset / 2);
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
            else if (address >= 0x520000 && address <= 0x520001)
            {
                if (address % 2 == 1)
                {
                    result = sbytec;
                }
            }
            else if (address >= 0x600000 && address <= 0x600007)
            {
                int offset = (address - 0x600000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)ics2115_0_word_r1(offset);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)ics2115_0_word_r2(offset);
                }
            }
            else if (address >= 0x700000 && address <= 0x700003)
            {
                int offset = (address - 0x700000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(vbowl_trackball[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)vbowl_trackball[offset];
                }
            }
            else if (address >= 0x800002 && address <= 0x800003)
            {
                if (address % 2 == 1)
                {
                    result = (sbyte)vbowl_igs003_r();
                }
            }
            else if (address >= 0xa80000 && address <= 0xa80001)
            {
                int offset = (address - 0xa80000) / 2;
                result = (sbyte)vbowl_unk_r();
            }
            else if (address >= 0xa88000 && address <= 0xa88001)
            {
                int offset = (address - 0xa88000) / 2;
                result = (sbyte)igs_4_dips_r();
            }
            else if (address >= 0xa90000 && address <= 0xa90001)
            {
                int offset = (address - 0xa90000) / 2;
                result = (sbyte)vbowl_unk_r();
            }
            else if (address >= 0xa98000 && address <= 0xa98001)
            {
                int offset = (address - 0xa98000) / 2;
                result = (sbyte)vbowl_unk_r();
            }
            return result;
        }
        public static short MReadOpWord_vbowl(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= prot1_addr + 8 && address + 1 <= prot1_addr + 9)
            {
                result = (short)igs011_prot1_r();
            }
            else if (address >= 0x000000 && address + 1 <= 0x07ffff)
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
        public static short MReadWord_vbowl(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= prot1_addr + 8 && address + 1 <= prot1_addr + 9)
            {
                result = (short)igs011_prot1_r();
            }
            else if (address >= 0 && address <= 0x1ffff)
            {
                int address2 = address & ~0x1c000;
                if (address2 >= 0x001610 && address2 + 1 <= 0x00161f)
                {
                    result = (short)igs012_prot_r();
                }
                else if (address2 >= 0x001660 && address2 + 1 <= 0x00166f)
                {
                    result = (short)igs012_prot_r();
                }
                else if (address >= 0x00d4c0 && address + 1 <= 0x00d4ff)
                {
                    result = (short)drgnwrldv20j_igs011_prot2_r();
                }
            }
            else if (address >= 0x50f600 && address + 1 <= 0x50f7ff)
            {
                result = (short)vbowl_igs011_prot2_r();
            }
            else if (address >= 0x000000 && address + 1 <= 0x07ffff)
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
                result = (short)(Generic.generic_nvram[offset] * 0x100 + Generic.generic_nvram[offset + 1]);
            }
            else if (address >= 0x200000 && address + 1 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                result = (short)priority_ram[offset];
            }
            else if (address >= 0x300000 && address + 1 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                result = (short)igs011_layers_r(offset);
            }
            else if (address >= 0x400000 && address + 1 <= 0x401fff)
            {
                int offset = (address - 0x400000) / 2;
                result = (short)paletteram16[offset];
            }
            else if (address >= 0x520000 && address + 1 <= 0x520001)
            {
                result = (short)((byte)sbytec);
            }
            else if (address >= 0x600000 && address + 1 <= 0x600007)
            {
                int offset = (address - 0x600000) / 2;
                result = (short)ics2115_0_word_r(offset);
            }
            else if (address >= 0x700000 && address + 1 <= 0x700003)
            {
                int offset = (address - 0x700000) / 2;
                result = (short)vbowl_trackball[offset];
            }
            else if (address >= 0x800002 && address + 1 <= 0x800003)
            {
                int offset = (address - 0x800002) / 2;
                result = (short)vbowl_igs003_r();
            }
            else if (address >= 0xa80000 && address + 1 <= 0xa80001)
            {
                int offset = (address - 0xa80000) / 2;
                result = (short)vbowl_unk_r();
            }
            else if (address >= 0xa88000 && address + 1 <= 0xa88001)
            {
                int offset = (address - 0xa88000) / 2;
                result = (short)igs_4_dips_r();
            }
            else if (address >= 0xa90000 && address + 1 <= 0xa90001)
            {
                int offset = (address - 0xa90000) / 2;
                result = (short)vbowl_unk_r();
            }
            else if (address >= 0xa98000 && address + 1 <= 0xa98001)
            {
                int offset = (address - 0xa98000) / 2;
                result = (short)vbowl_unk_r();
            }
            return result;
        }
        public static int MReadOpLong_vbowl(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x000000 && address + 3 <= 0x07ffff)
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
        public static int MReadLong_vbowl(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x000000 && address + 3 <= 0x07ffff)
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
                int offset = (address - 0x100000) / 2;
                result = (int)(Generic.generic_nvram[offset] * 0x1000000 + Generic.generic_nvram[offset + 1] * 0x10000 + Generic.generic_nvram[offset + 2] * 0x100 + Generic.generic_nvram[offset + 3]);
            }
            else if (address >= 0x200000 && address + 3 <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                result = (int)(priority_ram[offset] * 0x10000 + priority_ram[offset + 1]);
            }
            else if (address >= 0x300000 && address + 3 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                result = igs011_layers_r(offset) * 0x10000 + igs011_layers_r(offset + 1);
            }
            else if (address >= 0x400000 && address + 3 <= 0x401fff)
            {
                int offset = (address - 0x400000) / 2;
                result = (int)(paletteram16[offset] * 0x10000 + paletteram16[offset + 1]);
            }
            return result;
        }
        public static void MWriteByte_vbowl(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= prot1_addr && address <= prot1_addr + 7)
            {
                int offset = (int)(address - prot1_addr);
                igs011_prot1_w1(offset, (byte)value);
            }
            else if (address >= 0 && address <= 0x1ffff)
            {
                int address2 = address & ~0x1c000;
                if (address2 >= 0x001600 && address2 <= 0x00160f)
                {
                    igs012_prot_swap_w((ushort)value);
                }
                else if (address2 >= 0x001620 && address2 <= 0x00162f)
                {
                    igs012_prot_dec_inc_w((byte)value);
                }
                else if (address2 >= 0x001630 && address2 <= 0x00163f)
                {
                    igs012_prot_inc_w((ushort)value);
                }
                else if (address2 >= 0x001640 && address2 <= 0x00164f)
                {
                    igs012_prot_copy_w((ushort)value);
                }
                else if (address2 >= 0x001650 && address2 <= 0x00165f)
                {
                    igs012_prot_dec_copy_w((ushort)value);
                }
                else if (address2 >= 0x001670 && address2 <= 0x00167f)
                {
                    igs012_prot_mode_w((ushort)value);
                }
                else if (address >= 0x00d400 && address <= 0x00d43f)
                {
                    igs011_prot2_dec_w();
                }
                else if (address >= 0x00d440 && address <= 0x00d47f)
                {
                    drgnwrld_igs011_prot2_swap_w();
                }
                else if (address >= 0x00d480 && address <= 0x00d4bf)
                {
                    igs011_prot2_reset_w();
                }
            }
            else if (address >= 0x50f000 && address <= 0x50f1ff)
            {
                igs011_prot2_dec_w();
            }
            else if (address >= 0x50f200 && address <= 0x50f3ff)
            {
                int offset = (address - 0x50f200) / 2;
                vbowl_igs011_prot2_swap_w(offset);
            }
            else if (address >= 0x50f400 && address <= 0x50f5ff)
            {
                igs011_prot2_reset_w();
            }
            else if (address >= 0x902000 && address <= 0x902fff)
            {
                igs012_prot_reset_w();
            }
            else if (address >= 0x100000 && address <= 0x103fff)
            {
                int offset = address - 0x100000;
                Generic.generic_nvram[offset] = (byte)value;
            }
            else if (address >= 0x200000 && address <= 0x200fff)
            {
                int offset = (address - 0x200000) / 2;
                if (address % 2 == 0)
                {
                    priority_ram[offset] = (ushort)((value << 8) | (priority_ram[offset] & 0xff));
                }
                else if (address % 2 == 1)
                {
                    priority_ram[offset] = (ushort)((priority_ram[offset] & 0xff00) | (byte)value);
                }
            }
            else if (address >= 0x300000 && address <= 0x3fffff)
            {
                int offset = address - 0x300000;
                igs011_layers_w(offset, (byte)value);
            }
            else if (address >= 0x400000 && address <= 0x401fff)
            {
                int offset = address - 0x400000;
                igs011_palette(offset, (byte)value);
            }
            else if (address >= 0x600000 && address <= 0x600007)
            {
                int offset = (address - 0x600000) / 2;
                if (address % 2 == 0)
                {
                    ics2115_0_word_w1(offset, (byte)value);
                }
                else if (address % 2 == 1)
                {
                    ics2115_0_word_w2(offset, (byte)value);
                }
            }
            else if (address >= 0x700000 && address <= 0x700003)
            {
                int offset = (address - 0x700000) / 2;
                if (address % 2 == 0)
                {
                    vbowl_trackball[offset] = (ushort)((value << 8) | vbowl_trackball[offset] & 0xff);
                }
                else if (address % 2 == 1)
                {
                    vbowl_trackball[offset] = (ushort)((vbowl_trackball[offset] & 0xff00) | (byte)value);
                }
            }
            else if (address >= 0x700004 && address <= 0x700005)
            {
                vbowl_pen_hi_w((byte)value);
            }
            else if (address >= 0x800000 && address <= 0x800003)
            {
                int offset = (address - 0x800000) / 2;
                vbowl_igs003_w(offset, (ushort)value);
            }
            else if (address >= 0xa00000 && address <= 0xa00001)
            {
                vbowl_link_0_w();
            }
            else if (address >= 0xa08000 && address <= 0xa08001)
            {
                vbowl_link_1_w();
            }
            else if (address >= 0xa10000 && address <= 0xa10001)
            {
                vbowl_link_2_w();
            }
            else if (address >= 0xa18000 && address <= 0xa18001)
            {
                vbowl_link_3_w();
            }
            else if (address >= 0xa20000 && address <= 0xa20001)
            {
                int offset = address - 0xa20000;
                igs011_priority_w(offset, (byte)value);
            }
            else if (address >= 0xa40000 && address <= 0xa40001)
            {
                int offset = address - 0xa40000;
                igs_dips_w(offset, (byte)value);
            }
            else if (address >= 0xa48000 && address <= 0xa48001)
            {
                igs011_prot_addr_w((ushort)value);
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
        public static void MWriteWord_vbowl(int address, short value)
        {
            address &= 0xffffff;
            if (address >= prot1_addr && address + 1 <= prot1_addr + 7)
            {
                int offset = (int)(address - prot1_addr);
                igs011_prot1_w(offset, (ushort)value);
            }
            else if (address >= 0 && address + 1 <= 0x1ffff)
            {
                int address2 = address & ~0x1c000;
                if (address2 >= 0x001600 && address2 + 1 <= 0x00160f)
                {
                    igs012_prot_swap_w((ushort)value);
                }
                else if (address2 >= 0x001620 && address2 + 1 <= 0x00162f)
                {
                    igs012_prot_dec_inc_w((ushort)value);
                }
                else if (address2 >= 0x001630 && address2 + 1 <= 0x00163f)
                {
                    igs012_prot_inc_w((ushort)value);
                }
                else if (address2 >= 0x001640 && address2 + 1 <= 0x00164f)
                {
                    igs012_prot_copy_w((ushort)value);
                }
                else if (address2 >= 0x001650 && address2 + 1 <= 0x00165f)
                {
                    igs012_prot_dec_copy_w((ushort)value);
                }
                else if (address2 >= 0x001670 && address2 + 1 <= 0x00167f)
                {
                    igs012_prot_mode_w((ushort)value);
                }
                else if (address >= 0x00d400 && address + 1 <= 0x00d43f)
                {
                    igs011_prot2_dec_w();
                }
                else if (address >= 0x00d440 && address + 1 <= 0x00d47f)
                {
                    drgnwrld_igs011_prot2_swap_w();
                }
                else if (address >= 0x00d480 && address + 1 <= 0x00d4bf)
                {
                    igs011_prot2_reset_w();
                }
            }
            else if (address >= 0x50f000 && address + 1 <= 0x50f1ff)
            {
                igs011_prot2_dec_w();
            }
            else if (address >= 0x50f200 && address + 1 <= 0x50f3ff)
            {
                int offset = (address - 0x50f200) / 2;
                vbowl_igs011_prot2_swap_w(offset);
            }
            else if (address >= 0x50f400 && address + 1 <= 0x50f5ff)
            {
                igs011_prot2_reset_w();
            }
            else if (address >= 0x902000 && address + 1 <= 0x902fff)
            {
                igs012_prot_reset_w();
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
            else if (address >= 0x300000 && address + 1 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                igs011_layers_w(offset, (ushort)value);
            }
            else if (address >= 0x400000 && address + 1 <= 0x401fff)
            {
                int offset = (address - 0x400000) / 2;
                igs011_palette(offset, (ushort)value);
            }
            else if (address >= 0x600000 && address + 1 <= 0x600007)
            {
                int offset = (address - 0x600000) / 2;
                ics2115_0_word_w(offset, (ushort)value);
            }
            else if (address >= 0x700000 && address + 1 <= 0x700003)
            {
                int offset = (address - 0x700000) / 2;
                vbowl_trackball[offset] = (ushort)value;
            }
            else if (address >= 0x700004 && address + 1 <= 0x700005)
            {
                vbowl_pen_hi_w((byte)value);
            }
            else if (address >= 0x800000 && address + 1 <= 0x800003)
            {
                int offset = (address - 0x800000) / 2;
                vbowl_igs003_w(offset, (ushort)value);
            }
            else if (address >= 0xa00000 && address + 1 <= 0xa00001)
            {
                vbowl_link_0_w();
            }
            else if (address >= 0xa08000 && address + 1 <= 0xa08001)
            {
                vbowl_link_1_w();
            }
            else if (address >= 0xa10000 && address + 1 <= 0xa10001)
            {
                vbowl_link_2_w();
            }
            else if (address >= 0xa18000 && address + 1 <= 0xa18001)
            {
                vbowl_link_3_w();
            }
            else if (address >= 0xa20000 && address + 1 <= 0xa20001)
            {
                igs011_priority_w((ushort)value);
            }
            else if (address >= 0xa40000 && address + 1 <= 0xa40001)
            {
                igs_dips_w((ushort)value);
            }
            else if (address >= 0xa48000 && address + 1 <= 0xa48001)
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
        public static void MWriteLong_vbowl(int address, int value)
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
            else if (address >= 0x300000 && address + 3 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                igs011_layers_w(offset, (ushort)(value >> 16));
                igs011_layers_w(offset + 1, (ushort)value);
            }
            else if (address >= 0x400000 && address + 3 <= 0x401fff)
            {
                int offset = (address - 0x400000) / 2;
                igs011_palette(offset, (ushort)(value >> 16));
                igs011_palette(offset + 1, (ushort)value);
            }
        }
        public static sbyte MReadOpByte_nkishusp(int address)
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
            else if (address >= 0x000000 && address <= 0x07ffff)
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
        public static sbyte MReadByte_nkishusp(int address)
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
            else if (address >= 0x023400 && address <= 0x0235ff)
            {
                result = (sbyte)lhb2_igs011_prot2_r();
            }
            else if (address >= 0x000000 && address <= 0x07ffff)
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
                result = (sbyte)Generic.generic_nvram[offset];
            }
            else if (address >= 0x200000 && address <= 0x200001)
            {
                result = (sbyte)OKI6295.okim6295_status_0_lsb_r();
            }
            else if (address >= 0x208002 && address <= 0x208003)
            {
                if (address % 2 == 1)
                {
                    result = (sbyte)lhb2_igs003_r();
                }
            }
            else if (address >= 0x20c000 && address <= 0x20cfff)
            {
                int offset = (address - 0x20c000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(priority_ram[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)priority_ram[offset];
                }
            }
            else if (address >= 0x210000 && address <= 0x211fff)
            {
                int offset = (address - 0x210000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)(paletteram16[offset] >> 8);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)paletteram16[offset];
                }
            }
            else if (address >= 0x214000 && address <= 0x214001)
            {
                if (address % 2 == 1)
                {
                    result = sbytec;
                }
            }
            else if (address >= 0x300000 && address <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                if (address % 2 == 0)
                {
                    result = (sbyte)igs011_layers_r1(offset / 2);
                }
                else if (address % 2 == 1)
                {
                    result = (sbyte)igs011_layers_r2(offset / 2);
                }
            }
            else if (address >= 0xa88000 && address <= 0xa88001)
            {
                result = (sbyte)igs_3_dips_r();
            }
            return result;
        }
        public static short MReadOpWord_nkishusp(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= prot1_addr + 8 && address + 1 <= prot1_addr + 9)
            {
                result = (short)igs011_prot1_r();
            }
            else if (address >= 0x000000 && address + 1 <= 0x07ffff)
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
        public static short MReadWord_nkishusp(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= prot1_addr + 8 && address + 1 <= prot1_addr + 9)
            {
                result = (short)igs011_prot1_r();
            }
            else if (address >= 0x023400 && address + 1 <= 0x0235ff)
            {
                result = (short)lhb2_igs011_prot2_r();
            }
            else if (address >= 0x000000 && address + 1 <= 0x07ffff)
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
                result = (short)(Generic.generic_nvram[offset] * 0x100 + Generic.generic_nvram[offset + 1]);
            }
            else if (address >= 0x200000 && address + 1 <= 0x200001)
            {
                result = (short)OKI6295.okim6295_status_0_lsb_r();
            }
            else if (address >= 0x208002 && address + 1 <= 0x208003)
            {
                result = (short)lhb2_igs003_r();
            }
            else if (address >= 0x20c000 && address + 1 <= 0x20cfff)
            {
                int offset = (address - 0x20c000) / 2;
                result = (short)priority_ram[offset];
            }
            else if (address >= 0x210000 && address + 1 <= 0x211fff)
            {
                int offset = (address - 0x210000) / 2;
                result = (short)paletteram16[offset];
            }
            else if (address >= 0x214000 && address + 1 <= 0x214001)
            {
                result = (short)((byte)sbytec);
            }
            else if (address >= 0x300000 && address + 1 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                result = (short)igs011_layers_r(offset);
            }
            else if (address >= 0xa88000 && address + 1 <= 0xa88001)
            {
                result = (short)igs_3_dips_r();
            }
            return result;
        }
        public static int MReadOpLong_nkishusp(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x000000 && address + 3 <= 0x07ffff)
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
        public static int MReadLong_nkishusp(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x000000 && address + 3 <= 0x07ffff)
            {
                if (address + 3 < Memory.mainromLength)
                {
                    int offset = (address - 0x023000) / 2;
                    result = (int)(Memory.mainrom[address] * 0x1000000 + Memory.mainrom[address + 1] * 0x10000 + Memory.mainrom[address + 2] * 0x100 + Memory.mainrom[address + 3]);
                }
                else
                {
                    result = 0;
                }
            }
            else if (address >= 0x100000 && address + 3 <= 0x103fff)
            {
                int offset = (address - 0x100000) / 2;
                result = (int)(Generic.generic_nvram[offset] * 0x1000000 + Generic.generic_nvram[offset + 1] * 0x10000 + Generic.generic_nvram[offset + 2] * 0x100 + Generic.generic_nvram[offset + 3]);
            }
            else if (address >= 0x20c000 && address + 3 <= 0x20cfff)
            {
                int offset = (address - 0x20c000) / 2;
                result = (int)(priority_ram[offset] * 0x10000 + priority_ram[offset + 1]);
            }
            else if (address >= 0x210000 && address + 3 <= 0x211fff)
            {
                int offset = (address - 0x210000) / 2;
                result = (int)(paletteram16[offset] * 0x10000 + paletteram16[offset + 1]);
            }
            else if (address >= 0x300000 && address + 3 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
                result = igs011_layers_r(offset) * 0x10000 + igs011_layers_r(offset + 1);
            }
            return result;
        }
        public static void MWriteByte_nkishusp(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x023000 && address <= 0x0231ff)
            {
                int offset = (address - 0x023000) / 2;
            }
            else if (address >= 0x023200 && address <= 0x0233ff)
            {
                int offset = (address - 0x023200) / 2;
            }
            else if (address >= 0x023600 && address <= 0x0237ff)
            {
                int offset = (address - 0x023600) / 2;
            }
            else if (address >= 0x200000 && address <= 0x200001)
            {
                int offset = (address - 0x200000) / 2;
            }
            else if (address >= 0x204000 && address <= 0x204001)
            {
                int offset = (address - 0x204000) / 2;
            }
            else if (address >= 0x204002 && address <= 0x204003)
            {
                int offset = (address - 0x204002) / 2;
            }
            else if (address >= 0x208000 && address <= 0x208003)
            {
                int offset = (address - 0x208000) / 2;
            }
            else if (address >= 0x210000 && address <= 0x211fff)
            {
                int offset = (address - 0x210000) / 2;
            }
            else if (address >= 0x300000 && address <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
            }
            else if (address >= 0xa20000 && address <= 0xa20001)
            {
                int offset = (address - 0xa20000) / 2;
            }
            else if (address >= 0xa38000 && address <= 0xa38001)
            {
                int offset = (address - 0xa38000) / 2;
            }
            else if (address >= 0xa40000 && address <= 0xa40001)
            {
                int offset = (address - 0xa40000) / 2;
            }
            else if (address >= 0xa50000 && address <= 0xa50001)
            {
                int offset = (address - 0xa50000) / 2;
            }
            else if (address >= 0xa58000 && address <= 0xa58001)
            {
                int offset = (address - 0xa58000) / 2;
            }
            else if (address >= 0xa58800 && address <= 0xa58801)
            {
                int offset = (address - 0xa58800) / 2;
            }
            else if (address >= 0xa59000 && address <= 0xa59001)
            {
                int offset = (address - 0xa59000) / 2;
            }
            else if (address >= 0xa59800 && address <= 0xa59801)
            {
                int offset = (address - 0xa59800) / 2;
            }
            else if (address >= 0xa5a000 && address <= 0xa5a001)
            {
                int offset = (address - 0xa5a000) / 2;
            }
            else if (address >= 0xa5a800 && address <= 0xa5a801)
            {
                int offset = (address - 0xa5a800) / 2;
            }
            else if (address >= 0xa5b000 && address <= 0xa5b001)
            {
                int offset = (address - 0xa5b000) / 2;
            }
            else if (address >= 0xa5b800 && address <= 0xa5b801)
            {
                int offset = (address - 0xa5b800) / 2;
            }
            else if (address >= 0xa5c000 && address <= 0xa5c001)
            {
                int offset = (address - 0xa5c000) / 2;
            }
        }
        public static void MWriteWord_nkishusp(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x023000 && address + 1 <= 0x0231ff)
            {
                int offset = (address - 0x023000) / 2;
            }
            else if (address >= 0x023200 && address + 1 <= 0x0233ff)
            {
                int offset = (address - 0x023200) / 2;
            }
            else if (address >= 0x023600 && address + 1 <= 0x0237ff)
            {
                int offset = (address - 0x023600) / 2;
            }
            else if (address >= 0x200000 && address + 1 <= 0x200001)
            {
                int offset = (address - 0x200000) / 2;
            }
            else if (address >= 0x204000 && address + 1 <= 0x204001)
            {
                int offset = (address - 0x204000) / 2;
            }
            else if (address >= 0x204002 && address + 1 <= 0x204003)
            {
                int offset = (address - 0x204002) / 2;
            }
            else if (address >= 0x208000 && address + 1 <= 0x208003)
            {
                int offset = (address - 0x208000) / 2;
            }
            else if (address >= 0x210000 && address + 1 <= 0x211fff)
            {
                int offset = (address - 0x210000) / 2;
            }
            else if (address >= 0x300000 && address + 1 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
            }
            else if (address >= 0xa20000 && address + 1 <= 0xa20001)
            {
                int offset = (address - 0xa20000) / 2;
            }
            else if (address >= 0xa38000 && address + 1 <= 0xa38001)
            {
                int offset = (address - 0xa38000) / 2;
            }
            else if (address >= 0xa40000 && address + 1 <= 0xa40001)
            {
                int offset = (address - 0xa40000) / 2;
            }
            else if (address >= 0xa50000 && address + 1 <= 0xa50001)
            {
                int offset = (address - 0xa50000) / 2;
            }
            else if (address >= 0xa58000 && address + 1 <= 0xa58001)
            {
                int offset = (address - 0xa58000) / 2;
            }
            else if (address >= 0xa58800 && address + 1 <= 0xa58801)
            {
                int offset = (address - 0xa58800) / 2;
            }
            else if (address >= 0xa59000 && address + 1 <= 0xa59001)
            {
                int offset = (address - 0xa59000) / 2;
            }
            else if (address >= 0xa59800 && address + 1 <= 0xa59801)
            {
                int offset = (address - 0xa59800) / 2;
            }
            else if (address >= 0xa5a000 && address + 1 <= 0xa5a001)
            {
                int offset = (address - 0xa5a000) / 2;
            }
            else if (address >= 0xa5a800 && address + 1 <= 0xa5a801)
            {
                int offset = (address - 0xa5a800) / 2;
            }
            else if (address >= 0xa5b000 && address + 1 <= 0xa5b001)
            {
                int offset = (address - 0xa5b000) / 2;
            }
            else if (address >= 0xa5b800 && address + 1 <= 0xa5b801)
            {
                int offset = (address - 0xa5b800) / 2;
            }
            else if (address >= 0xa5c000 && address + 1 <= 0xa5c001)
            {
                int offset = (address - 0xa5c000) / 2;
            }
        }
        public static void MWriteLong_nkishusp(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x023000 && address + 3 <= 0x0231ff)
            {
                int offset = (address - 0x023000) / 2;
            }
            else if (address >= 0x023200 && address + 3 <= 0x0233ff)
            {
                int offset = (address - 0x023200) / 2;
            }
            else if (address >= 0x023600 && address + 3 <= 0x0237ff)
            {
                int offset = (address - 0x023600) / 2;
            }
            else if (address >= 0x200000 && address + 3 <= 0x200001)
            {
                int offset = (address - 0x200000) / 2;
            }
            else if (address >= 0x204000 && address + 3 <= 0x204001)
            {
                int offset = (address - 0x204000) / 2;
            }
            else if (address >= 0x204002 && address + 3 <= 0x204003)
            {
                int offset = (address - 0x204002) / 2;
            }
            else if (address >= 0x208000 && address + 3 <= 0x208003)
            {
                int offset = (address - 0x208000) / 2;
            }
            else if (address >= 0x210000 && address + 3 <= 0x211fff)
            {
                int offset = (address - 0x210000) / 2;
            }
            else if (address >= 0x300000 && address + 3 <= 0x3fffff)
            {
                int offset = (address - 0x300000) / 2;
            }
            else if (address >= 0xa20000 && address + 3 <= 0xa20001)
            {
                int offset = (address - 0xa20000) / 2;
            }
            else if (address >= 0xa38000 && address + 3 <= 0xa38001)
            {
                int offset = (address - 0xa38000) / 2;
            }
            else if (address >= 0xa40000 && address + 3 <= 0xa40001)
            {
                int offset = (address - 0xa40000) / 2;
            }
            else if (address >= 0xa50000 && address + 3 <= 0xa50001)
            {
                int offset = (address - 0xa50000) / 2;
            }
            else if (address >= 0xa58000 && address + 3 <= 0xa58001)
            {
                int offset = (address - 0xa58000) / 2;
            }
            else if (address >= 0xa58800 && address + 3 <= 0xa58801)
            {
                int offset = (address - 0xa58800) / 2;
            }
            else if (address >= 0xa59000 && address + 3 <= 0xa59001)
            {
                int offset = (address - 0xa59000) / 2;
            }
            else if (address >= 0xa59800 && address + 3 <= 0xa59801)
            {
                int offset = (address - 0xa59800) / 2;
            }
            else if (address >= 0xa5a000 && address + 3 <= 0xa5a001)
            {
                int offset = (address - 0xa5a000) / 2;
            }
            else if (address >= 0xa5a800 && address + 3 <= 0xa5a801)
            {
                int offset = (address - 0xa5a800) / 2;
            }
            else if (address >= 0xa5b000 && address + 3 <= 0xa5b001)
            {
                int offset = (address - 0xa5b000) / 2;
            }
            else if (address >= 0xa5b800 && address + 3 <= 0xa5b801)
            {
                int offset = (address - 0xa5b800) / 2;
            }
            else if (address >= 0xa5c000 && address + 3 <= 0xa5c001)
            {
                int offset = (address - 0xa5c000) / 2;
            }
        }


    }
}
