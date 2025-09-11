using AxibugProtobuf;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AxibugEmuOnline.Client.InputDevices
{
    public abstract class InputDevice_D
    {
        /// <summary> 指示该设备是否只能由一个Binder独占 </summary>
        public virtual bool Exclusive => true;
        public string UniqueName => m_resolver.GetDeviceName(this);
        /// <summary> 指示该设备类型，这个类型将与服务器类型对齐 </summary>
        public abstract GamePadType PadType { get; }
        /// <summary> 指示该设备是否在线 </summary>
        public bool Online => m_resolver.CheckOnline(this);
        /// <summary> 指示该设备当前帧是否有任意控件被激发 </summary>
        public bool AnyKeyDown { get; private set; }
        /// <summary> 获得输入解决器 </summary>
        internal InputResolver Resolver => m_resolver;

        protected Dictionary<string, InputControl_C> m_controlMapper = new Dictionary<string, InputControl_C>();
        protected InputResolver m_resolver;
        public InputDevice_D(InputResolver resolver)
        {
            m_resolver = resolver;
            DefineControls();
        }

        private void DefineControls()
        {
            //反射初始化所有控制对象 比如Button_C
            foreach (var field in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!typeof(InputControl_C).IsAssignableFrom(field.FieldType)) continue;

                var controlIns = Activator.CreateInstance(field.FieldType, this, field.Name) as InputControl_C;
                field.SetValue(this, controlIns);

                m_controlMapper[field.Name] = controlIns;
            }
        }

        public void Update()
        {
            AnyKeyDown = false;

            foreach (var control in m_controlMapper.Values)
            {
                control.Update();
                if (control.Start)
                {
                    AnyKeyDown = true;
                }
            }
        }

        public override string ToString()
        {
            return Resolver.GetDeviceName(this);
        }
    }
}
