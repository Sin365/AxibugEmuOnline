using AxibugEmuOnline.Client.Assets.Script.NesEmulator;
using Codice.CM.Client.Differences;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using VirtualNes.Core;

namespace AxibugEmuOnline.Client
{
    public class VideoProvider : MonoBehaviour
    {
        public RawImage Image;

        private Texture2D wrapTex;

        public void SetDrawData(byte[] screenData, byte[] lineColorMode, int screenWidth, int screenHeight)
        {
            if (wrapTex == null) wrapTex = new Texture2D(screenWidth, screenHeight);

            var str = Encoding.ASCII.GetString(screenData, 0, screenData.Length);

            uint[] pPal;
            int pScn = 0;
            int width;

            var Dst = wrapTex.GetPixels32();
            var pDst = 0;

            for (int line = 0; line < screenHeight; line++)
            {
                if ((lineColorMode[line] & 0x80) != 0)
                {
                    pPal = PaletteDefine.m_cnPalette[lineColorMode[line] & 0x07];
                }
                else
                {
                    pPal = PaletteDefine.m_mnPalette[lineColorMode[line] & 0x07];
                }

                width = screenWidth;

                while (width > 0)
                {
                    Color32 temp = new Color32(255, 255, 255, 255);
                    var edx = screenData[pScn];

                    byte index = (byte)(edx & 0xFF);
                    var colorData = pPal[index];
                    var rawData = BitConverter.GetBytes(colorData);
                    Dst[pDst] = new Color32(rawData[0], rawData[1], rawData[2], 255);

                    pScn += 1;
                    pDst += 1;
                    width -= 1;
                }

                pScn += PPU.SCREEN_WIDTH - screenWidth;
            }

            wrapTex.SetPixels32(Dst);
            wrapTex.Apply();

            Graphics.Blit(wrapTex, Image.mainTexture as RenderTexture);
        }


    }
}
