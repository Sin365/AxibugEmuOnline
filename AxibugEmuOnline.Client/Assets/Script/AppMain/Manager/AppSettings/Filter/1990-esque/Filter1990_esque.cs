using AxibugEmuOnline.Client;
using UnityEngine;
using UnityEngine.Rendering;

public class Filter1990_esque : FilterEffect
{
    public override string Name => "1990-esque";

    protected override string ShaderName => "Filter/1990-esque";

    protected override void OnRenderer(Material renderMat, Texture src, RenderTexture result)
    {
        Graphics.Blit(src, result, renderMat, 1);
    }
}
