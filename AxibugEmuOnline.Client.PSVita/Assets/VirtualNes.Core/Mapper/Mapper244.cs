//////////////////////////////////////////////////////////////////////////
// Mapper244                                                            //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
    public class Mapper244 : Mapper
    {
        public Mapper244(NES parent) : base(parent)
        {
        }

        //void Mapper244::Reset()
        public override void Reset()
        {
            SetPROM_32K_Bank(0);
        }

        //void Mapper244::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            if (addr >= 0x8065 && addr <= 0x80A4)
            {
                SetPROM_32K_Bank((addr - 0x8065) & 0x3);
            }

            if (addr >= 0x80A5 && addr <= 0x80E4)
            {
                SetVROM_8K_Bank((addr - 0x80A5) & 0x7);
            }
        }

    }
}
