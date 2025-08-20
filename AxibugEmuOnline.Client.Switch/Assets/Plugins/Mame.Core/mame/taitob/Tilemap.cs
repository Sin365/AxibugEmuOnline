using System;

namespace MAME.Core
{
    public unsafe partial class Taitob
    {
        public static Tmap bg_tilemap, fg_tilemap, tx_tilemap;
        public static void tilemap_init()
        {
            int i;
            framebuffer = new ushort[2][];
            for (i = 0; i < 2; i++)
            {
                framebuffer[i] = new ushort[0x200 * 0x100];
            }
            bg_tilemap = new Tmap();
            bg_tilemap.cols = 64;
            bg_tilemap.rows = 64;
            bg_tilemap.tilewidth = 16;
            bg_tilemap.tileheight = 16;
            bg_tilemap.width = 0x400;
            bg_tilemap.height = 0x400;
            bg_tilemap.enable = true;
            bg_tilemap.all_tiles_dirty = true;
            bg_tilemap.total_elements = gfx1romLength / 0x100;
            bg_tilemap.pixmap = new ushort[0x400 * 0x400];
            bg_tilemap.flagsmap = new byte[0x400, 0x400];
            bg_tilemap.tileflags = new byte[64, 64];
            bg_tilemap.pen_data_set = new byte[0x100];
            bg_tilemap.pen_to_flags = new byte[1, 16];
            for (i = 0; i < 16; i++)
            {
                bg_tilemap.pen_to_flags[0, i] = 0x10;
            }
            bg_tilemap.scrollrows = 1;
            bg_tilemap.scrollcols = 1;
            bg_tilemap.rowscroll = new int[bg_tilemap.scrollrows];
            bg_tilemap.colscroll = new int[bg_tilemap.scrollcols];
            bg_tilemap.tilemap_draw_instance3 = bg_tilemap.tilemap_draw_instanceTaitob;
            bg_tilemap.tile_update3 = bg_tilemap.tile_updateTaitobbg;

            fg_tilemap = new Tmap();
            fg_tilemap.cols = 64;
            fg_tilemap.rows = 64;
            fg_tilemap.tilewidth = 16;
            fg_tilemap.tileheight = 16;
            fg_tilemap.width = 0x400;
            fg_tilemap.height = 0x400;
            fg_tilemap.enable = true;
            fg_tilemap.all_tiles_dirty = true;
            fg_tilemap.total_elements = gfx1romLength / 0x100;
            fg_tilemap.pixmap = new ushort[0x400 * 0x400];
            fg_tilemap.flagsmap = new byte[0x400, 0x400];
            fg_tilemap.tileflags = new byte[64, 64];
            fg_tilemap.pen_data_set = new byte[0x100];
            fg_tilemap.pen_to_flags = new byte[1, 16];
            for (i = 1; i < 16; i++)
            {
                fg_tilemap.pen_to_flags[0, i] = 0x10;
            }
            fg_tilemap.pen_to_flags[0, 0] = 0;
            fg_tilemap.scrollrows = 1;
            fg_tilemap.scrollcols = 1;
            fg_tilemap.rowscroll = new int[fg_tilemap.scrollrows];
            fg_tilemap.colscroll = new int[fg_tilemap.scrollcols];
            fg_tilemap.tilemap_draw_instance3 = fg_tilemap.tilemap_draw_instanceTaitob;
            fg_tilemap.tile_update3 = fg_tilemap.tile_updateTaitobfg;


            tx_tilemap = new Tmap();
            tx_tilemap.cols = 64;
            tx_tilemap.rows = 32;
            tx_tilemap.tilewidth = 8;
            tx_tilemap.tileheight = 8;
            tx_tilemap.width = 0x200;
            tx_tilemap.height = 0x100;
            tx_tilemap.enable = true;
            tx_tilemap.all_tiles_dirty = true;
            tx_tilemap.total_elements = gfx1romLength / 0x40;
            tx_tilemap.pixmap = new ushort[0x100 * 0x200];
            tx_tilemap.flagsmap = new byte[0x100, 0x200];
            tx_tilemap.tileflags = new byte[32, 64];
            tx_tilemap.pen_data_set = new byte[0x40];
            tx_tilemap.pen_to_flags = new byte[1, 16];
            for (i = 1; i < 16; i++)
            {
                tx_tilemap.pen_to_flags[0, i] = 0x10;
            }
            tx_tilemap.pen_to_flags[0, 0] = 0;
            tx_tilemap.scrollrows = 1;
            tx_tilemap.scrollcols = 1;
            tx_tilemap.rowscroll = new int[tx_tilemap.scrollrows];
            tx_tilemap.colscroll = new int[tx_tilemap.scrollcols];
            tx_tilemap.tilemap_draw_instance3 = tx_tilemap.tilemap_draw_instanceTaitob;
            tx_tilemap.tile_update3 = tx_tilemap.tile_updateTaitobtx;
        }
    }
    public unsafe partial class Tmap
    {
        public void tilemap_draw_instanceTaitob(RECT cliprect, int xpos, int ypos)
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
        public void tile_updateTaitobbg(int col, int row)
        {
            int x0 = tilewidth * col;
            int y0 = tileheight * row;
            int flags;
            int tile_index;
            int tile, code, color;
            int pen_data_offset, palette_base, group;
            tile_index = row * cols + col;
            tile = Taitob.TC0180VCU_ram[tile_index + Taitob.bg_rambank[0]];
            code = tile % total_elements;
            color = Taitob.TC0180VCU_ram[tile_index + Taitob.bg_rambank[1]];
            pen_data_offset = code * 0x100;
            palette_base = 0x10 * (Taitob.b_bg_color_base + (color & 0x3f));
            group = 0;
            flags = (((color & 0x00c0) >> 6) & 0x03) ^ (attributes & 0x03);
            tileflags[row, col] = tile_drawTaitob(Taitob.gfx1rom, pen_data_offset, x0, y0, palette_base, group, flags);
        }
        public void tile_updateTaitobfg(int col, int row)
        {
            int x0 = tilewidth * col;
            int y0 = tileheight * row;
            int flags;
            int tile_index;
            int tile, code, color;
            int pen_data_offset, palette_base, group;
            tile_index = row * cols + col;
            tile = Taitob.TC0180VCU_ram[tile_index + Taitob.fg_rambank[0]];
            code = tile % total_elements;
            color = Taitob.TC0180VCU_ram[tile_index + Taitob.fg_rambank[1]];
            pen_data_offset = code * 0x100;
            palette_base = 0x10 * (Taitob.b_fg_color_base + (color & 0x3f));
            group = 0;
            flags = (((color & 0x00c0) >> 6) & 0x03) ^ (attributes & 0x03);
            tileflags[row, col] = tile_drawTaitob(Taitob.gfx1rom, pen_data_offset, x0, y0, palette_base, group, flags);
        }
        public void tile_updateTaitobtx(int col, int row)
        {
            int x0 = tilewidth * col;
            int y0 = tileheight * row;
            int flags;
            int tile_index;
            int tile, code, color;
            int pen_data_offset, palette_base, group;
            tile_index = row * cols + col;
            tile = Taitob.TC0180VCU_ram[tile_index + Taitob.tx_rambank];
            code = ((tile & 0x07ff) | ((Taitob.TC0180VCU_ctrl[4 + ((tile & 0x800) >> 11)] >> 8) << 11)) % total_elements;
            color = Taitob.b_tx_color_base + ((tile >> 12) & 0x0f);
            pen_data_offset = code * 0x40;
            palette_base = 0x10 * color;
            group = 0;
            flags = attributes & 0x03;
            tileflags[row, col] = tile_drawTaitobtx(Taitob.gfx0rom, pen_data_offset, x0, y0, palette_base, group, flags);
        }
        public byte tile_drawTaitob(byte* bb1, int pen_data_offset, int x0, int y0, int palette_base, int group, int flags)
        {
            byte andmask = 0xff, ormask = 0;
            int dx0 = 1, dy0 = 1;
            int tx, ty;
            byte pen, map;
            int offset1 = 0;
            int offsety1;
            int xoffs;
            AxiArray.Copy(bb1, pen_data_offset, pen_data, 0, 0x100);
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
        public byte tile_drawTaitobtx(byte* bb1, int pen_data_offset, int x0, int y0, int palette_base, int group, int flags)
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
