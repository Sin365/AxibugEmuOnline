using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AxibugEmuOnline.Client
{
    public class CommandListener : ICommandListener
    {
        private PlayerInput m_uiInput;
        private IEnumerable<CommandExecuter> m_executers;

        public CommandListener()
        {
            m_uiInput = GameObject.FindGameObjectWithTag("UIInput").GetComponent<PlayerInput>();
            m_uiInput.onActionTriggered += M_uiInput_onActionTriggered;
        }
        public void Update(IEnumerable<CommandExecuter> executer)
        {
            m_executers = executer;
        }

        private void M_uiInput_onActionTriggered(InputAction.CallbackContext obj)
        {
            CommandState? cs = null;
            switch (obj.action.phase)
            {
                case InputActionPhase.Started:
                    cs = new CommandState { Cancel = false, Cmd = ToCommandEnum(obj.action.name) };
                    break;
                case InputActionPhase.Canceled:
                    cs = new CommandState { Cancel = true, Cmd = ToCommandEnum(obj.action.name) };
                    break;
            }
            if (!cs.HasValue) return;

            foreach (var exec in m_executers)
            {
                if (!exec.Enable) continue;
                exec.ExecuteCommand(cs.Value.Cmd, cs.Value.Cancel);
            }
        }

        public void ApplyKeyMapper(IKeyMapperChanger changer)
        {
            var actionMapName = (string)changer.GetConfig();
            m_uiInput.SwitchCurrentActionMap(actionMapName);
        }

        EnumCommand ToCommandEnum(string actionName)
        {
            return actionName switch
            {
                "SelectItemLeft" => EnumCommand.SelectItemLeft,
                "SelectItemRight" => EnumCommand.SelectItemRight,
                "SelectItemUp" => EnumCommand.SelectItemUp,
                "SelectItemDown" => EnumCommand.SelectItemDown,
                "Enter" => EnumCommand.Enter,
                "Back" => EnumCommand.Back,
                "OptionMenu" => EnumCommand.OptionMenu,
                _ => throw new Exception("Not Support Action")
            };
        }
    }
}
