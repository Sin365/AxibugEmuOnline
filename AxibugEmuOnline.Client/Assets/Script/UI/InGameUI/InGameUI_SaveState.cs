using AxibugEmuOnline.Client.ClientCore;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace AxibugEmuOnline.Client
{
    public class InGameUI_SaveState : ExecuteMenu
    {
        private InGameUI m_gameUI;

        public override bool Visible => !m_gameUI.IsOnline;

        public InGameUI_SaveState(InGameUI gameUI) : base("保存快照", null)
        {
            m_gameUI = gameUI;
        }

        public override void OnExcute()
        {
            Stopwatch sw = Stopwatch.StartNew();
            switch (m_gameUI.RomFile.Platform)
            {
                case EnumPlatform.NES:
                    var state = m_gameUI.GetCore<NesEmulator>().NesCore.GetState();
                    m_gameUI.SaveQuickState(state);
                    break;
            }
            sw.Stop();
            App.log.Info($"{m_gameUI.RomFile.Platform}====>获取快照耗时:{sw.Elapsed.TotalMilliseconds}ms");
        }
    }
}
