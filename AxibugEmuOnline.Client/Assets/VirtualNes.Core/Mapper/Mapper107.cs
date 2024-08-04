//////////////////////////////////////////////////////////////////////////
// Mapper107  Magic Dragon Mapper                                       //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;

namespace VirtualNes.Core
{
	public class _MapName : Mapper
	{
		public _MapName(NES parent) : base(parent)
		{
		}

		public override void Reset()
		{
			SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);
			SetVROM_8K_Bank(0);
		}

		//void Mapper107::Write(WORD addr, BYTE data)
		public override void Write(ushort addr, byte data)
		{
			SetPROM_32K_Bank((data >> 1) & 0x03);
			SetVROM_8K_Bank(data & 0x07);
		}


	}
}
