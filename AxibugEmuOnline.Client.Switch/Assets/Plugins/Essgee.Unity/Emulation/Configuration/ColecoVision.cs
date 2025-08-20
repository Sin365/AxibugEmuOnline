using Essgee.Utilities;

namespace Essgee.Emulation.Configuration
{
    ////todo Unity [ElementPriorityAttribute(4)]
    public class ColecoVision : IConfiguration
    {
        [IsBootstrapRomPath]
        //todo Unity [FileBrowserControl("General", "BIOS Path", "ColecoVision BIOS ROM (*.col;*.zip)|*.col;*.zip")]
        public string BiosRom { get; set; }

        //todo Unity [DropDownControl("Controls", "Up", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey ControlsUp { get; set; }
        //todo Unity [DropDownControl("Controls", "Down", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey ControlsDown { get; set; }
        //todo Unity [DropDownControl("Controls", "Left", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey ControlsLeft { get; set; }
        //todo Unity [DropDownControl("Controls", "Right", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey ControlsRight { get; set; }
        //todo Unity [DropDownControl("Controls", "Left Button", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey ControlsButtonLeft { get; set; }
        //todo Unity [DropDownControl("Controls", "Right Button", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey ControlsButtonRight { get; set; }

        //todo Unity [DropDownControl("Controls", "Keypad 1", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey ControlsKeypad1 { get; set; }
        //todo Unity [DropDownControl("Controls", "Keypad 2", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey ControlsKeypad2 { get; set; }
        //todo Unity [DropDownControl("Controls", "Keypad 3", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey ControlsKeypad3 { get; set; }
        //todo Unity [DropDownControl("Controls", "Keypad 4", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey ControlsKeypad4 { get; set; }
        //todo Unity [DropDownControl("Controls", "Keypad 5", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey ControlsKeypad5 { get; set; }
        //todo Unity [DropDownControl("Controls", "Keypad 6", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey ControlsKeypad6 { get; set; }
        //todo Unity [DropDownControl("Controls", "Keypad 7", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey ControlsKeypad7 { get; set; }
        //todo Unity [DropDownControl("Controls", "Keypad 8", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey ControlsKeypad8 { get; set; }
        //todo Unity [DropDownControl("Controls", "Keypad 9", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey ControlsKeypad9 { get; set; }
        //todo Unity [DropDownControl("Controls", "Keypad 0", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey ControlsKeypad0 { get; set; }
        //todo Unity [DropDownControl("Controls", "Keypad *", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey ControlsKeypadStar { get; set; }
        //todo Unity [DropDownControl("Controls", "Keypad #", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey ControlsKeypadNumberSign { get; set; }

        public ColecoVision()
        {
            BiosRom = string.Empty;

            ControlsUp = EssgeeMotionKey.Up;
            ControlsDown = EssgeeMotionKey.Down;
            ControlsLeft = EssgeeMotionKey.Left;
            ControlsRight = EssgeeMotionKey.Right;
            ControlsButtonLeft = EssgeeMotionKey.A;
            ControlsButtonRight = EssgeeMotionKey.S;

            ControlsKeypad1 = EssgeeMotionKey.NumPad1;
            ControlsKeypad2 = EssgeeMotionKey.NumPad2;
            ControlsKeypad3 = EssgeeMotionKey.NumPad3;
            ControlsKeypad4 = EssgeeMotionKey.NumPad4;
            ControlsKeypad5 = EssgeeMotionKey.NumPad5;
            ControlsKeypad6 = EssgeeMotionKey.NumPad6;
            ControlsKeypad7 = EssgeeMotionKey.NumPad7;
            ControlsKeypad8 = EssgeeMotionKey.NumPad8;
            ControlsKeypad9 = EssgeeMotionKey.NumPad9;
            ControlsKeypad0 = EssgeeMotionKey.NumPad0;
            ControlsKeypadStar = EssgeeMotionKey.Multiply;
            ControlsKeypadNumberSign = EssgeeMotionKey.Divide;
        }
    }
}
