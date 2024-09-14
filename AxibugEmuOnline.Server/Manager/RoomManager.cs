using AxibugEmuOnline.Server.Common;
using AxibugEmuOnline.Server.Manager;
using AxibugEmuOnline.Server.NetWork;
using AxibugProtobuf;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace AxibugEmuOnline.Server
{

    public class RoomManager
    {
        Dictionary<int, Data_RoomData> mDictRoom = new Dictionary<int, Data_RoomData>();
        List<int> mKeyRoomList = new List<int>();
        AutoResetEvent roomTickARE;
        Thread threadRoomTick;

        int RoomIDSeed = 1;
        public RoomManager()
        {
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomList, OnCmdRoomList);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomCreate, OnCmdRoomCreate);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomJoin, OnCmdRoomJoin);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomLeave, OnCmdRoomLeave);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomHostPlayerUpdateStateRaw, OnHostPlayerUpdateStateRaw);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomPlayerReady, OnRoomPlayerReady);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomSingelPlayerInput, OnSingelPlayerInput);


            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdScreen, OnCmdScreen);

            roomTickARE = AppSrv.g_Tick.AddNewARE(TickManager.TickType.Interval_16MS);
            threadRoomTick = new Thread(UpdateLoopTick);
            threadRoomTick.Start();
        }

        #region 房间管理

        int GetNewRoomID()
        {
            return RoomIDSeed++;
        }

        void AddRoom(Data_RoomData data)
        {
            lock (mDictRoom)
            {
                if (!mDictRoom.ContainsKey(data.RoomID))
                {
                    mDictRoom.Add(data.RoomID, data);
                    mKeyRoomList.Add(data.RoomID);
                }
            }
        }

        void RemoveRoom(int RoomID)
        {
            lock (mDictRoom)
            {
                if (mDictRoom.ContainsKey(RoomID))
                {
                    mDictRoom.Remove(RoomID);
                    mKeyRoomList.Remove(RoomID);
                }
            }
        }

        public Data_RoomData GetRoomData(int RoomID)
        {
            if (!mDictRoom.TryGetValue(RoomID, out Data_RoomData data))
                return null;
            return data;
        }

        public List<Data_RoomData> GetRoomList()
        {
            lock (mDictRoom)
            {
                List<Data_RoomData> temp = new List<Data_RoomData>();
                foreach (var room in mDictRoom)
                {
                    temp.AddRange(mDictRoom.Values);
                }
                return temp;
            }
        }

        #endregion

        private Protobuf_Room_MiniInfo GetProtoDataRoom(Data_RoomData room)
        {
            Protobuf_Room_MiniInfo result = new Protobuf_Room_MiniInfo()
            {
                GameRomID = room.GameRomID,
                RoomID = room.RoomID,
                GameRomHash = room.RomHash,
                ScreenProviderUID = room.ScreenProviderUID,
                HostPlayerUID = room.HostUID,
                GameState = room.GameState,
                ObsUserCount = 0,//TODO
                Player1UID = room.Player1_UID,
                Player2UID = room.Player2_UID,
                Player3UID = room.Player3_UID,
                Player4UID = room.Player4_UID,
            };

            if (result.Player1UID >= 0 && AppSrv.g_ClientMgr.GetClientByUID(result.Player1UID, out ClientInfo _c1))
                result.Player1NickName = _c1.NickName;

            if (result.Player2UID >= 0 && AppSrv.g_ClientMgr.GetClientByUID(result.Player2UID, out ClientInfo _c2))
                result.Player2NickName = _c2.NickName;

            if (result.Player3UID >= 0 && AppSrv.g_ClientMgr.GetClientByUID(result.Player1UID, out ClientInfo _c3))
                result.Player3NickName = _c3.NickName;

            if (result.Player4UID >= 0 && AppSrv.g_ClientMgr.GetClientByUID(result.Player2UID, out ClientInfo _c4))
                result.Player4NickName = _c4.NickName;

            return result;
        }

        public void OnCmdRoomList(Socket sk, byte[] reqData)
        {
            AppSrv.g_Log.Debug($"OnCmdRoomList ");
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            Protobuf_Room_List msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_List>(reqData);

            Protobuf_Room_List_RESP resp = new Protobuf_Room_List_RESP();
            List<Data_RoomData> temp = GetRoomList();
            foreach (var room in temp)
                resp.RoomMiniInfoList.Add(GetProtoDataRoom(room));
            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdChatmsg, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(resp));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="RoomID"></param>
        /// <param name="type">//[0] 更新或新增 [1] 删除</param>
        public void SendRoomUpdateToAll(int RoomID, int type)
        {
            Data_RoomData room = GetRoomData(RoomID);
            if (room == null)
                return;

            Protobuf_Room_Update_RESP resp = new Protobuf_Room_Update_RESP()
            {
                UpdateType = type,
                RoomMiniInfo = GetProtoDataRoom(room)
            };

            AppSrv.g_ClientMgr.ClientSendALL((int)CommandID.CmdRoomListUpdate, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(resp));
        }

        public void OnCmdRoomCreate(Socket sk, byte[] reqData)
        {
            AppSrv.g_Log.Debug($"OnCmdRoomCreate ");
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            Protobuf_Room_Create msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_Create>(reqData);
            Protobuf_Room_Create_RESP resp = new Protobuf_Room_Create_RESP();

            Data_RoomData newRoom = new Data_RoomData();
            newRoom.Init(GetNewRoomID(), msg.GameRomID, msg.GameRomHash, _c.UID);
            AddRoom(newRoom);
            ErrorCode joinErrcode = ErrorCode.ErrorOk;
            //加入
            if (newRoom.Join(msg.JoinPlayerIdx, _c, out joinErrcode, out bool bHadRoomStateChange))
            {
                //创建成功下行
                resp.RoomMiniInfo = GetProtoDataRoom(newRoom);
            }
            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomCreate, (int)joinErrcode, ProtoBufHelper.Serizlize(resp));

            if (joinErrcode == ErrorCode.ErrorOk && bHadRoomStateChange)
                SendRoomStateChange(newRoom);
        }

        public void OnCmdRoomJoin(Socket sk, byte[] reqData)
        {
            AppSrv.g_Log.Debug($"OnCmdRoomJoin ");
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            Protobuf_Room_Join msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_Join>(reqData);
            Protobuf_Room_Create_RESP resp = new Protobuf_Room_Create_RESP();
            ErrorCode joinErrcode;
            Data_RoomData room = GetRoomData(_c.RoomState.RoomID);
            bool bHadRoomStateChange = false;
            if (room == null)
                joinErrcode = ErrorCode.ErrorRoomNotFound;
            else
            {
                //加入
                if (room.Join(msg.PlayerNum, _c, out joinErrcode, out bHadRoomStateChange))
                {
                    Data_RoomData roomData = GetRoomData(msg.RoomID);
                    resp.RoomMiniInfo = GetProtoDataRoom(roomData);
                }
            }

            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomJoin, (int)joinErrcode, ProtoBufHelper.Serizlize(resp));
            Protobuf_Room_MyRoom_State_Change(msg.RoomID);

            if (joinErrcode == ErrorCode.ErrorOk && bHadRoomStateChange)
                SendRoomStateChange(room);
        }
        public void OnCmdRoomLeave(Socket sk, byte[] reqData)
        {
            AppSrv.g_Log.Debug($"OnCmdRoomJoin ");
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            Protobuf_Room_Leave msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_Leave>(reqData);
            Protobuf_Room_Leave_RESP resp = new Protobuf_Room_Leave_RESP();
            ErrorCode errcode;
            Data_RoomData room = GetRoomData(_c.RoomState.RoomID);
            bool bHadRoomStateChange = false;
            if (room == null)
                errcode = ErrorCode.ErrorRoomNotFound;
            else
            {
                if (room.Leave(_c, out errcode, out bHadRoomStateChange))
                {
                    resp.RoomID = msg.RoomID;
                }
            }
            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomLeave, (int)errcode, ProtoBufHelper.Serizlize(resp));
            Protobuf_Room_MyRoom_State_Change(msg.RoomID);

            if (errcode == ErrorCode.ErrorOk && bHadRoomStateChange)
                SendRoomStateChange(room);
        }

        public void OnHostPlayerUpdateStateRaw(Socket sk, byte[] reqData)
        {
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            Protobuf_Room_HostPlayer_UpdateStateRaw msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_HostPlayer_UpdateStateRaw>(reqData);
            Protobuf_Room_HostPlayer_UpdateStateRaw_RESP resp = new Protobuf_Room_HostPlayer_UpdateStateRaw_RESP();
            ErrorCode errcode = ErrorCode.ErrorOk;
            Data_RoomData room = GetRoomData(_c.RoomState.RoomID);
            if (room == null)
                errcode = ErrorCode.ErrorRoomNotFound;
            else if (room.GameState != RoomGameState.WaitRawUpdate)
                errcode = ErrorCode.ErrorRoomCantDoCurrState;

            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomHostPlayerUpdateStateRaw, (int)errcode, ProtoBufHelper.Serizlize(resp));

            if (errcode == ErrorCode.ErrorOk)
            {
                room.SetLoadRaw(msg.LoadStateRaw, out bool bHadRoomStateChange);

                if (bHadRoomStateChange)
                    SendRoomStateChange(room);
            }
        }

        public void OnRoomPlayerReady(Socket sk, byte[] reqData)
        {
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            Protobuf_Room_Player_Ready msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_Player_Ready>(reqData);
            ErrorCode errcode = ErrorCode.ErrorOk;
            Data_RoomData room = GetRoomData(_c.RoomState.RoomID);
            if (room == null)
                return;

        }

        public void OnSingelPlayerInput(Socket sk, byte[] reqData)
        {
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            Protobuf_Room_SinglePlayerInputData msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_SinglePlayerInputData>(reqData);

            Data_RoomData room = GetRoomData(_c.RoomState.RoomID);
            if (room == null)
                return;
            room.SetPlayerInput(_c.RoomState.PlayerIdx, msg.FrameID, (ushort)msg.InputData);
        }

        public void OnCmdScreen(Socket sk, byte[] reqData)
        {
            AppSrv.g_Log.Debug($"OnCmdScreen lenght:{reqData.Length}");
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            Protobuf_Screnn_Frame msg = ProtoBufHelper.DeSerizlize<Protobuf_Screnn_Frame>(reqData);
            Data_RoomData room = AppSrv.g_Room.GetRoomData(msg.RoomID);
            room.InputScreenData(msg.RawBitmap);
        }

        /// <summary>
        /// 广播房间状态变化
        /// </summary>
        /// <param name="RoomID"></param>
        public void Protobuf_Room_MyRoom_State_Change(int RoomID)
        {
            Data_RoomData room = GetRoomData(RoomID);
            if (room == null)
                return;

            Protobuf_Room_MyRoom_State_Change resp = new Protobuf_Room_MyRoom_State_Change()
            {
                RoomMiniInfo = GetProtoDataRoom(room)
            };

            List<ClientInfo> userlist = room.GetAllPlayerClientList();

            foreach (ClientInfo _c in userlist)
            {
                AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomMyRoomStateChanged, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(resp));
            }
        }

        public void SendRoomStateChange(Data_RoomData room)
        {
            List<ClientInfo> roomClient = room.GetAllPlayerClientList();
            switch (room.GameState)
            {
                case RoomGameState.WaitRawUpdate:
                    {
                        Protobuf_Room_WaitStep_RESP resp = new Protobuf_Room_WaitStep_RESP()
                        {
                            WaitStep = 0
                        };
                        AppSrv.g_ClientMgr.ClientSend(roomClient, (int)CommandID.CmdRoomWaitStep, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(resp));
                    }
                    break;
                case RoomGameState.WaitReady:
                    {
                        Protobuf_Room_WaitStep_RESP resp = new Protobuf_Room_WaitStep_RESP()
                        {
                            WaitStep = 1,
                            LoadStateRaw = room.NextStateRaw
                        };
                        AppSrv.g_ClientMgr.ClientSend(roomClient, (int)CommandID.CmdRoomWaitStep, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(resp));
                    }
                    break;
                case RoomGameState.InOnlineGame:
                    {
                        Protobuf_Room_WaitStep_RESP resp = new Protobuf_Room_WaitStep_RESP()
                        {
                            WaitStep = 2,
                        };
                        AppSrv.g_ClientMgr.ClientSend(roomClient, (int)CommandID.CmdRoomWaitStep, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(resp));
                    }
                    break;
            }
        }



        #region 房间帧循环
        void UpdateLoopTick()
        {
            while (true)
            {
                roomTickARE.WaitOne();
                UpdateAllRoomLogic();
            }
        }
        void UpdateAllRoomLogic()
        {
            for (int i = 0; i < mKeyRoomList.Count; i++)
            {
                int roomid = mKeyRoomList[i];
                if (!mDictRoom.TryGetValue(roomid, out Data_RoomData room))
                    continue;
                if (room.GameState < RoomGameState.InOnlineGame)
                    continue;
                //更新帧
                room.TakeFrame();
                //广播
                room.SynInputData();
            }
        }
        #endregion
    }

    public class Data_RoomData
    {
        public int RoomID { get; private set; }
        public int GameRomID { get; private set; }
        public string RomHash { get; private set; }
        public long HostUID { get; private set; }
        public long ScreenProviderUID { get; private set; }
        public long Player1_UID { get; private set; }
        public long Player2_UID { get; private set; }
        public long Player3_UID { get; private set; }
        public long Player4_UID { get; private set; }
        public Google.Protobuf.ByteString? NextStateRaw { get; private set; }
        public Google.Protobuf.ByteString? ScreenRaw { get; private set; }
        public bool[] PlayerReadyState { get; private set; }

        public List<long> SynUIDs;
        //public RoomPlayerState PlayerState => getPlayerState();
        private RoomGameState mGameState;
        public uint mCurrFrameId = 0;
        public ServerInputSnapShot mCurrInputData;
        public Queue<(uint, ServerInputSnapShot)> mInputQueue;
        //TODO
        public Dictionary<int, Queue<byte[]>> mDictPlayerIdx2SendQueue;
        public RoomGameState GameState
        {
            get { return mGameState; }
            set
            {
                if (mGameState != value)
                {
                    mGameState = value;
                    switch (value)
                    {
                        case RoomGameState.WaitRawUpdate:
                            NextStateRaw = null;
                            break;
                        case RoomGameState.WaitReady:
                            Array.Fill<bool>(PlayerReadyState, false);
                            break;
                    }
                }
            }
        }

        bool IsAllReady()
        {
            bool Ready = true;
            if (
                (Player1_UID > 0 && !PlayerReadyState[0])
                ||
                (Player2_UID > 0 && !PlayerReadyState[1])
                ||
                (Player3_UID > 0 && !PlayerReadyState[2])
                ||
                (Player4_UID > 0 && !PlayerReadyState[3])
                )
            {
                Ready = false;
            }
            return Ready;
        }

        public void Init(int roomID, int gameRomID, string roomHash, long hostUId, bool bloadState = false)
        {
            RoomID = roomID;
            GameRomID = gameRomID;
            RomHash = roomHash;
            HostUID = hostUId;
            ScreenProviderUID = hostUId;
            Player1_UID = -1;
            Player2_UID = -1;
            Player3_UID = -1;
            Player4_UID = -1;
            PlayerReadyState = new bool[4];
            SynUIDs = new List<long>();//广播角色列表
            GameState = RoomGameState.NoneGameState;
            mCurrInputData = new ServerInputSnapShot();
            mInputQueue = new Queue<(uint, ServerInputSnapShot)>();
            mDictPlayerIdx2SendQueue = new Dictionary<int, Queue<byte[]>>();
        }

        public void SetPlayerUID(int PlayerIdx, ClientInfo _c)
        {
            long oldUID = -1;
            switch (PlayerIdx)
            {
                case 0: oldUID = Player1_UID; Player1_UID = _c.UID; break;
                case 1: oldUID = Player2_UID; Player2_UID = _c.UID; break;
                case 2: oldUID = Player3_UID; Player3_UID = _c.UID; break;
                case 3: oldUID = Player4_UID; Player4_UID = _c.UID; break;
            }
            if (oldUID >= 0)
                SynUIDs.Remove(oldUID);
            SynUIDs.Add(_c.UID);
            _c.RoomState.SetRoomData(this.RoomID, PlayerIdx);
        }

        public void RemovePlayer(ClientInfo _c)
        {
            int PlayerIdx = GetPlayerIdx(_c);
            switch (PlayerIdx)
            {
                case 0: Player1_UID = -1; SynUIDs.Remove(_c.UID); break;
                case 1: Player2_UID = -1; SynUIDs.Remove(_c.UID); break;
                case 2: Player3_UID = -1; SynUIDs.Remove(_c.UID); break;
                case 3: Player4_UID = -1; SynUIDs.Remove(_c.UID); break;
            }
            _c.RoomState.ClearRoomData();
        }

        int GetPlayerIdx(ClientInfo _c)
        {
            if (Player1_UID == _c.UID) return 0;
            if (Player2_UID == _c.UID) return 1;
            if (Player3_UID == _c.UID) return 2;
            if (Player4_UID == _c.UID) return 3;
            return -1;
        }

        public bool GetPlayerUIDByIdx(int Idx, out long UID)
        {
            switch (Idx)
            {
                case 0: UID = Player1_UID; break;
                case 1: UID = Player2_UID; break;
                case 2: UID = Player3_UID; break;
                case 3: UID = Player4_UID; break;
                default: UID = -1; break;
            }
            return UID > 0;
        }
        public bool GetPlayerClientByIdx(int Idx, out ClientInfo _c)
        {
            _c = null;
            if (!GetPlayerUIDByIdx(Idx, out long UID))
                return false;

            if (!AppSrv.g_ClientMgr.GetClientByUID(UID, out _c))
                return false;

            return true;
        }

        public List<long> GetAllPlayerUIDs()
        {
            List<long> list = new List<long>();
            if (Player1_UID > 0) list.Add(Player1_UID);
            if (Player2_UID > 0) list.Add(Player2_UID);
            if (Player3_UID > 0) list.Add(Player3_UID);
            if (Player4_UID > 0) list.Add(Player4_UID);
            return list;
        }

        public List<ClientInfo> GetAllPlayerClientList()
        {
            List<ClientInfo> list = new List<ClientInfo>();

            List<long> Uids = GetAllPlayerUIDs();

            foreach (long uid in Uids)
            {
                if (!AppSrv.g_ClientMgr.GetClientByUID(uid, out ClientInfo _c, true))
                    continue;

                list.Add(_c);
            }

            return list;
        }

        public void SetPlayerInput(int PlayerIdx, long mFrameID, ushort input)
        {
            switch (PlayerIdx)
            {
                case 0: mCurrInputData.p1 = input; break;
                case 1: mCurrInputData.p2 = input; break;
                case 2: mCurrInputData.p3 = input; break;
                case 3: mCurrInputData.p4 = input; break;
            }
        }

        public void ClearPlayerInput(int PlayerIdx)
        {
            switch (PlayerIdx)
            {
                case 0: mCurrInputData.p1 = 0; break;
                case 1: mCurrInputData.p2 = 0; break;
                case 2: mCurrInputData.p3 = 0; break;
                case 3: mCurrInputData.p4 = 0; break;
            }
        }

        public int GetPlayerCount()
        {
            int count = 0;
            if (Player1_UID > 0) count++;
            if (Player2_UID > 0) count++;
            if (Player3_UID > 0) count++;
            if (Player4_UID > 0) count++;
            return count;
        }

        void StartNewTick()
        {
            mInputQueue.Clear();
            mDictPlayerIdx2SendQueue.Clear();

            List<ClientInfo> playerlist = GetAllPlayerClientList();
            double maxNetDelay = 0;
            for (int i = 0; i < playerlist.Count; i++)
            {
                ClientInfo player = playerlist[i];
                maxNetDelay = Math.Max(maxNetDelay, player.AveNetDelay);
            }
            mCurrFrameId = 0;

            int TaskFrameCount = (int)((maxNetDelay / 0.016f) + 5f);

            for (int i = 0; i < TaskFrameCount; i++)
            {
                TakeFrame();
            }
        }

        public void TakeFrame()
        {
            mInputQueue.Enqueue((mCurrFrameId, mCurrInputData));
            mCurrFrameId++;
        }

        /// <summary>
        /// 广播数据
        /// </summary>
        public void SynInputData()
        {
            while (mInputQueue.Count > 0)
            {
                (uint frameId, ServerInputSnapShot inputdata) data = mInputQueue.Dequeue();
                Protobuf_Room_Syn_RoomFrameAllInputData resp = new Protobuf_Room_Syn_RoomFrameAllInputData()
                {
                    FrameID = data.frameId,
                    InputData = data.inputdata.all,
                    ServerFrameID = mCurrFrameId
                };
                AppSrv.g_ClientMgr.ClientSend(SynUIDs, (int)CommandID.CmdRoomSynPlayerInput, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(resp));
            }
        }


        #region 房间进出
        /// <summary>
        /// 进入房间
        /// </summary>
        /// <param name="RoomID"></param>
        /// <param name="PlayerNum"></param>
        /// <param name="_c"></param>
        /// <param name="errcode"></param>
        /// <returns></returns>
        public bool Join(int PlayerNum, ClientInfo _c, out ErrorCode errcode, out bool bHadRoomStateChange)
        {
            bHadRoomStateChange = false;
            int oldPlayerCount = GetPlayerCount();
            if (GetPlayerUIDByIdx(PlayerNum, out long hadUID))
            {
                errcode = ErrorCode.ErrorRoomSlotReadlyHadPlayer;
                return false;
            }
            SetPlayerUID(PlayerNum, _c);
            int newPlayerCount = GetPlayerCount();
            errcode = ErrorCode.ErrorOk;

            bHadRoomStateChange = CheckRoomStateChange(oldPlayerCount, newPlayerCount);
            return true;
        }

        /// <summary>
        /// 离开房间
        /// </summary>
        /// <param name="RoomID"></param>
        /// <param name="_c"></param>
        /// <param name="errcode"></param>
        /// <returns></returns>
        public bool Leave(ClientInfo _c, out ErrorCode errcode, out bool bHadRoomStateChange)
        {
            int oldPlayerCount = GetPlayerCount();
            RemovePlayer(_c);
            int newPlayerCount = GetPlayerCount();
            errcode = ErrorCode.ErrorOk;
            bHadRoomStateChange = CheckRoomStateChange(oldPlayerCount, newPlayerCount);
            return true;
        }
        #endregion

        bool CheckRoomStateChange(int oldPlayerCount, int newPlayerCount)
        {
            bool bChanged = false;
            bool bNewToOnlyHost = (oldPlayerCount != 1 && newPlayerCount == 1);
            bool bMorePlayer = (oldPlayerCount < 2 && newPlayerCount >= 2) || (newPlayerCount > oldPlayerCount);
            switch (this.GameState)
            {
                case RoomGameState.NoneGameState:
                    if (bNewToOnlyHost)
                    {
                        this.GameState = RoomGameState.OnlyHost;
                        bChanged = true;
                    }
                    break;
                case RoomGameState.OnlyHost:
                    if (bMorePlayer)//加入更多玩家
                    {
                        this.GameState = RoomGameState.WaitRawUpdate;
                        bChanged = true;
                        break;
                    }
                    break;
                case RoomGameState.WaitRawUpdate:
                    if (bMorePlayer)//加入更多玩家
                    {
                        this.GameState = RoomGameState.WaitRawUpdate;
                        bChanged = true;
                        break;
                    }
                    if (NextStateRaw != null)//已经上传即时存档
                    {
                        this.GameState = RoomGameState.WaitReady;
                        bChanged = true;
                        break;
                    }
                    break;
                case RoomGameState.WaitReady:
                    if (bMorePlayer)//加入更多玩家
                    {
                        this.GameState = RoomGameState.WaitRawUpdate;
                        bChanged = true;
                        break;
                    }
                    //没有为准备的
                    bool bAllReady = IsAllReady();
                    if (bAllReady)
                    {
                        //新开Tick
                        StartNewTick();

                        this.GameState = RoomGameState.InOnlineGame;
                        bChanged = true;
                        break;
                    }
                    break;
                case RoomGameState.Pause:
                    if (bMorePlayer)//加入更多玩家
                    {
                        this.GameState = RoomGameState.WaitRawUpdate;
                        bChanged = true;
                        break;
                    }
                    break;
                case RoomGameState.InOnlineGame:
                    if (bMorePlayer)//加入更多玩家
                    {
                        this.GameState = RoomGameState.WaitRawUpdate;
                        //TODO 服务器Tick提前跑帧
                        bChanged = true;
                        break;
                    }
                    break;
            }
            return bChanged;
        }

        public void SetLoadRaw(Google.Protobuf.ByteString NextStateRaw, out bool bHadRoomStateChange)
        {
            int oldPlayerCount = GetPlayerCount();
            this.NextStateRaw = NextStateRaw;
            int newPlayerCount = GetPlayerCount();
            bHadRoomStateChange = CheckRoomStateChange(oldPlayerCount, newPlayerCount);
        }

        public void InputScreenData(Google.Protobuf.ByteString screenRaw)
        {
            this.ScreenRaw = NextStateRaw;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ServerInputSnapShot
    {
        [FieldOffset(0)]
        public UInt64 all;
        [FieldOffset(0)]
        public ushort p1;
        [FieldOffset(2)]
        public ushort p2;
        [FieldOffset(4)]
        public ushort p3;
        [FieldOffset(6)]
        public ushort p4;
    }
}