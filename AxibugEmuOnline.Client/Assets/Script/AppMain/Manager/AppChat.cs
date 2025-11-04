using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.Event;
using AxibugEmuOnline.Client.Network;
using AxibugProtobuf;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppChat
    {
        public AppChat()
        {
            NetMsg.Instance.RegNetMsgEvent<Protobuf_ChatMsg_RESP>((int)CommandID.CmdChatmsg, RecvChatMsg);
        }

        public void SendChatMsg(string ChatMsg)
        {
            Protobuf_ChatMsg msg = new Protobuf_ChatMsg()
            {
                ChatMsg = ChatMsg,
            };
            App.network.SendToServer((int)CommandID.CmdChatmsg, ProtoBufHelper.Serizlize(msg));
        }

        public void RecvChatMsg(Protobuf_ChatMsg_RESP msg)
        {
            Eventer.Instance.PostEvent(EEvent.OnChatMsg, msg.NickName, msg.ChatMsg);
        }
    }
}
