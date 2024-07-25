using Codice.CM.Client.Differences;
using System;

namespace VirtualNes.Core
{
    public class PPU
    {
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
        private byte[] lpScreen;
        private byte[] lpColormode;
        private bool bVSMode;
        private int nVSColorMap;
        private byte VSSecurityData;
        private byte[] Bit2Rev = new byte[256];
        private int ScanlineNo;
        /// <summary> 作为lpScreen数组的索引 </summary>
        private int lpScanline;

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
        }

        public void Dispose()
        {
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
                lpScanline = (int)(Screen.SCREEN_WIDTH) * scanline;
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

        private enum Screen
        {
            SCREEN_WIDTH = 256 + 16,
            SCREEN_HEIGHT = 240
        }
    }
}
