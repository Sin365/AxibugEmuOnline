using Assets.Script.AppMain.Filter;
using AxibugEmuOnline.Client;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public abstract class FilterChainEffect : FilterEffect
{
    #region SealedForDisable
    protected sealed override string ShaderName => null;
    protected sealed override void OnInit(Material renderMat) { }

    public sealed override void Render(Texture src, RenderTexture result)
    {
        OnRenderer(src, result);
    }
    protected sealed override void OnRenderer(Material renderMat, Texture src, RenderTexture result) { }
    #endregion

    List<PassDefine> m_passes = new List<PassDefine>();

    static int Original;
    static int OriginalSize;
    static int Source;
    static int SourceSize;
    static int FrameCount;
    static int OutputSize;

    List<int> m_passOutputTexNames = new List<int>();
    List<int> m_passOutputTexSizes = new List<int>();

    static FilterChainEffect()
    {
        Original = Shader.PropertyToID(nameof(Original));
        OriginalSize = Shader.PropertyToID(nameof(OriginalSize));
        Source = Shader.PropertyToID(nameof(Source));
        SourceSize = Shader.PropertyToID(nameof(SourceSize));
        FrameCount = Shader.PropertyToID(nameof(FrameCount));
        OutputSize = Shader.PropertyToID(nameof(OutputSize));
    }

    protected sealed override void Init()
    {
        DefinePasses(ref m_passes);
        for (int i = 0; i < m_passes.Count; i++)
        {
            m_passes[i].Init(i);
            m_passOutputTexNames.Add(Shader.PropertyToID(m_passes[i].NormalOutputTextureName));
            m_passOutputTexSizes.Add(Shader.PropertyToID($"{m_passes[i].NormalOutputTextureName}Size"));
            if (m_passes[i].AliasOutputTextureName != null)
            {
                m_passOutputTexNames.Add(Shader.PropertyToID(m_passes[i].AliasOutputTextureName));
                m_passOutputTexSizes.Add(Shader.PropertyToID($"{m_passes[i].AliasOutputTextureName}Size"));
            }
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
            pass.OnRender();

            pass.Mat.SetTexture(Original, input);
            pass.Mat.SetVector(OriginalSize, originalSize);
            pass.Mat.SetTexture(Source, lastoutput);
            pass.Mat.SetVector(SourceSize, new Vector4(lastoutput.width, lastoutput.height, 1f / lastoutput.width, 1f / lastoutput.height));
            pass.Mat.SetFloat(FrameCount, Time.frameCount);

            for (int index = 0; index < m_passOutputTexNames.Count; index++)
            {
                var existoutput = m_passOutputTexNames[index];
                var existoutputSize = m_passOutputTexSizes[index];
                if (m_outputCaches.TryGetValue(existoutput, out var passOutput))
                {
                    if (pass.Mat.HasTexture(existoutput))
                        pass.Mat.SetTexture(existoutput, passOutput);
                    if (pass.Mat.HasVector(existoutputSize))
                        pass.Mat.SetVector(existoutputSize, new Vector4(passOutput.width, passOutput.height, 1f / passOutput.width, 1f / passOutput.height));
                }
            }

            var output = pass.GetOutput(input, lastoutput, finalOut);

            pass.Mat.SetVector(OutputSize, new Vector4(output.width, output.height, 1f / output.width, 1f / output.height));

            m_outputCaches[pass.NormalOutputTextureName_PID] = output;
            if (pass.AliasOutputTextureName != null) m_outputCaches[pass.AliasOutputTextureName_PID] = output;

            Graphics.Blit(lastoutput, output, pass.Mat);

            lastoutput = output;
        }

        Graphics.Blit(lastoutput, finalOut);

        foreach (var rt in m_outputCaches.Values)
            RenderTexture.ReleaseTemporary(rt);
    }

    protected abstract void DefinePasses(ref List<PassDefine> passes);

    public class PassDefine
    {
        public string ShaderName { get; private set; }
        public FilterMode FilterMode { get; private set; }
        public TextureWrapMode WrapMode { get; private set; }
        public EnumScaleMode ScaleModeX { get; private set; }
        public EnumScaleMode ScaleModeY { get; private set; }
        public float ScaleX { get; private set; }
        public float ScaleY { get; private set; }
        public string AliasOutputTextureName { get; private set; }
        public int AliasOutputTextureName_PID { get; private set; }
        public string NormalOutputTextureName { get; private set; }
        public int NormalOutputTextureName_PID { get; private set; }
        public bool sRGB { get; private set; }

        private PassDefine() { }

        public static PassDefine Create(
            string shaderName,
            FilterMode filterMode = FilterMode.Point,
            TextureWrapMode wrapMode = TextureWrapMode.Clamp,
            EnumScaleMode scaleModeX = EnumScaleMode.Source, EnumScaleMode scaleModeY = EnumScaleMode.Source, float scaleX = 1f, float scaleY = 1f,
            string outputAlias = null,
            bool sRGB = false
            )
        {
            return new PassDefine()
            {
                ShaderName = shaderName,
                FilterMode = filterMode,
                WrapMode = wrapMode,
                ScaleModeX = scaleModeX,
                ScaleModeY = scaleModeY,
                ScaleX = scaleX,
                ScaleY = scaleY,
                AliasOutputTextureName = outputAlias,
                sRGB = sRGB,
            };
        }

        private Dictionary<string, FilterParameter> m_linkingParams = new Dictionary<string, FilterParameter>();
        public PassDefine SetParameters(string shaderValName, FilterParameter para)
        {
            m_linkingParams[shaderValName] = para;
            return this;
        }

        public int PassIndex { get; private set; }
        public Material Mat { get; private set; }

        public void OnRender()
        {
            foreach (var item in m_linkingParams)
            {
                var valType = item.Value.ValueType;
                var val = item.Value.Value;
                var paraName = item.Key;
                if (valType == typeof(float))
                    Mat.SetFloat(paraName, (float)val);
            }
        }
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
            switch (ScaleModeX)
            {
                case EnumScaleMode.Viewport:
                    width = (int)(final.width * ScaleX);
                    break;
                case EnumScaleMode.Source:
                    width = (int)(source.width * ScaleX);
                    break;
                case EnumScaleMode.Absolute:
                    width = (int)ScaleX;
                    break;
            }
            int height = 0;
            switch (ScaleModeY)
            {
                case EnumScaleMode.Viewport:
                    height = (int)(final.height * ScaleY);
                    break;
                case EnumScaleMode.Source:
                    height = (int)(source.height * ScaleY);
                    break;
                case EnumScaleMode.Absolute:
                    height = (int)ScaleY;
                    break;
            }

            //if (sRGB) format = GraphicsFormat.R8G8B8A8_SRGB;
            var rt = RenderTexture.GetTemporary(width, height, 0, GraphicsFormat.R8G8B8A8_UNorm, 1);

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
