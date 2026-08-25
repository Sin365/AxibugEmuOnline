using AxibugEmuOnline.Client.ClientCore;
using AxibugProtobuf;
using System;
using System.Collections.Generic;

namespace AxibugEmuOnline.Client
{
    public class InGameUI_RoomLock : ExpandMenu
    {
        private List<InternalOptionMenu> m_subMenus = new List<InternalOptionMenu>();
        public override string Name
        {
            get { return "房间开放状态"; }
        }
        public InGameUI_RoomLock(InGameUI inGameUI)
        { 
            foreach (RoomLockType scaleModeValue in Enum.GetValues(typeof(RoomLockType)))
            {
                m_subMenus.Add(new Scale(inGameUI, scaleModeValue));
            }
        }

        protected override List<InternalOptionMenu> GetOptionMenus()
        {
            return m_subMenus;
        }

        public class Scale : ExecuteMenu
        {
            private RoomLockType? m_mode;
            private InGameUI m_gameUI;

            public override bool IsApplied
            {
                get
                {
                    if (m_gameUI.Core == null && App.emu.RomID <= 0) return false;
                    if (m_mode == null) return false;
                    return m_mode.Value == App.roomMgr.mineRoomMiniInfo.LockState;
                }
            }
            public override string Name => ModeToName(m_mode);
            public Scale(InGameUI inGameUI, RoomLockType? mode)
            {
                m_mode = mode;
                m_gameUI = inGameUI;
            }

            public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
            {
                if (m_gameUI.Core == null || App.emu.RomID <= 0 || m_mode == null) return;
                App.roomMgr.SendRoomCloseStatusChange(m_mode.Value);
            }

            static string ModeToName(RoomLockType? mode)
            {
                switch (mode.Value)
                {
                    case RoomLockType.RoomOpening: return "开放";
                    case RoomLockType.RoomLocked: return "不开放";
                    default: throw new Exception($"Not Support Mode : {mode.Value}");
                }
            }
        }
    }
}