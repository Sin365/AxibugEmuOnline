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
    }
}
