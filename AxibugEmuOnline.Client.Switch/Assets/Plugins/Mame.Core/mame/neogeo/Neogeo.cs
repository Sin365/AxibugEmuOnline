using System;
using System.Runtime.InteropServices;

namespace MAME.Core
{
    public unsafe partial class Neogeo
    {
        private static int NEOGEO_HBEND = 0x01e;//30	/* this should really be 29.5 */
        private static int NEOGEO_HBSTART = 0x15e;//350 /* this should really be 349.5 */
        private static int NEOGEO_VTOTAL = 0x108;//264
        private static int NEOGEO_VBEND = 0x010;
        private static int NEOGEO_VBSTART = 0x0f0;//240
        private static int NEOGEO_VBLANK_RELOAD_HPOS = 0x11f;//287
        private static byte display_position_interrupt_control;
        private static uint display_counter;
        private static int vblank_interrupt_pending;
        private static int display_position_interrupt_pending;
        private static int irq3_pending;
        public static byte dsw;
        public static EmuTimer.emu_timer display_position_interrupt_timer;
        public static EmuTimer.emu_timer display_position_vblank_timer;
        public static EmuTimer.emu_timer vblank_interrupt_timer;
        private static byte controller_select;
        public static int main_cpu_bank_address;
        public static byte main_cpu_vector_table_source;
        //public static byte audio_result;
        public static byte[] audio_cpu_banks;
        public static byte[] mainbiosrom, /*mainram2,*/ audiobiosrom, fixedrom, fixedbiosrom, zoomyrom, /*spritesrom,*/ pvc_cartridge_ram;
        public static byte[] extra_ram = new byte[0x2000];
        public static uint fatfury2_prot_data;
        public static ushort neogeo_rng;
        private static byte save_ram_unlocked;
        public static bool audio_cpu_nmi_enabled, audio_cpu_nmi_pending;


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

        #region //指针化 spritesrom
        static byte[] spritesrom_src;
        static GCHandle spritesrom_handle;
        public static byte* spritesrom;
        public static int spritesromLength;
        public static bool spritesrom_IsNull => spritesrom == null;
        public static byte[] spritesrom_set
        {
            set
            {
                spritesrom_handle.ReleaseGCHandle();
                if (value == null)
                    return;
                spritesrom_src = value;
                spritesromLength = value.Length;
                spritesrom_src.GetObjectPtr(ref spritesrom_handle, ref spritesrom);
            }
        }
        #endregion

        public static void NeogeoInit()
        {
            audio_cpu_banks = new byte[4];
            pvc_cartridge_ram = new byte[0x2000];
            Memory.Set_mainram(new byte[0x10000]);
            mainram2_set = new byte[0x10000];
            Memory.Set_audioram(new byte[0x800]);
            Machine.bRom = true;
            dsw = 0xff;
            fixedbiosrom = MameMainMotion.resource.sfix;
            zoomyrom = MameMainMotion.resource._000_lo;
            audiobiosrom = MameMainMotion.resource.sm1;
            mainbiosrom = MameMainMotion.resource.mainbios;
            Memory.Set_mainrom(Machine.GetRom("maincpu.rom"));
            Memory.Set_audiorom(Machine.GetRom("audiocpu.rom"));
            fixedrom = Machine.GetRom("fixed.rom");
            FM.ymsndrom = Machine.GetRom("ymsnd.rom");
            YMDeltat.ymsnddeltatrom = Machine.GetRom("ymsnddeltat.rom");
            spritesrom_set = Machine.GetRom("sprites.rom");
            if (fixedbiosrom == null || zoomyrom == null || audiobiosrom == null || mainbiosrom == null || Memory.mainrom_IsNull || Memory.audiorom_IsNull || fixedrom == null || FM.ymsndrom == null || spritesrom == null)
            {
                Machine.bRom = false;
            }
            if (Machine.bRom)
            {
                switch (Machine.sName)
                {
                    case "irrmaze":
                    case "kizuna4p":
                        mainbiosrom = Machine.GetRom("mainbios.rom");
                        break;
                    case "kof99":
                    case "kof99h":
                    case "kof99e":
                    case "kof99k":
                    case "garou":
                    case "garouh":
                    case "mslug3":
                    case "mslug3h":
                    case "mslug4":
                    case "mslug4h":
                    case "ms4plus":
                    case "ganryu":
                    case "s1945p":
                    case "preisle2":
                    case "bangbead":
                    case "nitd":
                    case "zupapa":
                    case "sengoku3":
                    case "rotd":
                    case "rotdh":
                    case "pnyaa":
                    case "mslug5":
                    case "mslug5h":
                    case "ms5plus":
                    case "samsho5":
                    case "samsho5h":
                    case "samsho5b":
                    case "samsh5sp":
                    case "samsh5sph":
                    case "samsh5spho":
                    case "jockeygp":
                    case "jockeygpa":
                        neogeo_fixed_layer_bank_type = 1;
                        break;
                    case "kof2000":
                    case "kof2000n":
                    case "matrim":
                    case "matrimbl":
                    case "svc":
                    case "kof2003":
                    case "kof2003h":
                        neogeo_fixed_layer_bank_type = 2;
                        break;
                    default:
                        neogeo_fixed_layer_bank_type = 0;
                        break;
                }
            }
        }
        private static void adjust_display_position_interrupt_timer()
        {
            if ((display_counter + 1) != 0)
            {
                Atime period = Attotime.attotime_mul(new Atime(0, Attotime.ATTOSECONDS_PER_SECOND / 6000000), display_counter + 1);
                EmuTimer.timer_adjust_periodic(display_position_interrupt_timer, period, Attotime.ATTOTIME_NEVER);
            }
        }
        private static void update_interrupts()
        {
            int level = 0;
            if (vblank_interrupt_pending != 0)
            {
                level = 1;
            }
            if (display_position_interrupt_pending != 0)
            {
                level = 2;
            }
            if (irq3_pending != 0)
            {
                level = 3;
            }
            if (level == 1)
            {
                Cpuint.cpunum_set_input_line(0, 1, LineState.ASSERT_LINE);
            }
            else if (level == 2)
            {
                Cpuint.cpunum_set_input_line(0, 2, LineState.ASSERT_LINE);
            }
            else if (level == 3)
            {
                Cpuint.cpunum_set_input_line(0, 3, LineState.ASSERT_LINE);
            }
            else
            {
                Cpuint.cpunum_set_input_line(0, 7, LineState.CLEAR_LINE);
            }
        }
        public static void display_position_interrupt_callback()
        {
            if ((display_position_interrupt_control & 0x10) != 0)
            {
                display_position_interrupt_pending = 1;
                update_interrupts();
            }
            if ((display_position_interrupt_control & 0x80) != 0)
            {
                adjust_display_position_interrupt_timer();
            }
        }
        public static void display_position_vblank_callback()
        {
            if ((display_position_interrupt_control & 0x40) != 0)
            {
                adjust_display_position_interrupt_timer();
            }
            EmuTimer.timer_adjust_periodic(display_position_vblank_timer, Video.video_screen_get_time_until_pos(NEOGEO_VBSTART, NEOGEO_VBLANK_RELOAD_HPOS), Attotime.ATTOTIME_NEVER);
        }
        public static void vblank_interrupt_callback()
        {
            calendar_clock();
            vblank_interrupt_pending = 1;
            update_interrupts();
            EmuTimer.timer_adjust_periodic(vblank_interrupt_timer, Video.video_screen_get_time_until_pos(NEOGEO_VBSTART, 0), Attotime.ATTOTIME_NEVER);
        }
        public static void audio_cpu_irq(int assert)
        {
            Cpuint.cpunum_set_input_line(1, 0, assert != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
        }
        private static void select_controller(byte data)
        {
            controller_select = data;
        }
        public static void io_control_w(int offset, byte data)
        {
            switch (offset)
            {
                case 0x00: select_controller(data); break;
                //	case 0x18: set_output_latch(data & 0x00ff); break;
                //	case 0x20: set_output_data(data & 0x00ff); break;
                case 0x28: Pd4900a.pd4990a_control_16_w(data); break;
                //  case 0x30: break; // coin counters
                //  case 0x31: break; // coin counters
                //  case 0x32: break; // coin lockout
                //  case 0x33: break; // coui lockout
                default:
                    break;
            }
        }
        private static void calendar_init()
        {
            //DateTime time = DateTime.Now;
            DateTime time = DateTime.Parse("1970-1-1");
            Pd4900a.pd4990a.seconds = ((time.Second / 10) << 4) + (time.Second % 10);
            Pd4900a.pd4990a.minutes = ((time.Minute / 10) << 4) + (time.Minute % 10);
            Pd4900a.pd4990a.hours = ((time.Hour / 10) << 4) + (time.Hour % 10);
            Pd4900a.pd4990a.days = ((time.Day / 10) << 4) + (time.Day % 10);
            Pd4900a.pd4990a.month = time.Month;
            Pd4900a.pd4990a.year = ((((time.Year - 1900) % 100) / 10) << 4) + ((time.Year - 1900) % 10);
            Pd4900a.pd4990a.weekday = (int)time.DayOfWeek;
        }
        public static void calendar_clock()
        {
            Pd4900a.pd4990a_addretrace();
        }
        public static uint get_calendar_status()
        {
            uint i1 = (uint)((Pd4900a.outputbit << 1) | Pd4900a.testbit);
            return i1;
        }
        public static void save_ram_w(int offset, byte data)
        {
            if (save_ram_unlocked != 0)
                mainram2[offset] = data;
        }
        public static void audio_cpu_check_nmi()
        {
            Cpuint.cpunum_set_input_line(1, (int)LineState.INPUT_LINE_NMI, (audio_cpu_nmi_enabled && audio_cpu_nmi_pending) ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
        }
        public static void audio_cpu_enable_nmi_w(int offset)
        {
            audio_cpu_nmi_enabled = (offset & 0x10) == 0;
            audio_cpu_check_nmi();
        }
        public static void audio_command_w(byte data)
        {
            Sound.soundlatch_w(data);
            audio_cpu_nmi_pending = true;
            audio_cpu_check_nmi();
            Cpuexec.cpu_boost_interleave(Attotime.ATTOTIME_ZERO, new Atime(0, (long)(50 * 1e12)));
        }
        public static byte audio_command_r()
        {
            byte ret = (byte)Sound.soundlatch_r();
            audio_cpu_nmi_pending = false;
            audio_cpu_check_nmi();
            return ret;
        }
        public static void audio_result_w(byte data)
        {
            Sound.soundlatch2_w(data);
        }
        public static byte get_audio_result()
        {
            return (byte)Sound.soundlatch2_r();
        }
        public static void main_cpu_bank_select_w(int data)
        {
            int bank_address;
            int len = Memory.mainromLength;
            if ((len <= 0x100000) && ((data & 0x07) != 0))
            {
                int i1 = 1;
            }
            else
            {
                bank_address = ((data & 0x07) + 1) * 0x100000;
                if (bank_address >= len)
                {
                    bank_address = 0x100000;
                }
                main_cpu_bank_address = bank_address;
            }
        }
        public static void system_control_w(int offset)
        {
            //if (ACCESSING_BITS_0_7)
            {
                byte bit = (byte)((offset >> 3) & 0x01);
                switch (offset & 0x07)
                {
                    default:
                    case 0x00:
                        neogeo_set_screen_dark(bit);
                        break;
                    case 0x01:
                        main_cpu_vector_table_source = bit;
                        break;
                    case 0x05:
                        fixed_layer_source = bit;
                        break;
                    case 0x06:
                        save_ram_unlocked = bit;
                        break;
                    case 0x07:
                        neogeo_set_palette_bank(bit);
                        break;
                    case 0x02: /* unknown - HC32 middle pin 1 */
                    case 0x03: /* unknown - uPD4990 pin ? */
                    case 0x04: /* unknown - HC32 middle pin 10 */
                        break;
                }
            }
        }
        public static void watchdog_w()
        {
            Watchdog.watchdog_reset();
        }
        public static void machine_start_neogeo()
        {
            if (Memory.mainromLength > 0x100000)
            {
                main_cpu_bank_address = 0x100000;
            }
            else
            {
                main_cpu_bank_address = 0x000000;
            }
            audio_cpu_banks[0] = 0x1e;
            audio_cpu_banks[1] = 0x0e;
            audio_cpu_banks[2] = 0x06;
            audio_cpu_banks[3] = 0x02;
            display_position_interrupt_timer = EmuTimer.timer_alloc_common( EmuTimer.TIME_ACT.Neogeo_display_position_interrupt_callback, false);
            display_position_vblank_timer = EmuTimer.timer_alloc_common( EmuTimer.TIME_ACT.Neogeo_display_position_vblank_callback, false);
            vblank_interrupt_timer = EmuTimer.timer_alloc_common( EmuTimer.TIME_ACT.Neogeo_vblank_interrupt_callback, false);
            Pd4900a.pd4990a_init();
            calendar_init();
            irq3_pending = 1;
        }

        public static void nvram_handler_load_neogeo()
        {
            if (MameMainMotion.IoSupport.File_Exists("nvram\\" + Machine.sName + ".nv"))
            {
                MameMainMotion.IoSupport.File_ReadAllBytes("nvram\\" + Machine.sName + ".nv");
                //FileStream fs1 = new FileStream("nvram\\" + Machine.sName + ".nv", FileMode.Open);
                //int n = (int)fs1.Length;
                //fs1.Read(mainram2, 0, n);
                //fs1.Close();
            }
        }
        public static void nvram_handler_save_neogeo()
        {
            //FileStream fs1 = new FileStream("nvram\\" + Machine.sName + ".nv", FileMode.Create);
            //fs1.Write(mainram2, 0, 0x2000);
            //fs1.Close();

            byte[] temp = new byte[0x2000];
            Buffer.BlockCopy(mainram2_src, 0, temp, 0, temp.Length);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(temp))
            { 
                MameMainMotion.IoSupport.File_WriteAllBytesFromStre("nvram\\" + Machine.sName + ".nv",ms);
            }
        }
        public static void machine_reset_neogeo()
        {
            int offs;
            for (offs = 0; offs < 8; offs++)
                system_control_w(offs);
            audio_cpu_nmi_enabled = false;
            audio_cpu_nmi_pending = false;
            audio_cpu_check_nmi();
            EmuTimer.timer_adjust_periodic(vblank_interrupt_timer, Video.video_screen_get_time_until_pos(NEOGEO_VBSTART, 0), Attotime.ATTOTIME_NEVER);
            EmuTimer.timer_adjust_periodic(display_position_vblank_timer, Video.video_screen_get_time_until_pos(NEOGEO_VBSTART, NEOGEO_VBLANK_RELOAD_HPOS), Attotime.ATTOTIME_NEVER);
            update_interrupts();
            start_sprite_line_timer();
            start_auto_animation_timer();
        }
    }
}
