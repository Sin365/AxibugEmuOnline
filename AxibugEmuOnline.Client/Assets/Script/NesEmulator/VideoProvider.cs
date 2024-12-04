using AxibugEmuOnline.Client.Common;
using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using VirtualNes.Core;
using static UnityEngine.UI.CanvasScaler;

namespace AxibugEmuOnline.Client
{
    public class VideoProvider : MonoBehaviour
    {
        #region UI_REF
        public NesEmulator NesEmu;
        public Canvas DrawCanvas;
        public RawImage Image;
        #endregion


        #region GPU_TURBO
        //图像数据字节数
        private int TexBufferSize_gpu;
        //图像数据指针
        private IntPtr wrapTexBufferPointer_gpu;
        //Unity 2D纹理对象,用于UI上绘制最终输出画面
        private Texture2D wrapTex_gpu;
        //nes调色板数据,已转换为unity纹理对象
        private Texture2D pPal_gpu;
        private Material GPUTurboMat_gpu;
        #endregion

        #region CPU
        //图像数据字节数
        private int TexBufferSize_cpu;
        //图像数据指针
        private GCHandle wrapTexBufferGH;
        private IntPtr wrapTexBufferPointer_cpu;
        //Unity 2D纹理对象,用于UI上绘制最终输出画面
        private Texture2D wrapTex_cpu;
        #endregion

        public bool GPUTurbo = true;

        private void Awake()
        {
            DrawCanvas.worldCamera = Camera.main;
            GPUTurboMat_gpu = Image.material;
        }

        private void OnDestroy()
        {
            if (wrapTexBufferGH.IsAllocated)
                wrapTexBufferGH.Free();
        }

        public unsafe void SetDrawData(uint* screenData)
        {
            PrepareUI(screenData);
            if (GPUTurbo) PrepareForGPU(screenData);
            else PrepareForCPU(screenData);

            if (GPUTurbo)
            {
                wrapTex_gpu.LoadRawTextureData(wrapTexBufferPointer_gpu, TexBufferSize_gpu);
                wrapTex_gpu.Apply();
            }
            else
            {
                wrapTex_cpu.LoadRawTextureData(wrapTexBufferPointer_cpu, TexBufferSize_cpu);
                wrapTex_cpu.Apply();
            }
        }

        private unsafe void PrepareUI(uint* screenData)
        {
            if (GPUTurbo)
            {
                if (Image.material != GPUTurboMat_gpu) Image.material = GPUTurboMat_gpu;

                if (wrapTex_gpu == null)
                {
                    wrapTex_gpu = new Texture2D(PPU.SCREEN_WIDTH, PPU.SCREEN_HEIGHT, TextureFormat.RGBA32, false);
                    wrapTex_gpu.filterMode = FilterMode.Point;
                    wrapTexBufferPointer_gpu = (IntPtr)screenData;

                    TexBufferSize_gpu = wrapTex_gpu.width * wrapTex_gpu.height * 4;
                }

                if (Image.texture != wrapTex_gpu) Image.texture = wrapTex_gpu;
            }
            else
            {
                if (Image.material == GPUTurboMat_gpu) Image.material = null;

                if (wrapTex_cpu == null)
                {
                    wrapTex_cpu = new Texture2D(PPU.SCREEN_WIDTH - 16, PPU.SCREEN_HEIGHT, TextureFormat.RGBA32, false);
                    wrapTex_cpu.filterMode = FilterMode.Point;

                    uint[] cpuTexBuffer = new uint[wrapTex_cpu.width * wrapTex_cpu.height];

                    wrapTexBufferGH = GCHandle.Alloc(cpuTexBuffer, GCHandleType.Pinned);
                    wrapTexBufferPointer_cpu = wrapTexBufferGH.AddrOfPinnedObject();
                    TexBufferSize_cpu = cpuTexBuffer.Length * 4;
                }
                if (Image.texture != wrapTex_cpu) Image.texture = wrapTex_cpu;
            }
        }

        private unsafe void PrepareForGPU(uint* screenData)
        {
            if (pPal_gpu == null)
            {
                var palRaw = PaletteDefine.m_cnPalette[0];

                pPal_gpu = new Texture2D(palRaw.Length, 1, TextureFormat.RGBA32, false);
                pPal_gpu.filterMode = FilterMode.Point;

                for (int i = 0; i < palRaw.Length; i++)
                {
                    uint colorRaw = palRaw[i];
                    var argbColor = BitConverter.GetBytes(colorRaw);
                    Color temp = Color.white;
                    temp.r = argbColor[2] / 255f;
                    temp.g = argbColor[1] / 255f;
                    temp.b = argbColor[0] / 255f;
                    temp.a = 1;
                    pPal_gpu.SetPixel(i, 0, temp);
                }
                pPal_gpu.Apply();
                GPUTurboMat_gpu.SetTexture("_PalTex", pPal_gpu);
            }
        }

        private unsafe void PrepareForCPU(uint* screenData)
        {
            int pScn = 0;
            int width;

            var Dst = (uint*)wrapTexBufferPointer_cpu;
            var pDst = 0;
            var palRaw = PaletteDefine.m_cnPalette[0];

            for (int line = 0; line < PPU.SCREEN_HEIGHT; line++)
            {
                width = PPU.SCREEN_WIDTH - 16;

                while (width > 0)
                {
                    var edx = screenData[pScn + 8];

                    uint index = edx & 0xFF;
                    var colorData = palRaw[index];
                    //dst中颜色排列为abgr,而colorData排列为argb
                    uint r = (colorData & 0x00FF0000) >> 16; // 提取Red通道
                    uint g = (colorData & 0x0000FF00) >> 8;  // 提取Green通道
                    uint b = (colorData & 0x000000FF);       // 提取Blue通道

                    uint abgr = 0xFF000000 | (b << 16) | (g << 8) | (r << 0);

                    Dst[pDst] = abgr;

                    pScn += 1;
                    pDst += 1;
                    width -= 1;
                }

                pScn += 16;
            }
        }
    }
}
