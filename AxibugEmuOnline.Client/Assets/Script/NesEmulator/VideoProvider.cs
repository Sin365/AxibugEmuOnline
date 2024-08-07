using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public class VideoProvider : MonoBehaviour
    {
        public NesEmulator NesEmu;

        public RawImage Image;

        private UInt32[] wrapTexBuffer;
        private IntPtr wrapTexBufferPointer;
        private Texture2D wrapTex;
        private int TexBufferSize;

        private uint[] pPal;
        public void SetDrawData(byte[] screenData, byte[] lineColorMode, int screenWidth, int screenHeight)
        {
            if (wrapTex == null)
            {
                wrapTex = new Texture2D(screenWidth, screenHeight, TextureFormat.BGRA32, false);
                wrapTexBuffer = new UInt32[screenWidth * screenHeight];
                // 固定数组，防止垃圾回收器移动它  
                GCHandle handle = GCHandle.Alloc(wrapTexBuffer, GCHandleType.Pinned);
                // 获取数组的指针  
                wrapTexBufferPointer = handle.AddrOfPinnedObject();

                Image.texture = wrapTex;
                pPal = PaletteDefine.m_cnPalette[0];

                TexBufferSize = wrapTexBuffer.Length * 4;
            }

            int pScn = 0;
            int width;

            var Dst = wrapTexBuffer;
            var pDst = 0;

            for (int line = 0; line < screenHeight; line++)
            {
                width = screenWidth;

                while (width > 0)
                {
                    var edx = screenData[pScn + 8];

                    int index = edx & 0xFF;
                    var colorData = pPal[index];
                    Dst[pDst] = 0xFF000000 | colorData;

                    pScn += 1;
                    pDst += 1;
                    width -= 1;
                }

                pScn += 16;// PPU.SCREEN_WIDTH - screenWidth;
            }

            wrapTex.LoadRawTextureData(wrapTexBufferPointer, TexBufferSize);
            wrapTex.Apply();
        }
    }
}
