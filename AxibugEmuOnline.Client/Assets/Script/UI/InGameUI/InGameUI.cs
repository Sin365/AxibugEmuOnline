using System;
using System.Collections.Generic;

namespace AxibugEmuOnline.Client
{
    public class InGameUI : CommandExecuter
    {

        public static InGameUI Instance { get; private set; }

        public RomFile RomFile => m_rom;
        public override bool Enable => gameObject.activeInHierarchy;
        private RomFile m_rom;
        private object m_core;

        private InGameUI_SaveState m_saveMenu;

        protected override void Awake()
        {
            Instance = this;
            gameObject.SetActiveEx(false);
            base.Awake();
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public T GetCore<T>() => (T)m_core;

        public void Show(RomFile currentRom, object core)
        {
            m_saveMenu = new InGameUI_SaveState(this);
            CommandDispatcher.Instance.RegistController(this);

            m_rom = currentRom;
            m_core = core;
            gameObject.SetActiveEx(true);
        }

        public void Hide()
        {
            CommandDispatcher.Instance.UnRegistController(this);

            m_rom = null;
            m_core = null;
            gameObject.SetActiveEx(false);
        }

        protected override void OnCmdOptionMenu()
        {
            OptionUI.Instance.Pop(new List<OptionMenu> { m_saveMenu });
        }
    }
}
