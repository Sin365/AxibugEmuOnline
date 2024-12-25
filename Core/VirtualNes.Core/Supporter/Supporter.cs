using System.IO;

namespace VirtualNes.Core
{
    public static class Supporter
    {
        private static ISupporterImpl s_support;
        internal static ISupporterImpl S => s_support;

        public static void Setup(ISupporterImpl supporter)
        {
            s_support = supporter;
        }
    }

    public interface ISupporterImpl
    {
        Stream OpenRom(string fname);
        void GetRomPathInfo(string fname, out string fullPath, out string directPath);
        Stream OpenFile_DISKSYS();
        void SaveSRAMToFile(byte[] sramContent, string romName);
        void SaveDISKToFile(byte[] diskFileContent, string romName);
        EmulatorConfig Config { get; }
        void PrepareDirectory(string directPath);
        void SaveFile(byte[] fileData, string directPath, string fileName);
        Stream OpenFile(string directPath, string fileName);
        bool TryGetMapperNo(ROM rom, out int mapperNo);
        ControllerState GetControllerState();
        void SampleInput(uint frameCount);
    }
}