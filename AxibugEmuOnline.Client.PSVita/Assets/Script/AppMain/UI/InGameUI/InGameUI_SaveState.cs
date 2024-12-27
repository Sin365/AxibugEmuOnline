using AxibugEmuOnline.Client.ClientCore;
using System.Diagnostics;

namespace AxibugEmuOnline.Client
{
    public class InGameUI_SaveState : ExecuteMenu
    {
        private InGameUI m_gameUI;

        public override bool Visible => !m_gameUI.IsNetPlay;

        public InGameUI_SaveState(InGameUI gameUI) : base("保存快照", null)
        {
            m_gameUI = gameUI;
        }

        public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
        {
            if (m_gameUI.IsNetPlay) return;

            Stopwatch sw = Stopwatch.StartNew();
            object state = m_gameUI.Core.GetState();

            m_gameUI.SaveQuickState(state);
            sw.Stop();
            App.log.Info($"{m_gameUI.RomFile.Platform}====>获取快照耗时:{sw.Elapsed.TotalMilliseconds}ms");
        }
    }
}
