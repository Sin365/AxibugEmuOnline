using System;

namespace Essgee.Emulation
{
    public unsafe static class Utilities
    {
        static Utilities()
        {
            InitRGB222toBGRA8888Cache();
            InitRGB444toBGRA8888Cache();
            InitRGBCGBtoBGRA8888Cache();
        }
        public static bool IsBitSet(byte value, int bit)
        {
            return ((value & (1 << bit)) != 0);
        }

        //public static void RGB222toBGRA8888(int color, ref byte[] buffer, int address)
        //{
        //    byte r = (byte)((color >> 0) & 0x3), g = (byte)((color >> 2) & 0x3), b = (byte)((color >> 4) & 0x3);
        //    buffer[address + 0] = (byte)((b << 6) | (b << 4) | (b << 2) | b);
        //    buffer[address + 1] = (byte)((g << 6) | (g << 4) | (g << 2) | g);
        //    buffer[address + 2] = (byte)((r << 6) | (r << 4) | (r << 2) | r);
        //    buffer[address + 3] = 0xFF;
        //}


        private static readonly byte[] RGB222toBGRA8888Cache_LutR = new byte[32768];
        private static readonly byte[] RGB222toBGRA8888Cache_LutG = new byte[32768];
        private static readonly byte[] RGB222toBGRA8888Cache_LutB = new byte[32768];

        static void InitRGB222toBGRA8888Cache()  // 静态构造函数初始化LUT
        {
            for (int i = 0; i < 32768; i++)
            {
                byte r = (byte)((i >> 0) & 0x3), g = (byte)((i >> 2) & 0x3), b = (byte)((i >> 4) & 0x3);
                RGB222toBGRA8888Cache_LutR[i] = (byte)((b << 6) | (b << 4) | (b << 2) | b);
                RGB222toBGRA8888Cache_LutG[i] = (byte)((g << 6) | (g << 4) | (g << 2) | g);
                RGB222toBGRA8888Cache_LutB[i] = (byte)((r << 6) | (r << 4) | (r << 2) | r);
            }
        }

        public static void RGB222toBGRA8888(int color, ref byte* buffer, int address)
        {
            //byte r = (byte)((color >> 0) & 0x3), g = (byte)((color >> 2) & 0x3), b = (byte)((color >> 4) & 0x3);
            //buffer[address + 0] = (byte)((b << 6) | (b << 4) | (b << 2) | b);
            //buffer[address + 1] = (byte)((g << 6) | (g << 4) | (g << 2) | g);
            //buffer[address + 2] = (byte)((r << 6) | (r << 4) | (r << 2) | r);
            //buffer[address + 3] = 0xFF;
            *(buffer + address) = RGB222toBGRA8888Cache_LutR[color];
            *(buffer + address + 1) = RGB222toBGRA8888Cache_LutR[color];
            *(buffer + address + 2) = RGB222toBGRA8888Cache_LutR[color];
            *(buffer + address + 3) = 0xFF;
        }

        //public static void RGB444toBGRA8888(int color, ref byte[] buffer, int address)
        //{
        //    byte r = (byte)((color >> 0) & 0xF), g = (byte)((color >> 4) & 0xF), b = (byte)((color >> 8) & 0xF);
        //    buffer[address + 0] = (byte)((b << 4) | b);
        //    buffer[address + 1] = (byte)((g << 4) | g);
        //    buffer[address + 2] = (byte)((r << 4) | r);
        //    buffer[address + 3] = 0xFF;
        //}

        // 预计算所有可能的颜色转换
        private static readonly byte[] RGB444toBGRA8888Cache_LutR = new byte[32768]; 
        private static readonly byte[] RGB444toBGRA8888Cache_LutG = new byte[32768];
        private static readonly byte[] RGB444toBGRA8888Cache_LutB = new byte[32768];

        static void InitRGB444toBGRA8888Cache()  // 静态构造函数初始化LUT
        {
            for (int i = 0; i < 32768; i++)
            {
                byte r = (byte)((i >> 0) & 0xF), g = (byte)((i >> 4) & 0xF), b = (byte)((i >> 8) & 0xF);
                RGB444toBGRA8888Cache_LutR[i] = (byte)((b << 4) | b);
                RGB444toBGRA8888Cache_LutG[i] = (byte)((g << 4) | g);
                RGB444toBGRA8888Cache_LutB[i] = (byte)((r << 4) | r);
            }
        }

        public unsafe static void RGB444toBGRA8888(int color, ref byte* buffer, int address)
        {
            //byte r = (byte)((color >> 0) & 0xF), g = (byte)((color >> 4) & 0xF), b = (byte)((color >> 8) & 0xF);
            //buffer[address + 0] = (byte)((b << 4) | b);
            //buffer[address + 1] = (byte)((g << 4) | g);
            //buffer[address + 2] = (byte)((r << 4) | r);
            //buffer[address + 3] = 0xFF;
            *(buffer + address) = RGB444toBGRA8888Cache_LutG[color];
            *(buffer + address + 1) = RGB444toBGRA8888Cache_LutG[color];
            *(buffer + address + 2) = RGB444toBGRA8888Cache_LutG[color];
            *(buffer + address + 3) = 0xFF;
        }

        //public static void RGBCGBtoBGRA8888(int color, ref byte[] buffer, int address)
        //{
        //    /* https://byuu.net/video/color-emulation -- "LCD emulation: Game Boy Color" */
        //    byte r = (byte)((color >> 0) & 0x1F), g = (byte)((color >> 5) & 0x1F), b = (byte)((color >> 10) & 0x1F);
        //    buffer[address + 0] = (byte)(Math.Min(960, (r * 6) + (g * 4) + (b * 22)) >> 2);
        //    buffer[address + 1] = (byte)(Math.Min(960, (g * 24) + (b * 8)) >> 2);
        //    buffer[address + 2] = (byte)(Math.Min(960, (r * 26) + (g * 4) + (b * 2)) >> 2);
        //    buffer[address + 3] = 0xFF;
        //}

        // 预计算所有可能的颜色转换
        private static readonly byte[] RGBCGBtoBGRA8888Cache_LutR = new byte[32768];  // 15位颜色 = 32768种
        private static readonly byte[] RGBCGBtoBGRA8888Cache_LutG = new byte[32768];
        private static readonly byte[] RGBCGBtoBGRA8888Cache_LutB = new byte[32768];

        static void InitRGBCGBtoBGRA8888Cache()  // 静态构造函数初始化LUT
        {
            for (int i = 0; i < 32768; i++)
            {
                byte r = (byte)(i & 0x1F);
                byte g = (byte)((i >> 5) & 0x1F);
                byte b = (byte)((i >> 10) & 0x1F);
                RGBCGBtoBGRA8888Cache_LutR[i] = (byte)(Math.Min(960, (r * 6) + (g * 4) + (b * 22)) >> 2);
                RGBCGBtoBGRA8888Cache_LutG[i] = (byte)(Math.Min(960, (g * 24) + (b * 8)) >> 2);
                RGBCGBtoBGRA8888Cache_LutB[i] = (byte)(Math.Min(960, (r * 26) + (g * 4) + (b * 2)) >> 2);
            }
        }

        public static void RGBCGBtoBGRA8888(int color, ref byte* buffer, int address)
        {
            /* https://byuu.net/video/color-emulation -- "LCD emulation: Game Boy Color" */
            //byte r = (byte)((color >> 0) & 0x1F), g = (byte)((color >> 5) & 0x1F), b = (byte)((color >> 10) & 0x1F);
            //buffer[address + 0] = (byte)(Math.Min(960, (r * 6) + (g * 4) + (b * 22)) >> 2);
            //buffer[address + 1] = (byte)(Math.Min(960, (g * 24) + (b * 8)) >> 2);
            //buffer[address + 2] = (byte)(Math.Min(960, (r * 26) + (g * 4) + (b * 2)) >> 2);
            //buffer[address + 3] = 0xFF;
            *(buffer + address) = RGBCGBtoBGRA8888Cache_LutR[color];
            *(buffer + address + 1) = RGBCGBtoBGRA8888Cache_LutG[color];
            *(buffer + address + 2) = RGBCGBtoBGRA8888Cache_LutB[color];
            *(buffer + address + 3) = 0xFF;
        }
    }
}
