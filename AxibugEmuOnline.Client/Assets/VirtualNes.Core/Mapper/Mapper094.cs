/////////////////////////////////
// Mapper094  Capcom 74161/32                                           //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper094 : Mapper
	{
		public Mapper094(NES parent) : base(parent)
		{
		}

		public override void Reset()
		{
			SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);
		}

		//void Mapper094::Write(WORD addr, BYTE data)
		public override void Write(ushort addr, byte data)
		{
			if ((addr & 0xFFF0) == 0xFF00)
			{
				SetPROM_16K_Bank(4, (data >> 2) & 0x7);
			}
		}


	}
}
