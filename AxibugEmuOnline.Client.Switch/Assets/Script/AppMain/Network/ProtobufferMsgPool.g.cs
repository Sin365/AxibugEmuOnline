using AxibugEmuOnline.Client.Network;

namespace AxibugProtobuf
{
    public sealed partial class Protobuf_ChatMsg : IResetable
    {
        public void Reset()
        {
			ChatMsg = string.Empty;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_ChatMsg_RESP : IResetable
    {
        public void Reset()
        {
			NickName = string.Empty;
			ChatMsg = string.Empty;
			Date = new System.Int64();
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Ping : IResetable
    {
        public void Reset()
        {
			Seed = 0;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Pong : IResetable
    {
        public void Reset()
        {
			Seed = 0;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Login : IResetable
    {
        public void Reset()
        {
			LoginType = (LoginType)0;
			DeviceType = (DeviceType)0;
			DeviceStr = string.Empty;
			Account = string.Empty;
			Password = string.Empty;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Login_RESP : IResetable
    {
        public void Reset()
        {
			NickName = string.Empty;
			Token = string.Empty;
			LastLoginDate = string.Empty;
			RegDate = string.Empty;
			Status = (LoginResultStatus)0;
			UID = new System.Int64();
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Token_Struct : IResetable
    {
        public void Reset()
        {
			UID = new System.Int64();
			TokenGenDate = new System.Int64();
			Seed = new System.Int64();
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_UserList : IResetable
    {
        public void Reset()
        {
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_UserList_RESP : IResetable
    {
        public void Reset()
        {
			UserCount = 0;
			UserList?.Clear();
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_UserJoin_RESP : IResetable
    {
        public void Reset()
        {
			UserInfo?.Reset();
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_UserLeave_RESP : IResetable
    {
        public void Reset()
        {
			UID = new System.Int64();
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_UserState_RESP : IResetable
    {
        public void Reset()
        {
			UID = new System.Int64();
			State = 0;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class UserMiniInfo : IResetable
    {
        public void Reset()
        {
			UID = new System.Int64();
			NickName = string.Empty;
			DeviceType = (DeviceType)0;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Modify_NickName : IResetable
    {
        public void Reset()
        {
			NickName = string.Empty;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Modify_NickName_RESP : IResetable
    {
        public void Reset()
        {
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Update_UserInfo_RESP : IResetable
    {
        public void Reset()
        {
			UserInfo?.Reset();
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Update_OtherUserInfo_RESP : IResetable
    {
        public void Reset()
        {
			UID = new System.Int64();
			UserInfo?.Reset();
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_List : IResetable
    {
        public void Reset()
        {
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_List_RESP : IResetable
    {
        public void Reset()
        {
			RoomMiniInfoList?.Clear();
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_MiniInfo : IResetable
    {
        public void Reset()
        {
			RoomID = 0;
			GameRomID = 0;
			GameRomHash = string.Empty;
			GamePlatformType = (RomPlatformType)0;
			HostPlayerUID = new System.Int64();
			GameState = (RoomGameState)0;
			ObsUserCount = 0;
			ScreenProviderUID = new System.Int64();
			GamePlaySlotList?.Clear();
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_GamePlaySlot : IResetable
    {
        public void Reset()
        {
			PlayerUID = new System.Int64();
			PlayerNickName = string.Empty;
			DeviceType = (DeviceType)0;
			PlayerLocalJoyIdx = 0;
			PlayerLocalGamePadType = (GamePadType)0;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_Update_RESP : IResetable
    {
        public void Reset()
        {
			UpdateType = 0;
			RoomMiniInfo?.Reset();
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Screnn_Frame : IResetable
    {
        public void Reset()
        {
			RoomID = 0;
			FrameID = 0;
			RawBitmap = Google.Protobuf.ByteString.Empty;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_SinglePlayerInputData : IResetable
    {
        public void Reset()
        {
			FrameID = new System.UInt32();
			InputData = new System.UInt64();
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_Syn_RoomFrameAllInputData : IResetable
    {
        public void Reset()
        {
			FrameID = new System.UInt32();
			InputData = new System.UInt64();
			ServerFrameID = new System.UInt32();
			ServerForwardCount = new System.UInt32();
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_Create : IResetable
    {
        public void Reset()
        {
			GameRomID = 0;
			GameRomHash = string.Empty;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_Create_RESP : IResetable
    {
        public void Reset()
        {
			RoomMiniInfo?.Reset();
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_Join : IResetable
    {
        public void Reset()
        {
			RoomID = 0;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_Join_RESP : IResetable
    {
        public void Reset()
        {
			RoomMiniInfo?.Reset();
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_Leave : IResetable
    {
        public void Reset()
        {
			RoomID = 0;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_Leave_RESP : IResetable
    {
        public void Reset()
        {
			RoomID = 0;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_MyRoom_State_Change : IResetable
    {
        public void Reset()
        {
			RoomMiniInfo?.Reset();
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_Change_PlaySlotWithJoy : IResetable
    {
        public void Reset()
        {
			SlotWithJoy?.Clear();
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_PlaySlotIdxWithJoyIdx : IResetable
    {
        public void Reset()
        {
			PlayerSlotIdx = 0;
			PlayerLocalJoyIdx = 0;
			PlayerLocalGamePadType = (GamePadType)0;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_Change_PlaySlotWithJoy_RESP : IResetable
    {
        public void Reset()
        {
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_WaitStep_RESP : IResetable
    {
        public void Reset()
        {
			WaitStep = 0;
			LoadStateRaw = Google.Protobuf.ByteString.Empty;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_HostPlayer_UpdateStateRaw : IResetable
    {
        public void Reset()
        {
			LoadStateRaw = Google.Protobuf.ByteString.Empty;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_HostPlayer_UpdateStateRaw_RESP : IResetable
    {
        public void Reset()
        {
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_Player_Ready : IResetable
    {
        public void Reset()
        {
			PushFrameNeedTimeUs = 0f;
			LoadStateNeedTimeUs = 0f;
			VideoFrameShowNeedTimeUs = 0f;
			AudioFramePlayNeedTimeUs = 0f;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_Get_Screen : IResetable
    {
        public void Reset()
        {
			RoomID = 0;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_Get_Screen_RESP : IResetable
    {
        public void Reset()
        {
			RoomID = 0;
			FrameID = 0;
			RawBitmap = Google.Protobuf.ByteString.Empty;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Game_Mark : IResetable
    {
        public void Reset()
        {
			RomID = 0;
			Motion = 0;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Game_Mark_RESP : IResetable
    {
        public void Reset()
        {
			RomID = 0;
			IsStar = 0;
			Stars = 0;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Mine_GetGameSavList : IResetable
    {
        public void Reset()
        {
			RomID = 0;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Mine_GetGameSavList_RESP : IResetable
    {
        public void Reset()
        {
			RomID = 0;
			SavDataList?.Clear();
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Mine_GameSavInfo : IResetable
    {
        public void Reset()
        {
			BHadSaveData = false;
			SavID = new System.Int64();
			Uid = new System.Int64();
			SavDataIdx = 0;
			RomID = 0;
			GamePlatformType = (RomPlatformType)0;
			SavDate = string.Empty;
			SavName = string.Empty;
			Note = string.Empty;
			SavImgUrl = string.Empty;
			SavUrl = string.Empty;
			Sequence = 0;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Mine_DelGameSav : IResetable
    {
        public void Reset()
        {
			RomID = 0;
			SavDataIdx = 0;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Mine_DelGameSav_RESP : IResetable
    {
        public void Reset()
        {
			RomID = 0;
			SavDataIdx = 0;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Mine_UpLoadGameSav : IResetable
    {
        public void Reset()
        {
			RomID = 0;
			SavDataIdx = 0;
			Name = string.Empty;
			Note = string.Empty;
			SavImg = Google.Protobuf.ByteString.Empty;
			StateRaw = Google.Protobuf.ByteString.Empty;
			Sequence = 0;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Mine_UpLoadGameSav_RESP : IResetable
    {
        public void Reset()
        {
			RomID = 0;
			SavDataIdx = 0;
			UploadSevInfo?.Reset();
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_GameScreen_Img_Upload : IResetable
    {
        public void Reset()
        {
			RomID = 0;
			SavImg = Google.Protobuf.ByteString.Empty;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_GameScreen_Img_Upload_RESP : IResetable
    {
        public void Reset()
        {
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class pb_AxiEssgssStatusData : IResetable
    {
        public void Reset()
        {
			MemberData?.Clear();
			Array2DMemberData?.Clear();
			ClassData?.Clear();
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class pb_AxiEssgssStatusData_ByteData : IResetable
    {
        public void Reset()
        {
			KeyName = string.Empty;
			Raw = Google.Protobuf.ByteString.Empty;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class pb_AxiEssgssStatusData_2DArray : IResetable
    {
        public void Reset()
        {
			KeyName = string.Empty;
			Rows = 0;
			Cols = 0;
			Raw = Google.Protobuf.ByteString.Empty;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class pb_AxiEssgssStatusData_ClassData : IResetable
    {
        public void Reset()
        {
			KeyName = string.Empty;
			ClassData?.Reset();
        }
    }
}

