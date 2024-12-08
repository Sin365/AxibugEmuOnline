//////////////////////////////////////////////////////////////////////////
// Mapper231  20-in-1                                                   //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
    public class Mapper231 : Mapper
    {
        public Mapper231(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(0);

            if (VROM_1K_SIZE != 0)
            {
                SetVROM_8K_Bank(0);
            }
        }

        //void Mapper231::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            if ((addr & 0x0020) != 0)
            {
                SetPROM_32K_Bank((BYTE)(addr >> 1));
            }
            else
            {
                BYTE bank = (byte)(addr & 0x1E);
                SetPROM_8K_Bank(4, bank * 2 + 0);
                SetPROM_8K_Bank(5, bank * 2 + 1);
                SetPROM_8K_Bank(6, bank * 2 + 0);
                SetPROM_8K_Bank(7, bank * 2 + 1);
            }

            if ((addr & 0x0080) != 0)
            {
                SetVRAM_Mirror(VRAM_HMIRROR);
            }
            else
            {
                SetVRAM_Mirror(VRAM_VMIRROR);
            }
        }

    }
}
