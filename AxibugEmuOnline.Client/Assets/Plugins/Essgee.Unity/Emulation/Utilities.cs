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

        // RGB222
        private static readonly uint[] RGB222toBGRA8888Cache = new uint[32768];

        static void InitRGB222toBGRA8888Cache()
        {
            for (int i = 0; i < 32768; i++)
            {
                byte r = (byte)((i >> 0) & 0x3);
                byte g = (byte)((i >> 2) & 0x3);
                byte b = (byte)((i >> 4) & 0x3);

                byte br = (byte)((b << 6) | (b << 4) | (b << 2) | b);
                byte gr = (byte)((g << 6) | (g << 4) | (g << 2) | g);
                byte rr = (byte)((r << 6) | (r << 4) | (r << 2) | r);

                RGB222toBGRA8888Cache[i] =
                    (uint)(br | (gr << 8) | (rr << 16) | (0xFFu << 24));
            }
        }

        public static void RGB222toBGRA8888(int color, ref byte* buffer, int address)
        {
            *(uint*)(buffer + address) = RGB222toBGRA8888Cache[color];
        }

        // RGB444
        private static readonly uint[] RGB444toBGRA8888Cache = new uint[32768];

        static void InitRGB444toBGRA8888Cache()
        {
            for (int i = 0; i < 32768; i++)
            {
                byte r = (byte)((i >> 0) & 0xF);
                byte g = (byte)((i >> 4) & 0xF);
                byte b = (byte)((i >> 8) & 0xF);

                byte br = (byte)((b << 4) | b);
                byte gr = (byte)((g << 4) | g);
                byte rr = (byte)((r << 4) | r);

                RGB444toBGRA8888Cache[i] =
                    (uint)(br | (gr << 8) | (rr << 16) | (0xFFu << 24));
            }
        }

        public static void RGB444toBGRA8888(int color, ref byte* buffer, int address)
        {
            *(uint*)(buffer + address) = RGB444toBGRA8888Cache[color];
        }

        // RGBCGB
        private static readonly uint[] RGBCGBtoBGRA8888Cache = new uint[32768];

        static void InitRGBCGBtoBGRA8888Cache()
        {
            for (int i = 0; i < 32768; i++)
            {
                byte r = (byte)(i & 0x1F);
                byte g = (byte)((i >> 5) & 0x1F);
                byte b = (byte)((i >> 10) & 0x1F);

                byte br = (byte)(Math.Min(960, (r * 6) + (g * 4) + (b * 22)) >> 2);
                byte gr = (byte)(Math.Min(960, (g * 24) + (b * 8)) >> 2);
                byte rr = (byte)(Math.Min(960, (r * 26) + (g * 4) + (b * 2)) >> 2);

                RGBCGBtoBGRA8888Cache[i] =
                    (uint)(br | (gr << 8) | (rr << 16) | (0xFFu << 24));
            }
        }

        public static void RGBCGBtoBGRA8888(int color, ref byte* buffer, int address)
        {
            *(uint*)(buffer + address) = RGBCGBtoBGRA8888Cache[color];
        }
    }
}

//using System;

//namespace Essgee.Emulation
//{
//    public unsafe static class Utilities
//    {
//        static Utilities()
//        {
//            InitRGB222toBGRA8888Cache();
//            InitRGB444toBGRA8888Cache();
//            InitRGBCGBtoBGRA8888Cache();
//        }

//        public static bool IsBitSet(byte value, int bit)
//        {
//            return ((value & (1 << bit)) != 0);
//        }

//        //public static void RGB222toBGRA8888(int color, ref byte[] buffer, int address)
//        //{
//        //    byte r = (byte)((color >> 0) & 0x3), g = (byte)((color >> 2) & 0x3), b = (byte)((color >> 4) & 0x3);
//        //    buffer[address + 0] = (byte)((b << 6) | (b << 4) | (b << 2) | b);
//        //    buffer[address + 1] = (byte)((g << 6) | (g << 4) | (g << 2) | g);
//        //    buffer[address + 2] = (byte)((r << 6) | (r << 4) | (r << 2) | r);
//        //    buffer[address + 3] = 0xFF;
//        //}

//        private static readonly byte[] RGB222toBGRA8888Cache_B = new byte[32768];
//        private static readonly byte[] RGB222toBGRA8888Cache_G = new byte[32768];
//        private static readonly byte[] RGB222toBGRA8888Cache_R = new byte[32768];

//        static void InitRGB222toBGRA8888Cache()
//        {
//            for (int i = 0; i < 32768; i++)
//            {
//                byte r = (byte)((i >> 0) & 0x3), g = (byte)((i >> 2) & 0x3), b = (byte)((i >> 4) & 0x3);
//                RGB222toBGRA8888Cache_B[i] = (byte)((b << 6) | (b << 4) | (b << 2) | b);
//                RGB222toBGRA8888Cache_G[i] = (byte)((g << 6) | (g << 4) | (g << 2) | g);
//                RGB222toBGRA8888Cache_R[i] = (byte)((r << 6) | (r << 4) | (r << 2) | r);
//            }
//        }

//        public static void RGB222toBGRA8888(int color, ref byte* buffer, int address)
//        {
//            *(buffer + address + 0) = RGB222toBGRA8888Cache_B[color];
//            *(buffer + address + 1) = RGB222toBGRA8888Cache_G[color];
//            *(buffer + address + 2) = RGB222toBGRA8888Cache_R[color];
//            *(buffer + address + 3) = 0xFF;
//        }

//        //public static void RGB444toBGRA8888(int color, ref byte[] buffer, int address)
//        //{
//        //    byte r = (byte)((color >> 0) & 0xF), g = (byte)((color >> 4) & 0xF), b = (byte)((color >> 8) & 0xF);
//        //    buffer[address + 0] = (byte)((b << 4) | b);
//        //    buffer[address + 1] = (byte)((g << 4) | g);
//        //    buffer[address + 2] = (byte)((r << 4) | r);
//        //    buffer[address + 3] = 0xFF;
//        //}

//        private static readonly byte[] RGB444toBGRA8888Cache_B = new byte[32768];
//        private static readonly byte[] RGB444toBGRA8888Cache_G = new byte[32768];
//        private static readonly byte[] RGB444toBGRA8888Cache_R = new byte[32768];

//        static void InitRGB444toBGRA8888Cache()
//        {
//            for (int i = 0; i < 32768; i++)
//            {
//                byte r = (byte)((i >> 0) & 0xF), g = (byte)((i >> 4) & 0xF), b = (byte)((i >> 8) & 0xF);
//                RGB444toBGRA8888Cache_B[i] = (byte)((b << 4) | b);
//                RGB444toBGRA8888Cache_G[i] = (byte)((g << 4) | g);
//                RGB444toBGRA8888Cache_R[i] = (byte)((r << 4) | r);
//            }
//        }

//        public unsafe static void RGB444toBGRA8888(int color, ref byte* buffer, int address)
//        {
//            *(buffer + address + 0) = RGB444toBGRA8888Cache_B[color];
//            *(buffer + address + 1) = RGB444toBGRA8888Cache_G[color];
//            *(buffer + address + 2) = RGB444toBGRA8888Cache_R[color];
//            *(buffer + address + 3) = 0xFF;
//        }

//        //public static void RGBCGBtoBGRA8888(int color, ref byte[] buffer, int address)
//        //{
//        //    byte r = (byte)((color >> 0) & 0x1F), g = (byte)((color >> 5) & 0x1F), b = (byte)((color >> 10) & 0x1F);
//        //    buffer[address + 0] = (byte)(Math.Min(960, (r * 6) + (g * 4) + (b * 22)) >> 2);
//        //    buffer[address + 1] = (byte)(Math.Min(960, (g * 24) + (b * 8)) >> 2);
//        //    buffer[address + 2] = (byte)(Math.Min(960, (r * 26) + (g * 4) + (b * 2)) >> 2);
//        //    buffer[address + 3] = 0xFF;
//        //}

//        private static readonly byte[] RGBCGBtoBGRA8888Cache_B = new byte[32768];
//        private static readonly byte[] RGBCGBtoBGRA8888Cache_G = new byte[32768];
//        private static readonly byte[] RGBCGBtoBGRA8888Cache_R = new byte[32768];

//        static void InitRGBCGBtoBGRA8888Cache()
//        {
//            for (int i = 0; i < 32768; i++)
//            {
//                byte r = (byte)(i & 0x1F);
//                byte g = (byte)((i >> 5) & 0x1F);
//                byte b = (byte)((i >> 10) & 0x1F);
//                RGBCGBtoBGRA8888Cache_B[i] = (byte)(Math.Min(960, (r * 6) + (g * 4) + (b * 22)) >> 2);
//                RGBCGBtoBGRA8888Cache_G[i] = (byte)(Math.Min(960, (g * 24) + (b * 8)) >> 2);
//                RGBCGBtoBGRA8888Cache_R[i] = (byte)(Math.Min(960, (r * 26) + (g * 4) + (b * 2)) >> 2);
//            }
//        }

//        public static void RGBCGBtoBGRA8888(int color, ref byte* buffer, int address)
//        {
//            *(buffer + address + 0) = RGBCGBtoBGRA8888Cache_B[color];
//            *(buffer + address + 1) = RGBCGBtoBGRA8888Cache_G[color];
//            *(buffer + address + 2) = RGBCGBtoBGRA8888Cache_R[color];
//            *(buffer + address + 3) = 0xFF;
//        }
//    }
//}