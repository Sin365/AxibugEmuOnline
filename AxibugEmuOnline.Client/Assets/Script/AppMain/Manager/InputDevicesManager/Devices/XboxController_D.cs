namespace AxibugEmuOnline.Client.InputDevices
{
    public class XboxController_D : InputDevice_D
    {
        public Button_C X;
        public Button_C Y;
        public Button_C A;
        public Button_C B;
        public Button_C Up;
        public Button_C Down;
        public Button_C Left;
        public Button_C Right;
        public Button_C View;
        public Button_C Menu;
        public Button_C LeftBumper;
        public Button_C LeftTrigger;
        public Button_C LeftStickPress;
        public Button_C RightBumper;
        public Button_C RightTrigger;
        public Button_C RightStickPress;
        public Stick_C LeftStick;
        public Stick_C RightStick;


        public XboxController_D(InputResolver resolver) : base(resolver) { }
    }
}