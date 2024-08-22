using AxibugEmuOnline.Client.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public abstract class MenuItemController : MonoBehaviour
    {
        [SerializeField]
        float PulseInvoke_Delay = 0.4f;
        [SerializeField]
        float PulseInvoke_Interval = 0.05f;
        
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
            }
        }

        private PulseInvoker m_pulsInvoker_Left;
        private PulseInvoker m_pulsInvoker_Right;
        private PulseInvoker m_pulsInvoker_Up;
        private PulseInvoker m_pulsInvoker_Down;

        private int m_selectIndex = -1;
        protected List<MenuItem> m_runtimeMenuUI = new List<MenuItem>();

        public int SelectIndex
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

        [SerializeField]
        protected Transform m_menuItemRoot;

        protected virtual void Start()
        {
            for (int i = 0; i < m_menuItemRoot.childCount; i++)
            {
                Transform child = m_menuItemRoot.GetChild(i);
                m_runtimeMenuUI.Add(child.GetComponent<MenuItem>());
            }

            Canvas.ForceUpdateCanvases();
            SelectIndex = 0;

            m_pulsInvoker_Left = new PulseInvoker(OnCmdSelectItemLeft, PulseInvoke_Delay, PulseInvoke_Interval);
            m_pulsInvoker_Right = new PulseInvoker(OnCmdSelectItemRight, PulseInvoke_Delay, PulseInvoke_Interval);
            m_pulsInvoker_Up = new PulseInvoker(OnCmdSelectItemUp, PulseInvoke_Delay, PulseInvoke_Interval);
            m_pulsInvoker_Down = new PulseInvoker(OnCmdSelectItemDown, PulseInvoke_Delay, PulseInvoke_Interval);
        }

        private void OnDestroy()
        {
            if (CommandDispatcher.Instance != null)
                CommandDispatcher.Instance.UnRegistController(this);
        }

        protected virtual void Update()
        {
            m_pulsInvoker_Left.Update(Time.deltaTime);
            m_pulsInvoker_Right.Update(Time.deltaTime);
            m_pulsInvoker_Up.Update(Time.deltaTime);
            m_pulsInvoker_Down.Update(Time.deltaTime);
        }

        protected abstract void OnSelectMenuChanged();

        MenuItem m_enteredItem = null;
        public void ExecuteCommand(EnumCommand cmd, bool cancel)
        {
            if (!cancel)
            {
                switch (cmd)
                {
                    case EnumCommand.SelectItemLeft:
                        if (m_enteredItem == null)
                        {
                            m_pulsInvoker_Left.SetActive();
                            OnCmdSelectItemLeft();
                        }
                        break;
                    case EnumCommand.SelectItemRight:
                        if (m_enteredItem == null)
                        {
                            m_pulsInvoker_Right.SetActive();
                            OnCmdSelectItemRight();
                        }
                        break;
                    case EnumCommand.SelectItemUp:
                        if (m_enteredItem == null)
                        {
                            m_pulsInvoker_Up.SetActive();
                            OnCmdSelectItemUp();
                        }
                        break;
                    case EnumCommand.SelectItemDown:
                        if (m_enteredItem == null)
                        {
                            m_pulsInvoker_Down.SetActive();
                            OnCmdSelectItemDown();
                        }
                        break;
                    case EnumCommand.Enter:
                        if (m_enteredItem == null)
                        {
                            m_enteredItem = m_runtimeMenuUI[SelectIndex];
                            OnCmdEnter(m_enteredItem);

                            m_pulsInvoker_Left.DisActive();
                            m_pulsInvoker_Right.DisActive();
                            m_pulsInvoker_Up.DisActive();
                            m_pulsInvoker_Down.DisActive();
                        }
                        break;
                    case EnumCommand.Back:
                        if (m_enteredItem != null)
                        {
                            OnCmdBack(m_enteredItem);
                            m_enteredItem = null;
                        }
                        break;
                    case EnumCommand.OptionMenu:
                        OnCmdOptionMenu();
                        break;
                }
            }
            else
            {
                switch (cmd)
                {
                    case EnumCommand.SelectItemLeft:
                        m_pulsInvoker_Left.DisActive(); break;
                    case EnumCommand.SelectItemRight:
                        m_pulsInvoker_Right.DisActive(); break;
                    case EnumCommand.SelectItemUp:
                        m_pulsInvoker_Up.DisActive(); break;
                    case EnumCommand.SelectItemDown:
                        m_pulsInvoker_Down.DisActive(); break;
                }
            }
        }

        protected virtual void OnCmdSelectItemLeft() { }

        protected virtual void OnCmdSelectItemRight() { }

        protected virtual void OnCmdSelectItemUp() { }

        protected virtual void OnCmdSelectItemDown() { }

        protected virtual void OnCmdOptionMenu() { }
        protected virtual void OnCmdEnter(MenuItem item) { item.OnEnterItem(); }
        protected virtual void OnCmdBack(MenuItem item) { item.OnExitItem(); }
        public enum EnumCommand
        {
            SelectItemLeft,
            SelectItemRight,
            SelectItemUp,
            SelectItemDown,
            Enter,
            Back,
            OptionMenu
        }
    }
}
