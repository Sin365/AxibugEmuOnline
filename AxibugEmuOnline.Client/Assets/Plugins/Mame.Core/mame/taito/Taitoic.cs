using System;

namespace MAME.Core
{
    public unsafe partial class Taito
    {
        public static int PC080SN_chips;
        public static ushort[][] PC080SN_ctrl;
        public static ushort[][] PC080SN_ram;
        public static ushort[][] PC080SN_bg_ram_offset, PC080SN_bgscroll_ram_offset;
        public static int[][] PC080SN_bgscrollx, PC080SN_bgscrolly;
        public static int PC080SN_xoffs, PC080SN_yoffs;
        public static Tmap[][] PC080SN_tilemap;
        public static int PC080SN_yinvert, PC080SN_dblwidth;
        public static ushort PC090OJ_ctrl, PC090OJ_buffer, PC090OJ_gfxnum;
        public static ushort PC090OJ_sprite_ctrl;
        public static ushort[] PC090OJ_ram, PC090OJ_ram_buffered;
        public static int PC090OJ_xoffs, PC090OJ_yoffs;
        public static void taitoic_init()
        {
            int i;
            PC080SN_ctrl = new ushort[2][];
            PC080SN_ram = new ushort[2][];
            PC080SN_bg_ram_offset = new ushort[2][];
            PC080SN_bgscroll_ram_offset = new ushort[2][];
            PC080SN_bgscrollx = new int[2][];
            PC080SN_bgscrolly = new int[2][];
            PC080SN_tilemap = new Tmap[2][];
            for (i = 0; i < 2; i++)
            {
                PC080SN_ctrl[i] = new ushort[8];
                PC080SN_bg_ram_offset[i] = new ushort[2];
                PC080SN_bgscroll_ram_offset[i] = new ushort[2];
                PC080SN_bgscrollx[i] = new int[2];
                PC080SN_bgscrolly[i] = new int[2];
                PC080SN_tilemap[i] = new Tmap[2];
            }
        }
        public static void tilemap_init()
        {
            switch (Machine.sName)
            {
                case "opwolf":
                case "opwolfa":
                case "opwolfj":
                case "opwolfu":
                case "opwolfb":
                case "opwolfp":
                    taitoic_init();
                    break;
            }
        }
        public static void PC080SN_vh_start(int chips, int gfxnum, int x_offset, int y_offset, int y_invert, int opaque, int dblwidth)
        {
            int i, j, k;
            PC080SN_chips = chips;
            PC080SN_yinvert = y_invert;
            PC080SN_dblwidth = dblwidth;
            PC080SN_xoffs = x_offset;
            PC080SN_yoffs = y_offset;
            for (i = 0; i < chips; i++)
            {
                int xd, yd;
                for (j = 0; j < 2; j++)
                {
                    PC080SN_tilemap[i][j] = new Tmap();
                    if (PC080SN_dblwidth == 0)	/* standard tilemaps */
                    {
                        PC080SN_tilemap[i][j].cols = 64;
                        PC080SN_tilemap[i][j].rows = 64;
                        PC080SN_tilemap[i][j].tilewidth = 8;
                        PC080SN_tilemap[i][j].tileheight = 8;
                        PC080SN_tilemap[i][j].width = 0x200;
                        PC080SN_tilemap[i][j].height = 0x200;
                        PC080SN_tilemap[i][j].enable = true;
                        PC080SN_tilemap[i][j].all_tiles_dirty = true;
                        PC080SN_tilemap[i][j].total_elements = gfx1romLength / 0x40;
                        PC080SN_tilemap[i][j].pixmap = new ushort[0x200 * 0x200];
                        PC080SN_tilemap[i][j].flagsmap = new byte[0x200, 0x200];
                        PC080SN_tilemap[i][j].tileflags = new byte[64, 64];
                        PC080SN_tilemap[i][j].pen_data_set = new byte[0x40];
                        PC080SN_tilemap[i][j].pen_to_flags = new byte[1, 16];
                        PC080SN_tilemap[i][j].pen_to_flags[0, 0] = 0;
                        for (k = 1; k < 16; k++)
                        {
                            PC080SN_tilemap[i][j].pen_to_flags[0, k] = 0x10;
                        }
                        PC080SN_tilemap[i][j].scrollrows = 1;
                        PC080SN_tilemap[i][j].scrollcols = 1;
                        PC080SN_tilemap[i][j].rowscroll = new int[PC080SN_tilemap[i][j].scrollrows];
                        PC080SN_tilemap[i][j].colscroll = new int[PC080SN_tilemap[i][j].scrollcols];
                        PC080SN_tilemap[i][j].tilemap_draw_instance3 = PC080SN_tilemap[i][j].tilemap_draw_instanceTaito_opwolf;
                    }
                    else	/* double width tilemaps */
                    {
                        PC080SN_tilemap[i][j].cols = 128;
                        PC080SN_tilemap[i][j].rows = 64;
                        PC080SN_tilemap[i][j].tilewidth = 8;
                        PC080SN_tilemap[i][j].tileheight = 8;
                        PC080SN_tilemap[i][j].width = 0x400;
                        PC080SN_tilemap[i][j].height = 0x200;
                        PC080SN_tilemap[i][j].enable = true;
                        PC080SN_tilemap[i][j].all_tiles_dirty = true;
                        PC080SN_tilemap[i][j].total_elements = gfx1romLength / 0x40;
                        PC080SN_tilemap[i][j].pixmap = new ushort[0x200 * 0x400];
                        PC080SN_tilemap[i][j].flagsmap = new byte[0x200, 0x400];
                        PC080SN_tilemap[i][j].tileflags = new byte[128, 64];
                        PC080SN_tilemap[i][j].pen_data_set = new byte[0x40];
                        PC080SN_tilemap[i][j].pen_to_flags = new byte[1, 16];
                        PC080SN_tilemap[i][j].pen_to_flags[0, 0] = 0;
                        for (k = 1; k < 16; k++)
                        {
                            PC080SN_tilemap[i][j].pen_to_flags[0, k] = 0x10;
                        }
                        PC080SN_tilemap[i][j].scrollrows = 1;
                        PC080SN_tilemap[i][j].scrollcols = 1;
                        PC080SN_tilemap[i][j].rowscroll = new int[PC080SN_tilemap[i][j].scrollrows];
                        PC080SN_tilemap[i][j].colscroll = new int[PC080SN_tilemap[i][j].scrollcols];
                        PC080SN_tilemap[i][j].tilemap_draw_instance3 = PC080SN_tilemap[i][j].tilemap_draw_instanceTaito_opwolf;
                    }
                }
                PC080SN_tilemap[i][0].tile_update3 = PC080SN_tilemap[i][0].tile_updateTaitobg_opwolf;
                PC080SN_tilemap[i][1].tile_update3 = PC080SN_tilemap[i][1].tile_updateTaitofg_opwolf;
                PC080SN_ram[i] = new ushort[0x8000];
                PC080SN_bg_ram_offset[i][0] = 0x0000 / 2;
                PC080SN_bg_ram_offset[i][1] = 0x8000 / 2;
                PC080SN_bgscroll_ram_offset[i][0] = 0x4000 / 2;
                PC080SN_bgscroll_ram_offset[i][1] = 0xc000 / 2;
                Array.Clear(PC080SN_ram[i], 0, 0x8000);
                /*state_save_register_item_pointer("PC080SN", i, PC080SN_ram[i], PC080SN_RAM_SIZE / 2);
                state_save_register_item_array("PC080SN", i, PC080SN_ctrl[i]);
                state_save_register_postload(machine, PC080SN_restore_scroll, (void*)(FPTR)i);*/

                //tilemap_set_transparent_pen(PC080SN_tilemap[i][0], 0);
                //tilemap_set_transparent_pen(PC080SN_tilemap[i][1], 0);

                /* I'm setting optional chip #2 with the same offsets (Topspeed) */
                xd = (i == 0) ? -x_offset : -x_offset;
                yd = (i == 0) ? y_offset : y_offset;
                PC080SN_tilemap[i][0].tilemap_set_scrolldx(-16 + xd, -16 - xd);
                PC080SN_tilemap[i][0].tilemap_set_scrolldy(yd, -yd);
                PC080SN_tilemap[i][1].tilemap_set_scrolldx(-16 + xd, -16 - xd);
                PC080SN_tilemap[i][1].tilemap_set_scrolldy(yd, -yd);
                if (PC080SN_dblwidth == 0)
                {
                    PC080SN_tilemap[i][0].scrollrows = 512;
                    PC080SN_tilemap[i][0].rowscroll = new int[PC080SN_tilemap[i][0].scrollrows];
                    PC080SN_tilemap[i][1].scrollrows = 512;
                    PC080SN_tilemap[i][1].rowscroll = new int[PC080SN_tilemap[i][1].scrollrows];
                }
            }
        }
        public static ushort PC080SN_word_0_r(int offset)
        {
            return PC080SN_ram[0][offset];
        }
        public static ushort PC080SN_word_1_r(int offset)
        {
            return PC080SN_ram[1][offset];
        }
        public static void PC080SN_word_w(int chip, int offset, ushort data)
        {
            int row, col, memindex;
            PC080SN_ram[chip][offset] = data;
            if (PC080SN_dblwidth == 0)
            {
                if (offset < 0x2000)
                {
                    memindex = offset / 2;
                    row = memindex / PC080SN_tilemap[chip][0].cols;
                    col = memindex % PC080SN_tilemap[chip][0].cols;
                    PC080SN_tilemap[chip][0].tilemap_mark_tile_dirty(row, col);
                }
                else if (offset >= 0x4000 && offset < 0x6000)
                {
                    memindex = (offset & 0x1fff) / 2;
                    row = memindex / PC080SN_tilemap[chip][1].cols;
                    col = memindex % PC080SN_tilemap[chip][1].cols;
                    PC080SN_tilemap[chip][1].tilemap_mark_tile_dirty(row, col);
                }
            }
            else
            {
                if (offset < 0x4000)
                {
                    memindex = offset & 0x1fff;
                    row = memindex / PC080SN_tilemap[chip][0].cols;
                    col = memindex % PC080SN_tilemap[chip][0].cols;
                    PC080SN_tilemap[chip][0].tilemap_mark_tile_dirty(row, col);
                }
                else if (offset >= 0x4000 && offset < 0x8000)
                {
                    memindex = offset & 0x1fff;
                    row = memindex / PC080SN_tilemap[chip][1].cols;
                    col = memindex % PC080SN_tilemap[chip][1].cols;
                    PC080SN_tilemap[chip][1].tilemap_mark_tile_dirty(row, col);
                }
            }
        }
        public static void PC080SN_word_w1(int chip, int offset, byte data)
        {
            int row, col, memindex;
            PC080SN_ram[chip][offset] = (ushort)((data << 8) | (PC080SN_ram[chip][offset] & 0xff));
            if (PC080SN_dblwidth == 0)
            {
                if (offset < 0x2000)
                {
                    memindex = offset / 2;
                    row = memindex / PC080SN_tilemap[chip][0].cols;
                    col = memindex % PC080SN_tilemap[chip][0].cols;
                    PC080SN_tilemap[chip][0].tilemap_mark_tile_dirty(row, col);
                }
                else if (offset >= 0x4000 && offset < 0x6000)
                {
                    memindex = (offset & 0x1fff) / 2;
                    row = memindex / PC080SN_tilemap[chip][1].cols;
                    col = memindex % PC080SN_tilemap[chip][1].cols;
                    PC080SN_tilemap[chip][1].tilemap_mark_tile_dirty(row, col);
                }
            }
            else
            {
                if (offset < 0x4000)
                {
                    memindex = offset & 0x1fff;
                    row = memindex / PC080SN_tilemap[chip][0].cols;
                    col = memindex % PC080SN_tilemap[chip][0].cols;
                    PC080SN_tilemap[chip][0].tilemap_mark_tile_dirty(row, col);
                }
                else if (offset >= 0x4000 && offset < 0x8000)
                {
                    memindex = offset & 0x1fff;
                    row = memindex / PC080SN_tilemap[chip][1].cols;
                    col = memindex % PC080SN_tilemap[chip][1].cols;
                    PC080SN_tilemap[chip][1].tilemap_mark_tile_dirty(row, col);
                }
            }
        }
        public static void PC080SN_word_w2(int chip, int offset, byte data)
        {
            int row, col, memindex;
            PC080SN_ram[chip][offset] = (ushort)((PC080SN_ram[chip][offset] & 0xff00) | data);
            if (PC080SN_dblwidth == 0)
            {
                if (offset < 0x2000)
                {
                    memindex = offset / 2;
                    row = memindex / PC080SN_tilemap[chip][0].cols;
                    col = memindex % PC080SN_tilemap[chip][0].cols;
                    PC080SN_tilemap[chip][0].tilemap_mark_tile_dirty(row, col);
                }
                else if (offset >= 0x4000 && offset < 0x6000)
                {
                    memindex = (offset & 0x1fff) / 2;
                    row = memindex / PC080SN_tilemap[chip][1].cols;
                    col = memindex % PC080SN_tilemap[chip][1].cols;
                    PC080SN_tilemap[chip][1].tilemap_mark_tile_dirty(row, col);
                }
            }
            else
            {
                if (offset < 0x4000)
                {
                    memindex = offset & 0x1fff;
                    row = memindex / PC080SN_tilemap[chip][0].cols;
                    col = memindex % PC080SN_tilemap[chip][0].cols;
                    PC080SN_tilemap[chip][0].tilemap_mark_tile_dirty(row, col);
                }
                else if (offset >= 0x4000 && offset < 0x8000)
                {
                    memindex = offset & 0x1fff;
                    row = memindex / PC080SN_tilemap[chip][1].cols;
                    col = memindex % PC080SN_tilemap[chip][1].cols;
                    PC080SN_tilemap[chip][1].tilemap_mark_tile_dirty(row, col);
                }
            }
        }
        public static void PC080SN_word_0_w(int offset, ushort data)
        {
            PC080SN_word_w(0, offset, data);
        }
        public static void PC080SN_word_0_w1(int offset, byte data)
        {
            PC080SN_word_w1(0, offset, data);
        }
        public static void PC080SN_word_0_w2(int offset, byte data)
        {
            PC080SN_word_w2(0, offset, data);
        }
        public static void PC080SN_word_1_w(int offset, ushort data)
        {
            PC080SN_word_w(1, offset, data);
        }
        public static void PC080SN_word_1_w1(int offset, byte data)
        {
            PC080SN_word_w1(1, offset, data);
        }
        public static void PC080SN_word_1_w2(int offset, byte data)
        {
            PC080SN_word_w2(1, offset, data);
        }
        public static void PC080SN_xscroll_word_w(int chip, int offset, ushort data)
        {
            ushort data1;
            PC080SN_ctrl[chip][offset] = data;
            data1 = PC080SN_ctrl[chip][offset];
            switch (offset)
            {
                case 0x00:
                    PC080SN_bgscrollx[chip][0] = -data1;
                    break;
                case 0x01:
                    PC080SN_bgscrollx[chip][1] = -data1;
                    break;
            }
        }
        public static void PC080SN_xscroll_word_w1(int chip, int offset, byte data)
        {
            ushort data1;
            PC080SN_ctrl[chip][offset] = (ushort)((data << 8) | (PC080SN_ctrl[chip][offset] & 0xff));
            data1 = PC080SN_ctrl[chip][offset];
            switch (offset)
            {
                case 0x00:
                    PC080SN_bgscrollx[chip][0] = -data1;
                    break;
                case 0x01:
                    PC080SN_bgscrollx[chip][1] = -data1;
                    break;
            }
        }
        public static void PC080SN_xscroll_word_w2(int chip, int offset, byte data)
        {
            ushort data1;
            PC080SN_ctrl[chip][offset] = (ushort)((PC080SN_ctrl[chip][offset] & 0xff00) | data);
            data1 = PC080SN_ctrl[chip][offset];
            switch (offset)
            {
                case 0x00:
                    PC080SN_bgscrollx[chip][0] = -data1;
                    break;
                case 0x01:
                    PC080SN_bgscrollx[chip][1] = -data1;
                    break;
            }
        }
        public static void PC080SN_yscroll_word_w(int chip, int offset, ushort data)
        {
            int data1;
            PC080SN_ctrl[chip][offset + 2] = data;
            data1 = PC080SN_ctrl[chip][offset + 2];
            if (PC080SN_yinvert != 0)
            {
                data1 = -data1;
            }
            switch (offset)
            {
                case 0x00:
                    PC080SN_bgscrolly[chip][0] = -data1;
                    break;
                case 0x01:
                    PC080SN_bgscrolly[chip][1] = -data1;
                    break;
            }
        }
        public static void PC080SN_yscroll_word_w1(int chip, int offset, byte data)
        {
            int data1;
            PC080SN_ctrl[chip][offset + 2] = (ushort)((data << 8) | (PC080SN_ctrl[chip][offset + 2] & 0xff));
            data1 = PC080SN_ctrl[chip][offset + 2];
            if (PC080SN_yinvert != 0)
            {
                data1 = -data1;
            }
            switch (offset)
            {
                case 0x00:
                    PC080SN_bgscrolly[chip][0] = -data1;
                    break;
                case 0x01:
                    PC080SN_bgscrolly[chip][1] = -data1;
                    break;
            }
        }
        public static void PC080SN_yscroll_word_w2(int chip, int offset, byte data)
        {
            int data1;
            PC080SN_ctrl[chip][offset + 2] = (ushort)((PC080SN_ctrl[chip][offset + 2] & 0xff00) | data);
            data1 = PC080SN_ctrl[chip][offset + 2];
            if (PC080SN_yinvert != 0)
            {
                data1 = -data1;
            }
            switch (offset)
            {
                case 0x00:
                    PC080SN_bgscrolly[chip][0] = -data1;
                    break;
                case 0x01:
                    PC080SN_bgscrolly[chip][1] = -data1;
                    break;
            }
        }
        public static void PC080SN_ctrl_word_w(int chip, int offset, ushort data)
        {
            ushort data1;
            PC080SN_ctrl[chip][offset + 4] = data;
            data1 = PC080SN_ctrl[chip][offset + 4];
            switch (offset)
            {
                case 0x00:
                    {
                        int flip = (data1 & 0x01) != 0 ? (Tilemap.TILEMAP_FLIPX | Tilemap.TILEMAP_FLIPY) : 0;
                        //tilemap_set_flip(PC080SN_tilemap[chip][0],flip);
                        //tilemap_set_flip(PC080SN_tilemap[chip][1],flip);
                        break;
                    }
            }
        }
        public static void PC080SN_ctrl_word_w1(int chip, int offset, byte data)
        {
            ushort data1;
            PC080SN_ctrl[chip][offset + 4] = (ushort)((data << 8) | (PC080SN_ctrl[chip][offset + 4] & 0xff));
            data1 = PC080SN_ctrl[chip][offset + 4];
            switch (offset)
            {
                case 0x00:
                    {
                        int flip = (data1 & 0x01) != 0 ? (Tilemap.TILEMAP_FLIPX | Tilemap.TILEMAP_FLIPY) : 0;
                        break;
                    }
            }
        }
        public static void PC080SN_ctrl_word_w2(int chip, int offset, byte data)
        {
            ushort data1;
            PC080SN_ctrl[chip][offset + 4] = (ushort)((PC080SN_ctrl[chip][offset + 4] & 0xff00) | data);
            data1 = PC080SN_ctrl[chip][offset + 4];
            switch (offset)
            {
                case 0x00:
                    {
                        int flip = (data1 & 0x01) != 0 ? (Tilemap.TILEMAP_FLIPX | Tilemap.TILEMAP_FLIPY) : 0;
                        break;
                    }
            }
        }
        public static void PC080SN_xscroll_word_0_w(int offset, ushort data)
        {
            PC080SN_xscroll_word_w(0, offset, data);
        }
        public static void PC080SN_xscroll_word_0_w1(int offset, byte data)
        {
            PC080SN_xscroll_word_w1(0, offset, data);
        }
        public static void PC080SN_xscroll_word_0_w2(int offset, byte data)
        {
            PC080SN_xscroll_word_w2(0, offset, data);
        }
        public static void PC080SN_xscroll_word_1_w(int offset, ushort data)
        {
            PC080SN_xscroll_word_w(1, offset, data);
        }
        public static void PC080SN_xscroll_word_1_w1(int offset, byte data)
        {
            PC080SN_xscroll_word_w1(1, offset, data);
        }
        public static void PC080SN_xscroll_word_1_w2(int offset, byte data)
        {
            PC080SN_xscroll_word_w2(1, offset, data);
        }
        public static void PC080SN_yscroll_word_0_w(int offset, ushort data)
        {
            PC080SN_yscroll_word_w(0, offset, data);
        }
        public static void PC080SN_yscroll_word_0_w1(int offset, byte data)
        {
            PC080SN_yscroll_word_w1(0, offset, data);
        }
        public static void PC080SN_yscroll_word_0_w2(int offset, byte data)
        {
            PC080SN_yscroll_word_w2(0, offset, data);
        }
        public static void PC080SN_yscroll_word_1_w(int offset, ushort data)
        {
            PC080SN_yscroll_word_w(1, offset, data);
        }
        public static void PC080SN_yscroll_word_1_w1(int offset, byte data)
        {
            PC080SN_yscroll_word_w1(1, offset, data);
        }
        public static void PC080SN_yscroll_word_1_w2(int offset, byte data)
        {
            PC080SN_yscroll_word_w2(1, offset, data);
        }
        public static void PC080SN_ctrl_word_0_w(int offset, ushort data)
        {
            PC080SN_ctrl_word_w(0, offset, data);
        }
        public static void PC080SN_ctrl_word_0_w1(int offset, byte data)
        {
            PC080SN_ctrl_word_w1(0, offset, data);
        }
        public static void PC080SN_ctrl_word_0_w2(int offset, byte data)
        {
            PC080SN_ctrl_word_w2(0, offset, data);
        }
        public static void PC080SN_ctrl_word_1_w(int offset, ushort data)
        {
            PC080SN_ctrl_word_w(1, offset, data);
        }
        public static void PC080SN_ctrl_word_1_w1(int offset, byte data)
        {
            PC080SN_ctrl_word_w1(1, offset, data);
        }
        public static void PC080SN_ctrl_word_1_w2(int offset, byte data)
        {
            PC080SN_ctrl_word_w2(1, offset, data);
        }
        public static void PC080SN_tilemap_update()
        {
            int chip, j;
            for (chip = 0; chip < PC080SN_chips; chip++)
            {
                PC080SN_tilemap[chip][0].tilemap_set_scrolly(0, PC080SN_bgscrolly[chip][0]);
                PC080SN_tilemap[chip][1].tilemap_set_scrolly(0, PC080SN_bgscrolly[chip][1]);
                if (PC080SN_dblwidth == 0)
                {
                    for (j = 0; j < 256; j++)
                    {
                        PC080SN_tilemap[chip][0].tilemap_set_scrollx((j + PC080SN_bgscrolly[chip][0]) & 0x1ff, PC080SN_bgscrollx[chip][0] - PC080SN_ram[chip][PC080SN_bgscroll_ram_offset[chip][0] + j]);
                        //PC080SN_tilemap[chip][0].tilemap_set_scrollx(j, PC080SN_bgscrollx[chip][0]);

                    }
                    for (j = 0; j < 256; j++)
                    {
                        PC080SN_tilemap[chip][1].tilemap_set_scrollx((j + PC080SN_bgscrolly[chip][1]) & 0x1ff, PC080SN_bgscrollx[chip][1] - PC080SN_ram[chip][PC080SN_bgscroll_ram_offset[chip][1] + j]);
                        //PC080SN_tilemap[chip][1].tilemap_set_scrollx(j, PC080SN_bgscrollx[chip][1]);
                    }
                }
                else
                {
                    PC080SN_tilemap[chip][0].tilemap_set_scrollx(0, PC080SN_bgscrollx[chip][0]);
                    PC080SN_tilemap[chip][1].tilemap_set_scrollx(0, PC080SN_bgscrollx[chip][1]);
                }
            }
        }
        public static void PC080SN_tilemap_draw(int chip, int layer, int flags, byte priority)
        {
            PC080SN_tilemap[chip][layer].tilemap_draw_primask(Video.screenstate.visarea, flags, priority);
        }
        public static void PC090OJ_vh_start(int gfxnum, int x_offset, int y_offset, int use_buffer)
        {
            PC090OJ_gfxnum = (ushort)gfxnum;
            PC090OJ_xoffs = x_offset;
            PC090OJ_yoffs = y_offset;
            PC090OJ_buffer = (ushort)use_buffer;
            PC090OJ_ram = new ushort[0x2000];
            PC090OJ_ram_buffered = new ushort[0x2000];
            Array.Clear(PC090OJ_ram, 0, 0x2000);
            Array.Clear(PC090OJ_ram_buffered, 0, 0x2000);
        }
        public static ushort PC090OJ_word_0_r(int offset)
        {
            return PC090OJ_ram[offset];
        }
        public static void PC090OJ_word_w(int offset, ushort data)
        {
            PC090OJ_ram[offset] = data;
            if (PC090OJ_buffer == 0)
            {
                PC090OJ_ram_buffered[offset] = PC090OJ_ram[offset];
            }
            if (offset == 0xdff)
            {
                PC090OJ_ctrl = data;
            }
        }
        public static void PC090OJ_word_w1(int offset, byte data)
        {
            PC090OJ_ram[offset] = (ushort)((data << 8) | (PC090OJ_ram[offset] & 0xff));
            if (PC090OJ_buffer == 0)
            {
                PC090OJ_ram_buffered[offset] = PC090OJ_ram[offset];
            }
            if (offset == 0xdff)
            {
                PC090OJ_ctrl = (ushort)((data << 8) | (PC090OJ_ctrl & 0xff));
            }
        }
        public static void PC090OJ_word_w2(int offset, byte data)
        {
            PC090OJ_ram[offset] = (ushort)((PC090OJ_ram[offset] & 0xff00) | data);
            if (PC090OJ_buffer == 0)
            {
                PC090OJ_ram_buffered[offset] = PC090OJ_ram[offset];
            }
            if (offset == 0xdff)
            {
                PC090OJ_ctrl = (ushort)((PC090OJ_ctrl & 0xff00) | data);
            }
        }
        public static void PC090OJ_word_0_w(int offset, ushort data)
        {
            PC090OJ_word_w(offset, data);
        }
        public static void PC090OJ_word_0_w1(int offset, byte data)
        {
            PC090OJ_word_w1(offset, data);
        }
        public static void PC090OJ_word_0_w2(int offset, byte data)
        {
            PC090OJ_word_w2(offset, data);
        }
        public static void PC090OJ_draw_sprites(int pri_type)
        {
            int offs, priority = 0;
            int sprite_colbank = (PC090OJ_sprite_ctrl & 0xf) << 4;	/* top nibble */
            switch (pri_type)
            {
                case 0x00:
                    priority = 0;	/* sprites over top bg layer */
                    break;
                case 0x01:
                    priority = 1;	/* sprites under top bg layer */
                    break;
                case 0x02:
                    priority = PC090OJ_sprite_ctrl >> 15;	/* variable sprite/tile priority */
                    break;
            }
            for (offs = 0; offs < 0x800 / 2; offs += 4)
            {
                int flipx, flipy;
                int x, y;
                int data, code, color;
                data = PC090OJ_ram_buffered[offs + 0];
                flipy = (data & 0x8000) >> 15;
                flipx = (data & 0x4000) >> 14;
                color = (data & 0x000f) | sprite_colbank;
                code = PC090OJ_ram_buffered[offs + 2] & 0x1fff;
                x = PC090OJ_ram_buffered[offs + 3] & 0x1ff;   /* mask verified with Rainbowe board */
                y = PC090OJ_ram_buffered[offs + 1] & 0x1ff;   /* mask verified with Rainbowe board */
                if (x > 0x140)
                {
                    x -= 0x200;
                }
                if (y > 0x140)
                {
                    y -= 0x200;
                }
                if ((PC090OJ_ctrl & 1) == 0)
                {
                    x = 320 - x - 16;
                    y = 256 - y - 16;
                    flipx = (flipx == 0 ? 1 : 0);
                    flipy = (flipy == 0 ? 1 : 0);
                }
                x += PC090OJ_xoffs;
                y += PC090OJ_yoffs;
                Drawgfx.common_drawgfx_opwolf(gfx2rom, code, color, flipx, flipy, x, y, cliprect, (uint)((priority != 0 ? 0xfc : 0xf0) | (1 << 31)));
            }
        }
    }
}
