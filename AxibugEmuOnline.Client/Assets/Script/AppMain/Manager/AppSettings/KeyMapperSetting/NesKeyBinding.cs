using AxibugEmuOnline.Client.InputDevices;
using AxibugProtobuf;
using VirtualNes.Core;

namespace AxibugEmuOnline.Client.Settings
{
    public class NesKeyBinding : EmuCoreBinder<EnumButtonType>
    {
        public override RomPlatformType Platform => RomPlatformType.Nes;
        public override int ControllerCount => 4;

        public override void Bind(Keyboard_D device, ControllerBinder controller)
        {
            switch (controller.ControllerIndex)
            {
                case 0:
                    controller.SetBinding(EnumButtonType.LEFT, device.A, 0);
                    controller.SetBinding(EnumButtonType.RIGHT, device.D, 0);
                    controller.SetBinding(EnumButtonType.UP, device.W, 0);
                    controller.SetBinding(EnumButtonType.DOWN, device.S, 0);
                    controller.SetBinding(EnumButtonType.A, device.K, 0);
                    controller.SetBinding(EnumButtonType.B, device.J, 0);
                    controller.SetBinding(EnumButtonType.SELECT, device.V, 0);
                    controller.SetBinding(EnumButtonType.START, device.B, 0);
                    controller.SetBinding(EnumButtonType.MIC, device.M, 0);
                    break;
                case 1:
                    controller.SetBinding(EnumButtonType.UP, device.UpArrow, 0);
                    controller.SetBinding(EnumButtonType.DOWN, device.DownArrow, 0);
                    controller.SetBinding(EnumButtonType.LEFT, device.LeftArrow, 0);
                    controller.SetBinding(EnumButtonType.RIGHT, device.RightArrow, 0);
                    controller.SetBinding(EnumButtonType.A, device.Keypad2, 0);
                    controller.SetBinding(EnumButtonType.B, device.Keypad1, 0);
                    controller.SetBinding(EnumButtonType.SELECT, device.Keypad0, 0);
                    controller.SetBinding(EnumButtonType.START, device.KeypadPeriod, 0);
                    break;
            }
        }
        public override void Bind(DualShockController_D device, ControllerBinder controller)
        {
            controller.SetBinding(EnumButtonType.LEFT, device.Left, 0);
            controller.SetBinding(EnumButtonType.RIGHT, device.Right, 0);
            controller.SetBinding(EnumButtonType.UP, device.Up, 0);
            controller.SetBinding(EnumButtonType.DOWN, device.Down, 0);
            controller.SetBinding(EnumButtonType.A, device.Cross, 0);
            controller.SetBinding(EnumButtonType.B, device.Square, 0);
            controller.SetBinding(EnumButtonType.SELECT, device.Share, 0);
            controller.SetBinding(EnumButtonType.START, device.Options, 0);
            controller.SetBinding(EnumButtonType.MIC, device.L1, 0);

            controller.SetBinding(EnumButtonType.LEFT, device.LeftStick.Left, 1);
            controller.SetBinding(EnumButtonType.RIGHT, device.LeftStick.Right, 1);
            controller.SetBinding(EnumButtonType.UP, device.LeftStick.Up, 1);
            controller.SetBinding(EnumButtonType.DOWN, device.LeftStick.Down, 1);
        }
        public override void Bind(GamePad_D device, ControllerBinder controller)
        {
            controller.SetBinding(EnumButtonType.LEFT, device.Left, 0);
            controller.SetBinding(EnumButtonType.RIGHT, device.Right, 0);
            controller.SetBinding(EnumButtonType.UP, device.Up, 0);
            controller.SetBinding(EnumButtonType.DOWN, device.Down, 0);
            controller.SetBinding(EnumButtonType.A, device.South, 0);
            controller.SetBinding(EnumButtonType.B, device.West, 0);
            controller.SetBinding(EnumButtonType.SELECT, device.Select, 0);
            controller.SetBinding(EnumButtonType.START, device.Start, 0);
            controller.SetBinding(EnumButtonType.MIC, device.LeftShoulder, 0);

            controller.SetBinding(EnumButtonType.LEFT, device.LeftStick.Left, 1);
            controller.SetBinding(EnumButtonType.RIGHT, device.LeftStick.Right, 1);
            controller.SetBinding(EnumButtonType.UP, device.LeftStick.Up, 1);
            controller.SetBinding(EnumButtonType.DOWN, device.LeftStick.Down, 1);
        }
        public override void Bind(PSVController_D device, ControllerBinder controller)
        {
            controller.SetBinding(EnumButtonType.LEFT, device.Left, 0);
            controller.SetBinding(EnumButtonType.RIGHT, device.Right, 0);
            controller.SetBinding(EnumButtonType.UP, device.Up, 0);
            controller.SetBinding(EnumButtonType.DOWN, device.Down, 0);
            controller.SetBinding(EnumButtonType.A, device.Cross, 0);
            controller.SetBinding(EnumButtonType.B, device.Square, 0);
            controller.SetBinding(EnumButtonType.SELECT, device.Select, 0);
            controller.SetBinding(EnumButtonType.START, device.Start, 0);
            controller.SetBinding(EnumButtonType.MIC, device.L, 0);

            controller.SetBinding(EnumButtonType.LEFT, device.LeftStick.Left, 1);
            controller.SetBinding(EnumButtonType.RIGHT, device.LeftStick.Right, 1);
            controller.SetBinding(EnumButtonType.UP, device.LeftStick.Up, 1);
            controller.SetBinding(EnumButtonType.DOWN, device.LeftStick.Down, 1);
        }
        public override void Bind(XboxController_D device, ControllerBinder controller)
        {
            controller.SetBinding(EnumButtonType.LEFT, device.Left, 0);
            controller.SetBinding(EnumButtonType.RIGHT, device.Right, 0);
            controller.SetBinding(EnumButtonType.UP, device.Up, 0);
            controller.SetBinding(EnumButtonType.DOWN, device.Down, 0);
            controller.SetBinding(EnumButtonType.A, device.A, 0);
            controller.SetBinding(EnumButtonType.B, device.X, 0);
            controller.SetBinding(EnumButtonType.SELECT, device.View, 0);
            controller.SetBinding(EnumButtonType.START, device.Menu, 0);
            controller.SetBinding(EnumButtonType.MIC, device.LeftBumper, 0);

            controller.SetBinding(EnumButtonType.LEFT, device.LeftStick.Left, 1);
            controller.SetBinding(EnumButtonType.RIGHT, device.LeftStick.Right, 1);
            controller.SetBinding(EnumButtonType.UP, device.LeftStick.Up, 1);
            controller.SetBinding(EnumButtonType.DOWN, device.LeftStick.Down, 1);
        }
        public override void Bind(ScreenGamepad_D device, ControllerBinder controller)
        {
            controller.SetBinding(EnumButtonType.LEFT, device.LEFT, 0);
            controller.SetBinding(EnumButtonType.RIGHT, device.RIGHT, 0);
            controller.SetBinding(EnumButtonType.UP, device.UP, 0);
            controller.SetBinding(EnumButtonType.DOWN, device.DOWN, 0);
            controller.SetBinding(EnumButtonType.A, device.BTN_A, 0);
            controller.SetBinding(EnumButtonType.B, device.BTN_B, 0);
            controller.SetBinding(EnumButtonType.START, device.OPTION_1, 0);
            controller.SetBinding(EnumButtonType.SELECT, device.OPTION_2, 0);
            controller.SetBinding(EnumButtonType.MIC, device.OPTION_3, 0);

            controller.SetBinding(EnumButtonType.LEFT, device.JOYSTICK.Left, 1);
            controller.SetBinding(EnumButtonType.RIGHT, device.JOYSTICK.Right, 1);
            controller.SetBinding(EnumButtonType.UP, device.JOYSTICK.Up, 1);
            controller.SetBinding(EnumButtonType.DOWN, device.JOYSTICK.Down, 1);
        }

        public override void Bind(StandaloneSwitchProController_D device, ControllerBinder controller)
        {
            controller.SetBinding(EnumButtonType.LEFT, device.Left, 0);
            controller.SetBinding(EnumButtonType.RIGHT, device.Right, 0);
            controller.SetBinding(EnumButtonType.UP, device.Up, 0);
            controller.SetBinding(EnumButtonType.DOWN, device.Down, 0);
            controller.SetBinding(EnumButtonType.A, device.A, 0);
            controller.SetBinding(EnumButtonType.B, device.B, 0);
            controller.SetBinding(EnumButtonType.SELECT, device.Minus, 0);
            controller.SetBinding(EnumButtonType.START, device.Plus, 0);
            controller.SetBinding(EnumButtonType.MIC, device.RightSL, 0);

            controller.SetBinding(EnumButtonType.LEFT, device.LeftStick.Left, 1);
            controller.SetBinding(EnumButtonType.RIGHT, device.LeftStick.Right, 1);
            controller.SetBinding(EnumButtonType.UP, device.LeftStick.Up, 1);
            controller.SetBinding(EnumButtonType.DOWN, device.LeftStick.Down, 1);
        }

        public override void Bind(SwitchJoyCon_D device, ControllerBinder controller)
        {
            controller.SetBinding(EnumButtonType.LEFT, device.Left, 0);
            controller.SetBinding(EnumButtonType.RIGHT, device.Right, 0);
            controller.SetBinding(EnumButtonType.UP, device.Up, 0);
            controller.SetBinding(EnumButtonType.DOWN, device.Down, 0);
            controller.SetBinding(EnumButtonType.A, device.A, 0);
            controller.SetBinding(EnumButtonType.B, device.B, 0);
            controller.SetBinding(EnumButtonType.SELECT, device.Minus, 0);
            controller.SetBinding(EnumButtonType.START, device.Plus, 0);
            controller.SetBinding(EnumButtonType.MIC, device.RightSL, 0);

            controller.SetBinding(EnumButtonType.LEFT, device.LeftStick.Left, 1);
            controller.SetBinding(EnumButtonType.RIGHT, device.LeftStick.Right, 1);
            controller.SetBinding(EnumButtonType.UP, device.LeftStick.Up, 1);
            controller.SetBinding(EnumButtonType.DOWN, device.LeftStick.Down, 1);
        }
    }
}