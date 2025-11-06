using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.Event;
using AxibugEmuOnline.Client.Network;
using AxibugProtobuf;
using AxiReplay;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppRoom
    {
        public Protobuf_Room_MiniInfo mineRoomMiniInfo { get; private set; } = null;
        public bool InRoom => App.user.IsLoggedIn && mineRoomMiniInfo != null;
        public bool IsHost => mineRoomMiniInfo?.HostPlayerUID == App.user.userdata.UID;
        public bool IsScreenProviderUID => mineRoomMiniInfo?.ScreenProviderUID == App.user.userdata.UID;
        public RoomGameState RoomState => mineRoomMiniInfo.GameState;
        public int WaitStep { get; private set; } = -1;
        public byte[] RawData { get; private set; } = null;
        public NetReplay netReplay { get; private set; }

        Dictionary<int, Protobuf_Room_MiniInfo> dictRoomListID2Info = new Dictionary<int, Protobuf_Room_MiniInfo>();

        struct S_PlayerMiniInfo
        {
            public long UID;
            public string NickName;
        }

        Protobuf_Room_List _Protobuf_Room_List = new Protobuf_Room_List();
        Protobuf_Room_Get_Screen _Protobuf_Room_Get_Screen = new Protobuf_Room_Get_Screen();
        Protobuf_Room_Create _Protobuf_Room_Create = new Protobuf_Room_Create();
        Protobuf_Room_Join _Protobuf_Room_Join = new Protobuf_Room_Join();
        Protobuf_Room_Leave _Protobuf_Room_Leave = new Protobuf_Room_Leave();
        Protobuf_Room_Change_PlaySlotWithJoy _Protobuf_Room_Change_PlaySlotWithJoy = new Protobuf_Room_Change_PlaySlotWithJoy();
        Protobuf_Room_Player_Ready _Protobuf_Room_Player_Ready = new Protobuf_Room_Player_Ready();
        Protobuf_Room_SinglePlayerInputData _Protobuf_Room_SinglePlayerInputData = new Protobuf_Room_SinglePlayerInputData();
        Protobuf_Screnn_Frame _Protobuf_Screnn_Frame = new Protobuf_Screnn_Frame();
        Protobuf_Room_HostPlayer_UpdateStateRaw _Protobuf_Room_HostPlayer_UpdateStateRaw = new Protobuf_Room_HostPlayer_UpdateStateRaw();
        public AppRoom()
        {
            NetMsg.Instance.RegNetMsgEvent<Protobuf_Room_List_RESP>((int)CommandID.CmdRoomList, RecvGetRoomList);
            NetMsg.Instance.RegNetMsgEvent<Protobuf_Room_Update_RESP>((int)CommandID.CmdRoomListUpdate, RecvGetRoomListUpdate);
            NetMsg.Instance.RegNetMsgEvent<Protobuf_Room_Get_Screen_RESP>((int)CommandID.CmdRoomGetScreen, RecvRoomGetScreen);
            NetMsg.Instance.RegNetMsgEvent<Protobuf_Room_Create_RESP>((int)CommandID.CmdRoomCreate, RecvCreateRoom);
            NetMsg.Instance.RegNetMsgEvent<Protobuf_Room_Join_RESP>((int)CommandID.CmdRoomJoin, RecvJoinRoom);
            NetMsg.Instance.RegNetMsgEvent<Protobuf_Room_Leave_RESP>((int)CommandID.CmdRoomLeave, RecvLeavnRoom);
            NetMsg.Instance.RegNetMsgEvent<Protobuf_Room_MyRoom_State_Change>((int)CommandID.CmdRoomMyRoomStateChanged, RecvRoomMyRoomStateChange);
            NetMsg.Instance.RegNetMsgEvent<Protobuf_Room_WaitStep_RESP>((int)CommandID.CmdRoomWaitStep, RecvRoom_WaitStep);
            NetMsg.Instance.RegNetMsgEvent<Protobuf_Room_HostPlayer_UpdateStateRaw_RESP>((int)CommandID.CmdRoomHostPlayerUpdateStateRaw, RecvHostPlayer_UpdateStateRaw);
            NetMsg.Instance.RegNetMsgEvent<Protobuf_Room_Syn_RoomFrameAllInputData>((int)CommandID.CmdRoomSynPlayerInput, RecvHostSyn_RoomFrameAllInputData);
            NetMsg.Instance.RegNetMsgEvent<Protobuf_Screnn_Frame>((int)CommandID.CmdScreen, OnScreen);
        }

        #region 房间列表管理
        bool AddOrUpdateRoomList(Protobuf_Room_MiniInfo roomInfo)
        {
            bool bNew = !dictRoomListID2Info.ContainsKey(roomInfo.RoomID);
            dictRoomListID2Info[roomInfo.RoomID] = roomInfo;
            return bNew;
        }
        bool RemoveRoomList(int roomId)
        {
            if (dictRoomListID2Info.ContainsKey(roomId))
            {
                dictRoomListID2Info.Remove(roomId);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取单个房间MiniInfo
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="MiniInfo"></param>
        /// <returns></returns>
        public bool GetRoomListMiniInfo(int roomId, out Protobuf_Room_MiniInfo MiniInfo)
        {
            if (dictRoomListID2Info.ContainsKey(roomId))
            {
                MiniInfo = dictRoomListID2Info[roomId];
                return true;
            }
            MiniInfo = null;
            return false;
        }
        public List<Protobuf_Room_MiniInfo> GetRoomList()
        {
            List<Protobuf_Room_MiniInfo> result = new List<Protobuf_Room_MiniInfo>();
            foreach (var item in dictRoomListID2Info)
            {
                result.Add(item.Value);
            }
            return result;
        }
        #endregion

        #region Replay
        public void InitRePlay()
        {
            netReplay = new NetReplay();
            netReplay.ResetData();
        }
        public void ReleaseRePlay()
        {
            netReplay.ResetData();
        }
        #endregion

        #region 房间管理
        List<Protobuf_Room_GamePlaySlot> GetMinePlayerSlotInfo()
        {
            if (mineRoomMiniInfo == null)
                return null;
            return mineRoomMiniInfo.GamePlaySlotList.Where(w => w.PlayerUID == App.user.userdata.UID).ToList();
        }

        long[] GetRoom4PlayerUIDs()
        {
            if (mineRoomMiniInfo == null)
                return null;
            long[] result = new long[mineRoomMiniInfo.GamePlaySlotList.Count];
            for (int i = 0; i < mineRoomMiniInfo.GamePlaySlotList.Count; i++)
            {
                if (mineRoomMiniInfo.GamePlaySlotList[i].PlayerUID > 0)
                    result[i] = mineRoomMiniInfo.GamePlaySlotList[i].PlayerUID;
            }
            return result;
        }

        Protobuf_Room_GamePlaySlot[] GetRoom4GameSlotMiniInfos()
        {
            if (mineRoomMiniInfo == null)
                return null;
            return mineRoomMiniInfo.GamePlaySlotList.ToArray();
        }

        #endregion

        /// <summary>
        /// 获取所有房间列表
        /// </summary>
        /// <param name="ChatMsg"></param>
        public void SendGetRoomList()
        {
            App.log.Info("拉取房间列表");
            App.network.SendToServer((int)CommandID.CmdRoomList, _Protobuf_Room_List);
        }

        /// <summary>
        /// 获取所有房间列表
        /// </summary>
        /// <param name="reqData"></param>
        void RecvGetRoomList(Protobuf_Room_List_RESP msg)
        {
            App.log.Info("取得完整房间列表");
            dictRoomListID2Info.Clear();
            for (int i = 0; i < msg.RoomMiniInfoList.Count; i++)
                AddOrUpdateRoomList(msg.RoomMiniInfoList[i]);
            Eventer.Instance.PostEvent(EEvent.OnRoomListAllUpdate);
        }

        /// <summary>
        /// 获取单个列表更新
        /// </summary>
        /// <param name="reqData"></param>
        void RecvGetRoomListUpdate(Protobuf_Room_Update_RESP msg)
        {
            App.log.Debug("单个房间状态更新");
            if (msg.UpdateType == 0)
            {
                if (AddOrUpdateRoomList(msg.RoomMiniInfo))
                {
                    Eventer.Instance.PostEvent(EEvent.OnRoomListSingleAdd, msg.RoomMiniInfo.RoomID);
                }
                else
                {
                    Eventer.Instance.PostEvent(EEvent.OnRoomListSingleUpdate, msg.RoomMiniInfo.RoomID);
                }
            }
            else
            {
                RemoveRoomList(msg.RoomMiniInfo.RoomID);
                Eventer.Instance.PostEvent(EEvent.OnRoomListSingleClose, msg.RoomMiniInfo.RoomID);
            }
        }

        /// <summary>
        /// 获取房间画面快照
        /// </summary>
        /// <param name="RoomID"></param>
        public void SendGetRoomScreen(int RoomID)
        {
            _Protobuf_Room_Get_Screen.RoomID = RoomID;
            App.log.Info($"获取房间画面快照");
            App.network.SendToServer((int)CommandID.CmdRoomGetScreen, _Protobuf_Room_Get_Screen);
        }
        /// <summary>
        /// 获取单个房间画面
        /// </summary>
        /// <param name="reqData"></param>
        void RecvRoomGetScreen(Protobuf_Room_Get_Screen_RESP msg)
        {
            App.log.Debug("单个房间状态更新");
            //解压
            byte[] data = Helper.DecompressByteArray(msg.RawBitmap.ToArray());
            Eventer.Instance.PostEvent(EEvent.OnRoomGetRoomScreen, msg.RoomID, data);
        }

        /// <summary>
        /// 创建房间
        /// </summary>
        /// <param name="GameRomID"></param>
        /// <param name="JoinPlayerIdx"></param>
        /// <param name="GameRomHash"></param>
        public void SendCreateRoom(int GameRomID, string GameRomHash = null)
        {
            _Protobuf_Room_Create.GameRomID = GameRomID;
            _Protobuf_Room_Create.GameRomHash = GameRomHash;
            App.log.Info($"创建房间");
            App.network.SendToServer((int)CommandID.CmdRoomCreate, _Protobuf_Room_Create);
        }

        /// <summary>
        /// 创建房间成功
        /// </summary>
        /// <param name="reqData"></param>
        void RecvCreateRoom(Protobuf_Room_Create_RESP msg)
        {
            App.log.Debug("创建房间成功");
            mineRoomMiniInfo = msg.RoomMiniInfo;
            InitRePlay();
            Eventer.Instance.PostEvent(EEvent.OnMineRoomCreated);
            OverlayManager.PopTip($"房间创建成功");

        }

        /// <summary>
        /// 创建房间
        /// </summary>
        /// <param name="RoomID"></param>
        /// <param name="JoinSlotIdx">加入时所在SlotIdx</param>
        /// <param name="LocalJoyIdx">加入时候本地对应JoyIdx</param>
        public void SendJoinRoom(int RoomID)
        {
            _Protobuf_Room_Join.RoomID = RoomID;
            App.log.Info($"加入房间");
            App.network.SendToServer((int)CommandID.CmdRoomJoin, _Protobuf_Room_Join);
        }

        /// <summary>
        /// 加入房间成功
        /// </summary>
        /// <param name="reqData"></param>
        void RecvJoinRoom(Protobuf_Room_Join_RESP msg)
        {
            App.log.Debug("加入房间成功");
            mineRoomMiniInfo = msg.RoomMiniInfo;
            InitRePlay();
            {
                Eventer.Instance.PostEvent(EEvent.OnMineJoinRoom);
                OverlayManager.PopTip($"已进入[{msg.RoomMiniInfo.GetHostNickName()}]的房间");
            }
        }

        /// <summary>
        /// 离开房间
        /// </summary>
        /// <param name="RoomID"></param>
        public void SendLeavnRoom()
        {
            if (!InRoom)
                return;
            _Protobuf_Room_Leave.RoomID = mineRoomMiniInfo.RoomID;
            App.log.Info($"LeavnRoom");
            App.network.SendToServer((int)CommandID.CmdRoomLeave, _Protobuf_Room_Leave);
        }

        /// <summary>
        /// 离开房间成功
        /// </summary>
        /// <param name="reqData"></param>
        void RecvLeavnRoom(Protobuf_Room_Leave_RESP msg)
        {
            App.log.Debug("离开房间成功");
            ReleaseRePlay();
            mineRoomMiniInfo = null;
            Eventer.Instance.PostEvent(EEvent.OnMineLeavnRoom);
            OverlayManager.PopTip($"你已经离开房间");
        }

        void RecvRoomMyRoomStateChange(Protobuf_Room_MyRoom_State_Change msg)
        {
            if (mineRoomMiniInfo == null)
            {
                App.log.Error("RecvRoomMyRoomStateChange 时 mineRoomMiniInfo 为空");
                return;
            }
            long[] oldRoomPlayer = GetRoom4PlayerUIDs();
            Protobuf_Room_GamePlaySlot[] oldslotArr = GetRoom4GameSlotMiniInfos();
            mineRoomMiniInfo = msg.RoomMiniInfo;
            long[] newRoomPlayer = GetRoom4PlayerUIDs();
            Protobuf_Room_GamePlaySlot[] newslotArr = GetRoom4GameSlotMiniInfos();

            oldRoomPlayer = oldRoomPlayer.Where(w => w > 0).Distinct().ToArray();
            newRoomPlayer = newRoomPlayer.Where(w => w > 0).Distinct().ToArray();
            //离开用户
            foreach (var leavn in oldRoomPlayer.Where(w => !newRoomPlayer.Contains(w)))
            {
                UserDataBase oldplayer = App.user.GetUserByUid(leavn);
                string oldPlayName = oldplayer != null ? oldplayer.NickName : "Player";
                OverlayManager.PopTip($"[{oldPlayName}]离开房间");
                Eventer.Instance.PostEvent(EEvent.OnOtherPlayerLeavnRoom, leavn);
            }
            //新加入用户
            foreach (var newJoin in newRoomPlayer.Where(w => !oldRoomPlayer.Contains(w)))
            {
                UserDataBase newplayer = App.user.GetUserByUid(newJoin);
                string newplayerName = newplayer != null ? newplayer.NickName : "Player";
                OverlayManager.PopTip($"[{newplayer.NickName}]进入房间");
                Eventer.Instance.PostEvent(EEvent.OnOtherPlayerJoinRoom, newJoin);
            }

            for (int i = 0; i < 4; i++)
            {
                var oldSlot = oldslotArr[i];
                var newSlot = newslotArr[i];
                if (oldSlot.PlayerUID <= 0 && newSlot.PlayerUID <= 0)
                    continue;
                if (
                    oldSlot.PlayerUID != newSlot.PlayerUID
                    ||
                    oldSlot.PlayerLocalJoyIdx != newSlot.PlayerLocalJoyIdx
                    )
                {
                    if (newSlot.PlayerUID > 0)
                    {
                        OverlayManager.PopTip($"[{newSlot.PlayerNickName}]使用:P{i}");
                    }
                }
            }

            Eventer.Instance.PostEvent(EEvent.OnRoomSlotDataChanged);


            //for (int i = 0; i < 4; i++)
            //{
            //    long OldPlayer = oldRoomPlayer[i];
            //    long NewPlayer = newRoomPlayer[i];
            //    if (OldPlayer == NewPlayer)
            //        continue;

            //    //位置之前有人，但是离开了
            //    if (OldPlayer > 0)
            //    {
            //        Eventer.Instance.PostEvent(EEvent.OnOtherPlayerLeavnRoom, i, OldPlayer);
            //        UserDataBase oldplayer = App.user.GetUserByUid(OldPlayer);
            //        string oldPlayName = oldplayer != null ? oldplayer.NickName : "Player";
            //        OverlayManager.PopTip($"[{oldPlayName}]离开房间,手柄位:P{i}");
            //        if (NewPlayer > 0)//而且害换了一个玩家
            //        {
            //            Eventer.Instance.PostEvent(EEvent.OnOtherPlayerJoinRoom, i, NewPlayer);
            //            mineRoomMiniInfo.GetPlayerNameByPlayerIdx((uint)i, out string PlayerName);
            //            OverlayManager.PopTip($"[{PlayerName}]进入房间,手柄位:P{i}");
            //        }
            //    }
            //    else //之前没人
            //    {
            //        Eventer.Instance.PostEvent(EEvent.OnOtherPlayerJoinRoom, i, NewPlayer);
            //        mineRoomMiniInfo.GetPlayerNameByPlayerIdx((uint)i, out string PlayerName);
            //        OverlayManager.PopTip($"[{PlayerName}]进入房间,手柄位:P{i}");
            //    }

            //    //位置之前有人，但是离开了
            //    if (OldPlayer > 0)
            //    {
            //        Eventer.Instance.PostEvent(EEvent.OnOtherPlayerLeavnRoom, i, OldPlayer);
            //        UserDataBase oldplayer = App.user.GetUserByUid(OldPlayer);
            //        string oldPlayName = oldplayer != null ? oldplayer.NickName : "Player";
            //        OverlayManager.PopTip($"[{oldPlayName}]离开房间,手柄位:P{i}");
            //        if (NewPlayer > 0)//而且害换了一个玩家
            //        {
            //            Eventer.Instance.PostEvent(EEvent.OnOtherPlayerJoinRoom, i, NewPlayer);
            //            mineRoomMiniInfo.GetPlayerNameByPlayerIdx((uint)i, out string PlayerName);
            //            OverlayManager.PopTip($"[{PlayerName}]进入房间,手柄位:P{i}");
            //        }
            //    }
            //    else //之前没人
            //    {
            //        Eventer.Instance.PostEvent(EEvent.OnOtherPlayerJoinRoom, i, NewPlayer);
            //        mineRoomMiniInfo.GetPlayerNameByPlayerIdx((uint)i, out string PlayerName);
            //        OverlayManager.PopTip($"[{PlayerName}]进入房间,手柄位:P{i}");
            //    }
            //}
        }

        /// <summary>
        /// 发送修改玩家槽位,但是增量
        /// </summary>
        /// <param name="dictSlotIdx2LocalJoyIdx">玩家占用房间GamePlaySlot和LocalJoyIdx字典</param>
        public void SendChangePlaySlotIdxWithJoyIdx(uint localJoyIndex, uint slotIndex)
        {
            if (!App.roomMgr.InRoom) return;

            Dictionary<uint, uint> temp = new Dictionary<uint, uint>();
            for (int i = 0; i < App.roomMgr.mineRoomMiniInfo.GamePlaySlotList.Count; i++)
            {
                var item = App.roomMgr.mineRoomMiniInfo.GamePlaySlotList[i];
                if (item.PlayerUID <= 0) continue;
                if (item.PlayerUID != App.user.userdata.UID) return;
                temp[(uint)i] = (uint)item.PlayerLocalJoyIdx;
            }
            temp[slotIndex] = localJoyIndex;

            SendChangePlaySlotIdxWithJoyIdx(temp);
        }

        /// <summary>
        /// 发送修改玩家槽位,全量
        /// </summary>
        /// <param name="dictSlotIdx2LocalJoyIdx">玩家占用房间GamePlaySlot和LocalJoyIdx字典</param>
        public void SendChangePlaySlotIdxWithJoyIdx(Dictionary<uint, uint> dictSlotIdx2LocalJoyIdx)
        {
            if (!InRoom)
                return;

            _Protobuf_Room_Change_PlaySlotWithJoy.SlotWithJoy.Clear();

            foreach (var slotdata in dictSlotIdx2LocalJoyIdx)
            {
                _Protobuf_Room_Change_PlaySlotWithJoy.SlotWithJoy.Add(new Protobuf_PlaySlotIdxWithJoyIdx()
                {
                    PlayerSlotIdx = (int)slotdata.Key,
                    PlayerLocalJoyIdx = (int)slotdata.Value,
                });
            }

            App.log.Info($"SendChangePlaySlotIdxWithJoyIdx");
            App.network.SendToServer((int)CommandID.CmdRoomChangePlayerWithJoy, _Protobuf_Room_Change_PlaySlotWithJoy);
        }
        /// <summary>
        /// 上报即时存档
        /// </summary>
        /// <param name="RoomID"></param>
        public void SendHostRaw(byte[] RawData)
        {
            //压缩
            byte[] compressRawData = Helper.CompressByteArray(RawData);
            _Protobuf_Room_HostPlayer_UpdateStateRaw.LoadStateRaw = Google.Protobuf.ByteString.CopyFrom(compressRawData);
            App.log.Info($"上报即时存档数据 原数据大小:{RawData.Length},压缩后;{compressRawData.Length}");
            App.network.SendToServer((int)CommandID.CmdRoomHostPlayerUpdateStateRaw, _Protobuf_Room_HostPlayer_UpdateStateRaw);
            _Protobuf_Room_HostPlayer_UpdateStateRaw.Reset();
        }

        void RecvRoom_WaitStep(Protobuf_Room_WaitStep_RESP msg)
        {
            if (WaitStep != msg.WaitStep)
            {
                WaitStep = msg.WaitStep;
                if (WaitStep == 1)
                {
                    byte[] decompressRawData = Helper.DecompressByteArray(msg.LoadStateRaw.ToByteArray());
                    App.log.Info($"收到即时存档数据 解压后;{decompressRawData.Length}");
                    RawData = decompressRawData;
                    ReleaseRePlay();
                }
                Eventer.Instance.PostEvent(EEvent.OnRoomWaitStepChange, WaitStep);
            }
        }

        void RecvHostPlayer_UpdateStateRaw(Protobuf_Room_HostPlayer_UpdateStateRaw_RESP msg)
        {
            App.log.Info($"即时存档上报成功");
        }

        /// <summary>
        /// 即时存档加载完毕
        /// </summary>
        /// <param name="PushFrameNeedTimeUs">push帧所需平均时间（微秒）</param>
        /// <param name="LoadStateNeedTimeUs">加载即时存档所需平均时间（微秒）</param>
        /// <param name="VideoFrameShowNeedTimeUs">视频一帧所需时间（微秒）</param>
        /// <param name="AudioFramePlayNeedTimeUs">音频处理一帧所需时间（微秒）</param>
        public void SendRoomPlayerReady(float PushFrameNeedTimeUs, float LoadStateNeedTimeUs, float VideoFrameShowNeedTimeUs, float AudioFramePlayNeedTimeUs)
        {
            App.log.Debug("上报准备完毕");
            _Protobuf_Room_Player_Ready.PushFrameNeedTimeUs = PushFrameNeedTimeUs;
            _Protobuf_Room_Player_Ready.LoadStateNeedTimeUs = LoadStateNeedTimeUs;
            _Protobuf_Room_Player_Ready.VideoFrameShowNeedTimeUs = VideoFrameShowNeedTimeUs;
            _Protobuf_Room_Player_Ready.AudioFramePlayNeedTimeUs = AudioFramePlayNeedTimeUs;
            App.network.SendToServer((int)CommandID.CmdRoomPlayerReady, _Protobuf_Room_Player_Ready);
        }

        /// <summary>
        /// 同步上行
        /// </summary>
        public void SendRoomSingelPlayerInput(uint FrameID, ulong InputData)
        {
            _Protobuf_Room_SinglePlayerInputData.FrameID = FrameID;
            _Protobuf_Room_SinglePlayerInputData.InputData = InputData;
            App.network.SendToServer((int)CommandID.CmdRoomSingelPlayerInput, _Protobuf_Room_SinglePlayerInputData);
        }

        ulong TestAllData = 0;
        void RecvHostSyn_RoomFrameAllInputData(Protobuf_Room_Syn_RoomFrameAllInputData msg)
        {
            if (TestAllData != msg.InputData)
            {
                TestAllData = msg.InputData;
                App.log.Debug($"ServerFrameID->{msg.ServerFrameID} FrameID->{msg.FrameID} ClientFrame->{netReplay.mCurrClientFrameIdx} InputData->{msg.InputData}");
            }
            netReplay.InData(new ReplayStep() { FrameStartID = (int)msg.FrameID, InPut = msg.InputData }, (int)msg.ServerFrameID, msg.ServerForwardCount);
        }

        public void SendScreen(byte[] RenderBuffer)
        {
            //压缩
            byte[] comData = Helper.CompressByteArray(RenderBuffer);
            _Protobuf_Screnn_Frame.FrameID = 0;
            _Protobuf_Screnn_Frame.RawBitmap = ByteString.CopyFrom(comData);
            App.network.SendToServer((int)CommandID.CmdScreen, _Protobuf_Screnn_Frame);
            _Protobuf_Screnn_Frame.Reset();
        }

        public void OnScreen(Protobuf_Screnn_Frame msg)
        {
            //解压
            byte[] data = Helper.DecompressByteArray(msg.RawBitmap.ToArray());
        }

        public void ChangeCurrRoomPlayerName(long uid)
        {
            UserDataBase userdata = App.user.GetUserByUid(uid);
            if (userdata == null)
                return;

            if (mineRoomMiniInfo == null)
            {
                foreach (var gameslot in mineRoomMiniInfo.GamePlaySlotList)
                {
                    if (gameslot.PlayerUID == uid)
                        gameslot.PlayerNickName = userdata.NickName;
                }
            }
        }
    }

    public static class RoomEX
    {
        /// <summary>
        /// 获取房间空闲席位下标 (返回True表示由余位）
        /// </summary>
        /// <param name="roomMiniInfo"></param>
        /// <param name="freeSlots"></param>
        /// <returns></returns>
        public static bool GetFreeSlot(this Protobuf_Room_MiniInfo roomMiniInfo, ref List<int> freeSlots)
        {
            freeSlots.Clear();
            for (int i = 0; i < roomMiniInfo.GamePlaySlotList.Count; i++)
            {
                if (roomMiniInfo.GamePlaySlotList[i].PlayerUID <= 0)
                    freeSlots.Add(i);
            }
            return freeSlots.Count > 0;
        }

        /// <summary>
        /// 指定uid和该uid的本地手柄序号,获取占用的手柄位
        /// </summary>
        public static bool GetPlayerSlotIdxByUid(this Protobuf_Room_MiniInfo roomMiniInfo, long uid, int joyIdx, out uint? slotIdx)
        {
            slotIdx = null;

            for (int i = 0; i < roomMiniInfo.GamePlaySlotList.Count; i++)
            {
                if (roomMiniInfo.GamePlaySlotList[i].PlayerUID == uid && roomMiniInfo.GamePlaySlotList[i].PlayerLocalJoyIdx == joyIdx)
                {
                    slotIdx = (uint)i;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 按照房间玩家下标获取昵称
        /// </summary>
        /// <param name="roomMiniInfo"></param>
        /// <param name="PlayerIndex"></param>
        /// <param name="PlayerName"></param>
        /// <returns></returns>
        public static bool GetPlayerNameByPlayerIdx(this Protobuf_Room_MiniInfo roomMiniInfo, uint GameSlotIdx, out string PlayerName)
        {
            PlayerName = string.Empty;
            if (roomMiniInfo.GamePlaySlotList[(int)GameSlotIdx].PlayerUID > 0)
                PlayerName = roomMiniInfo.GamePlaySlotList[(int)GameSlotIdx].PlayerNickName;
            return string.IsNullOrEmpty(PlayerName);
        }

        /// <summary>
        /// 获取玩家手柄类型
        /// </summary>
        /// <param name="roomMiniInfo"></param>
        /// <param name="GameSlotIdx"></param>
        /// <param name="gamePadType"></param>
        /// <returns></returns>
        public static bool GetPlayerGamePadTypeByPlayerIdx(this Protobuf_Room_MiniInfo roomMiniInfo, uint GameSlotIdx, out GamePadType gamePadType)
        {
            if (roomMiniInfo.GamePlaySlotList[(int)GameSlotIdx].PlayerUID > 0)
            {
                gamePadType = roomMiniInfo.GamePlaySlotList[(int)GameSlotIdx].PlayerLocalGamePadType;
                return true;
            }
            gamePadType = GamePadType.GlobalGamePad;
            return false;
        }
    }
}