using System;
using System.Runtime.InteropServices;
using static MAME.Core.EmuTimer;

namespace MAME.Core
{
    public unsafe partial class Taitob
    {
        //public static byte[] /*gfxrom,*/ /*gfx0rom,*/ /*gfx1rom,*/ mainram2, mainram3;

        #region //指针化 gfxrom
        static byte[] gfxrom_src;
        static GCHandle gfxrom_handle;
        public static byte* gfxrom;
        public static int gfxromLength;
        public static bool gfxrom_IsNull => gfxrom == null;
        public static byte[] gfxrom_set
        {
            set
            {
                gfxrom_handle.ReleaseGCHandle();
                gfxrom_src = value;
                gfxromLength = value.Length;
                gfxrom_src.GetObjectPtr(ref gfxrom_handle, ref gfxrom);
            }
        }
        #endregion

        #region //指针化 gfx0rom
        static byte[] gfx0rom_src;
        static GCHandle gfx0rom_handle;
        public static byte* gfx0rom;
        public static int gfx0romLength;
        public static bool gfx0rom_IsNull => gfx0rom == null;
        public static byte[] gfx0rom_set
        {
            set
            {
                gfx0rom_handle.ReleaseGCHandle();
                gfx0rom_src = value;
                gfx0romLength = value.Length;
                gfx0rom_src.GetObjectPtr(ref gfx0rom_handle, ref gfx0rom);
            }
        }
        #endregion

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

        #region //指针化 mainram2
        static byte[] mainram2_src;
        static GCHandle mainram2_handle;
        public static byte* mainram2;
        public static int mainram2Length;
        public static bool mainram2_IsNull => mainram2 == null;
        public static byte[] mainram2_set
        {
            set
            {
                mainram2_handle.ReleaseGCHandle();
                mainram2_src = value;
                mainram2Length = value.Length;
                mainram2_src.GetObjectPtr(ref mainram2_handle, ref mainram2);
            }
        }
        #endregion

        #region //指针化 mainram3
        static byte[] mainram3_src;
        static GCHandle mainram3_handle;
        public static byte* mainram3;
        public static int mainram3Length;
        public static bool mainram3_IsNull => mainram3 == null;
        public static byte[] mainram3_set
        {
            set
            {
                mainram3_handle.ReleaseGCHandle();
                mainram3_src = value;
                mainram3Length = value.Length;
                mainram3_src.GetObjectPtr(ref mainram3_handle, ref mainram3);
            }
        }
        #endregion


        public static ushort eep_latch;
        public static ushort coin_word;
        public static int basebanksnd;
        public static byte dswa, dswb, dswb_old;
        public static void TaitobInit()
        {
            int i, n;
            Generic.paletteram16_set = new ushort[0x1000];
            TC0180VCU_ram = new ushort[0x8000];
            TC0180VCU_ctrl = new ushort[0x10];
            TC0220IOC_regs = new byte[8];
            TC0220IOC_port = 0;
            TC0640FIO_regs = new byte[8];
            taitob_scroll = new ushort[0x400];
            Memory.Set_mainram(new byte[0x10000]);
            mainram2_set = new byte[0x1e80];
            mainram3_set = new byte[0x2000];
            Memory.Set_audioram(new byte[0x2000]);
            bg_rambank = new ushort[2];
            fg_rambank = new ushort[2];
            pixel_scroll = new ushort[2];
            taitob_spriteram = new ushort[0xcc0];
            TC0640FIO_regs = new byte[8];
            Machine.bRom = true;
            Taitosnd.taitosnd_start();
            basebanksnd = 0x10000;
            eep_latch = 0;
            video_control = 0;
            coin_word = 0;
            for (i = 0; i < 0x10; i++)
            {
                TC0180VCU_ctrl[i] = 0;
            }
            Machine.bRom = true;
            Memory.Set_mainrom(Machine.GetRom("maincpu.rom"));
            //Memory.Set_audiorom(Machine.GetRom("audiocpu.rom"));
            Memory.Set_audiorom(Machine.GetRom("audiocpu.rom"));
            gfxrom_set = Machine.GetRom("gfx1.rom");
            n = gfxromLength;
            gfx0rom_set = new byte[n * 2];
            gfx1rom_set = new byte[n * 2];
            for (i = 0; i < n; i++)
            {
                gfx1rom[i * 2] = (byte)(gfxrom[i] >> 4);
                gfx1rom[i * 2 + 1] = (byte)(gfxrom[i] & 0x0f);
            }
            for (i = 0; i < n; i++)
            {
                gfx0rom[((i / 0x10) % 8 + (i / 0x80 * 0x10) + ((i / 8) % 2) * 8) * 8 + (i % 8)] = gfx1rom[i];
            }
            FM.ymsndrom = Machine.GetRom("ymsnd.rom");
            YMDeltat.ymsnddeltatrom = Machine.GetRom("ymsnddeltat.rom");
            if (Memory.mainrom_IsNull || gfxrom == null || Memory.audiorom_IsNull || FM.ymsndrom == null)
            {
                Machine.bRom = false;
            }
            if (Machine.bRom)
            {
                switch (Machine.sName)
                {
                    case "pbobble":
                        dswa = 0xff;
                        dswb = 0xff;
                        break;
                    case "silentd":
                    case "silentdj":
                    case "silentdu":
                        dswa = 0xff;
                        dswb = 0xbf;
                        break;
                }
            }
        }
        public static void irqhandler(int irq)
        {
            Cpuint.cpunum_set_input_line(1, 0, irq != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
        }
        public static void bankswitch_w(byte data)
        {
            basebanksnd = 0x10000 + 0x4000 * ((data - 1) & 3);
        }
        public static void rsaga2_interrupt2()
        {
            Cpuint.cpunum_set_input_line(0, 2, LineState.HOLD_LINE);
        }
        public static void rastansaga2_interrupt()
        {
            EmuTimer.emu_timer timer = EmuTimer.timer_alloc_common(TIME_ACT.Taitob_rsaga2_interrupt2, true);
            EmuTimer.timer_adjust_periodic(timer, new Atime(0, (long)(5000 * Cpuexec.cpu[0].attoseconds_per_cycle)), Attotime.ATTOTIME_NEVER);
            Cpuint.cpunum_set_input_line(0, 4, LineState.HOLD_LINE);
        }
        public static void crimec_interrupt3()
        {
            Cpuint.cpunum_set_input_line(0, 3, LineState.HOLD_LINE);
        }
        public static void crimec_interrupt()
        {
            EmuTimer.emu_timer timer = EmuTimer.timer_alloc_common(TIME_ACT.Taitob_crimec_interrupt3, true);
            EmuTimer.timer_adjust_periodic(timer, new Atime(0, (long)(5000 * Cpuexec.cpu[0].attoseconds_per_cycle)), Attotime.ATTOTIME_NEVER);
            Cpuint.cpunum_set_input_line(0, 5, LineState.HOLD_LINE);
        }
        public static void hitice_interrupt6()
        {
            Cpuint.cpunum_set_input_line(0, 6, LineState.HOLD_LINE);
        }
        public static void hitice_interrupt()
        {
            EmuTimer.emu_timer timer = EmuTimer.timer_alloc_common(TIME_ACT.Taitob_hitice_interrupt6, true);
            EmuTimer.timer_adjust_periodic(timer, new Atime(0, (long)(5000 * Cpuexec.cpu[0].attoseconds_per_cycle)), Attotime.ATTOTIME_NEVER);
            Cpuint.cpunum_set_input_line(0, 4, LineState.HOLD_LINE);
        }
        public static void rambo3_interrupt1()
        {
            Cpuint.cpunum_set_input_line(0, 1, LineState.HOLD_LINE);
        }
        public static void rambo3_interrupt()
        {
            EmuTimer.emu_timer timer = EmuTimer.timer_alloc_common(TIME_ACT.Taitob_rambo3_interrupt1, true);
            EmuTimer.timer_adjust_periodic(timer, new Atime(0, (long)(5000 * Cpuexec.cpu[0].attoseconds_per_cycle)), Attotime.ATTOTIME_NEVER);
            Cpuint.cpunum_set_input_line(0, 6, LineState.HOLD_LINE);
        }
        public static void pbobble_interrupt5()
        {
            Cpuint.cpunum_set_input_line(0, 5, LineState.HOLD_LINE);
        }
        public static void pbobble_interrupt()
        {
            EmuTimer.emu_timer timer = EmuTimer.timer_alloc_common(TIME_ACT.Taitob_pbobble_interrupt5, true);
            EmuTimer.timer_adjust_periodic(timer, new Atime(0, (long)(5000 * Cpuexec.cpu[0].attoseconds_per_cycle)), Attotime.ATTOTIME_NEVER);
            Cpuint.cpunum_set_input_line(0, 3, LineState.HOLD_LINE);
        }
        public static void viofight_interrupt1()
        {
            Cpuint.cpunum_set_input_line(0, 1, LineState.HOLD_LINE);
        }
        public static void viofight_interrupt()
        {
            EmuTimer.emu_timer timer = EmuTimer.timer_alloc_common(TIME_ACT.Taitob_viofight_interrupt1, true);
            EmuTimer.timer_adjust_periodic(timer, new Atime(0, (long)(5000 * Cpuexec.cpu[0].attoseconds_per_cycle)), Attotime.ATTOTIME_NEVER);
            Cpuint.cpunum_set_input_line(0, 4, LineState.HOLD_LINE);
        }
        public static void masterw_interrupt4()
        {
            Cpuint.cpunum_set_input_line(0, 4, LineState.HOLD_LINE);
        }
        public static void masterw_interrupt()
        {
            EmuTimer.emu_timer timer = EmuTimer.timer_alloc_common(TIME_ACT.Taitob_masterw_interrupt4, true);
            EmuTimer.timer_adjust_periodic(timer, new Atime(0, (long)(5000 * Cpuexec.cpu[0].attoseconds_per_cycle)), Attotime.ATTOTIME_NEVER);
            Cpuint.cpunum_set_input_line(0, 5, LineState.HOLD_LINE);
        }
        public static void silentd_interrupt4()
        {
            Cpuint.cpunum_set_input_line(0, 4, LineState.HOLD_LINE);
        }
        public static void silentd_interrupt()
        {
            EmuTimer.emu_timer timer = EmuTimer.timer_alloc_common(TIME_ACT.Taitob_silentd_interrupt4, true);
            EmuTimer.timer_adjust_periodic(timer, new Atime(0, (long)(5000 * Cpuexec.cpu[0].attoseconds_per_cycle)), Attotime.ATTOTIME_NEVER);
            Cpuint.cpunum_set_input_line(0, 6, LineState.HOLD_LINE);
        }
        public static void selfeena_interrupt4()
        {
            Cpuint.cpunum_set_input_line(0, 4, LineState.HOLD_LINE);
        }
        public static void selfeena_interrupt()
        {
            EmuTimer.emu_timer timer = EmuTimer.timer_alloc_common(TIME_ACT.Taitob_selfeena_interrupt4, true);
            EmuTimer.timer_adjust_periodic(timer, new Atime(0, (long)(5000 * Cpuexec.cpu[0].attoseconds_per_cycle)), Attotime.ATTOTIME_NEVER);
            Cpuint.cpunum_set_input_line(0, 6, LineState.HOLD_LINE);
        }
        public static void sbm_interrupt5()
        {
            Cpuint.cpunum_set_input_line(0, 5, LineState.HOLD_LINE);
        }
        public static void sbm_interrupt()
        {
            EmuTimer.emu_timer timer = EmuTimer.timer_alloc_common(TIME_ACT.Taitob_sbm_interrupt5, true);
            EmuTimer.timer_adjust_periodic(timer, new Atime(0, (long)(10000 * Cpuexec.cpu[0].attoseconds_per_cycle)), Attotime.ATTOTIME_NEVER);
            Cpuint.cpunum_set_input_line(0, 4, LineState.HOLD_LINE);
        }
        public static void mb87078_gain_changed(int channel, int percent)
        {
            if (channel == 1)
            {
                AY8910.AA8910[0].stream.gain = (int)(0x100 * (percent / 100.0));
                //sound_type type = Machine->config->sound[0].type;
                //sndti_set_output_gain(type, 0, 0, percent / 100.0);
                //sndti_set_output_gain(type, 1, 0, percent / 100.0);
                //sndti_set_output_gain(type, 2, 0, percent / 100.0);
            }
        }
        public static void machine_reset_mb87078()
        {
            MB87078_start(0);
        }
        public static void gain_control_w1(int offset, byte data)
        {
            if (offset == 0)
            {
                MB87078_data_w(0, data, 0);
            }
            else
            {
                MB87078_data_w(0, data, 1);
            }
        }
        public static void gain_control_w(int offset, ushort data)
        {
            if (offset == 0)
            {
                MB87078_data_w(0, data >> 8, 0);
            }
            else
            {
                MB87078_data_w(0, data >> 8, 1);
            }
        }
        public static void nvram_handler_load_taitob()
        {

        }
        public static void nvram_handler_save_taitob()
        {

        }
        public static ushort eeprom_r()
        {
            ushort res;
            res = (ushort)(Eeprom.eeprom_read_bit() & 0x01);
            res |= (ushort)(dswb & 0xfe);
            return res;
        }
        public static ushort eep_latch_r()
        {
            return eep_latch;
        }
        public static void eeprom_w1(byte data)
        {
            eep_latch = (ushort)((data << 8) | (eep_latch & 0xff));
            Eeprom.eeprom_write_bit(data & 0x04);
            Eeprom.eeprom_set_clock_line(((data & 0x08) != 0) ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
            Eeprom.eeprom_set_cs_line(((data & 0x10) != 0) ? LineState.CLEAR_LINE : LineState.ASSERT_LINE);
        }
        public static void eeprom_w2(byte data)
        {
            eep_latch = (ushort)((eep_latch & 0xff00) | data);
        }
        public static void eeprom_w(ushort data)
        {
            eep_latch = data;
            data >>= 8;
            Eeprom.eeprom_write_bit(data & 0x04);
            Eeprom.eeprom_set_clock_line(((data & 0x08) != 0) ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
            Eeprom.eeprom_set_cs_line(((data & 0x10) != 0) ? LineState.CLEAR_LINE : LineState.ASSERT_LINE);
        }
        public static void player_34_coin_ctrl_w(ushort data)
        {
            coin_word = data;
            //coin_lockout_w(2, ~data & 0x0100);
            //coin_lockout_w(3, ~data & 0x0200);
            //coin_counter_w(2, data & 0x0400);
            //coin_counter_w(3, data & 0x0800);
        }
        public static ushort pbobble_input_bypass_r(int offset)
        {
            ushort result = 0;
            switch (offset)
            {
                case 0x01:
                    result = (ushort)(eeprom_r() << 8);
                    break;
                default:
                    result = (ushort)(TC0640FIO_r(offset) << 8);
                    break;
            }
            return result;
        }
    }
}
