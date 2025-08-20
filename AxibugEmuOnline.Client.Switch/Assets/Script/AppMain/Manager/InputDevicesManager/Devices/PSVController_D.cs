using AxibugProtobuf;

namespace AxibugEmuOnline.Client.InputDevices
{
    public class PSVController_D : InputDevice_D
    {
        public override GamePadType PadType =>  GamePadType.PsvitaControl;

        public Button_C Cross;
        public Button_C Circle;
        public Button_C Square;
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