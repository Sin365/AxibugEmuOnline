using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[System.Serializable]
[PostProcess(typeof(FixingPixelArtGrilleRenderer), PostProcessEvent.BeforeStack, "Filter/FixingPixelArtGrille")]
public sealed class FixingPixelArtGrille : PostProcessEffectSettings
{
    // 在这里可以添加效果的参数
}

public sealed class FixingPixelArtGrilleRenderer : PostProcessEffectRenderer<FixingPixelArtGrille>
{
    private Shader shader;
    private Material material;

    public override void Init()
    {
        shader = Shader.Find("PostEffect/FixingPixcelArtGrille");
        material = new Material(shader);
    }

    public override void Render(PostProcessRenderContext context)
    {
        context.command.Blit(context.source, context.destination, material);
    }
}