using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AxibugEmuOnline.Client.InputDevices
{
    public abstract class InputResolver
    {
        public static InputResolver Create()
        {
#if ENABLE_INPUT_SYSTEM //InputSystem
            return new ForInputSystem.InputSystemResolver();
#elif UNITY_PSP2 //特化实现
            return new ForPSV.PSVResolver();
#elif UNITY_PS3 //SDK
            throw new System.NotImplementedException();
#else 
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
        public abstract IEnumerable<InputDevice_D> GetDevices();

        /// <summary>
        /// 检查指定输入设备是否还保持着连接
        /// </summary>
        /// <returns></returns>
        public abstract bool CheckOnline(InputDevice_D device);

        /// <param name="lostDevice">丢失的设备</param>
        public delegate void OnDeviceLostHandle(InputDevice_D lostDevice);
        /// <summary> 当设备丢失时触发 </summary>
        public event OnDeviceLostHandle OnDeviceLost;
        protected void RaiseDeviceLost(InputDevice_D lostDevice)
        {
            OnDeviceLost?.Invoke(lostDevice);
        }

        /// <param name="connectDevice">建立连接的设备</param>
        public delegate void OnDeviceConnectedHandle(InputDevice_D connectDevice);
        /// <summary> 当设备连接时触发 </summary>
        public event OnDeviceConnectedHandle OnDeviceConnected;
        protected void RaiseDeviceConnected(InputDevice_D connectDevice)
        {
            OnDeviceConnected?.Invoke(connectDevice);
        }

        public abstract bool CheckPerforming<CONTROLLER>(CONTROLLER control) where CONTROLLER : InputControl_C;
        public abstract Vector2 GetVector2<CONTROLLER>(CONTROLLER control) where CONTROLLER : InputControl_C;
        public abstract float GetFloat<CONTROLLER>(CONTROLLER control) where CONTROLLER : InputControl_C;
        /// <summary>
        /// 获得输入设备的唯一名称
        /// </summary>
        /// <param name="inputDevice">这个设备必须是由resolver提供,并且保持着连接</param>
        /// <returns></returns>
        public abstract string GetDeviceName(InputDevice_D inputDevice);
    }
}