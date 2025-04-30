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
        private TweenerCore<Vector3, Vector3, VectorOptions> m_trackTween;

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
                m_rect.sizeDelta = itemUIRect.rect.size;
                m_rect.SetAsLastSibling();

                animator.SetTrigger("reactive");

                if (m_trackTween != null)
                {
                    m_trackTween.Kill();
                    m_trackTween = null;
                }
                m_trackTween = DOTween.To(() => m_rect.position, (_value) => m_rect.position = _value, itemUIRect.position, 0.125f);
                m_trackTween.onComplete = () => m_trackTween = null;
            }
        }

        private bool m_active;
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

        private void LateUpdate()
        {
            if (m_trackTween != null && m_trackTween.endValue != Target.position)
            {
                m_trackTween.ChangeEndValue(Target.position, true);
            }
        }
    }
}
