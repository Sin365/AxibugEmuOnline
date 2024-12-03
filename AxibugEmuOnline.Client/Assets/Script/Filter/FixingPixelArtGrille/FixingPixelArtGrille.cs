using AxibugEmuOnline.Client;
using AxibugEmuOnline.Client.ClientCore;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[System.Serializable]
[PostProcess(typeof(FixingPixelArtGrilleRenderer), PostProcessEvent.BeforeStack, "Filter/FixingPixelArtGrille")]
public sealed class FixingPixelArtGrille : FilterEffect
{
    public override string Name => nameof(FixingPixelArtGrille);

    public ParameterOverride<EnumMaskStyle> MaskStyle = new ParameterOverride<EnumMaskStyle> { value = EnumMaskStyle.ApertureGrille };

    [Tooltip("Emulated input resolution\nOptimize for resize")]
    public Vector2Parameter DrawResolution = new Vector2Parameter
    {
        value = new Vector2(272, 240)
    };

    [Tooltip("Hardness of scanline")]
    [Range(-32, 0)]
    public FloatParameter HardScan = new FloatParameter { value = -10 };

    [Tooltip("Hardness of pixels in scanline")]
    [Range(-6, 0)]
    public FloatParameter HardPix = new FloatParameter { value = -2 };

    [Tooltip("Hardness of short vertical bloom")]
    [Range(-8, 0)]
    public FloatParameter HardBloomScan = new FloatParameter { value = -4.0f };

    [Tooltip("Hardness of short horizontal bloom")]
    [Range(-4, 0)]
    public FloatParameter HardBloomPix = new FloatParameter { value = -1.5f };

    [Tooltip("Amount of small bloom effect")]
    [Range(0, 1)]
    public FloatParameter BloomAmount = new FloatParameter { value = 1 / 16f };

    [Tooltip("Display warp")]
    public Vector2Parameter Warp = new Vector2Parameter { value = new Vector2(1f / 64f, 1f / 24f) };

    [Tooltip("Amount of shadow mask Light")]
    [Range(1, 3)]
    public FloatParameter MaskLight = new FloatParameter { value = 1.5f };
    [Range(0.1f, 1)]
    [Tooltip("Amount of shadow mask Dark")]
    public FloatParameter MaskDrak = new FloatParameter { value = 0.5f };

    public enum EnumMaskStyle
    {
        TVStyle,
        ApertureGrille,
        StretchedVGA,
        VGAStyle
    }
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
        material.SetVector("_iResolution", new Vector4(Screen.width, Screen.height, 0, 0));
        var res = settings.DrawResolution;
        material.SetVector("_res", new Vector4(res.value.x, res.value.y, 0, 0));
        material.SetFloat("_hardScan", settings.HardScan.value);
        material.SetFloat("_hardPix", settings.HardPix.value);
        material.SetFloat("_hardBloomScan", settings.HardBloomScan.value);
        material.SetFloat("_hardBloomPix", settings.HardBloomPix.value);
        material.SetFloat("_bloomAmount", settings.BloomAmount.value);
        material.SetVector("_warp", settings.Warp.value);
        material.SetFloat("_maskDark", settings.MaskDrak.value);
        material.SetFloat("_maskLight", settings.MaskLight.value);

        material.DisableKeyword("_MASKSTYLE_VGASTYLE");
        material.DisableKeyword("_MASKSTYLE_TVSTYLE");
        material.DisableKeyword("_MASKSTYLE_APERTUREGRILLE");
        material.DisableKeyword("_MASKSTYLE_STRETCHEDVGA");

        switch (settings.MaskStyle.value)
        {
            case FixingPixelArtGrille.EnumMaskStyle.VGAStyle:
                material.EnableKeyword("_MASKSTYLE_VGASTYLE");
                break;
            case FixingPixelArtGrille.EnumMaskStyle.TVStyle:
                material.EnableKeyword("_MASKSTYLE_TVSTYLE");
                break;
            case FixingPixelArtGrille.EnumMaskStyle.ApertureGrille:
                material.EnableKeyword("_MASKSTYLE_APERTUREGRILLE");
                break;
            case FixingPixelArtGrille.EnumMaskStyle.StretchedVGA:
                material.EnableKeyword("_MASKSTYLE_STRETCHEDVGA");
                break;
        }

        context.command.Blit(context.source, context.destination, material);
    }
}