﻿using AxibugEmuOnline.Client.ClientCore;
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
        public bool InRoom => mineRoomMiniInfo != null;
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
        Protobuf_Room_Player_Ready _Protobuf_Room_Player_Ready = new Protobuf_Room_Player_Ready();
        Protobuf_Room_SinglePlayerInputData _Protobuf_Room_SinglePlayerInputData = new Protobuf_Room_SinglePlayerInputData();
        Protobuf_Screnn_Frame _Protobuf_Screnn_Frame = new Protobuf_Screnn_Frame();
        public AppRoom()
        {
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomList, RecvGetRoomList);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomListUpdate, RecvGetRoomListUpdate);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomGetScreen, RecvRoomGetScreen);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomCreate, RecvCreateRoom);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomJoin, RecvJoinRoom);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomLeave, RecvLeavnRoom);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomMyRoomStateChanged, RecvRoomMyRoomStateChange);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomWaitStep, RecvRoom_WaitStep);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomHostPlayerUpdateStateRaw, RecvHostPlayer_UpdateStateRaw);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomSynPlayerInput, RecvHostSyn_RoomFrameAllInputData);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdScreen, OnScreen);
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
            App.network.SendToServer((int)CommandID.CmdRoomList, ProtoBufHelper.Serizlize(_Protobuf_Room_List));
        }

        /// <summary>
        /// 获取所有房间列表
        /// </summary>
        /// <param name="reqData"></param>
        void RecvGetRoomList(byte[] reqData)
        {
            App.log.Info("取得完整房间列表");
            Protobuf_Room_List_RESP msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_List_RESP>(reqData);
            dictRoomListID2Info.Clear();
            for (int i = 0; i < msg.RoomMiniInfoList.Count; i++)
                AddOrUpdateRoomList(msg.RoomMiniInfoList[i]);
            Eventer.Instance.PostEvent(EEvent.OnRoomListAllUpdate);
        }

        /// <summary>
        /// 获取单个列表更新
        /// </summary>
        /// <param name="reqData"></param>
        void RecvGetRoomListUpdate(byte[] reqData)
        {
            App.log.Debug("单个房间状态更新");
            Protobuf_Room_Update_RESP msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_Update_RESP>(reqData);
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
            App.network.SendToServer((int)CommandID.CmdRoomGetScreen, ProtoBufHelper.Serizlize(_Protobuf_Room_Get_Screen));
        }
        /// <summary>
        /// 获取单个房间画面
        /// </summary>
        /// <param name="reqData"></param>
        void RecvRoomGetScreen(byte[] reqData)
        {
            App.log.Debug("单个房间状态更新");
            Protobuf_Room_Get_Screen_RESP msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_Get_Screen_RESP>(reqData);
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
            App.network.SendToServer((int)CommandID.CmdRoomCreate, ProtoBufHelper.Serizlize(_Protobuf_Room_Create));
        }

        /// <summary>
        /// 创建房间成功
        /// </summary>
        /// <param name="reqData"></param>
        void RecvCreateRoom(byte[] reqData)
        {
            App.log.Debug("创建房间成功");
            Protobuf_Room_Create_RESP msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_Create_RESP>(reqData);
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
            App.network.SendToServer((int)CommandID.CmdRoomJoin, ProtoBufHelper.Serizlize(_Protobuf_Room_Join));
        }

        /// <summary>
        /// 加入房间成功
        /// </summary>
        /// <param name="reqData"></param>
        void RecvJoinRoom(byte[] reqData)
        {
            App.log.Debug("加入房间成功");
            Protobuf_Room_Join_RESP msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_Join_RESP>(reqData);
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
            App.network.SendToServer((int)CommandID.CmdRoomLeave, ProtoBufHelper.Serizlize(_Protobuf_Room_Leave));
        }

        /// <summary>
        /// 离开房间成功
        /// </summary>
        /// <param name="reqData"></param>
        void RecvLeavnRoom(byte[] reqData)
        {
            App.log.Debug("离开房间成功");
            Protobuf_Room_Leave_RESP msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_Leave_RESP>(reqData);
            ReleaseRePlay();
            mineRoomMiniInfo = null;
            Eventer.Instance.PostEvent(EEvent.OnMineLeavnRoom);
            OverlayManager.PopTip($"你已经离开房间");
        }

        void RecvRoomMyRoomStateChange(byte[] reqData)
        {
            Protobuf_Room_MyRoom_State_Change msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_MyRoom_State_Change>(reqData);
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
                OverlayManager.PopTip($"[{newplayer}]进入房间");
                Eventer.Instance.PostEvent(EEvent.OnOtherPlayerJoinRoom, newJoin);
            }

            bool bChangeSlot = false;
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
                    bChangeSlot = true;
                    if (newSlot.PlayerUID > 0)
                    {
                        OverlayManager.PopTip($"[{newSlot.PlayerNickName}]使用:P{i}");
                    }
                }
            }

            if (bChangeSlot)
            {
                Eventer.Instance.PostEvent(EEvent.OnRoomSlotDataChanged);
            }

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
        /// 上报即时存档
        /// </summary>
        /// <param name="RoomID"></param>
        public void SendHostRaw(byte[] RawData)
        {
            //压缩
            byte[] compressRawData = Helper.CompressByteArray(RawData);
            Protobuf_Room_HostPlayer_UpdateStateRaw msg = new Protobuf_Room_HostPlayer_UpdateStateRaw()
            {
                LoadStateRaw = Google.Protobuf.ByteString.CopyFrom(compressRawData)
            };
            App.log.Info($"上报即时存档数据 原数据大小:{RawData.Length},压缩后;{compressRawData.Length}");
            App.network.SendToServer((int)CommandID.CmdRoomHostPlayerUpdateStateRaw, ProtoBufHelper.Serizlize(msg));
        }

        void RecvRoom_WaitStep(byte[] reqData)
        {
            Protobuf_Room_WaitStep_RESP msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_WaitStep_RESP>(reqData);
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

        void RecvHostPlayer_UpdateStateRaw(byte[] reqData)
        {
            Protobuf_Room_HostPlayer_UpdateStateRaw_RESP msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_HostPlayer_UpdateStateRaw_RESP>(reqData);
            App.log.Info($"鸡翅孙当上报成功");
        }

        /// <summary>
        /// 即时存档加载完毕
        /// </summary>
        public void SendRoomPlayerReady()
        {
            App.log.Debug("上报准备完毕");
            App.network.SendToServer((int)CommandID.CmdRoomPlayerReady, ProtoBufHelper.Serizlize(_Protobuf_Room_Player_Ready));
        }

        /// <summary>
        /// 同步上行
        /// </summary>
        public void SendRoomSingelPlayerInput(uint FrameID, uint InputData)
        {
            _Protobuf_Room_SinglePlayerInputData.FrameID = FrameID;
            _Protobuf_Room_SinglePlayerInputData.InputData = InputData;
            App.network.SendToServer((int)CommandID.CmdRoomSingelPlayerInput, ProtoBufHelper.Serizlize(_Protobuf_Room_SinglePlayerInputData));
        }

        ulong TestAllData = 0;
        void RecvHostSyn_RoomFrameAllInputData(byte[] reqData)
        {
            Protobuf_Room_Syn_RoomFrameAllInputData msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_Syn_RoomFrameAllInputData>(reqData);
            if (TestAllData != msg.InputData)
            {
                TestAllData = msg.InputData;
                App.log.Debug($"ServerFrameID->{msg.ServerFrameID} FrameID->{msg.FrameID} ClientFrame->{netReplay.mCurrClientFrameIdx} InputData->{msg.InputData}");
            }
            netReplay.InData(new ReplayStep() { FrameStartID = (int)msg.FrameID, InPut = msg.InputData }, (int)msg.ServerFrameID);
        }

        public void SendScreen(byte[] RenderBuffer)
        {
            //压缩
            byte[] comData = Helper.CompressByteArray(RenderBuffer);
            _Protobuf_Screnn_Frame.FrameID = 0;
            _Protobuf_Screnn_Frame.RawBitmap = ByteString.CopyFrom(comData);
            App.network.SendToServer((int)CommandID.CmdScreen, ProtoBufHelper.Serizlize(_Protobuf_Screnn_Frame));
        }

        public void OnScreen(byte[] reqData)
        {
            Protobuf_Screnn_Frame msg = ProtoBufHelper.DeSerizlize<Protobuf_Screnn_Frame>(reqData);
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
        public static bool GetFreeSlot(this Protobuf_Room_MiniInfo roomMiniInfo, out int[] freeSlots)
        {
            List<int> temp = new List<int>();
            for (int i = 0; i < roomMiniInfo.GamePlaySlotList.Count; i++)
            {
                if (roomMiniInfo.GamePlaySlotList[i].PlayerUID <= 0)
                    temp.Add(i);
            }
            freeSlots = temp.ToArray();
            return freeSlots.Length > 0;
        }

        /// <summary>
        /// 指定uid和该uid的本地手柄序号,获取占用的手柄位
        /// </summary>
        public static bool GetPlayerSlotIdxByUid(this Protobuf_Room_MiniInfo roomMiniInfo, long uid, int joyIdx, out uint? slotIdx)
        {
            slotIdx = null;
            //joyIdx取值返回[0,3],这个序号代表玩家本地的手柄编号
            //todo : 根据uid和controllerIndex 返回占用的位置

            //目前未实现,所有非0号位置的手柄,都返回false


            for (int i = 0; i < roomMiniInfo.GamePlaySlotList.Count; i++)
            {
                if (roomMiniInfo.GamePlaySlotList[i].PlayerUID == uid)
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
    }
}