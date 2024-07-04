using MyNes.Core;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public class UguiVideoProvider : IVideoProvider
    {
        public string Name => "Unity UI Video";

        public string ID => nameof(UguiVideoProvider).GetHashCode().ToString();

        private int[] m_texRawBuffer = new int[256 * 240];
        private Texture2D m_rawBufferWarper = new Texture2D(256, 240);
        private RawImage m_image;
        private RenderTexture m_drawRT;

        public void Initialize()
        {
            m_image = NesCoreProxy.Instance.DrawImage;
            m_image.texture = RenderTexture.GetTemporary(256, 240, 0, UnityEngine.Experimental.Rendering.GraphicsFormat.B8G8R8A8_UNorm);
        }

        public Color GetColor(uint value)
        {
            var r = 0xFF0000 & value;
            r >>= 16;
            var b = 0xFF & value;
            var g = 0xFF00 & value;
            g >>= 8;
            var color = new Color(r / 255f, g / 255f, b / 255f);
            return color;
        }

        public void Draw()
        {
            var colors = m_texRawBuffer.Select(w => GetColor((uint)w)).ToArray();
            m_rawBufferWarper.SetPixels(colors);
            m_rawBufferWarper.Apply();
            Graphics.Blit(m_rawBufferWarper, m_image.texture as RenderTexture);
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
            Array.Copy(buffer, m_texRawBuffer, m_texRawBuffer.Length);
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
