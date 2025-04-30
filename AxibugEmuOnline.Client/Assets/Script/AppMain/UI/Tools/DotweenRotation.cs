using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace AxibugEmuOnline.Client.Tools
{
    public class DotweenRotation : MonoBehaviour
    {
        [SerializeField]
        float m_duration = 1f;
        [SerializeField]
        Ease m_ease = Ease.Linear;
        [SerializeField]
        bool m_reverseRotation;

        private TweenerCore<Quaternion, Vector3, QuaternionOptions> m_tween;

        void OnEnable()
        {
            Restart();
        }

        private void Restart()
        {
            if (m_tween != null)
            {
                m_tween.Kill();
                m_tween = null;
            }

            transform.localRotation = Quaternion.identity;
            m_tween = transform.DOLocalRotate(new Vector3(0, 0, 360f * (m_reverseRotation ? -1 : 1)), m_duration, RotateMode.LocalAxisAdd)
                .SetEase(m_ease)
                .SetLoops(-1);
            m_tween.SetLink(gameObject);
        }

        private void OnDisable()
        {
            if (m_tween != null)
            {
                m_tween.Kill();
                m_tween = null;
            }
        }

        private void OnValidate()
        {
            Restart();
        }
    }
}
