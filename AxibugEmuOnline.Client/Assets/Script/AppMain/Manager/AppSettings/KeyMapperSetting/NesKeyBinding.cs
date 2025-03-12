using AxibugEmuOnline.Client.ClientCore;
using AxibugProtobuf;
using VirtualNes.Core;

namespace AxibugEmuOnline.Client
{
    public class NesKeyBinding : EmuCoreControllerKeyBinding<EnumButtonType>
    {
        public override RomPlatformType Platform => RomPlatformType.Nes;
        public override int ControllerCount => 4;

        protected override void OnLoadDefaultMapper(BindingPage binding)
        {
            var keyboard = App.inputDevicesMgr.GetKeyboard();
            switch (binding.ControllerIndex)
            {
                case 0:
                    binding.SetBinding(EnumButtonType.LEFT, keyboard.A, 0);
                    binding.SetBinding(EnumButtonType.RIGHT, keyboard.D, 0);
                    binding.SetBinding(EnumButtonType.UP, keyboard.W, 0);
                    binding.SetBinding(EnumButtonType.DOWN, keyboard.S, 0);
                    binding.SetBinding(EnumButtonType.A, keyboard.K, 0);
                    binding.SetBinding(EnumButtonType.B, keyboard.J, 0);
                    binding.SetBinding(EnumButtonType.SELECT, keyboard.V, 0);
                    binding.SetBinding(EnumButtonType.START, keyboard.B, 0);
                    binding.SetBinding(EnumButtonType.MIC, keyboard.M, 0);
                    break;
                case 1:
                    binding.SetBinding(EnumButtonType.UP, keyboard.UpArrow, 0);
                    binding.SetBinding(EnumButtonType.DOWN, keyboard.DownArrow, 0);
                    binding.SetBinding(EnumButtonType.LEFT, keyboard.LeftArrow, 0);
                    binding.SetBinding(EnumButtonType.RIGHT, keyboard.RightArrow, 0);
                    binding.SetBinding(EnumButtonType.A, keyboard.Keypad2, 0);
                    binding.SetBinding(EnumButtonType.B, keyboard.Keypad1, 0);
                    binding.SetBinding(EnumButtonType.SELECT, keyboard.Keypad0, 0);
                    binding.SetBinding(EnumButtonType.START, keyboard.KeypadPeriod, 0);
                    break;
            }
        }
    }
}