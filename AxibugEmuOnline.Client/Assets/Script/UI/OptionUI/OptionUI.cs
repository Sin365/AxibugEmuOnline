using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Graphs.Styles;

namespace AxibugEmuOnline.Client
{
    public class OptionUI : CommandExecuter
    {
        public static OptionUI Instance { get; private set; }

        [SerializeField]
        RectTransform MenuRoot;
        [SerializeField]
        RectTransform SelectBorder;

        [Space]
        [Header("模板")]
        [SerializeField] OptionUI_ExecuteItem TEMPLATE_EXECUTEITEM;

        public override bool AloneMode => true;
        public override bool Enable => m_bPoped;

        private bool m_bPoped = false;
        private List<OptionUI_MenuItem> m_runtimeMenuItems = new List<OptionUI_MenuItem>();

        public event Action OnHide;

        private int m_selectIndex = -1;
        public int SelectIndex
        {
            get { return m_selectIndex; }
            set
            {
                var selectableItems = m_runtimeMenuItems.Where(t => t.Visible).ToList();
                value = Mathf.Clamp(value, 0, selectableItems.Count - 1);
                if (m_selectIndex == value) return;

                m_selectIndex = value;

                OptionUI_MenuItem optionUI_MenuItem = selectableItems[m_selectIndex];
                optionUI_MenuItem.OnFocus();
                var itemUIRect = optionUI_MenuItem.transform as RectTransform;
                SelectBorder.pivot = itemUIRect.pivot;
                SelectBorder.sizeDelta = itemUIRect.rect.size;
                DOTween.To(() => SelectBorder.position, (value) => SelectBorder.position = value, itemUIRect.position, 0.125f);
                SelectBorder.SetAsLastSibling();
            }
        }

        protected override void Awake()
        {
            Instance = this;
            TEMPLATE_EXECUTEITEM.gameObject.SetActiveEx(false);
            SelectBorder.gameObject.SetActiveEx(false);
            base.Awake();
        }

        private void Start()
        {
            Canvas.ForceUpdateCanvases();
            var width = MenuRoot.rect.size.x;
            var temp = MenuRoot.anchoredPosition;
            temp.x = width;
            MenuRoot.anchoredPosition = temp;
        }

        protected override void Update()
        {
            UpdateMenuState();
        }

        private void UpdateMenuState()
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
            if (dirty)
            {
                Canvas.ForceUpdateCanvases();

                if (m_runtimeMenuItems[SelectIndex].Visible == false)
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
                    var selectItem = m_runtimeMenuItems[SelectIndex];
                    var itemUIRect = selectItem.transform as RectTransform;
                    SelectBorder.pivot = itemUIRect.pivot;
                    SelectBorder.position = itemUIRect.position;
                    SelectBorder.sizeDelta = itemUIRect.rect.size;
                }
            }
        }

        ControlScheme m_lastCS;
        public void Pop<T>(List<T> menus, int defaultIndex = 0) where T : OptionMenu
        {
            ReleaseRuntimeMenus();
            foreach (var menu in menus) CreateRuntimeMenuItem(menu);
            CommandDispatcher.Instance.RegistController(this);
            SelectBorder.gameObject.SetActiveEx(true);

            Canvas.ForceUpdateCanvases();

            m_selectIndex = defaultIndex;
            OptionUI_MenuItem optionUI_MenuItem = m_runtimeMenuItems[defaultIndex];
            optionUI_MenuItem.OnFocus();
            var itemUIRect = optionUI_MenuItem.transform as RectTransform;
            SelectBorder.pivot = itemUIRect.pivot;
            SelectBorder.position = itemUIRect.position;
            SelectBorder.sizeDelta = itemUIRect.rect.size;
            SelectBorder.SetAsLastSibling();

            if (!m_bPoped)
            {
                m_bPoped = true;

                DOTween.To(
                    () => MenuRoot.anchoredPosition.x,
                    (value) =>
                    {
                        var temp = MenuRoot.anchoredPosition;
                        temp.x = value;
                        MenuRoot.anchoredPosition = temp;
                    },
                    0,
                    0.3f
                    ).SetEase(Ease.OutCubic);

                m_lastCS = ControlScheme.Current;
                ControlScheme.Current = ControlSchemeSetts.Normal;
            }

        }

        public void Hide()
        {
            if (m_bPoped)
            {
                ReleaseRuntimeMenus();
                m_runtimeMenuItems.Clear();

                SelectBorder.gameObject.SetActiveEx(false);

                CommandDispatcher.Instance.UnRegistController(this);
                Canvas.ForceUpdateCanvases();
                var width = MenuRoot.rect.width;
                DOTween.To(
                    () => MenuRoot.anchoredPosition.x,
                    (value) =>
                    {
                        var temp = MenuRoot.anchoredPosition;
                        temp.x = value;
                        MenuRoot.anchoredPosition = temp;
                    },
                    width,
                    0.3f
                    ).SetEase(Ease.OutCubic);

                m_bPoped = false;

                ControlScheme.Current = m_lastCS;

                OnHide?.Invoke();
            }
        }

        private void CreateRuntimeMenuItem(OptionMenu menuData)
        {
            if (menuData is ExecuteMenu executeMenu)
            {
                var menuUI = GameObject.Instantiate(TEMPLATE_EXECUTEITEM.gameObject, TEMPLATE_EXECUTEITEM.transform.parent).GetComponent<OptionUI_ExecuteItem>();
                menuUI.gameObject.SetActive(true);
                menuUI.SetData(executeMenu);
                m_runtimeMenuItems.Add(menuUI);
            }
            else
            {
                throw new NotImplementedException($"暂不支持的菜单类型{menuData.GetType().Name}");
            }
        }

        private void ReleaseRuntimeMenus()
        {
            foreach (var item in m_runtimeMenuItems)
            {
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

        protected override bool OnCmdEnter()
        {
            var executer = m_runtimeMenuItems[SelectIndex];
            Hide();
            executer.OnExecute();
            return true;
        }
    }

    public abstract class OptionMenu
    {
        public string Name { get; protected set; }
        public Sprite Icon { get; protected set; }
        public virtual bool Visible => true;
        public virtual bool Enable => true;

        public OptionMenu(string name, Sprite icon = null)
        {
            Name = name;
            Icon = icon;
        }

        public virtual void OnFocus() { }
        public virtual void OnShow(OptionUI_MenuItem ui) { }
    }

    public abstract class ExecuteMenu : OptionMenu
    {
        public ExecuteMenu(string name, Sprite icon = null) : base(name, icon) { }

        public abstract void OnExcute();
    }

    public class ValueSetMenu<T> : ValueSetMenu
    {
        public sealed override Type ValueType => typeof(T);

        public T Value { get; private set; }

        public sealed override object ValueRaw => Value;

        public sealed override void OnValueChanged(object newValue)
        {
            Value = (T)newValue;
        }
        protected ValueSetMenu(string name) : base(name) { }
    }

    public abstract class ValueSetMenu : OptionMenu
    {
        public ValueSetMenu(string name) : base(name) { }

        public abstract Type ValueType { get; }
        public abstract object ValueRaw { get; }
        public abstract void OnValueChanged(object newValue);
    }


}
