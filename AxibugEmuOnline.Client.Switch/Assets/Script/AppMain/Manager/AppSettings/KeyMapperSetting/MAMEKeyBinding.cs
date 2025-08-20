using AxibugEmuOnline.Client.InputDevices;
using AxibugProtobuf;
using UnityEngine.UIElements;

namespace AxibugEmuOnline.Client.Settings
{
    public enum UMAMEKSingleKey
    {
        INSERT_COIN,
        GAMESTART,
        UP,
        DOWN,
        LEFT,
        RIGHT,
        BTN_A,
        BTN_B,
        BTN_C,
        BTN_D,
        BTN_E,
        BTN_F
    }

    public abstract class MAMEKeyBinding : EmuCoreBinder<UMAMEKSingleKey>
    {
        public override int ControllerCount => 4;

        public override void Bind(Keyboard_D device, ControllerBinder controller)
        {
            switch (controller.ControllerIndex)
            {
                case 0:
                    controller.SetBinding(UMAMEKSingleKey.INSERT_COIN, device.Q, 0);
                    controller.SetBinding(UMAMEKSingleKey.GAMESTART, device.E, 0);
                    controller.SetBinding(UMAMEKSingleKey.UP, device.W, 0);
                    controller.SetBinding(UMAMEKSingleKey.DOWN, device.S, 0);
                    controller.SetBinding(UMAMEKSingleKey.LEFT, device.A, 0);
                    controller.SetBinding(UMAMEKSingleKey.RIGHT, device.D, 0);
                    controller.SetBinding(UMAMEKSingleKey.BTN_A, device.J, 0);
                    controller.SetBinding(UMAMEKSingleKey.BTN_B, device.K, 0);
                    controller.SetBinding(UMAMEKSingleKey.BTN_C, device.L, 0);
                    controller.SetBinding(UMAMEKSingleKey.BTN_D, device.U, 0);
                    controller.SetBinding(UMAMEKSingleKey.BTN_E, device.I, 0);
                    controller.SetBinding(UMAMEKSingleKey.BTN_F, device.O, 0);
                    break;
                case 1:
                    controller.SetBinding(UMAMEKSingleKey.INSERT_COIN, device.Delete, 0);
                    controller.SetBinding(UMAMEKSingleKey.GAMESTART, device.PageDown, 0);
                    controller.SetBinding(UMAMEKSingleKey.UP, device.UpArrow, 0);
                    controller.SetBinding(UMAMEKSingleKey.DOWN, device.DownArrow, 0);
                    controller.SetBinding(UMAMEKSingleKey.LEFT, device.LeftArrow, 0);
                    controller.SetBinding(UMAMEKSingleKey.RIGHT, device.RightArrow, 0);
                    controller.SetBinding(UMAMEKSingleKey.BTN_A, device.Keypad1, 0);
                    controller.SetBinding(UMAMEKSingleKey.BTN_B, device.Keypad2, 0);
                    controller.SetBinding(UMAMEKSingleKey.BTN_C, device.Keypad3, 0);
                    controller.SetBinding(UMAMEKSingleKey.BTN_D, device.Keypad4, 0);
                    controller.SetBinding(UMAMEKSingleKey.BTN_E, device.Keypad5, 0);
                    controller.SetBinding(UMAMEKSingleKey.BTN_F, device.Keypad6, 0);
                    break;
            }
        }
        public override void Bind(DualShockController_D device, ControllerBinder controller)
        {
            controller.SetBinding(UMAMEKSingleKey.INSERT_COIN, device.Share, 0);
            controller.SetBinding(UMAMEKSingleKey.GAMESTART, device.Options, 0);
            controller.SetBinding(UMAMEKSingleKey.UP, device.Up, 0);
            controller.SetBinding(UMAMEKSingleKey.DOWN, device.Down, 0);
            controller.SetBinding(UMAMEKSingleKey.LEFT, device.Left, 0);
            controller.SetBinding(UMAMEKSingleKey.RIGHT, device.Right, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_A, device.Square, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_B, device.Cross, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_C, device.Circle, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_D, device.Triangle, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_E, device.R1, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_F, device.R2, 0);

            controller.SetBinding(UMAMEKSingleKey.UP, device.LeftStick.Up, 1);
            controller.SetBinding(UMAMEKSingleKey.DOWN, device.LeftStick.Down, 1);
            controller.SetBinding(UMAMEKSingleKey.LEFT, device.LeftStick.Left, 1);
            controller.SetBinding(UMAMEKSingleKey.RIGHT, device.LeftStick.Right, 1);
        }
        public override void Bind(GamePad_D device, ControllerBinder controller)
        {
            controller.SetBinding(UMAMEKSingleKey.INSERT_COIN, device.Select, 0);
            controller.SetBinding(UMAMEKSingleKey.GAMESTART, device.Start, 0);
            controller.SetBinding(UMAMEKSingleKey.UP, device.Up, 0);
            controller.SetBinding(UMAMEKSingleKey.DOWN, device.Down, 0);
            controller.SetBinding(UMAMEKSingleKey.LEFT, device.Left, 0);
            controller.SetBinding(UMAMEKSingleKey.RIGHT, device.Right, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_A, device.West, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_B, device.South, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_C, device.East, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_D, device.North, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_E, device.RightShoulder, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_F, device.RightTrigger, 0);

            controller.SetBinding(UMAMEKSingleKey.UP, device.LeftStick.Up, 1);
            controller.SetBinding(UMAMEKSingleKey.DOWN, device.LeftStick.Down, 1);
            controller.SetBinding(UMAMEKSingleKey.LEFT, device.LeftStick.Left, 1);
            controller.SetBinding(UMAMEKSingleKey.RIGHT, device.LeftStick.Right, 1);
        }
        public override void Bind(PSVController_D device, ControllerBinder controller)
        {
            controller.SetBinding(UMAMEKSingleKey.INSERT_COIN, device.Select, 0);
            controller.SetBinding(UMAMEKSingleKey.GAMESTART, device.Start, 0);
            controller.SetBinding(UMAMEKSingleKey.UP, device.Up, 0);
            controller.SetBinding(UMAMEKSingleKey.DOWN, device.Down, 0);
            controller.SetBinding(UMAMEKSingleKey.LEFT, device.Left, 0);
            controller.SetBinding(UMAMEKSingleKey.RIGHT, device.Right, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_A, device.Square, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_B, device.Cross, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_C, device.Circle, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_D, device.Triangle, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_E, device.L, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_F, device.R, 0);

            controller.SetBinding(UMAMEKSingleKey.UP, device.LeftStick.Up, 1);
            controller.SetBinding(UMAMEKSingleKey.DOWN, device.LeftStick.Down, 1);
            controller.SetBinding(UMAMEKSingleKey.LEFT, device.LeftStick.Left, 1);
            controller.SetBinding(UMAMEKSingleKey.RIGHT, device.LeftStick.Right, 1);
        }
        public override void Bind(XboxController_D device, ControllerBinder controller)
        {
            controller.SetBinding(UMAMEKSingleKey.INSERT_COIN, device.View, 0);
            controller.SetBinding(UMAMEKSingleKey.GAMESTART, device.Menu, 0);
            controller.SetBinding(UMAMEKSingleKey.UP, device.Up, 0);
            controller.SetBinding(UMAMEKSingleKey.DOWN, device.Down, 0);
            controller.SetBinding(UMAMEKSingleKey.LEFT, device.Left, 0);
            controller.SetBinding(UMAMEKSingleKey.RIGHT, device.Right, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_A, device.X, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_B, device.A, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_C, device.B, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_D, device.Y, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_E, device.RightBumper, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_F, device.RightTrigger, 0);

            controller.SetBinding(UMAMEKSingleKey.UP, device.LeftStick.Up, 1);
            controller.SetBinding(UMAMEKSingleKey.DOWN, device.LeftStick.Down, 1);
            controller.SetBinding(UMAMEKSingleKey.LEFT, device.LeftStick.Left, 1);
            controller.SetBinding(UMAMEKSingleKey.RIGHT, device.LeftStick.Right, 1);
        }
        public override void Bind(ScreenGamepad_D device, ControllerBinder controller)
        {
            controller.SetBinding(UMAMEKSingleKey.INSERT_COIN, device.OPTION_1, 0);
            controller.SetBinding(UMAMEKSingleKey.GAMESTART, device.OPTION_2, 0);
            controller.SetBinding(UMAMEKSingleKey.UP, device.UP, 0);
            controller.SetBinding(UMAMEKSingleKey.DOWN, device.DOWN, 0);
            controller.SetBinding(UMAMEKSingleKey.LEFT, device.LEFT, 0);
            controller.SetBinding(UMAMEKSingleKey.RIGHT, device.RIGHT, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_A, device.BTN_A, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_B, device.BTN_B, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_C, device.BTN_C, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_D, device.BTN_D, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_E, device.BTN_E, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_F, device.BTN_F, 0);

            controller.SetBinding(UMAMEKSingleKey.UP, device.JOYSTICK.Up, 1);
            controller.SetBinding(UMAMEKSingleKey.DOWN, device.JOYSTICK.Down, 1);
            controller.SetBinding(UMAMEKSingleKey.LEFT, device.JOYSTICK.Left, 1);
            controller.SetBinding(UMAMEKSingleKey.RIGHT, device.JOYSTICK.Right, 1);
        }

        public override void Bind(SwitchJoyCon_D device, ControllerBinder controller)
        {
            controller.SetBinding(UMAMEKSingleKey.INSERT_COIN, device.Minus, 0);
            controller.SetBinding(UMAMEKSingleKey.GAMESTART, device.Plus, 0);
            controller.SetBinding(UMAMEKSingleKey.UP, device.Up, 0);
            controller.SetBinding(UMAMEKSingleKey.DOWN, device.Down, 0);
            controller.SetBinding(UMAMEKSingleKey.LEFT, device.Left, 0);
            controller.SetBinding(UMAMEKSingleKey.RIGHT, device.Right, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_A, device.A, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_B, device.B, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_C, device.X, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_D, device.Y, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_E, device.LeftSL, 0);
            controller.SetBinding(UMAMEKSingleKey.BTN_F, device.RightSL, 0);

            controller.SetBinding(UMAMEKSingleKey.UP, device.LeftStick.Up, 1);
            controller.SetBinding(UMAMEKSingleKey.DOWN, device.LeftStick.Down, 1);
            controller.SetBinding(UMAMEKSingleKey.LEFT, device.LeftStick.Left, 1);
            controller.SetBinding(UMAMEKSingleKey.RIGHT, device.LeftStick.Right, 1);
        }
    }

    public class NEOGEOKeyBinding : MAMEKeyBinding
    {
        public override RomPlatformType Platform => RomPlatformType.Neogeo;
    }

    public class CPS1KeyBinding : MAMEKeyBinding
    {
        public override RomPlatformType Platform => RomPlatformType.Cps1;
    }

    public class CPS2KeyBinding : MAMEKeyBinding
    {
        public override RomPlatformType Platform => RomPlatformType.Cps2;
    }

    public class IGSKeyBinding : MAMEKeyBinding
    {
        public override RomPlatformType Platform => RomPlatformType.Igs;
    }

    public class OldArcadeKeyBinding : MAMEKeyBinding
    {
        public override RomPlatformType Platform => RomPlatformType.ArcadeOld;
    }
}