using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MAME.Core
{
    public unsafe partial class CPS
    {
        private static int iXAll, iYAll, nBitmap;
        //private static Bitmap bmAll=new Bitmap(512,512);
        private static List<string> lBitmapHash = new List<string>();
        private static int cpsb_addr, cpsb_value, mult_factor1, mult_factor2, mult_result_lo, mult_result_hi;
        public static int layercontrol, layer_control, palette_control, in2_addr, in3_addr, out2_addr, bootleg_kludge;
        public static int[] priority, layer_enable_mask;
        public static int total_elements;
        public static uint[] primasks;
        private static int CPS1_OBJ_BASE = 0;    /* Base address of objects */
        private static int CPS1_SCROLL1_BASE = (0x02 / 2);   /* Base address of scroll 1 */
        private static int CPS1_SCROLL2_BASE = (0x04 / 2);    /* Base address of scroll 2 */
        private static int CPS1_SCROLL3_BASE = (0x06 / 2);    /* Base address of scroll 3 */
        private static int CPS1_OTHER_BASE = (0x08 / 2);    /* Base address of other video */
        private static int CPS1_PALETTE_BASE = (0x0a / 2);    /* Base address of palette */
        public static int CPS1_SCROLL1_SCROLLX = (0x0c / 2);    /* Scroll 1 X */
        public static int CPS1_SCROLL1_SCROLLY = (0x0e / 2);    /* Scroll 1 Y */
        public static int CPS1_SCROLL2_SCROLLX = (0x10 / 2);    /* Scroll 2 X */
        public static int CPS1_SCROLL2_SCROLLY = (0x12 / 2);    /* Scroll 2 Y */
        public static int CPS1_SCROLL3_SCROLLX = (0x14 / 2);    /* Scroll 3 X */
        public static int CPS1_SCROLL3_SCROLLY = (0x16 / 2);    /* Scroll 3 Y */
        private static int CPS1_STARS1_SCROLLX = (0x18 / 2);    /* Stars 1 X */
        private static int CPS1_STARS1_SCROLLY = (0x1a / 2);    /* Stars 1 Y */
        private static int CPS1_STARS2_SCROLLX = (0x1c / 2);    /* Stars 2 X */
        private static int CPS1_STARS2_SCROLLY = (0x1e / 2);    /* Stars 2 Y */
        public static int CPS1_ROWSCROLL_OFFS = (0x20 / 2);    /* base of row scroll offsets in other RAM */
        private static int CPS1_VIDEOCONTROL = (0x22 / 2);    /* flip screen, rowscroll enable */
        public static int scroll1, scroll2, scroll3;
        public static int scroll1xoff = 0, scroll2xoff = 0, scroll3xoff = 0;
        public static int obj, other;
        private static ushort[] cps1_buffered_obj, /*cps2_buffered_obj,*/ uuBFF;
        private static int cps1_last_sprite_offset;
        private static int[] cps1_stars_enabled;
        private static byte TILEMAP_PIXEL_TRANSPARENT = 0x00;		/* transparent if in none of the layers below */
        private static byte TILEMAP_PIXEL_LAYER0 = 0x10;		/* pixel is opaque in layer 0 */
        private static byte TILEMAP_PIXEL_LAYER1 = 0x20;		/* pixel is opaque in layer 1 */
        private static byte TILEMAP_PIXEL_LAYER2 = 0x40;		/* pixel is opaque in layer 2 */
        public static int scroll1x, scroll1y;
        public static int scroll2x, scroll2y;
        public static int scroll3x, scroll3y;
        private static int stars1x, stars1y, stars2x, stars2y;
        public static int pri_ctrl;

        public static bool bRecord;


        #region //指针化 cps2_buffered_obj
        static ushort[] cps2_buffered_obj_src;
        static GCHandle cps2_buffered_obj_handle;
        public static ushort* cps2_buffered_obj;
        public static int cps2_buffered_objLength;
        public static bool cps2_buffered_obj_IsNull => cps2_buffered_obj == null;
        public static ushort[] cps2_buffered_obj_set
        {
            set
            {
                cps2_buffered_obj_handle.ReleaseGCHandle();
                cps2_buffered_obj_src = value;
                cps2_buffered_objLength = value.Length;
                cps2_buffered_obj_src.GetObjectPtr(ref cps2_buffered_obj_handle, ref cps2_buffered_obj);
            }
        }
        #endregion

        private static int cps1_base(int offset, int boundary)
        {
            int base1 = cps_a_regs[offset] * 256;
            int poffset;
            base1 &= ~(boundary - 1);
            poffset = base1 & 0x3ffff;
            return poffset / 2;
        }
        private static void cps1_cps_a_w(int add, byte data)
        {
            int offset = add / 2;
            if (add % 2 == 0)
            {
                cps_a_regs[offset] = (ushort)((data << 8) | (cps_a_regs[offset] & 0xff));
            }
            else
            {
                cps_a_regs[offset] = (ushort)((cps_a_regs[offset] & 0xff00) | data);
            }
            if (offset == CPS1_PALETTE_BASE)
            {
                cps1_build_palette(cps1_base(CPS1_PALETTE_BASE, 0x0400));
            }
        }
        private static void cps1_cps_a_w(int offset, ushort data)
        {
            cps_a_regs[offset] = data;
            if (offset == CPS1_PALETTE_BASE)
            {
                cps1_build_palette(cps1_base(CPS1_PALETTE_BASE, 0x0400));
            }
        }
        private static ushort cps1_cps_b_r(int offset)
        {
            if (offset == cpsb_addr / 2)
                return (ushort)cpsb_value;
            if (offset == mult_result_lo / 2)
                return (ushort)((cps_b_regs[mult_factor1 / 2] * cps_b_regs[mult_factor2 / 2]) & 0xffff);
            if (offset == mult_result_hi / 2)
                return (ushort)((cps_b_regs[mult_factor1 / 2] * cps_b_regs[mult_factor2 / 2]) >> 16);
            if (offset == in2_addr / 2)	/* Extra input ports (on C-board) */
            {
                return (ushort)short2;
            }
            if (offset == in3_addr / 2)	/* Player 4 controls (on C-board) ("Captain Commando") */
            {
                return (ushort)sbyte3;
            }
            if (cps_version == 2)
            {
                if (offset == 0x10 / 2)
                {
                    return cps_b_regs[0x10 / 2];
                }
                if (offset == 0x12 / 2)
                {
                    return cps_b_regs[0x12 / 2];
                }
            }
            return 0xffff;
        }
        private static void cps1_cps_b_w(int offset, ushort data)
        {
            cps_b_regs[offset] = data;
            if (cps_version == 2)
            {
                if (offset == 0x0e / 2)
                {
                    return;
                }
                if (offset == 0x10 / 2)
                {
                    cps1_scanline1 = (data & 0x1ff);
                    return;
                }
                if (offset == 0x12 / 2)
                {
                    cps1_scanline2 = (data & 0x1ff);
                    return;
                }
            }
            if (offset == out2_addr / 2)
            {
                Generic.coin_lockout_w(2, ~data & 0x02);
                Generic.coin_lockout_w(3, ~data & 0x08);
            }
        }
        private static void cps1_get_video_base()
        {
            int videocontrol;
            if (scroll1 != cps1_base(CPS1_SCROLL1_BASE, 0x4000))
            {
                scroll1 = cps1_base(CPS1_SCROLL1_BASE, 0x4000);
                ttmap[0].all_tiles_dirty = true;
            }
            if (scroll2 != cps1_base(CPS1_SCROLL2_BASE, 0x4000))
            {
                scroll2 = cps1_base(CPS1_SCROLL2_BASE, 0x4000);
                ttmap[1].all_tiles_dirty = true;
            }
            if (scroll3 != cps1_base(CPS1_SCROLL3_BASE, 0x4000))
            {
                scroll3 = cps1_base(CPS1_SCROLL3_BASE, 0x4000);
                ttmap[2].all_tiles_dirty = true;
            }
            if (bootleg_kludge == 1)
            {
                cps_a_regs[CPS1_OBJ_BASE] = 0x9100;
            }
            else if (bootleg_kludge == 2)
            {
                cps_a_regs[CPS1_OBJ_BASE] = 0x9100;
            }
            else if (bootleg_kludge == 0x88) // 3wondersb
            {
                cps_b_regs[0x30 / 2] = 0x3f;
                cps_a_regs[CPS1_VIDEOCONTROL] = 0x3e;
                cps_a_regs[CPS1_SCROLL2_BASE] = 0x90c0;
                cps_a_regs[CPS1_SCROLL3_BASE] = 0x9100;
                cps_a_regs[CPS1_PALETTE_BASE] = 0x9140;
            }
            obj = cps1_base(CPS1_OBJ_BASE, 0x800);
            other = cps1_base(CPS1_OTHER_BASE, 0x800);
            scroll1x = cps_a_regs[CPS1_SCROLL1_SCROLLX] + scroll1xoff;
            scroll1y = cps_a_regs[CPS1_SCROLL1_SCROLLY];
            scroll2x = cps_a_regs[CPS1_SCROLL2_SCROLLX] + scroll2xoff;
            scroll2y = cps_a_regs[CPS1_SCROLL2_SCROLLY];
            scroll3x = cps_a_regs[CPS1_SCROLL3_SCROLLX] + scroll3xoff;
            scroll3y = cps_a_regs[CPS1_SCROLL3_SCROLLY];
            ttmap[0].rowscroll[0] = cps_a_regs[CPS1_SCROLL1_SCROLLX] + scroll1xoff;
            ttmap[0].colscroll[0] = cps_a_regs[CPS1_SCROLL1_SCROLLX];
            ttmap[1].rowscroll[0] = cps_a_regs[CPS1_SCROLL2_SCROLLX] + scroll1xoff;
            ttmap[1].colscroll[0] = cps_a_regs[CPS1_SCROLL2_SCROLLX];
            ttmap[2].rowscroll[0] = cps_a_regs[CPS1_SCROLL3_SCROLLX] + scroll1xoff;
            ttmap[2].colscroll[0] = cps_a_regs[CPS1_SCROLL3_SCROLLX];
            stars1x = cps_a_regs[CPS1_STARS1_SCROLLX];
            stars1y = cps_a_regs[CPS1_STARS1_SCROLLY];
            stars2x = cps_a_regs[CPS1_STARS2_SCROLLX];
            stars2y = cps_a_regs[CPS1_STARS2_SCROLLY];
            layercontrol = cps_b_regs[layer_control / 2];
            videocontrol = cps_a_regs[CPS1_VIDEOCONTROL];
            ttmap[0].enable = ((layercontrol & layer_enable_mask[0]) != 0);
            ttmap[1].enable = ((layercontrol & layer_enable_mask[1]) != 0 && (videocontrol & 4) != 0);
            ttmap[2].enable = ((layercontrol & layer_enable_mask[2]) != 0 && (videocontrol & 8) != 0);
            cps1_stars_enabled[0] = layercontrol & layer_enable_mask[3];
            cps1_stars_enabled[1] = layercontrol & layer_enable_mask[4];
        }
        private static void cps1_gfxram_w(int offset)
        {
            int row, col;
            int page = (offset >> 7) & 0x3c0;
            int memindex;
            if (page == (cps_a_regs[CPS1_SCROLL1_BASE] & 0x3c0))
            {
                memindex = offset / 2 & 0x0fff;
                row = memindex / 0x800 * 0x20;
                memindex %= 0x800;
                row += memindex % 0x20;
                col = memindex / 0x20;
                ttmap[0].tilemap_mark_tile_dirty(row, col);
            }
            if (page == (cps_a_regs[CPS1_SCROLL2_BASE] & 0x3c0))
            {
                memindex = offset / 2 & 0x0fff;
                row = memindex / 0x400 * 0x10;
                memindex %= 0x400;
                row += memindex % 0x10;
                col = memindex / 0x10;
                ttmap[1].tilemap_mark_tile_dirty(row, col);
            }
            if (page == (cps_a_regs[CPS1_SCROLL3_BASE] & 0x3c0))
            {
                memindex = offset / 2 & 0x0fff;
                row = memindex / 0x200 * 0x08;
                memindex %= 0x200;
                row += memindex % 0x08;
                col = memindex / 0x08;
                ttmap[2].tilemap_mark_tile_dirty(row, col);
            }
        }
        private static void cps1_update_transmasks()
        {
            int group, pen, mask;
            for (group = 0; group < 4; group++)
            {
                if (priority[group] != -1)
                {
                    mask = cps_b_regs[priority[group] / 2] ^ 0xffff;
                }
                else
                {
                    if ((layercontrol & (1 << group)) != 0)
                    {
                        mask = 0x8000;
                    }
                    else
                    {
                        mask = 0xffff;
                    }
                }
                for (pen = 0; pen < 16; pen++)
                {
                    byte fgbits = (((mask >> pen) & 1) != 0) ? TILEMAP_PIXEL_TRANSPARENT : TILEMAP_PIXEL_LAYER0;
                    byte bgbits = (((0x8000 >> pen) & 1) != 0) ? TILEMAP_PIXEL_TRANSPARENT : TILEMAP_PIXEL_LAYER1;
                    byte layermask = (byte)(fgbits | bgbits);
                    if (ttmap[0].pen_to_flags[group, pen] != layermask)
                    {
                        ttmap[0].pen_to_flags[group, pen] = layermask;
                        ttmap[0].all_tiles_dirty = true;
                    }
                    if (ttmap[1].pen_to_flags[group, pen] != layermask)
                    {
                        ttmap[1].pen_to_flags[group, pen] = layermask;
                        ttmap[1].all_tiles_dirty = true;
                    }
                    if (ttmap[2].pen_to_flags[group, pen] != layermask)
                    {
                        ttmap[2].pen_to_flags[group, pen] = layermask;
                        ttmap[2].all_tiles_dirty = true;
                    }
                }
            }
        }
        public static void video_start_cps()
        {
            //bmAll = new Bitmap(512, 512);
            //Graphics g = Graphics.FromImage(bmAll);
            //g.Clear(Color.Magenta);
            //g.Dispose();
            int i;
            ttmap[0].enable = true;
            ttmap[1].enable = true;
            ttmap[2].enable = true;
            ttmap[0].all_tiles_dirty = true;
            ttmap[1].all_tiles_dirty = true;
            ttmap[2].all_tiles_dirty = true;
            Array.Clear(ttmap[0].pen_to_flags, 0, 0x40);
            Array.Clear(ttmap[1].pen_to_flags, 0, 0x40);
            Array.Clear(ttmap[2].pen_to_flags, 0, 0x40);
            cps1_update_transmasks();
            for (i = 0; i < 0xc00; i++)
            {
                Palette.palette_entry_set_color1(i, Palette.make_rgb(0, 0, 0));
            }
            primasks = new uint[8];
            cps1_stars_enabled = new int[2];
            cps1_buffered_obj = new ushort[0x400];
            cps2_buffered_obj_set = new ushort[0x1000];
            cps2_objram1_set = new ushort[0x1000];
            cps2_objram2_set = new ushort[0x1000];

            uuBFF = new ushort[0x200 * 0x200];
            for (i = 0; i < 0x40000; i++)
            {
                uuBFF[i] = 0xbff;
            }
            Array.Clear(cps1_buffered_obj, 0, 0x400);
            AxiArray.Clear(cps2_buffered_obj, 0, 0x1000);

            AxiArray.Clear(gfxram, 0, 0x30000);
            AxiArray.Clear(cps_a_regs, 0, 0x20);
            AxiArray.Clear(cps_b_regs, 0, 0x20);
            AxiArray.Clear(cps2_objram1, 0, 0x1000);
            AxiArray.Clear(cps2_objram2, 0, 0x1000);

            cps_a_regs[CPS1_OBJ_BASE] = 0x9200;
            cps_a_regs[CPS1_SCROLL1_BASE] = 0x9000;
            cps_a_regs[CPS1_SCROLL2_BASE] = 0x9040;
            cps_a_regs[CPS1_SCROLL3_BASE] = 0x9080;
            cps_a_regs[CPS1_OTHER_BASE] = 0x9100;
            if (bootleg_kludge == 0)
            {
                scroll1xoff = 0;
                scroll2xoff = 0;
                scroll3xoff = 0;
            }
            else if (bootleg_kludge == 1)
            {
                scroll1xoff = -0x0c;
                scroll2xoff = -0x0e;
                scroll3xoff = -0x10;
            }
            else if (bootleg_kludge == 2)
            {
                scroll1xoff = -0x0c;
                scroll2xoff = -0x10;
                scroll3xoff = -0x10;
            }
            else if (bootleg_kludge == 3)
            {
                scroll1xoff = -0x08;
                scroll2xoff = -0x0b;
                scroll3xoff = -0x0c;
            }
            else if (bootleg_kludge == 0x88)
            {
                scroll1xoff = 0x4;
                scroll2xoff = 0x6;
                scroll3xoff = 0xa;
            }
            cps1_get_video_base();   /* Calculate base pointers */
            cps1_get_video_base();   /* Calculate old base pointers */
        }
        private static void cps1_build_palette(int palette_offset)
        {
            int offset, page;
            int pallete_offset1 = palette_offset;
            int ctrl = cps_b_regs[palette_control / 2];
            for (page = 0; page < 6; ++page)
            {
                if (((ctrl >> page) & 1) != 0)
                {
                    for (offset = 0; offset < 0x200; ++offset)
                    {
                        int palette = gfxram[pallete_offset1 * 2] * 0x100 + gfxram[pallete_offset1 * 2 + 1];
                        pallete_offset1++;
                        int r, g, b, bright;
                        bright = 0x0f + ((palette >> 12) << 1);
                        r = ((palette >> 8) & 0x0f) * 0x11 * bright / 0x2d;
                        g = ((palette >> 4) & 0x0f) * 0x11 * bright / 0x2d;
                        b = ((palette >> 0) & 0x0f) * 0x11 * bright / 0x2d;
                        Palette.palette_entry_set_color1(0x200 * page + offset, Palette.make_rgb(r, g, b));
                    }
                }
                else
                {
                    if (pallete_offset1 != palette_offset)
                        pallete_offset1 += 0x200;
                }
            }
        }
        private static void cps1_find_last_sprite()    /* Find the offset of last sprite */
        {
            int offset = 0;
            /* Locate the end of table marker */
            while (offset < 0x400)
            {
                if (bootleg_kludge == 3)
                {
                    /* captcommb - same end of sprite marker as CPS-2 */
                    int colour = cps1_buffered_obj[offset + 1];
                    if (colour >= 0x8000)
                    {
                        /* Marker found. This is the last sprite. */
                        cps1_last_sprite_offset = offset - 4;
                        return;
                    }
                }
                else
                {
                    int colour = cps1_buffered_obj[offset + 3];
                    if ((colour & 0xff00) == 0xff00)
                    {
                        /* Marker found. This is the last sprite. */
                        cps1_last_sprite_offset = offset - 4;
                        return;
                    }
                }
                offset += 4;
            }
            /* Sprites must use full sprite RAM */
            cps1_last_sprite_offset = 0x400 - 4;
        }
        private static void cps1_render_sprites()
        {
            int i, match;
            int baseoffset, baseadd;
            if ((bootleg_kludge == 1) || (bootleg_kludge == 2) || (bootleg_kludge == 3))
            {
                baseoffset = cps1_last_sprite_offset;
                baseadd = -4;
            }
            else
            {
                baseoffset = 0;
                baseadd = 4;
            }
            for (i = 0; i <= cps1_last_sprite_offset; i += 4)
            {
                int x, y, code, colour, col;
                x = cps1_buffered_obj[baseoffset];
                y = cps1_buffered_obj[baseoffset + 1];
                code = cps1_buffered_obj[baseoffset + 2];
                colour = cps1_buffered_obj[baseoffset + 3];
                col = colour & 0x1f;
                if (x == 0 && y == 0 && code == 0 && colour == 0)
                {
                    baseoffset += baseadd;
                    continue;
                }
                match = 0;
                foreach (gfx_range r in lsRangeS)
                {
                    if (code >= r.start && code <= r.end)
                    {
                        code += r.add;
                        match = 1;
                        break;
                    }
                }
                if (match == 0)
                {
                    baseoffset += baseadd;
                    continue;
                }
                y += 0x100;
                if ((colour & 0xff00) != 0)
                {
                    int nx = (colour & 0x0f00) >> 8;
                    int ny = (colour & 0xf000) >> 12;
                    int nxs, nys, sx, sy;
                    nx++;
                    ny++;
                    if ((colour & 0x40) != 0)
                    {
                        /* Y flip */
                        if ((colour & 0x20) != 0)
                        {
                            for (nys = 0; nys < ny; nys++)
                            {
                                for (nxs = 0; nxs < nx; nxs++)
                                {
                                    sx = (x + nxs * 16) & 0x1ff;
                                    sy = (y + nys * 16) & 0x1ff;
                                    Drawgfx.common_drawgfx_c(CPS.gfx1rom, (code & ~0xf) + ((code + (nx - 1) - nxs) & 0xf) + 0x10 * (ny - 1 - nys), col, 1, 1, sx, sy, 0x80000002, Video.screenstate.visarea);
                                }
                            }
                        }
                        else
                        {
                            for (nys = 0; nys < ny; nys++)
                            {
                                for (nxs = 0; nxs < nx; nxs++)
                                {
                                    sx = (x + nxs * 16) & 0x1ff;
                                    sy = (y + nys * 16) & 0x1ff;
                                    Drawgfx.common_drawgfx_c(CPS.gfx1rom, (code & ~0xf) + ((code + nxs) & 0xf) + 0x10 * (ny - 1 - nys), col, 0, 1, sx, sy, 0x80000002, Video.screenstate.visarea);
                                }
                            }
                        }
                    }
                    else
                    {
                        if ((colour & 0x20) != 0)
                        {
                            for (nys = 0; nys < ny; nys++)
                            {
                                for (nxs = 0; nxs < nx; nxs++)
                                {
                                    sx = (x + nxs * 16) & 0x1ff;
                                    sy = (y + nys * 16) & 0x1ff;
                                    Drawgfx.common_drawgfx_c(CPS.gfx1rom, (code & ~0xf) + ((code + (nx - 1) - nxs) & 0xf) + 0x10 * nys, col, 1, 0, sx, sy, 0x80000002, Video.screenstate.visarea);
                                }
                            }
                        }
                        else
                        {
                            for (nys = 0; nys < ny; nys++)
                            {
                                for (nxs = 0; nxs < nx; nxs++)
                                {
                                    sx = (x + nxs * 16) & 0x1ff;
                                    sy = (y + nys * 16) & 0x1ff;
                                    Drawgfx.common_drawgfx_c(CPS.gfx1rom, (code & ~0xf) + ((code + nxs) & 0xf) + 0x10 * nys, col, 0, 0, sx, sy, 0x80000002, Video.screenstate.visarea);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Drawgfx.common_drawgfx_c(CPS.gfx1rom, code, col, colour & 0x20, colour & 0x40, x & 0x1ff, y & 0x1ff, 0x80000002, Video.screenstate.visarea);
                }
                baseoffset += baseadd;
            }
        }
        private static void cps2_render_sprites()
        {
            int i, x, y, priority, code, colour, col, cps2_last_sprite_offset;
            int xoffs = 64 - cps2_port(0x08);
            int yoffs = 16 - cps2_port(0x0a);
            cps2_last_sprite_offset = 0x3ff;
            for (i = 0; i < 0x400; i++)
            {
                y = cps2_buffered_obj[i * 4 + 1];
                colour = cps2_buffered_obj[i * 4 + 3];
                if (y >= 0x8000 || colour >= 0xff00)
                {
                    cps2_last_sprite_offset = i - 1;
                    break;
                }
            }
            for (i = cps2_last_sprite_offset; i >= 0; i--)
            {
                x = cps2_buffered_obj[i * 4];
                y = cps2_buffered_obj[i * 4 + 1];
                priority = (x >> 13) & 0x07;
                code = cps2_buffered_obj[i * 4 + 2] + ((y & 0x6000) << 3);
                colour = cps2_buffered_obj[i * 4 + 3];
                col = colour & 0x1f;
                if ((colour & 0x80) != 0)
                {
                    x += cps2_port(0x08);  /* fix the offset of some games */
                    y += cps2_port(0x0a);  /* like Marvel vs. Capcom ending credits */
                }
                y += 0x100;
                if ((colour & 0xff00) != 0)
                {
                    /* handle blocked sprites */
                    int nx = (colour & 0x0f00) >> 8;
                    int ny = (colour & 0xf000) >> 12;
                    int nxs, nys, sx, sy;
                    nx++;
                    ny++;
                    if ((colour & 0x40) != 0)
                    {
                        /* Y flip */
                        if ((colour & 0x20) != 0)
                        {
                            for (nys = 0; nys < ny; nys++)
                            {
                                for (nxs = 0; nxs < nx; nxs++)
                                {
                                    sx = (x + nxs * 16 + xoffs) & 0x3ff;
                                    sy = (y + nys * 16 + yoffs) & 0x3ff;
                                    Drawgfx.common_drawgfx_c(CPS.gfx1rom, code + (nx - 1) - nxs + 0x10 * (ny - 1 - nys), (col & 0x1f), 1, 1, sx, sy, (uint)(primasks[priority] | 0x80000000), Video.screenstate.visarea);
                                }
                            }
                        }
                        else
                        {
                            for (nys = 0; nys < ny; nys++)
                            {
                                for (nxs = 0; nxs < nx; nxs++)
                                {
                                    sx = (x + nxs * 16 + xoffs) & 0x3ff;
                                    sy = (y + nys * 16 + yoffs) & 0x3ff;
                                    Drawgfx.common_drawgfx_c(CPS.gfx1rom, code + nxs + 0x10 * (ny - 1 - nys), (col & 0x1f), 0, 1, sx, sy, (uint)(primasks[priority] | 0x80000000), Video.screenstate.visarea);
                                }
                            }
                        }
                    }
                    else
                    {
                        if ((colour & 0x20) != 0)
                        {
                            for (nys = 0; nys < ny; nys++)
                            {
                                for (nxs = 0; nxs < nx; nxs++)
                                {
                                    sx = (x + nxs * 16 + xoffs) & 0x3ff;
                                    sy = (y + nys * 16 + yoffs) & 0x3ff;
                                    Drawgfx.common_drawgfx_c(CPS.gfx1rom, code + (nx - 1) - nxs + 0x10 * nys, (col & 0x1f), 1, 0, sx, sy, (uint)(primasks[priority] | 0x80000000), Video.screenstate.visarea);
                                }
                            }
                        }
                        else
                        {
                            for (nys = 0; nys < ny; nys++)
                            {
                                for (nxs = 0; nxs < nx; nxs++)
                                {
                                    sx = (x + nxs * 16 + xoffs) & 0x3ff;
                                    sy = (y + nys * 16 + yoffs) & 0x3ff;
                                    Drawgfx.common_drawgfx_c(CPS.gfx1rom, (code & ~0xf) + ((code + nxs) & 0xf) + 0x10 * nys, (col & 0x1f), 0, 0, sx, sy, (uint)(primasks[priority] | 0x80000000), Video.screenstate.visarea);
                                }
                            }
                        }
                    }
                }
                else
                {
                    /* Simple case... 1 sprite */
                    Drawgfx.common_drawgfx_c(CPS.gfx1rom, code, (col & 0x1f), colour & 0x20, colour & 0x40, (x + xoffs) & 0x3ff, (y + yoffs) & 0x3ff, (uint)(primasks[priority] | 0x80000000), Video.screenstate.visarea);
                }
            }
        }
        private static void cps1_render_stars()
        {
            int offs;
            if (starsrom == null && (cps1_stars_enabled[0] != 0 || cps1_stars_enabled[1] != 0))
            {
                return;//stars enabled but no stars ROM
            }
            if (cps1_stars_enabled[0] != 0)
            {
                for (offs = 0; offs < 0x2000 / 2; offs++)
                {
                    int col = starsrom[8 * offs + 4];
                    if (col != 0x0f)
                    {
                        int sx = (offs / 256) * 32;
                        int sy = (offs % 256);
                        sx = (sx - stars2x + (col & 0x1f)) & 0x1ff;
                        sy = ((sy - stars2y) & 0xff) + 0x100;
                        col = (int)(((col & 0xe0) >> 1) + (Video.screenstate.frame_number / 16 & 0x0f));
                        if (sx >= Video.screenstate.visarea.min_x && sx <= Video.screenstate.visarea.max_x && sy >= Video.screenstate.visarea.min_y && sy <= Video.screenstate.visarea.max_y)
                            Video.bitmapbase_Ptrs[Video.curbitmap][sy * 0x200 + sx] = (ushort)(0xa00 + col);
                    }
                }
            }
            if (cps1_stars_enabled[1] != 0)
            {
                for (offs = 0; offs < 0x2000 / 2; offs++)
                {
                    int col = starsrom[8 * offs];
                    if (col != 0x0f)
                    {
                        int sx = (offs / 256) * 32;
                        int sy = (offs % 256);
                        sx = (sx - stars1x + (col & 0x1f)) & 0x1ff;
                        sy = ((sy - stars1y) & 0xff) + 0x100;
                        col = (int)(((col & 0xe0) >> 1) + (Video.screenstate.frame_number / 16 & 0x0f));
                        if (sx >= Video.screenstate.visarea.min_x && sx <= Video.screenstate.visarea.max_x && sy >= Video.screenstate.visarea.min_y && sy <= Video.screenstate.visarea.max_y)
                            Video.bitmapbase_Ptrs[Video.curbitmap][sy * 0x200 + sx] = (ushort)(0x800 + col);
                    }
                }
            }
        }
        private static void cps1_render_layer(int layer, byte primask)
        {
            switch (layer)
            {
                case 0:
                    cps1_render_sprites();
                    break;
                case 1:
                    ttmap[0].tilemap_draw_primask(Video.screenstate.visarea, 0x20, primask);
                    break;
                case 2:
                    ttmap[1].tilemap_draw_primask(Video.screenstate.visarea, 0x20, primask);
                    break;
                case 3:
                    ttmap[2].tilemap_draw_primask(Video.screenstate.visarea, 0x20, primask);
                    break;
            }
        }
        private static void cps1_render_high_layer(int layer)
        {
            switch (layer)
            {
                case 0:
                    break;
                case 1:
                    ttmap[0].tilemap_draw_primask(Video.screenstate.visarea, 0x10, 1);
                    break;
                case 2:
                    ttmap[1].tilemap_draw_primask(Video.screenstate.visarea, 0x10, 1);
                    break;
                case 3:
                    ttmap[2].tilemap_draw_primask(Video.screenstate.visarea, 0x10, 1);
                    break;
            }
        }
        public unsafe static void video_update_cps1()
        {
            int i;
            int l0, l1, l2, l3;
            int videocontrol = cps_a_regs[CPS1_VIDEOCONTROL];
            layercontrol = cps_b_regs[layer_control / 2];
            cps1_get_video_base();
            cps1_find_last_sprite();
            cps1_update_transmasks();
            ttmap[0].tilemap_set_scrollx(0, scroll1x);
            ttmap[0].tilemap_set_scrolly(0, scroll1y);
            if ((videocontrol & 0x01) != 0)	/* linescroll enable */
            {
                int scrly = -scroll2y;
                int otheroffs;
                ttmap[1].scrollrows = 1024;
                otheroffs = cps_a_regs[CPS1_ROWSCROLL_OFFS];
                for (i = 0; i < 0x400; i++)//0x100
                {
                    ttmap[1].tilemap_set_scrollx((i - scrly) & 0x3ff, scroll2x + gfxram[(other + ((otheroffs + i) & 0x3ff)) * 2] * 0x100 + gfxram[(other + ((otheroffs + i) & 0x3ff)) * 2 + 1]);
                }
            }
            else
            {
                ttmap[1].scrollrows = 1;
                ttmap[1].tilemap_set_scrollx(0, scroll2x);
            }
            ttmap[1].tilemap_set_scrolly(0, scroll2y);
            ttmap[2].tilemap_set_scrollx(0, scroll3x);
            ttmap[2].tilemap_set_scrolly(0, scroll3y);
            Array.Copy(uuBFF, Video.bitmapbase[Video.curbitmap], 0x40000);
            cps1_render_stars();
            l0 = (layercontrol >> 0x06) & 03;
            l1 = (layercontrol >> 0x08) & 03;
            l2 = (layercontrol >> 0x0a) & 03;
            l3 = (layercontrol >> 0x0c) & 03;
            Array.Clear(Tilemap.priority_bitmap, 0, 0x40000);
            if (cps_version == 1)
            {
                if ((bootleg_kludge & 0x80) != 0)
                {
                    cps1_build_palette(cps1_base(CPS1_PALETTE_BASE, 0x0400));
                }
                cps1_render_layer(l0, 0);
                if (l1 == 0)
                    cps1_render_high_layer(l0); /* prepare mask for sprites */
                cps1_render_layer(l1, 0);
                if (l2 == 0)
                    cps1_render_high_layer(l1); /* prepare mask for sprites */
                cps1_render_layer(l2, 0);
                if (l3 == 0)
                    cps1_render_high_layer(l2); /* prepare mask for sprites */
                cps1_render_layer(l3, 0);
            }
            else
            {
                int l0pri, l1pri, l2pri, l3pri;
                l0pri = (pri_ctrl >> 4 * l0) & 0x0f;
                l1pri = (pri_ctrl >> 4 * l1) & 0x0f;
                l2pri = (pri_ctrl >> 4 * l2) & 0x0f;
                l3pri = (pri_ctrl >> 4 * l3) & 0x0f;
                /* take out the CPS1 sprites layer */
                if (l0 == 0)
                {
                    l0 = l1; l1 = 0; l0pri = l1pri;
                }
                if (l1 == 0)
                {
                    l1 = l2; l2 = 0; l1pri = l2pri;
                }
                if (l2 == 0)
                {
                    l2 = l3; l3 = 0; l2pri = l3pri;
                }
                {
                    int mask0 = 0xaa;
                    int mask1 = 0xcc;
                    if (l0pri > l1pri)
                    {
                        mask0 &= ~0x88;
                    }
                    if (l0pri > l2pri)
                    {
                        mask0 &= ~0xa0;
                    }
                    if (l1pri > l2pri)
                    {
                        mask1 &= ~0xc0;
                    }
                    primasks[0] = 0xff;
                    for (i = 1; i < 8; i++)
                    {
                        if (i <= l0pri && i <= l1pri && i <= l2pri)
                        {
                            primasks[i] = 0xfe;
                            continue;
                        }
                        primasks[i] = 0;
                        if (i <= l0pri)
                        {
                            primasks[i] |= (uint)mask0;
                        }
                        if (i <= l1pri)
                        {
                            primasks[i] |= (uint)mask1;
                        }
                        if (i <= l2pri)
                        {
                            primasks[i] |= 0xf0;
                        }
                    }
                }
                cps1_render_layer(l0, 1);
                cps1_render_layer(l1, 2);
                cps1_render_layer(l2, 4);
                cps2_render_sprites();//screen->machine, bitmap, cliprect, primasks);
            }
        }
        public static void video_eof_cps1()
        {
            int i;
            cps1_get_video_base();
            if (cps_version == 1)
            {
                for (i = 0; i < 0x400; i++)
                {
                    cps1_buffered_obj[i] = (ushort)(gfxram[obj * 2 + i * 2] * 0x100 + gfxram[obj * 2 + i * 2 + 1]);
                }
            }
        }
        public static int cps2_port(int offset)
        {
            return cps2_output[offset / 2];
        }
        public static void cps2_set_sprite_priorities()
        {
            pri_ctrl = cps2_port(0x04);
        }
        public static void cps2_objram_latch()
        {
            cps2_set_sprite_priorities();
            //memcpy(cps2_buffered_obj, cps2_objbase(), cps2_obj_size);
            int baseptr;
            baseptr = 0x7000;
            if ((cps2_objram_bank & 1) != 0)
            {
                baseptr ^= 0x0080;
            }
            if (baseptr == 0x7000)
            {
                AxiArray.Copy(cps2_objram1, cps2_buffered_obj, 0x1000);
            }
            else
            {
                AxiArray.Copy(cps2_objram2, cps2_buffered_obj, 0x1000);
            }
        }
    }
}