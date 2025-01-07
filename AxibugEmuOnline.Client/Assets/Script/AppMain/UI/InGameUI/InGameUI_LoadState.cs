namespace AxibugEmuOnline.Client
{
    public class InGameUI_LoadState : ExecuteMenu
    {
        private InGameUI m_gameUI;
        public override bool Visible => !m_gameUI.IsNetPlay;
        public override string Name => "读取快照";

        public InGameUI_LoadState(InGameUI gameUI)
        {
            m_gameUI = gameUI;
        }

        public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
        {
            if (m_gameUI.IsNetPlay) return;

            object state = m_gameUI.GetQuickState();
            if (state != null)
            {
                m_gameUI.Core.LoadState(state);
            }
        }
    }
}
