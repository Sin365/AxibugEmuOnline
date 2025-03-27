using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AxibugEmuOnline.Client.InputDevices
{
    /// <summary>
    /// 输入设备的抽象控件接口
    /// </summary>
    public abstract class InputControl_C
    {
        /// <summary> 控件所属设备 </summary>
        public InputDevice_D Device { get; internal set; }

        /// <summary> 获取该控件是否在当前调用帧被激发 </summary>
        public bool Start { get; private set; }
        /// <summary> 获取该控件是否在当前调用帧被释放 </summary>
        public bool Release { get; private set; }

        bool m_performingLastFrame;
        /// <summary> 获取该控件是否在当前调用帧是否处于活动状态 </summary>
        public virtual bool Performing => Device.Resolver.CheckPerforming(this);

        /// <summary> 获得该控件的以二维向量表达的值 </summary>
        /// <returns></returns>
        public virtual Vector2 GetVector2() => Device.Resolver.GetVector2(this);
        /// <summary> 获得该控件的以浮点数表达的值 </summary>
        public virtual float GetFlaot() => Device.Resolver.GetFloat(this);

        internal void Update()
        {
            UpdateReleaseStartState();
            OnUpdate();
        }
        private void UpdateReleaseStartState()
        {
            var oldPerforming = m_performingLastFrame;
            var newPerforming = Performing;

            Start = false;
            Release = false;
            if (oldPerforming != newPerforming)
            {
                if (oldPerforming == false) Start = true;
                else Release = true;
            }
            m_performingLastFrame = Performing;
        }

        protected virtual void OnUpdate() { }

        public InputControl_C Parent { get; private set; }
        /// <summary> 控件名,这个控件名称必须是唯一的 </summary>
        public string ControlName { get; private set; }
        protected Dictionary<string, InputControl_C> m_controlMapper = new Dictionary<string, InputControl_C>();
        internal InputControl_C(InputDevice_D device, string controlName)
        {
            Device = device;
            ControlName = controlName;

            DefineControls();
        }

        private void DefineControls()
        {
            foreach (var field in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!typeof(InputControl_C).IsAssignableFrom(field.FieldType)) continue;

                var controlIns = Activator.CreateInstance(field.FieldType, this, field.Name) as InputControl_C;
                controlIns.Parent = this;
                field.SetValue(this, controlIns);

                m_controlMapper[field.Name] = controlIns;
            }
        }

        public override string ToString()
        {
            if (Parent != null)
            {
                return $"{Parent}/{ControlName}";
            }
            else
            {
                return $"{Device}/{ControlName}";
            }
        }
    }
}