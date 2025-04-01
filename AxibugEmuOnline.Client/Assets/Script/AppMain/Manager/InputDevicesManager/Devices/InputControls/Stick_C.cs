using UnityEngine;

namespace AxibugEmuOnline.Client.InputDevices
{
    /// <summary>
    /// 摇杆类型的输入控件,支持的返回值为Vector2
    /// </summary>
    public class Stick_C : InputControl_C
    {
        public VirtualButton Up;
        public VirtualButton Down;
        public VirtualButton Left;
        public VirtualButton Right;

        public Stick_C(InputDevice_D device, string controlName) : base(device, controlName) { }

        protected override void OnUpdate()
        {
            var axis = GetVector2();

            Up.m_performing = axis.y > 0f;
            Up.Update();

            Down.m_performing = axis.y < 0f;
            Down.Update();

            Left.m_performing = axis.x < 0f;
            Left.Update();

            Right.m_performing = axis.x > 0f;
            Right.Update();
        }


        public class VirtualButton : InputControl_C
        {
            internal bool m_performing;

            public VirtualButton(InputDevice_D device, string controlName) : base(device, controlName) { }

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
        }
    }
}
