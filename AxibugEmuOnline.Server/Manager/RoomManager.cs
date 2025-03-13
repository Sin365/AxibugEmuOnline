using AxibugEmuOnline.Server.Common;
using AxibugEmuOnline.Server.Data;
using AxibugEmuOnline.Server.Manager;
using AxibugEmuOnline.Server.Manager.Client;
using AxibugEmuOnline.Server.Manager.Room;
using AxibugEmuOnline.Server.NetWork;
using AxibugProtobuf;
using MySql.Data.MySqlClient;
using System.Net.Sockets;

namespace AxibugEmuOnline.Server
{
    public class RoomManager
    {
        Dictionary<int, GameRoom> mDictRoom = new Dictionary<int, GameRoom>();
        List<int> mKeyRoomList = new List<int>();
        AutoResetEvent roomTickARE;
        Thread threadRoomTick;

        int RoomIDSeed = 1;
        public RoomManager()
        {
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomList, OnCmdRoomList);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomGetScreen, CmdRoomGetScreen);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomCreate, OnCmdRoomCreate);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomJoin, OnCmdRoomJoin);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomLeave, OnCmdRoomLeave);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomChangePlayerWithJoy, OnCmdRoomChangePlayerWithJoy);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomHostPlayerUpdateStateRaw, OnHostPlayerUpdateStateRaw);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomPlayerReady, OnRoomPlayerReady);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomSingelPlayerInput, OnSingelPlayerInput);

            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdScreen, OnCmdScreen);

            roomTickARE = AppSrv.g_Tick.AddNewARE(TickManager.TickType.Interval_16MS);
            threadRoomTick = new Thread(UpdateLoopTick);
            threadRoomTick.Start();

            //System.Timers.Timer mTimer16ms = new System.Timers.Timer(16);//实例化Timer类
            //mTimer16ms.Elapsed += new System.Timers.ElapsedEventHandler((source, e) => { UpdateAllRoomLogic(); });//到达时间的时候执行事件；
            //mTimer16ms.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
            //mTimer16ms.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
            //mTimer16ms.Start();
        }

        #region 房间管理

        int GetNewRoomID()
        {
            return RoomIDSeed++;
        }

        void AddRoom(GameRoom data)
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
                    mDictRoom[RoomID].Dispose();

                    mDictRoom.Remove(RoomID);
                    mKeyRoomList.Remove(RoomID);
                }
            }
        }

        public GameRoom GetRoomData(int RoomID)
        {
            if (!mDictRoom.TryGetValue(RoomID, out GameRoom data))
                return null;
            return data;
        }

        public void GetRoomList(ref List<GameRoom> roomList)
        {
            lock (mDictRoom)
            {
                foreach (var room in mDictRoom)
                {
                    roomList.AddRange(mDictRoom.Values);
                }
            }
        }

        #endregion

        #region

        public enum RoomLogType
        {
            Create = 0,
            Join = 1,
            Leave = 2
        }
        public void RoomLog(long uid, int platform, int RoomID, int RomID, RoomLogType state)
        {
            string query = "INSERT INTO `haoyue_emu`.`room_log` (`uid`, `platform`, `romid`,`roomid`, `state`) VALUES ( ?uid, ?platform, ?romid, ?roomid, ?state);";
            using (MySqlConnection conn = SQLRUN.GetConn("RoomLog"))
            {
                using (var command = new MySqlCommand(query, conn))
                {
                    // 设置参数值
                    command.Parameters.AddWithValue("?uid", uid);
                    command.Parameters.AddWithValue("?platform", platform);
                    command.Parameters.AddWithValue("?romid", RomID);
                    command.Parameters.AddWithValue("?roomid", RoomID);
                    command.Parameters.AddWithValue("?state", state);
                    command.ExecuteNonQuery();
                }

                if (state == RoomLogType.Create)
                {
                    query = "update romlist set playcount = playcount + 1 where id = ?romid";
                    using (var command = new MySqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("?romid", RomID);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
        #endregion

        private Protobuf_Room_MiniInfo GetProtoDataRoom(GameRoom room)
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
                GamePlatformType = room.GameRomPlatformType
            };

            for (byte i = 0; i < room.PlayerSlot.Count(); i++)
            {
                Protobuf_Room_GamePlaySlot pbSlot = new Protobuf_Room_GamePlaySlot();
                GameRoomSlot slot = room.PlayerSlot[i];
                if (slot.UID > 0)
                {
                    pbSlot.PlayerUID = slot.UID;
                    pbSlot.PlayerLocalJoyIdx = (int)slot.LocalJoyIdx;
                    pbSlot.PlayerLocalGamePadType = slot.LocalGamePadType;
                    if (AppSrv.g_ClientMgr.GetClientByUID(pbSlot.PlayerUID, out ClientInfo _client))
                    {
                        pbSlot.PlayerNickName = _client.NickName;
                        pbSlot.DeviceType = _client.deviceType;
                    }
                }
                result.GamePlaySlotList.Add(pbSlot);
            }

            return result;
        }

        public void OnCmdRoomList(Socket sk, byte[] reqData)
        {
            AppSrv.g_Log.DebugCmd($"OnCmdRoomList");
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            Protobuf_Room_List msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_List>(reqData);

            Protobuf_Room_List_RESP resp = new Protobuf_Room_List_RESP();
            List<GameRoom> temp = ObjectPoolAuto.AcquireList<GameRoom>();
            GetRoomList(ref temp);
            foreach (var room in temp)
                resp.RoomMiniInfoList.Add(GetProtoDataRoom(room));
            ObjectPoolAuto.Release(temp);
            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomList, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(resp));
        }
        public void CmdRoomGetScreen(Socket sk, byte[] reqData)
        {
            AppSrv.g_Log.DebugCmd($"CmdRoomGetScreen");
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            Protobuf_Room_Get_Screen msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_Get_Screen>(reqData);

            GameRoom room = GetRoomData(_c.RoomState.RoomID);
            bool bHadRoomStateChange = false;
            ErrorCode Errcode = ErrorCode.ErrorOk;
            Protobuf_Room_Get_Screen_RESP resp = new Protobuf_Room_Get_Screen_RESP();
            if (room == null)
                Errcode = ErrorCode.ErrorRoomNotFound;
            else
            {
                resp.FrameID = (int)room.mCurrServerFrameId;
                resp.RoomID = room.RoomID;
                resp.RawBitmap = room.ScreenRaw;
            }

            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomGetScreen, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(resp));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="RoomID"></param>
        /// <param name="type">//[0] 更新或新增 [1] 删除</param>
        public void SendRoomUpdateToAll(GameRoom room, int type)
        {
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
            AppSrv.g_Log.DebugCmd($"OnCmdRoomCreate");
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            Protobuf_Room_Create msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_Create>(reqData);
            Protobuf_Room_Create_RESP resp = new Protobuf_Room_Create_RESP();
            GameRoom newRoom = new GameRoom();

            RomPlatformType ptype = AppSrv.g_GameShareMgr.GetRomPlatformType(msg.GameRomID);
            newRoom.Init(GetNewRoomID(), msg.GameRomID, msg.GameRomHash, _c.UID, false, ptype);
            AddRoom(newRoom);
            ErrorCode joinErrcode = ErrorCode.ErrorOk;
            //加入
            if (newRoom.Join(0, 0, _c, out joinErrcode, out bool bHadRoomStateChange))
            {
                //创建成功下行
                resp.RoomMiniInfo = GetProtoDataRoom(newRoom);
            }
            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomCreate, (int)joinErrcode, ProtoBufHelper.Serizlize(resp));

            if (joinErrcode == ErrorCode.ErrorOk && bHadRoomStateChange)
                SendRoomStepChange(newRoom);

            SendRoomUpdateToAll(newRoom, 0);

            RoomLog(_c.UID, 1, newRoom.RoomID, newRoom.GameRomID, RoomLogType.Create);
        }

        public void OnCmdRoomJoin(Socket sk, byte[] reqData)
        {
            AppSrv.g_Log.DebugCmd($"OnCmdRoomJoin");
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            Protobuf_Room_Join msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_Join>(reqData);
            Protobuf_Room_Join_RESP resp = new Protobuf_Room_Join_RESP();
            ErrorCode joinErrcode;
            GameRoom room = GetRoomData(msg.RoomID);
            bool bHadRoomStateChange = false;
            if (room == null)
            {
                joinErrcode = ErrorCode.ErrorRoomNotFound;

                AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomJoin, (int)joinErrcode, ProtoBufHelper.Serizlize(resp));
                return;
            }


            lock (room)
            {

                if (!room.GetFreeSlot(out uint SlotIdx))
                {
                    joinErrcode = ErrorCode.ErrorRoomSlotAlreadlyHadPlayer;
                    AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomJoin, (int)joinErrcode, ProtoBufHelper.Serizlize(resp));
                    return;
                }

                //加入
                if (room.Join(SlotIdx, (uint)0, _c, out joinErrcode, out bHadRoomStateChange))
                {
                    GameRoom roomData = GetRoomData(msg.RoomID);
                    resp.RoomMiniInfo = GetProtoDataRoom(roomData);
                }

                AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomJoin, (int)joinErrcode, ProtoBufHelper.Serizlize(resp));
                Protobuf_Room_MyRoom_State_Change(msg.RoomID);

                if (joinErrcode == ErrorCode.ErrorOk && bHadRoomStateChange)
                    SendRoomStepChange(room);

                if (room != null)
                {
                    SendRoomUpdateToAll(room, 0);
                }
            }
            RoomLog(_c.UID, 1, room.RoomID, room.GameRomID, RoomLogType.Join);
        }
        public void OnCmdRoomLeave(Socket sk, byte[] reqData)
        {
            AppSrv.g_Log.DebugCmd($"OnCmdRoomLeave");
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            Protobuf_Room_Leave msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_Leave>(reqData);
            LeaveRoom(_c, msg.RoomID);
            //Protobuf_Room_Leave_RESP resp = new Protobuf_Room_Leave_RESP();
            //ErrorCode errcode;
            //Data_RoomData room = GetRoomData(_c.RoomState.RoomID);
            //bool bHadRoomStateChange = false;
            //if (room == null)
            //    errcode = ErrorCode.ErrorRoomNotFound;
            //else
            //{
            //    if (room.Leave(_c, out errcode, out bHadRoomStateChange))
            //    {
            //        resp.RoomID = msg.RoomID;
            //    }
            //}
            //AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomLeave, (int)errcode, ProtoBufHelper.Serizlize(resp));
            //Protobuf_Room_MyRoom_State_Change(msg.RoomID);

            //if (errcode == ErrorCode.ErrorOk && bHadRoomStateChange)
            //    SendRoomStepChange(room);

            //SendRoomUpdateToAll(room.RoomID, 1);
            //if (room.GetPlayerCount() < 1)
            //    RemoveRoom(room.RoomID);
        }
        public void LeaveRoom(ClientInfo _c, int RoomID)
        {
            AppSrv.g_Log.Debug($"LeaveRoom");
            if (RoomID < 0)
                return;
            Protobuf_Room_Leave_RESP resp = new Protobuf_Room_Leave_RESP();
            ErrorCode errcode;
            GameRoom room = GetRoomData(_c.RoomState.RoomID);
            bool bHadRoomStateChange = false;
            if (room == null)
            {
                errcode = ErrorCode.ErrorRoomNotFound;
                AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomLeave, (int)errcode, ProtoBufHelper.Serizlize(resp));
                return;
            }

            if (room.Leave(_c, out errcode, out bHadRoomStateChange))
            {
                resp.RoomID = RoomID;
            }
            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomLeave, (int)errcode, ProtoBufHelper.Serizlize(resp));
            Protobuf_Room_MyRoom_State_Change(RoomID);

            if (errcode == ErrorCode.ErrorOk && bHadRoomStateChange)
                SendRoomStepChange(room);

            if (room.GetPlayerCount() < 1)
            {
                SendRoomUpdateToAll(room, 1);
                RemoveRoom(room.RoomID);
            }
            else
                SendRoomUpdateToAll(room, 0);

            RoomLog(_c.UID, 1, room.RoomID, room.GameRomID, RoomLogType.Leave);
        }

        public void OnCmdRoomChangePlayerWithJoy(Socket sk, byte[] reqData)
        {
            AppSrv.g_Log.DebugCmd($"OnCmdRoomChangePlayerjoySlot");
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            Protobuf_Room_Change_PlaySlotWithJoy msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_Change_PlaySlotWithJoy>(reqData);
            Protobuf_Room_Change_PlaySlotWithJoy_RESP resp = new Protobuf_Room_Change_PlaySlotWithJoy_RESP();
            ErrorCode errcode = ErrorCode.ErrorOk;
            GameRoom room = GetRoomData(_c.RoomState.RoomID);
            if (room == null)
            {
                errcode = ErrorCode.ErrorRoomNotFound;
                AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomChangePlayerWithJoy, (int)errcode, ProtoBufHelper.Serizlize(resp));
                return;
            }

            Dictionary<uint, (uint, GamePadType)> newSlotIdx2JoyIdx = new Dictionary<uint, (uint, GamePadType)>();
            foreach (var slotinfo in msg.SlotWithJoy)
            {
                //如果有任意一个槽位有人
                if (room.GetPlayerUIDByIdx((uint)slotinfo.PlayerSlotIdx, out long UID))
                {
                    //且人不是自己，则不允许换位
                    if (UID != _c.UID)
                    {
                        errcode = ErrorCode.ErrorRoomSlotAlreadlyHadPlayer;
                        AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomChangePlayerWithJoy, (int)errcode, ProtoBufHelper.Serizlize(resp));
                        return;
                    }
                }
                newSlotIdx2JoyIdx[(uint)slotinfo.PlayerSlotIdx] = ((uint)slotinfo.PlayerLocalJoyIdx, slotinfo.PlayerLocalGamePadType);
            }
            room.SetPlayerSlotData(_c, ref newSlotIdx2JoyIdx);

            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomChangePlayerWithJoy, (int)errcode, ProtoBufHelper.Serizlize(resp));

            Protobuf_Room_MyRoom_State_Change(room.RoomID);
        }

        public void OnHostPlayerUpdateStateRaw(Socket sk, byte[] reqData)
        {
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            AppSrv.g_Log.DebugCmd($"OnHostPlayerUpdateStateRaw 上报即时存档 UID->{_c.UID}");
            Protobuf_Room_HostPlayer_UpdateStateRaw msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_HostPlayer_UpdateStateRaw>(reqData);
            Protobuf_Room_HostPlayer_UpdateStateRaw_RESP resp = new Protobuf_Room_HostPlayer_UpdateStateRaw_RESP();
            ErrorCode errcode = ErrorCode.ErrorOk;
            GameRoom room = GetRoomData(_c.RoomState.RoomID);
            if (room == null)
                errcode = ErrorCode.ErrorRoomNotFound;
            else if (room.GameState != RoomGameState.WaitRawUpdate)
                errcode = ErrorCode.ErrorRoomCantDoCurrState;

            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomHostPlayerUpdateStateRaw, (int)errcode, ProtoBufHelper.Serizlize(resp));

            if (errcode == ErrorCode.ErrorOk)
            {
                room.SetLoadRaw(msg.LoadStateRaw, out bool bHadRoomStateChange);
                if (bHadRoomStateChange)
                    SendRoomStepChange(room);
            }
        }

        public void OnRoomPlayerReady(Socket sk, byte[] reqData)
        {
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            AppSrv.g_Log.DebugCmd($"OnRoomPlayerReady _c->{_c.UID}");
            Protobuf_Room_Player_Ready msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_Player_Ready>(reqData);
            ErrorCode errcode = ErrorCode.ErrorOk;
            GameRoom room = GetRoomData(_c.RoomState.RoomID);
            if (room == null)
                return;
            lock (room)
            {
                AppSrv.g_Log.Debug($"SetRePlayerReady RoomID->{room.RoomID},UID->{_c.UID}");
                room.SetRePlayerReady(_c.UID, out errcode, out bool bHadRoomStateChange);
                if (bHadRoomStateChange)
                {
                    SendRoomStepChange(room);
                }
            }
        }

        public void OnSingelPlayerInput(Socket sk, byte[] reqData)
        {
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            Protobuf_Room_SinglePlayerInputData msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_SinglePlayerInputData>(reqData);
            GameRoom room = GetRoomData(_c.RoomState.RoomID);
            if (room == null)
                return;

            //取玩家操作数据中的第一个
            ServerInputSnapShot temp = new ServerInputSnapShot();
            temp.all = msg.InputData;

            #region 服务器推帧方案
            room.SetPlayerInput(_c.UID, msg.FrameID, temp);
            #endregion

            #region 客户端推帧方案
            /*
            //是否需要推帧
            if (room.GetNeedForwardTick(msg.FrameID, out long forwaFrame))
            {
                for (int i = 0; i < forwaFrame; i++)
                {
                    if (i + 1 == forwaFrame)//最后一帧
                    {
                        //写入操作前、将网络波动堆积，可能造成瞬时多个连续推帧结果（最后一帧除外）立即广播，不等16msTick
                        //if (forwaFrame > 1)
                        //    room.SynInputData();

                        //推帧过程中，最后一帧才写入操作
                        room.SetPlayerInput(_c.UID, msg.FrameID, temp);
                    }
                    //推帧
                    room.TakeFrame();
                }
            }
            else//不需要推帧
            {
                //虽然不推帧，但是存入Input
                room.SetPlayerInput(_c.UID, msg.FrameID, temp);
            }
            */
            #endregion

            if (room.LastTestRecv != room.mCurrInputData.all)
            {
                room.LastTestRecv = room.mCurrInputData.all;
#if DEBUG
                AppSrv.g_Log.Debug($" {DateTime.Now.ToString("hh:mm:ss.fff")} SynTestRecv=> UID->{_c.UID} roomId->{room.mCurrServerFrameId} input->{msg.InputData}");
#endif
            }
        }

        public void OnCmdScreen(Socket sk, byte[] reqData)
        {
            AppSrv.g_Log.DebugCmd($"OnCmdScreen lenght:{reqData.Length}");
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            Protobuf_Screnn_Frame msg = ProtoBufHelper.DeSerizlize<Protobuf_Screnn_Frame>(reqData);
            GameRoom room = AppSrv.g_Room.GetRoomData(msg.RoomID);
            room.InputScreenData(msg.RawBitmap);
        }

        /// <summary>
        /// 广播房间状态变化
        /// </summary>
        /// <param name="RoomID"></param>
        public void Protobuf_Room_MyRoom_State_Change(int RoomID)
        {
            GameRoom room = GetRoomData(RoomID);
            if (room == null)
                return;

            Protobuf_Room_MyRoom_State_Change resp = new Protobuf_Room_MyRoom_State_Change()
            {
                RoomMiniInfo = GetProtoDataRoom(room)
            };

            List<ClientInfo> userlist = ObjectPoolAuto.AcquireList<ClientInfo>();
            room.GetAllPlayerClientList(ref userlist);

            foreach (ClientInfo _c in userlist)
            {
                AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomMyRoomStateChanged, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(resp));
            }

            ObjectPoolAuto.Release(userlist);
        }

        /// <summary>
        /// 广播联机Step
        /// </summary>
        /// <param name="room"></param>
        public void SendRoomStepChange(GameRoom room)
        {

            List<ClientInfo> roomClient = ObjectPoolAuto.AcquireList<ClientInfo>();
            room.GetAllPlayerClientList(ref roomClient);

            switch (room.GameState)
            {
                case RoomGameState.WaitRawUpdate:
                    {
                        Protobuf_Room_WaitStep_RESP resp = new Protobuf_Room_WaitStep_RESP()
                        {
                            WaitStep = 0
                        };
                        AppSrv.g_Log.DebugCmd($"Step:0 WaitRawUpdate 广播等待主机上报即时存档");
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
                        AppSrv.g_Log.DebugCmd($"Step:1 WaitReady 广播即时存档");
                        AppSrv.g_ClientMgr.ClientSend(roomClient, (int)CommandID.CmdRoomWaitStep, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(resp));
                    }
                    break;
                case RoomGameState.InOnlineGame:
                    {
                        Protobuf_Room_WaitStep_RESP resp = new Protobuf_Room_WaitStep_RESP()
                        {
                            WaitStep = 2,
                        };
                        AppSrv.g_Log.DebugCmd($"Step:2 InOnlineGame 广播开始游戏");
                        AppSrv.g_ClientMgr.ClientSend(roomClient, (int)CommandID.CmdRoomWaitStep, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(resp));
                    }
                    break;
            }

            ObjectPoolAuto.Release(roomClient);
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
            if (mKeyRoomList.Count < 1)
                return;
            for (int i = 0; i < mKeyRoomList.Count; i++)
            {
                int roomid = mKeyRoomList[i];
                if (!mDictRoom.TryGetValue(roomid, out GameRoom room) || room.GameState < RoomGameState.InOnlineGame)
                    continue;
                //更新帧（服务器主动跑时用）
                room.TakeFrame();
                //广播
                room.SynInputData();
            }
        }
        #endregion
    }


}