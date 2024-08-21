using AxibugEmuOnline.Server.Common;
using AxibugEmuOnline.Server.Manager;
using AxibugEmuOnline.Server.NetWork;
using AxibugProtobuf;
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
            AppSrv.g_Log.Debug($"OnCmdScreen lenght:{reqData.Length}");
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            Protobuf_Screnn_Frame msg = ProtoBufHelper.DeSerizlize<Protobuf_Screnn_Frame>(reqData);

            Data_RoomData room = AppSrv.g_Room.GetRoomData(msg.RoomID);

            if (room == null)
            { 
                AppSrv.g_ClientMgr.ClientSend(_c,(int)CommandID.CmdScreen, (int)ErrorCode.ErrorRoomNotFound, new byte[1]);
                return;
            }

            List<ClientInfo> userlist = room.GetAllPlayerClientList();
            AppSrv.g_ClientMgr.ClientSend(userlist, (int)CommandID.CmdScreen, (int)ErrorCode.ErrorOk, reqData, _c.UID);
        }

    }
}