using AxibugEmuOnline.Client.ClientCore;

namespace AxibugEmuOnline.Client
{
    public class InGameUI_Reset : ExecuteMenu
    {
        private InGameUI m_gameUI;
        public override string Name => "复位";
        public override bool Visible => !m_gameUI.Core.IsNetPlay || App.roomMgr.IsHost;

        public InGameUI_Reset(InGameUI gameUI)
        {
            m_gameUI = gameUI;
        }

        public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
        {
            if (!m_gameUI.Core.IsNetPlay)
            {
                App.emu.ResetGame();
            }

        }
    }
}
