using AxibugEmuOnline.Client.ClientCore;
using System.Diagnostics;

namespace AxibugEmuOnline.Client
{
    public class InGameUI_SaveStateQuick : ExecuteMenu
    {
        private InGameUI m_gameUI;

        public override bool Visible => !m_gameUI.Core.IsNetPlay;
        public override string Name => "快速保存";

        public InGameUI_SaveStateQuick(InGameUI gameUI)
        {
            m_gameUI = gameUI;
        }

        public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
        {
            if (m_gameUI.Core.IsNetPlay) return;

            Stopwatch sw = Stopwatch.StartNew();
            object state = m_gameUI.Core.GetState();

            m_gameUI.SaveQuickState(state);
            sw.Stop();
            App.log.Info($"{m_gameUI.RomFile.Platform}====>获取快照耗时:{sw.Elapsed.TotalMilliseconds}ms");
        }
    }
}
