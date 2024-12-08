//////////////////////////////////////////////////////////////////////////
// Mapper079  Nina-3                                                    //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper079 : Mapper
	{
		public Mapper079(NES parent) : base(parent)
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

		//void Mapper079::WriteLow(WORD addr, BYTE data)
		public override void WriteLow(ushort addr, byte data)
		{
			if ((addr & 0x0100) != 0)
			{
				SetPROM_32K_Bank((data >> 3) & 0x01);
				SetVROM_8K_Bank(data & 0x07);
			}
		}


	}
}
