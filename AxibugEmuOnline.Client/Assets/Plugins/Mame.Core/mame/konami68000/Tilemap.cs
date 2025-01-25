using System;

namespace MAME.Core
{
    public partial class Konami68000
    {
        public static Tmap[] K052109_tilemap, K053251_tilemaps;
        public static void tilemap_init()
        {

        }
    }
    public unsafe partial class Tmap
    {
        public void tilemap_draw_instanceKonami68000(RECT cliprect, int xpos, int ypos)
        {
            int mincol, maxcol;
            int x1, y1, x2, y2;
            int y, nexty;
            int offsety1, offsety2;
            int i;
            x1 = Math.Max(xpos, cliprect.min_x);
            x2 = Math.Min(xpos + width, cliprect.max_x + 1);
            y1 = Math.Max(ypos, cliprect.min_y);
            y2 = Math.Min(ypos + height, cliprect.max_y + 1);
            if (x1 >= x2 || y1 >= y2)
                return;
            x1 -= xpos;
            y1 -= ypos;
            x2 -= xpos;
            y2 -= ypos;
            offsety1 = y1;
            mincol = x1 / tilewidth;
            maxcol = (x2 + tilewidth - 1) / tilewidth;
            y = y1;
            nexty = tileheight * (y1 / tileheight) + tileheight;
            nexty = Math.Min(nexty, y2);
            for (; ; )
            {
                int row = y / tileheight;
                trans_t prev_trans = trans_t.WHOLLY_TRANSPARENT;
                trans_t cur_trans;
                int x_start = x1;
                int column;
                for (column = mincol; column <= maxcol; column++)
                {
                    int x_end;
                    if (column == maxcol)
                    {
                        cur_trans = trans_t.WHOLLY_TRANSPARENT;
                    }
                    else
                    {
                        if (tileflags[row, column] == Tilemap.TILE_FLAG_DIRTY)
                        {
                            tile_update3(column, row);
                        }
                        if ((tileflags[row, column] & mask) != 0)
                        {
                            cur_trans = trans_t.MASKED;
                        }
                        else
                        {
                            cur_trans = ((flagsmap[offsety1, column * tilewidth] & mask) == value) ? trans_t.WHOLLY_OPAQUE : trans_t.WHOLLY_TRANSPARENT;
                        }
                    }
                    if (cur_trans == prev_trans)
                    {
                        continue;
                    }
                    x_end = column * tilewidth;
                    x_end = Math.Max(x_end, x1);
                    x_end = Math.Min(x_end, x2);
                    if (prev_trans != trans_t.WHOLLY_TRANSPARENT)
                    {
                        int cury;
                        offsety2 = offsety1;
                        if (prev_trans == trans_t.WHOLLY_OPAQUE)
                        {
                            for (cury = y; cury < nexty; cury++)
                            {
                                Array.Copy(pixmap, offsety2 * width + x_start, Video.bitmapbase[Video.curbitmap], (offsety2 + ypos) * 0x200 + xpos + x_start, x_end - x_start);
                                if (priority != 0)
                                {
                                    for (i = xpos + x_start; i < xpos + x_end; i++)
                                    {
                                        Tilemap.priority_bitmap[offsety2 + ypos, i] = (byte)(Tilemap.priority_bitmap[offsety2 + ypos, i] | priority);
                                    }
                                }
                                offsety2++;
                            }
                        }
                        else if (prev_trans == trans_t.MASKED)
                        {
                            for (cury = y; cury < nexty; cury++)
                            {
                                for (i = xpos + x_start; i < xpos + x_end; i++)
                                {
                                    if ((flagsmap[offsety2, i - xpos] & mask) == value)
                                    {
                                        Video.bitmapbase_Ptrs[Video.curbitmap][(offsety2 + ypos) * 0x200 + i] = pixmap[offsety2 * width + i - xpos];
                                        Tilemap.priority_bitmap[offsety2 + ypos, i] = (byte)(Tilemap.priority_bitmap[offsety2 + ypos, i] | priority);
                                    }
                                }
                                offsety2++;
                            }
                        }
                    }
                    x_start = x_end;
                    prev_trans = cur_trans;
                }
                if (nexty == y2)
                    break;
                offsety1 += (nexty - y);
                y = nexty;
                nexty += tileheight;
                nexty = Math.Min(nexty, y2);
            }
        }
        public void tile_updateKonami68000_0(int col, int row)
        {
            int x0 = tilewidth * col;
            int y0 = tileheight * row;
            int tile_index;
            int code, color;
            int pen_data_offset, palette_base, group;
            int flipy = 0;
            tile_index = row * cols + col;
            code = Konami68000.K052109_ram[Konami68000.K052109_videoram_F_offset + tile_index] + 256 * Konami68000.K052109_ram[Konami68000.K052109_videoram2_F_offset + tile_index];
            color = Konami68000.K052109_ram[Konami68000.K052109_colorram_F_offset + tile_index];
            int flags = 0;
            int priority = 0;
            int bank = Konami68000.K052109_charrombank[(color & 0x0c) >> 2];
            if (Konami68000.has_extra_video_ram != 0)
            {
                bank = (color & 0x0c) >> 2;
            }
            color = (color & 0xf3) | ((bank & 0x03) << 2);
            bank >>= 2;
            flipy = color & 0x02;
            int code2, color2, flags2;
            Konami68000.K052109_callback(0, bank, code, color, flags, priority, out code2, out color2, out flags2);
            code = code2;
            color = color2;
            flags = flags2;
            if ((Konami68000.K052109_tileflip_enable & 1) == 0)
            {
                flags &= ~0x01;
            }
            if (flipy != 0 && (Konami68000.K052109_tileflip_enable & 2) != 0)
            {
                flags |= 0x02;
            }
            code = code % Konami68000.K052109_tilemap[0].total_elements;
            pen_data_offset = code * 0x40;
            palette_base = 0x10 * color;
            group = 0;
            flags = flags ^ (attributes & 0x03);
            tileflags[row, col] = tile_drawKonami68000(Konami68000.gfx12rom, pen_data_offset, x0, y0, palette_base, group, flags);
        }
        public void tile_updateKonami68000_1(int col, int row)
        {
            int x0 = tilewidth * col;
            int y0 = tileheight * row;
            int tile_index;
            int code, color;
            int pen_data_offset, palette_base, group;
            int flipy = 0;
            tile_index = row * cols + col;
            code = Konami68000.K052109_ram[Konami68000.K052109_videoram_A_offset + tile_index] + 256 * Konami68000.K052109_ram[Konami68000.K052109_videoram2_A_offset + tile_index];
            color = Konami68000.K052109_ram[Konami68000.K052109_colorram_A_offset + tile_index];
            int flags = 0;
            int priority = 0;
            int bank = Konami68000.K052109_charrombank[(color & 0x0c) >> 2];
            if (Konami68000.has_extra_video_ram != 0)
            {
                bank = (color & 0x0c) >> 2;
            }
            color = (color & 0xf3) | ((bank & 0x03) << 2);
            bank >>= 2;
            flipy = color & 0x02;
            int code2, color2, flags2;
            Konami68000.K052109_callback(1, bank, code, color, flags, priority, out code2, out color2, out flags2);
            code = code2;
            color = color2;
            flags = flags2;
            if ((Konami68000.K052109_tileflip_enable & 1) == 0)
            {
                flags &= ~0x01;
            }
            if (flipy != 0 && (Konami68000.K052109_tileflip_enable & 2) != 0)
            {
                flags |= 0x02;
            }
            code = code % Konami68000.K052109_tilemap[1].total_elements;
            pen_data_offset = code * 0x40;
            palette_base = 0x10 * color;
            group = 0;
            flags = flags ^ (attributes & 0x03);
            tileflags[row, col] = tile_drawKonami68000(Konami68000.gfx12rom, pen_data_offset, x0, y0, palette_base, group, flags);
        }
        public void tile_updateKonami68000_2(int col, int row)
        {
            int x0 = tilewidth * col;
            int y0 = tileheight * row;
            int tile_index;
            int code, color;
            int pen_data_offset, palette_base, group;
            int flipy = 0;
            tile_index = row * cols + col;
            code = Konami68000.K052109_ram[Konami68000.K052109_videoram_B_offset + tile_index] + 256 * Konami68000.K052109_ram[Konami68000.K052109_videoram2_B_offset + tile_index];
            color = Konami68000.K052109_ram[Konami68000.K052109_colorram_B_offset + tile_index];
            int flags = 0;
            int priority = 0;
            int bank = Konami68000.K052109_charrombank[(color & 0x0c) >> 2];
            if (Konami68000.has_extra_video_ram != 0)
            {
                bank = (color & 0x0c) >> 2;
            }
            color = (color & 0xf3) | ((bank & 0x03) << 2);
            bank >>= 2;
            flipy = color & 0x02;
            int code2, color2, flags2;
            Konami68000.K052109_callback(2, bank, code, color, flags, priority, out code2, out color2, out flags2);
            code = code2;
            color = color2;
            flags = flags2;
            if ((Konami68000.K052109_tileflip_enable & 1) == 0)
            {
                flags &= ~0x01;
            }
            if (flipy != 0 && (Konami68000.K052109_tileflip_enable & 2) != 0)
            {
                flags |= 0x02;
            }
            code = code % Konami68000.K052109_tilemap[2].total_elements;
            pen_data_offset = code * 0x40;
            palette_base = 0x10 * color;
            group = 0;
            flags = flags ^ (attributes & 0x03);
            tileflags[row, col] = tile_drawKonami68000(Konami68000.gfx12rom, pen_data_offset, x0, y0, palette_base, group, flags);
        }
        public byte tile_drawKonami68000(byte* bb1, int pen_data_offset, int x0, int y0, int palette_base, int group, int flags)
        {
            byte andmask = 0xff, ormask = 0;
            int dx0 = 1, dy0 = 1;
            int tx, ty;
            byte pen, map;
            int offset1 = 0;
            int offsety1;
            int xoffs;
            AxiArray.Copy(bb1, pen_data_offset, pen_data, 0, 0x40);
            if ((flags & Tilemap.TILE_FLIPY) != 0)
            {
                y0 += tileheight - 1;
                dy0 = -1;
            }
            if ((flags & Tilemap.TILE_FLIPX) != 0)
            {
                x0 += tilewidth - 1;
                dx0 = -1;
            }
            for (ty = 0; ty < tileheight; ty++)
            {
                xoffs = 0;
                offsety1 = y0;
                y0 += dy0;
                for (tx = 0; tx < tilewidth; tx++)
                {
                    pen = pen_data[offset1];
                    map = pen_to_flags[group, pen];
                    offset1++;
                    pixmap[(offsety1 % width) * width + x0 + xoffs] = (ushort)(palette_base + pen);
                    flagsmap[offsety1 % width, x0 + xoffs] = map;
                    andmask &= map;
                    ormask |= map;
                    xoffs += dx0;
                }
            }
            return (byte)(andmask ^ ormask);
        }
    }
}
