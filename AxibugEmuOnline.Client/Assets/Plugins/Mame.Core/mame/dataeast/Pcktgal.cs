using System;
using System.Runtime.InteropServices;

namespace MAME.Core
{
    public unsafe partial class Dataeast
    {
        //public static byte[] /*audioromop,*/ /*gfx1rom,*/ gfx2rom, gfx12rom, gfx22rom, prom;


        #region //指针化 audioromop
        static byte[] audioromop_src;
        static GCHandle audioromop_handle;
        public static byte* audioromop;
        public static int audioromopLength;
        public static bool audioromop_IsNull => audioromop == null;
        public static byte[] audioromop_set
        {
            set
            {
                audioromop_handle.ReleaseGCHandle();
                audioromop_src = value;
                audioromopLength = value.Length;
                audioromop_src.GetObjectPtr(ref audioromop_handle, ref audioromop);
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

        #region //指针化 gfx22rom
        static byte[] gfx22rom_src;
        static GCHandle gfx22rom_handle;
        public static byte* gfx22rom;
        public static int gfx22romLength;
        public static bool gfx22rom_IsNull => gfx22rom == null;
        public static byte[] gfx22rom_set
        {
            set
            {
                gfx22rom_handle.ReleaseGCHandle();
                gfx22rom_src = value;
                gfx22romLength = value.Length;
                gfx22rom_src.GetObjectPtr(ref gfx22rom_handle, ref gfx22rom);
            }
        }
        #endregion

        #region //指针化 prom
        static byte[] prom_src;
        static GCHandle prom_handle;
        public static byte* prom;
        public static int promLength;
        public static bool prom_IsNull => prom == null;
        public static byte[] prom_set
        {
            set
            {
                prom_handle.ReleaseGCHandle();
                prom_src = value;
                promLength = value.Length;
                prom_src.GetObjectPtr(ref prom_handle, ref prom);
            }
        }
        #endregion

        public static byte dsw;
        public static int basebankmain1, basebankmain2, basebanksnd, msm5205next, toggle;
        public static void DataeastInit()
        {
            int i, n;
            Machine.bRom = true;
            Memory.Set_mainram(new byte[0x800]);
            Memory.Set_audioram(new byte[0x800]);
            Generic.spriteram_set = new byte[0x200];
            Generic.videoram_set = new byte[0x800];
            switch (Machine.sName)
            {
                case "pcktgal":
                case "pcktgalb":
                    Memory.Set_mainrom(Machine.GetRom("maincpu.rom"));
                    //Memory.Set_audiorom(Machine.GetRom("audiocpu.rom"));
                    Memory.Set_audiorom(Machine.GetRom("audiocpu.rom"));
                    audioromop_set = Machine.GetRom("audiocpuop.rom");
                    gfx1rom_set = Machine.GetRom("gfx1.rom");
                    gfx2rom_set = Machine.GetRom("gfx2.rom");
                    prom_set = Machine.GetRom("proms.rom");
                    if (Memory.mainrom_IsNull || Memory.audiorom_IsNull || audioromop == null || gfx1rom == null || gfx2rom == null || prom == null)
                    {
                        Machine.bRom = false;
                    }
                    break;
                case "pcktgal2":
                case "pcktgal2j":
                case "spool3":
                case "spool3i":
                    Memory.Set_mainrom(Machine.GetRom("maincpu.rom"));
                    //Memory.Set_audiorom(Machine.GetRom("audiocpu.rom"));
                    Memory.Set_audiorom(Machine.GetRom("audiocpu.rom"));
                    gfx1rom_set = Machine.GetRom("gfx1.rom");
                    gfx2rom_set = Machine.GetRom("gfx2.rom");
                    prom_set = Machine.GetRom("proms.rom");
                    if (Memory.mainrom_IsNull || Memory.audiorom_IsNull || gfx1rom == null || gfx2rom == null || prom == null)
                    {
                        Machine.bRom = false;
                    }
                    break;
            }
            if (Machine.bRom)
            {
                dsw = 0xbf;
            }
        }
        public static void irqhandler(int irq)
        {

        }
        public static void pcktgal_bank_w(byte data)
        {
            if ((data & 1) != 0)
            {
                basebankmain1 = 0x4000;
            }
            else
            {
                basebankmain1 = 0x10000;
            }
            if ((data & 2) != 0)
            {
                basebankmain2 = 0x6000;
            }
            else
            {
                basebankmain2 = 0x12000;
            }
        }
        public static void pcktgal_sound_bank_w(byte data)
        {
            basebanksnd = 0x10000 + 0x4000 * ((data >> 2) & 1);
        }
        public static void pcktgal_sound_w(byte data)
        {
            Sound.soundlatch_w(data);
            Cpuint.cpunum_set_input_line(1, (int)LineState.INPUT_LINE_NMI, LineState.PULSE_LINE);
        }
        public static void pcktgal_adpcm_int(int data)
        {
            MSM5205.msm5205_data_w(0, msm5205next >> 4);
            msm5205next <<= 4;
            toggle = 1 - toggle;
            if (toggle != 0)
            {
                Cpuint.cpunum_set_input_line(1, 0, LineState.HOLD_LINE);
            }
        }
        public static void pcktgal_adpcm_data_w(byte data)
        {
            msm5205next = data;
        }
        public static byte pcktgal_adpcm_reset_r()
        {
            MSM5205.msm5205_reset_w(0, 0);
            return 0;
        }
        public static void machine_reset_dataeast()
        {
            basebankmain1 = 0;
            basebankmain2 = 0;
            basebanksnd = 0;
            msm5205next = 0;
            toggle = 0;
        }
    }
}
