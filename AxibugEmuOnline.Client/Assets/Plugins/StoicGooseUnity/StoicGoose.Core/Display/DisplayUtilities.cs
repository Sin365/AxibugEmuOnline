using System;
using System.Reflection;
using StoicGoose.Core.Interfaces;
using StoicGoose.Core.Machines;

namespace StoicGoose.Core.Display
{
	public static class DisplayUtilities
	{
        private static readonly uint[] ColorLut = new uint[512];
        public const uint Color_Black = 0xFF000000;
        public const uint Color_White = 0xFFFFFFFF;
        /// <summary>
        /// 改成颜色全部预先缓存，然后颜色抛弃元组，然后颜色全部用uint32表示，byte指针写4个通道改为一次uint32指针写入
        /// </summary>
        static DisplayUtilities()
        {
            //至于为什么，WonderSwan 某些寄存器里颜色是 9 - bit index
            for (int i = 0; i < 512; i++)
            {
                var r = (byte)((i >> 8) & 0xF);
                var g = (byte)((i >> 4) & 0xF);
                var b = (byte)(i & 0xF);

                r = (byte)((r << 4) | r);
                g = (byte)((g << 4) | g);
                b = (byte)((b << 4) | b);

                // ARGB32 → 内存布局：R G B A（小端）
                ColorLut[i] = 0xFF000000u
                            | (uint)(r << 24)
                            | (uint)(g << 16)
                            | (uint)(b << 8);
            }
        }
        // TODO: WSC high contrast mode

        private static ushort ReadMemory16(IMachine machine, uint address) => (ushort)(machine.ReadMemory(address + 1) << 8 | machine.ReadMemory(address));
		private static uint ReadMemory32(IMachine machine, uint address) => (uint)(machine.ReadMemory(address + 3) << 24 | machine.ReadMemory(address + 2) << 16 | machine.ReadMemory(address + 1) << 8 | machine.ReadMemory(address));

		public static byte ReadPixel(IMachine machine, ushort tile, int y, int x, bool isPacked, bool is4bpp, bool isColor)
		{
			/* http://perfectkiosk.net/stsws.html#color_mode */

			/* WonderSwan OR Color/Crystal in 2bpp mode */
			if (!isColor || (isColor && !is4bpp))
			{
				var data = ReadMemory16(machine, (uint)(0x2000 + (tile << 4) + ((y % 8) << 1)));
				return (byte)((((data >> 15 - (x % 8)) & 0b1) << 1 | ((data >> 7 - (x % 8)) & 0b1)) & 0b11);
			}

			/* WonderSwan Color/Crystal in 4bpp mode */
			else if (isColor && is4bpp)
			{
				/* 4bpp planar mode */
				if (!isPacked)
				{
					var data = ReadMemory32(machine, (uint)(0x4000 + ((tile & 0x03FF) << 5) + ((y % 8) << 2)));
					return (byte)((((data >> 31 - (x % 8)) & 0b1) << 3 | ((data >> 23 - (x % 8)) & 0b1) << 2 | ((data >> 15 - (x % 8)) & 0b1) << 1 | ((data >> 7 - (x % 8)) & 0b1)) & 0b1111);
				}

				/* 4bpp packed mode */
				else if (isPacked)
				{
					var data = machine.ReadMemory((ushort)(0x4000 + ((tile & 0x03FF) << 5) + ((y % 8) << 2) + ((x % 8) >> 1)));
					return (byte)((data >> 4 - (((x % 8) & 0b1) << 2)) & 0b1111);
				}
			}

			throw new Exception("Invalid display controller configuration");
		}

		public static ushort ReadColor(IMachine machine, byte paletteIdx, byte colorIdx)
		{
			var address = (uint)(0x0FE00 + (paletteIdx << 5) + (colorIdx << 1));
			return (ushort)(machine.ReadMemory(address + 1) << 8 | machine.ReadMemory(address));
		}

		private static byte DuplicateBits(int value) => (byte)((value & 0b1111) | (value & 0b1111) << 4);

        public static uint GeneratePixel(ushort data)
        {
            return ColorLut[data & 0x1FF];
        }
        public static uint GeneratePixel(byte data)
        {
            return ColorLut[data & 0x1FF];
        }

        public static unsafe void CopyPixel(uint color, byte* data, int x, int y, int stride)
        {
            *(uint*)(data + ((y * stride) + x) * 4) = color;
        }
        public static unsafe void CopyPixel(uint color, byte* data, long address)
        {
            *(uint*)(data + address) = color;
        }


  //      public static (byte r, byte g, byte b) GeneratePixel(byte data) => (DuplicateBits(data), DuplicateBits(data), DuplicateBits(data));
		//public static (byte r, byte g, byte b) GeneratePixel(ushort data) => (DuplicateBits(data >> 8), DuplicateBits(data >> 4), DuplicateBits(data >> 0));

		//public static unsafe void CopyPixel((byte r, byte g, byte b) pixel, byte* data, int x, int y, int stride) => CopyPixel(pixel, data, ((y * stride) + x) * 4);
  //      public static unsafe void CopyPixel((byte r, byte g, byte b) pixel, byte* data, long address)
  //      {
  //          data[address + 0] = pixel.r;
  //          data[address + 1] = pixel.g;
  //          data[address + 2] = pixel.b;
  //          data[address + 3] = 255;
  //      }
        //public static void CopyPixel((byte r, byte g, byte b) pixel, byte[] data, int x, int y, int stride) => CopyPixel(pixel, data, ((y * stride) + x) * 4);
        //public static void CopyPixel((byte r, byte g, byte b) pixel, byte[] data, long address)
        //{
        //	data[address + 0] = pixel.r;
        //	data[address + 1] = pixel.g;
        //	data[address + 2] = pixel.b;
        //	data[address + 3] = 255;
        //}
    }
}
