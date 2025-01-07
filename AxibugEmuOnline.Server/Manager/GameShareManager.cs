using AxibugEmuOnline.Server.Common;
using AxibugEmuOnline.Server.NetWork;
using AxibugProtobuf;
using MySql.Data.MySqlClient;
using System.Net.Sockets;
using System.Security.Policy;

namespace AxibugEmuOnline.Server.Manager
{
    public class GameShareManager
    {
        Dictionary<int, RomPlatformType> mDictRomID2Platform = new Dictionary<int, RomPlatformType>();
        public GameShareManager()
        {
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdGameMark, RecvGameMark);
        }

        public void RecvGameMark(Socket _socket, byte[] reqData)
        {
            Protobuf_Game_Mark msg = ProtoBufHelper.DeSerizlize<Protobuf_Game_Mark>(reqData);
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(_socket);
            Protobuf_Game_Mark_RESP respData = new Protobuf_Game_Mark_RESP();

            MySqlConnection conn = Haoyue_SQLPoolManager.DequeueSQLConn("RecvGameMark");
            try
            {
                string query = "SELECT id from rom_stars where uid = ?uid and romid = ?platform and platform =  ?romid";
                bool bHad = false;
                using (var command = new MySqlCommand(query, conn))
                {
                    // 设置参数值
                    command.Parameters.AddWithValue("?uid", _c.UID);
                    command.Parameters.AddWithValue("?platform", 1);
                    command.Parameters.AddWithValue("?romid", msg.RomID);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader.GetInt32(0) > 0)
                                bHad = true;
                        }
                    }
                }

                if (msg.State == 0)
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
            Haoyue_SQLPoolManager.EnqueueSQLConn(conn);
            respData.RomID = msg.RomID;
            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdGameMark, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(respData));
        }

        public RomPlatformType GetRomPlatformType(int RomID)
        {
            if (mDictRomID2Platform.TryGetValue(RomID, out RomPlatformType ptype))
                return ptype;

            ptype = RomPlatformType.Invalid;
            MySqlConnection conn = Haoyue_SQLPoolManager.DequeueSQLConn("GetRomPlatformType");
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

            if (ptype == RomPlatformType.Invalid)
                AppSrv.g_Log.Error($"RomID {RomID} 没找到平台配置");

            Haoyue_SQLPoolManager.EnqueueSQLConn(conn);

            return ptype;
        }
    }
}
