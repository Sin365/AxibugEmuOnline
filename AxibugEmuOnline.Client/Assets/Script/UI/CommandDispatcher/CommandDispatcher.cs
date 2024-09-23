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

            m_keyMapper[KeyCode.A] = EnumCommand.SelectItemLeft;
            m_keyMapper[KeyCode.D] = EnumCommand.SelectItemRight;
            m_keyMapper[KeyCode.W] = EnumCommand.SelectItemUp;
            m_keyMapper[KeyCode.S] = EnumCommand.SelectItemDown;
            m_keyMapper[KeyCode.K] = EnumCommand.Enter;
            m_keyMapper[KeyCode.L] = EnumCommand.Back;
            m_keyMapper[KeyCode.I] = EnumCommand.OptionMenu;
            m_keyMapper[KeyCode.LeftArrow] = EnumCommand.SelectItemLeft;
            m_keyMapper[KeyCode.RightArrow] = EnumCommand.SelectItemRight;
            m_keyMapper[KeyCode.UpArrow] = EnumCommand.SelectItemUp;
            m_keyMapper[KeyCode.DownArrow] = EnumCommand.SelectItemDown;
            m_keyMapper[KeyCode.Return] = EnumCommand.Enter;
            m_keyMapper[KeyCode.Escape] = EnumCommand.Back;
            m_keyMapper[KeyCode.RightShift] = EnumCommand.OptionMenu;
            m_keyMapper[KeyCode.LeftShift] = EnumCommand.OptionMenu;


            if (Application.platform == RuntimePlatform.PSP2)
            {
                m_keyMapper[Common.PSVitaKey.Left] = EnumCommand.SelectItemLeft;
                m_keyMapper[Common.PSVitaKey.Right] = EnumCommand.SelectItemRight;
                m_keyMapper[Common.PSVitaKey.Up] = EnumCommand.SelectItemUp;
                m_keyMapper[Common.PSVitaKey.Down] = EnumCommand.SelectItemDown;
                m_keyMapper[Common.PSVitaKey.Circle] = EnumCommand.Enter;
                m_keyMapper[Common.PSVitaKey.Cross] = EnumCommand.Back;
                m_keyMapper[Common.PSVitaKey.Triangle] = EnumCommand.OptionMenu;
            }
            //手柄
            else
            {
                
            }
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
