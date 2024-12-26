using Assets.Script.AppMain.Filter;
using AxibugEmuOnline.Client;
using UnityEngine;

[Strip(RuntimePlatform.PSP2)]
public sealed class FixingPixelArtGrille : FilterEffect
{
    public override string Name => nameof(FixingPixelArtGrille);

    protected override string ShaderName => "PostEffect/FixingPixcelArtGrille";
    public FilterParameter<EnumMaskStyle> MaskStyle = new FilterParameter<EnumMaskStyle>(EnumMaskStyle.ApertureGrille);
    public Vector2Parameter DrawResolution = new Vector2Parameter(new Vector2(272, 240));
    [Range(-32, 0)]
    public FloatParameter HardScan = new FloatParameter(-10);
    [Range(-6, 0)]
    public FloatParameter HardPix = new FloatParameter(-2);
    [Range(-8, 0)]
    public FloatParameter HardBloomScan = new FloatParameter(-4.0f);
    [Range(-4, 0)]
    public FloatParameter HardBloomPix = new FloatParameter(-1.5f);
    [Range(0, 1)]
    public FloatParameter BloomAmount = new FloatParameter(1 / 16f);
    public Vector2Parameter Warp = new Vector2Parameter(new Vector2(1f / 64f, 1f / 24f));
    [Range(1, 3)]
    public FloatParameter MaskLight = new FloatParameter(1.5f);
    [Range(0.1f, 1)]
    public FloatParameter MaskDrak = new FloatParameter(0.5f);

    public enum EnumMaskStyle
    {
        TVStyle,
        ApertureGrille,
        StretchedVGA,
        VGAStyle
    }


    protected override void OnRenderer(Material renderMat, Texture src, RenderTexture result)
    {
        renderMat.SetVector("_iResolution", new Vector4(result.width, result.height, 0, 0));
        renderMat.SetVector("_res", new Vector4(DrawResolution.GetValue().x, DrawResolution.GetValue().y, 0, 0));
        renderMat.SetFloat("_hardScan", HardScan.GetValue());
        renderMat.SetFloat("_hardPix", HardPix.GetValue());
        renderMat.SetFloat("_hardBloomScan", HardBloomScan.GetValue());
        renderMat.SetFloat("_hardBloomPix", HardBloomPix.GetValue());
        renderMat.SetFloat("_bloomAmount", BloomAmount.GetValue());
        renderMat.SetVector("_warp", Warp.GetValue());
        renderMat.SetFloat("_maskDark", MaskDrak.GetValue());
        renderMat.SetFloat("_maskLight", MaskLight.GetValue());

        renderMat.DisableKeyword("_MASKSTYLE_VGASTYLE");
        renderMat.DisableKeyword("_MASKSTYLE_TVSTYLE");
        renderMat.DisableKeyword("_MASKSTYLE_APERTUREGRILLE");
        renderMat.DisableKeyword("_MASKSTYLE_STRETCHEDVGA");

        switch (MaskStyle.GetValue())
        {
            case EnumMaskStyle.VGAStyle:
                renderMat.EnableKeyword("_MASKSTYLE_VGASTYLE");
                break;
            case EnumMaskStyle.TVStyle:
                renderMat.EnableKeyword("_MASKSTYLE_TVSTYLE");
                break;
            case EnumMaskStyle.ApertureGrille:
                renderMat.EnableKeyword("_MASKSTYLE_APERTUREGRILLE");
                break;
            case EnumMaskStyle.StretchedVGA:
                renderMat.EnableKeyword("_MASKSTYLE_STRETCHEDVGA");
                break;
        }
        Graphics.Blit(src, result, renderMat);
    }
}