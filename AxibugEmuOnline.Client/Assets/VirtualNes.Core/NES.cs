using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VirtualNes.Core.Debug;

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

                ppu.InitBuffer();

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
                Debuger.Log($"PROM-CRC      : #{rom.GetPROM_CRC():X2}");
                Debuger.Log($"VROM-CRC      : #{rom.GetVROM_CRC():X2}");
                Debuger.Log($"PRG SIZE      : {16 * rom.GetPROM_SIZE():0000}K");
                Debuger.Log($"CHR SIZE      : {8 * rom.GetVROM_SIZE():0000}K");

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

        internal int GetIrqType()
        {
            return nIRQtype;
        }

        private void LoadTurboFile()
        {
            MemoryUtility.ZEROMEMORY(MMU.ERAM, MMU.ERAM.Length);

            if (pad.GetExController() != (int)EXCONTROLLER.EXCONTROLLER_TURBOFILE)
                return;

            var fp = Supporter.S.OpenFile(Supporter.S.Config.path.szSavePath, "TurboFile.vtf");
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

            var saveFileDir = Supporter.S.Config.path.szSavePath;
            var saveFileName = $"{rom.GetRomName()}.sav";

            var fp = Supporter.S.OpenFile(saveFileDir, saveFileName);

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

        public void CheatInitial()
        {
            m_CheatCode.Clear();
        }

        public uint FrameCount { get; private set; }
        public void EmulateFrame(bool bDraw)
        {
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
                                ppu.Scanline(scanline, Supporter.S.Config.graphics.bAllSprite, Supporter.S.Config.graphics.bLeftClip);
                            }
                            else
                            {
                                if (pad.IsZapperMode() && scanline == ZapperY)
                                {
                                    ppu.Scanline(scanline, Supporter.S.Config.graphics.bAllSprite, Supporter.S.Config.graphics.bLeftClip);
                                }
                                else
                                {
                                    if (!ppu.IsSprite0(scanline))
                                    {
                                        ppu.DummyScanline(scanline);
                                    }
                                    else
                                    {
                                        ppu.Scanline(scanline, Supporter.S.Config.graphics.bAllSprite, Supporter.S.Config.graphics.bLeftClip);
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
                                ppu.Scanline(scanline, Supporter.S.Config.graphics.bAllSprite, Supporter.S.Config.graphics.bLeftClip);
                            }
                            else
                            {
                                if (pad.IsZapperMode() && scanline == ZapperY)
                                {
                                    ppu.Scanline(scanline, Supporter.S.Config.graphics.bAllSprite, Supporter.S.Config.graphics.bLeftClip);
                                }
                                else
                                {
                                    if (!ppu.IsSprite0(scanline))
                                    {
                                        ppu.DummyScanline(scanline);
                                    }
                                    else
                                    {
                                        ppu.Scanline(scanline, Supporter.S.Config.graphics.bAllSprite, Supporter.S.Config.graphics.bLeftClip);
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
                            ppu.Scanline(scanline, Supporter.S.Config.graphics.bAllSprite, Supporter.S.Config.graphics.bLeftClip);
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
                                ppu.Scanline(scanline, Supporter.S.Config.graphics.bAllSprite, Supporter.S.Config.graphics.bLeftClip);
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
                                    ppu.Scanline(scanline, Supporter.S.Config.graphics.bAllSprite, Supporter.S.Config.graphics.bLeftClip);
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

            FrameCount++;
        }

        internal void DrawString(int x, int y, string str, byte col)
        {
            foreach (var @char in str)
            {
                DrawFont(x, y, (byte)@char, col);
                x += 6;
            }
        }

        internal unsafe void DrawFont(int x, int y, byte chr, byte col)
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

        public void Reset()
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



        public void SoftReset()
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

                Supporter.S.SaveSRAMToFile(MMU.WRAM, romName);
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

                Supporter.S.SaveDISKToFile(contents.ToArray(), rom.GetRomName());
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

                Supporter.S.SaveFile(MMU.ERAM, Supporter.S.Config.path.szSavePath, "TurboFile.vtf");
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

            if ((m_TapeCycles -= cycles) > 0)
                return;

            m_TapeCycles += (nescfg.CpuClock / 32000.0);
            //	m_TapeCycles += (nescfg.CpuClock / 22050.0);	// 抶偡偓偰僟儊偭傐偄

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

        internal void SetSAVERAM_SIZE(int size)
        {
            SAVERAM_SIZE = size;
        }

        internal byte GetBarcodeStatus()
        {
            return m_BarcodeOut;
        }

        public State GetState()
        {
            State state = new State();

            //HEADER
            {
                state.HEADER.ID = "VirtuaNES ST";
                state.HEADER.BlockVersion = 0x0200;

                if (rom.GetMapperNo() != 20)
                    state.HEADER.Ext0 = rom.GetPROM_CRC();
                else
                {
                    state.HEADER.Ext0 = rom.GetGameID();
                    state.HEADER.Ext1 = (ushort)rom.GetMakerID();
                    state.HEADER.Ext2 = (ushort)rom.GetDiskNo();
                }
            }

            //REGISTER STATE
            {
                state.regBLOCK.ID = "REG DATA";
                state.regBLOCK.BlockVersion = 0x0210;
                state.regBLOCK.BlockSize = state.reg.GetSize();

                R6502 R = null;
                cpu.GetContext(ref R);

                state.reg.cpureg.PC = R.PC;
                state.reg.cpureg.A = R.A;
                state.reg.cpureg.X = R.X;
                state.reg.cpureg.Y = R.Y;
                state.reg.cpureg.S = R.S;
                state.reg.cpureg.P = R.P;
                state.reg.cpureg.I = R.INT_pending;

                int cycles = 0;
                apu.GetFrameIRQ(ref cycles,
                    ref state.reg.cpureg.FrameIRQ_count,
                    ref state.reg.cpureg.FrameIRQ_type,
                    ref state.reg.cpureg.FrameIRQ,
                    ref state.reg.cpureg.FrameIRQ_occur);
                state.reg.cpureg.FrameIRQ_cycles = cycles;	// 参照がINTな為（ぉ

                state.reg.cpureg.DMA_cycles = cpu.GetDmaCycles();
                state.reg.cpureg.emul_cycles = emul_cycles;
                state.reg.cpureg.base_cycles = base_cycles;

                // SAVE PPU STATE
                state.reg.ppureg.reg0 = MMU.PPUREG[0];
                state.reg.ppureg.reg1 = MMU.PPUREG[1];
                state.reg.ppureg.reg2 = MMU.PPUREG[2];
                state.reg.ppureg.reg3 = MMU.PPUREG[3];
                state.reg.ppureg.reg7 = MMU.PPU7_Temp;
                state.reg.ppureg.loopy_t = MMU.loopy_t;
                state.reg.ppureg.loopy_v = MMU.loopy_v;
                state.reg.ppureg.loopy_x = MMU.loopy_x;
                state.reg.ppureg.toggle56 = MMU.PPU56Toggle;
            }

            //RAM STATE
            {
                state.ram = RAMSTAT.GetDefault();
                uint size = 0;

                // SAVE RAM STATE
                MemoryUtility.memcpy(state.ram.RAM, MMU.RAM, state.ram.RAM.Length);
                MemoryUtility.memcpy(state.ram.BGPAL, MMU.BGPAL, state.ram.BGPAL.Length);
                MemoryUtility.memcpy(state.ram.SPPAL, MMU.SPPAL, state.ram.SPPAL.Length);
                MemoryUtility.memcpy(state.ram.SPRAM, MMU.SPRAM, state.ram.SPRAM.Length);

                // S-RAM STATE(使用/未使用に関わらず存在すればセーブする)
                if (rom.IsSAVERAM())
                {
                    size = (uint)SAVERAM_SIZE;
                }

                // Create Header
                state.ramBLOCK.ID = "RAM DATA";
                state.ramBLOCK.BlockVersion = 0x0100;
                state.ramBLOCK.BlockSize = size + state.ram.GetSize();

                if (rom.IsSAVERAM())
                {
                    state.WRAM = new byte[SAVERAM_SIZE];
                    Array.Copy(MMU.WRAM, state.WRAM, SAVERAM_SIZE);
                }
            }

            //BANK STATE
            {
                state.mmu = MMUSTAT.GetDefault();
                uint size = 0;

                // SAVE CPU MEMORY BANK DATA
                // BANK0,1,2はバンクセーブに関係なし
                // VirtuaNES0.30から
                // バンク３はSRAM使用に関わらずセーブ
                for (int i = 3; i < 8; i++)
                {
                    state.mmu.CPU_MEM_TYPE[i] = MMU.CPU_MEM_TYPE[i];
                    state.mmu.CPU_MEM_PAGE[i] = (ushort)MMU.CPU_MEM_PAGE[i];

                    if (MMU.CPU_MEM_TYPE[i] == MMU.BANKTYPE_RAM
                        || MMU.CPU_MEM_TYPE[i] == MMU.BANKTYPE_DRAM)
                    {
                        size += 8 * 1024;   // 8K BANK
                    }
                }

                // SAVE VRAM MEMORY DATA
                for (int i = 0; i < 12; i++)
                {
                    state.mmu.PPU_MEM_TYPE[i] = MMU.PPU_MEM_TYPE[i];
                    state.mmu.PPU_MEM_PAGE[i] = (ushort)MMU.PPU_MEM_PAGE[i];
                }
                size += 4 * 1024;   // 1K BANK x 4 (VRAM)

                for (int i = 0; i < 8; i++)
                {
                    state.mmu.CRAM_USED[i] = MMU.CRAM_USED[i];
                    if (MMU.CRAM_USED[i] != 0)
                    {
                        size += 4 * 1024;   // 4K BANK
                    }
                }

                // Create Header
                state.mmuBLOCK.ID = "MMU DATA";
                state.mmuBLOCK.BlockVersion = 0x0200;
                state.mmuBLOCK.BlockSize = size + state.mmu.GetSize();

                state.CPU_MEM_BANK = new List<byte>();
                // WRITE CPU RAM MEMORY BANK
                for (int i = 3; i < 8; i++)
                {
                    if (state.mmu.CPU_MEM_TYPE[i] != MMU.BANKTYPE_ROM)
                    {
                        state.CPU_MEM_BANK.AddRange(MMU.CPU_MEM_BANK[i].Span(0, 8 * 1024).ToArray());
                    }
                }

                // WRITE VRAM MEMORY(常に4K分すべて書き込む)
                state.VRAM = new byte[4 * 1024];
                Array.Copy(MMU.VRAM, state.VRAM, state.VRAM.Length);

                state.CRAM = new List<byte>();
                // WRITE CRAM MEMORY
                for (int i = 0; i < 8; i++)
                {
                    if (MMU.CRAM_USED[i] != 0)
                    {
                        var bytes = new byte[4 * 1024];
                        Array.Copy(MMU.CRAM, 0x1000 * i, bytes, 0, bytes.Length);
                        state.CRAM.AddRange(bytes);
                    }
                }
            }

            // MMC STATE
            {
                state.mmc = MMCSTAT.GetDefault();

                // Create Header
                state.mmcBLOCK.ID = "MMC DATA";
                state.mmcBLOCK.BlockVersion = 0x0100;
                state.mmcBLOCK.BlockSize = state.mmc.GetSize();

                if (mapper.IsStateSave())
                {
                    mapper.SaveState(state.mmc.mmcdata);
                }
            }

            //CONTROLLER STATE
            {
                // Create Header
                state.ctrBLOCK.ID = "CTR DATA";
                state.ctrBLOCK.BlockVersion = 0x0100;
                state.ctrBLOCK.BlockSize = state.ctr.GetSize();

                state.ctr.pad1bit = 0;
                state.ctr.pad2bit = 0;
                state.ctr.pad3bit = 0;
                state.ctr.pad4bit = 0;
                state.ctr.strobe = 0;
            }

            //SND STATE
            {
                state.snd = SNDSTAT.GetDefault();

                // Create Header
                state.sndBLOCK.ID = "SND DATA";
                state.sndBLOCK.BlockVersion = 0x0100;
                state.sndBLOCK.BlockSize = state.snd.GetSize();

                StateBuffer buffer = new StateBuffer();
                apu.SaveState(buffer);
                Array.Copy(buffer.Data.ToArray(), state.snd.snddata, buffer.Data.Count);
            }

            // DISKIMAGE STATE
            if (rom.GetMapperNo() == 20)
            {
                var lpDisk = rom.GetPROM();
                var lpWrite = rom.GetDISK();
                int DiskSize = 16 + 65500 * rom.GetDiskNo();


                // 相違数をカウント
                for (int i = 16; i < DiskSize; i++)
                {
                    if (lpWrite[i] != 0)
                        state.dsk.DifferentSize++;
                }

                state.dskBLOCK.ID = "DISKDATA";
                state.dskBLOCK.BlockVersion = 0x0210;
                state.dskBLOCK.BlockSize = 0;

                state.dskdata = new List<uint>();

                for (int i = 16; i < DiskSize; i++)
                {
                    if (lpWrite[i] != 0)
                    {
                        uint data = (uint)(i & 0x00FFFFFF);
                        data |= ((uint)lpDisk[i] & 0xFF) << 24;
                        state.dskdata.Add(data);
                    }
                }
            }

            // EXCTR STATE
            if (pad.GetExController() != 0)
            {
                state.exctrBLOCK.ID = "EXCTRDAT";
                state.exctrBLOCK.BlockVersion = 0x0100;
                state.exctrBLOCK.BlockSize = state.exctr.GetSize();

                // Some excontrollers will default 0
                state.exctr.data = pad.GetSyncExData();
            }

            return state;
        }

        public void LoadState(State state)
        {
            FrameCount = 0;
            //HEADER
            {
                state.HEADER.ID = "VirtuaNES ST";
                state.HEADER.BlockVersion = 0x0200;

                if (rom.GetMapperNo() != 20)
                    rom.SetPROM_CRC(state.HEADER.Ext0);
                else
                {
                    rom.SetGameID(state.HEADER.Ext0);
                    rom.SetMakerID(state.HEADER.Ext1);
                    rom.SetDiskNo(state.HEADER.Ext2);
                }
            }

            //REGISTER STATE
            {
                R6502 R = new R6502();
                R.PC = state.reg.cpureg.PC;
                R.A = state.reg.cpureg.A;
                R.X = state.reg.cpureg.X;
                R.Y = state.reg.cpureg.Y;
                R.S = state.reg.cpureg.S;
                R.P = state.reg.cpureg.P;
                R.INT_pending = state.reg.cpureg.I;
                cpu.SetContext(R);

                apu.SetFrameIRQ(
                    state.reg.cpureg.FrameIRQ_cycles,
                    state.reg.cpureg.FrameIRQ_count,
                    state.reg.cpureg.FrameIRQ_type,
                    state.reg.cpureg.FrameIRQ,
                    state.reg.cpureg.FrameIRQ_occur
                );


                cpu.SetDmaCycles(state.reg.cpureg.DMA_cycles);
                emul_cycles = state.reg.cpureg.emul_cycles;
                base_cycles = state.reg.cpureg.base_cycles;

                // LOAD PPU STATE
                MMU.PPUREG[0] = state.reg.ppureg.reg0;
                MMU.PPUREG[1] = state.reg.ppureg.reg1;
                MMU.PPUREG[2] = state.reg.ppureg.reg2;
                MMU.PPUREG[3] = state.reg.ppureg.reg3;
                MMU.PPU7_Temp = state.reg.ppureg.reg7;
                MMU.loopy_t = state.reg.ppureg.loopy_t;
                MMU.loopy_v = state.reg.ppureg.loopy_v;
                MMU.loopy_x = state.reg.ppureg.loopy_x;
                MMU.PPU56Toggle = state.reg.ppureg.toggle56;
            }

            //RAM STATE
            {
                // SAVE RAM STATE
                MemoryUtility.memcpy(MMU.RAM, state.ram.RAM, state.ram.RAM.Length);
                MemoryUtility.memcpy(MMU.BGPAL, state.ram.BGPAL, state.ram.BGPAL.Length);
                MemoryUtility.memcpy(MMU.SPPAL, state.ram.SPPAL, state.ram.SPPAL.Length);
                MemoryUtility.memcpy(MMU.SPRAM, state.ram.SPRAM, state.ram.SPRAM.Length);

                if (rom.IsSAVERAM())
                {
                    Array.Copy(state.WRAM, MMU.WRAM, SAVERAM_SIZE);
                }
            }

            //BANK STATE
            {
                // SAVE CPU MEMORY BANK DATA
                // BANK0,1,2はバンクセーブに関係なし
                // VirtuaNES0.30から
                // バンク３はSRAM使用に関わらずセーブ
                for (byte i = 3; i < 8; i++)
                {
                    MMU.CPU_MEM_TYPE[i] = state.mmu.CPU_MEM_TYPE[i];
                    MMU.CPU_MEM_PAGE[i] = state.mmu.CPU_MEM_PAGE[i];
                    if (MMU.CPU_MEM_TYPE[i] == MMU.BANKTYPE_ROM)
                        MMU.SetPROM_8K_Bank(i, MMU.CPU_MEM_PAGE[i]);
                    else
                    {
                        MMU.CPU_MEM_BANK[i].SetArray(state.CPU_MEM_BANK.ToArray(), 0);
                    }
                }

                // VRAM
                MemoryUtility.memcpy(MMU.VRAM, state.VRAM, 4 * 1024);
                // CRAM
                for (int i = 0; i < 8; i++)
                {
                    MMU.CRAM_USED[i] = state.mmu.CRAM_USED[i];
                }
                // SAVE VRAM MEMORY DATA
                for (byte i = 0; i < 12; i++)
                {
                    if (state.mmu.PPU_MEM_TYPE[i] == MMU.BANKTYPE_VROM)
                    {
                        MMU.SetVROM_1K_Bank(i, state.mmu.PPU_MEM_PAGE[i]);
                    }
                    else if (state.mmu.PPU_MEM_TYPE[i] == MMU.BANKTYPE_CRAM)
                    {
                        MMU.SetCRAM_1K_Bank(i, state.mmu.PPU_MEM_PAGE[i]);
                    }
                    else if (state.mmu.PPU_MEM_TYPE[i] == MMU.BANKTYPE_VRAM)
                    {
                        MMU.SetVRAM_1K_Bank(i, state.mmu.PPU_MEM_PAGE[i]);
                    }
                    else
                    {
                        throw new Exception("Unknown bank types.");
                    }
                }

                // WRITE CPU RAM MEMORY BANK

                int stateStep = 0;
                var stateCPU_MEM_BANK = state.CPU_MEM_BANK.ToArray();

                for (int i = 3; i < 8; i++)
                {
                    if (state.mmu.CPU_MEM_TYPE[i] != MMU.BANKTYPE_ROM)
                    {
                        var sourceData = new Span<byte>(stateCPU_MEM_BANK, stateStep * 8 * 1024, 8 * 1024);
                        MMU.CPU_MEM_BANK[i].WriteTo(sourceData.ToArray(), 0, 8 * 1024);
                        stateStep++;
                    }
                }

                Array.Copy(state.VRAM, MMU.VRAM, state.VRAM.Length);

                stateStep = 0;
                var stateCRAM = state.CRAM.ToArray();
                // LOAD CRAM MEMORY
                for (int i = 0; i < 8; i++)
                {
                    if (MMU.CRAM_USED[i] != 0)
                    {
                        var sourceData = stateCRAM.AsSpan(stateStep * 4 * 1024, 4 * 1024).ToArray();
                        Array.Copy(sourceData, 0, MMU.CRAM, 0x1000 * i, 4 * 1024);
                    }
                }
            }

            // MMC STATE
            {
                mapper.LoadState(state.mmc.mmcdata);
            }

            //CONTROLLER STATE
            {
                pad.pad1bit = state.ctr.pad1bit;
                pad.pad2bit = state.ctr.pad2bit;
                pad.pad3bit = state.ctr.pad3bit;
                pad.pad4bit = state.ctr.pad4bit;
                pad.SetStrobe(state.ctr.strobe == 0 ? false : true);
            }

            //SND STATE
            {
                var buffer = new StateReader(state.snd.snddata);
                apu.LoadState(buffer);
            }

            // DISKIMAGE STATE
            if (rom.GetMapperNo() == 20)
            {
                var lpDisk = rom.GetPROM();
                var lpWrite = rom.GetDISK();
                int DiskSize = 16 + 65500 * rom.GetDiskNo();

                Array.Clear(lpWrite, 0, DiskSize);

                for (int i = 0; i < state.dsk.DifferentSize; i++)
                {
                    var pos = state.dskdata[i];
                    byte data = (byte)(pos >> 24);
                    pos &= 0x00FFFFFF;

                    if (pos >= 16 && pos < DiskSize)
                    {
                        lpDisk[pos] = data;
                        lpWrite[pos] = 0xFF;
                    }
                }
            }

            // EXCTR STATE
            if (pad.GetExController() != 0)
            {
                pad.SetSyncExData(state.exctr.data);
            }
        }

        internal void SetZapperPos(int x, int y)
        {
            ZapperX = x; ZapperY = y;
        }

        public enum IRQMETHOD
        {
            IRQ_HSYNC = 0, IRQ_CLOCK = 1
        }
    }
}
