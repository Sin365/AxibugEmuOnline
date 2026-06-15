using static VirtualNes.MMU;
using BYTE = System.Byte;
using INT = System.Int32;


namespace VirtualNes.Core
{
    public class Mapper191 : Mapper
    {
        BYTE[] reg = new BYTE[8];
        BYTE prg0, prg1;
        BYTE[] chr = new BYTE[8];
        BYTE we_sram;

        BYTE irq_type;
        BYTE irq_enable;
        BYTE irq_counter;
        BYTE irq_latch;
        BYTE irq_request;

        BYTE patch;
        public Mapper191(NES parent) : base(parent)
        {

        }

        public override bool IsStateSave()
        {
            return true;
        }

        public override void Reset()
        {
            for (INT i = 0; i < 8; i++)
            {
                reg[i] = 0x00;
            }
            prg0 = 0;
            prg1 = 1;
            SetBank_CPU();

            chr[0] = 0;
            chr[2] = 2;
            chr[4] = 4;
            chr[5] = 5;
            chr[6] = 6;
            chr[7] = 7;
            SetBank_PPU();

            we_sram = 0;    // Disable
            irq_enable = 0; // Disable
            irq_counter = 0;
            irq_latch = 0;
            irq_request = 0;
        }


        //BYTE Mapper191::ReadLow(WORD addr)
        public override byte ReadLow(ushort addr)
        {
            if (addr >= 0x5000 && addr <= 0x5FFF)
            {
                return XRAM[addr - 0x4000];
            }
            else
            {
                return base.ReadLow(addr);
            }
        }

        //void Mapper191::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            if (addr >= 0x5000 && addr <= 0x5FFF)
            {
                XRAM[addr - 0x4000] = data;
            }
            else
            {
                base.WriteLow(addr, data);
            }
        }

        //void Mapper191::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            switch (addr & 0xE001)
            {
                case 0x8000:
                    reg[0] = data;
                    SetBank_CPU();
                    SetBank_PPU();
                    break;
                case 0x8001:
                    reg[1] = data;

                    switch (reg[0] & 0x07)
                    {
                        case 0x00:
                            chr[0] = (byte)(data & 0xFE);
                            SetBank_PPU();
                            break;
                        case 0x01:
                            chr[2] = (byte)(data & 0xFE);
                            SetBank_PPU();
                            break;
                        case 0x02:
                            chr[4] = data;
                            SetBank_PPU();
                            break;
                        case 0x03:
                            chr[5] = data;
                            SetBank_PPU();
                            break;
                        case 0x04:
                            chr[6] = data;
                            SetBank_PPU();
                            break;
                        case 0x05:
                            chr[7] = data;
                            SetBank_PPU();
                            break;
                        case 0x06:
                            prg0 = data;
                            SetBank_CPU();
                            break;
                        case 0x07:
                            prg1 = data;
                            SetBank_CPU();
                            break;
                    }
                    break;
                case 0xA000:
                    reg[2] = data;
                    if (!nes.rom.Is4SCREEN())
                    {
                        if ((data & 0x01) != 0) SetVRAM_Mirror(VRAM_HMIRROR);
                        else SetVRAM_Mirror(VRAM_VMIRROR);
                    }
                    break;
                case 0xA001:
                    reg[3] = data;
                    break;
                case 0xC000:
                    reg[4] = data;
                    irq_counter = data;
                    irq_request = 0;
                    break;
                case 0xC001:
                    reg[5] = data;
                    irq_latch = data;
                    irq_request = 0;
                    break;
                case 0xE000:
                    reg[6] = data;
                    irq_enable = 0;
                    irq_request = 0;
                    nes.cpu.ClrIRQ(CPU.IRQ_MAPPER);
                    break;
                case 0xE001:
                    reg[7] = data;
                    irq_enable = 1;
                    irq_request = 0;
                    break;
            }

        }

        //void Mapper191::HSync(INT scanline)
        public override void HSync(int scanline)
        {
            if ((scanline >= 0 && scanline <= 239))
            {
                if (nes.ppu.IsDispON())
                {
                    if (irq_enable != 0 && irq_request == 0)
                    {
                        if (scanline == 0)
                        {
                            if (irq_counter != 0)
                            {
                                irq_counter--;
                            }
                        }
                        if ((irq_counter--) == 0)
                        {
                            irq_request = 0xFF;
                            irq_counter = irq_latch;
                            nes.cpu.SetIRQ(CPU.IRQ_MAPPER);
                        }
                    }
                }
            }
        }

        //void Mapper191::SetBank_CPU()

        private void SetBank_CPU()
        {
            if ((reg[0] & 0x40) != 0)
            {
                SetPROM_32K_Bank(PROM_8K_SIZE - 2, prg1, prg0, PROM_8K_SIZE - 1);
            }
            else
            {
                SetPROM_32K_Bank(prg0, prg1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);
            }
        }

        //void Mapper191::SetBank_PPU()
        private void SetBank_PPU()
        {
            chr[1] = (byte)(chr[0] + 1);
            chr[3] = (byte)(chr[2] + 1);

            if ((reg[0] & 0x80) != 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    SetBank_PPUSUB(i, chr[(i + 4) & 7], (chr[(i + 4) & 7] & 0x80) != 0);
                }
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    SetBank_PPUSUB(i, chr[i], (chr[i] & 0x80) != 0);
                }
            }
        }

        //void Mapper191::SetBank_PPUSUB(int bank, int page, BOOL bRAM)
        private void SetBank_PPUSUB(int bank, int page, bool bRAM)
        {
            if (bRAM)
            {
                SetCRAM_1K_Bank((BYTE)bank, page & 7);
            }
            else
            {
                SetVROM_1K_Bank((BYTE)bank, page);
            }
        }

        //void Mapper191::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            for (INT i = 0; i < 8; i++)
            {
                p[i] = reg[i];
            }
            p[8] = prg0;
            p[9] = prg1;
            p[10] = chr[0];
            p[11] = chr[2];
            p[12] = chr[4];
            p[13] = chr[5];
            p[14] = chr[6];
            p[15] = chr[7];
            p[16] = irq_enable;
            p[17] = irq_counter;
            p[18] = irq_latch;
            p[19] = irq_request;
        }

        //void Mapper191::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            for (INT i = 0; i < 8; i++)
            {
                reg[i] = p[i];
            }
            prg0 = p[8];
            prg1 = p[9];
            chr[0] = p[10];
            chr[2] = p[11];
            chr[4] = p[12];
            chr[5] = p[13];
            chr[6] = p[14];
            chr[7] = p[15];
            irq_enable = p[16];
            irq_counter = p[17];
            irq_latch = p[18];
            irq_request = p[19];
        }
    }
}

////////////////////////////////////////////////////////////////////////////
//// Mapper191 SACHEN Super Cartridge Xin1 (Ver.1-9)                      //
////           SACHEN Q-BOY Support                                       //
////////////////////////////////////////////////////////////////////////////
//using static VirtualNes.MMU;
//using BYTE = System.Byte;
//using INT = System.Int32;


//namespace VirtualNes.Core
//{
//    public class Mapper191 : Mapper
//    {
//        BYTE[] reg = new BYTE[8];
//        BYTE prg0, prg1;
//        BYTE chr0, chr1, chr2, chr3;
//        BYTE highbank;
//        public Mapper191(NES parent) : base(parent)
//        {
//        }

//        public override void Reset()

//        {
//            for (INT i = 0; i < 8; i++)
//            {
//                reg[i] = 0x00;
//            }

//            prg0 = 0;
//            //	prg1 = 1;
//            SetBank_CPU();

//            chr0 = 0;
//            chr1 = 0;
//            chr2 = 0;
//            chr3 = 0;
//            highbank = 0;
//            SetBank_PPU();
//        }

//        //void Mapper191::WriteLow(WORD addr, BYTE data)
//        public override void WriteLow(ushort addr, byte data)
//        {
//            switch (addr)
//            {
//                case 0x4100:
//                    reg[0] = data;
//                    break;
//                case 0x4101:
//                    reg[1] = data;
//                    switch (reg[0])
//                    {
//                        case 0:
//                            chr0 = (byte)(data & 7);
//                            SetBank_PPU();
//                            break;
//                        case 1:
//                            chr1 = (byte)(data & 7);
//                            SetBank_PPU();
//                            break;
//                        case 2:
//                            chr2 = (byte)(data & 7);
//                            SetBank_PPU();
//                            break;
//                        case 3:
//                            chr3 = (byte)(data & 7);
//                            SetBank_PPU();
//                            break;
//                        case 4:
//                            highbank = (byte)(data & 7);
//                            SetBank_PPU();
//                            break;
//                        case 5:
//                            prg0 = (byte)(data & 7);
//                            SetBank_CPU();
//                            break;
//                        case 7:
//                            if ((data & 0x02) != 0) SetVRAM_Mirror(VRAM_HMIRROR);
//                            else SetVRAM_Mirror(VRAM_VMIRROR);
//                            break;
//                    }
//                    break;
//            }
//        }

//        void SetBank_CPU()
//        {
//            SetPROM_32K_Bank(prg0);
//        }

//        void SetBank_PPU()
//        {
//            if (VROM_1K_SIZE != 0)
//            {
//                SetVROM_1K_Bank(0, (((highbank << 3) + chr0) << 2) + 0);
//                SetVROM_1K_Bank(1, (((highbank << 3) + chr0) << 2) + 1);
//                SetVROM_1K_Bank(2, (((highbank << 3) + chr1) << 2) + 2);
//                SetVROM_1K_Bank(3, (((highbank << 3) + chr1) << 2) + 3);
//                SetVROM_1K_Bank(4, (((highbank << 3) + chr2) << 2) + 0);
//                SetVROM_1K_Bank(5, (((highbank << 3) + chr2) << 2) + 1);
//                SetVROM_1K_Bank(6, (((highbank << 3) + chr3) << 2) + 2);
//                SetVROM_1K_Bank(7, (((highbank << 3) + chr3) << 2) + 3);
//            }
//        }

//        public override bool IsStateSave()
//        {
//            return true;
//        }



//        //void Mapper191::SaveState(LPBYTE p)
//        public override void SaveState(byte[] p)
//        {
//            p[0] = prg0;
//            p[1] = chr0;
//            p[2] = chr1;
//            p[3] = chr2;
//            p[4] = chr3;
//            p[5] = highbank;
//        }

//        //void Mapper191::LoadState(LPBYTE p)
//        public override void LoadState(byte[] p)
//        {
//            prg0 = p[0];
//            chr0 = p[1];
//            chr1 = p[2];
//            chr2 = p[3];
//            chr3 = p[4];
//            highbank = p[5];
//        }
//    }
//}
