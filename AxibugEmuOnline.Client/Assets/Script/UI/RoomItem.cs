using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Event;
using AxibugEmuOnline.Client.UI;
using AxibugProtobuf;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public class RoomItem : MenuItem, IVirtualItem
    {
        [SerializeField]
        Image m_roomPreview;
        [SerializeField]
        Slider m_downloadProgress;
        [SerializeField]
        GameObject m_downloadingFlag;
        [SerializeField]
        GameObject m_romReadyFlag;

        private RomFile m_romFile;

        public int Index { get; set; }
        public int roomID { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            Eventer.Instance.RegisterEvent<int>(EEvent.OnRoomListSingleUpdate, OnRoomSingleUpdate);
        }

        protected override void OnDestroy()
        {
            Eventer.Instance.UnregisterEvent<int>(EEvent.OnRoomListSingleUpdate, OnRoomSingleUpdate);
        }

        private void OnRoomSingleUpdate(int roomId)
        {
            if (roomId != roomID) return;

            if (App.roomMgr.GetRoomListMiniInfo(roomId, out var roomInfo))
            {
                UpdateUI(roomInfo);
            }
        }

        public void SetData(object data)
        {
            var roomInfo = data as Protobuf_Room_MiniInfo;
            roomID = roomInfo.RoomID;

            UpdateUI(roomInfo);
        }

        private void UpdateUI(Protobuf_Room_MiniInfo roomInfo)
        {
            var hostNick = roomInfo.GetHostNickName();
            roomInfo.GetRoomPlayers(out var cur, out var max);
            SetBaseInfo(string.Empty, $"<b>{hostNick}</b>�ķ��� - {cur}/{max}");
            SetIcon(null);

            roomInfo.FetchRomFileInRoomInfo(EnumPlatform.NES, (romFile) =>
            {
                m_romFile = romFile;

                if (romFile.ID == roomInfo.GameRomID)
                    Txt.text = romFile.Alias;

                UpdateRomInfoView();
                App.CacheMgr.GetSpriteCache(romFile.ImageURL, OnGetRomImage);
            });
        }

        private void Update()
        {
            UpdateRomInfoView();
        }

        private void UpdateRomInfoView()
        {
            float? downloadingProgress = null;
            bool romReady = false;

            if (m_romFile != null)
            {
                if (m_romFile.IsDownloading)
                    downloadingProgress = m_romFile.Progress;
                if (m_romFile.RomReady)
                    romReady = true;
            }

            m_downloadingFlag.SetActiveEx(downloadingProgress.HasValue);
            if (downloadingProgress.HasValue)
                m_downloadProgress.value = downloadingProgress.Value;

            m_romReadyFlag.SetActiveEx(romReady);
        }

        private void OnGetRomImage(Sprite sprite, string url)
        {
            if (m_romFile == null) return;
            if (m_romFile.ImageURL != url) return;

            SetIcon(sprite);
        }

        public void SetDependencyProperty(object data)
        {
            SetSelectState(data is ThirdMenuRoot tr && tr.SelectIndex == Index);
        }

        public void Release()
        {
            Reset();
        }
    }
}
