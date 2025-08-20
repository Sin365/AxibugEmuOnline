using System;
using System.Runtime.InteropServices;

namespace MAME.Core
{
    public unsafe partial class Capcom
    {
        //public static byte[] gng_fgvideoram, gng_bgvideoram;
        //public static byte[] scrollx, scrolly;

        #region //指针化 gng_fgvideoram
        static byte[] gng_fgvideoram_src;
        static GCHandle gng_fgvideoram_handle;
        public static byte* gng_fgvideoram;
        public static int gng_fgvideoramLength;
        public static bool gng_fgvideoram_IsNull => gng_fgvideoram == null;
        public static byte[] gng_fgvideoram_set
        {
            set
            {
                gng_fgvideoram_handle.ReleaseGCHandle();
                gng_fgvideoram_src = value;
                gng_fgvideoramLength = value.Length;
                gng_fgvideoram_src.GetObjectPtr(ref gng_fgvideoram_handle, ref gng_fgvideoram);
            }
        }
        #endregion

        #region //指针化 gng_bgvideoram
        static byte[] gng_bgvideoram_src;
        static GCHandle gng_bgvideoram_handle;
        public static byte* gng_bgvideoram;
        public static int gng_bgvideoramLength;
        public static bool gng_bgvideoram_IsNull => gng_bgvideoram == null;
        public static byte[] gng_bgvideoram_set
        {
            set
            {
                gng_bgvideoram_handle.ReleaseGCHandle();
                gng_bgvideoram_src = value;
                gng_bgvideoramLength = value.Length;
                gng_bgvideoram_src.GetObjectPtr(ref gng_bgvideoram_handle, ref gng_bgvideoram);
            }
        }
        #endregion

        #region //指针化 scrollx
        static byte[] scrollx_src;
        static GCHandle scrollx_handle;
        public static byte* scrollx;
        public static int scrollxLength;
        public static bool scrollx_IsNull => scrollx == null;
        public static byte[] scrollx_set
        {
            set
            {
                scrollx_handle.ReleaseGCHandle();
                if (value == null)
                    return;
                scrollx_src = value;
                scrollxLength = value.Length;
                scrollx_src.GetObjectPtr(ref scrollx_handle, ref scrollx);
            }
        }
        #endregion

        #region //指针化 scrolly
        static byte[] scrolly_src;
        static GCHandle scrolly_handle;
        public static byte* scrolly;
        public static int scrollyLength;
        public static bool scrolly_IsNull => scrolly == null;
        public static byte[] scrolly_set
        {
            set
            {
                scrolly_handle.ReleaseGCHandle();
                if (value == null)
                    return;
                scrolly_src = value;
                scrollyLength = value.Length;
                scrolly_src.GetObjectPtr(ref scrolly_handle, ref scrolly);
            }
        }
        #endregion

        public static void gng_bankswitch_w(byte data)
        {
            if (data == 4)
            {
                basebankmain = 0x4000;
            }
            else
            {
                basebankmain = 0x10000 + 0x2000 * (data & 0x03);
            }
        }
        public static void gng_coin_counter_w(int offset, byte data)
        {
            Generic.coin_counter_w(offset, data);
        }
        public static void video_start_gng()
        {
            gng_fgvideoram_set = new byte[0x800];
            gng_bgvideoram_set = new byte[0x800];
            scrollx_set = new byte[2];
            scrolly_set = new byte[2];
        }
        public static void gng_fgvideoram_w(int offset, byte data)
        {
            gng_fgvideoram[offset] = data;
            int row, col;
            row = (offset & 0x3ff) / 0x20;
            col = (offset & 0x3ff) % 0x20;
            fg_tilemap.tilemap_mark_tile_dirty(row, col);
        }
        public static void gng_bgvideoram_w(int offset, byte data)
        {
            gng_bgvideoram[offset] = data;
            int row, col;
            row = (offset & 0x3ff) % 0x20;
            col = (offset & 0x3ff) / 0x20;
            bg_tilemap.tilemap_mark_tile_dirty(row, col);
        }
        public static void gng_bgscrollx_w(int offset, byte data)
        {
            scrollx[offset] = data;
            bg_tilemap.tilemap_set_scrollx(0, scrollx[0] + 256 * scrollx[1]);
        }
        public static void gng_bgscrolly_w(int offset, byte data)
        {
            scrolly[offset] = data;
            bg_tilemap.tilemap_set_scrolly(0, scrolly[0] + 256 * scrolly[1]);
        }
        public static void gng_flipscreen_w(byte data)
        {
            Generic.flip_screen_set(~data & 1);
        }
        public static void draw_sprites_gng(RECT cliprect)
        {
            int offs;
            for (offs = 0x200 - 4; offs >= 0; offs -= 4)
            {
                byte attributes = Generic.buffered_spriteram[offs + 1];
                int sx = Generic.buffered_spriteram[offs + 3] - 0x100 * (attributes & 0x01);
                int sy = Generic.buffered_spriteram[offs + 2];
                int flipx = attributes & 0x04;
                int flipy = attributes & 0x08;
                if (Generic.flip_screen_get() != 0)
                {
                    sx = 240 - sx;
                    sy = 240 - sy;
                    flipx = (flipx == 0 ? 1 : 0);
                    flipy = (flipy == 0 ? 1 : 0);
                }
                Drawgfx.common_drawgfx_gng(gfx3rom, Generic.buffered_spriteram[offs] + ((attributes << 2) & 0x300), (attributes >> 4) & 3, flipx, flipy, sx, sy, cliprect);
            }
        }
        public static void video_update_gng()
        {
            bg_tilemap.tilemap_draw_primask(Video.screenstate.visarea, 0x20, 0);
            draw_sprites_gng(Video.screenstate.visarea);
            bg_tilemap.tilemap_draw_primask(Video.screenstate.visarea, 0x10, 0);
            fg_tilemap.tilemap_draw_primask(Video.screenstate.visarea, 0x10, 0);
        }
        public static void video_eof_gng()
        {
            Generic.buffer_spriteram_w();
        }
    }
}
