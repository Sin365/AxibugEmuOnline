using AxibugEmuOnline.Client;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class FilterChainEffect : FilterEffect
{
    #region SealedForDisable
    protected sealed override string ShaderName => null;
    protected sealed override void OnInit(Material renderMat) { }
    protected sealed override void OnRenderer(Material renderMat, Texture src, RenderTexture result)
    {
        OnRenderer(src, result);
    }
    #endregion

    List<PassDefine> m_passes = new List<PassDefine>();

    static int Original;
    static int OriginalSize;
    static int Source;
    static int SourceSize;

    List<int> m_passOutputTexNames = new List<int>();

    static FilterChainEffect()
    {
        Original = Shader.PropertyToID(nameof(Original));
        OriginalSize = Shader.PropertyToID(nameof(OriginalSize));
        Source = Shader.PropertyToID(nameof(Source));
        SourceSize = Shader.PropertyToID(nameof(SourceSize));
    }

    protected sealed override void Init()
    {
        DefinePasses(ref m_passes);
        for (int i = 0; i < m_passes.Count; i++)
        {
            m_passes[i].Init(i);
            m_passOutputTexNames.Add(Shader.PropertyToID(m_passes[i].NormalOutputTextureName));
            if (m_passes[i].AliasOutputTextureName != null)
                m_passOutputTexNames.Add(Shader.PropertyToID(m_passes[i].AliasOutputTextureName));
        }
    }

    Dictionary<int, RenderTexture> m_outputCaches = new Dictionary<int, RenderTexture>();
    private void OnRenderer(Texture input, RenderTexture finalOut)
    {
        m_outputCaches.Clear();

        Vector4 originalSize = new Vector4(input.width, input.height, 1f / input.width, 1f / input.height);

        Texture lastoutput = input;
        for (int i = 0; i < m_passes.Count; i++)
        {
            var pass = m_passes[i];

            pass.Mat.SetTexture(Original, input);
            pass.Mat.SetVector(OriginalSize, originalSize);
            pass.Mat.SetTexture(Source, lastoutput);
            pass.Mat.SetVector(SourceSize, new Vector4(lastoutput.width, lastoutput.height, 1f / lastoutput.width, 1f / lastoutput.height));
            foreach (var existoutput in m_passOutputTexNames)
            {
                if (pass.Mat.HasTexture(existoutput) && m_outputCaches.TryGetValue(existoutput, out var passOutput))
                    pass.Mat.SetTexture(existoutput, passOutput);
            }

            var output = pass.GetOutput(input, lastoutput, finalOut);

            m_outputCaches[pass.NormalOutputTextureName_PID] = output;
            if (pass.AliasOutputTextureName != null) m_outputCaches[pass.AliasOutputTextureName_PID] = output;

            Graphics.Blit(lastoutput, output, pass.Mat);

            lastoutput = output;
        }

        Graphics.Blit(lastoutput, finalOut);
    }

    protected abstract void DefinePasses(ref List<PassDefine> passes);

    public class PassDefine
    {
        public string ShaderName { get; private set; }
        public FilterMode FilterMode { get; private set; }
        public TextureWrapMode WrapMode { get; private set; }
        public EnumScaleMode ScaleMode { get; private set; }
        public float ScaleX { get; private set; }
        public float ScaleY { get; private set; }
        public string AliasOutputTextureName { get; private set; }
        public int AliasOutputTextureName_PID { get; private set; }
        public string NormalOutputTextureName { get; private set; }
        public int NormalOutputTextureName_PID { get; private set; }

        private PassDefine() { }

        public static PassDefine Create(
            string shaderName,
            FilterMode filterMode = FilterMode.Point,
            TextureWrapMode wrapMode = TextureWrapMode.Clamp,
            EnumScaleMode scaleMode = EnumScaleMode.Source, float scaleX = 1f, float scaleY = 1f,
            string outputAlias = null
            )
        {
            return new PassDefine()
            {
                ShaderName = shaderName,
                FilterMode = filterMode,
                WrapMode = wrapMode,
                ScaleMode = scaleMode,
                ScaleX = scaleX,
                ScaleY = scaleY,
                AliasOutputTextureName = outputAlias,
            };
        }

        public int PassIndex { get; private set; }
        public Material Mat { get; private set; }
        internal void Init(int passIndex)
        {
            Mat = new Material(Shader.Find(ShaderName));
            PassIndex = passIndex;
            NormalOutputTextureName = $"PassOutput{passIndex}";
            NormalOutputTextureName_PID = Shader.PropertyToID(NormalOutputTextureName);

            if (AliasOutputTextureName != null) AliasOutputTextureName_PID = Shader.PropertyToID(AliasOutputTextureName);
        }

        internal RenderTexture GetOutput(Texture original, Texture source, Texture final)
        {
            int width = 0;
            int height = 0;
            switch (ScaleMode)
            {
                case EnumScaleMode.Viewport:
                    width = (int)(final.width * ScaleX);
                    height = (int)(final.height * ScaleY);
                    break;
                case EnumScaleMode.Source:
                    width = (int)(source.width * ScaleX);
                    height = (int)(source.height * ScaleY);
                    break;
                case EnumScaleMode.Absolute:
                    width = (int)ScaleX;
                    height = (int)ScaleY;
                    break;
            }
            var rt = RenderTexture.GetTemporary(width, height);
            rt.wrapMode = WrapMode;
            rt.filterMode = FilterMode;

            return rt;
        }
    }

    public enum EnumScaleMode
    {
        /// <summary> 以输入源为缩放基准 </summary           
        Source,
        /// <summary> 以分辨率作为缩放基准 </summary
        Viewport,
        /// <summary> 以固定值定义尺寸 </summary
        Absolute
    }
}
