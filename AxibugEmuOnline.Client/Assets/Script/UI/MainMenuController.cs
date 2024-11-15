using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client.UI
{
    public class MainMenuController : MenuItemController<MenuData>
    {
        [SerializeField]
        HorizontalLayoutGroup GroupRoot;
        [SerializeField]
        MenuItem Template;
        [SerializeField]
        List<MenuData> MenuSetting;
        [SerializeField]
        float HoriRollSpd = 1f;
        [SerializeField]
        int InitSelect = -1;

        private RectTransform groupRootRect => m_menuItemRoot as RectTransform;

        private TweenerCore<Vector2, Vector2, VectorOptions> rollTween;
        private List<CanvasGroup> m_runtimeMenuUICanvas;
        private Sequence seq;


        protected override void Start()
        {
            base.Start();

            m_runtimeMenuUICanvas = m_runtimeMenuUI.Select(menu => menu.gameObject.AddComponent<CanvasGroup>()).ToList();
            m_runtimeMenuUICanvas.ForEach(canv => canv.gameObject.AddComponent<AutoRaycastCanvasGroup>());

            if (InitSelect != -1)
            {
                m_selectIndex = InitSelect;
                PlaySelectItemAnim(false);
            }
        }
        public override void Init(List<MenuData> menuDataList) { }

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
            PlaySelectItemAnim(true);
        }

        private void PlaySelectItemAnim(bool useAnim)
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

            if (useAnim)
            {
                rollTween = DOTween.To(
                    () => groupRootRect.anchoredPosition,
                    (x) => groupRootRect.anchoredPosition = x,
                    targetPosition,
                    HoriRollSpd)
                    .SetSpeedBased();
            }
            else
            {
                groupRootRect.anchoredPosition = targetPosition;
            }
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
                var settingData = MenuSetting[i];

                var templatePrefab = settingData.OverrideTemplate != null ? settingData.OverrideTemplate.gameObject : Template.gameObject;
                MenuItem itemScript = null;
                var prefabClone = UnityEditor.PrefabUtility.InstantiatePrefab(templatePrefab) as GameObject;
                itemScript = prefabClone.GetComponent<MenuItem>();
                itemScript.gameObject.SetActive(true);
                itemScript.transform.SetParent(GroupRoot.transform);

                itemScript.SetData(settingData);

                itemScript.transform.localScale = Vector3.one;
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
        public string SubTitle;
        public string Description;
        public MenuItem OverrideTemplate;
        public List<MenuData> SubMenus;
    }
}
