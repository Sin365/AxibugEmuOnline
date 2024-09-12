using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class OptionUI : MonoBehaviour
    {
        public static OptionUI Instance { get; private set; }

        [SerializeField]
        RectTransform MenuRoot;

        private bool m_bPoped = false;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Canvas.ForceUpdateCanvases();
            var width = MenuRoot.rect.size.x;
            var temp = MenuRoot.anchoredPosition;
            temp.x = -width;
        }

        public void Pop(IEnumerable<OptionMenu> menus)
        {
        }

        public void Hide()
        {

        }
    }

    public abstract class OptionMenu
    {
        public string Name { get; protected set; }

        public OptionMenu(string name)
        {
            Name = name;
        }
    }

    public abstract class ExecuteMenu : OptionMenu
    {
        protected ExecuteMenu(string name) : base(name) { }

        public abstract void OnExcute();
    }

    public abstract class ValueSetMenu : OptionMenu
    {
        protected ValueSetMenu(string name) : base(name) { }

        public abstract Type ValueType { get; }
        public abstract object ValueRaw { get; }
        public abstract void OnValueChanged(object newValue);
    }

    public abstract class ValueSetMenu<T> : ValueSetMenu
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
