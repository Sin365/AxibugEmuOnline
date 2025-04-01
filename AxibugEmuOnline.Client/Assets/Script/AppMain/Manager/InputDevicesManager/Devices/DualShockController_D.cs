namespace AxibugEmuOnline.Client.InputDevices
{
    /// <summary> PS3,PS4控制器 </summary>
    public class DualShockController_D : InputDevice_D
    {
        public Button_C Circle;
        public Button_C Triangle;
        public Button_C Cross;
        public Button_C Square;
        public Button_C Up;
        public Button_C Down;
        public Button_C Left;
        public Button_C Right;
        public Button_C L1;
        public Button_C L2;
        public Button_C L3;
        public Button_C R1;
        public Button_C R2;
        public Button_C R3;
        public Button_C Share;
        public Button_C Options;
        public Button_C TouchpadBtn;
        public Stick_C LeftStick;
        public Stick_C RightStick;

        public DualShockController_D(InputResolver resolver) : base(resolver) { }
    }
}