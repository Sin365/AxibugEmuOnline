//////////////////////////////////////////////////////////////////////////
// Mapper010 Nintendo MMC4                                              //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper010 : Mapper
	{
		BYTE[] reg = new byte[4];
		BYTE latch_a, latch_b;

		public Mapper010(NES parent) : base(parent) { }

		public override void Reset()
		{
			SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);

			reg[0] = 0; reg[1] = 4;
			reg[2] = 0; reg[3] = 0;

			latch_a = 0xFE;
			latch_b = 0xFE;
			SetVROM_4K_Bank(0, 4);
			SetVROM_4K_Bank(4, 0);

			nes.ppu.SetChrLatchMode(true);
		}

		//void Mapper010::Write(WORD addr, BYTE data)
		public override void Write(ushort addr, byte data)
		{
			switch (addr & 0xF000)
			{
				case 0xA000:
					SetPROM_16K_Bank(4, data);
					break;
				case 0xB000:
					reg[0] = data;
					if (latch_a == 0xFD)
					{
						SetVROM_4K_Bank(0, reg[0]);
					}
					break;
				case 0xC000:
					reg[1] = data;
					if (latch_a == 0xFE)
					{
						SetVROM_4K_Bank(0, reg[1]);
					}
					break;
				case 0xD000:
					reg[2] = data;
					if (latch_b == 0xFD)
					{
						SetVROM_4K_Bank(4, reg[2]);
					}
					break;
				case 0xE000:
					reg[3] = data;
					if (latch_b == 0xFE)
					{
						SetVROM_4K_Bank(4, reg[3]);
					}
					break;
				case 0xF000:
					if ((data & 0x01) != 0)
						SetVRAM_Mirror(VRAM_HMIRROR);
					else SetVRAM_Mirror(VRAM_VMIRROR);
					break;
			}
		}

		//void Mapper010::PPU_ChrLatch(WORD addr)
		public override void PPU_ChrLatch(ushort addr)
		{
			if ((addr & 0x1FF0) == 0x0FD0 && latch_a != 0xFD)
			{
				latch_a = 0xFD;
				SetVROM_4K_Bank(0, reg[0]);
			}
			else if ((addr & 0x1FF0) == 0x0FE0 && latch_a != 0xFE)
			{
				latch_a = 0xFE;
				SetVROM_4K_Bank(0, reg[1]);
			}
			else if ((addr & 0x1FF0) == 0x1FD0 && latch_b != 0xFD)
			{
				latch_b = 0xFD;
				SetVROM_4K_Bank(4, reg[2]);
			}
			else if ((addr & 0x1FF0) == 0x1FE0 && latch_b != 0xFE)
			{
				latch_b = 0xFE;
				SetVROM_4K_Bank(4, reg[3]);
			}
		}

		//void Mapper010::SaveState(LPBYTE p)
		public override void SaveState(byte[] p)
		{
			p[0] = reg[0];
			p[1] = reg[1];
			p[2] = reg[2];
			p[3] = reg[3];
			p[4] = latch_a;
			p[5] = latch_b;
		}

		//void Mapper010::LoadState(LPBYTE p)
		public override void LoadState(byte[] p)
		{
			reg[0] = p[0];
			reg[1] = p[1];
			reg[2] = p[2];
			reg[3] = p[3];
			latch_a = p[4];
			latch_b = p[5];
		}
		public override bool IsStateSave()
		{
			return true;
		}
	}
}
