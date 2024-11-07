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
        public bool InRoom => mineRoomMiniInfo != null;
        public bool IsHost => mineRoomMiniInfo?.HostPlayerUID == App.user.userdata.UID;
        public bool IsScreenProviderUID => mineRoomMiniInfo?.ScreenProviderUID == App.user.userdata.UID;
        public RoomGameState RoomState => mineRoomMiniInfo.GameState;
        public int MinePlayerIdx => GetMinePlayerIndex();
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
        void AddOrUpdateRoomList(Protobuf_Room_MiniInfo roomInfo)
        {
            dictRoomListID2Info[roomInfo.RoomID] = roomInfo;
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
        int GetMinePlayerIndex()
        {
            if (mineRoomMiniInfo == null)
                return -1;

            if (mineRoomMiniInfo.Player1UID == App.user.userdata.UID)
                return 0;
            if (mineRoomMiniInfo.Player2UID == App.user.userdata.UID)
                return 1;
            if (mineRoomMiniInfo.Player3UID == App.user.userdata.UID)
                return 2;
            if (mineRoomMiniInfo.Player4UID == App.user.userdata.UID)
                return 3;
            return -1;
        }

        long[] GetRoom4PlayerUIDs()
        {
            if (mineRoomMiniInfo == null)
                return null;
            long[] result = new long[4];
            if (mineRoomMiniInfo.Player1UID > 0)
                result[0] = mineRoomMiniInfo.Player1UID;
            if (mineRoomMiniInfo.Player2UID > 0)
                result[1] = mineRoomMiniInfo.Player2UID;
            if (mineRoomMiniInfo.Player3UID > 0)
                result[2] = mineRoomMiniInfo.Player3UID;
            if (mineRoomMiniInfo.Player4UID > 0)
                result[3] = mineRoomMiniInfo.Player4UID;
            return result;
        }

        S_PlayerMiniInfo[] GetRoom4PlayerMiniInfos()
        {
            if (mineRoomMiniInfo == null)
                return null;
            S_PlayerMiniInfo[] result = new S_PlayerMiniInfo[4];
            if (mineRoomMiniInfo.Player1UID > 0)
            {
                result[0].UID = mineRoomMiniInfo.Player1UID;
                result[0].NickName = mineRoomMiniInfo.Player1NickName;
            }
            if (mineRoomMiniInfo.Player2UID > 0)
            {
                result[1].UID = mineRoomMiniInfo.Player2UID;
                result[1].NickName = mineRoomMiniInfo.Player2NickName;
            }
            if (mineRoomMiniInfo.Player3UID > 0)
            {
                result[2].UID = mineRoomMiniInfo.Player3UID;
                result[2].NickName = mineRoomMiniInfo.Player3NickName;
            }
            if (mineRoomMiniInfo.Player4UID > 0)
            {
                result[3].UID = mineRoomMiniInfo.Player4UID;
                result[3].NickName = mineRoomMiniInfo.Player4NickName;
            }
            return result;
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
                AddOrUpdateRoomList(msg.RoomMiniInfo);
                Eventer.Instance.PostEvent(EEvent.OnRoomListSingleUpdate, msg.RoomMiniInfo.GameRomID);
            }
            else
            {
                RemoveRoomList(msg.RoomMiniInfo.GameRomID);
                Eventer.Instance.PostEvent(EEvent.OnRoomListSingleClose, msg.RoomMiniInfo.GameRomID);
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
        public void SendCreateRoom(int GameRomID, int JoinPlayerIdx, string GameRomHash = null)
        {
            _Protobuf_Room_Create.JoinPlayerIdx = JoinPlayerIdx;
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
        }

        /// <summary>
        /// 创建房间
        /// </summary>
        /// <param name="GameRomID"></param>
        /// <param name="JoinPlayerIdx"></param>
        /// <param name="GameRomHash"></param>
        public void SendJoinRoom(int RoomID, int JoinPlayerIdx)
        {
            _Protobuf_Room_Join.RoomID = RoomID;
            _Protobuf_Room_Join.PlayerNum = JoinPlayerIdx;
            App.log.Info($"创建房间");
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
            Eventer.Instance.PostEvent(EEvent.OnMineJoinRoom);
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
            App.log.Debug("加入房间成功");
            Protobuf_Room_Leave_RESP msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_Leave_RESP>(reqData);
            ReleaseRePlay();
            mineRoomMiniInfo = null;
            Eventer.Instance.PostEvent(EEvent.OnMineLeavnRoom);
        }

        void RecvRoomMyRoomStateChange(byte[] reqData)
        {
            Protobuf_Room_MyRoom_State_Change msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_MyRoom_State_Change>(reqData);
            long[] oldRoomPlayer = GetRoom4PlayerUIDs();
            mineRoomMiniInfo = msg.RoomMiniInfo;
            long[] newRoomPlayer = GetRoom4PlayerUIDs();
            for (int i = 0; i < 4; i++)
            {
                long OldPlayer = oldRoomPlayer[i];
                long NewPlayer = newRoomPlayer[i];
                if (OldPlayer == NewPlayer)
                    continue;
                //位置之前有人，但是离开了
                if (OldPlayer > 0)
                {
                    Eventer.Instance.PostEvent(EEvent.OnOtherPlayerLeavnRoom, i, OldPlayer);
                    if (NewPlayer > 0)//而且害换了一个玩家
                        Eventer.Instance.PostEvent(EEvent.OnOtherPlayerJoinRoom, i, NewPlayer);
                }
                else //之前没人
                    Eventer.Instance.PostEvent(EEvent.OnOtherPlayerJoinRoom, i, NewPlayer);
            }
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
                Eventer.Instance.PostEvent(EEvent.OnRoomWaitStepChange, WaitStep);
                if (WaitStep == 1)
                {
                    byte[] decompressRawData = Helper.DecompressByteArray(msg.LoadStateRaw.ToByteArray());
                    App.log.Info($"收到即时存档数据 解压后;{decompressRawData.Length}");
                    RawData = decompressRawData;
                }
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

        void RecvHostSyn_RoomFrameAllInputData(byte[] reqData)
        {
            Protobuf_Room_Syn_RoomFrameAllInputData msg = ProtoBufHelper.DeSerizlize<Protobuf_Room_Syn_RoomFrameAllInputData>(reqData);
            //netReplay.InData(new ReplayStep() { FrameStartID = (int)msg.FrameID, InPut = msg.InputData }, (int)msg.ServerFrameID);
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
    }
}