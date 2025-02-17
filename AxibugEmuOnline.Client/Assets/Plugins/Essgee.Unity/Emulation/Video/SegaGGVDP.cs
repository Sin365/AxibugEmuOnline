using Essgee.EventArguments;
using Essgee.Utilities;
using System;
using static Essgee.Emulation.Utilities;

namespace Essgee.Emulation.Video
{
    /* Sega 315-5378, Game Gear */
    public unsafe class SegaGGVDP : SegaSMSVDP
    {
        protected override int numTotalScanlines => NumTotalScanlinesNtsc;

        public override (int X, int Y, int Width, int Height) Viewport => (0, 0, 160, 144);

        [StateRequired]
        ushort cramLatch;

        public SegaGGVDP() : base()
        {
            //cram = new byte[0x40];
            cram_set = new byte[0x40];
        }

        public override void Reset()
        {
            base.Reset();

            cramLatch = 0x0000;
        }

        public override void SetRevision(int rev)
        {
            // TODO: can GG VDP be detected by software? if so, implement diffs as revision
            base.SetRevision(rev);
        }

        protected override void ReconfigureTimings()
        {
            /* Calculate cycles/line */
            clockCyclesPerLine = (int)Math.Round((clockRate / refreshRate) / numTotalScanlines);

            /* Create arrays */
            screenUsage = new byte[numVisiblePixels * numVisibleScanlines];
            //outputFramebuffer = new byte[Viewport.Width * Viewport.Height * 4];
            outputFramebuffer_set = new byte[Viewport.Width * Viewport.Height * 4];

            /* Update resolution/display timing */
            UpdateResolution();
        }

        //GCHandle? lasyRenderHandle;
        protected override void PrepareRenderScreen()
        {
            //// 固定数组，防止垃圾回收器移动它  
            //var bitmapcolorRect_handle = GCHandle.Alloc(outputFramebuffer.Clone() as byte[], GCHandleType.Pinned);
            //// 获取数组的指针  
            //IntPtr mFrameDataPtr = bitmapcolorRect_handle.AddrOfPinnedObject();

            var eventArgs = RenderScreenEventArgs.Create(numVisiblePixels, numVisibleScanlines, outputFramebuffer_Ptr);
            OnRenderScreen(eventArgs);
            eventArgs.Release();
            //if (lasyRenderHandle != null)
            //    lasyRenderHandle.Value.Free();
            //lasyRenderHandle = bitmapcolorRect_handle;

            //OnRenderScreen(new RenderScreenEventArgs(Viewport.Width, Viewport.Height, outputFramebuffer.Clone() as byte[]));
        }

        private bool ModifyAndVerifyCoordinates(ref int x, ref int y)
        {
            // TODO: correctly derive from timing/resolution values
            x -= 61;
            y -= 51;

            return x >= 0 && x < Viewport.Width && y >= 0 && y < Viewport.Height;
        }

        protected override void SetPixel(int y, int x, int palette, int color)
        {
            if (!ModifyAndVerifyCoordinates(ref x, ref y)) return;
            WriteColorToFramebuffer(palette, color, ((y * Viewport.Width) + (x % Viewport.Width)) * 4);
        }

        protected override void SetPixel(int y, int x, byte b, byte g, byte r)
        {
            if (!ModifyAndVerifyCoordinates(ref x, ref y)) return;
            WriteColorToFramebuffer(b, g, r, ((y * Viewport.Width) + (x % Viewport.Width)) * 4);
        }

        protected override void WriteColorToFramebuffer(int palette, int color, int address)
        {
            int cramAddress = ((palette * 32) + (color * 2));
            WriteColorToFramebuffer((ushort)(cram[cramAddress + 1] << 8 | cram[cramAddress]), address);
        }

        protected unsafe override void WriteColorToFramebuffer(ushort colorValue, int address)
        {
            RGB444toBGRA8888(colorValue, ref outputFramebuffer, address);
        }

        protected override void WriteDataPort(byte value)
        {
            isSecondControlWrite = false;

            readBuffer = value;

            switch (codeRegister)
            {
                case 0x00:
                case 0x01:
                case 0x02:
                    vram[addressRegister] = value;
                    break;
                case 0x03:
                    if ((addressRegister & 0x0001) != 0)
                    {
                        cramLatch = (ushort)((cramLatch & 0x00FF) | (value << 8));
                        cram[(addressRegister & 0x003E) | 0x0000] = (byte)((cramLatch >> 0) & 0xFF);
                        cram[(addressRegister & 0x003E) | 0x0001] = (byte)((cramLatch >> 8) & 0xFF);
                    }
                    else
                        cramLatch = (ushort)((cramLatch & 0xFF00) | (value << 0));
                    break;
            }

            addressRegister++;
        }
    }
}
