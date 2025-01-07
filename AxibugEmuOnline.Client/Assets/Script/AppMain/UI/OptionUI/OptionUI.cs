using DG.Tweening;
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

        [Space]
        [Header("模板")]
        [SerializeField] OptionUI_ExecuteItem TEMPLATE_EXECUTEITEM;
        [SerializeField] OptionUI_ValueEditItem TEMPLATE_VALUEEDITITEM;

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

                while (!m_runtimeMenuItems[value].Visible)
                {
                    var temp = value;
                    if (gap > 0)
                    {
                        temp++;
                    }
                    else
                    {
                        temp--;
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
            TEMPLATE_EXECUTEITEM.gameObject.SetActiveEx(false);
            TEMPLATE_VALUEEDITITEM.gameObject.SetActiveEx(false);

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
            if (checkDirty())
            {
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

        IKeyMapperChanger m_lastCS;
        private Action m_onClose;

        /// <summary>
        /// 当菜单弹出时,动态添加一个菜单选项
        /// </summary>
        /// <param name="menu"></param>
        public void AddOptionMenuWhenPoping(OptionMenu menu)
        {
            if (!m_bPoped) return;

            CreateRuntimeMenuItem(menu);
            Canvas.ForceUpdateCanvases();

            OptionUI_MenuItem optionUI_MenuItem = m_runtimeMenuItems[m_selectIndex];
            SelectBorder.Target = null;
            SelectBorder.Target = optionUI_MenuItem.transform as RectTransform;
        }

        public void Pop<T>(List<T> menus, int defaultIndex = 0, Action onClose = null) where T : OptionMenu
        {
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
                DOTween.To(
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

                m_lastCS = CommandDispatcher.Instance.Current;
                CommandDispatcher.Instance.Current = CommandDispatcher.Instance.Normal;
            }

        }

        public void Hide()
        {
            if (m_bPoped)
            {
                Vector2 start = new Vector2(-MenuRoot.rect.width, MenuRoot.anchoredPosition.y);
                Vector2 end = new Vector2(0, MenuRoot.anchoredPosition.y);


                ReleaseRuntimeMenus();
                m_runtimeMenuItems.Clear();

                SelectBorder.gameObject.SetActiveEx(false);

                CommandDispatcher.Instance.UnRegistController(this);
                Canvas.ForceUpdateCanvases();

                DOTween.To(
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

                m_bPoped = false;

                CommandDispatcher.Instance.Current = m_lastCS;

                m_onClose?.Invoke();
                m_onClose = null;
            }
        }

        private void CreateRuntimeMenuItem(OptionMenu menuData)
        {
            if (menuData is ExecuteMenu)
            {
                ExecuteMenu executeMenu = (ExecuteMenu)menuData;
                var menuUI = Instantiate(TEMPLATE_EXECUTEITEM.gameObject, TEMPLATE_EXECUTEITEM.transform.parent).GetComponent<OptionUI_ExecuteItem>();
                menuUI.gameObject.SetActive(true);
                menuUI.SetData(this, executeMenu);
                m_runtimeMenuItems.Add(menuUI);
            }
            else if (menuData is ValueSetMenu)
            {
                var valueSetMenu = (ValueSetMenu)menuData;
                var menuUI = Instantiate(TEMPLATE_VALUEEDITITEM.gameObject, TEMPLATE_VALUEEDITITEM.transform.parent).GetComponent<OptionUI_ValueEditItem>();
                menuUI.gameObject.SetActive(true);
                menuUI.SetData(this, valueSetMenu);
                m_runtimeMenuItems.Add(menuUI);
            }
            else throw new NotImplementedException($"暂不支持的菜单类型{menuData.GetType().Name}");
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
            SelectIndex++;
        }

        protected override void OnCmdSelectItemUp()
        {
            SelectIndex--;
        }

        protected override void OnCmdBack()
        {
            Hide();
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

            return false;
        }

        /// <summary>
        /// 展开下级菜单
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="menus"></param>
        /// <param name="defaultIndex"></param>
        /// <param name="onClose"></param>
        public void ExpandSubMenu<T>(List<T> menus, int defaultIndex = 0, Action onClose = null) where T : OptionMenu
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
    public abstract class ExecuteMenu : OptionMenu
    {
        /// <summary> 设置这个值以控制菜单中显示"已应用"标记 </summary>
        public virtual bool IsApplied { get; }
        protected ExecuteMenu(string name, Sprite icon = null) : base(name, icon) { }

        public abstract void OnExcute(OptionUI optionUI, ref bool cancelHide);
    }

    /// <summary>
    /// 带有展开行为的可执行菜单
    /// </summary>
    public abstract class ExpandMenu : ExecuteMenu
    {
        protected ExpandMenu(string name, Sprite icon = null) : base(name, icon) { }

        public sealed override void OnExcute(OptionUI optionUI, ref bool cancelHide)
        {
            cancelHide = true;
            optionUI.ExpandSubMenu(GetOptionMenus());
        }

        protected abstract List<OptionMenu> GetOptionMenus();
    }
    /// <summary>
    /// 带有值类型显示和编辑的菜单
    /// </summary>
    public abstract class ValueSetMenu : OptionMenu
    {
        protected ValueSetMenu(string name) : base(name) { }

        public abstract Type ValueType { get; }
        public abstract object ValueRaw { get; }
        public abstract void OnValueChanged(object newValue);
        public abstract object Min { get; }
        public abstract object Max { get; }
    }

    /// <summary> 不要直接继承这个类 </summary>
    public abstract class OptionMenu
    {
        public string Name { get; protected set; }
        public Sprite Icon { get; protected set; }
        public virtual bool Visible => true;
        public virtual bool Enable => true;

        protected OptionMenu(string name, Sprite icon = null)
        {
            Name = name;
            Icon = icon;
        }

        public virtual void OnFocus() { }
        public virtual void OnShow(OptionUI_MenuItem ui) { }
        public virtual void OnHide() { }
    }

}
