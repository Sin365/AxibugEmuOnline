using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
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

        public void SendChatMsg(string ChatMsg)
        {
            _Protobuf_ChatMsg.ChatMsg = ChatMsg;
            App.network.SendToServer((int)CommandID.CmdChatmsg, _Protobuf_ChatMsg);
        }

        public void RecvChatMsg(Protobuf_ChatMsg_RESP msg)
        {
            Eventer.Instance.PostEvent(EEvent.OnChatMsg, msg.NickName, msg.ChatMsg);
        }
    }
}
