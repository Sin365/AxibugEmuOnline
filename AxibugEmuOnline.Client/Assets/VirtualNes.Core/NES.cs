using Codice.CM.Client.Differences;
using Google.Protobuf.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using UnityEngine.UIElements;
using UnityEngine;
using VirtualNes.Core.Debug;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

namespace VirtualNes.Core
{
    public class NES
    {
        public const int FETCH_CYCLES = 8;

        public CPU cpu;
        public PPU ppu;
        public APU apu;
        public ROM rom;
        public PAD pad;
        public Mapper mapper;
        public NesConfig nescfg;

        private List<CHEATCODE> m_CheatCode = new List<CHEATCODE>();
        private List<GENIECODE> m_GenieCode = new List<GENIECODE>();
        private bool m_bDiskThrottle;
        private int m_CommandRequest;
        private int m_nSnapNo;
        private bool m_bNsfPlaying;
        private bool m_bNsfInit;
        private int m_nNsfSongNo;
        private int m_nNsfSongMode;
        private bool m_bMoviePlay;
        private bool m_bMovieRec;
        private Stream m_fpMovie;
        private uint m_MovieControl;
        private int m_MovieStepTotal;
        private int m_MovieStep;
        private bool m_bTapePlay;
        private bool m_bTapeRec;
        private Stream m_fpTape;
        private double m_TapeCycles;
        private byte m_TapeIn;
        private byte m_TapeOut;

        // For Barcode
        private bool m_bBarcode;
        private byte m_BarcodeOut;
        private byte m_BarcodePtr;
        private int m_BarcodeCycles;
        private byte[] m_BarcodeData = new byte[256];

        // For Barcode
        private bool m_bBarcode2;
        private int m_Barcode2seq;
        private int m_Barcode2ptr;
        private int m_Barcode2cnt;
        private byte m_Barcode2bit;
        private byte[] m_Barcode2data = new byte[32];

        private int m_TurboFileBank;
        private int SAVERAM_SIZE;
        private int nIRQtype;
        private bool bFrameIRQ;
        private bool bVideoMode;
        private int NES_scanline;
        private EnumRenderMethod RenderMethod;
        private bool bZapper;
        private int ZapperX;
        private int ZapperY;
        private long base_cycles;
        private long emul_cycles;

        // For VS-Unisystem
        byte m_VSDipValue;
        VSDIPSWITCH[] m_VSDipTable;

        private byte[] m_PadImg = new byte[226]
        {
            28, 8,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x0F, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x0F, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x0F, 0x00, 0x00, 0x00, 0x0F, 0x0F, 0x00, 0x00,
            0x00, 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x0F, 0x0F, 0x0F, 0x0F, 0x00, 0x0F, 0x0F, 0x0F, 0x0F, 0x00,
            0x00, 0x00, 0x00, 0x0F, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x0F, 0x0F, 0x00, 0x0F,
            0x0F, 0x0F, 0x00, 0x00, 0x0F, 0x0F, 0x0F, 0x0F, 0x00, 0x0F, 0x0F, 0x0F, 0x0F, 0x00,
            0x00, 0x00, 0x00, 0x0F, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x0F, 0x0F, 0x00, 0x0F,
            0x0F, 0x0F, 0x00, 0x00, 0x00, 0x0F, 0x0F, 0x00, 0x00, 0x00, 0x0F, 0x0F, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        };

        private byte[] m_KeyImg0 = new byte[6]
        {
            2,    2,
            0x2A, 0x2A,
            0x2A, 0x2A,
        };

        private byte[] m_KeyImg1 = new byte[8]
        {
            3, 3,
            0x2A, 0x2A, 0x2A,
            0x2A, 0x2A, 0x2A,
        };

        private byte[] m_KeyImg2 = new byte[18]
        {
            4, 4,
            0xFF, 0x2A, 0x2A, 0xFF,
            0x2A, 0x2A, 0x2A, 0x2A,
            0x2A, 0x2A, 0x2A, 0x2A,
            0xFF, 0x2A, 0x2A, 0xFF,
        };

        private byte[] Font6x8 = new byte[768]
        {
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x20,0x20,0x20,0x20,0x20,0x00,0x20,0x00,
            0x50,0x50,0x00,0x00,0x00,0x00,0x00,0x00,0x50,0x50,0xF8,0x50,0xF8,0x50,0x50,0x00,
            0x20,0x78,0xA0,0x70,0x28,0xF0,0x20,0x00,0x48,0xB0,0x50,0x20,0x50,0x68,0x90,0x00,
            0x40,0xA0,0xA8,0x68,0x90,0x90,0x68,0x00,0x30,0x20,0x00,0x00,0x00,0x00,0x00,0x00,
            0x10,0x20,0x40,0x40,0x40,0x20,0x10,0x00,0x40,0x20,0x10,0x10,0x10,0x20,0x40,0x00,
            0x00,0x88,0x50,0x20,0x50,0x88,0x00,0x00,0x00,0x20,0x20,0xF8,0x20,0x20,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x20,0x40,0x00,0x00,0x00,0x00,0xF8,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x20,0x00,0x08,0x10,0x10,0x20,0x40,0x40,0x80,0x00,
            0x70,0x88,0x98,0xA8,0xC8,0x88,0x70,0x00,0x20,0x60,0x20,0x20,0x20,0x20,0xF8,0x00,
            0x70,0x88,0x08,0x30,0x40,0x80,0xF8,0x00,0x70,0x88,0x08,0x30,0x08,0x88,0x70,0x00,
            0x30,0x50,0x90,0x90,0xF8,0x10,0x10,0x00,0xF8,0x80,0x80,0xF0,0x08,0x08,0xF0,0x00,
            0x70,0x88,0x80,0xF0,0x88,0x88,0x70,0x00,0xF8,0x08,0x10,0x10,0x20,0x20,0x20,0x00,
            0x70,0x88,0x88,0x70,0x88,0x88,0x70,0x00,0x70,0x88,0x88,0x78,0x08,0x88,0x70,0x00,
            0x00,0x20,0x00,0x00,0x00,0x20,0x00,0x00,0x00,0x20,0x00,0x00,0x00,0x20,0x40,0x00,
            0x10,0x20,0x40,0x80,0x40,0x20,0x10,0x00,0x00,0x00,0xF8,0x00,0xF8,0x00,0x00,0x00,
            0x40,0x20,0x10,0x08,0x10,0x20,0x40,0x00,0x70,0x88,0x08,0x10,0x20,0x00,0x20,0x00,
            0x30,0x48,0x88,0x98,0xA8,0xA8,0x78,0x00,0x20,0x50,0x50,0x88,0xF8,0x88,0x88,0x00,
            0xF0,0x88,0x88,0xF0,0x88,0x88,0xF0,0x00,0x70,0x88,0x80,0x80,0x80,0x88,0x70,0x00,
            0xF0,0x88,0x88,0x88,0x88,0x88,0xF0,0x00,0xF8,0x80,0x80,0xF0,0x80,0x80,0xF8,0x00,
            0xF8,0x80,0x80,0xF0,0x80,0x80,0x80,0x00,0x70,0x88,0x80,0xB8,0x88,0x88,0x70,0x00,
            0x88,0x88,0x88,0xF8,0x88,0x88,0x88,0x00,0xF8,0x20,0x20,0x20,0x20,0x20,0xF8,0x00,
            0x38,0x08,0x08,0x08,0x08,0x88,0x70,0x00,0x88,0x88,0x90,0xE0,0x90,0x88,0x88,0x00,
            0x80,0x80,0x80,0x80,0x80,0x80,0xF8,0x00,0x88,0xD8,0xA8,0xA8,0xA8,0xA8,0xA8,0x00,
            0x88,0xC8,0xA8,0xA8,0xA8,0x98,0x88,0x00,0x70,0x88,0x88,0x88,0x88,0x88,0x70,0x00,
            0xF0,0x88,0x88,0xF0,0x80,0x80,0x80,0x00,0x70,0x88,0x88,0x88,0xA8,0x90,0x68,0x00,
            0xF0,0x88,0x88,0xF0,0x88,0x88,0x88,0x00,0x70,0x88,0x80,0x70,0x08,0x88,0x70,0x00,
            0xF8,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x88,0x88,0x88,0x88,0x88,0x88,0x70,0x00,
            0x88,0x88,0x88,0x50,0x50,0x50,0x20,0x00,0x88,0xA8,0xA8,0xA8,0xA8,0xD8,0x88,0x00,
            0x88,0x88,0x50,0x20,0x50,0x88,0x88,0x00,0x88,0x88,0x88,0x70,0x20,0x20,0x20,0x00,
            0xF8,0x08,0x10,0x20,0x40,0x80,0xF8,0x00,0x70,0x40,0x40,0x40,0x40,0x40,0x70,0x00,
            0x88,0x50,0xF8,0x20,0xF8,0x20,0x20,0x00,0x70,0x10,0x10,0x10,0x10,0x10,0x70,0x00,
            0x20,0x50,0x88,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF8,0x00,
            0x80,0xC0,0xE0,0xF0,0xE0,0xC0,0x80,0x00,0x00,0x00,0x70,0x08,0x78,0x88,0xF8,0x00,
            0x80,0x80,0x80,0xF0,0x88,0x88,0xF0,0x00,0x00,0x00,0x78,0x80,0x80,0x80,0x78,0x00,
            0x08,0x08,0x08,0x78,0x88,0x88,0x78,0x00,0x00,0x00,0x70,0x88,0xF8,0x80,0x78,0x00,
            0x18,0x20,0xF8,0x20,0x20,0x20,0x20,0x00,0x00,0x00,0x78,0x88,0x78,0x08,0xF0,0x00,
            0x80,0x80,0x80,0xF0,0x88,0x88,0x88,0x00,0x20,0x00,0x20,0x20,0x20,0x20,0x20,0x00,
            0x20,0x00,0x20,0x20,0x20,0x20,0xC0,0x00,0x80,0x80,0x88,0x90,0xE0,0x90,0x88,0x00,
            0x20,0x20,0x20,0x20,0x20,0x20,0x30,0x00,0x00,0x00,0xF0,0xA8,0xA8,0xA8,0xA8,0x00,
            0x00,0x00,0xF0,0x88,0x88,0x88,0x88,0x00,0x00,0x00,0x70,0x88,0x88,0x88,0x70,0x00,
            0x00,0x00,0xF0,0x88,0xF0,0x80,0x80,0x00,0x00,0x00,0x78,0x88,0x78,0x08,0x08,0x00,
            0x00,0x00,0xB8,0xC0,0x80,0x80,0x80,0x00,0x00,0x00,0x78,0x80,0x70,0x08,0xF0,0x00,
            0x20,0x20,0xF8,0x20,0x20,0x20,0x20,0x00,0x00,0x00,0x88,0x88,0x88,0x88,0x70,0x00,
            0x00,0x00,0x88,0x88,0x50,0x50,0x20,0x00,0x00,0x00,0x88,0xA8,0xA8,0xD8,0x88,0x00,
            0x00,0x00,0x88,0x50,0x20,0x50,0x88,0x00,0x00,0x00,0x88,0x88,0x78,0x08,0xF0,0x00,
            0x00,0x00,0xF8,0x08,0x70,0x80,0xF8,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
        };

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

            nescfg = NesConfig.NESCONFIG_NTSC;

            CheatInitial();

            try
            {
                Debuger.Log("Allocating CPU...");
                cpu = new CPU(this);

                Debuger.Log("Allocating PPU...");
                ppu = new PPU(this);

                var screenBuffer = new byte[PPU.SCREEN_WIDTH * PPU.SCREEN_HEIGHT];
                var colormode = new byte[PPU.SCREEN_HEIGHT];

                ppu.SetScreenPtr(screenBuffer, colormode);

                Debuger.Log("Allocating APU...");
                apu = new APU(this);

                Debuger.Log("Allocating PAD...");
                pad = new PAD(this);

                Debuger.Log("Loading ROM Image...");
                rom = new ROM(fname);

                mapper = Mapper.CreateMapper(this, rom.GetMapperNo());

                Debuger.Log("OK");

                Debuger.Log($"{rom.GetRomName()}");
                Debuger.Log($"Mapper        : #{rom.GetMapperNo():D3}");
                Debuger.Log($"PRG SIZE      : {16 * rom.GetPROM_SIZE():4:0000}K");
                Debuger.Log($"CHR SIZE      : {8 * rom.GetVROM_SIZE():4:0000}K");

                Debuger.Log($"V MIRROR      :{rom.IsVMIRROR()}");
                Debuger.Log($"4 SCREEN      :{rom.Is4SCREEN()}");
                Debuger.Log($"SAVE RAM      :{rom.IsSAVERAM()}");
                Debuger.Log($"TRAINER       :{rom.IsTRAINER()}");
                Debuger.Log($"VS-Unisystem  :{rom.IsVSUNISYSTEM()}");

                NesSub_MemoryInitial();
                LoadSRAM();
                LoadDISK();

                {
                    // Padクラス内だと初期化タイミングが遅いのでここで
                    uint crc = rom.GetPROM_CRC();
                    if (
                        crc == 0xe792de94       // Best Play - Pro Yakyuu (New) (J)
                        || crc == 0xf79d684a        // Best Play - Pro Yakyuu (Old) (J)
                        || crc == 0xc2ef3422        // Best Play - Pro Yakyuu 2 (J)
                        || crc == 0x974e8840        // Best Play - Pro Yakyuu '90 (J)
                        || crc == 0xb8747abf        // Best Play - Pro Yakyuu Special (J)
                        || crc == 0x9fa1c11f        // Castle Excellent (J)
                        || crc == 0x0b0d4d1b        // Derby Stallion - Zenkoku Ban (J)
                        || crc == 0x728c3d98        // Downtown - Nekketsu Monogatari (J)
                        || crc == 0xd68a6f33        // Dungeon Kid (J)
                        || crc == 0x3a51eb04        // Fleet Commander (J)
                        || crc == 0x7c46998b        // Haja no Fuuin (J)
                        || crc == 0x7e5d2f1a        // Itadaki Street - Watashi no Mise ni Yottette (J)
                        || crc == 0xcee5857b        // Ninjara Hoi! (J)
                        || crc == 0x50ec5e8b        // Wizardry - Legacy of Llylgamyn (J)
                        || crc == 0x343e9146        // Wizardry - Proving Grounds of the Mad Overlord (J)
                        || crc == 0x33d07e45)
                    {   // Wizardry - The Knight of Diamonds (J)
                        pad.SetExController(EXCONTROLLER.EXCONTROLLER_TURBOFILE);
                    }
                }

                LoadTurboFile();

                // VS-Unisystemのデフォルト設定
                if (rom.IsVSUNISYSTEM())
                {
                    uint crc = rom.GetPROM_CRC();

                    m_VSDipValue = 0;
                    m_VSDipTable = VsUnisystem.vsdip_default;
                }

                Reset();

                // ゲーム固有のデフォルトオプションを設定(設定戻す時に使う為)
                GameOption.defRenderMethod = (int)GetRenderMethod();
                GameOption.defIRQtype = GetIrqType();
                GameOption.defFrameIRQ = GetFrameIRQmode();
                GameOption.defVideoMode = GetVideoMode();

                // 設定をロードして設定する(エントリが無ければデフォルトが入る)
                if (rom.GetMapperNo() != 20)
                {
                    GameOption.Load(rom.GetPROM_CRC());
                }
                else
                {
                    GameOption.Load(rom.GetGameID(), rom.GetMakerID());
                }

                SetRenderMethod((EnumRenderMethod)GameOption.nRenderMethod);
                SetIrqType((IRQMETHOD)GameOption.nIRQtype);
                SetFrameIRQmode(GameOption.bFrameIRQ);
                SetVideoMode(GameOption.bVideoMode);
            }
            catch (Exception ex)
            {
                Debuger.LogError(ex.ToString());
                throw ex;
            }
        }

        private int GetIrqType()
        {
            return nIRQtype;
        }

        private void LoadTurboFile()
        {
            MemoryUtility.ZEROMEMORY(MMU.ERAM, MMU.ERAM.Length);

            if (pad.GetExController() != (int)EXCONTROLLER.EXCONTROLLER_TURBOFILE)
                return;

            var fp = Supporter.OpenFile(Supporter.Config.path.szSavePath, "TurboFile.vtf");
            try
            {
                if (fp == null)
                {
                    // xxx ファイルを開けません
                    throw new Exception($"Can Not Open File [TurboFile.vtf]");
                }

                long size = fp.Length;
                // ファイルサイズ取得
                if (size > 32 * 1024)
                {
                    size = 32 * 1024;
                }

                fp.Read(MMU.ERAM, 0, MMU.ERAM.Length);
                fp.Close();
            }
            catch (Exception ex)
            {
                fp?.Close();
                Debuger.LogError($"Loading TurboFile Error.\n{ex}");
            }
        }

        private void LoadDISK()
        {
            //todo : 磁碟机读取支持
        }

        private void LoadSRAM()
        {
            if (rom.IsNSF())
                return;

            MemoryUtility.ZEROMEMORY(MMU.WRAM, MMU.WRAM.Length);

            if (!rom.IsSAVERAM())
                return;

            var saveFileDir = Supporter.Config.path.szSavePath;
            var saveFileName = $"{rom.GetRomName()}.sav";

            var fp = Supporter.OpenFile(saveFileDir, saveFileName);

            try
            {
                if (fp == null)
                {
                    throw new Exception("not find ram file to read");
                }

                Debuger.Log("Loading SAVERAM...");

                int size = (int)fp.Length;
                if (size <= 128 * 1024)
                {
                    fp.Read(MMU.WRAM, 0, size);
                }
                Debuger.Log("OK.");
                fp.Close();
            }
            catch (Exception ex)
            {
                fp?.Close();
                fp = null;
            }
        }

        private void NesSub_MemoryInitial()
        {
            int i;

            // 儊儌儕僋儕傾
            MemoryUtility.ZEROMEMORY(MMU.RAM, MMU.RAM.Length);
            MemoryUtility.ZEROMEMORY(MMU.WRAM, MMU.WRAM.Length);
            MemoryUtility.ZEROMEMORY(MMU.DRAM, MMU.DRAM.Length);
            MemoryUtility.ZEROMEMORY(MMU.ERAM, MMU.ERAM.Length);
            MemoryUtility.ZEROMEMORY(MMU.XRAM, MMU.XRAM.Length);
            MemoryUtility.ZEROMEMORY(MMU.CRAM, MMU.CRAM.Length);
            MemoryUtility.ZEROMEMORY(MMU.VRAM, MMU.VRAM.Length);

            MemoryUtility.ZEROMEMORY(MMU.SPRAM, MMU.SPRAM.Length);
            MemoryUtility.ZEROMEMORY(MMU.BGPAL, MMU.BGPAL.Length);
            MemoryUtility.ZEROMEMORY(MMU.SPPAL, MMU.SPPAL.Length);

            MemoryUtility.ZEROMEMORY(MMU.CPUREG, MMU.CPUREG.Length);
            MemoryUtility.ZEROMEMORY(MMU.PPUREG, MMU.PPUREG.Length);

            MMU.FrameIRQ = 0xC0;

            MMU.PROM = MMU.VROM = null;

            // 0 彍嶼杊巭懳嶔
            MMU.PROM_8K_SIZE = MMU.PROM_16K_SIZE = MMU.PROM_32K_SIZE = 1;
            MMU.VROM_1K_SIZE = MMU.VROM_2K_SIZE = MMU.VROM_4K_SIZE = MMU.VROM_8K_SIZE = 1;

            // 僨僼僅儖僩僶儞僋愝掕
            for (i = 0; i < 8; i++)
            {
                MMU.CPU_MEM_BANK[i] = null;
                MMU.CPU_MEM_TYPE[i] = MMU.BANKTYPE_ROM;
                MMU.CPU_MEM_PAGE[i] = 0;
            }

            // 撪憻RAM/WRAM
            MMU.SetPROM_Bank(0, MMU.RAM, MMU.BANKTYPE_RAM);
            MMU.SetPROM_Bank(3, MMU.WRAM, MMU.BANKTYPE_RAM);

            // 僟儈乕
            MMU.SetPROM_Bank(1, MMU.XRAM, MMU.BANKTYPE_ROM);
            MMU.SetPROM_Bank(2, MMU.XRAM, MMU.BANKTYPE_ROM);

            for (i = 0; i < 8; i++)
            {
                MMU.CRAM_USED[i] = 0;
            }
        }

        public void Command(NESCOMMAND cmd)
        {
            CommandParam(cmd, 0);
        }

        public bool CommandParam(NESCOMMAND cmd, int param)
        {
            switch (cmd)
            {
                case NESCOMMAND.NESCMD_NONE: break;
                case NESCOMMAND.NESCMD_HWRESET:
                    Reset();
                    m_CommandRequest = (int)cmd;
                    break;
                case NESCOMMAND.NESCMD_SWRESET:
                    SoftReset();
                    m_CommandRequest = (int)cmd;
                    break;
                default:
                    throw new NotImplementedException($"{cmd} not impl right now");
            }

            return true;
        }

        public void CheatInitial()
        {
            m_CheatCode.Clear();
        }

        private int FrameCount = 0;
        public void EmulateFrame(bool bDraw)
        {
            FrameCount++;

            int scanline = 0;
            if (rom.IsNSF())
            {
                EmulateNSF();
                return;
            }

            CheatCodeProcess();

            NES_scanline = scanline;
            bool NotTile = RenderMethod != EnumRenderMethod.TILE_RENDER;

            if (RenderMethod != EnumRenderMethod.TILE_RENDER)
            {
                bZapper = false;
                while (true)
                {
                    ppu.SetRenderScanline(scanline);

                    if (scanline == 0)
                    {
                        if (RenderMethod < EnumRenderMethod.POST_RENDER)
                        {
                            EmulationCPU(nescfg.ScanlineCycles);
                            ppu.FrameStart();
                            ppu.ScanlineNext();
                            mapper.HSync(scanline);
                            ppu.ScanlineStart();
                        }
                        else
                        {
                            EmulationCPU(nescfg.HDrawCycles);
                            ppu.FrameStart();
                            ppu.ScanlineNext();
                            mapper.HSync(scanline);
                            EmulationCPU(FETCH_CYCLES * 32);
                            ppu.ScanlineStart();
                            EmulationCPU(FETCH_CYCLES * 10 + nescfg.ScanlineEndCycles);
                        }
                    }
                    else if (scanline < 240)
                    {
                        if (RenderMethod < EnumRenderMethod.POST_RENDER)
                        {
                            if (RenderMethod == EnumRenderMethod.POST_ALL_RENDER)
                                EmulationCPU(nescfg.ScanlineCycles);
                            if (bDraw)
                            {
                                ppu.Scanline(scanline, Supporter.Config.graphics.bAllSprite, Supporter.Config.graphics.bLeftClip);
                            }
                            else
                            {
                                if (pad.IsZapperMode() && scanline == ZapperY)
                                {
                                    ppu.Scanline(scanline, Supporter.Config.graphics.bAllSprite, Supporter.Config.graphics.bLeftClip);
                                }
                                else
                                {
                                    if (!ppu.IsSprite0(scanline))
                                    {
                                        ppu.DummyScanline(scanline);
                                    }
                                    else
                                    {
                                        ppu.Scanline(scanline, Supporter.Config.graphics.bAllSprite, Supporter.Config.graphics.bLeftClip);
                                    }
                                }
                            }
                            ppu.ScanlineNext();                // 偙傟偺埵抲偱儔僗僞乕宯偼夋柺偑堘偆
                            if (RenderMethod == EnumRenderMethod.PRE_ALL_RENDER)
                                EmulationCPU(nescfg.ScanlineCycles);

                            mapper.HSync(scanline);
                            ppu.ScanlineStart();
                        }
                        else
                        {
                            if (RenderMethod == EnumRenderMethod.POST_RENDER)
                                EmulationCPU(nescfg.HDrawCycles);
                            if (bDraw)
                            {
                                ppu.Scanline(scanline, Supporter.Config.graphics.bAllSprite, Supporter.Config.graphics.bLeftClip);
                            }
                            else
                            {
                                if (pad.IsZapperMode() && scanline == ZapperY)
                                {
                                    ppu.Scanline(scanline, Supporter.Config.graphics.bAllSprite, Supporter.Config.graphics.bLeftClip);
                                }
                                else
                                {
                                    if (!ppu.IsSprite0(scanline))
                                    {
                                        ppu.DummyScanline(scanline);
                                    }
                                    else
                                    {
                                        ppu.Scanline(scanline, Supporter.Config.graphics.bAllSprite, Supporter.Config.graphics.bLeftClip);
                                    }
                                }
                            }
                            if (RenderMethod == EnumRenderMethod.PRE_RENDER)
                                EmulationCPU(nescfg.HDrawCycles);
                            ppu.ScanlineNext();
                            mapper.HSync(scanline);
                            EmulationCPU(FETCH_CYCLES * 32);
                            ppu.ScanlineStart();
                            EmulationCPU(FETCH_CYCLES * 10 + nescfg.ScanlineEndCycles);
                        }
                    }
                    else if (scanline == 240)
                    {
                        mapper.VSync();
                        if (RenderMethod < EnumRenderMethod.POST_RENDER)
                        {
                            EmulationCPU(nescfg.ScanlineCycles);
                            mapper.HSync(scanline);
                        }
                        else
                        {
                            EmulationCPU(nescfg.HDrawCycles);
                            mapper.HSync(scanline);
                            EmulationCPU(nescfg.HBlankCycles);
                        }
                    }
                    else if (scanline <= nescfg.TotalScanlines - 1)
                    {
                        pad.VSync();

                        // VBLANK婜娫
                        if (scanline == nescfg.TotalScanlines - 1)
                        {
                            ppu.VBlankEnd();
                        }
                        if (RenderMethod < EnumRenderMethod.POST_RENDER)
                        {
                            if (scanline == 241)
                            {
                                ppu.VBlankStart();
                                if ((MMU.PPUREG[0] & PPU.PPU_VBLANK_BIT) != 0)
                                {
                                    cpu.NMI();
                                }
                            }
                            EmulationCPU(nescfg.ScanlineCycles);
                            mapper.HSync(scanline);
                        }
                        else
                        {
                            if (scanline == 241)
                            {
                                ppu.VBlankStart();
                                if ((MMU.PPUREG[0] & PPU.PPU_VBLANK_BIT) != 0)
                                {
                                    cpu.NMI();
                                }
                            }
                            EmulationCPU(nescfg.HDrawCycles);
                            mapper.HSync(scanline);
                            EmulationCPU(nescfg.HBlankCycles);
                        }

                        if (scanline == nescfg.TotalScanlines - 1)
                        {
                            break;
                        }
                    }
                    if (pad.IsZapperMode())
                    {
                        if (scanline == ZapperY)
                            bZapper = true;
                        else
                            bZapper = false;
                    }

                    scanline++;
                    NES_scanline = scanline;
                }
            }
            else
            {
                bZapper = false;
                while (true)
                {
                    ppu.SetRenderScanline(scanline);

                    if (scanline == 0)
                    {
                        // 僟儈乕僗僉儍儞儔僀儞
                        // H-Draw (4fetches*32)
                        EmulationCPU(FETCH_CYCLES * 128);
                        ppu.FrameStart();
                        ppu.ScanlineNext();
                        EmulationCPU(FETCH_CYCLES * 10);
                        mapper.HSync(scanline);
                        EmulationCPU(FETCH_CYCLES * 22);
                        ppu.ScanlineStart();
                        EmulationCPU(FETCH_CYCLES * 10 + nescfg.ScanlineEndCycles);
                    }
                    else if (scanline < 240)
                    {
                        // 僗僋儕乕儞昤夋(Scanline 1乣239)
                        if (bDraw)
                        {
                            ppu.Scanline(scanline, Supporter.Config.graphics.bAllSprite, Supporter.Config.graphics.bLeftClip);
                            ppu.ScanlineNext();
                            EmulationCPU(FETCH_CYCLES * 10);
                            mapper.HSync(scanline);
                            EmulationCPU(FETCH_CYCLES * 22);
                            ppu.ScanlineStart();
                            EmulationCPU(FETCH_CYCLES * 10 + nescfg.ScanlineEndCycles);
                        }
                        else
                        {
                            if (pad.IsZapperMode() && scanline == ZapperY)
                            {
                                ppu.Scanline(scanline, Supporter.Config.graphics.bAllSprite, Supporter.Config.graphics.bLeftClip);
                                ppu.ScanlineNext();
                                EmulationCPU(FETCH_CYCLES * 10);
                                mapper.HSync(scanline);
                                EmulationCPU(FETCH_CYCLES * 22);
                                ppu.ScanlineStart();
                                EmulationCPU(FETCH_CYCLES * 10 + nescfg.ScanlineEndCycles);
                            }
                            else
                            {
                                if (!ppu.IsSprite0(scanline))
                                {
                                    // H-Draw (4fetches*32)
                                    EmulationCPU(FETCH_CYCLES * 128);
                                    ppu.DummyScanline(scanline);
                                    ppu.ScanlineNext();
                                    EmulationCPU(FETCH_CYCLES * 10);
                                    mapper.HSync(scanline);
                                    EmulationCPU(FETCH_CYCLES * 22);
                                    ppu.ScanlineStart();
                                    EmulationCPU(FETCH_CYCLES * 10 + nescfg.ScanlineEndCycles);
                                }
                                else
                                {
                                    ppu.Scanline(scanline, Supporter.Config.graphics.bAllSprite, Supporter.Config.graphics.bLeftClip);
                                    ppu.ScanlineNext();
                                    EmulationCPU(FETCH_CYCLES * 10);
                                    mapper.HSync(scanline);
                                    EmulationCPU(FETCH_CYCLES * 22);
                                    ppu.ScanlineStart();
                                    EmulationCPU(FETCH_CYCLES * 10 + nescfg.ScanlineEndCycles);
                                }
                            }
                        }
                    }
                    else if (scanline == 240)
                    {
                        // 僟儈乕僗僉儍儞儔僀儞 (Scanline 240)
                        mapper.VSync();

                        EmulationCPU(nescfg.HDrawCycles);
                        // H-Sync
                        mapper.HSync(scanline);

                        EmulationCPU(nescfg.HBlankCycles);
                    }
                    else if (scanline <= nescfg.TotalScanlines - 1)
                    {
                        pad.VSync();

                        // VBLANK婜娫
                        if (scanline == nescfg.TotalScanlines - 1)
                        {
                            ppu.VBlankEnd();
                        }
                        if (scanline == 241)
                        {
                            ppu.VBlankStart();
                            if ((MMU.PPUREG[0] & PPU.PPU_VBLANK_BIT) != 0)
                            {
                                cpu.NMI();
                            }
                        }
                        EmulationCPU(nescfg.HDrawCycles);

                        // H-Sync
                        mapper.HSync(scanline);

                        EmulationCPU(nescfg.HBlankCycles);

                        if (scanline == nescfg.TotalScanlines - 1)
                        {
                            break;
                        }
                    }
                    if (pad.IsZapperMode())
                    {
                        if (scanline == ZapperY)
                            bZapper = true;
                        else
                            bZapper = false;
                    }

                    scanline++;
                    NES_scanline = scanline;
                }
            }

            if (bDraw)
            {
                DrawPad();
            }
        }

        private void DrawPad()
        {
            if (m_bMoviePlay)
            {
                int offset_h = 12;
                int offset_v = Supporter.Config.graphics.bAllLine ? (240 - 18) : (240 - 22);

                if (Supporter.Config.movie.bPadDisplay)
                {
                    uint dwData = pad.GetSyncData();
                    for (int i = 0; i < 4; i++)
                    {
                        byte Data = (byte)(dwData >> (i * 8));
                        if ((m_MovieControl & (1 << i)) != 0)
                        {
                            DrawBitmap(offset_h, offset_v, m_PadImg);

                            // KEY
                            if ((Data & (1 << 4)) != 0) DrawBitmap(offset_h + 3, offset_v + 1, m_KeyImg0); // U
                            if ((Data & (1 << 5)) != 0) DrawBitmap(offset_h + 3, offset_v + 5, m_KeyImg0); // D
                            if ((Data & (1 << 6)) != 0) DrawBitmap(offset_h + 1, offset_v + 3, m_KeyImg0); // L
                            if ((Data & (1 << 7)) != 0) DrawBitmap(offset_h + 5, offset_v + 3, m_KeyImg0); // R

                            // START,SELECT
                            if ((Data & (1 << 2)) != 0) DrawBitmap(offset_h + 9, offset_v + 5, m_KeyImg1); // SELECT
                            if ((Data & (1 << 3)) != 0) DrawBitmap(offset_h + 13, offset_v + 5, m_KeyImg1); // START

                            // A,B
                            if ((Data & (1 << 0)) != 0) DrawBitmap(offset_h + 23, offset_v + 3, m_KeyImg2); // A
                            if ((Data & (1 << 1)) != 0) DrawBitmap(offset_h + 18, offset_v + 3, m_KeyImg2); // B

                            offset_h += 30;
                        }
                    }
                }

                if (Supporter.Config.movie.bTimeDisplay)
                {
                    // Time display
                    int t = m_MovieStep;
                    int h = t / 216000;
                    t -= h * 216000;
                    int m = t / 3600;
                    t -= m * 3600;
                    int s = t / 60;
                    t -= s * 60;

                    string szTemp = $"{h:00}:{m:00}:{s:00}.{t * 100 / 60:00}";
                    DrawString(256 - 80 + 0, offset_v - 1, szTemp, 0x1F);
                    DrawString(256 - 80 + 0, offset_v + 1, szTemp, 0x1F);
                    DrawString(256 - 80 - 1, offset_v + 0, szTemp, 0x1F);
                    DrawString(256 - 80 + 1, offset_v + 0, szTemp, 0x1F);
                    DrawString(256 - 80, offset_v, szTemp, 0x30);
                }
            }
        }

        internal void DrawString(int x, int y, string str, byte col)
        {
            foreach (var @char in str)
            {
                DrawFont(x, y, (byte)@char, col);
                x += 6;
            }
        }

        internal void DrawFont(int x, int y, byte chr, byte col)
        {
            int i;
            int pFnt;
            int pPtr;
            var Scn = ppu.GetScreenPtr();
            int pScn = 8;


            if (chr < 0x20 || chr > 0x7F)
                return;
            chr -= 0x20;
            pFnt = chr * 8;
            pPtr = pScn + (256 + 16) * y + x;
            for (i = 0; i < 8; i++)
            {
                if ((Font6x8[pFnt + i] & 0x80) != 0) Scn[pPtr + 0] = col;
                if ((Font6x8[pFnt + i] & 0x40) != 0) Scn[pPtr + 1] = col;
                if ((Font6x8[pFnt + i] & 0x20) != 0) Scn[pPtr + 2] = col;
                if ((Font6x8[pFnt + i] & 0x10) != 0) Scn[pPtr + 3] = col;
                if ((Font6x8[pFnt + i] & 0x08) != 0) Scn[pPtr + 4] = col;
                if ((Font6x8[pFnt + i] & 0x04) != 0) Scn[pPtr + 5] = col;
                pPtr += (256 + 16);
            }
        }

        private void DrawBitmap(int x, int y, byte[] bitMap)
        {
            int i, j;
            int h, v;
            var Scn = ppu.GetScreenPtr();
            int pScn = 8 + (256 + 16) * y + x;
            int pPtr;

            int lpBitmap = 0;
            h = bitMap[lpBitmap++];
            v = bitMap[lpBitmap++];

            for (j = 0; j < v; j++)
            {
                pPtr = pScn;
                for (i = 0; i < h; i++)
                {
                    if (bitMap[lpBitmap] != 0xFF)
                    {
                        Scn[pPtr] = bitMap[lpBitmap];
                    }
                    lpBitmap++;
                    pPtr++;
                }
                pScn += 256 + 16;
            }
        }

        int CPU_CALL_COUNT = 0;
        internal void EmulationCPU(int basecycles)
        {
            int cycles;

            base_cycles += basecycles;
            cycles = (int)((base_cycles / 12) - emul_cycles);

            if (cycles > 0)
            {
                var cycleAdd = cpu.EXEC(cycles);
                emul_cycles += cycleAdd;
            }

            CPU_CALL_COUNT++;
        }

        internal void Reset()
        {
            SaveSRAM();
            SaveDISK();
            SaveTurboFile();

            // RAM Clear
            MemoryUtility.ZEROMEMORY(MMU.RAM, MMU.RAM.Length);
            if (rom.GetPROM_CRC() == 0x29401686)
            {   // Minna no Taabou no Nakayoshi Dai Sakusen(J)
                MemoryUtility.memset(MMU.RAM, 0xFF, MMU.RAM.Length);
            }

            // RAM set
            if (!rom.IsSAVERAM() && rom.GetMapperNo() != 20)
            {
                MemoryUtility.memset(MMU.WRAM, 0xFF, MMU.WRAM.Length);
            }

            MemoryUtility.ZEROMEMORY(MMU.CRAM, MMU.CRAM.Length);
            MemoryUtility.ZEROMEMORY(MMU.VRAM, MMU.VRAM.Length);

            MemoryUtility.ZEROMEMORY(MMU.SPRAM, MMU.SPRAM.Length);
            MemoryUtility.ZEROMEMORY(MMU.BGPAL, MMU.BGPAL.Length);
            MemoryUtility.ZEROMEMORY(MMU.SPPAL, MMU.SPPAL.Length);

            MemoryUtility.ZEROMEMORY(MMU.CPUREG, MMU.CPUREG.Length);
            MemoryUtility.ZEROMEMORY(MMU.PPUREG, MMU.PPUREG.Length);

            m_bDiskThrottle = false;

            SetRenderMethod(EnumRenderMethod.PRE_RENDER);

            if (rom.IsPAL())
            {
                SetVideoMode(true);
            }

            MMU.PROM = rom.GetPROM();
            MMU.VROM = rom.GetVROM();

            MMU.PROM_8K_SIZE = rom.GetPROM_SIZE() * 2;
            MMU.PROM_16K_SIZE = rom.GetPROM_SIZE();
            MMU.PROM_32K_SIZE = rom.GetPROM_SIZE() / 2;

            MMU.VROM_1K_SIZE = rom.GetVROM_SIZE() * 8;
            MMU.VROM_2K_SIZE = rom.GetVROM_SIZE() * 4;
            MMU.VROM_4K_SIZE = rom.GetVROM_SIZE() * 2;
            MMU.VROM_8K_SIZE = rom.GetVROM_SIZE();

            // 僨僼僅儖僩僶儞僋
            if (MMU.VROM_8K_SIZE != 0)
            {
                MMU.SetVROM_8K_Bank(0);
            }
            else
            {
                MMU.SetCRAM_8K_Bank(0);
            }

            // 儈儔乕
            if (rom.Is4SCREEN())
            {
                MMU.SetVRAM_Mirror(MMU.VRAM_MIRROR4);
            }
            else if (rom.IsVMIRROR())
            {
                MMU.SetVRAM_Mirror(MMU.VRAM_VMIRROR);
            }
            else
            {
                MMU.SetVRAM_Mirror(MMU.VRAM_HMIRROR);
            }

            apu.SelectExSound(0);

            ppu.Reset();
            mapper.Reset();

            // Trainer
            if (rom.IsTRAINER())
            {
                Array.Copy(rom.GetTRAINER(), 0, MMU.WRAM, 0x1000, 512);
            }

            pad.Reset();
            cpu.Reset();
            apu.Reset();

            if (rom.IsNSF())
            {
                mapper.Reset();
            }

            base_cycles = emul_cycles = 0;
        }

        internal void SetVideoMode(bool bMode)
        {
            bVideoMode = bMode;
            if (!bVideoMode)
            {
                nescfg = NesConfig.NESCONFIG_NTSC;
            }
            else
            {
                nescfg = NesConfig.NESCONFIG_PAL;
            }
            apu.SoundSetup();
        }



        internal void SoftReset()
        {
            pad.Reset();
            cpu.Reset();
            apu.Reset();

            if (rom.IsNSF())
            {
                mapper.Reset();
            }

            m_bDiskThrottle = false;

            base_cycles = emul_cycles = 0;
        }

        internal void EmulateNSF()
        {
            R6502 reg = null;

            ppu.Reset();
            mapper.VSync();

            //DEBUGOUT( "Frame\n" );

            if (m_bNsfPlaying)
            {
                if (m_bNsfInit)
                {
                    MemoryUtility.ZEROMEMORY(MMU.RAM, MMU.RAM.Length);
                    if ((rom.GetNsfHeader().ExtraChipSelect & 0x04) == 0)
                    {
                        MemoryUtility.ZEROMEMORY(MMU.RAM, 0x2000);
                    }

                    apu.Reset();
                    apu.Write(0x4015, 0x0F);
                    apu.Write(0x4017, 0xC0);
                    apu.ExWrite(0x4080, 0x80); // FDS Volume 0
                    apu.ExWrite(0x408A, 0xE8); // FDS Envelope Speed

                    cpu.GetContext(ref reg);
                    reg.PC = 0x4710;    // Init Address
                    reg.A = (byte)m_nNsfSongNo;
                    reg.X = (byte)m_nNsfSongMode;
                    reg.Y = 0;
                    reg.S = 0xFF;
                    reg.P = CPU.Z_FLAG | CPU.R_FLAG | CPU.I_FLAG;

                    // 埨慡懳嶔傪寭偹偰偁偊偰儖乕僾偵(1昩暘)
                    for (int i = 0; i < nescfg.TotalScanlines * 60; i++)
                    {
                        EmulationCPU(nescfg.ScanlineCycles);
                        cpu.GetContext(ref reg);

                        // 柍尷儖乕僾偵擖偭偨偙偲傪妋擣偟偨傜敳偗傞
                        if (reg.PC == 0x4700)
                        {
                            break;
                        }
                    }

                    m_bNsfInit = false;
                }

                cpu.GetContext(ref reg);
                // 柍尷儖乕僾偵擖偭偰偄偨傜嵞愝掕偡傞
                if (reg.PC == 0x4700)
                {
                    reg.PC = 0x4720;    // Play Address
                    reg.A = 0;
                    reg.S = 0xFF;
                }

                for (int i = 0; i < nescfg.TotalScanlines; i++)
                {
                    EmulationCPU(nescfg.ScanlineCycles);
                }
            }
            else
            {
                cpu.GetContext(ref reg);
                reg.PC = 0x4700;    // 柍尷儖乕僾
                reg.S = 0xFF;

                EmulationCPU(nescfg.ScanlineCycles * nescfg.TotalScanlines);
            }
        }

        internal void CheatCodeProcess()
        {
            foreach (var it in m_CheatCode)
            {
                if ((it.enable & CHEATCODE.CHEAT_ENABLE) == 0)
                    continue;

                switch (it.type)
                {
                    case CHEATCODE.CHEAT_TYPE_ALWAYS:
                        CheatWrite(it.length, it.address, it.data);
                        break;
                    case CHEATCODE.CHEAT_TYPE_ONCE:
                        CheatWrite(it.length, it.address, it.data);
                        it.enable = 0;
                        break;
                    case CHEATCODE.CHEAT_TYPE_GREATER:
                        if (CheatRead(it.length, it.address) > it.data)
                        {
                            CheatWrite(it.length, it.address, it.data);
                        }
                        break;
                    case CHEATCODE.CHEAT_TYPE_LESS:
                        if (CheatRead(it.length, it.address) < it.data)
                        {
                            CheatWrite(it.length, it.address, it.data);
                        }
                        break;
                }
            }
        }

        private uint CheatRead(byte length, ushort addr)
        {
            uint data = 0;
            for (int i = 0; i <= length; i++)
            {
                data |= (uint)(Read((ushort)(addr + i)) * (1 << (i * 8)));
            }

            return data;
        }

        private void CheatWrite(int length, ushort addr, uint data)
        {
            for (int i = 0; i <= length; i++)
            {
                Write((ushort)(addr + i), (byte)(data & 0xFF));
                data >>= 8;
            }
        }

        public void Dispose()
        {
            cpu?.Dispose();
            ppu?.Dispose();
            apu?.Dispose();
            pad?.Dispose();
            rom?.Dispose();
        }

        private void SaveSRAM()
        {
            int i;
            if (rom.IsNSF()) return;
            if (rom.IsSAVERAM()) return;

            for (i = 0; i < SAVERAM_SIZE; i++)
            {
                if (MMU.WRAM[i] != 0x00)
                    break;
            }

            if (i < SAVERAM_SIZE)
            {
                var romName = rom.GetRomName();

                Debuger.Log($"Saving SAVERAM...[{romName}]");

                Supporter.SaveSRAMToFile(MMU.WRAM, romName);
            }
        }

        private void SaveDISK()
        {
            if (rom.GetMapperNo() != 20)
                return;

            int i = 0;
            Stream fp = null;
            DISKFILEHDR ifh;
            byte[] lpDisk = rom.GetPROM();
            byte[] lpWrite = rom.GetDISK();
            long DiskSize = 16 + 65500 * rom.GetDiskNo();
            ulong data;

            try
            {
                ifh = new DISKFILEHDR();
                ifh.ID = ASCIIEncoding.ASCII.GetBytes("VirtuaNES DI");
                ifh.BlockVersion = 0x0210;
                ifh.ProgID = rom.GetGameID();
                ifh.MakerID = (ushort)rom.GetMakerID();
                ifh.DiskNo = (ushort)rom.GetDiskNo();

                for (i = 16; i < DiskSize; i++)
                {
                    if (lpWrite[i] > 0)
                        ifh.DifferentSize++;
                }

                if (ifh.DifferentSize == 0)
                    return;

                List<byte> contents = new List<byte>();
                contents.AddRange(ifh.ToBytes());

                for (i = 16; i < DiskSize; i++)
                {
                    if (lpWrite[i] > 0)
                    {
                        data = (ulong)(i & 0x00FFFFFF);
                        data |= ((ulong)lpDisk[i] & 0xFF) << 24;
                        contents.AddRange(BitConverter.GetBytes(data));
                    }
                }

                Supporter.SaveDISKToFile(contents.ToArray(), rom.GetRomName());
            }
            catch (Exception ex)
            {
                Debuger.LogError(ex.ToString());
            }
        }

        private void SaveTurboFile()
        {
            int i;

            if (pad.GetExController() != (int)EXCONTROLLER.EXCONTROLLER_TURBOFILE)
                return;

            for (i = 0; i < MMU.ERAM.Length; i++)
            {
                if (MMU.ERAM[i] != 0x00)
                    break;
            }

            if (i < MMU.ERAM.Length)
            {
                Debuger.Log("Saving TURBOFILE...");

                Supporter.SaveFile(MMU.ERAM, Supporter.Config.path.szSavePath, "TurboFile.vtf");
            }
        }

        internal void Clock(int cycles)
        {
            Tape(cycles);
            Barcode(cycles);
        }

        private void Barcode(int cycles)
        {
            if (m_bBarcode)
            {
                m_BarcodeCycles += cycles;
                if (m_BarcodeCycles > 1000)
                {
                    m_BarcodeCycles = 0;
                    // 掆巭丠
                    if (m_BarcodeData[m_BarcodePtr] != 0xFF)
                    {
                        m_BarcodeOut = m_BarcodeData[m_BarcodePtr++];
                    }
                    else
                    {
                        m_bBarcode = false;
                        m_BarcodeOut = 0;
                        Debuger.Log("Barcode data trasnfer complete!!");

                        if (!(IsTapePlay() || IsTapeRec()))
                        {
                            cpu.SetClockProcess(false);
                        }
                    }
                }
            }
        }

        public bool IsTapeRec()
        {
            return m_bTapeRec;
        }

        public bool IsTapePlay()
        {
            return m_bTapePlay;
        }

        internal void Tape(int cycles)
        {
            if (!(IsTapePlay() || IsTapeRec()))
            {
                return;
            }

            if ((m_TapeCycles -= (double)cycles) > 0)
                return;

            m_TapeCycles += (nescfg.CpuClock / 32000.0);
            //	m_TapeCycles += (nescfg->CpuClock / 22050.0);	// 抶偡偓偰僟儊偭傐偄

            if (m_bTapePlay)
            {
                int data = m_fpTape.ReadByte();
                if (data != -1) //EOF
                {
                    if ((data & 0xFF) >= 0x8C)
                    {
                        m_TapeOut = 0x02;
                    }
                    else
                        if ((data & 0xFF) <= 0x74)
                    {
                        m_TapeOut = 0x00;
                    }
                }
                else
                {
                    TapeStop();
                }
            }
            if (m_bTapeRec)
            {
                m_fpTape.WriteByte((m_TapeIn & 7) == 7 ? (byte)0x90 : (byte)0x70);
            }
        }

        private void TapeStop()
        {
            if (!m_bBarcode)
            {
                cpu.SetClockProcess(false);
            }

            m_bTapePlay = m_bTapeRec = false;
            m_fpTape?.Dispose();
            m_fpTape = null;
        }

        internal byte Read(ushort addr)
        {
            switch (addr >> 13)
            {
                case 0x00:  // $0000-$1FFF
                    return MMU.RAM[addr & 0x07FF];
                case 0x01:  // $2000-$3FFF
                    return ppu.Read((ushort)(addr & 0xE007));
                case 0x02:  // $4000-$5FFF
                    if (addr < 0x4100)
                    {
                        return ReadReg(addr);
                    }
                    else
                    {
                        return mapper.ReadLow(addr);
                    }
                case 0x03:  // $6000-$7FFF
                    return mapper.ReadLow(addr);
                case 0x04:  // $8000-$9FFF
                case 0x05:  // $A000-$BFFF
                case 0x06:  // $C000-$DFFF
                case 0x07:  // $E000-$FFFF
                    return MMU.CPU_MEM_BANK[addr >> 13][addr & 0x1FFF];
            }

            return 0x00;    // Warning梊杊
        }

        private byte ReadReg(ushort addr)
        {
            switch (addr & 0xFF)
            {
                case 0x00:
                case 0x01:
                case 0x02:
                case 0x03:
                case 0x04:
                case 0x05:
                case 0x06:
                case 0x07:
                case 0x08:
                case 0x09:
                case 0x0A:
                case 0x0B:
                case 0x0C:
                case 0x0D:
                case 0x0E:
                case 0x0F:
                case 0x10:
                case 0x11:
                case 0x12:
                case 0x13:
                    return apu.Read(addr);
                case 0x15:
                    return apu.Read(addr);
                case 0x14:
                    return (byte)(addr & 0xFF);
                case 0x16:
                    if (rom.IsVSUNISYSTEM())
                    {
                        return pad.Read(addr);
                    }
                    else
                    {
                        return (byte)(pad.Read(addr) | 0x40 | m_TapeOut);
                    }
                case 0x17:
                    if (rom.IsVSUNISYSTEM())
                    {
                        return pad.Read(addr);
                    }
                    else
                    {
                        return (byte)(pad.Read(addr) | apu.Read(addr));
                    }
                default:
                    return mapper.ExRead(addr);
            }
        }

        internal byte Barcode2()
        {
            byte ret = 0x00;

            if (!m_bBarcode2 || m_Barcode2seq < 0)
                return ret;

            switch (m_Barcode2seq)
            {
                case 0:
                    m_Barcode2seq++;
                    m_Barcode2ptr = 0;
                    ret = 0x04;     // d3
                    break;

                case 1:
                    m_Barcode2seq++;
                    m_Barcode2bit = m_Barcode2data[m_Barcode2ptr];
                    m_Barcode2cnt = 0;
                    ret = 0x04;     // d3
                    break;

                case 2:
                    ret = (byte)((m_Barcode2bit & 0x01) != 0 ? 0x00 : 0x04); // Bit rev.
                    m_Barcode2bit >>= 1;
                    if (++m_Barcode2cnt > 7)
                    {
                        m_Barcode2seq++;
                    }
                    break;
                case 3:
                    if (++m_Barcode2ptr > 19)
                    {
                        m_bBarcode2 = false;
                        m_Barcode2seq = -1;
                    }
                    else
                    {
                        m_Barcode2seq = 1;
                    }
                    break;
                default:
                    break;
            }

            return ret;
        }
        public void SetRenderMethod(EnumRenderMethod type)
        {
            RenderMethod = type;
        }
        internal void Write(ushort addr, byte data)
        {
            switch (addr >> 13)
            {
                case 0x00:  // $0000-$1FFF
                    MMU.RAM[addr & 0x07FF] = data;
                    break;
                case 0x01:  // $2000-$3FFF
                    if (!rom.IsNSF())
                    {
                        ppu.Write((ushort)(addr & 0xE007), data);
                    }
                    break;
                case 0x02:  // $4000-$5FFF
                    if (addr < 0x4100)
                    {
                        WriteReg(addr, data);
                    }
                    else
                    {
                        mapper.WriteLow(addr, data);
                    }
                    break;
                case 0x03:  // $6000-$7FFF
                    mapper.WriteLow(addr, data);
                    break;
                case 0x04:  // $8000-$9FFF
                case 0x05:  // $A000-$BFFF
                case 0x06:  // $C000-$DFFF
                case 0x07:  // $E000-$FFFF
                    mapper.Write(addr, data);

                    GenieCodeProcess();
                    break;
            }
        }

        private void GenieCodeProcess()
        {
            ushort addr;

            for (int i = 0; i < m_GenieCode.Count; i++)
            {
                addr = m_GenieCode[i].address;
                if ((addr & 0x8000) != 0)
                {
                    // 8character codes
                    if (MMU.CPU_MEM_BANK[addr >> 13][addr & 0x1FFF] == m_GenieCode[i].cmp)
                    {
                        MMU.CPU_MEM_BANK[addr >> 13][addr & 0x1FFF] = m_GenieCode[i].data;
                    }
                }
                else
                {
                    // 6character codes
                    addr |= 0x8000;
                    MMU.CPU_MEM_BANK[addr >> 13][addr & 0x1FFF] = m_GenieCode[i].data;
                }
            }
        }

        private void WriteReg(ushort addr, byte data)
        {
            switch (addr & 0xFF)
            {
                case 0x00:
                case 0x01:
                case 0x02:
                case 0x03:
                case 0x04:
                case 0x05:
                case 0x06:
                case 0x07:
                case 0x08:
                case 0x09:
                case 0x0A:
                case 0x0B:
                case 0x0C:
                case 0x0D:
                case 0x0E:
                case 0x0F:
                case 0x10:
                case 0x11:
                case 0x12:
                case 0x13:
                case 0x15:
                    apu.Write(addr, data);
                    MMU.CPUREG[addr & 0xFF] = data;
                    break;
                case 0x14:
                    ppu.DMA(data);
                    cpu.DMA(514); // DMA Pending cycle
                    MMU.CPUREG[addr & 0xFF] = data;
                    break;
                case 0x16:
                    mapper.ExWrite(addr, data);    // For VS-Unisystem
                    pad.Write(addr, data);
                    MMU.CPUREG[addr & 0xFF] = data;
                    m_TapeIn = data;
                    break;
                case 0x17:
                    MMU.CPUREG[addr & 0xFF] = data;
                    pad.Write(addr, data);
                    apu.Write(addr, data);
                    break;
                // VirtuaNES屌桳億乕僩
                case 0x18:
                    apu.Write(addr, data);
                    break;
                default:
                    mapper.ExWrite(addr, data);
                    break;
            }
        }

        internal bool GetVideoMode()
        {
            return bVideoMode;
        }

        internal void SetFrameIRQmode(bool bMode)
        {
            bFrameIRQ = bMode;
        }

        internal bool GetFrameIRQmode()
        {
            return bFrameIRQ;
        }

        internal EnumRenderMethod GetRenderMethod()
        {
            return RenderMethod;
        }

        internal void SetIrqType(IRQMETHOD nType)
        {
            nIRQtype = (int)nType;
        }

        internal int GetScanline()
        {
            return NES_scanline;
        }

        public enum IRQMETHOD
        {
            IRQ_HSYNC = 0, IRQ_CLOCK = 1
        }
    }
}
