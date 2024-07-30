namespace VirtualNes.Core
{
    public class CfgGraphics
    {
        public bool bAspect = false;
        public bool bAllSprite = true;
        public bool bAllLine = false;
        public bool bFPSDisp = false;
        public bool bTVFrame = false;
        public bool bScanline = false;
        public int nScanlineColor = 75;
        public bool bSyncDraw = false;
        public bool bFitZoom = false;

        public bool bLeftClip = true;

        public bool bWindowVSync = false;

        public bool bSyncNoSleep = false;

        public bool bDiskAccessLamp = false;

        public bool bDoubleSize = false;
        public bool bSystemMemory = false;
        public bool bUseHEL = false;

        public bool bNoSquareList = false;

        public int nGraphicsFilter = 0;

        public uint dwDisplayWidth = 640;
        public uint dwDisplayHeight = 480;
        public uint dwDisplayDepth = 16;
        public uint dwDisplayRate = 0;

        public bool bPaletteFile = false;
        public char[] szPaletteFile = new char[260];
    }
}