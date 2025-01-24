using System;
using System.Runtime.InteropServices;

namespace MAME.Core
{
    public unsafe partial class Capcom
    {
        public static Tmap bg_tilemap, fg_tilemap, tx_tilemap;
        public static int bg_scrollx, fg_scrollx;
        public static int sf_active;
        //public static ushort[] uuB0000;

        #region //指针化 uuB0000
        static ushort[] uuB0000_src;
        static GCHandle uuB0000_handle;
        public static ushort* uuB0000;
        public static int uuB0000Length;
        public static bool uuB0000_IsNull => uuB0000 == null;
        public static ushort[] uuB0000_set
        {
            set
            {
                uuB0000_handle.ReleaseGCHandle();
                if (value == null)
                    return;
                uuB0000_src = value;
                uuB0000Length = value.Length;
                uuB0000_src.GetObjectPtr(ref uuB0000_handle, ref uuB0000);
            }
        }
        #endregion

        public static void video_start_sf()
        {
            int i;
            sf_active = 0;
            uuB0000_set = new ushort[0x200 * 0x100];
            for (i = 0; i < 0x20000; i++)
            {
                uuB0000[i] = 0x0;
            }
        }
        public static void sf_videoram_w(int offset, ushort data)
        {
            int row, col;
            sf_videoram[offset] = data;
            row = offset / 64;
            col = offset % 64;
            tx_tilemap.tilemap_mark_tile_dirty(row, col);
        }
        public static void sf_videoram_w1(int offset, byte data)
        {
            int row, col;
            sf_videoram[offset] = (ushort)((data << 8) | (sf_videoram[offset] & 0xff));
            row = offset / 64;
            col = offset % 64;
            tx_tilemap.tilemap_mark_tile_dirty(row, col);
        }
        public static void sf_videoram_w2(int offset, byte data)
        {
            int row, col;
            sf_videoram[offset] = (ushort)((sf_videoram[offset] & 0xff00) | data);
            row = offset / 64;
            col = offset % 64;
            tx_tilemap.tilemap_mark_tile_dirty(row, col);
        }
        public static void sf_bg_scroll_w(ushort data)
        {
            bg_scrollx = data;
            bg_tilemap.tilemap_set_scrollx(0, bg_scrollx);
        }
        public static void sf_bg_scroll_w1(byte data)
        {
            bg_scrollx = (data << 8) | (bg_scrollx & 0xff);
            bg_tilemap.tilemap_set_scrollx(0, bg_scrollx);
        }
        public static void sf_bg_scroll_w2(byte data)
        {
            bg_scrollx = (bg_scrollx & 0xff00) | data;
            bg_tilemap.tilemap_set_scrollx(0, bg_scrollx);
        }
        public static void sf_fg_scroll_w(ushort data)
        {
            fg_scrollx = data;
            fg_tilemap.tilemap_set_scrollx(0, fg_scrollx);
        }
        public static void sf_fg_scroll_w1(byte data)
        {
            fg_scrollx = (data << 8) | (fg_scrollx & 0xff);
            fg_tilemap.tilemap_set_scrollx(0, fg_scrollx);
        }
        public static void sf_fg_scroll_w2(byte data)
        {
            fg_scrollx = (fg_scrollx & 0xff00) | data;
            fg_tilemap.tilemap_set_scrollx(0, fg_scrollx);
        }
        public static void sf_gfxctrl_w(ushort data)
        {
            sf_active = data & 0xff;
            Generic.flip_screen_set(data & 0x04);
            tx_tilemap.enable = (data & 0x08) != 0;
            bg_tilemap.enable = (data & 0x20) != 0;
            fg_tilemap.enable = (data & 0x40) != 0;
        }
        public static void sf_gfxctrl_w2(byte data)
        {
            sf_active = data & 0xff;
            Generic.flip_screen_set(data & 0x04);
            tx_tilemap.enable = (data & 0x08) != 0;
            bg_tilemap.enable = (data & 0x20) != 0;
            fg_tilemap.enable = (data & 0x40) != 0;
        }
        public static int sf_invert(int nb)
        {
            int[] delta = new int[4] { 0x00, 0x18, 0x18, 0x00 };
            return nb ^ delta[(nb >> 3) & 3];
        }
        public static void draw_sprites(RECT cliprect)
        {
            int offs;
            for (offs = 0x1000 - 0x20; offs >= 0; offs -= 0x20)
            {
                int c = sf_objectram[offs];
                int attr = sf_objectram[offs + 1];
                int sy = sf_objectram[offs + 2];
                int sx = sf_objectram[offs + 3];
                int color = attr & 0x000f;
                int flipx = attr & 0x0100;
                int flipy = attr & 0x0200;
                if ((attr & 0x400) != 0)
                {
                    int c1, c2, c3, c4, t;
                    if (Generic.flip_screen_get() != 0)
                    {
                        sx = 480 - sx;
                        sy = 224 - sy;
                        flipx = (flipx == 0) ? 1 : 0;
                        flipy = (flipy == 0) ? 1 : 0;
                    }
                    c1 = c;
                    c2 = c + 1;
                    c3 = c + 16;
                    c4 = c + 17;
                    if (flipx != 0)
                    {
                        t = c1; c1 = c2; c2 = t;
                        t = c3; c3 = c4; c4 = t;
                    }
                    if (flipy != 0)
                    {
                        t = c1; c1 = c3; c3 = t;
                        t = c2; c2 = c4; c4 = t;
                    }
                    Drawgfx.common_drawgfx_sf(gfx3rom, sf_invert(c1), color, flipx, flipy, sx, sy, cliprect);
                    Drawgfx.common_drawgfx_sf(gfx3rom, sf_invert(c2), color, flipx, flipy, sx + 16, sy, cliprect);
                    Drawgfx.common_drawgfx_sf(gfx3rom, sf_invert(c3), color, flipx, flipy, sx, sy + 16, cliprect);
                    Drawgfx.common_drawgfx_sf(gfx3rom, sf_invert(c4), color, flipx, flipy, sx + 16, sy + 16, cliprect);
                }
                else
                {
                    if (Generic.flip_screen_get() != 0)
                    {
                        sx = 496 - sx;
                        sy = 240 - sy;
                        flipx = (flipx == 0) ? 1 : 0;
                        flipy = (flipy == 0) ? 1 : 0;
                    }
                    Drawgfx.common_drawgfx_sf(gfx3rom, sf_invert(c), color, flipx, flipy, sx, sy, cliprect);
                }
            }
        }
        public static void video_update_sf()
        {
            if ((sf_active & 0x20) != 0)
            {
                bg_tilemap.tilemap_draw_primask(Video.screenstate.visarea, 0x10, 0);
            }
            else
            {
                AxiArray.Copy(uuB0000, Video.bitmapbase_Ptrs[Video.curbitmap], 0x20000);
            }
            fg_tilemap.tilemap_draw_primask(Video.screenstate.visarea, 0x10, 0);
            if ((sf_active & 0x80) != 0)
            {
                draw_sprites(Video.screenstate.visarea);
            }
            tx_tilemap.tilemap_draw_primask(Video.screenstate.visarea, 0x10, 0);
        }
        public static void video_eof()
        {

        }
    }
}
