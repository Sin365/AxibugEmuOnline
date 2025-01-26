using Assets.Script.AppMain.Filter;
using UnityEngine;
using UnityEngine.Rendering;

namespace AxibugEmuOnline.Client.Filters
{
    [Strip(RuntimePlatform.PSP2)]
    public class RetroArchMattiasCRTGlow : FilterEffect
    {
        public override string Name => nameof(RetroArchMattiasCRTGlow);
        protected override string ShaderName => "Filter/RetroArch/MattiasCRTWithGlow";

        [Range(0.02f, 20f)]
        public FloatParameter InputGamma = new FloatParameter(11);
        [Range(0.02f, 1f)]
        public FloatParameter GaussianWidth = new FloatParameter(0.4f);
        [Range(0.02f, 1f)]
        public FloatParameter ColorBoost = new FloatParameter(0.4f);
        [Range(0.02f, 1f)]
        public FloatParameter GlowWhitePoint = new FloatParameter(0.4f);
        [Range(0.1f, 6f)]
        public FloatParameter GlowRolloff = new FloatParameter(2.2f);
        [Range(0.05f, 0.8f)]
        public FloatParameter GlowStrength = new FloatParameter(0.45f);
        [Range(0.02f, 2.6f)]
        public FloatParameter MonitorGamma = new FloatParameter(2.2f);

        int m_gamma_ID = Shader.PropertyToID("_gamma");
        int m_horiz_gauss_width_ID = Shader.PropertyToID("_horiz_gauss_width");
        int m_BOOST_ID = Shader.PropertyToID("_BOOST");
        int m_GLOW_WHITEPOINT_ID = Shader.PropertyToID("_GLOW_WHITEPOINT");
        int m_GLOW_ROLLOFF_ID = Shader.PropertyToID("_GLOW_ROLLOFF");
        int m_BLOOM_STRENGTH_ID = Shader.PropertyToID("_BLOOM_STRENGTH");
        int m_OUTPUT_GAMMA_ID = Shader.PropertyToID("_OUTPUT_GAMMA");


        CommandBuffer m_multipPassCmd;
        int m_wrapRT;

        protected override void OnInit(Material renderMat)
        {
            m_multipPassCmd = new CommandBuffer();
            m_wrapRT = Shader.PropertyToID($"{Name}.WrapRT");
        }

        protected override void OnRenderer(Material renderMat, Texture src, RenderTexture result)
        {
            m_multipPassCmd.Clear();

            m_multipPassCmd.GetTemporaryRT(m_wrapRT, result.width, result.height);

            renderMat.SetFloat(m_gamma_ID, InputGamma.GetValue());
            renderMat.SetFloat(m_horiz_gauss_width_ID, GaussianWidth.GetValue());
            renderMat.SetFloat(m_BOOST_ID, ColorBoost.GetValue());
            renderMat.SetFloat(m_GLOW_WHITEPOINT_ID, GlowWhitePoint.GetValue());
            renderMat.SetFloat(m_GLOW_ROLLOFF_ID, GlowRolloff.GetValue());
            renderMat.SetFloat(m_BLOOM_STRENGTH_ID, GlowStrength.GetValue());
            renderMat.SetFloat(m_OUTPUT_GAMMA_ID, MonitorGamma.GetValue());

            m_multipPassCmd.Blit(src, result);
            for (int i = 0; i < renderMat.shader.passCount; i++)
            {
                m_multipPassCmd.Blit(result, m_wrapRT, renderMat, i);
                m_multipPassCmd.Blit(m_wrapRT, result);
            }

            m_multipPassCmd.ReleaseTemporaryRT(m_wrapRT);
            Graphics.ExecuteCommandBuffer(m_multipPassCmd);
        }
    }
}
