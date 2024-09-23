using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class CommandDispatcher : MonoBehaviour
    {
        public static CommandDispatcher Instance { get; private set; }

        /// <summary> 平级注册对象,都会响应指令 </summary>
        List<CommandExecuter> m_register = new List<CommandExecuter>();
        /// <summary> 独占注册对象,指令会被列表中最后一个对象独占 </summary>
        List<CommandExecuter> m_registerHigh = new List<CommandExecuter>();

        Dictionary<KeyCode, EnumCommand> m_keyMapper = new Dictionary<KeyCode, EnumCommand>();

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public void RegistController(CommandExecuter controller)
        {
            if (!controller.AloneMode)
            {
                if (m_register.Contains(controller)) { return; }

                m_register.Add(controller);
            }
            else
            {
                if (m_registerHigh.Contains(controller)) { return; }

                m_registerHigh.Add(controller);
            }
        }

        public void UnRegistController(CommandExecuter menuItemController)
        {
            if (!menuItemController.AloneMode)
                m_register.Remove(menuItemController);
            else
                m_registerHigh.Remove(menuItemController);
        }

        readonly List<CommandExecuter> oneFrameRegister = new List<CommandExecuter>();
        private void Update()
        {
            foreach (var item in m_keyMapper)
            {
                peekRegister(oneFrameRegister);

                if (Input.GetKeyDown(item.Key))
                {
                    foreach (var controller in oneFrameRegister)
                    {
                        if (!controller.Enable) continue;
                        controller.ExecuteCommand(item.Value, false);
                    }
                }
                if (Input.GetKeyUp(item.Key))
                {
                    foreach (var controller in oneFrameRegister)
                    {
                        if (!controller.Enable) continue;
                        controller.ExecuteCommand(item.Value, true);
                    }
                }
            }

            if (m_waitMapperSetting != null)
                m_keyMapper = m_waitMapperSetting;
        }

        private Dictionary<KeyCode, EnumCommand> m_waitMapperSetting = null;
        public void SetKeyMapper(Dictionary<KeyCode, EnumCommand> mapper)
        {
            m_waitMapperSetting = mapper;
        }

        private List<CommandExecuter> peekRegister(List<CommandExecuter> results)
        {
            results.Clear();

            if (m_registerHigh.Count > 0)
            {
                for (int i = m_registerHigh.Count - 1; i >= 0; i--)
                {
                    var controller = m_registerHigh[i];
                    if (controller.Enable)
                    {
                        results.Add(controller);
                        return results;
                    }
                }
            }

            foreach (var controller in m_register)
            {
                if (!controller.Enable) continue;

                results.Add(controller);
            }

            return results;
        }


#if UNITY_EDITOR
        public void GetRegisters(out IReadOnlyList<CommandExecuter> normal, out IReadOnlyList<CommandExecuter> alone)
        {
            normal = m_register;
            alone = m_registerHigh;
        }

#endif
    }
}
