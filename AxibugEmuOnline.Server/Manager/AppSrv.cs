using AxibugEmuOnline.Server.Common;
using AxibugEmuOnline.Server.Manager;
using AxibugEmuOnline.Server.NetWork;
using System.Net;

namespace AxibugEmuOnline.Server
{

    public static class AppSrv
    {
        public static TickManager g_Tick;
        public static ClientManager g_ClientMgr;
        public static LogManager g_Log;
        public static LoginManager g_Login;
        public static ChatManager g_Chat;
        public static UserManager g_UserMgr;
        public static IOCPNetWork g_SocketMgr;
        public static RoomManager g_Room;
        public static GameShareManager g_GameShareMgr;
        private static SavDataManager g_GameSavMgr;

        public static void InitServer(int port)
        {
            g_Log = new LogManager();
            Config.LoadConfig();
            AESHelper.LoadKeyIVCfg(Config.cfg.AesKey, Config.cfg.AesIv);
            SQLRUN.InitConnMgr();
            g_Tick = new TickManager();
            g_ClientMgr = new ClientManager();
            g_ClientMgr.Init(45000, 120);
            g_Login = new LoginManager();
            g_Chat = new ChatManager();
            g_UserMgr = new UserManager();
            g_SocketMgr = new IOCPNetWork(1024, 4096 * 2);
            g_Room = new RoomManager();
            g_GameShareMgr = new GameShareManager();
            g_GameSavMgr = new SavDataManager();

            g_SocketMgr.Init();
            g_SocketMgr.Start(new IPEndPoint(IPAddress.Any.Address, port));
            Console.WriteLine("Succeed!");
        }
    }
}