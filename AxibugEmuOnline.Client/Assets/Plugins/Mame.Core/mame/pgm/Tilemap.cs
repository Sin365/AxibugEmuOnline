using System;

namespace MAME.Core
{
    public unsafe partial class PGM
    {
        public static Tmap pgm_tx_tilemap, pgm_bg_tilemap;
        public static void tilemap_init()
        {
            int i;
            pgm_tx_tilemap = new Tmap();
            pgm_tx_tilemap.rows = 32;
            pgm_tx_tilemap.cols = 64;
            pgm_tx_tilemap.tilewidth = 8;
            pgm_tx_tilemap.tileheight = 8;
            pgm_tx_tilemap.width = 0x200;
            pgm_tx_tilemap.height = 0x100;
            pgm_tx_tilemap.enable = true;
            pgm_tx_tilemap.all_tiles_dirty = true;
            pgm_tx_tilemap.pixmap = new ushort[0x100 * 0x200];
            pgm_tx_tilemap.flagsmap = new byte[0x100, 0x200];
            pgm_tx_tilemap.tileflags = new byte[0x20, 0x40];
            pgm_tx_tilemap.tile_update3 = pgm_tx_tilemap.tile_updatePgmtx;
            pgm_tx_tilemap.tilemap_draw_instance3 = pgm_tx_tilemap.tilemap_draw_instancePgm;
            pgm_tx_tilemap.total_elements = 0x800000 / 0x20;
            pgm_tx_tilemap.pen_data_set = new byte[0x40];
            pgm_tx_tilemap.pen_to_flags = new byte[1, 16];
            for (i = 0; i < 15; i++)
            {
                pgm_tx_tilemap.pen_to_flags[0, i] = 0x10;
            }
            pgm_tx_tilemap.pen_to_flags[0, 15] = 0;
            pgm_tx_tilemap.scrollrows = 1;
            pgm_tx_tilemap.scrollcols = 1;
            pgm_tx_tilemap.rowscroll = new int[pgm_tx_tilemap.scrollrows];
            pgm_tx_tilemap.colscroll = new int[pgm_tx_tilemap.scrollcols];
            pgm_bg_tilemap = new Tmap();
            pgm_bg_tilemap.cols = 64;
            pgm_bg_tilemap.rows = 64;
            pgm_bg_tilemap.tilewidth = 32;
            pgm_bg_tilemap.tileheight = 32;
            pgm_bg_tilemap.width = 0x800;
            pgm_bg_tilemap.height = 0x800;
            pgm_bg_tilemap.enable = true;
            pgm_bg_tilemap.all_tiles_dirty = true;
            pgm_bg_tilemap.pixmap = new ushort[0x800 * 0x800];
            pgm_bg_tilemap.flagsmap = new byte[0x800, 0x800];
            pgm_bg_tilemap.tileflags = new byte[0x40, 0x40];
            pgm_bg_tilemap.tile_update3 = pgm_bg_tilemap.tile_updatePgmbg;
            pgm_bg_tilemap.tilemap_draw_instance3 = pgm_bg_tilemap.tilemap_draw_instancePgm;
            pgm_bg_tilemap.total_elements = 0x3333;
            pgm_bg_tilemap.pen_data_set = new byte[0x400];
            pgm_bg_tilemap.pen_to_flags = new byte[1, 32];
            for (i = 0; i < 31; i++)
            {
                pgm_bg_tilemap.pen_to_flags[0, i] = 0x10;
            }
            pgm_bg_tilemap.pen_to_flags[0, 31] = 0;
            pgm_bg_tilemap.scrollrows = 64 * 32;
            pgm_bg_tilemap.scrollcols = 1;
            pgm_bg_tilemap.rowscroll = new int[pgm_bg_tilemap.scrollrows];
            pgm_bg_tilemap.colscroll = new int[pgm_bg_tilemap.scrollcols];
        }
    }
    public unsafe partial class Tmap
    {
        public void tile_updatePgmtx(int col, int row)
        {
            int x0 = tilewidth * col;
            int y0 = tileheight * row;
            int tileno, colour, flipyx;
            int code, tile_index, palette_base;
            byte flags;
            tile_index = row * PGM.pgm_tx_tilemap.cols + col;
            tileno = (PGM.pgm_tx_videoram[tile_index * 4] * 0x100 + PGM.pgm_tx_videoram[tile_index * 4 + 1]) & 0xffff;
            colour = (PGM.pgm_tx_videoram[tile_index * 4 + 3] & 0x3e) >> 1;
            flipyx = (PGM.pgm_tx_videoram[tile_index * 4 + 3] & 0xc0) >> 6;
            if (tileno > 0xbfff)
            {
                tileno -= 0xc000;
                tileno += 0x20000;
            }
            code = tileno % PGM.pgm_tx_tilemap.total_elements;
            flags = (byte)(flipyx & 3);
            palette_base = 0x800 + 0x10 * colour;
            tileflags[row, col] = tile_drawPgmtx(code * 0x40, x0, y0, palette_base, flags);
            //tileflags[row, col] = tile_apply_bitmaskPgmtx(code << 3, x0, y0, flags);
            //tileinfo_set( tileinfo, 0,tileno,colour,flipyx&3)
        }
        public byte tile_drawPgmtx(int pendata_offset, int x0, int y0, int palette_base, byte flags)
        {
            int height = tileheight;
            int width = tilewidth;
            int dx0 = 1, dy0 = 1;
            int tx, ty;
            int offset1 = 0;
            int offsety1;
            int xoffs;
            byte andmask = 0xff, ormask = 0;
            byte pen, map;
            AxiArray.Copy(PGM.tiles1rom, pendata_offset, pen_data, 0, 0x40);
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
                    map = pen_to_flags[0, pen];
                    offset1++;
                    pixmap[offsety1 * 0x200 + x0 + xoffs] = (ushort)(palette_base + pen);
                    flagsmap[offsety1, x0 + xoffs] = map;
                    andmask &= map;
                    ormask |= map;
                    xoffs += dx0;
                }
            }
            return (byte)(andmask ^ ormask);
        }
        public unsafe void tilemap_draw_instancePgm(RECT cliprect, int xpos, int ypos)
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
                        cur_trans = trans_t.WHOLLY_TRANSPARENT;
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
                                for (i = xpos + x_start; i < xpos + x_end; i++)
                                {
                                    Tilemap.priority_bitmap[offsety2 + ypos, i] = priority;
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
                                        Tilemap.priority_bitmap[offsety2 + ypos, i] = priority;
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
        public void tile_updatePgmbg(int col, int row)
        {
            int x0 = tilewidth * col;
            int y0 = tileheight * row;
            int tileno, colour, flipyx;
            int code, tile_index, palette_base;
            byte flags;
            tile_index = row * PGM.pgm_bg_tilemap.cols + col;
            tileno = (PGM.pgm_bg_videoram[tile_index * 4] * 0x100 + PGM.pgm_bg_videoram[tile_index * 4 + 1]) & 0xffff;
            if (tileno > 0x7ff)
            {
                tileno += 0x1000;
            }
            colour = (PGM.pgm_bg_videoram[tile_index * 4 + 3] & 0x3e) >> 1;
            flipyx = (PGM.pgm_bg_videoram[tile_index * 4 + 3] & 0xc0) >> 6;
            code = tileno % PGM.pgm_bg_tilemap.total_elements;
            flags = (byte)(flipyx & 3);
            palette_base = 0x400 + 0x20 * colour;
            tileflags[row, col] = tile_drawPgmbg(code * 0x400, x0, y0, palette_base, flags);
        }
        public byte tile_drawPgmbg(int pendata_offset, int x0, int y0, int palette_base, byte flags)
        {
            int height = tileheight;
            int width = tilewidth;
            int dx0 = 1, dy0 = 1;
            int tx, ty;
            int offset1 = 0;
            int offsety1;
            int xoffs;
            byte andmask = 0xff, ormask = 0;
            byte pen, map;
            AxiArray.Copy(PGM.tiles2rom, pendata_offset, pen_data, 0, 0x400);
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
                    map = pen_to_flags[0, pen];
                    offset1++;
                    pixmap[offsety1 * 0x800 + x0 + xoffs] = (ushort)(palette_base + pen);
                    flagsmap[offsety1, x0 + xoffs] = map;
                    andmask &= map;
                    ormask |= map;
                    xoffs += dx0;
                }
            }
            return (byte)(andmask ^ ormask);
        }
    }
}
