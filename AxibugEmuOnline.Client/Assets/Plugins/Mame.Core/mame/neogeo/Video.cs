using System;

namespace MAME.Core
{
    public unsafe partial class Neogeo
    {
        public static byte[] sprite_gfx;
        public static uint sprite_gfx_address_mask;
        public static ushort[] neogeo_videoram;
        public static ushort[,] palettes;
        public static int[] pens;
        private static int neogeo_scanline_param;
        private static byte palette_bank;
        private static byte screen_dark;
        private static ushort videoram_read_buffer;
        private static ushort videoram_modulo;
        private static ushort videoram_offset;
        public static byte fixed_layer_source;
        public static int neogeo_fixed_layer_bank_type;
        private static byte auto_animation_speed;
        public static byte auto_animation_disabled;
        public static int auto_animation_counter;
        private static int auto_animation_frame_counter;
        public static EmuTimer.emu_timer auto_animation_timer, sprite_line_timer;
        private static double[] rgb_weights_normal;
        private static double[] rgb_weights_normal_bit15;
        private static double[] rgb_weights_dark;
        private static double[] rgb_weights_dark_bit15;
        public static int[,] zoom_x_tables;
        private static int[] transarray, bgarray;
        private static int trans_color;
        private static byte combine_5_weights(double[] tab, int w0, int w1, int w2, int w3, int w4)
        {
            return (byte)(tab[0] * w0 + tab[1] * w1 + tab[2] * w2 + tab[3] * w3 + tab[4] * w4 + 0.5);
        }
        private static int get_pen(ushort data)
        {
            double[] weights;
            byte r, g, b;
            if (screen_dark != 0)
            {
                if ((data & 0x8000) != 0)
                    weights = rgb_weights_dark_bit15;
                else
                    weights = rgb_weights_dark;
            }
            else
            {
                if ((data & 0x8000) != 0)
                    weights = rgb_weights_normal_bit15;
                else
                    weights = rgb_weights_normal;
            }
            r = combine_5_weights(weights,
                                  (data >> 11) & 0x01,
                                  (data >> 10) & 0x01,
                                  (data >> 9) & 0x01,
                                  (data >> 8) & 0x01,
                                  (data >> 14) & 0x01);
            g = combine_5_weights(weights,
                                  (data >> 7) & 0x01,
                                  (data >> 6) & 0x01,
                                  (data >> 5) & 0x01,
                                  (data >> 4) & 0x01,
                                  (data >> 13) & 0x01);
            b = combine_5_weights(weights,
                                  (data >> 3) & 0x01,
                                  (data >> 2) & 0x01,
                                  (data >> 1) & 0x01,
                                  (data >> 0) & 0x01,
                                  (data >> 12) & 0x01);
            //return (r << 16) | (g << 8) | b;

            //通道修改，BGRA->RGBA
            return (b << 16) | (g << 8) | r;
        }
        public static void regenerate_pens()
        {
            int i;
            for (i = 0; i < 0x1000; i++)
            {
                pens[i] = get_pen(palettes[palette_bank, i]);
            }
            for (i = 0; i < 384 * 264; i++)
            {
                bgarray[i] = pens[0xfff];
            }
        }
        private static void neogeo_set_palette_bank(byte data)
        {
            if (data != palette_bank)
            {
                palette_bank = data;
                regenerate_pens();
            }
        }
        private static void neogeo_set_screen_dark(byte data)
        {
            if (data != screen_dark)
            {
                screen_dark = data;
                regenerate_pens();
            }
        }
        private static void neogeo_paletteram_w(int offset, ushort data)
        {
            int i;
            palettes[palette_bank, offset] = data;
            pens[offset] = get_pen(data);
            if (offset == 0xfff)
            {
                for (i = 0; i < 384 * 264; i++)
                {
                    bgarray[i] = pens[0xfff];
                }
            }
        }
        public static void auto_animation_timer_callback()
        {
            if (auto_animation_frame_counter == 0)
            {
                auto_animation_frame_counter = auto_animation_speed;
                auto_animation_counter = auto_animation_counter + 1;
            }
            else
            {
                auto_animation_frame_counter = auto_animation_frame_counter - 1;
            }
            EmuTimer.timer_adjust_periodic(auto_animation_timer, Video.video_screen_get_time_until_pos(0, 0), Attotime.ATTOTIME_NEVER);
        }
        private static void create_auto_animation_timer()
        {
            //auto_animation_timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.Neogeo_auto_animation_timer_callback, false);
            EmuTimer.timer_alloc_common(ref auto_animation_timer, EmuTimer.TIME_ACT.Neogeo_auto_animation_timer_callback, false);
        }
        private static void start_auto_animation_timer()
        {
            EmuTimer.timer_adjust_periodic(auto_animation_timer, Video.video_screen_get_time_until_pos(0, 0), Attotime.ATTOTIME_NEVER);
        }
        private static int rows_to_height(int rows)
        {
            if ((rows == 0) || (rows > 0x20))
                rows = 0x20;
            return rows * 0x10;
        }
        public static bool sprite_on_scanline(int scanline, int y, int rows)
        {
            int max_y = (y + rows_to_height(rows) - 1) & 0x1ff;
            return (((max_y >= y) && (scanline >= y) && (scanline <= max_y)) ||
                    ((max_y < y) && ((scanline >= y) || (scanline <= max_y))));
        }

        //private static void draw_sprites(int iBitmap, int scanline)
        //{
        //    int x_2, code_2;
        //    int x, y, rows, zoom_x, zoom_y, sprite_list_offset, sprite_index, max_sprite_index, sprite_number, sprite_y, tile, attr_and_code_offs, code, zoom_x_table_offset, gfx_offset, line_pens_offset, x_inc, sprite_line, zoom_line;
        //    ushort y_control, zoom_control, attr;
        //    byte sprite_y_and_tile;
        //    bool invert;
        //    y = 0;
        //    x = 0;
        //    rows = 0;
        //    zoom_y = 0;
        //    zoom_x = 0;
        //    if ((scanline & 0x01) != 0)
        //    {
        //        sprite_list_offset = 0x8680;
        //    }
        //    else
        //    {
        //        sprite_list_offset = 0x8600;
        //    }
        //    Span<ushort> span_neogeo_videoram = neogeo_videoram.AsSpan();
        //    Span<int> span_bitmapbaseN_iBitmap = Video.bitmapbaseN_Ptrs[iBitmap].AsSpan();
        //    Span<byte> span_sprite_gfx = sprite_gfx.AsSpan();
        //    Span<int> span_pens = pens.AsSpan();
        //    for (max_sprite_index = 95; max_sprite_index >= 0; max_sprite_index--)
        //    {
        //        if (span_neogeo_videoram[sprite_list_offset + max_sprite_index] != 0)
        //        {
        //            break;
        //        }
        //    }
        //    if (max_sprite_index != 95)
        //    {
        //        max_sprite_index = max_sprite_index + 1;
        //    }
        //    for (sprite_index = 0; sprite_index < max_sprite_index; sprite_index++)
        //    {
        //        sprite_number = span_neogeo_videoram[sprite_list_offset + sprite_index] & 0x1ff;
        //        y_control = span_neogeo_videoram[0x8200 | sprite_number];
        //        zoom_control = span_neogeo_videoram[0x8000 | sprite_number];
        //        x_2 = span_neogeo_videoram[0x8400 | sprite_number];
        //        code_2 = span_neogeo_videoram[sprite_number << 6];
        //        if ((y_control & 0x40) != 0)
        //        {
        //            x = (x + zoom_x + 1) & 0x01ff;
        //            zoom_x = (zoom_control >> 8) & 0x0f;
        //        }
        //        else
        //        {
        //            y = 0x200 - (y_control >> 7);
        //            x = span_neogeo_videoram[0x8400 | sprite_number] >> 7;
        //            zoom_y = zoom_control & 0xff;
        //            zoom_x = (zoom_control >> 8) & 0x0f;
        //            rows = y_control & 0x3f;
        //        }
        //        if ((x >= 0x140) && (x <= 0x1f0))
        //        {
        //            continue;
        //        }
        //        if (sprite_on_scanline(scanline, y, rows))
        //        {
        //            sprite_line = (scanline - y) & 0x1ff;
        //            zoom_line = sprite_line & 0xff;
        //            invert = ((sprite_line & 0x100) != 0) ? true : false;
        //            if (invert)
        //            {
        //                zoom_line ^= 0xff;
        //            }
        //            if (rows > 0x20)
        //            {
        //                zoom_line = zoom_line % ((zoom_y + 1) << 1);
        //                if (zoom_line > zoom_y)
        //                {
        //                    zoom_line = ((zoom_y + 1) << 1) - 1 - zoom_line;
        //                    invert = !invert;
        //                }
        //            }
        //            sprite_y_and_tile = zoomyrom[(zoom_y << 8) | zoom_line];
        //            sprite_y = sprite_y_and_tile & 0x0f;
        //            tile = sprite_y_and_tile >> 4;
        //            if (invert)
        //            {
        //                sprite_y ^= 0x0f;
        //                tile ^= 0x1f;
        //            }
        //            attr_and_code_offs = (sprite_number << 6) | (tile << 1);
        //            attr = span_neogeo_videoram[attr_and_code_offs + 1];
        //            code = ((attr << 12) & 0x70000) | span_neogeo_videoram[attr_and_code_offs];
        //            if (auto_animation_disabled == 0)
        //            {
        //                if ((attr & 0x0008) != 0)
        //                {
        //                    code = (code & ~0x07) | (auto_animation_counter & 0x07);
        //                }
        //                else if ((attr & 0x0004) != 0)
        //                {
        //                    code = (code & ~0x03) | (auto_animation_counter & 0x03);
        //                }
        //            }
        //            if ((attr & 0x0002) != 0)
        //            {
        //                sprite_y ^= 0x0f;
        //            }
        //            zoom_x_table_offset = 0;
        //            gfx_offset = (int)(((code << 8) | (sprite_y << 4)) & sprite_gfx_address_mask);
        //            line_pens_offset = attr >> 8 << 4;
        //            if ((attr & 0x0001) != 0)
        //            {
        //                gfx_offset = gfx_offset + 0x0f;
        //                x_inc = -1;
        //            }
        //            else
        //            {
        //                x_inc = 1;
        //            }
        //            int pixel_addr_offsetx, pixel_addr_offsety;
        //            if (x <= 0x01f0)
        //            {
        //                int i;
        //                pixel_addr_offsetx = x + NEOGEO_HBEND;
        //                pixel_addr_offsety = scanline;
        //                for (i = 0; i < 0x10; i++)
        //                {
        //                    if (zoom_x_tables[zoom_x, zoom_x_table_offset] != 0)
        //                    {
        //                        //if (sprite_gfx[gfx_offset] != 0)
        //                        if (span_sprite_gfx[gfx_offset] != 0)
        //                        {
        //                            //Video.bitmapbaseN_Ptrs[iBitmap][pixel_addr_offsety * 384 + pixel_addr_offsetx] = pens[line_pens_offset + sprite_gfx[gfx_offset]];
        //                            span_bitmapbaseN_iBitmap[pixel_addr_offsety * 384 + pixel_addr_offsetx] = span_pens[line_pens_offset + span_sprite_gfx[gfx_offset]];
        //                        }
        //                        pixel_addr_offsetx++;
        //                    }
        //                    zoom_x_table_offset++;
        //                    gfx_offset += x_inc;
        //                }
        //            }
        //            else
        //            {
        //                int i;
        //                int x_save = x;
        //                pixel_addr_offsetx = NEOGEO_HBEND;
        //                pixel_addr_offsety = scanline;
        //                for (i = 0; i < 0x10; i++)
        //                {
        //                    if (zoom_x_tables[zoom_x, zoom_x_table_offset] != 0)
        //                    {
        //                        if (x >= 0x200)
        //                        {
        //                            //if (sprite_gfx[gfx_offset] != 0)
        //                            if (span_sprite_gfx[gfx_offset] != 0)
        //                            {
        //                                //Video.bitmapbaseN_Ptrs[iBitmap][pixel_addr_offsety * 384 + pixel_addr_offsetx] = pens[line_pens_offset + sprite_gfx[gfx_offset]];
        //                                span_bitmapbaseN_iBitmap[pixel_addr_offsety * 384 + pixel_addr_offsetx] = span_pens[line_pens_offset + span_sprite_gfx[gfx_offset]];
        //                            }
        //                            pixel_addr_offsetx++;
        //                        }
        //                        x++;
        //                    }
        //                    zoom_x_table_offset++;
        //                    gfx_offset += x_inc;
        //                }
        //                x = x_save;
        //            }
        //        }
        //    }
        //}

        /// <summary>
        ///  draw_sprites (Unsafa 尝试提升效率）
        /// </summary>
        /// <param name="iBitmap"></param>
        /// <param name="scanline"></param>
        unsafe private static void draw_sprites(int iBitmap, int scanline)
        {

            fixed (ushort* videoramPtr = &neogeo_videoram[0])
            //fixed (int* bitmapbasePtr = &Video.bitmapbaseN_Ptrs[iBitmap][0])
            fixed (byte* spriteGfxPtr = &sprite_gfx[0])
            fixed (int* pensPtr = &pens[0])
            fixed (byte* zoomyromPtr = &zoomyrom[0])
            {
                ushort* neogeo_videoram = videoramPtr;
                //int* bitmapbase = bitmapbasePtr;
                int* bitmapbase = &Video.bitmapbaseN_Ptrs[iBitmap][0];
                byte* spriteGfx = spriteGfxPtr;
                int* pens = pensPtr;
                byte* zoomyrom = zoomyromPtr;

                int x_2, code_2;
                int x, y, rows, zoom_x, zoom_y, sprite_list_offset, sprite_index, max_sprite_index, sprite_number, sprite_y, tile, attr_and_code_offs, code, zoom_x_table_offset, gfx_offset, line_pens_offset, x_inc, sprite_line, zoom_line;
                ushort y_control, zoom_control, attr;
                byte sprite_y_and_tile;
                bool invert;
                y = 0;
                x = 0;
                rows = 0;
                zoom_y = 0;
                zoom_x = 0;
                if ((scanline & 0x01) != 0)
                {
                    sprite_list_offset = 0x8680;
                }
                else
                {
                    sprite_list_offset = 0x8600;
                }
                for (max_sprite_index = 95; max_sprite_index >= 0; max_sprite_index--)
                {
                    if (neogeo_videoram[sprite_list_offset + max_sprite_index] != 0)
                    {
                        break;
                    }
                }
                if (max_sprite_index != 95)
                {
                    max_sprite_index = max_sprite_index + 1;
                }
                for (sprite_index = 0; sprite_index < max_sprite_index; sprite_index++)
                {
                    sprite_number = neogeo_videoram[sprite_list_offset + sprite_index] & 0x1ff;
                    y_control = neogeo_videoram[0x8200 | sprite_number];
                    zoom_control = neogeo_videoram[0x8000 | sprite_number];
                    x_2 = neogeo_videoram[0x8400 | sprite_number];
                    code_2 = neogeo_videoram[sprite_number << 6];

                    //sprite_number = (*(videoram + sprite_list_offset + sprite_index) & 0x1ff);
                    //y_control = (ushort)(*(videoram + 0x8200) | sprite_number);
                    //zoom_control = (ushort)(*(videoram + 0x8000) | sprite_number);
                    //x_2 = (ushort)(*(videoram + 0x8400) | sprite_number);
                    //code_2 = *(videoram + (sprite_number << 6));

                    if ((y_control & 0x40) != 0)
                    {
                        x = (x + zoom_x + 1) & 0x01ff;
                        zoom_x = (zoom_control >> 8) & 0x0f;
                    }
                    else
                    {
                        y = 0x200 - (y_control >> 7);
                        //x = neogeo_videoram[0x8400 | sprite_number] >> 7;
                        x = x_2 >> 7;
                        zoom_y = zoom_control & 0xff;
                        zoom_x = (zoom_control >> 8) & 0x0f;
                        rows = y_control & 0x3f;
                    }

                    fixed (int* zoom_x_tablesPtr = &zoom_x_tables[zoom_x, 0])
                    {
                        int* zoom_x_tables = zoom_x_tablesPtr;

                        if ((x >= 0x140) && (x <= 0x1f0))
                        {
                            continue;
                        }
                        if (sprite_on_scanline(scanline, y, rows))
                        {
                            sprite_line = (scanline - y) & 0x1ff;
                            zoom_line = sprite_line & 0xff;
                            invert = ((sprite_line & 0x100) != 0) ? true : false;
                            if (invert)
                            {
                                zoom_line ^= 0xff;
                            }
                            if (rows > 0x20)
                            {
                                zoom_line = zoom_line % ((zoom_y + 1) << 1);
                                if (zoom_line > zoom_y)
                                {
                                    zoom_line = ((zoom_y + 1) << 1) - 1 - zoom_line;
                                    invert = !invert;
                                }
                            }
                            sprite_y_and_tile = zoomyrom[(zoom_y << 8) | zoom_line];
                            sprite_y = sprite_y_and_tile & 0x0f;
                            tile = sprite_y_and_tile >> 4;
                            if (invert)
                            {
                                sprite_y ^= 0x0f;
                                tile ^= 0x1f;
                            }
                            attr_and_code_offs = (sprite_number << 6) | (tile << 1);
                            attr = neogeo_videoram[attr_and_code_offs + 1];
                            code = ((attr << 12) & 0x70000) | neogeo_videoram[attr_and_code_offs];
                            if (auto_animation_disabled == 0)
                            {
                                if ((attr & 0x0008) != 0)
                                {
                                    code = (code & ~0x07) | (auto_animation_counter & 0x07);
                                }
                                else if ((attr & 0x0004) != 0)
                                {
                                    code = (code & ~0x03) | (auto_animation_counter & 0x03);
                                }
                            }
                            if ((attr & 0x0002) != 0)
                            {
                                sprite_y ^= 0x0f;
                            }
                            zoom_x_table_offset = 0;
                            gfx_offset = (int)(((code << 8) | (sprite_y << 4)) & sprite_gfx_address_mask);
                            line_pens_offset = attr >> 8 << 4;
                            if ((attr & 0x0001) != 0)
                            {
                                gfx_offset = gfx_offset + 0x0f;
                                x_inc = -1;
                            }
                            else
                            {
                                x_inc = 1;
                            }
                            int pixel_addr_offsetx, pixel_addr_offsety;
                            if (x <= 0x01f0)
                            {
                                int i;
                                pixel_addr_offsetx = x + NEOGEO_HBEND;
                                pixel_addr_offsety = scanline;
                                for (i = 0; i < 0x10; i++)
                                {
                                    //if (zoom_x_tables[zoom_x, zoom_x_table_offset] != 0)
                                    if (zoom_x_tables[zoom_x_table_offset] != 0)
                                    {
                                        //if (sprite_gfx[gfx_offset] != 0)
                                        if (spriteGfx[gfx_offset] != 0)
                                        {
                                            //Video.bitmapbaseN_Ptrs[iBitmap][pixel_addr_offsety * 384 + pixel_addr_offsetx] = pens[line_pens_offset + sprite_gfx[gfx_offset]];
                                            bitmapbase[pixel_addr_offsety * 384 + pixel_addr_offsetx] = pens[line_pens_offset + spriteGfx[gfx_offset]];
                                        }
                                        pixel_addr_offsetx++;
                                    }
                                    zoom_x_table_offset++;
                                    gfx_offset += x_inc;
                                }
                            }
                            else
                            {
                                int i;
                                int x_save = x;
                                pixel_addr_offsetx = NEOGEO_HBEND;
                                pixel_addr_offsety = scanline;
                                for (i = 0; i < 0x10; i++)
                                {
                                    //if (zoom_x_tables[zoom_x, zoom_x_table_offset] != 0)
                                    if (zoom_x_tables[zoom_x_table_offset] != 0)
                                    {
                                        if (x >= 0x200)
                                        {
                                            //if (sprite_gfx[gfx_offset] != 0)
                                            if (spriteGfx[gfx_offset] != 0)
                                            {
                                                //Video.bitmapbaseN_Ptrs[iBitmap][pixel_addr_offsety * 384 + pixel_addr_offsetx] = pens[line_pens_offset + sprite_gfx[gfx_offset]];
                                                bitmapbase[pixel_addr_offsety * 384 + pixel_addr_offsetx] = pens[line_pens_offset + spriteGfx[gfx_offset]];
                                            }
                                            pixel_addr_offsetx++;
                                        }
                                        x++;
                                    }
                                    zoom_x_table_offset++;
                                    gfx_offset += x_inc;
                                }
                                x = x_save;
                            }
                        }
                    }
                }
            }
        }

        //private static void draw_sprites(int iBitmap, int scanline)
        //{
        //    int x_2, code_2;
        //    int x, y, rows, zoom_x, zoom_y, sprite_list_offset, sprite_index, max_sprite_index, sprite_number, sprite_y, tile, attr_and_code_offs, code, zoom_x_table_offset, gfx_offset, line_pens_offset, x_inc, sprite_line, zoom_line;
        //    ushort y_control, zoom_control, attr;
        //    byte sprite_y_and_tile;
        //    bool invert;
        //    y = 0;
        //    x = 0;
        //    rows = 0;
        //    zoom_y = 0;
        //    zoom_x = 0;
        //    if ((scanline & 0x01) != 0)
        //    {
        //        sprite_list_offset = 0x8680;
        //    }
        //    else
        //    {
        //        sprite_list_offset = 0x8600;
        //    }
        //    for (max_sprite_index = 95; max_sprite_index >= 0; max_sprite_index--)
        //    {
        //        if (neogeo_videoram[sprite_list_offset + max_sprite_index] != 0)
        //        {
        //            break;
        //        }
        //    }
        //    if (max_sprite_index != 95)
        //    {
        //        max_sprite_index = max_sprite_index + 1;
        //    }
        //    for (sprite_index = 0; sprite_index < max_sprite_index; sprite_index++)
        //    {
        //        sprite_number = neogeo_videoram[sprite_list_offset + sprite_index] & 0x1ff;
        //        y_control = neogeo_videoram[0x8200 | sprite_number];
        //        zoom_control = neogeo_videoram[0x8000 | sprite_number];
        //        x_2 = neogeo_videoram[0x8400 | sprite_number];
        //        code_2 = neogeo_videoram[sprite_number << 6];
        //        if ((y_control & 0x40) != 0)
        //        {
        //            x = (x + zoom_x + 1) & 0x01ff;
        //            zoom_x = (zoom_control >> 8) & 0x0f;
        //        }
        //        else
        //        {
        //            y = 0x200 - (y_control >> 7);
        //            x = neogeo_videoram[0x8400 | sprite_number] >> 7;
        //            zoom_y = zoom_control & 0xff;
        //            zoom_x = (zoom_control >> 8) & 0x0f;
        //            rows = y_control & 0x3f;
        //        }
        //        if ((x >= 0x140) && (x <= 0x1f0))
        //        {
        //            continue;
        //        }
        //        if (sprite_on_scanline(scanline, y, rows))
        //        {
        //            sprite_line = (scanline - y) & 0x1ff;
        //            zoom_line = sprite_line & 0xff;
        //            invert = ((sprite_line & 0x100) != 0) ? true : false;
        //            if (invert)
        //            {
        //                zoom_line ^= 0xff;
        //            }
        //            if (rows > 0x20)
        //            {
        //                zoom_line = zoom_line % ((zoom_y + 1) << 1);
        //                if (zoom_line > zoom_y)
        //                {
        //                    zoom_line = ((zoom_y + 1) << 1) - 1 - zoom_line;
        //                    invert = !invert;
        //                }
        //            }
        //            sprite_y_and_tile = zoomyrom[(zoom_y << 8) | zoom_line];
        //            sprite_y = sprite_y_and_tile & 0x0f;
        //            tile = sprite_y_and_tile >> 4;
        //            if (invert)
        //            {
        //                sprite_y ^= 0x0f;
        //                tile ^= 0x1f;
        //            }
        //            attr_and_code_offs = (sprite_number << 6) | (tile << 1);
        //            attr = neogeo_videoram[attr_and_code_offs + 1];
        //            code = ((attr << 12) & 0x70000) | neogeo_videoram[attr_and_code_offs];
        //            if (auto_animation_disabled == 0)
        //            {
        //                if ((attr & 0x0008) != 0)
        //                {
        //                    code = (code & ~0x07) | (auto_animation_counter & 0x07);
        //                }
        //                else if ((attr & 0x0004) != 0)
        //                {
        //                    code = (code & ~0x03) | (auto_animation_counter & 0x03);
        //                }
        //            }
        //            if ((attr & 0x0002) != 0)
        //            {
        //                sprite_y ^= 0x0f;
        //            }
        //            zoom_x_table_offset = 0;
        //            gfx_offset = (int)(((code << 8) | (sprite_y << 4)) & sprite_gfx_address_mask);
        //            line_pens_offset = attr >> 8 << 4;
        //            if ((attr & 0x0001) != 0)
        //            {
        //                gfx_offset = gfx_offset + 0x0f;
        //                x_inc = -1;
        //            }
        //            else
        //            {
        //                x_inc = 1;
        //            }
        //            int pixel_addr_offsetx, pixel_addr_offsety;
        //            if (x <= 0x01f0)
        //            {
        //                int i;
        //                pixel_addr_offsetx = x + NEOGEO_HBEND;
        //                pixel_addr_offsety = scanline;
        //                for (i = 0; i < 0x10; i++)
        //                {
        //                    if (zoom_x_tables[zoom_x, zoom_x_table_offset] != 0)
        //                    {
        //                        if (sprite_gfx[gfx_offset] != 0)
        //                        {
        //                            Video.bitmapbaseN_Ptrs[iBitmap][pixel_addr_offsety * 384 + pixel_addr_offsetx] = pens[line_pens_offset + sprite_gfx[gfx_offset]];
        //                        }
        //                        pixel_addr_offsetx++;
        //                    }
        //                    zoom_x_table_offset++;
        //                    gfx_offset += x_inc;
        //                }
        //            }
        //            else
        //            {
        //                int i;
        //                int x_save = x;
        //                pixel_addr_offsetx = NEOGEO_HBEND;
        //                pixel_addr_offsety = scanline;
        //                for (i = 0; i < 0x10; i++)
        //                {
        //                    if (zoom_x_tables[zoom_x, zoom_x_table_offset] != 0)
        //                    {
        //                        if (x >= 0x200)
        //                        {
        //                            if (sprite_gfx[gfx_offset] != 0)
        //                            {
        //                                Video.bitmapbaseN_Ptrs[iBitmap][pixel_addr_offsety * 384 + pixel_addr_offsetx] = pens[line_pens_offset + sprite_gfx[gfx_offset]];
        //                            }
        //                            pixel_addr_offsetx++;
        //                        }
        //                        x++;
        //                    }
        //                    zoom_x_table_offset++;
        //                    gfx_offset += x_inc;
        //                }
        //                x = x_save;
        //            }
        //        }
        //    }
        //}
        private static void parse_sprites(int scanline)
        {
            ushort sprite_number, y_control;
            int y = 0;
            int rows = 0;
            int sprite_list_offset;
            int active_sprite_count = 0;
            if ((scanline & 0x01) != 0)
            {
                sprite_list_offset = 0x8680;
            }
            else
            {
                sprite_list_offset = 0x8600;
            }
            for (sprite_number = 0; sprite_number < 381; sprite_number++)
            {
                y_control = neogeo_videoram[0x8200 | sprite_number];
                if ((~y_control & 0x40) != 0)
                {
                    y = 0x200 - (y_control >> 7);
                    rows = y_control & 0x3f;
                }
                if (rows == 0)
                {
                    continue;
                }
                if (!sprite_on_scanline(scanline, y, rows))
                {
                    continue;
                }
                neogeo_videoram[sprite_list_offset] = sprite_number;
                sprite_list_offset++;
                active_sprite_count++;
                if (active_sprite_count == 96)
                {
                    break;
                }
            }
            for (; active_sprite_count <= 96; active_sprite_count++)
            {
                neogeo_videoram[sprite_list_offset] = 0;
                sprite_list_offset++;
            }
        }
        public static void sprite_line_timer_callback()
        {
            int scanline = neogeo_scanline_param;
            if (scanline != 0)
            {
                Video.video_screen_update_partial(scanline - 1);
            }
            parse_sprites(scanline);
            scanline = (scanline + 1) % 264;
            neogeo_scanline_param = scanline;
            EmuTimer.timer_adjust_periodic(sprite_line_timer, Video.video_screen_get_time_until_pos(scanline, 0), Attotime.ATTOTIME_NEVER);
        }
        private static void create_sprite_line_timer()
        {
            EmuTimer.timer_alloc_common(ref sprite_line_timer, EmuTimer.TIME_ACT.Neogeo_sprite_line_timer_callback, false);
        }
        private static void start_sprite_line_timer()
        {
            neogeo_scanline_param = 0;
            EmuTimer.timer_adjust_periodic(sprite_line_timer, Video.video_screen_get_time_until_pos(0, 0), Attotime.ATTOTIME_NEVER);
        }
        readonly static int[] pix_offsets = new int[] { 0x10, 0x18, 0x00, 0x08 };
        static int[] garouoffsets = new int[32];
        private static void draw_fixed_layer(int iBitmap, int scanline)
        {
            int i, j, x, y;
            byte[] gfx_base;
            //int[] garouoffsets = new int[32], pix_offsets = new int[] { 0x10, 0x18, 0x00, 0x08 };
            int addr_mask;
            int gfx_offset, char_pens_offset;
            byte data;
            bool banked;
            int garoubank, k, code;
            ushort code_and_palette;
            if (fixed_layer_source != 0)
            {
                gfx_base = fixedrom;
                addr_mask = fixedrom.Length - 1;
            }
            else
            {
                gfx_base = fixedbiosrom;
                addr_mask = fixedbiosrom.Length - 1;
            }
            int video_data_offset = 0x7000 | (scanline >> 3);
            banked = (fixed_layer_source != 0) && (addr_mask > 0x1ffff);
            if (banked && neogeo_fixed_layer_bank_type == 1)
            {
                garoubank = 0;
                k = 0;
                y = 0;
                while (y < 32)
                {
                    if (neogeo_videoram[0x7500 + k] == 0x0200 && (neogeo_videoram[0x7580 + k] & 0xff00) == 0xff00)
                    {
                        garoubank = neogeo_videoram[0x7580 + k] & 3;
                        garouoffsets[y++] = garoubank;
                    }
                    garouoffsets[y++] = garoubank;
                    k += 2;
                }
            }
            for (x = 0; x < 40; x++)
            {
                code_and_palette = neogeo_videoram[video_data_offset];
                code = code_and_palette & 0x0fff;
                if (banked)
                {
                    y = scanline >> 3;
                    switch (neogeo_fixed_layer_bank_type)
                    {
                        case 1:
                            code += 0x1000 * (garouoffsets[(y - 2) & 31] ^ 3);
                            break;
                        case 2:
                            code += 0x1000 * (((neogeo_videoram[0x7500 + ((y - 1) & 31) + 32 * (x / 6)] >> (5 - (x % 6)) * 2) & 3) ^ 3);
                            break;
                    }
                }
                data = 0;
                gfx_offset = ((code << 5) | (scanline & 0x07)) & addr_mask;
                char_pens_offset = code_and_palette >> 12 << 4;
                for (i = 0; i < 8; i++)
                {
                    if ((i & 0x01) != 0)
                    {
                        data = (byte)(data >> 4);
                    }
                    else
                    {
                        data = gfx_base[gfx_offset + pix_offsets[i >> 1]];
                    }
                    if ((data & 0x0f) != 0)
                    {
                        Video.bitmapbaseN_Ptrs[iBitmap][384 * scanline + 30 + x * 8 + i] = pens[char_pens_offset + (data & 0x0f)];
                    }
                }
                video_data_offset += 0x20;
            }
        }
        private static void optimize_sprite_data()
        {
            sprite_gfx_address_mask = (uint)(spritesromLength * 2 - 1);
            for (int i = 0; i < spritesromLength; i++)
            {
                sprite_gfx[i * 2] = (byte)((spritesrom[i] & 0xf0) >> 4);
                sprite_gfx[i * 2 + 1] = (byte)(spritesrom[i] & 0x0f);
            }
        }
        private static ushort get_video_control()
        {
            int ret;
            int v_counter;
            v_counter = Video.video_screen_get_vpos() + 0x100;
            if (v_counter >= 0x200)
                v_counter = v_counter - NEOGEO_VTOTAL;
            ret = (v_counter << 7) | (auto_animation_counter & 0x07);
            return (ushort)ret;
        }
        private static ushort neogeo_video_register_r(int offset)
        {
            ushort ret;
            switch (offset)
            {
                default:
                case 0x00:
                case 0x01:
                    ret = videoram_read_buffer;
                    break;
                case 0x02:
                    ret = videoram_modulo;
                    break;
                case 0x03:
                    ret = get_video_control();
                    break;
            }
            return ret;
        }
        private static void neogeo_video_register_w(int offset, ushort data)
        {
            switch (offset)
            {
                case 0x00:
                    videoram_offset = data;
                    videoram_read_buffer = neogeo_videoram[videoram_offset];
                    break;
                case 0x01:
                    if (videoram_offset == 0x842d && data == 0x0)
                    {
                        int i1 = 1;
                    }
                    if (videoram_offset == 0x8263 && data == 0xb102)
                    {
                        int i1 = 1;
                    }
                    if (videoram_offset == 0x18c0 && data == 0xcb06)
                    {
                        int i1 = 1;
                    }
                    neogeo_videoram[videoram_offset] = data;
                    videoram_offset = (ushort)((videoram_offset & 0x8000) | ((videoram_offset + videoram_modulo) & 0x7fff));
                    videoram_read_buffer = neogeo_videoram[videoram_offset];
                    break;
                case 0x02:
                    videoram_modulo = data;
                    break;
                case 0x03:
                    auto_animation_speed = (byte)(data >> 8);
                    auto_animation_disabled = (byte)(data & 0x08);
                    display_position_interrupt_control = (byte)(data & 0xf0);
                    break;
                case 0x04:
                    display_counter = (display_counter & 0x0000ffff) | ((uint)data << 16);
                    break;
                case 0x05:
                    display_counter = (display_counter & 0xffff0000) | data;
                    if ((display_position_interrupt_control & 0x20) != 0)
                    {
                        adjust_display_position_interrupt_timer();
                    }
                    break;
                case 0x06:
                    if ((data & 0x01) != 0)
                        irq3_pending = 0;
                    if ((data & 0x02) != 0)
                        display_position_interrupt_pending = 0;
                    if ((data & 0x04) != 0)
                        vblank_interrupt_pending = 0;
                    update_interrupts();
                    break;
                case 0x07:
                    break;
            }
        }
        public static void video_start_neogeo()
        {
            sprite_gfx = new byte[spritesromLength * 2];
            neogeo_videoram = new ushort[0x10000];
            palettes = new ushort[2, 0x1000];
            pens = new int[0x1000];
            transarray = new int[384 * 264];
            bgarray = new int[384 * 264];
            trans_color = unchecked((int)0xffff00ff);
            rgb_weights_normal = new double[5] { 138.24919544273797, 64.712389372353314, 30.414823021125919, 13.824919571647138, 7.7986725921356159 };
            rgb_weights_normal_bit15 = new double[5] { 136.26711031260342, 63.784604843122239, 29.978764292156193, 13.626711058241233, 7.6868626613063098 };
            rgb_weights_dark = new double[5] { 77.012238506947057, 36.048281863327709, 16.942692484743652, 7.7012238659431276, 4.3442801368916566 };
            rgb_weights_dark_bit15 = new double[5] { 76.322306339305158, 35.725334891159271, 16.790907407744047, 7.6322306490423326, 4.3053608862660706 };
            zoom_x_tables = new int[,]
            {
                { 0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0 },
                { 0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0 },
                { 0,0,0,0,1,0,0,0,1,0,0,0,1,0,0,0 },
                { 0,0,1,0,1,0,0,0,1,0,0,0,1,0,0,0 },
                { 0,0,1,0,1,0,0,0,1,0,0,0,1,0,1,0 },
                { 0,0,1,0,1,0,1,0,1,0,0,0,1,0,1,0 },
                { 0,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0 },
                { 1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0 },
                { 1,0,1,0,1,0,1,0,1,1,1,0,1,0,1,0 },
                { 1,0,1,1,1,0,1,0,1,1,1,0,1,0,1,0 },
                { 1,0,1,1,1,0,1,0,1,1,1,0,1,0,1,1 },
                { 1,0,1,1,1,0,1,1,1,1,1,0,1,0,1,1 },
                { 1,0,1,1,1,0,1,1,1,1,1,0,1,1,1,1 },
                { 1,1,1,1,1,0,1,1,1,1,1,0,1,1,1,1 },
                { 1,1,1,1,1,0,1,1,1,1,1,1,1,1,1,1 },
                { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1 }
            };
            Array.Clear(palettes, 0, 0x2000);
            Array.Clear(pens, 0, 0x1000);
            Array.Clear(neogeo_videoram, 0, 0x10000);
            for (int i = 0; i < 384 * 264; i++)
            {
                transarray[i] = trans_color;
            }
            create_sprite_line_timer();
            create_auto_animation_timer();
            optimize_sprite_data();
            videoram_read_buffer = 0;
            videoram_offset = 0;
            videoram_modulo = 0;
            auto_animation_speed = 0;
            auto_animation_disabled = 0;
            auto_animation_counter = 0;
            auto_animation_frame_counter = 0;
        }
        public static void video_update_neogeo()
        {
            Array.Copy(bgarray, 0, Video.bitmapbaseN[Video.curbitmap], 384 * Video.new_clip.min_y, 384 * (Video.new_clip.max_y - Video.new_clip.min_y + 1));
            draw_sprites(Video.curbitmap, Video.new_clip.min_y);
            draw_fixed_layer(Video.curbitmap, Video.new_clip.min_y);
        }
        public static void video_eof_neogeo()
        {

        }
    }
}
