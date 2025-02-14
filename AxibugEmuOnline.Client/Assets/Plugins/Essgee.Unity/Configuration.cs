using Essgee.Emulation.Configuration;
using Essgee.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Drawing;

namespace Essgee
{
    public class Configuration
    {
        public const int RecentFilesCapacity = 15;
        public const string DefaultShaderName = "Basic";

        public bool LimitFps { get; set; }
        public bool ShowFps { get; set; }
        public bool Mute { get; set; }
        public float Volume { get; set; }
        public int SampleRate { get; set; }
        public bool LowPassFilter { get; set; }
        public int ScreenSize { get; set; }
        //[JsonConverter(typeof(StringEnumConverter))]
        public ScreenSizeMode ScreenSizeMode { get; set; }
        public string LastShader { get; set; }
        public bool EnableXInput { get; set; }
        public bool EnableRumble { get; set; }
        public bool AutoPause { get; set; }

        public List<string> RecentFiles { get; set; }

        //[JsonConverter(typeof(InterfaceDictionaryConverter<IConfiguration>))]
        public Dictionary<string, IConfiguration> Machines { get; set; }

        public Dictionary<string, Point> DebugWindows { get; set; }

        public Configuration()
        {
            LimitFps = true;
            ShowFps = false;
            Mute = false;
            Volume = 1.0f;
            SampleRate = 44100;
            LowPassFilter = true;
            ScreenSize = 2;
            ScreenSizeMode = ScreenSizeMode.Scale;
            LastShader = DefaultShaderName;
            EnableXInput = false;
            EnableRumble = false;
            AutoPause = true;

            RecentFiles = new List<string>(RecentFilesCapacity);

            Machines = new Dictionary<string, IConfiguration>();

            DebugWindows = new Dictionary<string, Point>();
        }
    }
}
