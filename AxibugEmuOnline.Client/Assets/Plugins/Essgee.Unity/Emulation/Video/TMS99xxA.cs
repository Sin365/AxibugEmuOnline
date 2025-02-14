using Essgee.EventArguments;
using Essgee.Utilities;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static Essgee.Emulation.Utilities;

namespace Essgee.Emulation.Video
{
    /* Texas Instruments TMS99xxA family */
    public unsafe class TMS99xxA : IVideo
    {
        public const int NumTotalScanlinesPal = 313;
        public const int NumTotalScanlinesNtsc = 262;

        public const int NumActiveScanlines = 192;
        public const int NumActivePixelsPerScanline = 256;

        protected const string layerBackgroundOptionName = "GraphicsLayersShowBackground";
        protected const string layerSpritesOptionName = "GraphicsLayersShowSprites";
        protected const string layerBordersOptionName = "GraphicsLayersShowBorders";

        protected readonly int numTotalPixelsPerScanline = 342;
        protected virtual int numTotalScanlines => (isPalChip ? NumTotalScanlinesPal : NumTotalScanlinesNtsc);

        protected int topBorderSize, verticalActiveDisplaySize, bottomBorderSize;
        protected int scanlineTopBorder, scanlineActiveDisplay, scanlineBottomBorder;
        protected int numVisibleScanlines;

        protected int leftBorderSize, horizontalActiveDisplaySize, rightBorderSize;
        protected int pixelLeftBorder, pixelActiveDisplay, pixelRightBorder;
        protected int numVisiblePixels;

        public virtual (int X, int Y, int Width, int Height) Viewport => (0, 0, numVisiblePixels, numVisibleScanlines);

        public virtual event EventHandler<SizeScreenEventArgs> SizeScreen;
        public virtual void OnSizeScreen(SizeScreenEventArgs e) { SizeScreen?.Invoke(this, e); }

        public virtual event EventHandler<RenderScreenEventArgs> RenderScreen;
        public virtual void OnRenderScreen(RenderScreenEventArgs e) { RenderScreen?.Invoke(this, e); }

        public virtual event EventHandler<EventArgs> EndOfScanline;
        public virtual void OnEndOfScanline(EventArgs e) { EndOfScanline?.Invoke(this, e); }

        protected const int NumSprites = 32;
        protected const int NumSpritesPerLine = 4;

        protected double clockRate, refreshRate;
        protected bool isPalChip;

        //[StateRequired]
        //protected byte[] registers, vram;

        #region //指针化 registers
        static byte[] registers_src;
        static GCHandle registers_handle;
        public static byte* registers;
        public static int registersLength;
        public static bool registers_IsNull => registers == null;
        public static byte[] registers_set
        {
            set
            {
                registers_handle.ReleaseGCHandle();
                registers_src = value;
                registersLength = value.Length;
                registers_src.GetObjectPtr(ref registers_handle, ref registers);
            }
        }
        #endregion

        #region //指针化 vram
        static byte[] vram_src;
        static GCHandle vram_handle;
        public static byte* vram;
        public static int vramLength;
        public static bool vram_IsNull => vram == null;
        public static byte[] vram_set
        {
            set
            {
                vram_handle.ReleaseGCHandle();
                vram_src = value;
                vramLength = value.Length;
                vram_src.GetObjectPtr(ref vram_handle, ref vram);
            }
        }
        #endregion

        [StateRequired]
        protected (int Number, int Y, int X, int Pattern, int Attribute)[][] spriteBuffer;

        //protected ushort vramMask16k => 0x3FFF;
        //protected ushort vramMask4k => 0x0FFF;
        protected const ushort vramMask16k = 0x3FFF;
        protected const ushort vramMask4k = 0x0FFF;

        [StateRequired]
        protected bool isSecondControlWrite;
        [StateRequired]
        protected ushort controlWord;
        [StateRequired]
        protected byte readBuffer;

        protected byte codeRegister => (byte)((controlWord >> 14) & 0x03);
        
        protected ushort addressRegister
        {
            get { return (ushort)(controlWord & 0x3FFF); }
            set { controlWord = (ushort)((controlWord & 0xC000) | (value & 0x3FFF)); }
        }

        [Flags]
        protected enum StatusFlags : byte
        {
            None = 0,
            SpriteCollision = (1 << 5),
            SpriteOverflow = (1 << 6),
            FrameInterruptPending = (1 << 7)
        }
        [StateRequired]
        protected StatusFlags statusFlags;
        protected bool isSpriteCollision
        {
            get { return ((statusFlags & StatusFlags.SpriteCollision) == StatusFlags.SpriteCollision); }
            set { statusFlags = ((statusFlags & ~StatusFlags.SpriteCollision) | (value ? StatusFlags.SpriteCollision : StatusFlags.None)); }
        }
        protected bool isSpriteOverflow
        {
            get { return ((statusFlags & StatusFlags.SpriteOverflow) == StatusFlags.SpriteOverflow); }
            set { statusFlags = ((statusFlags & ~StatusFlags.SpriteOverflow) | (value ? StatusFlags.SpriteOverflow : StatusFlags.None)); }
        }
        protected bool isFrameInterruptPending
        {
            get { return ((statusFlags & StatusFlags.FrameInterruptPending) == StatusFlags.FrameInterruptPending); }
            set { statusFlags = ((statusFlags & ~StatusFlags.FrameInterruptPending) | (value ? StatusFlags.FrameInterruptPending : StatusFlags.None)); }
        }
        protected bool isFrameInterruptEnabled => IsBitSet(registers[0x01], 5);

        [StateRequired]
        public InterruptState InterruptLine { get; set; }

        [StateRequired]
        protected int currentScanline;

        protected bool isDisplayBlanked => !IsBitSet(registers[0x01], 6);

        protected bool is16kVRAMEnabled => IsBitSet(registers[0x01], 7);

        protected bool isBitM1Set => IsBitSet(registers[0x01], 4);
        protected bool isBitM2Set => IsBitSet(registers[0x00], 1);
        protected bool isBitM3Set => IsBitSet(registers[0x01], 3);

        protected virtual bool isModeGraphics1 => !(isBitM1Set || isBitM2Set || isBitM3Set);
        protected virtual bool isModeText => (isBitM1Set && !(isBitM2Set || isBitM3Set));
        protected virtual bool isModeGraphics2 => (isBitM2Set && !(isBitM1Set || isBitM3Set));
        protected virtual bool isModeMulticolor => (isBitM3Set && !(isBitM1Set || isBitM2Set));

        protected bool isLargeSprites => IsBitSet(registers[0x01], 1);
        protected bool isZoomedSprites => IsBitSet(registers[0x01], 0);

        protected virtual ushort nametableBaseAddress => (ushort)((registers[0x02] & 0x0F) << 10);
        protected virtual ushort spriteAttribTableBaseAddress => (ushort)((registers[0x05] & 0x7F) << 7);
        protected virtual ushort spritePatternGenBaseAddress => (ushort)((registers[0x06] & 0x07) << 11);

        protected byte backgroundColor => (byte)(registers[0x07] & 0x0F);
        protected byte textColor => (byte)((registers[0x07] >> 4) & 0x0F);

        /* http://www.smspower.org/Development/Palette */
        readonly byte[][] colorValuesBgr = new byte[][]
        {
			/*              B     G     R */
			new byte[] { 0x00, 0x00, 0x00 },	/* Transparent */
			new byte[] { 0x00, 0x00, 0x00 },	/* Black */
			new byte[] { 0x3B, 0xB7, 0x47 },	/* Medium green */
			new byte[] { 0x6F, 0xCF, 0x7C },	/* Light green */
			new byte[] { 0xFF, 0x4E, 0x5D },	/* Dark blue */
			new byte[] { 0xFF, 0x72, 0x80 },	/* Light blue */
			new byte[] { 0x47, 0x62, 0xB6 },	/* Dark red */
			new byte[] { 0xED, 0xC8, 0x5D },	/* Cyan */
			new byte[] { 0x48, 0x6B, 0xD7 },	/* Medium red */
			new byte[] { 0x6C, 0x8F, 0xFB },	/* Light red */
			new byte[] { 0x41, 0xCD, 0xC3 },	/* Dark yellow */
			new byte[] { 0x76, 0xDA, 0xD3 },	/* Light yellow */
			new byte[] { 0x2F, 0x9F, 0x3E },	/* Dark green */
			new byte[] { 0xC7, 0x64, 0xB6 },	/* Magenta */
			new byte[] { 0xCC, 0xCC, 0xCC },	/* Gray */
			new byte[] { 0xFF, 0xFF, 0xFF }		/* White */
		};

        protected const byte screenUsageEmpty = 0;
        protected const byte screenUsageSprite = (1 << 0);
        protected const byte screenUsageBackground = (1 << 1);
        [StateRequired]
        protected byte[] screenUsage;

        [StateRequired]
        protected int cycleCount;
        //protected byte[] outputFramebuffer;

        #region //指针化 outputFramebuffer
        static byte[] outputFramebuffer_src;
        static GCHandle outputFramebuffer_handle;
        public static IntPtr outputFramebuffer_Ptr;
        public static byte* outputFramebuffer;
        public static int outputFramebufferLength;
        public static bool outputFramebuffer_IsNull => outputFramebuffer == null;
        public static byte[] outputFramebuffer_set
        {
            set
            {

                outputFramebuffer_handle.ReleaseGCHandle();
                outputFramebuffer_src = value;
                outputFramebufferLength = value.Length;
                outputFramebuffer_src.GetObjectPtr(ref outputFramebuffer_handle, ref outputFramebuffer);
                outputFramebuffer_Ptr = outputFramebuffer_handle.AddrOfPinnedObject();
            }
        }
        #endregion


        protected int clockCyclesPerLine;

        public (string Name, string Description)[] RuntimeOptions => new (string name, string description)[]
        {
            (layerBackgroundOptionName, "Background"),
            (layerSpritesOptionName, "Sprites"),
            (layerBordersOptionName, "Borders"),
        };

        protected bool layerBackgroundForceEnable, layerSpritesForceEnable, layerBordersForceEnable;

        public TMS99xxA()
        {
            //registers = new byte[0x08];
            //vram = new byte[0x4000];
            registers_set = new byte[0x08];
            vram_set = new byte[0x4000];

            spriteBuffer = new (int Number, int Y, int X, int Pattern, int Attribute)[NumActiveScanlines][];
            for (int i = 0; i < spriteBuffer.Length; i++) spriteBuffer[i] = new (int Number, int Y, int X, int Pattern, int Attribute)[NumSpritesPerLine];

            layerBackgroundForceEnable = true;
            layerSpritesForceEnable = true;
            layerBordersForceEnable = true;
        }

        public object GetRuntimeOption(string name)
        {
            switch (name)
            {
                case layerBackgroundOptionName: return layerBackgroundForceEnable;
                case layerSpritesOptionName: return layerSpritesForceEnable;
                case layerBordersOptionName: return layerBordersForceEnable;
                default: return null;
            }
        }

        public void SetRuntimeOption(string name, object value)
        {
            switch (name)
            {
                case layerBackgroundOptionName: layerBackgroundForceEnable = (bool)value; break;
                case layerSpritesOptionName: layerSpritesForceEnable = (bool)value; break;
                case layerBordersOptionName: layerBordersForceEnable = (bool)value; break;
            }
        }

        public virtual void Startup()
        {
            Reset();

            Debug.Assert(clockRate != 0.0, "Clock rate is zero", "{0} clock rate is not configured", GetType().FullName);
            Debug.Assert(refreshRate != 0.0, "Refresh rate is zero", "{0} refresh rate is not configured", GetType().FullName);
        }

        public virtual void Shutdown()
        {
            //
        }

        public virtual void Reset()
        {
            //for (int i = 0; i < registers.Length; i++) registers[i] = 0;
            //for (int i = 0; i < vram.Length; i++) vram[i] = 0;
            for (int i = 0; i < registersLength; i++) registers[i] = 0;
            for (int i = 0; i < vramLength; i++) vram[i] = 0;

            for (int i = 0; i < spriteBuffer.Length; i++)
                for (int j = 0; j < spriteBuffer[i].Length; j++)
                    spriteBuffer[i][j] = (-1, 0, 0, 0, 0);

            isSecondControlWrite = false;
            controlWord = 0x0000;
            readBuffer = 0;

            statusFlags = StatusFlags.None;

            // TODO/FIXME: begin on random scanline (i.e. Vcounter for SMS/GG) on reset, http://www.smspower.org/forums/post62735#62735
            currentScanline = new Random().Next(scanlineTopBorder, numVisibleScanlines);

            ClearScreenUsage();

            cycleCount = 0;
        }

        public void SetClockRate(double clock)
        {
            clockRate = clock;

            ReconfigureTimings();
        }

        public void SetRefreshRate(double refresh)
        {
            refreshRate = refresh;
            isPalChip = (refreshRate <= 50.0);

            ReconfigureTimings();
        }

        public virtual void SetRevision(int rev)
        {
            Debug.Assert(rev == 0, "Invalid revision", "{0} revision is invalid; only rev 0 is valid", GetType().FullName);
        }

        protected virtual void ReconfigureTimings()
        {
            /* Calculate cycles/line */
            clockCyclesPerLine = (int)Math.Round((clockRate / refreshRate) / numTotalScanlines);

            /* Create arrays */
            screenUsage = new byte[numVisiblePixels * numVisibleScanlines];
            //outputFramebuffer = new byte[(numVisiblePixels * numVisibleScanlines) * 4];
            outputFramebuffer_set = new byte[(numVisiblePixels * numVisibleScanlines) * 4];

            /* Scanline parameters */
            if (!isPalChip)
            {
                topBorderSize = 27;
                verticalActiveDisplaySize = 192;
                bottomBorderSize = 24;
            }
            else
            {
                topBorderSize = 54;
                verticalActiveDisplaySize = 192;
                bottomBorderSize = 48;
            }

            scanlineTopBorder = 0;
            scanlineActiveDisplay = (scanlineTopBorder + topBorderSize);
            scanlineBottomBorder = (scanlineActiveDisplay + verticalActiveDisplaySize);

            numVisibleScanlines = (topBorderSize + verticalActiveDisplaySize + bottomBorderSize);

            /* Pixel parameters */
            leftBorderSize = 13;
            horizontalActiveDisplaySize = 256;
            rightBorderSize = 15;

            pixelLeftBorder = 0;
            pixelActiveDisplay = (pixelLeftBorder + leftBorderSize);
            pixelRightBorder = (pixelActiveDisplay + horizontalActiveDisplaySize);

            numVisiblePixels = (leftBorderSize + horizontalActiveDisplaySize + rightBorderSize);
            var eventArgs = SizeScreenEventArgs.Create(numVisiblePixels, numVisibleScanlines);
            OnSizeScreen(eventArgs);
            eventArgs.Release();

        }

        public virtual void Step(int clockCyclesInStep)
        {
            InterruptLine = ((isFrameInterruptEnabled && isFrameInterruptPending) ? InterruptState.Assert : InterruptState.Clear);

            cycleCount += clockCyclesInStep;

            if (cycleCount >= clockCyclesPerLine)
            {
                OnEndOfScanline(EventArgs.Empty);

                CheckSpriteOverflow(currentScanline);

                RenderLine(currentScanline);

                if (currentScanline == (scanlineBottomBorder + 1))
                    isFrameInterruptPending = true;

                currentScanline++;
                if (currentScanline == numTotalScanlines)
                {
                    currentScanline = 0;
                    ClearScreenUsage();

                    PrepareRenderScreen();
                }

                ParseSpriteTable(currentScanline);

                cycleCount -= clockCyclesPerLine;
                if (cycleCount <= -clockCyclesPerLine) cycleCount = 0;
            }
        }

        //GCHandle? lasyRenderHandle;
        protected virtual void PrepareRenderScreen()
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
            //OnRenderScreen(new RenderScreenEventArgs(numVisiblePixels, numVisibleScanlines, outputFramebuffer.Clone() as byte[]));
        }

        protected virtual void ClearScreenUsage()
        {
            for (int i = 0; i < screenUsage.Length; i++)
                screenUsage[i] = screenUsageEmpty;
        }

        protected virtual void RenderLine(int y)
        {
            if (y >= scanlineTopBorder && y < scanlineActiveDisplay)
            {
                if (layerBordersForceEnable) SetLine(y, backgroundColor);
                else SetLine(y, 0x00, 0x00, 0x00);
            }
            else if (y >= scanlineActiveDisplay && y < scanlineBottomBorder)
            {
                if (layerBackgroundForceEnable)
                {
                    if (isModeGraphics1)
                        RenderLineGraphics1Background(y);
                    else if (isModeGraphics2)
                        RenderLineGraphics2Background(y);
                    else if (isModeMulticolor)
                        RenderLineMulticolorBackground(y);
                    else if (isModeText)
                        RenderLineTextBackground(y);
                }
                else
                    SetLine(y, 0x00, 0x00, 0x00);

                if (layerSpritesForceEnable)
                {
                    if (!isModeText)
                        RenderLineSprites(y);
                }

                RenderBorders(y);
            }
            else if (y >= scanlineBottomBorder && y < numVisibleScanlines)
            {
                if (layerBordersForceEnable) SetLine(y, backgroundColor);
                else SetLine(y, 0x00, 0x00, 0x00);
            }
        }

        protected virtual void RenderBorders(int y)
        {
            for (int x = pixelLeftBorder; x < pixelActiveDisplay; x++)
            {
                if (layerBordersForceEnable) SetPixel(y, x, backgroundColor);
                else SetPixel(y, x, 0x00, 0x00, 0x00);
            }
            for (int x = pixelRightBorder; x < numVisiblePixels; x++)
            {
                if (layerBordersForceEnable) SetPixel(y, x, backgroundColor);
                else SetPixel(y, x, 0x00, 0x00, 0x00);
            }
        }

        protected void SetLine(int y, ushort colorValue)
        {
            for (int x = 0; x < numVisiblePixels; x++)
                SetPixel(y, x, colorValue);
        }

        protected virtual void SetLine(int y, byte b, byte g, byte r)
        {
            for (int x = 0; x < numVisiblePixels; x++)
                SetPixel(y, x, b, g, r);
        }

        protected void SetPixel(int y, int x, ushort colorValue)
        {
            WriteColorToFramebuffer(colorValue, ((y * numVisiblePixels) + (x % numVisiblePixels)) * 4);
        }

        protected virtual void SetPixel(int y, int x, byte b, byte g, byte r)
        {
            WriteColorToFramebuffer(b, g, r, ((y * numVisiblePixels) + (x % numVisiblePixels)) * 4);
        }

        protected byte GetScreenUsageFlag(int y, int x)
        {
            return screenUsage[(y * numVisiblePixels) + (x % numVisiblePixels)];
        }

        protected bool IsScreenUsageFlagSet(int y, int x, byte flag)
        {
            return ((GetScreenUsageFlag(y, x) & flag) == flag);
        }

        protected void SetScreenUsageFlag(int y, int x, byte flag)
        {
            screenUsage[(y * numVisiblePixels) + (x % numVisiblePixels)] |= flag;
        }

        protected void ClearScreenUsageFlag(int y, int x, byte flag)
        {
            screenUsage[(y * numVisiblePixels) + (x % numVisiblePixels)] &= (byte)~flag;
        }

        protected void RenderLineGraphics1Background(int y)
        {
            /* Determine coordinates in active display */
            int activeDisplayY = (y - scanlineActiveDisplay);

            int numColumns = 32;
            ushort patternGeneratorBaseAddress = (ushort)((registers[0x04] & 0x07) << 11);
            ushort colorTableBaseAddress = (ushort)(registers[0x03] << 6);

            for (int column = 0; column < numColumns; column++)
            {
                /* Calculate nametable address, fetch character number */
                ushort nametableAddress = (ushort)(nametableBaseAddress + ((activeDisplayY / 8) * numColumns) + column);
                byte characterNumber = ReadVram(nametableAddress);

                /* Fetch pixel and color data for current pixel line (1 byte, 8 pixels) */
                byte pixelLineData = ReadVram((ushort)(patternGeneratorBaseAddress + (characterNumber * 8) + (activeDisplayY % 8)));
                byte pixelLineColor = ReadVram((ushort)(colorTableBaseAddress + (characterNumber / 8)));

                /* Extract background and foreground color indices */
                byte[] colorIndicesBackgroundForeground = new byte[2];
                colorIndicesBackgroundForeground[0] = (byte)(pixelLineColor & 0x0F);
                colorIndicesBackgroundForeground[1] = (byte)(pixelLineColor >> 4);

                for (int pixel = 0; pixel < 8; pixel++)
                {
                    /* Fetch color index for current pixel (bit clear means background, bit set means foreground color) */
                    byte c = colorIndicesBackgroundForeground[((pixelLineData >> (7 - pixel)) & 0x01)];
                    /* Color index 0 is transparent, use background color */
                    if (c == 0 || isDisplayBlanked) c = backgroundColor;

                    /* Record screen usage, write to framebuffer */
                    int x = pixelActiveDisplay + (column * 8) + pixel;
                    if (GetScreenUsageFlag(y, x) == screenUsageEmpty)
                    {
                        SetPixel(y, x, c);
                        SetScreenUsageFlag(y, x, screenUsageBackground);
                    }
                }
            }
        }

        protected void RenderLineGraphics2Background(int y)
        {
            /* Determine coordinates in active display */
            int activeDisplayY = (y - scanlineActiveDisplay);

            int numColumns = (NumActivePixelsPerScanline / 8);

            /* Calculate some base addresses */
            ushort patternGeneratorBaseAddress = (ushort)((registers[0x04] & 0x04) << 11);
            ushort colorTableBaseAddress = (ushort)((registers[0x03] & 0x80) << 6);

            for (int column = 0; column < numColumns; column++)
            {
                /* Calculate nametable address */
                ushort nametableAddress = (ushort)(nametableBaseAddress + ((activeDisplayY / 8) * numColumns) + column);

                /* Calculate character number and masks */
                ushort characterNumber = (ushort)(((activeDisplayY / 64) << 8) | ReadVram(nametableAddress));
                ushort characterNumberDataMask = (ushort)(((registers[0x04] & 0x03) << 8) | 0xFF);
                ushort characterNumberColorMask = (ushort)(((registers[0x03] & 0x7F) << 3) | 0x07);

                /* Fetch pixel and color data for current pixel line (1 byte, 8 pixels) */
                byte pixelLineData = ReadVram((ushort)(patternGeneratorBaseAddress + ((characterNumber & characterNumberDataMask) * 8) + (activeDisplayY % 8)));
                byte pixelLineColor = ReadVram((ushort)(colorTableBaseAddress + ((characterNumber & characterNumberColorMask) * 8) + (activeDisplayY % 8)));

                /* Extract background and foreground color indices */
                byte[] colorIndicesBackgroundForeground = new byte[2];
                colorIndicesBackgroundForeground[0] = (byte)(pixelLineColor & 0x0F);
                colorIndicesBackgroundForeground[1] = (byte)(pixelLineColor >> 4);

                for (int pixel = 0; pixel < 8; pixel++)
                {
                    /* Fetch color index for current pixel (bit clear means background, bit set means foreground color) */
                    byte c = colorIndicesBackgroundForeground[((pixelLineData >> (7 - pixel)) & 0x01)];
                    /* Color index 0 is transparent, use background color */
                    if (c == 0 || isDisplayBlanked) c = backgroundColor;

                    /* Record screen usage, write to framebuffer */
                    int x = pixelActiveDisplay + (column * 8) + pixel;
                    if (GetScreenUsageFlag(y, x) == screenUsageEmpty)
                    {
                        SetPixel(y, x, c);
                        SetScreenUsageFlag(y, x, screenUsageBackground);
                    }
                }
            }
        }

        protected void RenderLineMulticolorBackground(int y)
        {
            // TODO: check accuracy w/ games besides Smurfs Paint & Play Workshop (if there are any)

            /* Determine coordinates in active display */
            int activeDisplayY = (y - scanlineActiveDisplay);

            int numColumns = 32;
            int row = (activeDisplayY / 8);
            ushort patternGeneratorBaseAddress = (ushort)((registers[0x04] & 0x07) << 11);

            for (int column = 0; column < numColumns; column++)
            {
                /* Calculate nametable address, fetch character number */
                ushort nametableAddress = (ushort)(nametableBaseAddress + (row * numColumns) + column);
                byte characterNumber = ReadVram(nametableAddress);

                /* Calculate pattern generator address, fetch pattern data */
                ushort patternGeneratorAddress = (ushort)(patternGeneratorBaseAddress + (characterNumber * 8) + ((row & 0x03) * 2) + ((activeDisplayY / 4) % 2));
                byte patternData = ReadVram(patternGeneratorAddress);

                for (int block = 0; block < 8; block += 4)
                {
                    /* Fetch color index for current 4x4 block */
                    byte c = (byte)((patternData >> (4 - block)) & 0x0F);

                    /* Color index 0 is transparent, use background color */
                    if (c == 0 || isDisplayBlanked) c = backgroundColor;

                    for (int pixel = 0; pixel < 4; pixel++)
                    {
                        /* Record screen usage, write to framebuffer */
                        int x = pixelActiveDisplay + (column * 8) + (block + pixel);
                        if (GetScreenUsageFlag(y, x) == screenUsageEmpty)
                        {
                            SetPixel(y, x, c);
                            SetScreenUsageFlag(y, x, screenUsageBackground);
                        }
                    }
                }
            }
        }

        protected void RenderLineTextBackground(int y)
        {
            /* Determine coordinates in active display */
            int activeDisplayY = (y - scanlineActiveDisplay);

            int numColumns = 40;
            ushort patternGeneratorBaseAddress = (ushort)((registers[0x04] & 0x07) << 11);

            /* Get background and text color indices */
            byte[] colorIndicesBackgroundForeground = new byte[2];
            colorIndicesBackgroundForeground[0] = backgroundColor;
            colorIndicesBackgroundForeground[1] = textColor;

            /* Draw left and right 8px borders */
            for (int pixel = 0; pixel < 8; pixel++)
            {
                int x = pixelActiveDisplay + pixel;
                SetPixel(y, x + 0, backgroundColor);
                SetPixel(y, x + 8 + (numColumns * 6), backgroundColor);
            }

            /* Draw text columns */
            for (int column = 0; column < numColumns; column++)
            {
                /* Calculate nametable address, fetch character number */
                ushort nametableAddress = (ushort)(nametableBaseAddress + ((activeDisplayY / 8) * numColumns) + column);
                byte characterNumber = ReadVram(nametableAddress);

                /* Fetch pixel data for current pixel line (1 byte, 8 pixels) */
                byte pixelLineData = ReadVram((ushort)(patternGeneratorBaseAddress + (characterNumber * 8) + (activeDisplayY % 8)));

                for (int pixel = 0; pixel < 6; pixel++)
                {
                    /* Fetch color index for current pixel (bit clear means background, bit set means text color) */
                    byte c = (isDisplayBlanked ? backgroundColor : colorIndicesBackgroundForeground[((pixelLineData >> (7 - pixel)) & 0x01)]);

                    /* Record screen usage, write to framebuffer */
                    int x = pixelActiveDisplay + 8 + (column * 6) + pixel;
                    if (GetScreenUsageFlag(y, x) == screenUsageEmpty)
                    {
                        SetPixel(y, x, c);
                        SetScreenUsageFlag(y, x, screenUsageBackground);
                    }
                }
            }
        }

        protected virtual void WriteSpriteNumberToStatus(int spriteNumber)
        {
            statusFlags &= (StatusFlags.FrameInterruptPending | StatusFlags.SpriteOverflow | StatusFlags.SpriteCollision);
            statusFlags |= (StatusFlags)spriteNumber;
        }

        protected virtual void CheckSpriteOverflow(int y)
        {
            /* Ensure current scanline is within active display */
            if (y >= scanlineActiveDisplay && y < scanlineBottomBorder)
            {
                int activeDisplayY = (y - scanlineActiveDisplay);

                /* If last sprite in buffer is valid, sprite overflow occured */
                int lastSpriteInBuffer = spriteBuffer[activeDisplayY][NumSpritesPerLine - 1].Number;
                if (lastSpriteInBuffer != -1)
                {
                    isSpriteOverflow = true;

                    /* Store sprite number in status register */
                    WriteSpriteNumberToStatus(lastSpriteInBuffer);
                }
            }
        }

        protected virtual void ParseSpriteTable(int y)
        {
            if (y < scanlineActiveDisplay || y >= scanlineBottomBorder) return;

            /* Determine coordinates in active display */
            int activeDisplayY = (y - scanlineActiveDisplay);

            /* Clear sprite list for current line */
            for (int i = 0; i < spriteBuffer[activeDisplayY].Length; i++) spriteBuffer[activeDisplayY][i] = (-1, 0, 0, 0, 0);

            /* Determine sprite size & get zoomed sprites adjustment */
            int zoomShift = (isZoomedSprites ? 1 : 0);
            int spriteHeight = ((isLargeSprites ? 16 : 8) << zoomShift);

            int numValidSprites = 0;
            for (int sprite = 0; sprite < NumSprites; sprite++)
            {
                int yCoordinate = ReadVram((ushort)(spriteAttribTableBaseAddress + (sprite * 4)));

                /* Ignore following if Y coord is 208 */
                if (yCoordinate == 208)
                {
                    /* Store first "illegal sprite" number in status register */
                    WriteSpriteNumberToStatus(sprite);
                    return;
                }

                /* Modify Y coord as needed */
                yCoordinate++;
                if (yCoordinate > NumActiveScanlines + 32) yCoordinate -= 256;

                /* Ignore this sprite if on incorrect lines */
                if (activeDisplayY < yCoordinate || activeDisplayY >= (yCoordinate + spriteHeight)) continue;

                /* Check if maximum number of sprites per line is reached */
                numValidSprites++;
                if (numValidSprites > NumSpritesPerLine) return;

                /* Mark sprite for rendering */
                int xCoordinate = ReadVram((ushort)(spriteAttribTableBaseAddress + (sprite * 4) + 1));
                int patternNumber = ReadVram((ushort)(spriteAttribTableBaseAddress + (sprite * 4) + 2));
                int attributes = ReadVram((ushort)(spriteAttribTableBaseAddress + (sprite * 4) + 3));

                spriteBuffer[activeDisplayY][numValidSprites - 1] = (sprite, yCoordinate, xCoordinate, patternNumber, attributes);
            }

            /* Because we didn't bow out before already, store total number of sprites in status register */
            WriteSpriteNumberToStatus(NumSprites - 1);
        }

        protected void RenderLineSprites(int y)
        {
            if (y < scanlineActiveDisplay || y >= scanlineBottomBorder) return;

            /* Determine coordinates in active display */
            int activeDisplayY = (y - scanlineActiveDisplay);

            /* Determine sprite size & get zoomed sprites adjustment */
            int spriteSize = (isLargeSprites ? 16 : 8);
            int zoomShift = (isZoomedSprites ? 1 : 0);
            int numSpritePixels = (spriteSize << zoomShift);

            foreach (var sprite in spriteBuffer[activeDisplayY])
            {
                if (sprite.Number == -1) continue;

                if (!isDisplayBlanked)
                {
                    int yCoordinate = sprite.Y;
                    int xCoordinate = sprite.X;
                    int patternNumber = sprite.Pattern;
                    int attributes = sprite.Attribute;

                    /* Fetch sprite information, extract attributes */
                    bool earlyClock = ((attributes & 0x80) == 0x80);
                    int spriteColor = (attributes & 0x0F);

                    /* Adjust according to registers/attributes */
                    if (earlyClock) xCoordinate -= 32;
                    if (isLargeSprites) patternNumber &= 0xFC;

                    for (int pixel = 0; pixel < numSpritePixels; pixel++)
                    {
                        /* Check if sprite is outside active display, else continue to next sprite */
                        if ((xCoordinate + pixel) < 0 || (xCoordinate + pixel) >= NumActivePixelsPerScanline) continue;

                        /* Determine coordinate inside sprite */
                        int inSpriteXCoord = (pixel >> zoomShift) % spriteSize;
                        int inSpriteYCoord = ((activeDisplayY - yCoordinate) >> zoomShift) % spriteSize;

                        /* Calculate address and fetch pixel data */
                        ushort spritePatternAddress = spritePatternGenBaseAddress;
                        spritePatternAddress += (ushort)(patternNumber << 3);
                        spritePatternAddress += (ushort)inSpriteYCoord;
                        if (inSpriteXCoord >= 8) spritePatternAddress += 16;

                        byte pixelLineData = ReadVram(spritePatternAddress);

                        /* Check if pixel from pattern needs to be drawn, else continue to next sprite */
                        if (((pixelLineData >> (7 - (inSpriteXCoord % 8))) & 0x01) == 0x00) continue;

                        int x = pixelActiveDisplay + (xCoordinate + pixel);
                        if (IsScreenUsageFlagSet(y, x, screenUsageSprite))
                        {
                            /* If sprite was already at this location, set sprite collision flag */
                            isSpriteCollision = true;
                        }
                        else
                        {
                            /* If color isn't transparent, draw pixel to framebuffer */
                            if (spriteColor != 0)
                                SetPixel(y, x, (ushort)spriteColor);
                        }

                        /* Note that there is a sprite here regardless */
                        SetScreenUsageFlag(y, x, screenUsageSprite);
                    }
                }
            }
        }

        protected virtual void WriteColorToFramebuffer(byte b, byte g, byte r, int address)
        {
            outputFramebuffer[address + 0] = b;
            outputFramebuffer[address + 1] = g;
            outputFramebuffer[address + 2] = r;
            outputFramebuffer[address + 3] = 0xFF;
        }

        protected virtual void WriteColorToFramebuffer(ushort colorValue, int address)
        {
            outputFramebuffer[address + 0] = colorValuesBgr[colorValue & 0x0F][0];
            outputFramebuffer[address + 1] = colorValuesBgr[colorValue & 0x0F][1];
            outputFramebuffer[address + 2] = colorValuesBgr[colorValue & 0x0F][2];
            outputFramebuffer[address + 3] = 0xFF;
        }

        protected virtual byte ReadVram(ushort address)
        {
            if (is16kVRAMEnabled)
                return vram[address & vramMask16k];
            else
                return vram[address & vramMask4k];
        }

        protected virtual void WriteVram(ushort address, byte value)
        {
            if (is16kVRAMEnabled)
                vram[address & vramMask16k] = value;
            else
                vram[address & vramMask4k] = value;
        }

        protected virtual byte ReadDataPort()
        {
            isSecondControlWrite = false;
            statusFlags = StatusFlags.None;

            byte data = readBuffer;
            readBuffer = ReadVram(addressRegister);
            addressRegister++;

            return data;
        }

        protected virtual byte ReadControlPort()
        {
            byte statusCurrent = (byte)statusFlags;

            statusFlags = StatusFlags.None;
            isSecondControlWrite = false;

            InterruptLine = InterruptState.Clear;

            return statusCurrent;
        }

        public virtual byte ReadPort(byte port)
        {
            if ((port & 0x01) == 0x00)
                return ReadDataPort();
            else
                return ReadControlPort();
        }

        protected virtual void WriteDataPort(byte value)
        {
            isSecondControlWrite = false;

            readBuffer = value;
            WriteVram(addressRegister, value);
            addressRegister++;
        }

        protected virtual void WriteControlPort(byte value)
        {
            if (!isSecondControlWrite)
                controlWord = (ushort)((controlWord & 0xFF00) | (value << 0));
            else
            {
                controlWord = (ushort)((controlWord & 0x00FF) | (value << 8));

                switch (codeRegister)
                {
                    case 0x00: readBuffer = ReadVram(addressRegister); addressRegister++; break;
                    case 0x01: break;
                    case 0x02: WriteRegister((byte)((controlWord >> 8) & 0x0F), (byte)(controlWord & 0x00FF)); break;
                    case 0x03: break;
                }
            }

            isSecondControlWrite = !isSecondControlWrite;
        }

        public virtual void WritePort(byte port, byte value)
        {
            if ((port & 0x01) == 0x00)
                WriteDataPort(value);
            else
                WriteControlPort(value);
        }

        protected virtual void WriteRegister(byte register, byte value)
        {
            // TODO: confirm register mirroring
            registers[register & 0x07] = value;
        }
    }
}
