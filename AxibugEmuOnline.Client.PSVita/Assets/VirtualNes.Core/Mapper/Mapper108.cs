//////////////////////////////////////////////////////////////////////////
// Mapper108                                                            //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;

namespace VirtualNes.Core
{
	public class Mapper108 : Mapper
	{
		public Mapper108(NES parent) : base(parent)
		{
		}

		public override void Reset()
		{
			SetPROM_32K_Bank(0xC, 0xD, 0xE, 0xF);
			SetPROM_8K_Bank(3, 0);
		}

		//void Mapper108::Write(WORD addr, BYTE data)
		public override void Write(ushort addr, byte data)
		{
			SetPROM_8K_Bank(3, data);
		}

	}
}