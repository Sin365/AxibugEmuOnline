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
        /// 获得一个键盘设备
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
        public string UniqueName => m_resolver.GetDeviceName(this);

        /// <summary> 指示该设备是否在线 </summary>
        public bool Online => m_resolver.CheckOnline(this);
        /// <summary> 指示该设备当前帧是否有任意控件被激发 </summary>
        public bool AnyKeyDown { get; private set; }
        /// <summary> 获得输入解决器 </summary>
        internal InputResolver Resolver => m_resolver;

        protected Dictionary<string, InputControl> m_controlMapper = new Dictionary<string, InputControl>();
        protected InputResolver m_resolver;
        public InputDevice(InputResolver resolver)
        {
            m_resolver = resolver;

            foreach (var control in DefineControls())
            {
                m_controlMapper.Add(control.ControlName, control);
            }
        }

        public void Update()
        {
            AnyKeyDown = false;

            foreach (var control in m_controlMapper.Values)
            {
                if (control.Start)
                {
                    AnyKeyDown = true;
                }
            }
        }

        /// <summary> 用于列出这个输入设备的所有输入控件实例 </summary>
        /// <returns></returns>
        protected abstract IEnumerable<InputControl> DefineControls();

        /// <summary> 通过控件名称,找到对应的控件 </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public InputControl FindControlByName(string controlName)
        {
            m_controlMapper.TryGetValue(controlName, out var key);
            return key;
        }

        /// <summary>
        /// 输入设备的抽象控件接口
        /// </summary>
        public abstract class InputControl
        {
            /// <summary> 控件所属设备 </summary>
            public InputDevice Device { get; internal set; }

            /// <summary> 获取该控件是否在当前调用帧被激发 </summary>
            public abstract bool Start { get; }
            /// <summary> 获取该控件是否在当前调用帧被释放 </summary>
            public abstract bool Release { get; }
            /// <summary> 获取该控件是否在当前调用帧是否处于活动状态 </summary>
            public abstract bool Performing { get; }

            /// <summary> 获得该控件的以二维向量表达的值 </summary>
            /// <returns></returns>
            public abstract Vector2 GetVector2();
            /// <summary> 获得该控件的以浮点数表达的值 </summary>
            public abstract float GetFlaot();

            /// <summary> 控件名,这个控件名称必须是唯一的 </summary>
            public abstract string ControlName { get; }
            public string GetPath()
            {
                return $"{Device.UniqueName}/{ControlName}";
            }
        }

    }
}