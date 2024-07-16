using MyNes.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace AxibugEmuOnline.Client
{
    public class UguiVideoProvider : MonoBehaviour, IVideoProvider
    {
        public string Name => "Unity UI Video";

        public string ID => nameof(UguiVideoProvider).GetHashCode().ToString();

        [SerializeField]
        private RawImage m_drawCanvas;
        [SerializeField]
        private Text m_fpsText;

        private Color[] m_texRawBuffer = new Color[256 * 240];
        private Texture2D m_rawBufferWarper;
        private RenderTexture m_drawRT;
        private Color temp = Color.white;


        public void Initialize()
        {
            m_rawBufferWarper = new Texture2D(256, 240);
            //m_drawCanvas.texture = RenderTexture.GetTemporary(256, 240, 0, UnityEngine.Experimental.Rendering.GraphicsFormat.B8G8R8A8_SRGB);
        }        

        public void GetColor(uint value, ref Color res)
        {
            var r = 0xFF0000 & value;
            r >>= 16;
            var b = 0xFF & value;
            var g = 0xFF00 & value;
            g >>= 8;
            res.r = r / 255f;
            res.g = g / 255f;
            res.b = b / 255f;
        }

        public void Update()
        {
            var colors = m_texRawBuffer;
            m_rawBufferWarper.SetPixels(colors);
            m_rawBufferWarper.Apply();
            Graphics.Blit(m_rawBufferWarper, m_drawCanvas.texture as RenderTexture);

            m_fpsText.text = $"Audio:{NesCoreProxy.Instance.AudioCom.FPS}";
        }

        public void WriteErrorNotification(string message, bool instant)
        {

        }

        public void WriteInfoNotification(string message, bool instant)
        {

        }

        public void WriteWarningNotification(string message, bool instant)
        {
        }

        public void TakeSnapshotAs(string path, string format)
        {

        }

        public void TakeSnapshot()
        {

        }

        public void ShutDown()
        {
        }

        public void SignalToggle(bool started)
        {
        }

        public void SubmitFrame(ref int[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                GetColor((uint)buffer[i], ref temp);
                m_texRawBuffer[i] = temp;
            }
        }

        public void ResizeBegin()
        {
        }

        public void ResizeEnd()
        {
        }

        public void ApplyRegionChanges()
        {
        }

        public void Resume()
        {
        }

        public void ToggleAspectRatio(bool keep_aspect)
        {
        }

        public void ToggleFPS(bool show_fps)
        {
        }

        public void ApplyFilter()
        {
        }

    }
}
