using AxibugEmuOnline.Client.InputDevices;
using AxibugProtobuf;

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

    public abstract class MAMEKeyBinding : EmuCoreControllerKeyBinding<UMAMEKSingleKey>
    {
        public override int ControllerCount => 4;

        protected override void OnRegistDevices(InputDevice device, BindingPage binding)
        {
            if (device is KeyBoard keyboard)
            {
                switch (binding.ControllerIndex)
                {
                    case 0:
                        binding.SetBinding(UMAMEKSingleKey.INSERT_COIN, keyboard.Q, 0);
                        binding.SetBinding(UMAMEKSingleKey.GAMESTART, keyboard.E, 0);
                        binding.SetBinding(UMAMEKSingleKey.UP, keyboard.W, 0);
                        binding.SetBinding(UMAMEKSingleKey.DOWN, keyboard.S, 0);
                        binding.SetBinding(UMAMEKSingleKey.LEFT, keyboard.A, 0);
                        binding.SetBinding(UMAMEKSingleKey.RIGHT, keyboard.D, 0);
                        binding.SetBinding(UMAMEKSingleKey.BTN_A, keyboard.J, 0);
                        binding.SetBinding(UMAMEKSingleKey.BTN_B, keyboard.K, 0);
                        binding.SetBinding(UMAMEKSingleKey.BTN_C, keyboard.L, 0);
                        binding.SetBinding(UMAMEKSingleKey.BTN_D, keyboard.U, 0);
                        binding.SetBinding(UMAMEKSingleKey.BTN_E, keyboard.I, 0);
                        binding.SetBinding(UMAMEKSingleKey.BTN_F, keyboard.O, 0);
                        break;
                    case 1:
                        binding.SetBinding(UMAMEKSingleKey.INSERT_COIN, keyboard.Delete, 0);
                        binding.SetBinding(UMAMEKSingleKey.GAMESTART, keyboard.PageDown, 0);
                        binding.SetBinding(UMAMEKSingleKey.UP, keyboard.UpArrow, 0);
                        binding.SetBinding(UMAMEKSingleKey.DOWN, keyboard.DownArrow, 0);
                        binding.SetBinding(UMAMEKSingleKey.LEFT, keyboard.LeftArrow, 0);
                        binding.SetBinding(UMAMEKSingleKey.RIGHT, keyboard.RightArrow, 0);
                        binding.SetBinding(UMAMEKSingleKey.BTN_A, keyboard.Keypad1, 0);
                        binding.SetBinding(UMAMEKSingleKey.BTN_B, keyboard.Keypad2, 0);
                        binding.SetBinding(UMAMEKSingleKey.BTN_C, keyboard.Keypad3, 0);
                        binding.SetBinding(UMAMEKSingleKey.BTN_D, keyboard.Keypad4, 0);
                        binding.SetBinding(UMAMEKSingleKey.BTN_E, keyboard.Keypad5, 0);
                        binding.SetBinding(UMAMEKSingleKey.BTN_F, keyboard.Keypad6, 0);
                        break;
                }
            }
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