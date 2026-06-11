using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Event;
using AxibugEmuOnline.Client.Network;
using AxibugProtobuf;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppChat
    {
        Protobuf_ChatMsg _Protobuf_ChatMsg = new Protobuf_ChatMsg();
        public AppChat()
        {
            NetMsg.Instance.RegNetMsgEvent<Protobuf_ChatMsg_RESP>((int)CommandID.CmdChatmsg, RecvChatMsg);
        }

        public void ShowInputToChat()
        {
            OverlayManager.Input(((msg) =>
            {
                if (string.IsNullOrEmpty(msg.Trim()))
                {
                    OverlayManager.PopTip("未输入聊天内容");
                    return;
                }

                if (msg.Length > 100)
                {
                    OverlayManager.PopTip("不可超过100个字符");
                    return;
                }

                App.chat.SendChatMsg(msg);
            }),
                "[发送聊天]请输入聊天内容"
                , string.Empty);
        }

        public void SendChatMsg(string ChatMsg)
        {
            if (!App.user.IsLoggedIn)
            {
                OverlayManager.PopTip("您当前并未登录");
                return;
            }
            _Protobuf_ChatMsg.ChatMsg = ChatMsg;
            App.network.SendToServer((int)CommandID.CmdChatmsg, _Protobuf_ChatMsg);
        }

        public void RecvChatMsg(Protobuf_ChatMsg_RESP msg)
        {
            Eventer.Instance.PostEvent(EEvent.OnChatMsg, msg.NickName, msg.ChatMsg);
            OverlayManager.PopTip("[" + msg.NickName + "]:" + msg.ChatMsg);
        }
    }
}
