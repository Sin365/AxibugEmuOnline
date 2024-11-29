using AxibugEmuOnline.Server.Common;
using AxibugEmuOnline.Server.Event;
using AxibugEmuOnline.Server.NetWork;
using AxibugProtobuf;
using MySql.Data.MySqlClient;
using System.Net.Sockets;
using static AxibugEmuOnline.Server.RoomManager;

namespace AxibugEmuOnline.Server.Manager
{
    public class GameShareManager
    {
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
                        query = "INSERT INTO `haoyue_emu`.`rom_stars` (`uid`, `platform`, `romid`) VALUES (?uid, ?platform, ?romid);";
                        using (var command = new MySqlCommand(query, conn))
                        {
                            // 设置参数值
                            command.Parameters.AddWithValue("?uid", _c.UID);
                            command.Parameters.AddWithValue("?platform", (int)msg.PlatformType);
                            command.Parameters.AddWithValue("?romid", msg.RomID);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                else//取消收藏
                {
                    if (bHad)
                    {
                        query = "DELETE from rom_stars where uid = ?uid and romid = ?romid and platform = ?platform";
                        using (var command = new MySqlCommand(query, conn))
                        {
                            // 设置参数值
                            command.Parameters.AddWithValue("?uid", _c.UID);
                            command.Parameters.AddWithValue("?platform", (int)msg.PlatformType);
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
                query = "update romlist_nes set stars = (SELECT COUNT(id) from rom_stars where rom_stars.romid = ?romid and rom_stars.platform = ?platform) where romlist_nes.id = ?romid";
                using (var command = new MySqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("?platform", (int)msg.PlatformType);
                    command.Parameters.AddWithValue("?romid", msg.RomID);
                    command.ExecuteNonQuery();
                }

            }
            catch (Exception e)
            {
            }
            Haoyue_SQLPoolManager.EnqueueSQLConn(conn);
            respData.PlatformType = msg.PlatformType;
            respData.RomID = msg.RomID;
            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdGameMark, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(respData));
        }
    }
}
