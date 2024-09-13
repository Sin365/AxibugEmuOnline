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