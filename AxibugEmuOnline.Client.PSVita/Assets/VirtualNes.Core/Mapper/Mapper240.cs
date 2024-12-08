//////////////////////////////
// Mapper240  Gen Ke Le Zhuan                                           //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
    public class Mapper240 : Mapper
    {
        public Mapper240(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);
            if (VROM_1K_SIZE != 0)
            {
                SetVROM_8K_Bank(0);
            }
        }

        //void Mapper240::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            if (addr >= 0x4020 && addr < 0x6000)
            {
                SetPROM_32K_Bank((data & 0xF0) >> 4);
                SetVROM_8K_Bank(data & 0xF);
            }
        }


    }
}
