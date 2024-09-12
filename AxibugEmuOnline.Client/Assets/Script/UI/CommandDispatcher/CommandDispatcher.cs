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
