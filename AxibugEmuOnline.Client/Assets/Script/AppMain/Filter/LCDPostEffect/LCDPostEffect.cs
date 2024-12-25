using AxibugEmuOnline.Client;
using UnityEngine;

public sealed class LCDPostEffect : FilterEffect
{
    public override string Name => nameof(LCDPostEffect);

    protected override string ShaderName => "Filter/LCDPostEffect";

    protected override void OnRenderer(Material renderMat, Texture src, RenderTexture result)
    {
        renderMat.SetVector("_iResolution", new Vector4(Screen.width, Screen.height, 0, 0));
        Graphics.Blit(src, result, renderMat);
    }
}
