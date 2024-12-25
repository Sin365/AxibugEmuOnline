using System.Collections.Generic;
using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Event;
using AxibugProtobuf;
using VirtualNes.Core;

namespace AxibugEmuOnline.Client
{
    public class InGameUI : CommandExecuter
    {
        private bool m_delayCreateRoom;
        private object m_state;
        private StepPerformer m_stepPerformer;

        private readonly List<OptionMenu> menus = new List<OptionMenu>();
        public static InGameUI Instance { get; private set; }

        public RomFile RomFile { get; private set; }

        public override bool Enable => gameObject.activeInHierarchy;

        /// <summary> 指示该游戏实例是否处于联机模式 </summary>
        public bool IsNetPlay
        {
            get
            {
                if (!App.user.IsLoggedIn) return false;
                if (App.roomMgr.mineRoomMiniInfo == null) return false;
                if (App.roomMgr.RoomState <= RoomGameState.OnlyHost) return false;

                return true;
            }
        }

        public IEmuCore Core { get; private set; }

        protected override void Awake()
        {
            Instance = this;
            gameObject.SetActiveEx(false);

            m_stepPerformer = new StepPerformer(this);

            menus.Add(new InGameUI_FilterSetting(this));
            menus.Add(new InGameUI_Reset(this));
            menus.Add(new InGameUI_SaveState(this));
            menus.Add(new InGameUI_LoadState(this));
            menus.Add(new InGameUI_QuitGame(this));

            base.Awake();
        }

        protected override void OnDestroy()
        {
            Instance = null;
            base.OnDestroy();
        }

        /// <summary> 保存快速快照 </summary>
        public void SaveQuickState(object state)
        {
            m_state = state;
        }

        /// <summary>
        ///     读取快速快照
        /// </summary>
        public object GetQuickState()
        {
            return m_state;
        }

        public void Show(RomFile currentRom, IEmuCore core)
        {
            m_delayCreateRoom = false;
            m_state = null; //清空游戏快照
            CommandDispatcher.Instance.RegistController(this);

            RomFile = currentRom;
            Core = core;
            m_stepPerformer.Reset();

            if (!App.roomMgr.InRoom)
            {
                if (App.user.IsLoggedIn)
                {
                    App.roomMgr.SendCreateRoom(RomFile.ID, RomFile.Hash);
                }
                else
                {
                    m_delayCreateRoom = true;
                    OverlayManager.PopTip("稍后将会建立房间");
                }
            }

            Eventer.Instance.RegisterEvent(EEvent.OnLoginSucceed, OnLoggedIn);
            Eventer.Instance.RegisterEvent<int>(EEvent.OnRoomWaitStepChange, OnServerStepUpdate);
            Eventer.Instance.RegisterEvent(EEvent.OnMineRoomCreated, OnRoomCreated);

            gameObject.SetActiveEx(true);

            var filterSetting = App.filter.GetFilterSetting(currentRom);
            if (filterSetting.filter != null)
            {
                var filter = filterSetting.filter;
                var preset = filterSetting.preset ?? filter.DefaultPreset;

                filter.ApplyPreset(preset);
                App.filter.EnableFilter(filter);
            }
        }

        private void OnRoomCreated()
        {
            if (m_delayCreateRoom)
            {
                m_delayCreateRoom = false;
                //延迟创建房间成功后,同步本地手柄连接状态
                Dictionary<uint, uint> temp = new Dictionary<uint, uint>();
                var setuper = Supporter.GetControllerSetuper();
                for (int i = 0; i < 4; i++)
                {
                    var joyIndex = setuper.GetSlotConnectingController(i);
                    if (joyIndex != null) temp[(uint)i] = (uint)joyIndex.Value;
                }
                App.roomMgr.SendChangePlaySlotIdxWithJoyIdx(temp);
            }
        }

        private void OnLoggedIn()
        {
            if (m_delayCreateRoom) App.roomMgr.SendCreateRoom(RomFile.ID, RomFile.Hash);
        }

        private void OnServerStepUpdate(int step)
        {
            m_stepPerformer.Perform(step);
        }

        public void Hide()
        {
            CommandDispatcher.Instance.UnRegistController(this);
            gameObject.SetActiveEx(false);

            App.filter.ShutDownFilter();
        }

        protected override void OnCmdOptionMenu()
        {
            OverlayManager.PopSideBar(menus, 0, PopMenu_OnHide);

            if (!IsNetPlay) //单人模式暂停模拟器
                Core.Pause();
        }

        //菜单关闭时候
        private void PopMenu_OnHide()
        {
            if (!IsNetPlay) //单人模式恢复模拟器的暂停
                Core.Resume();
        }

        public void QuitGame()
        {
            Eventer.Instance.UnregisterEvent<int>(EEvent.OnRoomWaitStepChange, OnServerStepUpdate);
            Eventer.Instance.UnregisterEvent(EEvent.OnLoginSucceed, OnLoggedIn);
            Eventer.Instance.UnregisterEvent(EEvent.OnMineRoomCreated, OnRoomCreated);
            App.roomMgr.SendLeavnRoom();
            App.emu.StopGame();
        }
    }
}