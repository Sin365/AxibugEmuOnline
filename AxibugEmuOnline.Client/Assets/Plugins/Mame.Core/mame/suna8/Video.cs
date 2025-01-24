using System;

namespace MAME.Core
{
    public unsafe partial class SunA8
    {
        public static RECT cliprect;
        public static ushort[] uuFF;
        public enum GFXBANK_TYPE
        {
            GFXBANK_TYPE_SPARKMAN = 0,
            GFXBANK_TYPE_BRICKZN,
            GFXBANK_TYPE_STARFIGH
        }
        public static byte suna8_banked_paletteram_r(int offset)
        {
            offset += m_palettebank * 0x200;
            return Generic.paletteram[offset];
        }
        public static byte suna8_banked_spriteram_r(int offset)
        {
            offset += m_spritebank * 0x2000;
            return Generic.spriteram[offset];
        }
        public static void suna8_spriteram_w(int offset, byte data)
        {
            Generic.spriteram[offset] = data;
        }
        public static void suna8_banked_spriteram_w(int offset, byte data)
        {
            offset += m_spritebank * 0x2000;
            Generic.spriteram[offset] = data;
        }
        public static void brickzn_banked_paletteram_w(int offset, byte data)
        {
            byte r, g, b;
            ushort rgb;
            offset += m_palettebank * 0x200;
            Generic.paletteram[offset] = data;
            rgb = (ushort)((Generic.paletteram[offset & ~1] << 8) + Generic.paletteram[offset | 1]);
            r = (byte)((((rgb & (1 << 0xc)) != 0 ? 1 : 0) << 0) |
                    (((rgb & (1 << 0xb)) != 0 ? 1 : 0) << 1) |
                    (((rgb & (1 << 0xe)) != 0 ? 1 : 0) << 2) |
                    (((rgb & (1 << 0xf)) != 0 ? 1 : 0) << 3));
            g = (byte)((((rgb & (1 << 0x8)) != 0 ? 1 : 0) << 0) |
                    (((rgb & (1 << 0x9)) != 0 ? 1 : 0) << 1) |
                    (((rgb & (1 << 0xa)) != 0 ? 1 : 0) << 2) |
                    (((rgb & (1 << 0xd)) != 0 ? 1 : 0) << 3));
            b = (byte)((((rgb & (1 << 0x4)) != 0 ? 1 : 0) << 0) |
                    (((rgb & (1 << 0x3)) != 0 ? 1 : 0) << 1) |
                    (((rgb & (1 << 0x6)) != 0 ? 1 : 0) << 2) |
                    (((rgb & (1 << 0x7)) != 0 ? 1 : 0) << 3));
            Palette.palette_set_callback(offset / 2, Palette.make_rgb(Palette.pal4bit(r), Palette.pal4bit(g), Palette.pal4bit(b)));
        }
        public static void suna8_vh_start_common(int has_text, GFXBANK_TYPE gfxbank_type)
        {
            m_has_text = has_text;
            m_spritebank = 0;
            m_gfxbank = 0;
            m_gfxbank_type = gfxbank_type;
            m_palettebank = 0;
            if (m_has_text == 0)
            {
                Generic.paletteram_set = new byte[0x200 * 2];
                Generic.spriteram_set = new byte[0x2000 * 2 * 2];
                AxiArray.Clear(Generic.spriteram, 0, 0x2000 * 2 * 2);
            }
        }
        public static void video_start_suna8_text()
        {
            suna8_vh_start_common(1, GFXBANK_TYPE.GFXBANK_TYPE_SPARKMAN);
        }
        public static void video_start_suna8_sparkman()
        {
            suna8_vh_start_common(0, GFXBANK_TYPE.GFXBANK_TYPE_SPARKMAN);
        }
        public static void video_start_suna8_brickzn()
        {
            suna8_vh_start_common(0, GFXBANK_TYPE.GFXBANK_TYPE_BRICKZN);
        }
        public static void video_start_suna8_starfigh()
        {
            int i;
            uuFF = new ushort[0x100 * 0x100];
            for (i = 0; i < 0x10000; i++)
            {
                uuFF[i] = 0xff;
            }
            cliprect = new RECT();
            cliprect.min_x = 0;
            cliprect.max_x = 0xff;
            cliprect.min_y = 0x10;
            cliprect.max_y = 0xef;
            suna8_vh_start_common(0, GFXBANK_TYPE.GFXBANK_TYPE_STARFIGH);
        }
        public static void draw_sprites(int start, int end, int which)
        {
            int i, x, y, bank, read_mask;
            int gfxbank, code, code2, color, addr, tile, attr, tile_flipx, tile_flipy, sx, sy;
            int spriteram_offset = which * 0x2000 * 2;
            int mx = 0;
            int max_x = 0xf8;
            int max_y = 0xf8;
            if (m_has_text != 0)
            {
                //fillbitmap(priority_bitmap,0,cliprect);
            }
            for (i = start; i < end; i += 4)
            {
                int srcpg, srcx, srcy, dimx, dimy, tx, ty;
                int colorbank = 0, flipx, flipy, multisprite;
                y = Generic.spriteram[spriteram_offset + i + 0];
                code = Generic.spriteram[spriteram_offset + i + 1];
                x = Generic.spriteram[spriteram_offset + i + 2];
                bank = Generic.spriteram[spriteram_offset + i + 3];
                read_mask = 0;
                if (m_has_text != 0)
                {
                    read_mask = 1;
                    if ((bank & 0xc0) == 0xc0)
                    {
                        int text_list = (i - start) & 0x20;
                        int text_start = text_list != 0 ? 0x19c0 : 0x1980;
                        int write_mask = (text_list == 0) ? 1 : 0;
                        //draw_text_sprites(machine, bitmap, cliprect, text_start, text_start + 0x80, y, write_mask);
                        continue;
                    }
                    flipx = 0;
                    flipy = 0;
                    gfxbank = bank & 0x3f;
                    switch (code & 0x80)
                    {
                        case 0x80:
                            dimx = 2;
                            dimy = 32;
                            srcx = (code & 0xf) * 2;
                            srcy = 0;
                            srcpg = (code >> 4) & 3;
                            break;
                        case 0x00:
                        default:
                            dimx = 2;
                            dimy = 2;
                            srcx = (code & 0xf) * 2;
                            srcy = ((code >> 5) & 0x3) * 8 + 6;
                            srcpg = (code >> 4) & 1;
                            break;
                    }
                    multisprite = ((code & 0x80) != 0 && (code & 0x40) != 0) ? 1 : 0;
                }
                else
                {
                    switch (code & 0xc0)
                    {
                        case 0xc0:
                            dimx = 4;
                            dimy = 32;
                            srcx = (code & 0xe) * 2;
                            srcy = 0;
                            flipx = (code & 0x1);
                            flipy = 0;
                            gfxbank = bank & 0x1f;
                            srcpg = (code >> 4) & 3;
                            break;
                        case 0x80:
                            dimx = 2;
                            dimy = 32;
                            srcx = (code & 0xf) * 2;
                            srcy = 0;
                            flipx = 0;
                            flipy = 0;
                            gfxbank = bank & 0x1f;
                            srcpg = (code >> 4) & 3;
                            break;
                        case 0x40:
                            dimx = 4;
                            dimy = 4;
                            srcx = (code & 0xe) * 2;
                            flipx = code & 0x01;
                            flipy = bank & 0x10;
                            srcy = (((bank & 0x80) >> 4) + (bank & 0x04) + ((~bank >> 4) & 2)) * 2;
                            srcpg = ((code >> 4) & 3) + 4;
                            gfxbank = (bank & 0x3);
                            switch (m_gfxbank_type)
                            {
                                case GFXBANK_TYPE.GFXBANK_TYPE_SPARKMAN:
                                    break;
                                case GFXBANK_TYPE.GFXBANK_TYPE_BRICKZN:
                                    gfxbank += 4;
                                    break;
                                case GFXBANK_TYPE.GFXBANK_TYPE_STARFIGH:
                                    if (gfxbank == 3)
                                        gfxbank += m_gfxbank;
                                    break;
                            }
                            colorbank = (bank & 8) >> 3;
                            break;
                        case 0x00:
                        default:
                            dimx = 2;
                            dimy = 2;
                            srcx = (code & 0xf) * 2;
                            flipx = 0;
                            flipy = 0;
                            srcy = (((bank & 0x80) >> 4) + (bank & 0x04) + ((~bank >> 4) & 3)) * 2;
                            srcpg = (code >> 4) & 3;
                            gfxbank = bank & 0x03;
                            switch (m_gfxbank_type)
                            {
                                case GFXBANK_TYPE.GFXBANK_TYPE_STARFIGH:
                                    if (gfxbank == 3)
                                    {
                                        gfxbank += m_gfxbank;
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                    }
                    multisprite = ((code & 0x80) != 0 && (bank & 0x80) != 0) ? 1 : 0;
                }
                x = x - ((bank & 0x40) != 0 ? 0x100 : 0);
                y = (0x100 - y - dimy * 8) & 0xff;
                if (multisprite != 0)
                {
                    mx += dimx * 8;
                    x = mx;
                }
                else
                {
                    mx = x;
                }
                gfxbank *= 0x400;
                for (ty = 0; ty < dimy; ty++)
                {
                    for (tx = 0; tx < dimx; tx++)
                    {
                        addr = (srcpg * 0x20 * 0x20) + ((srcx + (flipx != 0 ? dimx - tx - 1 : tx)) & 0x1f) * 0x20 + ((srcy + (flipy != 0 ? dimy - ty - 1 : ty)) & 0x1f);
                        tile = Generic.spriteram[addr * 2 + 0];
                        attr = Generic.spriteram[addr * 2 + 1];
                        tile_flipx = attr & 0x40;
                        tile_flipy = attr & 0x80;
                        sx = x + tx * 8;
                        sy = (y + ty * 8) & 0xff;
                        if (flipx != 0)
                        {
                            tile_flipx = (tile_flipx == 0 ? 1 : 0);
                        }
                        if (flipy != 0)
                        {
                            tile_flipy = (tile_flipy == 0 ? 1 : 0);
                        }
                        if (Generic.flip_screen_get() != 0)
                        {
                            sx = max_x - sx;
                            tile_flipx = (tile_flipx == 0 ? 1 : 0);
                            sy = max_y - sy;
                            tile_flipy = (tile_flipy == 0 ? 1 : 0);
                        }
                        code2 = tile + (attr & 0x3) * 0x100 + gfxbank;
                        color = (((attr >> 2) & 0xf) ^ colorbank) + 0x10 * m_palettebank;
                        if (read_mask != 0)
                        {
                            //((mygfx_element*)(m_gfxdecode->gfx(which)))->prio_mask_transpen(bitmap, cliprect,	code, color, tile_flipx, tile_flipy, sx, sy, screen.priority(), 0xf);
                        }
                        else
                        {
                            Drawgfx.common_drawgfx_starfigh(gfx1rom, code2, color, tile_flipx, tile_flipy, sx, sy, cliprect);
                            //m_gfxdecode->gfx(which)->transpen(bitmap, cliprect,	code, color, tile_flipx, tile_flipy, sx, sy, 0xf);
                        }
                    }
                }
            }
        }
        public static void video_update_suna8()
        {
            Array.Copy(uuFF, Video.bitmapbase[Video.curbitmap], 0x10000);
            draw_sprites(0x1d00, 0x2000, 0);
        }
        public static void video_eof_suna8()
        {

        }
    }
}
