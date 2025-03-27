using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.InputDevices;
using AxibugProtobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AxibugEmuOnline.Client.Settings
{
    /// <summary>
    /// 管理键位映射设置
    /// </summary>
    public class KeyMapperSetting
    {
        Dictionary<RomPlatformType, EmuCoreControllerKeyBinding> m_binders = new Dictionary<RomPlatformType, EmuCoreControllerKeyBinding>();
        Dictionary<Type, EmuCoreControllerKeyBinding> m_bindersByType = new Dictionary<Type, EmuCoreControllerKeyBinding>();
        public KeyMapperSetting()
        {
            var baseType = typeof(EmuCoreControllerKeyBinding);
            foreach (var t in baseType.Assembly.ExportedTypes)
            {
                if (t.IsAbstract) continue;
                if (!baseType.IsAssignableFrom(t)) continue;

                var binderIns = Activator.CreateInstance(t) as EmuCoreControllerKeyBinding;
                m_binders.Add(binderIns.Platform, binderIns);
                m_bindersByType.Add(binderIns.GetType(), binderIns);
            }
        }

        public T GetBinder<T>() where T : EmuCoreControllerKeyBinding
        {
            m_bindersByType.TryGetValue(typeof(T), out var binder);
            return binder as T;
        }

        public T GetBinder<T>(RomPlatformType romType) where T : EmuCoreControllerKeyBinding
        {
            m_binders.TryGetValue(romType, out var binder);
            return binder as T;
        }
    }

    /// <summary>
    /// 此类为内部继承, 请勿继承此类
    /// </summary>
    public abstract class EmuCoreControllerKeyBinding
    {
        /// <summary> 所属核心 </summary>
        public abstract RomPlatformType Platform { get; }
        /// <summary> 控制器数量 </summary>
        public abstract int ControllerCount { get; }
    }

    /// <summary>
    /// 模拟器核心控制器键位绑定器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class EmuCoreControllerKeyBinding<T> : EmuCoreControllerKeyBinding
        where T : Enum
    {
        List<BindingPage> m_bindingPages = new List<BindingPage>();

        public EmuCoreControllerKeyBinding()
        {
            for (int i = 0; i < ControllerCount; i++)
            {
                m_bindingPages.Add(new BindingPage(i, this));
            }

            var keyboard = App.input.GetDevice<Keyboard_D>();
            if (keyboard != null)
            {
                foreach (var binding in m_bindingPages)
                {
                    binding.RegistInputDevice(keyboard);
                }
            }

            var psvController = App.input.GetDevice<PSVController_D>();
            if (psvController != null)
            {
                foreach (var binding in m_bindingPages)
                {
                    binding.RegistInputDevice(psvController);
                }
            }

            App.input.OnDeviceLost += InputDevicesMgr_OnDeviceLost;
            App.input.OnDeviceConnected += InputDevicesMgr_OnDeviceConnected;
        }

        private void InputDevicesMgr_OnDeviceConnected(InputDevice_D connectDevice)
        {
            if (connectDevice is Keyboard_D)
            {
                foreach (var binding in m_bindingPages)
                {
                    binding.RegistInputDevice(connectDevice);
                }
            }
        }

        private void InputDevicesMgr_OnDeviceLost(InputDevice_D lostDevice)
        {
            foreach (var binding in m_bindingPages)
            {
                binding.UnregistInputDevice(lostDevice);
            }
            if (lostDevice is Keyboard_D) //键盘丢失,立即查找还存在的键盘并建立连接
            {
                var anotherKeyboard = App.input.GetDevice<Keyboard_D>();
                if (anotherKeyboard != null)
                {
                    foreach (var binding in m_bindingPages)
                    {
                        binding.UnregistInputDevice(lostDevice);
                    }
                }
            }
        }

        IEnumerable<T> DefineKeys()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        internal void RaiseDeviceRegist(InputDevice_D device, BindingPage binding)
        {
            OnRegistDevices(device, binding);
        }
        protected abstract void OnRegistDevices(InputDevice_D device, BindingPage binding);

        public bool Start(T emuControl, int controllerIndex)
        {
            var binding = m_bindingPages[controllerIndex];
            foreach (var key in binding.GetBinding(emuControl))
            {
                if (key.Start) return true;
            }

            return false;
        }

        public bool Release(T emuControl, int controllerIndex)
        {
            var binding = m_bindingPages[controllerIndex];
            foreach (var key in binding.GetBinding(emuControl))
            {
                if (key.Release) return true;
            }

            return false;
        }

        /// <summary>
        /// 获取指定控件是否处于按下状态
        /// <para>如果绑定了多个物理按键,则只有这多个物理按键全部不处于按下状态时,才会返回false</para>
        /// </summary>
        /// <param name="emuControl"></param>
        /// <param name="controllerIndex"></param>
        /// <returns></returns>
        public bool GetKey(T emuControl, int controllerIndex)
        {
            var binding = m_bindingPages[controllerIndex];
            foreach (var key in binding.GetBinding(emuControl))
            {
                if (key.Performing) return true;
            }

            return false;
        }

        /// <summary>
        /// 获取调用帧是否有任意按键触发了按下操作
        /// </summary>
        /// <param name="controllerIndex"></param>
        /// <returns></returns>
        public bool AnyKeyDown(int controllerIndex)
        {
            var binding = m_bindingPages[controllerIndex];
            return binding.AnyKeyDown();
        }

        /// <summary>
        /// 获取指定控件的向量值
        /// <para>通常用于摇杆类型的控件</para>
        /// <para>如果同时绑定了多个物理输入设备,只会返回其中一个物理设备的向量值</para>
        /// </summary>
        /// <param name="emuControl">模拟器平台的具体键枚举</param>
        /// <param name="controllerIndex">模拟器平台的控制器序号</param>
        /// <returns></returns>
        public Vector2 GetVector2(T emuControl, int controllerIndex)
        {
            var binding = m_bindingPages[controllerIndex];
            foreach (var control in binding.GetBinding(emuControl))
            {
                if (!control.Performing) continue;

                return control.GetVector2();
            }

            return default(Vector2);
        }

        /// <summary>
        /// 获取指定控件的浮点值,取值范围为[0f,1f]
        /// <para>通常用于线性类按键,例如PS手柄的扳机键</para>
        /// <para>普通的按键也能读取这个值,但返回值只会有0f和1f两种值</para>
        /// <para>如果同时绑定了多个物理控件,则会从所有处于按下状态的物理控件中取平均值</para>
        /// </summary>
        /// <param name="emuControl">模拟器平台的具体键枚举</param>
        /// <param name="controllerIndex">模拟器平台的控制器序号</param>
        /// <returns></returns>
        public float GetFloat(T emuControl, int controllerIndex)
        {
            var totalFloat = 0f;
            var totalControl = 0;

            var binding = m_bindingPages[controllerIndex];
            foreach (var key in binding.GetBinding(emuControl))
            {
                if (!key.Performing) continue;

                totalControl++;
                totalFloat += key.GetFlaot();
            }

            if (totalControl == 0) return default(float);
            else return totalFloat / totalControl;
        }

        public class MapSetting : Dictionary<T, List<InputControl_D>> { }

        public class BindingPage
        {
            Dictionary<Type, InputDevice_D> m_registedDevices = new Dictionary<Type, InputDevice_D>();
            Dictionary<InputDevice_D, MapSetting> m_mapSetting = new Dictionary<InputDevice_D, MapSetting>();

            public int ControllerIndex { get; }
            public EmuCoreControllerKeyBinding<T> Host { get; }

            internal BindingPage(int controllerIndex, EmuCoreControllerKeyBinding<T> host)
            {
                ControllerIndex = controllerIndex;
                Host = host;
            }

            internal bool IsRegisted<DEVICE>() where DEVICE : InputDevice_D
            {
                var type = typeof(T);
                return IsRegisted(type);
            }
            internal bool IsRegisted(Type deviceType)
            {
                return m_registedDevices.ContainsKey(deviceType);
            }

            internal void RegistInputDevice(InputDevice_D device)
            {
                var type = device.GetType();
                if (IsRegisted(type)) return;

                m_registedDevices.Add(type, device);
                m_mapSetting[device] = new MapSetting();
                Host.RaiseDeviceRegist(device, this);
            }

            internal void UnregistInputDevice(InputDevice_D device)
            {
                var type = device.GetType();
                if (!IsRegisted(type)) return;

                m_registedDevices.Remove(type);
                m_mapSetting.Remove(device);
            }

            public void SetBinding(T emuBtn, InputControl_D key, int settingSlot)
            {
                var device = key.Device;
                m_registedDevices.TryGetValue(device.GetType(), out var inputDevice);

                Debug.Assert(inputDevice == device);

                var setting = m_mapSetting[inputDevice];
                if (!setting.TryGetValue(emuBtn, out var settingList))
                {
                    settingList = new List<InputControl_D>();
                    setting[emuBtn] = settingList;
                }

                int needFixCount = settingSlot - settingList.Count + 1;
                if (needFixCount > 0) for (int i = 0; i < needFixCount; i++) settingList.Add(null);

                settingList[settingSlot] = key;
            }

            public InputControl_D GetBinding(T emuBtn, InputDevice_D device, int settingSlot)
            {
                m_mapSetting.TryGetValue(device, out var mapSetting);
                if (mapSetting == null) return null;

                mapSetting.TryGetValue(emuBtn, out var settingList);
                if (settingList == null || settingSlot >= settingList.Count) return null;

                return settingList[settingSlot];
            }

            private List<InputControl_D> m_caches = new List<InputControl_D>();
            public IEnumerable<InputControl_D> GetBinding(T emuBtn)
            {
                m_caches.Clear();

                foreach (var mapSettings in m_mapSetting.Values)
                {
                    mapSettings.TryGetValue(emuBtn, out var bindControls);
                    if (bindControls != null)
                    {
                        m_caches.AddRange(bindControls);
                    }
                }

                return m_caches;
            }

            public bool AnyKeyDown()
            {
                foreach (var mapSettings in m_mapSetting.Values)
                {
                    foreach (var keys in mapSettings.Values)
                    {
                        foreach (var key in keys)
                        {
                            if (key.Start) return true;
                        }
                    }
                }

                return false;
            }
        }

    }
}