﻿//////////////////////////////////////////////////////////////////////////
// Mapper105  Nintendo World Championship                               //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper105 : Mapper
	{
		BYTE init_state;
		BYTE write_count;
		BYTE bits;
		BYTE[] reg = new byte[4];

		BYTE irq_enable;
		INT irq_counter;
		public Mapper105(NES parent) : base(parent)
		{
		}

		public override void Reset()
		{
			SetPROM_32K_Bank(0);

			reg[0] = 0x0C;
			reg[1] = 0x00;
			reg[2] = 0x00;
			reg[3] = 0x10;

			bits = 0;
			write_count = 0;

			irq_counter = 0;
			irq_enable = 0;
			init_state = 0;
		}

		//void Mapper105::Write(WORD addr, BYTE data)
		public override void Write(ushort addr, byte data)
		{
			//WORD reg_num = (addr & 0x7FFF) >> 13;
			uint reg_num = (byte)((addr & 0x7FFF) >> 13);

			if ((data & 0x80) != 0)
			{
				bits = write_count = 0;
				if (reg_num == 0)
				{
					reg[reg_num] |= 0x0C;
				}
			}
			else
			{
				//bits |= (data & 1) << write_count++;
				bits |= (byte)((data & 1) << write_count++);
				if (write_count == 5)
				{
					reg[reg_num] = (byte)(bits & 0x1F);
					bits = write_count = 0;
				}
			}

			if ((reg[0] & 0x02) != 0)
			{
				if ((reg[0] & 0x01) != 0)
				{
					SetVRAM_Mirror(VRAM_HMIRROR);
				}
				else
				{
					SetVRAM_Mirror(VRAM_VMIRROR);
				}
			}
			else
			{
				if ((reg[0] & 0x01) != 0)
				{
					SetVRAM_Mirror(VRAM_MIRROR4H);
				}
				else
				{
					SetVRAM_Mirror(VRAM_MIRROR4L);
				}
			}

			switch (init_state)
			{
				case 0:
				case 1:
					init_state++;
					break;
				case 2:
					if ((reg[1] & 0x08) != 0)
					{
						if ((reg[0] & 0x08) != 0)
						{
							if ((reg[0] & 0x04) != 0)
							{
								SetPROM_8K_Bank(4, ((reg[3] & 0x07) * 2 + 16));
								SetPROM_8K_Bank(5, ((reg[3] & 0x07) * 2 + 17));
								SetPROM_8K_Bank(6, 30);
								SetPROM_8K_Bank(7, 31);
							}
							else
							{
								SetPROM_8K_Bank(4, 16);
								SetPROM_8K_Bank(5, 17);
								SetPROM_8K_Bank(6, ((reg[3] & 0x07) * 2 + 16));
								SetPROM_8K_Bank(7, ((reg[3] & 0x07) * 2 + 17));
							}
						}
						else
						{
							SetPROM_8K_Bank(4, ((reg[3] & 0x06) * 2 + 16));
							SetPROM_8K_Bank(5, ((reg[3] & 0x06) * 2 + 17));
							SetPROM_8K_Bank(6, ((reg[3] & 0x06) * 2 + 18));
							SetPROM_8K_Bank(7, ((reg[3] & 0x06) * 2 + 19));
						}
					}
					else
					{
						SetPROM_8K_Bank(4, ((reg[1] & 0x06) * 2 + 0));
						SetPROM_8K_Bank(5, ((reg[1] & 0x06) * 2 + 1));
						SetPROM_8K_Bank(6, ((reg[1] & 0x06) * 2 + 2));
						SetPROM_8K_Bank(7, ((reg[1] & 0x06) * 2 + 3));
					}

					if ((reg[1] & 0x10) != 0)
					{
						irq_counter = 0;
						irq_enable = 0;
					}
					else
					{
						irq_enable = 1;
					}
					//			nes.cpu.ClrIRQ( IRQ_MAPPER );
					break;
				default:
					break;
			}
		}

		//void Mapper105::HSync(INT scanline)
		public override void HSync(int scanline)
		{
			if (scanline != 0)
			{
				if (irq_enable != 0)
				{
					irq_counter += 29781;
				}
				if (((irq_counter | 0x21FFFFFF) & 0x3E000000) == 0x3E000000)
				{
					//			nes.cpu.IRQ_NotPending();
					//			nes.cpu.SetIRQ( IRQ_MAPPER );
					nes.cpu.SetIRQ(IRQ_TRIGGER2);
				}
			}
		}

		//void Mapper105::SaveState(LPBYTE p)
		public override void SaveState(byte[] p)
		{
			//for (INT i = 0; i < 4; i++)
			//{
			//	p[i] = reg[i];
			//}
			//p[8] = init_state;
			//p[9] = write_count;
			//p[10] = bits;
			//p[11] = irq_enable;
			//*((INT*)&p[12]) = irq_counter;
		}

		//void Mapper105::LoadState(LPBYTE p)
		public override void LoadState(byte[] p)
		{
			//for (INT i = 0; i < 4; i++)
			//{
			//	reg[i] = p[i];
			//}
			//init_state = p[8];
			//write_count = p[9];
			//bits = p[10];
			//irq_enable = p[11];
			//irq_counter = *((INT*)&p[12]);
		}


		public override bool IsStateSave()
		{
			return true;
		}

	}
}
