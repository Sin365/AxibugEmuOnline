namespace AxibugEmuOnline.Client.Event
{
    public enum EEvent
    {
        // 添加你自己需要的事件类型
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
    }
}
