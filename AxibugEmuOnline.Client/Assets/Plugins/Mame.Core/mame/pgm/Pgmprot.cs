namespace MAME.Core
{
    public partial class PGM
    {
        public static byte asic3_reg, asic3_x;
        public static byte[] asic3_latch = new byte[3];
        public static ushort asic3_hold, asic3_hilo;
        public static int bt(uint v, int bit)
        {
            if ((v & (1 << bit)) != 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        public static void asic3_compute_hold(int y, int z)
        {
            ushort old = asic3_hold;
            asic3_hold = (ushort)((old << 1) | (old >> 15));
            asic3_hold ^= 0x2bad;
            asic3_hold ^= (ushort)BIT(z, y);
            asic3_hold ^= (ushort)(BIT(asic3_x, 2) << 10);
            asic3_hold ^= (ushort)BIT(old, 5);
            switch (short4)
            {
                case 0:
                case 1:
                    asic3_hold ^= (ushort)(BIT(old, 10) ^ BIT(old, 8) ^ (BIT(asic3_x, 0) << 1) ^ (BIT(asic3_x, 1) << 6) ^ (BIT(asic3_x, 3) << 14));
                    break;
                case 2:
                    asic3_hold ^= (ushort)(BIT(old, 10) ^ BIT(old, 8) ^ (BIT(asic3_x, 0) << 4) ^ (BIT(asic3_x, 1) << 6) ^ (BIT(asic3_x, 3) << 12));
                    break;
                case 3:
                    asic3_hold ^= (ushort)(BIT(old, 7) ^ BIT(old, 6) ^ (BIT(asic3_x, 0) << 4) ^ (BIT(asic3_x, 1) << 6) ^ (BIT(asic3_x, 3) << 12));
                    break;
                case 4:
                    asic3_hold ^= (ushort)(BIT(old, 7) ^ BIT(old, 6) ^ (BIT(asic3_x, 0) << 3) ^ (BIT(asic3_x, 1) << 8) ^ (BIT(asic3_x, 3) << 14));
                    break;
            }
        }
        public static ushort pgm_asic3_r()
        {
            byte res = 0;
            switch (asic3_reg)
            {
                case 0x00:
                    res = (byte)((asic3_latch[0] & 0xf7) | ((short4 << 3) & 0x08));
                    break;
                case 0x01:
                    res = asic3_latch[1];
                    break;
                case 0x02:
                    res = (byte)((asic3_latch[2] & 0x7f) | ((short4 << 6) & 0x80));
                    break;
                case 0x03:
                    res = (byte)BITSWAP8(asic3_hold, 5, 2, 9, 7, 10, 13, 12, 15);
                    break;
                case 0x20: res = 0x49; break;
                case 0x21: res = 0x47; break;
                case 0x22: res = 0x53; break;
                case 0x24: res = 0x41; break;
                case 0x25: res = 0x41; break;
                case 0x26: res = 0x7f; break;
                case 0x27: res = 0x41; break;
                case 0x28: res = 0x41; break;
                case 0x2a: res = 0x3e; break;
                case 0x2b: res = 0x41; break;
                case 0x2c: res = 0x49; break;
                case 0x2d: res = 0xf9; break;
                case 0x2e: res = 0x0a; break;
                case 0x30: res = 0x26; break;
                case 0x31: res = 0x49; break;
                case 0x32: res = 0x49; break;
                case 0x33: res = 0x49; break;
                case 0x34: res = 0x32; break;
            }
            return res;
        }
        public static void pgm_asic3_w(ushort data)
        {
            //if(ACCESSING_BITS_0_7)
            {
                if (asic3_reg < 3)
                    asic3_latch[asic3_reg] = (byte)(data << 1);
                else if (asic3_reg == 0x40)
                {
                    asic3_hilo = (ushort)((asic3_hilo << 8) | data);
                }
                else if (asic3_reg == 0x48)
                {
                    asic3_x = 0;
                    if ((asic3_hilo & 0x0090) == 0)
                        asic3_x |= 0x01;
                    if ((asic3_hilo & 0x0006) == 0)
                        asic3_x |= 0x02;
                    if ((asic3_hilo & 0x9000) == 0)
                        asic3_x |= 0x04;
                    if ((asic3_hilo & 0x0a00) == 0)
                        asic3_x |= 0x08;
                }
                else if (asic3_reg >= 0x80 && asic3_reg <= 0x87)
                {
                    asic3_compute_hold(asic3_reg & 0x07, data);
                }
                else if (asic3_reg == 0xa0)
                {
                    asic3_hold = 0;
                }
            }
        }
        public static void pgm_asic3_reg_w(ushort data)
        {
            //if(ACCESSING_BITS_0_7)
            asic3_reg = (byte)(data & 0xff);
        }
    }
}
