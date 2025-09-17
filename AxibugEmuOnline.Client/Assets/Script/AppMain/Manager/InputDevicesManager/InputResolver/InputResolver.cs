using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Event;
using AxiInputSP.UGUI;
using System;
using System.Collections.Generic;
using UnityEngine;

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

        DualWayDictionary<AxiScreenGamepad, ScreenGamepad_D> m_devices = new DualWayDictionary<AxiScreenGamepad, ScreenGamepad_D>();

        /// <summary> 禁止外部构造 </summary>
        protected InputResolver()
        {
            AxiScreenGamepad.OnGamepadActive += AxiScreenGamepad_OnGamepadActive;
            AxiScreenGamepad.OnGamepadDisactive += AxiScreenGamepad_OnGamepadDisactive;
            Eventer.Instance.RegisterEvent(EEvent.OnEmuBeginGame, OnEmuBeginGame);
            OnInit();
        }

        private void OnEmuBeginGame()
        {
            ClearLastCheckPerformingValue();
        }

        private void AxiScreenGamepad_OnGamepadDisactive(AxiScreenGamepad sender)
        {
            if (m_devices.TryGetValue(sender, out var device))
            {
                m_devices.Remove(sender);
                RaiseDeviceLost(device);
            }
        }

        private void AxiScreenGamepad_OnGamepadActive(AxiScreenGamepad sender)
        {
            var newDevice = new ScreenGamepad_D(sender, this);
            m_devices[sender] = newDevice;
            RaiseDeviceConnected(newDevice);
        }

        protected abstract void OnInit();

        List<InputDevice_D> m_devicesResultCache = new List<InputDevice_D>();
        /// <summary>
        /// 获得所有当前已连入的输入设备
        /// </summary>
        /// <returns></returns>
        public List<InputDevice_D> GetDevices()
        {
            m_devicesResultCache.Clear();
            m_devicesResultCache.AddRange(m_devices.Values);
            m_devicesResultCache.AddRange(OnGetDevices());
            return m_devicesResultCache;
        }
        /// <inheritdoc cref="GetDevices"/>
        protected abstract IEnumerable<InputDevice_D> OnGetDevices();

        /// <summary> 检查指定输入设备是否还保持着连接 </summary>
        public bool CheckOnline(InputDevice_D device)
        {
            if (device is ScreenGamepad_D)
            {
                return m_devices.TryGetKey(device as ScreenGamepad_D, out var _);
            }
            else
            {
                return OnCheckOnline(device);
            }
        }
        /// <inheritdoc cref="CheckOnline(InputDevice_D)"/>
        protected abstract bool OnCheckOnline(InputDevice_D device);

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

        long last_CheckPerformingFrameIdx = -100;
        bool last_CheckPerformingValue = false;
        void ClearLastCheckPerformingValue()
        {
            last_CheckPerformingFrameIdx = -100;
            last_CheckPerformingValue = false;
        }

        public bool CheckPerforming<CONTROLLER>(CONTROLLER control) where CONTROLLER : InputControl_C
        {
            //减少遍历开销，因为每帧200+次的调用 居然CPU占用了2~3%
            if (App.emu?.Core == null || last_CheckPerformingFrameIdx != App.emu.Core.Frame)
            {
                if (control.Device is ScreenGamepad_D)
                {
                    ScreenGamepad_D device = control.Device as ScreenGamepad_D;

                    last_CheckPerformingValue = device.CheckPerforming(control);
                }
                else last_CheckPerformingValue = OnCheckPerforming(control);

                if (App.emu?.Core != null)
                    last_CheckPerformingFrameIdx = App.emu.Core.Frame;
            }
            return last_CheckPerformingValue;


            //if (control.Device is ScreenGamepad_D)
            //{
            //    ScreenGamepad_D device = control.Device as ScreenGamepad_D;

            //    return device.CheckPerforming(control);
            //}
            //else return OnCheckPerforming(control);
        }
        protected abstract bool OnCheckPerforming<CONTROLLER>(CONTROLLER control) where CONTROLLER : InputControl_C;

        public Vector2 GetVector2<CONTROLLER>(CONTROLLER control) where CONTROLLER : InputControl_C
        {
            if (control.Device is ScreenGamepad_D)
            {
                ScreenGamepad_D device = control.Device as ScreenGamepad_D;

                return device.GetVector2(control);
            }
            return OnGetVector2(control);
        }
        protected abstract Vector2 OnGetVector2<CONTROLLER>(CONTROLLER control) where CONTROLLER : InputControl_C;

        public float GetFloat<CONTROLLER>(CONTROLLER control) where CONTROLLER : InputControl_C
        {
            return OnGetFloat(control);
        }
        protected abstract float OnGetFloat<CONTROLLER>(CONTROLLER control) where CONTROLLER : InputControl_C;

        /// <summary>
        /// 获得输入设备的唯一名称
        /// </summary>
        /// <param name="inputDevice">这个设备必须是由resolver提供,并且保持着连接</param>
        /// <returns></returns>
        public string GetDeviceName(InputDevice_D inputDevice)
        {
            if (inputDevice is ScreenGamepad_D)
            {
                m_devices.TryGetKey(inputDevice as ScreenGamepad_D, out var realDeviceScript);
                return $"{realDeviceScript.GetType().Name}_{realDeviceScript.GetHashCode()}";
            }
            else return OnGetDeviceName(inputDevice);
        }
        /// <inheritdoc cref="GetDeviceName(InputDevice_D)"/>
        protected abstract string OnGetDeviceName(InputDevice_D inputDevice);
    }
}