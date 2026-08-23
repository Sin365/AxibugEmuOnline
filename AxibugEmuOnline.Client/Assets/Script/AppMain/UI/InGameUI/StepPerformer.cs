using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.GBA.Unity;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class StepPerformer
    {
        private readonly InGameUI m_inGameUI;
        private int m_step = -1;

        public StepPerformer(InGameUI inGameUI)
        {
            m_inGameUI = inGameUI;
        }

        public void Perform(int step)
        {
            m_step = step;

            if (App.roomMgr.SyncMode == AxibugProtobuf.SyncModeType.NormalMode)
            {
                switch (m_step)
                {
                    //等待主机上报快照
                    case 0:
                        PauseCore();
                        if (App.roomMgr.IsHost)
                        {
                            var stateRaw = m_inGameUI.Core.GetStateBytes();
                            App.log.Info($"快照上报:{Helper.FileMD5Hash(stateRaw)}");
                            App.roomMgr.SendHostRaw(stateRaw);
                        }
                        break;
                    //加载存档并发送Ready通知
                    case 1:
                        PauseCore();
                        App.log.Info($"快照加载:{Helper.FileMD5Hash(App.roomMgr.RawData)}");
                        m_inGameUI.Core.LoadStateFromBytes(App.roomMgr.RawData);
                        //TODO ready时上报性能指标
                        App.roomMgr.SendRoomPlayerReady(0, 0, 0, 0);
                        break;
                    case 2:
                        m_step = -1;
                        ResumeCore();
                        break;
                }
            }
            else if (App.roomMgr.SyncMode == AxibugProtobuf.SyncModeType.GbaLinkMode)
            {
                switch (m_step)
                {
                    //等待所有玩家上报即时存档
                    case 0:
                        PauseCore();
                        var savDataRaw = m_inGameUI.Core.GetGameSaveDataBytes();
                        App.log.Info($"上报:{Helper.FileMD5Hash(savDataRaw)}");
                        App.roomMgr.SendSavDataRaw(savDataRaw);
                        break;
                    //加载存档并发送Ready通知
                    case 1:
                        PauseCore();
                        App.log.Info($"存档加载加载");
                        if (!(m_inGameUI.Core is GBAEmulator))
                        {
                            App.log.Error("严重错误 ，不是GbaLinkMode所需的GBAEmulator核心");
                            OverlayManager.PopTip("严重错误 ，不是GbaLinkMode所需的GBAEmulator核心");
                            return;
                        }

                        ((GBAEmulator)m_inGameUI.Core).LoadGameSavDataForMulti(
                            new GBAGameStartInfo(
                                m_inGameUI.RomFile,
                            (byte)App.roomMgr.GetMinePlayerNetSlotIdx(),
                            (byte)App.roomMgr.GetRoomPlayerCount()
                            ),
                            App.roomMgr.GbaLinkModeSavDataRawList);
                        //TODO ready时上报性能指标
                        App.roomMgr.SendRoomPlayerReady(0, 0, 0, 0);
                        break;
                    case 2:
                        m_step = -1;
                        ResumeCore();
                        break;
                }
            }
            else
            {
                App.log.Error("严重错误 ，联机类型错误");
                OverlayManager.PopTip("严重错误 ，联机类型错误");
                return;
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
