using AxibugEmuOnline.Client.ClientCore;

namespace AxibugEmuOnline.Client
{
    public class InGameUI_OpenOrHideScreenJoyStrick : ExecuteMenu
    {
        private InGameUI m_gameUI;
        public override string Name => "开关虚拟按键";

        public InGameUI_OpenOrHideScreenJoyStrick(InGameUI gameUI)
        {
            m_gameUI = gameUI;
        }

        public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
        {
            OverlayManager.HideOrShwoGUIButton();
        }
    }
}
