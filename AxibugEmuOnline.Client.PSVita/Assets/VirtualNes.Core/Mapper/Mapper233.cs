//////////////////////////////////////////////////////////////////////////
// Mapper233  42-in-1                                                   //
//////////////////////////////////////////////////////////////////////////using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using static VirtualNes.MMU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;

namespace VirtualNes.Core
{
    public class Mapper233 : Mapper
    {
        public Mapper233(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(0, 1, 2, 3);
        }

        //void Mapper233::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            if ((data & 0x20) != 0)
            {
                SetPROM_8K_Bank(4, (data & 0x1F) * 2 + 0);
                SetPROM_8K_Bank(5, (data & 0x1F) * 2 + 1);
                SetPROM_8K_Bank(6, (data & 0x1F) * 2 + 0);
                SetPROM_8K_Bank(7, (data & 0x1F) * 2 + 1);
            }
            else
            {
                BYTE bank = (byte)((data & 0x1E) >> 1);

                SetPROM_8K_Bank(4, bank * 4 + 0);
                SetPROM_8K_Bank(5, bank * 4 + 1);
                SetPROM_8K_Bank(6, bank * 4 + 2);
                SetPROM_8K_Bank(7, bank * 4 + 3);
            }

            if ((data & 0xC0) == 0x00)
            {
                SetVRAM_Mirror(0, 0, 0, 1);
            }
            else if ((data & 0xC0) == 0x40)
            {
                SetVRAM_Mirror(VRAM_VMIRROR);
            }
            else if ((data & 0xC0) == 0x80)
            {
                SetVRAM_Mirror(VRAM_HMIRROR);
            }
            else
            {
                SetVRAM_Mirror(VRAM_MIRROR4H);
            }
        }

    }
}
