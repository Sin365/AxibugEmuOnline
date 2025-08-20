using System;

namespace MAME.Core
{
    public partial class M72
    {
        public static Tmap bg_tilemap, fg_tilemap, bg_tilemap_large;
        public static void tilemap_init()
        {
            int i;
            bg_tilemap = new Tmap();
            bg_tilemap.rows = 64;
            bg_tilemap.cols = 64;
            bg_tilemap.tilewidth = 8;
            bg_tilemap.tileheight = 8;
            bg_tilemap.width = 0x200;
            bg_tilemap.height = 0x200;
            bg_tilemap.enable = true;
            bg_tilemap.all_tiles_dirty = true;
            bg_tilemap.pixmap = new ushort[0x200 * 0x200];
            bg_tilemap.flagsmap = new byte[0x200, 0x200];
            bg_tilemap.tileflags = new byte[0x40, 0x40];
            bg_tilemap.total_elements = M72.gfx21romLength / 0x40;
            bg_tilemap.pen_data_set = new byte[0x40];
            bg_tilemap.pen_to_flags = new byte[3, 16];
            for (i = 0; i < 16; i++)
            {
                bg_tilemap.pen_to_flags[0, i] = 0x20;
            }
            for (i = 0; i < 8; i++)
            {
                bg_tilemap.pen_to_flags[1, i] = 0x20;
            }
            for (i = 8; i < 16; i++)
            {
                bg_tilemap.pen_to_flags[1, i] = 0x10;
            }
            bg_tilemap.pen_to_flags[2, 0] = 0x20;
            for (i = 1; i < 16; i++)
            {
                bg_tilemap.pen_to_flags[2, i] = 0x10;
            }
            bg_tilemap.scrollrows = 1;
            bg_tilemap.scrollcols = 1;
            bg_tilemap.rowscroll = new int[bg_tilemap.scrollrows];
            bg_tilemap.colscroll = new int[bg_tilemap.scrollcols];
            bg_tilemap.tilemap_draw_instance3 = bg_tilemap.tilemap_draw_instanceM72;
            fg_tilemap = new Tmap();
            fg_tilemap.cols = 64;
            fg_tilemap.rows = 64;
            fg_tilemap.tilewidth = 8;
            fg_tilemap.tileheight = 8;
            fg_tilemap.width = 0x200;
            fg_tilemap.height = 0x200;
            fg_tilemap.enable = true;
            fg_tilemap.all_tiles_dirty = true;
            fg_tilemap.pixmap = new ushort[0x200 * 0x200];
            fg_tilemap.flagsmap = new byte[0x200, 0x200];
            fg_tilemap.tileflags = new byte[0x40, 0x40];
            fg_tilemap.total_elements = M72.gfx21romLength / 0x40;
            fg_tilemap.pen_data_set = new byte[0x400];
            fg_tilemap.pen_to_flags = new byte[3, 32];
            fg_tilemap.pen_to_flags[0, 0] = 0;
            for (i = 1; i < 16; i++)
            {
                fg_tilemap.pen_to_flags[0, i] = 0x20;
            }
            fg_tilemap.pen_to_flags[1, 0] = 0;
            for (i = 1; i < 8; i++)
            {
                fg_tilemap.pen_to_flags[1, i] = 0x20;
            }
            for (i = 8; i < 16; i++)
            {
                fg_tilemap.pen_to_flags[1, i] = 0x10;
            }
            fg_tilemap.pen_to_flags[2, 0] = 0;
            for (i = 1; i < 16; i++)
            {
                fg_tilemap.pen_to_flags[2, i] = 0x10;
            }
            fg_tilemap.scrollrows = 1;
            fg_tilemap.scrollcols = 1;
            fg_tilemap.rowscroll = new int[fg_tilemap.scrollrows];
            fg_tilemap.colscroll = new int[fg_tilemap.scrollcols];
            fg_tilemap.tilemap_draw_instance3 = fg_tilemap.tilemap_draw_instanceM72;
            switch (Machine.sName)
            {
                case "airduel":
                case "airduelm72":
                    bg_tilemap.tile_update3 = bg_tilemap.tile_updateM72_bg_m72;
                    fg_tilemap.tile_update3 = fg_tilemap.tile_updateM72_fg_m72;
                    break;
                case "ltswords":
                case "kengo":
                case "kengoa":
                    bg_tilemap.tile_update3 = bg_tilemap.tile_updateM72_bg_rtype2;
                    fg_tilemap.tile_update3 = fg_tilemap.tile_updateM72_fg_rtype2;
                    break;
            }
        }
        public static void tilemap_init_m82()
        {
            int i;
            bg_tilemap = new Tmap();
            bg_tilemap.rows = 64;
            bg_tilemap.cols = 64;
            bg_tilemap.tilewidth = 8;
            bg_tilemap.tileheight = 8;
            bg_tilemap.width = 0x200;
            bg_tilemap.height = 0x200;
            bg_tilemap.enable = true;
            bg_tilemap.all_tiles_dirty = true;
            bg_tilemap.pixmap = new ushort[0x200 * 0x200];
            bg_tilemap.flagsmap = new byte[0x200, 0x200];
            bg_tilemap.tileflags = new byte[0x40, 0x40];
            bg_tilemap.total_elements = M72.gfx21romLength / 0x40;
            bg_tilemap.pen_data_set = new byte[0x40];
            bg_tilemap.pen_to_flags = new byte[3, 16];
            for (i = 0; i < 16; i++)
            {
                bg_tilemap.pen_to_flags[0, i] = 0x20;
            }
            for (i = 0; i < 8; i++)
            {
                bg_tilemap.pen_to_flags[1, i] = 0x20;
            }
            for (i = 8; i < 16; i++)
            {
                bg_tilemap.pen_to_flags[1, i] = 0x10;
            }
            bg_tilemap.pen_to_flags[2, 0] = 0x20;
            for (i = 1; i < 16; i++)
            {
                bg_tilemap.pen_to_flags[2, i] = 0x10;
            }
            bg_tilemap.scrollrows = 1;
            bg_tilemap.scrollcols = 1;
            bg_tilemap.rowscroll = new int[bg_tilemap.scrollrows];
            bg_tilemap.colscroll = new int[bg_tilemap.scrollcols];
            bg_tilemap.tilemap_draw_instance3 = bg_tilemap.tilemap_draw_instanceM72;
            fg_tilemap = new Tmap();
            fg_tilemap.cols = 64;
            fg_tilemap.rows = 64;
            fg_tilemap.tilewidth = 8;
            fg_tilemap.tileheight = 8;
            fg_tilemap.width = 0x200;
            fg_tilemap.height = 0x200;
            fg_tilemap.enable = true;
            fg_tilemap.all_tiles_dirty = true;
            fg_tilemap.pixmap = new ushort[0x200 * 0x200];
            fg_tilemap.flagsmap = new byte[0x200, 0x200];
            fg_tilemap.tileflags = new byte[0x40, 0x40];
            fg_tilemap.total_elements = M72.gfx21romLength / 0x40;
            fg_tilemap.pen_data_set = new byte[0x400];
            fg_tilemap.pen_to_flags = new byte[3, 32];
            fg_tilemap.pen_to_flags[0, 0] = 0;
            for (i = 1; i < 16; i++)
            {
                fg_tilemap.pen_to_flags[0, i] = 0x20;
            }
            fg_tilemap.pen_to_flags[1, 0] = 0;
            for (i = 1; i < 8; i++)
            {
                fg_tilemap.pen_to_flags[1, i] = 0x20;
            }
            for (i = 8; i < 16; i++)
            {
                fg_tilemap.pen_to_flags[1, i] = 0x10;
            }
            fg_tilemap.pen_to_flags[2, 0] = 0;
            for (i = 1; i < 16; i++)
            {
                fg_tilemap.pen_to_flags[2, i] = 0x10;
            }
            fg_tilemap.scrollrows = 1;
            fg_tilemap.scrollcols = 1;
            fg_tilemap.rowscroll = new int[fg_tilemap.scrollrows];
            fg_tilemap.colscroll = new int[fg_tilemap.scrollcols];
            fg_tilemap.tilemap_draw_instance3 = fg_tilemap.tilemap_draw_instanceM72;
            bg_tilemap_large = new Tmap();
            bg_tilemap_large.rows = 0x40;
            bg_tilemap_large.cols = 0x80;
            bg_tilemap_large.tilewidth = 8;
            bg_tilemap_large.tileheight = 8;
            bg_tilemap_large.width = 0x400;
            bg_tilemap_large.height = 0x200;
            bg_tilemap_large.enable = true;
            bg_tilemap_large.all_tiles_dirty = true;
            bg_tilemap_large.pixmap = new ushort[0x400 * 0x200];
            bg_tilemap_large.flagsmap = new byte[0x200, 0x400];
            bg_tilemap_large.tileflags = new byte[0x40, 0x80];
            bg_tilemap_large.total_elements = M72.gfx21romLength / 0x40;
            bg_tilemap_large.pen_data_set = new byte[0x40];
            bg_tilemap_large.pen_to_flags = new byte[3, 16];
            for (i = 0; i < 16; i++)
            {
                bg_tilemap_large.pen_to_flags[0, i] = 0x20;
            }
            for (i = 0; i < 8; i++)
            {
                bg_tilemap_large.pen_to_flags[1, i] = 0x20;
            }
            for (i = 8; i < 16; i++)
            {
                bg_tilemap_large.pen_to_flags[1, i] = 0x10;
            }
            bg_tilemap_large.pen_to_flags[2, 0] = 0x20;
            for (i = 1; i < 16; i++)
            {
                bg_tilemap_large.pen_to_flags[2, i] = 0x10;
            }
            bg_tilemap_large.scrollrows = 1;
            bg_tilemap_large.scrollcols = 1;
            bg_tilemap_large.rowscroll = new int[bg_tilemap.scrollrows];
            bg_tilemap_large.colscroll = new int[bg_tilemap.scrollcols];
            bg_tilemap_large.tilemap_draw_instance3 = bg_tilemap_large.tilemap_draw_instanceM72;
            bg_tilemap.tile_update3 = bg_tilemap.tile_updateM72_bg_m72;
            fg_tilemap.tile_update3 = fg_tilemap.tile_updateM72_fg_m72;
            bg_tilemap_large.tile_update3 = bg_tilemap.tile_updateM72_bg_m72;
        }
    }
    public unsafe partial class Tmap
    {
        public void tilemap_draw_instanceM72(RECT cliprect, int xpos, int ypos)
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
                        continue;
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
                                for (i = xpos + x_start; i < xpos + x_end; i++)
                                {
                                    Video.bitmapbase_Ptrs[Video.curbitmap][(offsety2 + ypos) * 0x200 + i] = (ushort)(pixmap[offsety2 * width + i - xpos] + palette_offset);
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
                                        Video.bitmapbase_Ptrs[Video.curbitmap][(offsety2 + ypos) * 0x200 + i] = (ushort)(pixmap[offsety2 * width + i - xpos] + palette_offset);
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
        public void tile_updateM72_bg_m72(int col, int row)
        {
            int x0 = tilewidth * col;
            int y0 = tileheight * row;
            int code, code1, attr, color, pri, tile_index;
            int pen_data_offset;
            tile_index = (0x40 * row + col) * 2;
            code = M72.m72_videoram2[tile_index * 2] + M72.m72_videoram2[tile_index * 2 + 1] * 0x100;
            color = M72.m72_videoram2[(tile_index + 1) * 2];
            attr = M72.m72_videoram2[(tile_index + 1) * 2 + 1];
            if ((attr & 0x01) != 0)
            {
                pri = 2;
            }
            else if ((color & 0x80) != 0)
            {
                pri = 1;
            }
            else
            {
                pri = 0;
            }
            code1 = (code + ((attr & 0x3f) << 8)) % M72.bg_tilemap.total_elements;
            pen_data_offset = code1 * 0x40;
            tileflags[row, col] = tile_drawM72(M72.gfx31rom, pen_data_offset, x0, y0, 0x100 + 0x10 * (color & 0x0f), pri, (((color & 0xc0) >> 6) & 3) ^ (attributes & 0x03));
        }
        public void tile_updateM72_bg_rtype2(int col, int row)
        {
            int x0 = tilewidth * col;
            int y0 = tileheight * row;
            int code, code1, attr, color, pri, tile_index;
            int pen_data_offset;
            tile_index = (0x40 * row + col) * 2;
            code = M72.m72_videoram2[tile_index * 2] + M72.m72_videoram2[tile_index * 2 + 1] * 0x100;
            color = M72.m72_videoram2[(tile_index + 1) * 2];
            attr = M72.m72_videoram2[(tile_index + 1) * 2 + 1];
            if ((attr & 0x01) != 0)
            {
                pri = 2;
            }
            else if ((color & 0x80) != 0)
            {
                pri = 1;
            }
            else
            {
                pri = 0;
            }
            code1 = code % M72.bg_tilemap.total_elements;
            pen_data_offset = code1 * 0x40;
            tileflags[row, col] = tile_drawM72(M72.gfx21rom, pen_data_offset, x0, y0, 0x100 + 0x10 * (color & 0x0f), pri, (((color & 0x60) >> 5) & 3) ^ (attributes & 0x03));
        }
        public byte tile_drawM72(byte* bb1, int pen_data_offset, int x0, int y0, int palette_base, int group, int flags)
        {
            int height = tileheight;
            int width = tilewidth;
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
                y0 += height - 1;
                dy0 = -1;
            }
            if ((flags & Tilemap.TILE_FLIPX) != 0)
            {
                x0 += width - 1;
                dx0 = -1;
            }
            for (ty = 0; ty < height; ty++)
            {
                xoffs = 0;
                offsety1 = y0;
                y0 += dy0;
                for (tx = 0; tx < width; tx++)
                {
                    pen = pen_data[offset1];
                    map = pen_to_flags[group, pen];
                    offset1++;
                    pixmap[(offsety1 % 0x200) * 0x200 + x0 + xoffs] = (ushort)(palette_base + pen);
                    flagsmap[offsety1 % 0x200, x0 + xoffs] = map;
                    andmask &= map;
                    ormask |= map;
                    xoffs += dx0;
                }
            }
            return (byte)(andmask ^ ormask);
        }
        public void tile_updateM72_fg_m72(int col, int row)
        {
            int x0 = tilewidth * col;
            int y0 = tileheight * row;
            int code, code1, attr, color, pri, tile_index;
            int pen_data_offset;
            tile_index = (0x40 * row + col) * 2;
            code = M72.m72_videoram1[tile_index * 2] + M72.m72_videoram1[tile_index * 2 + 1] * 0x100;
            color = M72.m72_videoram1[(tile_index + 1) * 2];
            attr = M72.m72_videoram1[(tile_index + 1) * 2 + 1];
            if ((attr & 0x01) != 0)
            {
                pri = 2;
            }
            else if ((color & 0x80) != 0)
            {
                pri = 1;
            }
            else
            {
                pri = 0;
            }
            code1 = code % M72.fg_tilemap.total_elements;
            pen_data_offset = code1 * 0x40;
            tileflags[row, col] = tile_drawM72(M72.gfx21rom, pen_data_offset, x0, y0, 0x100 + 0x10 * (color & 0x0f), pri, (((color & 0x60) >> 5) & 3) ^ (attributes & 0x03));
        }
        public void tile_updateM72_fg_rtype2(int col, int row)
        {
            int x0 = tilewidth * col;
            int y0 = tileheight * row;
            int y0offset;
            int code, code1, attr, color, pri, tile_index;
            int pen_data_offset;
            tile_index = (0x40 * row + col) * 2;
            code = M72.m72_videoram1[tile_index * 2] + M72.m72_videoram1[tile_index * 2 + 1] * 0x100;
            color = M72.m72_videoram1[(tile_index + 1) * 2];
            attr = M72.m72_videoram1[(tile_index + 1) * 2 + 1];
            if ((attr & 0x01) != 0)
            {
                pri = 2;
            }
            else if ((color & 0x80) != 0)
            {
                pri = 1;
            }
            else
            {
                pri = 0;
            }
            if (pri == 0)
            {
                y0offset = 0x90;
            }
            else
            {
                y0offset = 0;
            }
            code1 = code % M72.fg_tilemap.total_elements;
            pen_data_offset = code1 * 0x40;
            tileflags[row, col] = tile_drawM72(M72.gfx21rom, pen_data_offset, x0, y0, 0x100 + 0x10 * (color & 0x0f), pri, (((color & 0x60) >> 5) & 3) ^ (attributes & 0x03));
        }
    }
}
