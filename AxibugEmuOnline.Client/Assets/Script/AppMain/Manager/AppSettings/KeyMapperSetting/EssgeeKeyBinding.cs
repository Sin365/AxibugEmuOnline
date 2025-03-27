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

    public abstract class EssgeeKeyBinding : EmuCoreControllerKeyBinding<EssgeeSingleKey>
    {
        protected override void OnRegistDevices(InputDevice_D device, BindingPage binding)
        {
            if (device is Keyboard_D keyboard)
            {
                switch (binding.ControllerIndex)
                {
                    case 0:
                        binding.SetBinding(EssgeeSingleKey.OPTION_1, keyboard.Return, 0);
                        binding.SetBinding(EssgeeSingleKey.OPTION_2, keyboard.RightShift, 0);
                        binding.SetBinding(EssgeeSingleKey.UP, keyboard.W, 0);
                        binding.SetBinding(EssgeeSingleKey.DOWN, keyboard.S, 0);
                        binding.SetBinding(EssgeeSingleKey.LEFT, keyboard.A, 0);
                        binding.SetBinding(EssgeeSingleKey.RIGHT, keyboard.D, 0);
                        binding.SetBinding(EssgeeSingleKey.BTN_1, keyboard.J, 0);
                        binding.SetBinding(EssgeeSingleKey.BTN_2, keyboard.K, 0);
                        binding.SetBinding(EssgeeSingleKey.BTN_3, keyboard.U, 0);
                        binding.SetBinding(EssgeeSingleKey.BTN_4, keyboard.I, 0);
                        break;
                    case 1:
                        binding.SetBinding(EssgeeSingleKey.OPTION_1, keyboard.Keypad0, 0);
                        binding.SetBinding(EssgeeSingleKey.OPTION_2, keyboard.Delete, 0);
                        binding.SetBinding(EssgeeSingleKey.UP, keyboard.UpArrow, 0);
                        binding.SetBinding(EssgeeSingleKey.DOWN, keyboard.DownArrow, 0);
                        binding.SetBinding(EssgeeSingleKey.LEFT, keyboard.LeftArrow, 0);
                        binding.SetBinding(EssgeeSingleKey.RIGHT, keyboard.RightArrow, 0);
                        binding.SetBinding(EssgeeSingleKey.BTN_1, keyboard.Keypad1, 0);
                        binding.SetBinding(EssgeeSingleKey.BTN_2, keyboard.Keypad2, 0);
                        binding.SetBinding(EssgeeSingleKey.BTN_3, keyboard.Keypad3, 0);
                        binding.SetBinding(EssgeeSingleKey.BTN_4, keyboard.Keypad4, 0);
                        break;
                }
            }
            else if (device is PSVController_D psvCon && binding.ControllerIndex == 0)
            {
                binding.SetBinding(EssgeeSingleKey.OPTION_1, psvCon.Start, 0);
                binding.SetBinding(EssgeeSingleKey.OPTION_2, psvCon.Select, 0);
                binding.SetBinding(EssgeeSingleKey.UP, psvCon.Up, 0);
                binding.SetBinding(EssgeeSingleKey.DOWN, psvCon.Down, 0);
                binding.SetBinding(EssgeeSingleKey.LEFT, psvCon.Left, 0);
                binding.SetBinding(EssgeeSingleKey.RIGHT, psvCon.Right, 0);
                binding.SetBinding(EssgeeSingleKey.BTN_1, psvCon.Cross, 0);
                binding.SetBinding(EssgeeSingleKey.BTN_2, psvCon.Circle, 0);
                binding.SetBinding(EssgeeSingleKey.BTN_3, psvCon.Square, 0);
                binding.SetBinding(EssgeeSingleKey.BTN_4, psvCon.Triangle, 0);
                //PSV 摇杆
                binding.SetBinding(EssgeeSingleKey.UP, psvCon.LeftStick.UP, 1);
                binding.SetBinding(EssgeeSingleKey.DOWN, psvCon.LeftStick.Down, 1);
                binding.SetBinding(EssgeeSingleKey.LEFT, psvCon.LeftStick.Left, 1);
                binding.SetBinding(EssgeeSingleKey.RIGHT, psvCon.LeftStick.Right, 1);
            }
        }
    }

    public class MasterSystemKeyBinding : EssgeeKeyBinding
    {
        public override RomPlatformType Platform => RomPlatformType.MasterSystem;
        public override int ControllerCount => 2;
    }

    public class SG1000KeyBinding : EssgeeKeyBinding
    {
        public override RomPlatformType Platform => RomPlatformType.Sg1000;
        public override int ControllerCount => 2;
    }

    public class ColecoVisionKeyBinding : EssgeeKeyBinding
    {
        public override RomPlatformType Platform => RomPlatformType.ColecoVision;
        public override int ControllerCount => 2;
    }

    public class GameBoyColorKeyBinding : EssgeeKeyBinding
    {
        public override RomPlatformType Platform => RomPlatformType.GameBoyColor;
        public override int ControllerCount => 1;

        protected override void OnRegistDevices(InputDevice_D device, BindingPage binding)
        {
            if (device is Keyboard_D keyboard)
            {
                switch (binding.ControllerIndex)
                {
                    case 0:
                        binding.SetBinding(EssgeeSingleKey.OPTION_1, keyboard.Return, 0);
                        binding.SetBinding(EssgeeSingleKey.OPTION_2, keyboard.RightShift, 0);
                        binding.SetBinding(EssgeeSingleKey.UP, keyboard.W, 0);
                        binding.SetBinding(EssgeeSingleKey.DOWN, keyboard.S, 0);
                        binding.SetBinding(EssgeeSingleKey.LEFT, keyboard.A, 0);
                        binding.SetBinding(EssgeeSingleKey.RIGHT, keyboard.D, 0);
                        binding.SetBinding(EssgeeSingleKey.BTN_1, keyboard.J, 0);
                        binding.SetBinding(EssgeeSingleKey.BTN_2, keyboard.K, 0);
                        binding.SetBinding(EssgeeSingleKey.BTN_3, keyboard.U, 0);
                        binding.SetBinding(EssgeeSingleKey.BTN_4, keyboard.I, 0);
                        break;
                }
            }
        }
    }
    public class GameBoyKeyBinding : EssgeeKeyBinding
    {
        public override RomPlatformType Platform => RomPlatformType.GameBoy;
        public override int ControllerCount => 1;

        protected override void OnRegistDevices(InputDevice_D device, BindingPage binding)
        {
            if (device is Keyboard_D keyboard)
            {
                switch (binding.ControllerIndex)
                {
                    case 0:
                        binding.SetBinding(EssgeeSingleKey.OPTION_1, keyboard.Return, 0);
                        binding.SetBinding(EssgeeSingleKey.OPTION_2, keyboard.RightShift, 0);
                        binding.SetBinding(EssgeeSingleKey.UP, keyboard.W, 0);
                        binding.SetBinding(EssgeeSingleKey.DOWN, keyboard.S, 0);
                        binding.SetBinding(EssgeeSingleKey.LEFT, keyboard.A, 0);
                        binding.SetBinding(EssgeeSingleKey.RIGHT, keyboard.D, 0);
                        binding.SetBinding(EssgeeSingleKey.BTN_1, keyboard.J, 0);
                        binding.SetBinding(EssgeeSingleKey.BTN_2, keyboard.K, 0);
                        binding.SetBinding(EssgeeSingleKey.BTN_3, keyboard.U, 0);
                        binding.SetBinding(EssgeeSingleKey.BTN_4, keyboard.I, 0);
                        break;
                }
            }
        }
    }

    public class GameGearKeyBinding : EssgeeKeyBinding
    {
        public override RomPlatformType Platform => RomPlatformType.GameGear;
        public override int ControllerCount => 2;
    }

    public class SC3000KeyBinding : EssgeeKeyBinding
    {
        public override RomPlatformType Platform => RomPlatformType.Sc3000;
        public override int ControllerCount => 2;
    }
}