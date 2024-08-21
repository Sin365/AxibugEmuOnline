using AxibugEmuOnline.Server.Common;
using AxibugEmuOnline.Server.NetWork;
using AxibugProtobuf;
using System.Net.Sockets;
using System.Timers;

namespace AxibugEmuOnline.Server.Manager
{
    public class ClientInfo
    {
        public long UID { get; set; }
        public string NickName { get; set; }
        public string Account { get; set; }
        public Socket _socket { get; set; }
        public bool IsOffline { get; set; } = false;
        public DateTime LogOutDT { get; set; }
        public DateTime LogInDT { get; set; }
        public UserRoomState RoomState { get; set; } = new UserRoomState();

        public TimeSpan LastStartPingTime { get; set; }
        public int LastPingSeed { get; set; }
        public double NetDelay { get; set; }
    }

    public class UserRoomState
    {
        public int RoomID { get; private set; }
        public int PlayerIdx { get; private set; }
        public UserRoomState()
        {
            ClearRoomData();
        }

        public void SetRoomData(int roomID,int playerIdx)
        {
            RoomID = roomID;
            PlayerIdx = playerIdx;
        }

        public void ClearRoomData()
        {
            RoomID = -1;
            PlayerIdx = -1;
        }
    }

    public class ClientManager
    {
        private List<ClientInfo> ClientList = new List<ClientInfo>();
        private Dictionary<Socket, ClientInfo> _DictSocketClient = new Dictionary<Socket, ClientInfo>();
        private Dictionary<long?, ClientInfo> _DictUIDClient = new Dictionary<long?, ClientInfo>();
        private long TestUIDSeed = 0;

        private System.Timers.Timer clientCheckTimer;
        private AutoResetEvent pingTickARE;
        private long _RemoveOfflineCacheMin;
        private Thread threadPingTick;
        /// <summary>
        ///  初始化并指定检查时间
        /// </summary>
        /// <param name="ticktime">tick检查毫秒数</param>
        /// <param name="RemoveOfflineCache">清理掉线分钟数</param>
        public void Init(long ticktime, long RemoveOfflineCacheMin)
        {
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdPing, OnCmdPing);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdPong, OnCmdPong);

            pingTickARE = AppSrv.g_Tick.AddNewARE(TickManager.TickType.Interval_2000MS);

            //换算成毫秒
            _RemoveOfflineCacheMin = RemoveOfflineCacheMin;
            clientCheckTimer = new System.Timers.Timer();
            clientCheckTimer.Interval = ticktime;
            clientCheckTimer.AutoReset = true;
            clientCheckTimer.Elapsed += new ElapsedEventHandler(ClientCheckClearOffline_Elapsed);
            clientCheckTimer.Enabled = true;

            pingTickARE = AppSrv.g_Tick.AddNewARE(TickManager.TickType.Interval_2000MS);
            threadPingTick = new Thread(PingAllLoop);
            threadPingTick.Start();
        }

        public long GetNextUID()
        {
            return ++TestUIDSeed;
        }

        private void ClientCheckClearOffline_Elapsed(object sender, ElapsedEventArgs e)
        {
            DateTime CheckDT = DateTime.Now.AddMinutes(-1 * _RemoveOfflineCacheMin);
            ClientInfo[] OfflineClientlist = ClientList.Where(w => w.IsOffline == true && w.LogOutDT < CheckDT).ToArray();

            Console.WriteLine("开始清理离线过久的玩家的缓存");
            for (int i = 0; i < OfflineClientlist.Length; i++)
            {
                //to do 掉线处理
                RemoveClient(OfflineClientlist[i]);
            }
            GC.Collect();
        }


        //通用处理
        #region clientlist 处理

        public ClientInfo JoinNewClient(Protobuf_Login data, Socket _socket)
        {
            //也许这个函数需加lock
            ClientInfo cinfo = GetClientForSocket(_socket);
            //如果连接还在
            if (cinfo != null)
            {
                cinfo.IsOffline = false;
            }
            else
            {
                cinfo = new ClientInfo()
                {
                    UID = GetNextUID(),
                    _socket = _socket,
                    Account = data.Account,
                    IsOffline = false,
                };
                AddClient(cinfo);
            }
            return cinfo;
        }


        /// <summary>
        /// 增加用户
        /// </summary>
        /// <param name="client"></param>
        void AddClient(ClientInfo clientInfo)
        {
            try
            {
                Console.WriteLine("追加连接玩家 UID=>" + clientInfo.UID + " | " + clientInfo.Account);
                lock (ClientList)
                {
                    _DictUIDClient.Add(clientInfo.UID, clientInfo);
                    _DictSocketClient.Add(clientInfo._socket, clientInfo);
                    ClientList.Add(clientInfo);
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        /// <summary>
        /// 清理连接
        /// </summary>
        /// <param name="client"></param>
        public void RemoveClient(ClientInfo client)
        {
            lock (ClientList)
            {
                if (_DictUIDClient.ContainsKey(client.UID))
                    _DictUIDClient.Remove(client.UID);

                if (_DictSocketClient.ContainsKey(client._socket))
                    _DictSocketClient.Remove(client._socket);

                ClientList.Remove(client);
            }
        }

        /// <summary>
        /// 清理连接
        /// </summary>
        /// <param name="client"></param>
        public bool GetClientByUID(long uid, out ClientInfo client, bool bNeedOnline = false)
        {
            lock (ClientList)
            {
                if (!_DictUIDClient.ContainsKey(uid))
                {
                    client = null;
                    return false;
                }

                client = _DictUIDClient[uid];


                if (bNeedOnline && client.IsOffline)
                    return false;
                return true;
            }
        }


        public ClientInfo GetClientForSocket(Socket sk)
        {
            return _DictSocketClient.ContainsKey(sk) ? _DictSocketClient[sk] : null;
        }

        /// <summary>
        /// 获取在线玩家
        /// </summary>
        /// <returns></returns>
        public List<ClientInfo> GetOnlineClientList()
        {
            return ClientList.Where(w => w.IsOffline == false).ToList();
        }


        /// <summary>
        /// 设置玩家离线
        /// </summary>
        /// <param name="sk"></param>
        public void SetClientOfflineForSocket(Socket sk)
        {
            if (!_DictSocketClient.ContainsKey(sk))
                return;

            ClientInfo cinfo = _DictSocketClient[sk];
            Console.WriteLine("标记玩家UID" + cinfo.UID + "为离线");
            cinfo.IsOffline = true;
            cinfo.LogOutDT = DateTime.Now;
        }

        public void RemoveClientForSocket(Socket sk)
        {
            if (!_DictSocketClient.ContainsKey(sk))
                return;

            RemoveClient(_DictSocketClient[sk]);
        }

        #endregion

        #region Ping
        void PingAllLoop()
        {
            while (true)
            {
                pingTickARE.WaitOne();
                AppSrv.g_Log.Info("PingAll");
                PingAll();
            }
        }
        void PingAll()
        {
            List<ClientInfo> clientlist = GetOnlineClientList();
            int randSeed = new Random().Next(0, int.MaxValue);
            foreach (var _c in clientlist)
            {
                _c.LastPingSeed = randSeed;
                _c.LastStartPingTime = AppSrv.g_Tick.sw.Elapsed;

                Protobuf_Ping resp = new Protobuf_Ping()
                {
                    Seed = randSeed,
                };
                AppSrv.g_ClientMgr.ClientSend(_c._socket, (int)CommandID.CmdPing, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(resp));
            }
        }
        public void OnCmdPing(Socket sk, byte[] reqData)
        {
            AppSrv.g_Log.Debug($"OnCmdPing");
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            Protobuf_Ping msg = ProtoBufHelper.DeSerizlize<Protobuf_Ping>(reqData);

            //创建成功下行
            Protobuf_Pong resp = new Protobuf_Pong()
            {
                Seed = msg.Seed,
            };
            AppSrv.g_ClientMgr.ClientSend(_c._socket, (int)CommandID.CmdPong, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(resp));
        }
        public void OnCmdPong(Socket sk, byte[] reqData)
        {
            AppSrv.g_Log.Debug($"OnCmdPong");
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            Protobuf_Pong msg = ProtoBufHelper.DeSerizlize<Protobuf_Pong>(reqData);

            if (_c.LastPingSeed == msg.Seed)
            {
                TimeSpan current = AppSrv.g_Tick.sw.Elapsed;
                TimeSpan delta = current - _c.LastStartPingTime;
                _c.NetDelay = delta.TotalSeconds;
            }
        }
        #endregion

        public void ClientSendALL(int CMDID, int ERRCODE, byte[] data, long SkipUID = -1)
        {
            ClientSend(ClientList, CMDID, ERRCODE, data, SkipUID);
        }

        public void ClientSend(List<long> UIDs,int CMDID, int ERRCODE, byte[] data, long SkipUID = -1)
        {
            for (int i = 0; i < UIDs.Count(); i++)
            {
                if (!GetClientByUID(UIDs[i], out ClientInfo _c, true))
                    continue;
                AppSrv.g_SocketMgr.SendToSocket(_c._socket, CMDID, ERRCODE, data);
            }
        }

        /// <summary>
        /// 给一组用户发送数据
        /// </summary>
        /// <param name="_toclientlist"></param>
        /// <param name="CMDID"></param>
        /// <param name="ERRCODE"></param>
        /// <param name="data"></param>
        public void ClientSend(List<ClientInfo> _toclientlist, int CMDID, int ERRCODE, byte[] data,long SkipUID = -1)
        {
            for (int i = 0; i < _toclientlist.Count(); i++)
            {
                if (_toclientlist[i] == null || _toclientlist[i].IsOffline)
                    continue;

                if(SkipUID > -1 && _toclientlist[i].UID == SkipUID)
                    continue;

                AppSrv.g_SocketMgr.SendToSocket(_toclientlist[i]._socket, CMDID, ERRCODE, data);
            }
        }

        public void ClientSend(Socket _socket, int CMDID, int ERRCODE, byte[] data)
        {
            //Console.WriteLine("发送数据 CMDID->"+ CMDID);
            AppSrv.g_SocketMgr.SendToSocket(_socket, CMDID, ERRCODE, data);
        }

        /// <summary>
        /// 给一个连接发送数据
        /// </summary>
        /// <param name="_c"></param>
        /// <param name="CMDID"></param>
        /// <param name="ERRCODE"></param>
        /// <param name="data"></param>
        public void ClientSend(ClientInfo _c, int CMDID, int ERRCODE, byte[] data)
        {
            if (_c == null || _c.IsOffline)
                return;
            AppSrv.g_SocketMgr.SendToSocket(_c._socket, CMDID, ERRCODE, data);
        }

        public int GetOnlineClient()
        {
            return ClientList.Where(w => !w.IsOffline).Count();
        }
    }
}
