using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client.InputDevices
{
    public class PSVController : InputDevice
    {
        /// <summary> × </summary>
        public Button Cross { get; private set; }
        /// <summary> ○ </summary>
        public Button Circle { get; private set; }
        /// <summary> □ </summary>
        public Button Square { get; private set; }
        /// <summary> △ </summary>
        public Button Triangle { get; private set; }
        public Button L { get; private set; }
        public Button R { get; private set; }
        public Button Select { get; private set; }
        public Button Start { get; private set; }
        public Button Up { get; private set; }
        public Button Right { get; private set; }
        public Button Down { get; private set; }
        public Button Left { get; private set; }
        public Stick LeftStick { get; private set; }
        public Stick RightStick { get; private set; }

        public PSVController(InputResolver resolver) : base(resolver) { }

        protected override List<InputControl> DefineControls()
        {
            List<InputControl> result = new List<InputControl>();

            Cross = new Button(this, "X");
            Circle = new Button(this, "⭕");
            Square = new Button(this, "□");
            Triangle = new Button(this, "△");

            L = new Button(this, "L");
            R = new Button(this, "R");

            Select = new Button(this, "SELECT");
            Start = new Button(this, "START");

            Up = new Button(this, "UP");
            Right = new Button(this, "RIGHT");
            Down = new Button(this, "DOWN");
            Left = new Button(this, "LEFT");

            return result;
        }

        public class Button : InputControl
        {
            internal string m_controlName;

            public Button(InputDevice device, string controlName) : base(device)
            {
                m_controlName = controlName;
            }

            public override string ControlName => m_controlName;
        }

        public class Stick : InputControl
        {
            internal bool m_left;

            public VirtualButton UP { get; private set; }
            public VirtualButton Down { get; private set; }
            public VirtualButton Left { get; private set; }
            public VirtualButton Right { get; private set; }

            public Stick(InputDevice device, bool left) : base(device)
            {
                m_left = left;

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

            public override string ControlName => $"{nameof(Stick)}_{(m_left ? "left" : "right")}";

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
}