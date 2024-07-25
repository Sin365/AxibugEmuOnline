namespace VirtualNes.Core
{
    public struct NesConfig
    {
        public float BaseClock;         // NTSC:21477270.0  PAL:21281364.0
        public float CpuClock;          // NTSC: 1789772.5  PAL: 1773447.0

        public int TotalScanLines;      // NTSC: 262  PAL: 312

        public int ScanlineCycles;      // NTSC:1364  PAL:1362

        public int HDrawCycles;         // NTSC:1024  PAL:1024
        public int HBlankCycles;        // NTSC: 340  PAL: 338
        public int ScanlineEndCycles;   // NTSC:   4  PAL:   2

        public int FrameCycles;         // NTSC:29829.52  PAL:35468.94
        public int FrameIrqCycles;      // NTSC:29829.52  PAL:35468.94

        public int FrameRate;           // NTSC:60(59.94) PAL:50
        public float FramePeriod;       // NTSC:16.683    PAL:20.0

        public static NesConfig GetNTSC()
        {
            return new NesConfig
            {
                BaseClock = 21477270.0f,
                CpuClock = 1789772.5f,
                TotalScanLines = 262,
                ScanlineCycles = 1364,
                HDrawCycles = 1024,
                HBlankCycles = 340,
                ScanlineEndCycles = 4,
                FrameCycles = 1364 * 262,
                FrameIrqCycles = 29830,
                FrameRate = 60,
                FramePeriod = 1000.0f / 60.0f
            };
        }

        public static NesConfig GetPAL()
        {
            return new NesConfig
            {
                BaseClock = 26601714.0f,
                CpuClock = 1662607.125f,
                TotalScanLines = 312,
                ScanlineCycles = 1278,
                HDrawCycles = 960,
                HBlankCycles = 318,
                ScanlineEndCycles = 2,
                FrameCycles = 1278 * 312,
                FrameIrqCycles = 33252,
                FrameRate = 50,
                FramePeriod = 1000.0f / 50.0f
            };
        }
    }
}