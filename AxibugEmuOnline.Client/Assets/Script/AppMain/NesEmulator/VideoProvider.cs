using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using VirtualNes.Core;

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
        [SerializeField]
        private Material GPUTurboMat_gpu;
        private RenderTexture rt_gpu;
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
        }

        private void OnDestroy()
        {
            if (wrapTexBufferGH.IsAllocated)
                wrapTexBufferGH.Free();

            if (rt_gpu != null)
            {
                RenderTexture.ReleaseTemporary(rt_gpu);
                rt_gpu = null;
            }
        }

        public unsafe void SetDrawData(uint* screenData)
        {
            PrepareUI(screenData);
            if (GPUTurbo) PrepareForGPU(screenData);//判断使用GPU还是CPU
            else PrepareForCPU(screenData);//使用CPU

            if (GPUTurbo)
            {
                wrapTex_gpu.LoadRawTextureData(wrapTexBufferPointer_gpu, TexBufferSize_gpu);
                wrapTex_gpu.Apply();
                Graphics.Blit(wrapTex_gpu, rt_gpu, GPUTurboMat_gpu);
            }
            else
            {
                wrapTex_cpu.LoadRawTextureData(wrapTexBufferPointer_cpu, TexBufferSize_cpu);
                wrapTex_cpu.Apply();
            }
        }

        public void ApplyFilterEffect()
        {
            App.settings.Filter.ExecuteFilterRender(rt_gpu, Image);
        }

        private unsafe void PrepareUI(uint* screenData)
        {
            if (GPUTurbo)
            {
                if (wrapTex_gpu == null)
                {
                    wrapTex_gpu = new Texture2D(PPU.SCREEN_WIDTH, PPU.SCREEN_HEIGHT, TextureFormat.RGBA32, false);
                    wrapTex_gpu.filterMode = FilterMode.Point;
                    wrapTexBufferPointer_gpu = (IntPtr)screenData;
                    rt_gpu = RenderTexture.GetTemporary(256, wrapTex_gpu.height, 0);
                    rt_gpu.filterMode = FilterMode.Point;
                    rt_gpu.anisoLevel = 0;
                    rt_gpu.antiAliasing = 1;

                    TexBufferSize_gpu = wrapTex_gpu.width * wrapTex_gpu.height * 4;
                }

                if (Image.texture != rt_gpu) Image.texture = rt_gpu;
            }
            else
            {
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
                //PS：如果是CPU计算，宽度减少16的不必要部分，才能对齐
                width = PPU.SCREEN_WIDTH - 16;

                while (width > 0)
                {
                    var edx = screenData[pScn + 8];

                    uint index = edx & 0xFF;
                    //按下标颜色查找表中真实颜色
                    var colorData = palRaw[index];
                    //dst中颜色排列为abgr,而colorData排列为argb
                    uint r = (colorData & 0x00FF0000) >> 16; // 提取Red通道
                    uint g = (colorData & 0x0000FF00) >> 8;  // 提取Green通道
                    uint b = (colorData & 0x000000FF);       // 提取Blue通道

                    //用rgb构建颜色对象（如果非unity 可以用这个rgb 构建System.Drawing.Color 单个颜色对象）
                    uint abgr = 0xFF000000 | (b << 16) | (g << 8) | (r << 0);

                    //放进颜色矩阵
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
