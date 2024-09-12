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

        [Space]
        [Header("Ä£°å")]
        [SerializeField] OptionUI_ExecuteItem TEMPLATE_EXECUTEITEM;

        private bool m_bPoped = false;
        private List<MonoBehaviour> m_runtimeMenuItems = new List<MonoBehaviour>();

        protected override void Awake()
        {
            Instance = this;
            TEMPLATE_EXECUTEITEM.gameObject.SetActiveEx(false);

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
            if (Input.GetKeyDown(KeyCode.T))
            {
                if (m_bPoped) Hide();
                else Pop(new List<OptionMenu>
                {
                    new ExecuteMenu("²âÊÔ²Ëµ¥1"),
                    new ExecuteMenu("Copilot"),
                    new ExecuteMenu("ChatGPT 4o"),
                });
            }
        }

        public void Pop(IEnumerable<OptionMenu> menus)
        {
            ReleaseRuntimeMenus();
            foreach (var menu in menus) CreateRuntimeMenuItem(menu);
            CommandDispatcher.Instance.RegistController(this);
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

        public override bool Enable => m_bPoped;

        protected override void OnSelectMenuChanged()
        {
            
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
