namespace MAME.Core
{
    public unsafe partial class Drawgfx
    {
        public static void common_drawgfx_na(int sizex, int sizey, int tx, int ty, int code, int color, int flipx, int flipy, int sx, int sy, RECT clip)
        {
            int ox;
            int oy;
            int ex;
            int ey;
            ox = sx;
            oy = sy;
            ex = sx + sizex - 1;
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
            ey = sy + sizey - 1;
            if (sy < 0)
            {
                sy = 0;
            }
            if (sy < clip.min_y)
            {
                sy = clip.min_y;
            }
            if (ey >= 0x200)
            {
                ey = 0x200 - 1;
            }
            if (ey > clip.max_y)
            {
                ey = clip.max_y;
            }
            if (sy > ey)
            {
                return;
            }
            int sw = sizex;
            int sh = sizey;
            int ls = sx - ox;
            int ts = sy - oy;
            int dw = ex - sx + 1;
            int dh = ey - sy + 1;
            int colorbase = 0x10 * color;
            blockmove_8toN_transpen_pri16(tx, ty, code, sw, sh, ls, ts, flipx, flipy, dw, dh, colorbase, sy, sx);
        }
        private unsafe static void setpixelcolorNa(int offsety, int offsetx, int n)
        {
            if (Tilemap.priority_bitmap[offsety, offsetx] != 0x1f && Tilemap.priority_bitmap[offsety, offsetx] <= Namcos1.namcos1_pri)
            {
                Video.bitmapbase_Ptrs[Video.curbitmap][offsety * 0x200 + offsetx] = (ushort)n;
            }
            Tilemap.priority_bitmap[offsety, offsetx] = 0x1f;
        }
        public static void blockmove_8toN_transpen_pri16(int tx, int ty, int code, int srcwidth, int srcheight, int leftskip, int topskip, int flipx, int flipy, int dstwidth, int dstheight, int colorbase, int offsety, int offsetx)
        {
            int xdir, ydir;
            int src_offset;
            int iwidth, iheight;
            int col;
            src_offset = code * 0x400 + tx + ty * 0x20;
            if (flipy != 0)
            {
                offsety += dstheight - 1;
                src_offset += (srcheight - dstheight - topskip) * 0x20;
                ydir = -1;
            }
            else
            {
                src_offset += topskip * 0x20;
                ydir = 1;
            }
            if (flipx != 0)
            {
                offsetx += dstwidth - 1;
                src_offset += (srcwidth - dstwidth - leftskip);
                xdir = -1;
            }
            else
            {
                src_offset += leftskip;
                xdir = 1;
            }
            for (iheight = 0; iheight < dstheight; iheight++)
            {
                for (iwidth = 0; iwidth < dstwidth; iwidth++)
                {
                    col = Namcos1.gfx3rom[src_offset + iheight * 0x20 + iwidth];
                    if (col != 0x0f)
                    {
                        setpixelcolorNa(offsety + iheight * ydir, offsetx + iwidth * xdir, colorbase + col);
                    }
                }
            }
        }
    }
}
