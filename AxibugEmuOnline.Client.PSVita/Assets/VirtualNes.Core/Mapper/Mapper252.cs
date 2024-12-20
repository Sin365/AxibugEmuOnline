﻿using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;

namespace VirtualNes.Core
{
    public class Mapper252 : Mapper
    {
        BYTE[] reg = new BYTE[9];
        BYTE irq_enable;
        BYTE irq_counter;
        BYTE irq_latch;
        BYTE irq_occur;
        INT irq_clock;

        public Mapper252(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            for (byte i = 0; i < 8; i++)
            {
                reg[i] = i;
            }

            reg[8] = 0;

            irq_enable = 0;
            irq_counter = 0;
            irq_latch = 0;
            irq_clock = 0;
            irq_occur = 0;

            SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);
            SetVROM_8K_Bank(0);

            nes.SetRenderMethod(EnumRenderMethod.POST_RENDER);
        }

        public override void Write(ushort addr, byte data)
        {
         
            if ((addr & 0xF000) == 0x8000)
            {
                SetPROM_8K_Bank(4, data);
                return;
            }
            if ((addr & 0xF000) == 0xA000)
            {
                SetPROM_8K_Bank(5, data);
                return;
            }
            switch (addr & 0xF00C)
            {
                case 0xB000:
                    reg[0] = (byte)((reg[0] & 0xF0) | (data & 0x0F));
                    SetVROM_1K_Bank(0, reg[0]);
                    break;
                case 0xB004:
                    reg[0] = (byte)((reg[0] & 0x0F) | ((data & 0x0F) << 4));
                    SetVROM_1K_Bank(0, reg[0]);
                    break;
                case 0xB008:
                    reg[1] = (byte)((reg[1] & 0xF0) | (data & 0x0F));
                    SetVROM_1K_Bank(1, reg[1]);
                    break;
                case 0xB00C:
                    reg[1] = (byte)((reg[1] & 0x0F) | ((data & 0x0F) << 4));
                    SetVROM_1K_Bank(1, reg[1]);
                    break;

                case 0xC000:
                    reg[2] = (byte)((reg[2] & 0xF0) | (data & 0x0F));
                    SetVROM_1K_Bank(2, reg[2]);
                    break;
                case 0xC004:
                    reg[2] = (byte)(((reg[2] & 0x0F) | ((data & 0x0F) << 4)));
                    SetVROM_1K_Bank(2, reg[2]);
                    break;
                case 0xC008:
                    reg[3] = (byte)((reg[3] & 0xF0) | (data & 0x0F));
                    SetVROM_1K_Bank(3, reg[3]);
                    break;
                case 0xC00C:
                    reg[3] = (byte)((reg[3] & 0x0F) | ((data & 0x0F) << 4));
                    SetVROM_1K_Bank(3, reg[3]);
                    break;

                case 0xD000:
                    reg[4] = (byte)((reg[4] & 0xF0) | (data & 0x0F));
                    SetVROM_1K_Bank(4, reg[4]);
                    break;
                case 0xD004:
                    reg[4] = (byte)((reg[4] & 0x0F) | ((data & 0x0F) << 4));
                    SetVROM_1K_Bank(4, reg[4]);
                    break;
                case 0xD008:
                    reg[5] = (byte)((reg[5] & 0xF0) | (data & 0x0F));
                    SetVROM_1K_Bank(5, reg[5]);
                    break;
                case 0xD00C:
                    reg[5] = (byte)((reg[5] & 0x0F) | ((data & 0x0F) << 4));
                    SetVROM_1K_Bank(5, reg[5]);
                    break;

                case 0xE000:
                    reg[6] = (byte)((reg[6] & 0xF0) | (data & 0x0F));
                    SetVROM_1K_Bank(6, reg[6]);
                    break;
                case 0xE004:
                    reg[6] = (byte)((reg[6] & 0x0F) | ((data & 0x0F) << 4));
                    SetVROM_1K_Bank(6, reg[6]);
                    break;
                case 0xE008:
                    reg[7] = (byte)((reg[7] & 0xF0) | (data & 0x0F));
                    SetVROM_1K_Bank(7, reg[7]);
                    break;
                case 0xE00C:
                    reg[7] = (byte)((reg[7] & 0x0F) | ((data & 0x0F) << 4));
                    SetVROM_1K_Bank(7, reg[7]);
                    break;

                case 0xF000:
                    irq_latch = (byte)((irq_latch & 0xF0) | (data & 0x0F));
                    irq_occur = 0;
                    break;
                case 0xF004:
                    irq_latch = (byte)((irq_latch & 0x0F) | ((data & 0x0F) << 4));
                    irq_occur = 0;
                    break;

                case 0xF008:
                    irq_enable = (byte)(data & 0x03);
                    if ((irq_enable & 0x02) != 0)
                    {
                        irq_counter = irq_latch;
                        irq_clock = 0;
                    }
                    irq_occur = 0;
                    nes.cpu.ClrIRQ(IRQ_MAPPER);
                    break;

                case 0xF00C:
                    irq_enable = (byte)((irq_enable & 0x01) * 3);
                    irq_occur = 0;
                    nes.cpu.ClrIRQ(IRQ_MAPPER);
                    break;
            }
        }

        //void Mapper252::Clock(INT cycles)
        public override void Clock(int cycles)
        {
            if ((irq_enable & 0x02) != 0)
            {
                if ((irq_clock += cycles) >= 0x72)
                {
                    irq_clock -= 0x72;
                    if (irq_counter == 0xFF)
                    {
                        irq_occur = 0xFF;
                        irq_counter = irq_latch;
                        irq_enable = (byte)((irq_enable & 0x01) * 3);

                        nes.cpu.SetIRQ(IRQ_MAPPER);
                    }
                    else
                    {
                        irq_counter++;
                    }
                }
                //		if( irq_occur ) {
                //			nes->cpu->IRQ_NotPending();
                //		}
            }
        }
        public override bool IsStateSave()
        {
            return true;
        }
        //void Mapper252::SaveState(LPBYTE p)
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
            Array.Copy(p, 12, BitConverter.GetBytes(irq_clock), 0, 4);
        }

        //void Mapper252::LoadState(LPBYTE p)
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
            irq_clock = BitConverter.ToInt32(p,12);
        }
    }
}
