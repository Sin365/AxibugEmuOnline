using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Event;
using AxibugEmuOnline.Client.Network;
using AxibugProtobuf;
using System;
using System.Collections.Generic;

namespace AxibugEmuOnline.Client
{
    public class InGameUI : CommandExecuter
    {
        private bool m_delayCreateRoom;
        private object m_state;
        private StepPerformer m_stepPerformer;

        private readonly List<InternalOptionMenu> menus = new List<InternalOptionMenu>();
        public static InGameUI Instance { get; private set; }

        public RomFile RomFile { get; private set; }

        public override bool Enable => gameObject.activeInHierarchy;

        public EmuCore Core { get; private set; }

        protected override void Awake()
        {
            Instance = this;
            gameObject.SetActiveEx(false);

            m_stepPerformer = new StepPerformer(this);

            menus.Add(new InGameUI_FilterSetting(this));
            menus.Add(new InGameUI_Scaler(this));
            menus.Add(new InGameUI_Reset(this));
            menus.Add(new InGameUI_SaveStateQuick(this));
            menus.Add(new InGameUI_LoadStateQuick(this));
            menus.Add(new InGameUI_SaveStateMenu(this));
            menus.Add(new InGameUI_QuitGame(this));

            base.Awake();
        }

        protected override void Update()
        {
            base.Update();

            PushCoreFrame();

            App.settings.Filter.ExecuteFilterRender(Core.OutputPixel, Core.DrawCanvas);
            App.settings.ScreenScaler.CalcScale(Core.DrawCanvas, Core.Platform);
        }

        void PushCoreFrame()
        {
            if (Core == null) return;
            //fluash netMsg
            NetMsg.Instance.DequeueNesMsg();

            Core.PushEmulatorFrame();
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

        public void Show(RomFile currentRom, EmuCore core)
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

            var filterSetting = App.settings.Filter.GetFilterSetting(currentRom);
            if (filterSetting.filter != null)
            {
                var filter = filterSetting.filter;
                var preset = filterSetting.preset ?? filter.DefaultPreset;

                filter.ApplyPreset(preset);
                App.settings.Filter.EnableFilter(filter);
            }
        }

        private void OnRoomCreated()
        {
            if (m_delayCreateRoom)
            {
                m_delayCreateRoom = false;
                //延迟创建房间成功后,同步本地手柄连接状态
                Dictionary<uint, ValueTuple<uint, GamePadType>> temp = new Dictionary<uint, ValueTuple<uint, GamePadType>>();
                var setuper = App.emu.Core.GetControllerSetuper();
                for (int i = 0; i < 4; i++)
                {
                    var joyIndex = setuper.GetSlotConnectingControllerIndex(i);

                    //TODO 手柄类型
                    GamePadType gpType = GamePadType.GlobalGamePad;

                    if (joyIndex != null) temp[(uint)i] = new ValueTuple<uint, GamePadType>((uint)joyIndex.Value, gpType);
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

            App.settings.Filter.ShutDownFilter();
        }

        protected override void OnCmdOptionMenu()
        {
            OverlayManager.PopSideBar(menus, 0, PopMenu_OnHide);

            if (!Core.IsNetPlay) //单人模式暂停模拟器
                Core.Pause();
        }

        //菜单关闭时候
        private void PopMenu_OnHide()
        {
            if (!Core.IsNetPlay) //单人模式恢复模拟器的暂停
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