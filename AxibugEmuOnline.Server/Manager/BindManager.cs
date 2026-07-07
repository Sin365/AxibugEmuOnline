using AxibugEmuOnline.Server.Common;
using AxibugEmuOnline.Server.Manager.Client;
using AxibugEmuOnline.Server.NetWork;
using AxibugProtobuf;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Ocsp;
using System.Net.Sockets;

namespace AxibugEmuOnline.Server.Manager
{
    public class BindManager
    {
        public BindManager()
        {
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdBindAcc, BindAcc);
        }

        private void BindAcc(Socket socket, byte[] reqData)
        {
            AppSrv.g_Log.DebugCmd("BindAcc");
            Protobuf_Bind msg = ProtoBufHelper.DeSerizlize<Protobuf_Bind>(reqData);
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(socket);
            long code_id = -1;
            long code_targetuid = -1;
            DateTime code_createTime = default;
            bool code_isused = default;
            Protobuf_Bind_RESP _resp = new Protobuf_Bind_RESP();
            ErrorCode err = ErrorCode.ErrorOk;
            AppSrv.g_Log.DebugCmd($"BindCode => {msg.BindCode}");
            using (MySqlConnection conn = SQLRUN.GetConn("BindAcc"))
            {
                string query = "SELECT id,uid,createtime,isused from user_bindcode where user_bindcode.`code` = ?code ";
                using (var command = new MySqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("?code", msg.BindCode);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            code_id = reader.GetInt64(0);
                            code_targetuid = reader.GetInt64(1);
                            code_createTime = reader.IsDBNull(2) ? DateTime.Now : reader.GetDateTime(2);
                            code_isused = reader.IsDBNull(3) ? false : reader.GetBoolean(3);
                        }
                    }

                    if (code_id <= 0)
                    {
                        AppSrv.g_Log.DebugCmd($"BindCode 不存在");
                        err = ErrorCode.ErrorBindCodeNotExist;
                    }
                    else if (code_targetuid == _c.ParentUID)
                    {
                        AppSrv.g_Log.DebugCmd($"该设备已经绑定过该账户，请勿重复绑定");
                        err = ErrorCode.ErrorBindCodeAccountAlreadyBindThisDev;
                    }
                    else if (code_isused)
                    {
                        AppSrv.g_Log.DebugCmd($"BindCode 已被使用");
                        err = ErrorCode.ErrorBindCodeAlreadyInUse;
                    }
                    else if (Math.Abs((DateTime.Now - code_createTime).TotalMinutes) > 5)
                    {
                        AppSrv.g_Log.DebugCmd($"BindCode 超时");
                        err = ErrorCode.ErrorBindCodeTimeOut;
                    }
                }

                //检查用户是否存在
                if (err == ErrorCode.ErrorOk)
                {
                    try
                    {
                        query = "SELECT uid,nikename from users where uid = ?target";
                        using (var command = new MySqlCommand(query, conn))
                        {
                            command.Parameters.AddWithValue("?target", code_targetuid);
                            long tmp_targetuidcheck = -1;
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    tmp_targetuidcheck = reader.GetInt64(0);
                                    _resp.MoveToNickName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                                }
                            }
                            if (tmp_targetuidcheck <= 0)
                            {
                                err = ErrorCode.ErrorBindCodeAccCant;
                                AppSrv.g_Log.DebugCmd($"用户不存在，{_resp.MoveStarCount}个");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        err = ErrorCode.ErrorBindCodeException;
                        AppSrv.g_Log.DebugCmd($"迁移异常，{e.ToString()}");
                    }
                }

                //开始处理
                if (err == ErrorCode.ErrorOk)
                {
                    try
                    {
                        query = "update rom_stars set uid = ?target where uid = ?src";
                        using (var command = new MySqlCommand(query, conn))
                        {
                            command.Parameters.AddWithValue("?target", code_targetuid);
                            command.Parameters.AddWithValue("?src", _c.UID);
                            _resp.MoveStarCount = command.ExecuteNonQuery();
                            AppSrv.g_Log.DebugCmd($"迁移收藏，{_resp.MoveStarCount}个");
                        }

                        query = "update user_gamesavedata set uid = ?target where uid = ?src";
                        using (var command = new MySqlCommand(query, conn))
                        {
                            command.Parameters.AddWithValue("?target", code_targetuid);
                            command.Parameters.AddWithValue("?src", _c.UID);
                            _resp.MoveSavCount = command.ExecuteNonQuery();
                            AppSrv.g_Log.DebugCmd($"迁移存档，{_resp.MoveSavCount}个");
                        }
                    }
                    catch (Exception e)
                    {
                        err = ErrorCode.ErrorBindCodeException;
                        AppSrv.g_Log.DebugCmd($"迁移异常，{e.ToString()}");
                    }
                }

                //修改账户
                if (err == ErrorCode.ErrorOk)
                {
                    try
                    {
                        query = "update users set parentuid = ?target where uid = ?src";
                        using (var command = new MySqlCommand(query, conn))
                        {
                            command.Parameters.AddWithValue("?target", code_targetuid);
                            command.Parameters.AddWithValue("?src", _c.UID);
                            _resp.MoveStarCount = command.ExecuteNonQuery();
                            AppSrv.g_Log.DebugCmd($"{_c.UID}修改parentui->{code_targetuid}");
                        }

                        query = "UPDATE user_bindcode set isused = TRUE,usedtime = now(),useduid = ?src where id = ?id";
                        using (var command = new MySqlCommand(query, conn))
                        {
                            command.Parameters.AddWithValue("?src", _c.UID);
                            command.Parameters.AddWithValue("?src", code_id);
                            _resp.MoveStarCount = command.ExecuteNonQuery();
                            AppSrv.g_Log.DebugCmd($"标记code为不可用");
                        }
                    }
                    catch (Exception e)
                    {
                        err = ErrorCode.ErrorBindCodeException;
                        AppSrv.g_Log.DebugCmd($"迁移异常，{e.ToString()}");
                    }
                }

                //内存中父ID也发生变化 （虽然客户端请求结束后要重新登录，但这里修改，避免存档等并发上传）
                if (err == ErrorCode.ErrorOk)
                {
                    _c.ParentUID = code_targetuid;
                    _c.NickName = _resp.MoveToNickName;
                }
            }

            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdGamesavGetGameSavList, (int)err, ProtoBufHelper.Serizlize(_resp));
        }
    }
}