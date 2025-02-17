using System;

using static Essgee.Emulation.CPU.SM83;
using static Essgee.Emulation.Utilities;

namespace Essgee.Emulation.Video.Nintendo
{
    public class CGBVideo : DMGVideo
    {
        protected override int numSkippedFramesLcdOn => 1;

        // FF4F - VBK
        byte vramBank;

        // FF51 - HDMA1
        byte dmaSourceHi;

        // FF52 - HDMA2
        byte dmaSourceLo;

        // FF53 - HDMA3
        byte dmaDestinationHi;

        // FF54 - HDMA4
        byte dmaDestinationLo;

        // FF55 - HDMA5
        byte dmaTransferBlockLength;
        bool dmaTransferIsHDMA;

        // FF68 - BCPS
        byte bgPaletteIndex;
        bool bgPaletteAutoIncrement;

        // FF69 - BCPD

        // FF6A - OCPS
        byte objPaletteIndex;
        bool objPaletteAutoIncrement;

        // FF6B - OCPD

        //

        byte[] bgPaletteData, objPaletteData;

        ushort dmaSourceAddress, dmaDestinationAddress;
        int dmaTransferByteLength;

        bool hdmaIsActive;
        byte hdmaBytesLeft;

        public int GDMAWaitCycles { get; set; }

        protected const byte screenUsageBackgroundHighPriority = (1 << 3);

        public CGBVideo(MemoryReadDelegate memoryRead, RequestInterruptDelegate requestInterrupt) : base(memoryRead, requestInterrupt)
        {
            vram = new byte[2, 0x2000];

            //

            bgPaletteData = new byte[64];
            objPaletteData = new byte[64];
        }

        #region AxiState

        public void LoadAxiStatus(AxiEssgssStatusData data)
        {
            base.LoadAxiStatus(data);
        }

        public AxiEssgssStatusData SaveAxiStatus()
        {
            AxiEssgssStatusData data = base.SaveAxiStatus();
            return data;
        }
        #endregion

        public override void Reset()
        {
            base.Reset();

            vramBank = 0;

            dmaTransferBlockLength = 0;
            dmaTransferIsHDMA = false;

            bgPaletteAutoIncrement = true;
            objPaletteAutoIncrement = true;

            for (var i = 0; i < bgPaletteData.Length; i += 2)
            {
                bgPaletteData[i + 1] = 0x7F;
                bgPaletteData[i + 0] = 0xFF;
            }
            for (var i = 0; i < objPaletteData.Length; i += 2)
            {
                objPaletteData[i + 1] = 0x7F;
                objPaletteData[i + 0] = 0xFF;
            }

            dmaSourceAddress = dmaDestinationAddress = 0;
            dmaTransferByteLength = 0;

            hdmaIsActive = false;
            hdmaBytesLeft = 0;
        }


        //

        protected override void StepHBlank()
        {
            /* Check and perform HDMA */
            if (hdmaIsActive && dmaTransferIsHDMA)
            {
                if (hdmaBytesLeft > 0 && (cycleCount % 2) == 0)
                {
                    WriteVram(dmaDestinationAddress, memoryReadDelegate(dmaSourceAddress));
                    dmaDestinationAddress++;
                    dmaSourceAddress++;

                    dmaDestinationAddress &= 0x1FFF;

                    dmaTransferByteLength--;
                    hdmaBytesLeft--;

                    if (dmaTransferByteLength == 0)
                        hdmaIsActive = false;

                    UpdateDMAStatus();
                }
            }

            /* Increment cycle count & check for next LCD mode */
            cycleCount++;
            if (cycleCount == clockCyclesPerLine)
            {
                if (hdmaIsActive)
                    hdmaBytesLeft = (byte)Math.Min(0x10, 0x10 - (dmaTransferByteLength % 16));

                EndHBlank();
            }
        }

        //

        protected override void RenderPixel(int y, int x)
        {
            if (x < 0 || x >= displayActiveWidth || y < 0 || y >= displayActiveHeight) return;

            if (skipFrames > 0)
            {
                SetPixel(y, x, 0xFF, 0xFF, 0xFF);
                return;
            }

            screenUsageFlags[x, y] = screenUsageEmpty;

            RenderBackground(y, x);

            if (wndEnable) RenderWindow(y, x);
            if (objEnable) RenderSprites(y, x);
        }

        protected override void RenderBackground(int y, int x)
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

            // Get & extract tile attributes
            var tileAttribs = vram[1, mapAddress];
            var tileBgPalette = tileAttribs & 0b111;
            var tileVramBank = (tileAttribs >> 3) & 0b1;
            var tileHorizontalFlip = ((tileAttribs >> 5) & 0b1) == 0b1;
            var tileVerticalFlip = ((tileAttribs >> 6) & 0b1) == 0b1;
            var tileBgHasPriority = ((tileAttribs >> 7) & 0b1) == 0b1;

            // Calculate tile address & get pixel color index
            var xShift = tileHorizontalFlip ? (xTransformed % 8) : (7 - (xTransformed % 8));
            var yShift = tileVerticalFlip ? (7 - (yTransformed & 7)) : (yTransformed & 7);
            var tileAddress = tileBase + (tileNumber << 4) + (yShift << 1);
            var ba = (vram[tileVramBank, tileAddress + 0] >> xShift) & 0b1;
            var bb = (vram[tileVramBank, tileAddress + 1] >> xShift) & 0b1;
            var c = (byte)((bb << 1) | ba);

            // If color is not 0, note that a BG pixel (normal or high-priority) exists here
            if (c != 0)
                screenUsageFlags[x, y] |= tileBgHasPriority ? screenUsageBackgroundHighPriority : screenUsageBackground;

            // Calculate color address in palette & draw pixel
            if (layerBackgroundForceEnable)
            {
                var paletteAddress = (tileBgPalette << 3) + ((c & 0b11) << 1);
                SetPixel(y, x, (ushort)((bgPaletteData[paletteAddress + 1] << 8) | bgPaletteData[paletteAddress + 0]));
            }
            else
                SetPixel(y, x, (ushort)((bgPaletteData[1] << 8) | bgPaletteData[0]));
        }

        protected override void RenderWindow(int y, int x)
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

            // Get & extract tile attributes
            var tileAttribs = vram[1, mapAddress];
            var tileBgPalette = tileAttribs & 0b111;
            var tileVramBank = (tileAttribs >> 3) & 0b1;
            var tileHorizontalFlip = ((tileAttribs >> 5) & 0b1) == 0b1;
            var tileVerticalFlip = ((tileAttribs >> 6) & 0b1) == 0b1;
            var tileBgHasPriority = ((tileAttribs >> 7) & 0b1) == 0b1;

            // Calculate tile address & get pixel color index
            var xShift = tileHorizontalFlip ? (xTransformed % 8) : (7 - (xTransformed % 8));
            var yShift = tileVerticalFlip ? (7 - (yTransformed & 7)) : (yTransformed & 7);
            var tileAddress = tileBase + (tileNumber << 4) + (yShift << 1);
            var ba = (vram[tileVramBank, tileAddress + 0] >> xShift) & 0b1;
            var bb = (vram[tileVramBank, tileAddress + 1] >> xShift) & 0b1;
            var c = (byte)((bb << 1) | ba);

            // If color is not 0, note that a Window pixel (normal or high-priority) exists here
            if (c != 0)
                screenUsageFlags[x, y] |= tileBgHasPriority ? screenUsageBackgroundHighPriority : screenUsageWindow;    // TODO correct?

            // Calculate color address in palette & draw pixel
            if (layerWindowForceEnable)
            {
                var paletteAddress = (tileBgPalette << 3) + ((c & 0b11) << 1);
                SetPixel(y, x, (ushort)((bgPaletteData[paletteAddress + 1] << 8) | bgPaletteData[paletteAddress + 0]));
            }
            else
                SetPixel(y, x, (ushort)((bgPaletteData[1] << 8) | bgPaletteData[0]));
        }

        protected override void RenderSprites(int y, int x)
        {
            var objHeight = objSize ? 16 : 8;

            // Iterate over sprite on line backwards
            for (var s = numSpritesOnLine - 1; s >= 0; s--)
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
                var objVramBank = (objAttributes >> 3) & 0b1;
                var objPalNumber = (objAttributes >> 0) & 0b111;

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
                    var ba = (vram[objVramBank, tileAddress + 0] >> xShift) & 0b1;
                    var bb = (vram[objVramBank, tileAddress + 1] >> xShift) & 0b1;

                    // Combine to color index, continue drawing if color is not 0
                    var c = (byte)((bb << 1) | ba);
                    if (c != 0)
                    {
                        // If sprite does not have priority i.e. if sprite should not be drawn, continue to next pixel
                        if (!HasSpritePriority(y, x, i)) continue;

                        screenUsageFlags[x, y] |= screenUsageSprite;
                        screenUsageSpriteSlots[x, y] = (byte)i;
                        screenUsageSpriteXCoords[x, y] = objX;

                        // Calculate color address in palette & draw pixel
                        if (layerSpritesForceEnable)
                        {
                            var paletteAddress = (objPalNumber << 3) + ((c & 0b11) << 1);
                            SetPixel(y, x, (ushort)((objPaletteData[paletteAddress + 1] << 8) | objPaletteData[paletteAddress + 0]));
                        }
                    }
                }
            }
        }

        protected override bool HasSpritePriority(int y, int x, int objSlot)
        {
            // If BG and window have priority, check further conditions
            if (bgEnable)
            {
                // Get new sprite OBJ-to-BG priority attribute
                var objIsBehindBg = ((oam[(objSlot * 4) + 3] >> 7) & 0b1) == 0b1;

                // If high-priority BG pixel has already been drawn, -or- new sprite is shown behind BG/Window -and- a BG/Window pixel has already been drawn, new sprite does not have priority
                if (IsScreenUsageFlagSet(y, x, screenUsageBackgroundHighPriority) ||
                    (objIsBehindBg && (IsScreenUsageFlagSet(y, x, screenUsageBackground) || IsScreenUsageFlagSet(y, x, screenUsageWindow)))) return false;
            }

            // New sprite has priority
            return true;
        }

        protected void SetPixel(int y, int x, ushort c)
        {
            WriteColorToFramebuffer(c, ((y * displayActiveWidth) + (x % displayActiveWidth)) * 4);
        }

        private void WriteColorToFramebuffer(ushort c, int address)
        {
            RGBCGBtoBGRA8888(c, ref outputFramebuffer, address);
        }

        //

        private void RunGDMA()
        {
            while (--dmaTransferByteLength >= 0)
            {
                if ((dmaSourceAddress >= 0x0000 && dmaSourceAddress <= 0x7FFF) || (dmaSourceAddress >= 0xA000 && dmaSourceAddress <= 0xDFFF))
                    WriteVram(dmaDestinationAddress, memoryReadDelegate(dmaSourceAddress));

                dmaDestinationAddress++;
                dmaSourceAddress++;

                dmaDestinationAddress &= 0x1FFF;
            }

            UpdateDMAStatus();
        }

        private void UpdateDMAStatus()
        {
            dmaTransferBlockLength = (byte)((dmaTransferByteLength >> 4) & 0xFF);

            dmaSourceHi = (byte)((dmaSourceAddress >> 8) & 0x1F);
            dmaSourceLo = (byte)((dmaSourceAddress >> 0) & 0xF0);
            dmaDestinationHi = (byte)((dmaDestinationAddress >> 8) & 0xFF);
            dmaDestinationLo = (byte)((dmaDestinationAddress >> 0) & 0xF0);
        }

        //

        public override byte ReadVram(ushort address)
        {
            if (modeNumber != 3)
                return vram[vramBank, address & 0x1FFF];
            else
                return 0xFF;
        }

        public override void WriteVram(ushort address, byte value)
        {
            if (modeNumber != 3)
                vram[vramBank, address & 0x1FFF] = value;
        }

        public override byte ReadPort(byte port)
        {
            switch (port)
            {
                case 0x4F:
                    // VBK
                    return (byte)(
                        0xFE |
                        (vramBank & 0b1));

                case 0x55:
                    // HDMA5
                    return dmaTransferBlockLength;

                case 0x68:
                    // BCPS
                    return (byte)(
                        0x40 |
                        (bgPaletteIndex & 0x3F) |
                        (bgPaletteAutoIncrement ? (1 << 7) : 0));

                case 0x69:
                    // BCPD
                    if (modeNumber != 3) return bgPaletteData[bgPaletteIndex];
                    else return 0xFF;

                case 0x6A:
                    // OCPS
                    return (byte)(
                        0x40 |
                        (objPaletteIndex & 0x3F) |
                        (objPaletteAutoIncrement ? (1 << 7) : 0));

                case 0x6B:
                    // OCPD
                    if (modeNumber != 3) return objPaletteData[objPaletteIndex];
                    else return 0xFF;

                default:
                    return base.ReadPort(port);
            }
        }

        public override void WritePort(byte port, byte value)
        {
            switch (port)
            {
                case 0x41:
                    // STAT
                    lycLyInterrupt = ((value >> 6) & 0b1) == 0b1;
                    m2OamInterrupt = ((value >> 5) & 0b1) == 0b1;
                    m1VBlankInterrupt = ((value >> 4) & 0b1) == 0b1;
                    m0HBlankInterrupt = ((value >> 3) & 0b1) == 0b1;

                    CheckAndRequestStatInterupt();
                    break;

                case 0x4F:
                    // VBK
                    vramBank = (byte)(value & 0b1);
                    break;

                case 0x51:
                    // HDMA1
                    dmaSourceHi = value;
                    break;

                case 0x52:
                    // HDMA2
                    dmaSourceLo = (byte)(value & 0xF0);
                    break;

                case 0x53:
                    // HDMA3
                    dmaDestinationHi = (byte)(value & 0x1F);
                    break;

                case 0x54:
                    // HDMA4
                    dmaDestinationLo = (byte)(value & 0xF0);
                    break;

                case 0x55:
                    // HDMA5
                    dmaTransferBlockLength = (byte)(value & 0x7F);
                    dmaTransferIsHDMA = ((value >> 7) & 0b1) == 0b1;

                    // Check for HDMA cancellation
                    if (!dmaTransferIsHDMA && hdmaIsActive)
                        hdmaIsActive = false;
                    else
                    {
                        // Calculate DMA addresses & length
                        dmaTransferByteLength = (dmaTransferBlockLength + 1) << 4;
                        dmaSourceAddress = (ushort)((dmaSourceHi << 8 | dmaSourceLo) & 0xFFF0);
                        dmaDestinationAddress = (ushort)((dmaDestinationHi << 8 | dmaDestinationLo) & 0x1FF0);

                        // Run General-Purpose DMA
                        if (!dmaTransferIsHDMA)
                        {
                            GDMAWaitCycles = 8 * (dmaTransferBlockLength + 1);
                            RunGDMA();
                        }
                        else
                            hdmaIsActive = true;
                    }
                    break;

                case 0x68:
                    // BCPS
                    bgPaletteIndex = (byte)(value & 0x3F);
                    bgPaletteAutoIncrement = ((value >> 7) & 0b1) == 0b1;
                    break;

                case 0x69:
                    // BCPD
                    if (modeNumber != 3) bgPaletteData[bgPaletteIndex] = value;
                    if (bgPaletteAutoIncrement)
                    {
                        bgPaletteIndex++;
                        bgPaletteIndex &= 0x3F;
                    }
                    break;

                case 0x6A:
                    // OCPS
                    objPaletteIndex = (byte)(value & 0x3F);
                    objPaletteAutoIncrement = ((value >> 7) & 0b1) == 0b1;
                    break;

                case 0x6B:
                    // OCPD
                    if (modeNumber != 3) objPaletteData[objPaletteIndex] = value;
                    if (objPaletteAutoIncrement)
                    {
                        objPaletteIndex++;
                        objPaletteIndex &= 0x3F;
                    }
                    break;

                default:
                    base.WritePort(port, value);
                    break;
            }
        }
    }
}
