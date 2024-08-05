//////////////////////////////////////////////////////////////////////////
// Mapper185  Character disable protect                                 //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
    public class Mapper185 : Mapper
    {
        BYTE patch;
        public Mapper185(NES parent) : base(parent)
        {
        }

        public override void Reset()

        {
            switch (PROM_16K_SIZE)
            {
                case 1: // 16K only
                    SetPROM_16K_Bank(4, 0);
                    SetPROM_16K_Bank(6, 0);
                    break;
                case 2: // 32K
                    SetPROM_32K_Bank(0);
                    break;
            }

            for (INT i = 0; i < 0x400; i++)
            {
                VRAM[0x800 + i] = 0xFF;
            }
            patch = 0;

            uint crc = nes.rom.GetPROM_CRC();
            if (crc == 0xb36457c7)
            {   // Spy vs Spy(J)
                patch = 1;
            }
        }

        //void Mapper185::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            if (((patch == 0) && ((data & 0x03) != 0)) || ((patch != 0) && data == 0x21))
            {
                SetVROM_8K_Bank(0);
            }
            else
            {
                SetVRAM_1K_Bank(0, 2);  // use vram bank 2
                SetVRAM_1K_Bank(1, 2);
                SetVRAM_1K_Bank(2, 2);
                SetVRAM_1K_Bank(3, 2);
                SetVRAM_1K_Bank(4, 2);
                SetVRAM_1K_Bank(5, 2);
                SetVRAM_1K_Bank(6, 2);
                SetVRAM_1K_Bank(7, 2);
            }
        }

    }
}
