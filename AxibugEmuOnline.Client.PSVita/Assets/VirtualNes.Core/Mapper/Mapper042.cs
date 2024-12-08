//////////////////////////////////////////////////////////////////////////
// Mapper042  Mario Baby                                                //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper042 : Mapper
	{
		BYTE irq_enable;
		BYTE irq_counter;
		public Mapper042(NES parent) : base(parent)
		{
		}


		public override void Reset()
		{
			irq_enable = 0;
			irq_counter = 0;

			SetPROM_8K_Bank(3, 0);
			SetPROM_32K_Bank(PROM_8K_SIZE - 4, PROM_8K_SIZE - 3, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);

			if (VROM_1K_SIZE != 0)
			{
				SetVROM_8K_Bank(0);
			}
		}

		//void Mapper042::Write(WORD addr, BYTE data)
		public override void Write(ushort addr, byte data)
		{
			switch (addr & 0xE003)
			{
				case 0xE000:
					SetPROM_8K_Bank(3, data & 0x0F);
					break;

				case 0xE001:
					if ((data & 0x08) != 0) SetVRAM_Mirror(VRAM_HMIRROR);
					else SetVRAM_Mirror(VRAM_VMIRROR);
					break;

				case 0xE002:
					if ((data & 0x02) != 0)
					{
						irq_enable = 0xFF;
					}
					else
					{
						irq_enable = 0;
						irq_counter = 0;
					}
					nes.cpu.ClrIRQ(IRQ_MAPPER);
					break;
			}
		}

		//void Mapper042::HSync(INT scanline)
		public override void HSync(int scanline)
		{
			nes.cpu.ClrIRQ(IRQ_MAPPER);
			if (irq_enable != 0)
			{
				if (irq_counter < 215)
				{
					irq_counter++;
				}
				if (irq_counter == 215)
				{
					irq_enable = 0;
					//			nes.cpu.IRQ();
					nes.cpu.SetIRQ(IRQ_MAPPER);
				}
			}
		}

		//void Mapper042::SaveState(LPBYTE p)
		public override void SaveState(byte[] p)
		{
			p[0] = irq_enable;
			p[1] = irq_counter;
		}

		//void Mapper042::LoadState(LPBYTE p)
		public override void LoadState(byte[] p)
		{
			irq_enable = p[0];
			irq_counter = p[1];
		}


		public override bool IsStateSave()
		{
			return true;
		}
	}
}
