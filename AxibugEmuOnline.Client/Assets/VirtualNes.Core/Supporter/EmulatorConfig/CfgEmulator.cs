namespace VirtualNes.Core
{
    public class CfgEmulator
    {
        public bool bIllegalOp { get; set; } = false;
        public bool bAutoFrameSkip { get; set; } = true;
        public bool bThrottle { get; set; } = true;
        public int nThrottleFPS { get; set; } = 120;
        public bool bBackground { get; set; } = false;
        public int nPriority { get; set; } = 3;
        public bool bFourPlayer { get; set; } = true;
        public bool bCrcCheck { get; set; } = true;
        public bool bDiskThrottle { get; set; } = true;
        public bool bLoadFullscreen { get; set; } = false;
        public bool bPNGsnapshot { get; set; } = false;
        public bool bAutoIPS { get; set; } = false;
    }
}