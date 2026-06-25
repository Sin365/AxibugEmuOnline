using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Event;
using AxibugProtobuf;
using System;
using System.Collections;
using UnityEngine;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppEmu
    {
        /// <summary>
        /// unity的c#实现有bug,以接口类型保存的monobehaviour引用,!=和==运算符没有调用到monobehaviour重写过的运算符
        /// 但是Equals方法可以,所以,这个接口判断为空请使用Equals
        /// </summary>
        private EmuCore m_emuCore;
        public int RomID { get; private set; }
        public RomPlatformType Platform { get; private set; }

        private IControllerSetuper m_controllerSetuper;

        /// <summary>
        /// unity的c#实现有bug,以接口类型保存的monobehaviour引用,!=和==运算符没有调用到monobehaviour重写过的运算符
        /// 但是Equals方法可以,所以,这个接口判断为空请使用Equals
        /// </summary>
        public EmuCore Core => m_emuCore;

        public AppEmu()
        {
            Eventer.Instance.RegisterEvent(EEvent.OnMineJoinRoom, OnSelfJoinRoom);
        }

        private void OnSelfJoinRoom()
        {
            //如果当前正在游戏中,就先结束游戏
            if (m_emuCore != null) StopGame();

            var roomInfo = App.roomMgr.mineRoomMiniInfo;
            roomInfo.FetchRomFileInRoomInfo((_, romFile) =>
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
            if (BeginGameCorout.bHadCoroutine)
                return;
            App.StartCoroutine(BeginGameAsync(romFile));
        }

        class BeginGameCorout : IDisposable
        {
            public static BeginGameCorout Acquire()
            {
                return new BeginGameCorout();
            }
            public BeginGameCorout()
            {
                m_Doing = true;
            }
            static bool m_Doing;
            public static bool bHadCoroutine { get { return m_Doing; } }
            void IDisposable.Dispose()
            {
                m_Doing = false;
            }
        }

        IEnumerator BeginGameAsync(RomFile romFile)
        {
            using (BeginGameCorout.Acquire())
            {
                yield return null;
                if (m_emuCore != null)
                {
                    yield break;
                }
                string MameObjPrefab = string.Empty;
                switch (romFile.Platform)
                {
                    case RomPlatformType.Nes:
                        MameObjPrefab = "NES/NesEmulator";
                        break;
                    case RomPlatformType.Cps1:
                    case RomPlatformType.Cps2:
                    case RomPlatformType.Igs:
                    case RomPlatformType.Neogeo:
                    case RomPlatformType.ArcadeOld:
                        MameObjPrefab = "MAME/UMAME";
                        break;
                    case RomPlatformType.MasterSystem:
                    case RomPlatformType.GameGear:
                    case RomPlatformType.GameBoy:
                    case RomPlatformType.GameBoyColor:
                    case RomPlatformType.ColecoVision:
                    case RomPlatformType.Sc3000:
                    case RomPlatformType.Sg1000:
                        MameObjPrefab = "EssgeeUnity/EssgeeUnity";
                        break;
                    case RomPlatformType.WonderSwan:
                    case RomPlatformType.WonderSwanColor:
                        MameObjPrefab = "StoicGooseUnity/StoicGooseUnity";
                        break;
                }

                MsgBool result = null;
                m_emuCore = GameObject.Instantiate(Resources.Load<GameObject>(MameObjPrefab)).GetComponent<EmuCore>();
                if (m_emuCore is UMAME)
                {
                    UMAME umame = (UMAME)m_emuCore;
                    if (!(umame.bAwakeDone))
                    {
                        App.log.Warning("模拟器核心未准备好");
                        yield break;
                    }
                    yield return new WaitForSeconds(0.1f);
                }
                try
                {
                    result = m_emuCore.StartGame(romFile);
                }
                catch (Exception ex)
                {
                    App.log.Error("启动异常中断");
                    if (m_emuCore != null)
                    {
                        GameObject.Destroy(m_emuCore.gameObject);
                    }
                    m_emuCore = null;
                    RomID = -1;
                    Platform = RomPlatformType.Invalid;

                    InGameUI.Instance.Hide();
                    LaunchUI.Instance.ShowMainMenu();
                    m_controllerSetuper = null;
                    Eventer.Instance.UnregisterEvent(EEvent.OnRoomSlotDataChanged, OnSlotDataChanged);
                    Eventer.Instance.PostEvent(EEvent.OnScreenGamepadPlatformTypeChanged);

                    yield break;
                }

                if (result)
                {
                    LaunchUI.Instance.HideMainMenu();
                    InGameUI.Instance.Show(romFile, m_emuCore);

                    CommandDispatcher.Instance.Mode = CommandListener.ScheduleType.Gaming;

                    m_controllerSetuper = m_emuCore.GetControllerSetuper();

                    //自动分配0号手柄到0号手柄位
                    m_controllerSetuper.SetConnect(con0ToSlot: 0);
                    Eventer.Instance.PostEvent(EEvent.OnControllerConnectChanged);

                    Eventer.Instance.RegisterEvent(EEvent.OnRoomSlotDataChanged, OnSlotDataChanged);
                }
                else
                {
                    StopGame();
                    OverlayManager.PopTip(result);
                }
                Eventer.Instance.PostEvent(EEvent.OnEmuBeginGame);

                RomID = romFile.ID;
                Platform = romFile.Platform;
            }
        }

        //public void BeginGame(RomFile romFile)
        //{
        //    if (m_emuCore != null) return;
        //    string MameObjPrefab = string.Empty;
        //    switch (romFile.Platform)
        //    {
        //        case RomPlatformType.Nes:
        //            MameObjPrefab = "NES/NesEmulator";
        //            break;
        //        case RomPlatformType.Cps1:
        //        case RomPlatformType.Cps2:
        //        case RomPlatformType.Igs:
        //        case RomPlatformType.Neogeo:
        //        case RomPlatformType.ArcadeOld:
        //            MameObjPrefab = "MAME/UMAME";
        //            break;
        //        case RomPlatformType.MasterSystem:
        //        case RomPlatformType.GameGear:
        //        case RomPlatformType.GameBoy:
        //        case RomPlatformType.GameBoyColor:
        //        case RomPlatformType.ColecoVision:
        //        case RomPlatformType.Sc3000:
        //        case RomPlatformType.Sg1000:
        //            MameObjPrefab = "EssgeeUnity/EssgeeUnity";
        //            break;
        //        case RomPlatformType.WonderSwan:
        //        case RomPlatformType.WonderSwanColor:
        //            MameObjPrefab = "StoicGooseUnity/StoicGooseUnity";
        //            break;
        //    }

        //    MsgBool result = null;
        //    try
        //    {
        //        m_emuCore = GameObject.Instantiate(Resources.Load<GameObject>(MameObjPrefab)).GetComponent<EmuCore>();
        //        if (m_emuCore is UMAME)
        //        {
        //            if (!((UMAME)m_emuCore).bAwakeDone)
        //            {
        //                throw new Exception("模拟器核心未准备好");
        //            }
        //        }
        //        result = m_emuCore.StartGame(romFile);
        //    }
        //    catch (Exception ex)
        //    {
        //        App.log.Error("启动异常中断");
        //        if (m_emuCore != null)
        //        {
        //            GameObject.Destroy(m_emuCore.gameObject);
        //        }
        //        m_emuCore = null;
        //        RomID = -1;
        //        Platform = RomPlatformType.Invalid;

        //        InGameUI.Instance.Hide();
        //        LaunchUI.Instance.ShowMainMenu();
        //        m_controllerSetuper = null;
        //        Eventer.Instance.UnregisterEvent(EEvent.OnRoomSlotDataChanged, OnSlotDataChanged);
        //        Eventer.Instance.PostEvent(EEvent.OnScreenGamepadPlatformTypeChanged);
        //        return;
        //    }
        //    if (result)
        //    {
        //        LaunchUI.Instance.HideMainMenu();
        //        InGameUI.Instance.Show(romFile, m_emuCore);

        //        CommandDispatcher.Instance.Mode = CommandListener.ScheduleType.Gaming;

        //        m_controllerSetuper = m_emuCore.GetControllerSetuper();

        //        //自动分配0号手柄到0号手柄位
        //        m_controllerSetuper.SetConnect(con0ToSlot: 0);
        //        Eventer.Instance.PostEvent(EEvent.OnControllerConnectChanged);

        //        Eventer.Instance.RegisterEvent(EEvent.OnRoomSlotDataChanged, OnSlotDataChanged);
        //    }
        //    else
        //    {
        //        StopGame();
        //        OverlayManager.PopTip(result);
        //    }
        //    Eventer.Instance.PostEvent(EEvent.OnEmuBeginGame);

        //    RomID = romFile.ID;
        //    Platform = romFile.Platform;
        //}

        private void OnSlotDataChanged()
        {
            long selfUID = App.user.userdata.UID;
            uint? con0Slot;
            uint? con1Slot;
            uint? con2Slot;
            uint? con3Slot;

            App.roomMgr.mineRoomMiniInfo.GetPlayerSlotIdxByUid(selfUID, 0, out con0Slot);
            App.roomMgr.mineRoomMiniInfo.GetPlayerSlotIdxByUid(selfUID, 1, out con1Slot);
            App.roomMgr.mineRoomMiniInfo.GetPlayerSlotIdxByUid(selfUID, 2, out con2Slot);
            App.roomMgr.mineRoomMiniInfo.GetPlayerSlotIdxByUid(selfUID, 3, out con3Slot);

            m_controllerSetuper.SetConnect(con0Slot, con1Slot, con2Slot, con3Slot);

            Eventer.Instance.PostEvent(EEvent.OnControllerConnectChanged);
        }

        public void StopGame()
        {
            if (m_emuCore == null) return;
            m_emuCore.Dispose();
            GameObject.Destroy(m_emuCore.gameObject);
            m_emuCore = null;
            RomID = -1;
            Platform = RomPlatformType.Invalid;

            InGameUI.Instance.Hide();
            LaunchUI.Instance.ShowMainMenu();
            m_controllerSetuper = null;
            Eventer.Instance.UnregisterEvent(EEvent.OnRoomSlotDataChanged, OnSlotDataChanged);
            Eventer.Instance.PostEvent(EEvent.OnScreenGamepadPlatformTypeChanged);
        }

        public void ResetGame()
        {
            if (m_emuCore == null) return;

            m_emuCore.DoReset();
        }
    }
}