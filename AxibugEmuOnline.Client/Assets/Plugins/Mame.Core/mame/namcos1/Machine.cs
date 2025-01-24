using System;

namespace MAME.Core
{
    public partial class Namcos1
    {
        public static byte[] namcos1_paletteram;
        public static byte[] s1ram, namcos1_triram;
        public static int audiocpurom_offset, mcurom_offset;
        public static int key_id, key_reg, key_rng, key_swap4_arg, key_swap4, key_bottom4, key_top4;
        public static uint key_quotient, key_reminder, key_numerator_high_word;
        public static byte[] key;
        public static int mcu_patch_data;
        public static int namcos1_reset = 0;
        public static int wdog;
        public static int[,] cus117_offset;
        public static int[,] user1rom_offset;
        public static Func<int, byte> key_r;
        public static Action<int, byte> key_w;
        public class namcos1_specific
        {
            public int key_id;
            public int key_reg1;
            public int key_reg2;
            public int key_reg3;
            public int key_reg4;
            public int key_reg5;
            public int key_reg6;
            public namcos1_specific(int i1, int i2, int i3, int i4, int i5, int i6, int i7)
            {
                key_id = i1;
                key_reg1 = i2;
                key_reg2 = i3;
                key_reg3 = i4;
                key_reg4 = i5;
                key_reg5 = i6;
                key_reg6 = i7;
            }
        };
        public static void namcos1_3dcs_w(int offset)
        {
            if ((offset & 1) != 0)
            {
                //popmessage("LEFT");
            }
            else
            {
                //popmessage("RIGHT");
            }
        }
        public static byte no_key_r(int offset)
        {
            return 0;
        }
        public static void no_key_w(int offset, byte data)
        {

        }
        public static byte key_type1_r(int offset)
        {
            if (offset < 3)
            {
                int d = key[0];
                int n = (key[1] << 8) | key[2];
                int q, r;
                if (d != 0)
                {
                    q = n / d;
                    r = n % d;
                }
                else
                {
                    q = 0xffff;
                    r = 0x00;
                }
                if (offset == 0)
                    return (byte)r;
                if (offset == 1)
                    return (byte)(q >> 8);
                if (offset == 2)
                    return (byte)(q & 0xff);
            }
            else if (offset == 3)
                return (byte)key_id;
            return 0;
        }
        public static void key_type1_w(int offset, byte data)
        {
            if (offset < 4)
                key[offset] = data;
        }
        public static byte key_type2_r(int offset)
        {
            key_numerator_high_word = 0;
            if (offset < 4)
            {
                if (offset == 0)
                    return (byte)(key_reminder >> 8);
                if (offset == 1)
                    return (byte)(key_reminder & 0xff);
                if (offset == 2)
                    return (byte)(key_quotient >> 8);
                if (offset == 3)
                    return (byte)(key_quotient & 0xff);
            }
            else if (offset == 4)
                return (byte)key_id;
            return 0;
        }
        public static void key_type2_w(int offset, byte data)
        {
            if (offset < 5)
            {
                key[offset] = data;
                if (offset == 3)
                {
                    uint d = (uint)((key[0] << 8) | key[1]);
                    uint n = (uint)((key_numerator_high_word << 16) | (uint)(key[2] << 8) | key[3]);
                    if (d != 0)
                    {
                        key_quotient = n / d;
                        key_reminder = n % d;
                    }
                    else
                    {
                        key_quotient = 0xffff;
                        key_reminder = 0x0000;
                    }
                    key_numerator_high_word = (uint)((key[2] << 8) | key[3]);
                }
            }
        }
        public static byte key_type3_r(int offset)
        {
            int op;
            op = (offset & 0x70) >> 4;
            if (op == key_reg)
                return (byte)key_id;
            if (op == key_rng)
                return 0;// (byte)mame_rand(machine);
            if (op == key_swap4)
                return (byte)((key[key_swap4_arg] << 4) | (key[key_swap4_arg] >> 4));
            if (op == key_bottom4)
                return (byte)((offset << 4) | (key[key_swap4_arg] & 0x0f));
            if (op == key_top4)
                return (byte)((offset << 4) | (key[key_swap4_arg] >> 4));
            return 0;
        }
        public static void key_type3_w(int offset, byte data)
        {
            key[(offset & 0x70) >> 4] = data;
        }
        public static void namcos1_sound_bankswitch_w(byte data)
        {
            int bank = (data & 0x70) >> 4;
            audiocpurom_offset = 0x4000 * bank;
        }
        public static void namcos1_cpu_control_w(byte data)
        {
            if (((data & 1) ^ namcos1_reset) != 0)
            {
                mcu_patch_data = 0;
                namcos1_reset = data & 1;
            }
            Cpuint.cpunum_set_input_line(1, (int)LineState.INPUT_LINE_RESET, ((data & 1) != 0) ? LineState.CLEAR_LINE : LineState.ASSERT_LINE);
            Cpuint.cpunum_set_input_line(2, (int)LineState.INPUT_LINE_RESET, ((data & 1) != 0) ? LineState.CLEAR_LINE : LineState.ASSERT_LINE);
            Cpuint.cpunum_set_input_line(3, (int)LineState.INPUT_LINE_RESET, ((data & 1) != 0) ? LineState.CLEAR_LINE : LineState.ASSERT_LINE);
        }
        public static void namcos1_watchdog_w()
        {
            wdog |= 1 << Cpuexec.activecpu;
            if (wdog == 7 || (namcos1_reset == 0))
            {
                wdog = 0;
                Generic.watchdog_reset_w();
            }
        }
        public static byte soundram_r(int offset)
        {
            if (offset < 0x1000)
            {
                offset &= 0x3ff;
                return Namco.namcos1_cus30_r(offset);
            }
            else
            {
                offset &= 0x7ff;
                return namcos1_triram[offset];
            }
        }
        public static void soundram_w(int offset, byte data)
        {
            if (offset < 0x1000)
            {
                offset &= 0x3ff;
                Namco.namcos1_cus30_w(offset, data);
            }
            else
            {
                offset &= 0x7ff;
                namcos1_triram[offset] = data;
            }
        }
        public static void namcos1_bankswitch(int cpu, int offset, byte data)
        {
            int reg = (offset >> 9) & 0x7;
            if ((offset & 1) != 0)
            {
                cus117_offset[cpu, reg] = (cus117_offset[cpu, reg] & 0x600000) | (data * 0x2000);
            }
            else
            {
                cus117_offset[cpu, reg] = (cus117_offset[cpu, reg] & 0x1fe000) | ((data & 0x03) * 0x200000);
            }
            if (cus117_offset[cpu, reg] >= 0x400000 && cus117_offset[cpu, reg] <= 0x7fffff)
            {
                user1rom_offset[cpu, reg] = cus117_offset[cpu, reg] - 0x400000;
            }
        }
        public static void namcos1_bankswitch_w(int offset, byte data)
        {
            namcos1_bankswitch(Cpuexec.activecpu, offset, data);
        }
        public static void namcos1_subcpu_bank_w(byte data)
        {
            cus117_offset[1, 7] = 0x600000 | (data * 0x2000);
            user1rom_offset[1, 7] = cus117_offset[1, 7] - 0x400000;
        }
        public static void machine_reset_namcos1()
        {
            cus117_offset[0, 0] = 0x0180 * 0x2000;
            cus117_offset[0, 1] = 0x0180 * 0x2000;
            cus117_offset[0, 7] = 0x03ff * 0x2000;
            cus117_offset[1, 0] = 0x0180 * 0x2000;
            cus117_offset[1, 7] = 0x03ff * 0x2000;
            user1rom_offset[0, 7] = cus117_offset[0, 7] - 0x400000;
            user1rom_offset[1, 7] = cus117_offset[1, 7] - 0x400000;
            Cpuint.cpunum_set_input_line(1, (int)LineState.INPUT_LINE_RESET, LineState.ASSERT_LINE);
            Cpuint.cpunum_set_input_line(2, (int)LineState.INPUT_LINE_RESET, LineState.ASSERT_LINE);
            Cpuint.cpunum_set_input_line(3, (int)LineState.INPUT_LINE_RESET, LineState.ASSERT_LINE);
            mcu_patch_data = 0;
            namcos1_reset = 0;
            namcos1_init_DACs();
            int i, j;
            for (i = 0; i < 8; i++)
            {
                key[i] = 0;
            }
            for (i = 0; i < 2; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    cus117_offset[i, j] = 0;
                }
            }
            wdog = 0;
        }
        public static void namcos1_mcu_bankswitch_w(byte data)
        {
            int addr;
            switch (data & 0xfc)
            {
                case 0xf8:
                    addr = 0x00000;
                    data ^= 2;
                    break;
                case 0xf4:
                    addr = 0x20000;
                    break;
                case 0xec:
                    addr = 0x40000;
                    break;
                case 0xdc:
                    addr = 0x60000;
                    break;
                case 0xbc:
                    addr = 0x80000;
                    break;
                case 0x7c:
                    addr = 0xa0000;
                    break;
                default:
                    addr = 0x00000;
                    break;
            }
            addr += (data & 3) * 0x8000;
            mcurom_offset = addr;
        }
        public static void namcos1_mcu_patch_w(byte data)
        {
            if (mcu_patch_data == 0xa6)
                return;
            mcu_patch_data = data;
            namcos1_triram[0] = data;
        }
        public static void namcos1_driver_init(namcos1_specific specific)
        {
            key_id = specific.key_id;
            key_reg = specific.key_reg1;
            key_rng = specific.key_reg2;
            key_swap4_arg = specific.key_reg3;
            key_swap4 = specific.key_reg4;
            key_bottom4 = specific.key_reg5;
            key_top4 = specific.key_reg6;
            s1ram = new byte[0x8000];
            namcos1_triram = new byte[0x800];
            namcos1_paletteram = new byte[0x8000];
        }
        public static void driver_init()
        {
            switch (Machine.sName)
            {
                case "shadowld":
                case "youkaidk2":
                case "youkaidk1":
                    key_r = no_key_r;
                    key_w = no_key_w;
                    namcos1_driver_init(new namcos1_specific(0, 0, 0, 0, 0, 0, 0));
                    break;
                case "dspirit":
                case "dspirit2":
                case "dspirit1":
                    key_r = key_type1_r;
                    key_w = key_type1_w;
                    namcos1_driver_init(new namcos1_specific(0x36, 0, 0, 0, 0, 0, 0));
                    break;
                case "blazer":
                    key_r = key_type1_r;
                    key_w = key_type1_w;
                    namcos1_driver_init(new namcos1_specific(0x13, 0, 0, 0, 0, 0, 0));
                    break;
                case "quester":
                case "questers":
                    key_r = no_key_r;
                    key_w = no_key_w;
                    namcos1_driver_init(new namcos1_specific(0, 0, 0, 0, 0, 0, 0));
                    //quester_paddle_r
                    break;
                case "pacmania":
                case "pacmaniao":
                case "pacmaniaj":
                    key_r = key_type2_r;
                    key_w = key_type2_w;
                    namcos1_driver_init(new namcos1_specific(0x12, 0, 0, 0, 0, 0, 0));
                    break;
                case "galaga88":
                case "galaga88a":
                case "galaga88j":
                    key_r = key_type2_r;
                    key_w = key_type2_w;
                    namcos1_driver_init(new namcos1_specific(0x31, 0, 0, 0, 0, 0, 0));
                    break;
                case "ws":
                    key_r = key_type2_r;
                    key_w = key_type2_w;
                    namcos1_driver_init(new namcos1_specific(0x07, 0, 0, 0, 0, 0, 0));
                    break;
                case "berabohm":
                case "berabohmb":
                    key_r = no_key_r;
                    key_w = no_key_w;
                    namcos1_driver_init(new namcos1_specific(0, 0, 0, 0, 0, 0, 0));
                    //berabohm_buttons_r
                    break;
                case "mmaze":
                    key_r = key_type2_r;
                    key_w = key_type2_w;
                    namcos1_driver_init(new namcos1_specific(0x25, 0, 0, 0, 0, 0, 0));
                    break;
                case "bakutotu":
                    key_r = key_type2_r;
                    key_w = key_type2_w;
                    namcos1_driver_init(new namcos1_specific(0x22, 0, 0, 0, 0, 0, 0));
                    break;
                case "wldcourt":
                    key_r = key_type1_r;
                    key_w = key_type1_w;
                    namcos1_driver_init(new namcos1_specific(0x35, 0, 0, 0, 0, 0, 0));
                    break;
                case "splatter":
                case "splatter2":
                case "splatterj":
                    key_r = key_type3_r;
                    key_w = key_type3_w;
                    namcos1_driver_init(new namcos1_specific(181, 3, 4, -1, -1, -1, -1));
                    break;
                case "faceoff":
                    key_r = no_key_r;
                    key_w = no_key_w;
                    namcos1_driver_init(new namcos1_specific(0, 0, 0, 0, 0, 0, 0));
                    //faceoff_inputs_r
                    break;
                case "rompers":
                case "romperso":
                    key_r = key_type3_r;
                    key_w = key_type3_w;
                    namcos1_driver_init(new namcos1_specific(182, 7, -1, -1, -1, -1, -1));
                    break;
                case "blastoff":
                    key_r = key_type3_r;
                    key_w = key_type3_w;
                    namcos1_driver_init(new namcos1_specific(183, 0, 7, 3, 5, -1, -1));
                    break;
                case "ws89":
                    key_r = key_type3_r;
                    key_w = key_type3_w;
                    namcos1_driver_init(new namcos1_specific(184, 2, -1, -1, -1, -1, -1));
                    break;
                case "dangseed":
                    key_r = key_type3_r;
                    key_w = key_type3_w;
                    namcos1_driver_init(new namcos1_specific(308, 6, -1, 5, -1, 0, 4));
                    break;
                case "ws90":
                    key_r = key_type3_r;
                    key_w = key_type3_w;
                    namcos1_driver_init(new namcos1_specific(310, 4, -1, 7, -1, 3, -1));
                    break;
                case "pistoldm":
                    key_r = key_type3_r;
                    key_w = key_type3_w;
                    namcos1_driver_init(new namcos1_specific(309, 1, 2, 0, -1, 4, -1));
                    break;
                case "boxyboy":
                case "soukobdx":
                    key_r = key_type3_r;
                    key_w = key_type3_w;
                    namcos1_driver_init(new namcos1_specific(311, 2, 3, 0, -1, 4, -1));
                    break;
                case "puzlclub":
                    key_r = key_type1_r;
                    key_w = key_type1_w;
                    namcos1_driver_init(new namcos1_specific(0x35, 0, 0, 0, 0, 0, 0));
                    break;
                case "tankfrce":
                case "tankfrcej":
                    key_r = key_type3_r;
                    key_w = key_type3_w;
                    namcos1_driver_init(new namcos1_specific(185, 5, -1, 1, -1, 2, -1));
                    break;
                case "tankfrce4":
                    key_r = key_type3_r;
                    key_w = key_type3_w;
                    namcos1_driver_init(new namcos1_specific(185, 5, -1, 1, -1, 2, -1));
                    //tankfrc4_input_r
                    break;
            }
        }
    }
}
