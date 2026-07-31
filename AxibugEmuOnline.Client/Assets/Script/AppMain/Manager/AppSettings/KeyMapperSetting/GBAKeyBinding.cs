using AxibugEmuOnline.Client.InputDevices;
using AxibugProtobuf;

namespace AxibugEmuOnline.Client.Settings
{
    public class GBAKeyBinding : EmuCoreBinder<GBAKeyCode>
    {
        public override int ControllerCount => 4;

        public override RomPlatformType Platform => RomPlatformType.GameBoyAdvance;

        public override void Bind(Keyboard_D device, ControllerBinder controller)
        {
            switch (controller.ControllerIndex)
            {
                case 0:
                    controller.SetBinding(GBAKeyCode.Select, device.RightShift, 0);
                    controller.SetBinding(GBAKeyCode.Start, device.Return, 0);
                    controller.SetBinding(GBAKeyCode.Up, device.W, 0);
                    controller.SetBinding(GBAKeyCode.Down, device.S, 0);
                    controller.SetBinding(GBAKeyCode.Left, device.A, 0);
                    controller.SetBinding(GBAKeyCode.Right, device.D, 0);
                    controller.SetBinding(GBAKeyCode.B, device.J, 0);
                    controller.SetBinding(GBAKeyCode.A, device.K, 0);
                    controller.SetBinding(GBAKeyCode.L, device.U, 0);
                    controller.SetBinding(GBAKeyCode.R, device.I, 0);
                    break;
            }
        }
        public override void Bind(DualShockController_D device, ControllerBinder controller)
        {
            controller.SetBinding(GBAKeyCode.Select, device.Share, 0);
            controller.SetBinding(GBAKeyCode.Start, device.Options, 0);
            controller.SetBinding(GBAKeyCode.Up, device.Up, 0);
            controller.SetBinding(GBAKeyCode.Down, device.Down, 0);
            controller.SetBinding(GBAKeyCode.Left, device.Left, 0);
            controller.SetBinding(GBAKeyCode.Right, device.Right, 0);
            controller.SetBinding(GBAKeyCode.B, device.Cross, 0);
            controller.SetBinding(GBAKeyCode.A, device.Circle, 0);
            controller.SetBinding(GBAKeyCode.L, device.L1, 0);
            controller.SetBinding(GBAKeyCode.R, device.R1, 0);
            controller.SetBinding(GBAKeyCode.Up, device.LeftStick.Up, 1);
            controller.SetBinding(GBAKeyCode.Down, device.LeftStick.Down, 1);
            controller.SetBinding(GBAKeyCode.Left, device.LeftStick.Left, 1);
            controller.SetBinding(GBAKeyCode.Right, device.LeftStick.Right, 1);
        }
        public override void Bind(GamePad_D device, ControllerBinder controller)
        {

            controller.SetBinding(GBAKeyCode.Select, device.Select, 0);
            controller.SetBinding(GBAKeyCode.Start, device.Start, 0);
            controller.SetBinding(GBAKeyCode.Up, device.Up, 0);
            controller.SetBinding(GBAKeyCode.Down, device.Down, 0);
            controller.SetBinding(GBAKeyCode.Left, device.Left, 0);
            controller.SetBinding(GBAKeyCode.Right, device.Right, 0);
            controller.SetBinding(GBAKeyCode.B, device.South, 0);
            controller.SetBinding(GBAKeyCode.A, device.East, 0);
            controller.SetBinding(GBAKeyCode.L, device.LeftShoulder, 0);
            controller.SetBinding(GBAKeyCode.R, device.RightShoulder, 0);
            controller.SetBinding(GBAKeyCode.Up, device.LeftStick.Up, 1);
            controller.SetBinding(GBAKeyCode.Down, device.LeftStick.Down, 1);
            controller.SetBinding(GBAKeyCode.Left, device.LeftStick.Left, 1);
            controller.SetBinding(GBAKeyCode.Right, device.LeftStick.Right, 1);
        }
        public override void Bind(PSVController_D device, ControllerBinder controller)
        {
            controller.SetBinding(GBAKeyCode.Select, device.Select, 0);
            controller.SetBinding(GBAKeyCode.Start, device.Start, 0);
            controller.SetBinding(GBAKeyCode.Up, device.Up, 0);
            controller.SetBinding(GBAKeyCode.Down, device.Down, 0);
            controller.SetBinding(GBAKeyCode.Left, device.Left, 0);
            controller.SetBinding(GBAKeyCode.Right, device.Right, 0);
            controller.SetBinding(GBAKeyCode.B, device.Cross, 0);
            controller.SetBinding(GBAKeyCode.A, device.Circle, 0);
            controller.SetBinding(GBAKeyCode.L, device.L, 0);
            controller.SetBinding(GBAKeyCode.R, device.R, 0);
            controller.SetBinding(GBAKeyCode.Up, device.LeftStick.Up, 1);
            controller.SetBinding(GBAKeyCode.Down, device.LeftStick.Down, 1);
            controller.SetBinding(GBAKeyCode.Left, device.LeftStick.Left, 1);
            controller.SetBinding(GBAKeyCode.Right, device.LeftStick.Right, 1);
        }
        public override void Bind(XboxController_D device, ControllerBinder controller)
        {
            controller.SetBinding(GBAKeyCode.Select, device.View, 0);
            controller.SetBinding(GBAKeyCode.Start, device.Menu, 0);
            controller.SetBinding(GBAKeyCode.Up, device.Up, 0);
            controller.SetBinding(GBAKeyCode.Down, device.Down, 0);
            controller.SetBinding(GBAKeyCode.Left, device.Left, 0);
            controller.SetBinding(GBAKeyCode.Right, device.Right, 0);
            controller.SetBinding(GBAKeyCode.B, device.A, 0);
            controller.SetBinding(GBAKeyCode.A, device.B, 0);
            controller.SetBinding(GBAKeyCode.L, device.LeftTrigger, 0);
            controller.SetBinding(GBAKeyCode.R, device.RightTrigger, 0);
            controller.SetBinding(GBAKeyCode.Up, device.LeftStick.Up, 1);
            controller.SetBinding(GBAKeyCode.Down, device.LeftStick.Down, 1);
            controller.SetBinding(GBAKeyCode.Left, device.LeftStick.Left, 1);
            controller.SetBinding(GBAKeyCode.Right, device.LeftStick.Right, 1);
        }
        public override void Bind(ScreenGamepad_D device, ControllerBinder controller)
        {
            controller.SetBinding(GBAKeyCode.Select, device.OPTION_2, 0);
            controller.SetBinding(GBAKeyCode.Start, device.OPTION_1, 0);
            controller.SetBinding(GBAKeyCode.Up, device.UP, 0);
            controller.SetBinding(GBAKeyCode.Down, device.DOWN, 0);
            controller.SetBinding(GBAKeyCode.Left, device.LEFT, 0);
            controller.SetBinding(GBAKeyCode.Right, device.RIGHT, 0);
            controller.SetBinding(GBAKeyCode.A, device.BTN_A, 0);
            controller.SetBinding(GBAKeyCode.B, device.BTN_B, 0);
            controller.SetBinding(GBAKeyCode.L, device.BTN_L, 0);
            controller.SetBinding(GBAKeyCode.R, device.BTN_R, 0);

            controller.SetBinding(GBAKeyCode.Up, device.JOYSTICK.Up, 1);
            controller.SetBinding(GBAKeyCode.Down, device.JOYSTICK.Down, 1);
            controller.SetBinding(GBAKeyCode.Left, device.JOYSTICK.Left, 1);
            controller.SetBinding(GBAKeyCode.Right, device.JOYSTICK.Right, 1);
        }
        public override void Bind(StandaloneSwitchProController_D device, ControllerBinder controller)
        {
            controller.SetBinding(GBAKeyCode.Select, device.Minus, 0);
            controller.SetBinding(GBAKeyCode.Start, device.Plus, 0);
            controller.SetBinding(GBAKeyCode.Up, device.Up, 0);
            controller.SetBinding(GBAKeyCode.Down, device.Down, 0);
            controller.SetBinding(GBAKeyCode.Left, device.Left, 0);
            controller.SetBinding(GBAKeyCode.Right, device.Right, 0);
            controller.SetBinding(GBAKeyCode.B, device.B, 0);
            controller.SetBinding(GBAKeyCode.A, device.A, 0);
            controller.SetBinding(GBAKeyCode.L, device.leftTrigger, 0);
            controller.SetBinding(GBAKeyCode.R, device.leftShoulder, 0);
            controller.SetBinding(GBAKeyCode.Up, device.LeftStick.Up, 1);
            controller.SetBinding(GBAKeyCode.Down, device.LeftStick.Down, 1);
            controller.SetBinding(GBAKeyCode.Left, device.LeftStick.Left, 1);
            controller.SetBinding(GBAKeyCode.Right, device.LeftStick.Right, 1);
        }
        public override void Bind(SwitchJoyCon_D device, ControllerBinder controller)
        {
            controller.SetBinding(GBAKeyCode.Select, device.Minus, 0);
            controller.SetBinding(GBAKeyCode.Start, device.Plus, 0);
            controller.SetBinding(GBAKeyCode.Up, device.Up, 0);
            controller.SetBinding(GBAKeyCode.Down, device.Down, 0);
            controller.SetBinding(GBAKeyCode.Left, device.Left, 0);
            controller.SetBinding(GBAKeyCode.Right, device.Right, 0);
            controller.SetBinding(GBAKeyCode.B, device.B, 0);
            controller.SetBinding(GBAKeyCode.A, device.A, 0);
            controller.SetBinding(GBAKeyCode.L, device.leftTrigger, 0);
            controller.SetBinding(GBAKeyCode.L, device.leftShoulder, 0);
            controller.SetBinding(GBAKeyCode.R, device.rightTrigger, 0);
            controller.SetBinding(GBAKeyCode.R, device.rightShoulder, 0);
            controller.SetBinding(GBAKeyCode.Up, device.LeftStick.Up, 1);
            controller.SetBinding(GBAKeyCode.Down, device.LeftStick.Down, 1);
            controller.SetBinding(GBAKeyCode.Left, device.LeftStick.Left, 1);
            controller.SetBinding(GBAKeyCode.Right, device.LeftStick.Right, 1);
        }
    }
}