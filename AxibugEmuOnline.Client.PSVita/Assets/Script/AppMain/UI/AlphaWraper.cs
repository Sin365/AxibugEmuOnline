using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class AlphaWraper
    {
        private bool m_on;
        private CanvasGroup m_offUI;
        private CanvasGroup m_onUI;
        private TweenerCore<float, float, FloatOptions> m_onTween;
        private TweenerCore<float, float, FloatOptions> m_offTween;

        public bool On
        {
            get => m_on;
            set
            {
                if (m_on == value) return;

                m_on = value;

                if (m_onTween != null)
                {
                    m_onTween.Kill();
                    m_onTween = null;
                }
                if (m_offTween != null)
                {
                    m_offTween.Kill();
                    m_offTween = null;
                }
                m_onUI.gameObject.SetActiveEx(true);
                m_offUI.gameObject.SetActiveEx(true);

                if (On)
                {
                    float progress = 0f;
                    m_onTween = DOTween.To(() => progress, (x) =>
                    {
                        progress = x;
                        m_onUI.alpha = progress;
                        m_offUI.alpha = 1 - progress;
                    }, 1f, 0.3f);
                    m_onTween.onComplete = () =>
                    {
                        m_offUI.gameObject.SetActiveEx(false);
                    };
                }
                else
                {
                    float progress = 0f;
                    m_offTween = DOTween.To(() => progress, (x) =>
                    {
                        progress = x;
                        m_onUI.alpha = 1 - progress;
                        m_offUI.alpha = progress;
                    }, 1f, 0.3f);
                    m_offTween.onComplete = () =>
                    {
                        m_onUI.gameObject.SetActiveEx(false);
                    };
                }
            }
        }

        public AlphaWraper(CanvasGroup offUI, CanvasGroup onUI, bool defaultOn)
        {
            m_offUI = offUI;
            m_onUI = onUI;

            m_on = defaultOn;
            if (On)
            {
                onUI.alpha = 1;
                onUI.gameObject.SetActiveEx(true);
                offUI.alpha = 0;
                offUI.gameObject.SetActiveEx(false);
            }
            else
            {
                onUI.alpha = 0;
                onUI.gameObject.SetActiveEx(false);
                offUI.alpha = 1;
                offUI.gameObject.SetActiveEx(true);
            }
        }
    }
}
