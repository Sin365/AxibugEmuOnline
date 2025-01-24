using System;
using System.Runtime.InteropServices;

namespace MAME.Core
{
    public unsafe partial class SunA8
    {
        public static byte m_rombank, m_spritebank, m_palettebank, spritebank_latch;
        public static byte suna8_unknown;
        public static byte m_gfxbank;
        public static int m_has_text;
        public static GFXBANK_TYPE m_gfxbank_type;
        public static byte dsw1, dsw2, dswcheat;
        public static byte m_rombank_latch, m_nmi_enable;
        //public static byte[] /*mainromop,*/ /*gfx1rom,*/ gfx12rom, samplesrom;
        public static int basebankmain;
        //public static short[] samplebuf, samplebuf2;
        public static int sample;
        public static int sample_offset;

        #region //指针化 mainromop
        static byte[] mainromop_src;
        static GCHandle mainromop_handle;
        public static byte* mainromop;
        public static int mainromopLength;
        public static bool mainromop_IsNull => mainromop == null;
        public static byte[] mainromop_set
        {
            set
            {
                mainromop_handle.ReleaseGCHandle();
                mainromop_src = value;
                mainromopLength = value.Length;
                mainromop_src.GetObjectPtr(ref mainromop_handle, ref mainromop);
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


        #region //指针化 gfx12rom
        static byte[] gfx12rom_src;
        static GCHandle gfx12rom_handle;
        public static byte* gfx12rom;
        public static int gfx12romLength;
        public static bool gfx12rom_IsNull => gfx12rom == null;
        public static byte[] gfx12rom_set
        {
            set
            {
                gfx12rom_handle.ReleaseGCHandle();
                gfx12rom_src = value;
                gfx12romLength = value.Length;
                gfx12rom_src.GetObjectPtr(ref gfx12rom_handle, ref gfx12rom);
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

        #region //指针化 samplebuf
        static short[] samplebuf_src;
        static GCHandle samplebuf_handle;
        public static short* samplebuf;
        public static int samplebufLength;
        public static bool samplebuf_IsNull => samplebuf == null;
        public static short[] samplebuf_set
        {
            set
            {
                samplebuf_handle.ReleaseGCHandle();
                samplebuf_src = value;
                samplebufLength = value.Length;
                samplebuf_src.GetObjectPtr(ref samplebuf_handle, ref samplebuf);
            }
        }
        #endregion


        #region //指针化 samplebuf2
        static short[] samplebuf2_src;
        static GCHandle samplebuf2_handle;
        public static short* samplebuf2;
        public static int samplebuf2Length;
        public static bool samplebuf2_IsNull => samplebuf2 == null;
        public static short[] samplebuf2_set
        {
            set
            {
                samplebuf2_handle.ReleaseGCHandle();
                samplebuf2_src = value;
                samplebuf2Length = value.Length;
                samplebuf2_src.GetObjectPtr(ref samplebuf2_handle, ref samplebuf2);
            }
        }
        #endregion


        public static void SunA8Init()
        {
            int i, n;
            Machine.bRom = true;
            switch (Machine.sName)
            {
                case "starfigh":
                    Generic.spriteram_set = new byte[0x4000];
                    mainromop_set = Machine.GetRom("maincpuop.rom");
                    Memory.Set_mainrom(Machine.GetRom("maincpu.rom"));
                    Memory.Set_audiorom(Machine.GetRom("audiocpu.rom"));
                    samplesrom_set = Machine.GetRom("samples.rom");
                    gfx12rom_set = Machine.GetRom("gfx1.rom");
                    n = gfx12romLength;
                    gfx1rom_set = new byte[n * 2];
                    for (i = 0; i < n; i++)
                    {
                        gfx1rom[i * 2] = (byte)(gfx12rom[i] >> 4);
                        gfx1rom[i * 2 + 1] = (byte)(gfx12rom[i] & 0x0f);
                    }
                    Memory.Set_mainram(new byte[0x1800]);
                    Memory.Set_audioram(new byte[0x800]);
                    Generic.paletteram_set = new byte[0x200];
                    if (mainromop == null || Memory.mainrom_IsNull || Memory.audiorom_IsNull || samplesrom == null || gfx12rom == null)
                    {
                        Machine.bRom = false;
                    }
                    break;
            }
            if (Machine.bRom)
            {
                switch (Machine.sName)
                {
                    case "starfigh":
                        dsw1 = 0x5f;
                        dsw2 = 0xff;
                        dswcheat = 0xbf;
                        Sample.info.starthandler = suna8_sh_start;
                        break;
                }
            }
        }
        public static void hardhea2_flipscreen_w(byte data)
        {
            Generic.flip_screen_set(data & 0x01);
        }
        public static void starfigh_leds_w(byte data)
        {
            int bank;
            //set_led_status(0, data & 0x01);
            //set_led_status(1, data & 0x02);
            Generic.coin_counter_w(0, data & 0x04);
            m_gfxbank = (byte)((data & 0x08) != 0 ? 4 : 0);
            bank = m_rombank_latch & 0x0f;
            basebankmain = 0x10000 + bank * 0x4000;
            //memory_set_bank(1,bank);
            m_rombank = m_rombank_latch;
        }
        public static void starfigh_rombank_latch_w(byte data)
        {
            m_rombank_latch = data;
        }
        public static void starfigh_sound_latch_w(byte data)
        {
            if ((m_rombank_latch & 0x20) == 0)
            {
                Sound.soundlatch_w(data);
            }
        }
        public static byte starfigh_cheats_r()
        {
            byte b1 = dswcheat;
            if (Video.video_screen_get_vblank())
            {
                b1 = (byte)(dswcheat | 0x40);
            }
            return b1;
        }
        public static void hardhea2_interrupt()
        {
            switch (Cpuexec.iloops)
            {
                case 240:
                    Cpuint.cpunum_set_input_line(0, 0, LineState.HOLD_LINE);
                    break;
                case 112:
                    if (m_nmi_enable != 0)
                    {
                        Cpuint.cpunum_set_input_line(0, (int)LineState.INPUT_LINE_NMI, LineState.PULSE_LINE);
                    }
                    break;
            }
        }
        public static void starfigh_spritebank_latch_w(byte data)
        {
            spritebank_latch = (byte)((data >> 2) & 1);
            m_nmi_enable = (byte)((data >> 5) & 1);
        }
        public static void starfigh_spritebank_w()
        {
            m_spritebank = spritebank_latch;
        }
        public static void suna8_play_samples_w(int offset, byte data)
        {
            if (data != 0)
            {
                if ((~data & 0x10) != 0)
                {
                    sample_offset = 0x800 * sample;
                    if (sample_offset == 0x3000)
                    {
                        int i1 = 1;
                    }
                    AxiArray.Copy(samplebuf, 0x800 * sample, samplebuf2, 0, 0x800);
                    Sample.sample_start_raw(0, samplebuf2, 0x0800, 4000, 0);
                }
                else if ((~data & 0x08) != 0)
                {
                    sample &= 3;
                    sample_offset = 0x800 * (sample + 7);
                    AxiArray.Copy(samplebuf, 0x800 * (sample + 7), samplebuf2, 0, 0x800);
                    Sample.sample_start_raw(0, samplebuf2, 0x0800, 4000, 0);
                }
            }
        }
        public static void suna8_samples_number_w(int offset, byte data)
        {
            sample = data & 0xf;
        }
        public static void suna8_sh_start()
        {
            int i, len = samplesromLength;
            samplebuf_set = new short[len];
            samplebuf2_set = new short[0x800];
            for (i = 0; i < len; i++)
            {
                samplebuf[i] = (short)((sbyte)(samplesrom[i] ^ 0x80) * 256);
            }
        }
        public static void machine_reset_suna8()
        {

        }
    }
}
