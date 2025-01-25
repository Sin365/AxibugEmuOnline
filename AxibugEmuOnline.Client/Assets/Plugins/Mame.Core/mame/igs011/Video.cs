namespace MAME.Core
{
    public unsafe partial class IGS011
    {
        private static byte lhb2_pen_hi;
        private static byte[][] layer;
        public struct blitter_t
        {
            public ushort x, y, w, h, gfx_lo, gfx_hi, depth, pen, flags;
        };
        private static blitter_t blitter;
        private static void igs011_priority_w(int offset, byte data)
        {
            if (offset % 2 == 0)
            {
                priority = (ushort)((data << 8) | (priority & 0xff));
            }
            else if (offset % 2 == 1)
            {
                priority = (ushort)((priority & 0xff00) | data);
            }
        }
        private static void igs011_priority_w(ushort data)
        {
            priority = data;
        }
        public static void video_start_igs011()
        {
            int i;
            layer = new byte[8][];
            for (i = 0; i < 8; i++)
            {
                layer[i] = new byte[0x20000];
            }
            lhb2_pen_hi = 0;
        }
        public unsafe static void video_update_igs011()
        {
            int x, y, l, scr_addr, pri_addr;
            int pri_ram_offset;
            pri_ram_offset = (priority & 7) * 0x100;
            for (y = 0; y <= 0xff; y++)//ef
            {
                for (x = 0; x <= 0x1ff; x++)
                {
                    scr_addr = x + y * 0x200;
                    pri_addr = 0xff;
                    for (l = 0; l < 8; l++)
                    {
                        if (layer[l][scr_addr] != 0xff)
                        {
                            pri_addr &= ~(1 << l);
                        }
                    }
                    l = priority_ram[pri_ram_offset + pri_addr] & 7;
                    Video.bitmapbase_Ptrs[Video.curbitmap][y * 0x200 + x] = (ushort)(layer[l][scr_addr] | (l << 8));
                }
            }
        }
        public static void video_eof_igs011()
        {

        }
        private static byte igs011_layers_r1(int offset)
        {
            int layer0 = ((offset & (0x80000 / 2)) != 0 ? 4 : 0) + ((offset & 1) != 0 ? 0 : 2);
            offset >>= 1;
            offset &= 0x1ffff;
            return (byte)(layer[layer0][offset] << 8);
        }
        private static byte igs011_layers_r2(int offset)
        {
            int layer0 = ((offset & (0x80000 / 2)) != 0 ? 4 : 0) + ((offset & 1) != 0 ? 0 : 2);
            offset >>= 1;
            offset &= 0x1ffff;
            return (byte)layer[layer0 + 1][offset];
        }
        private static ushort igs011_layers_r(int offset)
        {
            int layer0 = ((offset & (0x80000 / 2)) != 0 ? 4 : 0) + ((offset & 1) != 0 ? 0 : 2);
            offset >>= 1;
            offset &= 0x1ffff;
            return (ushort)((layer[layer0][offset] << 8) | layer[layer0 + 1][offset]);
        }
        private static void igs011_layers_w(int offset, byte data)
        {
            int layer0 = ((offset & 0x80000) != 0 ? 4 : 0) + ((offset & 2) != 0 ? 0 : 2);
            offset >>= 2;
            offset &= 0x1ffff;
            if (offset % 2 == 0)
            {
                layer[layer0][offset] = data;
            }
            else if (offset % 2 == 1)
            {
                layer[layer0 + 1][offset] = data;
            }
        }
        private static void igs011_layers_w(int offset, ushort data)
        {
            int layer0 = (((offset & (0x80000 / 2)) != 0) ? 4 : 0) + ((offset & 1) != 0 ? 0 : 2);
            offset >>= 1;
            offset &= 0x1ffff;
            layer[layer0][offset] = (byte)(data >> 8);
            layer[layer0 + 1][offset] = (byte)data;
        }
        private static void igs011_palette(int offset, byte data)
        {
            int rgb;
            if (offset % 2 == 0)
            {
                paletteram16[offset / 2] = (ushort)((data << 8) | (paletteram16[offset / 2] & 0xff));
            }
            else if (offset % 2 == 1)
            {
                paletteram16[offset / 2] = (ushort)((paletteram16[offset / 2] & 0xff00) | data);
            }
            rgb = (paletteram16[(offset / 2) & 0x7ff] & 0xff) | ((paletteram16[(offset / 2) | 0x800] & 0xff) << 8);

            //Palette.palette_entry_set_color1((offset / 2) & 0x7ff, (uint)((Palette.pal5bit((byte)(rgb >> 0)) << 16) | (Palette.pal5bit((byte)(rgb >> 5)) << 8) | Palette.pal5bit((byte)(rgb >> 10))));
            //通道修改，BGRA->RGBA
            Palette.palette_entry_set_color1((offset / 2) & 0x7ff,
                (uint)(
                (Palette.pal5bit((byte)(rgb >> 10)) << 16) |
                (Palette.pal5bit((byte)(rgb >> 5)) << 8) |
                Palette.pal5bit((byte)(rgb >> 0))  
                )
                );
        }
        private static void igs011_palette(int offset, ushort data)
        {
            int rgb;
            paletteram16[offset] = data;
            rgb = (paletteram16[offset & 0x7ff] & 0xff) | ((paletteram16[offset | 0x800] & 0xff) << 8);
            //Palette.palette_entry_set_color1(offset & 0x7ff, (uint)((Palette.pal5bit((byte)(rgb >> 0)) << 16) | (Palette.pal5bit((byte)(rgb >> 5)) << 8) | Palette.pal5bit((byte)(rgb >> 10))));
            //通道修改，BGRA->RGBA
            Palette.palette_entry_set_color1(offset & 0x7ff,
                (uint)(
                (Palette.pal5bit((byte)(rgb >> 10)) << 16) |
                (Palette.pal5bit((byte)(rgb >> 5)) << 8) |
                Palette.pal5bit((byte)(rgb >> 0)))
                );
        }
        private static void igs011_blit_x_w(int offset, byte data)
        {
            if (offset % 2 == 0)
            {
                blitter.x = (ushort)((data << 8) | (blitter.x & 0xff));
            }
            else if (offset % 2 == 1)
            {
                blitter.x = (ushort)((blitter.x & 0xff00) | data);
            }
        }
        private static void igs011_blit_x_w(ushort data)
        {
            blitter.x = data;
        }
        private static void igs011_blit_y_w(int offset, byte data)
        {
            if (offset % 2 == 0)
            {
                blitter.y = (ushort)((data << 8) | (blitter.y & 0xff));
            }
            else if (offset % 2 == 1)
            {
                blitter.y = (ushort)((blitter.y & 0xff00) | data);
            }
        }
        private static void igs011_blit_y_w(ushort data)
        {
            blitter.y = data;
        }
        private static void igs011_blit_gfx_lo_w(int offset, byte data)
        {
            if (offset % 2 == 0)
            {
                blitter.gfx_lo = (ushort)((data << 8) | (blitter.gfx_lo & 0xff));
            }
            else if (offset % 2 == 1)
            {
                blitter.gfx_lo = (ushort)((blitter.gfx_lo & 0xff00) | data);
            }
        }
        private static void igs011_blit_gfx_lo_w(ushort data)
        {
            blitter.gfx_lo = data;
        }
        private static void igs011_blit_gfx_hi_w(int offset, byte data)
        {
            if (offset % 2 == 0)
            {
                blitter.gfx_hi = (ushort)((data << 8) | (blitter.gfx_hi & 0xff));
            }
            else if (offset % 2 == 1)
            {
                blitter.gfx_hi = (ushort)((blitter.gfx_hi & 0xff00) | data);
            }
        }
        private static void igs011_blit_gfx_hi_w(ushort data)
        {
            blitter.gfx_hi = data;
        }
        private static void igs011_blit_w_w(int offset, byte data)
        {
            if (offset % 2 == 0)
            {
                blitter.w = (ushort)((data << 8) | (blitter.w & 0xff));
            }
            else if (offset % 2 == 1)
            {
                blitter.w = (ushort)((blitter.w & 0xff00) | data);
            }
        }
        private static void igs011_blit_w_w(ushort data)
        {
            blitter.w = data;
        }
        private static void igs011_blit_h_w(int offset, byte data)
        {
            if (offset % 2 == 0)
            {
                blitter.h = (ushort)((data << 8) | (blitter.h & 0xff));
            }
            else if (offset % 2 == 1)
            {
                blitter.h = (ushort)((blitter.h & 0xff00) | data);
            }
        }
        private static void igs011_blit_h_w(ushort data)
        {
            blitter.h = data;
        }
        private static void igs011_blit_depth_w(int offset, byte data)
        {
            if (offset % 2 == 0)
            {
                blitter.depth = (ushort)((data << 8) | (blitter.depth & 0xff));
            }
            else if (offset % 2 == 1)
            {
                blitter.depth = (ushort)((blitter.depth & 0xff00) | data);
            }
        }
        private static void igs011_blit_depth_w(ushort data)
        {
            blitter.depth = data;
        }
        private static void igs011_blit_pen_w(int offset, byte data)
        {
            if (offset % 2 == 0)
            {
                blitter.pen = (ushort)((data << 8) | (blitter.pen & 0xff));
            }
            else if (offset % 2 == 1)
            {
                blitter.pen = (ushort)((blitter.pen & 0xff00) | data);
            }
        }
        private static void igs011_blit_pen_w(ushort data)
        {
            blitter.pen = data;
        }
        private static void igs011_blit_flags_w(ushort data)
        {
            int x, xstart, xend, xinc, flipx;
            int y, ystart, yend, yinc, flipy;
            bool depth4;
            int clear, opaque, z;
            byte trans_pen, clear_pen, pen_hi, pen = 0;
            int gfx_size = gfx1romLength;
            int gfx2_size = 0;
            if (gfx2rom != null)
            {
                gfx2_size = gfx2romLength;
            }
            blitter.flags = data;
            opaque = (blitter.flags & 0x0008) == 0 ? 1 : 0;
            clear = blitter.flags & 0x0010;
            flipx = blitter.flags & 0x0020;
            flipy = blitter.flags & 0x0040;
            if ((blitter.flags & 0x0400) == 0)
            {
                return;
            }
            pen_hi = (byte)((lhb2_pen_hi & 0x07) << 5);
            z = blitter.gfx_lo + (blitter.gfx_hi << 16);
            depth4 = !((blitter.flags & 0x7) < (4 - (blitter.depth & 0x7))) || ((z & 0x800000) != 0);
            z &= 0x7fffff;
            if (depth4)
            {
                z *= 2;
                if (gfx2rom != null && (blitter.gfx_hi & 0x80) != 0)
                {
                    trans_pen = 0x1f;
                }
                else
                {
                    trans_pen = 0x0f;
                }
                clear_pen = (byte)(blitter.pen | 0xf0);
            }
            else
            {
                if (gfx2rom != null)
                {
                    trans_pen = 0x1f;
                }
                else
                {
                    trans_pen = 0xff;
                }
                clear_pen = (byte)blitter.pen;
            }
            xstart = (blitter.x & 0x1ff) - (blitter.x & 0x200);
            ystart = (blitter.y & 0x0ff) - (blitter.y & 0x100);
            if (flipx != 0)
            {
                xend = xstart - (blitter.w & 0x1ff) - 1;
                xinc = -1;
            }
            else
            {
                xend = xstart + (blitter.w & 0x1ff) + 1;
                xinc = 1;
            }
            if (flipy != 0)
            {
                yend = ystart - (blitter.h & 0x0ff) - 1;
                yinc = -1;
            }
            else
            {
                yend = ystart + (blitter.h & 0x0ff) + 1;
                yinc = 1;
            }
            for (y = ystart; y != yend; y += yinc)
            {
                for (x = xstart; x != xend; x += xinc)
                {
                    if (clear == 0)
                    {
                        if (depth4)
                        {
                            pen = (byte)((gfx1rom[(z / 2) % gfx_size] >> (((z & 1) != 0) ? 4 : 0)) & 0x0f);
                        }
                        else
                        {
                            pen = gfx1rom[z % gfx_size];
                        }
                        if (gfx2rom != null)
                        {
                            pen &= 0x0f;
                            if ((gfx2rom[(z / 8) % gfx2_size] & (1 << (z & 7))) != 0)
                            {
                                pen |= 0x10;
                            }
                        }
                    }
                    if (x >= 0 && x <= 0x1ff && y >= 0 && y <= 0xef)
                    {
                        if (clear != 0)
                        {
                            layer[blitter.flags & 0x0007][x + y * 512] = clear_pen;
                        }
                        else if (pen != trans_pen)
                        {
                            if ((blitter.flags & 0x0007) == 0 && x == 0xa4 && y == 0x41)
                            {
                                int i1 = 1;
                            }
                            layer[blitter.flags & 0x0007][x + y * 512] = (byte)(pen | pen_hi);
                        }
                        else if (opaque != 0)
                        {
                            layer[blitter.flags & 0x0007][x + y * 512] = 0xff;
                        }
                    }
                    z++;
                }
            }
        }
    }
}
