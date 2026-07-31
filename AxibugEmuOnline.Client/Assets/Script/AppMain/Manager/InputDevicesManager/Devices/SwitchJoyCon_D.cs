using AxibugProtobuf;

namespace AxibugEmuOnline.Client.InputDevices
{
    public class SwitchJoyCon_D : InputDevice_D
    {
        public Button_C leftTrigger;
        public Button_C leftShoulder;
        public Button_C rightTrigger;
        public Button_C rightShoulder;

        public Button_C B;
        public Button_C A;
        public Button_C Y;
        public Button_C X;

        public Button_C Up;
        public Button_C Down;
        public Button_C Left;
        public Button_C Right;

        public Button_C Minus;
        public Button_C Plus;

        public Stick_C LeftStick;
        public Stick_C RightStick;
        public Button_C LeftStickPress;
        public Button_C RightStickPress;

        public SwitchJoyCon_D(InputResolver resolver) : base(resolver) { }

        public override GamePadType PadType => GamePadType.SwitchJoyCon;
    }
}