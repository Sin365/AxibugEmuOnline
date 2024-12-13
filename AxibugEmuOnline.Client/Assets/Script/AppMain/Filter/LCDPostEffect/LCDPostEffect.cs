using AxibugEmuOnline.Client;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[System.Serializable]
[PostProcess(typeof(LCDPostEffectRenderer), PostProcessEvent.BeforeStack, "Filter/LCDPostEffect")]
public sealed class LCDPostEffect : FilterEffect
{
    public override string Name => nameof(LCDPostEffect);
}

public sealed class LCDPostEffectRenderer : PostProcessEffectRenderer<LCDPostEffect>
{
    private Shader shader;
    private Material material;

    public override void Init()
    {
        shader = Shader.Find("Filter/LCDPostEffect");
        material = new Material(shader);
    }

    public override void Render(PostProcessRenderContext context)
    {
        material.SetVector("_iResolution", new Vector4(Screen.width, Screen.height, 0, 0));
        context.command.Blit(context.source, context.destination, material);
    }
}