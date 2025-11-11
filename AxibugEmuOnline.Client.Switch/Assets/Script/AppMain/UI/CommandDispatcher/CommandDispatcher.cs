using AxibugEmuOnline.Client.Event;
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

        CommandListener m_listener;

        CommandListener.ScheduleType? m_waitMapperSetting = null;
        public CommandListener.ScheduleType Mode
        {
            get => m_listener.Schedule;
            set => m_waitMapperSetting = value;
        }

        private void Awake()
        {
            Instance = this;

            //初始化command监视器
            m_listener = new CommandListener();
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public bool IsRegisted(CommandExecuter commandExecuter)
        {
            return m_register.Contains(commandExecuter) || m_registerHigh.Contains(commandExecuter);
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
            if (!InputUI.IsInputing)
            {
                peekRegister(oneFrameRegister);
                m_listener.Update(oneFrameRegister);
            }

            //键位映射在按键响应的堆栈结束后处理,防止迭代器修改问题
            if (m_waitMapperSetting != null)
            {
                m_listener.Schedule = m_waitMapperSetting.Value;
                Eventer.Instance.PostEvent(EEvent.OnScreenGamepadPlatformTypeChanged);
                m_waitMapperSetting = null;
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
