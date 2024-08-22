using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static GluonGui.WorkspaceWindow.Views.Checkin.Operations.CheckinViewDeleteOperation;

namespace AxibugEmuOnline.Client.UI
{
    public class MainMenuController : MenuItemController
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
        private List<CanvasGroup> m_runtimeMenuUICanvas;
        private Sequence seq;

        protected override void Start()
        {
            base.Start();

            m_runtimeMenuUICanvas = m_runtimeMenuUI.Select(menu => menu.gameObject.AddComponent<CanvasGroup>()).ToList();
        }


        public void EnterDetailState()
        {
            if (seq != null)
            {
                seq.Kill();
                seq = null;
            }

            var selectItem = m_runtimeMenuUICanvas[SelectIndex];
            var hideItem = m_runtimeMenuUICanvas.Where(i => i != selectItem).ToList();
            seq = DOTween.Sequence();

            seq.Append(
                DOTween.To(() => selectItem.alpha, (x) => selectItem.alpha = x, 1, 0.2f)
            )
            .Join(
               DOTween.To(() => hideItem[0].alpha, (x) => hideItem.ForEach(i => i.alpha = x), 0, 0.2f)
            );

            seq.Play();

        }

        public void ExitDetailState()
        {
            if (seq != null)
            {
                seq.Kill();
                seq = null;
            }

            var selectItem = m_runtimeMenuUICanvas[SelectIndex];
            var hideItem = m_runtimeMenuUICanvas.Where(i => i != selectItem).ToList();
            seq = DOTween.Sequence();

            seq.Append(
                DOTween.To(() => selectItem.alpha, (x) => selectItem.alpha = x, 1, 0.2f)
            )
            .Join(
               DOTween.To(() => hideItem[0].alpha, (x) => hideItem.ForEach(i => i.alpha = x), 1, 0.2f)
            );

            seq.Play();
        }

        protected override void OnSelectMenuChanged()
        {
            var step = GroupRoot.spacing;
            var needSelectItem = m_runtimeMenuUI[SelectIndex];
            var offset = needSelectItem.Rect.anchoredPosition.x;

            var targetPosition = groupRootRect.anchoredPosition;
            targetPosition.x = -offset;

            if (rollTween != null) { rollTween.Kill(); rollTween = null; }

            for (var i = 0; i < m_runtimeMenuUI.Count; i++)
            {
                var item = m_runtimeMenuUI[i];
                item.SetSelectState(i == SelectIndex);
            }

            rollTween = DOTween.To(
                () => groupRootRect.anchoredPosition,
                (x) => groupRootRect.anchoredPosition = x,
                targetPosition,
                HoriRollSpd)
                .SetSpeedBased();
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
