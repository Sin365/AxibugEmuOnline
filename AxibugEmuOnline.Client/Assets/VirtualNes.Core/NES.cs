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
        private int m_CommandRequest;
        private int m_nSnapNo;
        private bool m_bNsfPlaying;
        private bool m_bNsfInit;
        private int m_nNsfSongNo;
        private int m_nNsfSongMode;
        private bool m_bMoviePlay;
        private bool m_bMovieRec;
        private Stream m_fpMovie;
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
        private long base_cycles;
        private long emul_cycles;

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

                    }
                }
            }
        }

        internal void EmulationCPU(int basecycles)
        {
            int cycles;

            base_cycles += basecycles;
            cycles = (int)((base_cycles / 12) - emul_cycles);

            if (cycles > 0)
            {
                emul_cycles += cpu.EXEC(cycles);
            }
        }

        internal void Reset()
        {
            SaveSRAM();
            SaveDISK();
            SaveTurboFile();

            // RAM Clear
            MemoryUtility.ZEROMEMORY(MMU.RAM, (uint)MMU.RAM.Length);
            if (rom.GetPROM_CRC() == 0x29401686)
            {	// Minna no Taabou no Nakayoshi Dai Sakusen(J)
                MemoryUtility.memset(MMU.RAM, 0xFF, (uint)MMU.RAM.Length);
            }

            // RAM set
            if (!rom.IsSAVERAM() && rom.GetMapperNo() != 20)
            {
                MemoryUtility.memset(MMU.WRAM, 0xFF, (uint)MMU.WRAM.Length);
            }

            MemoryUtility.ZEROMEMORY(MMU.CRAM, (uint)MMU.CRAM.Length);
            MemoryUtility.ZEROMEMORY(MMU.VRAM, (uint)MMU.VRAM.Length);

            MemoryUtility.ZEROMEMORY(MMU.SPRAM, (uint)MMU.SPRAM.Length);
            MemoryUtility.ZEROMEMORY(MMU.BGPAL, (uint)MMU.BGPAL.Length);
            MemoryUtility.ZEROMEMORY(MMU.SPPAL, (uint)MMU.SPPAL.Length);

            MemoryUtility.ZEROMEMORY(MMU.CPUREG, (uint)MMU.CPUREG.Length);
            MemoryUtility.ZEROMEMORY(MMU.PPUREG, (uint)MMU.PPUREG.Length);

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

        public void SetRenderMethod(EnumRenderMethod type)
        {
            RenderMethod = type;
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
                    MemoryUtility.ZEROMEMORY(MMU.RAM, (uint)MMU.RAM.Length);
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
            //todo : 实现作弊码
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
            //todo : 实现
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
            //todo : 实现Tape (目测是记录玩家操作再Play,优先级很低)
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
                    return MMU.CPU_MEM_BANK[addr >> 13].Span[addr & 0x1FFF];
            }

            return 0x00;	// Warning梊杊
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
                    if (MMU.CPU_MEM_BANK[addr >> 13].Span[addr & 0x1FFF] == m_GenieCode[i].cmp)
                    {
                        MMU.CPU_MEM_BANK[addr >> 13].Span[addr & 0x1FFF] = m_GenieCode[i].data;
                    }
                }
                else
                {
                    // 6character codes
                    addr |= 0x8000;
                    MMU.CPU_MEM_BANK[addr >> 13].Span[addr & 0x1FFF] = m_GenieCode[i].data;
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
    }
}
