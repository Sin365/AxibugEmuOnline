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
        ItemPresent RomGroup;
        [SerializeField]
        CanvasGroup RomGroupRoot;
        private TweenerCore<float, float, FloatOptions> m_showTween;

        private void Awake()
        {
            RomGroupRoot.gameObject.SetActive(false);
            RomGroupRoot.alpha = 0;
        }

        public override void OnEnterItem()
        {
            RomGroupRoot.gameObject.SetActive(true);
            RomGroupRoot.alpha = 0;

            if (m_showTween != null)
            {
                m_showTween.Kill();
                m_showTween = null;
            }
            m_showTween = DOTween.To(() => RomGroupRoot.alpha, (x) => RomGroupRoot.alpha = x, 1, 0.2f);

            App.nesRomLib.FetchRomCount((roms) =>
            {
                RomGroup.UpdateDependencyProperty(App.nesRomLib);
                RomGroup.SetData(roms);
            });
        }

        public override void OnExitItem()
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
        }
    }
}
