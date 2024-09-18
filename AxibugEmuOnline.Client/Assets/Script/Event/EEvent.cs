namespace AxibugEmuOnline.Client.Event
{
    public enum EEvent
    {
        // 添加你自己需要的事件类型
        OnChatMsg,


        OnRoomListAllUpdate,//房间列表全量刷新
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
