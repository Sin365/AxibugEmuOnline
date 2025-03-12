using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client.InputDevices
{
    public class InputDevicesManager
    {
        Dictionary<string, InputDevice> m_devices = new Dictionary<string, InputDevice>();

        public InputDevicesManager()
        {
            AddDevice(new KeyBoard());
        }

        public void AddDevice(InputDevice device)
        {
            m_devices[device.UniqueName] = device;
        }

        public void RemoveDevice(InputDevice device)
        {
            m_devices.Remove(device.UniqueName);
        }

        public InputDevice.KeyBase GetKeyByPath(string path)
        {
            var temp = path.Split("/");
            Debug.Assert(temp.Length == 2, "Invalid Path Format");

            var deviceName = temp[0];
            var keyName = temp[1];

            var targetDevice = FindDeviceByName(deviceName);
            if (targetDevice == null) return null;

            var key = targetDevice.FindKeyByKeyName(keyName);
            return key;
        }

        public InputDevice FindDeviceByName(string deviceName)
        {
            m_devices.TryGetValue(deviceName, out var device);
            return device;
        }

        /// <summary>
        /// 获得键盘设备
        /// <para>键盘设备被设计为有且仅有一个,所以这里应该总是能获得键盘设备</para>
        /// </summary>
        public KeyBoard GetKeyboard()
        {
            foreach (var d in m_devices.Values)
            {
                if (d is KeyBoard kb) return kb;
            }

            return null;
        }

        /// <summary> 由外部驱动的逻辑更新入口 </summary>
        public void Update()
        {
            foreach (var device in m_devices.Values) device.Update();
        }
    }

    public abstract class InputDevice
    {
        public abstract string UniqueName { get; }

        public delegate void OnKeyStateChangedHandle(KeyBase sender);
        public event OnKeyStateChangedHandle OnKeyStateChanged;

        /// <summary> 指示该设备是否在线 </summary>
        public abstract bool Online { get; }
        /// <summary> 指示该设备当前帧是否有任意键被按下 </summary>
        public bool AnyKeyDown { get; private set; }

        protected Dictionary<string, KeyBase> m_keyMapper = new Dictionary<string, KeyBase>();

        public InputDevice()
        {
            foreach (var key in DefineKeys())
            {
                m_keyMapper[key.KeyName] = key;
            }
        }

        public void Update()
        {
            AnyKeyDown = false;

            foreach (var key in m_keyMapper.Values)
            {
                if (key.GetButtonDown())
                {
                    AnyKeyDown = true;
                    RaiseKeyEvent(key);
                }
            }
        }

        protected abstract IEnumerable<KeyBase> DefineKeys();
        protected void RaiseKeyEvent(KeyBase sender)
        {
            OnKeyStateChanged?.Invoke(sender);
        }

        public KeyBase FindKeyByKeyName(string keyName)
        {
            m_keyMapper.TryGetValue(keyName, out var key);
            return key;
        }

        /// <summary>
        /// 输入设备的键接口
        /// </summary>
        public abstract class KeyBase
        {
            public InputDevice HostDevice { get; internal set; }
            /// <summary> 获取该键是否在当前调用帧被按下 </summary>
            public abstract bool GetButtonDown();
            /// <summary> 获取该键是否在当前调用帧被抬起 </summary>
            public abstract bool GetButtonUp();
            /// <summary> 获取该键是否在当前调用帧是否处于按下状态 </summary>
            public abstract bool IsPressing();

            public virtual Vector2 GetVector2() { throw new System.NotImplementedException(); }
            public virtual float GetFlaot() { throw new System.NotImplementedException(); }

            /// <summary> 键名 </summary>
            public abstract string KeyName { get; }
            public string GetPath()
            {
                return $"{HostDevice.UniqueName}/{KeyName}";
            }
        }

    }
}