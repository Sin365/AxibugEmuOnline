using AxibugEmuOnline.Server.Common;
using AxibugEmuOnline.Server.Data;
using AxibugEmuOnline.Server.Manager.Client;
using AxibugProtobuf;

namespace AxibugEmuOnline.Server.Manager.Room
{
    public class GameRoom : IDisposable
    {
        public int RoomID { get; private set; }
        public int GameRomID { get; private set; }
        public RomPlatformType GameRomPlatformType { get; private set; }
        public string RomHash { get; private set; }
        public long HostUID { get; private set; }
        public long ScreenProviderUID { get; private set; }
        public GameRoomSlot[] PlayerSlot { get; private set; }
        public long Player1_UID => PlayerSlot[0].UID;
        public long Player2_UID => PlayerSlot[1].UID;
        public long Player3_UID => PlayerSlot[2].UID;
        public long Player4_UID => PlayerSlot[3].UID;
        public Google.Protobuf.ByteString? NextStateRaw { get; private set; }
        public Google.Protobuf.ByteString? ScreenRaw { get; private set; }
        //public bool[] PlayerReadyState { get; private set; }
        public List<long> SynUIDs;
        //public RoomPlayerState PlayerState => getPlayerState();
        private RoomGameState mGameState;
        public uint mCurrServerFrameId = 0;
        public ServerInputSnapShot mCurrInputData;
        public Queue<(uint, ServerInputSnapShot)> mInputQueue;

        public List<double> send2time;
        const int SynLimitOnSec = 61;

        Lock synInputLock = new Lock();
        //TODO
        //public Dictionary<int, Queue<byte[]>> mDictPlayerIdx2SendQueue;
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
                            ClearAllSlotReadyState();//清理玩家所有准备状态
                            break;
                    }
                }
            }
        }
        /// <summary>
        /// 服务器提前帧数
        /// </summary>
        public uint SrvForwardFrames { get; set; }

        public void Init(int roomID, int gameRomID, string roomHash, long hostUId, bool bloadState = false, RomPlatformType ptype = default)
        {
            Dispose();
            RoomID = roomID;
            GameRomID = gameRomID;
            GameRomPlatformType = ptype;
            RomHash = roomHash;
            HostUID = hostUId;
            ScreenProviderUID = hostUId;

            if (PlayerSlot == null)
            {
                PlayerSlot = new GameRoomSlot[4];
                for (uint i = 0; i < PlayerSlot.Length; i++)
                    PlayerSlot[i] = new GameRoomSlot();
            }

            for (uint i = 0; i < PlayerSlot.Length; i++)
                PlayerSlot[i].Init(i);

            //PlayerReadyState = new bool[4];
            SynUIDs = ObjectPoolAuto.AcquireList<long>();//new List<long>();//广播角色列表
            GameState = RoomGameState.NoneGameState;
            mCurrInputData = new ServerInputSnapShot();
            mInputQueue = ObjectPoolAuto.AcquireQueue<(uint, ServerInputSnapShot)>();
            // new Queue<(uint, ServerInputSnapShot)>();
            //mDictPlayerIdx2SendQueue = new Dictionary<int, Queue<byte[]>>();
            send2time = ObjectPoolAuto.AcquireList<double>();
        }

        /// <summary>
        /// 房间释放时，需要调用
        /// </summary>
        public void Dispose()
        {
            if (SynUIDs != null)
            {
                ObjectPoolAuto.Release(SynUIDs);
                SynUIDs = null;
            }

            if (mInputQueue != null)
            {
                ObjectPoolAuto.Release(mInputQueue);
                mInputQueue = null;
            }

            if (send2time != null)
            {
                ObjectPoolAuto.Release(send2time);
                send2time = null;
            }
        }

        public bool GetSlotDataByUID(long uid, out Dictionary<uint, uint> slotIdx2JoyIdx)
        {
            slotIdx2JoyIdx = new Dictionary<uint, uint>();
            var dataarr = PlayerSlot.Where(w => w.UID == uid).ToArray();
            foreach (var slot in dataarr)
                slotIdx2JoyIdx[slot.SlotIdx] = slot.LocalJoyIdx;
            return slotIdx2JoyIdx.Count > 0;
        }
        /// <summary>
        /// 按照SlotIdx设置Input
        /// </summary>
        /// <param name="slotIdx"></param>
        void SetInputDataBySlotIdx(uint slotIdx, ServerInputSnapShot data)
        {
            ushort val = 0;
            switch (GameRomPlatformType)
            {
                case RomPlatformType.Cps1:
                case RomPlatformType.Cps2:
                case RomPlatformType.Neogeo:
                case RomPlatformType.Igs:
                case RomPlatformType.ArcadeOld:
                    {
                        switch (slotIdx)
                        {
                            case 0: val = data.p1_ushort; break;
                            case 1: val = data.p2_ushort; break;
                            case 2: val = data.p3_ushort; break;
                            case 4: val = data.p4_ushort; break;
                        }

                        //ushort 类型作为单个玩家操作
                        switch (slotIdx)
                        {
                            case 0: mCurrInputData.p1_ushort = val; break;
                            case 1: mCurrInputData.p2_ushort = val; break;
                            case 2: mCurrInputData.p3_ushort = val; break;
                            case 4: mCurrInputData.p3_ushort = val; break;
                        }
                    }
                    break;
                default:
                    {
                        switch (slotIdx)
                        {
                            case 0: val = data.p1_byte; break;
                            case 1: val = data.p2_byte; break;
                            case 2: val = data.p3_byte; break;
                            case 4: val = data.p4_byte; break;
                        }
                        //byte 类型作为单个玩家操作
                        switch (slotIdx)
                        {
                            case 0: mCurrInputData.p1_byte = (byte)val; break;
                            case 1: mCurrInputData.p2_byte = (byte)val; break;
                            case 2: mCurrInputData.p3_byte = (byte)val; break;
                            case 4: mCurrInputData.p3_byte = (byte)val; break;
                        }
                    }
                    break;
            }
        }
        /// <summary>
        /// 按照UID清理SlotData
        /// </summary>
        /// <param name="uid"></param>
        void ClearSlotDataByUid(long uid)
        {
            var dataarr = PlayerSlot.Where(w => w.UID == uid).ToArray();
            foreach (var slot in dataarr)
            {
                slot.Init(slot.SlotIdx);
                ClearInputDataBySlotIdx(slot.SlotIdx);
            }
        }
        /// <summary>
        /// 按照SlotIdx清理SlotData
        /// </summary>
        /// <param name="slotIdx"></param>
        void ClearSlotDataBySlotIdx(uint slotIdx)
        {
            PlayerSlot[slotIdx].Init(slotIdx);
            ClearInputDataBySlotIdx(slotIdx);
        }
        /// <summary>
        /// 按照SlotIdx清理Input
        /// </summary>
        /// <param name="slotIdx"></param>
        void ClearInputDataBySlotIdx(uint slotIdx)
        {
            switch (GameRomPlatformType)
            {
                case RomPlatformType.Cps1:
                case RomPlatformType.Cps2:
                case RomPlatformType.Neogeo:
                case RomPlatformType.Igs:
                case RomPlatformType.ArcadeOld:
                    {
                        //ushort 类型作为单个玩家操作
                        switch (slotIdx)
                        {
                            case 0: mCurrInputData.p1_ushort = 0; break;
                            case 1: mCurrInputData.p2_ushort = 0; break;
                            case 2: mCurrInputData.p3_ushort = 0; break;
                            case 4: mCurrInputData.p4_ushort = 0; break;
                        }
                    }
                    break;
                default:
                    {
                        //byte 类型作为单个玩家操作
                        switch (slotIdx)
                        {
                            case 0: mCurrInputData.p1_byte = 0; break;
                            case 1: mCurrInputData.p2_byte = 0; break;
                            case 2: mCurrInputData.p3_byte = 0; break;
                            case 4: mCurrInputData.p3_byte = 0; break;
                        }
                    }
                    break;
            }

        }
        /// <summary>
        /// 更新同步名单
        /// </summary>
        void UpdateSynUIDs()
        {
            for (int i = SynUIDs.Count - 1; i >= 0; i--)
            {
                long uid = SynUIDs[i];

                bool bHad = false;
                if (Player1_UID == uid) bHad = true;
                else if (Player2_UID == uid) bHad = true;
                else if (Player3_UID == uid) bHad = true;
                else if (Player4_UID == uid) bHad = true;
                if (!bHad)
                    SynUIDs.RemoveAt(i);
            }
            if (Player1_UID > 0 && !SynUIDs.Contains(Player1_UID)) SynUIDs.Add(Player1_UID);
            if (Player2_UID > 0 && !SynUIDs.Contains(Player2_UID)) SynUIDs.Add(Player2_UID);
            if (Player3_UID > 0 && !SynUIDs.Contains(Player3_UID)) SynUIDs.Add(Player3_UID);
            if (Player4_UID > 0 && !SynUIDs.Contains(Player4_UID)) SynUIDs.Add(Player4_UID);
        }

        #region 准备状态管理
        bool IsAllReady()
        {
            bool Ready = true;
            if (
                (Player1_UID > 0 && !PlayerSlot[0].Ready)
                ||
                (Player2_UID > 0 && !PlayerSlot[1].Ready)
                ||
                (Player3_UID > 0 && !PlayerSlot[2].Ready)
                ||
                (Player4_UID > 0 && !PlayerSlot[3].Ready)
                )
            {
                Ready = false;
            }
            return Ready;
        }
        /// <summary>
        /// 清除所有槽位准备状态
        /// </summary>
        void ClearAllSlotReadyState()
        {
            for (var i = 0; i < PlayerSlot.Length; i++)
            {
                PlayerSlot[i].Ready = false;
            }
        }
        /// <summary>
        /// 按照UID设置Ready信息
        /// </summary>
        /// <param name="uid"></param>
        void SetReadyByUid(long uid)
        {
            for (var i = 0; i < PlayerSlot.Length; i++)
            {
                if (PlayerSlot[i].UID == uid)
                    PlayerSlot[i].Ready = true;
            }
        }
        #endregion

        public void SetPlayerSlotData(ClientInfo _c, ref readonly Dictionary<uint, (uint, GamePadType)> newSlotIdx2JoyIdx)
        {
            GetSlotDataByUID(_c.UID, out Dictionary<uint, uint> oldSlotIdx2JoyIdx);
            HashSet<uint> diffSlotIdxs = ObjectPoolAuto.AcquireSet<uint>();// new HashSet<uint>();
            foreach (var old in oldSlotIdx2JoyIdx)
            {
                uint old_slotIdx = old.Key;
                //如果旧位置已经不存在于新位置，则需要算作diff
                if (!newSlotIdx2JoyIdx.ContainsKey(old_slotIdx))
                {
                    diffSlotIdxs.Add(old_slotIdx); continue;
                }
                uint old_slotjoyIdx = old.Value;
                //如果旧位置不变，但客户端本地JoyIdx变化则算作diff
                if (old_slotjoyIdx != newSlotIdx2JoyIdx[old_slotIdx].Item1)
                {
                    diffSlotIdxs.Add(old_slotIdx); continue;
                }
            }
            //如果是在旧数据中不存在的位置，则算作diff
            foreach (var newdata in newSlotIdx2JoyIdx)
            {
                uint new_slotIdx = newdata.Key;
                if (!oldSlotIdx2JoyIdx.ContainsKey(new_slotIdx))
                {
                    diffSlotIdxs.Add(new_slotIdx); continue;
                }
            }
            //必要的diff slot 清理键值数据
            foreach (var diffSlotIdx in diffSlotIdxs)
            {
                ClearSlotDataBySlotIdx(diffSlotIdx);
            }
            //设置新的槽位
            foreach (var slotdata in newSlotIdx2JoyIdx)
            {
                PlayerSlot[slotdata.Key].LocalJoyIdx = slotdata.Value.Item1;
                PlayerSlot[slotdata.Key].LocalGamePadType = slotdata.Value.Item2;
                PlayerSlot[slotdata.Key].UID = _c.UID;
                AppSrv.g_Log.DebugCmd($"SetPlayerSlot RoomID->{RoomID} _c.UID->{_c.UID}  PlayerSlotIdx->{slotdata.Key} LocalJoyIdx->{slotdata.Value}");
            }
            //更新需要同步的UID
            UpdateSynUIDs();
            _c.RoomState.SetRoomData(this.RoomID);

            ObjectPoolAuto.Release(diffSlotIdxs);
        }
        public void RemovePlayer(ClientInfo _c)
        {
            ClearSlotDataByUid(_c.UID);
            UpdateSynUIDs();
            _c.RoomState.ClearRoomData();
        }
        public bool GetPlayerUIDByIdx(uint Idx, out long UID)
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
        public bool GetFreeSlot(out uint SlotIdx)
        {
            for (uint i = 0; i < PlayerSlot.Length; i++)
            {
                if (PlayerSlot[i].UID < 0)
                {
                    SlotIdx = i;
                    return true;
                }
            }
            SlotIdx = 0;
            return false;
        }
        public bool GetPlayerClientByIdx(uint Idx, out ClientInfo _c)
        {
            _c = null;
            if (!GetPlayerUIDByIdx(Idx, out long UID))
                return false;

            if (!AppSrv.g_ClientMgr.GetClientByUID(UID, out _c))
                return false;

            return true;
        }
        public void GetAllPlayerClientList(ref List<ClientInfo> list)
        {
            List<long> Uids = SynUIDs;
            foreach (long uid in Uids)
            {
                if (!AppSrv.g_ClientMgr.GetClientByUID(uid, out ClientInfo _c, true))
                    continue;
                list.Add(_c);
            }
        }

        void SetInputBySlotIdxJoyIdx(uint SlotIdx, uint LocalJoyIdx, ServerInputSnapShot clieninput)
        {
            switch (LocalJoyIdx)
            {
                case 0: SetInputDataBySlotIdx(SlotIdx, clieninput); break;
                case 1: SetInputDataBySlotIdx(SlotIdx, clieninput); break;
                case 2: SetInputDataBySlotIdx(SlotIdx, clieninput); break;
                case 3: SetInputDataBySlotIdx(SlotIdx, clieninput); break;
            }
        }
        public int GetPlayerCount()
        {
            return SynUIDs.Count;
        }
        public void UpdateRoomForwardNum()
        {

            List<ClientInfo> playerlist = ObjectPoolAuto.AcquireList<ClientInfo>();
            GetAllPlayerClientList(ref playerlist);

            double maxNetDelay = 0;
            for (int i = 0; i < playerlist.Count; i++)
            {
                ClientInfo player = playerlist[i];
                maxNetDelay = Math.Max(maxNetDelay, player.AveNetDelay);
            }
            float MustTaskFrame = 1;
            SrvForwardFrames = (uint)((maxNetDelay / 0.016f) + MustTaskFrame);
            if (SrvForwardFrames < 2)
                SrvForwardFrames = 2;
            //AppSrv.g_Log.Debug($"服务器提前跑帧数：Max(2,({maxNetDelay} / {0.016f}) + {MustTaskFrame}) = {SrvForwardFrames}");

            ObjectPoolAuto.Release(playerlist);
        }

        #region 帧相关
        void StartNewTick()
        {
            mInputQueue.Clear();
            //mDictPlayerIdx2SendQueue.Clear();

            mCurrServerFrameId = 0;
            //??????????=>>mCurrInputData.all = 1;

            mCurrInputData.all = 0;

            UpdateRoomForwardNum();

            uint StartForwardFrames = (SrvForwardFrames * 2) + 5;
            StartForwardFrames = Math.Max(10, StartForwardFrames);
            //服务器提前跑帧数
            for (int i = 0; i < StartForwardFrames; i++)
                TakeFrame();

            AppSrv.g_Log.Info($"房间初始提前量=>{StartForwardFrames}，当前延迟提前量=>{SrvForwardFrames}");
        }
        public void TakeFrame()
        {
            lock (synInputLock)
            {
                mInputQueue.Enqueue((mCurrServerFrameId, mCurrInputData));
                mCurrServerFrameId++;
                if (mCurrServerFrameId % 60 == 0)
                {
                    UpdateRoomForwardNum();
                }
            }
        }
        #endregion

        ulong LastTestSend = 0;
        internal ulong LastTestRecv;


        /// <summary>
        /// 广播数据
        /// </summary>
        public void SynInputData()
        {
            List<(uint frameId, ServerInputSnapShot inputdata)> temp = new List<(uint frameId, ServerInputSnapShot inputdata)>();
            bool flagInitList = false;
            lock (synInputLock)
            {
                #region 限制帧速率
                //double timeNow = AppSrv.g_Tick.timeNow;
                //while (mInputQueue.Count > 0)
                //{
                //    if (send2time.Count >= SynLimitOnSec)
                //    {
                //        //AppSrv.g_Log.Info($"{timeNow} - {send2time[0]} =>{timeNow - send2time[0]}");
                //        if (timeNow - send2time[0] < 1f) //最早的历史发送还在一秒之内
                //            break;
                //        else
                //            send2time.RemoveAt(0);
                //    }
                //    if (!flagInitList)
                //    {
                //        flagInitList = true;
                //        //temp = new List<(uint frameId, ServerInputSnapShot inputdata)>();
                //        temp = ObjectPoolAuto.AcquireList<(uint frameId, ServerInputSnapShot inputdata)>();
                //    }
                //    temp.Add(mInputQueue.Dequeue());
                //    send2time.Add(timeNow);
                //}

                //第二种限制速率办法
                //int SendCount = 0; ;
                //while (mInputQueue.Count > 0)
                //{
                //    SendCount++;
                //    temp.Add(mInputQueue.Dequeue());
                //    if (SendCount >= SynLimitOnSec)
                //    {
                //        AppSrv.g_Log.Debug($"outSide SendCount=>{SendCount},morequeue.count->{mInputQueue.Count}");
                //        break;
                //    }
                //}
                #endregion

                int SendCount = 0; ;
                while (mInputQueue.Count > 0)
                {
                    SendCount++;
                    temp.Add(mInputQueue.Dequeue());
                    if (SendCount >= SynLimitOnSec)
                    {
                        AppSrv.g_Log.Debug($"outSide SendCount=>{SendCount},morequeue.count->{mInputQueue.Count}");
                        break;
                    }
                }
            }

            //if (!flagInitList)
            //    return;

            for (int i = 0; i < temp.Count; i++)
            {
                (uint frameId, ServerInputSnapShot inputdata) data = temp[i];

                Protobuf_Room_Syn_RoomFrameAllInputData resp = new Protobuf_Room_Syn_RoomFrameAllInputData()
                {
                    FrameID = data.frameId,
                    InputData = data.inputdata.all,
                    ServerFrameID = mCurrServerFrameId,
                    ServerForwardCount = this.SrvForwardFrames
                };
                AppSrv.g_ClientMgr.ClientSend(SynUIDs, (int)CommandID.CmdRoomSynPlayerInput, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(resp));
                //if (LastTestSend != data.inputdata.all)
                //{
                //    LastTestSend = data.inputdata.all;
                //    AppSrv.g_Log.Debug($" {DateTime.Now.ToString("hh:mm:ss.fff")} SynInput=> RoomID->{RoomID} ServerFrameID->{mCurrServerFrameId} SynUIDs=>{string.Join(",", SynUIDs)} ");
                //}
            }

            ObjectPoolAuto.Release(temp);
        }

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
                    //没有未准备的
                    bool bAllReady = IsAllReady();
                    if (bAllReady)
                    {
                        this.GameState = RoomGameState.InOnlineGame;
                        //新开Tick
                        StartNewTick();
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
                        bChanged = true;
                        break;
                    }
                    break;
            }



            //房主离线,自动选择延迟最低另一名玩家做房主
            if (!GetSlotDataByUID(this.HostUID, out Dictionary<uint, uint> slotIdx2JoyIdx))
            {
                List<ClientInfo> userlist = ObjectPoolAuto.AcquireList<ClientInfo>();
                GetAllPlayerClientList(ref userlist);
                if (userlist.Count > 0)
                {
                    ClientInfo? client = userlist.OrderBy(w => w.AveNetDelay).FirstOrDefault();
                    this.HostUID = client.UID;
                    AppSrv.g_Log.DebugCmd($"更换房主为{this.HostUID}");
                    bChanged = true;
                }
                ObjectPoolAuto.Release(userlist);
            }

            if (this.GameState > RoomGameState.OnlyHost && newPlayerCount == 1)
            {
                this.GameState = RoomGameState.OnlyHost;
                AppSrv.g_Log.DebugCmd("回到OnlyHost状态");
                bChanged = true;
            }


            return bChanged;
        }


        #region 对外开放函数

        #region 房间进出
        /// <summary>
        /// 进入房间
        /// </summary>
        /// <param name="RoomID"></param>
        /// <param name="PlayerNum"></param>
        /// <param name="_c"></param>
        /// <param name="errcode"></param>
        /// <returns></returns>
        public bool Join(uint slotIdx, uint joyIdx, ClientInfo _c, out ErrorCode errcode, out bool bHadRoomStateChange)
        {
            bHadRoomStateChange = false;
            int oldPlayerCount = GetPlayerCount();
            if (GetPlayerUIDByIdx(slotIdx, out long hadUID))
            {
                errcode = ErrorCode.ErrorRoomSlotAlreadlyHadPlayer;
                return false;
            }
            AppSrv.g_Log.Debug($"Join _c.UID->{_c.UID} RoomID->{RoomID}");
            Dictionary<uint, (uint, GamePadType)> slotInfo = new Dictionary<uint, (uint, GamePadType)>();
            slotInfo[slotIdx] = (joyIdx, GamePadType.GlobalGamePad);
            SetPlayerSlotData(_c, ref slotInfo);
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
        public void SetPlayerInput(long UID, uint FrameID, ServerInputSnapShot clieninput)
        {
            for (uint i = 0; i < PlayerSlot.Count(); i++)
            {
                GameRoomSlot slotData = PlayerSlot[i];
                if (slotData.UID != UID)
                    continue;
                SetInputBySlotIdxJoyIdx(slotData.SlotIdx, slotData.LocalJoyIdx, clieninput);
            }
        }
        public bool SetRePlayerReady(long UID, out ErrorCode errcode, out bool bHadRoomStateChange)
        {
            int oldPlayerCount = GetPlayerCount();
            SetReadyByUid(UID);
            int newPlayerCount = GetPlayerCount();
            errcode = ErrorCode.ErrorOk;
            bHadRoomStateChange = CheckRoomStateChange(oldPlayerCount, newPlayerCount);
            return true;
        }
        public void SetLoadRaw(Google.Protobuf.ByteString NextStateRaw, out bool bHadRoomStateChange)
        {
            int oldPlayerCount = GetPlayerCount();
            AppSrv.g_Log.Debug($"SetLoadRaw proto Lenght->{NextStateRaw.Length}");
            this.NextStateRaw = NextStateRaw;
            int newPlayerCount = GetPlayerCount();
            bHadRoomStateChange = CheckRoomStateChange(oldPlayerCount, newPlayerCount);
        }
        public void InputScreenData(Google.Protobuf.ByteString screenRaw)
        {
            this.ScreenRaw = NextStateRaw;
        }


        #region 客户端推帧方案
        public bool GetNeedForwardTick(uint clientFrame, out long forwaFrame)
        {
            forwaFrame = 0;
            //目标帧，客户端+服务器提前量
            long targetFrame = clientFrame + SrvForwardFrames;
            if (targetFrame > mCurrServerFrameId)//更靠前
                forwaFrame = targetFrame - mCurrServerFrameId;
            return forwaFrame > 0;
        }
        #endregion

        #endregion
    }
}
