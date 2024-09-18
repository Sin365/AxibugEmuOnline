using AxibugEmuOnline.Server.Manager;

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
                            var roomlist = AppSrv.g_Room.GetRoomList();
                            AppSrv.g_Log.Info($"RoomCount:{roomlist.Count}");
                            foreach (var room in roomlist)
                            {
                                AppSrv.g_Log.Info($"-----    RoomID:{room.RoomID}   -----");
                                AppSrv.g_Log.Info($"GameRomID:{room.GameRomID}");
                                AppSrv.g_Log.Info($"GameState:{room.GameState}");
                                AppSrv.g_Log.Info($"HostUID:{room.HostUID}");
                                AppSrv.g_Log.Info($"mCurrFrameId:{room.mCurrFrameId}");
                                AppSrv.g_Log.Info($"input all:{room.mCurrInputData.all}");
                                AppSrv.g_Log.Info($"input p1:{room.mCurrInputData.p1_byte}");
                                AppSrv.g_Log.Info($"input p2:{room.mCurrInputData.p2_byte}");
                                AppSrv.g_Log.Info($"input p3:{room.mCurrInputData.p3_byte}");
                                AppSrv.g_Log.Info($"input p4:{room.mCurrInputData.p4_byte}");
                                AppSrv.g_Log.Info($"GetPlayerCount:{room.GetPlayerCount()}");
                                for (int i = 0; i < 4; i++)
                                {
                                    AppSrv.g_Log.Info($"    P{i}：");
                                    if (room.GetPlayerClientByIdx(i, out ClientInfo _c))
                                    {
                                        AppSrv.g_Log.Info($"    UID->{_c.UID}");
                                        AppSrv.g_Log.Info($"    NickName->{_c.NickName}");
                                        AppSrv.g_Log.Info($"    AveNetDelay->{_c.AveNetDelay}");
                                    }
                                    else
                                    {
                                        AppSrv.g_Log.Info($"    None");
                                    }
                                }
                            }
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
                    default:
                        Console.WriteLine("未知命令" + CommandStr);
                        break;
                }
            }
        }
    }
}