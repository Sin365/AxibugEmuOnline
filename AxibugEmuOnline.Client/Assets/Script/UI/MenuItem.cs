using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client.UI
{
    public class MenuItem : MonoBehaviour
    {
        [SerializeField]
        protected Image Icon;
        [SerializeField]
        protected Text Txt;
        [SerializeField]
        protected Text Descript;
        [SerializeField]
        protected Transform Root;
        [SerializeField]
        protected Image ShadowIcon;
        [SerializeField]
        protected CanvasGroup InfoNode;
        [SerializeField]
        protected SubMenuItemGroup SubMenuItemGroup;

        public float SelectScale = 1f;
        public float UnSelectScale = 0.85f;

        public RectTransform Rect => transform as RectTransform;

        protected bool m_select;
        protected TweenerCore<float, float, FloatOptions> progressTween;
        protected float m_progress;

        protected virtual void Awake()
        {
            Reset();
        }

        protected virtual void OnDestroy()
        {
            
        }

        public void SetData(MenuData data)
        {
            Reset();

            SetBaseInfo(data.Name, data.Description);
            SetIcon(data.Icon);
            if (SubMenuItemGroup != null) SubMenuItemGroup.Init(data.SubMenus);
        }

        protected void Reset()
        {
            m_select = false;
            m_progress = 0f;

            Root.localScale = Vector3.one * UnSelectScale;
            if (progressTween != null) { progressTween.Kill(); progressTween = null; }

            if (ShadowIcon != null) ShadowIcon.gameObject.SetActive(false);

            if (InfoNode != null) InfoNode.alpha = 0;
            if (ShadowIcon != null) ShadowIcon.gameObject.SetActiveEx(false);
            if (SubMenuItemGroup != null) SubMenuItemGroup.SetSelect(false);
        }

        protected void SetBaseInfo(string name, string descript)
        {
            this.name = name;

            if (Txt != null) Txt.text = name;
            if (Descript != null) Descript.text = descript;
        }

        protected void SetIcon(Sprite icon)
        {
            if (Icon != null) Icon.sprite = icon;
            if (ShadowIcon != null) ShadowIcon.sprite = icon;
        }

        public virtual void SetSelectState(bool selected)
        {
            if (m_select == selected) return;

            m_select = selected;

            if (ShadowIcon != null) ShadowIcon.gameObject.SetActiveEx(selected);
            if (SubMenuItemGroup != null)
            {
                SubMenuItemGroup.SetSelect(selected);
            }

            if (progressTween != null) { progressTween.Kill(); progressTween = null; }

            progressTween = DOTween.To(() => m_progress, (x) => m_progress = x, m_select ? 1 : 0, 5)
                .SetSpeedBased().OnUpdate(() =>
                {
                    if (InfoNode != null) InfoNode.alpha = m_progress;

                    Root.localScale = Vector3.one * Mathf.Lerp(UnSelectScale, SelectScale, m_progress);
                });
        }

        public virtual bool OnEnterItem() => true;

        public virtual bool OnExitItem() => true;
    }
}
