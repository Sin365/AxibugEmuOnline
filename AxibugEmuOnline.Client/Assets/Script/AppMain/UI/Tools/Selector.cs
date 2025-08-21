using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    /// <summary>
    /// 选择指示器,用于控制RectTransform在屏幕坐标上的高宽和位置同步,同时带有过度动画
    /// </summary>
    public class Selector : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;
        private RectTransform m_rect => transform as RectTransform;

        private RectTransform m_target;

        public RectTransform Target
        {
            get => m_target;
            set
            {
                if (m_target == value) return;

                m_target = value;
                if (m_target == null)
                {
                    if (m_trackTween != null)
                    {
                        m_trackTween.Kill();
                        m_trackTween = null;
                    }
                    return;
                }

                var itemUIRect = m_target.transform as RectTransform;
                m_rect.pivot = itemUIRect.pivot;
                m_rect.SetAsLastSibling();

                animator.SetTrigger("reactive");

                if (m_trackTween != null)
                {
                    m_trackTween.Kill();
                    m_trackTween = null;
                }

                var startSize = m_rect.sizeDelta;
                var startPos = m_rect.position;

                m_trackTween = DOTween.To(
                    () => 0f,
                    (_value) =>
                    {
                        var progress = _value;
                        m_rect.position = Vector3.Lerp(startPos, itemUIRect.position, progress);
                        m_rect.sizeDelta = Vector2.Lerp(startSize, itemUIRect.rect.size, progress);
                    },
                    1f,
                    0.125f);
                m_trackTween.onComplete = () =>
                {
                    m_trackTween = null;
                };
            }
        }

        private bool m_active;
        private TweenerCore<float, float, FloatOptions> m_trackTween;

        public bool Active
        {
            get => m_active;
            set
            {
                if (m_active == value) return;
                m_active = value;

                animator.SetBool("active", value);
            }
        }

        public void RefreshPosition()
        {
            if (Target != null)
            {
                m_rect.position = Target.position;
            }
        }

        struct TrackTarget
        {
            Vector3 pos;
            Vector2 size;
        }

        struct TrackTargetOption : IPlugOptions
        {
            public void Reset()
            {
            }
        }
    }
}
