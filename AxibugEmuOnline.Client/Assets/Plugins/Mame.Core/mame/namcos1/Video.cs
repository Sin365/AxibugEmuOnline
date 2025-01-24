using System;

namespace MAME.Core
{
    public partial class Namcos1
    {
        public static byte[] namcos1_videoram;
        public static byte[] namcos1_cus116;
        public static byte[] namcos1_spriteram;
        public static byte[] namcos1_playfield_control;
        private static ushort[] uu2000;
        public static int flip_screen_x, flip_screen_y;
        public static int copy_sprites;
        public static void video_start_namcos1()
        {
            int i;
            namcos1_videoram = new byte[0x8000];
            namcos1_cus116 = new byte[0x10];
            namcos1_spriteram = new byte[0x1000];
            namcos1_playfield_control = new byte[0x20];
            Array.Clear(namcos1_videoram, 0, 0x8000);
            Array.Clear(namcos1_paletteram, 0, 0x8000);
            Array.Clear(namcos1_cus116, 0, 0x10);
            Array.Clear(namcos1_playfield_control, 0, 0x20);
            uu2000 = new ushort[0x200 * 0x200];
            for (i = 0; i < 0x40000; i++)
            {
                uu2000[i] = 0x2000;
            }
            ttmap[4].tilemap_set_scrolldx(73, 512 - 73);
            ttmap[5].tilemap_set_scrolldx(73, 512 - 73);
            ttmap[4].tilemap_set_scrolldy(0x10, 0x110);
            ttmap[5].tilemap_set_scrolldy(0x10, 0x110);
            for (i = 0; i < 0x2000; i++)
            {
                Palette.palette_entry_set_color1(i, Palette.make_rgb(0, 0, 0));
            }
            copy_sprites = 0;
        }
        public static byte namcos1_videoram_r(int offset)
        {
            return namcos1_videoram[offset];
        }
        public static void namcos1_videoram_w(int offset, byte data)
        {
            namcos1_videoram[offset] = data;
            if (offset < 0x7000)
            {
                int layer = offset >> 13;
                int num = (offset & 0x1fff) >> 1;
                int row, col;
                //row = num / Tilemap.ttmap[layer].cols;
                //col = num % Tilemap.ttmap[layer].cols;
                //Tilemap.tilemap_mark_tile_dirty(Tilemap.ttmap[layer], row, col);
                //row = num / Tmap.ttmap[layer].cols;
                //col = num % Tmap.ttmap[layer].cols;
                ttmap[layer].get_row_col(num, out row, out col);
                ttmap[layer].tilemap_mark_tile_dirty(row, col);
            }
            else
            {
                int layer = (offset >> 11 & 1) + 4;
                int num = ((offset & 0x7ff) - 0x10) >> 1;
                int row, col;
                if (num >= 0 && num < 0x3f0)
                {
                    //row = num / Tilemap.ttmap[layer].cols;
                    //col = num % Tilemap.ttmap[layer].cols;
                    //Tilemap.tilemap_mark_tile_dirty(Tilemap.ttmap[layer], row, col);
                    //row = num / Tmap.ttmap[layer].cols;
                    //col = num % Tmap.ttmap[layer].cols;
                    ttmap[layer].get_row_col(num, out row, out col);
                    ttmap[layer].tilemap_mark_tile_dirty(row, col);
                }
            }
        }
        public static void namcos1_paletteram_w(int offset, byte data)
        {
            if (namcos1_paletteram[offset] == data)
                return;
            if ((offset & 0x1800) != 0x1800)
            {
                int r, g, b;
                int color = ((offset & 0x6000) >> 2) | (offset & 0x7ff);
                namcos1_paletteram[offset] = data;
                offset &= ~0x1800;
                r = namcos1_paletteram[offset];
                g = namcos1_paletteram[offset + 0x0800];
                b = namcos1_paletteram[offset + 0x1000];
                Palette.palette_entry_set_color1(color, Palette.make_rgb(r, g, b));
            }
            else
            {
                int i, j;
                namcos1_cus116[offset & 0x0f] = data;
                for (i = 0x1800; i < 0x8000; i += 0x2000)
                {
                    offset = (offset & 0x0f) | i;
                    for (j = 0; j < 0x80; j++, offset += 0x10)
                    {
                        namcos1_paletteram[offset] = data;
                    }
                }
            }
        }
        public static byte namcos1_spriteram_r(int offset)
        {
            if (offset < 0x1000)
                return namcos1_spriteram[offset];
            else
                return namcos1_playfield_control[offset & 0x1f];
        }
        public static void namcos1_spriteram_w(int offset, byte data)
        {
            if (offset < 0x1000)
            {
                namcos1_spriteram[offset] = data;
                if (offset == 0x0ff2)
                {
                    copy_sprites = 1;
                }
            }
            else
            {
                namcos1_playfield_control[offset & 0x1f] = data;
            }
        }
        public static void draw_sprites(int iBitmap, RECT cliprect)
        {
            int source_offset;
            int sprite_xoffs = namcos1_spriteram[0x800 + 0x07f5] + ((namcos1_spriteram[0x800 + 0x07f4] & 1) << 8);
            int sprite_yoffs = namcos1_spriteram[0x800 + 0x07f7];
            for (source_offset = 0xfe0; source_offset >= 0x800; source_offset -= 0x10)
            {
                int[] sprite_size = new int[] { 16, 8, 32, 4 };
                int attr1 = namcos1_spriteram[source_offset + 10];
                int attr2 = namcos1_spriteram[source_offset + 14];
                int color = namcos1_spriteram[source_offset + 12];
                int flipx = (attr1 & 0x20) >> 5;
                int flipy = (attr2 & 0x01);
                int sizex = sprite_size[(attr1 & 0xc0) >> 6];
                int sizey = sprite_size[(attr2 & 0x06) >> 1];
                int tx = (attr1 & 0x18) & (~(sizex - 1));
                int ty = (attr2 & 0x18) & (~(sizey - 1));
                int sx = namcos1_spriteram[source_offset + 13] + ((color & 0x01) << 8);
                int sy = -namcos1_spriteram[source_offset + 15] - sizey;
                int sprite = namcos1_spriteram[source_offset + 11];
                int sprite_bank = attr1 & 7;
                int priority = (namcos1_spriteram[source_offset + 14] & 0xe0) >> 5;
                namcos1_pri = priority;
                sprite += sprite_bank * 256;
                color = color >> 1;
                sx += sprite_xoffs;
                sy -= sprite_yoffs;
                if (Video.flip_screen_get())
                {
                    sx = -sx - sizex;
                    sy = -sy - sizey;
                    flipx ^= 1;
                    flipy ^= 1;
                }
                sy++;
                Drawgfx.common_drawgfx_na(sizex, sizey, tx, ty, sprite, color, flipx, flipy, sx & 0x1ff, ((sy + 16) & 0xff) - 16, cliprect);
            }
        }
        public static void video_update_namcos1()
        {
            int i, j, scrollx, scrolly;
            byte priority;
            RECT new_clip = new RECT();
            new_clip.min_x = 0x49;
            new_clip.max_x = 0x168;
            new_clip.min_y = 0x10;
            new_clip.max_y = 0xef;
            Video.flip_screen_set_no_update((namcos1_spriteram[0x800 + 0x07f6] & 1) != 0);
            tilemap_set_flip(Video.flip_screen_get() ? (byte)(Tilemap.TILEMAP_FLIPY | Tilemap.TILEMAP_FLIPX) : (byte)0);
            Array.Copy(uu2000, Video.bitmapbase[Video.curbitmap], 0x40000);
            i = ((namcos1_cus116[0] << 8) | namcos1_cus116[1]) - 1;
            if (new_clip.min_x < i)
                new_clip.min_x = i;
            i = ((namcos1_cus116[2] << 8) | namcos1_cus116[3]) - 1 - 1;
            if (new_clip.max_x > i)
                new_clip.max_x = i;
            i = ((namcos1_cus116[4] << 8) | namcos1_cus116[5]) - 0x11;
            if (new_clip.min_y < i)
                new_clip.min_y = i;
            i = ((namcos1_cus116[6] << 8) | namcos1_cus116[7]) - 0x11 - 1;
            if (new_clip.max_y > i)
                new_clip.max_y = i;
            if (new_clip.max_x < new_clip.min_x || new_clip.max_y < new_clip.min_y)
                return;
            for (i = 0; i < 6; i++)
            {
                ttmap[i].tilemap_set_palette_offset((namcos1_playfield_control[i + 24] & 7) * 256);
            }
            for (i = 0; i < 4; i++)
            {
                int[] disp_x = new int[] { 25, 27, 28, 29 };
                j = i << 2;
                scrollx = (namcos1_playfield_control[j + 1] + (namcos1_playfield_control[j + 0] << 8)) - disp_x[i];
                scrolly = (namcos1_playfield_control[j + 3] + (namcos1_playfield_control[j + 2] << 8)) + 8;
                if (Video.flip_screen_get())
                {
                    scrollx = -scrollx;
                    scrolly = -scrolly;
                }
                ttmap[i].tilemap_set_scrollx(0, scrollx);
                ttmap[i].tilemap_set_scrolly(0, scrolly);
            }
            Array.Clear(Tilemap.priority_bitmap, 0, 0x40000);
            for (i = 0; i < 0x200; i++)
            {
                for (j = 0; j < 0x200; j++)
                {
                    Tilemap.priority_bitmap[i, j] = 0;
                }
            }
            for (priority = 0; priority < 8; priority++)
            {
                for (i = 0; i < 6; i++)
                {
                    if (namcos1_playfield_control[16 + i] == priority)
                    {
                        ttmap[i].tilemap_draw_primask(new_clip, 0x10, priority);
                    }
                }
            }
            draw_sprites(Video.curbitmap, new_clip);
        }
        public static void video_eof_namcos1()
        {
            if (copy_sprites != 0)
            {
                int i, j;
                for (i = 0; i < 0x800; i += 16)
                {
                    for (j = 10; j < 16; j++)
                    {
                        namcos1_spriteram[0x800 + i + j] = namcos1_spriteram[0x800 + i + j - 6];
                    }
                }
                copy_sprites = 0;
            }
        }
    }
}
