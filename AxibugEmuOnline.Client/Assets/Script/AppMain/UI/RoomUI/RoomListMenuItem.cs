using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Event;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class RoomListMenuItem : VirtualSubMenuItem
    {
        bool m_entering;

        protected override void Awake()
        {
            Eventer.Instance.RegisterEvent(EEvent.OnRoomListAllUpdate, OnRoomListUpdateAll);
            Eventer.Instance.RegisterEvent<int>(EEvent.OnRoomListSingleClose, OnRoomClosed);
            Eventer.Instance.RegisterEvent<int>(EEvent.OnRoomListSingleAdd, OnRoomSingleAdd);
            base.Awake();
        }


        protected override void OnDestroy()
        {
            Eventer.Instance.UnregisterEvent(EEvent.OnRoomListAllUpdate, OnRoomListUpdateAll);
            Eventer.Instance.UnregisterEvent<int>(EEvent.OnRoomListSingleUpdate, OnRoomSingleAdd);
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

        private void OnRoomSingleAdd(int obj)
        {
            if (m_entering)
            {
                RefreshUI();
            }
        }

        private void OnRoomListUpdateAll()
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

        protected override void GetVirtualListDatas(VirtualListDataHandle callback)
        {
            var roomList = App.roomMgr.GetRoomList();
            callback.Invoke(roomList, 0);
        }

    }
}
