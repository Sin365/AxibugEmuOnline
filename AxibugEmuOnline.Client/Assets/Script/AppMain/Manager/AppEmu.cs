using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Event;
using UnityEngine;
using VirtualNes.Core;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppEmu
    {
        /// <summary>
        /// unity的c#实现有bug,以接口类型保存的monobehaviour引用,!=和==运算符没有调用到monobehaviour重写过的运算符
        /// 但是Equals方法可以,所以,这个接口判断为空请使用Equals
        /// </summary>
        private IEmuCore m_emuCore;

        private IControllerSetuper m_controllerSetuper;

        /// <summary>
        /// unity的c#实现有bug,以接口类型保存的monobehaviour引用,!=和==运算符没有调用到monobehaviour重写过的运算符
        /// 但是Equals方法可以,所以,这个接口判断为空请使用Equals
        /// </summary>
        public IEmuCore Core => m_emuCore;

        public AppEmu()
        {
            Eventer.Instance.RegisterEvent(EEvent.OnMineJoinRoom, OnSelfJoinRoom);
        }

        private void OnSelfJoinRoom()
        {
            //如果当前正在游戏中,就先结束游戏
            if (!m_emuCore.IsNull()) StopGame();

            var roomInfo = App.roomMgr.mineRoomMiniInfo;
            roomInfo.FetchRomFileInRoomInfo(EnumPlatform.NES, (_, romFile) =>
            {
                if (!romFile.RomReady) //这个rom并没有下载,所以取消进入房间
                {
                    App.roomMgr.SendLeavnRoom();
                }
                else
                {
                    BeginGame(romFile);
                }
            });
        }


        public void BeginGame(RomFile romFile)
        {
            if (!m_emuCore.IsNull()) return;

            switch (romFile.Platform)
            {
                case EnumPlatform.NES:
                    m_emuCore = GameObject.Instantiate(Resources.Load<GameObject>("NES/NesEmulator")).GetComponent<IEmuCore>();
                    break;
            }

            m_emuCore.StartGame(romFile);
            LaunchUI.Instance.HideMainMenu();
            InGameUI.Instance.Show(romFile, m_emuCore);

            m_emuCore.SetupScheme();

            m_controllerSetuper = Supporter.GetControllerSetuper();
            
            SetupController();
            
            Eventer.Instance.RegisterEvent(EEvent.OnRoomSlotDataChanged,OnSlotDataChanged);
        }

        private void OnSlotDataChanged()
        {
            SetupController();
        }

        private void SetupController()
        {
            if (!App.roomMgr.InRoom) //不在房间中,自动分配0号手柄到0号手柄位
            {
                m_controllerSetuper.SetConnect(con0ToSlot: 0);
            }
            else //在房间中则使用服务器下发的手柄槽位信息分配本地手柄
            {
                long selfUID = App.user.userdata.UID;
                App.roomMgr.mineRoomMiniInfo.GetPlayerSlotIdxByUid(selfUID, 0, out var con0Slot);
                App.roomMgr.mineRoomMiniInfo.GetPlayerSlotIdxByUid(selfUID, 1, out var con1Slot);
                App.roomMgr.mineRoomMiniInfo.GetPlayerSlotIdxByUid(selfUID, 2, out var con2Slot);
                App.roomMgr.mineRoomMiniInfo.GetPlayerSlotIdxByUid(selfUID, 3, out var con3Slot);

                m_controllerSetuper.SetConnect(con0Slot, con1Slot, con2Slot, con3Slot);
            }
        }

        public void StopGame()
        {
            if (m_emuCore.IsNull()) return;
            GameObject.Destroy(m_emuCore.gameObject);
            m_emuCore = null;

            InGameUI.Instance.Hide();
            LaunchUI.Instance.ShowMainMenu();
            m_controllerSetuper = null;
            Eventer.Instance.UnregisterEvent(EEvent.OnRoomSlotDataChanged,OnSlotDataChanged);
        }

        public void ResetGame()
        {
            if (m_emuCore.IsNull()) return;

            m_emuCore.DoReset();
        }
    }
}