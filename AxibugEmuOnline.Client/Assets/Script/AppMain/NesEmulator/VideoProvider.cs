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
        //ͼ�������ֽ���
        private int TexBufferSize_gpu;
        //ͼ������ָ��
        private IntPtr wrapTexBufferPointer_gpu;
        //Unity 2D��������,����UI�ϻ��������������
        private Texture2D wrapTex_gpu;
        //nes��ɫ������,��ת��Ϊunity��������
        private Texture2D pPal_gpu;
        private Material GPUTurboMat_gpu;
        #endregion

        #region CPU
        //ͼ�������ֽ���
        private int TexBufferSize_cpu;
        //ͼ������ָ��
        private GCHandle wrapTexBufferGH;
        private IntPtr wrapTexBufferPointer_cpu;
        //Unity 2D��������,����UI�ϻ��������������
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
            if (GPUTurbo) PrepareForGPU(screenData);//�ж�ʹ��GPU����CPU
            else PrepareForCPU(screenData);//ʹ��CPU

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
                //PS�������CPU���㣬���ȼ���16�Ĳ���Ҫ���֣����ܶ���
                width = PPU.SCREEN_WIDTH - 16;

                while (width > 0)
                {
                    var edx = screenData[pScn + 8];

                    uint index = edx & 0xFF;
                    //���±���ɫ���ұ�����ʵ��ɫ
                    var colorData = palRaw[index];
                    //dst����ɫ����Ϊabgr,��colorData����Ϊargb
                    uint r = (colorData & 0x00FF0000) >> 16; // ��ȡRedͨ��
                    uint g = (colorData & 0x0000FF00) >> 8;  // ��ȡGreenͨ��
                    uint b = (colorData & 0x000000FF);       // ��ȡBlueͨ��

                    //��rgb������ɫ���������unity ���������rgb ����System.Drawing.Color ������ɫ����
                    uint abgr = 0xFF000000 | (b << 16) | (g << 8) | (r << 0);

                    //�Ž���ɫ����
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