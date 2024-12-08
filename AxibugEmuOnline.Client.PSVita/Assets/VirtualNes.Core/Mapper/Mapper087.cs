//////////////////////////////////////////////////////////////////////////
// Mapper087  Konami 74161/32                                           //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
    public class Mapper087 : Mapper
    {
		public Mapper087(NES parent) : base(parent)
        {
        }

        public override void Reset()
		{
			SetPROM_32K_Bank(0, 1, 2, 3);
			SetVROM_8K_Bank(0);
		}

		//void Mapper087::WriteLow(WORD addr, BYTE data)
		public override void WriteLow(ushort addr, byte data)
		{
			if (addr == 0x6000)
			{
				SetVROM_8K_Bank((data & 0x02) >> 1);
			}
		}


	}
}
