using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client.InputDevices
{
    public abstract class InputResolver
    {
        public static InputResolver Create()
        {
#if ENABLE_INPUT_SYSTEM //InputSystem
            return new InputSystemResolver();
#elif UNITY_PSP2 //SDK
            throw new System.NotImplementedException();
#elif UNITY_PS3 //SDK
            throw new System.NotImplementedException();
#else //使用旧Input
            throw new System.NotImplementedException();    
#endif
        }
        /// <summary> 禁止外部构造 </summary>
        protected InputResolver()
        {
            OnInit();
        }

        protected abstract void OnInit();

        /// <summary>
        /// 获得所有当前已连入的输入设备
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<InputDevice> GetDevices();

        /// <summary>
        /// 检查指定输入设备是否还保持着连接
        /// </summary>
        /// <returns></returns>
        public abstract bool CheckOnline(InputDevice device);

        /// <param name="lostDevice">丢失的设备</param>
        public delegate void OnDeviceLostHandle(InputDevice lostDevice);
        /// <summary> 当设备丢失时触发 </summary>
        public event OnDeviceLostHandle OnDeviceLost;
        protected void RaiseDeviceLost(InputDevice lostDevice)
        {
            OnDeviceLost?.Invoke(lostDevice);
        }

        /// <param name="connectDevice">建立连接的设备</param>
        public delegate void OnDeviceConnectedHandle(InputDevice connectDevice);
        /// <summary> 当设备连接时触发 </summary>
        public event OnDeviceConnectedHandle OnDeviceConnected;
        protected void RaiseDeviceConnected(InputDevice connectDevice)
        {
            OnDeviceConnected?.Invoke(connectDevice);
        }

        /// <summary>
        /// 获取一个键盘设备的指定按键当前调用帧是否触发了按下动作
        /// </summary>
        /// <param name="keyboard">键盘设备实例,来自Resolver提供的设备实例</param>
        /// <param name="key">键盘按键枚举值</param>
        /// <returns></returns>
        public abstract bool GetKeyDown(KeyBoard keyboard, KeyCode key);
        /// <summary>
        /// 获取一个键盘设备的指定按键当前调用帧是否触发了放开动作
        /// </summary>
        /// <param name="keyboard">键盘设备实例,来自Resolver提供的设备实例</param>
        /// <param name="key">键盘按键枚举值</param>
        public abstract bool GetKeyUp(KeyBoard keyboard, KeyCode key);
        /// <summary>
        /// 获取一个键盘设备的指定按键当前调用帧是否处于按下状态
        /// </summary>
        /// <param name="keyboard">键盘设备实例,来自Resolver提供的设备实例</param>
        /// <param name="key">键盘按键枚举值</param>
        public abstract bool GetKey(KeyBoard keyboard, KeyCode key);

    }
}