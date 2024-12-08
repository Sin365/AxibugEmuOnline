//////////////////////////////////////////////////////////////////////////
// Mapper060                                                            //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper060 : Mapper
	{
		BYTE patch;
		BYTE game_sel;
		public Mapper060(NES parent) : base(parent)
		{
		}

		public override void Reset()
		{
			patch = 0;

			uint crc = nes.rom.GetPROM_CRC();
			if (crc == 0xf9c484a0)
			{   // Reset Based 4-in-1(Unl)
				SetPROM_16K_Bank(4, game_sel);
				SetPROM_16K_Bank(6, game_sel);
				SetVROM_8K_Bank(game_sel);
				game_sel++;
				game_sel &= 3;
			}
			else
			{
				patch = 1;
				SetPROM_32K_Bank(0);
				SetVROM_8K_Bank(0);
			}
		}

		//void Mapper060::Write(WORD addr, BYTE data)
		public override void Write(ushort addr, byte data)
		{
			if (patch != 0)
			{
				if ((addr & 0x80) != 0)
				{
					SetPROM_16K_Bank(4, (addr & 0x70) >> 4);
					SetPROM_16K_Bank(6, (addr & 0x70) >> 4);
				}
				else
				{
					SetPROM_32K_Bank((addr & 0x70) >> 5);
				}

				SetVROM_8K_Bank(addr & 0x07);

				if ((data & 0x08) != 0) SetVRAM_Mirror(VRAM_VMIRROR);
				else SetVRAM_Mirror(VRAM_HMIRROR);
			}
		}


	}
}
