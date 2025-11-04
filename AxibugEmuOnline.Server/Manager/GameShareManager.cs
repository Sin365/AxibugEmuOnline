using AxibugEmuOnline.Server.Common;
using AxibugEmuOnline.Server.Manager.Client;
using AxibugEmuOnline.Server.NetWork;
using AxibugProtobuf;
using MySql.Data.MySqlClient;
using System.Net.Sockets;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace AxibugEmuOnline.Server.Manager
{
    public class GameShareManager
    {
        Dictionary<int, RomPlatformType> mDictRomID2Platform = new Dictionary<int, RomPlatformType>();
        public GameShareManager()
        {
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdGameMark, RecvGameMark);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdGamescreenImgUpload, RecvGameScreenCoverImgUpload);
        }


        public void RecvGameMark(Socket _socket, byte[] reqData)
        {
            AppSrv.g_Log.DebugCmd("RecvGameMark");
            Protobuf_Game_Mark msg = ProtoBufHelper.DeSerizlize<Protobuf_Game_Mark>(reqData);
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(_socket);
            Protobuf_Game_Mark_RESP respData = new Protobuf_Game_Mark_RESP();

            using (MySqlConnection conn = SQLRUN.GetConn("RecvGameMark"))
            {
                try
                {
                    string query = "SELECT id from rom_stars where uid = ?uid and romid = ?romid";
                    bool bHad = false;
                    using (var command = new MySqlCommand(query, conn))
                    {
                        // 设置参数值
                        command.Parameters.AddWithValue("?uid", _c.UID);
                        command.Parameters.AddWithValue("?romid", msg.RomID);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                bHad = true;
                                break;
                            }
                        }
                    }

                    //收藏
                    if (msg.Motion == 1)
                    {
                        if (bHad)
                        {
                            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdGameMark, (int)ErrorCode.ErrorRomAlreadyHadStar, ProtoBufHelper.Serizlize(respData));
                            return;
                        }
                        else
                        {
                            query = "INSERT INTO `haoyue_emu`.`rom_stars` (`uid`,  `romid`) VALUES (?uid,  ?romid);";
                            using (var command = new MySqlCommand(query, conn))
                            {
                                // 设置参数值
                                command.Parameters.AddWithValue("?uid", _c.UID);
                                command.Parameters.AddWithValue("?romid", msg.RomID);
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    else//取消收藏
                    {
                        if (bHad)
                        {
                            query = "DELETE from rom_stars where uid = ?uid and romid = ?romid";
                            using (var command = new MySqlCommand(query, conn))
                            {
                                // 设置参数值
                                command.Parameters.AddWithValue("?uid", _c.UID);
                                command.Parameters.AddWithValue("?romid", msg.RomID);
                                command.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdGameMark, (int)ErrorCode.ErrorRomDontHadStar, ProtoBufHelper.Serizlize(respData));
                            return;
                        }
                    }
                    //更新收藏数
                    query = "update romlist set stars = (SELECT COUNT(id) from rom_stars where rom_stars.romid = ?romid) where romlist.id = ?romid";
                    using (var command = new MySqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("?romid", msg.RomID);
                        command.ExecuteNonQuery();
                    }

                }
                catch (Exception e)
                {
                }
            }

            respData.Stars = GetRomStart(msg.RomID);
            respData.IsStar = CheckIsRomStar(msg.RomID, _c.UID) ? 1 : 0;

            respData.RomID = msg.RomID;
            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdGameMark, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(respData));
        }

        private void RecvGameScreenCoverImgUpload(Socket _socket, byte[] reqData)
        {
            AppSrv.g_Log.DebugCmd("RecvGameScreenCoverImgUpload");
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(_socket);
            Protobuf_GameScreen_Img_Upload msg = ProtoBufHelper.DeSerizlize<Protobuf_GameScreen_Img_Upload>(reqData);
            Protobuf_GameScreen_Img_Upload_RESP respData = new Protobuf_GameScreen_Img_Upload_RESP();
            CheckRomHadCover(msg.RomID, out bool bhadGame, out bool bHadCover, out string coverPath);
            ErrorCode errCode = ErrorCode.ErrorOk;
            if (!bhadGame || bHadCover)
            {
                errCode = ErrorCode.ErrorRomAlreadyHadCoverimg;
            }

            if (errCode == ErrorCode.ErrorOk)
            {
                Helper.FileDelete(Path.Combine(Config.cfg.wwwRootPath, coverPath));
                byte[] ImgData = msg.SavImg.ToArray();
                string imgpath = Path.Combine("UpCover", $"{_c.UID}_{msg.RomID}.jpg");

                ImgData = Helper.DecompressByteArray(ImgData);

                if (Helper.CreateFile(Path.Combine(Config.cfg.wwwRootPath, imgpath), ImgData))
                {
                    string query = "update romlist set ImgUrl = ?imgUrl WHERE id = ?romid;";
                    using (MySqlConnection conn = SQLRUN.GetConn("RecvGameScreenCoverImgUpload_updateImg"))
                    {
                        using (var command = new MySqlCommand(query, conn))
                        {
                            // 设置参数值
                            command.Parameters.AddWithValue("?romid", msg.RomID);
                            command.Parameters.AddWithValue("?imgUrl", imgpath);
                            if (command.ExecuteNonQuery() < 1)
                            {
                                AppSrv.g_Log.Error("执行即时存档保存失败");
                            }
                        }
                    }
                    query = "INSERT INTO `haoyue_emu`.`uploadcover_log` (`uid`, `romid`) VALUES (?uid, ?romid);";
                    using (MySqlConnection conn = SQLRUN.GetConn("RecvGameScreenCoverImgUpload_insertlog"))
                    {
                        using (var command = new MySqlCommand(query, conn))
                        {
                            // 设置参数值
                            command.Parameters.AddWithValue("?uid", _c.UID);
                            command.Parameters.AddWithValue("?romid", msg.RomID);
                            if (command.ExecuteNonQuery() < 1)
                            {
                                AppSrv.g_Log.Error("执行即时存档保存失败");
                            }
                        }
                    }
                }
                else
                {
                    errCode = ErrorCode.ErrorRomFailCoverimg;
                }
            }
            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdGamescreenImgUpload, (int)errCode, ProtoBufHelper.Serizlize(respData));
        }

        public int GetRomStart(int RomId)
        {
            int stars = 0;
            using (MySqlConnection conn = SQLRUN.GetConn("GetStart"))
            {
                try
                {
                    string query = $"SELECT `stars` FROM romlist where id = ?romid;";
                    using (var command = new MySqlCommand(query, conn))
                    {
                        // 设置参数值  
                        command.Parameters.AddWithValue("?RomID", RomId);
                        // 执行查询并处理结果  
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                stars = reader.GetInt32(0);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    AppSrv.g_Log.Error(e);
                }
            }
            return stars;
        }

        public bool CheckIsRomStar(int RomId, long uid)
        {
            bool bhad = false;
            using (MySqlConnection conn = SQLRUN.GetConn("CheckIsRomStart"))
            {
                try
                {
                    string query = $"SELECT count(id) from rom_stars where uid = ?uid and romid = ?romid";
                    using (var command = new MySqlCommand(query, conn))
                    {
                        // 设置参数值  
                        command.Parameters.AddWithValue("?romid", RomId);
                        command.Parameters.AddWithValue("?uid", uid);
                        // 执行查询并处理结果  
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                bhad = reader.GetInt32(0) > 0;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    AppSrv.g_Log.Error("CheckIsRomStar：" + e);
                }
            }
            return bhad;
        }


        public void CheckRomHadCover(int RomId, out bool bhadGame, out bool bHadCover, out string coverPath)
        {
            bhadGame = false;
            bHadCover = false;
            coverPath = string.Empty;
            using (MySqlConnection conn = SQLRUN.GetConn("CheckRomHadCover"))
            {
                try
                {
                    string query = $"SELECT id,ImgUrl from romlist where Id = ?romid";
                    using (var command = new MySqlCommand(query, conn))
                    {
                        // 设置参数值  
                        command.Parameters.AddWithValue("?romid", RomId);
                        // 执行查询并处理结果  
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                bhadGame = reader.GetInt32(0) > 0;
                                coverPath = reader.GetString(1);
                                if (!string.IsNullOrEmpty(coverPath))
                                    bHadCover = true;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    AppSrv.g_Log.Error("CheckRomHadCover：" + e);
                }
            }
        }

        public RomPlatformType GetRomPlatformType(int RomID)
        {
            if (mDictRomID2Platform.TryGetValue(RomID, out RomPlatformType ptype))
                return ptype;

            ptype = RomPlatformType.Invalid;
            using (MySqlConnection conn = SQLRUN.GetConn("GetRomPlatformType"))
            {
                try
                {
                    string query = "SELECT PlatformType from romlist where Id = ?RomID ";
                    using (var command = new MySqlCommand(query, conn))
                    {
                        // 设置参数值  
                        command.Parameters.AddWithValue("?RomID", RomID);
                        // 执行查询并处理结果  
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ptype = (RomPlatformType)reader.GetInt32(0);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    AppSrv.g_Log.Error(e);
                }
            }

            if (ptype == RomPlatformType.Invalid)
                AppSrv.g_Log.Error($"RomID {RomID} 没找到平台配置");
            else
                mDictRomID2Platform[RomID] = ptype;

            return ptype;
        }
    }
}
