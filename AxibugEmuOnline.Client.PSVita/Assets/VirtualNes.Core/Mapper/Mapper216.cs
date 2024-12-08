//////////////////////////////////////////////////////////////////////////
// Mapper216                                                            //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper216 : Mapper
	{
		public Mapper216(NES parent) : base(parent)
		{
		}

		public override void Reset()
		{
			SetVROM_8K_Bank(0);
			SetPROM_32K_Bank(0);
		}

		//void Mapper216::Write(WORD addr, BYTE data)
		public override void Write(ushort addr, byte data)
		{
			SetVROM_8K_Bank((addr & 0x0E) >> 1);
			SetPROM_32K_Bank(addr & 1);
		}

	}
}
