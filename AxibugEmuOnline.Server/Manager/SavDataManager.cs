using AxibugEmuOnline.Server.Common;
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
            MySqlConnection conn = SQLPool.DequeueSQLConn("RecvGameMark");
            try
            {
                string query = "SELECT `romid`, `savidx`, `savName`,`savNote`, `savUrl`,`savImgUrl`, `savDate` from user_gamesavedata where uid = ?uid and romid = ?romid";
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
                            Protobuf_Mine_GameSavInfo resp = new Protobuf_Mine_GameSavInfo()
                            {
                                BHadSaveData = true,
                                RomID = reader.GetInt32(0),
                                SavDataIdx = reader.GetInt32(1),
                                SavName = reader.GetString(2),
                                Note = reader.GetString(3),
                                SavUrl = reader.GetString(4),
                                SavImgUrl = reader.GetString(5),
                                SavDate = reader.GetDateTime(6).ToString()
                            };
                            respData.SavDataList[resp.SavDataIdx] = resp;
                        }
                    }
                }

            }
            catch (Exception e)
            {
            }
            SQLPool.EnqueueSQLConn(conn);

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
            MySqlConnection conn = SQLPool.DequeueSQLConn("RecvGameMark");
            try
            {
                string query = "SELECT `savUrl`,`savImgUrl`, `savDate` from user_gamesavedata where uid = ?uid and romid = ?romid and savidx = ?savidx";
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

            }
            catch (Exception e)
            {
            }


            if (!bHad)
            {
                errCode = ErrorCode.ErrorRomDontHadSavedata;
            }
            else
            {
                bool bDelSav = FileDelete(Path.Combine(Config.cfg.savDataPath, SavUrl));
                bool bDelImg = FileDelete(Path.Combine(Config.cfg.savDataPath, SavImgUrl));
                if (bDelSav || !bDelImg)
                {
                    errCode = ErrorCode.ErrorRomDontHadSavedata;
                }
                else
                {
                    
                }
            }

            SQLPool.EnqueueSQLConn(conn);

            respData.RomID = msg.RomID;
            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdGamesavGetGameSavList, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(respData));
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

        public bool FileDelete(string path, byte[] data)
        {
            try
            {
                File.WriteAllBytes(path, data);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
