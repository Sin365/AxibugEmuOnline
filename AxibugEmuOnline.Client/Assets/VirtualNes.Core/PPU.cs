using Codice.CM.Client.Differences;
using System;

namespace VirtualNes.Core
{
    public class PPU
    {
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

        private NES m_nes;
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
            m_nes = nes;
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

        private enum Screen
        {
            SCREEN_WIDTH = 256 + 16,
            SCREEN_HEIGHT = 240
        }
    }
}
