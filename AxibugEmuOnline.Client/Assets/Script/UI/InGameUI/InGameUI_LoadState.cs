using AxibugEmuOnline.Client.ClientCore;
using System.Diagnostics;

namespace AxibugEmuOnline.Client
{
    public class InGameUI_LoadState : ExecuteMenu
    {
        private InGameUI m_gameUI;
        public override bool Visible => !m_gameUI.IsOnline;

        public InGameUI_LoadState(InGameUI gameUI) : base("读取快照", null)
        {
            m_gameUI = gameUI;
        }

        public override void OnExcute()
        {
            object state = m_gameUI.GetQuickState();
            Stopwatch sw = Stopwatch.StartNew();
            if (state != null)
            {
                m_gameUI.Core.LoadState(state);
                sw.Stop();
                App.log.Info($"{m_gameUI.RomFile.Platform}====>快照加载耗时:{sw.Elapsed.TotalMilliseconds}ms");
            }
        }
    }
}
