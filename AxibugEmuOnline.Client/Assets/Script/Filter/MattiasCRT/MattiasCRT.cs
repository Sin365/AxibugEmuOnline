using AxibugEmuOnline.Client;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[System.Serializable]
[PostProcess(typeof(MattiasCRTRenderer), PostProcessEvent.BeforeStack, "Filter/MattiasCRT")]
public sealed class MattiasCRT : FilterEffect
{
    public override string Name => nameof(MattiasCRT); 
}

public sealed class MattiasCRTRenderer : PostProcessEffectRenderer<MattiasCRT>
{
    private Shader shader;
    private Material material;

    public override void Init()
    {
        shader = Shader.Find("Filter/MattiasCRT");
        material = new Material(shader);
    }

    public override void Render(PostProcessRenderContext context)
    {
        material.SetVector("_iResolution", new Vector4(Screen.width, Screen.height, 0, 0));
        context.command.Blit(context.source, context.destination, material);
    }
}