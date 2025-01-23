namespace AxibugEmuOnline.Client
{
    public static class PaletteDefine
    {
        public struct RGBQUAD
        {
            public byte rgbBlue;
            public byte rgbGreen;
            public byte rgbRed;
            public byte rgbReserved;
        }

        public class PALBUF
        {
            public byte r;
            public byte g;
            public byte b;

            public PALBUF(byte r, byte g, byte b)
            {
                this.r = r;
                this.g = g;
                this.b = b;
            }
        }

        // スキャンラインカラー
        private static int m_nScanlineColor => 75; //patternViewer调试器用的,参照EmulatorConfig.graphics.nScanlineColor的值

        public static float[][] PalConvTbl = new float[8][]
                {
           new float[3]{1.00f, 1.00f, 1.00f},
           new float[3]{1.00f, 0.80f, 0.73f},
           new float[3]{0.73f, 1.00f, 0.70f},
           new float[3]{0.76f, 0.78f, 0.58f},
           new float[3]{0.86f, 0.80f, 1.00f},
           new float[3]{0.83f, 0.68f, 0.85f},
           new float[3]{0.67f, 0.77f, 0.83f},
           new float[3]{0.68f, 0.68f, 0.68f},
                };

        public static PALBUF[] m_PaletteBuf = new PALBUF[64]
        {
            new PALBUF(0x7F, 0x7F, 0x7F),
            new PALBUF(0x20, 0x00, 0xB0),
            new PALBUF(0x28, 0x00, 0xB8),
            new PALBUF(0x60, 0x10, 0xA0),
            new PALBUF(0x98, 0x20, 0x78),
            new PALBUF(0xB0, 0x10, 0x30),
            new PALBUF(0xA0, 0x30, 0x00),
            new PALBUF(0x78, 0x40, 0x00),
            new PALBUF(0x48, 0x58, 0x00),
            new PALBUF(0x38, 0x68, 0x00),
            new PALBUF(0x38, 0x6C, 0x00),
            new PALBUF(0x30, 0x60, 0x40),
            new PALBUF(0x30, 0x50, 0x80),
            new PALBUF(0x00, 0x00, 0x00),
            new PALBUF(0x00, 0x00, 0x00),
            new PALBUF(0x00, 0x00, 0x00),
            new PALBUF(0xBC, 0xBC, 0xBC),
            new PALBUF(0x40, 0x60, 0xF8),
            new PALBUF(0x40, 0x40, 0xFF),
            new PALBUF(0x90, 0x40, 0xF0),
            new PALBUF(0xD8, 0x40, 0xC0),
            new PALBUF(0xD8, 0x40, 0x60),
            new PALBUF(0xE0, 0x50, 0x00),
            new PALBUF(0xC0, 0x70, 0x00),
            new PALBUF(0x88, 0x88, 0x00),
            new PALBUF(0x50, 0xA0, 0x00),
            new PALBUF(0x48, 0xA8, 0x10),
            new PALBUF(0x48, 0xA0, 0x68),
            new PALBUF(0x40, 0x90, 0xC0),
            new PALBUF(0x00, 0x00, 0x00),
            new PALBUF(0x00, 0x00, 0x00),
            new PALBUF(0x00, 0x00, 0x00),
            new PALBUF(0xFF, 0xFF, 0xFF),
            new PALBUF(0x60, 0xA0, 0xFF),
            new PALBUF(0x50, 0x80, 0xFF),
            new PALBUF(0xA0, 0x70, 0xFF),
            new PALBUF(0xF0, 0x60, 0xFF),
            new PALBUF(0xFF, 0x60, 0xB0),
            new PALBUF(0xFF, 0x78, 0x30),
            new PALBUF(0xFF, 0xA0, 0x00),
            new PALBUF(0xE8, 0xD0, 0x20),
            new PALBUF(0x98, 0xE8, 0x00),
            new PALBUF(0x70, 0xF0, 0x40),
            new PALBUF(0x70, 0xE0, 0x90),
            new PALBUF(0x60, 0xD0, 0xE0),
            new PALBUF(0x60, 0x60, 0x60),
            new PALBUF(0x00, 0x00, 0x00),
            new PALBUF(0x00, 0x00, 0x00),
            new PALBUF(0xFF, 0xFF, 0xFF),
            new PALBUF(0x90, 0xD0, 0xFF),
            new PALBUF(0xA0, 0xB8, 0xFF),
            new PALBUF(0xC0, 0xB0, 0xFF),
            new PALBUF(0xE0, 0xB0, 0xFF),
            new PALBUF(0xFF, 0xB8, 0xE8),
            new PALBUF(0xFF, 0xC8, 0xB8),
            new PALBUF(0xFF, 0xD8, 0xA0),
            new PALBUF(0xFF, 0xF0, 0x90),
            new PALBUF(0xC8, 0xF0, 0x80),
            new PALBUF(0xA0, 0xF0, 0xA0),
            new PALBUF(0xA0, 0xFF, 0xC8),
            new PALBUF(0xA0, 0xFF, 0xF0),
            new PALBUF(0xA0, 0xA0, 0xA0),
            new PALBUF(0x00, 0x00, 0x00),
            new PALBUF(0x00, 0x00, 0x00),
        };

        #region 256色モード用
        // Color
        public static RGBQUAD[][] m_cpPalette = new RGBQUAD[8][]
        {
            new RGBQUAD[64*2],
            new RGBQUAD[64*2],
            new RGBQUAD[64*2],
            new RGBQUAD[64*2],
            new RGBQUAD[64*2],
            new RGBQUAD[64*2],
            new RGBQUAD[64*2],
            new RGBQUAD[64*2],
        };
        // Monochrome
        public static RGBQUAD[][] m_mpPalette = new RGBQUAD[8][]
        {
            new RGBQUAD[64*2],
            new RGBQUAD[64*2],
            new RGBQUAD[64*2],
            new RGBQUAD[64*2],
            new RGBQUAD[64*2],
            new RGBQUAD[64*2],
            new RGBQUAD[64*2],
            new RGBQUAD[64*2],
        };
        #endregion

        #region ピクセルフォーマットに変換したパレット
        // Color
        public static uint[][] m_cnPalette = new uint[8][]
        {
            new uint[256],
            new uint[256],
            new uint[256],
            new uint[256],
            new uint[256],
            new uint[256],
            new uint[256],
            new uint[256],
        };
        // Color/Scanline
        public static uint[][] m_csPalette = new uint[8][]
        {
            new uint[256],
            new uint[256],
            new uint[256],
            new uint[256],
            new uint[256],
            new uint[256],
            new uint[256],
            new uint[256],
        };

        // Monochrome
        public static uint[][] m_mnPalette = new uint[8][]
        {
            new uint[256],
            new uint[256],
            new uint[256],
            new uint[256],
            new uint[256],
            new uint[256],
            new uint[256],
            new uint[256],
        };

        // Monochrome/Scanline
        public static uint[][] m_msPalette = new uint[8][]
        {
            new uint[256],
            new uint[256],
            new uint[256],
            new uint[256],
            new uint[256],
            new uint[256],
            new uint[256],
            new uint[256],
        };
        #endregion

        public static RGBQUAD[] GetPaletteData()
        {
            RGBQUAD[] rgb = new RGBQUAD[256];
            for (int i = 0; i < 64; i++)
            {
                rgb[i] = m_cpPalette[0][i];
                rgb[i + 0x40] = m_mpPalette[0][i];
            }

            return rgb;
        }

        static PaletteDefine()
        {
            int Rbit = 0, Gbit = 0, Bbit = 0;
            int Rsft = 0, Gsft = 0, Bsft = 0;

            GetBitMask(0xFF0000, ref Rsft, ref Rbit);
            GetBitMask(0x00FF00, ref Gsft, ref Gbit);
            GetBitMask(0x0000FF, ref Bsft, ref Bbit);

            for (int j = 0; j < 8; j++)
            {
                for (int i = 0; i < 64; i++)
                {
                    uint Rn, Gn, Bn;
                    uint Rs, Gs, Bs;

                    // Normal
                    Rn = (uint)(PalConvTbl[j][0] * m_PaletteBuf[i].r);
                    Gn = (uint)(PalConvTbl[j][1] * m_PaletteBuf[i].g);
                    Bn = (uint)(PalConvTbl[j][2] * m_PaletteBuf[i].b);
                    // Scanline
                    Rs = (uint)(PalConvTbl[j][0] * m_PaletteBuf[i].r * m_nScanlineColor / 100.0f);
                    Gs = (uint)(PalConvTbl[j][1] * m_PaletteBuf[i].g * m_nScanlineColor / 100.0f);
                    Bs = (uint)(PalConvTbl[j][2] * m_PaletteBuf[i].b * m_nScanlineColor / 100.0f);

                    m_cpPalette[j][i + 0x00].rgbRed = (byte)Rn;
                    m_cpPalette[j][i + 0x00].rgbGreen = (byte)Gn;
                    m_cpPalette[j][i + 0x00].rgbBlue = (byte)Bn;
                    m_cpPalette[j][i + 0x40].rgbRed = (byte)Rs;
                    m_cpPalette[j][i + 0x40].rgbGreen = (byte)Gs;
                    m_cpPalette[j][i + 0x40].rgbBlue = (byte)Bs;

                    m_cnPalette[j][i] = ((Rn >> (8 - Rbit)) << Rsft) | ((Gn >> (8 - Gbit)) << Gsft) | ((Bn >> (8 - Bbit)) << Bsft);
                    m_csPalette[j][i] = ((Rs >> (8 - Rbit)) << Rsft) | ((Gs >> (8 - Gbit)) << Gsft) | ((Bs >> (8 - Bbit)) << Bsft);

                    // Monochrome
                    Rn = (uint)(m_PaletteBuf[i & 0x30].r);
                    Gn = (uint)(m_PaletteBuf[i & 0x30].g);
                    Bn = (uint)(m_PaletteBuf[i & 0x30].b);
                    Rn =
                    Gn =
                    Bn = (uint)(0.299f * Rn + 0.587f * Gn + 0.114f * Bn);
                    Rn = (uint)(PalConvTbl[j][0] * Rn);
                    Gn = (uint)(PalConvTbl[j][1] * Gn);
                    Bn = (uint)(PalConvTbl[j][2] * Bn);
                    if (Rn > 0xFF) Rs = 0xFF;
                    if (Gn > 0xFF) Gs = 0xFF;
                    if (Bn > 0xFF) Bs = 0xFF;
                    // Scanline
                    Rs = (uint)(m_PaletteBuf[i & 0x30].r * m_nScanlineColor / 100.0f);
                    Gs = (uint)(m_PaletteBuf[i & 0x30].g * m_nScanlineColor / 100.0f);
                    Bs = (uint)(m_PaletteBuf[i & 0x30].b * m_nScanlineColor / 100.0f);
                    Rs =
                    Gs =
                    Bs = (uint)(0.299f * Rs + 0.587f * Gs + 0.114f * Bs);
                    Rs = (uint)(PalConvTbl[j][0] * Rs);
                    Gs = (uint)(PalConvTbl[j][1] * Gs);
                    Bs = (uint)(PalConvTbl[j][2] * Bs);
                    if (Rs > 0xFF) Rs = 0xFF;
                    if (Gs > 0xFF) Gs = 0xFF;
                    if (Bs > 0xFF) Bs = 0xFF;

                    m_mpPalette[j][i + 0x00].rgbRed = (byte)Rn;
                    m_mpPalette[j][i + 0x00].rgbGreen = (byte)Gn;
                    m_mpPalette[j][i + 0x00].rgbBlue = (byte)Bn;
                    m_mpPalette[j][i + 0x40].rgbRed = (byte)Rs;
                    m_mpPalette[j][i + 0x40].rgbGreen = (byte)Gs;
                    m_mpPalette[j][i + 0x40].rgbBlue = (byte)Bs;

                    m_mnPalette[j][i] = ((Rn >> (8 - Rbit)) << Rsft) | ((Gn >> (8 - Gbit)) << Gsft) | ((Bn >> (8 - Bbit)) << Bsft);
                    m_msPalette[j][i] = ((Rs >> (8 - Rbit)) << Rsft) | ((Gs >> (8 - Gbit)) << Gsft) | ((Bs >> (8 - Bbit)) << Bsft);
                }
            }
        }

        // ビット位置の取得
        static void GetBitMask(uint val, ref int shift, ref int bits)
        {
            shift = 0;
            while (((val & (1 << shift)) == 0) && (shift < 32))
            {
                shift++;
            }

            bits = 32;
            while (((val & (1 << (bits - 1))) == 0) && (bits > 0))
            {
                bits--;
            }
            bits = bits - shift;
        }
    }
}
