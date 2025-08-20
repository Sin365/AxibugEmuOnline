using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public class PopTipsItem : MonoBehaviour
    {
        [SerializeField] Text m_msgText;

        public RectTransform RectTransform => transform as RectTransform;

        public float OutTime = 0.235f;
        public Ease OutEase = Ease.OutQuint;
        public float StandTime = 1f;
        public float InTime = 0.235f;
        public Ease InEase = Ease.OutQuint;

        public void Popout(string msg)
        {
            gameObject.SetActiveEx(true);
            m_msgText.text = msg;
            Canvas.ForceUpdateCanvases();

            var targetPos = new Vector2(-RectTransform.rect.width - 20, RectTransform.anchoredPosition.y);
            DOTween.To(
                () => RectTransform.anchoredPosition,
                (value) => RectTransform.anchoredPosition = value,
                targetPos,
                OutTime)
                .SetLink(gameObject)
                .SetEase(OutEase)
                .OnComplete(() =>
                {
                    DOTween.To(
                        () => RectTransform.anchoredPosition,
                        (value) => RectTransform.anchoredPosition = value,
                        new Vector2(0, RectTransform.anchoredPosition.y),
                        InTime)
                    .SetDelay(StandTime)
                    .SetEase(InEase)
                    .OnComplete(() => gameObject.SetActiveEx(false))
                    .SetLink(gameObject);
                });
        }
    }
}
