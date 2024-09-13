using AxibugEmuOnline.Client.UI;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using App = AxibugEmuOnline.Client.ClientCore.App;

namespace AxibugEmuOnline.Client
{
    public class RomListMenuItem : MenuItem
    {
        [SerializeField]
        CanvasGroup RomGroupRoot;
        [SerializeField]
        EnumPlatform Platform;

        private TweenerCore<float, float, FloatOptions> m_showTween;

        private RomLib RomLib
        {
            get
            {
                switch (Platform)
                {
                    case EnumPlatform.NES:
                        return App.nesRomLib;
                    default:
                        throw new System.NotImplementedException($"未实现的平台 {Platform}");
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();

            RomGroupRoot.gameObject.SetActive(false);
            RomGroupRoot.alpha = 0;
        }

        public override void SetSelectState(bool selected)
        {
            if (m_select == selected) return;

            m_select = selected;

            if (ShadowIcon != null) ShadowIcon.gameObject.SetActiveEx(selected);

            if (progressTween != null) { progressTween.Kill(); progressTween = null; }

            progressTween = DOTween.To(() => m_progress, (x) => m_progress = x, m_select ? 1 : 0, 5)
                .SetSpeedBased().OnUpdate(() =>
                {
                    if (InfoNode != null) InfoNode.alpha = m_progress;

                    Root.localScale = Vector3.one * Mathf.Lerp(UnSelectScale, SelectScale, m_progress);
                });
        }

        public override bool OnEnterItem()
        {
            RomGroupRoot.gameObject.SetActive(true);
            RomGroupRoot.alpha = 0;

            if (m_showTween != null)
            {
                m_showTween.Kill();
                m_showTween = null;
            }
            m_showTween = DOTween.To(() => RomGroupRoot.alpha, (x) => RomGroupRoot.alpha = x, 1, 0.2f);

            var thirdMenuGroup = SubMenuItemGroup as ThirdMenuRoot;
            thirdMenuGroup.itemGroup.Clear();
            RomLib.FetchRomCount((roms) =>
            {
                var thirdMenuGroup = SubMenuItemGroup as ThirdMenuRoot;
                thirdMenuGroup.itemGroup.UpdateDependencyProperty(thirdMenuGroup);
                thirdMenuGroup.itemGroup.SetData(roms);
                thirdMenuGroup.itemGroup.UpdateProxyVisualState();
                thirdMenuGroup.ResetToFirst();
            });

            if (SubMenuItemGroup != null) SubMenuItemGroup.SetSelect(true);

            return true;
        }

        public override bool OnExitItem()
        {
            if (m_showTween != null)
            {
                m_showTween.Kill();
                m_showTween = null;
            }
            m_showTween = DOTween.To(() => RomGroupRoot.alpha, (x) => RomGroupRoot.alpha = x, 0, 0.2f)
                .OnComplete(() =>
            {
                RomGroupRoot.gameObject.SetActive(false);
            });

            if (SubMenuItemGroup != null) SubMenuItemGroup.SetSelect(false);

            return true;
        }
    }
}
