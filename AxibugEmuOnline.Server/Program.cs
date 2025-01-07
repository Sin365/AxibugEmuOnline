using AxibugEmuOnline.Server.Common;
using AxibugEmuOnline.Server.Manager;
using MySql.Data.MySqlClient;

namespace AxibugEmuOnline.Server
{
    internal class Program
    {
        static string Title = "AxibugEmuOnline.Server";
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Title = Title;
            AppSrv.InitServer(10492);
            while (true)
            {
                string CommandStr = Console.ReadLine();
                string Command = "";
                Command = ((CommandStr.IndexOf(" ") <= 0) ? CommandStr : CommandStr.Substring(0, CommandStr.IndexOf(" ")));
                switch (Command)
                {
                    case "rlist":
                        {
                            List<Data_RoomData> roomlist = ObjectPoolAuto.AcquireList<Data_RoomData>();
                            AppSrv.g_Room.GetRoomList(ref roomlist);

                            AppSrv.g_Log.Info($"RoomCount:{roomlist.Count}");
                            foreach (var room in roomlist)
                            {
                                AppSrv.g_Log.Info($"-----    RoomID:{room.RoomID}   -----");
                                AppSrv.g_Log.Info($"GameRomID:{room.GameRomID}");
                                AppSrv.g_Log.Info($"RomPlatformType:{room.GameRomPlatformType}");
                                AppSrv.g_Log.Info($"GameState:{room.GameState}");
                                AppSrv.g_Log.Info($"HostUID:{room.HostUID}");
                                AppSrv.g_Log.Info($"mCurrFrameId:{room.mCurrServerFrameId}");
                                AppSrv.g_Log.Info($"SrvForwardFrames:{room.SrvForwardFrames}");
                                AppSrv.g_Log.Info($"room.send2time.Count:{room.send2time.Count}");
                                AppSrv.g_Log.Info($"input all:{room.mCurrInputData.all}");
                                AppSrv.g_Log.Info($"input p1:{room.mCurrInputData.p1_byte}");
                                AppSrv.g_Log.Info($"input p2:{room.mCurrInputData.p2_byte}");
                                AppSrv.g_Log.Info($"input p3:{room.mCurrInputData.p3_byte}");
                                AppSrv.g_Log.Info($"input p4:{room.mCurrInputData.p4_byte}");
                                AppSrv.g_Log.Info($"GetPlayerCount:{room.GetPlayerCount()}");
                                for (int i = 0; i < room.PlayerSlot.Length; i++)
                                {
                                    AppSrv.g_Log.Info($"    P{i}：");

                                    if (AppSrv.g_ClientMgr.GetClientByUID(room.PlayerSlot[i].UID, out ClientInfo _c))
                                    {
                                        AppSrv.g_Log.Info($"    UID->{room.PlayerSlot[i].UID}");
                                        AppSrv.g_Log.Info($"    NickName->{_c.NickName}");
                                        AppSrv.g_Log.Info($"    AveNetDelay->{_c.AveNetDelay}");
                                        AppSrv.g_Log.Info($"    LocalJoyIdx->{room.PlayerSlot[i].LocalJoyIdx}");
                                    }
                                    else
                                    {
                                        AppSrv.g_Log.Info($"    None");
                                    }
                                }
                            }
                            ObjectPoolAuto.Release(roomlist);
                        }
                        break;
                    case "list":
                        {
                            AppSrv.g_Log.Info("当前在线:" + AppSrv.g_ClientMgr.GetOnlineClient());
                            var onlinelist = AppSrv.g_ClientMgr.GetOnlineClientList();
                            for (int i = 0; i < onlinelist.Count; i++)
                            {
                                ClientInfo cinfo = onlinelist[i];
                                AppSrv.g_Log.Info($"UID->{cinfo.UID}   Name->{cinfo.NickName}  Ping->{cinfo.AveNetDelay}");
                            }
                        }
                        break;
                    case "updatehash":
                        {
                            UpdateRomHash();
                        }
                        break;
                    default:
                        Console.WriteLine("未知命令" + CommandStr);
                        break;
                }
            }
        }


        static void UpdateRomHash()
        {
            AppSrv.g_Log.Info("UpdateRomHash");
            MySqlConnection conn = Haoyue_SQLPoolManager.DequeueSQLConn("UpdateRomHash");
            try
            {
                List<(int id, string romurl, string name)> list = new List<(int id, string romurl, string name)>();
                List<(int id, string romurl, string name)> Nonelist = new List<(int id, string romurl, string name)>();

                string query = $"SELECT id,`Name`,GameType,Note,RomUrl,ImgUrl,`Hash` FROM romlist";
                using (var command = new MySqlCommand(query, conn))
                {
                    // 执行查询并处理结果  
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(
                                (reader.GetInt32(0),
                                !reader.IsDBNull(4) ? reader.GetString(4) : string.Empty,
                                !reader.IsDBNull(1) ? reader.GetString(1) : string.Empty
                                ));
                        }
                    }
                }

                for (int i = 0; i < list.Count; i++)
                {
                    string rompath = Config.cfg.RomDir + "/" + list[i].romurl;
                    rompath = System.Net.WebUtility.UrlDecode(rompath);
                    if (!File.Exists(rompath))
                    {
                        Nonelist.Add(list[i]);
                        continue;
                    }
                    string romhash = Helper.FileMD5Hash(rompath);
                    AppSrv.g_Log.Info($"第{i}个，Name->{list[i].name},Hash->{romhash}");
                    query = $"update romlist SET `Hash` = '{romhash}' where Id ={list[i].id}";
                    using (var command = new MySqlCommand(query, conn))
                    {
                        // 执行查询并处理结果  
                        int reader = command.ExecuteNonQuery();
                        if (reader > 0)
                            AppSrv.g_Log.Info($"第{i}个，处理成功");
                        else
                            AppSrv.g_Log.Info($"第{i}个，处理失败");
                    }
                }
                AppSrv.g_Log.Info($"处理完毕，共{Nonelist.Count}个文件没有找到");
            }
            catch (Exception e)
            {
                AppSrv.g_Log.Info($"err:{e.ToString()}");
            }
            Haoyue_SQLPoolManager.EnqueueSQLConn(conn);
        }
    }
}