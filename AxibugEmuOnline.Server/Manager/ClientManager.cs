using AxibugEmuOnline.Server.Common;
using AxibugEmuOnline.Server.Event;
using AxibugEmuOnline.Server.Manager.Client;
using AxibugEmuOnline.Server.NetWork;
using AxibugProtobuf;
using System.Net.Sockets;
using System.Timers;

namespace AxibugEmuOnline.Server.Manager
{
    public class ClientManager
    {
        private HashSet<ClientInfo> mClientList = new HashSet<ClientInfo>();
        private Dictionary<Socket, ClientInfo> mDictSocketClient = new Dictionary<Socket, ClientInfo>();
        private Dictionary<long?, ClientInfo> mDictUIDClient = new Dictionary<long?, ClientInfo>();
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
            ClientInfo[] OfflineClientlist = mClientList.Where(w => w.IsOffline == true && w.LogOutDT < CheckDT).ToArray();

            for (int i = 0; i < OfflineClientlist.Length; i++)
            {
                Console.WriteLine($"清理离线过久玩家 UID->{OfflineClientlist[i].UID} Name->{OfflineClientlist[i].NickName}");
                //掉线处理
                RemoveClient(OfflineClientlist[i]);
            }
            GC.Collect();
        }


        //通用处理
        #region clientlist 处理

        public ClientInfo JoinNewClient(long _uid, Socket _socket)
        {
            //也许这个函数需加lock
            //ClientInfo cinfo = GetClientForSocket(_socket);
            GetClientByUID(_uid, out ClientInfo cinfo, false);
            //如果连接还在
            if (cinfo != null)
            {
                cinfo.IsOffline = false;
                Socket oldsocket = cinfo._socket;
                cinfo._socket = _socket;

                if (mDictSocketClient.ContainsKey(oldsocket))
                    mDictSocketClient.Remove(oldsocket);

                mDictSocketClient[_socket] = cinfo;
            }
            else
            {
                cinfo = new ClientInfo()
                {
                    UID = _uid,
                    _socket = _socket,
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
                lock (mClientList)
                {
                    mDictUIDClient.Add(clientInfo.UID, clientInfo);
                    mDictSocketClient.Add(clientInfo._socket, clientInfo);
                    mClientList.Add(clientInfo);
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
            lock (mClientList)
            {
                if (mDictUIDClient.ContainsKey(client.UID))
                    mDictUIDClient.Remove(client.UID);

                if (mDictSocketClient.ContainsKey(client._socket))
                    mDictSocketClient.Remove(client._socket);

                mClientList.Remove(client);
            }
        }

        /// <summary>
        /// 清理连接
        /// </summary>
        /// <param name="client"></param>
        public bool GetClientByUID(long uid, out ClientInfo client, bool bNeedOnline = false)
        {
            lock (mClientList)
            {
                if (!mDictUIDClient.ContainsKey(uid))
                {
                    client = null;
                    return false;
                }

                client = mDictUIDClient[uid];


                if (bNeedOnline && client.IsOffline)
                    return false;
                return true;
            }
        }

        public ClientInfo GetClientForUID(long UID)
        {
            return mDictUIDClient.ContainsKey(UID) ? mDictUIDClient[UID] : null;
        }

        public ClientInfo GetClientForSocket(Socket sk)
        {
            return mDictSocketClient.ContainsKey(sk) ? mDictSocketClient[sk] : null;
        }

        /// <summary>
        /// 获取在线玩家
        /// </summary>
        /// <returns></returns>
        public List<ClientInfo> GetOnlineClientList()
        {
            return mClientList.Where(w => w.IsOffline == false).ToList();
        }


        /// <summary>
        /// 设置玩家离线
        /// </summary>
        /// <param name="sk"></param>
        public void SetClientOfflineForSocket(Socket sk)
        {
            if (!mDictSocketClient.ContainsKey(sk))
                return;

            ClientInfo cinfo = mDictSocketClient[sk];
            Console.WriteLine("标记玩家UID" + cinfo.UID + "为离线");
            cinfo.IsOffline = true;
            cinfo.LogOutDT = DateTime.Now;
            AppSrv.g_Room.LeaveRoom(cinfo, cinfo.RoomState.RoomID);
            EventSystem.Instance.PostEvent(EEvent.OnUserOffline, cinfo.UID);
        }

        public void RemoveClientForSocket(Socket sk)
        {
            if (!mDictSocketClient.ContainsKey(sk))
                return;

            RemoveClient(mDictSocketClient[sk]);
        }

        #endregion

        #region Ping
        void PingAllLoop()
        {
            while (true)
            {
                pingTickARE.WaitOne();
                //AppSrv.g_Log.Info("PingAll");
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
            //AppSrv.g_Log.Debug($"OnCmdPing");
            Protobuf_Ping msg = ProtoBufHelper.DeSerizlize<Protobuf_Ping>(reqData);

            //创建成功下行
            Protobuf_Pong resp = new Protobuf_Pong()
            {
                Seed = msg.Seed,
            };
            AppSrv.g_ClientMgr.ClientSend(sk, (int)CommandID.CmdPong, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(resp));
        }
        public void OnCmdPong(Socket sk, byte[] reqData)
        {
            //AppSrv.g_Log.Debug($"OnCmdPong");
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            Protobuf_Pong msg = ProtoBufHelper.DeSerizlize<Protobuf_Pong>(reqData);

            if (_c.LastPingSeed == msg.Seed)
            {
                TimeSpan current = AppSrv.g_Tick.sw.Elapsed;
                TimeSpan delta = current - _c.LastStartPingTime;
                _c.NetDelays.Add(delta.TotalSeconds);

                while (_c.NetDelays.Count > ClientInfo.NetAveDelayCount)
                    _c.NetDelays.RemoveAt(0);

                double tempMin = double.MaxValue;
                double tempMax = double.MinValue;
                for (int i = 0; i < _c.NetDelays.Count; i++)
                {
                    tempMin = Math.Min(_c.NetDelays[i], tempMin);
                    tempMax = Math.Max(_c.NetDelays[i], tempMax);
                }
                _c.MinNetDelay = tempMin;
                _c.MaxNetDelay = tempMax;
                _c.AveNetDelay = _c.NetDelays.Average(w => w);
            }
        }
        #endregion


        public void ClientSendALL(int CMDID, int ERRCODE, byte[] data, long SkipUID = -1)
        {
            ClientSend(mClientList, CMDID, ERRCODE, data, SkipUID);
        }

        public void ClientSend(List<long> UIDs, int CMDID, int ERRCODE, byte[] data, long SkipUID = -1)
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
        public void ClientSend(IEnumerable<ClientInfo> _toclientlist, int CMDID, int ERRCODE, byte[] data, long SkipUID = -1)
        {
            //for (int i = 0; i < _toclientlist.Count(); i++)
            foreach(var _c in _toclientlist)
            {
                if (_c == null || _c.IsOffline)
                    continue;

                if (SkipUID > -1 && _c.UID == SkipUID)
                    continue;

                AppSrv.g_SocketMgr.SendToSocket(_c._socket, CMDID, ERRCODE, data);
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
            return mClientList.Where(w => !w.IsOffline).Count();
        }
    }
}
