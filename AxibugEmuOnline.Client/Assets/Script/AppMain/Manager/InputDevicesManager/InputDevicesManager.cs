using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AxibugEmuOnline.Client.InputDevices
{
    public class InputDevicesManager
    {
        InputResolver m_inputResolver = InputResolver.Create();
        Dictionary<string, InputDevice_D> m_devices = new Dictionary<string, InputDevice_D>();

        public delegate void OnDeviceConnectedHandle(InputDevice_D connectDevice);
        public event OnDeviceConnectedHandle OnDeviceConnected;
        public delegate void OnDeviceLostHandle(InputDevice_D lostDevice);
        public event OnDeviceLostHandle OnDeviceLost;

        bool m_quiting;
        public InputDevicesManager()
        {
            Application.quitting += Application_quitting;
            m_inputResolver.OnDeviceConnected += Resolver_OnDeviceConnected;
            m_inputResolver.OnDeviceLost += Resolver_OnDeviceLost;
            foreach (var device in m_inputResolver.GetDevices())
                AddDevice(device);
        }

        private void Application_quitting()
        {
            m_quiting = true;
        }

        private void Resolver_OnDeviceLost(InputDevice_D lostDevice)
        {
            RemoveDevice(lostDevice);
        }

        private void Resolver_OnDeviceConnected(InputDevice_D connectDevice)
        {
            AddDevice(connectDevice);
        }

        void AddDevice(InputDevice_D device)
        {
            m_devices[device.UniqueName] = device;
            OnDeviceConnected?.Invoke(device);
        }

        void RemoveDevice(InputDevice_D device)
        {
            if (m_quiting) return;

            m_devices.Remove(device.UniqueName);
            OnDeviceLost?.Invoke(device);
        }

        /// <summary>
        /// 获得一个指定类型的设备
        /// </summary>
        public T GetDevice<T>() where T : InputDevice_D
        {
            foreach (var d in m_devices.Values)
            {
                if (d is T) return d as T;
            }

            return null;
        }
        /// <summary>
        /// 获得所有存在的输入设备
        /// </summary>
        /// <returns></returns>
        public IEnumerable<InputDevice_D> GetDevices()
        {
            return m_devices.Values;
        }

        /// <summary> 由外部驱动的逻辑更新入口 </summary>
        public void Update()
        {
            foreach (var device in m_devices.Values) device.Update();

            DebugInputSystem();
        }

        private static void DebugInputSystem()
        {
            return;
#if ENABLE_INPUT_SYSTEM
            foreach (var device in InputSystem.devices)
            {
                if (device is Mouse)
                    continue;
                for (int i = 0; i < device.allControls.Count; i++)
                {
                    if (device.allControls[i].IsPressed(0))
                    {
                        Debug.Log($"{device.displayName}| {device.allControls[i].displayName}|{device.allControls[i].name}");
                    }
                }
            }
#endif
        }
    }
}