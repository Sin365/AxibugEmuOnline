namespace MAME.Core
{
    public partial class PGM
    {
        public static sbyte MPReadByte_orlegend(int address)
        {
            sbyte result;
            address &= 0xffffff;
            if (address == 0xc0400f)
            {
                result = (sbyte)pgm_asic3_r();
            }
            else
            {
                result = MReadByte(address);
            }
            return result;
        }
        public static short MPReadWord_orlegend(int address)
        {
            short result;
            address &= 0xffffff;
            if (address == 0xc0400e)
            {
                result = (short)pgm_asic3_r();
            }
            else
            {
                result = MReadWord(address);
            }
            return result;
        }
        public static void MPWriteByte_orlegend(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address == 0xc04001)
            {
                pgm_asic3_reg_w((ushort)value);
            }
            else if (address == 0xc0400f)
            {
                pgm_asic3_w((ushort)value);
            }
            else
            {
                MWriteByte(address, value);
            }
        }
        public static void MPWriteWord_orlegend(int address, short value)
        {
            address &= 0xffffff;
            if (address == 0xc04000)
            {
                pgm_asic3_reg_w((ushort)value);
            }
            else if (address == 0xc0400e)
            {
                pgm_asic3_w((ushort)value);
            }
            else
            {
                MWriteWord(address, value);
            }
        }
        public static sbyte MPReadByte_drgw2(int address)
        {
            sbyte result;
            address &= 0xffffff;
            if (address >= 0xd80000 && address <= 0xd80003)
            {
                result = 0;
            }
            else
            {
                result = MReadByte(address);
            }
            return result;
        }
        public static short MPReadWord_drgw2(int address)
        {
            short result;
            address &= 0xffffff;
            if (address == 0xd80000 && address + 1 <= 0xd80003)
            {
                int offset = (address - 0xd80000) / 2;
                result = (short)killbld_igs025_prot_r(offset);
            }
            else
            {
                result = MReadWord(address);
            }
            return result;
        }
        public static int MPReadLong_drgw2(int address)
        {
            int result;
            address &= 0xffffff;
            if (address == 0xd80000 && address + 3 <= 0xd80003)
            {
                result = 0;
            }
            else
            {
                result = MReadLong(address);
            }
            return result;
        }
        public static void MPWriteByte_drgw2(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address == 0xd80000 && address <= 0xd80003)
            {
                int offset = (address - 0xd80000) / 2;
                //drgw2_d80000_protection_w(offset, (ushort)value);
            }
            else
            {
                MWriteByte(address, value);
            }
        }
        public static void MPWriteWord_drgw2(int address, short value)
        {
            address &= 0xffffff;
            if (address == 0xd80000 && address + 1 <= 0xd80003)
            {
                int offset = (address - 0xd80000) / 2;
                drgw2_d80000_protection_w(offset, (ushort)value);
            }
            else
            {
                MWriteWord(address, value);
            }
        }
        public static void MPWriteLong_drgw2(int address, int value)
        {
            address &= 0xffffff;
            if (address == 0xd80000 && address + 3 <= 0xd80003)
            {
                int offset = (address - 0xd80000) / 2;
                //drgw2_d80000_protection_w(offset, (ushort)value);
            }
            else
            {
                MWriteLong(address, value);
            }
        }
    }
}
