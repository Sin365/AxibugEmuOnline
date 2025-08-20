namespace MAME.Core
{
    public unsafe partial class Drawgfx
    {
        public static void common_drawgfx_bublbobl(byte* bb1, int code, int color, int flipx, int flipy, int sx, int sy, RECT clip)
        {
            int ox;
            int oy;
            int ex;
            int ey;
            ox = sx;
            oy = sy;
            ex = sx + 8 - 1;
            if (sx < 0)
            {
                sx = 0;
            }
            if (sx < clip.min_x)
            {
                sx = clip.min_x;
            }
            if (ex >= 0x100)
            {
                ex = 0x100 - 1;
            }
            if (ex > clip.max_x)
            {
                ex = clip.max_x;
            }
            if (sx > ex)
            {
                return;
            }
            ey = sy + 8 - 1;
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
            int sw = 8;
            int sh = 8;
            int ls = sx - ox;
            int ts = sy - oy;
            int dw = ex - sx + 1;
            int dh = ey - sy + 1;
            int colorbase = 0x10 * color;
            blockmove_8toN_transpen16_bublbobl(bb1, code, sw, sh, 8, ls, ts, flipx, flipy, dw, dh, colorbase, sx, sy);
        }
        public unsafe static void blockmove_8toN_transpen16_bublbobl(byte* bb1, int code, int srcwidth, int srcheight, int srcmodulo, int leftskip, int topskip, int flipx, int flipy, int dstwidth, int dstheight, int colorbase, int offsetx, int offsety)
        {
            int ydir, xdir, col, i, j;
            int srcdata_offset = code * 0x40;
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
                    if (col != 0x0f)
                    {
                        Video.bitmapbase_Ptrs[Video.curbitmap][(offsety + ydir * i) * 0x100 + offsetx + xdir * j] = (ushort)(colorbase + col);
                    }
                }
            }
        }
        public static void common_drawgfx_opwolf(byte* bb1, int code, int color, int flipx, int flipy, int sx, int sy, RECT clip, uint pri_mask)
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
            if (ex >= 0x140)
            {
                ex = 0x140 - 1;
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
            blockmove_8toN_transpen_pri16_opwolf(bb1, code, sw, sh, 0x10, ls, ts, flipx, flipy, dw, dh, colorbase, pri_mask, sx, sy);
        }
        public unsafe static void blockmove_8toN_transpen_pri16_opwolf(byte* bb1, int code, int srcwidth, int srcheight, int srcmodulo,
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
                            Video.bitmapbase_Ptrs[Video.curbitmap][(offsety + ydir * i) * 0x140 + offsetx + xdir * j] = (ushort)(colorbase + col);
                        }
                        Tilemap.priority_bitmap[offsety + ydir * i, offsetx + xdir * j] = (byte)((Tilemap.priority_bitmap[offsety + ydir * i, offsetx + xdir * j] & 0x7f) | 0x1f);
                    }
                }
            }
        }
    }
}
