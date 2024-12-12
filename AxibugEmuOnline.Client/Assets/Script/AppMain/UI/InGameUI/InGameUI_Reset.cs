using AxibugEmuOnline.Client.ClientCore;

namespace AxibugEmuOnline.Client
{
    public class InGameUI_Reset : ExecuteMenu
    {
        private InGameUI m_gameUI;

        public override bool Visible => !m_gameUI.IsNetPlay || App.roomMgr.IsHost;

        public InGameUI_Reset(InGameUI gameUI) : base("¸´Î»", null)
        {
            m_gameUI = gameUI;
        }

        public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
        {
            if (!m_gameUI.IsNetPlay)
            {
                App.emu.ResetGame();
            }

        }
    }
}
