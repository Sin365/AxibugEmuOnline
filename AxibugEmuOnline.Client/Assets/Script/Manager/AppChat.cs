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
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdChatmsg, RecvChatMsg);
        }

        public void SendChatMsg(string ChatMsg)
        {
            Protobuf_ChatMsg msg = new Protobuf_ChatMsg()
            {
                ChatMsg = ChatMsg,
            };
            App.networkHelper.SendToServer((int)CommandID.CmdChatmsg, ProtoBufHelper.Serizlize(msg));
        }

        public void RecvChatMsg(byte[] reqData)
        {
            Protobuf_ChatMsg_RESP msg = ProtoBufHelper.DeSerizlize<Protobuf_ChatMsg_RESP>(reqData);
            EventSystem.Instance.PostEvent(EEvent.OnChatMsg, msg.NickName, msg.ChatMsg);
        }
    }
}
