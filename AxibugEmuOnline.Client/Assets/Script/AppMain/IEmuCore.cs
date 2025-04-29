using AxibugProtobuf;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public abstract class IEmuCore : MonoBehaviour
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
        /// <summary> 模拟器核心推帧 </summary>
        public abstract bool PushEmulatorFrame();
        /// <summary> 模拟器核心推帧结束 </summary>
        public abstract void AfterPushFrame();
        public abstract void GetAudioParams(out int frequency, out int channels);
        public abstract Texture OutputPixel { get; }
        public abstract RawImage DrawCanvas { get; }
    }
}
