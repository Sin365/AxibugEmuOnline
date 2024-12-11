using DG.Tweening;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public abstract class OverlayUI : CommandExecuter
    {
        public override bool AloneMode => true;
        public override bool Enable => true;

        public float StartAlpha = 0;
        public float StartScale = 1.2f;
        public float Duration = 0.5f;
        public Ease Ease;

        [SerializeField]
        protected Transform m_root;
        [SerializeField]
        protected CanvasGroup m_cg;

        protected override void OnEnable()
        {
            base.OnEnable();

            float progress = 0;
            DOTween.To(() => progress, (x) =>
            {
                progress = x;
                m_cg.alpha = Mathf.Lerp(StartAlpha, 1, x);
                m_root.localScale = Vector3.Lerp(Vector3.one * StartScale, Vector3.one, x);
            }, 1, Duration).SetEase(Ease).SetLink(gameObject, LinkBehaviour.KillOnDisable);

            CommandDispatcher.Instance.RegistController(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            CommandDispatcher.Instance.UnRegistController(this);
        }

        public void Show(object param)
        {
            gameObject.SetActive(true);
            OnShow(param);
        }
        public void Close()
        {
            OnClose();
            gameObject.SetActive(false);
        }
        protected abstract void OnShow(object param);
        protected virtual void OnClose() { }
    }
}
