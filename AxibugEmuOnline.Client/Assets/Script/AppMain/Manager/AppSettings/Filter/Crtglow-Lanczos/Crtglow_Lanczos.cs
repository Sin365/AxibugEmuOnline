using System.Collections.Generic;
using UnityEngine;

[Strip(RuntimePlatform.PSP2)]
public class Crtglow_Lanczos : FilterChainEffect
{
    protected override void DefinePasses(ref List<PassDefine> passes)
    {
        passes.Add(PassDefine.Create(
                                        "Filter/RetroArch/Glow/Linearize",
                                         sRGB: true
                                    ));
        passes.Add(PassDefine.Create(
                                        "Filter/RetroArch/Glow/Lanczos_horiz",
                                         scaleModeX: EnumScaleMode.Viewport,
                                         scaleModeY: EnumScaleMode.Source,
                                         scaleX: 1f, scaleY: 1f,
                                         sRGB: true
                                    ));
        passes.Add(PassDefine.Create(
                                        "Filter/RetroArch/Glow/Gauss_vert",
                                        scaleModeX: EnumScaleMode.Viewport, scaleModeY: EnumScaleMode.Viewport,
                                        scaleX: 1f, scaleY: 1f,
                                        sRGB: true,
                                        outputAlias: "CRTPass"
                                    ));
        passes.Add(PassDefine.Create(
                                        "Filter/RetroArch/Glow/threshold",
                                        sRGB: true
                                    ));
        passes.Add(PassDefine.Create(
                                        "Filter/RetroArch/Glow/blur_horiz",
                                        filterMode: FilterMode.Bilinear,
                                        scaleModeX: EnumScaleMode.Source, scaleModeY: EnumScaleMode.Source,
                                        scaleX: 0.25f, scaleY: 0.25f,
                                        sRGB: true
                                    ));
        passes.Add(PassDefine.Create(
                                        "Filter/RetroArch/Glow/blur_vert",
                                        filterMode: FilterMode.Bilinear,
                                        sRGB: true
                                    ));
        passes.Add(PassDefine.Create(
                                        "Filter/RetroArch/Glow/blur_vert",
                                        filterMode: FilterMode.Bilinear,
                                        sRGB: true
                                    ));
        passes.Add(PassDefine.Create(
                                        "Filter/RetroArch/Glow/blur_vert",
                                        filterMode: FilterMode.Bilinear,
                                        scaleModeX: EnumScaleMode.Viewport, scaleModeY: EnumScaleMode.Viewport
                                    ));

    }

    public override string Name => "Crtglow-lanczos";
}
