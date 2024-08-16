using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client.UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField]
        HorizontalLayoutGroup GroupRoot;
        [SerializeField]
        MenuItem Template;
        [SerializeField]
        List<MainMenuData> MenuSetting;
        [SerializeField]
        float HoriRollSpd = 1f;

        private RectTransform groupRootRect => GroupRoot.transform as RectTransform;
        private List<MenuItem> m_runtimeMenuUI = new List<MenuItem>();

        private int m_selectIndex;
        private TweenerCore<Vector2, Vector2, VectorOptions> rollTween;

        public int SelectIndex
        {
            get => m_selectIndex;
            set
            {
                value = Mathf.Clamp(value, 0, m_runtimeMenuUI.Count - 1);
                m_selectIndex = value;

                var step = GroupRoot.spacing;
                var needSelectItem = m_runtimeMenuUI[value];
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
                        foreach (var item in m_runtimeMenuUI)
                        {
                            var distance = Mathf.Abs(item.Rect.anchoredPosition.x + groupRootRect.anchoredPosition.x);
                            var selectProg = (1 - distance / 200f);

                            SetSelectProgress(item, selectProg);
                        }
                    });
            }
        }

        private void SetSelectProgress(MenuItem item, float selectProg)
        {
            item.ControlSelectProgress(selectProg);
        }

        private void Start()
        {
            for (int i = 0; i < GroupRoot.transform.childCount; i++)
            {
                Transform child = GroupRoot.transform.GetChild(i);
                m_runtimeMenuUI.Add(child.GetComponent<MenuItem>());
            }

            Canvas.ForceUpdateCanvases();
            SelectIndex = 0;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.D))
                SelectIndex += 1;
            else if (Input.GetKeyDown(KeyCode.A))
                SelectIndex -= 1;
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
    public class MainMenuData
    {
        public Sprite Icon;
        public string Name;
    }
}
