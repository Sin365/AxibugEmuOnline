using System.Collections.Generic;

namespace AxibugEmuOnline.Client.InputDevices
{
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
                control.Update();
                if (control.Start)
                {
                    AnyKeyDown = true;
                }
            }
        }

        /// <summary> 用于列出这个输入设备的所有输入控件实例 </summary>
        /// <returns></returns>
        protected abstract List<InputControl> DefineControls();

        /// <summary> 通过控件名称,找到对应的控件 </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public InputControl FindControlByName(string controlName)
        {
            m_controlMapper.TryGetValue(controlName, out var key);
            return key;
        }

    }
}
