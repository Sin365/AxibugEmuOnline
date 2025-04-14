namespace AxibugEmuOnline.Client
{
    public class InGameUI_LoadStateQuick : ExecuteMenu
    {
        private InGameUI m_gameUI;
        public override bool Visible => !m_gameUI.IsNetPlay && m_gameUI.GetQuickState() != null;
        public override string Name => "快速读取";

        public InGameUI_LoadStateQuick(InGameUI gameUI)
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
