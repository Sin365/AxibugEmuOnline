//////////////////////////////////////////////////////////////////////////
// Mapper241  Fon Serm Bon                                              //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
    public class Mapper241 : Mapper
    {
        public Mapper241(NES parent) : base(parent)
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

        //void Mapper241::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            if (addr == 0x8000)
            {
                SetPROM_32K_Bank(data);
            }
        }

    }
}
