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
            Eventer.Instance.RegisterEvent<int>(EEvent.OnRoomListSingleClose, OnRoomClosed);
            Eventer.Instance.RegisterEvent<int>(EEvent.OnRoomListSingleUpdate, OnRoomSingleUpdate);
            base.Awake();
        }


        protected override void OnDestroy()
        {
            Eventer.Instance.UnregisterEvent<int>(EEvent.OnRoomListAllUpdate, OnRoomListUpdateAll);
            Eventer.Instance.UnregisterEvent<int>(EEvent.OnRoomListSingleUpdate, OnRoomSingleUpdate);
            Eventer.Instance.UnregisterEvent<int>(EEvent.OnRoomListSingleClose, OnRoomClosed);
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

        private void OnRoomSingleUpdate(int obj)
        {
            if (m_entering)
            {
                RefreshUI();
            }
        }

        private void OnRoomListUpdateAll(int obj)
        {
            if (m_entering)
            {
                RefreshUI();
            }
        }
        private void OnRoomClosed(int obj)
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
