using AxibugEmuOnline.Client.ClientCore;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

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

        public override void OnExcute()
        {
            if (!m_gameUI.IsNetPlay)
            {
                App.emu.ResetGame();
            }
        }
    }
}
