using AxibugEmuOnline.Server.Common;
using AxibugEmuOnline.Server.Manager;
using AxibugEmuOnline.Server.NetWork;
using AxibugProtobuf;
using System.Net;
using System.Net.Sockets;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AxibugEmuOnline.Server
{

    public class RoomManager
    {
        Dictionary<int, Data_RoomData> mDictRoom = new Dictionary<int, Data_RoomData>();
        int RoomIDSeed = 1;
        public RoomManager()
        {
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomList, OnCmdRoomList);

            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomCreate, OnCmdRoomCreate);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomJoin, OnCmdRoomJoin);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdRoomLeave, OnCmdRoomLeave);
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
                }
            }
        }

        void RemoveRoom(int RoomID) 
        {
            lock(mDictRoom) 
            {
                if (mDictRoom.ContainsKey(RoomID))
                {
                    mDictRoom.Remove(RoomID);
                }
            }
        }

        public Data_RoomData GetRoomData(int RoomID) 
        {
            if (!mDictRoom.ContainsKey(RoomID))
                return null;

            return mDictRoom[RoomID];
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
        public void SendRoomUpdateToAll(int RoomID,int type)
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
            newRoom.Init(GetNewRoomID(), msg.GameRomID,msg.GameRomHash);
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

            foreach(ClientInfo _c in userlist) 
            {
                AppSrv.g_ClientMgr.ClientSend(_c,(int)CommandID.CmdRoomMyRoomStateChanged, (int)ErrorCode.ErrorOk, ProtoBufHelper.Serizlize(resp));
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
        bool Join(int RoomID,int PlayerNum,ClientInfo _c,out ErrorCode errcode)
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
                room.Player1_UID = _c.UID;
            }
            //其他玩家
            else
            {
                if (room.PlayerState != RoomPlayerState.OnlyP1)
                {
                    errcode = ErrorCode.ErrorRoomSlotReadlyHadPlayer;
                    return false;
                }
                room.Player2_UID = _c.UID;
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

            if (room.Player1_UID == _c.UID)
                room.Player1_UID = -1;
            if (room.Player2_UID == _c.UID)
                room.Player2_UID = -1;

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
    }

    public class Data_RoomData
    {
        public int RoomID;
        public int GameRomID;
        public string RomHash;
        public long Player1_UID;
        public long Player2_UID;
        public RoomPlayerState PlayerState => getPlayerState();
        public RoomGameState GameState;

        public void Init(int roomID,int gameRomID,string roomHash)
        {
            RoomID = roomID;
            GameRomID = gameRomID;
            RomHash = roomHash;
            Player1_UID = -1;
            Player2_UID = -1;
            GameState = RoomGameState.NoneGameState;
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
                if (!AppSrv.g_ClientMgr.GetClientByUID(uid, out ClientInfo _c,true))
                    continue;

                list.Add(_c);
            }

            return list;
        }
    }
}