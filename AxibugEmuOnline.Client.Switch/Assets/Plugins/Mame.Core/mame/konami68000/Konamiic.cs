namespace MAME.Core
{
    public unsafe partial class Konami68000
    {
        private static byte[] K052109_memory_region;
        public static int K052109_videoram_F_offset, K052109_videoram2_F_offset, K052109_colorram_F_offset, K052109_videoram_A_offset, K052109_videoram2_A_offset, K052109_colorram_A_offset, K052109_videoram_B_offset, K052109_videoram2_B_offset, K052109_colorram_B_offset;
        public static byte[] K052109_ram, K052109_charrombank, K052109_charrombank_2;
        public static LineState K052109_RMRD_line;
        public static byte K052109_romsubbank, K052109_scrollctrl, K052109_irq_enabled, has_extra_video_ram;
        public static int[] K052109_dx, K052109_dy;
        public static int K052109_tileflip_enable;

        public static byte[] K051960_memory_region;
        public static int K051960_romoffset, K051960_spriteflip, K051960_readroms;
        public static byte[] K051960_spriterombank, K051960_ram;
        public static int K051960_dx, K051960_dy;
        public static int K051960_irq_enabled, K051960_nmi_enabled;

        private static byte[][] K053245_memory_region;
        private static int K05324x_z_rejection;
        private static int[] K053244_rombank, K053245_ramsize, K053245_dx, K053245_dy;
        public static ushort[][] K053245_buffer;
        public static byte[][] K053245_ram, K053244_regs;
        public static byte[] K054000_ram;

        public static ushort[] K053936_0_ctrl, K053936_0_linectrl;
        public static ushort[] K053936_1_ctrl, K053936_1_linectrl;
        public static int[][] K053936_offset;
        public static int[] K053936_wraparound;

        public static byte[] K053251_ram;
        public static int[] K053251_palette_index;
        public static int K053251_tilemaps_set;

        public static int counter;
        public delegate void K052109_delegate(int tmap, int bank, int code, int color, int flags, int priority, out int code2, out int color2, out int flags2);
        public static K052109_delegate K052109_callback;
        public delegate void K051960_delegate(int code, int color, int priority, int shadow, out int code2, out int color2, out int priority2);
        public static K051960_delegate K051960_callback;
        public delegate void K053245_delegate(int code, int color, out int code2, out int color2, out int priority_mask);
        public static K053245_delegate K053245_callback;
        public static void K052109_vh_start()
        {
            int i, j;
            K052109_RMRD_line = LineState.CLEAR_LINE;
            K052109_irq_enabled = 0;
            has_extra_video_ram = 0;
            K052109_tilemap = new Tmap[3];
            for (i = 0; i < 3; i++)
            {
                K052109_tilemap[i] = new Tmap();
                K052109_tilemap[i].tilewidth = 8;
                K052109_tilemap[i].tileheight = 8;
                K052109_tilemap[i].cols = 0x40;
                K052109_tilemap[i].rows = 0x20;
                K052109_tilemap[i].width = K052109_tilemap[i].cols * K052109_tilemap[i].tilewidth;
                K052109_tilemap[i].height = K052109_tilemap[i].rows * K052109_tilemap[i].tileheight;
                K052109_tilemap[i].enable = true;
                K052109_tilemap[i].all_tiles_dirty = true;
            }
            K052109_ram = new byte[0x6000];
            K052109_colorram_F_offset = 0x0000;
            K052109_colorram_A_offset = 0x0800;
            K052109_colorram_B_offset = 0x1000;
            K052109_videoram_F_offset = 0x2000;
            K052109_videoram_A_offset = 0x2800;
            K052109_videoram_B_offset = 0x3000;
            K052109_videoram2_F_offset = 0x4000;
            K052109_videoram2_A_offset = 0x4800;
            K052109_videoram2_B_offset = 0x5000;
            //tilemap_set_transparent_pen(K052109_tilemap[0],0);
            //tilemap_set_transparent_pen(K052109_tilemap[1],0);
            //tilemap_set_transparent_pen(K052109_tilemap[2],0);
            K052109_tilemap[0].scrollrows = 1;
            K052109_tilemap[0].scrollcols = 1;
            K052109_tilemap[1].scrollrows = 256;
            K052109_tilemap[2].scrollrows = 256;
            K052109_tilemap[1].scrollcols = 512;
            K052109_tilemap[2].scrollcols = 512;
            for (i = 0; i < 3; i++)
            {
                K052109_tilemap[i].rowscroll = new int[K052109_tilemap[i].scrollrows];
                K052109_tilemap[i].colscroll = new int[K052109_tilemap[1].scrollcols];
                K052109_tilemap[i].tilemap_draw_instance3 = K052109_tilemap[i].tilemap_draw_instanceKonami68000;
                K052109_tilemap[i].pixmap = new ushort[0x100 * 0x200];
                K052109_tilemap[i].flagsmap = new byte[0x100, 0x200];
                K052109_tilemap[i].tileflags = new byte[0x20, 0x40];
                K052109_tilemap[i].pen_data_set = new byte[0x40];
                K052109_tilemap[i].pen_to_flags = new byte[1, 16];
                K052109_tilemap[i].pen_to_flags[0, 0] = 0;
                for (j = 1; j < 16; j++)
                {
                    K052109_tilemap[i].pen_to_flags[0, j] = 0x10;
                }
                K052109_tilemap[i].total_elements = gfx12romLength / 0x40;
            }
            K052109_tilemap[0].tile_update3 = K052109_tilemap[0].tile_updateKonami68000_0;
            K052109_tilemap[1].tile_update3 = K052109_tilemap[1].tile_updateKonami68000_1;
            K052109_tilemap[2].tile_update3 = K052109_tilemap[2].tile_updateKonami68000_2;
            for (i = 0; i < 3; i++)
            {
                K052109_dx[i] = K052109_dy[i] = 0;
            }
        }
        public static byte K052109_r(int offset)
        {
            if (K052109_RMRD_line == LineState.CLEAR_LINE)
            {
                return K052109_ram[offset];
            }
            else
            {
                int code = (offset & 0x1fff) >> 5;
                int color = K052109_romsubbank;
                int code2, color2, flags2;
                int flags = 0;
                int priority = 0;
                int bank = K052109_charrombank[(color & 0x0c) >> 2] >> 2;
                int addr;
                bank |= (K052109_charrombank_2[(color & 0x0c) >> 2] >> 2);
                if (has_extra_video_ram != 0)
                {
                    code |= color << 8;
                }
                else
                {
                    K052109_callback(0, bank, code, color, flags, priority, out code2, out color2, out flags2);
                    code = code2;
                    color = color2;
                }
                addr = (code << 5) + (offset & 0x1f);
                addr &= K052109_memory_region.Length - 1;
                return K052109_memory_region[addr];
            }
        }
        public static void K052109_w(int offset, byte data)
        {
            int row, col;
            if (offset == 0x90d)
            {
                int i1 = 1;
            }
            if (offset == 0x290d)
            {
                int i1 = 1;
            }
            if ((offset & 0x1fff) < 0x1800)
            {
                if (offset >= 0x4000)
                {
                    has_extra_video_ram = 1;
                }
                K052109_ram[offset] = data;
                row = (offset & 0x7ff) / 0x40;
                col = (offset & 0x7ff) % 0x40;
                K052109_tilemap[(offset & 0x1800) >> 11].tilemap_mark_tile_dirty(row, col);
                //tilemap_mark_tile_dirty(K052109_tilemap[(offset & 0x1800) >> 11], offset & 0x7ff);
            }
            else
            {
                K052109_ram[offset] = data;
                if (offset >= 0x180c && offset < 0x1834)
                {

                }
                else if (offset >= 0x1a00 && offset < 0x1c00)
                {

                }
                else if (offset == 0x1c80)
                {
                    if (K052109_scrollctrl != data)
                    {
                        K052109_scrollctrl = data;
                    }
                }
                else if (offset == 0x1d00)
                {
                    K052109_irq_enabled = (byte)(data & 0x04);
                }
                else if (offset == 0x1d80)
                {
                    int dirty = 0;
                    if (K052109_charrombank[0] != (data & 0x0f)) dirty |= 1;
                    if (K052109_charrombank[1] != ((data >> 4) & 0x0f)) dirty |= 2;
                    if (dirty != 0)
                    {
                        int i;
                        K052109_charrombank[0] = (byte)(data & 0x0f);
                        K052109_charrombank[1] = (byte)((data >> 4) & 0x0f);
                        for (i = 0; i < 0x1800; i++)
                        {
                            int bank = (K052109_ram[i] & 0x0c) >> 2;
                            if ((bank == 0 && ((dirty & 1) != 0) || (bank == 1 && ((dirty & 2) != 0))))
                            {
                                row = (i & 0x7ff) / 0x40;
                                col = (i & 0x7ff) % 0x40;
                                K052109_tilemap[(i & 0x1800) >> 11].tilemap_mark_tile_dirty(row, col);
                                //tilemap_mark_tile_dirty(K052109_tilemap[(i & 0x1800) >> 11], i & 0x7ff);
                            }
                        }
                    }
                }
                else if (offset == 0x1e00 || offset == 0x3e00)
                {
                    K052109_romsubbank = data;
                }
                else if (offset == 0x1e80)
                {
                    //tilemap_set_flip(K052109_tilemap[0], (data & 1) ? (TILEMAP_FLIPY | TILEMAP_FLIPX) : 0);
                    //tilemap_set_flip(K052109_tilemap[1], (data & 1) ? (TILEMAP_FLIPY | TILEMAP_FLIPX) : 0);
                    //tilemap_set_flip(K052109_tilemap[2], (data & 1) ? (TILEMAP_FLIPY | TILEMAP_FLIPX) : 0);
                    if (K052109_tileflip_enable != ((data & 0x06) >> 1))
                    {
                        K052109_tileflip_enable = ((data & 0x06) >> 1);
                        K052109_tilemap[0].all_tiles_dirty = true;
                        K052109_tilemap[1].all_tiles_dirty = true;
                        K052109_tilemap[2].all_tiles_dirty = true;
                    }
                }
                else if (offset == 0x1f00)
                {
                    int dirty = 0;
                    if (K052109_charrombank[2] != (data & 0x0f)) dirty |= 1;
                    if (K052109_charrombank[3] != ((data >> 4) & 0x0f)) dirty |= 2;
                    if (dirty != 0)
                    {
                        int i;
                        K052109_charrombank[2] = (byte)(data & 0x0f);
                        K052109_charrombank[3] = (byte)((data >> 4) & 0x0f);
                        for (i = 0; i < 0x1800; i++)
                        {
                            int bank = (K052109_ram[i] & 0x0c) >> 2;
                            if ((bank == 2 && ((dirty & 1) != 0)) || (bank == 3 && ((dirty & 2) != 0)))
                            {
                                row = (i & 0x7ff) / 0x40;
                                col = (i & 0x7ff) % 0x40;
                                K052109_tilemap[(i & 0x1800) >> 11].tilemap_mark_tile_dirty(row, col);
                                //tilemap_mark_tile_dirty(K052109_tilemap[(i & 0x1800) >> 11], i & 0x7ff);
                            }
                        }
                    }
                }
                else if (offset >= 0x380c && offset < 0x3834)
                {

                }
                else if (offset >= 0x3a00 && offset < 0x3c00)
                {

                }
                else if (offset == 0x3d80)
                {
                    K052109_charrombank_2[0] = (byte)(data & 0x0f);
                    K052109_charrombank_2[1] = (byte)((data >> 4) & 0x0f);
                }
                else if (offset == 0x3f00)
                {
                    K052109_charrombank_2[2] = (byte)(data & 0x0f);
                    K052109_charrombank_2[3] = (byte)((data >> 4) & 0x0f);
                }
            }
        }
        public static ushort K052109_word_r(int offset)
        {
            return (ushort)(K052109_r(offset + 0x2000) | (K052109_r(offset) << 8));
        }
        public static void K052109_word_w(int offset, ushort data)
        {
            K052109_w(offset, (byte)((data >> 8) & 0xff));
            K052109_w(offset + 0x2000, (byte)(data & 0xff));
        }
        public static void K052109_word_w1(int offset, byte data)
        {
            K052109_w(offset, data);
        }
        public static void K052109_word_w2(int offset, byte data)
        {
            K052109_w(offset + 0x2000, data);
        }
        public static void K052109_set_RMRD_line(LineState state)
        {
            K052109_RMRD_line = state;
        }
        public static int K052109_get_RMRD_line()
        {
            return (int)K052109_RMRD_line;
        }
        public static void K052109_tilemap_update()
        {
            if ((K052109_scrollctrl & 0x03) == 0x02)
            {
                int xscroll, yscroll, offs;
                int scrollram_offset = 0x1a00;
                K052109_tilemap[1].tilemap_set_scroll_rows(256);
                K052109_tilemap[1].tilemap_set_scroll_cols(1);
                yscroll = K052109_ram[0x180c];
                K052109_tilemap[1].tilemap_set_scrolly(0, yscroll + K052109_dy[1]);
                for (offs = 0; offs < 256; offs++)
                {
                    xscroll = K052109_ram[scrollram_offset + 2 * (offs & 0xfff8) + 0] + 256 * K052109_ram[scrollram_offset + 2 * (offs & 0xfff8) + 1];
                    xscroll -= 6;
                    K052109_tilemap[1].tilemap_set_scrollx((offs + yscroll) & 0xff, xscroll + K052109_dx[1]);
                }
            }
            else if ((K052109_scrollctrl & 0x03) == 0x03)
            {
                int xscroll, yscroll, offs;
                int scrollram_offset = 0x1a00;
                K052109_tilemap[1].tilemap_set_scroll_rows(256);
                K052109_tilemap[1].tilemap_set_scroll_cols(1);
                yscroll = K052109_ram[0x180c];
                K052109_tilemap[1].tilemap_set_scrolly(0, yscroll + K052109_dy[1]);
                for (offs = 0; offs < 256; offs++)
                {
                    xscroll = K052109_ram[scrollram_offset + 2 * offs + 0] + 256 * K052109_ram[scrollram_offset + 2 * offs + 1];
                    xscroll -= 6;
                    K052109_tilemap[1].tilemap_set_scrollx((offs + yscroll) & 0xff, xscroll + K052109_dx[1]);
                }
            }
            else if ((K052109_scrollctrl & 0x04) == 0x04)
            {
                int xscroll, yscroll, offs;
                int scrollram_offset = 0x1800;
                K052109_tilemap[1].tilemap_set_scroll_rows(1);
                K052109_tilemap[1].tilemap_set_scroll_cols(512);
                xscroll = K052109_ram[0x1a00] + 256 * K052109_ram[0x1a01];
                xscroll -= 6;
                K052109_tilemap[1].tilemap_set_scrollx(0, xscroll + K052109_dx[1]);
                for (offs = 0; offs < 512; offs++)
                {
                    yscroll = K052109_ram[scrollram_offset + offs / 8];
                    K052109_tilemap[1].tilemap_set_scrolly((offs + xscroll) & 0x1ff, yscroll + K052109_dy[1]);
                }
            }
            else
            {
                int xscroll, yscroll;
                int scrollram_offset = 0x1a00;
                K052109_tilemap[1].tilemap_set_scroll_rows(1);
                K052109_tilemap[1].tilemap_set_scroll_cols(1);
                xscroll = K052109_ram[scrollram_offset + 0] + 256 * K052109_ram[scrollram_offset + 1];
                xscroll -= 6;
                yscroll = K052109_ram[0x180c];
                K052109_tilemap[1].tilemap_set_scrollx(0, xscroll + K052109_dx[1]);
                K052109_tilemap[1].tilemap_set_scrolly(0, yscroll + K052109_dy[1]);
            }
            if ((K052109_scrollctrl & 0x18) == 0x10)
            {
                int xscroll, yscroll, offs;
                int scrollram_offset = 0x3a00;
                K052109_tilemap[2].tilemap_set_scroll_rows(256);
                K052109_tilemap[2].tilemap_set_scroll_cols(1);
                yscroll = K052109_ram[0x380c];
                K052109_tilemap[2].tilemap_set_scrolly(0, yscroll + K052109_dy[2]);
                for (offs = 0; offs < 256; offs++)
                {
                    xscroll = K052109_ram[scrollram_offset + 2 * (offs & 0xfff8) + 0] + 256 * K052109_ram[scrollram_offset + 2 * (offs & 0xfff8) + 1];
                    xscroll -= 6;
                    K052109_tilemap[2].tilemap_set_scrollx((offs + yscroll) & 0xff, xscroll + K052109_dx[2]);
                }
            }
            else if ((K052109_scrollctrl & 0x18) == 0x18)
            {
                int xscroll, yscroll, offs;
                int scrollram_offset = 0x3a00;
                K052109_tilemap[2].tilemap_set_scroll_rows(256);
                K052109_tilemap[2].tilemap_set_scroll_cols(1);
                yscroll = K052109_ram[0x380c];
                K052109_tilemap[2].tilemap_set_scrolly(0, yscroll + K052109_dy[2]);
                for (offs = 0; offs < 256; offs++)
                {
                    xscroll = K052109_ram[scrollram_offset + 2 * offs + 0] + 256 * K052109_ram[scrollram_offset + 2 * offs + 1];
                    xscroll -= 6;
                    K052109_tilemap[2].tilemap_set_scrollx((offs + yscroll) & 0xff, xscroll + K052109_dx[2]);
                }
            }
            else if ((K052109_scrollctrl & 0x20) == 0x20)
            {
                int xscroll, yscroll, offs;
                int scrollram_offset = 0x3800;
                K052109_tilemap[2].tilemap_set_scroll_rows(1);
                K052109_tilemap[2].tilemap_set_scroll_cols(512);
                xscroll = K052109_ram[0x3a00] + 256 * K052109_ram[0x3a01];
                xscroll -= 6;
                K052109_tilemap[2].tilemap_set_scrollx(0, xscroll + K052109_dx[2]);
                for (offs = 0; offs < 512; offs++)
                {
                    yscroll = K052109_ram[scrollram_offset + offs / 8];
                    K052109_tilemap[2].tilemap_set_scrolly((offs + xscroll) & 0x1ff, yscroll + K052109_dy[2]);
                }
            }
            else
            {
                int xscroll, yscroll;
                int scrollram_offset = 0x3a00;
                K052109_tilemap[2].tilemap_set_scroll_rows(1);
                K052109_tilemap[2].tilemap_set_scroll_cols(1);
                xscroll = K052109_ram[scrollram_offset + 0] + 256 * K052109_ram[scrollram_offset + 1];
                xscroll -= 6;
                yscroll = K052109_ram[0x380c];
                K052109_tilemap[2].tilemap_set_scrollx(0, xscroll + K052109_dx[2]);
                K052109_tilemap[2].tilemap_set_scrolly(0, yscroll + K052109_dy[2]);
            }
        }
        public static int K052109_is_IRQ_enabled()
        {
            return K052109_irq_enabled;
        }
        public static void SaveStateBinary_K052109(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(K052109_ram, 0, 0x6000);
            writer.Write((int)K052109_RMRD_line);
            writer.Write(K052109_romsubbank);
            writer.Write(K052109_scrollctrl);
            writer.Write(K052109_irq_enabled);
            writer.Write(K052109_charrombank, 0, 4);
            writer.Write(K052109_charrombank_2, 0, 4);
            for (i = 0; i < 3; i++)
            {
                writer.Write(K052109_dx[i]);
            }
            for (i = 0; i < 3; i++)
            {
                writer.Write(K052109_dy[i]);
            }
            writer.Write(has_extra_video_ram);
            writer.Write(K052109_tileflip_enable);
        }
        public static void LoadStateBinary_K052109(System.IO.BinaryReader reader)
        {
            int i;
            K052109_ram = reader.ReadBytes(0x6000);
            K052109_RMRD_line = (LineState)reader.ReadInt32();
            K052109_romsubbank = reader.ReadByte();
            K052109_scrollctrl = reader.ReadByte();
            K052109_irq_enabled = reader.ReadByte();
            K052109_charrombank = reader.ReadBytes(4);
            K052109_charrombank_2 = reader.ReadBytes(4);
            for (i = 0; i < 3; i++)
            {
                K052109_dx[i] = reader.ReadInt32();
            }
            for (i = 0; i < 3; i++)
            {
                K052109_dy[i] = reader.ReadInt32();
            }
            has_extra_video_ram = reader.ReadByte();
            K052109_tileflip_enable = reader.ReadInt32();
        }
        public static void LoadStateBinary_K052109_2(System.IO.BinaryReader reader)
        {
            int i;
            reader.ReadBytes(0x6000);
            reader.ReadInt32();
            reader.ReadByte();
            reader.ReadByte();
            reader.ReadByte();
            reader.ReadBytes(4);
            reader.ReadBytes(4);
            for (i = 0; i < 3; i++)
            {
                reader.ReadInt32();
            }
            for (i = 0; i < 3; i++)
            {
                reader.ReadInt32();
            }
            reader.ReadByte();
            reader.ReadInt32();
        }
        public static void K051960_vh_start()
        {
            K051960_dx = K051960_dy = 0;
            K051960_ram = new byte[0x400];
            K051960_spriterombank = new byte[3];
        }
        public static byte K051960_fetchromdata(int byte1)
        {
            int code, color, pri, shadow, off1, addr, code2, color2, pri2;
            addr = K051960_romoffset + (K051960_spriterombank[0] << 8) +
                    ((K051960_spriterombank[1] & 0x03) << 16);
            code = (addr & 0x3ffe0) >> 5;
            off1 = addr & 0x1f;
            color = ((K051960_spriterombank[1] & 0xfc) >> 2) + ((K051960_spriterombank[2] & 0x03) << 6);
            pri = 0;
            shadow = color & 0x80;
            K051960_callback(code, color, pri, shadow, out code2, out color2, out pri2);
            addr = (code2 << 7) | (off1 << 2) | byte1;
            addr &= K051960_memory_region.Length - 1;
            return K051960_memory_region[addr];
        }
        public static byte K051960_r(int offset)
        {
            if (K051960_readroms != 0)
            {
                K051960_romoffset = (offset & 0x3fc) >> 2;
                return K051960_fetchromdata(offset & 3);
            }
            else
            {
                return K051960_ram[offset];
            }
        }
        public static void K051960_w(int offset, byte data)
        {
            K051960_ram[offset] = data;
        }
        public static byte K051937_r(int offset)
        {
            if (K051960_readroms != 0 && offset >= 4 && offset < 8)
            {
                return K051960_fetchromdata(offset & 3);
            }
            else
            {
                if (offset == 0)
                {
                    return (byte)((counter++) & 1);
                }
                return 0;
            }
        }
        public static void K051937_w(int offset, byte data)
        {
            if (offset == 0)
            {
                K051960_irq_enabled = (data & 0x01);
                K051960_nmi_enabled = (data & 0x04);
                K051960_spriteflip = data & 0x08;
                K051960_readroms = data & 0x20;
            }
            else if (offset == 1)
            {

            }
            else if (offset >= 2 && offset < 5)
            {
                K051960_spriterombank[offset - 2] = data;
            }
            else
            {

            }
        }
        readonly static int[] K051960_sprites_draw_xoffset = new int[] { 0, 1, 4, 5, 16, 17, 20, 21 };
        readonly static int[] K051960_sprites_draw_yoffset = new int[] { 0, 2, 8, 10, 32, 34, 40, 42 };
        readonly static int[] K051960_sprites_draw_width = new int[] { 1, 2, 1, 2, 4, 2, 4, 8 };
        readonly static int[] K051960_sprites_draw_height = new int[] { 1, 1, 2, 2, 2, 4, 4, 8 };
        static int[] K051960_sprites_draw_sortedlist = new int[128];
        public static void K051960_sprites_draw(RECT cliprect, int min_priority, int max_priority)
        {
            int ox, oy, code, color, pri, shadow, size, w, h, x, y, flipx, flipy, zoomx, zoomy, code2, color2, pri2;
            int offs, pri_code;
            for (offs = 0; offs < 128; offs++)
            {
                K051960_sprites_draw_sortedlist[offs] = -1;
            }
            for (offs = 0; offs < 0x400; offs += 8)
            {
                if ((K051960_ram[offs] & 0x80) != 0)
                {
                    if (max_priority == -1)
                    {
                        K051960_sprites_draw_sortedlist[(K051960_ram[offs] & 0x7f) ^ 0x7f] = offs;
                    }
                    else
                    {
                        K051960_sprites_draw_sortedlist[K051960_ram[offs] & 0x7f] = offs;
                    }
                }
            }
            for (pri_code = 0; pri_code < 128; pri_code++)
            {
                offs = K051960_sprites_draw_sortedlist[pri_code];
                if (offs == -1)
                {
                    continue;
                }
                code = K051960_ram[offs + 2] + ((K051960_ram[offs + 1] & 0x1f) << 8);
                color = K051960_ram[offs + 3] & 0xff;
                pri = 0;
                shadow = color & 0x80;
                K051960_callback(code, color, pri, shadow, out code2, out color2, out pri2);
                code = code2;
                color = color2;
                pri = pri2;
                if (max_priority != -1)
                {
                    if (pri < min_priority || pri > max_priority)
                    {
                        continue;
                    }
                }
                size = (K051960_ram[offs + 1] & 0xe0) >> 5;
                w = K051960_sprites_draw_width[size];
                h = K051960_sprites_draw_height[size];
                if (w >= 2) code &= ~0x01;
                if (h >= 2) code &= ~0x02;
                if (w >= 4) code &= ~0x04;
                if (h >= 4) code &= ~0x08;
                if (w >= 8) code &= ~0x10;
                if (h >= 8) code &= ~0x20;
                ox = (256 * K051960_ram[offs + 6] + K051960_ram[offs + 7]) & 0x01ff;
                oy = 256 - ((256 * K051960_ram[offs + 4] + K051960_ram[offs + 5]) & 0x01ff);
                ox += K051960_dx;
                oy += K051960_dy;
                flipx = K051960_ram[offs + 6] & 0x02;
                flipy = K051960_ram[offs + 4] & 0x02;
                zoomx = (K051960_ram[offs + 6] & 0xfc) >> 2;
                zoomy = (K051960_ram[offs + 4] & 0xfc) >> 2;
                zoomx = 0x10000 / 128 * (128 - zoomx);
                zoomy = 0x10000 / 128 * (128 - zoomy);
                if (K051960_spriteflip != 0)
                {
                    ox = 512 - (zoomx * w >> 12) - ox;
                    oy = 256 - (zoomy * h >> 12) - oy;
                    flipx = (flipx == 0) ? 1 : 0;
                    flipy = (flipy == 0) ? 1 : 0;
                }
                if (zoomx == 0x10000 && zoomy == 0x10000)
                {
                    int sx, sy;
                    for (y = 0; y < h; y++)
                    {
                        sy = oy + 16 * y;
                        for (x = 0; x < w; x++)
                        {
                            int c = code;
                            sx = ox + 16 * x;
                            if (flipx != 0)
                            {
                                c += K051960_sprites_draw_xoffset[(w - 1 - x)];
                            }
                            else
                            {
                                c += K051960_sprites_draw_xoffset[x];
                            }
                            if (flipy != 0)
                            {
                                c += K051960_sprites_draw_yoffset[(h - 1 - y)];
                            }
                            else
                            {
                                c += K051960_sprites_draw_yoffset[y];
                            }
                            if (max_priority == -1)
                            {
                                common_drawgfx_konami68000(gfx22rom, c, color, flipx, flipy, sx & 0x1ff, sy, cliprect, (uint)(pri | (1 << 31)));
                                /*pdrawgfx(bitmap, K051960_gfx,
                                        c,
                                        color,
                                        flipx, flipy,
                                        sx & 0x1ff, sy,
                                        cliprect, shadow ? TRANSPARENCY_PEN_TABLE : TRANSPARENCY_PEN, 0, pri);*/
                            }
                            else
                            {
                                common_drawgfx_konami68000(gfx22rom, c, color, flipx, flipy, sx & 0x1ff, sy, cliprect, 0);
                                /*drawgfx(bitmap, K051960_gfx,
                                        c,
                                        color,
                                        flipx, flipy,
                                        sx & 0x1ff, sy,
                                        cliprect, shadow ? TRANSPARENCY_PEN_TABLE : TRANSPARENCY_PEN, 0);*/
                            }
                        }
                    }
                }
                else
                {
                    int sx, sy, zw, zh;
                    for (y = 0; y < h; y++)
                    {
                        sy = oy + ((zoomy * y + (1 << 11)) >> 12);
                        zh = (oy + ((zoomy * (y + 1) + (1 << 11)) >> 12)) - sy;
                        for (x = 0; x < w; x++)
                        {
                            int c = code;
                            sx = ox + ((zoomx * x + (1 << 11)) >> 12);
                            zw = (ox + ((zoomx * (x + 1) + (1 << 11)) >> 12)) - sx;
                            if (flipx != 0)
                            {
                                c += K051960_sprites_draw_xoffset[(w - 1 - x)];
                            }
                            else c += K051960_sprites_draw_xoffset[x];
                            if (flipy != 0)
                            {
                                c += K051960_sprites_draw_yoffset[(h - 1 - y)];
                            }
                            else
                            {
                                c += K051960_sprites_draw_yoffset[y];
                            }
                            if (max_priority == -1)
                            {
                                common_drawgfxzoom_konami68000(gfx22rom, c, color, flipx, flipy, sx & 0x1ff, sy, cliprect, 0, (zw << 16) / 16, (zh << 16) / 16, (uint)(pri | (1 << 31)));
                                /*pdrawgfxzoom(bitmap, K051960_gfx,
                                        c,
                                        color,
                                        flipx, flipy,
                                        sx & 0x1ff, sy,
                                        cliprect, shadow ? TRANSPARENCY_PEN_TABLE : TRANSPARENCY_PEN, 0,
                                        (zw << 16) / 16, (zh << 16) / 16, pri);*/
                            }
                            else
                            {
                                common_drawgfxzoom_konami68000(gfx22rom, c, color, flipx, flipy, sx & 0x1ff, sy, cliprect, 0, (zw << 16) / 16, (zh << 16) / 16);
                                /*drawgfxzoom(bitmap, K051960_gfx,
                                        c,
                                        color,
                                        flipx, flipy,
                                        sx & 0x1ff, sy,
                                        cliprect, shadow ? TRANSPARENCY_PEN_TABLE : TRANSPARENCY_PEN, 0,
                                        (zw << 16) / 16, (zh << 16) / 16);*/
                            }
                        }
                    }
                }
            }
        }
        public static void SaveStateBinary_K051960(System.IO.BinaryWriter writer)
        {
            writer.Write(K051960_romoffset);
            writer.Write(K051960_spriteflip);
            writer.Write(K051960_readroms);
            writer.Write(K051960_spriterombank, 0, 3);
            writer.Write(K051960_ram, 0, 0x400);
            writer.Write(K051960_dx);
            writer.Write(K051960_dy);
            writer.Write(K051960_irq_enabled);
            writer.Write(K051960_nmi_enabled);
        }
        public static void LoadStateBinary_K051960(System.IO.BinaryReader reader)
        {
            K051960_romoffset = reader.ReadInt32();
            K051960_spriteflip = reader.ReadInt32();
            K051960_readroms = reader.ReadInt32();
            K051960_spriterombank = reader.ReadBytes(3);
            K051960_ram = reader.ReadBytes(0x400);
            K051960_dx = reader.ReadInt32();
            K051960_dy = reader.ReadInt32();
            K051960_irq_enabled = reader.ReadInt32();
            K051960_nmi_enabled = reader.ReadInt32();
        }
        public static void LoadStateBinary_K051960_2(System.IO.BinaryReader reader)
        {
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadBytes(3);
            reader.ReadBytes(0x400);
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
        }
        public static void K05324x_set_z_rejection(int zcode)
        {
            K05324x_z_rejection = zcode;
        }
        public static void K053245_vh_start()
        {
            int i;
            Drawgfx.gfx_drawmode_table[0] = 0;
            for (i = 1; i < 15; i++)
            {
                Drawgfx.gfx_drawmode_table[i] = 1;
            }
            Drawgfx.gfx_drawmode_table[15] = 2;
            K05324x_z_rejection = -1;
            K053244_rombank[0] = 0;
            K053245_ramsize[0] = 0x800;
            K053245_ram[0] = new byte[K053245_ramsize[0]];
            K053245_dx[0] = K053245_dy[0] = 0;
            K053245_buffer[0] = new ushort[K053245_ramsize[0] / 2];
            for (i = 0; i < K053245_ramsize[0]; i++)
            {
                K053245_ram[0][i] = 0;
            }
            for (i = 0; i < K053245_ramsize[0] / 2; i++)
            {
                K053245_buffer[0][i] = 0;
            }
        }
        public static ushort K053245_word_r(int offset)
        {
            return (ushort)(K053245_ram[0][offset * 2] * 0x100 + K053245_ram[0][offset * 2 + 1]);
        }
        public static void K053245_word_w(int offset, ushort data)
        {
            K053245_ram[0][offset * 2] = (byte)(data >> 8);
            K053245_ram[0][offset * 2 + 1] = (byte)data;
        }
        public static void K053245_word_w2(int offset, ushort data)
        {
            K053245_ram[0][offset * 2 + 1] = (byte)data;
        }
        public static void K053245_clear_buffer(int chip)
        {
            int i, e;
            for (e = K053245_ramsize[chip] / 2, i = 0; i < e; i += 8)
            {
                K053245_buffer[chip][i] = 0;
            }
        }
        public static void K053245_update_buffer(int chip)
        {
            int i;
            for (i = 0; i < K053245_ramsize[chip] / 2; i++)
            {
                K053245_buffer[chip][i] = (ushort)(K053245_ram[chip][i * 2] * 0x100 + K053245_ram[chip][i * 2 + 1]);
            }
        }
        public static byte K053244_chip_r(int chip, int offset)
        {
            if ((K053244_regs[chip][5] & 0x10) != 0 && offset >= 0x0c && offset < 0x10)
            {
                int addr;
                addr = (K053244_rombank[chip] << 19) | ((K053244_regs[chip][11] & 0x7) << 18) | (K053244_regs[chip][8] << 10) | (K053244_regs[chip][9] << 2) | ((offset & 3) ^ 1);
                addr &= K053245_memory_region[chip].Length - 1;
                return K053245_memory_region[chip][addr];
            }
            else if (offset == 0x06)
            {
                K053245_update_buffer(chip);
                return 0;
            }
            else
            {
                return 0;
            }
        }
        public static byte K053244_r(int offset)
        {
            return K053244_chip_r(0, offset);
        }
        public static void K053244_chip_w(int chip, int offset, byte data)
        {
            K053244_regs[chip][offset] = data;
            switch (offset)
            {
                case 0x05:
                    {
                        break;
                    }
                case 0x06:
                    K053245_update_buffer(chip);
                    break;
            }
        }
        public static void K053244_w(int offset, byte data)
        {
            K053244_chip_w(0, offset, data);
        }
        public static ushort K053244_lsb_r(int offset)
        {
            return (ushort)K053244_r(offset);
        }
        public static void K053244_lsb_w(int offset, ushort data)
        {
            //if (ACCESSING_BITS_0_7)
            K053244_w(offset, (byte)(data & 0xff));
        }
        public static void K053244_lsb_w2(int offset, byte data)
        {
            K053244_w(offset, data);
        }
        public static void K053244_bankselect(int chip, int bank)
        {
            K053244_rombank[chip] = bank;
        }
        public static void K053245_sprites_draw(RECT cliprect)
        {
            int offs, pri_code, i;
            int[] sortedlist = new int[128];
            int flipscreenX, flipscreenY, spriteoffsX, spriteoffsY;
            flipscreenX = K053244_regs[0][5] & 0x01;
            flipscreenY = K053244_regs[0][5] & 0x02;
            spriteoffsX = (K053244_regs[0][0] << 8) | K053244_regs[0][1];
            spriteoffsY = (K053244_regs[0][2] << 8) | K053244_regs[0][3];
            for (offs = 0; offs < 128; offs++)
            {
                sortedlist[offs] = -1;
            }
            i = K053245_ramsize[0] / 2;
            for (offs = 0; offs < i; offs += 8)
            {
                pri_code = K053245_buffer[0][offs];
                if ((pri_code & 0x8000) != 0)
                {
                    pri_code &= 0x007f;
                    if (offs != 0 && pri_code == K05324x_z_rejection)
                    {
                        continue;
                    }
                    if (sortedlist[pri_code] == -1)
                    {
                        sortedlist[pri_code] = offs;
                    }
                }
            }
            for (pri_code = 127; pri_code >= 0; pri_code--)
            {
                int ox, oy, color, color2, code, code2, size, w, h, x, y, flipx, flipy, mirrorx, mirrory, shadow, zoomx, zoomy, pri, pri2;
                offs = sortedlist[pri_code];
                if (offs == -1)
                {
                    continue;
                }
                code = K053245_buffer[0][offs + 1];
                code = ((code & 0xffe1) + ((code & 0x0010) >> 2) + ((code & 0x0008) << 1) + ((code & 0x0004) >> 1) + ((code & 0x0002) << 2));
                color = K053245_buffer[0][offs + 6] & 0x00ff;
                pri = 0;
                K053245_callback(code, color, out code2, out color2, out pri2);
                size = (K053245_buffer[0][offs] & 0x0f00) >> 8;
                w = 1 << (size & 0x03);
                h = 1 << ((size >> 2) & 0x03);
                zoomy = K053245_buffer[0][offs + 4];
                if (zoomy > 0x2000)
                {
                    continue;
                }
                if (zoomy != 0)
                {
                    zoomy = (0x400000 + zoomy / 2) / zoomy;
                }
                else
                {
                    zoomy = 2 * 0x400000;
                }
                if ((K053245_buffer[0][offs] & 0x4000) == 0)
                {
                    zoomx = K053245_buffer[0][offs + 5];
                    if (zoomx > 0x2000)
                    {
                        continue;
                    }
                    if (zoomx != 0)
                    {
                        zoomx = (0x400000 + zoomx / 2) / zoomx;
                    }
                    else
                    {
                        zoomx = 2 * 0x400000;
                    }
                }
                else
                {
                    zoomx = zoomy;
                }
                ox = K053245_buffer[0][offs + 3] + spriteoffsX;
                oy = K053245_buffer[0][offs + 2];
                ox += K053245_dx[0];
                oy += K053245_dy[0];
                flipx = K053245_buffer[0][offs] & 0x1000;
                flipy = K053245_buffer[0][offs] & 0x2000;
                mirrorx = K053245_buffer[0][offs + 6] & 0x0100;
                if (mirrorx != 0)
                {
                    flipx = 0;
                }
                mirrory = K053245_buffer[0][offs + 6] & 0x0200;
                shadow = K053245_buffer[0][offs + 6] & 0x0080;
                if (flipscreenX != 0)
                {
                    ox = 512 - ox;
                    if (mirrorx == 0)
                    {
                        flipx = (flipx == 0) ? 1 : 0;
                    }
                }
                if (flipscreenY != 0)
                {
                    oy = -oy;
                    if (mirrory == 0)
                    {
                        flipy = (flipy == 0) ? 1 : 0;
                    }
                }
                ox = (ox + 0x5d) & 0x3ff;
                if (ox >= 768)
                {
                    ox -= 1024;
                }
                oy = (-(oy + spriteoffsY + 0x07)) & 0x3ff;
                if (oy >= 640)
                {
                    oy -= 1024;
                }
                ox -= (zoomx * w) >> 13;
                oy -= (zoomy * h) >> 13;
                for (y = 0; y < h; y++)
                {
                    int sx, sy, zw, zh;
                    sy = oy + ((zoomy * y + (1 << 11)) >> 12);
                    zh = (oy + ((zoomy * (y + 1) + (1 << 11)) >> 12)) - sy;
                    for (x = 0; x < w; x++)
                    {
                        int c, fx, fy;
                        sx = ox + ((zoomx * x + (1 << 11)) >> 12);
                        zw = (ox + ((zoomx * (x + 1) + (1 << 11)) >> 12)) - sx;
                        c = code2;
                        if (mirrorx != 0)
                        {
                            if ((flipx == 0) ^ (2 * x < w))
                            {
                                c += (w - x - 1);
                                fx = 1;
                            }
                            else
                            {
                                c += x;
                                fx = 0;
                            }
                        }
                        else
                        {
                            if (flipx != 0)
                            {
                                c += w - 1 - x;
                            }
                            else
                            {
                                c += x;
                            }
                            fx = flipx;
                        }
                        if (mirrory != 0)
                        {
                            if ((flipy == 0) ^ (2 * y >= h))
                            {
                                c += 8 * (h - y - 1);
                                fy = 1;
                            }
                            else
                            {
                                c += 8 * y;
                                fy = 0;
                            }
                        }
                        else
                        {
                            if (flipy != 0)
                            {
                                c += 8 * (h - 1 - y);
                            }
                            else
                            {
                                c += 8 * y;
                            }
                            fy = flipy;
                        }
                        c = (c & 0x3f) | (code2 & ~0x3f);
                        if (zoomx == 0x10000 && zoomy == 0x10000)
                        {
                            common_drawgfx_konami68000(gfx22rom, c, color2, fx, fy, sx, sy, cliprect, (uint)(pri2 | (1 << 31)));
                            /*pdrawgfx(bitmap, K053245_gfx[chip],
                                    c,
                                    color2,
                                    fx, fy,
                                    sx, sy,
                                    cliprect, shadow ? TRANSPARENCY_PEN_TABLE : TRANSPARENCY_PEN, 0, pri);*/
                        }
                        else
                        {
                            common_drawgfxzoom_konami68000(gfx22rom, c, color2, fx, fy, sx, sy, cliprect, 0, (zw << 16) / 16, (zh << 16) / 16, (uint)(pri2 | 1 << 31));
                            /*pdrawgfxzoom(bitmap, K053245_gfx[chip],
                                    c,
                                    color2,
                                    fx, fy,
                                    sx, sy,
                                    cliprect, shadow ? TRANSPARENCY_PEN_TABLE : TRANSPARENCY_PEN, 0,
                                    (zw << 16) / 16, (zh << 16) / 16, pri);*/
                        }
                    }
                }
            }
        }
        public static void SaveStateBinary_K053245(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(K05324x_z_rejection);
            writer.Write(K053245_ram[0], 0, 0x800);
            for (i = 0; i < 0x400; i++)
            {
                writer.Write(K053245_buffer[0][i]);
            }
            for (i = 0; i < 2; i++)
            {
                writer.Write(K053244_rombank[i]);
            }
            for (i = 0; i < 2; i++)
            {
                writer.Write(K053245_dx[i]);
            }
            for (i = 0; i < 2; i++)
            {
                writer.Write(K053245_dy[i]);
            }
            writer.Write(K053244_regs[0], 0, 0x10);
            writer.Write(K054000_ram, 0, 0x20);
        }
        public static void LoadStateBinary_K053245(System.IO.BinaryReader reader)
        {
            int i;
            K05324x_z_rejection = reader.ReadInt32();
            K053245_ram[0] = reader.ReadBytes(0x800);
            for (i = 0; i < 0x400; i++)
            {
                K053245_buffer[0][i] = reader.ReadUInt16();
            }
            for (i = 0; i < 2; i++)
            {
                K053244_rombank[i] = reader.ReadInt32();
            }
            for (i = 0; i < 2; i++)
            {
                K053245_dx[i] = reader.ReadInt32();
            }
            for (i = 0; i < 2; i++)
            {
                K053245_dy[i] = reader.ReadInt32();
            }
            K053244_regs[0] = reader.ReadBytes(0x10);
            K054000_ram = reader.ReadBytes(0x20);
        }
        public static void K053936_zoom_draw(int chip, ushort[] ctrl, ushort[] linectrl, RECT cliprect, Tmap tmap, int flags, uint priority)
        {
            if ((ctrl[0x07] & 0x0040) != 0)
            {
                uint startx, starty;
                int incxx, incxy;
                RECT my_clip;
                int y, maxy;
                if (((ctrl[0x07] & 0x0002) != 0) && (ctrl[0x09] != 0))
                {
                    my_clip.min_x = ctrl[0x08] + K053936_offset[chip][0] + 2;
                    my_clip.max_x = ctrl[0x09] + K053936_offset[chip][0] + 2 - 1;
                    if (my_clip.min_x < cliprect.min_x)
                    {
                        my_clip.min_x = cliprect.min_x;
                    }
                    if (my_clip.max_x > cliprect.max_x)
                    {
                        my_clip.max_x = cliprect.max_x;
                    }
                    y = ctrl[0x0a] + K053936_offset[chip][1] - 2;
                    if (y < cliprect.min_y)
                    {
                        y = cliprect.min_y;
                    }
                    maxy = ctrl[0x0b] + K053936_offset[chip][1] - 2 - 1;
                    if (maxy > cliprect.max_y)
                    {
                        maxy = cliprect.max_y;
                    }
                }
                else
                {
                    my_clip.min_x = cliprect.min_x;
                    my_clip.max_x = cliprect.max_x;
                    y = cliprect.min_y;
                    maxy = cliprect.max_y;
                }
                while (y <= maxy)
                {
                    //UINT16 *lineaddr = linectrl + 4*((y - K053936_offset[chip][1]) & 0x1ff);
                    int lineaddr_offset = 4 * ((y - K053936_offset[chip][1]) & 0x1ff);
                    my_clip.min_y = my_clip.max_y = y;
                    startx = (uint)(256 * (short)(linectrl[lineaddr_offset] + ctrl[0x00]));
                    starty = (uint)(256 * (short)(linectrl[lineaddr_offset + 1] + ctrl[0x01]));
                    incxx = (short)(linectrl[lineaddr_offset + 2]);
                    incxy = (short)(linectrl[lineaddr_offset + 3]);

                    if ((ctrl[0x06] & 0x8000) != 0)
                    {
                        incxx *= 256;
                    }
                    if ((ctrl[0x06] & 0x0080) != 0)
                    {
                        incxy *= 256;
                    }
                    startx -= (uint)(K053936_offset[chip][0] * incxx);
                    starty -= (uint)(K053936_offset[chip][0] * incxy);
                    //tilemap_draw_roz(bitmap,&my_clip,tmap,startx << 5,starty << 5,incxx << 5,incxy << 5,0,0,K053936_wraparound[chip],flags,priority);
                    y++;
                }
            }
            else
            {
                uint startx, starty;
                int incxx, incxy, incyx, incyy;
                startx = (uint)(256 * (short)(ctrl[0x00]));
                starty = (uint)(256 * (short)(ctrl[0x01]));
                incyx = (short)(ctrl[0x02]);
                incyy = (short)(ctrl[0x03]);
                incxx = (short)(ctrl[0x04]);
                incxy = (short)(ctrl[0x05]);
                if ((ctrl[0x06] & 0x4000) != 0)
                {
                    incyx *= 256; incyy *= 256;
                }
                if ((ctrl[0x06] & 0x0040) != 0)
                {
                    incxx *= 256; incxy *= 256;
                }
                startx -= (uint)(K053936_offset[chip][1] * incyx);
                starty -= (uint)(K053936_offset[chip][1] * incyy);
                startx -= (uint)(K053936_offset[chip][0] * incxx);
                starty -= (uint)(K053936_offset[chip][0] * incxy);
                //tilemap_draw_roz(bitmap,cliprect,tmap,startx << 5,starty << 5,incxx << 5,incxy << 5,incyx << 5,incyy << 5,K053936_wraparound[chip],flags,priority);
            }
        }
        public static void K053936_0_zoom_draw(RECT cliprect, Tmap tmap, int flags, uint priority)
        {
            K053936_zoom_draw(0, K053936_0_ctrl, K053936_0_linectrl, cliprect, tmap, flags, priority);
        }
        public static void K053936_wraparound_enable(int chip, int status)
        {
            K053936_wraparound[chip] = status;
        }
        public static void K053936_set_offset(int chip, int xoffs, int yoffs)
        {
            K053936_offset[chip][0] = xoffs;
            K053936_offset[chip][1] = yoffs;
        }
        public static void SaveStateBinary_K053936(System.IO.BinaryWriter writer)
        {
            int i, j;
            for (i = 0; i < 0x10; i++)
            {
                writer.Write(K053936_0_ctrl[i]);
            }
            for (i = 0; i < 0x800; i++)
            {
                writer.Write(K053936_0_linectrl[i]);
            }
            for (i = 0; i < 2; i++)
            {
                for (j = 0; j < 2; j++)
                {
                    writer.Write(K053936_offset[i][j]);
                }
            }
            for (i = 0; i < 2; i++)
            {
                writer.Write(K053936_wraparound[i]);
            }
        }
        public static void LoadStateBinary_K053936(System.IO.BinaryReader reader)
        {
            int i, j;
            for (i = 0; i < 0x10; i++)
            {
                K053936_0_ctrl[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 0x800; i++)
            {
                K053936_0_linectrl[i] = reader.ReadUInt16();
            }
            for (i = 0; i < 2; i++)
            {
                for (j = 0; j < 2; j++)
                {
                    K053936_offset[i][j] = reader.ReadInt32();
                }
            }
            for (i = 0; i < 2; i++)
            {
                K053936_wraparound[i] = reader.ReadInt32();
            }
        }
        public static void K053251_vh_start()
        {
            K053251_set_tilemaps(null, null, null, null, null);
        }
        public static void K053251_set_tilemaps(Tmap ci0, Tmap ci1, Tmap ci2, Tmap ci3, Tmap ci4)
        {
            K053251_tilemaps[0] = ci0;
            K053251_tilemaps[1] = ci1;
            K053251_tilemaps[2] = ci2;
            K053251_tilemaps[3] = ci3;
            K053251_tilemaps[4] = ci4;
            if (ci0 == null && ci1 == null && ci2 == null && ci3 == null && ci4 == null)
            {
                K053251_tilemaps_set = 0;
            }
            else
            {
                K053251_tilemaps_set = 1;
            }
        }
        public static void K053251_w(int offset, byte data)
        {
            int i, newind;
            data &= 0x3f;
            if (K053251_ram[offset] != data)
            {
                K053251_ram[offset] = data;
                if (offset == 9)
                {
                    for (i = 0; i < 3; i++)
                    {
                        newind = 32 * ((data >> 2 * i) & 0x03);
                        if (K053251_palette_index[i] != newind)
                        {
                            K053251_palette_index[i] = newind;
                            if (K053251_tilemaps[i] != null)
                            {
                                K053251_tilemaps[i].all_tiles_dirty = true;
                            }
                        }
                    }
                    if (K053251_tilemaps_set == 0)
                    {
                        for (i = 0; i < 3; i++)
                        {
                            K052109_tilemap[i].all_tiles_dirty = true;
                        }
                    }
                }
                else if (offset == 10)
                {
                    for (i = 0; i < 2; i++)
                    {
                        newind = 16 * ((data >> 3 * i) & 0x07);
                        if (K053251_palette_index[3 + i] != newind)
                        {
                            K053251_palette_index[3 + i] = newind;
                            if (K053251_tilemaps[3 + i] != null)
                            {
                                K053251_tilemaps[3 + i].all_tiles_dirty = true;
                            }
                        }
                    }
                    if (K053251_tilemaps_set == 0)
                    {
                        for (i = 0; i < 3; i++)
                        {
                            K052109_tilemap[i].all_tiles_dirty = true;
                        }
                    }
                }
            }
        }
        public static void K053251_lsb_w(int offset, ushort data)
        {
            //if (ACCESSING_BITS_0_7)
            K053251_w(offset, (byte)(data & 0xff));
        }
        public static void K053251_lsb_w2(int offset, byte data)
        {
            K053251_w(offset, data);
        }
        public static void K053251_msb_w(int offset, ushort data)
        {
            //if (ACCESSING_BITS_8_15)
            K053251_w(offset, (byte)((data >> 8) & 0xff));
        }
        public static void K053251_msb_w1(int offset, byte data)
        {
            K053251_w(offset, data);
        }
        public static int K053251_get_priority(int ci)
        {
            return K053251_ram[ci];
        }
        public static int K053251_get_palette_index(int ci)
        {
            return K053251_palette_index[ci];
        }
        public static void SaveStateBinary_K053251(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(K053251_ram);
            for (i = 0; i < 5; i++)
            {
                writer.Write(K053251_palette_index[i]);
            }
            writer.Write(K053251_tilemaps_set);
        }
        public static void LoadStateBinary_K053251(System.IO.BinaryReader reader)
        {
            int i;
            K053251_ram = reader.ReadBytes(0x10);
            for (i = 0; i < 5; i++)
            {
                K053251_palette_index[i] = reader.ReadInt32();
            }
            K053251_tilemaps_set = reader.ReadInt32();
        }
        public static void LoadStateBinary_K053251_2(System.IO.BinaryReader reader)
        {
            int i;
            reader.ReadBytes(0x10);
            for (i = 0; i < 5; i++)
            {
                reader.ReadInt32();
            }
            reader.ReadInt32();
        }
        public static void K054000_w(int offset, byte data)
        {
            K054000_ram[offset] = data;
        }
        public static byte K054000_r(int offset)
        {
            int Acx, Acy, Aax, Aay;
            int Bcx, Bcy, Bax, Bay;
            if (offset != 0x18)
            {
                return 0;
            }
            Acx = (K054000_ram[0x01] << 16) | (K054000_ram[0x02] << 8) | K054000_ram[0x03];
            Acy = (K054000_ram[0x09] << 16) | (K054000_ram[0x0a] << 8) | K054000_ram[0x0b];
            if (K054000_ram[0x04] == 0xff)
            {
                Acx += 3;
            }
            if (K054000_ram[0x0c] == 0xff)
            {
                Acy += 3;
            }
            Aax = K054000_ram[0x06] + 1;
            Aay = K054000_ram[0x07] + 1;
            Bcx = (K054000_ram[0x15] << 16) | (K054000_ram[0x16] << 8) | K054000_ram[0x17];
            Bcy = (K054000_ram[0x11] << 16) | (K054000_ram[0x12] << 8) | K054000_ram[0x13];
            Bax = K054000_ram[0x0e] + 1;
            Bay = K054000_ram[0x0f] + 1;
            if (Acx + Aax < Bcx - Bax)
            {
                return 1;
            }
            if (Bcx + Bax < Acx - Aax)
            {
                return 1;
            }
            if (Acy + Aay < Bcy - Bay)
            {
                return 1;
            }
            if (Bcy + Bay < Acy - Aay)
            {
                return 1;
            }
            return 0;
        }
        public static ushort K054000_lsb_r(int offset)
        {
            return K054000_r(offset);
        }
        public static void K054000_lsb_w(int offset, ushort data)
        {
            //if (ACCESSING_BITS_0_7)
            K054000_w(offset, (byte)(data & 0xff));
        }
        public static void K054000_lsb_w2(int offset, byte data)
        {
            K054000_w(offset, data);
        }
    }
}
