using AxibugEmuOnline.Client.ClientCore;

namespace AxibugEmuOnline.Client
{
    public class InGameUI_SendChat : ExecuteMenu
    {
        private InGameUI m_gameUI;
        public override string Name => "发送聊天信息";

        public InGameUI_SendChat(InGameUI gameUI)
        {
            m_gameUI = gameUI;
        }

        public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
        {
            App.chat.ShowInputToChat();
        }
    }
}
