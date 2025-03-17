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

            Cross = new Button(KeyCode.Joystick1Button0, this, "X");
            Circle = new Button(KeyCode.Joystick1Button1, this, "⭕");
            Square = new Button(KeyCode.Joystick1Button2, this, "□");
            Triangle = new Button(KeyCode.Joystick1Button3, this, "△");

            L = new Button(KeyCode.Joystick1Button4, this, "L");
            R = new Button(KeyCode.Joystick1Button5, this, "R");

            Select = new Button(KeyCode.Joystick1Button6, this, "SELECT");
            Start = new Button(KeyCode.Joystick1Button7, this, "START");

            Up = new Button(KeyCode.Joystick1Button8, this, "UP");
            Right = new Button(KeyCode.Joystick1Button9, this, "RIGHT");
            Down = new Button(KeyCode.Joystick1Button10, this, "DOWN");
            Left = new Button(KeyCode.Joystick1Button11, this, "LEFT");

            return result;
        }

        public class Button : InputControl
        {
            private KeyCode m_keyCode;
            private string m_controlName;

            public Button(KeyCode keycode, InputDevice device, string controlName) : base(device)
            {
                m_keyCode = keycode;
                m_controlName = controlName;
            }

            public override bool Performing => Input.GetKey(m_keyCode);

            public override Vector2 GetVector2()
            {
                return default;
            }

            public override float GetFlaot()
            {
                return Performing ? 1 : 0;
            }

            public override string ControlName => m_controlName;
        }

        public class Stick : InputControl
        {
            private bool m_left;

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

            public override bool Performing => GetVector2().x != 0 || GetVector2().y != 0;

            public override Vector2 GetVector2()
            {
                Vector2 result = Vector2.zero;

                if (m_left)
                {
                    result.x = Input.GetAxis("Horizontal");
                    result.y = Input.GetAxis("Vertical");
                }
                else
                {
                    result.x = Input.GetAxis("HorizontalR");
                    result.y = Input.GetAxis("VerticalR");
                }

                return result;
            }

            public override float GetFlaot()
            {
                return Performing ? 1 : 0;
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