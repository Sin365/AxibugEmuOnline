//////////////////////////////////////////////////////////////////////////
// Mapper064  Tengen Rambo-1                                            //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper064 : Mapper
	{
		BYTE[] reg = new byte[3];
		BYTE irq_enable;
		BYTE irq_mode;
		INT irq_counter;
		INT irq_counter2;
		BYTE irq_latch;
		BYTE irq_reset;
		public Mapper064(NES parent) : base(parent)
		{
		}

		public override void Reset()
		{
			SetPROM_32K_Bank(PROM_8K_SIZE - 1, PROM_8K_SIZE - 1, PROM_8K_SIZE - 1, PROM_8K_SIZE - 1);

			if (VROM_1K_SIZE != 0)
			{
				SetVROM_8K_Bank(0);
			}

			reg[0] = reg[1] = reg[2] = 0;

			irq_enable = 0;
			irq_mode = 0;
			irq_counter = 0;
			irq_counter2 = 0;
			irq_latch = 0;
			irq_reset = 0;
		}

		//void Mapper064::Write(WORD addr, BYTE data)
		public override void Write(ushort addr, byte data)
		{
			//DEBUGOUT( "$%04X:$%02X\n", addr, data );
			switch (addr & 0xF003)
			{
				case 0x8000:
					reg[0] = (byte)(data & 0x0F);
					reg[1] = (byte)(data & 0x40);
					reg[2] = (byte)(data & 0x80);
					break;

				case 0x8001:
					switch (reg[0])
					{
						case 0x00:
							if (reg[2] != 0)
							{
								SetVROM_1K_Bank(4, data + 0);
								SetVROM_1K_Bank(5, data + 1);
							}
							else
							{
								SetVROM_1K_Bank(0, data + 0);
								SetVROM_1K_Bank(1, data + 1);
							}
							break;
						case 0x01:
							if (reg[2] != 0)
							{
								SetVROM_1K_Bank(6, data + 0);
								SetVROM_1K_Bank(7, data + 1);
							}
							else
							{
								SetVROM_1K_Bank(2, data + 0);
								SetVROM_1K_Bank(3, data + 1);
							}
							break;
						case 0x02:
							if (reg[2] != 0)
							{
								SetVROM_1K_Bank(0, data);
							}
							else
							{
								SetVROM_1K_Bank(4, data);
							}
							break;
						case 0x03:
							if (reg[2] != 0)
							{
								SetVROM_1K_Bank(1, data);
							}
							else
							{
								SetVROM_1K_Bank(5, data);
							}
							break;
						case 0x04:
							if (reg[2] != 0)
							{
								SetVROM_1K_Bank(2, data);
							}
							else
							{
								SetVROM_1K_Bank(6, data);
							}
							break;
						case 0x05:
							if (reg[2] != 0)
							{
								SetVROM_1K_Bank(3, data);
							}
							else
							{
								SetVROM_1K_Bank(7, data);
							}
							break;
						case 0x06:
							if (reg[1] != 0)
							{
								SetPROM_8K_Bank(5, data);
							}
							else
							{
								SetPROM_8K_Bank(4, data);
							}
							break;
						case 0x07:
							if (reg[1] != 0)
							{
								SetPROM_8K_Bank(6, data);
							}
							else
							{
								SetPROM_8K_Bank(5, data);
							}
							break;
						case 0x08:
							SetVROM_1K_Bank(1, data);
							break;
						case 0x09:
							SetVROM_1K_Bank(3, data);
							break;
						case 0x0F:
							if (reg[1] != 0)
							{
								SetPROM_8K_Bank(4, data);
							}
							else
							{
								SetPROM_8K_Bank(6, data);
							}
							break;
					}
					break;

				case 0xA000:
					if ((data & 0x01) != 0) SetVRAM_Mirror(VRAM_HMIRROR);
					else SetVRAM_Mirror(VRAM_VMIRROR);
					break;

				case 0xC000:
					irq_latch = data;
					if (irq_reset != 0)
					{
						irq_counter = irq_latch;
					}
					break;
				case 0xC001:
					irq_reset = 0xFF;
					irq_counter = irq_latch;
					irq_mode = (byte)(data & 0x01);
					break;
				case 0xE000:
					irq_enable = 0;
					if (irq_reset != 0)
					{
						irq_counter = irq_latch;
					}
					nes.cpu.ClrIRQ(IRQ_MAPPER);
					break;
				case 0xE001:
					irq_enable = 0xFF;
					if (irq_reset != 0)
					{
						irq_counter = irq_latch;
					}
					break;
			}
		}

		//void Mapper064::Clock(INT cycles)
		public override void Clock(int cycles)
		{
			if (irq_mode == 0)
				return;

			irq_counter2 += cycles;
			while (irq_counter2 >= 4)
			{
				irq_counter2 -= 4;
				if (irq_counter >= 0)
				{
					irq_counter--;
					if (irq_counter < 0)
					{
						if (irq_enable != 0)
						{
							nes.cpu.SetIRQ(IRQ_MAPPER);
						}
					}
				}
			}
		}

		//void Mapper064::HSync(INT scanline)
		public override void HSync(int scanline)
		{
			if (irq_mode != 0)
				return;

			irq_reset = 0;

			if ((scanline >= 0 && scanline <= 239))
			{
				if (nes.ppu.IsDispON())
				{
					if (irq_counter >= 0)
					{
						irq_counter--;
						if (irq_counter < 0)
						{
							if (irq_enable != 0)
							{
								irq_reset = 1;
								nes.cpu.SetIRQ(IRQ_MAPPER);
							}
						}
					}
				}
			}
		}

		//void Mapper064::SaveState(LPBYTE p)
		public override void SaveState(byte[] p)
		{
			//p[0] = reg[0];
			//p[1] = reg[1];
			//p[2] = reg[2];
			//p[3] = irq_enable;
			//p[4] = irq_mode;
			//p[5] = irq_latch;
			//p[6] = irq_reset;
			//*((INT*)&p[8]) = irq_counter;
			//*((INT*)&p[12]) = irq_counter2;
		}

		//void Mapper064::LoadState(LPBYTE p)
		public override void LoadState(byte[] p)
		{
			//reg[0] = p[0];
			//reg[1] = p[1];
			//reg[2] = p[2];
			//irq_enable = p[3];
			//irq_mode = p[4];
			//irq_latch = p[5];
			//irq_reset = p[6];
			//irq_counter = *((INT*)&p[8]);
			//irq_counter2 = *((INT*)&p[12]);
		}

		public override bool IsStateSave()
		{
			return true;
		}
	}
}
