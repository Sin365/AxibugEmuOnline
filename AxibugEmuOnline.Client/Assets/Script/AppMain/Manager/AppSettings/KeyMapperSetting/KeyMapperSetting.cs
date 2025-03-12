using AxibugEmuOnline.Client.InputDevices;
using AxibugProtobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AxibugEmuOnline.Client
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

            LoadDefaultMapper();
        }

        IEnumerable<T> DefineKeys()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// 加载默认映射
        /// </summary>
        public void LoadDefaultMapper()
        {
            foreach (var binding in m_bindingPages)
            {
                binding.ClearBinding();
                OnLoadDefaultMapper(binding);
            }
        }

        protected abstract void OnLoadDefaultMapper(BindingPage binding);

        /// <summary>
        /// 获取指定按键是否处于按下状态
        /// </summary>
        /// <param name="emuBtn"></param>
        /// <param name="controllerIndex"></param>
        /// <returns></returns>
        public bool GetKey(T emuBtn, int controllerIndex)
        {
            var binding = m_bindingPages[controllerIndex];
            foreach (var key in binding.GetBinding(emuBtn))
            {
                if (key.IsPressing()) return true;
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
        /// 获取指定按键的值
        /// </summary>
        /// <param name="emuBtn">模拟器平台的具体键枚举</param>
        /// <param name="controllerIndex">模拟器平台的控制器序号</param>
        /// <returns></returns>
        public Vector2 GetVector2(T emuBtn, int controllerIndex)
        {
            var binding = m_bindingPages[controllerIndex];
            foreach (var key in binding.GetBinding(emuBtn))
            {
                if (!key.IsPressing()) continue;

                return key.GetVector2();
            }

            return default(Vector2);
        }

        /// <summary>
        /// 获取指定按键的值
        /// </summary>
        /// <param name="emuBtn">模拟器平台的具体键枚举</param>
        /// <param name="controllerIndex">模拟器平台的控制器序号</param>
        /// <returns></returns>
        public float GetFloat(T emuBtn, int controllerIndex)
        {
            var binding = m_bindingPages[controllerIndex];
            foreach (var key in binding.GetBinding(emuBtn))
            {
                if (!key.IsPressing()) continue;

                return key.GetFlaot();
            }

            return default(float);
        }

        public class BindingPage
        {
            Dictionary<T, List<InputDevice.KeyBase>> m_mapSetting = new Dictionary<T, List<InputDevice.KeyBase>>();

            public int ControllerIndex { get; }
            public EmuCoreControllerKeyBinding<T> Host { get; }

            internal BindingPage(int controllerIndex, EmuCoreControllerKeyBinding<T> host)
            {
                ControllerIndex = controllerIndex;
                Host = host;

                foreach (var emuBtn in host.DefineKeys())
                    m_mapSetting[emuBtn] = new List<InputDevice.KeyBase>();
            }

            public void ClearBinding()
            {
                foreach (var list in m_mapSetting.Values) list.Clear();
            }

            public void SetBinding(T emuBtn, InputDevice.KeyBase key, int settingSlot)
            {
                var settingList = m_mapSetting[emuBtn];

                int needFixCount = settingSlot - settingList.Count + 1;
                if (needFixCount > 0) for (int i = 0; i < needFixCount; i++) settingList.Add(null);

                settingList[settingSlot] = key;
            }

            public InputDevice.KeyBase GetBinding(T emuBtn, int settingSlot)
            {
                var settingList = m_mapSetting[emuBtn];
                if (settingSlot >= settingList.Count) return null;
                return settingList[settingSlot];
            }

            public List<InputDevice.KeyBase> GetBinding(T emuBtn)
            {
                return m_mapSetting[emuBtn];
            }

            public bool AnyKeyDown()
            {
                foreach (var item in m_mapSetting)
                {
                    foreach (var key in item.Value)
                    {
                        if (key.GetButtonDown()) return true;
                    }
                }

                return false;
            }
        }
    }
}