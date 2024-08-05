//////////////////////////////////////////////////////////////////////////
// Mapper073  Konami VRC3                                               //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper073 : Mapper
	{
		BYTE irq_enable;
		INT irq_counter;
		public Mapper073(NES parent) : base(parent)
		{
		}

		public override void Reset()
		{
			irq_enable = 0;
			irq_counter = 0;

			SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);
		}

		//void Mapper073::Write(WORD addr, BYTE data)
		public override void Write(ushort addr, byte data)
		{
			switch (addr)
			{
				case 0xF000:
					SetPROM_16K_Bank(4, data);
					break;

				case 0x8000:
					irq_counter = (irq_counter & 0xFFF0) | (data & 0x0F);
					break;
				case 0x9000:
					irq_counter = (irq_counter & 0xFF0F) | ((data & 0x0F) << 4);
					break;
				case 0xA000:
					irq_counter = (irq_counter & 0xF0FF) | ((data & 0x0F) << 8);
					break;
				case 0xB000:
					irq_counter = (irq_counter & 0x0FFF) | ((data & 0x0F) << 12);
					break;
				case 0xC000:
					irq_enable = (byte)(data & 0x02);
					nes.cpu.ClrIRQ(IRQ_MAPPER);
					break;
				case 0xD000:
					nes.cpu.ClrIRQ(IRQ_MAPPER);
					break;
			}
		}

		//void Mapper073::Clock(INT cycles)
		public override void Clock(int cycles)
		{
			if (irq_enable != 0)
			{
				if ((irq_counter += cycles) >= 0xFFFF)
				{
					irq_enable = 0;
					irq_counter &= 0xFFFF;
					nes.cpu.SetIRQ(IRQ_MAPPER);
				}
			}
		}

		//void Mapper073::SaveState(LPBYTE p)
		public override void SaveState(byte[] p)
		{
			//p[0] = irq_enable;
			//*(INT*)&p[1] = irq_counter;
		}

		//void Mapper073::LoadState(LPBYTE p)
		public override void LoadState(byte[] p)
		{
			//irq_enable = p[0];
			//irq_counter = *(INT*)&p[1];
		}

		public override bool IsStateSave()
		{
			return true;
		}
	}
}

