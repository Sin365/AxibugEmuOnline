using AxibugProtobuf;
using AxibugEmuOnline.Server.Common;
using AxibugEmuOnline.Server.NetWork;
using System.Net.Sockets;

namespace AxibugEmuOnline.Server.Manager
{
    public class LoginManager
    {
        public LoginManager()
        {
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdLogin, UserLogin);
        }

        public void UserLogin(Socket _socket, byte[] reqData)
        {
            AppSrv.g_Log.Debug("收到新的登录请求");
            Protobuf_Login msg = ProtoBufHelper.DeSerizlize<Protobuf_Login>(reqData);
            ClientInfo cinfo = AppSrv.g_ClientMgr.JoinNewClient(msg, _socket);

            byte[] respData = ProtoBufHelper.Serizlize(new Protobuf_Login_RESP()
            {
                Status = LoginResultStatus.Ok,
                RegDate = "",
                LastLoginDate = "",
                Token = "",
                UID = cinfo.UID
            });

        }
    }
}