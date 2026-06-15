using AxibugEmuOnline.Client.InputDevices;
using AxibugProtobuf;
using StoicGooseUnity;

namespace AxibugEmuOnline.Client.Settings
{
    public abstract class StoicGooseBinding : EmuCoreBinder<StoicGooseKey>
    {
        public override void Bind(Keyboard_D device, ControllerBinder controller)
        {
            switch (controller.ControllerIndex)
            {
                case 0:
                    controller.SetBinding(StoicGooseKey.Start, device.Return, 0);
                    controller.SetBinding(StoicGooseKey.X1, device.W, 0);
                    controller.SetBinding(StoicGooseKey.X2, device.D, 0);
                    controller.SetBinding(StoicGooseKey.X3, device.S, 0);
                    controller.SetBinding(StoicGooseKey.X4, device.A, 0);
                    controller.SetBinding(StoicGooseKey.Y1, device.G, 0);
                    controller.SetBinding(StoicGooseKey.Y2, device.B, 0);
                    controller.SetBinding(StoicGooseKey.Y3, device.V, 0);
                    controller.SetBinding(StoicGooseKey.Y4, device.C, 0);
                    controller.SetBinding(StoicGooseKey.B, device.J, 0);
                    controller.SetBinding(StoicGooseKey.A, device.K, 0);
                    break;
                case 1:
                    controller.SetBinding(StoicGooseKey.Start, device.KeypadEnter, 0);
                    controller.SetBinding(StoicGooseKey.X1, device.UpArrow, 0);
                    controller.SetBinding(StoicGooseKey.X2, device.RightArrow, 0);
                    controller.SetBinding(StoicGooseKey.X3, device.DownArrow, 0);
                    controller.SetBinding(StoicGooseKey.X4, device.LeftArrow, 0);
                    controller.SetBinding(StoicGooseKey.Y1, device.Home, 0);
                    controller.SetBinding(StoicGooseKey.Y2, device.PageDown, 0);
                    controller.SetBinding(StoicGooseKey.Y3, device.End, 0);
                    controller.SetBinding(StoicGooseKey.Y4, device.Delete, 0);
                    controller.SetBinding(StoicGooseKey.B, device.Keypad1, 0);
                    controller.SetBinding(StoicGooseKey.A, device.Keypad2, 0);
                    break;
            }
        }
        public override void Bind(PSVController_D device, ControllerBinder controller)
        {
            switch (controller.ControllerIndex)
            {
                case 0:
                    controller.SetBinding(StoicGooseKey.Start, device.Start, 0);
                    controller.SetBinding(StoicGooseKey.X1, device.Up, 0);
                    controller.SetBinding(StoicGooseKey.X2, device.Right, 0);
                    controller.SetBinding(StoicGooseKey.X3, device.Down, 0);
                    controller.SetBinding(StoicGooseKey.X4, device.Left, 0);
                    controller.SetBinding(StoicGooseKey.Y1, device.RightStick.Up, 0);
                    controller.SetBinding(StoicGooseKey.Y2, device.RightStick.Right, 0);
                    controller.SetBinding(StoicGooseKey.Y3, device.RightStick.Down, 0);
                    controller.SetBinding(StoicGooseKey.Y4, device.RightStick.Left, 0);
                    controller.SetBinding(StoicGooseKey.B, device.Cross, 0);
                    controller.SetBinding(StoicGooseKey.A, device.Circle, 0);
                    break;
            }
        }
        public override void Bind(DualShockController_D device, ControllerBinder controller)
        {
            controller.SetBinding(StoicGooseKey.Start, device.Options, 0);
            controller.SetBinding(StoicGooseKey.X1, device.Up, 0);
            controller.SetBinding(StoicGooseKey.X2, device.Right, 0);
            controller.SetBinding(StoicGooseKey.X3, device.Down, 0);
            controller.SetBinding(StoicGooseKey.X4, device.Left, 0);
            controller.SetBinding(StoicGooseKey.Y1, device.RightStick.Up, 0);
            controller.SetBinding(StoicGooseKey.Y2, device.RightStick.Right, 0);
            controller.SetBinding(StoicGooseKey.Y3, device.RightStick.Down, 0);
            controller.SetBinding(StoicGooseKey.Y4, device.RightStick.Left, 0);
            controller.SetBinding(StoicGooseKey.B, device.Cross, 0);
            controller.SetBinding(StoicGooseKey.A, device.Circle, 0);
        }
        public override void Bind(GamePad_D device, ControllerBinder controller)
        {
            controller.SetBinding(StoicGooseKey.Start, device.Start, 0);
            controller.SetBinding(StoicGooseKey.X1, device.Up, 0);
            controller.SetBinding(StoicGooseKey.X2, device.Right, 0);
            controller.SetBinding(StoicGooseKey.X3, device.Down, 0);
            controller.SetBinding(StoicGooseKey.X4, device.Left, 0);
            controller.SetBinding(StoicGooseKey.Y1, device.RightStick.Up, 0);
            controller.SetBinding(StoicGooseKey.Y2, device.RightStick.Right, 0);
            controller.SetBinding(StoicGooseKey.Y3, device.RightStick.Down, 0);
            controller.SetBinding(StoicGooseKey.Y4, device.RightStick.Left, 0);
            controller.SetBinding(StoicGooseKey.B, device.South, 0);
            controller.SetBinding(StoicGooseKey.A, device.East, 0);
        }
        public override void Bind(XboxController_D device, ControllerBinder controller)
        {
            controller.SetBinding(StoicGooseKey.Start, device.Menu, 0);
            controller.SetBinding(StoicGooseKey.X1, device.Up, 0);
            controller.SetBinding(StoicGooseKey.X2, device.Right, 0);
            controller.SetBinding(StoicGooseKey.X3, device.Down, 0);
            controller.SetBinding(StoicGooseKey.X4, device.Right, 0);
            controller.SetBinding(StoicGooseKey.Y1, device.RightStick.Up, 0);
            controller.SetBinding(StoicGooseKey.Y2, device.RightStick.Right, 0);
            controller.SetBinding(StoicGooseKey.Y3, device.RightStick.Down, 0);
            controller.SetBinding(StoicGooseKey.Y4, device.RightStick.Left, 0);
            controller.SetBinding(StoicGooseKey.B, device.A, 0);
            controller.SetBinding(StoicGooseKey.A, device.B, 0);
        }
        public override void Bind(ScreenGamepad_D device, ControllerBinder controller)
        {
            controller.SetBinding(StoicGooseKey.Start, device.OPTION_1, 0);

            controller.SetBinding(StoicGooseKey.Y1, device.SUB_UP, 0);
            controller.SetBinding(StoicGooseKey.Y2, device.SUB_RIGHT, 0);
            controller.SetBinding(StoicGooseKey.Y3, device.SUB_DOWN, 0);
            controller.SetBinding(StoicGooseKey.Y4, device.SUB_LEFT, 0);

            controller.SetBinding(StoicGooseKey.X1, device.JOYSTICK.Up, 0);
            controller.SetBinding(StoicGooseKey.X2, device.JOYSTICK.Right, 0);
            controller.SetBinding(StoicGooseKey.X3, device.JOYSTICK.Down, 0);
            controller.SetBinding(StoicGooseKey.X4, device.JOYSTICK.Left, 0);

            controller.SetBinding(StoicGooseKey.A, device.BTN_B, 0);
            controller.SetBinding(StoicGooseKey.B, device.BTN_A, 0);

            //屏幕暂时没有第二个方向控制暂时用CDEF代替

        }

        public override void Bind(StandaloneSwitchProController_D device, ControllerBinder controller)
        {
            controller.SetBinding(StoicGooseKey.Start, device.Plus, 0);
            controller.SetBinding(StoicGooseKey.X1, device.Up, 0);
            controller.SetBinding(StoicGooseKey.X2, device.Right, 0);
            controller.SetBinding(StoicGooseKey.X3, device.Down, 0);
            controller.SetBinding(StoicGooseKey.X4, device.Left, 0);
            controller.SetBinding(StoicGooseKey.Y1, device.RightStick.Up, 0);
            controller.SetBinding(StoicGooseKey.Y2, device.RightStick.Right, 0);
            controller.SetBinding(StoicGooseKey.Y3, device.RightStick.Down, 0);
            controller.SetBinding(StoicGooseKey.Y4, device.RightStick.Left, 0);
            controller.SetBinding(StoicGooseKey.B, device.A, 0);
            controller.SetBinding(StoicGooseKey.A, device.B, 0);
        }

        public override void Bind(SwitchJoyCon_D device, ControllerBinder controller)
        {
            controller.SetBinding(StoicGooseKey.Start, device.Plus, 0);
            controller.SetBinding(StoicGooseKey.X1, device.Up, 0);
            controller.SetBinding(StoicGooseKey.X2, device.Right, 0);
            controller.SetBinding(StoicGooseKey.X3, device.Down, 0);
            controller.SetBinding(StoicGooseKey.X4, device.Left, 0);
            controller.SetBinding(StoicGooseKey.Y1, device.RightStick.Up, 0);
            controller.SetBinding(StoicGooseKey.Y2, device.RightStick.Right, 0);
            controller.SetBinding(StoicGooseKey.Y3, device.RightStick.Down, 0);
            controller.SetBinding(StoicGooseKey.Y4, device.RightStick.Left, 0);
            controller.SetBinding(StoicGooseKey.B, device.A, 0);
            controller.SetBinding(StoicGooseKey.A, device.B, 0);
        }
    }

    public class WonderSwanColorKeyBinding : StoicGooseBinding
    {
        public override RomPlatformType Platform => RomPlatformType.WonderSwanColor;
        public override int ControllerCount => 1;
    }

    public class WonderSwanBinding : StoicGooseBinding
    {
        public override RomPlatformType Platform => RomPlatformType.WonderSwan;
        public override int ControllerCount => 1;
    }
}