using AxibugEmuOnline.Client.InputDevices;
using AxibugProtobuf;
using System;

namespace AxibugEmuOnline.Client.Settings
{
    [Flags]
    public enum EssgeeSingleKey : ushort
    {
        NONE = 0,
        UP = 1,
        DOWN = 1 << 1,
        LEFT = 1 << 2,
        RIGHT = 1 << 3,
        BTN_1 = 1 << 4,
        BTN_2 = 1 << 5,
        BTN_3 = 1 << 6,
        BTN_4 = 1 << 7,
        OPTION_1 = 1 << 8,
        OPTION_2 = 1 << 9,
    }

    public abstract class EssgeeKeyBinding : EmuCoreBinder<EssgeeSingleKey>
    {
        public override void Bind(Keyboard_D device, ControllerBinder controller)
        {
            switch (controller.ControllerIndex)
            {
                case 0:
                    controller.SetBinding(EssgeeSingleKey.OPTION_1, device.Return, 0);
                    controller.SetBinding(EssgeeSingleKey.OPTION_2, device.RightShift, 0);
                    controller.SetBinding(EssgeeSingleKey.UP, device.W, 0);
                    controller.SetBinding(EssgeeSingleKey.DOWN, device.S, 0);
                    controller.SetBinding(EssgeeSingleKey.LEFT, device.A, 0);
                    controller.SetBinding(EssgeeSingleKey.RIGHT, device.D, 0);
                    controller.SetBinding(EssgeeSingleKey.BTN_1, device.J, 0);
                    controller.SetBinding(EssgeeSingleKey.BTN_2, device.K, 0);
                    controller.SetBinding(EssgeeSingleKey.BTN_3, device.U, 0);
                    controller.SetBinding(EssgeeSingleKey.BTN_4, device.I, 0);
                    break;
                case 1:
                    controller.SetBinding(EssgeeSingleKey.OPTION_1, device.Keypad0, 0);
                    controller.SetBinding(EssgeeSingleKey.OPTION_2, device.Delete, 0);
                    controller.SetBinding(EssgeeSingleKey.UP, device.UpArrow, 0);
                    controller.SetBinding(EssgeeSingleKey.DOWN, device.DownArrow, 0);
                    controller.SetBinding(EssgeeSingleKey.LEFT, device.LeftArrow, 0);
                    controller.SetBinding(EssgeeSingleKey.RIGHT, device.RightArrow, 0);
                    controller.SetBinding(EssgeeSingleKey.BTN_1, device.Keypad1, 0);
                    controller.SetBinding(EssgeeSingleKey.BTN_2, device.Keypad2, 0);
                    controller.SetBinding(EssgeeSingleKey.BTN_3, device.Keypad3, 0);
                    controller.SetBinding(EssgeeSingleKey.BTN_4, device.Keypad4, 0);
                    break;
            }
        }
        public override void Bind(PSVController_D device, ControllerBinder controller)
        {
            switch (controller.ControllerIndex)
            {
                case 0:
                    controller.SetBinding(EssgeeSingleKey.OPTION_1, device.Start, 0);
                    controller.SetBinding(EssgeeSingleKey.OPTION_2, device.Select, 0);
                    controller.SetBinding(EssgeeSingleKey.UP, device.Up, 0);
                    controller.SetBinding(EssgeeSingleKey.DOWN, device.Down, 0);
                    controller.SetBinding(EssgeeSingleKey.LEFT, device.Left, 0);
                    controller.SetBinding(EssgeeSingleKey.RIGHT, device.Right, 0);
                    controller.SetBinding(EssgeeSingleKey.BTN_1, device.Cross, 0);
                    controller.SetBinding(EssgeeSingleKey.BTN_2, device.Circle, 0);
                    controller.SetBinding(EssgeeSingleKey.BTN_3, device.Square, 0);
                    controller.SetBinding(EssgeeSingleKey.BTN_4, device.Triangle, 0);

                    controller.SetBinding(EssgeeSingleKey.UP, device.LeftStick.Up, 1);
                    controller.SetBinding(EssgeeSingleKey.DOWN, device.LeftStick.Down, 1);
                    controller.SetBinding(EssgeeSingleKey.LEFT, device.LeftStick.Left, 1);
                    controller.SetBinding(EssgeeSingleKey.RIGHT, device.LeftStick.Right, 1);
                    break;
            }
        }
        public override void Bind(DualShockController_D device, ControllerBinder controller)
        {
            controller.SetBinding(EssgeeSingleKey.OPTION_1, device.Options, 0);
            controller.SetBinding(EssgeeSingleKey.OPTION_2, device.Share, 0);
            controller.SetBinding(EssgeeSingleKey.UP, device.Up, 0);
            controller.SetBinding(EssgeeSingleKey.DOWN, device.Down, 0);
            controller.SetBinding(EssgeeSingleKey.LEFT, device.Left, 0);
            controller.SetBinding(EssgeeSingleKey.RIGHT, device.Right, 0);
            controller.SetBinding(EssgeeSingleKey.BTN_1, device.Cross, 0);
            controller.SetBinding(EssgeeSingleKey.BTN_2, device.Circle, 0);
            controller.SetBinding(EssgeeSingleKey.BTN_3, device.Square, 0);
            controller.SetBinding(EssgeeSingleKey.BTN_4, device.Triangle, 0);

            controller.SetBinding(EssgeeSingleKey.UP, device.LeftStick.Up, 1);
            controller.SetBinding(EssgeeSingleKey.DOWN, device.LeftStick.Down, 1);
            controller.SetBinding(EssgeeSingleKey.LEFT, device.LeftStick.Left, 1);
            controller.SetBinding(EssgeeSingleKey.RIGHT, device.LeftStick.Right, 1);
        }
        public override void Bind(GamePad_D device, ControllerBinder controller)
        {
            controller.SetBinding(EssgeeSingleKey.OPTION_1, device.Start, 0);
            controller.SetBinding(EssgeeSingleKey.OPTION_2, device.Select, 0);
            controller.SetBinding(EssgeeSingleKey.UP, device.Up, 0);
            controller.SetBinding(EssgeeSingleKey.DOWN, device.Down, 0);
            controller.SetBinding(EssgeeSingleKey.LEFT, device.Left, 0);
            controller.SetBinding(EssgeeSingleKey.RIGHT, device.Right, 0);
            controller.SetBinding(EssgeeSingleKey.BTN_1, device.South, 0);
            controller.SetBinding(EssgeeSingleKey.BTN_2, device.East, 0);
            controller.SetBinding(EssgeeSingleKey.BTN_3, device.West, 0);
            controller.SetBinding(EssgeeSingleKey.BTN_4, device.North, 0);

            controller.SetBinding(EssgeeSingleKey.UP, device.LeftStick.Up, 1);
            controller.SetBinding(EssgeeSingleKey.DOWN, device.LeftStick.Down, 1);
            controller.SetBinding(EssgeeSingleKey.LEFT, device.LeftStick.Left, 1);
            controller.SetBinding(EssgeeSingleKey.RIGHT, device.LeftStick.Right, 1);
        }
        public override void Bind(XboxController_D device, ControllerBinder controller)
        {
            controller.SetBinding(EssgeeSingleKey.OPTION_1, device.Menu, 0);
            controller.SetBinding(EssgeeSingleKey.OPTION_2, device.View, 0);
            controller.SetBinding(EssgeeSingleKey.UP, device.Up, 0);
            controller.SetBinding(EssgeeSingleKey.DOWN, device.Down, 0);
            controller.SetBinding(EssgeeSingleKey.LEFT, device.Left, 0);
            controller.SetBinding(EssgeeSingleKey.RIGHT, device.Right, 0);
            controller.SetBinding(EssgeeSingleKey.BTN_1, device.A, 0);
            controller.SetBinding(EssgeeSingleKey.BTN_2, device.B, 0);
            controller.SetBinding(EssgeeSingleKey.BTN_3, device.X, 0);
            controller.SetBinding(EssgeeSingleKey.BTN_4, device.Y, 0);

            controller.SetBinding(EssgeeSingleKey.UP, device.LeftStick.Up, 1);
            controller.SetBinding(EssgeeSingleKey.DOWN, device.LeftStick.Down, 1);
            controller.SetBinding(EssgeeSingleKey.LEFT, device.LeftStick.Left, 1);
            controller.SetBinding(EssgeeSingleKey.RIGHT, device.LeftStick.Right, 1);
        }
        public override void Bind(ScreenGamepad_D device, ControllerBinder controller)
        {
            controller.SetBinding(EssgeeSingleKey.OPTION_1, device.OPTION_1, 0);
            controller.SetBinding(EssgeeSingleKey.OPTION_2, device.OPTION_2, 0);
            controller.SetBinding(EssgeeSingleKey.UP, device.UP, 0);
            controller.SetBinding(EssgeeSingleKey.DOWN, device.DOWN, 0);
            controller.SetBinding(EssgeeSingleKey.LEFT, device.LEFT, 0);
            controller.SetBinding(EssgeeSingleKey.RIGHT, device.RIGHT, 0);
            controller.SetBinding(EssgeeSingleKey.BTN_1, device.BTN_A, 0);
            controller.SetBinding(EssgeeSingleKey.BTN_2, device.BTN_B, 0);
            controller.SetBinding(EssgeeSingleKey.BTN_3, device.BTN_C, 0);
            controller.SetBinding(EssgeeSingleKey.BTN_4, device.BTN_D, 0);

            controller.SetBinding(EssgeeSingleKey.UP, device.JOYSTICK.Up, 1);
            controller.SetBinding(EssgeeSingleKey.DOWN, device.JOYSTICK.Down, 1);
            controller.SetBinding(EssgeeSingleKey.LEFT, device.JOYSTICK.Left, 1);
            controller.SetBinding(EssgeeSingleKey.RIGHT, device.JOYSTICK.Right, 1);
        }
    }

    public class MasterSystemKeyBinding : EssgeeKeyBinding
    {
        public override RomPlatformType Platform => RomPlatformType.MasterSystem;
        public override int ControllerCount => 4;
    }

    public class SG1000KeyBinding : EssgeeKeyBinding
    {
        public override RomPlatformType Platform => RomPlatformType.Sg1000;
        public override int ControllerCount => 4;
    }

    public class ColecoVisionKeyBinding : EssgeeKeyBinding
    {
        public override RomPlatformType Platform => RomPlatformType.ColecoVision;
        public override int ControllerCount => 4;
    }

    public class GameBoyColorKeyBinding : EssgeeKeyBinding
    {
        public override RomPlatformType Platform => RomPlatformType.GameBoyColor;
        public override int ControllerCount => 4;
    }
    public class GameBoyKeyBinding : EssgeeKeyBinding
    {
        public override RomPlatformType Platform => RomPlatformType.GameBoy;
        public override int ControllerCount => 4;
    }

    public class GameGearKeyBinding : EssgeeKeyBinding
    {
        public override RomPlatformType Platform => RomPlatformType.GameGear;
        public override int ControllerCount => 4;
    }

    public class SC3000KeyBinding : EssgeeKeyBinding
    {
        public override RomPlatformType Platform => RomPlatformType.Sc3000;
        public override int ControllerCount => 4;
    }
}