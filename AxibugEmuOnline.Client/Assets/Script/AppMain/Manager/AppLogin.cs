using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.Event;
using AxibugEmuOnline.Client.Network;
using AxibugProtobuf;
using System;
using UnityEngine;

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
            AxibugProtobuf.DeviceType devType;
            switch (Application.platform)
            {
                case RuntimePlatform.PSP2:
                    devType = AxibugProtobuf.DeviceType.Psv;
                    break;
                case RuntimePlatform.PS4:
                    devType = AxibugProtobuf.DeviceType.Ps4;
                    break;
                case RuntimePlatform.PS3:
                    devType = AxibugProtobuf.DeviceType.Ps3;
                    break;
                case RuntimePlatform.WiiU:
                    devType = AxibugProtobuf.DeviceType.WiiU;
                    break;
                case RuntimePlatform.XBOX360:
                    devType = AxibugProtobuf.DeviceType.Xbox360;
                    break;
                case RuntimePlatform.XboxOne:
                case RuntimePlatform.GameCoreXboxOne:
                    devType = AxibugProtobuf.DeviceType.Xboxone;
                    break;
                //case RuntimePlatform.:
                //    devType = AxibugProtobuf.DeviceType.Nintendo3Ds;
                //    break;
                case RuntimePlatform.IPhonePlayer:
                    devType = AxibugProtobuf.DeviceType.Ios;
                    break;
                default:
                    devType = AxibugProtobuf.DeviceType.Pc;
                    break;
            }

            Protobuf_Login msg = new Protobuf_Login()
            {
                LoginType = LoginType.UseDevice,
                DeviceStr = Initer.dev_UUID,
                DeviceType = devType,
            };

            App.network.SendToServer((int)CommandID.CmdLogin, ProtoBufHelper.Serizlize(msg));
        }

        public void RecvLoginMsg(byte[] reqData)
        {
            Protobuf_Login_RESP msg = ProtoBufHelper.DeSerizlize<Protobuf_Login_RESP>(reqData);
            if (msg.Status == LoginResultStatus.Ok)
            {
                App.log.Info("登录成功");
                App.user.InitMainUserData(App.user.userdata.Account, msg.UID, msg.Token);
                OverlayManager.PopTip("登录成功");


                App.log.Info("获取Room列表");
                App.roomMgr.SendGetRoomList();
                App.log.Info("获取在线玩家列表");
                App.user.Send_GetUserList();
                //开始同步存档
                App.SavMgr.StartSyncSlot();

                Eventer.Instance.PostEvent(EEvent.OnLoginSucceed);
            }
            else
            {
                App.log.Info("登录失败");
                OverlayManager.PopTip("登录失败");
                Eventer.Instance.PostEvent(EEvent.OnLoginFailed);
            }
#if UNITY_EDITOR
            //TestCreate();
#endif
        }

    }
}
