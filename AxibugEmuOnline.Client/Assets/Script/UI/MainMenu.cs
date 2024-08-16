using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client.UI
{
    public class MainMenu : MenuItemController
    {
        [SerializeField]
        HorizontalLayoutGroup GroupRoot;
        [SerializeField]
        MenuItem Template;
        [SerializeField]
        List<MenuData> MenuSetting;
        [SerializeField]
        float HoriRollSpd = 1f;

        private RectTransform groupRootRect => m_menuItemRoot as RectTransform;

        private TweenerCore<Vector2, Vector2, VectorOptions> rollTween;

        protected override void OnSelectMenuChanged()
        {
            var step = GroupRoot.spacing;
            var needSelectItem = m_runtimeMenuUI[SelectIndex];
            var offset = needSelectItem.Rect.anchoredPosition.x;

            var targetPosition = groupRootRect.anchoredPosition;
            targetPosition.x = -offset;

            if (rollTween != null) { rollTween.Kill(); rollTween = null; }

            rollTween = DOTween.To(
                () => groupRootRect.anchoredPosition,
                (x) => groupRootRect.anchoredPosition = x,
                targetPosition,
                HoriRollSpd)
                .SetSpeedBased().OnUpdate(() =>
                {
                    for (var i = 0; i < m_runtimeMenuUI.Count; i++)
                    {
                        var item = m_runtimeMenuUI[i];
                        item.SetSelectState(i == SelectIndex);
                    }
                });
        }

        protected override void OnCmdSelectItemLeft()
        {
            SelectIndex--;
        }

        protected override void OnCmdSelectItemRight()
        {
            SelectIndex++;
        }

#if UNITY_EDITOR
        [ContextMenu("UpdateMenuUI")]
        public void UpdateMenuUI()
        {
            while (GroupRoot.transform.childCount > 0)
            {
                DestroyImmediate(GroupRoot.transform.GetChild(GroupRoot.transform.childCount - 1).gameObject);
            }

            for (int i = 0; i < MenuSetting.Count; i++)
            {
                MenuItem itemScript = null;
                var prefabClone = UnityEditor.PrefabUtility.InstantiatePrefab(Template.gameObject) as GameObject;
                itemScript = prefabClone.GetComponent<MenuItem>();
                itemScript.gameObject.SetActive(true);
                itemScript.transform.SetParent(GroupRoot.transform);
                itemScript.transform.localScale = Vector3.one;

                itemScript.SetData(MenuSetting[i]);
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }

    [Serializable]
    public class MenuData
    {
        public Sprite Icon;
        public string Name;
        public string Description;

        public List<MenuData> SubMenus;
    }
}
