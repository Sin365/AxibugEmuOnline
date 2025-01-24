using cpu.m68000;
using System;
using System.Runtime.InteropServices;

namespace MAME.Core
{
    public unsafe partial class Capcom
    {
        //public static byte[] audiorom2;
        public static int basebankmain, basebanksnd1;
        //public static byte[] /*gfx1rom,*/ /*gfx2rom, */gfx3rom, gfx4rom, gfx5rom, gfx12rom, gfx22rom, gfx32rom, gfx42rom;

        #region //指针化 audiorom2
        static byte[] audiorom2_src;
        static GCHandle audiorom2_handle;
        public static byte* audiorom2;
        public static int audiorom2Length;
        public static bool audiorom2_IsNull => audiorom2 == null;
        public static byte[] audiorom2_set
        {
            set
            {
                audiorom2_handle.ReleaseGCHandle();
                audiorom2_src = value;
                audiorom2Length = value.Length;
                audiorom2_src.GetObjectPtr(ref audiorom2_handle, ref audiorom2);
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

        #region //指针化 gfx4rom
        static byte[] gfx4rom_src;
        static GCHandle gfx4rom_handle;
        public static byte* gfx4rom;
        public static int gfx4romLength;
        public static bool gfx4rom_IsNull => gfx4rom == null;
        public static byte[] gfx4rom_set
        {
            set
            {
                gfx4rom_handle.ReleaseGCHandle();
                gfx4rom_src = value;
                gfx4romLength = value.Length;
                gfx4rom_src.GetObjectPtr(ref gfx4rom_handle, ref gfx4rom);
            }
        }
        #endregion

        #region //指针化 gfx5rom
        static byte[] gfx5rom_src;
        static GCHandle gfx5rom_handle;
        public static byte* gfx5rom;
        public static int gfx5romLength;
        public static bool gfx5rom_IsNull => gfx5rom == null;
        public static byte[] gfx5rom_set
        {
            set
            {
                gfx5rom_handle.ReleaseGCHandle();
                gfx5rom_src = value;
                gfx5romLength = value.Length;
                gfx5rom_src.GetObjectPtr(ref gfx5rom_handle, ref gfx5rom);
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

        #region //指针化 gfx42rom
        static byte[] gfx42rom_src;
        static GCHandle gfx42rom_handle;
        public static byte* gfx42rom;
        public static int gfx42romLength;
        public static bool gfx42rom_IsNull => gfx42rom == null;
        public static byte[] gfx42rom_set
        {
            set
            {
                gfx42rom_handle.ReleaseGCHandle();
                gfx42rom_src = value;
                gfx42romLength = value.Length;
                gfx42rom_src.GetObjectPtr(ref gfx42rom_handle, ref gfx42rom);
            }
        }
        #endregion


        public static ushort dsw1, dsw2;
        public static byte bytedsw1, bytedsw2;
        public static ushort[] sf_objectram, sf_videoram;
        public static int[] scale = new int[8] { 0x00, 0x40, 0xe0, 0xfe, 0xfe, 0xfe, 0xfe, 0xfe };
        public static void CapcomInit()
        {
            int i, n;
            Machine.bRom = true;
            switch (Machine.sName)
            {
                case "gng":
                case "gnga":
                case "gngbl":
                case "gngprot":
                case "gngblita":
                case "gngc":
                case "gngt":
                case "makaimur":
                case "makaimurc":
                case "makaimurg":
                case "diamond":
                    Generic.spriteram_set = new byte[0x200];
                    Generic.buffered_spriteram_set = new byte[0x200];
                    Memory.Set_mainrom(Machine.GetRom("maincpu.rom"));
                    //Memory.Set_audiorom(Machine.GetRom("audiocpu.rom"));
                    Memory.Set_audiorom(Machine.GetRom("audiocpu.rom"));
                    gfx12rom_set = Machine.GetRom("gfx1.rom");
                    n = gfx12romLength;
                    gfx1rom_set = new byte[n * 2];
                    for (i = 0; i < n; i++)
                    {
                        gfx1rom[i * 2] = (byte)(gfx12rom[i] >> 4);
                        gfx1rom[i * 2 + 1] = (byte)(gfx12rom[i] & 0x0f);
                    }
                    gfx22rom_set = Machine.GetRom("gfx2.rom");
                    n = gfx22romLength;
                    gfx2rom_set = new byte[n * 2];
                    for (i = 0; i < n; i++)
                    {
                        gfx2rom[i * 2] = (byte)(gfx22rom[i] >> 4);
                        gfx2rom[i * 2 + 1] = (byte)(gfx22rom[i] & 0x0f);
                    }
                    gfx32rom_set = Machine.GetRom("gfx3.rom");
                    n = gfx32romLength;
                    gfx3rom_set = new byte[n * 2];
                    for (i = 0; i < n; i++)
                    {
                        gfx3rom[i * 2] = (byte)(gfx32rom[i] >> 4);
                        gfx3rom[i * 2 + 1] = (byte)(gfx32rom[i] & 0x0f);
                    }
                    Memory.Set_mainram(new byte[0x1e00]);
                    Memory.Set_audioram(new byte[0x800]);
                    Generic.paletteram_set = new byte[0x100];
                    Generic.paletteram_2_set = new byte[0x100];
                    if (Memory.mainrom_IsNull|| Memory.audiorom_IsNull || gfx12rom == null || gfx22rom == null || gfx32rom == null)
                    {
                        Machine.bRom = false;
                    }
                    break;
                case "sf":
                case "sfua":
                case "sfj":
                case "sfjan":
                case "sfan":
                case "sfp":
                    sf_objectram = new ushort[0x1000];
                    sf_videoram = new ushort[0x800];
                    Generic.paletteram16_set = new ushort[0x400];
                    Memory.Set_mainrom(Machine.GetRom("maincpu.rom"));
                    //Memory.Set_audiorom(Machine.GetRom("audiocpu.rom"));
                    Memory.Set_audiorom(Machine.GetRom("audiocpu.rom"));
                    audiorom2_set = Machine.GetRom("audio2.rom");
                    gfx12rom_set = Machine.GetRom("gfx1.rom");
                    n = gfx12romLength;
                    gfx1rom_set = new byte[n * 2];
                    for (i = 0; i < n; i++)
                    {
                        gfx1rom[i * 2] = (byte)(gfx12rom[i] >> 4);
                        gfx1rom[i * 2 + 1] = (byte)(gfx12rom[i] & 0x0f);
                    }
                    gfx22rom_set = Machine.GetRom("gfx2.rom");
                    n = gfx22romLength;
                    gfx2rom_set = new byte[n * 2];
                    for (i = 0; i < n; i++)
                    {
                        gfx2rom[i * 2] = (byte)(gfx22rom[i] >> 4);
                        gfx2rom[i * 2 + 1] = (byte)(gfx22rom[i] & 0x0f);
                    }
                    gfx32rom_set = Machine.GetRom("gfx3.rom");
                    n = gfx32romLength;
                    gfx3rom_set = new byte[n * 2];
                    for (i = 0; i < n; i++)
                    {
                        gfx3rom[i * 2] = (byte)(gfx32rom[i] >> 4);
                        gfx3rom[i * 2 + 1] = (byte)(gfx32rom[i] & 0x0f);
                    }
                    gfx42rom_set = Machine.GetRom("gfx4.rom");
                    n = gfx42romLength;
                    gfx4rom_set = new byte[n * 2];
                    for (i = 0; i < n; i++)
                    {
                        gfx4rom[i * 2] = (byte)(gfx42rom[i] >> 4);
                        gfx4rom[i * 2 + 1] = (byte)(gfx42rom[i] & 0x0f);
                    }
                    gfx5rom_set = Machine.GetRom("gfx5.rom");
                    Memory.Set_mainram(new byte[0x6000]);
                    Memory.Set_audioram(new byte[0x800]);
                    if (Memory.mainrom_IsNull || Memory.audiorom_IsNull || gfx12rom == null || gfx22rom == null || gfx32rom == null || gfx42rom == null || gfx5rom == null)
                    {
                        Machine.bRom = false;
                    }
                    break;
            }
            if (Machine.bRom)
            {
                switch (Machine.sName)
                {
                    case "gng":
                    case "gnga":
                    case "gngbl":
                    case "gngprot":
                    case "gngblita":
                    case "gngc":
                    case "gngt":
                    case "makaimur":
                    case "makaimurc":
                    case "makaimurg":
                        bytedsw1 = 0xdf;
                        bytedsw2 = 0xfb;
                        break;
                    case "diamond":
                        bytedsw1 = 0x81;
                        bytedsw2 = 0x07;
                        break;
                    case "sf":
                    case "sfua":
                    case "sfj":
                        dsw1 = 0xdfff;
                        dsw2 = 0xfbff;
                        shorts = unchecked((short)0xff7f);
                        break;
                    case "sfjan":
                    case "sfan":
                    case "sfp":
                        dsw1 = 0xdfff;
                        dsw2 = 0xffff;
                        shorts = unchecked((short)0xff7f);
                        break;
                }
            }
        }
        public static ushort dummy_r()
        {
            return 0xffff;
        }
        public static void sf_coin_w()
        {
            /*if (ACCESSING_BITS_0_7)
            {
                coin_counter_w(0, data & 0x01);
                coin_counter_w(1, data & 0x02);
                coin_lockout_w(0, ~data & 0x10);
                coin_lockout_w(1, ~data & 0x20);
                coin_lockout_w(2, ~data & 0x40);
            }*/
        }
        public static void sf_coin_w2()
        {

        }
        public static void soundcmd_w(ushort data)
        {
            //if (ACCESSING_BITS_0_7)
            {
                Sound.soundlatch_w((ushort)(data & 0xff));
                Cpuint.cpunum_set_input_line(1, (int)LineState.INPUT_LINE_NMI, LineState.PULSE_LINE);
            }
        }
        public static void soundcmd_w2(byte data)
        {
            Sound.soundlatch_w((ushort)(data & 0xff));
            Cpuint.cpunum_set_input_line(1, (int)LineState.INPUT_LINE_NMI, LineState.PULSE_LINE);
        }
        public static void write_dword(int offset, int data)
        {
            MC68000.m1.WriteWord(offset, (short)(data >> 16));
            MC68000.m1.WriteWord(offset + 2, (short)data);
        }
        public static void protection_w(ushort data)
        {
            int[,] maplist = new int[4, 10] {
                { 1, 0, 3, 2, 4, 5, 6, 7, 8, 9 },
                { 4, 5, 6, 7, 1, 0, 3, 2, 8, 9 },
                { 3, 2, 1, 0, 6, 7, 4, 5, 8, 9 },
                { 6, 7, 4, 5, 3, 2, 1, 0, 8, 9 }
            };
            int map;
            map = maplist[MC68000.m1.ReadByte(0xffc006), (MC68000.m1.ReadByte(0xffc003) << 1) + (MC68000.m1.ReadWord(0xffc004) >> 8)];
            switch (MC68000.m1.ReadByte(0xffc684))
            {
                case 1:
                    {
                        int base1;
                        base1 = 0x1b6e8 + 0x300e * map;
                        write_dword(0xffc01c, 0x16bfc + 0x270 * map);
                        write_dword(0xffc020, base1 + 0x80);
                        write_dword(0xffc024, base1);
                        write_dword(0xffc028, base1 + 0x86);
                        write_dword(0xffc02c, base1 + 0x8e);
                        write_dword(0xffc030, base1 + 0x20e);
                        write_dword(0xffc034, base1 + 0x30e);
                        write_dword(0xffc038, base1 + 0x38e);
                        write_dword(0xffc03c, base1 + 0x40e);
                        write_dword(0xffc040, base1 + 0x80e);
                        write_dword(0xffc044, base1 + 0xc0e);
                        write_dword(0xffc048, base1 + 0x180e);
                        write_dword(0xffc04c, base1 + 0x240e);
                        write_dword(0xffc050, 0x19548 + 0x60 * map);
                        write_dword(0xffc054, 0x19578 + 0x60 * map);
                        break;
                    }
                case 2:
                    {
                        int[] delta1 = new int[10]{
                            0x1f80, 0x1c80, 0x2700, 0x2400, 0x2b80, 0x2e80, 0x3300, 0x3600, 0x3a80, 0x3d80
                        };
                        int[] delta2 = new int[10]{
                            0x2180, 0x1800, 0x3480, 0x2b00, 0x3e00, 0x4780, 0x5100, 0x5a80, 0x6400, 0x6d80
                        };
                        int d1 = delta1[map] + 0xc0;
                        int d2 = delta2[map];
                        MC68000.m1.WriteWord(0xffc680, (short)d1);
                        MC68000.m1.WriteWord(0xffc682, (short)d2);
                        MC68000.m1.WriteWord(0xffc00c, 0xc0);
                        MC68000.m1.WriteWord(0xffc00e, 0);
                        sf_fg_scroll_w((ushort)d1);
                        sf_bg_scroll_w((ushort)d2);
                        break;
                    }
                case 4:
                    {
                        int pos = MC68000.m1.ReadByte(0xffc010);
                        pos = (pos + 1) & 3;
                        MC68000.m1.WriteByte(0xffc010, (sbyte)pos);
                        if (pos == 0)
                        {
                            int d1 = MC68000.m1.ReadWord(0xffc682);
                            int off = MC68000.m1.ReadWord(0xffc00e);
                            if (off != 512)
                            {
                                off++;
                                d1++;
                            }
                            else
                            {
                                off = 0;
                                d1 -= 512;
                            }
                            MC68000.m1.WriteWord(0xffc682, (short)d1);
                            MC68000.m1.WriteWord(0xffc00e, (short)off);
                            sf_bg_scroll_w((ushort)d1);
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
        public static void protection_w1(byte data)
        {
            int[,] maplist = new int[4, 10] {
                { 1, 0, 3, 2, 4, 5, 6, 7, 8, 9 },
                { 4, 5, 6, 7, 1, 0, 3, 2, 8, 9 },
                { 3, 2, 1, 0, 6, 7, 4, 5, 8, 9 },
                { 6, 7, 4, 5, 3, 2, 1, 0, 8, 9 }
            };
            int map;
            map = maplist[MC68000.m1.ReadByte(0xffc006), (MC68000.m1.ReadByte(0xffc003) << 1) + (MC68000.m1.ReadWord(0xffc004) >> 8)];
            switch (MC68000.m1.ReadByte(0xffc684))
            {
                case 1:
                    {
                        int base1;
                        base1 = 0x1b6e8 + 0x300e * map;
                        write_dword(0xffc01c, 0x16bfc + 0x270 * map);
                        write_dword(0xffc020, base1 + 0x80);
                        write_dword(0xffc024, base1);
                        write_dword(0xffc028, base1 + 0x86);
                        write_dword(0xffc02c, base1 + 0x8e);
                        write_dword(0xffc030, base1 + 0x20e);
                        write_dword(0xffc034, base1 + 0x30e);
                        write_dword(0xffc038, base1 + 0x38e);
                        write_dword(0xffc03c, base1 + 0x40e);
                        write_dword(0xffc040, base1 + 0x80e);
                        write_dword(0xffc044, base1 + 0xc0e);
                        write_dword(0xffc048, base1 + 0x180e);
                        write_dword(0xffc04c, base1 + 0x240e);
                        write_dword(0xffc050, 0x19548 + 0x60 * map);
                        write_dword(0xffc054, 0x19578 + 0x60 * map);
                        break;
                    }
                case 2:
                    {
                        int[] delta1 = new int[10]{
                            0x1f80, 0x1c80, 0x2700, 0x2400, 0x2b80, 0x2e80, 0x3300, 0x3600, 0x3a80, 0x3d80
                        };
                        int[] delta2 = new int[10]{
                            0x2180, 0x1800, 0x3480, 0x2b00, 0x3e00, 0x4780, 0x5100, 0x5a80, 0x6400, 0x6d80
                        };
                        int d1 = delta1[map] + 0xc0;
                        int d2 = delta2[map];
                        MC68000.m1.WriteWord(0xffc680, (short)d1);
                        MC68000.m1.WriteWord(0xffc682, (short)d2);
                        MC68000.m1.WriteWord(0xffc00c, 0xc0);
                        MC68000.m1.WriteWord(0xffc00e, 0);
                        sf_fg_scroll_w1((byte)(d1 >> 8));
                        sf_bg_scroll_w((byte)(d2 >> 8));
                        break;
                    }
                case 4:
                    {
                        int pos = MC68000.m1.ReadByte(0xffc010);
                        pos = (pos + 1) & 3;
                        MC68000.m1.WriteByte(0xffc010, (sbyte)pos);
                        if (pos == 0)
                        {
                            int d1 = MC68000.m1.ReadWord(0xffc682);
                            int off = MC68000.m1.ReadWord(0xffc00e);
                            if (off != 512)
                            {
                                off++;
                                d1++;
                            }
                            else
                            {
                                off = 0;
                                d1 -= 512;
                            }
                            MC68000.m1.WriteWord(0xffc682, (short)d1);
                            MC68000.m1.WriteWord(0xffc00e, (short)off);
                            sf_bg_scroll_w((byte)(d1 >> 8));
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
        public static void protection_w2(byte data)
        {
            int[,] maplist = new int[4, 10] {
                { 1, 0, 3, 2, 4, 5, 6, 7, 8, 9 },
                { 4, 5, 6, 7, 1, 0, 3, 2, 8, 9 },
                { 3, 2, 1, 0, 6, 7, 4, 5, 8, 9 },
                { 6, 7, 4, 5, 3, 2, 1, 0, 8, 9 }
            };
            int map;
            map = maplist[MC68000.m1.ReadByte(0xffc006), (MC68000.m1.ReadByte(0xffc003) << 1) + (MC68000.m1.ReadWord(0xffc004) >> 8)];
            switch (MC68000.m1.ReadByte(0xffc684))
            {
                case 1:
                    {
                        int base1;
                        base1 = 0x1b6e8 + 0x300e * map;
                        write_dword(0xffc01c, 0x16bfc + 0x270 * map);
                        write_dword(0xffc020, base1 + 0x80);
                        write_dword(0xffc024, base1);
                        write_dword(0xffc028, base1 + 0x86);
                        write_dword(0xffc02c, base1 + 0x8e);
                        write_dword(0xffc030, base1 + 0x20e);
                        write_dword(0xffc034, base1 + 0x30e);
                        write_dword(0xffc038, base1 + 0x38e);
                        write_dword(0xffc03c, base1 + 0x40e);
                        write_dword(0xffc040, base1 + 0x80e);
                        write_dword(0xffc044, base1 + 0xc0e);
                        write_dword(0xffc048, base1 + 0x180e);
                        write_dword(0xffc04c, base1 + 0x240e);
                        write_dword(0xffc050, 0x19548 + 0x60 * map);
                        write_dword(0xffc054, 0x19578 + 0x60 * map);
                        break;
                    }
                case 2:
                    {
                        int[] delta1 = new int[10]{
                            0x1f80, 0x1c80, 0x2700, 0x2400, 0x2b80, 0x2e80, 0x3300, 0x3600, 0x3a80, 0x3d80
                        };
                        int[] delta2 = new int[10]{
                            0x2180, 0x1800, 0x3480, 0x2b00, 0x3e00, 0x4780, 0x5100, 0x5a80, 0x6400, 0x6d80
                        };
                        int d1 = delta1[map] + 0xc0;
                        int d2 = delta2[map];
                        MC68000.m1.WriteWord(0xffc680, (short)d1);
                        MC68000.m1.WriteWord(0xffc682, (short)d2);
                        MC68000.m1.WriteWord(0xffc00c, 0xc0);
                        MC68000.m1.WriteWord(0xffc00e, 0);
                        sf_fg_scroll_w((byte)d1);
                        sf_bg_scroll_w((byte)d2);
                        break;
                    }
                case 4:
                    {
                        int pos = MC68000.m1.ReadByte(0xffc010);
                        pos = (pos + 1) & 3;
                        MC68000.m1.WriteByte(0xffc010, (sbyte)pos);
                        if (pos == 0)
                        {
                            int d1 = MC68000.m1.ReadWord(0xffc682);
                            int off = MC68000.m1.ReadWord(0xffc00e);
                            if (off != 512)
                            {
                                off++;
                                d1++;
                            }
                            else
                            {
                                off = 0;
                                d1 -= 512;
                            }
                            MC68000.m1.WriteWord(0xffc682, (short)d1);
                            MC68000.m1.WriteWord(0xffc00e, (short)off);
                            sf_bg_scroll_w((byte)d1);
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
        public static ushort button1_r()
        {
            return (ushort)((scale[sbyte3] << 8) | scale[sbyte1]);
        }
        public static ushort button2_r()
        {
            return (ushort)((scale[sbyte4] << 8) | scale[sbyte2]);
        }
        public static void msm5205_w(int offset, byte data)
        {
            MSM5205.msm5205_reset_w(offset, (data >> 7) & 1);
            /* ?? bit 6?? */
            MSM5205.msm5205_data_w(offset, data);
            MSM5205.msm5205_vclk_w(offset, 1);
            MSM5205.msm5205_vclk_w(offset, 0);
        }
        public static void irq_handler(int irq)
        {
            Cpuint.cpunum_set_input_line(1, 0, (irq != 0) ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
        }
        public static void machine_reset_capcom()
        {

        }
    }
}
