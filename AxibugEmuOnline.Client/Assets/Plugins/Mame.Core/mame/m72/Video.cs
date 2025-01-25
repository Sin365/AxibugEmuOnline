using System;
using System.Runtime.InteropServices;

namespace MAME.Core
{
    public unsafe partial class M72
    {
        public static byte[] m72_videoram1, m72_videoram2;
        public static ushort[] majtitle_rowscrollram;
        public static int m72_raster_irq_position;
        //public static ushort[] m72_spriteram;

        #region //指针化m72_spriteram
        static ushort[] m72_spriteram_src;
        static GCHandle m72_spriteram_handle;
        public static ushort* m72_spriteram;
        public static int m72_spriteramLength;
        public static ushort[] m72_spriteram_set
        {
            set
            {
                m72_spriteram_handle.ReleaseGCHandle();
                m72_spriteram_src = value;
                m72_spriteramLength = value.Length;
                m72_spriteram_src.GetObjectPtr(ref m72_spriteram_handle, ref m72_spriteram);
            }
        }
        #endregion

        private static ushort[] uuB200;
        public static int scrollx1, scrolly1, scrollx2, scrolly2;
        public static int video_off, spriteram_size, majtitle_rowscroll;
        public static ushort m72_palette1_r(int offset)
        {
            offset &= ~0x100;
            return (ushort)(Generic.paletteram16[offset] | 0xffe0);
        }
        public static ushort m72_palette2_r(int offset)
        {
            offset &= ~0x100;
            return (ushort)(Generic.paletteram16_2[offset] | 0xffe0);
        }
        public static void changecolor(int color, int r, int g, int b)
        {
            Palette.palette_entry_set_color1(color, Palette.make_rgb(Palette.pal5bit((byte)r), Palette.pal5bit((byte)g), Palette.pal5bit((byte)b)));
        }
        public static void m72_palette1_w(int offset, ushort data)
        {
            offset &= ~0x100;
            Generic.paletteram16[offset] = data;
            offset &= 0x0ff;
            changecolor(offset, Generic.paletteram16[offset + 0x000], Generic.paletteram16[offset + 0x200], Generic.paletteram16[offset + 0x400]);
        }
        public static void m72_palette2_w(int offset, ushort data)
        {
            offset &= ~0x100;
            Generic.paletteram16_2[offset] = data;
            offset &= 0x0ff;
            changecolor(offset + 256, Generic.paletteram16_2[offset + 0x000], Generic.paletteram16_2[offset + 0x200], Generic.paletteram16_2[offset + 0x400]);
        }
        public static void m72_videoram1_w(int offset, byte data)
        {
            int row, col;
            m72_videoram1[offset] = data;
            row = (offset / 4) / 0x40;
            col = (offset / 4) % 0x40;
            fg_tilemap.tilemap_mark_tile_dirty(row, col);
        }
        public static void m72_videoram2_w(int offset, byte data)
        {
            int row, col;
            m72_videoram2[offset] = data;
            row = (offset / 4) / 0x40;
            col = (offset / 4) % 0x40;
            bg_tilemap.tilemap_mark_tile_dirty(row, col);
        }
        public static void m72_videoram1_w(int offset, ushort data)
        {
            int row, col;
            m72_videoram1[offset * 2] = (byte)data;
            m72_videoram1[offset * 2 + 1] = (byte)(data >> 8);
            row = (offset / 2) / 0x40;
            col = (offset / 2) % 0x40;
            fg_tilemap.tilemap_mark_tile_dirty(row, col);
        }
        public static void m72_videoram2_w(int offset, ushort data)
        {
            int row, col;
            m72_videoram2[offset * 2] = (byte)data;
            m72_videoram2[offset * 2 + 1] = (byte)(data >> 8);
            row = (offset / 2) / 0x40;
            col = (offset / 2) % 0x40;
            bg_tilemap.tilemap_mark_tile_dirty(row, col);
        }
        public static void m72_irq_line_w(ushort data)
        {
            m72_raster_irq_position = data;
        }
        public static void m72_scrollx1_w(ushort data)
        {
            scrollx1 = data;
        }
        public static void m72_scrollx2_w(ushort data)
        {
            scrollx2 = data;
        }
        public static void m72_scrolly1_w(ushort data)
        {
            scrolly1 = data;
        }
        public static void m72_scrolly2_w(ushort data)
        {
            scrolly2 = data;
        }
        public static void m72_dmaon_w(ushort data)
        {
            //if (ACCESSING_BITS_0_7)
            AxiArray.Copy(Generic.spriteram16, m72_spriteram, spriteram_size / 2);
        }
        public static void m72_port02_w(ushort data)
        {
            //if (ACCESSING_BITS_0_7)
            {
                //coin_counter_w(0,data & 0x01);
                //coin_counter_w(1,data & 0x02);
                //flip_screen_set(((data & 0x04) >> 2) ^ ((~input_port_read(machine, "DSW") >> 8) & 1));
                video_off = data & 0x08;
                if ((data & 0x10) != 0)
                {
                    Cpuint.cpunum_set_input_line(1, (int)LineState.INPUT_LINE_RESET, LineState.CLEAR_LINE);
                }
                else
                {
                    Cpuint.cpunum_set_input_line(1, (int)LineState.INPUT_LINE_RESET, LineState.ASSERT_LINE);
                }
            }
        }
        public static void rtype2_port02_w(ushort data)
        {
            //if (ACCESSING_BITS_0_7)
            {
                //coin_counter_w(0,data & 0x01);
                //coin_counter_w(1,data & 0x02);
                //flip_screen_set(((data & 0x04) >> 2) ^ ((~input_port_read(machine, "DSW") >> 8) & 1));
                video_off = data & 0x08;
            }
        }
        public static void majtitle_gfx_ctrl_w(ushort data)
        {
            //if (ACCESSING_BITS_8_15)
            {
                if ((data & 0xff00) != 0)
                {
                    majtitle_rowscroll = 1;
                }
                else
                {
                    majtitle_rowscroll = 0;
                }
            }
        }
        public static void m72_draw_sprites(RECT rect)
        {
            int offs;
            offs = 0;
            while (offs < spriteram_size / 2)
            {
                int code, color, sx, sy, flipx, flipy, w, h, x, y;
                code = m72_spriteram[offs + 1];
                color = m72_spriteram[offs + 2] & 0x0f;
                sx = -256 + (m72_spriteram[offs + 3] & 0x3ff);
                sy = 384 - (m72_spriteram[offs + 0] & 0x1ff);
                flipx = m72_spriteram[offs + 2] & 0x0800;
                flipy = m72_spriteram[offs + 2] & 0x0400;
                w = 1 << ((m72_spriteram[offs + 2] & 0xc000) >> 14);
                h = 1 << ((m72_spriteram[offs + 2] & 0x3000) >> 12);
                sy -= 16 * h;
                /*if (flip_screen_get())
                {
                    sx = 512 - 16*w - sx;
                    sy = 284 - 16*h - sy;
                    flipx = !flipx;
                    flipy = !flipy;
                }*/
                for (x = 0; x < w; x++)
                {
                    for (y = 0; y < h; y++)
                    {
                        int c = code;
                        if (flipx != 0)
                        {
                            c += 8 * (w - 1 - x);
                        }
                        else
                        {
                            c += 8 * x;
                        }
                        if (flipy != 0)
                        {
                            c += h - 1 - y;
                        }
                        else
                        {
                            c += y;
                        }
                        Drawgfx.common_drawgfx_m72(M72.sprites1rom, c, color, flipx, flipy, sx + 16 * x, sy + 16 * y, rect);
                    }
                }
                offs += w * 4;
            }
        }
        public static void video_start_m72()
        {
            int i;
            uuB200 = new ushort[0x200 * 0x200];
            Video.new_clip = new RECT();
            spriteram_size = 0x400;
            for (i = 0; i < 0x40000; i++)
            {
                uuB200[i] = 0x200;
            }
            m72_spriteram_set = new ushort[0x200];
            m72_videoram1 = new byte[0x4000];
            m72_videoram2 = new byte[0x4000];
            fg_tilemap.tilemap_set_scrolldx(0, 0);
            fg_tilemap.tilemap_set_scrolldy(-128, 16);
            bg_tilemap.tilemap_set_scrolldx(0, 0);
            bg_tilemap.tilemap_set_scrolldy(-128, 16);
            switch (Machine.sName)
            {
                case "ltswords":
                case "kengo":
                case "kengoa":
                    fg_tilemap.tilemap_set_scrolldx(6, 0);
                    bg_tilemap.tilemap_set_scrolldx(6, 0);
                    break;
            }
        }
        public static void video_update_m72()
        {
            if (video_off != 0)
            {
                Array.Copy(uuB200, Video.bitmapbase[Video.curbitmap], 0x40000);
                return;
            }
            fg_tilemap.tilemap_set_scrollx(0, scrollx1);
            fg_tilemap.tilemap_set_scrolly(0, scrolly1);
            bg_tilemap.tilemap_set_scrollx(0, scrollx2);
            bg_tilemap.tilemap_set_scrolly(0, scrolly2);
            bg_tilemap.tilemap_draw_primask(Video.new_clip, 0x20, 0);
            fg_tilemap.tilemap_draw_primask(Video.new_clip, 0x20, 0);
            m72_draw_sprites(Video.new_clip);
            bg_tilemap.tilemap_draw_primask(Video.new_clip, 0x10, 0);
            fg_tilemap.tilemap_draw_primask(Video.new_clip, 0x10, 0);
        }
        public static void video_eof_m72()
        {

        }
        public static void video_start_m82()
        {
            int i;
            uuB200 = new ushort[0x400 * 0x200];
            Video.new_clip = new RECT();
            spriteram_size = 0x400;
            for (i = 0; i < 0x80000; i++)
            {
                uuB200[i] = 0x200;
            }

        }
    }
}
