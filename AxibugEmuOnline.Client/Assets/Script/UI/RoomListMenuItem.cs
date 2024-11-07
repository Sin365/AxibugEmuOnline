using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Event;
using System;

namespace AxibugEmuOnline.Client
{
    public class RoomListMenuItem : VirtualSubMenuItem
    {
        bool m_entering;

        protected override void Awake()
        {
            Eventer.Instance.RegisterEvent<int>(EEvent.OnRoomListAllUpdate, OnRoomListUpdateAll);
        }

        public override bool OnEnterItem()
        {
            var res = base.OnEnterItem();
            if (res) m_entering = true;
            return res;
        }

        public override bool OnExitItem()
        {
            var res = base.OnExitItem();
            if (res) m_entering = false;
            return res;
        }

        private void OnRoomListUpdateAll(int obj)
        {
            if (m_entering)
            {
                RefreshUI();
            }
        }

        protected override void GetVirtualListDatas(Action<object> datas)
        {
            var roomList = App.roomMgr.GetRoomList();
            datas.Invoke(roomList);
        }

    }
}
