using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Manager;
using System;
using System.Collections.Generic;

namespace AxibugEmuOnline.Client
{
    public class InGameUI : CommandExecuter
    {
        public static InGameUI Instance { get; private set; }

        public RomFile RomFile => m_rom;
        public override bool Enable => gameObject.activeInHierarchy;

        /// <summary> 指示该游戏实例是否处于联网模式 </summary>
        public bool IsOnline { get; private set; }

        private RomFile m_rom;
        private object m_core;
        private object m_state;

        private List<OptionMenu> menus = new List<OptionMenu>();


        protected override void Awake()
        {
            Instance = this;
            gameObject.SetActiveEx(false);
            menus.Add(new InGameUI_SaveState(this));
            menus.Add(new InGameUI_LoadState(this));
            menus.Add(new InGameUI_QuitGame(this));
            base.Awake();
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        /// <summary>
        /// 获取模拟器核心对象
        /// </summary>
        /// <typeparam name="T">模拟器核心对象类型</typeparam>
        public T GetCore<T>() => (T)m_core;
        /// <summary> 保存快速快照 </summary>
        public void SaveQuickState(object state)
        {
            m_state = state;
        }
        /// <summary>
        /// 读取快速快照
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool GetQuickState<T>(out T state)
        {
            state = default(T);

            if (m_state is T)
            {
                state = (T)m_state;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Show(RomFile currentRom, object core)
        {
            CommandDispatcher.Instance.RegistController(this);

            m_rom = currentRom;
            m_core = core;

            gameObject.SetActiveEx(true);
        }

        public void Hide()
        {
            CommandDispatcher.Instance.UnRegistController(this);

            gameObject.SetActiveEx(false);
        }

        protected override void OnCmdOptionMenu()
        {
            OptionUI.Instance.Pop(menus);
        }

        public void QuitGame()
        {
            App.emu.StopGame();
        }
    }
}
