using System;

namespace MAME.Core
{
    public unsafe partial class Neogeo
    {
        public static ushort fatfury2_protection_16_r(int offset)
        {
            ushort res = (ushort)(fatfury2_prot_data >> 24);
            switch (offset)
            {
                case 0x55550 / 2:
                case 0xffff0 / 2:
                case 0x00000 / 2:
                case 0xff000 / 2:
                case 0x36000 / 2:
                case 0x36008 / 2:
                    return res;
                case 0x36004 / 2:
                case 0x3600c / 2:
                    return (ushort)(((res & 0xf0) >> 4) | ((res & 0x0f) << 4));
                default:
                    return 0;
            }
        }
        public static void fatfury2_protection_16_w(int offset)
        {
            switch (offset)
            {
                case 0x11112 / 2:
                    fatfury2_prot_data = 0xff000000;
                    break;
                case 0x33332 / 2:
                    fatfury2_prot_data = 0x0000ffff;
                    break;
                case 0x44442 / 2:
                    fatfury2_prot_data = 0x00ff0000;
                    break;
                case 0x55552 / 2:
                    fatfury2_prot_data = 0xff00ff00;
                    break;
                case 0x56782 / 2:
                    fatfury2_prot_data = 0xf05a3601;
                    break;
                case 0x42812 / 2:
                    fatfury2_prot_data = 0x81422418;
                    break;
                case 0x55550 / 2:
                case 0xffff0 / 2:
                case 0xff000 / 2:
                case 0x36000 / 2:
                case 0x36004 / 2:
                case 0x36008 / 2:
                case 0x3600c / 2:
                    fatfury2_prot_data <<= 8;
                    break;
                default:
                    break;
            }
        }
        public static void kof98_prot_w(int value)
        {
            switch (value)
            {
                case 0x0090:
                    Memory.mainrom[0x100] = 0x00;
                    Memory.mainrom[0x101] = 0xc2;
                    Memory.mainrom[0x102] = 0x00;
                    Memory.mainrom[0x103] = 0xfd;
                    break;
                case 0x00f0:
                    Memory.mainrom[0x100] = 0x4e;
                    Memory.mainrom[0x101] = 0x45;
                    Memory.mainrom[0x102] = 0x4f;
                    Memory.mainrom[0x103] = 0x2d;
                    break;
                default:
                    break;
            }
        }
        public static ushort prot_9a37_r()
        {
            return 0x9a37;
        }
        public static ushort sma_random_r()
        {
            ushort old = neogeo_rng;
            ushort newbit = (ushort)(((neogeo_rng >> 2) ^
                             (neogeo_rng >> 3) ^
                             (neogeo_rng >> 5) ^
                             (neogeo_rng >> 6) ^
                             (neogeo_rng >> 7) ^
                             (neogeo_rng >> 11) ^
                             (neogeo_rng >> 12) ^
                             (neogeo_rng >> 15)) & 1);
            neogeo_rng = (ushort)((neogeo_rng << 1) | newbit);
            return old;
        }
        readonly static int[] kof99_bankswitch_w_bankoffset = new int[]
            {
                0x000000, 0x100000, 0x200000, 0x300000,
                0x3cc000, 0x4cc000, 0x3f2000, 0x4f2000,
                0x407800, 0x507800, 0x40d000, 0x50d000,
                0x417800, 0x517800, 0x420800, 0x520800,
                0x424800, 0x524800, 0x429000, 0x529000,
                0x42e800, 0x52e800, 0x431800, 0x531800,
                0x54d000, 0x551000, 0x567000, 0x592800,
                0x588800, 0x581800, 0x599800, 0x594800,
                0x598000
            };
        public static void kof99_bankswitch_w(int data)
        {
            int bankaddress;
            
            data =
                (((data >> 14) & 1) << 0) +
                (((data >> 6) & 1) << 1) +
                (((data >> 8) & 1) << 2) +
                (((data >> 10) & 1) << 3) +
                (((data >> 12) & 1) << 4) +
                (((data >> 5) & 1) << 5);
            bankaddress = 0x100000 + kof99_bankswitch_w_bankoffset[data];
            main_cpu_bank_address = bankaddress;
        }
        readonly static int[] garou_bankswitch_w_bankoffset = new int[]
            {
                0x000000, 0x100000, 0x200000, 0x300000, // 00
		        0x280000, 0x380000, 0x2d0000, 0x3d0000, // 04
		        0x2f0000, 0x3f0000, 0x400000, 0x500000, // 08
		        0x420000, 0x520000, 0x440000, 0x540000, // 12
		        0x498000, 0x598000, 0x4a0000, 0x5a0000, // 16
		        0x4a8000, 0x5a8000, 0x4b0000, 0x5b0000, // 20
		        0x4b8000, 0x5b8000, 0x4c0000, 0x5c0000, // 24
		        0x4c8000, 0x5c8000, 0x4d0000, 0x5d0000, // 28
		        0x458000, 0x558000, 0x460000, 0x560000, // 32
		        0x468000, 0x568000, 0x470000, 0x570000, // 36
		        0x478000, 0x578000, 0x480000, 0x580000, // 40
		        0x488000, 0x588000, 0x490000, 0x590000, // 44
		        0x5d0000, 0x5d8000, 0x5e0000, 0x5e8000, // 48
		        0x5f0000, 0x5f8000, 0x600000
            };
        public static void garou_bankswitch_w(int data)
        {
            int bankaddress;
            
            data =
                (((data >> 5) & 1) << 0) +
                (((data >> 9) & 1) << 1) +
                (((data >> 7) & 1) << 2) +
                (((data >> 6) & 1) << 3) +
                (((data >> 14) & 1) << 4) +
                (((data >> 12) & 1) << 5);
            bankaddress = 0x100000 + garou_bankswitch_w_bankoffset[data];
            main_cpu_bank_address = bankaddress;
        }
        readonly static int[] garouh_bankswitch_w_bankoffset = new int[]
            {
                0x000000, 0x100000, 0x200000, 0x300000, // 00
		        0x280000, 0x380000, 0x2d0000, 0x3d0000, // 04
		        0x2c8000, 0x3c8000, 0x400000, 0x500000, // 08
		        0x420000, 0x520000, 0x440000, 0x540000, // 12
		        0x598000, 0x698000, 0x5a0000, 0x6a0000, // 16
		        0x5a8000, 0x6a8000, 0x5b0000, 0x6b0000, // 20
		        0x5b8000, 0x6b8000, 0x5c0000, 0x6c0000, // 24
		        0x5c8000, 0x6c8000, 0x5d0000, 0x6d0000, // 28
		        0x458000, 0x558000, 0x460000, 0x560000, // 32
		        0x468000, 0x568000, 0x470000, 0x570000, // 36
		        0x478000, 0x578000, 0x480000, 0x580000, // 40
		        0x488000, 0x588000, 0x490000, 0x590000, // 44
		        0x5d8000, 0x6d8000, 0x5e0000, 0x6e0000, // 48
		        0x5e8000, 0x6e8000, 0x6e8000, 0x000000, // 52
		        0x000000, 0x000000, 0x000000, 0x000000, // 56
		        0x000000, 0x000000, 0x000000, 0x000000  // 60
	        };
        public static void garouh_bankswitch_w(int data)
        {
            int bankaddress;

            data =
                (((data >> 4) & 1) << 0) +
                (((data >> 8) & 1) << 1) +
                (((data >> 14) & 1) << 2) +
                (((data >> 2) & 1) << 3) +
                (((data >> 11) & 1) << 4) +
                (((data >> 13) & 1) << 5);
            bankaddress = 0x100000 + garouh_bankswitch_w_bankoffset[data];
            main_cpu_bank_address = bankaddress;
        }
        readonly static int[] mslug3_bankswitch_w_bankoffset = new int[]
        {
              0x000000, 0x020000, 0x040000, 0x060000, // 00
	          0x070000, 0x090000, 0x0b0000, 0x0d0000, // 04
	          0x0e0000, 0x0f0000, 0x120000, 0x130000, // 08
	          0x140000, 0x150000, 0x180000, 0x190000, // 12
	          0x1a0000, 0x1b0000, 0x1e0000, 0x1f0000, // 16
	          0x200000, 0x210000, 0x240000, 0x250000, // 20
	          0x260000, 0x270000, 0x2a0000, 0x2b0000, // 24
	          0x2c0000, 0x2d0000, 0x300000, 0x310000, // 28
	          0x320000, 0x330000, 0x360000, 0x370000, // 32
	          0x380000, 0x390000, 0x3c0000, 0x3d0000, // 36
	          0x400000, 0x410000, 0x440000, 0x450000, // 40
	          0x460000, 0x470000, 0x4a0000, 0x4b0000, // 44
	          0x4c0000
        };
        public static void mslug3_bankswitch_w(int data)
        {
            int bankaddress;
            data =
                (((data >> 14) & 1) << 0) +
                (((data >> 12) & 1) << 1) +
                (((data >> 15) & 1) << 2) +
                (((data >> 6) & 1) << 3) +
                (((data >> 3) & 1) << 4) +
                (((data >> 9) & 1) << 5);
            bankaddress = 0x100000 + mslug3_bankswitch_w_bankoffset[data];
            main_cpu_bank_address = bankaddress;
        }
        readonly static int[] kof2000_bankswitch_w_bankoffset = new int[]
            {
                0x000000, 0x100000, 0x200000, 0x300000, // 00
		        0x3f7800, 0x4f7800, 0x3ff800, 0x4ff800, // 04
		        0x407800, 0x507800, 0x40f800, 0x50f800, // 08
		        0x416800, 0x516800, 0x41d800, 0x51d800, // 12
		        0x424000, 0x524000, 0x523800, 0x623800, // 16
		        0x526000, 0x626000, 0x528000, 0x628000, // 20
		        0x52a000, 0x62a000, 0x52b800, 0x62b800, // 24
		        0x52d000, 0x62d000, 0x52e800, 0x62e800, // 28
		        0x618000, 0x619000, 0x61a000, 0x61a800, // 32
	        };
        public static void kof2000_bankswitch_w(int data)
        {
            int bankaddress;

            data =
                (((data >> 15) & 1) << 0) +
                (((data >> 14) & 1) << 1) +
                (((data >> 7) & 1) << 2) +
                (((data >> 3) & 1) << 3) +
                (((data >> 10) & 1) << 4) +
                (((data >> 5) & 1) << 5);
            bankaddress = 0x100000 + kof2000_bankswitch_w_bankoffset[data];
            main_cpu_bank_address = bankaddress;
        }
        public static void pvc_prot1()
        {
            byte b1, b2;
            b1 = pvc_cartridge_ram[0x1fe0];
            b2 = pvc_cartridge_ram[0x1fe1];
            pvc_cartridge_ram[0x1fe2] = (byte)((((b2 >> 4) & 0xf) << 1) | ((b1 >> 5) & 1));
            pvc_cartridge_ram[0x1fe3] = (byte)((((b2 >> 0) & 0xf) << 1) | ((b1 >> 4) & 1));
            pvc_cartridge_ram[0x1fe4] = (byte)(b1 >> 7);
            pvc_cartridge_ram[0x1fe5] = (byte)((((b1 >> 0) & 0xf) << 1) | ((b1 >> 6) & 1));
        }
        public static void pvc_prot2()
        {
            byte b1, b2, b3, b4;
            b1 = pvc_cartridge_ram[0x1fe8];
            b2 = pvc_cartridge_ram[0x1fe9];
            b3 = pvc_cartridge_ram[0x1fea];
            b4 = pvc_cartridge_ram[0x1feb];
            pvc_cartridge_ram[0x1fec] = (byte)((b4 >> 1) | ((b2 & 1) << 4) | ((b1 & 1) << 5) | ((b4 & 1) << 6) | ((b3 & 1) << 7));
            pvc_cartridge_ram[0x1fed] = (byte)((b2 >> 1) | ((b1 >> 1) << 4));
        }
        public static void pvc_write_bankswitch()
        {
            int bankaddress;
            bankaddress = pvc_cartridge_ram[0xff8 * 2] + pvc_cartridge_ram[0xff9 * 2] * 0x10000 + pvc_cartridge_ram[0xff9 * 2 + 1] * 0x100;
            pvc_cartridge_ram[0x1ff0] &= 0xfe;
            pvc_cartridge_ram[0x1ff1] = 0xa0;
            pvc_cartridge_ram[0x1ff2] &= 0x7f;
            main_cpu_bank_address = bankaddress + 0x100000;
        }
        public static void cthd2003_bankswitch_w(int data)
        {
            int bankaddress;
            int[] cthd2003_banks = new int[8]
            {
                1,0,1,0,1,0,3,2,
            };
            bankaddress = 0x100000 + cthd2003_banks[data & 7] * 0x100000;
            main_cpu_bank_address = bankaddress;
        }
        public static ushort mslug5_prot_r()
        {
            return 0xa0;
        }
        public static void ms5plus_bankswitch_w(int offset, int data)
        {
            int bankaddress;
            if ((offset == 0) && (data == 0xa0))
            {
                bankaddress = 0xa0;
                main_cpu_bank_address = bankaddress;
            }
            else if (offset == 2)
            {
                data = data >> 4;
                bankaddress = data * 0x100000;
                main_cpu_bank_address = bankaddress;
            }
        }
        public static void kof2003_w(int offset)
        {
            if (offset == 0x1ff0 / 2 || offset == 0x1ff2 / 2)
            {
                int address = (extra_ram[0x1ff2] << 16) | (extra_ram[0x1ff3] << 8) | extra_ram[0x1ff0];
                byte prt = extra_ram[0x1ff3];
                extra_ram[0x1ff0] &= 0xfe;
                extra_ram[0x1ff1] = 0xa0;
                extra_ram[0x1ff2] &= 0x7f;
                main_cpu_bank_address = address + 0x100000;
                Memory.mainrom[0x58197] = prt;
            }
        }
        public static void kof2003p_w(int offset)
        {
            if (offset == 0x1ff0 / 2 || offset == 0x1ff2 / 2)
            {
                int address = (extra_ram[0x1ff2] << 16) | (extra_ram[0x1ff3] << 8) | extra_ram[0x1ff1];
                byte prt = extra_ram[0x1ff3];
                extra_ram[0x1ff1] &= 0xfe;
                extra_ram[0x1ff2] &= 0x7f;
                main_cpu_bank_address = address + 0x100000;
                Memory.mainrom[0x58197] = prt;
            }
        }
        public static byte sbp_protection_r(int offset)
        {
            byte origdata = Memory.mainrom[offset + 0x200];
            byte data = (byte)BITSWAP8(origdata, 3, 2, 1, 0, 7, 6, 5, 4);
            int realoffset = 0x200 + offset;
            if (realoffset == 0xd5e || realoffset == 0xd5f)
                return origdata;
            return data;
        }
        public static void sbp_protection_w(int offset, int data)
        {
            int realoffset = 0x200 + (offset * 2);
            if (realoffset == 0x1080)
            {
                if (data == 0x4e75)
                {
                    return;
                }
                else if (data == 0xffff)
                {
                    return;
                }
            }
        }
        public static void kof10th_custom_w(int offset, byte data)
        {
            if (extra_ram[0x1ffc] == 0 && extra_ram[0x1ffd] == 0)
            {
                Memory.mainrom[0xe0000 + offset] = (byte)data;
            }
            else
            {
                Neogeo.fixedbiosrom[offset / 2] = (byte)BITSWAP8(data, 7, 6, 0, 4, 3, 2, 1, 5);
            }
        }
        public static void kof10th_custom_w(int offset, ushort data)
        {
            if (extra_ram[0x1ffc] == 0 && extra_ram[0x1ffd] == 0)
            {
                Memory.mainrom[0xe0000 + offset] = (byte)(data >> 8);
                Memory.mainrom[0xe0000 + offset + 1] = (byte)data;
            }
            else
            {
                Neogeo.fixedbiosrom[offset / 2] = (byte)BITSWAP8(data, 7, 6, 0, 4, 3, 2, 1, 5);
            }
        }
        public static void kof10th_bankswitch_w(int offset, byte data)
        {
            if (offset >= 0xbe000)
            {
                if (offset == 0xbfff0)
                {
                    int bank = 0x100000 + ((data & 7) << 20);
                    if (bank >= 0x700000)
                        bank = 0x100000;
                    main_cpu_bank_address = bank;
                }
                else if (offset == 0xbfff8 && (extra_ram[0x1ff8] * 0x100 + extra_ram[0x1ff9] != data))
                {
                    //Array.Copy(Memory.mainrom, ((data & 1) != 0) ? 0x810000 : 0x710000, Memory.mainrom, 0x10000, 0xcffff);

                    //TODO 验证拷贝可靠性
                    byte* source = Memory.mainrom + ((data & 1) != 0 ? 0x810000 : 0x710000);
                    byte* destination = Memory.mainrom + 0x10000;
                    int length = 0xcffff; // 注意：这个长度是否正确取决于你的实际需求
                    Buffer.MemoryCopy(source, destination, length, length);
                }
                extra_ram[(offset - 0xbe000) & 0x1fff] = (byte)data;
            }
        }
        public static void kof10th_bankswitch_w(int offset, ushort data)
        {
            if (offset >= 0xbe000)
            {
                if (offset == 0xbfff0)
                {
                    int bank = 0x100000 + ((data & 7) << 20);
                    if (bank >= 0x700000)
                        bank = 0x100000;
                    main_cpu_bank_address = bank;
                }
                else if (offset == 0xbfff8 && (extra_ram[0x1ff8] * 0x100 + extra_ram[0x1ff9] != data))
                {
                    //Array.Copy(Memory.mainrom, ((data & 1) != 0) ? 0x810000 : 0x710000, Memory.mainrom, 0x10000, 0xcffff);

                    //TODO 验证拷贝可靠性
                    byte* source = Memory.mainrom + ((data & 1) != 0 ? 0x810000 : 0x710000);
                    byte* destination = Memory.mainrom + 0x10000;
                    int length = 0xcffff; // 注意：这个长度是否正确取决于你的实际需求
                    Buffer.MemoryCopy(source, destination, length, length);

                }
                extra_ram[(offset - 0xbe000) & 0x1fff] = (byte)(data >> 8);
                extra_ram[((offset - 0xbe000) & 0x1fff) + 1] = (byte)data;
            }
        }
        public static int BIT(int x, int n)
        {
            return (x >> n) & 1;
        }
        public static int BITSWAP8(int val, int B7, int B6, int B5, int B4, int B3, int B2, int B1, int B0)
        {
            return ((BIT(val, B7) << 7) |
                (BIT(val, B6) << 6) |
                (BIT(val, B5) << 5) |
                (BIT(val, B4) << 4) |
                (BIT(val, B3) << 3) |
                (BIT(val, B2) << 2) |
                (BIT(val, B1) << 1) |
                (BIT(val, B0) << 0));
        }

        //TODO 暂时注释 用不上
        //public static int BITSWAP16(int val, int B15, int B14, int B13, int B12, int B11, int B10, int B9, int B8, int B7, int B6, int B5, int B4, int B3, int B2, int B1, int B0)
        //{
        //    return ((BIT(val, B15) << 15) |
        //        (BIT(val, B14) << 14) |
        //        (BIT(val, B13) << 13) |
        //        (BIT(val, B12) << 12) |
        //        (BIT(val, B11) << 11) |
        //        (BIT(val, B10) << 10) |
        //        (BIT(val, B9) << 9) |
        //        (BIT(val, B8) << 8) |
        //        (BIT(val, B7) << 7) |
        //        (BIT(val, B6) << 6) |
        //        (BIT(val, B5) << 5) |
        //        (BIT(val, B4) << 4) |
        //        (BIT(val, B3) << 3) |
        //        (BIT(val, B2) << 2) |
        //        (BIT(val, B1) << 1) |
        //        (BIT(val, B0) << 0));
        //}
    }
}
