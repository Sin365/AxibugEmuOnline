﻿//////////////////////////////////////////////////////////////////////////
// Mapper006  FFE F4xxx                                                 //
//////////////////////////////////////////////////////////////////////////
using System;
using static VirtualNes.Core.CPU;
using static VirtualNes.MMU;
using BYTE = System.Byte;
using INT = System.Int32;


namespace VirtualNes.Core
{
    public class Mapper006 : Mapper
    {
        BYTE irq_enable;
        INT irq_counter;
        public Mapper006(NES parent) : base(parent) { }

        public override void Reset()
        {
            SetPROM_32K_Bank(0, 1, 14, 15);

            if (VROM_1K_SIZE != 0)
            {
                SetVROM_8K_Bank(0);
            }
            else
            {
                SetCRAM_8K_Bank(0);
            }

            irq_enable = 0;
            irq_counter = 0;
        }

        //void Mapper006::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            switch (addr)
            {
                case 0x42FE:
                    if ((data & 0x10) != 0) SetVRAM_Mirror(VRAM_MIRROR4H);
                    else SetVRAM_Mirror(VRAM_MIRROR4L);
                    break;
                case 0x42FF:
                    if ((data & 0x10) != 0) SetVRAM_Mirror(VRAM_HMIRROR);
                    else SetVRAM_Mirror(VRAM_VMIRROR);
                    break;

                case 0x4501:
                    irq_enable = 0;

                    nes.cpu.ClrIRQ(IRQ_MAPPER);
                    break;
                case 0x4502:
                    irq_counter = (irq_counter & 0xFF00) | data;
                    break;
                case 0x4503:
                    irq_counter = (irq_counter & 0x00FF) | ((INT)data << 8);
                    irq_enable = 0xFF;

                    nes.cpu.ClrIRQ(IRQ_MAPPER);
                    break;
                default:
                    base.WriteLow(addr, data);
                    break;
            }
        }

        //void Mapper006::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            SetPROM_16K_Bank(4, (data & 0x3C) >> 2);
            SetCRAM_8K_Bank(data & 0x03);
        }

        //void Mapper006::HSync(INT scanline)
        public override void HSync(int scanline)
        {
            if (irq_enable != 0)
            {
                irq_counter += 133;
                if (irq_counter >= 0xFFFF)
                {
                    //			nes.cpu.IRQ();
                    irq_counter = 0;

                    nes.cpu.SetIRQ(IRQ_MAPPER);
                }
            }
        }

        //void Mapper006::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            p[0] = irq_enable;
            //*(INT*)&p[1] = irq_counter;
            BitConverter.GetBytes(irq_counter).CopyTo(p, 1);
        }

        //void Mapper006::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            irq_enable = p[0];
            //irq_counter = *(INT*)&p[1];
            irq_counter = BitConverter.ToInt32(p, 1);
        }


        public override bool IsStateSave()
        {
            return true;
        }
    }
}
