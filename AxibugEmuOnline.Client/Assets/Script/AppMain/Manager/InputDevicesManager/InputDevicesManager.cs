using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client.InputDevices
{
    public class InputDevicesManager
    {
        InputResolver m_inputResolver = InputResolver.Create();
        Dictionary<string, InputDevice> m_devices = new Dictionary<string, InputDevice>();

        public delegate void OnDeviceConnectedHandle(InputDevice connectDevice);
        public event OnDeviceConnectedHandle OnDeviceConnected;
        public delegate void OnDeviceLostHandle(InputDevice lostDevice);
        public event OnDeviceLostHandle OnDeviceLost;

        public InputDevicesManager()
        {
            m_inputResolver.OnDeviceConnected += Resolver_OnDeviceConnected;
            m_inputResolver.OnDeviceLost += Resolver_OnDeviceLost;
            foreach (var device in m_inputResolver.GetDevices())
                AddDevice(device);
        }

        private void Resolver_OnDeviceLost(InputDevice lostDevice)
        {
            RemoveDevice(lostDevice);
        }

        private void Resolver_OnDeviceConnected(InputDevice connectDevice)
        {
            AddDevice(connectDevice);
        }

        void AddDevice(InputDevice device)
        {
            m_devices[device.UniqueName] = device;
            OnDeviceConnected?.Invoke(device);
        }

        void RemoveDevice(InputDevice device)
        {
            m_devices.Remove(device.UniqueName);
            OnDeviceLost?.Invoke(device);
        }

        /// <summary>
        /// 获得一个指定类型的设备
        /// </summary>
        public T GetDevice<T>() where T : InputDevice
        {
            foreach (var d in m_devices.Values)
            {
                if (d is T) return d as T;
            }

            return null;
        }

        /// <summary> 由外部驱动的逻辑更新入口 </summary>
        public void Update()
        {
            foreach (var device in m_devices.Values) device.Update();
        }
    }
}