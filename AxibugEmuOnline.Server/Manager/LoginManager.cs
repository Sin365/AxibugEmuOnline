using AxibugEmuOnline.Server.Common;
using AxibugEmuOnline.Server.Event;
using AxibugEmuOnline.Server.Manager.Client;
using AxibugEmuOnline.Server.NetWork;
using AxibugProtobuf;
using MySql.Data.MySqlClient;
using System.Net.Sockets;

namespace AxibugEmuOnline.Server.Manager
{
    public class LoginManager
    {
        static long tokenSeed = 1;

        public LoginManager()
        {
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdLogin, UserLogin);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdModifyNickName, OnCmdModifyNickName);
        }

        static long GetNextTokenSeed()
        {
            return tokenSeed++;
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

            UpdateUserData(_uid, _c, msg.DeviceType);

            string tokenstr = GenToken(_c);

            EventSystem.Instance.PostEvent(EEvent.OnUserOnline, _c.UID);

            byte[] respData = ProtoBufHelper.Serizlize(new Protobuf_Login_RESP()
            {
                Status = LoginResultStatus.Ok,
                RegDate = _c.RegisterDT.ToString("yyyy-MM-dd HH:mm:ss"),
                LastLoginDate = _c.LastLogInDT.ToString("yyyy-MM-dd HH:mm:ss"),
                Token = tokenstr,
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
            using (MySqlConnection conn = SQLRUN.GetConn("ModifyNikeName"))
            {
                try
                {
                    string query = "update users set nikename = ?nikename where uid = ?uid ";
                    using (var command = new MySqlCommand(query, conn))
                    {
                        // 设置参数值
                        command.Parameters.AddWithValue("?uid", _c.UID);
                        command.Parameters.AddWithValue("?nikename", msg.NickName);
                        if (command.ExecuteNonQuery() > 0)
                        {
                            bDone = true;
                        }
                    }
                }
                catch (Exception e)
                {
                }
            }

            if (bDone)
            {
                _c.NickName = msg.NickName;

                UserMiniInfo miniinfo = new UserMiniInfo()
                {
                    NickName = _c.NickName,
                    DeviceType = _c.deviceType,
                    UID = _c.UID
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
            using (MySqlConnection conn = SQLRUN.GetConn("GetUidByDevice"))
            {
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
                        query = "INSERT INTO `haoyue_emu`.`users` (`nikename`, `regdate`,`lastlogindate`) VALUES (NULL,now(),now());SELECT LAST_INSERT_ID(); ";
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

                        //设置默认名字
                        query = "update users set nikename = ?nikename where uid = ?uid ";
                        using (var command = new MySqlCommand(query, conn))
                        {
                            // 设置参数值
                            command.Parameters.AddWithValue("?uid", uid);
                            command.Parameters.AddWithValue("?nikename", GetRandomNickName(uid));
                            if (command.ExecuteNonQuery() < 1)
                            {
                                bDone = false;
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
                    AppSrv.g_Log.Error($"ex=>{e.ToString()}");
                    bDone = false;
                }
            }

            if (uid <= 0)
                bDone = false;
            return bDone;
        }

        public void UpdateUserData(long uid, ClientInfo _c, DeviceType deviceType)
        {
            using (MySqlConnection conn = SQLRUN.GetConn("UpdateUserData"))
            {
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
                                _c.NickName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                                _c.LogInDT = DateTime.Now;
                                _c.RegisterDT = reader.IsDBNull(2) ? DateTime.Now : reader.GetDateTime(2);
                                _c.LastLogInDT = reader.IsDBNull(3) ? DateTime.Now : reader.GetDateTime(3);
                                _c.deviceType = deviceType;
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
            }
        }

        static string GenToken(ClientInfo _c)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Protobuf_Token_Struct _resp = new Protobuf_Token_Struct()
            {
                UID = _c.UID,
                TokenGenDate = timestamp,
                Seed = GetNextTokenSeed()
            };
            byte[] protobufData = ProtoBufHelper.Serizlize(_resp);
            ProtoBufHelper.DeSerizlize<Protobuf_Token_Struct>(protobufData);
            byte[] encryptData = AESHelper.Encrypt(protobufData);
            string tobase64 = Convert.ToBase64String(encryptData);
            AppSrv.g_Log.Debug("token_base64=>" + tobase64);
            string result = Uri.EscapeDataString(tobase64);
            AppSrv.g_Log.Debug("token_urlcode=>" + result);
            return result;
        }

        static Protobuf_Token_Struct DecrypToken(string tokenStr)
        {
            byte[] encryptData = Convert.FromBase64String(tokenStr);
            byte[] decryptData = AESHelper.Decrypt(encryptData);
            return ProtoBufHelper.DeSerizlize<Protobuf_Token_Struct>(decryptData);
        }

        public string GetRandomNickName(long uid)
        {
            int Idx = new Random((int)DateTime.Now.TimeOfDay.TotalMilliseconds).Next(0, RandomNickName.Length - 1);
            return $"{RandomNickName[Idx]}_{uid}";
        }

        static string[] RandomNickName =
            [
"马里奥",
"路易基",
"碧琪公主",
"库巴",
"耀西",
"瓦里奥",
"瓦路易吉",
"奇诺比奥",
"罗塞塔公主",
"布斯特",
"凯萨琳/凯瑟琳",
"盖拉库巴",
"黛西",
"保罗","马里奥先锋",
            "塞尔达勇士",
    "银河猎人",
    "主手柄",
    "8-bit英雄",
    "像素战士",
    "魂斗罗精英",
    "城堡征服者",
    "塔特博尔打击",
    "奇迹神庙",
    "勇者斗恶龙",
    "龟速侠",
    "拳皇大师",
    "无敌破坏王",
    "时空之刃",
    "跳跃达人",
    "炸弹超人",
    "复古玩家",
    "混沌骑士",
    "传说之翼",
    "火箭骑士",
    "像素魔法师",
    "超级马车手",
    "冒险之星",
    "银河护卫",
    "弹跳英雄",
    "红白机战神",
    "像素忍者",
    "激战霸主",
    "挑战之王",
    "像素巫师",
    "红白机复仇者",
    "马里奥兄弟",
    "赛博战士",
    "像素传说",
    "红白机神话",
    "复古传承者",
    "街头霸王",
    "合金装备",
    "像素英雄",
    "打砖块大师",
    "复活节彩蛋",
    "8-bit传奇",
    "炸弹达人",
    "原子战士",
    "猎天使魔女",
    "探险家",
    "奇异探险",
    "像素大师",
    "星际狂热",
    "点阵图王",
    "掘地者",
    "街机勇士",
    "极速赛车手",
    "星际旅行者",
    "红白机守护者",
    "扭曲像素",
    "冒险家俱乐部",
    "像素方块",
    "时空探险者",
    "红白机奇迹",
    "战士之魂",
    "复古英雄",
    "超级星战士",
    "跳跃精灵",
    "时空旅行者",
    "银河征服者",
    "奇妙世界",
    "无敌小子",
    "打怪达人",
    "复古王者",
    "超级马拉松",
    "银河守护者",
    "街机传奇",
    "像素探险家",
    "红白机战士",
    "复活小队",
    "时间扭曲者",
    "像素骑士",
    "复古探索者",
    "超级跳跃者",
    "银河冒险者",
    "复古时代",
    "超级英雄",
    "街机探险者",
    "红白机大师",
    "时间旅行者",
    "星际战士",
    "冒险之子",
    "红白机复仇",
    "像素探索者",
    "超级宇航员",
    "复古复仇者",
    "时间守护者",
    "银河之子",
    "超级格斗家",
    "时空征服者",
    "复古之星",
    "超级战斗机",
    "时间冒险者",
    "银河神话",
    "超级星际",
    "冒险之魂",
    "复古传奇",
    "超级街机",
    "像素英雄王",
    "红白机探险",
    "超级复仇者",
    "时间征服者",
    "银河之王",
    "超级探险家",
    "复古守护者",
    "超级跳跃王",
    "冒险之王",
    "超级像素",
    "复古战斗",
    "超级复古",
    "时间之神",
    "银河征服",
    "超级传奇",
    "冒险战士",
    "红白机冒险",
    "超级复仇",
    "复古之神",
    "超级时间",
    "银河守护",
    "超级银河",
    "冒险守护",
    "红白机战斗",
    "超级探险",
    "超级星际战士",
    "时间之子",
    "银河之神",
    "超级复古战士",
    "红白机之魂",
    "超级时间旅行者",
    "冒险之神",
    "复古探险家",
    "超级银河探险",
    "时间复仇者",
    "银河战斗机",
    "超级复古探险",
    "红白机战士王",
    "超级冒险家",
    "复古之王",
    "超级像素战士",
    "时间战士",
    "银河探险家",
    "超级红白机",
    "复古复仇者王",
    "超级时间之神",
    "银河冒险家",
    "超级复古传奇",
    "超级冒险王",
    "复古之魂",
    "超级银河战士",
    "时间探险者",
    "银河战士之神",
    "超级复古冒险",
    "红白机探险王",
    "超级时间之子",
    "冒险战士王",
    "复古战士王",
    "超级像素传奇",
    "时间探险王",
    "银河之魂",
    "超级复古神话",
    "超级冒险传奇",
    "复古战士之神",
    "超级银河探险家",
    "时间战士之神",
    "银河战士传奇",
    "超级复古之神",
    "超级冒险神话",
    "复古探险传奇",
    "超级像素神话",
    "时间探险之神",
    "银河冒险传奇",
    "超级复古探险家",
    "超级时间探险者",
    "复古战士传奇",
    "超级银河之神",
    "时间冒险传奇",
    "银河探险传奇",
    "超级红白机神话",
    "超级银河战士之神",
    "超级复古探险之神",
    "红白机战士之魂",
    "超级时间探险家"
            ];


    }
}