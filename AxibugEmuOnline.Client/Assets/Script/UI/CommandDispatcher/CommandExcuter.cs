using AxibugEmuOnline.Client.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public abstract class CommandExecuter : MonoBehaviour
    {
        private PulseInvoker m_pulsInvoker_Left;
        private PulseInvoker m_pulsInvoker_Right;
        private PulseInvoker m_pulsInvoker_Up;
        private PulseInvoker m_pulsInvoker_Down;

        float PulseInvoke_Delay = 0.4f;
        float PulseInvoke_Interval = 0.05f;

        public abstract bool Enable { get; }
        public virtual bool AloneMode { get; }
        public bool Registed => CommandDispatcher.Instance.IsRegisted(this);

        protected virtual void Awake()
        {
            m_pulsInvoker_Left = new PulseInvoker(OnCmdSelectItemLeft, PulseInvoke_Delay, PulseInvoke_Interval);
            m_pulsInvoker_Right = new PulseInvoker(OnCmdSelectItemRight, PulseInvoke_Delay, PulseInvoke_Interval);
            m_pulsInvoker_Up = new PulseInvoker(OnCmdSelectItemUp, PulseInvoke_Delay, PulseInvoke_Interval);
            m_pulsInvoker_Down = new PulseInvoker(OnCmdSelectItemDown, PulseInvoke_Delay, PulseInvoke_Interval);
        }

        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        protected virtual void OnDestroy() { }

        protected virtual void Update()
        {
            if (Registed && Enable)
            {
                m_pulsInvoker_Left.Update(Time.deltaTime);
                m_pulsInvoker_Right.Update(Time.deltaTime);
                m_pulsInvoker_Up.Update(Time.deltaTime);
                m_pulsInvoker_Down.Update(Time.deltaTime);
            }
            else
            {
                m_pulsInvoker_Left.DisActive();
                m_pulsInvoker_Right.DisActive();
                m_pulsInvoker_Up.DisActive();
                m_pulsInvoker_Down.DisActive();
            }
        }


        public void ResetPulsInvoker()
        {
            m_pulsInvoker_Left.DisActive();
            m_pulsInvoker_Right.DisActive();
            m_pulsInvoker_Up.DisActive();
            m_pulsInvoker_Down.DisActive();
        }

        public void ExecuteCommand(EnumCommand cmd, bool cancel)
        {
            if (cmd == EnumCommand.NONE) return;
            if (!cancel)
            {
                switch (cmd)
                {
                    case EnumCommand.SelectItemLeft:
                        m_pulsInvoker_Left.SetActive();
                        OnCmdSelectItemLeft();
                        break;
                    case EnumCommand.SelectItemRight:
                        m_pulsInvoker_Right.SetActive();
                        OnCmdSelectItemRight();
                        break;
                    case EnumCommand.SelectItemUp:
                        m_pulsInvoker_Up.SetActive();
                        OnCmdSelectItemUp();
                        break;
                    case EnumCommand.SelectItemDown:
                        m_pulsInvoker_Down.SetActive();
                        OnCmdSelectItemDown();
                        break;
                    case EnumCommand.Enter:
                        if (OnCmdEnter())
                        {
                            m_pulsInvoker_Left.DisActive();
                            m_pulsInvoker_Right.DisActive();
                            m_pulsInvoker_Up.DisActive();
                            m_pulsInvoker_Down.DisActive();
                        }
                        break;
                    case EnumCommand.Back:
                        OnCmdBack();
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
        protected virtual bool OnCmdEnter() => false;
        protected virtual void OnCmdBack() { }

    }

    public enum EnumCommand
    {
        NONE,
        SelectItemLeft,
        SelectItemRight,
        SelectItemUp,
        SelectItemDown,
        Enter,
        Back,
        OptionMenu
    }
}
