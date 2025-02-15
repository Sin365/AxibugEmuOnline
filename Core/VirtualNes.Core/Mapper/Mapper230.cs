﻿//////////////////////////////////////////////////////////////////////////
// Mapper230  22-in-1                                                   //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using BYTE = System.Byte;


namespace VirtualNes.Core
{
    public class Mapper230 : Mapper
    {
        BYTE rom_sw;
        public Mapper230(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            if (rom_sw != 0)
            {
                rom_sw = 0;
            }
            else
            {
                rom_sw = 1;
            }
            if (rom_sw != 0)
            {
                SetPROM_32K_Bank(0, 1, 14, 15);
            }
            else
            {
                SetPROM_32K_Bank(16, 17, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);
            }
        }

        //void Mapper230::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            if (rom_sw != 0)
            {
                SetPROM_8K_Bank(4, (data & 0x07) * 2 + 0);
                SetPROM_8K_Bank(5, (data & 0x07) * 2 + 1);
            }
            else
            {
                if ((data & 0x20) != 0)
                {
                    SetPROM_8K_Bank(4, (data & 0x1F) * 2 + 16);
                    SetPROM_8K_Bank(5, (data & 0x1F) * 2 + 17);
                    SetPROM_8K_Bank(6, (data & 0x1F) * 2 + 16);
                    SetPROM_8K_Bank(7, (data & 0x1F) * 2 + 17);
                }
                else
                {
                    SetPROM_8K_Bank(4, (data & 0x1E) * 2 + 16);
                    SetPROM_8K_Bank(5, (data & 0x1E) * 2 + 17);
                    SetPROM_8K_Bank(6, (data & 0x1E) * 2 + 18);
                    SetPROM_8K_Bank(7, (data & 0x1E) * 2 + 19);
                }
                if ((data & 0x40) != 0)
                {
                    SetVRAM_Mirror(VRAM_VMIRROR);
                }
                else
                {
                    SetVRAM_Mirror(VRAM_HMIRROR);
                }
            }
        }

    }
}
