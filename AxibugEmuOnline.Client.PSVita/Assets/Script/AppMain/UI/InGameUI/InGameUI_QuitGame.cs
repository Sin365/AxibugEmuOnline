namespace AxibugEmuOnline.Client
{
    public class InGameUI_QuitGame : ExecuteMenu
    {
        private InGameUI m_gameUI;


        public InGameUI_QuitGame(InGameUI gameUI) : base("�˳�", null)
        {
            m_gameUI = gameUI;
        }

        public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
        {
            m_gameUI.QuitGame();
        }
    }
}