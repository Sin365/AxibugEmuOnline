using System;

namespace MAME.Core
{
    public unsafe partial class M92
    {
        public static ushort[] pf_master_control;
        public static int m92_sprite_list;
        public static ushort[] m92_vram_data;
        public static ushort[] m92_spritecontrol;
        public static int m92_game_kludge;
        private static ushort[] uuB800;
        public static int m92_palette_bank;
        public struct pf_layer_info
        {
            public Tmap tmap;
            public Tmap wide_tmap;
            public ushort vram_base;
            public ushort[] control;
        };
        public static pf_layer_info[] pf_layer;
        public static void spritebuffer_callback()
        {
            m92_sprite_buffer_busy = 1;
            if (m92_game_kludge != 2)
            {
                m92_sprite_interrupt();
            }
        }
        public static void m92_spritecontrol_w1(int offset, byte data)
        {
            m92_spritecontrol[offset] = (ushort)((data << 8) | (m92_spritecontrol[offset] & 0xff));
            /*if (offset == 2)
            {
                if ((data & 0xff) == 8)
                {
                    m92_sprite_list = (((0x100 - m92_spritecontrol[0]) & 0xff) * 4);
                }
                else
                {
                    m92_sprite_list = 0x400;
                }
            }*/
            if (offset == 4)
            {
                Generic.buffer_spriteram16_w();
                m92_sprite_buffer_busy = 0;
                EmuTimer.emu_timer timer = EmuTimer.timer_alloc_common_NoRef(EmuTimer.TIME_ACT.M92_spritebuffer_callback, true);
                EmuTimer.timer_adjust_periodic(timer, Attotime.attotime_mul(new Atime(0, (long)(1e18 / 26666000)), 0x400), Attotime.ATTOTIME_NEVER);
            }
        }
        public static void m92_spritecontrol_w2(int offset, byte data)
        {
            m92_spritecontrol[offset] = (ushort)((m92_spritecontrol[offset] & 0xff00) | data);
            if (offset == 2)
            {
                if ((data & 0xff) == 8)
                {
                    m92_sprite_list = (((0x100 - m92_spritecontrol[0]) & 0xff) * 4);
                }
                else
                {
                    m92_sprite_list = 0x400;
                }
            }
            if (offset == 4)
            {
                Generic.buffer_spriteram16_w();
                m92_sprite_buffer_busy = 0;
                EmuTimer.emu_timer timer = EmuTimer.timer_alloc_common_NoRef(EmuTimer.TIME_ACT.M92_spritebuffer_callback, true);
                EmuTimer.timer_adjust_periodic(timer, Attotime.attotime_mul(new Atime(0, (long)(1e18 / 26666000)), 0x400), Attotime.ATTOTIME_NEVER);
            }
        }
        public static void m92_spritecontrol_w(int offset, ushort data)
        {
            m92_spritecontrol[offset] = data;
            if (offset == 2)
            {
                if ((data & 0xff) == 8)
                {
                    m92_sprite_list = (((0x100 - m92_spritecontrol[0]) & 0xff) * 4);
                }
                else
                {
                    m92_sprite_list = 0x400;
                }
            }
            if (offset == 4)
            {
                Generic.buffer_spriteram16_w();
                m92_sprite_buffer_busy = 0;
                EmuTimer.emu_timer timer = EmuTimer.timer_alloc_common_NoRef(EmuTimer.TIME_ACT.M92_spritebuffer_callback, true);
                EmuTimer.timer_adjust_periodic(timer, Attotime.attotime_mul(new Atime(0, (long)(1e18 / 26666000)), 0x400), Attotime.ATTOTIME_NEVER);
            }
        }
        public static void m92_videocontrol_w(byte data)
        {
            //if (ACCESSING_BITS_0_7)
            {
                m92_palette_bank = (data >> 1) & 1;
            }
        }
        public static ushort m92_paletteram_r(int offset)
        {
            return Generic.paletteram16[offset + 0x400 * m92_palette_bank];
        }
        public static void m92_paletteram_w(int offset, ushort data)
        {
            Generic.paletteram16_xBBBBBGGGGGRRRRR_word_w(offset + 0x400 * m92_palette_bank, data);
        }
        public static void m92_vram_w(int offset)
        {
            int laynum;
            //COMBINE_DATA(&m92_vram_data[offset]);
            for (laynum = 0; laynum < 3; laynum++)
            {
                if ((offset & 0x6000) == pf_layer[laynum].vram_base)
                {
                    pf_layer[laynum].tmap.tilemap_mark_tile_dirty(((offset & 0x1fff) / 2) / 0x40, ((offset & 0x1fff) / 2) % 0x40);//tilemap_mark_tile_dirty((offset & 0x1fff) / 2);
                    pf_layer[laynum].wide_tmap.tilemap_mark_tile_dirty(((offset & 0x3fff) / 2) / 0x80, ((offset & 0x3fff) / 2) % 0x80);
                }
                if ((offset & 0x6000) == pf_layer[laynum].vram_base + 0x2000)
                {
                    pf_layer[laynum].wide_tmap.tilemap_mark_tile_dirty(((offset & 0x3fff) / 2) / 0x80, ((offset & 0x3fff) / 2) % 0x80);
                }
            }
        }
        public static void m92_pf1_control_w1(int offset, byte data)
        {
            pf_layer[0].control[offset] = (ushort)((data << 8) | (pf_layer[0].control[offset] & 0xff));
        }
        public static void m92_pf1_control_w2(int offset, byte data)
        {
            pf_layer[0].control[offset] = (ushort)((pf_layer[0].control[offset] & 0xff00) | data);
        }
        public static void m92_pf1_control_w(int offset, ushort data)
        {
            pf_layer[0].control[offset] = data;
        }
        public static void m92_pf2_control_w1(int offset, byte data)
        {
            pf_layer[1].control[offset] = (ushort)((data << 8) | (pf_layer[1].control[offset] & 0xff));
        }
        public static void m92_pf2_control_w2(int offset, byte data)
        {
            pf_layer[1].control[offset] = (ushort)((pf_layer[1].control[offset] & 0xff00) | data);
        }
        public static void m92_pf2_control_w(int offset, ushort data)
        {
            pf_layer[1].control[offset] = data;
        }
        public static void m92_pf3_control_w1(int offset, byte data)
        {
            pf_layer[2].control[offset] = (ushort)((data << 8) | (pf_layer[2].control[offset] & 0xff));
        }
        public static void m92_pf3_control_w2(int offset, byte data)
        {
            pf_layer[2].control[offset] = (ushort)((pf_layer[2].control[offset] & 0xff00) | data);
        }
        public static void m92_pf3_control_w(int offset, ushort data)
        {
            pf_layer[2].control[offset] = data;
        }
        public static void m92_master_control_w1(int offset, byte data)
        {
            ushort old = pf_master_control[offset];
            pf_master_control[offset] = (ushort)((data << 8) | (pf_master_control[offset] & 0xff));
            switch (offset)
            {
                case 0:
                case 1:
                case 2:
                    pf_layer[offset].vram_base = (ushort)((pf_master_control[offset] & 3) * 0x2000);
                    if ((pf_master_control[offset] & 0x04) != 0)
                    {
                        pf_layer[offset].tmap.enable = false;
                        pf_layer[offset].wide_tmap.enable = ((~pf_master_control[offset] >> 4) & 1) != 0 ? true : false;
                    }
                    else
                    {
                        pf_layer[offset].tmap.enable = ((~pf_master_control[offset] >> 4) & 1) != 0 ? true : false;
                        pf_layer[offset].wide_tmap.enable = false;
                    }
                    if (((old ^ pf_master_control[offset]) & 0x07) != 0)
                    {
                        pf_layer[offset].tmap.all_tiles_dirty = true;
                        pf_layer[offset].wide_tmap.all_tiles_dirty = true;
                    }
                    break;
                case 3:
                    m92_raster_irq_position = pf_master_control[3] - 128;
                    break;
            }
        }
        public static void m92_master_control_w2(int offset, byte data)
        {
            ushort old = pf_master_control[offset];
            pf_master_control[offset] = (ushort)((pf_master_control[offset] & 0xff00) | data);
            switch (offset)
            {
                case 0:
                case 1:
                case 2:
                    pf_layer[offset].vram_base = (ushort)((pf_master_control[offset] & 3) * 0x2000);
                    if ((pf_master_control[offset] & 0x04) != 0)
                    {
                        pf_layer[offset].tmap.enable = false;
                        pf_layer[offset].wide_tmap.enable = ((~pf_master_control[offset] >> 4) & 1) != 0 ? true : false;
                    }
                    else
                    {
                        pf_layer[offset].tmap.enable = ((~pf_master_control[offset] >> 4) & 1) != 0 ? true : false;
                        pf_layer[offset].wide_tmap.enable = false;
                    }
                    if (((old ^ pf_master_control[offset]) & 0x07) != 0)
                    {
                        pf_layer[offset].tmap.all_tiles_dirty = true;
                        pf_layer[offset].wide_tmap.all_tiles_dirty = true;
                    }
                    break;
                case 3:
                    m92_raster_irq_position = pf_master_control[3] - 128;
                    break;
            }
        }
        public static void m92_master_control_w(int offset, ushort data)
        {
            ushort old = pf_master_control[offset];
            //COMBINE_DATA(&pf_master_control[offset]);
            pf_master_control[offset] = data;
            switch (offset)
            {
                case 0:
                case 1:
                case 2:
                    pf_layer[offset].vram_base = (ushort)((pf_master_control[offset] & 3) * 0x2000);
                    if ((pf_master_control[offset] & 0x04) != 0)
                    {
                        pf_layer[offset].tmap.enable = false;
                        pf_layer[offset].wide_tmap.enable = ((~pf_master_control[offset] >> 4) & 1) != 0 ? true : false;
                    }
                    else
                    {
                        pf_layer[offset].tmap.enable = ((~pf_master_control[offset] >> 4) & 1) != 0 ? true : false;
                        pf_layer[offset].wide_tmap.enable = false;
                    }
                    if (((old ^ pf_master_control[offset]) & 0x07) != 0)
                    {
                        pf_layer[offset].tmap.all_tiles_dirty = true;
                        pf_layer[offset].wide_tmap.all_tiles_dirty = true;
                    }
                    break;
                case 3:
                    m92_raster_irq_position = pf_master_control[3] - 128;
                    break;
            }
        }
        public static void video_start_m92()
        {
            int i;
            int laynum;
            uuB800 = new ushort[0x200 * 0x200];
            for (i = 0; i < 0x40000; i++)
            {
                uuB800[i] = 0x800;
            }
            for (laynum = 0; laynum < 3; laynum++)
            {
                pf_layer[laynum].tmap.tilemap_set_scrolldx(2 * laynum, -2 * laynum + 8);
                pf_layer[laynum].tmap.tilemap_set_scrolldy(-128, -128);
                pf_layer[laynum].wide_tmap.tilemap_set_scrolldx(2 * laynum - 256, -2 * laynum + 8 - 256);
                pf_layer[laynum].wide_tmap.tilemap_set_scrolldy(-128, -128);
            }
        }
        public static void draw_sprites(RECT cliprect)
        {
            // 预计算不变的值
            bool flip_screen = Generic.flip_screen_get() != 0;
            int sprite_count = m92_sprite_list;

            // 预先计算常用值避免重复计算
            const int SCREEN_WIDTH = 512;
            const int SCREEN_HEIGHT = 384;
            const int X_OFFSET = 16;
            const int Y_BASE = SCREEN_HEIGHT - X_OFFSET;

            for (int k = 0; k < 8; k++)
            {
                int offs = 0;

                // 使用while循环避免for循环的额外开销
                while (offs < sprite_count)
                {
                    // 一次性读取所有需要的数据到局部变量[7](@ref)
                    ushort data0 = Generic.buffered_spriteram16[offs];
                    ushort data1 = Generic.buffered_spriteram16[offs + 1];
                    ushort data2 = Generic.buffered_spriteram16[offs + 2];
                    ushort data3 = Generic.buffered_spriteram16[offs + 3];

                    // 提前进行优先级检查，避免不必要的计算[8](@ref)
                    int pri_sprite = (data0 & 0xe000) >> 13;
                    if (pri_sprite != k)
                    {
                        offs += 4;
                        continue;
                    }

                    // 提取精灵属性（使用局部变量避免重复内存访问）
                    int y = data0 & 0x1ff;
                    int x = data3 & 0x1ff;
                    int sprite = data1;
                    int colour = data2 & 0x007f;
                    int fx = (data2 >> 8) & 1;
                    int fy = (data2 >> 9) & 1;
                    int y_multi = 1 << ((data0 >> 9) & 3);
                    int x_multi = 1 << ((data0 >> 11) & 3);
                    bool high_priority = (data2 & 0x0080) != 0;
                    int pri_back = high_priority ? 0 : 2;

                    // 计算基础坐标变换
                    x = x - X_OFFSET;
                    y = Y_BASE - y;

                    // 处理水平翻转的偏移
                    if (fx != 0)
                    {
                        x += 16 * (x_multi - 1);
                    }

                    // 预先计算绘制参数
                    uint draw_flags = (uint)(pri_back | (1 << 31));
                    int x_step = fx != 0 ? -16 : 16;

                    // 内层循环优化：减少重复计算
                    for (int j = 0; j < x_multi; j++)
                    {
                        int s_ptr = 8 * j;
                        if (fy == 0)
                        {
                            s_ptr += y_multi - 1;
                        }

                        int current_x = x & 0x1ff;

                        for (int i = 0; i < y_multi; i++)
                        {
                            if (flip_screen)
                            {
                                // 翻转屏幕的绘制逻辑（已注释，保持原样）
                            }
                            else
                            {
                                //// 直接调用绘制函数，避免中间计算[3](@ref)
                                //Drawgfx.common_drawgfx_m92(gfx21rom, sprite + s_ptr, colour, fx, fy,
                                //                          current_x, y - i * 16, cliprect, draw_flags);
                                //Drawgfx.common_drawgfx_m92(gfx21rom, sprite + s_ptr, colour, fx, fy,
                                //                          current_x - SCREEN_WIDTH, y - i * 16, cliprect, draw_flags);

                                //手动内联
                                //--start
                                //参数common_drawgfx_m92(byte* bb1, int code, int color, int flipx, int flipy, int sx, int sy, RECT clip, uint primask)
                                // 使用常量折叠和预计算[5](@ref)

                                int code = sprite + s_ptr;
                                int color = colour;
                                int colorbase = color << 4; // 用移位代替乘法 0x10 * color
                                int flipx = fx;
                                int flipy = fy;
                                RECT clip = cliprect;
                                uint primask = draw_flags;

                                const int TEMP1 = 0x10 - 1;  // 15
                                const int TEMP3 = 0x100 - 1; // 255
                                const int TEMP4 = 0x200 - 1; // 511
                                //第1次调用
                                {
                                    int sx_1st = current_x;
                                    int sy_1st = y - i * 16;


                                    int ox = sx_1st;
                                    int oy = sy_1st;
                                    int ex = sx_1st + TEMP1;

                                    // 边界检查优化：减少分支预测错误[5](@ref)
                                    sx_1st = sx_1st < 0 ? 0 : sx_1st;
                                    sx_1st = sx_1st < clip.min_x ? clip.min_x : sx_1st;
                                    ex = ex >= 0x200 ? TEMP4 : ex;
                                    ex = ex > clip.max_x ? clip.max_x : ex;

                                    if (sx_1st > ex)
                                    {
                                    }
                                    else
                                    {
                                        int ey = sy_1st + TEMP1;
                                        sy_1st = sy_1st < 0 ? 0 : sy_1st;
                                        sy_1st = sy_1st < clip.min_y ? clip.min_y : sy_1st;
                                        ey = ey >= 0x100 ? TEMP3 : ey;
                                        ey = ey > clip.max_y ? clip.max_y : ey;

                                        if (sy_1st > ey)
                                        {

                                        }
                                        else
                                        {
                                            // 使用局部变量避免重复计算[2,4](@ref)
                                            int ls = sx_1st - ox;
                                            int ts = sy_1st - oy;
                                            int dw = ex - sx_1st + 1;
                                            int dh = ey - sy_1st + 1;

                                            // 内联关键函数调用
                                            Drawgfx.blockmove_8toN_transpen_pri16_m92(gfx21rom, code, 0x10, 0x10, 0x10, ls, ts, flipx, flipy, dw, dh, colorbase, sy_1st, sx_1st, primask);
                                        }
                                    }
                                }

                                //第2次调用
                                {
                                    int sx_2nd = current_x - SCREEN_WIDTH;
                                    int sy_2nd = y - i * 16;

                                    int ox = sx_2nd;
                                    int oy = sy_2nd;
                                    int ex = sx_2nd + TEMP1;

                                    // 边界检查优化：减少分支预测错误[5](@ref)
                                    sx_2nd = sx_2nd < 0 ? 0 : sx_2nd;
                                    sx_2nd = sx_2nd < clip.min_x ? clip.min_x : sx_2nd;
                                    ex = ex >= 0x200 ? TEMP4 : ex;
                                    ex = ex > clip.max_x ? clip.max_x : ex;

                                    if (sx_2nd > ex) { }
                                    else
                                    {
                                        int ey = sy_2nd + TEMP1;
                                        sy_2nd = sy_2nd < 0 ? 0 : sy_2nd;
                                        sy_2nd = sy_2nd < clip.min_y ? clip.min_y : sy_2nd;
                                        ey = ey >= 0x100 ? TEMP3 : ey;
                                        ey = ey > clip.max_y ? clip.max_y : ey;

                                        if (sy_2nd > ey) { }
                                        else
                                        {
                                            // 使用局部变量避免重复计算[2,4](@ref)
                                            int ls = sx_2nd - ox;
                                            int ts = sy_2nd - oy;
                                            int dw = ex - sx_2nd + 1;
                                            int dh = ey - sy_2nd + 1;

                                            // 内联关键函数调用
                                            Drawgfx.blockmove_8toN_transpen_pri16_m92(gfx21rom, code, 0x10, 0x10, 0x10, ls, ts, flipx, flipy, dw, dh, colorbase, sy_2nd, sx_2nd, primask);
                                        }
                                    }

                                }
                                //--end
                            }

                            // 优化指针更新
                            s_ptr += fy != 0 ? 1 : -1;
                        }

                        current_x += x_step;
                    }

                    offs += 4 * x_multi;
                }
            }
        }
        //public static void draw_sprites(RECT cliprect)
        //{
        //    int offs, k;
        //    for (k = 0; k < 8; k++)
        //    {
        //        for (offs = 0; offs < m92_sprite_list;)
        //        {
        //            int x, y, sprite, colour, fx, fy, x_multi, y_multi, i, j, s_ptr, pri_back, pri_sprite;
        //            y = Generic.buffered_spriteram16[offs + 0] & 0x1ff;
        //            x = Generic.buffered_spriteram16[offs + 3] & 0x1ff;
        //            if ((Generic.buffered_spriteram16[offs + 2] & 0x0080) != 0)
        //            {
        //                pri_back = 0;
        //            }
        //            else
        //            {
        //                pri_back = 2;
        //            }
        //            sprite = Generic.buffered_spriteram16[offs + 1];
        //            colour = Generic.buffered_spriteram16[offs + 2] & 0x007f;
        //            pri_sprite = (Generic.buffered_spriteram16[offs + 0] & 0xe000) >> 13;
        //            fx = (Generic.buffered_spriteram16[offs + 2] >> 8) & 1;
        //            fy = (Generic.buffered_spriteram16[offs + 2] >> 9) & 1;
        //            y_multi = (Generic.buffered_spriteram16[offs + 0] >> 9) & 3;
        //            x_multi = (Generic.buffered_spriteram16[offs + 0] >> 11) & 3;
        //            y_multi = 1 << y_multi;
        //            x_multi = 1 << x_multi;
        //            offs += 4 * x_multi;
        //            if (pri_sprite != k)
        //            {
        //                continue;
        //            }
        //            x = x - 16;
        //            y = 384 - 16 - y;
        //            if (fx != 0)
        //            {
        //                x += 16 * (x_multi - 1);
        //            }
        //            for (j = 0; j < x_multi; j++)
        //            {
        //                s_ptr = 8 * j;
        //                if (fy == 0)
        //                {
        //                    s_ptr += y_multi - 1;
        //                }
        //                x &= 0x1ff;
        //                for (i = 0; i < y_multi; i++)
        //                {
        //                    if (Generic.flip_screen_get() != 0)
        //                    {
        //                        int i1 = 1;
        //                        /*pdrawgfx(bitmap,machine->gfx[1],
        //                                sprite + s_ptr,
        //                                colour,
        //                                !fx,!fy,
        //                                464-x,240-(y-i*16),
        //                                cliprect,TRANSPARENCY_PEN,0,pri_back);
        //
        //                        pdrawgfx(bitmap,machine->gfx[1],
        //                                sprite + s_ptr,
        //                                colour,
        //                                !fx,!fy,
        //                                464-x+512,240-(y-i*16),
        //                                cliprect,TRANSPARENCY_PEN,0,pri_back);*/
        //
        //                     } 
        //                    else
        //                    {
        //                        /*pdrawgfx(bitmap,machine->gfx[1],
        //                                sprite + s_ptr,
        //                                colour,
        //                                fx,fy,
        //                                x,y-i*16,
        //                                cliprect,TRANSPARENCY_PEN,0,pri_back);
        //
        //                        pdrawgfx(bitmap,machine->gfx[1],
        //                                sprite + s_ptr,
        //                                colour,
        //                                fx,fy,
        //                                x-512,y-i*16,
        //                                cliprect,TRANSPARENCY_PEN,0,pri_back);*/
        //                        Drawgfx.common_drawgfx_m92(gfx21rom, sprite + s_ptr, colour, fx, fy, x, y - i * 16, cliprect, (uint)(pri_back | (1 << 31)));
        //                        Drawgfx.common_drawgfx_m92(gfx21rom, sprite + s_ptr, colour, fx, fy, x - 512, y - i * 16, cliprect, (uint)(pri_back | (1 << 31)));
        //                    }
        //                    if (fy != 0)
        //                    {
        //                        s_ptr++;
        //                    }
        //                    else
        //                    {
        //                        s_ptr--;
        //                    }
        //                }
        //                if (fx != 0)
        //                {
        //                    x -= 16;
        //                }
        //                else
        //                {
        //                    x += 16;
        //                }
        //            }
        //        }
        //    }
        //}
        public static void m92_update_scroll_positions()
        {
            int laynum;
            int i;
            for (laynum = 0; laynum < 3; laynum++)
            {
                if ((pf_master_control[laynum] & 0x40) != 0)
                {
                    int scrolldata_offset = (0xf400 + 0x400 * laynum) / 2;
                    pf_layer[laynum].tmap.tilemap_set_scroll_rows(512);
                    pf_layer[laynum].wide_tmap.tilemap_set_scroll_rows(512);
                    for (i = 0; i < 512; i++)
                    {
                        pf_layer[laynum].tmap.tilemap_set_scrollx(i, m92_vram_data[scrolldata_offset + i]);
                        pf_layer[laynum].wide_tmap.tilemap_set_scrollx(i, m92_vram_data[scrolldata_offset + i]);
                    }
                }
                else
                {
                    pf_layer[laynum].tmap.tilemap_set_scroll_rows(1);
                    pf_layer[laynum].wide_tmap.tilemap_set_scroll_rows(1);
                    pf_layer[laynum].tmap.tilemap_set_scrollx(0, pf_layer[laynum].control[2]);
                    pf_layer[laynum].wide_tmap.tilemap_set_scrollx(0, pf_layer[laynum].control[2]);
                }
                pf_layer[laynum].tmap.tilemap_set_scrolly(0, pf_layer[laynum].control[0]);
                pf_layer[laynum].wide_tmap.tilemap_set_scrolly(0, pf_layer[laynum].control[0]);
            }
        }
        public static void m92_screenrefresh(RECT cliprect)
        {
            Array.Copy(Tilemap.bb00, 0, Tilemap.priority_bitmap, 0x200 * cliprect.min_y, 0x200 * (cliprect.max_y - cliprect.min_y + 1));
            if (((~pf_master_control[2] >> 4) & 1) != 0)
            {
                pf_layer[2].wide_tmap.tilemap_draw_primask(cliprect, 0x20, 0);
                pf_layer[2].tmap.tilemap_draw_primask(cliprect, 0x20, 0);
                pf_layer[2].wide_tmap.tilemap_draw_primask(cliprect, 0x10, 1);
                pf_layer[2].tmap.tilemap_draw_primask(cliprect, 0x10, 1);
            }
            else
            {
                Array.Copy(uuB800, 0, Video.bitmapbase[Video.curbitmap], 0x200 * cliprect.min_y, 0x200 * (cliprect.max_y - cliprect.min_y + 1));
            }
            pf_layer[1].wide_tmap.tilemap_draw_primask(cliprect, 0x20, 0);
            pf_layer[1].tmap.tilemap_draw_primask(cliprect, 0x20, 0);
            pf_layer[1].wide_tmap.tilemap_draw_primask(cliprect, 0x10, 1);
            pf_layer[1].tmap.tilemap_draw_primask(cliprect, 0x10, 1);
            pf_layer[0].wide_tmap.tilemap_draw_primask(cliprect, 0x20, 0);
            pf_layer[0].tmap.tilemap_draw_primask(cliprect, 0x20, 0);
            pf_layer[0].wide_tmap.tilemap_draw_primask(cliprect, 0x10, 1);
            pf_layer[0].tmap.tilemap_draw_primask(cliprect, 0x10, 1);
            draw_sprites(cliprect);
        }
        public static void video_update_m92()
        {
            m92_update_scroll_positions();
            m92_screenrefresh(Video.new_clip);
            if ((dsw & 0x100) != 0)
            {
                Generic.flip_screen_set(0);
            }
            else
            {
                Generic.flip_screen_set(1);
            }
        }
        public static void video_eof_m92()
        {

        }
    }
}
