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
			Date = default;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Ping : IResetable
    {
        public void Reset()
        {
			Seed = default;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Pong : IResetable
    {
        public void Reset()
        {
			Seed = default;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Login : IResetable
    {
        public void Reset()
        {
			LoginType = default;
			DeviceType = default;
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
			Status = default;
			UID = default;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Token_Struct : IResetable
    {
        public void Reset()
        {
			UID = default;
			TokenGenDate = default;
			Seed = default;
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
			UserCount = default;
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
			UID = default;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_UserState_RESP : IResetable
    {
        public void Reset()
        {
			UID = default;
			State = default;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class UserMiniInfo : IResetable
    {
        public void Reset()
        {
			UID = default;
			NickName = string.Empty;
			DeviceType = default;
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
			UID = default;
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
			RoomID = default;
			GameRomID = default;
			GameRomHash = string.Empty;
			GamePlatformType = default;
			HostPlayerUID = default;
			GameState = default;
			ObsUserCount = default;
			ScreenProviderUID = default;
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
			PlayerUID = default;
			PlayerNickName = string.Empty;
			DeviceType = default;
			PlayerLocalJoyIdx = default;
			PlayerLocalGamePadType = default;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_Update_RESP : IResetable
    {
        public void Reset()
        {
			UpdateType = default;
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
			RoomID = default;
			FrameID = default;
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
			FrameID = default;
			InputData = default;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_Syn_RoomFrameAllInputData : IResetable
    {
        public void Reset()
        {
			FrameID = default;
			InputData = default;
			ServerFrameID = default;
			ServerForwardCount = default;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_Create : IResetable
    {
        public void Reset()
        {
			GameRomID = default;
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
			RoomID = default;
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
			RoomID = default;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_Leave_RESP : IResetable
    {
        public void Reset()
        {
			RoomID = default;
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
			PlayerSlotIdx = default;
			PlayerLocalJoyIdx = default;
			PlayerLocalGamePadType = default;
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
			WaitStep = default;
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
			PushFrameNeedTimeUs = default;
			LoadStateNeedTimeUs = default;
			VideoFrameShowNeedTimeUs = default;
			AudioFramePlayNeedTimeUs = default;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_Get_Screen : IResetable
    {
        public void Reset()
        {
			RoomID = default;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Room_Get_Screen_RESP : IResetable
    {
        public void Reset()
        {
			RoomID = default;
			FrameID = default;
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
			RomID = default;
			Motion = default;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Game_Mark_RESP : IResetable
    {
        public void Reset()
        {
			RomID = default;
			IsStar = default;
			Stars = default;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Mine_GetGameSavList : IResetable
    {
        public void Reset()
        {
			RomID = default;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Mine_GetGameSavList_RESP : IResetable
    {
        public void Reset()
        {
			RomID = default;
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
			BHadSaveData = default;
			SavID = default;
			Uid = default;
			SavDataIdx = default;
			RomID = default;
			GamePlatformType = default;
			SavDate = string.Empty;
			SavName = string.Empty;
			Note = string.Empty;
			SavImgUrl = string.Empty;
			SavUrl = string.Empty;
			Sequence = default;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Mine_DelGameSav : IResetable
    {
        public void Reset()
        {
			RomID = default;
			SavDataIdx = default;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Mine_DelGameSav_RESP : IResetable
    {
        public void Reset()
        {
			RomID = default;
			SavDataIdx = default;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Mine_UpLoadGameSav : IResetable
    {
        public void Reset()
        {
			RomID = default;
			SavDataIdx = default;
			Name = string.Empty;
			Note = string.Empty;
			SavImg = Google.Protobuf.ByteString.Empty;
			StateRaw = Google.Protobuf.ByteString.Empty;
			Sequence = default;
        }
    }
}


namespace AxibugProtobuf
{
    public sealed partial class Protobuf_Mine_UpLoadGameSav_RESP : IResetable
    {
        public void Reset()
        {
			RomID = default;
			SavDataIdx = default;
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
			RomID = default;
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
			Rows = default;
			Cols = default;
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

