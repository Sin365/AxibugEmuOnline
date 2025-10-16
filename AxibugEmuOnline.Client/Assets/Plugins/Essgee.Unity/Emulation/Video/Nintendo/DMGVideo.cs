using Essgee.EventArguments;
using Essgee.Exceptions;
using Essgee.Utilities;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static Essgee.Emulation.CPU.SM83;

namespace Essgee.Emulation.Video.Nintendo
{
    public unsafe class DMGVideo : IVideo
    {
        protected const int displayActiveWidth = 160;
        protected const int displayActiveHeight = 144;
        protected const int numDisplayPixels = displayActiveWidth * displayActiveHeight;

        protected const int displayTotalHeight = 154;

        protected const int numOamSlots = 40;
        protected const int maxSpritesPerLine = 10;

        protected const int mode2Boundary = 80;
        protected const int mode3Boundary = mode2Boundary + 168;

        protected const string layerBackgroundOptionName = "GraphicsLayersShowBackground";
        protected const string layerWindowOptionName = "GraphicsLayersShowWindow";
        protected const string layerSpritesOptionName = "GraphicsLayersShowSprites";

        protected virtual int numSkippedFramesLcdOn => 4;

        protected Action[] modeFunctions;

        protected readonly MemoryReadDelegate memoryReadDelegate;
        protected readonly RequestInterruptDelegate requestInterruptDelegate;

        public virtual (int X, int Y, int Width, int Height) Viewport => (0, 0, displayActiveWidth, displayActiveHeight);

        public virtual event EventHandler<SizeScreenEventArgs> SizeScreen;
        public virtual void OnSizeScreen(SizeScreenEventArgs e) { SizeScreen?.Invoke(this, e); }

        public virtual event EventHandler<RenderScreenEventArgs> RenderScreen;
        public virtual void OnRenderScreen(RenderScreenEventArgs e) { RenderScreen?.Invoke(this, e); }

        public virtual event EventHandler<EventArgs> EndOfScanline;
        public virtual void OnEndOfScanline(EventArgs e) { EndOfScanline?.Invoke(this, e); }

        //

        protected double clockRate, refreshRate;

        //

        [StateRequired]
        protected byte[,] vram;
        [StateRequired]
        protected byte[] oam;

        // FF40 - LCDC
        protected bool lcdEnable, wndMapSelect, wndEnable, bgWndTileSelect, bgMapSelect, objSize, objEnable, bgEnable;

        // FF41 - STAT
        protected bool lycLyInterrupt, m2OamInterrupt, m1VBlankInterrupt, m0HBlankInterrupt, coincidenceFlag;
        protected byte modeNumber;

        // FF42 - SCY
        protected byte scrollY;
        // FF43 - SCX
        protected byte scrollX;

        // FF44 - LY
        protected byte ly;
        // FF45 - LYC
        protected byte lyCompare;

        // FF46 - DMA
        protected byte oamDmaStart;

        // FF47 - BGP
        protected byte bgPalette;
        // FF48 - OBP0
        protected byte obPalette0;
        // FF49 - OBP1
        protected byte obPalette1;

        // FF4A - WY
        protected byte windowY;
        // FF4B - WX
        protected byte windowX;

        //

        protected int numSpritesOnLine, skipFrames;
        protected bool statIrqSignal, vBlankReady;
        protected int[] spritesOnLine;

        //      readonly byte[][] colorValuesBgr = new byte[][]
        //      {
        //	/*              B     G     R */
        //	new byte[] { 0xF8, 0xF8, 0xF8 },	/* White */
        //	new byte[] { 0x9B, 0x9B, 0x9B },	/* Light gray */
        //	new byte[] { 0x3E, 0x3E, 0x3E },	/* Dark gray */
        //	new byte[] { 0x1F, 0x1F, 0x1F },	/* Black */
        //};


        //取值范例 colorValuesBgr[colorIndex * 3 + channelIndex];
        const byte colorValuesBgr_singleLen = 3;
        // 转换后的一维数组
        readonly byte[] colorValuesBgr = new byte[]
        {
        /* White */    0xF8, 0xF8, 0xF8,
        /* Light gray */0x9B, 0x9B, 0x9B,
        /* Dark gray */ 0x3E, 0x3E, 0x3E,
        /* Black */    0x1F, 0x1F, 0x1F
        };

        protected const byte screenUsageEmpty = 0;
        protected const byte screenUsageBackground = 1 << 0;
        protected const byte screenUsageWindow = 1 << 1;
        protected const byte screenUsageSprite = 1 << 2;
        protected byte[,] screenUsageFlags, screenUsageSpriteXCoords, screenUsageSpriteSlots;

        protected int cycleCount, cycleDotPause, currentScanline;
        //protected byte[] outputFramebuffer;

        #region //指针化 outputFramebuffer
        byte[] outputFramebuffer_src;
        protected GCHandle outputFramebuffer_handle;
        public byte* outputFramebuffer;
        public int outputFramebufferLength;
        public bool outputFramebuffer_IsNull => outputFramebuffer == null;
        public byte[] outputFramebuffer_set
        {
            set
            {
                outputFramebuffer_handle.ReleaseGCHandle();
                outputFramebuffer_src = value;
                outputFramebufferLength = value.Length;
                outputFramebuffer_src.GetObjectPtr(ref outputFramebuffer_handle, ref outputFramebuffer);
            }
        }
        #endregion


        #region //指针化 outputFramebufferCopy
        byte[] outputFramebufferCopy_src;
        GCHandle outputFramebufferCopy_handle;
        public byte* outputFramebufferCopy;
        public int outputFramebufferCopyLength;
        public bool outputFramebufferCopy_IsNull => outputFramebufferCopy == null;
        private IntPtr outputFramebufferCopy_IntPtr;
        public byte[] outputFramebufferCopy_set
        {
            set
            {
                outputFramebufferCopy_handle.ReleaseGCHandle();
                outputFramebufferCopy_src = value;
                outputFramebufferCopyLength = value.Length;
                outputFramebufferCopy_src.GetObjectPtr(ref outputFramebufferCopy_handle, ref outputFramebufferCopy);
                outputFramebufferCopy_IntPtr = outputFramebufferCopy_handle.AddrOfPinnedObject();
            }
        }
        #endregion



        protected int clockCyclesPerLine;

        public bool IsDoubleSpeed { get; set; }

        //

        public (string Name, string Description)[] RuntimeOptions => new (string name, string description)[]
        {
            (layerBackgroundOptionName, "Background"),
            (layerWindowOptionName, "Window"),
            (layerSpritesOptionName, "Sprites"),
        };

        protected bool layerBackgroundForceEnable, layerWindowForceEnable, layerSpritesForceEnable;

        public DMGVideo(MemoryReadDelegate memoryRead, RequestInterruptDelegate requestInterrupt)
        {
            vram = new byte[1, 0x2000];
            oam = new byte[0xA0];

            //

            modeFunctions = new Action[] { StepHBlank, StepVBlank, StepOAMSearch, StepLCDTransfer };

            spritesOnLine = new int[maxSpritesPerLine];

            memoryReadDelegate = memoryRead;
            requestInterruptDelegate = requestInterrupt;

            layerBackgroundForceEnable = true;
            layerWindowForceEnable = true;
            layerSpritesForceEnable = true;
        }

        #region AxiState

        public void LoadAxiStatus(AxiEssgssStatusData data)
        {
            vram = data.Array2DMemberData[nameof(vram)].Get2DArrayBytesData();
            oam = data.MemberData[nameof(oam)];
        }

        public AxiEssgssStatusData SaveAxiStatus()
        {
            AxiEssgssStatusData data = new AxiEssgssStatusData();
            data.Array2DMemberData[nameof(vram)] = new AxiEssgssStatusData_2DArray(vram);
            data.MemberData[nameof(oam)] = oam;
            return data;
        }
        #endregion
        public object GetRuntimeOption(string name)
        {
            switch (name)
            {
                case layerBackgroundOptionName: return layerBackgroundForceEnable;
                case layerWindowOptionName: return layerWindowForceEnable;
                case layerSpritesOptionName: return layerSpritesForceEnable;
                default: return null;
            }
        }

        public void SetRuntimeOption(string name, object value)
        {
            switch (name)
            {
                case layerBackgroundOptionName: layerBackgroundForceEnable = (bool)value; break;
                case layerWindowOptionName: layerWindowForceEnable = (bool)value; break;
                case layerSpritesOptionName: layerSpritesForceEnable = (bool)value; break;
            }
        }

        public virtual void Startup()
        {
            Reset();

            if (memoryReadDelegate == null) throw new EmulationException("DMGVideo: Memory read delegate is null");
            if (requestInterruptDelegate == null) throw new EmulationException("DMGVideo: Request interrupt delegate is null");

            Debug.Assert(clockRate != 0.0, "Clock rate is zero", "{0} clock rate is not configured", GetType().FullName);
            Debug.Assert(refreshRate != 0.0, "Refresh rate is zero", "{0} refresh rate is not configured", GetType().FullName);
        }

        public virtual void Shutdown()
        {
            //
        }

        public virtual void Reset()
        {
            for (var i = 0; i < vram.GetLength(0); i++) vram[0, i] = 0;
            for (var i = 0; i < oam.Length; i++) oam[i] = 0;

            for (var i = (byte)0x40; i < 0x4C; i++)
            {
                // skip OAM dma
                if (i != 0x46) WritePort(i, 0x00);
            }

            numSpritesOnLine = skipFrames = 0;
            statIrqSignal = vBlankReady = false;

            for (var i = 0; i < spritesOnLine.Length; i++) spritesOnLine[i] = -1;

            ClearScreenUsage();

            cycleCount = cycleDotPause = currentScanline = 0;
        }

        public void SetClockRate(double clock)
        {
            clockRate = clock;

            ReconfigureTimings();
        }

        public void SetRefreshRate(double refresh)
        {
            refreshRate = refresh;

            ReconfigureTimings();
        }

        public virtual void SetRevision(int rev)
        {
            Debug.Assert(rev == 0, "Invalid revision", "{0} revision is invalid; only rev 0 is valid", GetType().FullName);
        }

        protected virtual void ReconfigureTimings()
        {
            /* Calculate cycles/line */
            clockCyclesPerLine = (int)Math.Round((clockRate / refreshRate) / displayTotalHeight);

            /* Create arrays */
            screenUsageFlags = new byte[displayActiveWidth, displayActiveHeight];
            screenUsageSpriteXCoords = new byte[displayActiveWidth, displayActiveHeight];
            screenUsageSpriteSlots = new byte[displayActiveWidth, displayActiveHeight];
            //outputFramebuffer = new byte[numDisplayPixels * 4];
            outputFramebuffer_set = new byte[numDisplayPixels * 4];

            outputFramebufferCopy_set = new byte[numDisplayPixels * 4];

            for (var y = 0; y < displayActiveHeight; y++)
                SetLine(y, 0xFF, 0xFF, 0xFF);
        }

        public virtual void Step(int clockCyclesInStep)
        {
            for (var c = 0; c < clockCyclesInStep; c++)
            {
                if (lcdEnable)
                {
                    /* LCD enabled, handle LCD modes */
                    modeFunctions[modeNumber]();
                }
                else
                {
                    /* LCD disabled */
                    modeNumber = 0;
                    cycleCount = 0;
                    cycleDotPause = 0;

                    currentScanline = 0;
                    ly = 0;
                }
            }
        }

        /* TODO: dot clock pause! -- https://gbdev.io/pandocs/#properties-of-stat-modes
		 * - non-zero (SCX % 8) causes pause of that many dots
		 * - active window causes pause of *at least* 6 dots?
		 * - sprite dot pauses??
		 * - influence of CGB double-speed mode on this?
		 * Required for: 
		 * - GBVideoPlayer2, pixel column alignment & garbage on left screen edge
		 * - snorpung/pocket demo, vertical scroller right edge of scroll area
		 * - Prehistorik Man, scroller alignment (ex. level start "START" text should be centered on screen)
		 * - ...probably more?
		*/

        protected virtual void StepVBlank()
        {
            // TODO: *should* be 4, but Altered Space hangs w/ any value lower than 8?
            if (cycleCount == 8 && vBlankReady)
            {
                requestInterruptDelegate(InterruptSource.VBlank);
                vBlankReady = false;
            }

            /* V-blank */
            cycleCount++;
            if (cycleCount == clockCyclesPerLine) EndVBlank();
        }

        protected virtual void EndVBlank()
        {
            /* End of scanline reached */
            OnEndOfScanline(EventArgs.Empty);
            currentScanline++;
            ly = (byte)currentScanline;

            /* Check for & request STAT interrupts */
            CheckAndRequestStatInterupt();

            if (currentScanline == displayTotalHeight - 1)
            {
                // TODO: specific cycle this happens?

                /* LY reports as 0 on line 153 */
                ly = 0;

                // TODO: verify if STAT/LYC interrupt is supposed to happen here? currently breaks Shantae's sprites if done
                //CheckAndRequestStatInterupt();
            }
            else if (currentScanline == displayTotalHeight)
            {
                /* End of V-blank reached */
                modeNumber = 2;
                currentScanline = 0;
                ly = 0;

                CheckAndRequestStatInterupt();

                ClearScreenUsage();
            }

            cycleCount = 0;
        }

        protected virtual void StepOAMSearch()
        {
            /* OAM search */

            if ((cycleCount % 2) == 0)
            {
                /* Get object Y coord */
                var objIndex = cycleCount >> 1;
                var objY = oam[(objIndex << 2) + 0] - 16;

                /* Check if object is on current scanline & maximum number of objects was not exceeded, then increment counter */
                if (currentScanline >= objY && currentScanline < (objY + (objSize ? 16 : 8)) && numSpritesOnLine < maxSpritesPerLine)
                {
                    var objX = oam[(objIndex << 2) + 1] - 8;
                    cycleDotPause += 11 - Math.Min(5, (objX + (objX >= windowX ? (255 - windowX) : scrollX)) % 8);  // TODO: correct?
                    spritesOnLine[numSpritesOnLine++] = objIndex;
                }
            }

            /* Increment cycle count & check for next LCD mode */
            cycleCount++;
            if (cycleCount == mode2Boundary) EndOAMSearch();
        }

        protected virtual void EndOAMSearch()
        {
            // 1) GBVideoPlayer2 & Prehistorik Man alignment, 2) GBVideoPlayer2 alignment
            cycleDotPause += (6 << (IsDoubleSpeed ? 1 : 0)) + (scrollX % 8);

            modeNumber = 3;
            CheckAndRequestStatInterupt();
        }

        protected virtual void StepLCDTransfer()
        {
            /* Data transfer to LCD */

            /* Render pixels */
            RenderPixel(currentScanline, cycleCount - mode2Boundary - cycleDotPause);

            /* Increment cycle count & check for next LCD mode */
            cycleCount++;
            // 3) snorpung/pocket scroller width
            if (cycleCount == mode3Boundary + cycleDotPause + (wndEnable ? 12 : 0)) EndLCDTransfer();
        }

        protected virtual void EndLCDTransfer()
        {
            modeNumber = 0;
            CheckAndRequestStatInterupt();
        }

        protected virtual void StepHBlank()
        {
            /* H-blank */

            /* Increment cycle count & check for next LCD mode */
            cycleCount++;
            if (cycleCount == clockCyclesPerLine) EndHBlank();
        }


        GCHandle? lastRenderHandle;

        protected virtual void EndHBlank()
        {
            /* End of scanline reached */
            OnEndOfScanline(EventArgs.Empty);
            currentScanline++;
            ly = (byte)currentScanline;

            CheckAndRequestStatInterupt();

            for (var i = 0; i < spritesOnLine.Length; i++) spritesOnLine[i] = -1;
            numSpritesOnLine = 0;

            if (currentScanline == displayActiveHeight)
            {
                modeNumber = 1;
                CheckAndRequestStatInterupt();

                /* Reached V-blank, request V-blank interrupt */
                vBlankReady = true;

                if (skipFrames > 0) skipFrames--;

                /* Submit screen for rendering */

                //// 固定数组，防止垃圾回收器移动它  
                //var bitmapcolorRect_handle = GCHandle.Alloc(outputFramebuffer_src.Clone() as byte[], GCHandleType.Pinned);
                //// 获取数组的指针  
                //IntPtr mFrameDataPtr = bitmapcolorRect_handle.AddrOfPinnedObject();


                for (int i = 0; i < outputFramebufferLength; i++)
                {
                    outputFramebufferCopy[i] = outputFramebuffer[i];
                }
                IntPtr mFrameDataPtr = outputFramebufferCopy_IntPtr;

                var eventArgs = RenderScreenEventArgs.Create(displayActiveWidth, displayActiveHeight, mFrameDataPtr);
                OnRenderScreen(eventArgs);
                eventArgs.Release();

                //if (lastRenderHandle.HasValue)
                //    lastRenderHandle.Value.Free();
                //lastRenderHandle = bitmapcolorRect_handle;

                //OnRenderScreen(new RenderScreenEventArgs(displayActiveWidth, displayActiveHeight, outputFramebuffer.Clone() as byte[]));
            }
            else
            {
                modeNumber = 2;
                CheckAndRequestStatInterupt();
            }

            cycleCount = 0;
            cycleDotPause = 0;
        }

        protected void CheckAndRequestStatInterupt()
        {
            if (!lcdEnable) return;

            var oldSignal = statIrqSignal;
            statIrqSignal = false;

            if (modeNumber == 0 && m0HBlankInterrupt) statIrqSignal = true;
            if (modeNumber == 1 && m1VBlankInterrupt) statIrqSignal = true;
            if (modeNumber == 2 && m2OamInterrupt) statIrqSignal = true;

            coincidenceFlag = (ly == lyCompare);
            if (coincidenceFlag && lycLyInterrupt) statIrqSignal = true;

            if (!oldSignal && statIrqSignal)
                requestInterruptDelegate(InterruptSource.LCDCStatus);
        }

        protected virtual void RenderPixel(int y, int x)
        {
            if (x < 0 || x >= displayActiveWidth || y < 0 || y >= displayActiveHeight) return;

            if (skipFrames > 0)
            {
                SetPixel(y, x, 0xFF, 0xFF, 0xFF);
                return;
            }

            screenUsageFlags[x, y] = screenUsageEmpty;

            if (bgEnable)
            {
                RenderBackground(y, x);
                if (wndEnable) RenderWindow(y, x);
            }
            else
                SetPixel(y, x, 0xFF, 0xFF, 0xFF);

            if (objEnable) RenderSprites(y, x);
        }

        protected virtual void RenderBackground(int y, int x)
        {
            // Get base addresses
            var tileBase = (ushort)(bgWndTileSelect ? 0x0000 : 0x0800);
            var mapBase = (ushort)(bgMapSelect ? 0x1C00 : 0x1800);

            // Calculate tilemap address & get tile
            var yTransformed = (byte)(scrollY + y);
            var xTransformed = (byte)(scrollX + x);
            var mapAddress = mapBase + ((yTransformed >> 3) << 5) + (xTransformed >> 3);
            var tileNumber = vram[0, mapAddress];
            if (!bgWndTileSelect)
                tileNumber = (byte)(tileNumber ^ 0x80);

            // Calculate tile address & get pixel color index
            var tileAddress = tileBase + (tileNumber << 4) + ((yTransformed & 7) << 1);
            var ba = (vram[0, tileAddress + 0] >> (7 - (xTransformed % 8))) & 0b1;
            var bb = (vram[0, tileAddress + 1] >> (7 - (xTransformed % 8))) & 0b1;
            var c = (byte)((bb << 1) | ba);

            // If color is not 0, note that a BG pixel exists here
            if (c != 0)
                screenUsageFlags[x, y] |= screenUsageBackground;

            // Draw pixel
            if (layerBackgroundForceEnable)
                SetPixel(y, x, (byte)((bgPalette >> (c << 1)) & 0x03));
            else
                SetPixel(y, x, (byte)(bgPalette & 0x03));
        }

        protected virtual void RenderWindow(int y, int x)
        {
            // Check if current coords are inside window
            if (y < windowY) return;
            if (x < (windowX - 7)) return;

            // Get base addresses
            var tileBase = (ushort)(bgWndTileSelect ? 0x0000 : 0x0800);
            var mapBase = (ushort)(wndMapSelect ? 0x1C00 : 0x1800);

            // Calculate tilemap address & get tile
            var yTransformed = (byte)(y - windowY);
            var xTransformed = (byte)((7 - windowX) + x);
            var mapAddress = mapBase + ((yTransformed >> 3) << 5) + (xTransformed >> 3);
            var tileNumber = vram[0, mapAddress];
            if (!bgWndTileSelect)
                tileNumber = (byte)(tileNumber ^ 0x80);

            // Calculate tile address & get pixel color index
            var tileAddress = tileBase + (tileNumber << 4) + ((yTransformed & 7) << 1);
            var ba = (vram[0, tileAddress + 0] >> (7 - (xTransformed % 8))) & 0b1;
            var bb = (vram[0, tileAddress + 1] >> (7 - (xTransformed % 8))) & 0b1;
            var c = (byte)((bb << 1) | ba);

            // If color is not 0, note that a Window pixel exists here
            if (c != 0)
                screenUsageFlags[x, y] |= screenUsageWindow;

            // Draw pixel
            if (layerWindowForceEnable)
                SetPixel(y, x, (byte)((bgPalette >> (c << 1)) & 0x03));
            else
                SetPixel(y, x, (byte)(bgPalette & 0x03));
        }

        protected virtual void RenderSprites(int y, int x)
        {
            var objHeight = objSize ? 16 : 8;

            // Iterate over sprite on line
            for (var s = 0; s < numSpritesOnLine; s++)
            {
                var i = spritesOnLine[s];

                // Get sprite Y coord & if sprite is not on current scanline, continue to next slot
                var objY = (short)(oam[(i * 4) + 0] - 16);
                if (y < objY || y >= (objY + objHeight)) continue;

                // Get sprite X coord, tile number & attributes
                var objX = (byte)(oam[(i * 4) + 1] - 8);
                var objTileNumber = oam[(i * 4) + 2];
                var objAttributes = oam[(i * 4) + 3];

                // Extract attributes
                var objFlipY = ((objAttributes >> 6) & 0b1) == 0b1;
                var objFlipX = ((objAttributes >> 5) & 0b1) == 0b1;
                var objPalNumber = (objAttributes >> 4) & 0b1;

                // Iterate over pixels
                for (var px = 0; px < 8; px++)
                {
                    // If sprite pixel X coord does not equal current rendering X coord, continue to next pixel
                    if (x != (byte)(objX + px)) continue;

                    // Calculate tile address
                    var xShift = objFlipX ? (px % 8) : (7 - (px % 8));
                    var yShift = objFlipY ? (7 - ((y - objY) % 8)) : ((y - objY) % 8);
                    if (objSize)
                    {
                        objTileNumber &= 0xFE;
                        if ((objFlipY && y < (objY + 8)) || (!objFlipY && y >= (objY + 8)))
                            objTileNumber |= 0x01;
                    }
                    var tileAddress = (objTileNumber << 4) + (yShift << 1);

                    // Get palette & bitplanes
                    var pal = objPalNumber == 0 ? obPalette0 : obPalette1;
                    var ba = (vram[0, tileAddress + 0] >> xShift) & 0b1;
                    var bb = (vram[0, tileAddress + 1] >> xShift) & 0b1;

                    // Combine to color index, continue drawing if color is not 0
                    var c = (byte)((bb << 1) | ba);
                    if (c != 0)
                    {
                        // If sprite does not have priority i.e. if sprite should not be drawn, continue to next pixel
                        if (!HasSpritePriority(y, x, i)) continue;

                        screenUsageFlags[x, y] |= screenUsageSprite;
                        screenUsageSpriteSlots[x, y] = (byte)i;
                        screenUsageSpriteXCoords[x, y] = objX;

                        // Draw pixel
                        if (layerSpritesForceEnable)
                            SetPixel(y, x, (byte)((pal >> (c << 1)) & 0x03));
                    }
                }
            }
        }

        protected virtual bool HasSpritePriority(int y, int x, int objSlot)
        {
            // Get new sprite X coord
            var objX = (byte)(oam[(objSlot * 4) + 1] - 8);

            // Get potentially existing sprite X coord and slot
            var prevX = screenUsageSpriteXCoords[x, y];
            var prevSlot = screenUsageSpriteSlots[x, y];

            // If existing sprite has lower X coord -or- both sprites have same X coord BUT existing sprite has lower slot, new sprite does not have priority
            if (prevX < objX || (prevX == objX && prevSlot < objSlot))
                return false;

            // Get new sprite OBJ-to-BG priority attribute
            var objIsBehindBg = ((oam[(objSlot * 4) + 3] >> 7) & 0b1) == 0b1;

            // If new sprite is shown behind BG/Window -and- a BG/Window pixel has already been drawn, new sprite does not have priority
            if (objIsBehindBg &&
                (IsScreenUsageFlagSet(y, x, screenUsageBackground) || IsScreenUsageFlagSet(y, x, screenUsageWindow))) return false;

            // New sprite has priority
            return true;
        }

        protected void SetLine(int y, byte c)
        {
            for (int x = 0; x < displayActiveWidth; x++)
                SetPixel(y, x, c);
        }

        protected void SetLine(int y, byte b, byte g, byte r)
        {
            for (int x = 0; x < displayActiveWidth; x++)
                SetPixel(y, x, b, g, r);
        }

        protected void SetPixel(int y, int x, byte c)
        {
            WriteColorToFramebuffer(c, ((y * displayActiveWidth) + (x % displayActiveWidth)) * 4);
        }

        protected void SetPixel(int y, int x, byte b, byte g, byte r)
        {
            WriteColorToFramebuffer(b, g, r, ((y * displayActiveWidth) + (x % displayActiveWidth)) * 4);
        }

        protected virtual void WriteColorToFramebuffer(byte c, int address)
        {
            //outputFramebuffer[address + 0] = colorValuesBgr[c & 0x03][0];
            //outputFramebuffer[address + 1] = colorValuesBgr[c & 0x03][1];
            //outputFramebuffer[address + 2] = colorValuesBgr[c & 0x03][2];
            outputFramebuffer[address + 0] = colorValuesBgr[(c & 0x03) * 3 + 0];
            outputFramebuffer[address + 1] = colorValuesBgr[(c & 0x03) * 3 + 1];
            outputFramebuffer[address + 2] = colorValuesBgr[(c & 0x03) * 3 + 2];
            outputFramebuffer[address + 3] = 0xFF;
        }

        protected virtual void WriteColorToFramebuffer(byte b, byte g, byte r, int address)
        {
            outputFramebuffer[address + 0] = b;
            outputFramebuffer[address + 1] = g;
            outputFramebuffer[address + 2] = r;
            outputFramebuffer[address + 3] = 0xFF;
        }

        protected virtual void ClearScreenUsage()
        {
            for (var y = 0; y < displayActiveHeight; y++)
            {
                for (var x = 0; x < displayActiveWidth; x++)
                {
                    screenUsageFlags[x, y] = screenUsageEmpty;
                    screenUsageSpriteXCoords[x, y] = 255;
                    screenUsageSpriteSlots[x, y] = numOamSlots;
                }
            }
        }

        protected bool IsScreenUsageFlagSet(int y, int x, byte flag)
        {
            return (screenUsageFlags[x, y] & flag) == flag;
        }

        public virtual byte ReadVram(ushort address)
        {
            if (modeNumber != 3)
                return vram[0, address & (vram.Length - 1)];
            else
                return 0xFF;
        }

        public virtual void WriteVram(ushort address, byte value)
        {
            if (modeNumber != 3)
                vram[0, address & (vram.Length - 1)] = value;
        }

        public virtual byte ReadOam(ushort address)
        {
            if (modeNumber != 2 && modeNumber != 3)
                return oam[address - 0xFE00];
            else
                return 0xFF;
        }

        public virtual void WriteOam(ushort address, byte value)
        {
            if (modeNumber != 2 && modeNumber != 3)
                oam[address - 0xFE00] = value;
        }

        public virtual byte ReadPort(byte port)
        {
            switch (port)
            {
                case 0x40:
                    // LCDC
                    return (byte)(
                        (lcdEnable ? (1 << 7) : 0) |
                        (wndMapSelect ? (1 << 6) : 0) |
                        (wndEnable ? (1 << 5) : 0) |
                        (bgWndTileSelect ? (1 << 4) : 0) |
                        (bgMapSelect ? (1 << 3) : 0) |
                        (objSize ? (1 << 2) : 0) |
                        (objEnable ? (1 << 1) : 0) |
                        (bgEnable ? (1 << 0) : 0));

                case 0x41:
                    // STAT
                    return (byte)(
                        0x80 |
                        (lycLyInterrupt ? (1 << 6) : 0) |
                        (m2OamInterrupt ? (1 << 5) : 0) |
                        (m1VBlankInterrupt ? (1 << 4) : 0) |
                        (m0HBlankInterrupt ? (1 << 3) : 0) |
                        (coincidenceFlag ? (1 << 2) : 0) |
                        ((modeNumber & 0b11) << 0));

                case 0x42:
                    // SCY
                    return scrollY;

                case 0x43:
                    // SCX
                    return scrollX;

                case 0x44:
                    // LY
                    return ly;

                case 0x45:
                    // LYC
                    return lyCompare;

                case 0x46:
                    // DMA
                    return oamDmaStart;

                case 0x47:
                    // BGP
                    return bgPalette;

                case 0x48:
                    // OBP0
                    return obPalette0;

                case 0x49:
                    // OBP1
                    return obPalette1;

                case 0x4A:
                    // WY
                    return windowY;

                case 0x4B:
                    //WX
                    return windowX;

                default:
                    return 0xFF;
            }
        }

        public virtual void WritePort(byte port, byte value)
        {
            switch (port)
            {
                case 0x40:
                    // LCDC
                    {
                        var newLcdEnable = ((value >> 7) & 0b1) == 0b1;
                        if (lcdEnable != newLcdEnable)
                        {
                            modeNumber = 2;
                            currentScanline = 0;
                            ly = 0;

                            CheckAndRequestStatInterupt();

                            if (newLcdEnable)
                                skipFrames = numSkippedFramesLcdOn;
                        }

                        lcdEnable = newLcdEnable;
                        wndMapSelect = ((value >> 6) & 0b1) == 0b1;
                        wndEnable = ((value >> 5) & 0b1) == 0b1;
                        bgWndTileSelect = ((value >> 4) & 0b1) == 0b1;
                        bgMapSelect = ((value >> 3) & 0b1) == 0b1;
                        objSize = ((value >> 2) & 0b1) == 0b1;
                        objEnable = ((value >> 1) & 0b1) == 0b1;
                        bgEnable = ((value >> 0) & 0b1) == 0b1;
                    }
                    break;

                case 0x41:
                    // STAT
                    lycLyInterrupt = ((value >> 6) & 0b1) == 0b1;
                    m2OamInterrupt = ((value >> 5) & 0b1) == 0b1;
                    m1VBlankInterrupt = ((value >> 4) & 0b1) == 0b1;
                    m0HBlankInterrupt = ((value >> 3) & 0b1) == 0b1;

                    CheckAndRequestStatInterupt();

                    // TODO: correct?
                    if (lcdEnable && modeNumber == 1 && currentScanline != 0)
                        requestInterruptDelegate(InterruptSource.LCDCStatus);
                    break;

                case 0x42:
                    // SCY
                    scrollY = value;
                    break;

                case 0x43:
                    // SCX
                    scrollX = value;
                    break;

                case 0x44:
                    // LY
                    break;

                case 0x45:
                    // LYC
                    lyCompare = value;
                    CheckAndRequestStatInterupt();
                    break;

                case 0x46:
                    // DMA
                    oamDmaStart = value;
                    for (int src = 0, dst = oamDmaStart << 8; src < 0xA0; src++, dst++)
                        oam[src] = memoryReadDelegate((ushort)dst);
                    break;

                case 0x47:
                    // BGP
                    bgPalette = value;
                    break;

                case 0x48:
                    // OBP0
                    obPalette0 = value;
                    break;

                case 0x49:
                    // OBP1
                    obPalette1 = value;
                    break;

                case 0x4A:
                    // WY
                    windowY = value;
                    break;

                case 0x4B:
                    // WX
                    windowX = value;
                    break;
            }
        }
    }
}
