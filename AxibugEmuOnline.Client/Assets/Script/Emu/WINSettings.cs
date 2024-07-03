using System.Reflection;
using MyNes.Core;

namespace AxibugEmuOnline.Client
{
    public class WINSettings : ISettings
    {
        public string App_Version = "";

        public int Win_Location_X = 10;

        public int Win_Location_Y = 10;

        public int Win_Size_W = 768;

        public int Win_Size_H = 743;

        public bool Win_StartInFullscreen;

        public string[] Misc_RecentFiles = new string[0];

        public bool PauseEmuWhenFocusLost = true;

        public bool ShowGettingStarted = true;

        public string InterfaceLanguage = "English";

        public bool ShutdowOnEscapePress = true;

        public bool LoadStateOpenRecent;

        public string Database_FilePath = "";

        public string[] Database_FoldersSnapshots;

        public string[] Database_FoldersCovers;

        public string[] Database_FoldersInfos;

        public string[] Database_FoldersScanned;

        public bool LauncherRememberLastSelection = true;

        public int LauncherLatestSelection;

        public int LauncherLocationX = 10;

        public int LauncherLocationY = 10;

        public int LauncherSizeW = 1480;

        public int LauncherSizeH = 920;

        public int LauncherSpliter1 = 807;

        public int LauncherSpliter2 = 420;

        public int LauncherSpliter3 = 308;

        public int LauncherSpliter4 = 271;

        public bool LauncherAutoMinimize = true;

        public bool LauncherAutoCycleImagesInGameTab = true;

        public bool LauncherShowAyAppStart;

        public int SnapsView_ImageMode = 1;

        public bool SnapsView_ShowBar = true;

        public bool SnapsView_ShowStatus = true;

        public bool SnapsView_AutoCycle = true;

        public int CoversView_ImageMode = 1;

        public bool CoversView_ShowBar = true;

        public bool CoversView_ShowStatus = true;

        public bool CoversView_AutoCycle = true;

        public WINSettings(string path)
            : base(path)
        {
        }

        public override void LoadSettings()
        {
            base.LoadSettings();
            if (App_Version != Assembly.GetExecutingAssembly().GetName().Version.ToString())
            {
                ShowGettingStarted = true;
                App_Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }
    }

}