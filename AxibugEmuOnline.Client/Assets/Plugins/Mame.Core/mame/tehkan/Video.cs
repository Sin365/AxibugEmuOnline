namespace MAME.Core
{
    public unsafe partial class Tehkan
    {
        public static byte[] pbaction_videoram2, pbaction_colorram2;
        public static int scroll;
        public static RECT cliprect;
        public static void pbaction_videoram_w(int offset, byte data)
        {
            int row, col;
            Generic.videoram[offset] = data;
            row = offset / 0x20;
            col = offset % 0x20;
            bg_tilemap.tilemap_mark_tile_dirty(row, col);
        }
        public static void pbaction_colorram_w(int offset, byte data)
        {
            int row, col;
            Generic.colorram[offset] = data;
            row = offset / 0x20;
            col = offset % 0x20;
            bg_tilemap.tilemap_mark_tile_dirty(row, col);
        }
        public static void pbaction_videoram2_w(int offset, byte data)
        {
            int row, col;
            pbaction_videoram2[offset] = data;
            row = offset / 0x20;
            col = offset % 0x20;
            fg_tilemap.tilemap_mark_tile_dirty(row, col);
        }
        public static void pbaction_colorram2_w(int offset, byte data)
        {
            int row, col;
            pbaction_colorram2[offset] = data;
            row = offset / 0x20;
            col = offset % 0x20;
            fg_tilemap.tilemap_mark_tile_dirty(row, col);
        }
        public static void pbaction_scroll_w(byte data)
        {
            scroll = data - 3;
            if (Generic.flip_screen_get() != 0)
            {
                scroll = -scroll;
            }
            bg_tilemap.tilemap_set_scrollx(0, scroll);
            fg_tilemap.tilemap_set_scrollx(0, scroll);
        }
        public static void pbaction_flipscreen_w(byte data)
        {
            Generic.flip_screen_set(data & 0x01);
        }
        public static void video_start_pbaction()
        {
            cliprect = new RECT();
            cliprect.min_x = 0;
            cliprect.max_x = 0xff;
            cliprect.min_y = 0x10;
            cliprect.max_y = 0xef;
        }
        public static void draw_sprites(RECT cliprect)
        {
            int offs;
            for (offs = 0x80 - 4; offs >= 0; offs -= 4)
            {
                int sx, sy, flipx, flipy;
                if (offs > 0 && (Generic.spriteram[offs - 4] & 0x80) != 0)
                {
                    continue;
                }
                sx = Generic.spriteram[offs + 3];
                if ((Generic.spriteram[offs] & 0x80) != 0)
                {
                    sy = 225 - Generic.spriteram[offs + 2];
                }
                else
                {
                    sy = 241 - Generic.spriteram[offs + 2];
                }
                flipx = Generic.spriteram[offs + 1] & 0x40;
                flipy = Generic.spriteram[offs + 1] & 0x80;
                if (Generic.flip_screen_get() != 0)
                {
                    if ((Generic.spriteram[offs] & 0x80) != 0)
                    {
                        sx = 224 - sx;
                        sy = 225 - sy;
                    }
                    else
                    {
                        sx = 240 - sx;
                        sy = 241 - sy;
                    }
                    flipx = (flipx == 0 ? 1 : 0);
                    flipy = (flipy == 0 ? 1 : 0);
                }
                if ((Generic.spriteram[offs] & 0x80) != 0)
                {
                    Drawgfx.common_drawgfx_pbaction(gfx32rom, 32, 32, 32, 0x20, Generic.spriteram[offs], Generic.spriteram[offs + 1] & 0x0f, flipx, flipy, sx + (Generic.flip_screen_get() != 0 ? scroll : -scroll), sy, cliprect);
                }
                else
                {
                    Drawgfx.common_drawgfx_pbaction(gfx3rom, 16, 16, 16, 0x80, Generic.spriteram[offs], Generic.spriteram[offs + 1] & 0x0f, flipx, flipy, sx + (Generic.flip_screen_get() != 0 ? scroll : -scroll), sy, cliprect);
                }
            }
        }
        public static void video_update_pbaction()
        {
            bg_tilemap.tilemap_draw_primask(cliprect, 0x10, 0);
            draw_sprites(cliprect);
            fg_tilemap.tilemap_draw_primask(cliprect, 0x10, 0);
        }
        public static void video_eof_pbaction()
        {

        }
    }
}
