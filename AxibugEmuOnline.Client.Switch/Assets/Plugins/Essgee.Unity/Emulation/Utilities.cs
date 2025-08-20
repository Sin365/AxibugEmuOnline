using System;

namespace Essgee.Emulation
{
    public unsafe static class Utilities
    {
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
        public static void RGB222toBGRA8888(int color, ref byte* buffer, int address)
        {
            byte r = (byte)((color >> 0) & 0x3), g = (byte)((color >> 2) & 0x3), b = (byte)((color >> 4) & 0x3);
            buffer[address + 0] = (byte)((b << 6) | (b << 4) | (b << 2) | b);
            buffer[address + 1] = (byte)((g << 6) | (g << 4) | (g << 2) | g);
            buffer[address + 2] = (byte)((r << 6) | (r << 4) | (r << 2) | r);
            buffer[address + 3] = 0xFF;
        }

        //public static void RGB444toBGRA8888(int color, ref byte[] buffer, int address)
        //{
        //    byte r = (byte)((color >> 0) & 0xF), g = (byte)((color >> 4) & 0xF), b = (byte)((color >> 8) & 0xF);
        //    buffer[address + 0] = (byte)((b << 4) | b);
        //    buffer[address + 1] = (byte)((g << 4) | g);
        //    buffer[address + 2] = (byte)((r << 4) | r);
        //    buffer[address + 3] = 0xFF;
        //}

        public unsafe static void RGB444toBGRA8888(int color, ref byte* buffer, int address)
        {
            byte r = (byte)((color >> 0) & 0xF), g = (byte)((color >> 4) & 0xF), b = (byte)((color >> 8) & 0xF);
            buffer[address + 0] = (byte)((b << 4) | b);
            buffer[address + 1] = (byte)((g << 4) | g);
            buffer[address + 2] = (byte)((r << 4) | r);
            buffer[address + 3] = 0xFF;
        }

        public static void RGBCGBtoBGRA8888(int color, ref byte[] buffer, int address)
        {
            /* https://byuu.net/video/color-emulation -- "LCD emulation: Game Boy Color" */
            byte r = (byte)((color >> 0) & 0x1F), g = (byte)((color >> 5) & 0x1F), b = (byte)((color >> 10) & 0x1F);
            buffer[address + 0] = (byte)(Math.Min(960, (r * 6) + (g * 4) + (b * 22)) >> 2);
            buffer[address + 1] = (byte)(Math.Min(960, (g * 24) + (b * 8)) >> 2);
            buffer[address + 2] = (byte)(Math.Min(960, (r * 26) + (g * 4) + (b * 2)) >> 2);
            buffer[address + 3] = 0xFF;
        }
    }
}
