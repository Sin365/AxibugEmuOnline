using AxibugEmuOnline.Server.Common;
using AxibugEmuOnline.Server.Manager;
using AxibugEmuOnline.Server.NetWork;
using AxibugProtobuf;
using System.Net;
using System.Net.Sockets;

namespace AxibugEmuOnline.Server
{

    public class GameManager
    {
        public GameManager()
        {
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdScreen, OnCmdScreen);
        }

        public void OnCmdScreen(Socket sk, byte[] reqData)
        {
            ServerManager.g_Log.Debug($"OnCmdScreen lenght:{reqData.Length}");
            ClientInfo _c = ServerManager.g_ClientMgr.GetClientForSocket(sk);
            Protobuf_Screnn_Frame msg = ProtoBufHelper.DeSerizlize<Protobuf_Screnn_Frame>(reqData);
            ServerManager.g_ClientMgr.ClientSendALL((int)CommandID.CmdScreen, (int)ErrorCode.ErrorOk, reqData, _c.UID);
        }
    }
}