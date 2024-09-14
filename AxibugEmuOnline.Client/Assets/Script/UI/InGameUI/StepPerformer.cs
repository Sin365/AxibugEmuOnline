using AxibugEmuOnline.Client.ClientCore;
using System;
using VirtualNes.Core;

namespace AxibugEmuOnline.Client
{
    public class StepPerformer
    {
        private InGameUI m_inGameUI;
        private int m_step = -1;

        public StepPerformer(InGameUI inGameUI)
        {
            m_inGameUI = inGameUI;
        }

        public void Perform(int step)
        {
            m_step = step;


            switch (m_step)
            {
                //等待主机上报快照
                case 0:
                    PauseCore();
                    if (App.roomMgr.IsHost)
                    {
                        if (m_inGameUI.RomFile.Platform == EnumPlatform.NES)
                        {
                            var stateRaw = m_inGameUI.GetCore<NesEmulator>().NesCore.GetState().ToBytes();
                            App.roomMgr.SendHostRaw(stateRaw);
                        }
                    }
                    break;
                //加载存档并发送Ready通知
                case 1:
                    PauseCore();
                    var state = new State();
                    state.FromByte(App.roomMgr.RawData);
                    if (m_inGameUI.RomFile.Platform == EnumPlatform.NES)
                    {
                        m_inGameUI.GetCore<NesEmulator>().NesCore.LoadState(state);
                    }
                    App.roomMgr.SendRoomPlayerReady();
                    break;
                case 2:
                    m_step = -1;
                    ResumeCore();
                    break;
            }
        }

        private void PauseCore()
        {
            if (m_inGameUI.RomFile.Platform == EnumPlatform.NES)
            {
                m_inGameUI.GetCore<NesEmulator>().Pause();
            }
        }

        private void ResumeCore()
        {
            if (m_inGameUI.RomFile.Platform == EnumPlatform.NES)
            {
                m_inGameUI.GetCore<NesEmulator>().Resume();
            }
        }

        internal void Reset()
        {
            m_step = -1;
        }
    }
}
