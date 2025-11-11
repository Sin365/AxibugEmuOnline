using cpu.m68000;
using System;
using System.Runtime.InteropServices;

namespace MAME.Core
{
    public unsafe partial class Konami68000
    {
        //public static byte[] /*gfx1rom,*/ /*gfx2rom, *//*gfx12rom,*/ gfx22rom, titlerom, user1rom, zoomrom;
        public static byte dsw1, dsw2, dsw3, bytee;
        //public static byte[] mainram2;
        //public static short[] sampledata;
        //public static ushort[] cuebrick_nvram, tmnt2_1c0800;
        private static int init_eeprom_count;
        private static int toggle, sprite_totel_element;
        private static int tmnt_soundlatch, cuebrick_snd_irqlatch, cuebrick_nvram_bank;
        public static int basebanksnd;

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

        #region //指针化 titlerom
        static byte[] titlerom_src;
        static GCHandle titlerom_handle;
        public static byte* titlerom;
        public static int titleromLength;
        public static bool titlerom_IsNull => titlerom == null;
        public static byte[] titlerom_set
        {
            set
            {
                titlerom_handle.ReleaseGCHandle();
                if (value == null)
                    return;
                titlerom_src = value;
                titleromLength = value.Length;
                titlerom_src.GetObjectPtr(ref titlerom_handle, ref titlerom);
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

        #region //指针化 zoomrom
        static byte[] zoomrom_src;
        static GCHandle zoomrom_handle;
        public static byte* zoomrom;
        public static int zoomromLength;
        public static bool zoomrom_IsNull => zoomrom == null;
        public static byte[] zoomrom_set
        {
            set
            {
                zoomrom_handle.ReleaseGCHandle();
                if (value == null)
                    return;
                zoomrom_src = value;
                zoomromLength = value.Length;
                zoomrom_src.GetObjectPtr(ref zoomrom_handle, ref zoomrom);
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


        #region //指针化 sampledata
        static short[] sampledata_src;
        static GCHandle sampledata_handle;
        public static short* sampledata;
        public static int sampledataLength;
        public static bool sampledata_IsNull => sampledata == null;
        public static short[] sampledata_set
        {
            set
            {
                sampledata_handle.ReleaseGCHandle();
                sampledata_src = value;
                sampledataLength = value.Length;
                sampledata_src.GetObjectPtr(ref sampledata_handle, ref sampledata);
            }
        }
        #endregion

        #region //指针化 cuebrick_nvram
        static ushort[] cuebrick_nvram_src;
        static GCHandle cuebrick_nvram_handle;
        public static ushort* cuebrick_nvram;
        public static int cuebrick_nvramLength;
        public static bool cuebrick_nvram_IsNull => cuebrick_nvram == null;
        public static ushort[] cuebrick_nvram_set
        {
            set
            {
                cuebrick_nvram_handle.ReleaseGCHandle();
                cuebrick_nvram_src = value;
                cuebrick_nvramLength = value.Length;
                cuebrick_nvram_src.GetObjectPtr(ref cuebrick_nvram_handle, ref cuebrick_nvram);
            }
        }
        #endregion

        #region //指针化 tmnt2_1c0800
        static ushort[] tmnt2_1c0800_src;
        static GCHandle tmnt2_1c0800_handle;
        public static ushort* tmnt2_1c0800;
        public static int tmnt2_1c0800Length;
        public static bool tmnt2_1c0800_IsNull => tmnt2_1c0800 == null;
        public static ushort[] tmnt2_1c0800_set
        {
            set
            {
                tmnt2_1c0800_handle.ReleaseGCHandle();
                if (value == null)
                    return;
                tmnt2_1c0800_src = value;
                tmnt2_1c0800Length = value.Length;
                tmnt2_1c0800_src.GetObjectPtr(ref tmnt2_1c0800_handle, ref tmnt2_1c0800);
            }
        }
        #endregion


        public static void Konami68000Init()
        {
            int i, n1, n2;
            Generic.paletteram16_set = new ushort[0x800];
            Generic.spriteram16_set = new ushort[0x2000];
            init_eeprom_count = 10;
            toggle = 0;
            Memory.Set_mainram(new byte[0x4000]);
            Memory.Set_audioram(new byte[0x2000]);//0x800 prmrsocr_0x2000
            mainram2_set = new byte[0x4000];//0x4000 tmnt2_ssriders_0x80
            layer_colorbase = new int[3];
            cuebrick_nvram_set = new ushort[0x400 * 0x20];
            tmnt2_1c0800_set = new ushort[0x10];
            K053245_memory_region = new byte[2][];
            K053244_rombank = new int[2];
            K053245_ramsize = new int[2];
            K053245_dx = new int[2];
            K053245_dy = new int[2];
            K053245_ram = new byte[2][];
            K053245_buffer = new ushort[2][];
            K053244_regs = new byte[2][];
            K052109_charrombank = new byte[4];
            K052109_charrombank_2 = new byte[4];
            K053251_ram = new byte[0x10];
            K053251_palette_index = new int[5];
            K052109_dx = new int[3];
            K052109_dy = new int[3];
            for (i = 0; i < 2; i++)
            {
                K053245_ram[i] = new byte[0];
                K053245_buffer[i] = new ushort[0];
                K053244_regs[i] = new byte[0x10];
            }
            K053251_tilemaps = new Tmap[5];
            K053936_offset = new int[2][];
            for (i = 0; i < 2; i++)
            {
                K053936_offset[i] = new int[2];
            }
            K053936_wraparound = new int[2];
            K053936_0_ctrl = new ushort[0x10];
            K053936_0_linectrl = new ushort[0x800];
            K054000_ram = new byte[0x20];
            layerpri = new int[3];
            sorted_layer = new int[3];
            Machine.bRom = true;
            Memory.Set_mainrom(Machine.GetRom("maincpu.rom"));
            Memory.Set_audiorom(Machine.GetRom("audiocpu.rom"));
            gfx1rom_set = Machine.GetRom("gfx1.rom");
            n1 = gfx1romLength;
            gfx12rom_set = new byte[n1 * 2];
            for (i = 0; i < n1; i++)
            {
                gfx12rom[i * 2] = (byte)(gfx1rom[i] >> 4);
                gfx12rom[i * 2 + 1] = (byte)(gfx1rom[i] & 0x0f);
            }
            gfx2rom_set = Machine.GetRom("gfx2.rom");
            n2 = gfx2romLength;
            gfx22rom_set = new byte[n2 * 2];
            for (i = 0; i < n2; i++)
            {
                gfx22rom[i * 2] = (byte)(gfx2rom[i] >> 4);
                gfx22rom[i * 2 + 1] = (byte)(gfx2rom[i] & 0x0f);
            }
            sprite_totel_element = gfx22romLength / 0x100;
            switch (Machine.sName)
            {
                case "cuebrick":
                    K052109_memory_region = Machine.GetRom("k052109.rom");
                    K051960_memory_region = Machine.GetRom("k051960.rom");
                    if (Memory.mainrom_IsNull || gfx1rom == null || gfx2rom == null || K052109_memory_region == null || K051960_memory_region == null)
                    {
                        Machine.bRom = false;
                    }
                    break;
                case "mia":
                case "mia2":
                    K052109_memory_region = Machine.GetRom("k052109.rom");
                    K051960_memory_region = Machine.GetRom("k051960.rom");
                    K007232.k007232rom = Machine.GetRom("k007232.rom");
                    if (Memory.mainrom_IsNull || gfx1rom == null || gfx2rom == null || K052109_memory_region == null || K051960_memory_region == null || Memory.audiorom_IsNull || K007232.k007232rom == null)
                    {
                        Machine.bRom = false;
                    }
                    break;
                case "tmnt":
                case "tmntu":
                case "tmntua":
                case "tmntub":
                case "tmht":
                case "tmhta":
                case "tmhtb":
                case "tmntj":
                case "tmnta":
                case "tmht2p":
                case "tmht2pa":
                case "tmnt2pj":
                case "tmnt2po":
                    K052109_memory_region = Machine.GetRom("k052109.rom");
                    K051960_memory_region = Machine.GetRom("k051960.rom");
                    K007232.k007232rom = Machine.GetRom("k007232.rom");
                    Upd7759.updrom = Machine.GetRom("upd.rom");
                    titlerom_set = Machine.GetRom("title.rom");
                    if (Memory.mainrom_IsNull || gfx1rom == null || gfx2rom == null || K052109_memory_region == null || K051960_memory_region == null || Memory.audiorom_IsNull || K007232.k007232rom == null || Upd7759.updrom == null || titlerom == null)
                    {
                        Machine.bRom = false;
                    }
                    break;
                case "punkshot":
                case "punkshot2":
                case "punkshotj":
                case "thndrx2":
                case "thndrx2a":
                case "thndrx2j":
                    K052109_memory_region = Machine.GetRom("k052109.rom");
                    K051960_memory_region = Machine.GetRom("k051960.rom");
                    K053260.k053260rom = Machine.GetRom("k053260.rom");
                    if (Memory.mainrom_IsNull || gfx1rom == null || gfx2rom == null || K052109_memory_region == null || K051960_memory_region == null || Memory.audiorom_IsNull || K053260.k053260rom == null)
                    {
                        Machine.bRom = false;
                    }
                    break;
                case "lgtnfght":
                case "lgtnfghta":
                case "lgtnfghtu":
                case "trigon":
                case "blswhstl":
                case "blswhstla":
                case "detatwin":
                case "tmnt2":
                case "tmnt2a":
                case "tmht22pe":
                case "tmht24pe":
                case "tmnt22pu":
                case "qgakumon":
                case "ssriders":
                case "ssriderseaa":
                case "ssridersebd":
                case "ssridersebc":
                case "ssridersuda":
                case "ssridersuac":
                case "ssridersuab":
                case "ssridersubc":
                case "ssridersadd":
                case "ssridersabd":
                case "ssridersjad":
                case "ssridersjac":
                case "ssridersjbd":
                    K052109_memory_region = Machine.GetRom("k052109.rom");
                    K053245_memory_region[0] = Machine.GetRom("k053245.rom");
                    K053260.k053260rom = Machine.GetRom("k053260.rom");
                    if (Memory.mainrom_IsNull || gfx1rom == null || gfx2rom == null || K052109_memory_region == null || K053245_memory_region[0] == null || Memory.audiorom_IsNull || K053260.k053260rom == null)
                    {
                        Machine.bRom = false;
                    }
                    break;
                case "glfgreat":
                case "glfgreatj":
                    K052109_memory_region = Machine.GetRom("k052109.rom");
                    K053245_memory_region[0] = Machine.GetRom("k053245.rom");
                    zoomrom_set = Machine.GetRom("zoom.rom");
                    user1rom_set = Machine.GetRom("user1.rom");
                    K053260.k053260rom = Machine.GetRom("k053260.rom");
                    if (Memory.mainrom_IsNull || gfx1rom == null || gfx2rom == null || K052109_memory_region == null || K053245_memory_region[0] == null || zoomrom == null || user1rom == null || Memory.audiorom_IsNull || K053260.k053260rom == null)
                    {
                        Machine.bRom = false;
                    }
                    break;
                case "prmrsocr":
                case "prmrsocrj":
                    K052109_memory_region = Machine.GetRom("k052109.rom");
                    K053245_memory_region[0] = Machine.GetRom("k053245.rom");
                    zoomrom_set = Machine.GetRom("zoom.rom");
                    user1rom_set = Machine.GetRom("user1.rom");
                    K054539.k054539rom = Machine.GetRom("k054539.rom");
                    if (Memory.mainrom_IsNull || gfx1rom == null || gfx2rom == null || K052109_memory_region == null || K053245_memory_region[0] == null || zoomrom == null || user1rom == null || Memory.audiorom_IsNull || K054539.k054539rom == null)
                    {
                        Machine.bRom = false;
                    }
                    break;
            }
            if (Machine.bRom)
            {
                switch (Machine.sName)
                {
                    case "cuebrick":
                        dsw1 = 0x56;
                        dsw2 = 0xff;
                        dsw3 = 0x0f;
                        K052109_callback = cuebrick_tile_callback;
                        K051960_callback = mia_sprite_callback;
                        break;
                    case "mia":
                    case "mia2":
                        dsw1 = 0xff;
                        dsw2 = 0x56;
                        dsw3 = 0x0f;
                        K052109_callback = mia_tile_callback;
                        K051960_callback = mia_sprite_callback;
                        break;
                    case "tmnt":
                    case "tmntu":
                    case "tmntua":
                    case "tmntub":
                    case "tmht":
                    case "tmhta":
                    case "tmhtb":
                    case "tmntj":
                    case "tmnta":
                        dsw1 = 0x0f;
                        dsw2 = 0x5f;
                        dsw3 = 0xff;
                        K052109_callback = tmnt_tile_callback;
                        K051960_callback = tmnt_sprite_callback;
                        break;
                    case "tmht2p":
                    case "tmht2pa":
                    case "tmnt2pj":
                    case "tmnt2po":
                        dsw1 = 0xff;
                        dsw2 = 0x5f;
                        dsw3 = 0xff;
                        K052109_callback = tmnt_tile_callback;
                        K051960_callback = tmnt_sprite_callback;
                        break;
                    case "punkshot":
                    case "punkshot2":
                    case "punkshotj":
                        dsw1 = 0xff;
                        dsw2 = 0x7f;
                        dsw3 = 0xff;
                        K052109_callback = tmnt_tile_callback;
                        K051960_callback = punkshot_sprite_callback;
                        break;
                    case "lgtnfght":
                    case "lgtnfghta":
                    case "lgtnfghtu":
                    case "trigon":
                        dsw1 = 0x5e;
                        dsw2 = 0xff;
                        dsw3 = 0xfd;
                        K052109_callback = tmnt_tile_callback;
                        K053245_callback = lgtnfght_sprite_callback;
                        break;
                    case "blswhstl":
                    case "blswhstla":
                    case "detatwin":
                        bytee = 0xfe;
                        K052109_callback = blswhstl_tile_callback;
                        K053245_callback = blswhstl_sprite_callback;
                        break;
                    case "glfgreat":
                    case "glfgreatj":
                        dsw1 = 0xff;
                        dsw2 = 0x59;
                        dsw3 = 0xf7;
                        K052109_callback = tmnt_tile_callback;
                        K053245_callback = lgtnfght_sprite_callback;
                        break;
                    case "tmnt2":
                    case "tmnt2a":
                    case "tmht22pe":
                    case "tmht24pe":
                    case "tmnt22pu":
                    case "qgakumon":
                    case "ssriders":
                    case "ssriderseaa":
                    case "ssridersebd":
                    case "ssridersebc":
                    case "ssridersuda":
                    case "ssridersuac":
                    case "ssridersuab":
                    case "ssridersubc":
                    case "ssridersadd":
                    case "ssridersabd":
                    case "ssridersjad":
                    case "ssridersjac":
                    case "ssridersjbd":
                        K052109_callback = tmnt_tile_callback;
                        K053245_callback = lgtnfght_sprite_callback;
                        break;
                    case "thndrx2":
                    case "thndrx2a":
                    case "thndrx2j":
                        bytee = 0xfe;
                        K052109_callback = tmnt_tile_callback;
                        K051960_callback = thndrx2_sprite_callback;
                        break;
                    case "prmrsocr":
                    case "prmrsocrj":
                        K052109_callback = tmnt_tile_callback;
                        K053245_callback = prmrsocr_sprite_callback;
                        break;
                }
            }
        }
        public static void cuebrick_irq_handler(int irq)
        {
            cuebrick_snd_irqlatch = irq;
        }
        public static void konami68000_ym2151_irq_handler(int irq)
        {

        }
        public static ushort K052109_word_noA12_r(int offset)
        {
            int offset1 = ((offset & 0x3000) >> 1) | (offset & 0x07ff);
            return K052109_word_r(offset1);
        }
        public static void K052109_word_noA12_w(int offset, ushort data)
        {
            int offset1;
            offset1 = ((offset & 0x3000) >> 1) | (offset & 0x07ff);
            K052109_word_w(offset1, data);
        }
        public static void K052109_word_noA12_w1(int offset, byte data)
        {
            int offset1;
            offset1 = ((offset & 0x3000) >> 1) | (offset & 0x07ff);
            K052109_w(offset1, data);
        }
        public static void K052109_word_noA12_w2(int offset, byte data)
        {
            int offset1;
            offset1 = ((offset & 0x3000) >> 1) | (offset & 0x07ff);
            K052109_w(offset1 + 0x2000, data);
        }
        public static void punkshot_K052109_word_w(int offset, ushort data)
        {
            //if (ACCESSING_BITS_8_15)
            K052109_w(offset, (byte)((data >> 8) & 0xff));
            //else if (ACCESSING_BITS_0_7)
            K052109_w(offset + 0x2000, (byte)(data & 0xff));
        }
        public static void punkshot_K052109_word_w1(int offset, byte data)
        {
            K052109_w(offset, data);
        }
        public static void punkshot_K052109_word_w2(int offset, byte data)
        {
            K052109_w(offset + 0x2000, data);
        }
        public static void punkshot_K052109_word_noA12_w(int offset, ushort data)
        {
            offset = ((offset & 0x3000) >> 1) | (offset & 0x07ff);
            punkshot_K052109_word_w(offset, data);
        }
        public static void punkshot_K052109_word_noA12_w1(int offset, byte data)
        {
            offset = ((offset & 0x3000) >> 1) | (offset & 0x07ff);
            punkshot_K052109_word_w1(offset, data);
        }
        public static void punkshot_K052109_word_noA12_w2(int offset, byte data)
        {
            offset = ((offset & 0x3000) >> 1) | (offset & 0x07ff);
            punkshot_K052109_word_w2(offset, data);
        }
        public static ushort K053245_scattered_word_r(int offset)
        {
            ushort result;
            if ((offset & 0x0031) != 0)
            {
                result = Generic.spriteram16[offset];
            }
            else
            {
                offset = ((offset & 0x000e) >> 1) | ((offset & 0x1fc0) >> 3);
                result = K053245_word_r(offset);
            }
            return result;
        }


        public static void K053245_scattered_word_w(int offset, ushort data)
        {
            Generic.spriteram16[offset] = data;
            if ((offset & 0x0031) == 0)
            {
                offset = ((offset & 0x000e) >> 1) | ((offset & 0x1fc0) >> 3);
                K053245_word_w(offset, data);
            }
        }
        public static void K053245_scattered_word_w1(int offset, byte data)
        {
            Generic.spriteram16[offset] = (ushort)((data << 8) | (Generic.spriteram16[offset] & 0xff));
            if ((offset & 0x0031) == 0)
            {
                offset = ((offset & 0x000e) >> 1) | ((offset & 0x1fc0) >> 3);
                //K053245_word_w(offset, data);
                K053245_ram[0][offset * 2] = data;
            }
        }
        public static void K053245_scattered_word_w2(int offset, byte data)
        {
            Generic.spriteram16[offset] = (ushort)((Generic.spriteram16[offset] & 0xff00) | data);
            if ((offset & 0x0031) == 0)
            {
                offset = ((offset & 0x000e) >> 1) | ((offset & 0x1fc0) >> 3);
                //K053245_word_w(offset, data);
                K053245_ram[0][offset * 2 + 1] = data;
            }
        }

        public static ushort K053244_word_noA1_r(int offset)
        {
            offset &= ~1;
            return (ushort)(K053244_r(offset + 1) | (K053244_r(offset) << 8));
        }
        public static void K053244_word_noA1_w(int offset, ushort data)
        {
            offset &= ~1;
            //if (ACCESSING_BITS_8_15)
            K053244_w(offset, (byte)((data >> 8) & 0xff));
            //if (ACCESSING_BITS_0_7)
            K053244_w(offset + 1, (byte)(data & 0xff));
        }
        public static void K053244_word_noA1_w1(int offset, byte data)
        {
            offset &= ~1;
            K053244_w(offset, (byte)(data & 0xff));
        }
        public static void K053244_word_noA1_w2(int offset, byte data)
        {
            offset &= ~1;
            K053244_w(offset + 1, (byte)(data & 0xff));
        }
        public static void cuebrick_interrupt()
        {
            switch (Cpuexec.iloops)
            {
                case 0:
                    Cpuint.cpunum_set_input_line(0, 5, LineState.HOLD_LINE);
                    break;
                default:
                    if (cuebrick_snd_irqlatch != 0)
                    {
                        Cpuint.cpunum_set_input_line(0, 6, LineState.HOLD_LINE);
                    }
                    break;
            }
        }
        public static void punkshot_interrupt()
        {
            if (K052109_is_IRQ_enabled() != 0)
            {
                Generic.irq4_line_hold(0);
            }
        }
        public static void lgtnfght_interrupt()
        {
            if (K052109_is_IRQ_enabled() != 0)
            {
                Generic.irq5_line_hold(0);
            }
        }
        public static void tmnt_sound_command_w(ushort data)
        {
            //if (ACCESSING_BITS_0_7)
            Sound.soundlatch_w((ushort)(data & 0xff));
        }
        public static void tmnt_sound_command_w2(byte data)
        {
            //if (ACCESSING_BITS_0_7)
            Sound.soundlatch_w((ushort)(data & 0xff));
        }
        public static ushort punkshot_sound_r(int offset)
        {
            return K053260.k053260_0_r(2 + offset);
        }
        public static ushort blswhstl_sound_r(int offset)
        {
            return K053260.k053260_0_r(2 + offset);
        }
        public static ushort glfgreat_sound_r(int offset)
        {
            return (ushort)(K053260.k053260_0_r(2 + offset) << 8);
        }
        public static byte glfgreat_sound_r1(int offset)
        {
            return K053260.k053260_0_r(2 + offset);
        }
        public static void glfgreat_sound_w(int offset, ushort data)
        {
            //if (ACCESSING_BITS_8_15)
            K053260.k053260_0_w(offset, (byte)((data >> 8) & 0xff));
            if (offset != 0)
            {
                Cpuint.cpunum_set_input_line(1, 0, LineState.HOLD_LINE);
            }
        }
        public static void glfgreat_sound_w1(int offset, byte data)
        {
            K053260.k053260_0_w(offset, data);
            if (offset != 0)
            {
                Cpuint.cpunum_set_input_line(1, 0, LineState.HOLD_LINE);
            }
        }
        public static void glfgreat_sound_w2(int offset, byte data)
        {
            if (offset != 0)
            {
                Cpuint.cpunum_set_input_line(1, 0, LineState.HOLD_LINE);
            }
        }
        public static ushort prmrsocr_sound_r()
        {
            return Sound.soundlatch3_r();
        }
        public static void prmrsocr_sound_cmd_w(int offset, ushort data)
        {
            //if (ACCESSING_BITS_0_7)
            {
                data &= 0xff;
                if (offset == 0)
                {
                    Sound.soundlatch_w(data);
                }
                else
                {
                    Sound.soundlatch2_w(data);
                }
            }
        }
        public static void prmrsocr_sound_cmd_w2(int offset, byte data)
        {
            data &= 0xff;
            if (offset == 0)
            {
                Sound.soundlatch_w(data);
            }
            else
            {
                Sound.soundlatch2_w(data);
            }
        }
        public static void prmrsocr_sound_irq_w()
        {
            Cpuint.cpunum_set_input_line(1, 0, LineState.HOLD_LINE);
        }
        public static void prmrsocr_audio_bankswitch_w(byte data)
        {
            basebanksnd = 0x10000 + (data & 7) * 0x4000;
        }
        public static ushort tmnt2_sound_r(int offset)
        {
            return K053260.k053260_0_r(2 + offset);
        }
        public static byte tmnt_sres_r()
        {
            return (byte)tmnt_soundlatch;
        }
        public static void tmnt_sres_w(byte data)
        {
            Upd7759.upd7759_reset_w(0, (byte)(data & 2));
            if ((data & 0x04) != 0)
            {
                if (Sample.sample_playing(0) == 0)
                {
                    Sample.sample_start_raw(0, sampledata, 0x40000, 20000, 0);
                }
            }
            else
            {
                Sample.sample_stop(0);
            }
            tmnt_soundlatch = data;
        }
        public static void tmnt_decode_sample()
        {
            int i;
            sampledata_set = new short[0x40000];
            for (i = 0; i < 0x40000; i++)
            {
                int val = titlerom[2 * i] + titlerom[2 * i + 1] * 256;
                int expo = val >> 13;
                val = (val >> 3) & (0x3ff);
                val -= 0x200;
                val <<= (expo - 3);
                sampledata[i] = (short)val;
            }
        }
        public static void nmi_callback()
        {
            Cpuint.cpunum_set_input_line(1, (int)LineState.INPUT_LINE_NMI, LineState.ASSERT_LINE);
        }
        public static void sound_arm_nmi_w()
        {
            Cpuint.cpunum_set_input_line(1, (int)LineState.INPUT_LINE_NMI, LineState.CLEAR_LINE);
            EmuTimer.emu_timer timer = EmuTimer.timer_alloc_common_NoRef(EmuTimer.TIME_ACT.Konami68000_nmi_callback, true);
            EmuTimer.timer_adjust_periodic(timer, new Atime(0, (long)50e12), Attotime.ATTOTIME_NEVER);
        }
        public static ushort punkshot_kludge_r()
        {
            return 0;
        }
        public static byte punkshot_kludge_r1()
        {
            return 0;
        }
        public static ushort ssriders_protection_r()
        {
            int data = (ushort)MC68000.m1.ReadWord(0x105a0a);
            int cmd = (ushort)MC68000.m1.ReadWord(0x1058fc);
            ushort result;
            switch (cmd)
            {
                case 0x100b:
                    result = 0x0064;
                    break;
                case 0x6003:
                    result = (ushort)(data & 0x000f);
                    break;
                case 0x6004:
                    result = (ushort)(data & 0x001f);
                    break;
                case 0x6000:
                    result = (ushort)(data & 0x0001);
                    break;
                case 0x0000:
                    result = (ushort)(data & 0x00ff);
                    break;
                case 0x6007:
                    result = (ushort)(data & 0x00ff);
                    break;
                case 0x8abc:
                    data = -(ushort)MC68000.m1.ReadWord(0x105818);
                    data = ((data / 8 - 4) & 0x1f) * 0x40;
                    data += (((ushort)MC68000.m1.ReadWord(0x105cb0) + 256 * K052109_r(0x1a01) + K052109_r(0x1a00) - 6) / 8 + 12) & 0x3f;
                    result = (ushort)data;
                    break;
                default:
                    result = 0xffff;
                    break;
            }
            return result;
        }
        public static void ssriders_protection_w(int offset)
        {
            if (offset == 1)
            {
                int logical_pri, hardware_pri;
                hardware_pri = 1;
                for (logical_pri = 1; logical_pri < 0x100; logical_pri <<= 1)
                {
                    int i;

                    for (i = 0; i < 128; i++)
                    {
                        if (((ushort)MC68000.m1.ReadWord(0x180006 + 128 * i) >> 8) == logical_pri)
                        {
                            K053245_word_w2(8 * i, (ushort)hardware_pri);
                            hardware_pri++;
                        }
                    }
                }
            }
        }
        public static byte getbytee()
        {
            byte result;
            if (Video.video_screen_get_vblank())
            {
                result = 0xfe;
            }
            else
            {
                result = 0xf6;
            }
            return result;
        }
        public static ushort blswhstl_coin_r()
        {
            int res;
            res = sbyte0;
            if (init_eeprom_count != 0)
            {
                init_eeprom_count--;
                res &= 0xf7;
            }
            toggle ^= 0x40;
            return (ushort)(res ^ toggle);
        }
        public static ushort blswhstl_eeprom_r()
        {
            int res;
            res = Eeprom.eeprom_read_bit() | bytee;
            return (ushort)res;
        }

        public static ushort ssriders_eeprom_r()
        {
            int res;
            res = (Eeprom.eeprom_read_bit() | getbytee());
            if (init_eeprom_count != 0)
            {
                init_eeprom_count--;
                res &= 0x7f;
            }
            toggle ^= 0x04;
            return (ushort)(res ^ toggle);
        }
        public static void blswhstl_eeprom_w(ushort data)
        {
            //if (ACCESSING_BITS_0_7)
            {
                Eeprom.eeprom_write_bit(data & 0x01);
                Eeprom.eeprom_set_cs_line((data & 0x02) != 0 ? LineState.CLEAR_LINE : LineState.ASSERT_LINE);
                Eeprom.eeprom_set_clock_line((data & 0x04) != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
            }
        }
        public static void blswhstl_eeprom_w2(byte data)
        {
            Eeprom.eeprom_write_bit(data & 0x01);
            Eeprom.eeprom_set_cs_line((data & 0x02) != 0 ? LineState.CLEAR_LINE : LineState.ASSERT_LINE);
            Eeprom.eeprom_set_clock_line((data & 0x04) != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
        }
        public static void ssriders_eeprom_w(ushort data)
        {
            //if (ACCESSING_BITS_0_7)
            {
                Eeprom.eeprom_write_bit(data & 0x01);
                Eeprom.eeprom_set_cs_line((data & 0x02) != 0 ? LineState.CLEAR_LINE : LineState.ASSERT_LINE);
                Eeprom.eeprom_set_clock_line((data & 0x04) != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
                dim_c = data & 0x18;
                K053244_bankselect(0, ((data & 0x20) >> 5) << 2);
            }
        }
        public static void ssriders_eeprom_w2(byte data)
        {
            //if (ACCESSING_BITS_0_7)
            {
                Eeprom.eeprom_write_bit(data & 0x01);
                Eeprom.eeprom_set_cs_line((data & 0x02) != 0 ? LineState.CLEAR_LINE : LineState.ASSERT_LINE);
                Eeprom.eeprom_set_clock_line((data & 0x04) != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
                dim_c = data & 0x18;
                K053244_bankselect(0, ((data & 0x20) >> 5) << 2);
            }
        }
        public static ushort thndrx2_in0_r()
        {
            ushort res;
            res = (ushort)((((byte)sbyte0) << 8) | (byte)sbyte1);
            if (init_eeprom_count != 0)
            {
                init_eeprom_count--;
                res &= 0xf7ff;
            }
            return res;
        }
        public static ushort thndrx2_eeprom_r()
        {
            int res;
            res = (Eeprom.eeprom_read_bit() << 8) | (ushort)((bytee << 8) | (byte)sbyte2);
            toggle ^= 0x0800;
            return (ushort)(res ^ toggle);
        }
        public static void thndrx2_eeprom_w(ushort data)
        {
            //if (ACCESSING_BITS_0_7)
            {
                Eeprom.eeprom_write_bit(data & 0x01);
                Eeprom.eeprom_set_cs_line((data & 0x02) != 0 ? LineState.CLEAR_LINE : LineState.ASSERT_LINE);
                Eeprom.eeprom_set_clock_line((data & 0x04) != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
                if (last == 0 && (data & 0x20) != 0)
                {
                    Cpuint.cpunum_set_input_line(1, 0, LineState.HOLD_LINE);
                    //cpunum_set_input_line_and_vector(machine, 1, 0, LineState.HOLD_LINE, 0xff);
                }
                last = data & 0x20;
                K052109_set_RMRD_line((data & 0x40) != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
            }
        }
        public static void thndrx2_eeprom_w2(byte data)
        {
            Eeprom.eeprom_write_bit(data & 0x01);
            Eeprom.eeprom_set_cs_line((data & 0x02) != 0 ? LineState.CLEAR_LINE : LineState.ASSERT_LINE);
            Eeprom.eeprom_set_clock_line((data & 0x04) != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
            if (last == 0 && (data & 0x20) != 0)
            {
                Cpuint.cpunum_set_input_line(1, 0, LineState.HOLD_LINE);
            }
            last = data & 0x20;
            K052109_set_RMRD_line((data & 0x40) != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
        }
        public static ushort prmrsocr_IN0_r()
        {
            ushort res;
            res = (ushort)((sbyte0 << 8) | (byte)sbyte1);
            if (init_eeprom_count != 0)
            {
                init_eeprom_count--;
                res &= 0xfdff;
            }
            return res;
        }
        public static ushort prmrsocr_eeprom_r()
        {
            return (ushort)((Eeprom.eeprom_read_bit() << 8) | ((ushort)(bytee << 8) | (byte)sbyte2));
        }
        public static void prmrsocr_eeprom_w(ushort data)
        {
            //if (ACCESSING_BITS_0_7)
            {
                prmrsocr_122000_w(data);
            }
            //if (ACCESSING_BITS_8_15)
            {
                Eeprom.eeprom_write_bit(data & 0x0100);
                Eeprom.eeprom_set_cs_line((data & 0x0200) != 0 ? LineState.CLEAR_LINE : LineState.ASSERT_LINE);
                Eeprom.eeprom_set_clock_line((data & 0x0400) != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
            }
        }
        public static void prmrsocr_eeprom_w1(byte data)
        {
            Eeprom.eeprom_write_bit((data & 0x01) << 8);
            Eeprom.eeprom_set_cs_line((data & 0x02) != 0 ? LineState.CLEAR_LINE : LineState.ASSERT_LINE);
            Eeprom.eeprom_set_clock_line((data & 0x04) != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
        }
        public static void prmrsocr_eeprom_w2(byte data)
        {
            prmrsocr_122000_w2(data);
        }
        public static ushort cuebrick_snd_r()
        {
            return (ushort)(YM2151.ym2151_status_port_0_r() << 8);
        }
        public static byte cuebrick_snd_r1()
        {
            return YM2151.ym2151_status_port_0_r();
        }
        public static void cuebrick_snd_w(int offset, ushort data)
        {
            if (offset != 0)
            {
                YM2151.ym2151_data_port_0_w((byte)(data >> 8));
            }
            else
            {
                YM2151.ym2151_register_port_0_w((byte)(data >> 8));
            }
        }
        public static void cuebrick_snd_w1(int offset, byte data)
        {
            if (offset != 0)
            {
                YM2151.ym2151_data_port_0_w(data);
            }
            else
            {
                YM2151.ym2151_register_port_0_w(data);
            }
        }
        public static void cuebrick_snd_w2(int offset, byte data)
        {
            if (offset != 0)
            {
                YM2151.ym2151_data_port_0_w(0);
            }
            else
            {
                YM2151.ym2151_register_port_0_w(0);
            }
        }
        public static ushort cuebrick_nv_r(int offset)
        {
            return cuebrick_nvram[offset + (cuebrick_nvram_bank * 0x400 / 2)];
        }
        public static byte cuebrick_nv_r1(int offset)
        {
            return (byte)(cuebrick_nvram[offset + (cuebrick_nvram_bank * 0x400 / 2)] >> 8);
        }
        public static byte cuebrick_nv_r2(int offset)
        {
            return (byte)cuebrick_nvram[offset + (cuebrick_nvram_bank * 0x400 / 2)];
        }
        public static void cuebrick_nv_w(int offset, ushort data)
        {
            cuebrick_nvram[offset + (cuebrick_nvram_bank * 0x400 / 2)] = data;
        }
        public static void cuebrick_nv_w1(int offset, byte data)
        {
            cuebrick_nvram[offset + (cuebrick_nvram_bank * 0x400 / 2)] = (ushort)((data << 8) | (cuebrick_nvram[offset + (cuebrick_nvram_bank * 0x400 / 2)] & 0xff));
        }
        public static void cuebrick_nv_w2(int offset, byte data)
        {
            cuebrick_nvram[offset + (cuebrick_nvram_bank * 0x400 / 2)] = (ushort)((cuebrick_nvram[offset + (cuebrick_nvram_bank * 0x400 / 2)] & 0xff00) | data);
        }

        public static void cuebrick_nvbank_w(ushort data)
        {
            cuebrick_nvram_bank = (data >> 8);
        }
        public static void cuebrick_nvbank_w1(byte data)
        {
            cuebrick_nvram_bank = data;
        }
        public static void cuebrick_nvbank_w2(byte data)
        {
            cuebrick_nvram_bank = 0;
        }

        public static void ssriders_soundkludge_w()
        {
            Cpuint.cpunum_set_input_line(1, 0, LineState.HOLD_LINE);
        }
        public unsafe static byte tmnt2_get_byte(int addr)
        {
            byte result = 0;
            if (addr <= 0x07ffff)
            {
                result = Memory.mainrom[addr];
            }
            else if (addr >= 0x104000 && addr <= 0x107fff)
            {
                int offset = addr - 0x104000;
                result = Memory.mainram[offset];
            }
            else if (addr >= 0x180000 && addr <= 0x183fff)
            {
                int offset = (addr - 0x180000) / 2;
                if (addr % 2 == 0)
                {
                    result = (byte)(Generic.spriteram16[offset] >> 8);
                }
                else
                {
                    result = (byte)Generic.spriteram16[offset];
                }
            }
            return result;
        }
        public unsafe static ushort tmnt2_get_word(int addr)
        {
            ushort result = 0;
            addr *= 2;
            if (addr <= 0x07ffff)
            {
                result = (ushort)(Memory.mainrom[addr] * 0x100 + Memory.mainrom[addr + 1]);
            }
            else if (addr >= 0x104000 && addr <= 0x107fff)
            {
                int offset = addr - 0x104000;
                result = (ushort)(Memory.mainram[offset] * 0x100 + Memory.mainram[offset + 1]);
            }
            else if (addr >= 0x180000 && addr <= 0x183fff)
            {
                int offset = (addr - 0x180000) / 2;
                result = Generic.spriteram16[offset];
            }
            return result;
        }
        public unsafe static void tmnt2_put_word(int addr, ushort data)
        {
            addr *= 2;
            if (addr >= 0x180000 && addr <= 0x183fff)
            {
                int offset = (addr - 0x180000) / 2;
                Generic.spriteram16[offset] = data;
                if ((offset & 0x0031) == 0)
                {
                    int offset2;
                    offset2 = ((offset & 0x000e) >> 1) | ((offset & 0x1fc0) >> 3);
                    K053245_word_w(offset2, data);
                    if (K053245_ram[0][2] == 0x11 && K053245_ram[0][3] == 0x80)
                    {
                        int i1 = 1;
                    }
                }
            }
            else if (addr >= 0x104000 && addr <= 0x107fff)
            {
                int offset = (addr - 0x104000) / 2;
                Memory.mainram[offset] = (byte)(data >> 8);
                Memory.mainram[offset + 1] = (byte)data;
            }
        }
        public static void tmnt2_1c0800_w(int offset, ushort data)
        {
            int src_addr, dst_addr, mod_addr, attr1, code, attr2, cbase, cmod, color;
            int xoffs, yoffs, xmod, ymod, zmod, xzoom, yzoom, i;
            ushort[] src, mod;
            src = new ushort[4];
            mod = new ushort[24];
            byte keepaspect, xlock, ylock, zlock;
            tmnt2_1c0800[offset] = data;
            if (offset != 0x18 / 2)// || !ACCESSING_BITS_8_15)
            {
                return;
            }
            if ((tmnt2_1c0800[8] & 0xff00) != 0x8200)
            {
                return;
            }
            src_addr = (tmnt2_1c0800[0] | (tmnt2_1c0800[1] & 0xff) << 16) >> 1;
            dst_addr = (tmnt2_1c0800[2] | (tmnt2_1c0800[3] & 0xff) << 16) >> 1;
            mod_addr = (tmnt2_1c0800[4] | (tmnt2_1c0800[5] & 0xff) << 16) >> 1;
            zlock = (byte)(((tmnt2_1c0800[8] & 0xff) == 0x0001) ? 1 : 0);
            for (i = 0; i < 4; i++)
            {
                src[i] = tmnt2_get_word(src_addr + i);
            }
            for (i = 0; i < 24; i++)
            {
                mod[i] = tmnt2_get_word(mod_addr + i);
            }
            code = src[0];
            i = src[1];
            attr1 = i >> 2 & 0x3f00;
            attr2 = i & 0x380;
            cbase = i & 0x01f;
            cmod = mod[0x2a / 2] >> 8;
            color = (cbase != 0x0f && cmod <= 0x1f && zlock == 0) ? cmod : cbase;
            xoffs = (short)src[2];
            yoffs = (short)src[3];
            i = mod[0];
            attr2 |= i & 0x0060;
            keepaspect = (byte)(((i & 0x0014) == 0x0014) ? 1 : 0);
            if ((i & 0x8000) != 0)
            {
                attr1 |= 0x8000;
            }
            if (keepaspect != 0)
            {
                attr1 |= 0x4000;
            }
            if ((i & 0x4000) != 0)
            {
                attr1 ^= 0x1000; xoffs = -xoffs;
            }
            xmod = (short)mod[6];
            ymod = (short)mod[7];
            zmod = (short)mod[8];
            xzoom = mod[0x1c / 2];
            yzoom = (keepaspect != 0) ? xzoom : mod[0x1e / 2];
            ylock = xlock = (byte)(((i & 0x0020) != 0 && (xzoom == 0 || xzoom == 0x100)) ? 1 : 0);
            if (xlock == 0)
            {
                i = xzoom - 0x4f00;
                if (i > 0)
                {
                    i >>= 8;
                    xoffs += (int)(Math.Pow(i, 1.891292) * xoffs / 599.250121);
                }
                else if (i < 0)
                {
                    i = (i >> 3) + (i >> 4) + (i >> 5) + (i >> 6) + xzoom;
                    xoffs = (i > 0) ? (xoffs * i / 0x4f00) : 0;
                }
            }
            if (ylock == 0)
            {
                i = yzoom - 0x4f00;
                if (i > 0)
                {
                    i >>= 8;
                    yoffs += (int)(Math.Pow(i, /*1.898461*/1.891292) * yoffs / 599.250121);
                }
                else if (i < 0)
                {
                    i = (i >> 3) + (i >> 4) + (i >> 5) + (i >> 6) + yzoom;
                    yoffs = (i > 0) ? (yoffs * i / 0x4f00) : 0;
                }
            }
            if (zlock == 0)
            {
                yoffs += zmod;
            }
            xoffs += xmod;
            yoffs += ymod;
            tmnt2_put_word(dst_addr + 0, (ushort)attr1);
            tmnt2_put_word(dst_addr + 2, (ushort)code);
            tmnt2_put_word(dst_addr + 4, (ushort)yoffs);
            tmnt2_put_word(dst_addr + 6, (ushort)xoffs);
            tmnt2_put_word(dst_addr + 12, (ushort)(attr2 | color));
        }
        public static void tmnt2_1c0800_w1(int offset, byte data)
        {
            int src_addr, dst_addr, mod_addr, attr1, code, attr2, cbase, cmod, color;
            int xoffs, yoffs, xmod, ymod, zmod, xzoom, yzoom, i;
            ushort[] src, mod;
            src = new ushort[4];
            mod = new ushort[24];
            byte keepaspect, xlock, ylock, zlock;
            tmnt2_1c0800[offset] = (ushort)((data << 8) | (tmnt2_1c0800[offset] & 0xff));
            if (offset != 0x18 / 2)
            {
                return;
            }
            if ((tmnt2_1c0800[8] & 0xff00) != 0x8200)
            {
                return;
            }
            src_addr = (tmnt2_1c0800[0] | (tmnt2_1c0800[1] & 0xff) << 16) >> 1;
            dst_addr = (tmnt2_1c0800[2] | (tmnt2_1c0800[3] & 0xff) << 16) >> 1;
            mod_addr = (tmnt2_1c0800[4] | (tmnt2_1c0800[5] & 0xff) << 16) >> 1;
            zlock = (byte)(((tmnt2_1c0800[8] & 0xff) == 0x0001) ? 1 : 0);
            for (i = 0; i < 4; i++)
            {
                src[i] = tmnt2_get_word(src_addr + i);
            }
            for (i = 0; i < 24; i++)
            {
                mod[i] = tmnt2_get_word(mod_addr + i);
            }
            code = src[0];
            i = src[1];
            attr1 = i >> 2 & 0x3f00;
            attr2 = i & 0x380;
            cbase = i & 0x01f;
            cmod = mod[0x2a / 2] >> 8;
            color = (cbase != 0x0f && cmod <= 0x1f && zlock == 0) ? cmod : cbase;
            xoffs = (short)src[2];
            yoffs = (short)src[3];
            i = mod[0];
            attr2 |= i & 0x0060;
            keepaspect = (byte)(((i & 0x0014) == 0x0014) ? 1 : 0);
            if ((i & 0x8000) != 0)
            {
                attr1 |= 0x8000;
            }
            if (keepaspect != 0)
            {
                attr1 |= 0x4000;
            }
            if ((i & 0x4000) != 0)
            {
                attr1 ^= 0x1000; xoffs = -xoffs;
            }
            xmod = (short)mod[6];
            ymod = (short)mod[7];
            zmod = (short)mod[8];
            xzoom = mod[0x1c / 2];
            yzoom = (keepaspect != 0) ? xzoom : mod[0x1e / 2];
            ylock = xlock = (byte)(((i & 0x0020) != 0 && (xzoom == 0 || xzoom == 0x100)) ? 1 : 0);
            if (xlock == 0)
            {
                i = xzoom - 0x4f00;
                if (i > 0)
                {
                    i >>= 8;
                    xoffs += (int)(Math.Pow(i, 1.891292) * xoffs / 599.250121);
                }
                else if (i < 0)
                {
                    i = (i >> 3) + (i >> 4) + (i >> 5) + (i >> 6) + xzoom;
                    xoffs = (i > 0) ? (xoffs * i / 0x4f00) : 0;
                }
            }
            if (ylock == 0)
            {
                i = yzoom - 0x4f00;
                if (i > 0)
                {
                    i >>= 8;
                    yoffs += (int)(Math.Pow(i, /*1.898461*/1.891292) * yoffs / 599.250121);
                }
                else if (i < 0)
                {
                    i = (i >> 3) + (i >> 4) + (i >> 5) + (i >> 6) + yzoom;
                    yoffs = (i > 0) ? (yoffs * i / 0x4f00) : 0;
                }
            }
            if (zlock == 0)
            {
                yoffs += zmod;
            }
            xoffs += xmod;
            yoffs += ymod;
            tmnt2_put_word(dst_addr + 0, (ushort)attr1);
            tmnt2_put_word(dst_addr + 2, (ushort)code);
            tmnt2_put_word(dst_addr + 4, (ushort)yoffs);
            tmnt2_put_word(dst_addr + 6, (ushort)xoffs);
            tmnt2_put_word(dst_addr + 12, (ushort)(attr2 | color));
        }
        public static void tmnt2_1c0800_w2(int offset, byte data)
        {
            tmnt2_1c0800[offset] = (ushort)((tmnt2_1c0800[offset] & 0xff00) | data);
        }
        public static byte k054539_0_ctrl_r(int offset)
        {
            return K054539.k054539_0_r(0x200 + offset);
        }
        public static void k054539_0_ctrl_w(int offset, byte data)
        {
            K054539.k054539_0_w(0x200 + offset, data);
        }
        public static void volume_callback(int v)
        {
            K007232.k007232_set_volume(0, 0, (v >> 4) * 0x11, 0);
            K007232.k007232_set_volume(0, 1, 0, (v & 0x0f) * 0x11);
        }
        public static void sound_nmi()
        {
            Cpuint.cpunum_set_input_line(1, (int)LineState.INPUT_LINE_NMI, LineState.PULSE_LINE);
        }
        public static void machine_reset_konami68000()
        {
            switch (Machine.sName)
            {
                case "tmnt":
                case "tmntu":
                case "tmntua":
                case "tmntub":
                case "tmht":
                case "tmhta":
                case "tmhtb":
                case "tmntj":
                case "tmnta":
                case "tmht2p":
                case "tmht2pa":
                case "tmnt2pj":
                case "tmnt2po":
                    Upd7759.upd7759_0_start_w(0);
                    Upd7759.upd7759_0_reset_w(1);
                    break;
                case "prmrsocr":
                case "prmrsocrj":
                    basebanksnd = 0;
                    break;
            }
        }
    }
}
