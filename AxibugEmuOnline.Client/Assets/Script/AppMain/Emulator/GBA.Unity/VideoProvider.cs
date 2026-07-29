using OptimeGBA;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;


namespace AxibugEmuOnline.Client.GBA.Unity
{
    public class VideoProvider : MonoBehaviour
    {
        public RawImage m_drawCanvas;
        private RectTransform m_drawCanvasrect;
        private IntPtr wrapTexBufferPointer;
        [HideInInspector]
        public Texture2D wrapTex;
        private int TexBufferSize;

        uint[] wrapTexBuffer = new uint[240 * 160];
        //Color32[] DisplayColorBuffer = new Color32[240 * 160];
        internal Vector3 srcCanvasLocalEulerAngles;

        private void Awake()
        {
            if (wrapTex == null)
            {
                wrapTex = new Texture2D(240, 160, TextureFormat.RGBA32, false);
                wrapTex.filterMode = FilterMode.Point;
                //wrapTexBuffer = screenData;

                // 固定数组，防止垃圾回收器移动它  
                GCHandle handle = GCHandle.Alloc(wrapTexBuffer, GCHandleType.Pinned);
                // 获取数组的指针  
                wrapTexBufferPointer = handle.AddrOfPinnedObject();
                m_drawCanvas.texture = wrapTex;
                TexBufferSize = wrapTexBuffer.Length * 4;


                m_drawCanvasrect = m_drawCanvas.GetComponent<RectTransform>();
                float targetWidth = ((float)240 / 160) * m_drawCanvasrect.rect.height;
                m_drawCanvasrect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
                srcCanvasLocalEulerAngles = m_drawCanvas.transform.localEulerAngles;
            }
        }

        public unsafe void OnRenderFrame()
        {
            var buf = Emulator.instance.ShowBackBuf ? Emulator.instance.gba.Ppu.Renderer.ScreenBack : Emulator.instance.gba.Ppu.Renderer.ScreenFront;
            unsafe
            {
                for (uint i = 0; i < 240 * 160; i++)
                {
                    wrapTexBuffer[i] = PpuRenderer.ColorLutCorrected[buf[i] & 0x7FFF];
                    //fixed (uint* p = &wrapTexBuffer[i])
                    //{
                    //    byte* bp = (byte*)p;
                    //    DisplayColorBuffer[i] = new Color32(*(bp++), *(bp++), *(bp++), *(bp++));
                    //}
                }
            }

            wrapTex.LoadRawTextureData(wrapTexBufferPointer, TexBufferSize);
            //wrapTex.SetPixels32(DisplayColorBuffer, 0);
            wrapTex.Apply();
        }
    }
}