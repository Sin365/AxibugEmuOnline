using System.Collections.Generic;

namespace AxibugEmuOnline.Client.InputDevices
{
    public class PSVController_D : InputDevice_D
    {
        /// <summary> × </summary>
        public Button_C Cross { get; private set; }
        /// <summary> ○ </summary>
        public Button_C Circle { get; private set; }
        /// <summary> □ </summary>
        public Button_C Square { get; private set; }
        /// <summary> △ </summary>
        public Button_C Triangle { get; private set; }
        public Button_C L { get; private set; }
        public Button_C R { get; private set; }
        public Button_C Select { get; private set; }
        public Button_C Start { get; private set; }
        public Button_C Up { get; private set; }
        public Button_C Right { get; private set; }
        public Button_C Down { get; private set; }
        public Button_C Left { get; private set; }
        public Stick_C LeftStick { get; private set; }
        public Stick_C RightStick { get; private set; }

        public PSVController_D(InputResolver resolver) : base(resolver) { }

        protected override List<InputControl_D> DefineControls()
        {
            List<InputControl_D> result = new List<InputControl_D>();

            Cross = new Button_C(this, "X");
            Circle = new Button_C(this, "⭕");
            Square = new Button_C(this, "□");
            Triangle = new Button_C(this, "△");

            L = new Button_C(this, "L");
            R = new Button_C(this, "R");

            Select = new Button_C(this, "SELECT");
            Start = new Button_C(this, "START");

            Up = new Button_C(this, "UP");
            Right = new Button_C(this, "RIGHT");
            Down = new Button_C(this, "DOWN");
            Left = new Button_C(this, "LEFT");

            LeftStick = new Stick_C(this, nameof(LeftStick));
            RightStick = new Stick_C(this, nameof(RightStick));

            return result;
        }
    }
}