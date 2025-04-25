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
        public FilterParameter<EnumBleedMode> BleedMode = new FilterParameter<EnumBleedMode>(EnumBleedMode.NTSCOld3Phase);
        [Range(0, 15)]
        public FloatParameter BleedAmount = new FloatParameter(1f);
        #endregion
        #region CRTAperture_Param
        [Range(0, 5)]
        public FloatParameter GlowHalation = new FloatParameter(0.1f);
        [Range(0, 2)]
        public FloatParameter GlowDifusion = new FloatParameter(0.05f);
        [Range(0, 2)]
        public FloatParameter MaskColors = new FloatParameter(2.0f);
        [Range(0, 1)]
        public FloatParameter MaskStrength = new FloatParameter(0.3f);
        [Range(0, 5)]
        public FloatParameter GammaInput = new FloatParameter(2.4f);
        [Range(0, 5)]
        public FloatParameter GammaOutput = new FloatParameter(2.4f);
        [Range(0, 2.5f)]
        public FloatParameter Brightness = new FloatParameter(1.5f);
        #endregion

        Material m_bleedMat;
        Material m_crtApertureMat;
        Material m_tvEffectMat;

        protected override void OnInit(Material renderMat)
        {
            m_bleedMat = new Material(Shader.Find("Filter/RLPro_Bleed"));
            m_crtApertureMat = new Material(Shader.Find("Filter/RLPro_CRT_Aperture"));
        }

        protected override void OnRenderer(Material renderMat, Texture src, RenderTexture result)
        {
            var rt1 = BleedPass(src);
            var rt2 = CRT_AperturePass(rt1);

            Graphics.Blit(rt2, result);

            RenderTexture.ReleaseTemporary(rt1);
            RenderTexture.ReleaseTemporary(rt2);
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

        public enum EnumBleedMode
        {
            NTSCOld3Phase,
            NTSC3Phase,
            NTSC2Phase,
        }
    }
}