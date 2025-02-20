using AxibugProtobuf;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public interface IEmuCore
    {
        GameObject gameObject { get; }

        /// <summary> 获得模拟器核心中的状态快照对象 </summary>
        object GetState();
        /// <summary> 获得模拟器核心中的状态快照字节数据 </summary>
        byte[] GetStateBytes();
        /// <summary> 加载状态快照 </summary>
        /// <param name="state">该对象应该来自核心的<see cref="GetState"/>方法的返回值,或是从<see cref="GetStateBytes"/>返回的byte数组构建</param>
        void LoadState(object state);
        /// <summary> 加载状态快照 </summary>
        /// <param name="data">该对象应该来自核心的<see cref="GetStateBytes"/>返回的byte数组</param>
        void LoadStateFromBytes(byte[] data);
        /// <summary> 暂停核心推帧 </summary>
        void Pause();
        /// <summary> 恢复核心推帧(从Pause状态恢复) </summary>
        void Resume();
        /// <summary> 启动模拟器逻辑 </summary>
        MsgBool StartGame(RomFile romFile);
        /// <summary> 释放模拟器核心 </summary>
        void Dispose();
        /// <summary> 重置核心,通常由模拟器核心提供的功能 </summary>
        void DoReset();
        /// <summary> 获得模拟器核心的控制器设置器 </summary>
        /// <returns></returns>
        IControllerSetuper GetControllerSetuper();
        /// <summary> 核心所属平台 </summary>
        RomPlatformType Platform { get; }
        /// <summary> 获取当前模拟器帧序号,在加载快照和Reset后,应当重置为0 </summary>
        uint Frame { get; }
        /// <summary> 模拟器核心推帧 </summary>
        bool PushEmulatorFrame();
        /// <summary> 模拟器核心推帧结束 </summary>
        void AfterPushFrame();
        public void GetAudioParams(out int frequency, out int channels);
        Texture OutputPixel { get; }
        RawImage DrawCanvas { get; }
    }

    public static class IEnumCoreTool
    {
        public static bool IsNull(this IEmuCore core)
        {
            if (core == null) return true;
            return core.Equals(null);
        }
    }
}
