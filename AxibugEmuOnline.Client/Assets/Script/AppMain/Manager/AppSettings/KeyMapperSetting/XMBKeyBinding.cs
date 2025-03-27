using AxibugEmuOnline.Client.InputDevices;
using AxibugEmuOnline.Client.Settings;
using AxibugProtobuf;

namespace AxibugEmuOnline.Client
{
    public class XMBKeyBinding : EmuCoreControllerKeyBinding<EnumCommand>
    {
        public override RomPlatformType Platform => RomPlatformType.Invalid;
        public override int ControllerCount => 2;

        protected override void OnRegistDevices(InputDevice_D device, BindingPage binding)
        {
            if (device is Keyboard_D keyboard)
            {
                switch (binding.ControllerIndex)
                {
                    case 0://设置标准UI控制
                           //第一套控制布局 WSAD+JKLI
                        binding.SetBinding(EnumCommand.Back, keyboard.L, 0);
                        binding.SetBinding(EnumCommand.Enter, keyboard.K, 0);
                        binding.SetBinding(EnumCommand.OptionMenu, keyboard.I, 0);
                        binding.SetBinding(EnumCommand.SelectItemDown, keyboard.S, 0);
                        binding.SetBinding(EnumCommand.SelectItemLeft, keyboard.A, 0);
                        binding.SetBinding(EnumCommand.SelectItemRight, keyboard.D, 0);
                        binding.SetBinding(EnumCommand.SelectItemUp, keyboard.W, 0);

                        //第二套控制布局 LOWB用
                        binding.SetBinding(EnumCommand.Back, keyboard.Escape, 1);
                        binding.SetBinding(EnumCommand.Back, keyboard.Backspace, 2);
                        binding.SetBinding(EnumCommand.Enter, keyboard.Return, 1);
                        binding.SetBinding(EnumCommand.OptionMenu, keyboard.LeftShift, 1);
                        binding.SetBinding(EnumCommand.OptionMenu, keyboard.RightShift, 2);
                        binding.SetBinding(EnumCommand.SelectItemDown, keyboard.DownArrow, 1);
                        binding.SetBinding(EnumCommand.SelectItemLeft, keyboard.LeftArrow, 1);
                        binding.SetBinding(EnumCommand.SelectItemRight, keyboard.RightArrow, 1);
                        binding.SetBinding(EnumCommand.SelectItemUp, keyboard.UpArrow, 1);
                        break;
                    case 1://游戏中UI控制
                        binding.SetBinding(EnumCommand.OptionMenu, keyboard.Escape, 0);
                        break;
                }
            }
        }
    }
}