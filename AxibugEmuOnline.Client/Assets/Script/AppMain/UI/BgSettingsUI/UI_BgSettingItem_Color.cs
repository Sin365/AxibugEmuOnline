using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.UI;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Linq;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    /// <summary>
    /// 背景颜色设置UI
    /// </summary>
    public class UI_BgSettingItem_Color : MenuItem, IVirtualItem
    {
        public int Index { get; set; }
        public BgColorSettings Datacontext { get; private set; }

        public void SetData(object data)
        {
            Datacontext = (BgColorSettings)data;
            Datacontext.OnColorChanged += Setting_OnColorChanged;

            UpdateView();
        }

        private void Setting_OnColorChanged(XMBColor color)
        {
            UpdateView();
        }

        private void UpdateView()
        {
            var color = Datacontext.CurrentColor;
            Icon.GetMaterial().SetColor("_Color1", color.color1);
            Icon.GetMaterial().SetColor("_Color2", color.color2);
            SetBaseInfo("主题色", "设置主题色", color.Name);
        }

        public void SetDependencyProperty(object data)
        {
            SetSelectState(data is ThirdMenuRoot tr && tr.SelectIndex == Index);
        }

        public void Release()
        {
            Datacontext.OnColorChanged -= Setting_OnColorChanged;
        }
        public override bool OnEnterItem()
        {
            var options = Datacontext.Presets.Select(preset => new ColorOption(preset)).ToList();
            var currentColor = Datacontext.CurrentColor;
            var index = options.FindIndex(op => op.Color.GetHashCode() == currentColor.GetHashCode());
            OverlayManager.Pop(options, Mathf.Clamp(index, 0, options.Count - 1));
            return false;
        }

        public class ColorOption : ExecuteMenu
        {
            public XMBColor Color;

            public ColorOption(XMBColor color) : base(color.Name)
            {
                Color = color;
            }

            public override void OnShow(OptionUI_MenuItem ui)
            {
                ui.IconUI.gameObject.SetActiveEx(true);
                ui.IconUI.SetMaterial(Resources.Load<Material>("Materials/XMBBackGroundPreview"));
                ui.IconUI.GetMaterial().SetColor("_Color1", Color.color1);
                ui.IconUI.GetMaterial().SetColor("_Color2", Color.color2);
            }

            private static TweenerCore<float, float, FloatOptions> s_colorChangeTween;
            public override void OnFocus()
            {
                float progress = 0;
                XMBColor start = App.settings.BgColor.CurrentColor;
                XMBColor endColor = Color;

                if (s_colorChangeTween != null)
                {
                    s_colorChangeTween.Kill();
                    s_colorChangeTween = null;
                }
                s_colorChangeTween = DOTween.To(() => progress, (x) =>
                {
                    progress = x;
                    var lerpColor = XMBColor.Lerp(start, endColor, x);
                    App.settings.BgColor.CurrentColor = lerpColor;
                }, 1, 1f).SetEase(Ease.OutCubic);
                s_colorChangeTween.onComplete = () =>
                {
                    s_colorChangeTween = null;
                };
            }

            public override void OnExcute(OptionUI optionUI, ref bool cancelHide) { }
        }
    }
}
