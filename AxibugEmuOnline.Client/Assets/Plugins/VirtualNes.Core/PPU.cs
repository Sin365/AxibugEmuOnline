using System;
using System.Runtime.InteropServices;

namespace VirtualNes.Core
{
    public unsafe class PPU
    {
        public const int SCREEN_WIDTH = 272;
        public const int SCREEN_HEIGHT = 240;

        private GCHandle BGwriteGCH;
        private GCHandle BGmonoGCH;
        private GCHandle SPwriteGCH;

        private byte* BGwrite;
        private byte* BGmono;
        private byte* SPwrite;

        private static byte[][] CreateCOLORMAP()
        {
            byte[][] res = new byte[5][];
            res[0] = new byte[64]
            {   0x35, 0xFF, 0x16, 0x22, 0x1C, 0xFF, 0xFF, 0x15,
                0xFF, 0x00, 0x27, 0x05, 0x04, 0x27, 0x08, 0x30,
                0x21, 0xFF, 0xFF, 0x29, 0x3C, 0xFF, 0x36, 0x12,
                0xFF, 0x2B, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x01,
                0xFF, 0x31, 0xFF, 0x2A, 0x2C, 0x0C, 0xFF, 0xFF,
                0xFF, 0x07, 0x34, 0x06, 0x13, 0xFF, 0x26, 0x0F,
                0xFF, 0x19, 0x10, 0x0A, 0xFF, 0xFF, 0xFF, 0x17,
                0xFF, 0x11, 0x09, 0xFF, 0xFF, 0x25, 0x18, 0xFF
            };
            res[1] = new byte[64]
            {   0xFF, 0x27, 0x18, 0xFF, 0x3A, 0x25, 0xFF, 0x31,
                0x16, 0x13, 0x38, 0x34, 0x20, 0x23, 0x31, 0x1A,
                0xFF, 0x21, 0x06, 0xFF, 0x1B, 0x29, 0xFF, 0x22,
                0xFF, 0x24, 0xFF, 0xFF, 0xFF, 0x08, 0xFF, 0x03,
                0xFF, 0x36, 0x26, 0x33, 0x11, 0xFF, 0x10, 0x02,
                0x14, 0xFF, 0x00, 0x09, 0x12, 0x0F, 0xFF, 0x30,
                0xFF, 0xFF, 0x2A, 0x17, 0x0C, 0x01, 0x15, 0x19,
                0xFF, 0x2C, 0x07, 0x37, 0xFF, 0x05, 0xFF, 0xFF
            };
            res[2] = new byte[64]
            {   0xFF, 0xFF, 0xFF, 0x10, 0x1A, 0x30, 0x31, 0x09,
                0x01, 0x0F, 0x36, 0x08, 0x15, 0xFF, 0xFF, 0xF0,
                0x22, 0x1C, 0xFF, 0x12, 0x19, 0x18, 0x17, 0xFF,
                0x00, 0xFF, 0xFF, 0x02, 0x16, 0x06, 0xFF, 0x35,
                0x23, 0xFF, 0x8B, 0xF7, 0xFF, 0x27, 0x26, 0x20,
                0x29, 0xFF, 0x21, 0x24, 0x11, 0xFF, 0xEF, 0xFF,
                0x2C, 0xFF, 0xFF, 0xFF, 0x07, 0xF9, 0x28, 0xFF,
                0x0A, 0xFF, 0x32, 0x37, 0x13, 0xFF, 0xFF, 0x0C
            };
            res[3] = new byte[64]
            {   0x18, 0xFF, 0x1C, 0x89, 0x0F, 0xFF, 0x01, 0x17,	// 00-07
            	0x10, 0x0F, 0x2A, 0xFF, 0x36, 0x37, 0x1A, 0xFF,	// 08-0F
            	0x25, 0xFF, 0x12, 0xFF, 0x0F, 0xFF, 0xFF, 0x26,	// 10-17
            	0xFF, 0xFF, 0x22, 0xFF, 0xFF, 0x0F, 0x3A, 0x21,	// 18-1F
            	0x05, 0x0A, 0x07, 0xC2, 0x13, 0xFF, 0x00, 0x15,	// 20-27
            	0x0C, 0xFF, 0x11, 0xFF, 0xFF, 0x38, 0xFF, 0xFF,	// 28-2F
            	0xFF, 0xFF, 0x08, 0x16, 0xFF, 0xFF, 0x30, 0x3C,	// 30-37
            	0x0F, 0x27, 0xFF, 0x60, 0x29, 0xFF, 0x30, 0x09 	// 38-3F
            };
            res[4] = new byte[64]
            {
            // Super Xevious/Gradius
            	0x35, 0xFF, 0x16, 0x22, 0x1C, 0x09, 0xFF, 0x15,	// 00-07
            	0x20, 0x00, 0x27, 0x05, 0x04, 0x28, 0x08, 0x30,	// 08-0F
            	0x21, 0xFF, 0xFF, 0x29, 0x3C, 0xFF, 0x36, 0x12,	// 10-17
            	0xFF, 0x2B, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x01,	// 18-1F
            	0xFF, 0x31, 0xFF, 0x2A, 0x2C, 0x0C, 0x1B, 0xFF,	// 20-27
            	0xFF, 0x07, 0x34, 0x06, 0xFF, 0x25, 0x26, 0x0F,	// 28-2F
            	0xFF, 0x19, 0x10, 0x0A, 0xFF, 0xFF, 0xFF, 0x17,	// 30-37
            	0xFF, 0x11, 0x1A, 0xFF, 0x38, 0xFF, 0x18, 0x3A,	// 38-3F
            };

            return res;
        }
        private static byte[][] VSColorMap = CreateCOLORMAP();

        // PPU Control Register #1	PPU #0
        public const byte PPU_VBLANK_BIT = 0x80;
        public const byte PPU_SPHIT_BIT = 0x40;       // 堘偆丠
        public const byte PPU_SP16_BIT = 0x20;
        public const byte PPU_BGTBL_BIT = 0x10;
        public const byte PPU_SPTBL_BIT = 0x08;
        public const byte PPU_INC32_BIT = 0x04;
        public const byte PPU_NAMETBL_BIT = 0x03;

        // PPU Control Register #2	PPU #1                 
        public const byte PPU_BGCOLOR_BIT = 0xE0;
        public const byte PPU_SPDISP_BIT = 0x10;
        public const byte PPU_BGDISP_BIT = 0x08;
        public const byte PPU_SPCLIP_BIT = 0x04;
        public const byte PPU_BGCLIP_BIT = 0x02;
        public const byte PPU_COLORMODE_BIT = 0x01;

        // PPU Status Register		PPU #2                 
        public const byte PPU_VBLANK_FLAG = 0x80;
        public const byte PPU_SPHIT_FLAG = 0x40;
        public const byte PPU_SPMAX_FLAG = 0x20;
        public const byte PPU_WENABLE_FLAG = 0x10;

        // SPRITE Attribute                                
        public const byte SP_VMIRROR_BIT = 0x80;
        public const byte SP_HMIRROR_BIT = 0x40;
        public const byte SP_PRIORITY_BIT = 0x20;
        public const byte SP_COLOR_BIT = 0x03;

        private NES nes;

        private bool bExtLatch; // For MMC5
        private bool bChrLatch; // For MMC2/MMC4
        private bool bExtNameTable; // For Super Monkey no Dai Bouken
        private bool bExtMono;	// For Final Fantasy

        private ushort loopy_y;
        private ushort loopy_shift;

        private GCHandle lpScreenGCH;
        private uint* lpScreen;
        /// <summary> 作为lpScreen数组的索引 </summary>
        private uint* lpScanline;
        private int ScanlineNo;
        private byte[] lpColormode;

        private bool bVSMode;
        private int nVSColorMap;
        private byte VSSecurityData;
        private byte[] Bit2Rev = new byte[256];


        public PPU(NES nes)
        {
            this.nes = nes;
            lpScreen = null;
            lpColormode = null;

            bVSMode = false;
            nVSColorMap = -1;
            VSSecurityData = 0;

            for (int i = 0; i < 256; i++)
            {
                byte m = 0x80;
                byte c = 0;
                for (int j = 0; j < 8; j++)
                {
                    if ((i & (1 << j)) > 0) c |= m;
                    m >>= 1;
                }
                Bit2Rev[i] = c;
            }

            BGwriteGCH = GCHandle.Alloc(new byte[33 + 1], GCHandleType.Pinned);
            BGmonoGCH = GCHandle.Alloc(new byte[33 + 1], GCHandleType.Pinned);
            SPwriteGCH = GCHandle.Alloc(new byte[33 + 1], GCHandleType.Pinned);
            BGwrite = (byte*)BGwriteGCH.AddrOfPinnedObject();
            BGmono = (byte*)BGmonoGCH.AddrOfPinnedObject();
            SPwrite = (byte*)SPwriteGCH.AddrOfPinnedObject();
        }

        public void Dispose()
        {
            lpScreenGCH.Free();
            BGwriteGCH.Free();
            BGmonoGCH.Free();
            SPwriteGCH.Free();
        }

        internal byte Read(ushort addr)
        {
            byte data = 0x00;

            switch (addr)
            {
                // Write only Register
                case 0x2000: // PPU Control Register #1(W)
                case 0x2001: // PPU Control Register #2(W)
                case 0x2003: // SPR-RAM Address Register(W)
                case 0x2005: // PPU Scroll Register(W2)
                case 0x2006: // VRAM Address Register(W2)
                    data = MMU.PPU7_Temp;   // 懡暘
                    break;
                // Read/Write Register
                case 0x2002: // PPU Status Register(R)
                             //DEBUGOUT( "2002 RD L:%3d C:%8d\n", ScanlineNo, nes->cpu->GetTotalCycles() );
                    data = (byte)(MMU.PPUREG[2] | VSSecurityData);
                    MMU.PPU56Toggle = 0;
                    byte temp = unchecked((byte)~PPU_VBLANK_FLAG);
                    MMU.PPUREG[2] &= temp;
                    break;
                case 0x2004: // SPR_RAM I/O Register(RW)
                    data = MMU.SPRAM[MMU.PPUREG[3]++];
                    break;
                case 0x2007: // VRAM I/O Register(RW)
                    addr = (ushort)(MMU.loopy_v & 0x3FFF);
                    data = MMU.PPU7_Temp;
                    if ((MMU.PPUREG[0] & PPU_INC32_BIT) != 0) MMU.loopy_v += 32;
                    else MMU.loopy_v++;
                    if (addr >= 0x3000)
                    {
                        if (addr >= 0x3F00)
                        {
                            //					data &= 0x3F;
                            if ((addr & 0x0010) == 0)
                            {
                                return MMU.BGPAL[addr & 0x000F];
                            }
                            else
                            {
                                return MMU.SPPAL[addr & 0x000F];
                            }
                        }
                        addr &= 0xEFFF;
                    }
                    MMU.PPU7_Temp = MMU.PPU_MEM_BANK[addr >> 10][addr & 0x03FF];
                    break;
            }

            return data;
        }

        internal void SetRenderScanline(int scanline)
        {
            ScanlineNo = scanline;
            if (scanline < 240)
            {
                lpScanline = lpScreen + SCREEN_WIDTH * scanline;
            }
        }

        internal void Write(ushort addr, byte data)
        {
            if (bVSMode && VSSecurityData != 0)
            {
                if (addr == 0x2000)
                {
                    addr = 0x2001;
                }
                else if (addr == 0x2001)
                {
                    addr = 0x2000;
                }
            }

            switch (addr)
            {
                // Read only Register
                case 0x2002: // PPU Status register(R)
                    break;
                // Write Register
                case 0x2000: // PPU Control Register #1(W)
                             // NameTable select
                             // t:0000110000000000=d:00000011
                    MMU.loopy_t = (ushort)((MMU.loopy_t & 0xF3FF) | ((data & 0x03) << 10));

                    if ((data & 0x80) != 0 && (MMU.PPUREG[0] & 0x80) == 0 && (MMU.PPUREG[2] & 0x80) != 0)
                    {
                        nes.cpu.NMI(); // hmm...
                    }

                    MMU.PPUREG[0] = data;
                    break;
                case 0x2001: // PPU Control Register #2(W)
                    MMU.PPUREG[1] = data;
                    break;
                case 0x2003: // SPR-RAM Address Register(W)
                    MMU.PPUREG[3] = data;
                    break;
                case 0x2004: // SPR_RAM I/O Register(RW)
                    MMU.SPRAM[MMU.PPUREG[3]++] = data;
                    break;

                case 0x2005: // PPU Scroll Register(W2)
                             //DEBUGOUT( "SCR WRT L:%3d C:%8d\n", ScanlineNo, nes->cpu->GetTotalCycles() );
                    if (MMU.PPU56Toggle == 0)
                    {
                        // First write
                        // tile X t:0000000000011111=d:11111000
                        MMU.loopy_t = (ushort)((MMU.loopy_t & 0xFFE0) | ((data) >> 3));
                        // scroll offset X x=d:00000111
                        MMU.loopy_x = (ushort)(data & 0x07);
                    }
                    else
                    {
                        // Second write
                        // tile Y t:0000001111100000=d:11111000
                        MMU.loopy_t = (ushort)((MMU.loopy_t & 0xFC1F) | (((data) & 0xF8) << 2));
                        // scroll offset Y t:0111000000000000=d:00000111
                        MMU.loopy_t = (ushort)((MMU.loopy_t & 0x8FFF) | (((data) & 0x07) << 12));
                    }
                    MMU.PPU56Toggle = (byte)(MMU.PPU56Toggle == 0 ? 1 : 0);
                    break;
                case 0x2006: // VRAM Address Register(W2)
                    if (MMU.PPU56Toggle == 0)
                    {
                        // First write
                        // t:0011111100000000=d:00111111
                        // t:1100000000000000=0
                        MMU.loopy_t = (ushort)((MMU.loopy_t & 0x00FF) | (((data) & 0x3F) << 8));
                    }
                    else
                    {
                        // Second write
                        // t:0000000011111111=d:11111111
                        MMU.loopy_t = (ushort)((MMU.loopy_t & 0xFF00) | data);
                        // v=t
                        MMU.loopy_v = MMU.loopy_t;
                        nes.mapper.PPU_Latch(MMU.loopy_v);
                    }
                    MMU.PPU56Toggle = (byte)(MMU.PPU56Toggle == 0 ? 1 : 0);
                    break;
                case 0x2007: // VRAM I/O Register(RW)
                    ushort vaddr = (ushort)(MMU.loopy_v & 0x3FFF);
                    if ((MMU.PPUREG[0] & PPU_INC32_BIT) != 0) MMU.loopy_v += 32;
                    else MMU.loopy_v++;

                    if (vaddr >= 0x3000)
                    {
                        if (vaddr >= 0x3F00)
                        {
                            data &= 0x3F;
                            if (bVSMode && nVSColorMap != -1)
                            {
                                byte temp = VSColorMap[nVSColorMap][data];
                                if (temp != 0xFF)
                                {
                                    data = (byte)(temp & 0x3F);
                                }
                            }

                            if ((vaddr & 0x000F) == 0)
                            {
                                MMU.BGPAL[0] = MMU.SPPAL[0] = data;
                            }
                            else if ((vaddr & 0x0010) == 0)
                            {
                                MMU.BGPAL[vaddr & 0x000F] = data;
                            }
                            else
                            {
                                MMU.SPPAL[vaddr & 0x000F] = data;
                            }
                            MMU.BGPAL[0x04] = MMU.BGPAL[0x08] = MMU.BGPAL[0x0C] = MMU.BGPAL[0x00];
                            MMU.SPPAL[0x00] = MMU.SPPAL[0x04] = MMU.SPPAL[0x08] = MMU.SPPAL[0x0C] = MMU.BGPAL[0x00];
                            return;
                        }
                        vaddr &= 0xEFFF;
                    }
                    if (MMU.PPU_MEM_TYPE[vaddr >> 10] != MMU.BANKTYPE_VROM)
                    {
                        MMU.PPU_MEM_BANK[vaddr >> 10][vaddr & 0x03FF] = data;
                    }
                    break;
            }
        }

        internal void DMA(byte data)
        {
            ushort addr = (ushort)(data << 8);

            for (ushort i = 0; i < 256; i++)
            {
                MMU.SPRAM[i] = nes.Read((ushort)(addr + i));
            }
        }

        internal void Reset()
        {
            bExtLatch = false;
            bChrLatch = false;
            bExtNameTable = false;
            bExtMono = false;

            MMU.PPUREG[0] = MMU.PPUREG[1] = 0;

            MMU.PPU56Toggle = 0;

            MMU.PPU7_Temp = 0xFF;   // VS Excitebike偱偍偐偟偔側傞($2006傪撉傒偵峴偔僶僌偑偁傞)
                                    //	PPU7_Temp = 0;

            MMU.loopy_v = MMU.loopy_t = 0;
            MMU.loopy_x = loopy_y = 0;
            loopy_shift = 0;

            if (lpScreen != null)
                MemoryUtility.memset(lpScreen, 0, 0, SCREEN_WIDTH * SCREEN_HEIGHT);
            if (lpColormode != null)
                MemoryUtility.memset(lpColormode, 0, SCREEN_HEIGHT);
        }

        internal void FrameStart()
        {
            if ((MMU.PPUREG[1] & (PPU_SPDISP_BIT | PPU_BGDISP_BIT)) != 0)
            {
                MMU.loopy_v = MMU.loopy_t;
                loopy_shift = MMU.loopy_x;
                loopy_y = (ushort)((MMU.loopy_v & 0x7000) >> 12);
            }

            if (lpScreen != null)
            {
                MemoryUtility.memset(lpScreen, 0, 0x3f, SCREEN_WIDTH);
            }
            if (lpColormode != null)
            {
                lpColormode[0] = 0;
            }
        }

        internal void ScanlineNext()
        {
            if ((MMU.PPUREG[1] & (PPU_BGDISP_BIT | PPU_SPDISP_BIT)) != 0)
            {
                if ((MMU.loopy_v & 0x7000) == 0x7000)
                {
                    MMU.loopy_v &= 0x8FFF;
                    if ((MMU.loopy_v & 0x03E0) == 0x03A0)
                    {
                        MMU.loopy_v ^= 0x0800;
                        MMU.loopy_v &= 0xFC1F;
                    }
                    else
                    {
                        if ((MMU.loopy_v & 0x03E0) == 0x03E0)
                        {
                            MMU.loopy_v &= 0xFC1F;
                        }
                        else
                        {
                            MMU.loopy_v += 0x0020;
                        }
                    }
                }
                else
                {
                    MMU.loopy_v += 0x1000;
                }
                loopy_y = (ushort)((MMU.loopy_v & 0x7000) >> 12);
            }
        }

        internal void ScanlineStart()
        {
            if ((MMU.PPUREG[1] & (PPU_BGDISP_BIT | PPU_SPDISP_BIT)) != 0)
            {
                MMU.loopy_v = (ushort)((MMU.loopy_v & 0xFBE0) | (MMU.loopy_t & 0x041F));
                loopy_shift = MMU.loopy_x;
                loopy_y = (ushort)((MMU.loopy_v & 0x7000) >> 12);
                nes.mapper.PPU_Latch((ushort)(0x2000 + (MMU.loopy_v & 0x0FFF)));
            }
        }

        internal void Scanline(int scanline, bool bMax, bool bLeftClip)
        {
            byte chr_h = 0, chr_l = 0, attr = 0;

            MemoryUtility.memset(BGwrite, 0, 34);
            MemoryUtility.memset(BGmono, 0, 34);

            // Linecolor mode
            lpColormode[scanline] = (byte)(((MMU.PPUREG[1] & PPU_BGCOLOR_BIT) >> 5) | ((MMU.PPUREG[1] & PPU_COLORMODE_BIT) << 7));

            // Render BG
            if ((MMU.PPUREG[1] & PPU_BGDISP_BIT) == 0)
            {
                MemoryUtility.memset(lpScanline, MMU.BGPAL[0], SCREEN_WIDTH);
                if (nes.GetRenderMethod() == EnumRenderMethod.TILE_RENDER)
                {
                    nes.EmulationCPU(NES.FETCH_CYCLES * 4 * 32);
                }
            }
            else
            {
                if (nes.GetRenderMethod() != EnumRenderMethod.TILE_RENDER)
                {
                    if (!bExtLatch)
                    {
                        // Without Extension Latch
                        uint* pScn = lpScanline + (8 - loopy_shift);
                        byte* pBGw = BGwrite;
                        int tileofs = (MMU.PPUREG[0] & PPU_BGTBL_BIT) << 8;
                        int ntbladr = 0x2000 + (MMU.loopy_v & 0x0FFF);
                        int attradr = 0x23C0 + (MMU.loopy_v & 0x0C00) + ((MMU.loopy_v & 0x0380) >> 4);
                        int ntbl_x = ntbladr & 0x001F;
                        int attrsft = (ntbladr & 0x0040) >> 4;
                        var pNTBL = MMU.PPU_MEM_BANK[ntbladr >> 10];

                        int tileadr;
                        int cache_tile = unchecked((int)(0xFFFF0000));
                        byte cache_attr = 0xFF;

                        chr_h = chr_l = attr = 0;

                        attradr &= 0x3FF;

                        for (int i = 0; i < 33; i++)
                        {
                            tileadr = tileofs + pNTBL[ntbladr & 0x03FF] * 0x10 + loopy_y;
                            attr = (byte)(((pNTBL[attradr + (ntbl_x >> 2)] >> ((ntbl_x & 2) + attrsft)) & 3) << 2);

                            if (cache_tile == tileadr && cache_attr == attr)
                            {
                                *(UInt128*)(pScn + 0) = *(UInt128*)(pScn - 8);
                                *(UInt128*)(pScn + 4) = *(UInt128*)(pScn - 4);
                                *(pBGw + 0) = *(pBGw - 1);
                            }
                            else
                            {
                                cache_tile = tileadr;
                                cache_attr = attr;
                                chr_l = MMU.PPU_MEM_BANK[tileadr >> 10][tileadr & 0x03FF];
                                chr_h = MMU.PPU_MEM_BANK[tileadr >> 10][(tileadr & 0x03FF) + 8];
                                *pBGw = (byte)(chr_h | chr_l);

                                fixed (byte* pBGPAL = &MMU.BGPAL[attr])
                                {
                                    int c1 = ((chr_l >> 1) & 0x55) | (chr_h & 0xAA);
                                    int c2 = (chr_l & 0x55) | ((chr_h << 1) & 0xAA);
                                    pScn[0] = pBGPAL[(c1 >> 6)];
                                    pScn[4] = pBGPAL[(c1 >> 2) & 3];
                                    pScn[1] = pBGPAL[(c2 >> 6)];
                                    pScn[5] = pBGPAL[(c2 >> 2) & 3];
                                    pScn[2] = pBGPAL[(c1 >> 4) & 3];
                                    pScn[6] = pBGPAL[c1 & 3];
                                    pScn[3] = pBGPAL[(c2 >> 4) & 3];
                                    pScn[7] = pBGPAL[c2 & 3];
                                }
                            }
                            pScn += 8;
                            pBGw++;

                            // Character latch(For MMC2/MMC4)
                            if (bChrLatch)
                            {
                                nes.mapper.PPU_ChrLatch((ushort)(tileadr));
                            }

                            if (++ntbl_x == 32)
                            {
                                ntbl_x = 0;
                                ntbladr ^= 0x41F;
                                attradr = 0x03C0 + ((ntbladr & 0x0380) >> 4);
                                pNTBL = MMU.PPU_MEM_BANK[ntbladr >> 10];
                            }
                            else
                            {
                                ntbladr++;
                            }
                        }
                    }
                    else
                    {
                        // With Extension Latch(For MMC5)
                        uint* pScn = lpScanline + (8 - loopy_shift);
                        byte* pBGw = BGwrite;

                        int ntbladr = 0x2000 + (MMU.loopy_v & 0x0FFF);
                        int ntbl_x = ntbladr & 0x1F;

                        int cache_tile = unchecked((int)(0xFFFF0000));
                        byte cache_attr = 0xFF;

                        byte exattr = 0;
                        chr_h = chr_l = attr = 0;

                        for (int i = 0; i < 33; i++)
                        {
                            nes.mapper.PPU_ExtLatchX(i);
                            nes.mapper.PPU_ExtLatch((ushort)ntbladr, ref chr_l, ref chr_h, ref exattr);
                            attr = (byte)(exattr & 0x0C);

                            if (cache_tile != ((chr_h << 8) + chr_l) || cache_attr != attr)
                            {
                                cache_tile = ((chr_h << 8) + chr_l);
                                cache_attr = attr;
                                *pBGw = (byte)(chr_h | chr_l);

                                fixed (byte* pBGPAL = &MMU.BGPAL[attr])
                                {
                                    int c1 = ((chr_l >> 1) & 0x55) | (chr_h & 0xAA);
                                    int c2 = (chr_l & 0x55) | ((chr_h << 1) & 0xAA);
                                    pScn[0] = pBGPAL[(c1 >> 6)];
                                    pScn[4] = pBGPAL[(c1 >> 2) & 3];
                                    pScn[1] = pBGPAL[(c2 >> 6)];
                                    pScn[5] = pBGPAL[(c2 >> 2) & 3];
                                    pScn[2] = pBGPAL[(c1 >> 4) & 3];
                                    pScn[6] = pBGPAL[c1 & 3];
                                    pScn[3] = pBGPAL[(c2 >> 4) & 3];
                                    pScn[7] = pBGPAL[c2 & 3];
                                }
                            }
                            else
                            {
                                *(UInt128*)(pScn + 0) = *(UInt128*)(pScn - 8);
                                *(UInt128*)(pScn + 4) = *(UInt128*)(pScn - 4);
                                *(pBGw + 0) = *(pBGw - 1);
                            }
                            pScn += 8;
                            pBGw++;

                            if (++ntbl_x == 32)
                            {
                                ntbl_x = 0;
                                ntbladr ^= 0x41F;
                            }
                            else
                            {
                                ntbladr++;
                            }
                        }
                    }
                }
                else
                {
                    if (!bExtLatch)
                    {
                        // Without Extension Latch
                        if (!bExtNameTable)
                        {
                            uint* pScn = lpScanline + (8 - loopy_shift);
                            byte* pBGw = BGwrite;

                            int ntbladr = 0x2000 + (MMU.loopy_v & 0x0FFF);
                            int attradr = 0x03C0 + ((MMU.loopy_v & 0x0380) >> 4);
                            int ntbl_x = ntbladr & 0x001F;
                            int attrsft = (ntbladr & 0x0040) >> 4;
                            var pNTBL = MMU.PPU_MEM_BANK[ntbladr >> 10];

                            int tileadr = 0;
                            int cache_tile = unchecked((int)(0xFFFF0000));
                            byte cache_attr = 0xFF;

                            chr_h = chr_l = attr = 0;

                            for (int i = 0; i < 33; i++)
                            {
                                tileadr = ((MMU.PPUREG[0] & PPU_BGTBL_BIT) << 8) + pNTBL[ntbladr & 0x03FF] * 0x10 + loopy_y;

                                if (i != 0)
                                {
                                    nes.EmulationCPU(NES.FETCH_CYCLES * 4);
                                }

                                attr = (byte)(((pNTBL[attradr + (ntbl_x >> 2)] >> ((ntbl_x & 2) + attrsft)) & 3) << 2);

                                if (cache_tile != tileadr || cache_attr != attr)
                                {
                                    cache_tile = tileadr;
                                    cache_attr = attr;

                                    chr_l = MMU.PPU_MEM_BANK[tileadr >> 10][tileadr & 0x03FF];
                                    chr_h = MMU.PPU_MEM_BANK[tileadr >> 10][(tileadr & 0x03FF) + 8];
                                    *pBGw = (byte)(chr_l | chr_h);

                                    fixed (byte* pBGPAL = &MMU.BGPAL[attr])
                                    {
                                        int c1 = ((chr_l >> 1) & 0x55) | (chr_h & 0xAA);
                                        int c2 = (chr_l & 0x55) | ((chr_h << 1) & 0xAA);
                                        pScn[0] = pBGPAL[(c1 >> 6)];
                                        pScn[4] = pBGPAL[(c1 >> 2) & 3];
                                        pScn[1] = pBGPAL[(c2 >> 6)];
                                        pScn[5] = pBGPAL[(c2 >> 2) & 3];
                                        pScn[2] = pBGPAL[(c1 >> 4) & 3];
                                        pScn[6] = pBGPAL[c1 & 3];
                                        pScn[3] = pBGPAL[(c2 >> 4) & 3];
                                        pScn[7] = pBGPAL[c2 & 3];
                                    }
                                }
                                else
                                {
                                    *(UInt128*)(pScn + 0) = *(UInt128*)(pScn - 8);
                                    *(UInt128*)(pScn + 4) = *(UInt128*)(pScn - 4);
                                    *(pBGw + 0) = *(pBGw - 1);
                                }
                                pScn += 8;
                                pBGw++;

                                // Character latch(For MMC2/MMC4)
                                if (bChrLatch)
                                {
                                    nes.mapper.PPU_ChrLatch((ushort)(tileadr));
                                }

                                if (++ntbl_x == 32)
                                {
                                    ntbl_x = 0;
                                    ntbladr ^= 0x41F;
                                    attradr = 0x03C0 + ((ntbladr & 0x0380) >> 4);
                                    pNTBL = MMU.PPU_MEM_BANK[ntbladr >> 10];
                                }
                                else
                                {
                                    ntbladr++;
                                }
                            }
                        }
                        else
                        {
                            uint* pScn = lpScanline + (8 - loopy_shift);
                            byte* pBGw = BGwrite;

                            int ntbladr;
                            int tileadr;
                            int cache_tile = unchecked((int)(0xFFFF0000));
                            byte cache_attr = 0xFF;

                            chr_h = chr_l = attr = 0;

                            ushort loopy_v_tmp = MMU.loopy_v;

                            for (int i = 0; i < 33; i++)
                            {
                                if (i != 0)
                                {
                                    nes.EmulationCPU(NES.FETCH_CYCLES * 4);
                                }

                                ntbladr = 0x2000 + (MMU.loopy_v & 0x0FFF);
                                tileadr = ((MMU.PPUREG[0] & PPU_BGTBL_BIT) << 8) + MMU.PPU_MEM_BANK[ntbladr >> 10][ntbladr & 0x03FF] * 0x10 + ((MMU.loopy_v & 0x7000) >> 12);
                                attr = (byte)(((MMU.PPU_MEM_BANK[ntbladr >> 10][0x03C0 + ((ntbladr & 0x0380) >> 4) + ((ntbladr & 0x001C) >> 2)] >> (((ntbladr & 0x40) >> 4) + (ntbladr & 0x02))) & 3) << 2);

                                if (cache_tile != tileadr || cache_attr != attr)
                                {
                                    cache_tile = tileadr;
                                    cache_attr = attr;

                                    chr_l = MMU.PPU_MEM_BANK[tileadr >> 10][tileadr & 0x03FF];
                                    chr_h = MMU.PPU_MEM_BANK[tileadr >> 10][(tileadr & 0x03FF) + 8];
                                    *pBGw = (byte)(chr_l | chr_h);

                                    fixed (byte* pBGPAL = &MMU.BGPAL[attr])
                                    {
                                        int c1 = ((chr_l >> 1) & 0x55) | (chr_h & 0xAA);
                                        int c2 = (chr_l & 0x55) | ((chr_h << 1) & 0xAA);
                                        pScn[0] = pBGPAL[(c1 >> 6)];
                                        pScn[4] = pBGPAL[(c1 >> 2) & 3];
                                        pScn[1] = pBGPAL[(c2 >> 6)];
                                        pScn[5] = pBGPAL[(c2 >> 2) & 3];
                                        pScn[2] = pBGPAL[(c1 >> 4) & 3];
                                        pScn[6] = pBGPAL[c1 & 3];
                                        pScn[3] = pBGPAL[(c2 >> 4) & 3];
                                        pScn[7] = pBGPAL[c2 & 3];
                                    }
                                }
                                else
                                {
                                    *(UInt128*)(pScn + 0) = *(UInt128*)(pScn - 8);
                                    *(UInt128*)(pScn + 4) = *(UInt128*)(pScn - 4);
                                    *(pBGw + 0) = *(pBGw - 1);
                                }
                                pScn += 8;
                                pBGw++;

                                // Character latch(For MMC2/MMC4)
                                if (bChrLatch)
                                {
                                    nes.mapper.PPU_ChrLatch((ushort)tileadr);
                                }

                                if ((MMU.loopy_v & 0x1F) == 0x1F)
                                {
                                    MMU.loopy_v ^= 0x041F;
                                }
                                else
                                {
                                    MMU.loopy_v++;
                                }
                            }
                            MMU.loopy_v = loopy_v_tmp;
                        }
                    }
                    else
                    {
                        // With Extension Latch(For MMC5)
                        uint* pScn = lpScanline + (8 - loopy_shift);
                        byte* pBGw = BGwrite;

                        int ntbladr = 0x2000 + (MMU.loopy_v & 0x0FFF);
                        int ntbl_x = ntbladr & 0x1F;

                        int cache_tile = unchecked((int)0xFFFF0000);
                        byte cache_attr = 0xFF;

                        byte exattr = 0;
                        chr_h = chr_l = attr = 0;

                        for (int i = 0; i < 33; i++)
                        {
                            if (i != 0)
                            {
                                nes.EmulationCPU(NES.FETCH_CYCLES * 4);
                            }
                            nes.mapper.PPU_ExtLatchX(i);
                            nes.mapper.PPU_ExtLatch((ushort)ntbladr, ref chr_l, ref chr_h, ref exattr);
                            attr = (byte)(exattr & 0x0C);

                            if (cache_tile != ((chr_h << 8) + chr_l) || cache_attr != attr)
                            {
                                cache_tile = ((chr_h << 8) + chr_l);
                                cache_attr = attr;
                                *pBGw = (byte)(chr_l | chr_h);

                                fixed (byte* pBGPAL = &MMU.BGPAL[attr])
                                {
                                    int c1 = ((chr_l >> 1) & 0x55) | (chr_h & 0xAA);
                                    int c2 = (chr_l & 0x55) | ((chr_h << 1) & 0xAA);
                                    pScn[0] = pBGPAL[(c1 >> 6)];
                                    pScn[4] = pBGPAL[(c1 >> 2) & 3];
                                    pScn[1] = pBGPAL[(c2 >> 6)];
                                    pScn[5] = pBGPAL[(c2 >> 2) & 3];
                                    pScn[2] = pBGPAL[(c1 >> 4) & 3];
                                    pScn[6] = pBGPAL[c1 & 3];
                                    pScn[3] = pBGPAL[(c2 >> 4) & 3];
                                    pScn[7] = pBGPAL[c2 & 3];
                                }
                            }
                            else
                            {
                                *(UInt128*)(pScn + 0) = *(UInt128*)(pScn - 8);
                                *(UInt128*)(pScn + 4) = *(UInt128*)(pScn - 4);
                                *(pBGw + 0) = *(pBGw - 1);
                            }
                            pScn += 8;
                            pBGw++;

                            if (++ntbl_x == 32)
                            {
                                ntbl_x = 0;
                                ntbladr ^= 0x41F;
                            }
                            else
                            {
                                ntbladr++;
                            }
                        }
                    }
                }
                if ((MMU.PPUREG[1] & PPU_BGCLIP_BIT) == 0 && bLeftClip)
                {
                    uint* pScn = lpScanline + 8;
                    for (int i = 0; i < 8; i++)
                    {
                        pScn[i] = MMU.BGPAL[0];
                    }
                }
            }

            // Render sprites
            var temp = ~PPU_SPMAX_FLAG;
            MMU.PPUREG[2] &= (byte)(MMU.PPUREG[2] & temp);

            // 昞帵婜娫奜偱偁傟偽僉儍儞僙儖
            if (scanline > 239)
                return;

            if ((MMU.PPUREG[1] & PPU_SPDISP_BIT) == 0)
            {
                return;
            }

            int spmax = 0;
            int spraddr = 0, sp_y = 0, sp_h = 0;
            chr_h = chr_l = 0;

            fixed (byte* pBit2Rev = &Bit2Rev[0])
            {
                byte* pBGw = BGwrite;
                byte* pSPw = SPwrite;
                MemoryUtility.memset(pSPw, 0, 34);

                spmax = 0;
                Sprite sp = new Sprite(MMU.SPRAM, 0);
                sp_h = (MMU.PPUREG[0] & PPU_SP16_BIT) != 0 ? 15 : 7;

                // Left clip
                if (bLeftClip && ((MMU.PPUREG[1] & PPU_SPCLIP_BIT) == 0))
                {
                    SPwrite[0] = 0xFF;
                }

                for (int i = 0; i < 64; i++, sp.AddOffset(1))
                {
                    sp_y = scanline - (sp.y + 1);
                    // 僗僉儍儞儔僀儞撪偵SPRITE偑懚嵼偡傞偐傪僠僃僢僋
                    if (sp_y != (sp_y & sp_h))
                        continue;

                    if ((MMU.PPUREG[0] & PPU_SP16_BIT) == 0)
                    {
                        // 8x8 Sprite
                        spraddr = ((MMU.PPUREG[0] & PPU_SPTBL_BIT) << 9) + (sp.tile << 4);
                        if ((sp.attr & SP_VMIRROR_BIT) == 0)
                            spraddr += sp_y;
                        else
                            spraddr += 7 - sp_y;
                    }
                    else
                    {
                        // 8x16 Sprite
                        spraddr = ((sp.tile & 1) << 12) + ((sp.tile & 0xFE) << 4);
                        if ((sp.attr & SP_VMIRROR_BIT) == 0)
                            spraddr += ((sp_y & 8) << 1) + (sp_y & 7);
                        else
                            spraddr += ((~sp_y & 8) << 1) + (7 - (sp_y & 7));
                    }
                    // Character pattern
                    chr_l = MMU.PPU_MEM_BANK[spraddr >> 10][spraddr & 0x3FF];
                    chr_h = MMU.PPU_MEM_BANK[spraddr >> 10][(spraddr & 0x3FF) + 8];

                    // Character latch(For MMC2/MMC4)
                    if (bChrLatch)
                    {
                        nes.mapper.PPU_ChrLatch((ushort)spraddr);
                    }

                    // pattern mask
                    if ((sp.attr & SP_HMIRROR_BIT) != 0)
                    {
                        chr_l = pBit2Rev[chr_l];
                        chr_h = pBit2Rev[chr_h];
                    }
                    byte SPpat = (byte)(chr_l | chr_h);

                    // Sprite hitcheck
                    if (i == 0 && (MMU.PPUREG[2] & PPU_SPHIT_FLAG) == 0)
                    {
                        int BGpos = ((sp.x & 0xF8) + ((loopy_shift + (sp.x & 7)) & 8)) >> 3;
                        int BGsft = 8 - ((loopy_shift + sp.x) & 7);
                        byte BGmsk = (byte)(((pBGw[BGpos + 0] << 8) | pBGw[BGpos + 1]) >> BGsft);

                        if ((SPpat & BGmsk) != 0)
                        {
                            MMU.PPUREG[2] |= PPU_SPHIT_FLAG;
                        }
                    }

                    // Sprite mask
                    int SPpos = sp.x / 8;
                    int SPsft = 8 - (sp.x & 7);
                    byte SPmsk = (byte)(((pSPw[SPpos + 0] << 8) | pSPw[SPpos + 1]) >> SPsft);
                    ushort SPwrt = (ushort)(SPpat << SPsft);
                    pSPw[SPpos + 0] = (byte)((pSPw[SPpos + 0]) | (SPwrt >> 8));
                    pSPw[SPpos + 1] = (byte)((pSPw[SPpos + 1]) | (SPwrt & 0xFF));
                    SPpat = (byte)(SPpat & ~SPmsk);

                    if ((sp.attr & SP_PRIORITY_BIT) != 0)
                    {
                        // BG > SP priority
                        int BGpos = ((sp.x & 0xF8) + ((loopy_shift + (sp.x & 7)) & 8)) >> 3;
                        int BGsft = 8 - ((loopy_shift + sp.x) & 7);
                        byte BGmsk = (byte)(((pBGw[BGpos + 0] << 8) | pBGw[BGpos + 1]) >> BGsft);

                        SPpat = (byte)(SPpat & ~BGmsk);
                    }

                    // Attribute
                    fixed (byte* pSPPAL = &MMU.SPPAL[(sp.attr & SP_COLOR_BIT) << 2])
                    {
                        // Ptr
                        uint* pScn = lpScanline + sp.x + 8;

                        if (!bExtMono)
                        {
                            int c1 = ((chr_l >> 1) & 0x55) | (chr_h & 0xAA);
                            int c2 = (chr_l & 0x55) | ((chr_h << 1) & 0xAA);
                            if ((SPpat & 0x80) != 0) pScn[0] = pSPPAL[(c1 >> 6)];
                            if ((SPpat & 0x08) != 0) pScn[4] = pSPPAL[(c1 >> 2) & 3];
                            if ((SPpat & 0x40) != 0) pScn[1] = pSPPAL[(c2 >> 6)];
                            if ((SPpat & 0x04) != 0) pScn[5] = pSPPAL[(c2 >> 2) & 3];
                            if ((SPpat & 0x20) != 0) pScn[2] = pSPPAL[(c1 >> 4) & 3];
                            if ((SPpat & 0x02) != 0) pScn[6] = pSPPAL[c1 & 3];
                            if ((SPpat & 0x10) != 0) pScn[3] = pSPPAL[(c2 >> 4) & 3];
                            if ((SPpat & 0x01) != 0) pScn[7] = pSPPAL[c2 & 3];
                        }
                        else
                        {
                            // Monocrome effect (for Final Fantasy)
                            byte mono = BGmono[((sp.x & 0xF8) + ((loopy_shift + (sp.x & 7)) & 8)) >> 3];

                            int c1 = ((chr_l >> 1) & 0x55) | (chr_h & 0xAA);
                            int c2 = (chr_l & 0x55) | ((chr_h << 1) & 0xAA);
                            if ((SPpat & 0x80) != 0) pScn[0] = (byte)(pSPPAL[c1 >> 6] | mono);
                            if ((SPpat & 0x08) != 0) pScn[4] = (byte)(pSPPAL[(c1 >> 2) & 3] | mono);
                            if ((SPpat & 0x40) != 0) pScn[1] = (byte)(pSPPAL[c2 >> 6] | mono);
                            if ((SPpat & 0x04) != 0) pScn[5] = (byte)(pSPPAL[(c2 >> 2) & 3] | mono);
                            if ((SPpat & 0x20) != 0) pScn[2] = (byte)(pSPPAL[(c1 >> 4) & 3] | mono);
                            if ((SPpat & 0x02) != 0) pScn[6] = (byte)(pSPPAL[c1 & 3] | mono);
                            if ((SPpat & 0x10) != 0) pScn[3] = (byte)(pSPPAL[(c2 >> 4) & 3] | mono);
                            if ((SPpat & 0x01) != 0) pScn[7] = (byte)(pSPPAL[c2 & 3] | mono);
                        }
                    }

                    if (++spmax > 8 - 1)
                    {
                        if (!bMax)
                            break;
                    }
                }
                if (spmax > 8 - 1)
                {
                    MMU.PPUREG[2] |= PPU_SPMAX_FLAG;
                }
            }
        }

        internal bool IsSprite0(int scanline)
        {
            // 僗僾儔僀僩orBG旕昞帵偼僉儍儞僙儖(僸僢僩偟側偄)
            if ((MMU.PPUREG[1] & (PPU_SPDISP_BIT | PPU_BGDISP_BIT)) != (PPU_SPDISP_BIT | PPU_BGDISP_BIT))
                return false;

            // 婛偵僸僢僩偟偰偄偨傜僉儍儞僙儖
            if ((MMU.PPUREG[2] & PPU_SPHIT_FLAG) != 0)
                return false;

            if ((MMU.PPUREG[0] & PPU_SP16_BIT) == 0)
            {
                // 8x8
                if ((scanline < MMU.SPRAM[0] + 1) || (scanline > (MMU.SPRAM[0] + 7 + 1)))
                    return false;
            }
            else
            {
                // 8x16
                if ((scanline < MMU.SPRAM[0] + 1) || (scanline > (MMU.SPRAM[0] + 15 + 1)))
                    return false;
            }

            return true;
        }

        internal void DummyScanline(int scanline)
        {
            int i;
            int spmax;
            int sp_h;

            MMU.PPUREG[2] = (byte)(MMU.PPUREG[2] & ~PPU_SPMAX_FLAG);

            // 僗僾儔僀僩旕昞帵偼僉儍儞僙儖
            if ((MMU.PPUREG[1] & PPU_SPDISP_BIT) == 0)
                return;

            // 昞帵婜娫奜偱偁傟偽僉儍儞僙儖
            if (scanline < 0 || scanline > 239)
                return;

            Sprite sp = new Sprite(MMU.SPRAM, 0);
            sp_h = (MMU.PPUREG[0] & PPU_SP16_BIT) != 0 ? 15 : 7;

            spmax = 0;
            // Sprite Max check
            for (i = 0; i < 64; i++, sp.AddOffset(1))
            {
                // 僗僉儍儞儔僀儞撪偵SPRITE偑懚嵼偡傞偐傪僠僃僢僋
                if ((scanline < sp.y + 1) || (scanline > (sp.y + sp_h + 1)))
                {
                    continue;
                }

                if (++spmax > 8 - 1)
                {
                    MMU.PPUREG[2] |= PPU_SPMAX_FLAG;
                    break;
                }
            }
        }

        internal void VBlankEnd()
        {
            MMU.PPUREG[2] = (byte)(MMU.PPUREG[2] & ~PPU_VBLANK_FLAG);
            // VBlank扙弌帪偵僋儕傾偝傟傞
            // 僄僉僒僀僩僶僀僋偱廳梫
            MMU.PPUREG[2] = (byte)(MMU.PPUREG[2] & ~PPU_SPHIT_FLAG);
        }

        internal void VBlankStart()
        {
            MMU.PPUREG[2] |= PPU_VBLANK_FLAG;
        }

        public uint* GetScreenPtr()
        {
            return lpScreen;
        }

        public byte[] GetLineColorMode()
        {
            return lpColormode;
        }

        internal void InitBuffer()
        {
            var screenBuffer = new uint[SCREEN_WIDTH * SCREEN_HEIGHT];
            var colormode = new byte[SCREEN_HEIGHT];

            lpScreenGCH = GCHandle.Alloc(screenBuffer, GCHandleType.Pinned);
            lpScreen = (uint*)lpScreenGCH.AddrOfPinnedObject();
            lpColormode = colormode;
        }


        internal bool IsDispON()
        {
            return (MMU.PPUREG[1] & (PPU_BGDISP_BIT | PPU_SPDISP_BIT)) != 0;
        }

        internal void SetExtLatchMode(bool bMode)
        {
            bExtLatch = bMode;
        }

        internal ushort GetPPUADDR()
        {
            return MMU.loopy_v;
        }

        internal ushort GetTILEY()
        {
            return loopy_y;
        }

        internal void SetChrLatchMode(bool bMode)
        {
            bChrLatch = bMode;
        }

        internal void SetExtNameTableMode(bool bMode)
        {
            bExtNameTable = bMode;
        }

        internal void SetExtMonoMode(bool bMode)
        {
            bExtMono = bMode;
        }

        internal int GetScanlineNo()
        {
            return ScanlineNo;
        }

        public struct Sprite
        {
            public byte y
            {
                get => raw[offset + 0];
                set => raw[offset + 0] = value;
            }

            public byte tile
            {
                get => raw[offset + 1];
                set => raw[offset + 1] = value;
            }
            public byte attr
            {
                get => raw[offset + 2];
                set => raw[offset + 2] = value;
            }
            public byte x
            {
                get => raw[offset + 3];
                set => raw[offset + 3] = value;
            }

            private byte[] raw;
            private int offset;

            public Sprite(byte[] raw, int offset)
            {
                this.raw = raw;
                this.offset = offset * 4;
            }

            public void AddOffset(int offset)
            {
                this.offset += offset * 4;
            }
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct UInt128
    {
        [FieldOffset(0)]
        public UInt32 a;
        [FieldOffset(4)]
        public UInt32 b;
        [FieldOffset(8)]
        public UInt32 c;
        [FieldOffset(12)]
        public UInt32 d;
    }
}
