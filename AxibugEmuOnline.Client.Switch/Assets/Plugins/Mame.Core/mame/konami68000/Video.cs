using System;

namespace MAME.Core
{
    public unsafe partial class Konami68000
    {
        private static int[] layer_colorbase;
        private static int sprite_colorbase, bg_colorbase;
        private static int priorityflag;
        private static int dim_c, dim_v;
        private static int lastdim, lasten, last;
        private static int[] layerpri, sorted_layer;
        private static int blswhstl_rombank, glfgreat_pixel;
        private static int glfgreat_roz_rom_bank, glfgreat_roz_char_bank, glfgreat_roz_rom_mode, prmrsocr_sprite_bank;
        private static Tmap roz_tilemap;
        public static void mia_tile_callback(int layer, int bank, int code, int color, int flags, int priority, out int code2, out int color2, out int flags2)
        {
            flags2 = (color & 0x04) != 0 ? 1 : 0;
            if (layer == 0)
            {
                code2 = code | ((color & 0x01) << 8);
                color2 = layer_colorbase[layer] + ((color & 0x80) >> 5) + ((color & 0x10) >> 1);
            }
            else
            {
                code2 = code | ((color & 0x01) << 8) | ((color & 0x18) << 6) | (bank << 11);
                color2 = layer_colorbase[layer] + ((color & 0xe0) >> 5);
            }
        }
        public static void cuebrick_tile_callback(int layer, int bank, int code, int color, int flags, int priority, out int code2, out int color2, out int flags2)
        {
            flags2 = flags;
            if ((K052109_get_RMRD_line() == 0) && (layer == 0))
            {
                code2 = code | ((color & 0x01) << 8);
                color2 = layer_colorbase[layer] + ((color & 0x80) >> 5) + ((color & 0x10) >> 1);
            }
            else
            {
                code2 = code | ((color & 0xf) << 8);
                color2 = layer_colorbase[layer] + ((color & 0xe0) >> 5);
            }
        }
        public static void tmnt_tile_callback(int layer, int bank, int code, int color, int flags, int priority, out int code2, out int color2, out int flags2)
        {
            flags2 = flags;
            code2 = code | ((color & 0x03) << 8) | ((color & 0x10) << 6) | ((color & 0x0c) << 9) | (bank << 13);
            color2 = layer_colorbase[layer] + ((color & 0xe0) >> 5);
        }
        public static void blswhstl_tile_callback(int layer, int bank, int code, int color, int flags, int priority, out int code2, out int color2, out int flags2)
        {
            flags2 = flags;
            code2 = code | ((color & 0x01) << 8) | ((color & 0x10) << 5) | ((color & 0x0c) << 8) | (bank << 12) | blswhstl_rombank << 14;
            color2 = layer_colorbase[layer] + ((color & 0xe0) >> 5);
        }
        public static void mia_sprite_callback(int code, int color, int priority, int shadow, out int code2, out int color2, out int priority2)
        {
            code2 = code;
            color2 = sprite_colorbase + (color & 0x0f);
            priority2 = priority;
        }
        public static void tmnt_sprite_callback(int code, int color, int priority, int shadow, out int code2, out int color2, out int priority2)
        {
            code2 = code | ((color & 0x10) << 9);
            color2 = sprite_colorbase + (color & 0x0f);
            priority2 = priority;
        }
        public static void punkshot_sprite_callback(int code, int color, int priority, int shadow, out int code2, out int color2, out int priority_mask)
        {
            int pri = 0x20 | ((color & 0x60) >> 2);
            if (pri <= layerpri[2])
            {
                priority_mask = 0;
            }
            else if (pri > layerpri[2] && pri <= layerpri[1])
            {
                priority_mask = 0xf0;
            }
            else if (pri > layerpri[1] && pri <= layerpri[0])
            {
                priority_mask = 0xf0 | 0xcc;
            }
            else
            {
                priority_mask = 0xf0 | 0xcc | 0xaa;
            }
            code2 = code | ((color & 0x10) << 9);
            color2 = sprite_colorbase + (color & 0x0f);
        }
        public static void thndrx2_sprite_callback(int code, int color, int proority, int shadow, out int code2, out int color2, out int priority_mask)
        {
            int pri = 0x20 | ((color & 0x60) >> 2);
            if (pri <= layerpri[2])
            {
                priority_mask = 0;
            }
            else if (pri > layerpri[2] && pri <= layerpri[1])
            {
                priority_mask = 0xf0;
            }
            else if (pri > layerpri[1] && pri <= layerpri[0])
            {
                priority_mask = 0xf0 | 0xcc;
            }
            else
            {
                priority_mask = 0xf0 | 0xcc | 0xaa;
            }
            code2 = code;
            color2 = sprite_colorbase + (color & 0x0f);
        }
        public static void lgtnfght_sprite_callback(int code, int color, out int code2, out int color2, out int priority_mask)
        {
            int pri = 0x20 | ((color & 0x60) >> 2);
            if (pri <= layerpri[2])
            {
                priority_mask = 0;
            }
            else if (pri > layerpri[2] && pri <= layerpri[1])
            {
                priority_mask = 0xf0;
            }
            else if (pri > layerpri[1] && pri <= layerpri[0])
            {
                priority_mask = 0xf0 | 0xcc;
            }
            else
            {
                priority_mask = 0xf0 | 0xcc | 0xaa;
            }
            code2 = code;
            color2 = sprite_colorbase + (color & 0x1f);
        }
        public static void blswhstl_sprite_callback(int code, int color, out int code2, out int color2, out int priority_mask)
        {
            int pri = 0x20 | ((color & 0x60) >> 2);
            if (pri <= layerpri[2])
            {
                priority_mask = 0;
            }
            else if (pri > layerpri[2] && pri <= layerpri[1])
            {
                priority_mask = 0xf0;
            }
            else if (pri > layerpri[1] && pri <= layerpri[0])
            {
                priority_mask = 0xf0 | 0xcc;
            }
            else
            {
                priority_mask = 0xf0 | 0xcc | 0xaa;
            }
            code2 = code;
            color2 = sprite_colorbase + (color & 0x1f);
        }
        public static void prmrsocr_sprite_callback(int code, int color, out int code2, out int color2, out int priority_mask)
        {
            int pri = 0x20 | ((color & 0x60) >> 2);
            if (pri <= layerpri[2])
            {
                priority_mask = 0;
            }
            else if (pri > layerpri[2] && pri <= layerpri[1])
            {
                priority_mask = 0xf0;
            }
            else if (pri > layerpri[1] && pri <= layerpri[0])
            {
                priority_mask = 0xf0 | 0xcc;
            }
            else
            {
                priority_mask = 0xf0 | 0xcc | 0xaa;
            }
            code2 = code | (prmrsocr_sprite_bank << 14);
            color2 = sprite_colorbase + (color & 0x1f);
        }

        public static void video_start_tmnt()
        {
            layer_colorbase[0] = 0;
            layer_colorbase[1] = 32;
            layer_colorbase[2] = 40;
            sprite_colorbase = 16;
            K052109_vh_start();
            K051960_vh_start();
        }
        public static void video_start_punkshot()
        {
            K053251_vh_start();
            K052109_vh_start();
            K051960_vh_start();
        }
        public static void video_start_lgtnfght()
        {
            K053251_vh_start();
            K052109_vh_start();
            K053245_vh_start();
            K05324x_set_z_rejection(0);
            dim_c = dim_v = lastdim = lasten = 0;
        }
        public static void video_start_blswhstl()
        {
            K053251_vh_start();
            K052109_vh_start();
            K053245_vh_start();
        }
        public static void video_start_glfgreat()
        {
            K053251_vh_start();
            K052109_vh_start();
            K053245_vh_start();
            roz_tilemap = new Tmap();
            roz_tilemap.rows = 512;
            roz_tilemap.cols = 512;
            roz_tilemap.tilewidth = 16;
            roz_tilemap.tileheight = 16;
            roz_tilemap.width = roz_tilemap.cols * roz_tilemap.tilewidth;
            roz_tilemap.height = roz_tilemap.rows * roz_tilemap.tileheight;
            roz_tilemap.enable = true;
            roz_tilemap.all_tiles_dirty = true;
            roz_tilemap.scrollrows = 1;
            roz_tilemap.scrollcols = 1;
            //roz_tilemap = tilemap_create(glfgreat_get_roz_tile_info,tilemap_scan_rows,16,16,512,512);
            //tilemap_set_transparent_pen(roz_tilemap,0);
            K053936_wraparound_enable(0, 1);
            K053936_set_offset(0, 85, 0);
        }
        public static void video_start_thndrx2()
        {
            K053251_vh_start();
            K052109_vh_start();// "k052109", NORMAL_PLANE_ORDER, tmnt_tile_callback);
            K051960_vh_start();//, "k051960", NORMAL_PLANE_ORDER, thndrx2_sprite_callback);
        }
        public static void video_start_prmrsocr()
        {
            K053251_vh_start();
            K052109_vh_start();//, "k052109", NORMAL_PLANE_ORDER, tmnt_tile_callback);
            K053245_vh_start();//, 0, "k053245", NORMAL_PLANE_ORDER, prmrsocr_sprite_callback);
            //roz_tilemap = tilemap_create(prmrsocr_get_roz_tile_info, tilemap_scan_rows, 16, 16, 512, 256);
            //tilemap_set_transparent_pen(roz_tilemap, 0);
            K053936_wraparound_enable(0, 0);
            K053936_set_offset(0, 85, 1);
        }
        public static void tmnt_paletteram_word_w(int offset, ushort data)
        {
            ushort data1;
            Generic.paletteram16[offset] = data;
            //COMBINE_DATA(paletteram16 + offset);
            offset &= ~1;
            data1 = (ushort)((Generic.paletteram16[offset] << 8) | Generic.paletteram16[offset + 1]);
            Palette.palette_set_callback(offset / 2, Palette.make_rgb(Palette.pal5bit((byte)(data1 >> 0)), Palette.pal5bit((byte)(data1 >> 5)), Palette.pal5bit((byte)(data1 >> 10))));
            //palette_set_color_rgb(machine,offset / 2,pal5bit(data >> 0),pal5bit(data >> 5),pal5bit(data >> 10));
        }
        public static void tmnt_paletteram_word_w1(int offset, byte data)
        {
            ushort data1;
            Generic.paletteram16[offset] = (ushort)((data << 8) | (Generic.paletteram16[offset] & 0xff));
            offset &= ~1;
            data1 = (ushort)((Generic.paletteram16[offset] << 8) | Generic.paletteram16[offset + 1]);
            Palette.palette_set_callback(offset / 2, Palette.make_rgb(Palette.pal5bit((byte)(data1 >> 0)), Palette.pal5bit((byte)(data1 >> 5)), Palette.pal5bit((byte)(data1 >> 10))));
        }
        public static void tmnt_paletteram_word_w2(int offset, byte data)
        {
            ushort data1;
            Generic.paletteram16[offset] = (ushort)((Generic.paletteram16[offset] & 0xff00) | data);
            offset &= ~1;
            data1 = (ushort)((Generic.paletteram16[offset] << 8) | Generic.paletteram16[offset + 1]);
            Palette.palette_set_callback(offset / 2, Palette.make_rgb(Palette.pal5bit((byte)(data1 >> 0)), Palette.pal5bit((byte)(data1 >> 5)), Palette.pal5bit((byte)(data1 >> 10))));
        }
        public static void tmnt_0a0000_w(ushort data)
        {
            //if (ACCESSING_BITS_0_7)
            {
                //coin_counter_w(0,data & 0x01);
                //coin_counter_w(1,data & 0x02);
                if (last == 0x08 && (data & 0x08) == 0)
                {
                    Cpuint.cpunum_set_input_line(1, 0, LineState.HOLD_LINE);
                    //cpunum_set_input_line_and_vector(machine, 1,0,HOLD_LINE,0xff);
                }
                last = data & 0x08;
                Generic.interrupt_enable_w((byte)(data & 0x20));
                K052109_set_RMRD_line((data & 0x80) != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
            }
        }
        public static void tmnt_0a0000_w2(byte data)
        {
            //if (ACCESSING_BITS_0_7)
            {
                //coin_counter_w(0,data & 0x01);
                //coin_counter_w(1,data & 0x02);
                if (last == 0x08 && (data & 0x08) == 0)
                {
                    Cpuint.cpunum_set_input_line(1, 0, LineState.HOLD_LINE);
                }
                last = data & 0x08;
                Generic.interrupt_enable_w((byte)(data & 0x20));
                K052109_set_RMRD_line((data & 0x80) != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
            }
        }
        public static void punkshot_0a0020_w(ushort data)
        {
            //if (ACCESSING_BITS_0_7)
            {
                //coin_counter_w(0,data & 0x01);
                if (last == 0x04 && (data & 0x04) == 0)
                {
                    Cpuint.cpunum_set_input_line(1, 0, LineState.HOLD_LINE);
                }
                last = data & 0x04;
                K052109_set_RMRD_line((data & 0x08) != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
            }
        }
        public static void punkshot_0a0020_w2(byte data)
        {
            //if (ACCESSING_BITS_0_7)
            {
                //coin_counter_w(0,data & 0x01);
                if (last == 0x04 && (data & 0x04) == 0)
                {
                    Cpuint.cpunum_set_input_line(1, 0, LineState.HOLD_LINE);
                }
                last = data & 0x04;
                K052109_set_RMRD_line((data & 0x08) != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
            }
        }
        public static void lgtnfght_0a0018_w(ushort data)
        {
            //if (ACCESSING_BITS_0_7)
            {
                //coin_counter_w(0,data & 0x01);
                //coin_counter_w(1,data & 0x02);
                if (last == 0x00 && (data & 0x04) == 0x04)
                {
                    Cpuint.cpunum_set_input_line(1, 0, LineState.HOLD_LINE);
                }
                last = data & 0x04;
                K052109_set_RMRD_line((data & 0x08) != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
            }
        }
        public static void lgtnfght_0a0018_w2(byte data)
        {
            //if (ACCESSING_BITS_0_7)
            {
                //coin_counter_w(0,data & 0x01);
                //coin_counter_w(1,data & 0x02);
                if (last == 0x00 && (data & 0x04) == 0x04)
                {
                    Cpuint.cpunum_set_input_line(1, 0, LineState.HOLD_LINE);
                }
                last = data & 0x04;
                K052109_set_RMRD_line((data & 0x08) != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
            }
        }
        public static void blswhstl_700300_w(ushort data)
        {
            //if (ACCESSING_BITS_0_7)
            {
                //coin_counter_w(0,data & 0x01);
                //coin_counter_w(1,data & 0x02);
                K052109_set_RMRD_line((data & 0x08) != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
                if (blswhstl_rombank != ((data & 0x80) >> 7))
                {
                    blswhstl_rombank = (data & 0x80) >> 7;
                    //tilemap_mark_all_tiles_dirty(ALL_TILEMAPS);
                }
            }
        }
        public static void blswhstl_700300_w2(byte data)
        {
            //coin_counter_w(0,data & 0x01);
            //coin_counter_w(1,data & 0x02);
            K052109_set_RMRD_line((data & 0x08) != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
            if (blswhstl_rombank != ((data & 0x80) >> 7))
            {
                blswhstl_rombank = (data & 0x80) >> 7;
                //tilemap_mark_all_tiles_dirty(ALL_TILEMAPS);
            }
        }
        public static ushort glfgreat_rom_r(int offset)
        {
            if (glfgreat_roz_rom_mode != 0)
            {
                return zoomrom[glfgreat_roz_char_bank * 0x80000 + offset];
            }
            else if (offset < 0x40000)
            {
                return (ushort)(user1rom[offset + 0x80000 + glfgreat_roz_rom_bank * 0x40000] + 256 * user1rom[offset + glfgreat_roz_rom_bank * 0x40000]);
            }
            else
            {
                return user1rom[((offset & 0x3ffff) >> 2) + 0x100000 + glfgreat_roz_rom_bank * 0x10000];
            }
        }
        public static void glfgreat_122000_w(ushort data)
        {
            //if (ACCESSING_BITS_0_7)
            {
                //coin_counter_w(0,data & 0x01);
                //coin_counter_w(1,data & 0x02);
                K052109_set_RMRD_line((data & 0x10) != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
                if (glfgreat_roz_rom_bank != (data & 0x20) >> 5)
                {
                    glfgreat_roz_rom_bank = (data & 0x20) >> 5;
                    roz_tilemap.all_tiles_dirty = true;
                }
                glfgreat_roz_char_bank = (data & 0xc0) >> 6;
            }
            //if (ACCESSING_BITS_8_15)
            {
                glfgreat_roz_rom_mode = data & 0x100;
            }
        }
        public static void glfgreat_122000_w1(byte data)
        {
            glfgreat_roz_rom_mode = (data << 8) & 0x100;
        }
        public static void glfgreat_122000_w2(byte data)
        {
            //coin_counter_w(0,data & 0x01);
            //coin_counter_w(1,data & 0x02);
            K052109_set_RMRD_line((data & 0x10) != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
            if (glfgreat_roz_rom_bank != (data & 0x20) >> 5)
            {
                glfgreat_roz_rom_bank = (data & 0x20) >> 5;
                roz_tilemap.all_tiles_dirty = true;
            }
            glfgreat_roz_char_bank = (data & 0xc0) >> 6;
        }

        public static void ssriders_1c0300_w(int offset, ushort data)
        {
            //if (ACCESSING_BITS_0_7)
            {
                //coin_counter_w(0,data & 0x01);
                //coin_counter_w(1,data & 0x02);
                K052109_set_RMRD_line((data & 0x08) != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
                dim_v = (data & 0x70) >> 4;
            }
        }
        public static void ssriders_1c0300_w2(int offset, byte data)
        {
            //if (ACCESSING_BITS_0_7)
            {
                //coin_counter_w(0,data & 0x01);
                //coin_counter_w(1,data & 0x02);
                K052109_set_RMRD_line((data & 0x08) != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
                dim_v = (data & 0x70) >> 4;
            }
        }
        public static void prmrsocr_122000_w(ushort data)
        {
            //if (ACCESSING_BITS_0_7)
            {
                //coin_counter_w(0,data & 0x01);
                //coin_counter_w(1,data & 0x02);
                K052109_set_RMRD_line((data & 0x10) != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
                prmrsocr_sprite_bank = (data & 0x40) >> 6;
                K053244_bankselect(0, prmrsocr_sprite_bank << 2);
                glfgreat_roz_char_bank = (data & 0x80) >> 7;
            }
        }
        public static void prmrsocr_122000_w2(byte data)
        {
            K052109_set_RMRD_line((data & 0x10) != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
            prmrsocr_sprite_bank = (data & 0x40) >> 6;
            K053244_bankselect(0, prmrsocr_sprite_bank << 2);
            glfgreat_roz_char_bank = (data & 0x80) >> 7;
        }
        public static ushort prmrsocr_rom_r(int offset)
        {
            ushort result;
            if (glfgreat_roz_char_bank != 0)
            {
                result = (ushort)(zoomrom[offset] * 0x100 + zoomrom[offset + 1]);// memory_region(machine, "zoom")[offset];
            }
            else
            {
                result = (ushort)(user1rom[offset] * 0x100 + user1rom[offset + 0x20000]);
            }
            return result;
        }
        public static byte prmrsocr_rom_r1(int offset)
        {
            byte result;
            if (glfgreat_roz_char_bank != 0)
            {
                result = zoomrom[offset];
            }
            else
            {
                result = user1rom[offset];
            }
            return result;
        }
        public static byte prmrsocr_rom_r2(int offset)
        {
            byte result;
            if (glfgreat_roz_char_bank != 0)
            {
                result = zoomrom[offset + 1];
            }
            else
            {
                result = user1rom[offset + 0x20000];
            }
            return result;
        }
        public static void tmnt_priority_w(ushort data)
        {
            //if (ACCESSING_BITS_0_7)
            {
                priorityflag = (data & 0x0c) >> 2;
            }
        }
        public static void tmnt_priority_w2(byte data)
        {
            priorityflag = (data & 0x0c) >> 2;
        }
        public static void swap(int[] layer, int[] pri, int a, int b)
        {
            if (pri[a] < pri[b])
            {
                int t;
                t = pri[a]; pri[a] = pri[b]; pri[b] = t;
                t = layer[a]; layer[a] = layer[b]; layer[b] = t;
            }
        }
        public static void sortlayers(int[] layer, int[] pri)
        {
            swap(layer, pri, 0, 1);
            swap(layer, pri, 0, 2);
            swap(layer, pri, 1, 2);
        }
        public static void video_update_mia()
        {
            K052109_tilemap_update();
            K052109_tilemap[2].tilemap_draw_primask(Video.screenstate.visarea, 0, 0);
            if ((priorityflag & 1) == 1)
            {
                K051960_sprites_draw(Video.screenstate.visarea, 0, 0);
            }
            K052109_tilemap[1].tilemap_draw_primask(Video.screenstate.visarea, 0x10, 0);
            if ((priorityflag & 1) == 0)
            {
                K051960_sprites_draw(Video.screenstate.visarea, 0, 0);
            }
            K052109_tilemap[0].tilemap_draw_primask(Video.screenstate.visarea, 0x10, 0);
        }
        public static void video_update_punkshot()
        {
            int i;
            bg_colorbase = K053251_get_palette_index(0);
            sprite_colorbase = K053251_get_palette_index(1);
            layer_colorbase[0] = K053251_get_palette_index(2);
            layer_colorbase[1] = K053251_get_palette_index(4);
            layer_colorbase[2] = K053251_get_palette_index(3);
            K052109_tilemap_update();
            sorted_layer[0] = 0;
            layerpri[0] = K053251_get_priority(2);
            sorted_layer[1] = 1;
            layerpri[1] = K053251_get_priority(4);
            sorted_layer[2] = 2;
            layerpri[2] = K053251_get_priority(3);
            sortlayers(sorted_layer, layerpri);
            Array.Clear(Tilemap.priority_bitmap, 0, 0x40000);
            K052109_tilemap[sorted_layer[0]].tilemap_draw_primask(Video.screenstate.visarea, 0, 1);
            K052109_tilemap[sorted_layer[1]].tilemap_draw_primask(Video.screenstate.visarea, 0x10, 2);
            K052109_tilemap[sorted_layer[2]].tilemap_draw_primask(Video.screenstate.visarea, 0x10, 4);
            K051960_sprites_draw(Video.screenstate.visarea, -1, -1);
        }
        public static void video_update_lgtnfght()
        {
            int i;
            bg_colorbase = K053251_get_palette_index(0);
            sprite_colorbase = K053251_get_palette_index(1);
            layer_colorbase[0] = K053251_get_palette_index(2);
            layer_colorbase[1] = K053251_get_palette_index(4);
            layer_colorbase[2] = K053251_get_palette_index(3);
            K052109_tilemap_update();
            sorted_layer[0] = 0;
            layerpri[0] = K053251_get_priority(2);
            sorted_layer[1] = 1;
            layerpri[1] = K053251_get_priority(4);
            sorted_layer[2] = 2;
            layerpri[2] = K053251_get_priority(3);
            sortlayers(sorted_layer, layerpri);
            Array.Clear(Tilemap.priority_bitmap, 0, 0x40000);
            for (i = 0; i < 0x20000; i++)
            {
                Video.bitmapbase_Ptrs[Video.curbitmap][i] = (ushort)(16 * bg_colorbase);
            }
            K052109_tilemap[sorted_layer[0]].tilemap_draw_primask(Video.screenstate.visarea, 0x10, 1);
            K052109_tilemap[sorted_layer[1]].tilemap_draw_primask(Video.screenstate.visarea, 0x10, 2);
            K052109_tilemap[sorted_layer[2]].tilemap_draw_primask(Video.screenstate.visarea, 0x10, 4);
            K053245_sprites_draw(Video.screenstate.visarea);
        }
        public static ushort glfgreat_ball_r()
        {
            if (glfgreat_pixel < 0x400 || glfgreat_pixel >= 0x500)
            {
                return 0;
            }
            else
            {
                return (ushort)(glfgreat_pixel & 0xff);
            }
        }
        public static void video_update_glfgreat()
        {
            int i;
            K053251_set_tilemaps(null, null, K052109_tilemap[0], K052109_tilemap[1], K052109_tilemap[2]);
            bg_colorbase = K053251_get_palette_index(0);
            sprite_colorbase = K053251_get_palette_index(1);
            layer_colorbase[0] = K053251_get_palette_index(2);
            layer_colorbase[1] = K053251_get_palette_index(3) + 8;
            layer_colorbase[2] = K053251_get_palette_index(4);
            K052109_tilemap_update();
            sorted_layer[0] = 0;
            layerpri[0] = K053251_get_priority(2);
            sorted_layer[1] = 1;
            layerpri[1] = K053251_get_priority(3);
            sorted_layer[2] = 2;
            layerpri[2] = K053251_get_priority(4);
            sortlayers(sorted_layer, layerpri);
            Array.Clear(Tilemap.priority_bitmap, 0, 0x40000);
            for (i = 0; i < 0x20000; i++)
            {
                Video.bitmapbase_Ptrs[Video.curbitmap][i] = (ushort)(16 * bg_colorbase);
            }
            K052109_tilemap[sorted_layer[0]].tilemap_draw_primask(Video.screenstate.visarea, 0x10, 1);
            if (layerpri[0] >= 0x30 && layerpri[1] < 0x30)
            {
                //K053936_0_zoom_draw(Video.screenstate.visarea, roz_tilemap, 0, 1);
                //glfgreat_pixel = *BITMAP_ADDR16(bitmap, 0x80, 0x105);
            }
            K052109_tilemap[sorted_layer[1]].tilemap_draw_primask(Video.screenstate.visarea, 0x10, 2);
            if (layerpri[1] >= 0x30 && layerpri[2] < 0x30)
            {
                //K053936_0_zoom_draw(Video.screenstate.visarea, roz_tilemap, 0, 1);
                //glfgreat_pixel = *BITMAP_ADDR16(bitmap, 0x80, 0x105);
            }
            K052109_tilemap[sorted_layer[2]].tilemap_draw_primask(Video.screenstate.visarea, 0x10, 4);
            if (layerpri[2] >= 0x30)
            {
                //K053936_0_zoom_draw(Video.screenstate.visarea, roz_tilemap, 0, 1);
                //glfgreat_pixel = *BITMAP_ADDR16(bitmap, 0x80, 0x105);
            }
            K053245_sprites_draw(Video.screenstate.visarea);
        }

        public static void video_update_tmnt2()
        {
            double brt;
            int i, newdim, newen, cb, ce;
            newdim = dim_v | ((~dim_c & 0x10) >> 1);
            newen = (K053251_get_priority(5) != 0 && K053251_get_priority(5) != 0x3e) ? 1 : 0;
            if (newdim != lastdim || newen != lasten)
            {
                brt = 1.0;
                if (newen != 0)
                {
                    brt -= (1.0 - 0.6) * newdim / 8;
                }
                lastdim = newdim;
                lasten = newen;
                cb = layer_colorbase[sorted_layer[2]] << 4;
                ce = cb + 128;
                /*for (i = 0; i < cb; i++)
                {
                    palette_set_brightness(screen->machine, i, brt);
                }
                for (i = cb; i < ce; i++)
                {
                    palette_set_brightness(screen->machine, i, 1.0);
                }
                for (i = ce; i < 2048; i++)
                {
                    palette_set_brightness(screen->machine, i, brt);
                }
                if ((~dim_c & 0x10)!=0)
                {
                    palette_set_shadow_mode(screen->machine, 1);
                }
                else
                {
                    palette_set_shadow_mode(screen->machine, 0);
                }*/
            }
            video_update_lgtnfght();
        }
        public static void video_update_thndrx2()
        {
            int i;
            bg_colorbase = K053251_get_palette_index(0);
            sprite_colorbase = K053251_get_palette_index(1);
            layer_colorbase[0] = K053251_get_palette_index(2);
            layer_colorbase[1] = K053251_get_palette_index(4);
            layer_colorbase[2] = K053251_get_palette_index(3);
            K052109_tilemap_update();
            sorted_layer[0] = 0;
            layerpri[0] = K053251_get_priority(2);
            sorted_layer[1] = 1;
            layerpri[1] = K053251_get_priority(4);
            sorted_layer[2] = 2;
            layerpri[2] = K053251_get_priority(3);
            sortlayers(sorted_layer, layerpri);
            Array.Clear(Tilemap.priority_bitmap, 0, 0x40000);
            for (i = 0; i < 0x20000; i++)
            {
                Video.bitmapbase_Ptrs[Video.curbitmap][i] = (ushort)(16 * bg_colorbase);
            }
            K052109_tilemap[sorted_layer[0]].tilemap_draw_primask(Video.screenstate.visarea, 0x10, 1);
            K052109_tilemap[sorted_layer[1]].tilemap_draw_primask(Video.screenstate.visarea, 0x10, 2);
            K052109_tilemap[sorted_layer[2]].tilemap_draw_primask(Video.screenstate.visarea, 0x10, 4);
            K051960_sprites_draw(Video.screenstate.visarea, -1, -1);
        }
        public static void video_eof()
        {

        }
        public static void video_eof_blswhstl()
        {
            K053245_clear_buffer(0);
        }
    }
}
