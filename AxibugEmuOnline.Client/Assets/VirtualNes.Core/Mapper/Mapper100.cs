﻿using static VirtualNes.Core.CPU;
using static VirtualNes.MMU;
using BYTE = System.Byte;
using INT = System.Int32;


namespace VirtualNes.Core
{
    public class Mapper100 : Mapper
    {

        BYTE[] reg = new byte[8];
        BYTE prg0, prg1, prg2, prg3;
        BYTE chr0, chr1, chr2, chr3, chr4, chr5, chr6, chr7;

        BYTE irq_enable;
        BYTE irq_counter;
        BYTE irq_latch;
        public Mapper100(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            for (INT i = 0; i < 8; i++)
            {
                reg[i] = 0x00;
            }

            prg0 = 0;
            prg1 = 1;
            prg2 = (byte)(PROM_8K_SIZE - 2);
            prg3 = (byte)(PROM_8K_SIZE - 1);
            SetBank_CPU();

            if (VROM_1K_SIZE != 0)
            {
                chr0 = 0;
                chr1 = 1;
                chr2 = 2;
                chr3 = 3;
                chr4 = 4;
                chr5 = 5;
                chr6 = 6;
                chr7 = 7;
                SetBank_PPU();
            }
            else
            {
                chr0 = chr2 = chr4 = chr5 = chr6 = chr7 = 0;
                chr1 = chr3 = 1;
            }

            irq_enable = 0; // Disable
            irq_counter = 0;
            irq_latch = 0;
        }

        //void Mapper100::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            switch (addr & 0xE001)
            {
                case 0x8000:
                    reg[0] = data;
                    break;
                case 0x8001:
                    reg[1] = data;

                    switch (reg[0] & 0xC7)
                    {
                        case 0x00:
                            if (VROM_1K_SIZE != 0)
                            {
                                chr0 = (byte)(data & 0xFE);
                                chr1 = (byte)(chr0 + 1);
                                SetBank_PPU();
                            }
                            break;
                        case 0x01:
                            if (VROM_1K_SIZE != 0)
                            {
                                chr2 = (byte)(data & 0xFE);
                                chr3 = (byte)(chr2 + 1);
                                SetBank_PPU();
                            }
                            break;
                        case 0x02:
                            if (VROM_1K_SIZE != 0)
                            {
                                chr4 = data;
                                SetBank_PPU();
                            }
                            break;
                        case 0x03:
                            if (VROM_1K_SIZE != 0)
                            {
                                chr5 = data;
                                SetBank_PPU();
                            }
                            break;
                        case 0x04:
                            if (VROM_1K_SIZE != 0)
                            {
                                chr6 = data;
                                SetBank_PPU();
                            }
                            break;
                        case 0x05:
                            if (VROM_1K_SIZE != 0)
                            {
                                chr7 = data;
                                SetBank_PPU();
                            }
                            break;

                        case 0x06:
                            prg0 = data;
                            SetBank_CPU();
                            break;
                        case 0x07:
                            prg1 = data;
                            SetBank_CPU();
                            break;
                        case 0x46:
                            prg2 = data;
                            SetBank_CPU();
                            break;
                        case 0x47:
                            prg3 = data;
                            SetBank_CPU();
                            break;

                        case 0x80:
                            if (VROM_1K_SIZE != 0)
                            {
                                chr4 = (byte)(data & 0xFE);
                                chr5 = (byte)(chr4 + 1);
                                SetBank_PPU();
                            }
                            break;
                        case 0x81:
                            if (VROM_1K_SIZE != 0)
                            {
                                chr6 = (byte)(data & 0xFE);
                                chr7 = (byte)(chr6 + 1);
                                SetBank_PPU();
                            }
                            break;
                        case 0x82:
                            if (VROM_1K_SIZE != 0)
                            {
                                chr0 = data;
                                SetBank_PPU();
                            }
                            break;
                        case 0x83:
                            if (VROM_1K_SIZE != 0)
                            {
                                chr1 = data;
                                SetBank_PPU();
                            }
                            break;
                        case 0x84:
                            if (VROM_1K_SIZE != 0)
                            {
                                chr2 = data;
                                SetBank_PPU();
                            }
                            break;
                        case 0x85:
                            if (VROM_1K_SIZE != 0)
                            {
                                chr3 = data;
                                SetBank_PPU();
                            }
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
                    break;
                case 0xC001:
                    reg[5] = data;
                    irq_latch = data;
                    break;
                case 0xE000:
                    reg[6] = data;
                    irq_enable = 0;
                    nes.cpu.ClrIRQ(IRQ_MAPPER);
                    break;
                case 0xE001:
                    reg[7] = data;
                    irq_enable = 0xFF;
                    break;
            }
        }

        //void Mapper100::HSync(INT scanline)
        public override void HSync(int scanline)
        {
            if ((scanline >= 0 && scanline <= 239))
            {
                if (nes.ppu.IsDispON())
                {
                    if (irq_enable != 0)
                    {
                        if ((irq_counter--) == 0)
                        {
                            irq_counter = irq_latch;
                            //					nes.cpu.IRQ();
                            nes.cpu.SetIRQ(IRQ_MAPPER);
                        }
                    }
                }
            }
        }

        void SetBank_CPU()
        {
            SetPROM_32K_Bank(prg0, prg1, prg2, prg3);
        }

        void SetBank_PPU()
        {
            if (VROM_1K_SIZE != 0)
            {
                SetVROM_8K_Bank(chr0, chr1, chr2, chr3, chr4, chr5, chr6, chr7);
            }
        }

        //void Mapper100::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            for (byte i = 0; i < 8; i++)
            {
                p[i] = reg[i];
            }
            p[8] = prg0;
            p[9] = prg1;
            p[10] = prg2;
            p[11] = prg3;
            p[12] = chr0;
            p[13] = chr1;
            p[14] = chr2;
            p[15] = chr3;
            p[16] = chr4;
            p[17] = chr5;
            p[18] = chr6;
            p[19] = chr7;
            p[20] = irq_enable;
            p[21] = irq_counter;
            p[22] = irq_latch;
        }

        //void Mapper100::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            for (byte i = 0; i < 8; i++)
            {
                reg[i] = p[i];
            }
            prg0 = p[8];
            prg1 = p[9];
            prg2 = p[10];
            prg3 = p[11];
            chr0 = p[12];
            chr1 = p[13];
            chr2 = p[14];
            chr3 = p[15];
            chr4 = p[16];
            chr5 = p[17];
            chr6 = p[18];
            chr7 = p[19];
            irq_enable = p[20];
            irq_counter = p[21];
            irq_latch = p[22];
        }


        public override bool IsStateSave()
        {
            return true;
        }

    }
}
