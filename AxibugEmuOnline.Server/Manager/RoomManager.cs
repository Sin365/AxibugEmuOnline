using AxibugEmuOnline.Server.Common;
using AxibugEmuOnline.Server.Manager;
using AxibugEmuOnline.Server.NetWork;
using AxibugProtobuf;
using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Xml;

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
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomSingelPlayerInput, OnSingelPlayerInput);

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
                    mKeyRoomList.Remove(data.RoomID);
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
            if (!mDictRoom.TryGetValue(RoomID,out Data_RoomData data))
                return null;
            return data;
        }

        List<Data_RoomData> GetRoomList()
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
                GameState = room.GameState,
                PlayerState = room.PlayerState,
                ObsUserCount = 0,//TODO
                Player1UID = room.Player1_UID,
                Player2UID = room.Player2_UID,
            };

            if (result.Player1UID >= 0 && AppSrv.g_ClientMgr.GetClientByUID(result.Player1UID, out ClientInfo _c1))
                result.Player1NickName = _c1.NickName;

            if (result.Player2UID >= 0 && AppSrv.g_ClientMgr.GetClientByUID(result.Player2UID, out ClientInfo _c2))
                result.Player2NickName = _c2.NickName;

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

            Data_RoomData newRoom = new Data_RoomData();
            newRoom.Init(GetNewRoomID(), msg.GameRomID, msg.GameRomHash);
            AddRoom(newRoom);


            //加入
            if (!Join(newRoom.GameRomID, 0, _c, out ErrorCode joinErrcode))
            {
                AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomCreate, (int)joinErrcode, new byte[1]);
                return;
            }

            //创建成功下行
            Protobuf_Room_Create_RESP resp = new Protobuf_Room_Create_RESP()
            {
                RoomMiniInfo = GetProtoDataRoom(newRoom)
            };
            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomCreate, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(resp));

        }

        public void OnCmdRoomJoin(Socket sk, byte[] reqData)
        {
            AppSrv.g_Log.Debug($"OnCmdRoomJoin ");
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            Protobuf_Room_Join msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_Join>(reqData);

            //加入
            if (!Join(msg.RoomID, msg.PlayerNum, _c, out ErrorCode joinErrcode))
            {
                AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomJoin, (int)joinErrcode, new byte[1]);
                return;
            }

            Data_RoomData roomData = GetRoomData(msg.RoomID);

            //创建成功下行
            Protobuf_Room_Join_RESP resp = new Protobuf_Room_Join_RESP()
            {
                RoomMiniInfo = GetProtoDataRoom(roomData)
            };

            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomJoin, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(resp));
            Protobuf_Room_MyRoom_State_Change(msg.RoomID);
        }
        public void OnCmdRoomLeave(Socket sk, byte[] reqData)
        {
            AppSrv.g_Log.Debug($"OnCmdRoomJoin ");
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            Protobuf_Room_Leave msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_Leave>(reqData);

            //加入
            if (!Leave(msg.RoomID, _c, out ErrorCode joinErrcode))
            {
                AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomLeave, (int)joinErrcode, new byte[1]);
                return;
            }

            //创建成功下行
            Protobuf_Room_Leave_RESP resp = new Protobuf_Room_Leave_RESP()
            {
                RoomID = msg.RoomID,
            };

            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomLeave, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(resp));
            Protobuf_Room_MyRoom_State_Change(msg.RoomID);
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

        #region 房间内逻辑
        /// <summary>
        /// 进入房间
        /// </summary>
        /// <param name="RoomID"></param>
        /// <param name="PlayerNum"></param>
        /// <param name="_c"></param>
        /// <param name="errcode"></param>
        /// <returns></returns>
        bool Join(int RoomID, int PlayerNum, ClientInfo _c, out ErrorCode errcode)
        {
            Data_RoomData room = GetRoomData(RoomID);
            if (room == null)
            {
                errcode = ErrorCode.ErrorRoomNotFound;
                return false;
            }
            //玩家1
            if (PlayerNum == 0)
            {
                if (room.PlayerState != RoomPlayerState.NonePlayerState)
                {
                    errcode = ErrorCode.ErrorRoomSlotReadlyHadPlayer;
                    return false;
                }
                room.SetPlayerUID(0,_c);
            }
            //其他玩家
            else
            {
                if (room.PlayerState != RoomPlayerState.OnlyP1)
                {
                    errcode = ErrorCode.ErrorRoomSlotReadlyHadPlayer;
                    return false;
                }
                room.SetPlayerUID(1, _c);
            }

            //广播房间
            SendRoomUpdateToAll(RoomID, 0);
            errcode = ErrorCode.ErrorOk;
            return true;
        }

        /// <summary>
        /// 离开房间
        /// </summary>
        /// <param name="RoomID"></param>
        /// <param name="_c"></param>
        /// <param name="errcode"></param>
        /// <returns></returns>
        bool Leave(int RoomID, ClientInfo _c, out ErrorCode errcode)
        {
            Data_RoomData room = GetRoomData(RoomID);
            if (room == null)
            {
                errcode = ErrorCode.ErrorRoomNotFound;
                return false;
            }

            room.RemovePlayer(_c);

            if (room.PlayerState == RoomPlayerState.NonePlayerState)
            {
                SendRoomUpdateToAll(RoomID, 1);
                RemoveRoom(RoomID);
            }
            else
            {
                //广播房间变化
                SendRoomUpdateToAll(RoomID, 0);
            }
            errcode = ErrorCode.ErrorOk;
            return true;
        }
        #endregion


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
                if (!mDictRoom.TryGetValue(roomid,out Data_RoomData room))
                    continue;
                if (room.GameState > RoomGameState.InGame)
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
        public bool bNeedLoadState { get; private set; }
        public int LoadStateFrame { get; private set; }
        public Google.Protobuf.ByteString LoadStateRaw { get; set; }
        public long Player1_UID { get; private set; }
        public long Player2_UID { get; private set; }
        public long Player3_UID { get; private set; }
        public long Player4_UID { get; private set; }
        public bool[] PlayerReadyState { get; private set; }
        public List<long> SynUIDs;
        public RoomPlayerState PlayerState => getPlayerState();
        public RoomGameState GameState;
        public uint mCurrFrameId = 0;
        public ServerInputSnapShot mCurrInputData;
        public Queue<(uint, ServerInputSnapShot)> mInputQueue;
        //TODO
        public Dictionary<int, Queue<byte[]>> mDictPlayerIdx2SendQueue;

        public void Init(int roomID, int gameRomID, string roomHash, bool bloadState = false)
        {
            RoomID = roomID;
            GameRomID = gameRomID;
            RomHash = roomHash;
            Player1_UID = -1;
            Player2_UID = -1;
            Player3_UID = -1;
            Player4_UID = -1;
            SynUIDs = new List<long>();//广播角色列表
            GameState = RoomGameState.NoneGameState;
            mCurrInputData = new ServerInputSnapShot();
            mInputQueue = new Queue<(uint, ServerInputSnapShot)>();
        }

        public void SetPlayerUID(int PlayerIdx,ClientInfo _c)
        {
            long oldUID = -1;
            switch (PlayerIdx)
            {
                case 0: oldUID = Player1_UID; Player1_UID = _c.UID; break;
                case 1: oldUID = Player2_UID; Player2_UID = _c.UID; break;
                case 2: oldUID = Player3_UID; Player3_UID = _c.UID; break;
                case 3: oldUID = Player4_UID; Player4_UID = _c.UID; break;
            }
            if(oldUID <= 0)
                SynUIDs.Remove(oldUID);
            SynUIDs.Add(_c.UID); 
            _c.RoomState.SetRoomData(this.RoomID, PlayerIdx);
        }

        public void RemovePlayer(ClientInfo _c)
        {
            int PlayerIdx = GetPlayerIdx(_c);
            switch (PlayerIdx)
            {
                case 0: Player1_UID = -1; SynUIDs.Remove(_c.UID);break;
                case 1: Player2_UID = -1; SynUIDs.Remove(_c.UID);break;
                case 2: Player3_UID = -1; SynUIDs.Remove(_c.UID);break;
                case 3: Player4_UID = -1; SynUIDs.Remove(_c.UID);break;
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

        RoomPlayerState getPlayerState()
        {
            if (Player1_UID < 0 && Player2_UID < 0)
                return RoomPlayerState.NonePlayerState;

            if (Player1_UID < 0)
                return RoomPlayerState.OnlyP2;

            if (Player2_UID < 0)
                return RoomPlayerState.OnlyP1;

            return RoomPlayerState.BothOnline;
        }

        public List<long> GetAllPlayerUIDs()
        {
            List<long> list = new List<long>();
            if (Player1_UID > 0) list.Add(Player1_UID);
            if (Player2_UID > 0) list.Add(Player2_UID);
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

        public void SetPlayerInput(int PlayerIdx,long mFrameID,ushort input)
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
                Protobuf_Room_Syn_RoomFrameAllInput resp = new Protobuf_Room_Syn_RoomFrameAllInput()
                {
                    FrameID = data.frameId,
                    InputData = data.inputdata.all
                };
                AppSrv.g_ClientMgr.ClientSendALL((int)CommandID.CmdRoomSyn, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(resp));
            }
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