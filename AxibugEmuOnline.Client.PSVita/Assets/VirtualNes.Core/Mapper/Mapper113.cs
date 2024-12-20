﻿////////////////////////////
// Mapper113  PC-Sachen/Hacker                                          //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper113 : Mapper
	{
		public Mapper113(NES parent) : base(parent)
		{
		}

		public override void Reset()
		{
			//	SetPROM_32K_Bank( 0, 1, PROM_8K_SIZE-2, PROM_8K_SIZE-1 );
			SetPROM_32K_Bank(0);
			SetVROM_8K_Bank(0);
		}

		//void Mapper113::WriteLow(WORD addr, BYTE data)
		public override void WriteLow(ushort addr, byte data)
		{
			//DEBUGOUT( "$%04X:$%02X L=%3d\n", addr, data, nes.GetScanline() );
			switch (addr)
			{
				case 0x4100:
				case 0x4111:
				case 0x4120:
				case 0x4194:
				case 0x4195:
				case 0x4900:
					if (nes.rom.GetPROM_CRC() == 0xA75AEDE5)
					{ // HES 6-in-1
						if ((data & 0x80) != 0)
						{
							SetVRAM_Mirror(VRAM_VMIRROR);
						}
						else
						{
							SetVRAM_Mirror(VRAM_HMIRROR);
						}
					}
					SetPROM_32K_Bank(data >> 3);
					SetVROM_8K_Bank(((data >> 3) & 0x08) + (data & 0x07));
					break;
			}
		}

		//void Mapper113::Write(WORD addr, BYTE data)
		public override void Write(ushort addr, byte data)
		{
			//DEBUGOUT( "$%04X:$%02X L=%3d\n", addr, data, nes.GetScanline() );
			switch (addr)
			{
				case 0x8008:
				case 0x8009:
					SetPROM_32K_Bank(data >> 3);
					SetVROM_8K_Bank(((data >> 3) & 0x08) + (data & 0x07));
					break;
				case 0x8E66:
				case 0x8E67:
					//SetVROM_8K_Bank((data & 0x07) ? 0 : 1);
					SetVROM_8K_Bank((data & 0x07) != 0 ? 0 : 1);
					break;
				case 0xE00A:
					SetVRAM_Mirror(VRAM_MIRROR4L);
					break;
			}
		}


	}
}
