using Codice.CM.Client.Differences;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        public NesConfig nescfg;

        private List<CHEATCODE> m_CheatCode = new List<CHEATCODE>();
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

            nescfg = NesConfig.GetNTSC();

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
                        }
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

            //todo : 实现
        }

        internal void SoftReset()
        {
            //todo : 实现
        }

        internal void EmulateNSF()
        {
            //todo : 实现NSF模拟
            throw new NotImplementedException("EmulateNSF");
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
                if (MMU.WARM[i] != 0x00)
                    break;
            }

            if (i < SAVERAM_SIZE)
            {
                var romName = rom.GetRomName();

                Debuger.Log($"Saving SAVERAM...[{romName}]");

                Supporter.SaveSRAMToFile(MMU.WARM, romName);
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
                    return MMU.CPU_MEM_BANK[addr >> 13][addr & 0x1FFF];
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
    }

}
