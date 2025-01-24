namespace MAME.Core
{
    public unsafe partial class Dataeast
    {
        public static void palette_init_pcktgal(byte* color_prom)
        {
            int i;
            for (i = 0; i < 0x200; i++)
            {
                int bit0, bit1, bit2, bit3, r, g, b;
                bit0 = (color_prom[i] >> 0) & 0x01;
                bit1 = (color_prom[i] >> 1) & 0x01;
                bit2 = (color_prom[i] >> 2) & 0x01;
                bit3 = (color_prom[i] >> 3) & 0x01;
                r = 0x0e * bit0 + 0x1f * bit1 + 0x43 * bit2 + 0x8f * bit3;
                bit0 = (color_prom[i] >> 4) & 0x01;
                bit1 = (color_prom[i] >> 5) & 0x01;
                bit2 = (color_prom[i] >> 6) & 0x01;
                bit3 = (color_prom[i] >> 7) & 0x01;
                g = 0x0e * bit0 + 0x1f * bit1 + 0x43 * bit2 + 0x8f * bit3;
                bit0 = (color_prom[i + 0x200] >> 0) & 0x01;
                bit1 = (color_prom[i + 0x200] >> 1) & 0x01;
                bit2 = (color_prom[i + 0x200] >> 2) & 0x01;
                bit3 = (color_prom[i + 0x200] >> 3) & 0x01;
                b = 0x0e * bit0 + 0x1f * bit1 + 0x43 * bit2 + 0x8f * bit3;
                Palette.palette_set_callback(i, Palette.make_rgb(r, g, b));
            }
        }
        public static void pcktgal_videoram_w(int offset, byte data)
        {
            int row, col;
            Generic.videoram[offset] = data;
            row = (offset / 2) / 0x20;
            col = (offset / 2) % 0x20;
            bg_tilemap.tilemap_mark_tile_dirty(row, col);
        }
        public static void pcktgal_flipscreen_w(byte data)
        {
            if (Generic.flip_screen_get() != (data & 0x80))
            {
                Generic.flip_screen_set(data & 0x80);
                bg_tilemap.all_tiles_dirty = true;
            }
        }
        public static void draw_sprites(RECT cliprect)
        {
            int offs;
            for (offs = 0; offs < 0x200; offs += 4)
            {
                if (Generic.spriteram[offs] != 0xf8)
                {
                    int sx, sy, flipx, flipy;
                    sx = 240 - Generic.spriteram[offs + 2];
                    sy = 240 - Generic.spriteram[offs];
                    flipx = Generic.spriteram[offs + 1] & 0x04;
                    flipy = Generic.spriteram[offs + 1] & 0x02;
                    if (Generic.flip_screen_get() != 0)
                    {
                        sx = 240 - sx;
                        sy = 240 - sy;
                        if (flipx != 0)
                        {
                            flipx = 0;
                        }
                        else
                        {
                            flipx = 1;
                        }
                        if (flipy != 0)
                        {
                            flipy = 0;
                        }
                        else
                        {
                            flipy = 1;
                        }
                    }
                    Drawgfx.common_drawgfx_pcktgal(gfx2rom, 16, 16, 16, 0x400, Generic.spriteram[offs + 3] + ((Generic.spriteram[offs + 1] & 1) << 8), (Generic.spriteram[offs + 1] & 0x70) >> 4, flipx, flipy, sx, sy, cliprect);
                }
            }
        }
        public static void video_update_pcktgal()
        {
            bg_tilemap.tilemap_draw_primask(Video.screenstate.visarea, 0x10, 0);
            draw_sprites(Video.screenstate.visarea);
        }
        public static void video_eof_pcktgal()
        {

        }
    }
}
