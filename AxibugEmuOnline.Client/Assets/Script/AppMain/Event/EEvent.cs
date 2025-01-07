namespace AxibugEmuOnline.Client.Event
{
    public enum EEvent
    {
        // 添加你自己需要的事件类型


        /// <summary>
        /// 登录成功
        /// </summary>
        OnLoginSucceed,
        /// <summary>
        /// 当登录被置为false时
        /// </summary>
        OnLossLoginState,
        /// <summary>
        /// 登录失败
        /// </summary>
        OnLoginFailed,

        OnChatMsg,

        //自己的信息更新(比如改名更新）
        OnSelfInfoUpdate,
        //更新其他用户信息
        OnOtherUserInfoUpdate,

        //当收藏数发生变化
        OnDoStars,

        //用户列表，登录和离开
        OnUserListAllUpdate,
        OnUserLogin,
        OnUserLoginOut,

        OnRoomListAllUpdate,//房间列表全量刷新
        OnRoomListSingleAdd,//房间列表中新增房间
        OnRoomListSingleUpdate,//房间列表中单个更新
        OnRoomListSingleClose,//房间关闭
        OnRoomGetRoomScreen,//获取到房间数据

        /// <summary>
        /// 我的房间创建成功
        /// </summary>
        OnMineRoomCreated,
        /// <summary>
        /// 我进入房间
        /// </summary>
        OnMineJoinRoom,
        /// <summary>
        /// 我离开房间
        /// </summary>
        OnMineLeavnRoom,

        /// <summary>
        /// 其他人进入房间
        /// </summary>
        OnOtherPlayerJoinRoom,

        /// <summary>
        /// 其他人离开房间
        /// </summary>
        OnOtherPlayerLeavnRoom,

        /// <summary>
        /// 服务器等待Step更新
        /// </summary>
        OnRoomWaitStepChange,

        /// <summary>
        /// 当房间中手柄位信息发生任何变化时触发,进入房间后也应该触发
        /// </summary>
        OnRoomSlotDataChanged,
        /// <summary>
        /// 当手柄连接设置发生变化时触发
        /// </summary>
        OnControllerConnectChanged,
        /// <summary>
        /// 当本机手柄渴望插入时触发
        /// <para>参数: <see cref="int"/> 本地手柄序号[0,3]</para>
        /// </summary>
        OnLocalJoyDesireInvert,
        /// <summary>
        /// 当Rom文件下载完毕时触发
        /// <para><see cref="int"/>RomID</para>
        /// </summary>
        OnRomFileDownloaded,
    }
}
