﻿//////////////////////////////////////////////////////////////////////////
// Mapper080  Taito X1-005                                              //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper080 : Mapper
	{
		public Mapper080(NES parent) : base(parent)
		{
		}

		public override void Reset()
		{
			SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);

			if (VROM_8K_SIZE != 0)
			{
				SetVROM_8K_Bank(0);
			}
		}

		//void Mapper080::WriteLow(WORD addr, BYTE data)
		public override void WriteLow(ushort addr, byte data)
		{
			switch (addr)
			{
				case 0x7EF0:
					SetVROM_2K_Bank(0, (data >> 1) & 0x3F);
					if (PROM_8K_SIZE == 32)
					{
						if ((data & 0x80) != 0)
						{
							SetVRAM_1K_Bank(8, 1);
							SetVRAM_1K_Bank(9, 1);
						}
						else
						{
							SetVRAM_1K_Bank(8, 0);
							SetVRAM_1K_Bank(9, 0);
						}
					}
					break;

				case 0x7EF1:
					SetVROM_2K_Bank(2, (data >> 1) & 0x3F);
					if (PROM_8K_SIZE == 32)
					{
						if ((data & 0x80) != 0)
						{
							SetVRAM_1K_Bank(10, 1);
							SetVRAM_1K_Bank(11, 1);
						}
						else
						{
							SetVRAM_1K_Bank(10, 0);
							SetVRAM_1K_Bank(11, 0);
						}
					}
					break;

				case 0x7EF2:
					SetVROM_1K_Bank(4, data);
					break;
				case 0x7EF3:
					SetVROM_1K_Bank(5, data);
					break;
				case 0x7EF4:
					SetVROM_1K_Bank(6, data);
					break;
				case 0x7EF5:
					SetVROM_1K_Bank(7, data);
					break;

				case 0x7EF6:
					if ((data & 0x01) != 0) SetVRAM_Mirror(VRAM_VMIRROR);
					else SetVRAM_Mirror(VRAM_HMIRROR);
					break;

				case 0x7EFA:
				case 0x7EFB:
					SetPROM_8K_Bank(4, data);
					break;
				case 0x7EFC:
				case 0x7EFD:
					SetPROM_8K_Bank(5, data);
					break;
				case 0x7EFE:
				case 0x7EFF:
					SetPROM_8K_Bank(6, data);
					break;
				default:
					base.WriteLow(addr, data);
					break;
			}
		}


	}
}
