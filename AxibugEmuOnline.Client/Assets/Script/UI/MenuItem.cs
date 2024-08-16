using Coffee.UIExtensions;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;
using static Codice.Client.BaseCommands.Import.Commit;

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
        SubMenuItemGroup SubMenuItemGroup;

        public float SelectScale = 1f;
        public float UnSelectScale = 0.85f;

        public RectTransform Rect => transform as RectTransform;

        private bool m_select;
        private TweenerCore<float, float, FloatOptions> progressTween;
        public float m_progress;
        private void Awake()
        {
            m_select = false;
            m_progress = 0f;

            if (ShadowIcon != null) ShadowIcon.gameObject.SetActive(false);

            var temp = Txt.color;
            temp.a = 0;
            Txt.color = temp;
            if (Descript != null)
            {
                temp = Descript.color;
                temp.a = 0;
                Descript.color = temp;
            }
            if (ShadowIcon != null) ShadowIcon.gameObject.SetActiveEx(false);
            if (SubMenuItemGroup != null) SubMenuItemGroup.SetSelect(false);
        }

        public void SetData(MenuData data)
        {
            Icon.sprite = data.Icon;

            if (ShadowIcon != null) ShadowIcon.sprite = data.Icon;

            Txt.text = data.Name;
            if (Descript != null) Descript.text = data.Description;

            if (SubMenuItemGroup != null)
                SubMenuItemGroup.Init(data.SubMenus);
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
                    var temp = Txt.color;
                    temp.a = m_progress;
                    Txt.color = temp;
                    if (Descript != null)
                    {
                        temp = Descript.color;
                        temp.a = m_progress;
                        Descript.color = temp;
                    }

                    Root.localScale = Vector3.one * Mathf.Lerp(UnSelectScale, SelectScale, m_progress);
                });
        }
    }
}
