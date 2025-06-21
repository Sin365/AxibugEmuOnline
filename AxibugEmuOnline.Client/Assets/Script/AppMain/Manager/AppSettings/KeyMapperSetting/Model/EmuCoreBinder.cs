using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.InputDevices;
using AxibugEmuOnline.Client.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 模拟器核心控制器键位绑定器
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class EmuCoreBinder<T> : InternalEmuCoreBinder,
    IDeviceBinder<T, Keyboard_D>,
    IDeviceBinder<T, GamePad_D>,
    IDeviceBinder<T, DualShockController_D>,
    IDeviceBinder<T, XboxController_D>,
    IDeviceBinder<T, PSVController_D>,
    IDeviceBinder<T, ScreenGamepad_D>
    where T : Enum
{
    //每一个实例代表一个对应模拟器平台的控制器索引
    List<ControllerBinder> m_controllerBinders = new List<ControllerBinder>();

    public EmuCoreBinder()
    {
        var types = GetType().GetInterfaces();

        for (int i = 0; i < ControllerCount; i++)
        {
            m_controllerBinders.Add(new ControllerBinder(i, this));
        }

        foreach (var device in App.input.GetDevices())
        {
            foreach (var binding in m_controllerBinders)
            {
                if (device.Exclusive && GetRegistedBinder(device) != null) continue;

                binding.RegistInputDevice(device);
            }
        }

        App.input.OnDeviceLost += InputDevicesMgr_OnDeviceLost;
        App.input.OnDeviceConnected += InputDevicesMgr_OnDeviceConnected;
    }

    /// <summary> 获取指定设备的注册Binder对象 </summary>
    /// <param name="device"></param>
    /// <returns>返回首个匹配对象</returns>
    ControllerBinder GetRegistedBinder(InputDevice_D device)
    {
        foreach (var binding in m_controllerBinders)
        {
            if (binding.IsRegisted(device)) return binding;
        }

        return null;
    }

    private void InputDevicesMgr_OnDeviceConnected(InputDevice_D connectDevice)
    {
        foreach (var binding in m_controllerBinders)
        {
            if (connectDevice.Exclusive && GetRegistedBinder(connectDevice) != null) continue;
            binding.RegistInputDevice(connectDevice);
        }
    }

    private void InputDevicesMgr_OnDeviceLost(InputDevice_D lostDevice)
    {
        foreach (var binding in m_controllerBinders)
        {
            binding.UnregistInputDevice(lostDevice);
        }
    }

    internal void RaiseDeviceRegist(InputDevice_D device, ControllerBinder binding)
    {
        if (device is Keyboard_D keyboard) Bind(keyboard, binding);
        else if (device is GamePad_D gamePad) Bind(gamePad, binding);
        else if (device is DualShockController_D dsC) Bind(dsC, binding);
        else if (device is XboxController_D xbC) Bind(xbC, binding);
        else if (device is PSVController_D psvC) Bind(psvC, binding);
        else if (device is ScreenGamepad_D screenGamepad) Bind(screenGamepad, binding);
        else throw new NotImplementedException($"{device.GetType()}");
    }

    public bool Start(T emuControl, int controllerIndex)
    {
        var binding = m_controllerBinders[controllerIndex];
        foreach (var key in binding.GetBinding(emuControl))
        {
            if (key.Start) return true;
        }

        return false;
    }

    public bool Release(T emuControl, int controllerIndex)
    {
        var binding = m_controllerBinders[controllerIndex];
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
        var binding = m_controllerBinders[controllerIndex];
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
        var binding = m_controllerBinders[controllerIndex];
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
        var binding = m_controllerBinders[controllerIndex];
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

        var binding = m_controllerBinders[controllerIndex];
        foreach (var key in binding.GetBinding(emuControl))
        {
            if (!key.Performing) continue;

            totalControl++;
            totalFloat += key.GetFlaot();
        }

        if (totalControl == 0) return default(float);
        else return totalFloat / totalControl;
    }

    public class MapSetting : Dictionary<T, List<InputControl_C>> { }

    public class ControllerBinder
    {
        Dictionary<Type, InputDevice_D> m_registedDevices = new Dictionary<Type, InputDevice_D>();
        Dictionary<InputDevice_D, MapSetting> m_mapSetting = new Dictionary<InputDevice_D, MapSetting>();

        public int ControllerIndex { get; }
        public EmuCoreBinder<T> Host { get; }

        internal ControllerBinder(int controllerIndex, EmuCoreBinder<T> host)
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
        internal bool IsRegisted(InputDevice_D device)
        {
            return m_registedDevices.Values.Contains(device);
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

        public void SetBinding(T emuBtn, InputControl_C key, int settingSlot)
        {
            var device = key.Device;
            m_registedDevices.TryGetValue(device.GetType(), out var inputDevice);

            Debug.Assert(inputDevice == device);

            var setting = m_mapSetting[inputDevice];
            if (!setting.TryGetValue(emuBtn, out var settingList))
            {
                settingList = new List<InputControl_C>();
                setting[emuBtn] = settingList;
            }

            int needFixCount = settingSlot - settingList.Count + 1;
            if (needFixCount > 0) for (int i = 0; i < needFixCount; i++) settingList.Add(null);

            settingList[settingSlot] = key;
        }

        public InputControl_C GetBinding(T emuBtn, InputDevice_D device, int settingSlot)
        {
            m_mapSetting.TryGetValue(device, out var mapSetting);
            if (mapSetting == null) return null;

            mapSetting.TryGetValue(emuBtn, out var settingList);
            if (settingList == null || settingSlot >= settingList.Count) return null;

            return settingList[settingSlot];
        }

        private List<InputControl_C> m_caches = new List<InputControl_C>();
        public IEnumerable<InputControl_C> GetBinding(T emuBtn)
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

    public abstract void Bind(Keyboard_D device, ControllerBinder controller);
    public abstract void Bind(GamePad_D device, ControllerBinder controller);
    public abstract void Bind(DualShockController_D device, ControllerBinder controller);
    public abstract void Bind(XboxController_D device, ControllerBinder controller);
    public abstract void Bind(PSVController_D device, ControllerBinder controller);
    public abstract void Bind(ScreenGamepad_D device, ControllerBinder controller);
}