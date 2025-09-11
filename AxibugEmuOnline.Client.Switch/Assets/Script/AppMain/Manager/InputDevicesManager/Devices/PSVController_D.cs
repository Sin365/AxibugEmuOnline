using AxibugProtobuf;

namespace AxibugEmuOnline.Client.InputDevices
{
    public class PSVController_D : InputDevice_D
    {
        public override GamePadType PadType =>  GamePadType.PsvitaControl;

        /// <summary>
        /// X
        /// </summary>
        public Button_C Cross;
        /// <summary>
        /// O
        /// </summary>
        public Button_C Circle;
        /// <summary>
        /// 方框
        /// </summary>
        public Button_C Square;
        /// <summary>
        /// 三角
        /// </summary>
        public Button_C Triangle;
        public Button_C L;
        public Button_C R;
        public Button_C Select;
        public Button_C Start;
        public Button_C Up;
        public Button_C Right;
        public Button_C Down;
        public Button_C Left;
        public Stick_C LeftStick;
        public Stick_C RightStick;

        public PSVController_D(InputResolver resolver) : base(resolver) { }
    }
}