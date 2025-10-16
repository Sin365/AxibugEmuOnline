using AxibugEmuOnline.Client.ClientCore;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class OptionUI : CommandExecuter
    {
        [SerializeField]
        RectTransform MenuRoot;
        [SerializeField]
        Selector SelectBorder;

        Dictionary<Type, OptionUI_MenuItem> m_menuUI_templates = new Dictionary<Type, OptionUI_MenuItem>();

        private OptionUI m_child;
        private OptionUI m_parent;

        public override bool AloneMode => true;
        public override bool Enable => m_bPoped && (!m_child || !m_child.m_bPoped);

        private bool m_bPoped;
        private readonly List<OptionUI_MenuItem> m_runtimeMenuItems = new List<OptionUI_MenuItem>();

        private int m_selectIndex = -1;
        public int SelectIndex
        {
            get => m_selectIndex;
            set
            {
                value = Mathf.Clamp(value, 0, m_runtimeMenuItems.Count - 1);
                if (m_selectIndex == value) return;

                var gap = value - m_selectIndex;

                var temp = value;
                while (!m_runtimeMenuItems[value].Visible)
                {
                    if (gap > 0)
                    {
                        temp++;
                        if (temp >= m_runtimeMenuItems.Count) return;
                    }
                    else
                    {
                        temp--;
                        if (temp < 0) return;
                    }

                    if (temp >= 0 && temp < m_runtimeMenuItems.Count)
                        value = temp;
                }

                m_selectIndex = value;

                OptionUI_MenuItem optionUI_MenuItem = m_runtimeMenuItems[m_selectIndex];
                optionUI_MenuItem.OnFocus();
                var itemUIRect = optionUI_MenuItem.transform as RectTransform;
                SelectBorder.Target = itemUIRect;
            }
        }

        protected override void Awake()
        {
            foreach (var templateIns in GetComponentsInChildren<OptionUI_MenuItem>(true))
            {
                m_menuUI_templates[templateIns.GetType()] = templateIns;
                templateIns.gameObject.SetActiveEx(false);
            }

            SelectBorder.gameObject.SetActiveEx(false);
            base.Awake();
        }

        protected override void Update()
        {
            SelectBorder.Active = Enable;
            UpdateMenuState();

            base.Update();
        }

        private void UpdateMenuState()
        {
            if (!m_bPoped) return;

            if (checkDirty())
            {
                Canvas.ForceUpdateCanvases();

                if (m_popTween != null)
                {
                    Vector2 end = new Vector2(-MenuRoot.rect.width, MenuRoot.anchoredPosition.y);
                    m_popTween.ChangeEndValue(end, false);
                }
                else
                {
                    Vector2 end = new Vector2(-MenuRoot.rect.width, MenuRoot.anchoredPosition.y);
                    var topParent = m_parent;
                    while (topParent != null && topParent.m_parent != null)
                    {
                        topParent = topParent.m_parent;
                    }
                    if (topParent != null)
                    {
                        topParent.MenuRoot.anchoredPosition = end;
                    }
                    else
                    {
                        MenuRoot.anchoredPosition = end;
                    }
                }
                RebuildSelectIndex();
            }
        }

        private void RebuildSelectIndex()
        {
            Canvas.ForceUpdateCanvases();

            SelectIndex = Mathf.Clamp(SelectIndex, 0, m_runtimeMenuItems.Count - 1);
            var selectItem = m_runtimeMenuItems[SelectIndex];

            if (selectItem.Visible == false)
            {
                bool find = false;
                int currentSelect = SelectIndex;
                while (currentSelect > 0)
                {
                    currentSelect--;
                    if (m_runtimeMenuItems[currentSelect].Visible)
                    {
                        find = true;
                    }
                }
                if (!find)
                {
                    currentSelect = SelectIndex;
                    while (currentSelect < m_runtimeMenuItems.Count)
                    {
                        if (m_runtimeMenuItems[currentSelect].Visible)
                        {
                            find = true;
                            break;
                        }
                        currentSelect++;
                    }
                }

                if (find)
                    SelectIndex = currentSelect;
            }
            else
            {
                var itemUIRect = selectItem.transform as RectTransform;
                SelectBorder.Target = itemUIRect;
            }
        }

        private bool checkDirty()
        {
            bool dirty = false;
            foreach (var menuItem in m_runtimeMenuItems)
            {
                if (menuItem.gameObject.activeSelf != menuItem.Visible)
                {
                    dirty = true;
                    menuItem.gameObject.SetActive(menuItem.Visible);
                }
            }

            return dirty;
        }

        private TweenerCore<Vector2, Vector2, VectorOptions> m_popTween;
        private TweenerCore<Vector2, Vector2, VectorOptions> m_hideTween;
        CommandListener.ScheduleType? m_lastCS;
        private Action m_onClose;

        /// <summary>
        /// 当菜单弹出时,动态添加一个菜单选项
        /// </summary>
        /// <param name="menu"></param>
        public void AddOptionMenuWhenPoping(InternalOptionMenu menu)
        {
            if (!m_bPoped) return;

            CreateRuntimeMenuItem(menu);
            Canvas.ForceUpdateCanvases();

            OptionUI_MenuItem optionUI_MenuItem = m_runtimeMenuItems[m_selectIndex];
            SelectBorder.Target = null;
            SelectBorder.Target = optionUI_MenuItem.transform as RectTransform;
        }


        public void Pop<T>(List<T> menus, int defaultIndex = 0, Action onClose = null) where T : InternalOptionMenu
        {
            if (m_hideTween != null)
            {
                m_hideTween.Kill(true);
            }
            if (menus.Count == 0) return;

            m_onClose = onClose;
            ReleaseRuntimeMenus();
            foreach (var menu in menus) CreateRuntimeMenuItem(menu);
            CommandDispatcher.Instance.RegistController(this);
            SelectBorder.gameObject.SetActiveEx(true);

            Canvas.ForceUpdateCanvases();

            m_selectIndex = defaultIndex;
            OptionUI_MenuItem optionUI_MenuItem = m_runtimeMenuItems[defaultIndex];
            optionUI_MenuItem.OnFocus();

            var itemUIRect = optionUI_MenuItem.transform as RectTransform;
            SelectBorder.Target = itemUIRect;
            SelectBorder.RefreshPosition();

            if (!m_bPoped)
            {

                m_bPoped = true;
                Vector2 start = new Vector2(0, MenuRoot.anchoredPosition.y);
                Vector2 end = new Vector2(-MenuRoot.rect.width, MenuRoot.anchoredPosition.y);
                m_popTween = DOTween.To(
                    () => start,
                    (value) =>
                    {
                        var moveDelta = value - start;
                        start = value;

                        var topParent = m_parent;
                        while (topParent != null && topParent.m_parent != null)
                        {
                            topParent = topParent.m_parent;
                        }
                        if (topParent != null)
                        {
                            topParent.MenuRoot.anchoredPosition += moveDelta;
                        }
                        else
                        {
                            MenuRoot.anchoredPosition += moveDelta;
                        }
                    },
                    end,
                    0.3f
                    ).SetEase(Ease.OutCubic);
                m_popTween.onComplete = () => m_popTween = null;

                m_lastCS = CommandDispatcher.Instance.Mode;
                CommandDispatcher.Instance.Mode = CommandListener.ScheduleType.Normal;
            }

        }

        /// <summary>
        /// 关闭这个侧边栏选项UI
        /// </summary>
        public void Hide()
        {
            if (m_bPoped)
            {
                if (m_child != null)
                {
                    m_child.Hide();
                }

                Vector2 start = new Vector2(-MenuRoot.rect.width, MenuRoot.anchoredPosition.y);
                Vector2 end = new Vector2(0, MenuRoot.anchoredPosition.y);

                SelectBorder.gameObject.SetActiveEx(false);

                CommandDispatcher.Instance.UnRegistController(this);
                Canvas.ForceUpdateCanvases();

                if (m_popTween != null)
                {
                    m_popTween.Kill(true);
                }

                m_hideTween = DOTween.To(
                    () => start,
                    (value) =>
                    {
                        var moveDelta = value - start;
                        start = value;

                        var topParent = m_parent;
                        while (topParent != null && topParent.m_parent != null)
                        {
                            topParent = topParent.m_parent;
                        }
                        if (topParent != null)
                        {
                            topParent.MenuRoot.anchoredPosition += moveDelta;
                        }
                        else
                        {
                            MenuRoot.anchoredPosition += moveDelta;
                        }
                    },
                    end,
                    0.3f
                    ).SetEase(Ease.OutCubic);
                m_hideTween.onComplete = () =>
                {
                    ReleaseRuntimeMenus();
                    m_runtimeMenuItems.Clear();
                    m_hideTween = null;
                };

                m_bPoped = false;

                CommandDispatcher.Instance.Mode = m_lastCS.Value;

                m_onClose?.Invoke();
                m_onClose = null;
            }
        }

        private void CreateRuntimeMenuItem(InternalOptionMenu menuData)
        {
            m_menuUI_templates.TryGetValue(menuData.MenuUITemplateType, out var template);
            if (template == null)
            {
                throw new NotImplementedException($"{menuData.GetType().Name}指定的MenuUI类型实例未在OptionUI中找到");
            }

            var menuUI = Instantiate(template.gameObject, template.transform.parent).GetComponent<OptionUI_MenuItem>();
            menuUI.gameObject.SetActive(true);
            menuUI.SetData(this, menuData);
            m_runtimeMenuItems.Add(menuUI);
        }

        private void ReleaseRuntimeMenus()
        {
            foreach (var item in m_runtimeMenuItems)
            {
                item.OnHide();
                Destroy(item.gameObject);
            }
            m_runtimeMenuItems.Clear();
        }

        protected override void OnCmdSelectItemDown()
        {
            int old = SelectIndex;
            SelectIndex++;
            if (old != SelectIndex)
                App.audioMgr.PlaySFX(AudioMgr.E_SFXTYPE.Option);
        }

        protected override void OnCmdSelectItemUp()
        {
            int old = SelectIndex;
            SelectIndex--;
            if (old != SelectIndex)
                App.audioMgr.PlaySFX(AudioMgr.E_SFXTYPE.Option);
        }

        protected override void OnCmdBack()
        {
            Hide();
            App.audioMgr.PlaySFX(AudioMgr.E_SFXTYPE.Cancel);
        }

        protected override void OnCmdSelectItemLeft()
        {
            var executer = m_runtimeMenuItems[SelectIndex];
            if (executer)
            {
                executer.OnLeft();
            }
        }

        protected override void OnCmdSelectItemRight()
        {
            var executer = m_runtimeMenuItems[SelectIndex];
            if (!executer.IsExpandMenu)
            {
                executer.OnRight();
                return;
            }

            OnCmdEnter();
        }

        protected override bool OnCmdEnter()
        {
            var executer = m_runtimeMenuItems[SelectIndex];
            bool cancelHide = false;
            executer.OnExecute(this, ref cancelHide);
            if (!cancelHide) Hide();
            App.audioMgr.PlaySFX(AudioMgr.E_SFXTYPE.Option);
            return false;
        }

        /// <summary>
        /// 展开下级菜单
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="menus"></param>
        /// <param name="defaultIndex"></param>
        /// <param name="onClose"></param>
        public void ExpandSubMenu<T>(List<T> menus, int defaultIndex = 0, Action onClose = null) where T : InternalOptionMenu
        {
            if (m_child == null)
            {
                var sourcePrefab = Resources.Load<GameObject>("UIPrefabs/OptionUI");
                m_child = Instantiate(sourcePrefab, transform).GetComponent<OptionUI>();
                m_child.name = $"{name}_Sub";
                m_child.m_parent = this;
            }

            Canvas.ForceUpdateCanvases();

            m_child.Pop(menus, 0, onClose);
        }

        public void RemoveItem(OptionUI_MenuItem ui)
        {
            var index = m_runtimeMenuItems.IndexOf(ui);
            if (index == -1) return;

            m_runtimeMenuItems.Remove(ui);
            ui.OnHide();
            Destroy(ui.gameObject);

            RebuildSelectIndex();
        }
    }

    /// <summary>
    /// 带有执行行为的菜单
    /// </summary>
    public abstract class ExecuteMenu : InternalOptionMenu
    {
        public override Type MenuUITemplateType => typeof(OptionUI_ExecuteItem);
        /// <summary> 设置这个值以控制菜单中显示"已应用"标记 </summary>
        public virtual bool IsApplied { get; }

        public abstract void OnExcute(OptionUI optionUI, ref bool cancelHide);
    }

    /// <summary>
    /// 带有展开行为的可执行菜单
    /// </summary>
    public abstract class ExpandMenu : ExecuteMenu
    {
        protected ExpandMenu() : base() { }

        public sealed override void OnExcute(OptionUI optionUI, ref bool cancelHide)
        {
            cancelHide = true;
            optionUI.ExpandSubMenu(GetOptionMenus());
        }

        protected abstract List<InternalOptionMenu> GetOptionMenus();
    }
    /// <summary>
    /// 带有值类型显示和编辑的菜单
    /// </summary>
    public abstract class ValueSetMenu : InternalOptionMenu
    {
        public override Type MenuUITemplateType => typeof(OptionUI_ValueEditItem);
        protected ValueSetMenu() : base() { }

        public abstract Type ValueType { get; }
        public abstract object ValueRaw { get; }
        public abstract void OnValueChanged(object newValue);
        public abstract object Min { get; }
        public abstract object Max { get; }
    }

    /// <summary> 不要直接继承这个类 </summary>
    public abstract class InternalOptionMenu
    {
        public abstract Type MenuUITemplateType { get; }
        public abstract string Name { get; }
        public virtual Sprite Icon { get; }
        public virtual bool Visible => true;

        public virtual void OnFocus() { }
        public virtual void OnShow(OptionUI_MenuItem ui) { }
        public virtual void OnHide() { }
    }
}
