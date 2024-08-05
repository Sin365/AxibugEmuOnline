namespace VirtualNes.Core
{
    public class EmulatorConfig
    {
        private bool m_bKeyboardDisable;

        public CfgGeneral general { get; private set; } = new CfgGeneral();
        public CfgPath path { get; private set; } = new CfgPath();
        public CfgEmulator emulator { get; private set; } = new CfgEmulator();
        public CfgGraphics graphics { get; private set; } = new CfgGraphics();
        public CfgSound sound { get; private set; } = new CfgSound();
        public CfgShortCut shortcut { get; private set; } = new CfgShortCut();
        public CfgLanguage language { get; private set; } = new CfgLanguage();
        public CfgController controller { get; private set; } = new CfgController();
        public CfgMovie movie { get; private set; } = new CfgMovie();
        public CfgLauncher launcher { get; private set; } = new CfgLauncher();
        public CfgExtraSound extsound { get; private set; } = new CfgExtraSound();
        public CfgNetPlay netplay { get; private set; } = new CfgNetPlay();
    }

    public static class GameOption
    {
        // Default保存
        public static int defRenderMethod;
        public static int defIRQtype;
        public static bool defFrameIRQ;
        public static bool defVideoMode;

        // データ
        public static int nRenderMethod;
        public static int nIRQtype;
        public static bool bFrameIRQ;
        public static bool bVideoMode;

        public static void Load(uint crc)
        {
            nRenderMethod = defRenderMethod;
            nIRQtype = defIRQtype;
            bFrameIRQ = defFrameIRQ;
            bVideoMode = defVideoMode;
        }

        public static void Load(uint gid, uint mid)
        {
            nRenderMethod = defRenderMethod;
            nIRQtype = defIRQtype;
            bFrameIRQ = defFrameIRQ;
            bVideoMode = defVideoMode;
        }
    }
}
