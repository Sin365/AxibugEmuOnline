using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public class VideoProvider : MonoBehaviour
    {
        public RawImage Image;

        private Texture2D wrapTex;

        public void SetDrawData(byte[] data, int width, int height)
        {
            if (wrapTex == null) wrapTex = new Texture2D(width, height);

            var colors = data.Select(d => new Color((d / 255f), (d / 255f), (d / 255f), 1)).ToArray();
            wrapTex.SetPixels(colors);
            wrapTex.Apply();
            Graphics.Blit(wrapTex, Image.texture as RenderTexture);
        }
    }
}
