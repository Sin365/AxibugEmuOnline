using AxibugEmuOnline.Server.Common;
using AxibugEmuOnline.Server.Event;
using AxibugEmuOnline.Server.NetWork;
using AxibugProtobuf;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Ocsp;
using System.Net.Sockets;

namespace AxibugEmuOnline.Server.Manager
{
    public class LoginManager
    {
        public LoginManager()
        {
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdLogin, UserLogin);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdModifyNickName, OnCmdModifyNickName);
        }

        void UserLogin(Socket _socket, byte[] reqData)
        {
            AppSrv.g_Log.DebugCmd("UserLogin");
            Protobuf_Login msg = ProtoBufHelper.DeSerizlize<Protobuf_Login>(reqData);
            long _uid = 0;

            AppSrv.g_Log.Info($"LoginType -> {msg.LoginType.ToString()}");
            if (msg.LoginType == LoginType.UseDevice)
            {
                if (!GetUidByDevice(msg.DeviceStr, msg.DeviceType, out _uid))
                {
                    byte[] ErrRespData = ProtoBufHelper.Serizlize(new Protobuf_Login_RESP()
                    {
                        Status = LoginResultStatus.AccountErr,
                    });
                    AppSrv.g_ClientMgr.ClientSend(_socket, (int)CommandID.CmdLogin, (int)ErrorCode.ErrorOk, ErrRespData);
                    return;
                }
            }
            else
            {
                byte[] ErrRespData = ProtoBufHelper.Serizlize(new Protobuf_Login_RESP()
                {
                    Status = LoginResultStatus.AccountErr,
                });
                AppSrv.g_ClientMgr.ClientSend(_socket, (int)CommandID.CmdLogin, (int)ErrorCode.ErrorOk, ErrRespData);
                return;
            }

            ClientInfo _c = AppSrv.g_ClientMgr.JoinNewClient(_uid, _socket);

            UpdateUserData(_uid, _c);

            EventSystem.Instance.PostEvent(EEvent.OnUserOnline, _c.UID);

            byte[] respData = ProtoBufHelper.Serizlize(new Protobuf_Login_RESP()
            {
                Status = LoginResultStatus.Ok,
                RegDate = _c.RegisterDT.ToString("yyyy-MM-dd HH:mm:ss"),
                LastLoginDate = _c.LastLogInDT.ToString("yyyy-MM-dd HH:mm:ss"),
                Token = "",
                NickName = _c.NickName,
                UID = _c.UID
            });
            AppSrv.g_Log.Info($"玩家登录成功 UID->{_c.UID} NikeName->{_c.NickName}");
            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdLogin, (int)ErrorCode.ErrorOk, respData);
        }

        void OnCmdModifyNickName(Socket socket, byte[] reqData)
        {
            AppSrv.g_Log.DebugCmd("OnCmdModifyNikeName");
            bool bDone = false;
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(socket);
            Protobuf_Modify_NickName msg = ProtoBufHelper.DeSerizlize<Protobuf_Modify_NickName>(reqData);
            MySqlConnection conn = Haoyue_SQLPoolManager.DequeueSQLConn("ModifyNikeName");
            try
            {
                string query = "update users set nikename = ?nikename where uid = ?uid ";
                using (var command = new MySqlCommand(query, conn))
                {
                    // 设置参数值
                    command.Parameters.AddWithValue("?uid", _c.UID);
                    command.Parameters.AddWithValue("?uid", msg.NickName);

                    if (command.ExecuteNonQuery() > 0)
                    {
                        bDone = true;
                    }
                }
            }
            catch (Exception e)
            {
            }
            Haoyue_SQLPoolManager.EnqueueSQLConn(conn);

            if (bDone)
            {
                _c.NickName = msg.NickName;

                UserMiniInfo miniinfo = new UserMiniInfo()
                {
                    NickName = _c.NickName,
                };

                Protobuf_Update_UserInfo_RESP infodata = new Protobuf_Update_UserInfo_RESP()
                {
                    UserInfo = miniinfo,
                };
                //回执给自己
                AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdUpdateSelfUserInfo, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(infodata));

                Protobuf_Update_OtherUserInfo_RESP otherinfo = new Protobuf_Update_OtherUserInfo_RESP()
                {
                    UID = _c.UID,
                    UserInfo = miniinfo
                };
                //广播给他人
                AppSrv.g_ClientMgr.ClientSendALL((int)CommandID.CmdUpdateOtherUserInfo, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(otherinfo), SkipUID: _c.UID);
            }
        }

        public bool GetUidByDevice(string deviceStr, DeviceType DeviceType, out long uid)
        {
            uid = 0;
            bool bDone = true;
            MySqlConnection conn = Haoyue_SQLPoolManager.DequeueSQLConn("GetUidByDevice");
            try
            {
                string query = "SELECT uid from user_devices where device = ?deviceStr ";
                using (var command = new MySqlCommand(query, conn))
                {
                    // 设置参数值  
                    command.Parameters.AddWithValue("?deviceStr", deviceStr);
                    // 执行查询并处理结果  
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            uid = reader.GetInt64(0);
                        }
                    }
                }

                if (uid > 0)
                {
                    AppSrv.g_Log.Info($"设备串：{deviceStr} 对应 UID:{uid}");
                }
                else
                {
                    query = "INSERT INTO `haoyue_emu`.`users` (`nikename`, `regdate`, `lastlogindate`) VALUES (NULL,now(),now());SELECT LAST_INSERT_ID(); ";
                    using (var command = new MySqlCommand(query, conn))
                    {
                        // 设置参数值  
                        // 执行查询并处理结果  
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                uid = reader.GetInt64(0);
                            }
                        }
                    }

                    query = "INSERT INTO `haoyue_emu`.`user_devices` (`device`, `devicetype`, `uid`) VALUES (?deviceStr, ?DeviceType, ?uid);";
                    using (var command = new MySqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("?deviceStr", deviceStr);
                        command.Parameters.AddWithValue("?DeviceType", (int)DeviceType);
                        command.Parameters.AddWithValue("?uid", uid);
                        if (command.ExecuteNonQuery() < 1)
                            bDone = false;
                    }

                    AppSrv.g_Log.Info($"创建新账户，设备:{deviceStr},设备类型:{DeviceType.ToString()},是否成功:{bDone}");
                }

            }
            catch (Exception e)
            {
                bDone = false;
            }
            Haoyue_SQLPoolManager.EnqueueSQLConn(conn);

            if (uid <= 0)
                bDone = false;
            return bDone;
        }

        public void UpdateUserData(long uid, ClientInfo _c)
        {
            MySqlConnection conn = Haoyue_SQLPoolManager.DequeueSQLConn("UpdateUserData");
            try
            {
                string query = "SELECT account,nikename,regdate,lastlogindate from users where uid = ?uid ";
                using (var command = new MySqlCommand(query, conn))
                {
                    // 设置参数值
                    command.Parameters.AddWithValue("?uid", uid);
                    // 执行查询并处理结果  
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            _c.Account = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                            _c.NickName = reader.IsDBNull(1) ? string.Empty:reader.GetString(1);
                            _c.LogInDT = DateTime.Now;
                            _c.RegisterDT = reader.IsDBNull(2) ? DateTime.Now : reader.GetDateTime(2);
                            _c.LastLogInDT = reader.IsDBNull(3) ? DateTime.Now : reader.GetDateTime(3);
                        }
                    }
                }
                query = "update users set lastlogindate = now() where uid = ?uid ";
                using (var command = new MySqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("?uid", uid);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {

            }
            Haoyue_SQLPoolManager.EnqueueSQLConn(conn);
        }

    }
}