namespace MAME.Core
{
    public unsafe partial class Konami68000
    {
        public static void common_drawgfxzoom_konami68000(byte* bb1, int code, int color, int flipx, int flipy, int sx, int sy, RECT clip, int transparent_color, int scalex, int scaley)
        {
            if ((scalex == 0) || (scaley == 0))
            {
                return;
            }
            if (scalex == 0x10000 && scaley == 0x10000)
            {
                common_drawgfx_konami68000(bb1, code, color, flipx, flipy, sx, sy, clip, 0);
                return;
            }
            RECT myclip;
            myclip.min_x = clip.min_x;
            myclip.max_x = clip.max_x;
            myclip.min_y = clip.min_y;
            myclip.max_y = clip.max_y;
            if (myclip.min_x < 0)
                myclip.min_x = 0;
            if (myclip.max_x >= 0x200)
                myclip.max_x = 0x200 - 1;
            if (myclip.min_y < 0)
                myclip.min_y = 0;
            if (myclip.max_y >= 0x100)
                myclip.max_y = 0x100 - 1;
            int colorbase = 0x10 * (color % 0x80);
            int source_baseoffset = (code % sprite_totel_element) * 0x100;
            int sprite_screen_height = (scaley * 0x10 + 0x8000) >> 16;
            int sprite_screen_width = (scalex * 0x10 + 0x8000) >> 16;
            int countx, county, i, j, srcoffset, dstoffset;
            if (sprite_screen_width != 0 && sprite_screen_height != 0)
            {
                int dx = (0x10 << 16) / sprite_screen_width;
                int dy = (0x10 << 16) / sprite_screen_height;
                int ex = sx + sprite_screen_width;
                int ey = sy + sprite_screen_height;
                int x_index_base;
                int y_index;
                if (flipx != 0)
                {
                    x_index_base = (sprite_screen_width - 1) * dx;
                    dx = -dx;
                }
                else
                {
                    x_index_base = 0;
                }
                if (flipy != 0)
                {
                    y_index = (sprite_screen_height - 1) * dy;
                    dy = -dy;
                }
                else
                {
                    y_index = 0;
                }
                if (sx < myclip.min_x)
                {
                    int pixels = myclip.min_x - sx;
                    sx += pixels;
                    x_index_base += pixels * dx;
                }
                if (sy < myclip.min_y)
                {
                    int pixels = myclip.min_y - sy;
                    sy += pixels;
                    y_index += pixels * dy;
                }
                if (ex > myclip.max_x + 1)
                {
                    int pixels = ex - myclip.max_x - 1;
                    ex -= pixels;
                }
                if (ey > myclip.max_y + 1)
                {
                    int pixels = ey - myclip.max_y - 1;
                    ey -= pixels;
                }
                if (ex > sx)
                {
                    countx = ex - sx;
                    county = ey - sy;
                    for (i = 0; i < county; i++)
                    {
                        for (j = 0; j < countx; j++)
                        {
                            int c;
                            srcoffset = ((y_index + dy * i) >> 16) * 0x10 + ((x_index_base + dx * j) >> 16);
                            dstoffset = (sy + i) * 0x200 + sx + j;
                            c = bb1[source_baseoffset + srcoffset];
                            if (c != transparent_color)
                            {
                                Video.bitmapbase_Ptrs[Video.curbitmap][(sy + i) * 0x200 + sx + j] = (ushort)(colorbase + c);
                            }
                        }
                    }
                }
            }
        }
        public static void common_drawgfxzoom_konami68000(byte* bb1, int code, int color, int flipx, int flipy, int sx, int sy, RECT clip, int transparent_color, int scalex, int scaley, uint pri_mask)
        {
            if ((scalex == 0) || (scaley == 0))
            {
                return;
            }
            if (scalex == 0x10000 && scaley == 0x10000)
            {
                common_drawgfx_konami68000(bb1, code, color, flipx, flipy, sx, sy, clip, pri_mask);
                return;
            }
            RECT myclip;
            myclip.min_x = clip.min_x;
            myclip.max_x = clip.max_x;
            myclip.min_y = clip.min_y;
            myclip.max_y = clip.max_y;
            if (myclip.min_x < 0)
                myclip.min_x = 0;
            if (myclip.max_x >= 0x200)
                myclip.max_x = 0x200 - 1;
            if (myclip.min_y < 0)
                myclip.min_y = 0;
            if (myclip.max_y >= 0x100)
                myclip.max_y = 0x100 - 1;
            int colorbase = 0x10 * (color % 0x80);
            int source_baseoffset = (code % sprite_totel_element) * 0x100;
            int sprite_screen_height = (scaley * 0x10 + 0x8000) >> 16;
            int sprite_screen_width = (scalex * 0x10 + 0x8000) >> 16;
            int countx, county, i, j, srcoffset, dstoffset;
            if (sprite_screen_width != 0 && sprite_screen_height != 0)
            {
                int dx = (0x10 << 16) / sprite_screen_width;
                int dy = (0x10 << 16) / sprite_screen_height;
                int ex = sx + sprite_screen_width;
                int ey = sy + sprite_screen_height;
                int x_index_base;
                int y_index;
                if (flipx != 0)
                {
                    x_index_base = (sprite_screen_width - 1) * dx;
                    dx = -dx;
                }
                else
                {
                    x_index_base = 0;
                }
                if (flipy != 0)
                {
                    y_index = (sprite_screen_height - 1) * dy;
                    dy = -dy;
                }
                else
                {
                    y_index = 0;
                }
                if (sx < myclip.min_x)
                {
                    int pixels = myclip.min_x - sx;
                    sx += pixels;
                    x_index_base += pixels * dx;
                }
                if (sy < myclip.min_y)
                {
                    int pixels = myclip.min_y - sy;
                    sy += pixels;
                    y_index += pixels * dy;
                }
                if (ex > myclip.max_x + 1)
                {
                    int pixels = ex - myclip.max_x - 1;
                    ex -= pixels;
                }
                if (ey > myclip.max_y + 1)
                {
                    int pixels = ey - myclip.max_y - 1;
                    ey -= pixels;
                }
                if (ex > sx)
                {
                    countx = ex - sx;
                    county = ey - sy;
                    for (i = 0; i < county; i++)
                    {
                        for (j = 0; j < countx; j++)
                        {
                            int c;
                            srcoffset = ((y_index + dy * i) >> 16) * 0x10 + ((x_index_base + dx * j) >> 16);
                            dstoffset = (sy + i) * 0x200 + sx + j;
                            c = bb1[source_baseoffset + srcoffset];
                            if (c != transparent_color)
                            {
                                if (((1 << Tilemap.priority_bitmap[sy + i, sx + j]) & pri_mask) == 0)
                                {
                                    Video.bitmapbase_Ptrs[Video.curbitmap][(sy + i) * 0x200 + sx + j] = (ushort)(colorbase + c);
                                }
                                Tilemap.priority_bitmap[sy + i, sx + j] = 0x1f;
                            }
                        }
                    }
                }
            }
        }
        public static void common_drawgfx_konami68000(byte* bb1, int code, int color, int flipx, int flipy, int sx, int sy, RECT clip, uint pri_mask)
        {
            int ox;
            int oy;
            int ex;
            int ey;
            ox = sx;
            oy = sy;
            ex = sx + 0x10 - 1;
            if (code > 0x4000 || color > 0x80)
            {
                int i1 = 1;
            }
            if (sx < 0)
            {
                sx = 0;
            }
            if (sx < clip.min_x)
            {
                sx = clip.min_x;
            }
            if (ex >= 0x200)
            {
                ex = 0x200 - 1;
            }
            if (ex > clip.max_x)
            {
                ex = clip.max_x;
            }
            if (sx > ex)
            {
                return;
            }
            ey = sy + 0x10 - 1;
            if (sy < 0)
            {
                sy = 0;
            }
            if (sy < clip.min_y)
            {
                sy = clip.min_y;
            }
            if (ey >= 0x100)
            {
                ey = 0x100 - 1;
            }
            if (ey > clip.max_y)
            {
                ey = clip.max_y;
            }
            if (sy > ey)
            {
                return;
            }
            int sw = 0x10;
            int sh = 0x10;
            int ls = sx - ox;
            int ts = sy - oy;
            int dw = ex - sx + 1;
            int dh = ey - sy + 1;
            int colorbase = color * 0x10;
            blockmove_8toN_transpen_pri16_konami68000(bb1, code, sw, sh, 0x10, ls, ts, flipx, flipy, dw, dh, colorbase, pri_mask, sx, sy);
        }
        public static void blockmove_8toN_transpen_pri16_konami68000(byte* bb1, int code, int srcwidth, int srcheight, int srcmodulo,
                int leftskip, int topskip, int flipx, int flipy,
                int dstwidth, int dstheight, int colorbase, uint pmask, int sx, int sy)
        {
            int ydir, xdir, col, i, j, offsetx, offsety;
            int srcdata_offset = code * 0x100;
            offsetx = sx;
            offsety = sy;
            if (flipy != 0)
            {
                offsety += (dstheight - 1);
                srcdata_offset += (srcheight - dstheight - topskip) * srcmodulo;
                ydir = -1;
            }
            else
            {
                srcdata_offset += topskip * srcmodulo;
                ydir = 1;
            }
            if (flipx != 0)
            {
                offsetx += (dstwidth - 1);
                srcdata_offset += (srcwidth - dstwidth - leftskip);
                xdir = -1;
            }
            else
            {
                srcdata_offset += leftskip;
                xdir = 1;
            }
            for (i = 0; i < dstheight; i++)
            {
                for (j = 0; j < dstwidth; j++)
                {
                    col = bb1[srcdata_offset + srcmodulo * i + j];
                    if (col != 0)
                    {
                        if (((1 << (Tilemap.priority_bitmap[offsety + ydir * i, offsetx + xdir * j] & 0x1f)) & pmask) == 0)
                        {
                            if ((Tilemap.priority_bitmap[offsety + ydir * i, offsetx + xdir * j] & 0x80) != 0)
                            {
                                Video.bitmapbase_Ptrs[Video.curbitmap][(offsety + ydir * i) * 0x200 + offsetx + xdir * j] = (ushort)(colorbase + col);//palette_shadow_table[paldata[col]];
                            }
                            else
                            {
                                Video.bitmapbase_Ptrs[Video.curbitmap][(offsety + ydir * i) * 0x200 + offsetx + xdir * j] = (ushort)(colorbase + col);
                            }
                        }
                        Tilemap.priority_bitmap[offsety + ydir * i, offsetx + xdir * j] = (byte)((Tilemap.priority_bitmap[offsety + ydir * i, offsetx + xdir * j] & 0x7f) | 0x1f);
                    }
                }
            }
        }
    }
}
