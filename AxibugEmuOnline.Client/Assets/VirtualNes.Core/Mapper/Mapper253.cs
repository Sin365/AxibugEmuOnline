﻿//////////////////////////////////////////////////////////////////////////
// Mapper253         WaiXing LongZhu CN                                 //
//////////////////////////////////////////////////////////////////////////
using System;
using static VirtualNes.Core.CPU;
using static VirtualNes.MMU;
using BYTE = System.Byte;
using INT = System.Int32;

namespace VirtualNes.Core
{
    public class Mapper253 : Mapper
    {
        BYTE VRAM_switch;
        BYTE rom_type;
        BYTE[] reg = new BYTE[9];
        BYTE irq_enable;
        BYTE irq_counter;
        BYTE irq_latch;
        INT irq_clock;
        public Mapper253(NES parent) : base(parent)
        {
        }

        public override bool IsStateSave()
        {
            return true;
        }

        //void Mapper253::Reset()
        public override void Reset()
        {
            //	nes.ppu.SetVromWrite(1);
            for (INT i = 0; i < 8; i++)
            {
                reg[i] = (byte)i;
            }
            reg[8] = 0;

            irq_enable = 0;
            irq_counter = 0;
            irq_latch = 0;
            irq_clock = 0;
            VRAM_switch = 0;
            rom_type = 0;
            SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);
            SetVROM_8K_Bank(0);

            uint crc = nes.rom.GetPROM_CRC();
            if (crc == 0x0530b26e)
            {   //[HengGe] Shen Hua Jian Yun III (C)
                rom_type = 1;
            }
        }

        //void Mapper253::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            Debug.Debuger.Log($"Address={addr & 0xFFFF} Data={data & 0xFF}");

            if (addr == 0x8010)
            {
                SetPROM_8K_Bank(4, data);
                return;
            }
            if (addr == 0xA010)
            {
                SetPROM_8K_Bank(5, data);
                return;
            }
            if (addr == 0x9400)
            {
                data &= 0x03;
                if (data == 0) SetVRAM_Mirror(VRAM_VMIRROR);
                else if (data == 1) SetVRAM_Mirror(VRAM_HMIRROR);
                else if (data == 2) SetVRAM_Mirror(VRAM_MIRROR4L);
                else SetVRAM_Mirror(VRAM_MIRROR4H);
            }
            switch (addr & 0xF00C)
            {
                case 0xB000:
                    reg[0] = (byte)((reg[0] & 0xF0) | (data & 0x0F));
                    SetBank_PPUSUB(0, reg[0]);
                    break;
                case 0xB004:
                    reg[0] = (byte)((reg[0] & 0x0F) | ((data & 0x0F) << 4));
                    SetBank_PPUSUB(0, reg[0] + ((data >> 4) * 0x100));
                    break;
                case 0xB008:
                    reg[1] = (byte)((reg[1] & 0xF0) | (data & 0x0F));
                    SetBank_PPUSUB(1, reg[1]);
                    break;
                case 0xB00C:
                    reg[1] = (byte)((reg[1] & 0x0F) | ((data & 0x0F) << 4));
                    SetBank_PPUSUB(1, reg[1] + ((data >> 4) * 0x100));
                    break;
                case 0xC000:
                    reg[2] = (byte)((reg[2] & 0xF0) | (data & 0x0F));
                    SetBank_PPUSUB(2, reg[2]);
                    break;
                case 0xC004:
                    reg[2] = (byte)((reg[2] & 0x0F) | ((data & 0x0F) << 4));
                    SetBank_PPUSUB(2, reg[2] + ((data >> 4) * 0x100));
                    break;
                case 0xC008:
                    reg[3] = (byte)((reg[3] & 0xF0) | (data & 0x0F));
                    SetBank_PPUSUB(3, reg[3]);
                    break;
                case 0xC00C:
                    reg[3] = (byte)((reg[3] & 0x0F) | ((data & 0x0F) << 4));
                    SetBank_PPUSUB(3, reg[3] + ((data >> 4) * 0x100));
                    break;
                case 0xD000:
                    reg[4] = (byte)((reg[4] & 0xF0) | (data & 0x0F));
                    SetBank_PPUSUB(4, reg[4]);
                    break;
                case 0xD004:
                    reg[4] = (byte)((reg[4] & 0x0F) | ((data & 0x0F) << 4));
                    SetBank_PPUSUB(4, reg[4] + ((data >> 4) * 0x100));
                    break;
                case 0xD008:
                    reg[5] = (byte)((reg[5] & 0xF0) | (data & 0x0F));
                    SetBank_PPUSUB(5, reg[5]);
                    break;
                case 0xD00C:
                    reg[5] = (byte)((reg[5] & 0x0F) | ((data & 0x0F) << 4));
                    SetBank_PPUSUB(5, reg[5] + ((data >> 4) * 0x100));
                    break;
                case 0xE000:
                    reg[6] = (byte)((reg[6] & 0xF0) | (data & 0x0F));
                    SetBank_PPUSUB(6, reg[6]);
                    break;
                case 0xE004:
                    reg[6] = (byte)((reg[6] & 0x0F) | ((data & 0x0F) << 4));
                    SetBank_PPUSUB(6, reg[6] + ((data >> 4) * 0x100));
                    break;
                case 0xE008:
                    reg[7] = (byte)((reg[7] & 0xF0) | (data & 0x0F));
                    SetBank_PPUSUB(7, reg[7]);
                    break;
                case 0xE00C:
                    reg[7] = (byte)((reg[7] & 0x0F) | ((data & 0x0F) << 4));
                    SetBank_PPUSUB(7, reg[7] + ((data >> 4) * 0x100));
                    break;
                case 0xF000:
                    irq_latch = (byte)((irq_latch & 0xF0) | (data & 0x0F));
                    break;
                case 0xF004:
                    irq_latch = (byte)((irq_latch & 0x0F) | ((data & 0x0F) << 4));
                    break;
                case 0xF008:
                    irq_enable = (byte)(data & 0x03);
                    if ((irq_enable & 0x02) != null)
                    {
                        irq_counter = irq_latch;
                        irq_clock = 0;
                    }
                    nes.cpu.ClrIRQ(IRQ_MAPPER);
                    break;
            }
        }

        //void Mapper253::SetBank_PPUSUB(int bank, int page)
        private void SetBank_PPUSUB(int bank, int page)
        {
            if (rom_type == 1)
            {
                if (page == 0x88)
                {
                    VRAM_switch = 0;
                    return;
                }
                else if (page == 0xc8)
                {
                    VRAM_switch = 1;
                    return;
                }
            }
            if ((page == 4) || (page == 5))
            {
                if ((VRAM_switch == 0) && (rom_type == 1))
                {
                    SetVROM_1K_Bank((byte)bank, page);
                }
                else
                {
                    SetCRAM_1K_Bank((byte)bank, page);
                }
            }
            else
            {
                SetVROM_1K_Bank((byte)bank, page);
            }
        }

        //void Mapper253::Clock(INT cycles)
        public override void Clock(int cycles)
        {
            if ((irq_enable & 0x02)!= null)
            {
                if ((irq_clock += cycles) >= 0x72)
                {
                    irq_clock -= 0x72;
                    if (irq_counter == 0xFF)
                    {
                        irq_counter = irq_latch;
                        irq_enable = (BYTE)((irq_enable & 0x01) * 3);
                        nes.cpu.SetIRQ(IRQ_MAPPER);
                    }
                    else
                    {
                        irq_counter++;
                    }
                }
            }
        }

        //void Mapper253::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            for (INT i = 0; i < 9; i++)
            {
                p[i] = reg[i];
            }
            p[9] = irq_enable;
            p[10] = irq_counter;
            p[11] = irq_latch;
            //*(INT*)&p[12] = irq_clock;
            p[12] = (byte)irq_clock;
        }

        //void Mapper253::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            for (INT i = 0; i < 9; i++)
            {
                reg[i] = p[i];
            }
            irq_enable = p[9];
            irq_counter = p[10];
            irq_latch = p[11];
            //irq_clock = *(INT*)&p[12];
            irq_clock = p[12];
        }
    }
}
