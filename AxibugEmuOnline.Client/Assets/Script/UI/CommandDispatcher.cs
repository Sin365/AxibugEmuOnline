using AxibugEmuOnline.Client.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class CommandDispatcher : MonoBehaviour
    {
        public static CommandDispatcher Instance { get; private set; }

        List<CommandExecuter> m_register = new List<CommandExecuter>();
        Dictionary<KeyCode, EnumCommand> m_keyMapper = new Dictionary<KeyCode, EnumCommand>();

        private void Awake()
        {
            Instance = this;

            m_keyMapper.Add(KeyCode.A, EnumCommand.SelectItemLeft);
            m_keyMapper.Add(KeyCode.D, EnumCommand.SelectItemRight);
            m_keyMapper.Add(KeyCode.W, EnumCommand.SelectItemUp);
            m_keyMapper.Add(KeyCode.S, EnumCommand.SelectItemDown);
            m_keyMapper.Add(KeyCode.K, EnumCommand.Enter);
            m_keyMapper.Add(KeyCode.L, EnumCommand.Back);
            m_keyMapper.Add(KeyCode.I, EnumCommand.OptionMenu);

            m_keyMapper.Add(KeyCode.LeftArrow, EnumCommand.SelectItemLeft);
            m_keyMapper.Add(KeyCode.RightArrow, EnumCommand.SelectItemRight);
            m_keyMapper.Add(KeyCode.UpArrow, EnumCommand.SelectItemUp);
            m_keyMapper.Add(KeyCode.DownArrow, EnumCommand.SelectItemDown);
            m_keyMapper.Add(KeyCode.Return, EnumCommand.Enter);
            m_keyMapper.Add(KeyCode.Escape, EnumCommand.Back);
            m_keyMapper.Add(KeyCode.RightShift, EnumCommand.OptionMenu);
            m_keyMapper.Add(KeyCode.LeftShift, EnumCommand.OptionMenu);
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public void RegistController(CommandExecuter controller)
        {
            if (m_register.Contains(controller)) { return; }

            m_register.Add(controller);
        }

        public void UnRegistController(CommandExecuter menuItemController)
        {
            m_register.Remove(menuItemController);
        }

        readonly List<CommandExecuter> oneFrameRegister = new List<CommandExecuter>();
        private void Update()
        {
            foreach (var item in m_keyMapper)
            {
                if (Input.GetKeyDown(item.Key))
                {
                    oneFrameRegister.Clear();
                    oneFrameRegister.AddRange(m_register);

                    for (int i = 0; i < oneFrameRegister.Count; i++)
                    {
                        var controller = oneFrameRegister[i];
                        if (!controller.Enable) continue;
                        controller.ExecuteCommand(item.Value, false);
                    }
                }
                if (Input.GetKeyUp(item.Key))
                {
                    oneFrameRegister.Clear();
                    oneFrameRegister.AddRange(m_register);

                    for (int i = 0; i < oneFrameRegister.Count; i++)
                    {
                        var controller = oneFrameRegister[i];
                        if (!controller.Enable) continue;
                        controller.ExecuteCommand(item.Value, true);
                    }
                }
            }
        }
    }

    public abstract class CommandExecuter : MonoBehaviour
    {
        private PulseInvoker m_pulsInvoker_Left;
        private PulseInvoker m_pulsInvoker_Right;
        private PulseInvoker m_pulsInvoker_Up;
        private PulseInvoker m_pulsInvoker_Down;

        [SerializeField]
        float PulseInvoke_Delay = 0.4f;
        [SerializeField]
        float PulseInvoke_Interval = 0.05f;

        public abstract bool Enable { get; }

        protected virtual void Awake()
        {
            m_pulsInvoker_Left = new PulseInvoker(OnCmdSelectItemLeft, PulseInvoke_Delay, PulseInvoke_Interval);
            m_pulsInvoker_Right = new PulseInvoker(OnCmdSelectItemRight, PulseInvoke_Delay, PulseInvoke_Interval);
            m_pulsInvoker_Up = new PulseInvoker(OnCmdSelectItemUp, PulseInvoke_Delay, PulseInvoke_Interval);
            m_pulsInvoker_Down = new PulseInvoker(OnCmdSelectItemDown, PulseInvoke_Delay, PulseInvoke_Interval);
        }

        protected virtual void Update()
        {
            m_pulsInvoker_Left.Update(Time.deltaTime);
            m_pulsInvoker_Right.Update(Time.deltaTime);
            m_pulsInvoker_Up.Update(Time.deltaTime);
            m_pulsInvoker_Down.Update(Time.deltaTime);
        }

        public void ExecuteCommand(EnumCommand cmd, bool cancel)
        {
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
        protected abstract void OnSelectMenuChanged();

    }

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
