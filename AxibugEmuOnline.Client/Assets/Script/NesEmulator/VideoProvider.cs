using AxibugEmuOnline.Client.Assets.Script.NesEmulator;
using Codice.CM.Client.Differences;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using VirtualNes.Core;

namespace AxibugEmuOnline.Client
{
    public class VideoProvider : MonoBehaviour
    {
        public RawImage Image;

        private Color32[] wrapTexBuffer;
        private IntPtr wrapTexBufferPointer;
        private Texture2D wrapTex;

        public void SetDrawData(byte[] screenData, byte[] lineColorMode, int screenWidth, int screenHeight)
        {
            if (wrapTex == null)
            {
                wrapTex = new Texture2D(screenWidth, screenHeight, TextureFormat.BGRA32, false);
                wrapTexBuffer = new Color32[screenWidth * screenHeight];
                // 固定数组，防止垃圾回收器移动它  
                GCHandle handle = GCHandle.Alloc(wrapTexBuffer, GCHandleType.Pinned);
                // 获取数组的指针  
                wrapTexBufferPointer = handle.AddrOfPinnedObject();
            }

            uint[] pPal;
            int pScn = 0;
            int width;

            var Dst = wrapTexBuffer;
            var pDst = 0;

            for (int line = 0; line < screenHeight; line++)
            {
                if ((lineColorMode[line] & 0x80) == 0)
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
                    var edx = screenData[pScn + 8];

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

            //wrapTex.SetPixels32(wrapTexBuffer);
            wrapTex.LoadRawTextureData(wrapTexBufferPointer, screenWidth * screenHeight * 4);
            wrapTex.Apply();

            Graphics.Blit(wrapTex, Image.mainTexture as RenderTexture);
        }


    }
}
