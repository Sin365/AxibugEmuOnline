using System;

namespace MAME.Core
{
    public unsafe partial class CPS
    {
        public static Tmap[] ttmap;
        public static void tilemap_init()
        {
            int i;
            ttmap = new Tmap[3];
            ttmap[0] = new Tmap();
            ttmap[0].tilewidth = 8;
            ttmap[0].tileheight = 8;
            ttmap[0].width = 0x200;
            ttmap[0].height = 0x200;
            ttmap[0].scrollrows = 1;
            ttmap[0].pixmap = new ushort[0x200 * 0x200];
            ttmap[0].flagsmap = new byte[0x200, 0x200];
            ttmap[0].tileflags = new byte[0x40, 0x40];
            ttmap[0].pen_to_flags = new byte[4, 16];
            ttmap[0].pen_data_set = new byte[0x40];
            ttmap[1] = new Tmap();
            ttmap[1].tilewidth = 0x10;
            ttmap[1].tileheight = 0x10;
            ttmap[1].width = 0x400;
            ttmap[1].height = 0x400;
            ttmap[1].scrollrows = 0x400;
            ttmap[1].pixmap = new ushort[0x400 * 0x400];
            ttmap[1].flagsmap = new byte[0x400, 0x400];
            ttmap[1].tileflags = new byte[0x40, 0x40];
            ttmap[1].pen_to_flags = new byte[4, 16];
            ttmap[1].pen_data_set = new byte[0x100];
            ttmap[2] = new Tmap();
            ttmap[2].tilewidth = 0x20;
            ttmap[2].tileheight = 0x20;
            ttmap[2].width = 0x800;
            ttmap[2].height = 0x800;
            ttmap[2].scrollrows = 1;
            ttmap[2].pixmap = new ushort[0x800 * 0x800];
            ttmap[2].flagsmap = new byte[0x800, 0x800];
            ttmap[2].tileflags = new byte[0x40, 0x40];
            ttmap[2].pen_to_flags = new byte[4, 16];
            ttmap[2].pen_data_set = new byte[0x400];
            for (i = 0; i < 3; i++)
            {
                ttmap[i].rows = 0x40;
                ttmap[i].cols = 0x40;
                ttmap[i].enable = true;
                ttmap[i].all_tiles_dirty = true;
                ttmap[i].scrollcols = 1;
                ttmap[i].rowscroll = new int[ttmap[i].scrollrows];
                ttmap[i].colscroll = new int[ttmap[i].scrollcols];
                ttmap[i].tilemap_draw_instance3 = ttmap[i].tilemap_draw_instanceC;
                ttmap[i].tilemap_set_scrolldx(0, 0);
                ttmap[i].tilemap_set_scrolldy(0x100, 0);
            }
            ttmap[0].tile_update3 = ttmap[0].tile_updateC0;
            ttmap[1].tile_update3 = ttmap[1].tile_updateC1;
            ttmap[2].tile_update3 = ttmap[2].tile_updateC2;
            ttmap[0].total_elements = CPS.gfxromLength / 0x40;
            ttmap[1].total_elements = CPS.gfxromLength / 0x80;
            ttmap[2].total_elements = CPS.gfxromLength / 0x200;
        }
    }
    public unsafe partial class Tmap
    {
        public void tile_updateC0(int col, int row)
        {
            byte group0, flags0;
            int x0 = 0x08 * col;
            int y0 = 0x08 * row;
            int palette_base0;
            int code, attr;
            int memindex;
            int gfxset;
            int match;
            int i, j;
            memindex = (row & 0x1f) + ((col & 0x3f) << 5) + ((row & 0x20) << 6);
            {
                code = CPS.gfxram[(CPS.scroll1 + 2 * memindex) * 2] * 0x100 + CPS.gfxram[(CPS.scroll1 + 2 * memindex) * 2 + 1];
                match = 0;
                foreach (CPS.gfx_range r in CPS.lsRange0)
                {
                    if (code >= r.start && code <= r.end)
                    {
                        code += r.add;
                        match = 1;
                        break;
                    }
                }
                code %= CPS.ttmap[0].total_elements;
                gfxset = (memindex & 0x20) >> 5;
                attr = CPS.gfxram[(CPS.scroll1 + 2 * memindex + 1) * 2] * 0x100 + CPS.gfxram[(CPS.scroll1 + 2 * memindex + 1) * 2 + 1];
                {
                    if (match == 0)
                    {
                        AxiArray.Copy(Tilemap.bb0F, 0, pen_data, 0, 0x40);
                    }
                    else
                    {
                        for (j = 0; j < 0x08; j++)
                        {
                            AxiArray.Copy(CPS.gfx1rom, code * 0x80 + gfxset * 8 + j * 0x10, pen_data, j * 8, 8);
                        }
                    }
                    palette_base0 = 0x10 * ((attr & 0x1f) + 0x20);
                    flags0 = (byte)(((attr & 0x60) >> 5) & 3);
                }
                group0 = (byte)((attr & 0x0180) >> 7);
            }
            {
                int offset = 0;
                byte andmask = 0xff, ormask = 0;
                int dx0 = 1, dy0 = 1;
                int tx, ty;
                if ((flags0 & Tilemap.TILE_FLIPY) != 0)
                {
                    y0 += 0x07;
                    dy0 = -1;
                }
                if ((flags0 & Tilemap.TILE_FLIPX) != 0)
                {
                    x0 += 0x07;
                    dx0 = -1;
                }
                for (ty = 0; ty < 0x08; ty++)
                {
                    int offsetx1 = x0;
                    int offsety1 = y0;
                    int xoffs = 0;
                    y0 += dy0;
                    for (tx = 0; tx < 0x08; tx++)
                    {
                        byte pen, map;
                        pen = pen_data[offset];
                        map = pen_to_flags[group0, pen];
                        pixmap[offsety1 * 0x200 + offsetx1 + xoffs] = (ushort)(palette_base0 + pen);
                        flagsmap[offsety1, offsetx1 + xoffs] = map;
                        andmask &= map;
                        ormask |= map;
                        xoffs += dx0;
                        offset++;
                    }
                }
                tileflags[row, col] = (byte)(andmask ^ ormask);
            }
        }
        public void tile_updateC1(int col, int row)
        {
            byte group1, flags1;
            int x0 = 0x10 * col;
            int y0 = 0x10 * row;
            int palette_base1;
            int code, attr;
            int memindex;
            int match;
            memindex = (row & 0x0f) + ((col & 0x3f) << 4) + ((row & 0x30) << 6);
            {
                code = CPS.gfxram[(CPS.scroll2 + 2 * memindex) * 2] * 0x100 + CPS.gfxram[(CPS.scroll2 + 2 * memindex) * 2 + 1];
                match = 0;
                foreach (CPS.gfx_range r in CPS.lsRange1)
                {
                    if (code >= r.start && code <= r.end)
                    {
                        code += r.add;
                        match = 1;
                        break;
                    }
                }
                code %= CPS.ttmap[1].total_elements;
                attr = CPS.gfxram[(CPS.scroll2 + 2 * memindex + 1) * 2] * 0x100 + CPS.gfxram[(CPS.scroll2 + 2 * memindex + 1) * 2 + 1];
                if (match == 0)
                {
                    AxiArray.Copy(Tilemap.bb0F, 0, pen_data, 0, 0x100);
                }
                else
                {
                    AxiArray.Copy(CPS.gfx1rom, code * 0x100, pen_data, 0, 0x100);
                }
                palette_base1 = 0x10 * ((attr & 0x1f) + 0x40);
                flags1 = (byte)(((attr & 0x60) >> 5) & 3);
                group1 = (byte)((attr & 0x0180) >> 7);
            }
            {
                int offset = 0;
                byte andmask = 0xff, ormask = 0;
                int dx0 = 1, dy0 = 1;
                int tx, ty;
                if ((flags1 & Tilemap.TILE_FLIPY) != 0)
                {
                    y0 += 0x0f;
                    dy0 = -1;
                }
                if ((flags1 & Tilemap.TILE_FLIPX) != 0)
                {
                    x0 += 0x0f;
                    dx0 = -1;
                }
                for (ty = 0; ty < 0x10; ty++)
                {
                    int offsetx1 = x0;
                    int offsety1 = y0;
                    int xoffs = 0;
                    y0 += dy0;
                    for (tx = 0; tx < 0x10; tx++)
                    {
                        byte pen, map;
                        pen = pen_data[offset];
                        map = pen_to_flags[group1, pen];
                        pixmap[offsety1 * 0x400 + offsetx1 + xoffs] = (ushort)(palette_base1 + pen);
                        flagsmap[offsety1, offsetx1 + xoffs] = map;
                        andmask &= map;
                        ormask |= map;
                        xoffs += dx0;
                        offset++;
                    }
                }
                tileflags[row, col] = (byte)(andmask ^ ormask);
            }
        }
        public void tile_updateC2(int col, int row)
        {
            byte group2, flags2;
            int x0 = 0x20 * col;
            int y0 = 0x20 * row;
            int palette_base2;
            int code, attr;
            int memindex;
            int match;
            memindex = (row & 0x07) + ((col & 0x3f) << 3) + ((row & 0x38) << 6);
            {
                code = (CPS.gfxram[(CPS.scroll3 + 2 * memindex) * 2] * 0x100 + CPS.gfxram[(CPS.scroll3 + 2 * memindex) * 2 + 1]) & 0x3fff;
                match = 0;
                foreach (CPS.gfx_range r in CPS.lsRange2)
                {
                    if (code >= r.start && code <= r.end)
                    {
                        code += r.add;
                        match = 1;
                        break;
                    }
                }
                code %= CPS.ttmap[2].total_elements;
                attr = CPS.gfxram[(CPS.scroll3 + 2 * memindex + 1) * 2] * 0x100 + CPS.gfxram[(CPS.scroll3 + 2 * memindex + 1) * 2 + 1];
                if (match == 0)
                {
                    AxiArray.Copy(Tilemap.bb0F, 0, pen_data, 0, 0x400);
                }
                else
                {
                    AxiArray.Copy(CPS.gfx1rom, code * 0x400, pen_data, 0, 0x400);
                }
                palette_base2 = 0x10 * ((attr & 0x1f) + 0x60);
                flags2 = (byte)(((attr & 0x60) >> 5) & 3);
                group2 = (byte)((attr & 0x0180) >> 7);
            }
            {
                int offset = 0;
                byte andmask = 0xff, ormask = 0;
                int dx0 = 1, dy0 = 1;
                int tx, ty;
                if ((flags2 & Tilemap.TILE_FLIPY) != 0)
                {
                    y0 += 0x1f;
                    dy0 = -1;
                }
                if ((flags2 & Tilemap.TILE_FLIPX) != 0)
                {
                    x0 += 0x1f;
                    dx0 = -1;
                }
                for (ty = 0; ty < 0x20; ty++)
                {
                    int offsetx1 = x0;
                    int offsety1 = y0;
                    int xoffs = 0;
                    y0 += dy0;
                    for (tx = 0; tx < 0x20; tx++)
                    {
                        byte pen, map;
                        pen = pen_data[offset];
                        map = pen_to_flags[group2, pen];
                        pixmap[offsety1 * 0x800 + offsetx1 + xoffs] = (ushort)(palette_base2 + pen);
                        flagsmap[offsety1, offsetx1 + xoffs] = map;
                        andmask &= map;
                        ormask |= map;
                        xoffs += dx0;
                        offset++;
                    }
                }
                tileflags[row, col] = (byte)(andmask ^ ormask);
            }
        }
        public void tilemap_draw_instanceC(RECT cliprect, int xpos, int ypos)
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
    }
}
