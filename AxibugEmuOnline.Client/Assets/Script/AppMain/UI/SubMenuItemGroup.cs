using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.UI;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class SubMenuItemGroup : MenuItemController<MenuData>
    {
        [SerializeField]
        MenuItem SubMenuItemTemplate;
        [SerializeField]
        CanvasGroup alphaGroup;
        private TweenerCore<float, float, FloatOptions> selectTween;

        private bool m_selected;

        private TweenerCore<int, int, NoOptions> rollTween;


        protected override void Awake()
        {
            m_selected = false;
            if (alphaGroup != null) alphaGroup.alpha = 0;

            base.Awake();
        }

        public override void Init(List<MenuData> menuDataList)
        {
#if UNITY_EDITOR
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
#else
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
#endif

            m_runtimeMenuUI.Clear();
            foreach (MenuData menuData in menuDataList)
            {
                var template = menuData.OverrideTemplate != null ? menuData.OverrideTemplate : SubMenuItemTemplate;

                var item = Clone(template, transform);
                item.SetData(menuData);
                m_runtimeMenuUI.Add(item);
            }

            calcItemPosition();

            for (var i = 0; i < m_runtimeMenuUI.Count; i++)
            {
                var item = m_runtimeMenuUI[i];
                var needPos = m_itemUIPosition[i];
                item.Rect.anchoredPosition = needPos;
            }
        }

        protected override bool OnCmdEnter()
        {
            base.OnCmdEnter();

            LaunchUI.Instance.ToDetailMenuLayout();
            var item = GetItemUIByIndex(SelectIndex);
            item.SetSelectState(false);
            App.audioMgr.PlaySFX(AudioMgr.E_SFXTYPE.Option);
            return true;
        }

        protected override void OnCmdBack()
        {
            base.OnCmdBack();

            LaunchUI.Instance.ToMainMenuLayout();
            var item = GetItemUIByIndex(SelectIndex);
            item.SetSelectState(true);
            //TODO 已经关闭三级菜单的情况下 不播放
            App.audioMgr.PlaySFX(AudioMgr.E_SFXTYPE.Cancel);
        }

        protected override void OnCmdSelectItemUp()
        {
            if (m_enteredItem == null)
            { 
                SelectIndex--;
                //TODO 已经到底的情况下，不播放音效
                App.audioMgr.PlaySFX(AudioMgr.E_SFXTYPE.Option);
            }
        }

        protected override void OnCmdSelectItemDown()
        {
            if (m_enteredItem == null)
            { 
                SelectIndex++;
                //TODO 已经到顶的情况下，不播放音效
                App.audioMgr.PlaySFX(AudioMgr.E_SFXTYPE.Option);
            }
        }

        public virtual void SetSelect(bool select)
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

            ListenControlAction = m_selected;
        }

        protected override void OnCmdSelectItemLeft()
        {
            base.OnCmdSelectItemLeft();
            OnCmdBack();
            App.audioMgr.PlaySFX(AudioMgr.E_SFXTYPE.Cancel);
        }

        protected override void OnSelectMenuChanged()
        {
            if (rollTween != null) { rollTween.Kill(); rollTween = null; }

            float duration = 0.5f;

            calcItemPosition();

            for (var i = 0; i < m_runtimeMenuUI.Count; i++)
            {
                var item = m_runtimeMenuUI[i];
                bool isSelectItem = i == SelectIndex;
                item.SetSelectState(isSelectItem);
            }

            rollTween = DOTween.To(() => 1, (x) => { }, 1, duration).OnUpdate(() =>
            {
                var tweenProgress = rollTween.position / rollTween.Duration();
                for (var i = 0; i < m_runtimeMenuUI.Count; i++)
                {
                    var item = m_runtimeMenuUI[i];
                    var needPos = m_itemUIPosition[i];
                    item.Rect.anchoredPosition = Vector2.Lerp(item.Rect.anchoredPosition, needPos, tweenProgress);
                }
            }).OnComplete(() =>
            {
                for (var i = 0; i < m_runtimeMenuUI.Count; i++)
                {
                    var item = m_runtimeMenuUI[i];
                    var needPos = m_itemUIPosition[i];
                    item.Rect.anchoredPosition = needPos;
                }
            });
        }

        [SerializeField]
        Vector2 m_selectItemPosition = new Vector2(50, -51);
        [SerializeField]
        float step = 50f;
        [SerializeField]
        float splitStep = 200f;

        List<Vector2> m_itemUIPosition = new List<Vector2>();
        private void calcItemPosition()
        {
            m_itemUIPosition.Clear();
            for (int i = 0; i < m_runtimeMenuUI.Count; i++)
            {
                var gap = SelectIndex - i;
                var start = m_selectItemPosition;
                start.y += step * gap;
                if (i < SelectIndex) start.y += splitStep;

                m_itemUIPosition.Add(start);
            }
        }

        private MenuItem Clone(MenuItem template, Transform parent)
        {
#if UNITY_EDITOR

            //========================套娃宏========================
#if UNITY_2019_1_OR_NEWER //新版Unity，因UNITY2018.2用不了这玩意儿，编辑器也罢

            if (Application.isPlaying)
            {
                var item = GameObject.Instantiate(template.gameObject, parent).GetComponent<MenuItem>();
                item.transform.localPosition = Vector3.zero;
                return item;
            }
            else
            {
                var clone = UnityEditor.PrefabUtility.InstantiatePrefab(template.gameObject, parent) as GameObject;
                clone.transform.localPosition = Vector3.zero;
                return clone.GetComponent<MenuItem>();
            }
#else
			var item = GameObject.Instantiate(template.gameObject, parent).GetComponent<MenuItem>();
			item.transform.localPosition = Vector3.zero;
			return item;
#endif
            //========================套娃宏 End========================



#else
            return GameObject.Instantiate(SubMenuItemTemplate.gameObject, parent).GetComponent<MenuItem>();
#endif
        }
    }
}
