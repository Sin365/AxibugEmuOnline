using UnityEngine;

namespace AxibugEmuOnline.Client.InputDevices
{
    /// <summary>
    /// 摇杆类型的输入控件,支持的返回值为Vector2
    /// </summary>
    public class Stick : InputControl
    {
        string m_controlName;
        public override string ControlName => m_controlName;

        public VirtualButton UP { get; private set; }
        public VirtualButton Down { get; private set; }
        public VirtualButton Left { get; private set; }
        public VirtualButton Right { get; private set; }

        public Stick(InputDevice device, string controlName) : base(device)
        {
            m_controlName = controlName;

            UP = new VirtualButton(device);
            Down = new VirtualButton(device);
            Left = new VirtualButton(device);
            Right = new VirtualButton(device);
        }

        protected override void OnUpdate()
        {
            var axis = GetVector2();

            UP.m_performing = axis.y > 0f;
            UP.Update();

            Down.m_performing = axis.y < 0f;
            Down.Update();

            Left.m_performing = axis.x < 0f;
            Left.Update();

            Right.m_performing = axis.x > 0f;
            Right.Update();
        }


        public class VirtualButton : InputControl
        {
            internal bool m_performing;

            public VirtualButton(InputDevice device) : base(device) { }

            public override bool Performing
            {
                get => m_performing;
            }

            public override Vector2 GetVector2()
            {
                return default;
            }

            public override float GetFlaot()
            {
                return Performing ? 1 : 0;
            }

            public override string ControlName => "VirtualStickButton";
        }
    }
}
