using cpu.m68000;
using System;
using System.Runtime.InteropServices;

namespace MAME.Core
{
    public unsafe class Generic
    {
        //private static uint[] coin_count;
        //private static uint[] coinlockedout;
        //private static uint[] lastcoin;
        //public static byte[] videoram, colorram;
        //public static byte[] generic_nvram;
        //public static byte[] buffered_spriteram;
        //public static ushort[] buffered_spriteram16;
        //public static byte[] spriteram;
        //public static ushort[] spriteram16, spriteram16_2;
        //public static byte[] paletteram, paletteram_2;
        //public static ushort[] paletteram16, paletteram16_2;

        #region //指针化coin_count
        static uint[] coin_count_src;
        static GCHandle coin_count_handle;
        public static uint* coin_count;
        public static int coin_countLength;
        public static uint[] coin_count_set
        {
            set
            {
                coin_count_handle.ReleaseGCHandle();
                coin_count_src = value;
                coin_countLength = value.Length;
                coin_count_src.GetObjectPtr(ref coin_count_handle, ref coin_count);
            }
        }
        #endregion

        #region //指针化coinlockedout
        static uint[] coinlockedout_src;
        static GCHandle coinlockedout_handle;
        public static uint* coinlockedout;
        public static int coinlockedoutLength;
        public static uint[] coinlockedout_set
        {
            set
            {
                coinlockedout_handle.ReleaseGCHandle();
                coinlockedout_src = value;
                coinlockedoutLength = value.Length;
                coinlockedout_src.GetObjectPtr(ref coinlockedout_handle, ref coinlockedout);
            }
        }
        #endregion

        #region //指针化lastcoin
        static uint[] lastcoin_src;
        static GCHandle lastcoin_handle;
        public static uint* lastcoin;
        public static int lastcoinLength;
        public static uint[] lastcoin_set
        {
            set
            {
                lastcoin_handle.ReleaseGCHandle();
                lastcoin_src = value;
                lastcoinLength = value.Length;
                lastcoin_src.GetObjectPtr(ref lastcoin_handle, ref lastcoin);
            }
        }
        #endregion

        #region //指针化colorram
        static byte[] colorram_src;
        static GCHandle colorram_handle;
        public static byte* colorram;
        public static int colorramLength;
        public static byte[] colorram_set
        {
            set
            {
                colorram_handle.ReleaseGCHandle();
                colorram_src = value;
                colorramLength = value.Length;
                colorram_src.GetObjectPtr(ref colorram_handle, ref colorram);
            }
        }
        #endregion

        #region //指针化videoram
        static byte[] videoram_src;
        static GCHandle videoram_handle;
        public static byte* videoram;
        public static int videoramLength;
        public static byte[] videoram_set
        {
            set
            {
                videoram_handle.ReleaseGCHandle();
                if (value == null)
                    return;
                videoram_src = value;
                videoramLength = value.Length;
                videoram_src.GetObjectPtr(ref videoram_handle, ref videoram);
            }
        }
        #endregion

        #region //指针化generic_nvram
        static byte[] generic_nvram_src;
        static GCHandle generic_nvram_handle;
        public static byte* generic_nvram;
        public static int generic_nvramLength;
        public static byte[] generic_nvram_set
        {
            set
            {
                generic_nvram_handle.ReleaseGCHandle();
                generic_nvram_src = value;
                generic_nvramLength = value.Length;
                generic_nvram_src.GetObjectPtr(ref generic_nvram_handle, ref generic_nvram);
            }
        }
        #endregion

        #region //指针化buffered_spriteram
        static byte[] buffered_spriteram_src;
        static GCHandle buffered_spriteram_handle;
        public static byte* buffered_spriteram;
        public static int buffered_spriteramLength;
        public static byte[] buffered_spriteram_set
        {
            set
            {
                buffered_spriteram_handle.ReleaseGCHandle();
                buffered_spriteram_src = value;
                buffered_spriteramLength = value.Length;
                buffered_spriteram_src.GetObjectPtr(ref buffered_spriteram_handle, ref buffered_spriteram);
            }
        }
        #endregion

        #region //指针化buffered_spriteram16
        static ushort[] buffered_spriteram16_src;
        static GCHandle buffered_spriteram16_handle;
        public static ushort* buffered_spriteram16;
        public static int buffered_spriteram16Length;
        public static ushort[] buffered_spriteram16_set
        {
            set
            {
                buffered_spriteram16_handle.ReleaseGCHandle();
                buffered_spriteram16_src = value;
                buffered_spriteram16Length = value.Length;
                buffered_spriteram16_src.GetObjectPtr(ref buffered_spriteram16_handle, ref buffered_spriteram16);
            }
        }
        #endregion


        #region //指针化spriteram
        static byte[] spriteram_src;
        static GCHandle spriteram_handle;
        public static byte* spriteram;
        public static int spriteramLength;
        public static byte[] spriteram_set
        {
            set
            {
                spriteram_handle.ReleaseGCHandle();
                if (value == null)
                    return;
                spriteram_src = value;
                spriteramLength = value.Length;
                spriteram_src.GetObjectPtr(ref spriteram_handle, ref spriteram);
            }
        }
        #endregion

        #region //指针化spriteram16
        static ushort[] spriteram16_src;
        static GCHandle spriteram16_handle;
        public static ushort* spriteram16;
        public static int spriteram16Length;
        public static ushort[] spriteram16_set
        {
            set
            {
                spriteram16_handle.ReleaseGCHandle();
                if (value == null)
                    return;
                spriteram16_src = value;
                spriteram16Length = value.Length;
                spriteram16_src.GetObjectPtr(ref spriteram16_handle, ref spriteram16);
            }
        }
        #endregion

        #region //指针化spriteram16_2
        static ushort[] spriteram16_2_src;
        static GCHandle spriteram16_2_handle;
        public static ushort* spriteram16_2;
        public static int spriteram16_2Length;
        public static ushort[] spriteram16_2_set
        {
            set
            {
                spriteram16_2_handle.ReleaseGCHandle();
                if (value == null)
                    return;
                spriteram16_2_src = value;
                spriteram16_2Length = value.Length;
                spriteram16_2_src.GetObjectPtr(ref spriteram16_2_handle, ref spriteram16_2);
            }
        }
        #endregion

        #region //指针化paletteram
        static byte[] paletteram_src;
        static GCHandle paletteram_handle;
        public static byte* paletteram;
        public static int paletteramLength;
        public static byte[] paletteram_set
        {
            set
            {
                paletteram_handle.ReleaseGCHandle();
                paletteram_src = value;
                paletteramLength = value.Length;
                paletteram_src.GetObjectPtr(ref paletteram_handle, ref paletteram);
            }
        }
        #endregion

        #region //指针化paletteram_2
        static byte[] paletteram_2_src;
        static GCHandle paletteram_2_handle;
        public static byte* paletteram_2;
        public static int paletteram_2Length;
        public static byte[] paletteram_2_set
        {
            set
            {
                paletteram_2_handle.ReleaseGCHandle();
                paletteram_2_src = value;
                paletteram_2Length = value.Length;
                paletteram_2_src.GetObjectPtr(ref paletteram_2_handle, ref paletteram_2);
            }
        }
        #endregion

        #region //指针化paletteram16
        static ushort[] paletteram16_src;
        static GCHandle paletteram16_handle;
        public static ushort* paletteram16;
        public static int paletteram16Length;
        public static ushort[] paletteram16_set
        {
            set
            {
                paletteram16_handle.ReleaseGCHandle();
                paletteram16_src = value;
                paletteram16Length = value.Length;
                paletteram16_src.GetObjectPtr(ref paletteram16_handle, ref paletteram16);
            }
        }
        #endregion


        #region //指针化paletteram16_2
        static ushort[] paletteram16_2_src;
        static GCHandle paletteram16_2_handle;
        public static ushort* paletteram16_2;
        public static int paletteram16_2Length;
        public static ushort[] paletteram16_2_set
        {
            set
            {
                paletteram16_2_handle.ReleaseGCHandle();
                paletteram16_2_src = value;
                paletteram16_2Length = value.Length;
                paletteram16_2_src.GetObjectPtr(ref paletteram16_2_handle, ref paletteram16_2);
            }
        }
        #endregion


        public static int[] interrupt_enable;
        public static int objcpunum;
        public static int flip_screen_x, flip_screen_y;
        public static void generic_machine_init()
        {
            int counternum;
            coin_count_set = new uint[8];
            coinlockedout_set = new uint[8];
            lastcoin_set = new uint[8];
            for (counternum = 0; counternum < 8; counternum++)
            {
                lastcoin[counternum] = 0;
                coinlockedout[counternum] = 0;
            }
            interrupt_enable = new int[8];
        }
        public static void coin_counter_w(int num, int on)
        {
            if (num >= 8)
            {
                return;
            }
            if (on != 0 && (lastcoin[num] == 0))
            {
                coin_count[num]++;
            }
            lastcoin[num] = (uint)on;
        }
        public static void coin_lockout_w(int num, int on)
        {
            if (num >= 8)
            {
                return;
            }
            coinlockedout[num] = (uint)on;
        }
        public static void coin_lockout_global_w(int on)
        {
            int i;
            for (i = 0; i < 8; i++)
            {
                coin_lockout_w(i, on);
            }
        }
        public static void nvram_load()
        {
            switch (Machine.sBoard)
            {
                case "Neo Geo":
                    Neogeo.nvram_handler_load_neogeo();
                    break;
                    /*case "Namco System 1":
                        Namcos1.nvram_handler_load_namcos1();
                        break;*/
            }
        }
        public static void nvram_save()
        {
            switch (Machine.sBoard)
            {
                case "Neo Geo":
                    Neogeo.nvram_handler_save_neogeo();
                    break;
                    /*case "Namco System 1":
                        Namcos1.nvram_handler_save_namcos1();
                        break;*/
            }
        }
        public static void watchdog_reset16_w()
        {
            Watchdog.watchdog_reset();
        }
        public static ushort watchdog_reset16_r()
        {
            Watchdog.watchdog_reset();
            return 0xffff;
        }
        public static void nmi_0_line_pulse()
        {
            irqn_line_set(0, (int)LineState.INPUT_LINE_NMI, (int)LineState.PULSE_LINE);
        }
        public static void nmi_1_line_pulse()
        {
            irqn_line_set(1, (int)LineState.INPUT_LINE_NMI, (int)LineState.PULSE_LINE);
        }
        public static void irq_0_0_line_hold()
        {
            Cpuint.cpunum_set_input_line(0, 0, LineState.HOLD_LINE);
        }
        public static void irq_0_1_line_hold()
        {
            Cpuint.cpunum_set_input_line(0, 1, LineState.HOLD_LINE);
        }
        public static void irq_0_6_line_hold()
        {
            Cpuint.cpunum_set_input_line(0, 6, LineState.HOLD_LINE);
        }
        public static void irq_1_0_line_hold()
        {
            Cpuint.cpunum_set_input_line(1, 0, LineState.HOLD_LINE);
        }
        public static void irq_2_0_line_hold()
        {
            Cpuint.cpunum_set_input_line(2, 0, LineState.HOLD_LINE);
        }
        public static void watchdog_reset_w()
        {
            Watchdog.watchdog_reset();
        }
        public static void interrupt_reset()
        {
            int cpunum;
            for (cpunum = 0; cpunum < Cpuexec.ncpu; cpunum++)
            {
                interrupt_enable[cpunum] = 1;
            }
        }
        public static void clear_all_lines()
        {
            int inputcount = 0;
            int line;
            if (objcpunum == 0 && Cpuexec.cpu[0] == MC68000.m1)
            {
                inputcount = 8;
            }
            else
            {
                inputcount = 1;
            }
            Cpuint.cpunum_set_input_line(objcpunum, (int)LineState.INPUT_LINE_NMI, LineState.CLEAR_LINE);
            for (line = 0; line < inputcount; line++)
            {
                Cpuint.cpunum_set_input_line(objcpunum, line, LineState.CLEAR_LINE);
            }
        }
        public static void cpu_interrupt_enable(int cpunum, int enabled)
        {
            interrupt_enable[cpunum] = enabled;
            if (enabled == 0)
            {
                objcpunum = cpunum;
                EmuTimer.timer_set_internal(EmuTimer.TIME_ACT.Generic_clear_all_lines);
            }
        }
        public static void interrupt_enable_w(byte data)
        {
            cpu_interrupt_enable(Cpuexec.activecpu, data);
        }
        public static void irqn_line_set(int cpunum, int line, int state)
        {
            if (interrupt_enable[cpunum] != 0)
            {
                Cpuint.cpunum_set_input_line(cpunum, line, (LineState)state);
            }
        }
        public static void nmi_line_pulse0()
        {
            nmi_line_pulse(0);
        }
        public static void nmi_line_pulse(int cpunum)
        {
            irqn_line_set(cpunum, (int)LineState.INPUT_LINE_NMI, (int)LineState.PULSE_LINE);
        }
        public static void irq0_line_hold1()
        {
            irq0_line_hold(1);
        }
        public static void irq0_line_hold(int cpunum)
        {
            irqn_line_set(cpunum, 0, (int)LineState.HOLD_LINE);
        }
        public static void irq4_line_hold(int cpunum)
        {
            irqn_line_set(cpunum, 4, (int)LineState.HOLD_LINE);
        }
        public static void irq5_line_hold0()
        {
            irq5_line_hold(0);
        }
        public static void irq5_line_hold(int cpunum)
        {
            irqn_line_set(cpunum, 5, (int)LineState.HOLD_LINE);
        }
        public static ushort paletteram16_split(int offset)
        {
            return (ushort)(paletteram[offset] | (paletteram_2[offset] << 8));
        }
        public static void buffer_spriteram_w()
        {
            AxiArray.Copy(spriteram, buffered_spriteram, spriteramLength);
        }
        public static void buffer_spriteram16_w()
        {
            AxiArray.Copy(spriteram16, buffered_spriteram16, spriteram16Length);
        }
        public static ushort paletteram16_le(int offset)
        {
            return (ushort)(paletteram[offset & ~1] | (paletteram[offset | 1] << 8));
        }
        public static ushort paletteram16_be(int offset)
        {
            return (ushort)(paletteram[offset | 1] | (paletteram[offset & ~1] << 8));
        }
        public static void set_color_444(int color, int rshift, int gshift, int bshift, ushort data)
        {
            Palette.palette_set_callback(color, Palette.make_rgb(Palette.pal4bit((byte)(data >> rshift)), Palette.pal4bit((byte)(data >> gshift)), Palette.pal4bit((byte)(data >> bshift))));
        }
        public static void set_color_555(int color, int rshift, int gshift, int bshift, ushort data)
        {
            Palette.palette_set_callback(color, Palette.make_rgb(Palette.pal5bit((byte)(data >> rshift)), Palette.pal5bit((byte)(data >> gshift)), (int)Palette.pal5bit((byte)(data >> bshift))));
        }
        public static void updateflip()
        {
            int width = Video.screenstate.width;
            int height = Video.screenstate.height;
            long period = Video.screenstate.frame_period;
            RECT visarea = Video.screenstate.visarea;
            Tmap.tilemap_set_flip(null, (byte)((Tilemap.TILEMAP_FLIPX & flip_screen_x) | (Tilemap.TILEMAP_FLIPY & flip_screen_y)));
            if (flip_screen_x != 0)
            {
                int temp;
                temp = width - visarea.min_x - 1;
                visarea.min_x = width - visarea.max_x - 1;
                visarea.max_x = temp;
            }
            if (flip_screen_y != 0)
            {
                int temp;
                temp = height - visarea.min_y - 1;
                visarea.min_y = height - visarea.max_y - 1;
                visarea.max_y = temp;
            }
            Video.video_screen_configure(width, height, visarea, period);
        }
        public static void flip_screen_set(int on)
        {
            flip_screen_x_set(on);
            flip_screen_y_set(on);
        }
        public static void flip_screen_x_set(int on)
        {
            if (on != 0)
            {
                on = ~0;
            }
            if (flip_screen_x != on)
            {
                flip_screen_x = on;
                updateflip();
            }
        }
        public static void flip_screen_y_set(int on)
        {
            if (on != 0)
            {
                on = ~0;
            }
            if (flip_screen_y != on)
            {
                flip_screen_y = on;
                updateflip();
            }
        }
        public static int flip_screen_get()
        {
            return flip_screen_x;
        }
        public static void paletteram_xxxxBBBBGGGGRRRR_le_w(int offset, byte data)
        {
            paletteram[offset] = data;
            set_color_444(offset / 2, 0, 4, 8, paletteram16_le(offset));
        }
        public static void paletteram16_xxxxRRRRGGGGBBBB_word_w(int offset, ushort data)
        {
            paletteram16[offset] = data;
            set_color_444(offset, 8, 4, 0, paletteram16[offset]);
        }
        public static void paletteram16_xxxxRRRRGGGGBBBB_word_w1(int offset, byte data)
        {
            paletteram16[offset] = (ushort)((data << 8) | (paletteram16[offset] & 0xff));
            set_color_444(offset, 8, 4, 0, paletteram16[offset]);
        }
        public static void paletteram16_xxxxRRRRGGGGBBBB_word_w2(int offset, byte data)
        {
            paletteram16[offset] = (ushort)((paletteram16[offset] & 0xff00) | data);
            set_color_444(offset, 8, 4, 0, paletteram16[offset]);
        }
        public static void paletteram_RRRRGGGGBBBBxxxx_be_w(int offset, byte data)
        {
            paletteram[offset] = data;
            set_color_444(offset / 2, 12, 8, 4, paletteram16_be(offset));
        }
        public static void paletteram_RRRRGGGGBBBBxxxx_split1_w(int offset, byte data)
        {
            paletteram[offset] = data;
            set_color_444(offset, 12, 8, 4, paletteram16_split(offset));
        }
        public static void paletteram_RRRRGGGGBBBBxxxx_split2_w(int offset, byte data)
        {
            paletteram_2[offset] = data;
            set_color_444(offset, 12, 8, 4, paletteram16_split(offset));
        }

        public static void paletteram16_xBBBBBGGGGGRRRRR_word_w(int offset, ushort data)
        {
            paletteram16[offset] = data;
            set_color_555(offset, 0, 5, 10, paletteram16[offset]);
        }
        public static void paletteram16_xBBBBBGGGGGRRRRR_word_w1(int offset, byte data)
        {
            paletteram16[offset] = (ushort)((data << 8) | (paletteram16[offset] & 0xff));
            set_color_555(offset, 0, 5, 10, paletteram16[offset]);
        }
        public static void paletteram16_xBBBBBGGGGGRRRRR_word_w2(int offset, byte data)
        {
            paletteram16[offset] = (ushort)((paletteram16[offset] & 0xff00) | data);
            set_color_555(offset, 0, 5, 10, paletteram16[offset]);
        }
        public static void paletteram16_xRRRRRGGGGGBBBBB_word_w(int offset)
        {
            set_color_555(offset, 10, 5, 0, paletteram16[offset]);
        }
        public static void paletteram16_RRRRGGGGBBBBRGBx_word_w(int offset, ushort data)
        {
            paletteram16[offset] = data;
            ushort data1 = paletteram16[offset];
            //TODO  通道修改，BGRA->RGBA
            Palette.palette_set_callback(offset, (uint)((Palette.pal5bit((byte)(((data1 >> 11) & 0x1e) | ((data1 >> 3) & 0x01))) << 16) | (Palette.pal5bit((byte)(((data >> 7) & 0x1e) | ((data >> 2) & 0x01))) << 8) | Palette.pal5bit((byte)(((data >> 3) & 0x1e) | ((data >> 1) & 0x01)))));
        }
        public static void paletteram16_RRRRGGGGBBBBRGBx_word_w1(int offset, byte data)
        {
            paletteram16[offset] = (ushort)((data << 8) | (paletteram16[offset] & 0xff));
            ushort data1 = paletteram16[offset];
            //TODO  通道修改，BGRA->RGBA
            Palette.palette_set_callback(offset, (uint)((Palette.pal5bit((byte)(((data1 >> 11) & 0x1e) | ((data1 >> 3) & 0x01))) << 16) | (Palette.pal5bit((byte)(((data >> 7) & 0x1e) | ((data >> 2) & 0x01))) << 8) | Palette.pal5bit((byte)(((data >> 3) & 0x1e) | ((data >> 1) & 0x01)))));
        }
        public static void paletteram16_RRRRGGGGBBBBRGBx_word_w2(int offset, byte data)
        {
            paletteram16[offset] = (ushort)((paletteram16[offset] & 0xff00) | data);
            ushort data1 = paletteram16[offset];

            //TODO  通道修改，BGRA->RGBA
            Palette.palette_set_callback(offset, (uint)((Palette.pal5bit((byte)(((data1 >> 11) & 0x1e) | ((data1 >> 3) & 0x01))) << 16) | (Palette.pal5bit((byte)(((data >> 7) & 0x1e) | ((data >> 2) & 0x01))) << 8) | Palette.pal5bit((byte)(((data >> 3) & 0x1e) | ((data >> 1) & 0x01)))));

        }
    }
}
