using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Event;
using AxibugEmuOnline.Client.Manager;
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

            Eventer.Instance.RegisterEvent<int>(EEvent.OnRoomListSingleUpdate, OnRoomSignelUpdate);
        }

        private void OnRoomSignelUpdate(int roomID)
        {
            if (this.roomID != roomID) return;

            if (App.roomMgr.GetRoomListMiniInfo(roomID, out var roomInfo))
                UpdateUI(roomInfo);
        }

        public void SetData(object data)
        {
            var roomInfo = data as Protobuf_Room_MiniInfo;
            roomID = roomInfo.RoomID;

            UpdateUI(roomInfo);
        }

        public override bool OnEnterItem()
        {
            if (m_romFile == null) return false;

            if (!m_romFile.RomReady)
            {
                m_romFile.BeginDownload();
                return false;
            }
            else
            {
                if (!App.roomMgr.GetRoomListMiniInfo(roomID, out Protobuf_Room_MiniInfo MiniInfo))
                {
                    OverlayManager.PopTip("房间不存在");
                    return false;
                }

                int[] freeSlots = null;
                if (!MiniInfo.GetFreeSlot(out freeSlots))
                {
                    OverlayManager.PopTip("无空闲位置");
                    return false;
                }

                App.roomMgr.SendJoinRoom(roomID, freeSlots[0]);
                return true;
            }
        }

        private void UpdateUI(Protobuf_Room_MiniInfo roomInfo)
        {
            var hostNick = roomInfo.GetHostNickName();
            roomInfo.GetRoomPlayers(out var cur, out var max);
            SetBaseInfo("--", $"<b>{hostNick}</b>的房间", $"{cur}/{max}");
            SetIcon(null);

            roomInfo.FetchRomFileInRoomInfo(EnumPlatform.NES, (room, romFile) =>
            {
                if (room.RoomID != roomID) return;

                m_romFile = romFile;
                Txt.text = romFile.Alias;

                UpdateRomInfoView();
                App.CacheMgr.GetSpriteCache(romFile.ImageURL, OnGetRomImage);
            });
        }

        protected override void Update()
        {
            UpdateRomInfoView();
            base.Update();
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
