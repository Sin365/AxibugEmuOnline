using System;

namespace MAME.Core
{
    public unsafe partial class PGM
    {
        public static ushort[] pgm_spritebufferram; // buffered spriteram
        public static ushort[] sprite_temp_render;
        private static ushort[] uu900;
        private static int pgm_sprite_source_offset;
        private static void pgm_prepare_sprite(int wide, int high, int palt, int boffset)
        {
            int bdatasize = sprmaskrom.Length - 1;
            int adatasize = pgm_sprite_a_region.Length - 1;
            int xcnt, ycnt, aoffset, x;
            ushort msk;
            aoffset = (sprmaskrom[(boffset + 3) & bdatasize] << 24) | (sprmaskrom[(boffset + 2) & bdatasize] << 16) | (sprmaskrom[(boffset + 1) & bdatasize] << 8) | (sprmaskrom[(boffset + 0) & bdatasize] << 0);
            aoffset = aoffset >> 2;
            aoffset *= 3;
            boffset += 4;
            for (ycnt = 0; ycnt < high; ycnt++)
            {
                for (xcnt = 0; xcnt < wide; xcnt++)
                {
                    msk = (ushort)((sprmaskrom[(boffset + 1) & bdatasize] << 8) | (sprmaskrom[(boffset + 0) & bdatasize] << 0));
                    for (x = 0; x < 16; x++)
                    {
                        if ((msk & 0x0001) == 0)
                        {
                            sprite_temp_render[(ycnt * (wide * 16)) + (xcnt * 16 + x)] = (ushort)(pgm_sprite_a_region[aoffset & adatasize] + palt * 32);
                            aoffset++;
                        }
                        else
                        {
                            sprite_temp_render[(ycnt * (wide * 16)) + (xcnt * 16 + x)] = 0x8000;
                        }
                        msk >>= 1;
                    }
                    boffset += 2;
                }
            }
        }
        private static void pgm_draw_pix(int xdrawpos, int ydrawpos, int pri, ushort srcdat)
        {
            if ((xdrawpos >= 0) && (xdrawpos < 448))
            {
                if ((Tilemap.priority_bitmap[ydrawpos, xdrawpos] & 1) == 0)
                {
                    if (pri == 0)
                    {
                        Video.bitmapbase_Ptrs[Video.curbitmap][ydrawpos * 0x200 + xdrawpos] = srcdat;
                    }
                    else
                    {
                        if ((Tilemap.priority_bitmap[ydrawpos, xdrawpos] & 2) == 0)
                        {
                            Video.bitmapbase_Ptrs[Video.curbitmap][ydrawpos * 0x200 + xdrawpos] = srcdat;
                        }
                    }
                }
                Tilemap.priority_bitmap[ydrawpos, xdrawpos] |= 1;
            }
        }
        private static void pgm_draw_pix_nopri(int xdrawpos, int ydrawpos, ushort srcdat)
        {
            if ((xdrawpos >= 0) && (xdrawpos < 448))
            {
                if ((Tilemap.priority_bitmap[ydrawpos, xdrawpos] & 1) == 0)
                {
                    Video.bitmapbase_Ptrs[Video.curbitmap][ydrawpos * 0x200 + xdrawpos] = srcdat;
                }
                Tilemap.priority_bitmap[ydrawpos, xdrawpos] |= 1;
            }
        }
        private static void pgm_draw_pix_pri(int xdrawpos, int ydrawpos, ushort srcdat)
        {
            if ((xdrawpos >= 0) && (xdrawpos < 448))
            {
                if ((Tilemap.priority_bitmap[ydrawpos, xdrawpos] & 1) == 0)
                {
                    if ((Tilemap.priority_bitmap[ydrawpos, xdrawpos] & 2) == 0)
                    {
                        Video.bitmapbase_Ptrs[Video.curbitmap][ydrawpos * 0x200 + xdrawpos] = srcdat;
                    }
                }
                Tilemap.priority_bitmap[ydrawpos, xdrawpos] |= 1;
            }
        }
        private static void draw_sprite_line(int wide, int ydrawpos, int xzoom, int xgrow, int yoffset, int flip, int xpos)
        {
            int xcnt, xcntdraw;
            int xzoombit;
            int xoffset;
            int xdrawpos = 0;
            ushort srcdat;
            xcnt = 0;
            xcntdraw = 0;
            while (xcnt < wide * 16)
            {
                if ((flip & 0x01) == 0)
                    xoffset = xcnt;
                else
                    xoffset = (wide * 16) - xcnt - 1;
                srcdat = sprite_temp_render[yoffset + xoffset];
                xzoombit = (xzoom >> (xcnt & 0x1f)) & 1;
                if (xzoombit == 1 && xgrow == 1)
                {
                    xdrawpos = xpos + xcntdraw;
                    if ((srcdat & 0x8000) == 0)
                    {
                        if ((xdrawpos >= 0) && (xdrawpos < 448))
                            Video.bitmapbase_Ptrs[Video.curbitmap][ydrawpos * 0x200 + xdrawpos] = srcdat;
                    }
                    xcntdraw++;
                    xdrawpos = xpos + xcntdraw;
                    if ((srcdat & 0x8000) == 0)
                    {
                        if ((xdrawpos >= 0) && (xdrawpos < 448))
                            Video.bitmapbase_Ptrs[Video.curbitmap][ydrawpos * 0x200 + xdrawpos] = srcdat;
                    }
                    xcntdraw++;
                }
                else if (xzoombit == 1 && xgrow == 0)
                {

                }
                else
                {
                    xdrawpos = xpos + xcntdraw;
                    if ((srcdat & 0x8000) == 0)
                    {
                        if ((xdrawpos >= 0) && (xdrawpos < 448))
                            Video.bitmapbase_Ptrs[Video.curbitmap][ydrawpos * 0x200 + xdrawpos] = srcdat;
                    }
                    xcntdraw++;
                }
                xcnt++;
                if (xdrawpos == 448)
                    xcnt = wide * 16;
            }
        }
        private static void draw_sprite_new_zoomed(int wide, int high, int xpos, int ypos, int palt, int boffset, int flip, int xzoom, int xgrow, int yzoom, int ygrow)
        {
            int ycnt;
            int ydrawpos;
            int yoffset;
            int ycntdraw;
            int yzoombit;
            pgm_prepare_sprite(wide, high, palt, boffset);
            ycnt = 0;
            ycntdraw = 0;
            while (ycnt < high)
            {
                yzoombit = (yzoom >> (ycnt & 0x1f)) & 1;
                if (yzoombit == 1 && ygrow == 1)
                {
                    ydrawpos = ypos + ycntdraw;
                    if ((flip & 0x02) == 0)
                        yoffset = (ycnt * (wide * 16));
                    else
                        yoffset = ((high - ycnt - 1) * (wide * 16));
                    if ((ydrawpos >= 0) && (ydrawpos < 224))
                    {
                        draw_sprite_line(wide, ydrawpos, xzoom, xgrow, yoffset, flip, xpos);
                    }
                    ycntdraw++;
                    ydrawpos = ypos + ycntdraw;
                    if ((flip & 0x02) == 0)
                        yoffset = (ycnt * (wide * 16));
                    else
                        yoffset = ((high - ycnt - 1) * (wide * 16));
                    if ((ydrawpos >= 0) && (ydrawpos < 224))
                    {
                        draw_sprite_line(wide, ydrawpos, xzoom, xgrow, yoffset, flip, xpos);
                    }
                    ycntdraw++;
                    if (ydrawpos == 224)
                        ycnt = high;
                }
                else if (yzoombit == 1 && ygrow == 0)
                {

                }
                else
                {
                    ydrawpos = ypos + ycntdraw;
                    if ((flip & 0x02) == 0)
                        yoffset = (ycnt * (wide * 16));
                    else
                        yoffset = ((high - ycnt - 1) * (wide * 16));
                    if ((ydrawpos >= 0) && (ydrawpos < 224))
                    {
                        draw_sprite_line(wide, ydrawpos, xzoom, xgrow, yoffset, flip, xpos);
                    }
                    ycntdraw++;
                    if (ydrawpos == 224)
                        ycnt = high;
                }
                ycnt++;
            }
        }
        unsafe private static void draw_sprites(int priority)
        {
            fixed (ushort* pSpriteBuffer = &pgm_spritebufferram[0])
            fixed (byte* pVideoRegs = &pgm_videoregs[0])
            {
                ushort* spritePtr = pSpriteBuffer + pgm_sprite_source_offset;
                int offset = 0;

                while (pgm_sprite_source_offset < 0x500)
                {
                    // 读取数据
                    ushort* spriteData = (ushort*)spritePtr;
                    int xpos = spriteData[0] & 0x07FF;
                    int ypos = spriteData[1] & 0x03FF;
                    int xzom = (spriteData[0] & 0x7800) >> 11;
                    int xgrow = (spriteData[0] & 0x8000) >> 15;
                    int yzom = (spriteData[1] & 0x7800) >> 11;
                    int ygrow = (spriteData[1] & 0x8000) >> 15;
                    ushort* spriteData2 = (ushort*)(spritePtr + 4);
                    int palt = (spriteData2[0] & 0x1F00) >> 8;
                    int flip = (spriteData2[0] & 0x6000) >> 13;
                    int boff = ((spriteData2[0] & 0x007F) << 16) | spriteData2[1];
                    ushort* spriteData3 = (ushort*)(spritePtr + 6);
                    int wide = (spriteData3[0] & 0x7E00) >> 9;
                    int high = spriteData3[0] & 0x01FF;
                    int pri = (spriteData2[0] & 0x0080) >> 7;
                    int pgm_sprite_zoomtable_offset = 0x1000;
                    // 处理缩放
                    int xzoom, yzoom;
                    int* zoomTablePtr = (int*)(pVideoRegs + pgm_sprite_zoomtable_offset);
                    if (xgrow != 0)
                    {
                        xzom = 0x10 - xzom;
                    }
                    if (ygrow != 0)
                    {
                        yzom = 0x10 - yzom;
                    }
                    xzoom = zoomTablePtr[xzom * 4] * 0x10000 + zoomTablePtr[xzom * 4 + 1] * 0x100 + zoomTablePtr[xzom * 4 + 2];
                    yzoom = zoomTablePtr[yzom * 4] * 0x10000 + zoomTablePtr[yzom * 4 + 1] * 0x100 + zoomTablePtr[yzom * 4 + 2];

                    // 调整偏移和边界检查
                    boff *= 2;
                    if (xpos > 0x3FF)
                        xpos -= 0x800;
                    if (ypos > 0x1FF)
                        ypos -= 0x400;
                    if (high == 0)
                        break;
                    if ((priority == 1) && (pri == 0))
                        break;

                    // 调用绘制函数（注意：这个函数也需要被修改为接受指针或适当的参数类型）
                    draw_sprite_new_zoomed(wide, high, xpos, ypos, palt, boff, flip, xzoom, xgrow, yzoom, ygrow);

                    // 移动到下一个精灵
                    spritePtr += 10; // 每个精灵占用5个ushort，即10个字节
                    pgm_sprite_source_offset += 5; // 假设pgm_sprite_source_offset是以ushort为单位递增的

                    // 注意：这里我们直接通过指针移动，因此不需要再次访问数组来更新pgm_sprite_source_offset对应的值
                }
            }
        }

        //private static void draw_sprites(int priority)
        //{
        //    while (pgm_sprite_source_offset < 0x500)
        //    {
        //        //用Span优化
        //        Span<ushort> span_pgm_spritebufferram = pgm_spritebufferram.AsSpan();
        //        int xpos = span_pgm_spritebufferram[pgm_sprite_source_offset + 0] & 0x07ff;
        //        int ypos = span_pgm_spritebufferram[pgm_sprite_source_offset + 1] & 0x03ff;
        //        int xzom = (span_pgm_spritebufferram[pgm_sprite_source_offset + 0] & 0x7800) >> 11;
        //        int xgrow = (span_pgm_spritebufferram[pgm_sprite_source_offset + 0] & 0x8000) >> 15;
        //        int yzom = (span_pgm_spritebufferram[pgm_sprite_source_offset + 1] & 0x7800) >> 11;
        //        int ygrow = (span_pgm_spritebufferram[pgm_sprite_source_offset + 1] & 0x8000) >> 15;
        //        int palt = (span_pgm_spritebufferram[pgm_sprite_source_offset + 2] & 0x1f00) >> 8;
        //        int flip = (span_pgm_spritebufferram[pgm_sprite_source_offset + 2] & 0x6000) >> 13;
        //        int boff = ((span_pgm_spritebufferram[pgm_sprite_source_offset + 2] & 0x007f) << 16) | (span_pgm_spritebufferram[pgm_sprite_source_offset + 3] & 0xffff);
        //        int wide = (span_pgm_spritebufferram[pgm_sprite_source_offset + 4] & 0x7e00) >> 9;
        //        int high = span_pgm_spritebufferram[pgm_sprite_source_offset + 4] & 0x01ff;
        //        int pri = (span_pgm_spritebufferram[pgm_sprite_source_offset + 2] & 0x0080) >> 7;


        //        //int xpos = pgm_spritebufferram[pgm_sprite_source_offset + 0] & 0x07ff;
        //        //int ypos = pgm_spritebufferram[pgm_sprite_source_offset + 1] & 0x03ff;
        //        //int xzom = (pgm_spritebufferram[pgm_sprite_source_offset + 0] & 0x7800) >> 11;
        //        //int xgrow = (pgm_spritebufferram[pgm_sprite_source_offset + 0] & 0x8000) >> 15;
        //        //int yzom = (pgm_spritebufferram[pgm_sprite_source_offset + 1] & 0x7800) >> 11;
        //        //int ygrow = (pgm_spritebufferram[pgm_sprite_source_offset + 1] & 0x8000) >> 15;
        //        //int palt = (pgm_spritebufferram[pgm_sprite_source_offset + 2] & 0x1f00) >> 8;
        //        //int flip = (pgm_spritebufferram[pgm_sprite_source_offset + 2] & 0x6000) >> 13;
        //        //int boff = ((pgm_spritebufferram[pgm_sprite_source_offset + 2] & 0x007f) << 16) | (pgm_spritebufferram[pgm_sprite_source_offset + 3] & 0xffff);
        //        //int wide = (pgm_spritebufferram[pgm_sprite_source_offset + 4] & 0x7e00) >> 9;
        //        //int high = pgm_spritebufferram[pgm_sprite_source_offset + 4] & 0x01ff;
        //        //int pri = (pgm_spritebufferram[pgm_sprite_source_offset + 2] & 0x0080) >> 7;

        //        int xzoom, yzoom;
        //        int pgm_sprite_zoomtable_offset = 0x1000;
        //        if (xgrow != 0)
        //        {
        //            xzom = 0x10 - xzom;
        //        }
        //        if (ygrow != 0)
        //        {
        //            yzom = 0x10 - yzom;
        //        }
        //        Span<byte> span_pgm_videoregs = pgm_videoregs.AsSpan();
        //        xzoom = ((span_pgm_videoregs[pgm_sprite_zoomtable_offset + xzom * 4] * 0x100 + span_pgm_videoregs[pgm_sprite_zoomtable_offset + xzom * 4 + 1]) << 16) | (span_pgm_videoregs[pgm_sprite_zoomtable_offset + xzom * 4 + 2] * 0x100 + span_pgm_videoregs[pgm_sprite_zoomtable_offset + xzom * 4 + 3]);
        //        yzoom = ((span_pgm_videoregs[pgm_sprite_zoomtable_offset + yzom * 4] * 0x100 + span_pgm_videoregs[pgm_sprite_zoomtable_offset + yzom * 4 + 1]) << 16) | (span_pgm_videoregs[pgm_sprite_zoomtable_offset + yzom * 4 + 2] * 0x100 + span_pgm_videoregs[pgm_sprite_zoomtable_offset + yzom * 4 + 3]);

        //        //xzoom = ((pgm_videoregs[pgm_sprite_zoomtable_offset + xzom * 4] * 0x100 + pgm_videoregs[pgm_sprite_zoomtable_offset + xzom * 4 + 1]) << 16) | (pgm_videoregs[pgm_sprite_zoomtable_offset + xzom * 4 + 2] * 0x100 + pgm_videoregs[pgm_sprite_zoomtable_offset + xzom * 4 + 3]);
        //        //yzoom = ((pgm_videoregs[pgm_sprite_zoomtable_offset + yzom * 4] * 0x100 + pgm_videoregs[pgm_sprite_zoomtable_offset + yzom * 4 + 1]) << 16) | (pgm_videoregs[pgm_sprite_zoomtable_offset + yzom * 4 + 2] * 0x100 + pgm_videoregs[pgm_sprite_zoomtable_offset + yzom * 4 + 3]);
        //        boff *= 2;
        //        if (xpos > 0x3ff)
        //            xpos -= 0x800;
        //        if (ypos > 0x1ff)
        //            ypos -= 0x400;
        //        if (high == 0)
        //            break;
        //        if ((priority == 1) && (pri == 0))
        //            break;
        //        draw_sprite_new_zoomed(wide, high, xpos, ypos, palt, boff, flip, xzoom, xgrow, yzoom, ygrow);
        //        pgm_sprite_source_offset += 5;
        //    }
        //}
        private static void pgm_tx_videoram_w(int offset, byte data)
        {
            int col, row;
            pgm_tx_videoram[offset] = data;
            col = (offset / 4) % 64;
            row = (offset / 4) / 64;
            pgm_tx_tilemap.tilemap_mark_tile_dirty(row, col);
        }
        private static void pgm_tx_videoram_w(int offset, ushort data)
        {
            int col, row;
            pgm_tx_videoram[offset * 2] = (byte)(data >> 8);
            pgm_tx_videoram[offset * 2 + 1] = (byte)data;
            col = (offset / 2) % 64;
            row = (offset / 2) / 64;
            pgm_tx_tilemap.tilemap_mark_tile_dirty(row, col);
        }
        private static void pgm_bg_videoram_w(int offset, byte data)
        {
            int col, row;
            pgm_bg_videoram[offset] = data;
            col = (offset / 4) % 64;
            row = (offset / 4) / 64;
            pgm_bg_tilemap.tilemap_mark_tile_dirty(row, col);
        }
        private static void pgm_bg_videoram_w(int offset, ushort data)
        {
            int col, row;
            pgm_bg_videoram[offset * 2] = (byte)(data >> 8);
            pgm_bg_videoram[offset * 2 + 1] = (byte)data;
            col = (offset / 2) % 64;
            row = (offset / 2) / 64;
            pgm_bg_tilemap.tilemap_mark_tile_dirty(row, col);
        }
        public static void video_start_pgm()
        {
            int i;
            uu900 = new ushort[0x200 * 0x200];
            for (i = 0; i < 0x40000; i++)
            {
                uu900[i] = 0x900;
            }
            pgm_spritebufferram = new ushort[0xa00 / 2];
            sprite_temp_render = new ushort[0x400 * 0x200];
        }
        public static void video_update_pgm()
        {
            int y;
            RECT new_clip = new RECT();
            new_clip.min_x = 0x00;
            new_clip.max_x = 0x1bf;
            new_clip.min_y = 0x00;
            new_clip.max_y = 0xdf;
            Array.Copy(uu900, Video.bitmapbase[Video.curbitmap], 0x40000);
            pgm_sprite_source_offset = 0;
            draw_sprites(1);
            pgm_bg_tilemap.tilemap_set_scrolly(0, pgm_videoregs[0x2000] * 0x100 + pgm_videoregs[0x2000 + 1]);
            for (y = 0; y < 224; y++)
            {
                pgm_bg_tilemap.tilemap_set_scrollx((y + pgm_videoregs[0x2000] * 0x100 + pgm_videoregs[0x2000 + 1]) & 0x7ff, pgm_videoregs[0x3000] * 0x100 + pgm_videoregs[0x3000 + 1] + pgm_rowscrollram[y * 2] * 0x100 + pgm_rowscrollram[y * 2 + 1]);
            }
            pgm_bg_tilemap.tilemap_draw_primask(new_clip, 0x10, 0);
            draw_sprites(0);
            //draw_sprites();
            pgm_tx_tilemap.tilemap_set_scrolly(0, pgm_videoregs[0x5000] * 0x100 + pgm_videoregs[0x5000 + 1]);
            pgm_tx_tilemap.tilemap_set_scrollx(0, pgm_videoregs[0x6000] * 0x100 + pgm_videoregs[0x6000 + 1]);
            pgm_tx_tilemap.tilemap_draw_primask(new_clip, 0x10, 0);
        }
        public unsafe static void video_eof_pgm()
        {
            int i;
            for (i = 0; i < 0x500; i++)
            {
                pgm_spritebufferram[i] = (ushort)(Memory.mainram[i * 2] * 0x100 + Memory.mainram[i * 2 + 1]);
            }
        }
    }
}
