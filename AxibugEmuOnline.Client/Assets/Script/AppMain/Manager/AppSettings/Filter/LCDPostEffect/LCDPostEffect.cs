using UnityEngine;

namespace AxibugEmuOnline.Client.Filters
{
    public sealed class LCDPostEffect : FilterEffect
    {
        public override string Name => nameof(LCDPostEffect);

        protected override string ShaderName => "Filter/LCDPostEffect";

        protected override void OnRenderer(Material renderMat, Texture src, RenderTexture result)
        {
            Graphics.Blit(src, result, renderMat);
        }
    }
}
