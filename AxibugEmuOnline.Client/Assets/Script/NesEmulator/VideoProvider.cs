using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using VirtualNes.Core;

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

        private Texture2D pPal;
        public void SetDrawData(uint[] screenData, byte[] lineColorMode, int screenWidth, int screenHeight)
        {
            if (wrapTex == null)
            {
                wrapTex = new Texture2D(272, 240, TextureFormat.BGRA32, false);
                wrapTex.filterMode = FilterMode.Point;
                wrapTexBuffer = screenData;

                // 固定数组，防止垃圾回收器移动它  
                GCHandle handle = GCHandle.Alloc(wrapTexBuffer, GCHandleType.Pinned);
                // 获取数组的指针  
                wrapTexBufferPointer = handle.AddrOfPinnedObject();
                
                Image.material.SetTexture("_MainTex", wrapTex);

                TexBufferSize = wrapTexBuffer.Length * 4;

                var palRaw = PaletteDefine.m_cnPalette[0];
                pPal = new Texture2D(palRaw.Length, 1, TextureFormat.BGRA32, 1, true);
                pPal.filterMode = FilterMode.Point;
                for (int i = 0; i < palRaw.Length; i++)
                {
                    uint colorRaw = palRaw[i];
                    var argbColor = BitConverter.GetBytes(colorRaw);
                    Color temp = Color.white;
                    temp.r = argbColor[2]/255f;
                    temp.g = argbColor[1]/255f;
                    temp.b = argbColor[0]/255f;
                    temp.a = 1;
                    pPal.SetPixel(i, 0, temp);
                }
                pPal.Apply();
                Image.material.SetTexture("_PalTex", pPal);
            }

            wrapTex.LoadRawTextureData(wrapTexBufferPointer, TexBufferSize);
            wrapTex.Apply();
        }
    }
}
