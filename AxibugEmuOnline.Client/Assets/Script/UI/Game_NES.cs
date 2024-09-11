using AxibugEmuOnline.Client.UI;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using App = AxibugEmuOnline.Client.ClientCore.AppAxibugEmuOnline;

namespace AxibugEmuOnline.Client
{
    public class Game_NES : MenuItem
    {
        [SerializeField]
        CanvasGroup RomGroupRoot;
        private TweenerCore<float, float, FloatOptions> m_showTween;

        private void Awake()
        {
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
            App.nesRomLib.FetchRomCount((roms) =>
            {
                var thirdMenuGroup = SubMenuItemGroup as ThirdMenuRoot;
                thirdMenuGroup.itemGroup.UpdateDependencyProperty(thirdMenuGroup);
                thirdMenuGroup.itemGroup.SetData(roms);
                thirdMenuGroup.itemGroup.UpdateProxyVisualState();
                thirdMenuGroup.SelectIndex = 0;
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
