using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client.UI
{
    public class MenuItem : MonoBehaviour
    {
        [SerializeField]
        Image Icon;
        [SerializeField]
        Text Txt;
        [SerializeField]
        Text Descript;
        [SerializeField]
        Transform Root;
        [SerializeField]
        Image ShadowIcon;
        [SerializeField]
        CanvasGroup InfoNode;
        [SerializeField]
        SubMenuItemGroup SubMenuItemGroup;

        public float SelectScale = 1f;
        public float UnSelectScale = 0.85f;

        public RectTransform Rect => transform as RectTransform;

        bool m_select;
        TweenerCore<float, float, FloatOptions> progressTween;
        float m_progress;
        private void Awake()
        {
            m_select = false;
            m_progress = 0f;

            if (ShadowIcon != null) ShadowIcon.gameObject.SetActive(false);

            InfoNode.alpha = 0;
            if (ShadowIcon != null) ShadowIcon.gameObject.SetActiveEx(false);
            if (SubMenuItemGroup != null) SubMenuItemGroup.SetSelect(false);
        }

        public void SetData(MenuData data)
        {
            SetBaseInfo(data.Name, data.Description, data.Icon);
            if (SubMenuItemGroup != null) SubMenuItemGroup.Init(data.SubMenus);
        }

        protected void SetBaseInfo(string name, string descript, Sprite icon)
        {
            this.name = name;

            if (Icon != null) Icon.sprite = icon;
            if (ShadowIcon != null) ShadowIcon.sprite = icon;
            if (Txt != null) Txt.text = name;
            if (Descript != null) Descript.text = descript;

        }

        public void SetSelectState(bool selected)
        {
            if (m_select == selected) return;

            m_select = selected;

            if (ShadowIcon != null) ShadowIcon.gameObject.SetActiveEx(selected);
            if (SubMenuItemGroup != null) SubMenuItemGroup.SetSelect(selected);

            if (progressTween != null) { progressTween.Kill(); progressTween = null; }

            progressTween = DOTween.To(() => m_progress, (x) => m_progress = x, m_select ? 1 : 0, 5)
                .SetSpeedBased().OnUpdate(() =>
                {
                    InfoNode.alpha = m_progress;

                    Root.localScale = Vector3.one * Mathf.Lerp(UnSelectScale, SelectScale, m_progress);
                });
        }

        public virtual void OnEnterItem() { }

        public virtual void OnExitItem() { }
    }
}
