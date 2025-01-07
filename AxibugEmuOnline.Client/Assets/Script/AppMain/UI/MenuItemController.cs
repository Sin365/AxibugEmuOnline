using AxibugEmuOnline.Client.UI;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public abstract class MenuItemController : CommandExecuter
    {
        [SerializeField]
        protected Transform m_menuItemRoot;
        protected List<MenuItem> m_runtimeMenuUI = new List<MenuItem>();

        protected MenuItem m_enteredItem = null;

        protected int m_selectIndex = -1;

        public virtual int SelectIndex
        {
            get => m_selectIndex;
            set
            {
                value = Mathf.Clamp(value, 0, m_runtimeMenuUI.Count - 1);
                if (m_selectIndex == value) return;
                m_selectIndex = value;

                OnSelectMenuChanged();
            }
        }

        protected virtual void Start()
        {
            if (m_menuItemRoot != null)
            {
                for (int i = 0; i < m_menuItemRoot.childCount; i++)
                {
                    Transform child = m_menuItemRoot.GetChild(i);
                    m_runtimeMenuUI.Add(child.GetComponent<MenuItem>());
                }
            }

            Canvas.ForceUpdateCanvases();
            if (m_selectIndex == -1) SelectIndex = 0;
        }

        protected virtual MenuItem GetItemUIByIndex(int index)
        {
            return m_runtimeMenuUI[SelectIndex];
        }

        protected override bool OnCmdEnter()
        {
            if (m_enteredItem == null)
            {
                var willEnterItem = GetItemUIByIndex(SelectIndex);
                bool res = willEnterItem.OnEnterItem();
                if (res)
                {
                    m_enteredItem = willEnterItem;
                }
                return res;
            }

            return false;
        }

        protected override void OnCmdBack()
        {
            if (m_enteredItem != null)
            {
                m_enteredItem.OnExitItem();
                m_enteredItem = null;
            }
        }

        protected abstract void OnSelectMenuChanged();
    }

    public abstract class MenuItemController<T> : MenuItemController
    {
        public override bool Enable => enabled && ListenControlAction;

        private bool m_listenControlAction;
        public bool ListenControlAction
        {
            get => m_listenControlAction;
            set
            {
                m_listenControlAction = value;

                if (value)
                    CommandDispatcher.Instance.RegistController(this);
                else
                    CommandDispatcher.Instance.UnRegistController(this);

                if (!value)
                    ResetPulsInvoker();
            }
        }

        public abstract void Init(List<T> menuDataList);


        protected override void OnDestroy()
        {
            if (CommandDispatcher.Instance != null)
                CommandDispatcher.Instance.UnRegistController(this);
        }
    }
}
