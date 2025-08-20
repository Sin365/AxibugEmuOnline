using System;
using System.Runtime.InteropServices;

namespace MAME.Core
{
    public unsafe partial class M72
    {
        public static byte[] protection_ram;
        public static EmuTimer.emu_timer scanline_timer;
        public static byte m72_irq_base;
        public static int m72_scanline_param;

        //public static byte[] spritesrom, sprites1rom, samplesrom /*,gfx2rom,*/ /*gfx21rom,*/ /*gfx3rom/*, gfx31rom*/;


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


        #region //指针化 sprites1rom
        static byte[] sprites1rom_src;
        static GCHandle sprites1rom_handle;
        public static byte* sprites1rom;
        public static int sprites1romLength;
        public static bool sprites1rom_IsNull => sprites1rom == null;
        public static byte[] sprites1rom_set
        {
            set
            {
                sprites1rom_handle.ReleaseGCHandle();
                if (value == null)
                    return;
                sprites1rom_src = value;
                sprites1romLength = value.Length;
                sprites1rom_src.GetObjectPtr(ref sprites1rom_handle, ref sprites1rom);
            }
        }
        #endregion

        #region //指针化 samplesrom
        static byte[] samplesrom_src;
        static GCHandle samplesrom_handle;
        public static byte* samplesrom;
        public static int samplesromLength;
        public static bool samplesrom_IsNull => samplesrom == null;
        public static byte[] samplesrom_set
        {
            set
            {
                samplesrom_handle.ReleaseGCHandle();
                if (value == null)
                    return;
                samplesrom_src = value;
                samplesromLength = value.Length;
                samplesrom_src.GetObjectPtr(ref samplesrom_handle, ref samplesrom);
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


        #region //指针化 gfx21rom
        static byte[] gfx21rom_src;
        static GCHandle gfx21rom_handle;
        public static byte* gfx21rom;
        public static int gfx21romLength;
        public static bool gfx21rom_IsNull => gfx21rom == null;
        public static byte[] gfx21rom_set
        {
            set
            {
                gfx21rom_handle.ReleaseGCHandle();
                gfx21rom_src = value;
                gfx21romLength = value.Length;
                gfx21rom_src.GetObjectPtr(ref gfx21rom_handle, ref gfx21rom);
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
                if (value == null)
                    return;
                gfx3rom_src = value;
                gfx3romLength = value.Length;
                gfx3rom_src.GetObjectPtr(ref gfx3rom_handle, ref gfx3rom);
            }
        }
        #endregion



        #region //指针化 gfx31rom
        static byte[] gfx31rom_src;
        static GCHandle gfx31rom_handle;
        public static byte* gfx31rom;
        public static int gfx31romLength;
        public static bool gfx31rom_IsNull => gfx31rom == null;
        public static byte[] gfx31rom_set
        {
            set
            {
                gfx31rom_handle.ReleaseGCHandle();
                gfx31rom_src = value;
                gfx31romLength = value.Length;
                gfx31rom_src.GetObjectPtr(ref gfx31rom_handle, ref gfx31rom);
            }
        }
        #endregion


        public static byte[] airduelm72_code = new byte[] {
            0x68, 0x00, 0xd0, 0x1f, 0xc6, 0x06, 0xc0, 0x1c, 0x57, 0xea, 0x69, 0x0b, 0x00, 0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
        };
        public static byte[] gunforce_decryption_table = new byte[256] {
            0xff,0x90,0x90,0x2c,0x90,0x90,0x43,0x88, 0x90,0x13,0x0a,0xbd,0xba,0x60,0xea,0x90, /* 00 */
	        0x90,0x90,0xf2,0x29,0xb3,0x22,0x90,0x0c, 0xa9,0x5f,0x9d,0x07,0x90,0x90,0x0b,0xbb, /* 10 */
	        0x8a,0x90,0x90,0x90,0x3a,0x3c,0x5a,0x38, 0x99,0x90,0xf8,0x89,0x90,0x91,0x90,0x55, /* 20 */
	        0xac,0x40,0x73,0x90,0x59,0x90,0xfc,0x90, 0x50,0xfa,0x90,0x25,0x90,0x34,0x47,0xb7, /* 30 */
	        0x90,0x90,0x90,0x49,0x90,0x0f,0x8b,0x05, 0xc3,0xa5,0xbf,0x83,0x86,0xc5,0x90,0x90, /* 40 */
	        0x28,0x77,0x24,0xb4,0x90,0x92,0x90,0x3b, 0x5e,0xb6,0x80,0x0d,0x2e,0xab,0xe7,0x90, /* 50 */
	        0x48,0x90,0xad,0xc0,0x90,0x1b,0xc6,0xa3, 0x04,0x90,0x90,0x90,0x16,0xb0,0x7d,0x98, /* 60 */
	        0x87,0x46,0x8c,0x90,0x90,0xfe,0x90,0xcf, 0x90,0x68,0x84,0x90,0xd2,0x90,0x18,0x51, /* 70 */
	        0x76,0xa4,0x36,0x52,0xfb,0x90,0xb9,0x90, 0x90,0xb1,0x1c,0x21,0xe6,0xb5,0x17,0x27, /* 80 */
	        0x3d,0x45,0xbe,0xae,0x90,0x4a,0x0e,0xe5, 0x90,0x58,0x1f,0x61,0xf3,0x02,0x90,0xe8, /* 90 */
	        0x90,0x90,0x90,0xf7,0x56,0x96,0x90,0xbc, 0x4f,0x90,0x90,0x79,0xd0,0x90,0x2a,0x12, /* A0 */
	        0x4e,0xb8,0x90,0x41,0x90,0x90,0xd3,0x90, 0x2d,0x33,0xf6,0x90,0x90,0x14,0x90,0x32, /* B0 */
	        0x5d,0xa8,0x53,0x26,0x2b,0x20,0x81,0x75, 0x7f,0x3e,0x90,0x90,0x00,0x93,0x90,0xb2, /* C0 */
	        0x57,0x90,0xa0,0x90,0x39,0x90,0x90,0x72, 0x90,0x01,0x42,0x74,0x9c,0x1e,0x90,0x5b, /* D0 */
	        0x90,0xf9,0x90,0x2f,0x85,0x90,0xeb,0xa2, 0x90,0xe2,0x11,0x90,0x4b,0x7e,0x90,0x78, /* E0 */
	        0x90,0x90,0x09,0xa1,0x03,0x90,0x23,0xc1, 0x8e,0xe9,0xd1,0x7c,0x90,0x90,0xc7,0x06, /* F0 */
        };
        public static byte[] airduelm72_crc = new byte[] { 0x72, 0x9c, 0xca, 0x85, 0xc9, 0x12, 0xcc, 0xea, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        public static void M72Init()
        {
            int i1, i2, i3, n1, n2, n3;
            Generic.paletteram16_set = new ushort[0x600];
            Generic.paletteram16_2_set = new ushort[0x600];
            Generic.spriteram16_set = new ushort[0x200];
            Machine.bRom = true;
            EmuTimer.setvector = setvector_callback;
            protection_ram = new byte[0x1000];
            Memory.Set_mainrom(Machine.GetRom("maincpu.rom"));
            Memory.Set_audiorom(Machine.GetRom("soundcpu.rom"));
            //Memory.audiorom_set = new byte[0x10000];
            spritesrom_set = Machine.GetRom("sprites.rom");
            n1 = spritesromLength;
            sprites1rom_set = new byte[n1 * 2];
            for (i1 = 0; i1 < n1; i1++)
            {
                sprites1rom[i1 * 2] = (byte)(spritesrom[i1] >> 4);
                sprites1rom[i1 * 2 + 1] = (byte)(spritesrom[i1] & 0x0f);
            }
            gfx2rom_set = Machine.GetRom("gfx2.rom");
            n2 = gfx2romLength;
            gfx21rom_set = new byte[n2 * 2];
            for (i2 = 0; i2 < n2; i2++)
            {
                gfx21rom[i2 * 2] = (byte)(gfx2rom[i2] >> 4);
                gfx21rom[i2 * 2 + 1] = (byte)(gfx2rom[i2] & 0x0f);
            }
            gfx3rom_set = Machine.GetRom("gfx3.rom");
            if (gfx3rom != null)
            {
                n3 = gfx3romLength;
                gfx31rom_set = new byte[n3 * 2];
                for (i3 = 0; i3 < n3; i3++)
                {
                    gfx31rom[i3 * 2] = (byte)(gfx3rom[i3] >> 4);
                    gfx31rom[i3 * 2 + 1] = (byte)(gfx3rom[i3] & 0x0f);
                }
            }
            samplesrom_set = Machine.GetRom("samples.rom");
            Memory.Set_mainram(new byte[0x4000]);
            Memory.Set_audioram(new byte[0x10000]);
            dsw = 0xffbf;
            if (Memory.mainrom_IsNull || Memory.audiorom_IsNull || sprites1rom == null || gfx21rom == null || samplesrom == null)
            {
                Machine.bRom = false;
            }
        }
        public static byte protection_r(byte[] protection_code, int offset)
        {
            Array.Copy(protection_code, protection_ram, 96);
            return protection_ram[0xffa + offset];
        }
        public static ushort protection_r2(byte[] protection_code, int offset)
        {
            Array.Copy(protection_code, protection_ram, 96);
            return (ushort)(protection_ram[0xffa + offset] + protection_ram[0xffa + 1 + offset] * 0x100);
        }
        public static void protection_w(byte[] protection_crc, int offset, byte data)
        {
            data ^= 0xff;
            protection_ram[offset] = data;
            data ^= 0xff;
            if (offset == 0xfff && data == 0)
            {
                Array.Copy(protection_crc, 0, protection_ram, 0xfe0, 18);
            }
        }
        public static void protection_w(byte[] protection_crc, int offset, ushort data)
        {
            data ^= 0xffff;
            protection_ram[offset * 2] = (byte)data;
            protection_ram[offset * 2 + 1] = (byte)(data >> 8);
            data ^= 0xffff;
            if (offset == 0x0fff / 2 && (data >> 8) == 0)
            {
                Array.Copy(protection_crc, 0, protection_ram, 0xfe0, 18);
            }
        }
        public static void fake_nmi()
        {
            byte sample = m72_sample_r();
            if (sample != 0)
            {
                m72_sample_w(sample);
            }
        }
        public static void airduelm72_sample_trigger_w(byte data)
        {
            int[] a = new int[16]{
                0x00000, 0x00020, 0x03ec0, 0x05640, 0x06dc0, 0x083a0, 0x0c000, 0x0eb60,
                0x112e0, 0x13dc0, 0x16520, 0x16d60, 0x18ae0, 0x1a5a0, 0x1bf00, 0x1c340
            };
            if ((data & 0xff) < 16)
            {
                m72_set_sample_start(a[data & 0xff]);
            }
        }
        public static byte soundram_r(int offset)
        {
            return Memory.audioram[offset];
        }
        public static ushort soundram_r2(int offset)
        {
            return (ushort)(Memory.audioram[offset * 2 + 0] | (Memory.audioram[offset * 2 + 1] << 8));
        }
        public static void soundram_w(int offset, byte data)
        {
            Memory.audioram[offset] = data;
        }
        public static void soundram_w(int offset, ushort data)
        {
            Memory.audioram[offset * 2] = (byte)data;
            Memory.audioram[offset * 2 + 1] = (byte)(data >> 8);
        }
        public static void machine_start_m72()
        {
            scanline_timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.M72_m72_scanline_interrupt, false);
        }
        public static void machine_reset_m72()
        {
            m72_irq_base = 0x20;
            machine_reset_m72_sound();
            EmuTimer.timer_adjust_periodic(scanline_timer, Video.video_screen_get_time_until_pos(0, 0), Attotime.ATTOTIME_NEVER);
        }
        public static void machine_reset_kengo()
        {
            m72_irq_base = 0x18;
            machine_reset_m72_sound();
            EmuTimer.timer_adjust_periodic(scanline_timer, Video.video_screen_get_time_until_pos(0, 0), Attotime.ATTOTIME_NEVER);
        }
        public static void m72_scanline_interrupt()
        {
            int scanline = m72_scanline_param;
            if (scanline < 256 && scanline == m72_raster_irq_position - 128)
            {
                Video.video_screen_update_partial(scanline);
                Cpuexec.cpu[0].cpunum_set_input_line_and_vector(0, 0, LineState.HOLD_LINE, m72_irq_base + 2);
            }
            else if (scanline == 256)
            {
                Video.video_screen_update_partial(scanline);
                Cpuexec.cpu[0].cpunum_set_input_line_and_vector(0, 0, LineState.HOLD_LINE, m72_irq_base + 0);
            }
            if (++scanline >= Video.screenstate.height)
            {
                scanline = 0;
            }
            m72_scanline_param = scanline;
            EmuTimer.timer_adjust_periodic(scanline_timer, Video.video_screen_get_time_until_pos(scanline, 0), Attotime.ATTOTIME_NEVER);
        }
    }
}
