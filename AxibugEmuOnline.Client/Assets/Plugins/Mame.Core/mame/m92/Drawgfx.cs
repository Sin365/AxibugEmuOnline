using System.Runtime.CompilerServices;

namespace MAME.Core
{
    public unsafe partial class Drawgfx
    {
        //public static void common_drawgfx_m92(byte* bb1, int code, int color, int flipx, int flipy, int sx, int sy, RECT clip, uint primask)
        //{
        //    int ox;
        //    int oy;
        //    int ex;
        //    int ey;
        //    ox = sx;
        //    oy = sy;
        //    ex = sx + 0x10 - 1;
        //    if (sx < 0)
        //    {
        //        sx = 0;
        //    }
        //    if (sx < clip.min_x)
        //    {
        //        sx = clip.min_x;
        //    }
        //    if (ex >= 0x200)
        //    {
        //        ex = 0x200 - 1;
        //    }
        //    if (ex > clip.max_x)
        //    {
        //        ex = clip.max_x;
        //    }
        //    if (sx > ex)
        //    {
        //        return;
        //    }
        //    ey = sy + 0x10 - 1;
        //    if (sy < 0)
        //    {
        //        sy = 0;
        //    }
        //    if (sy < clip.min_y)
        //    {
        //        sy = clip.min_y;
        //    }
        //    if (ey >= 0x100)
        //    {
        //        ey = 0x100 - 1;
        //    }
        //    if (ey > clip.max_y)
        //    {
        //        ey = clip.max_y;
        //    }
        //    if (sy > ey)
        //    {
        //        return;
        //    }
        //    int sw = 0x10;
        //    int sh = 0x10;
        //    int ls = sx - ox;
        //    int ts = sy - oy;
        //    int dw = ex - sx + 1;
        //    int dh = ey - sy + 1;
        //    int colorbase = 0x10 * color;
        //    blockmove_8toN_transpen_pri16_m92(bb1, code, sw, sh, 0x10, ls, ts, flipx, flipy, dw, dh, colorbase, sy, sx, primask);
        //}
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void common_drawgfx_m92(byte* bb1, int code, int color, int flipx, int flipy, int sx, int sy, RECT clip, uint primask)
        {
            // 使用常量折叠和预计算[5](@ref)
            const int TEMP1 = 0x10 - 1;  // 15
            const int TEMP3 = 0x100 - 1; // 255
            const int TEMP4 = 0x200 - 1; // 511

            int ox = sx;
            int oy = sy;
            int ex = sx + TEMP1;

            // 边界检查优化：减少分支预测错误[5](@ref)
            sx = sx < 0 ? 0 : sx;
            sx = sx < clip.min_x ? clip.min_x : sx;
            ex = ex >= 0x200 ? TEMP4 : ex;
            ex = ex > clip.max_x ? clip.max_x : ex;

            if (sx > ex) return;

            int ey = sy + TEMP1;
            sy = sy < 0 ? 0 : sy;
            sy = sy < clip.min_y ? clip.min_y : sy;
            ey = ey >= 0x100 ? TEMP3 : ey;
            ey = ey > clip.max_y ? clip.max_y : ey;

            if (sy > ey) return;

            // 使用局部变量避免重复计算[2,4](@ref)
            int ls = sx - ox;
            int ts = sy - oy;
            int dw = ex - sx + 1;
            int dh = ey - sy + 1;
            int colorbase = color << 4; // 用移位代替乘法 0x10 * color

            // 内联关键函数调用
            blockmove_8toN_transpen_pri16_m92(bb1, code, 0x10, 0x10, 0x10, ls, ts, flipx, flipy, dw, dh, colorbase, sy, sx, primask);
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
