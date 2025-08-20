namespace MAME.Core
{
    public unsafe partial class Drawgfx
    {
        public static void common_drawgfx_m92(byte* bb1, int code, int color, int flipx, int flipy, int sx, int sy, RECT clip, uint primask)
        {
            int ox;
            int oy;
            int ex;
            int ey;
            ox = sx;
            oy = sy;
            ex = sx + 0x10 - 1;
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
            int colorbase = 0x10 * color;
            blockmove_8toN_transpen_pri16_m92(bb1, code, sw, sh, 0x10, ls, ts, flipx, flipy, dw, dh, colorbase, sy, sx, primask);
        }
        public unsafe static void blockmove_8toN_transpen_pri16_m92(byte* bb1, int code, int srcwidth, int srcheight, int srcmodulo, int leftskip, int topskip, int flipx, int flipy, int dstwidth, int dstheight, int colorbase, int sy, int sx, uint primask)
        {
            int ydir, xdir, col, i, j;
            int offsetx = sx, offsety = sy;
            int srcdata_offset = code * 0x100;
            if (flipy != 0)
            {
                offsety += (dstheight - 1);
                srcdata_offset += (srcheight - dstheight - topskip) * 0x10;
                ydir = -1;
            }
            else
            {
                srcdata_offset += topskip * 0x10;
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
                        if ((1 << (Tilemap.priority_bitmap[offsety + ydir * i, offsetx + xdir * j] & 0x1f) & primask) == 0)
                        {
                            if ((Tilemap.priority_bitmap[offsety + ydir * i, offsetx + xdir * j] & 0x80) != 0)
                            {
                                Video.bitmapbase_Ptrs[Video.curbitmap][(offsety + ydir * i) * 0x200 + offsetx + xdir * j] = 0x800;
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
