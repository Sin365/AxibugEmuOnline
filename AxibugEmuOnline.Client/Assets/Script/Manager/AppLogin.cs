﻿using AxibugEmuOnline.Client.ClientCore;
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
            App.log.Debug("-->Login");
            if (string.IsNullOrEmpty(LastLoginGuid))
                LastLoginGuid = Guid.NewGuid().ToString();

            App.user.userdata.Account = LastLoginGuid;
            Protobuf_Login msg = new Protobuf_Login()
            {
                LoginType = 0,
                Account = App.user.userdata.Account,
            };
            App.network.SendToServer((int)CommandID.CmdLogin, ProtoBufHelper.Serizlize(msg));
        }

        public void RecvLoginMsg(byte[] reqData)
        {
            Protobuf_Login_RESP msg = ProtoBufHelper.DeSerizlize<Protobuf_Login_RESP>(reqData);
            if (msg.Status == LoginResultStatus.Ok)
            {
                App.log.Info("登录成功");
                App.user.InitMainUserData(App.user.userdata.Account, msg.UID);

                App.log.Info("获取Room列表");
                App.roomMgr.SendGetRoomList();
            }
            else
            {
                App.log.Info("登录失败");
            }
#if UNITY_EDITOR
            //TestCreate();
#endif
        }


        #region 测试
        void TestCreate()
        {
            App.roomMgr.SendCreateRoom(1, 0, string.Empty);
        }
        long TestFrameID = 0;
        void TestEmuUpdate()
        {
            
        }
        #endregion

    }
}
