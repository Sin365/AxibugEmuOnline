using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.Network;
using AxibugProtobuf;
using System;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppLogin
    {
        static string LastLoginGuid = "";
        public AppLogin()
        {
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdLogin, RecvLoginMsg);
        }

        public void Login()
        {
            AppAxibugEmuOnline.log.Debug("-->Login");
            if(string.IsNullOrEmpty(LastLoginGuid))
                LastLoginGuid = Guid.NewGuid().ToString();

            AppAxibugEmuOnline.user.userdata.Account = LastLoginGuid;
            Protobuf_Login msg = new Protobuf_Login()
            {
                LoginType = 0,
                Account = AppAxibugEmuOnline.user.userdata.Account,
            };
            AppAxibugEmuOnline.networkHelper.SendToServer((int)CommandID.CmdLogin, ProtoBufHelper.Serizlize(msg));
        }

        public void RecvLoginMsg(byte[] reqData)
        {
            Protobuf_Login_RESP msg = ProtoBufHelper.DeSerizlize<Protobuf_Login_RESP>(reqData);
            if (msg.Status == LoginResultStatus.Ok)
            {
                AppAxibugEmuOnline.log.Info("登录成功");
                AppAxibugEmuOnline.user.InitMainUserData(AppAxibugEmuOnline.user.userdata.Account,msg.UID);
            }
            else
            {
                AppAxibugEmuOnline.log.Info("登录失败");
            }
        }
    }
}
