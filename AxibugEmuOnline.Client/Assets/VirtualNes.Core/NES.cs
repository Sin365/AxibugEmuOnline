using System;
using System.Collections.Generic;
using System.IO;
using VirtualNes.Core.Debug;

namespace VirtualNes.Core
{
    public class NES
    {
        public CPU cpu;
        public PPU ppu;
        public APU apu;
        public ROM rom;
        public PAD pad;
        public Mapper mapper;
        public NesConfig NesCfg;

        protected List<CHEATCODE> m_CheatCode = new List<CHEATCODE>();

        private bool m_bDiskThrottle;
        private int m_CommandRequest;
        private int m_nSnapNo;
        private bool m_bNsfPlaying;
        private bool m_bMoviePlay;
        private bool m_bMovieRec;
        private Stream m_fpMovie;
        private bool m_bTapePlay;
        private bool m_bTapeRec;
        private Stream m_fpTape;
        private double m_TapeCycles;
        private byte m_TapeIn;
        private byte m_TapeOut;
        private bool m_bBarcode2;
        private int m_TurboFileBank;
        private int SAVERAM_SIZE;
        private int nIRQtype;
        private bool bFrameIRQ;
        private bool bVideoMode;

        public NES(string fname)
        {
            Debuger.Log("VirtuaNES - CSharpCore\n");

            m_bDiskThrottle = false;
            m_CommandRequest = 0;

            m_nSnapNo = 0;

            m_bNsfPlaying = false;

            m_bMoviePlay = m_bMovieRec = false;
            m_fpMovie = null;

            m_bTapePlay = m_bTapeRec = false;
            m_fpTape = null;
            m_TapeCycles = 0d;
            m_TapeIn = m_TapeOut = 0;

            m_bBarcode2 = false;

            m_TurboFileBank = 0;

            cpu = null;
            ppu = null;
            apu = null;
            rom = null;
            pad = null;
            mapper = null;

            SAVERAM_SIZE = 8 * 1024;

            nIRQtype = 0;

            bFrameIRQ = true;

            bVideoMode = false;

            NesCfg = NesConfig.GetNTSC();

            CheatInitial();

            try
            {
                Debuger.Log("Allocating CPU...");
                cpu = new CPU(this);

                Debuger.Log("Allocating PPU...");
                ppu = new PPU(this);

                Debuger.Log("Allocating APU...");
                apu = new APU(this);

                Debuger.Log("Allocating PAD...");
                pad = new PAD(this);

                Debuger.Log("Loading ROM Image...");

                rom = new ROM(fname);
            }
            catch (Exception ex)
            {
                Debuger.LogError(ex.ToString());
            }
        }

        public void CheatInitial()
        {
            m_CheatCode.Clear();
        }
    }

    public struct NesConfig
    {
        public float BaseClock;         // NTSC:21477270.0  PAL:21281364.0
        public float CpuClock;          // NTSC: 1789772.5  PAL: 1773447.0

        public int TotalScanLines;      // NTSC: 262  PAL: 312

        public int ScanlingCycles;      // NTSC:1364  PAL:1362

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
                ScanlingCycles = 1364,
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
                ScanlingCycles = 1278,
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
