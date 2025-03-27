using System.Collections.Generic;

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

            LeftStick = new Stick(this, nameof(LeftStick));
            RightStick = new Stick(this, nameof(RightStick));

            return result;
        }
    }
}