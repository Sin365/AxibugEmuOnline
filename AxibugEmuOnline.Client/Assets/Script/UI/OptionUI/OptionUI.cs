using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
        [Header("Ä£°å")]
        [SerializeField] OptionUI_ExecuteItem TEMPLATE_EXECUTEITEM;

        public override bool AloneMode => true;
        public override bool Enable => m_bPoped;

        private bool m_bPoped = false;
        private List<OptionUI_MenuItem> m_runtimeMenuItems = new List<OptionUI_MenuItem>();

        private int m_selectIndex = -1;
        public int SelectIndex
        {
            get { return m_selectIndex; }
            set
            {
                value = Mathf.Clamp(value, 0, m_runtimeMenuItems.Count - 1);
                if (m_selectIndex == value) return;

                m_selectIndex = value;

                OptionUI_MenuItem optionUI_MenuItem = m_runtimeMenuItems[m_selectIndex];
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

        public void Pop(List<OptionMenu> menus, int defaultIndex = 0)
        {
            ReleaseRuntimeMenus();
            foreach (var menu in menus) CreateRuntimeMenuItem(menu);
            CommandDispatcher.Instance.RegistController(this);
            SelectBorder.gameObject.SetActiveEx(true);

            Canvas.ForceUpdateCanvases();

            m_selectIndex = 0;
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
            }
        }

        public void Hide()
        {
            if (m_bPoped)
            {
                SelectBorder.gameObject.SetActiveEx(false);

                CommandDispatcher.Instance.UnRegistController(this);
                m_bPoped = false;
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
            m_runtimeMenuItems[SelectIndex].OnExecute();
            return true;
        }
    }

    public abstract class OptionMenu
    {
        public string Name { get; protected set; }
        public Sprite Icon { get; protected set; }
        public OptionMenu(string name, Sprite icon = null)
        {
            Name = name;
            Icon = icon;
        }
    }

    public class ExecuteMenu : OptionMenu
    {
        public ExecuteMenu(string name, Sprite icon = null) : base(name, icon) { }

        public virtual void OnExcute() { }
    }

    public abstract class ValueSetMenu : OptionMenu
    {
        public ValueSetMenu(string name) : base(name) { }

        public abstract Type ValueType { get; }
        public abstract object ValueRaw { get; }
        public abstract void OnValueChanged(object newValue);
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
}
