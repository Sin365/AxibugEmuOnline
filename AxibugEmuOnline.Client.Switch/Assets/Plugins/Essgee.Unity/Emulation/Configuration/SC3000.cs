namespace Essgee.Emulation.Configuration
{
    //todo Unity [ElementPriority(1)]
    public class SC3000 : IConfiguration
    {
        //todo Unity [DropDownControl("General", "TV Standard", typeof(TVStandard))]
        //[JsonConverter(typeof(StringEnumConverter))]
        public TVStandard TVStandard { get; set; }

        //todo Unity [DropDownControl("General", "Reset Button", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey InputReset { get; set; }

        //todo Unity [DropDownControl("General", "Change Input Mode", typeof(Keys), Keys.F11, Tooltip = "Selects which PC keyboard key is used to switch between SC-3000 keyboard and controller input.")]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey InputChangeMode { get; set; }

        //todo Unity [DropDownControl("General", "Play Tape", typeof(Keys), Keys.F11, Tooltip = "Note that tape emulation is currently non-functional.")]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey InputPlayTape { get; set; }

        //todo Unity [DropDownControl("Controller Port 1", "D-Pad Up", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey Joypad1Up { get; set; }
        //todo Unity [DropDownControl("Controller Port 1", "D-Pad Down", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey Joypad1Down { get; set; }
        //todo Unity [DropDownControl("Controller Port 1", "D-Pad Left", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey Joypad1Left { get; set; }
        //todo Unity [DropDownControl("Controller Port 1", "D-Pad Right", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey Joypad1Right { get; set; }
        //todo Unity [DropDownControl("Controller Port 1", "Button 1", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey Joypad1Button1 { get; set; }
        //todo Unity [DropDownControl("Controller Port 1", "Button 2", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey Joypad1Button2 { get; set; }

        //todo Unity [DropDownControl("Controller Port 2", "D-Pad Up", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey Joypad2Up { get; set; }
        //todo Unity [DropDownControl("Controller Port 2", "D-Pad Down", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey Joypad2Down { get; set; }
        //todo Unity [DropDownControl("Controller Port 2", "D-Pad Left", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey Joypad2Left { get; set; }
        //todo Unity [DropDownControl("Controller Port 2", "D-Pad Right", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey Joypad2Right { get; set; }
        //todo Unity [DropDownControl("Controller Port 2", "Button 1", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey Joypad2Button1 { get; set; }
        //todo Unity [DropDownControl("Controller Port 2", "Button 2", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey Joypad2Button2 { get; set; }

        public SC3000()
        {
            TVStandard = TVStandard.NTSC;

            InputReset = EssgeeMotionKey.F12;
            InputChangeMode = EssgeeMotionKey.F1;
            InputPlayTape = EssgeeMotionKey.F2;

            Joypad1Up = EssgeeMotionKey.Up;
            Joypad1Down = EssgeeMotionKey.Down;
            Joypad1Left = EssgeeMotionKey.Left;
            Joypad1Right = EssgeeMotionKey.Right;
            Joypad1Button1 = EssgeeMotionKey.A;
            Joypad1Button2 = EssgeeMotionKey.S;

            Joypad2Up = EssgeeMotionKey.NumPad8;
            Joypad2Down = EssgeeMotionKey.NumPad2;
            Joypad2Left = EssgeeMotionKey.NumPad4;
            Joypad2Right = EssgeeMotionKey.NumPad6;
            Joypad2Button1 = EssgeeMotionKey.NumPad1;
            Joypad2Button2 = EssgeeMotionKey.NumPad3;
        }
    }
}
