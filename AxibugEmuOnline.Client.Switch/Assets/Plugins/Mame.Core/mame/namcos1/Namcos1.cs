using System;
using System.Runtime.InteropServices;

namespace MAME.Core
{
    public unsafe partial class Namcos1
    {
        public static int dac0_value, dac1_value, dac0_gain, dac1_gain;
        //public static byte[] /*gfx1rom,*/ /*gfx2rom,*/ gfx3rom, user1rom, mcurom;
        //public static byte[] audiorom, voicerom, bank_ram20, bank_ram30;
        public static int namcos1_pri;
        public static byte dipsw;

        #region //指针化 gfx1rom
        static byte[] gfx1rom_src;
        static GCHandle gfx1rom_handle;
        public static byte* gfx1rom;
        public static int gfx1romLength;
        public static bool gfx1rom_IsNull => gfx1rom == null;
        public static byte[] gfx1rom_set
        {
            set
            {
                gfx1rom_handle.ReleaseGCHandle();
                gfx1rom_src = value;
                gfx1romLength = value.Length;
                gfx1rom_src.GetObjectPtr(ref gfx1rom_handle, ref gfx1rom);
            }
        }
        #endregion

        #region //指针化 gfx2rom
        static byte[] gfx2rom_src;
        static GCHandle gfx2rom_handle;
        public static byte* gfx2rom;
        public static int gfx2romLength;
        public static bool gfx2rom_IsNull => gfx2rom == null;
        public static byte[] gfx2rom_set
        {
            set
            {
                gfx2rom_handle.ReleaseGCHandle();
                gfx2rom_src = value;
                gfx2romLength = value.Length;
                gfx2rom_src.GetObjectPtr(ref gfx2rom_handle, ref gfx2rom);
            }
        }
        #endregion


        #region //指针化 gfx3rom
        static byte[] gfx3rom_src;
        static GCHandle gfx3rom_handle;
        public static byte* gfx3rom;
        public static int gfx3romLength;
        public static bool gfx3rom_IsNull => gfx3rom == null;
        public static byte[] gfx3rom_set
        {
            set
            {
                gfx3rom_handle.ReleaseGCHandle();
                gfx3rom_src = value;
                gfx3romLength = value.Length;
                gfx3rom_src.GetObjectPtr(ref gfx3rom_handle, ref gfx3rom);
            }
        }
        #endregion

        #region //指针化 user1rom
        static byte[] user1rom_src;
        static GCHandle user1rom_handle;
        public static byte* user1rom;
        public static int user1romLength;
        public static bool user1rom_IsNull => user1rom == null;
        public static byte[] user1rom_set
        {
            set
            {
                user1rom_handle.ReleaseGCHandle();
                if (value == null)
                    return;
                user1rom_src = value;
                user1romLength = value.Length;
                user1rom_src.GetObjectPtr(ref user1rom_handle, ref user1rom);
            }
        }
        #endregion

        #region //指针化 mcurom
        static byte[] mcurom_src;
        static GCHandle mcurom_handle;
        public static byte* mcurom;
        public static int mcuromLength;
        public static bool mcurom_IsNull => mcurom == null;
        public static byte[] mcurom_set
        {
            set
            {
                mcurom_handle.ReleaseGCHandle();
                mcurom_src = value;
                mcuromLength = value.Length;
                mcurom_src.GetObjectPtr(ref mcurom_handle, ref mcurom);
            }
        }
        #endregion


        #region //指针化 audiorom
        static byte[] audiorom_src;
        static GCHandle audiorom_handle;
        public static byte* audiorom;
        public static int audioromLength;
        public static bool audiorom_IsNull => audiorom == null;
        public static byte[] audiorom_set
        {
            set
            {
                audiorom_handle.ReleaseGCHandle();
                audiorom_src = value;
                audioromLength = value.Length;
                audiorom_src.GetObjectPtr(ref audiorom_handle, ref audiorom);
            }
        }
        #endregion

        #region //指针化 voicerom
        static byte[] voicerom_src;
        static GCHandle voicerom_handle;
        public static byte* voicerom;
        public static int voiceromLength;
        public static bool voicerom_IsNull => voicerom == null;
        public static byte[] voicerom_set
        {
            set
            {
                voicerom_handle.ReleaseGCHandle();
                if (value == null)
                    return;
                voicerom_src = value;
                voiceromLength = value.Length;
                voicerom_src.GetObjectPtr(ref voicerom_handle, ref voicerom);
            }
        }
        #endregion

        #region //指针化 bank_ram20
        static byte[] bank_ram20_src;
        static GCHandle bank_ram20_handle;
        public static byte* bank_ram20;
        public static int bank_ram20Length;
        public static bool bank_ram20_IsNull => bank_ram20 == null;
        public static byte[] bank_ram20_set
        {
            set
            {
                bank_ram20_handle.ReleaseGCHandle();
                bank_ram20_src = value;
                bank_ram20Length = value.Length;
                bank_ram20_src.GetObjectPtr(ref bank_ram20_handle, ref bank_ram20);
            }
        }
        #endregion


        #region //指针化 bank_ram30
        static byte[] bank_ram30_src;
        static GCHandle bank_ram30_handle;
        public static byte* bank_ram30;
        public static int bank_ram30Length;
        public static bool bank_ram30_IsNull => bank_ram30 == null;
        public static byte[] bank_ram30_set
        {
            set
            {
                bank_ram30_handle.ReleaseGCHandle();
                bank_ram30_src = value;
                bank_ram30Length = value.Length;
                bank_ram30_src.GetObjectPtr(ref bank_ram30_handle, ref bank_ram30);
            }
        }
        #endregion

        public static byte[] ByteTo2byte(byte[] bb1)
        {
            byte[] bb2 = null;
            int i1, n1;
            if (bb1 != null)
            {
                n1 = bb1.Length;
                bb2 = new byte[n1 * 2];
                for (i1 = 0; i1 < n1; i1++)
                {
                    bb2[i1 * 2] = (byte)(bb1[i1] >> 4);
                    bb2[i1 * 2 + 1] = (byte)(bb1[i1] & 0x0f);
                }
            }
            return bb2;
        }
        public static void Namcos1Init()
        {
            Machine.bRom = true;
            user1rom_offset = new int[2, 8];
            audiorom_set = Machine.GetRom("audiocpu.rom");
            gfx1rom_set = Machine.GetRom("gfx1.rom");
            gfx2rom_set = Machine.GetRom("gfx2.rom");
            gfx3rom_set = ByteTo2byte(Machine.GetRom("gfx3.rom"));
            user1rom_set = Machine.GetRom("user1.rom");
            mcurom_set = MameMainMotion.resource.mcu;
            voicerom_set = new byte[0xc0000];
            //byte[] bb1 = Machine.GetRom("voice.rom");
            //AxiArray.Copy(bb1, voicerom, bb1.Length);
            voicerom_set = Machine.GetRom("voice.rom");
            bank_ram20_set = new byte[0x2000];
            bank_ram30_set = new byte[0x80];
            Namco.namco_wavedata = new byte[0x400];
            Generic.generic_nvram_set = new byte[0x800];
            cus117_offset = new int[2, 8];
            key = new byte[8];
            if (audiorom == null || gfx1rom == null || gfx2rom == null || gfx3rom == null || user1rom == null || voicerom == null)
            {
                Machine.bRom = false;
            }
            if (Machine.bRom)
            {
                switch (Machine.sName)
                {
                    case "quester":
                    case "questers":
                        dipsw = 0xfb;
                        break;
                    default:
                        dipsw = 0xff;
                        break;
                }
            }
        }
        public static void namcos1_sub_firq_w()
        {
            Cpuint.cpunum_set_input_line(1, 1, LineState.ASSERT_LINE);
        }
        public static void irq_ack_w(int cpunum)
        {
            Cpuint.cpunum_set_input_line(cpunum, 0, LineState.CLEAR_LINE);
        }
        public static void firq_ack_w(int cpunum)
        {
            Cpuint.cpunum_set_input_line(cpunum, 1, LineState.CLEAR_LINE);
        }
        public static byte dsw_r(int offset)
        {
            int ret = dipsw;// 0xff;// input_port_read(machine, "DIPSW");
            if ((offset & 2) == 0)
            {
                ret >>= 4;
            }
            return (byte)(0xf0 | ret);
        }
        public static void namcos1_coin_w(byte data)
        {
            Generic.coin_lockout_global_w(~data & 1);
            Generic.coin_counter_w(0, data & 2);
            Generic.coin_counter_w(1, data & 4);
        }
        public static void namcos1_update_DACs()
        {
            DAC.dac_signed_data_16_w(0, (ushort)(0x8000 + (dac0_value * dac0_gain) + (dac1_value * dac1_gain)));
        }
        public static void namcos1_init_DACs()
        {
            dac0_value = 0;
            dac1_value = 0;
            dac0_gain = 0x80;
            dac1_gain = 0x80;
        }
        public static void namcos1_dac_gain_w(byte data)
        {
            int value;
            value = (data & 1) | ((data >> 1) & 2);
            dac0_gain = 0x20 * (value + 1);
            value = (data >> 3) & 3;
            dac1_gain = 0x20 * (value + 1);
            namcos1_update_DACs();
        }
        public static void namcos1_dac0_w(byte data)
        {
            dac0_value = data - 0x80;
            namcos1_update_DACs();
        }
        public static void namcos1_dac1_w(byte data)
        {
            dac1_value = data - 0x80;
            namcos1_update_DACs();
        }
    }
}
