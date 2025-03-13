using AxibugEmuOnline.Server.Common;
using AxibugEmuOnline.Server.Event;
using AxibugEmuOnline.Server.Manager.Client;
using AxibugEmuOnline.Server.NetWork;
using AxibugProtobuf;
using System.Net.Sockets;

namespace AxibugEmuOnline.Server.Manager
{
    public class UserManager
    {
        public UserManager()
        {
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdUserOnlinelist, RecvGetUserList);

            //事件
            EventSystem.Instance.RegisterEvent<long>(EEvent.OnUserOnline, OnUserJoin);
            EventSystem.Instance.RegisterEvent<long>(EEvent.OnUserOffline, OnUserLeave);
        }

        #region 事件
        void OnUserJoin(long UID)
        {
            AppSrv.g_Log.Debug($"P2PUserManager->OnUserJoin UID->{UID}");
            SendUserJoin(UID);
        }
        void OnUserLeave(long UID)
        {
            AppSrv.g_Log.Debug($"P2PUserManager->OnUserLeave UID->{UID}");
            SendUserLeave(UID);
        }
        #endregion

        public void RecvGetUserList(Socket _socket, byte[] reqData)
        {
            Protobuf_UserList msg = ProtoBufHelper.DeSerizlize<Protobuf_UserList>(reqData);

            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(_socket);
            Protobuf_UserList_RESP respData = new Protobuf_UserList_RESP();

            ClientInfo[] cArr = AppSrv.g_ClientMgr.GetOnlineClientList().ToArray();
            respData.UserCount = cArr.Length;
            for (int i = 0; i < cArr.Length; i++)
            {
                ClientInfo client = cArr[i];
                respData.UserList.Add(new UserMiniInfo()
                {
                    NickName = client.NickName,
                    UID = client.UID,
                    DeviceType = client.deviceType,
                });
            }
            AppSrv.g_Log.Debug($"拉取用户列表->{respData.UserCount}个用户");
            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdUserOnlinelist, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(respData));
        }


        public void SendUserJoin(long UID)
        {
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForUID(UID);
            if (_c == null)
                return;
            UserMiniInfo miniInfo = new UserMiniInfo()
            {
                DeviceType = _c.deviceType,
                NickName = _c.NickName,
                UID = _c.UID
            };
            Protobuf_UserJoin_RESP resp = new Protobuf_UserJoin_RESP()
            {
                UserInfo = miniInfo
            };
            AppSrv.g_ClientMgr.ClientSendALL((int)CommandID.CmdUserJoin, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(resp));
        }

        public void SendUserLeave(long UID)
        {
            Protobuf_UserLeave_RESP resp = new Protobuf_UserLeave_RESP()
            {
                UID = UID,
            };
            AppSrv.g_ClientMgr.ClientSendALL((int)CommandID.CmdUserLeave, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(resp));
        }
    }
}
