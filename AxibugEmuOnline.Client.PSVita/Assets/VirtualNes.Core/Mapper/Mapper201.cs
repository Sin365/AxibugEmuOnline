//////////////////////////////////////////////////////////////////////////
// Mapper201  21-in-1                                                   //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
    public class Mapper201 : Mapper
    {
        public Mapper201(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            //	SetPROM_32K_Bank( 0, 1, PROM_8K_SIZE-2, PROM_8K_SIZE-1 );
            SetPROM_16K_Bank(4, 0);
            SetPROM_16K_Bank(6, 0);

            if (VROM_1K_SIZE != 0)
            {
                SetVROM_8K_Bank(0);
            }
        }

        //void Mapper201::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            BYTE bank = (byte)((BYTE)addr & 0x03);
            if (!((addr & 0x08) != 0))
                bank = 0;
            SetPROM_32K_Bank(bank);
            SetVROM_8K_Bank(bank);
        }


    }
}
