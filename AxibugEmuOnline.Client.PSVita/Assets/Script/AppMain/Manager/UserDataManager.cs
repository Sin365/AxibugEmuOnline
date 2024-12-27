using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.Event;
using AxibugEmuOnline.Client.Network;
using AxibugProtobuf;
using System.Collections.Generic;
using System.Linq;

namespace AxibugEmuOnline.Client.Manager
{
    public class UserDataBase
    {
        public long UID { get; set; }
        public string Account { get; set; }
        public string NickName { get; set; }
    }

    public class MainUserDataBase : UserDataBase
    {
        public bool IsLoggedIn { get; set; } = false;
    }

    public class UserDataManager
    {
        public UserDataManager()
        {
            //注册重连成功事件，以便后续自动登录
            App.network.OnReConnected += OnReConnected;

            //网络事件注册
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdUserOnlinelist, RecvUserOnlinelist);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdUserJoin, RecvCmdUserJoin);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdUserLeave, RecvGetUserLeave);

            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdModifyNickName, RecvModifyNickName);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdUpdateSelfUserInfo, RecvUpdateSelfUserInfo);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdUpdateOtherUserInfo, RecvUpdateOtherUserInfo);
        }

        public MainUserDataBase userdata { get; private set; } = new MainUserDataBase();
        public bool IsLoggedIn => userdata.IsLoggedIn;
        Dictionary<long, UserDataBase> DictUID2User = new Dictionary<long, UserDataBase>();
        public int OnlinePlayerCount => DictUID2User.Count;
        public void InitMainUserData(string UName, long UID)
        {
            userdata.NickName = UName;
            userdata.IsLoggedIn = true;
            userdata.UID = UID;
            //以及其他数据初始化
            //...
        }
        /// <summary>
        /// 登出
        /// </summary>
        public void LoginOutData()
        {
            userdata.IsLoggedIn = false;
            //以及其他数据清理
            //...
        }
        /// <summary>
        /// 当重连成功
        /// </summary>
        public void OnReConnected()
        {
            //如果之前已登录，则重新登录
            if (userdata.IsLoggedIn)
            {
                App.login.Login();
            }
        }

        #region 角色管理
        public void UpdateOrAddUser(UserMiniInfo minfo, out bool isNewUser)
        {
            lock (DictUID2User)
            {
                if (!DictUID2User.ContainsKey(minfo.UID))
                {
                    DictUID2User[minfo.UID] = new UserDataBase()
                    {
                        UID = minfo.UID,
                        NickName = minfo.NickName,
                        Account = "",
                    };
                    isNewUser = true;
                }
                else
                {
                    isNewUser = false;
                    DictUID2User[minfo.UID].NickName = minfo.NickName;
                }
            }
        }

        public void RemoveUser(long UID)
        {
            bool bflag = false;
            string UName = "";
            lock (DictUID2User)
            {
                if (DictUID2User.ContainsKey(UID))
                {
                    UName = DictUID2User[UID].NickName;
                    DictUID2User.Remove(UID);
                    bflag = true;
                }
            }
            if (bflag)
            {
                //抛出用户离开事件
                Eventer.Instance.PostEvent(EEvent.OnUserLoginOut, UID, UName);
            }
        }
        public UserDataBase GetUserByUid(long UID)
        {
            lock (DictUID2User)
            {
                if (DictUID2User.ContainsKey(UID))
                {
                    return DictUID2User[UID];
                }
                return null;
            }
        }

        public UserDataBase[] GetUserList()
        {
            UserDataBase[] ulist = new UserDataBase[DictUID2User.Count];
            long[] UIDs = DictUID2User.Keys.ToArray();
            for (int i = 0; i < UIDs.Length; i++)
            {
                ulist[i] = DictUID2User[UIDs[i]];
            }
            return ulist;
        }
        #endregion

        /// <summary>
        /// 请求拉取房间列表
        /// </summary>
        public void Send_GetUserList()
        {
            Protobuf_UserList msg = new Protobuf_UserList()
            {
            };
            App.network.SendToServer((int)CommandID.CmdUserOnlinelist, ProtoBufHelper.Serizlize(msg));
        }

        public void RecvUserOnlinelist(byte[] reqData)
        {
            Protobuf_UserList_RESP msg = ProtoBufHelper.DeSerizlize<Protobuf_UserList_RESP>(reqData);
            DictUID2User.Clear();
            for (int i = 0; i < msg.UserList.Count; i++)
            {
                UserMiniInfo mi = msg.UserList[i];
                bool isNewUser;
				UpdateOrAddUser(mi, out isNewUser);
            }
            Eventer.Instance.PostEvent(EEvent.OnUserListAllUpdate);
        }
        public void RecvCmdUserJoin(byte[] reqData)
        {
            Protobuf_UserJoin_RESP msg = ProtoBufHelper.DeSerizlize<Protobuf_UserJoin_RESP>(reqData);
            bool isNewUser;
			UpdateOrAddUser(msg.UserInfo, out isNewUser);
            if (isNewUser)
            { 
                Eventer.Instance.PostEvent(EEvent.OnUserLogin, msg.UserInfo.UID, msg.UserInfo.NickName);
                OverlayManager.PopTip($"玩家[{msg.UserInfo.NickName}]上线了");
            }
        }

        public void RecvGetUserLeave(byte[] reqData)
        {
            Protobuf_UserLeave_RESP msg = ProtoBufHelper.DeSerizlize<Protobuf_UserLeave_RESP>(reqData);
            RemoveUser(msg.UID);
        }

        /// <summary>
        /// 发送修改昵称请求
        /// </summary>
        /// <param name="NickName"></param>
        public void Send_ModifyNickName(string NickName)
        {
            Protobuf_Modify_NickName msg = new Protobuf_Modify_NickName()
            {
                NickName = NickName
            };
            App.network.SendToServer((int)CommandID.CmdModifyNickName, ProtoBufHelper.Serizlize(msg));
        }

        void RecvModifyNickName(byte[] reqData)
        {
            Protobuf_Modify_NickName_RESP msg = ProtoBufHelper.DeSerizlize<Protobuf_Modify_NickName_RESP>(reqData);
        }

        private void RecvUpdateSelfUserInfo(byte[] reqData)
        {
            Protobuf_Update_UserInfo_RESP msg = ProtoBufHelper.DeSerizlize<Protobuf_Update_UserInfo_RESP>(reqData);
            userdata.NickName = msg.UserInfo.NickName;
            Eventer.Instance.PostEvent(EEvent.OnSelfInfoUpdate);
        }

        private void RecvUpdateOtherUserInfo(byte[] reqData)
        {
            Protobuf_Update_OtherUserInfo_RESP msg = ProtoBufHelper.DeSerizlize<Protobuf_Update_OtherUserInfo_RESP>(reqData);
            UserDataBase userdata = GetUserByUid(msg.UID);
            if (userdata == null)
                return;
            userdata.NickName = msg.UserInfo.NickName;

            App.roomMgr.ChangeCurrRoomPlayerName(msg.UID);

            Eventer.Instance.PostEvent(EEvent.OnOtherUserInfoUpdate, msg.UID);
        }

    }
}
