using AxibugEmuOnline.Client.ClientCore;
using AxibugProtobuf;
using AxiReplay;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public abstract class EmuCore : MonoBehaviour
    {
        /// <summary> 获得模拟器核心中的状态快照对象 </summary>
        public abstract object GetState();
        /// <summary> 获得模拟器核心中的状态快照字节数据 </summary>
        public abstract byte[] GetStateBytes();
        /// <summary> 加载状态快照 </summary>
        /// <param name="state">该对象应该来自核心的<see cref="GetState"/>方法的返回值,或是从<see cref="GetStateBytes"/>返回的byte数组构建</param>
        public abstract void LoadState(object state);
        /// <summary> 加载状态快照 </summary>
        /// <param name="data">该对象应该来自核心的<see cref="GetStateBytes"/>返回的byte数组</param>
        public abstract void LoadStateFromBytes(byte[] data);
        /// <summary> 暂停核心推帧 </summary>
        public abstract void Pause();
        /// <summary> 恢复核心推帧(从Pause状态恢复) </summary>
        public abstract void Resume();
        /// <summary> 启动模拟器逻辑 </summary>
        public abstract MsgBool StartGame(RomFile romFile);
        /// <summary> 释放模拟器核心 </summary>
        public abstract void Dispose();
        /// <summary> 重置核心,通常由模拟器核心提供的功能 </summary>
        public abstract void DoReset();
        /// <summary> 获得模拟器核心的控制器设置器 </summary>
        /// <returns></returns>
        public abstract IControllerSetuper GetControllerSetuper();
        /// <summary> 核心所属平台 </summary>
        public abstract RomPlatformType Platform { get; }
        /// <summary> 获取当前模拟器帧序号,在加载快照和Reset后,应当重置为0 </summary>
        public abstract uint Frame { get; }

        public abstract void PushEmulatorFrame();
        /// <summary> 模拟器核心推帧结束 </summary>
        protected abstract void AfterPushFrame();
        public abstract void GetAudioParams(out int frequency, out int channels);
        public abstract Texture OutputPixel { get; }
        public abstract RawImage DrawCanvas { get; }


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
    }

    /// <typeparam name="INPUTDATA">输入数据类型</typeparam>
    public abstract class EmuCore<INPUTDATA> : EmuCore
    {
        protected virtual bool EnableRollbackNetCode => false;

        public sealed override void PushEmulatorFrame()
        {
            if (!TryPushEmulatorFrame()) return;

            if (IsNetPlay) //skip frame handle
            {
                var skipFrameCount = App.roomMgr.netReplay.GetSkipFrameCount();
                if (skipFrameCount > 0) App.log.Debug($"SKIP FRAME : {skipFrameCount} ,CF:{App.roomMgr.netReplay.mCurrClientFrameIdx},RFIdx:{App.roomMgr.netReplay.mRemoteFrameIdx},RForward:{App.roomMgr.netReplay.mRemoteForwardCount} ,queue:{App.roomMgr.netReplay.mNetReplayQueue.Count}");
                for (var i = 0; i < skipFrameCount; i++)
                {
                    if (!TryPushEmulatorFrame()) break;
                }
            }

            AfterPushFrame();
        }

        bool TryPushEmulatorFrame()
        {
            if (SampleInputData(out var inputData))
            {
                if (IsNetPlay) SendLocalInput();

                return OnPushEmulatorFrame(inputData);
            }

            return false;
        }

        private void SendLocalInput()
        {
            var localState = GetLocalInput();
            var rawData = InputDataToNet(localState);
            App.roomMgr.SendRoomSingelPlayerInput(Frame, rawData);

            if (m_lastTestInput != rawData)
            {
                m_lastTestInput = rawData;
                App.log.Debug($"{DateTime.Now.ToString("hh:mm:ss.fff")} Input F:{App.roomMgr.netReplay.mCurrClientFrameIdx} | I:{rawData}");
            }
        }

        ulong m_lastTestInput;
        ReplayStep m_replayData;
        int m_frameDiff;
        bool m_inputDiff;
        protected bool SampleInputData(out INPUTDATA inputData)
        {
            bool result = false;
            inputData = default(INPUTDATA);

            if (IsNetPlay)
            {
                if (App.roomMgr.netReplay.TryGetNextFrame((int)Frame, EnableRollbackNetCode ? true : false, out m_replayData, out m_frameDiff, out m_inputDiff))
                {
                    if (m_inputDiff)
                    {
                        App.log.Debug($"{DateTime.Now.ToString("hh:mm:ss.fff")} TryGetNextFrame remoteFrame->{App.roomMgr.netReplay.mRemoteFrameIdx} diff->{m_frameDiff} " +
                            $"frame=>{m_replayData.FrameStartID} InPut=>{m_replayData.InPut}");
                    }

                    inputData = ConvertInputDataFromNet(m_replayData);
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            else//单机模式
            {
                inputData = GetLocalInput();
                result = true;
            }

            return result;
        }

        protected abstract INPUTDATA GetLocalInput();
        protected abstract INPUTDATA ConvertInputDataFromNet(ReplayStep step);
        protected abstract ulong InputDataToNet(INPUTDATA inputData);
        /// <summary> 模拟器核心推帧 </summary>
        protected abstract bool OnPushEmulatorFrame(INPUTDATA InputData);
    }
}
