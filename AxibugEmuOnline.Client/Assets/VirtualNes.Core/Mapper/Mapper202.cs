//////////////////////////////////////////////////////////////////////////
// Mapper202  150-in-1                                                  //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
    public class Mapper202 : Mapper
    {
        public Mapper202(NES parent) : base(parent)
        {
        }

        public override void Reset()

        {
            SetPROM_16K_Bank(4, 6);
            SetPROM_16K_Bank(6, 7);

            if (VROM_1K_SIZE != 0)
            {
                SetVROM_8K_Bank(0);
            }
        }

        //void Mapper202::ExWrite(WORD addr, BYTE data)
        public override void ExWrite(ushort addr, byte data)
        {
            if (addr >= 0x4020)
            {
                WriteSub(addr, data);
            }
        }

        //void Mapper202::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            WriteSub(addr, data);
        }

        //void Mapper202::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            WriteSub(addr, data);
        }

        void WriteSub(ushort addr, BYTE data)
        {
            INT bank = (addr >> 1) & 0x07;

            SetPROM_16K_Bank(4, bank);
            if ((addr & 0x0C) == 0x0C)
            {
                SetPROM_16K_Bank(6, bank + 1);
            }
            else
            {
                SetPROM_16K_Bank(6, bank);
            }
            SetVROM_8K_Bank(bank);

            if ((addr & 0x01) != 0)
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
