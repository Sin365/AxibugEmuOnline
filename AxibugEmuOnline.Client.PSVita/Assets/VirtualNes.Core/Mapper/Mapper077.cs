//////////////////////////////////////////////////////////////////////////
// Mapper077  Irem Early Mapper #0                                      //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper077 : Mapper
	{
		public Mapper077(NES parent) : base(parent)
		{
		}

		public override void Reset()
		{
			SetPROM_32K_Bank(0);

			SetVROM_2K_Bank(0, 0);
			SetCRAM_2K_Bank(2, 1);
			SetCRAM_2K_Bank(4, 2);
			SetCRAM_2K_Bank(6, 3);
		}

		//void Mapper077::Write(WORD addr, BYTE data)
		public override void Write(ushort addr, byte data)
		{
			SetPROM_32K_Bank(data & 0x07);

			SetVROM_2K_Bank(0, (data & 0xF0) >> 4);
		}


	}
}
