using System;

namespace MAME.Core
{
    public partial class M92
    {
        public static void tilemap_init()
        {
            int i, j, k;
            for (i = 0; i < 3; i++)
            {
                M92.pf_layer[i].tmap = new Tmap();
                M92.pf_layer[i].tmap.laynum = i;
                M92.pf_layer[i].tmap.rows = 64;
                M92.pf_layer[i].tmap.cols = 64;
                M92.pf_layer[i].tmap.tilewidth = 8;
                M92.pf_layer[i].tmap.tileheight = 8;
                M92.pf_layer[i].tmap.width = 0x200;
                M92.pf_layer[i].tmap.height = 0x200;
                M92.pf_layer[i].tmap.enable = true;
                M92.pf_layer[i].tmap.all_tiles_dirty = true;
                M92.pf_layer[i].tmap.pixmap = new ushort[0x200 * 0x200];
                M92.pf_layer[i].tmap.flagsmap = new byte[0x200, 0x200];
                M92.pf_layer[i].tmap.tileflags = new byte[0x40, 0x40];
                M92.pf_layer[i].tmap.total_elements = M92.gfx11romLength / 0x40;
                M92.pf_layer[i].tmap.pen_data_set = new byte[0x40];
                M92.pf_layer[i].tmap.pen_to_flags = new byte[3, 0x10];
                M92.pf_layer[i].tmap.scrollrows = 512;
                M92.pf_layer[i].tmap.scrollcols = 1;
                M92.pf_layer[i].tmap.rowscroll = new int[M92.pf_layer[i].tmap.scrollrows];
                M92.pf_layer[i].tmap.colscroll = new int[M92.pf_layer[i].tmap.scrollcols];
                M92.pf_layer[i].tmap.tilemap_draw_instance3 = M92.pf_layer[i].tmap.tilemap_draw_instanceM92;
                M92.pf_layer[i].tmap.tile_update3 = M92.pf_layer[i].tmap.tile_updateM92;

                M92.pf_layer[i].wide_tmap = new Tmap();
                M92.pf_layer[i].wide_tmap.laynum = i;
                M92.pf_layer[i].wide_tmap.rows = 64;
                M92.pf_layer[i].wide_tmap.cols = 128;
                M92.pf_layer[i].wide_tmap.tilewidth = 8;
                M92.pf_layer[i].wide_tmap.tileheight = 8;
                M92.pf_layer[i].wide_tmap.width = 0x400;
                M92.pf_layer[i].wide_tmap.height = 0x200;
                M92.pf_layer[i].wide_tmap.enable = true;
                M92.pf_layer[i].wide_tmap.all_tiles_dirty = true;
                M92.pf_layer[i].wide_tmap.pixmap = new ushort[0x200 * 0x400];
                M92.pf_layer[i].wide_tmap.flagsmap = new byte[0x200, 0x400];
                M92.pf_layer[i].wide_tmap.tileflags = new byte[0x40, 0x80];
                M92.pf_layer[i].wide_tmap.total_elements = M92.gfx11romLength / 0x40;
                M92.pf_layer[i].wide_tmap.pen_data_set = new byte[0x40];
                M92.pf_layer[i].wide_tmap.pen_to_flags = new byte[3, 0x10];
                M92.pf_layer[i].wide_tmap.scrollrows = 512;
                M92.pf_layer[i].wide_tmap.scrollcols = 1;
                M92.pf_layer[i].wide_tmap.rowscroll = new int[M92.pf_layer[i].tmap.scrollrows];
                M92.pf_layer[i].wide_tmap.colscroll = new int[M92.pf_layer[i].tmap.scrollcols];
                M92.pf_layer[i].wide_tmap.tilemap_draw_instance3 = M92.pf_layer[i].wide_tmap.tilemap_draw_instanceM92;
                M92.pf_layer[i].wide_tmap.tile_update3 = M92.pf_layer[i].wide_tmap.tile_updateM92;
            }
            for (i = 0; i < 2; i++)
            {
                for (j = 0; j < 3; j++)
                {
                    pf_layer[i].tmap.pen_to_flags[j, 0] = 0;
                    pf_layer[i].wide_tmap.pen_to_flags[j, 0] = 0;
                }
                for (k = 1; k < 0x10; k++)
                {
                    pf_layer[i].tmap.pen_to_flags[0, k] = 0x20;
                    pf_layer[i].wide_tmap.pen_to_flags[0, k] = 0x20;
                }
                for (k = 1; k < 8; k++)
                {
                    pf_layer[i].tmap.pen_to_flags[1, k] = 0x20;
                    pf_layer[i].wide_tmap.pen_to_flags[1, k] = 0x20;
                }
                for (k = 8; k < 0x10; k++)
                {
                    pf_layer[i].tmap.pen_to_flags[1, k] = 0x10;
                    pf_layer[i].wide_tmap.pen_to_flags[1, k] = 0x10;
                }
                for (k = 1; k < 0x10; k++)
                {
                    pf_layer[i].tmap.pen_to_flags[2, k] = 0x10;
                    pf_layer[i].wide_tmap.pen_to_flags[2, k] = 0x10;
                }
            }
            for (k = 0; k < 0x10; k++)
            {
                pf_layer[2].tmap.pen_to_flags[0, k] = 0x20;
                pf_layer[2].wide_tmap.pen_to_flags[0, k] = 0x20;
            }
            for (k = 0; k < 8; k++)
            {
                pf_layer[2].tmap.pen_to_flags[1, k] = 0x20;
                pf_layer[2].wide_tmap.pen_to_flags[1, k] = 0x20;
            }
            for (k = 8; k < 0x10; k++)
            {
                pf_layer[2].tmap.pen_to_flags[1, k] = 0x10;
                pf_layer[2].wide_tmap.pen_to_flags[1, k] = 0x10;
            }
            pf_layer[2].tmap.pen_to_flags[2, 0] = 0x20;
            pf_layer[2].wide_tmap.pen_to_flags[2, 0] = 0x20;
            for (k = 1; k < 0x10; k++)
            {
                pf_layer[2].tmap.pen_to_flags[2, k] = 0x10;
                pf_layer[2].wide_tmap.pen_to_flags[2, k] = 0x10;
            }
        }
    }
    public unsafe partial class Tmap
    {
        public void tilemap_draw_instanceM92(RECT cliprect, int xpos, int ypos)
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
        public void tile_updateM92(int col, int row)
        {
            int x0 = tilewidth * col;
            int y0 = tileheight * row;
            int flags;
            int tile_index;
            int tile, attrib, code;
            int pen_data_offset, palette_base, group;
            tile_index = 2 * (row * cols + col) + M92.pf_layer[laynum].vram_base;
            attrib = M92.m92_vram_data[tile_index + 1];
            tile = M92.m92_vram_data[tile_index] + ((attrib & 0x8000) << 1);
            code = tile % total_elements;
            pen_data_offset = code * 0x40;
            palette_base = 0x10 * (attrib & 0x7f);
            if ((attrib & 0x100) != 0)
            {
                group = 2;
            }
            else if ((attrib & 0x80) != 0)
            {
                group = 1;
            }
            else
            {
                group = 0;
            }
            flags = ((attrib >> 9) & 3) ^ (attributes & 0x03);
            tileflags[row, col] = tile_drawM92(M92.gfx11rom, pen_data_offset, x0, y0, palette_base, group, flags);
        }
        public byte tile_drawM92(byte* bb1, int pen_data_offset, int x0, int y0, int palette_base, int group, int flags)
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
                    pixmap[(offsety1 % 0x200) * width + x0 + xoffs] = (ushort)(palette_base + pen);
                    flagsmap[offsety1 % 0x200, x0 + xoffs] = map;
                    andmask &= map;
                    ormask |= map;
                    xoffs += dx0;
                }
            }
            return (byte)(andmask ^ ormask);
        }
    }
}
