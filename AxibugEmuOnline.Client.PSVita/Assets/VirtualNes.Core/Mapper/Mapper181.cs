///////////////////////////////////////////////////////
// Mapper181  Hacker International Type2                                //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
    public class Mapper181 : Mapper
    {
        public Mapper181(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(0);
            SetVROM_8K_Bank(0);
        }

        //void Mapper181::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            //DEBUGOUT( "$%04X:$%02X\n", addr, data );
            if (addr == 0x4120)
            {
                SetPROM_32K_Bank((data & 0x08) >> 3);
                SetVROM_8K_Bank(data & 0x07);
            }
        }


    }
}
