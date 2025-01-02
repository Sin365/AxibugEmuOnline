using AxibugEmuOnline.Server.Common;
using AxibugEmuOnline.Server.Manager;
using AxibugEmuOnline.Server.NetWork;
using AxibugProtobuf;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Crypto.Parameters;
using System.Collections.Generic;
using System.Data;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Policy;

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
                    mDictRoom[RoomID].Dispose();

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

        public void GetRoomList(ref List<Data_RoomData> roomList)
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
            MySqlConnection conn = Haoyue_SQLPoolManager.DequeueSQLConn("RoomLog");
            try
            {
                string query = "INSERT INTO `haoyue_emu`.`room_log` (`uid`, `platform`, `romid`,`roomid`, `state`) VALUES ( ?uid, ?platform, ?romid, ?roomid, ?state);";
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
                    query = "update romlist_nes set playcount = playcount + 1 where id = ?romid";
                    using (var command = new MySqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("?romid", RomID);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
            }
            Haoyue_SQLPoolManager.EnqueueSQLConn(conn);
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
            };

            for (byte i = 0; i < room.PlayerSlot.Count(); i++)
            {
                Protobuf_Room_GamePlaySlot pbSlot = new Protobuf_Room_GamePlaySlot();
                Data_RoomSlot slot = room.PlayerSlot[i];
                if (slot.UID > 0)
                {
                    pbSlot.PlayerUID = slot.UID;
                    pbSlot.PlayerLocalJoyIdx = (int)slot.LocalJoyIdx;
                    if (AppSrv.g_ClientMgr.GetClientByUID(pbSlot.PlayerUID, out ClientInfo _client))
                        pbSlot.PlayerNickName = _client.NickName;
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
            List<Data_RoomData> temp = ObjectPoolAuto.AcquireList<Data_RoomData>();
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

            Data_RoomData room = GetRoomData(_c.RoomState.RoomID);
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
        public void SendRoomUpdateToAll(Data_RoomData room, int type)
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

            Data_RoomData newRoom = new Data_RoomData();
            newRoom.Init(GetNewRoomID(), msg.GameRomID, msg.GameRomHash, _c.UID);
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
            Data_RoomData room = GetRoomData(msg.RoomID);
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
                    Data_RoomData roomData = GetRoomData(msg.RoomID);
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
            Data_RoomData room = GetRoomData(_c.RoomState.RoomID);
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
            Data_RoomData room = GetRoomData(_c.RoomState.RoomID);
            if (room == null)
            {
                errcode = ErrorCode.ErrorRoomNotFound;
                AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdRoomChangePlayerWithJoy, (int)errcode, ProtoBufHelper.Serizlize(resp));
                return;
            }

            Dictionary<uint, uint> newSlotIdx2JoyIdx = new Dictionary<uint, uint>();
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
                newSlotIdx2JoyIdx[(uint)slotinfo.PlayerSlotIdx] = (uint)slotinfo.PlayerLocalJoyIdx;
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
                    SendRoomStepChange(room);
            }
        }

        public void OnRoomPlayerReady(Socket sk, byte[] reqData)
        {
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            AppSrv.g_Log.DebugCmd($"OnRoomPlayerReady _c->{_c.UID}");
            Protobuf_Room_Player_Ready msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_Player_Ready>(reqData);
            ErrorCode errcode = ErrorCode.ErrorOk;
            Data_RoomData room = GetRoomData(_c.RoomState.RoomID);
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
            Data_RoomData room = GetRoomData(_c.RoomState.RoomID);
            if (room == null)
                return;

            //取玩家操作数据中的第一个
            ServerInputSnapShot temp = new ServerInputSnapShot();
            temp.all = msg.InputData;
            //room.SetPlayerInput(_c.RoomState.PlayerIdx, msg.FrameID, temp);

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

            if (room.LastTestRecv != room.mCurrInputData.all)
            {
                room.LastTestRecv = room.mCurrInputData.all;
                //AppSrv.g_Log.Debug($" {DateTime.Now.ToString("hh:mm:ss.fff")} SynTestRecv=> UID->{_c.UID} roomId->{room.mCurrServerFrameId} input->{msg.InputData}");
            }
        }

        public void OnCmdScreen(Socket sk, byte[] reqData)
        {
            AppSrv.g_Log.DebugCmd($"OnCmdScreen lenght:{reqData.Length}");
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
        public void SendRoomStepChange(Data_RoomData room)
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
                if (!mDictRoom.TryGetValue(roomid, out Data_RoomData room) || room.GameState < RoomGameState.InOnlineGame)
                    continue;
                //更新帧
                //room.TakeFrame();
                //广播
                room.SynInputData();
            }
        }
        #endregion
    }

    public class Data_RoomData : IDisposable
    {
        public int RoomID { get; private set; }
        public int GameRomID { get; private set; }
        public string RomHash { get; private set; }
        public long HostUID { get; private set; }
        public long ScreenProviderUID { get; private set; }
        public Data_RoomSlot[] PlayerSlot;
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
        const int SynLimitOnSec = 63;

        object synInputLock = new object();
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
        public void Init(int roomID, int gameRomID, string roomHash, long hostUId, bool bloadState = false)
        {
            Dispose();
            RoomID = roomID;
            GameRomID = gameRomID;
            RomHash = roomHash;
            HostUID = hostUId;
            ScreenProviderUID = hostUId;

            if (PlayerSlot == null)
            {
                PlayerSlot = new Data_RoomSlot[4];
                for (uint i = 0; i < PlayerSlot.Length; i++)
                    PlayerSlot[i] = new Data_RoomSlot();
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

        public Dictionary<uint, uint> GetSlotDataByUID(long uid)
        {
            Dictionary<uint, uint> slotIdx2JoyIdx = new Dictionary<uint, uint>();
            var dataarr = PlayerSlot.Where(w => w.UID == uid).ToArray();
            foreach (var slot in dataarr)
                slotIdx2JoyIdx[slot.SlotIdx] = slot.LocalJoyIdx;
            return slotIdx2JoyIdx;
        }
        /// <summary>
        /// 按照SlotIdx设置Input
        /// </summary>
        /// <param name="slotIdx"></param>
        void SetInputDataBySlotIdx(uint slotIdx, byte val)
        {
            switch (slotIdx)
            {
                case 0: mCurrInputData.p1_byte = val; break;
                case 1: mCurrInputData.p2_byte = val; break;
                case 2: mCurrInputData.p3_byte = val; break;
                case 4: mCurrInputData.p3_byte = val; break;
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
            switch (slotIdx)
            {
                case 0: mCurrInputData.p1_byte = 0; break;
                case 1: mCurrInputData.p2_byte = 0; break;
                case 2: mCurrInputData.p3_byte = 0; break;
                case 4: mCurrInputData.p3_byte = 0; break;
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

        public void SetPlayerSlotData(ClientInfo _c, ref readonly Dictionary<uint, uint> newSlotIdx2JoyIdx)
        {
            Dictionary<uint, uint> oldSlotIdx2JoyIdx = GetSlotDataByUID(_c.UID);
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
                if (old_slotjoyIdx != newSlotIdx2JoyIdx[old_slotIdx])
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
                PlayerSlot[slotdata.Key].LocalJoyIdx = slotdata.Value;
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
                case 0: SetInputDataBySlotIdx(SlotIdx, clieninput.p1_byte); break;
                case 1: SetInputDataBySlotIdx(SlotIdx, clieninput.p2_byte); break;
                case 2: SetInputDataBySlotIdx(SlotIdx, clieninput.p3_byte); break;
                case 3: SetInputDataBySlotIdx(SlotIdx, clieninput.p4_byte); break;
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
            mCurrInputData.all = 1;

            UpdateRoomForwardNum();

            uint StartForwardFrames = (SrvForwardFrames * 2) + 5;
            StartForwardFrames = Math.Min(10, StartForwardFrames);
            //服务器提前跑帧数
            for (int i = 0; i < SrvForwardFrames; i++)
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
            List<(uint frameId, ServerInputSnapShot inputdata)> temp = null;
            bool flagInitList = false;
            lock (synInputLock)
            {
                double timeNow = AppSrv.g_Tick.timeNow;
                while (mInputQueue.Count > 0)
                {
                    if (send2time.Count >= SynLimitOnSec)
                    {
                        //AppSrv.g_Log.Info($"{timeNow} - {send2time[0]} =>{timeNow - send2time[0]}");
                        if (timeNow - send2time[0] < 1f) //最早的历史发送还在一秒之内
                            break;
                        else
                            send2time.RemoveAt(0);
                    }

                    if (!flagInitList)
                    {
                        flagInitList = true;
                        //temp = new List<(uint frameId, ServerInputSnapShot inputdata)>();
                        temp = ObjectPoolAuto.AcquireList<(uint frameId, ServerInputSnapShot inputdata)>();
                    }
                    temp.Add(mInputQueue.Dequeue());
                    send2time.Add(timeNow);
                }
                //while (mInputQueue.Count > 0)
                //{
                //    temp.Add(mInputQueue.Dequeue());
                //}
            }

            if (!flagInitList)
                return;

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
            Dictionary<uint, uint> slotInfo = new Dictionary<uint, uint>();
            slotInfo[slotIdx] = joyIdx;
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
        public void SetPlayerInput(long UID, uint LocalJoyIdx, ServerInputSnapShot clieninput)
        {
            for (uint i = 0; i < PlayerSlot.Count(); i++)
            {
                Data_RoomSlot slotData = PlayerSlot[i];
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
    }

    public class Data_RoomSlot
    {
        public uint SlotIdx { get; set; }
        public long UID { get; set; }
        public uint LocalJoyIdx { get; set; }
        public bool Ready = false;
        public void Init(uint SlotIdx)
        {
            this.SlotIdx = SlotIdx;
            UID = -1;
            LocalJoyIdx = 0;
            Ready = false;
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct ServerInputSnapShot
    {
        [FieldOffset(0)]
        public UInt64 all;

        [FieldOffset(0)]
        public byte p1_byte;
        [FieldOffset(1)]
        public byte p2_byte;
        [FieldOffset(2)]
        public byte p3_byte;
        [FieldOffset(3)]
        public byte p4_byte;

        [FieldOffset(0)]
        public ushort p1_ushort;
        [FieldOffset(2)]
        public ushort p2_ushort;
        [FieldOffset(4)]
        public ushort p3_ushort;
        [FieldOffset(6)]
        public ushort p4_ushort;
    }
}