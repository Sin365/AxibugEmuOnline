//////////////////////////////////////////////////////////////////////////
// Mapper142  SMB2J                                                     //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
    public class Mapper142 : Mapper
    {
        BYTE prg_sel;
        BYTE irq_enable;
        INT irq_counter;
        public Mapper142(NES parent) : base(parent)
        {
        }

        public override void Reset()

        {
            prg_sel = 0;
            irq_enable = 0;
            irq_counter = 0;

            SetPROM_8K_Bank(3, 0);
            SetPROM_8K_Bank(7, 0x0F);

            if (VROM_1K_SIZE != 0)
            {
                SetVROM_8K_Bank(0);
            }
        }

        //void Mapper142::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            switch (addr & 0xF000)
            {
                case 0x8000:
                    irq_counter = (irq_counter & 0xFFF0) | ((data & 0x0F) << 0);
                    break;
                case 0x9000:
                    irq_counter = (irq_counter & 0xFF0F) | ((data & 0x0F) << 4);
                    break;
                case 0xA000:
                    irq_counter = (irq_counter & 0xF0FF) | ((data & 0x0F) << 8);
                    break;
                case 0xB000:
                    irq_counter = (irq_counter & 0x0FFF) | ((data & 0x0F) << 12);
                    break;
                case 0xC000:
                    irq_enable = (byte)(data & 0x0F);
                    nes.cpu.ClrIRQ(IRQ_MAPPER);
                    break;
                case 0xE000:
                    prg_sel = (byte)(data & 0x0F);
                    break;
                case 0xF000:
                    switch (prg_sel)
                    {
                        case 1: SetPROM_8K_Bank(4, data & 0x0F); break;
                        case 2: SetPROM_8K_Bank(5, data & 0x0F); break;
                        case 3: SetPROM_8K_Bank(6, data & 0x0F); break;
                        case 4: SetPROM_8K_Bank(3, data & 0x0F); break;
                    }
                    break;
            }
        }

        //void Mapper142::HSync(INT scanline)
        public override void HSync(int scanline)
        {
            if (irq_enable != 0)
            {
                if (irq_counter > (0xFFFF - 113))
                {
                    irq_counter = 0;
                    nes.cpu.SetIRQ(IRQ_MAPPER);
                }
                else
                {
                    irq_counter += 113;
                }
            }
        }

        //void Mapper142::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            //p[0] = prg_sel;
            //p[0] = irq_enable;
            //*(INT*)&p[2] = irq_counter;
        }

        //void Mapper142::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            //prg_sel = p[0];
            //irq_enable = p[1];
            //irq_counter = *(INT*)&p[2];
        }
        public override bool IsStateSave()
        {
            return true;
        }


    }
}
