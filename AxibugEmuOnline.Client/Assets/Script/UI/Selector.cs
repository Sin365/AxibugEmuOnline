using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class Selector : MonoBehaviour
    {
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

                //重置选择游标的动画
                gameObject.SetActive(false);
                gameObject.SetActive(true);

                var itemUIRect = m_target.transform as RectTransform;
                m_rect.pivot = itemUIRect.pivot;
                m_rect.sizeDelta = itemUIRect.rect.size;
                m_rect.SetAsLastSibling();

                if (m_trackTween != null)
                {
                    m_trackTween.Kill();
                    m_trackTween = null;
                }
                m_trackTween = DOTween.To(() => m_rect.position, (value) => m_rect.position = value, itemUIRect.position, 0.125f);
                m_trackTween.onComplete = () => m_trackTween = null;
            }
        }

        private void LateUpdate()
        {
            if (m_trackTween != null)
            {
                m_trackTween.endValue = Target.position;
            }
            if (Target == null) return;

            m_rect.position = Target.position;
        }
    }
}
