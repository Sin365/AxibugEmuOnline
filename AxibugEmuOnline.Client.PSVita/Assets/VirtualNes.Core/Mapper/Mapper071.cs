﻿//////////////////////////////////////////////////////////////////////////
// Mapper071  Camerica                                                  //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper071 : Mapper
	{
		public Mapper071(NES parent) : base(parent)
		{
		}

		public override void Reset()
		{
			SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);
		}

		//void Mapper071::WriteLow(WORD addr, BYTE data)
		public override void WriteLow(ushort addr, byte data)
		{
			if ((addr & 0xE000) == 0x6000)
			{
				SetPROM_16K_Bank(4, data);
			}
		}

		//void Mapper071::Write(WORD addr, BYTE data)
		public override void Write(ushort addr, byte data)
		{
			switch (addr & 0xF000)
			{
				case 0x9000:
					if ((data & 0x10) != 0) SetVRAM_Mirror(VRAM_MIRROR4H);
					else SetVRAM_Mirror(VRAM_MIRROR4L);
					break;

				case 0xC000:
				case 0xD000:
				case 0xE000:
				case 0xF000:
					SetPROM_16K_Bank(4, data);
					break;
			}
		}


	}
}
