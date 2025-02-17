using Essgee.Emulation.Cartridges.Nintendo;
using Essgee.Emulation.ExtDevices.Nintendo;
using Essgee.Utilities;
using Newtonsoft.Json;
using System;

namespace Essgee.Emulation.Configuration
{
    //todo Unity [ElementPriority(6)]
    public class GameBoyColor : IConfiguration
    {
        //todo Unity [CheckBoxControl("General", "Use Bootstrap ROM")]
        public bool UseBootstrap { get; set; }
        [IsBootstrapRomPath]
        //todo Unity [FileBrowserControl("General", "Bootstrap Path", "Game Boy Color Bootstrap ROM (*.gbc;*.bin;*.zip)|*.gbc;*.bin;*.zip")]
        public string BootstrapRom { get; set; }
        //todo Unity [DropDownControl("General", "Serial Device", typeof(ISerialDevice))]
        [JsonConverter(typeof(TypeNameJsonConverter), "Essgee.Emulation.ExtDevices.Nintendo")]
        public Type SerialDevice { get; set; }

        //todo Unity [DropDownControl("GB Camera", "Camera Source", typeof(GBCameraCartridge.ImageSources))]
        //[JsonConverter(typeof(StringEnumConverter))]
        public GBCameraCartridge.ImageSources CameraSource { get; set; }
        //todo Unity [FileBrowserControl("GB Camera", "Camera Image", "Image Files (*.png;*.bmp)|*.png;*.bmp")]
        public string CameraImageFile { get; set; }

        //todo Unity [DropDownControl("Infrared", "Infrared Source", typeof(Machines.GameBoyColor.InfraredSources))]
        //[JsonConverter(typeof(StringEnumConverter))]
        public Machines.GameBoyColor.InfraredSources InfraredSource { get; set; }
        //todo Unity [FileBrowserControl("Infrared", "Pokemon Pikachu DB", "Database Binary (*.bin)|*.bin")]
        public string InfraredDatabasePikachu { get; set; }

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
        //todo Unity [DropDownControl("Controls", "A", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey ControlsA { get; set; }
        //todo Unity [DropDownControl("Controls", "B", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey ControlsB { get; set; }
        //todo Unity [DropDownControl("Controls", "Select", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey ControlsSelect { get; set; }
        //todo Unity [DropDownControl("Controls", "Start", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey ControlsStart { get; set; }
        //todo Unity [DropDownControl("Controls", "Send IR Signal", typeof(Keys), Keys.F11)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public EssgeeMotionKey ControlsSendIR { get; set; }

        public GameBoyColor()
        {
            UseBootstrap = false;
            BootstrapRom = string.Empty;

            SerialDevice = typeof(DummyDevice);
            CameraSource = GBCameraCartridge.ImageSources.Noise;
            CameraImageFile = string.Empty;

            InfraredSource = Machines.GameBoyColor.InfraredSources.None;
            InfraredDatabasePikachu = string.Empty;

            ControlsUp = EssgeeMotionKey.Up;
            ControlsDown = EssgeeMotionKey.Down;
            ControlsLeft = EssgeeMotionKey.Left;
            ControlsRight = EssgeeMotionKey.Right;
            ControlsA = EssgeeMotionKey.S;
            ControlsB = EssgeeMotionKey.A;
            ControlsSelect = EssgeeMotionKey.Space;
            ControlsStart = EssgeeMotionKey.Enter;
            ControlsSendIR = EssgeeMotionKey.Back;
        }
    }
}
