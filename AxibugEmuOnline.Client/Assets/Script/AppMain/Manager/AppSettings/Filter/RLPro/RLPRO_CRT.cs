using Assets.Script.AppMain.Filter;
using UnityEngine;

namespace AxibugEmuOnline.Client.Filters
{
    [Strip(RuntimePlatform.PSP2)]
    public class RLPRO_CRT : FilterEffect
    {
        public override string Name => nameof(RLPRO_CRT);
        protected override string ShaderName => null;

        #region BleedPass_Param
        public FilterParameter<EnumBleedMode> BleedMode = EnumBleedMode.NTSCOld3Phase;
        [Range(0, 15)]
        public FloatParameter BleedAmount = 1f;
        #endregion
        #region CRTAperture_Param
        [Range(0, 5)]
        public FloatParameter GlowHalation = 0.1f;
        [Range(0, 2)]
        public FloatParameter GlowDifusion = 0.05f;
        [Range(0, 2)]
        public FloatParameter MaskColors = 2.0f;
        [Range(0, 1)]
        public FloatParameter MaskStrength = 0.3f;
        [Range(0, 5)]
        public FloatParameter GammaInput = 2.4f;
        [Range(0, 5)]
        public FloatParameter GammaOutput = 2.4f;
        [Range(0, 2.5f)]
        public FloatParameter Brightness = 1.5f;
        #endregion
        #region TV_Effect_Param
        public FilterParameter<EnumWrapMode> WrapMode = EnumWrapMode.SimpleWrap;
        public FloatParameter maskDark = 0.5f;
        public FloatParameter maskLight = 1.5f;
        public FloatParameter hardScan = -8.0f;
        public FloatParameter hardPix = -3.0f;
        public Vector2Parameter warp = new Vector2(1.0f / 32.0f, 1.0f / 24.0f);
        public Vector2Parameter res;
        public FloatParameter resScale;
        public FloatParameter scale;
        public FloatParameter fade;
        #endregion

        Material m_bleedMat;
        Material m_crtApertureMat;
        Material m_tvEffectMat;

        protected override void OnInit(Material renderMat)
        {
            m_bleedMat = new Material(Shader.Find("Filter/RLPro_Bleed"));
            m_crtApertureMat = new Material(Shader.Find("Filter/RLPro_CRT_Aperture"));
            m_tvEffectMat = new Material(Shader.Find("Filter/RLPro_TV_Effect"));
        }

        protected override void OnRenderer(Material renderMat, Texture src, RenderTexture result)
        {
            var rt1 = BleedPass(src);
            var rt2 = CRT_AperturePass(rt1);
            var rt3 = TV_Effect_Pass(rt2);

            Graphics.Blit(rt3, result);

            RenderTexture.ReleaseTemporary(rt1);
            RenderTexture.ReleaseTemporary(rt2);
            RenderTexture.ReleaseTemporary(rt3);
        }

        private RenderTexture BleedPass(Texture src)
        {
            var rt = RenderTexture.GetTemporary(src.width, src.height);

            m_bleedMat.DisableKeyword("_NTSCOld3Phase");
            m_bleedMat.DisableKeyword("_NTSC3Phase");
            m_bleedMat.DisableKeyword("_NTSC2Phase");

            switch (BleedMode.GetValue())
            {
                case EnumBleedMode.NTSCOld3Phase:
                    m_bleedMat.EnableKeyword("_NTSCOld3Phase");
                    break;
                case EnumBleedMode.NTSC3Phase:
                    m_bleedMat.EnableKeyword("_NTSC3Phase");
                    break;
                case EnumBleedMode.NTSC2Phase:
                    m_bleedMat.EnableKeyword("_NTSC2Phase");
                    break;
            }

            m_bleedMat.SetFloat("bleedAmount", BleedAmount.GetValue());

            Graphics.Blit(src, rt, m_bleedMat);

            return rt;
        }
        private RenderTexture CRT_AperturePass(Texture src)
        {
            var rt = RenderTexture.GetTemporary(src.width, src.height);

            m_crtApertureMat.SetFloat("GLOW_HALATION", GlowHalation.GetValue());
            m_crtApertureMat.SetFloat("GLOW_DIFFUSION", GlowDifusion.GetValue());
            m_crtApertureMat.SetFloat("MASK_COLORS", MaskColors.GetValue());
            m_crtApertureMat.SetFloat("MASK_STRENGTH", MaskStrength.GetValue());
            m_crtApertureMat.SetFloat("GAMMA_INPUT", GammaInput.GetValue());
            m_crtApertureMat.SetFloat("GAMMA_OUTPUT", GammaOutput.GetValue());
            m_crtApertureMat.SetFloat("BRIGHTNESS", Brightness.GetValue());

            Graphics.Blit(src, rt, m_crtApertureMat);

            return rt;
        }
        private RenderTexture TV_Effect_Pass(Texture src)
        {
            var rt = RenderTexture.GetTemporary(src.width, src.height);

            m_tvEffectMat.DisableKeyword("_SimpleWrap");
            m_tvEffectMat.DisableKeyword("_CubicDistortion");

            switch (WrapMode.GetValue())
            {
                case EnumWrapMode.SimpleWrap:
                    m_tvEffectMat.EnableKeyword("_SimpleWrap"); break;
                case EnumWrapMode.CubicDistortion:
                    m_tvEffectMat.EnableKeyword("_CubicDistortion"); break;
            }

            m_tvEffectMat.SetFloat("maskDark", maskDark.GetValue());
            m_tvEffectMat.SetFloat("maskLight", maskLight.GetValue());
            m_tvEffectMat.SetFloat("hardScan", hardScan.GetValue());
            m_tvEffectMat.SetVector("warp", warp.GetValue());
            m_tvEffectMat.SetVector("res", res.GetValue());
            m_tvEffectMat.SetFloat("resScale", resScale.GetValue());
            m_tvEffectMat.SetFloat("scale", scale.GetValue());
            m_tvEffectMat.SetFloat("fade", fade.GetValue());

            Graphics.Blit(src, rt, m_tvEffectMat);

            return rt;
        }

        public enum EnumBleedMode
        {
            NTSCOld3Phase,
            NTSC3Phase,
            NTSC2Phase,
        }

        public enum EnumWrapMode
        {
            SimpleWrap,
            CubicDistortion
        }
    }
}