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
}
