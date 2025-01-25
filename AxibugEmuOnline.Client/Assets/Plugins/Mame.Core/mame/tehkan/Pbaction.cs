using cpu.z80;
using System;
using System.Runtime.InteropServices;

namespace MAME.Core
{
    public unsafe partial class Tehkan
    {
        public static byte dsw1, dsw2;
        //public static byte[] /*mainromop,*/ /*gfx1rom,*/ /*gfx2rom,*/ gfx3rom, gfx32rom;

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

        #region //指针化 gfx32rom
        static byte[] gfx32rom_src;
        static GCHandle gfx32rom_handle;
        public static byte* gfx32rom;
        public static int gfx32romLength;
        public static bool gfx32rom_IsNull => gfx32rom == null;
        public static byte[] gfx32rom_set
        {
            set
            {
                gfx32rom_handle.ReleaseGCHandle();
                gfx32rom_src = value;
                gfx32romLength = value.Length;
                gfx32rom_src.GetObjectPtr(ref gfx32rom_handle, ref gfx32rom);
            }
        }
        #endregion


        public static void PbactionInit()
        {
            int i, n;
            Machine.bRom = true;
            switch (Machine.sName)
            {
                case "pbaction":
                case "pbaction2":
                    Memory.Set_mainrom(Machine.GetRom("maincpu.rom"));
                    //Memory.Set_audiorom(Machine.GetRom("audiocpu.rom"));
                    Memory.Set_audiorom(Machine.GetRom("audiocpu.rom"));
                    gfx1rom_set = Machine.GetRom("gfx1.rom");
                    gfx2rom_set = Machine.GetRom("gfx2.rom");
                    gfx3rom_set = Machine.GetRom("gfx3.rom");
                    gfx32rom_set = Machine.GetRom("gfx32.rom");
                    Memory.Set_mainram(new byte[0x1000]);
                    Memory.Set_audioram(new byte[0x800]);
                    Generic.videoram_set = new byte[0x400];
                    pbaction_videoram2 = new byte[0x400];
                    Generic.colorram_set = new byte[0x400];
                    pbaction_colorram2 = new byte[0x400];
                    Generic.spriteram_set = new byte[0x80];
                    Generic.paletteram_set = new byte[0x200];
                    if (Memory.mainrom_IsNull || Memory.audiorom_IsNull || gfx1rom == null || gfx2rom == null || gfx3rom == null || gfx32rom == null)
                    {
                        Machine.bRom = false;
                    }
                    break;
                case "pbaction3":
                case "pbaction4":
                case "pbaction5":
                    Memory.Set_mainrom(Machine.GetRom("maincpu.rom"));
                    mainromop_set = Machine.GetRom("maincpuop.rom");
                    //Memory.Set_audiorom(Machine.GetRom("audiocpu.rom"));
                    Memory.Set_audiorom(Machine.GetRom("audiocpu.rom"));
                    gfx1rom_set = Machine.GetRom("gfx1.rom");
                    gfx2rom_set = Machine.GetRom("gfx2.rom");
                    gfx3rom_set = Machine.GetRom("gfx3.rom");
                    gfx32rom_set = Machine.GetRom("gfx32.rom");
                    Memory.Set_mainram(new byte[0x1000]);
                    Memory.Set_audioram(new byte[0x800]);
                    Generic.videoram_set = new byte[0x400];
                    pbaction_videoram2 = new byte[0x400];
                    Generic.colorram_set = new byte[0x400];
                    pbaction_colorram2 = new byte[0x400];
                    Generic.spriteram_set = new byte[0x80];
                    Generic.paletteram_set = new byte[0x200];
                    if (Memory.mainrom_IsNull || mainromop == null || Memory.audiorom_IsNull || gfx1rom == null || gfx2rom == null || gfx3rom == null || gfx32rom == null)
                    {
                        Machine.bRom = false;
                    }
                    break;
            }
            if (Machine.bRom)
            {
                switch (Machine.sName)
                {
                    case "pbaction":
                    case "pbaction2":
                    case "pbaction3":
                    case "pbaction4":
                    case "pbaction5":
                        dsw1 = 0x40;
                        dsw2 = 0x00;
                        break;
                }
            }
        }
        public static void pbaction_sh_command_w(byte data)
        {
            Sound.soundlatch_w(data);
            Cpuint.cpunum_set_input_line_and_vector2(1, 0, LineState.HOLD_LINE, 0);
        }
        public static void pbaction_interrupt()
        {
            Cpuint.cpunum_set_input_line_and_vector2(1, 0, LineState.HOLD_LINE, 0x02);
        }
        public unsafe static byte pbaction3_prot_kludge_r()
        {
            byte result;
            if (Z80A.zz1[0].PC == 0xab80)
            {
                result = 0;
            }
            else
            {
                result = Memory.mainram[0];
            }
            return result;
        }
        public static void machine_reset_tehkan()
        {

        }
    }
}
