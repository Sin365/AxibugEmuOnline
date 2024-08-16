using AxibugEmuOnline.Client.UI;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public class SubMenuItemGroup : MenuItemController
    {
        [SerializeField]
        MenuItem SubMenuItemTemplate;
        [SerializeField]
        CanvasGroup alphaGroup;
        private TweenerCore<float, float, FloatOptions> selectTween;

        private bool m_selected;

        private TweenerCore<int, int, NoOptions> rollTween;


        private void Awake()
        {
            m_selected = false;
            alphaGroup.alpha = 0;
        }

        public void Init(List<MenuData> menuDataList)
        {
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            foreach (MenuData menuData in menuDataList)
            {
                Clone(transform).SetData(menuData);
            }
        }

        public void SetSelect(bool select)
        {
            if (m_selected == select) return;
            m_selected = select;

            if (selectTween != null)
            {
                selectTween.Kill();
                selectTween = null;
            }

            if (select)
            {
                selectTween = DOTween.To(() => alphaGroup.alpha, (x) => alphaGroup.alpha = x, 1, 10).SetSpeedBased();
            }
            else
            {
                selectTween = DOTween.To(() => alphaGroup.alpha, (x) => alphaGroup.alpha = x, 0, 10).SetSpeedBased();
            }
        }

        protected override void OnSelectMenuChanged()
        {
            if (rollTween != null) { rollTween.Kill(); rollTween = null; }

            rollTween = DOTween.To(() => 1, (x) => { }, 1, 1).OnUpdate(() =>
            {
                for (var i = 0; i < m_runtimeMenuUI.Count; i++)
                {
                    var item = m_runtimeMenuUI[i];
                    bool isSelectItem = i == SelectIndex;
                    item.SetSelectState(isSelectItem);
                }
            });
        }

        private MenuItem Clone(Transform parent)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                return GameObject.Instantiate(SubMenuItemTemplate.gameObject, parent).GetComponent<MenuItem>();
            }
            else
            {
                var clone = UnityEditor.PrefabUtility.InstantiatePrefab(SubMenuItemTemplate.gameObject, parent) as GameObject;
                return clone.GetComponent<MenuItem>();
            }
#else
                return GameObject.Instantiate(SubMenuItemTemplate.gameObject, parent).GetComponent<MenuItem>();
#endif
        }
    }
}
