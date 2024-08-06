using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VirtualNes;
using static AxibugEmuOnline.Client.PaletteDefine;

namespace AxibugEmuOnline.Client
{
    public class PatternViewer : MonoBehaviour
    {
        public RawImage img;

        private Color32[] m_lpPattern = new Color32[128 * 256];
        private Texture2D m_texture;
        private Dictionary<byte, RGBQUAD> colors = new Dictionary<byte, RGBQUAD>();

        private void Awake()
        {
            m_texture = new Texture2D(128, 256);
        }

        private void Update()
        {
            Paint();
        }

        public void Paint()
        {
            img.texture = m_texture;

            var pal = MMU.SPPAL;
            var palette = PaletteDefine.GetPaletteData();
            colors[0] = palette[pal[0]];
            colors[1] = palette[pal[1]];
            colors[2] = palette[pal[2]];
            colors[3] = palette[pal[3]];

            for (int i = 0; i < 8; i++)
            {
                var Ptn = MMU.PPU_MEM_BANK[i];
                int lpPtn = 0;
                for (int p = 0; p < 64; p++)
                {
                    int lpScn = i * 32 * 128 + (p & 15) * 8 + (p / 16) * 8 * 128;
                    for (int y = 0; y < 8; y++)
                    {
                        byte chr_l = Ptn[lpPtn + y];
                        byte chr_h = Ptn[lpPtn + y + 8];
                        m_lpPattern[lpScn + 0] = ToColor32(colors, (((chr_h >> 6) & 2) | ((chr_l >> 7) & 1)));
                        m_lpPattern[lpScn + 4] = ToColor32(colors, (((chr_h >> 2) & 2) | ((chr_l >> 3) & 1)));
                        m_lpPattern[lpScn + 1] = ToColor32(colors, (((chr_h >> 5) & 2) | ((chr_l >> 6) & 1)));
                        m_lpPattern[lpScn + 5] = ToColor32(colors, (((chr_h >> 1) & 2) | ((chr_l >> 2) & 1)));
                        m_lpPattern[lpScn + 2] = ToColor32(colors, (((chr_h >> 4) & 2) | ((chr_l >> 5) & 1)));
                        m_lpPattern[lpScn + 6] = ToColor32(colors, (((chr_h >> 0) & 2) | ((chr_l >> 1) & 1)));
                        m_lpPattern[lpScn + 3] = ToColor32(colors, (((chr_h >> 3) & 2) | ((chr_l >> 4) & 1)));
                        m_lpPattern[lpScn + 7] = ToColor32(colors, (((chr_h << 1) & 2) | ((chr_l >> 0) & 1)));
                        // Next line
                        lpScn += 128;
                    }
                    // Next pattern
                    lpPtn += 16;
                }
            }

            m_texture.SetPixels32(m_lpPattern);
            m_texture.Apply();
        }

        private Color32 ToColor32(Dictionary<byte, RGBQUAD> map, int v)
        {
            var raw = map[(byte)v];
            return new Color32(raw.rgbRed, raw.rgbGreen, raw.rgbBlue, 255);
        }
    }
}
