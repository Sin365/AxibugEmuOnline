//////////////////////////////////////////////////////////////////////////
// Mapper090  PC-JY-??                                                  //
//////////////////////////////////////////////////////////////////////////
using VirtualNes.Core.Debug;
using static VirtualNes.Core.CPU;
using static VirtualNes.MMU;
using BYTE = System.Byte;
using INT = System.Int32;

namespace VirtualNes.Core
{
    public class Mapper090 : Mapper
    {
        BYTE patch;

        BYTE[] prg_reg = new byte[4];
        BYTE[] nth_reg = new byte[4];
        BYTE[] ntl_reg = new byte[4];
        BYTE[] chh_reg = new byte[8];
        BYTE[] chl_reg = new byte[8];

        BYTE irq_enable;
        BYTE irq_counter;
        BYTE irq_latch;
        BYTE irq_occur;
        BYTE irq_preset;
        BYTE irq_offset;

        BYTE prg_6000, prg_E000;
        BYTE prg_size, chr_size;
        BYTE mir_mode, mir_type;

        BYTE key_val;
        BYTE mul_val1, mul_val2;
        BYTE sw_val;
        public Mapper090(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(PROM_8K_SIZE - 4, PROM_8K_SIZE - 3, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);
            SetVROM_8K_Bank(0);
            patch = 0;

            uint crc = nes.rom.GetPROM_CRC();

            if (crc == 0x2a268152)
            {
                patch = 1;
            }
            if (crc == 0x2224b882)
            {
                nes.SetRenderMethod(EnumRenderMethod.TILE_RENDER);
            }

            irq_enable = 0; // Disable
            irq_counter = 0;
            irq_latch = 0;
            irq_occur = 0;
            irq_preset = 0;
            irq_offset = 0;

            prg_6000 = 0;
            prg_E000 = 0;
            prg_size = 0;
            chr_size = 0;
            mir_mode = 0;
            mir_type = 0;

            key_val = 0;
            mul_val1 = mul_val2 = 0;

            for (byte i = 0; i < 4; i++)
            {
                prg_reg[i] = (byte)(PROM_8K_SIZE - 4 + i);
                ntl_reg[i] = 0;
                nth_reg[i] = 0;
                chl_reg[i] = i;
                chh_reg[i] = 0;
                chl_reg[i + 4] = (byte)(i + 4);
                chh_reg[i + 4] = 0;
            }

            if (sw_val != 0)
                sw_val = 0x00;
            else
                sw_val = 0xFF;

            //	nes.SetRenderMethod( NES::PRE_ALL_RENDER );
        }

        //BYTE Mapper090::ReadLow(WORD addr)
        public override byte ReadLow(ushort addr)
        {
            Debuger.Log($"RD:{addr:X4}");

            switch (addr)
            {
                case 0x5000:
                    return (byte)((sw_val != 0) ? 0x00 : 0xFF);
                case 0x5800:
                    return (BYTE)(mul_val1 * mul_val2);
                case 0x5801:
                    return (BYTE)((mul_val1 * mul_val2) >> 8);
                case 0x5803:
                    return key_val;
            }

            if (addr >= 0x6000)
            {
                return base.ReadLow(addr);
            }

            //	return	sw_val?0x00:0xFF;
            return (BYTE)(addr >> 8);
        }

        //void Mapper090::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            Debuger.Log($"WR:{addr:X4} {data:X2}");

            if (addr == 0x5800)
            {
                mul_val1 = data;
            }
            else
            if (addr == 0x5801)
            {
                mul_val2 = data;
            }
            else
            if (addr == 0x5803)
            {
                key_val = data;
            }
        }

        //void Mapper090::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            switch (addr & 0xF007)
            {
                case 0x8000:
                case 0x8001:
                case 0x8002:
                case 0x8003:
                    prg_reg[addr & 3] = data;
                    SetBank_CPU();
                    break;

                case 0x9000:
                case 0x9001:
                case 0x9002:
                case 0x9003:
                case 0x9004:
                case 0x9005:
                case 0x9006:
                case 0x9007:
                    chl_reg[addr & 7] = data;
                    SetBank_PPU();
                    break;

                case 0xA000:
                case 0xA001:
                case 0xA002:
                case 0xA003:
                case 0xA004:
                case 0xA005:
                case 0xA006:
                case 0xA007:
                    chh_reg[addr & 7] = data;
                    SetBank_PPU();
                    break;

                case 0xB000:
                case 0xB001:
                case 0xB002:
                case 0xB003:
                    ntl_reg[addr & 3] = data;
                    SetBank_VRAM();
                    break;

                case 0xB004:
                case 0xB005:
                case 0xB006:
                case 0xB007:
                    nth_reg[addr & 3] = data;
                    SetBank_VRAM();
                    break;

                case 0xC002:
                    irq_enable = 0;
                    irq_occur = 0;
                    nes.cpu.ClrIRQ(IRQ_MAPPER);
                    break;
                case 0xC003:
                    irq_enable = 0xFF;
                    irq_preset = 0xFF;
                    break;
                case 0xC004:
                    break;
                case 0xC005:
                    if ((irq_offset & 0x80) != 0)
                    {
                        irq_latch = (byte)(data ^ (irq_offset | 1));
                    }
                    else
                    {
                        irq_latch = (byte)(data | (irq_offset & 0x27));
                    }
                    irq_preset = 0xFF;
                    break;
                case 0xC006:
                    if (patch != 0)
                    {
                        irq_offset = data;
                    }
                    break;

                case 0xD000:
                    prg_6000 = (byte)(data & 0x80);
                    prg_E000 = (byte)(data & 0x04);
                    prg_size = (byte)(data & 0x03);
                    chr_size = (byte)((data & 0x18) >> 3);
                    mir_mode = (byte)(data & 0x20);
                    SetBank_CPU();
                    SetBank_PPU();
                    SetBank_VRAM();
                    break;

                case 0xD001:
                    mir_type = (byte)(data & 0x03);
                    SetBank_VRAM();
                    break;

                case 0xD003:
                    break;
            }
        }

        //void Mapper090::HSync(INT scanline)
        public override void HSync(int scanline)
        {
            if ((scanline >= 0 && scanline <= 239))
            {
                if (nes.ppu.IsDispON())
                {
                    if (irq_preset != 0)
                    {
                        irq_counter = irq_latch;
                        irq_preset = 0;
                    }
                    if (irq_counter != 0)
                    {
                        irq_counter--;
                    }
                    if (irq_counter == 0)
                    {
                        if (irq_enable != 0)
                        {
                            //					irq_occur = 0xFF;
                            nes.cpu.SetIRQ(IRQ_MAPPER);
                        }
                    }
                }
            }
        }

        //void Mapper090::Clock(INT cycles)
        public override void Clock(int cycles)
        {
            //	if( irq_occur ) {
            //		nes.cpu.IRQ_NotPending();
            //	}
        }

        void SetBank_CPU()
        {
            if (prg_size == 0)
            {
                SetPROM_32K_Bank(PROM_8K_SIZE - 4, PROM_8K_SIZE - 3, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);
            }
            else
            if (prg_size == 1)
            {
                SetPROM_32K_Bank(prg_reg[1] * 2, prg_reg[1] * 2 + 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);
            }
            else
            if (prg_size == 2)
            {
                if (prg_E000 != 0)
                {
                    SetPROM_32K_Bank(prg_reg[0], prg_reg[1], prg_reg[2], prg_reg[3]);
                }
                else
                {
                    if (prg_6000 != 0)
                    {
                        SetPROM_8K_Bank(3, prg_reg[3]);
                    }
                    SetPROM_32K_Bank(prg_reg[0], prg_reg[1], prg_reg[2], PROM_8K_SIZE - 1);
                }
            }
            else
            {
                SetPROM_32K_Bank(prg_reg[3], prg_reg[2], prg_reg[1], prg_reg[0]);
            }
        }

        void SetBank_PPU()
        {
            INT[] bank = new int[8];

            for (INT i = 0; i < 8; i++)
            {
                bank[i] = ((INT)chh_reg[i] << 8) | ((INT)chl_reg[i]);
            }

            if (chr_size == 0)
            {
                SetVROM_8K_Bank(bank[0]);
            }
            else
            if (chr_size == 1)
            {
                SetVROM_4K_Bank(0, bank[0]);
                SetVROM_4K_Bank(4, bank[4]);
            }
            else
            if (chr_size == 2)
            {
                SetVROM_2K_Bank(0, bank[0]);
                SetVROM_2K_Bank(2, bank[2]);
                SetVROM_2K_Bank(4, bank[4]);
                SetVROM_2K_Bank(6, bank[6]);
            }
            else
            {
                SetVROM_8K_Bank(bank[0], bank[1], bank[2], bank[3], bank[4], bank[5], bank[6], bank[7]);
            }
        }

        void SetBank_VRAM()
        {
            INT[] bank = new int[4];

            for (INT i = 0; i < 4; i++)
            {
                bank[i] = ((INT)nth_reg[i] << 8) | ((INT)ntl_reg[i]);
            }

            if (patch == 0 && mir_mode != 0)
            {
                for (INT i = 0; i < 4; i++)
                {
                    if (nth_reg[i] == 0 && (ntl_reg[i] == (BYTE)i))
                    {
                        mir_mode = 0;
                    }
                }

                if (mir_mode != 0)
                {
                    SetVROM_1K_Bank(8, bank[0]);
                    SetVROM_1K_Bank(9, bank[1]);
                    SetVROM_1K_Bank(10, bank[2]);
                    SetVROM_1K_Bank(11, bank[3]);
                }
            }
            else
            {
                if (mir_type == 0)
                {
                    SetVRAM_Mirror(VRAM_VMIRROR);
                }
                else
                if (mir_type == 1)
                {
                    SetVRAM_Mirror(VRAM_HMIRROR);
                }
                else
                {
                    SetVRAM_Mirror(VRAM_MIRROR4L);
                }
            }
        }

        //void Mapper090::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            INT i;

            for (i = 0; i < 4; i++)
            {
                p[i] = prg_reg[i];
            }
            for (i = 0; i < 8; i++)
            {
                p[i + 4] = chh_reg[i];
            }
            for (i = 0; i < 8; i++)
            {
                p[i + 12] = chl_reg[i];
            }
            for (i = 0; i < 4; i++)
            {
                p[i + 20] = nth_reg[i];
            }
            for (i = 0; i < 4; i++)
            {
                p[i + 24] = ntl_reg[i];
            }
            p[28] = irq_enable;
            p[29] = irq_counter;
            p[30] = irq_latch;
            p[31] = prg_6000;
            p[32] = prg_E000;
            p[33] = prg_size;
            p[34] = chr_size;
            p[35] = mir_mode;
            p[36] = mir_type;
            p[37] = mul_val1;
            p[38] = mul_val2;
            p[39] = key_val;
            p[40] = irq_occur;
            p[41] = irq_preset;
            p[42] = irq_offset;
        }

        //void Mapper090::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            INT i;

            for (i = 0; i < 4; i++)
            {
                prg_reg[i] = p[i];
            }
            for (i = 0; i < 8; i++)
            {
                chh_reg[i] = p[i + 4];
            }
            for (i = 0; i < 8; i++)
            {
                chl_reg[i] = p[i + 12];
            }
            for (i = 0; i < 4; i++)
            {
                nth_reg[i] = p[i + 20];
            }
            for (i = 0; i < 4; i++)
            {
                ntl_reg[i] = p[i + 24];
            }
            irq_enable = p[28];
            irq_counter = p[29];
            irq_latch = p[30];
            prg_6000 = p[31];
            prg_E000 = p[32];
            prg_size = p[33];
            chr_size = p[34];
            mir_mode = p[35];
            mir_type = p[36];
            mul_val1 = p[37];
            mul_val2 = p[38];
            key_val = p[39];
            irq_occur = p[40];
            irq_preset = p[41];
            irq_offset = p[42];
        }


        public override bool IsStateSave()
        {
            return true;
        }

    }
}
