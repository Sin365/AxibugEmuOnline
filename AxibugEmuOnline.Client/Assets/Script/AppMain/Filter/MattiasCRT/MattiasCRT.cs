using AxibugEmuOnline.Client;
using UnityEngine;

public sealed class MattiasCRT : FilterEffect
{
    public override string Name => nameof(MattiasCRT);

    protected override string ShaderName => "Filter/MattiasCRT";

    protected override void OnRenderer(Material renderMat, Texture src, RenderTexture result)
    {
        renderMat.SetVector("_iResolution", new Vector4(result.width, result.height, 0, 0));
        Graphics.Blit(src, result, renderMat);
    }
}