using Assets.Script.AppMain.Filter;
using AxibugEmuOnline.Client;
using UnityEngine;
using UnityEngine.Rendering;

namespace AxibugEmuOnline.Client.Filters
{
    public sealed class MattiasCRT : FilterEffect
    {
        public override string Name => nameof(MattiasCRT);

        protected override string ShaderName => "Filter/MattiasCRT";

        public FilterParameter<EnumQuality> Quality = new FilterParameter<EnumQuality>(EnumQuality.High);
        private LocalKeyword _kw_qualityLow;
        private LocalKeyword _kw_qualityMid;
        private LocalKeyword _kw_qualityHigh;

        protected override void OnInit(Material renderMat)
        {
            _kw_qualityLow = new LocalKeyword(renderMat.shader, "_QUALITY_LOW");
            _kw_qualityMid = new LocalKeyword(renderMat.shader, "_QUALITY_MID");
            _kw_qualityHigh = new LocalKeyword(renderMat.shader, "_QUALITY_HIGH");
        }

        protected override void OnRenderer(Material renderMat, Texture src, RenderTexture result)
        {
            renderMat.DisableKeyword(_kw_qualityLow);
            renderMat.DisableKeyword(_kw_qualityMid);
            renderMat.DisableKeyword(_kw_qualityHigh);
            switch (Quality.GetValue())
            {
                case EnumQuality.Low: renderMat.EnableKeyword(_kw_qualityLow); break;
                case EnumQuality.Mid: renderMat.EnableKeyword(_kw_qualityMid); break;
                case EnumQuality.High: renderMat.EnableKeyword(_kw_qualityHigh); break;
            }

            Graphics.Blit(src, result, renderMat);
        }

        public enum EnumQuality
        {
            Low,
            Mid,
            High
        }
    }
}