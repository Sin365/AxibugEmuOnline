using AxibugEmuOnline.Server.Manager;
using AxibugEmuOnline.Server.NetWork;
using System.Net;

namespace AxibugEmuOnline.Server
{

    public static class AppSrv
    {
        public static ClientManager g_ClientMgr;
        public static LogManager g_Log;
        public static LoginManager g_Login;
        public static ChatManager g_Chat;
        public static IOCPNetWork g_SocketMgr;
        public static RoomManager g_Room;
        public static GameManager g_Game;

        public static void InitServer(int port)
        {
            g_ClientMgr = new ClientManager();
            g_ClientMgr.Init(45000, 120);
            g_Log = new LogManager();
            g_Login = new LoginManager();
            g_Chat = new ChatManager();
            //g_SocketMgr = new IOCPNetWork(1024, 1024);
            g_SocketMgr = new IOCPNetWork(1024, 4096);
            g_Room = new RoomManager();
            g_Game = new GameManager();

            g_SocketMgr.Init();
            g_SocketMgr.Start(new IPEndPoint(IPAddress.Any.Address, port));
            Console.WriteLine("Succeed!");
        }
    }
}