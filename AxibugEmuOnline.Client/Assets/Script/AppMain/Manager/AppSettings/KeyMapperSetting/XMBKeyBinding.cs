using AxibugEmuOnline.Client.InputDevices;
using AxibugEmuOnline.Client.Settings;
using AxibugProtobuf;

namespace AxibugEmuOnline.Client
{
    public class XMBKeyBinding : EmuCoreControllerKeyBinding<EnumCommand>
    {
        public override RomPlatformType Platform => RomPlatformType.Invalid;
        public override int ControllerCount => 2;

        public override void Bind(Keyboard_D device, ControllerBinder controller)
        {
            switch (controller.ControllerIndex)
            {
                case 0://设置标准UI控制 //第一套控制布局 WSAD+JKLI
                    controller.SetBinding(EnumCommand.Back, device.L, 0);
                    controller.SetBinding(EnumCommand.Enter, device.K, 0);
                    controller.SetBinding(EnumCommand.OptionMenu, device.I, 0);
                    controller.SetBinding(EnumCommand.SelectItemDown, device.S, 0);
                    controller.SetBinding(EnumCommand.SelectItemLeft, device.A, 0);
                    controller.SetBinding(EnumCommand.SelectItemRight, device.D, 0);
                    controller.SetBinding(EnumCommand.SelectItemUp, device.W, 0);

                    //第二套控制布局 LOWB用
                    controller.SetBinding(EnumCommand.Back, device.Escape, 1);
                    controller.SetBinding(EnumCommand.Back, device.Backspace, 2);
                    controller.SetBinding(EnumCommand.Enter, device.Return, 1);
                    controller.SetBinding(EnumCommand.OptionMenu, device.LeftShift, 1);
                    controller.SetBinding(EnumCommand.OptionMenu, device.RightShift, 2);
                    controller.SetBinding(EnumCommand.SelectItemDown, device.DownArrow, 1);
                    controller.SetBinding(EnumCommand.SelectItemLeft, device.LeftArrow, 1);
                    controller.SetBinding(EnumCommand.SelectItemRight, device.RightArrow, 1);
                    controller.SetBinding(EnumCommand.SelectItemUp, device.UpArrow, 1);
                    break;
                case 1://游戏中UI控制
                    controller.SetBinding(EnumCommand.OptionMenu, device.Escape, 0);
                    break;
            }
        }
        public override void Bind(DualShockController_D device, ControllerBinder controller)
        {
            controller.SetBinding(EnumCommand.Back, device.Circle, 0);
            controller.SetBinding(EnumCommand.Enter, device.Cross, 0);
            controller.SetBinding(EnumCommand.OptionMenu, device.Options, 0);
            controller.SetBinding(EnumCommand.SelectItemDown, device.Down, 0);
            controller.SetBinding(EnumCommand.SelectItemLeft, device.Left, 0);
            controller.SetBinding(EnumCommand.SelectItemRight, device.Right, 0);
            controller.SetBinding(EnumCommand.SelectItemUp, device.Up, 0);

            controller.SetBinding(EnumCommand.SelectItemDown, device.LeftStick.Down, 1);
            controller.SetBinding(EnumCommand.SelectItemLeft, device.LeftStick.Left, 1);
            controller.SetBinding(EnumCommand.SelectItemRight, device.LeftStick.Right, 1);
            controller.SetBinding(EnumCommand.SelectItemUp, device.LeftStick.Up, 1);
        }
        public override void Bind(GamePad_D device, ControllerBinder controller)
        {
            controller.SetBinding(EnumCommand.Back, device.East, 0);
            controller.SetBinding(EnumCommand.Enter, device.South, 0);
            controller.SetBinding(EnumCommand.OptionMenu, device.Start, 0);
            controller.SetBinding(EnumCommand.SelectItemDown, device.Down, 0);
            controller.SetBinding(EnumCommand.SelectItemLeft, device.Left, 0);
            controller.SetBinding(EnumCommand.SelectItemRight, device.Right, 0);
            controller.SetBinding(EnumCommand.SelectItemUp, device.Up, 0);

            controller.SetBinding(EnumCommand.SelectItemDown, device.LeftStick.Down, 1);
            controller.SetBinding(EnumCommand.SelectItemLeft, device.LeftStick.Left, 1);
            controller.SetBinding(EnumCommand.SelectItemRight, device.LeftStick.Right, 1);
            controller.SetBinding(EnumCommand.SelectItemUp, device.LeftStick.Up, 1);
        }
        public override void Bind(PSVController_D device, ControllerBinder controller)
        {
            controller.SetBinding(EnumCommand.Back, device.Circle, 0);
            controller.SetBinding(EnumCommand.Enter, device.Cross, 0);
            controller.SetBinding(EnumCommand.OptionMenu, device.Start, 0);
            controller.SetBinding(EnumCommand.SelectItemDown, device.Down, 0);
            controller.SetBinding(EnumCommand.SelectItemLeft, device.Left, 0);
            controller.SetBinding(EnumCommand.SelectItemRight, device.Right, 0);
            controller.SetBinding(EnumCommand.SelectItemUp, device.Up, 0);

            controller.SetBinding(EnumCommand.SelectItemDown, device.LeftStick.Down, 1);
            controller.SetBinding(EnumCommand.SelectItemLeft, device.LeftStick.Left, 1);
            controller.SetBinding(EnumCommand.SelectItemRight, device.LeftStick.Right, 1);
            controller.SetBinding(EnumCommand.SelectItemUp, device.LeftStick.Up, 1);
        }
        public override void Bind(XboxController_D device, ControllerBinder controller)
        {
            controller.SetBinding(EnumCommand.Back, device.B, 0);
            controller.SetBinding(EnumCommand.Enter, device.A, 0);
            controller.SetBinding(EnumCommand.OptionMenu, device.Menu, 0);
            controller.SetBinding(EnumCommand.SelectItemDown, device.Down, 0);
            controller.SetBinding(EnumCommand.SelectItemLeft, device.Left, 0);
            controller.SetBinding(EnumCommand.SelectItemRight, device.Right, 0);
            controller.SetBinding(EnumCommand.SelectItemUp, device.Up, 0);

            controller.SetBinding(EnumCommand.SelectItemDown, device.LeftStick.Down, 1);
            controller.SetBinding(EnumCommand.SelectItemLeft, device.LeftStick.Left, 1);
            controller.SetBinding(EnumCommand.SelectItemRight, device.LeftStick.Right, 1);
            controller.SetBinding(EnumCommand.SelectItemUp, device.LeftStick.Up, 1);
        }
    }
}