using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using UnityEngine;

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
                        var stateRaw = m_inGameUI.Core.GetStateBytes();
                        Debug.Log($"快照上报:{Helper.FileMD5Hash(stateRaw)}");
                        App.roomMgr.SendHostRaw(stateRaw);
                    }
                    break;
                //加载存档并发送Ready通知
                case 1:
                    PauseCore();
                    Debug.Log($"快照加载:{Helper.FileMD5Hash(App.roomMgr.RawData)}");
                    m_inGameUI.Core.LoadStateFromBytes(App.roomMgr.RawData);
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
            m_inGameUI.Core.Pause();
        }

        private void ResumeCore()
        {
            m_inGameUI.Core.Resume();
        }

        internal void Reset()
        {
            m_step = -1;
        }
    }
}
