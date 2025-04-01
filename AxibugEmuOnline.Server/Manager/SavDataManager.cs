using AxibugEmuOnline.Server.Common;
using AxibugEmuOnline.Server.Manager.Client;
using AxibugEmuOnline.Server.NetWork;
using AxibugProtobuf;
using MySql.Data.MySqlClient;
using System.Net.Sockets;

namespace AxibugEmuOnline.Server.Manager
{
    public class SavDataManager
    {
        public SavDataManager()
        {
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdGamesavGetGameSavList, RecvGetGameSavList);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdGamesavDelGameSav, RecvDelGameSav);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdGamesavUploadGameSav, RecvUpLoadGameSav);
        }

        public void RecvGetGameSavList(Socket _socket, byte[] reqData)
        {
            AppSrv.g_Log.DebugCmd("RecvGetGameSavList");
            Protobuf_Mine_GetGameSavList msg = ProtoBufHelper.DeSerizlize<Protobuf_Mine_GetGameSavList>(reqData);
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(_socket);
            Protobuf_Mine_GetGameSavList_RESP respData = new Protobuf_Mine_GetGameSavList_RESP();

            respData.RomID = msg.RomID;
            Protobuf_Mine_GameSavInfo nulldata = null;
            respData.SavDataList.Add(nulldata);
            respData.SavDataList.Add(nulldata);
            respData.SavDataList.Add(nulldata);
            respData.SavDataList.Add(nulldata);

            string query = "SELECT `id`,`uid`,`romid`, `savidx`, `savName`,`savNote`, `savUrl`,`savImgUrl`, `savDate` from user_gamesavedata where uid = ?uid and romid = ?romid";
            bool bHad = false;
            using (MySqlConnection conn = SQLRUN.GetConn("RecvGameMark"))
            {
                using (var command = new MySqlCommand(query, conn))
                {
                    // 设置参数值
                    command.Parameters.AddWithValue("?uid", _c.UID);
                    command.Parameters.AddWithValue("?romid", msg.RomID);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Protobuf_Mine_GameSavInfo resp = new Protobuf_Mine_GameSavInfo()
                            {
                                BHadSaveData = true,
                                SavID = reader.GetInt64(0),
                                Uid = reader.GetInt64(1),
                                RomID = reader.GetInt32(2),
                                SavDataIdx = reader.GetInt32(3),
                                SavName = reader.GetString(4),
                                Note = reader.GetString(5),
                                SavUrl = reader.GetString(6),
                                SavImgUrl = reader.GetString(7),
                                SavDate = reader.GetDateTime(8).ToString(),
                                GamePlatformType = AppSrv.g_GameShareMgr.GetRomPlatformType(msg.RomID)
                            };
                            respData.SavDataList[resp.SavDataIdx] = resp;
                        }
                    }
                }
            }

            respData.RomID = msg.RomID;
            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdGamesavGetGameSavList, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(respData));
        }

        public void RecvDelGameSav(Socket _socket, byte[] reqData)
        {
            Protobuf_Mine_DelGameSav msg = ProtoBufHelper.DeSerizlize<Protobuf_Mine_DelGameSav>(reqData);
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(_socket);
            Protobuf_Mine_DelGameSav_RESP respData = new Protobuf_Mine_DelGameSav_RESP();
            ErrorCode errCode = ErrorCode.ErrorOk;
            respData.RomID = msg.RomID;
            bool bHad = false; string SavUrl = null; string SavImgUrl = null;

            string query = "SELECT `savUrl`,`savImgUrl`, `savDate` from user_gamesavedata where uid = ?uid and romid = ?romid and savidx = ?savidx";
            using (MySqlConnection conn = SQLRUN.GetConn("RecvGameMark"))
            {
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
                            SavUrl = reader.GetString(0);
                            SavImgUrl = reader.GetString(1);
                        }
                    }
                }

                if (!bHad)
                {
                    errCode = ErrorCode.ErrorRomDontHadSavedata;
                }
                else
                {
                    bool bDelSav = FileDelete(Path.Combine(Config.cfg.savDataPath, SavUrl));
                    bool bDelImg = FileDelete(Path.Combine(Config.cfg.savDataPath, SavImgUrl));
                    if (!bDelSav || !bDelImg)
                    {
                        errCode = ErrorCode.ErrorRomDontHadSavedata;
                    }
                    else
                    {
                        try
                        {
                            query = "delete from user_gamesavedata where uid = ?uid and romid = ?romid and savidx = ?savidx";
                            using (var command = new MySqlCommand(query, conn))
                            {
                                // 设置参数值
                                command.Parameters.AddWithValue("?uid", _c.UID);
                                command.Parameters.AddWithValue("?romid", msg.RomID);
                                if (command.ExecuteNonQuery() < 1)
                                {
                                    AppSrv.g_Log.Error("删除即时存档，但是他并没有.");
                                }
                            }
                        }
                        catch { }
                    }
                }
            }


            if (errCode == ErrorCode.ErrorOk)
            {
                respData.RomID = msg.RomID;
                respData.SavDataIdx = msg.SavDataIdx;
            }

            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdGamesavGetGameSavList, (int)errCode, ProtoBufHelper.Serizlize(respData));
        }

        public void RecvUpLoadGameSav(Socket _socket, byte[] reqData)
        {
            Protobuf_Mine_UpLoadGameSav msg = ProtoBufHelper.DeSerizlize<Protobuf_Mine_UpLoadGameSav>(reqData);
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(_socket);
            Protobuf_Mine_UpLoadGameSav_RESP respData = new Protobuf_Mine_UpLoadGameSav_RESP();
            ErrorCode errCode = ErrorCode.ErrorOk;
            respData.RomID = msg.RomID;

            RomPlatformType ptype = AppSrv.g_GameShareMgr.GetRomPlatformType(msg.RomID);

            if (GetProtobufMineGameSavInfo(_c.UID, msg.RomID, msg.SavDataIdx, out Protobuf_Mine_GameSavInfo oldSavInfo))
            {
                bool bDelSav = FileDelete(Path.Combine(Config.cfg.savDataPath, oldSavInfo.SavUrl));
                bool bDelImg = FileDelete(Path.Combine(Config.cfg.savDataPath, oldSavInfo.SavImgUrl));
                if (!bDelSav || !bDelImg)
                {
                    errCode = ErrorCode.ErrorRomDontHadSavedata;
                }

                if (!DeleteProtobufMineGameSavInfo(_c.UID, msg.RomID, msg.SavDataIdx))
                {
                    //删除失败
                    errCode = ErrorCode.ErrorRomDontHadSavedata;
                }
            }

            if (errCode == ErrorCode.ErrorOk)
            {
                byte[] StateRawData = msg.StateRaw.ToArray();
                byte[] ImgData = msg.SavImg.ToArray();
                GetNewRomPath(_c.UID, ptype, msg.RomID, msg.SavDataIdx, $"{msg.SavDataIdx}.axisav", out string rompath);
                GetNewRomPath(_c.UID, ptype, msg.RomID, msg.SavDataIdx, $"{msg.SavDataIdx}.axiimg", out string imgpath);

                if (!CreateFile(Path.Combine(Config.cfg.savDataPath, rompath), StateRawData)
                    ||
                !CreateFile(Path.Combine(Config.cfg.savDataPath, imgpath), StateRawData)
                    )
                {
                    //INSERT INTO `haoyue_emu`.`user_gamesavedata` ( `uid`, `romid`, `savidx`, `savName`, `savNote`, `savUrl`, `savImgUrl`, `savDate`) VALUES ( 0, 0, 2147483647, '', '', '', '', '0000-00-00 00:00:00');
                    string query = "INSERT INTO `haoyue_emu`.`user_gamesavedata`" +
                        " ( `uid`, `romid`, `savidx`, `savName`, `savNote`, `savUrl`, `savImgUrl`, `savDate`)" +
                        " VALUES ( ?uid, ?romid, ?savidx, ?savName, ?savNote, ?savUrl, ?savImgUrl, ?savDate);";


                    using (MySqlConnection conn = SQLRUN.GetConn("RecvUpLoadGameSav"))
                    {
                        using (var command = new MySqlCommand(query, conn))
                        {
                            // 设置参数值
                            command.Parameters.AddWithValue("?uid", _c.UID);
                            command.Parameters.AddWithValue("?romid", msg.RomID);
                            command.Parameters.AddWithValue("?savidx", msg.SavDataIdx);
                            command.Parameters.AddWithValue("?savName", msg.Name);
                            command.Parameters.AddWithValue("?savNote", msg.Note);
                            command.Parameters.AddWithValue("?savUrl", rompath);
                            command.Parameters.AddWithValue("?savImgUrl", imgpath);
                            command.Parameters.AddWithValue("?savDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            if (command.ExecuteNonQuery() < 1)
                            {
                                AppSrv.g_Log.Error("执行即时存档保存失败");
                            }
                        }
                    }
                }
            }


            if (errCode == ErrorCode.ErrorOk)
            {
                if (!GetProtobufMineGameSavInfo(_c.UID, msg.RomID, msg.SavDataIdx, out Protobuf_Mine_GameSavInfo protoData))
                {
                    //不存在
                    errCode = ErrorCode.ErrorRomDontHadSavedata;
                }
                else
                {
                    respData.RomID = msg.RomID;
                    respData.SavDataIdx = msg.SavDataIdx;
                    respData.UploadSevInfo = protoData;
                }
            }

            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdGamesavGetGameSavList, (int)errCode, ProtoBufHelper.Serizlize(respData));
        }
        public void GetNewRomPath(long uid, RomPlatformType ptype, int romid, int stateIdx, string filename, out string path)
        {
            path = Path.Combine("UserSav", uid.ToString(), ptype.ToString(), romid.ToString(), stateIdx.ToString(), filename);
        }
        public bool FileDelete(string path)
        {
            if (!File.Exists(path))
                return false;
            try
            {
                File.Delete(path);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool CreateFile(string path, byte[] data)
        {
            try
            {
                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using (var fs = new FileStream(path, FileMode.Create))
                {
                    fs.Write(data, 0, data.Length);
                }
                return true;
            }
            catch (Exception ex)
            {
                AppSrv.g_Log.Error($"CreeateFile Err =>{ex.ToString()}");
                return false;
            }
        }

        public bool GetProtobufMineGameSavInfo(long uid, int romid, int savIdx, out Protobuf_Mine_GameSavInfo protoData)
        {
            bool bhad = false;
            protoData = default;
            RomPlatformType ptype = AppSrv.g_GameShareMgr.GetRomPlatformType(romid);
            using (MySqlConnection conn = SQLRUN.GetConn("GetProtobufMineGameSavInfo"))
            {
                try
                {
                    string query = "SELECT `id`,`uid`, `romid`, `savidx`, `savName`, `savNote`, `savUrl`, `savImgUrl`, `savDate` from `user_gamesavedata` where uid = ?uid and romid = ?romid and savidx = ?savidx";
                    using (var command = new MySqlCommand(query, conn))
                    {
                        // 设置参数值
                        command.Parameters.AddWithValue("?uid", uid);
                        command.Parameters.AddWithValue("?romid", romid);
                        command.Parameters.AddWithValue("?savidx", savIdx);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                protoData = new Protobuf_Mine_GameSavInfo()
                                {
                                    BHadSaveData = true,
                                    SavID = reader.GetInt64(0),
                                    Uid = reader.GetInt64(1),
                                    RomID = reader.GetInt32(2),
                                    SavDataIdx = reader.GetInt32(3),
                                    SavName = reader.GetString(4),
                                    Note = reader.GetString(5),
                                    SavUrl = reader.GetString(6),
                                    SavImgUrl = reader.GetString(7),
                                    SavDate = reader.GetDateTime(8).ToString("yyyy-MM-dd HH:mm:ss"),
                                    GamePlatformType = ptype
                                };
                                bhad = true;
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                }
            }
            return bhad;
        }

        public bool DeleteProtobufMineGameSavInfo(long uid, int romid, int savIdx)
        {
            bool bDone = false;
            RomPlatformType ptype = AppSrv.g_GameShareMgr.GetRomPlatformType(romid);
            using (MySqlConnection conn = SQLRUN.GetConn("DeleteProtobufMineGameSavInfo"))
            {
                string query = "delete from `user_gamesavedata` where uid = ?uid and romid = ?romid and savidx = ?savidx";
                using (var command = new MySqlCommand(query, conn))
                {
                    // 设置参数值
                    command.Parameters.AddWithValue("?uid", uid);
                    command.Parameters.AddWithValue("?romid", romid);
                    command.Parameters.AddWithValue("?savidx", savIdx);
                    bDone = command.ExecuteNonQuery() > 0;
                }
            }
            return bDone;
        }
    }
}
