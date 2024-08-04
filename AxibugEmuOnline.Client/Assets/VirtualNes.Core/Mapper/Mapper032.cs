//////////////////////////////////////////////////////////////////////////
// Mapper032  Irem G101                                                 //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper032 : Mapper
	{
		BYTE patch;

		BYTE reg;
		public Mapper032(NES parent) : base(parent)
		{
		}


		public override void Reset()
		{
			patch = 0;
			reg = 0;

			SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);

			if (VROM_8K_SIZE != 0)
			{
				SetVROM_8K_Bank(0);
			}

			uint crc = nes.rom.GetPROM_CRC();

			// For Major League(J)
			if (crc == 0xc0fed437)
			{
				patch = 1;
			}
			// For Ai Sensei no Oshiete - Watashi no Hoshi(J)
			if (crc == 0xfd3fc292)
			{
				SetPROM_32K_Bank(30, 31, 30, 31);
			}
		}

		//void Mapper032::Write(WORD addr, BYTE data)
		public override void Write(ushort addr, byte data)
		{
			switch (addr & 0xF000)
			{
				case 0x8000:
					if ((reg & 0x02) != 0)
					{
						SetPROM_8K_Bank(6, data);
					}
					else
					{
						SetPROM_8K_Bank(4, data);
					}
					break;

				case 0x9000:
					reg = data;
					if ((data & 0x01) != 0) SetVRAM_Mirror(VRAM_HMIRROR);
					else SetVRAM_Mirror(VRAM_VMIRROR);
					break;

				case 0xA000:
					SetPROM_8K_Bank(5, data);
					break;
			}

			switch (addr & 0xF007)
			{
				case 0xB000:
				case 0xB001:
				case 0xB002:
				case 0xB003:
				case 0xB004:
				case 0xB005:
					SetVROM_1K_Bank((byte)(addr & 0x0007), data);
					break;
				case 0xB006:
					SetVROM_1K_Bank(6, data);

					if (patch != 0 && ((data & 0x40) != 0))
					{
						SetVRAM_Mirror(0, 0, 0, 1);
					}
					break;
				case 0xB007:
					SetVROM_1K_Bank(7, data);

					if (patch != 0 && ((data & 0x40) != 0))
					{
						SetVRAM_Mirror(0, 0, 0, 0);
					}
					break;
			}
		}

		//void Mapper032::SaveState(LPBYTE p)
		public override void SaveState(byte[] p)
		{
			p[0] = reg;
		}

		//void Mapper032::LoadState(LPBYTE p)
		public override void LoadState(byte[] p)
		{
			reg = p[0];
		}


		public override bool IsStateSave()
		{
			return true;
		}

	}
}
