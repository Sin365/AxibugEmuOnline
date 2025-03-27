namespace AxibugEmuOnline.Client.InputDevices
{
    /// <summary>
    /// 通用游戏控制器
    /// </summary>
    public class GamePad_D : InputDevice_D
    {
        public Button_C Up;
        public Button_C Down;
        public Button_C Left;
        public Button_C Right;
        public Button_C Option;
        public Button_C Start;
        public Button_C North;
        public Button_C South;
        public Button_C West;
        public Button_C East;
        public Button_C LeftShoulder;
        public Button_C RightShoulder;
        public Button_C LeftTrigger;
        public Button_C RightTrigger;
        public Button_C LeftStickPress;
        public Button_C RightStickPress;
        public Stick_C LeftStick;
        public Stick_C RightStick;

        public GamePad_D(InputResolver resolver) : base(resolver) { }
    }
}