using System.IO;

namespace VirtualNes.Core
{
    public static class Supporter
    {
        private static ISupporterImpl s_support;
        public static void Setup(ISupporterImpl supporter)
        {
            s_support = supporter;
        }

        public static Stream OpenRom(string fname)
        {
            return s_support.OpenRom(fname);
        }

        public static void GetFilePathInfo(string fname, out string fullPath, out string directPath)
        {
            s_support.GetRomPathInfo(fname, out fullPath, out directPath);
        }

        public static Stream OpenFile_DISKSYS()
        {
            return s_support.OpenFile_DISKSYS();
        }

        public static void SaveSRAMToFile(byte[] sramContent, string romName)
        {
            s_support.SaveSRAMToFile(sramContent, romName);
        }

        public static void SaveDISKToFile(byte[] diskFileContent, string romName)
        {
            s_support.SaveDISKToFile(diskFileContent, romName);
        }

        public static void PrepareDirectory(string directPath)
        {
            s_support.PrepareDirectory(directPath);
        }

        public static void SaveFile(byte[] fileData, string directPath, string fileName)
        {
            s_support.SaveFile(fileData, directPath, fileName);
        }
        public static Stream OpenFile(string directPath, string fileName)
        {
            return s_support.OpenFile(directPath, fileName);
        }

        public static bool TryGetMapperNo(ROM rom, out int mapperNo)
        {
            return s_support.TryGetMapperNo(rom, out mapperNo);
        }

        public static ControllerState GetControllerState()
        {
            return s_support.GetControllerState();
        }

        public static void SampleInput(uint frameCount)
        {
            s_support.SampleInput(frameCount);
        }

        public static EmulatorConfig Config => s_support.Config;
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
