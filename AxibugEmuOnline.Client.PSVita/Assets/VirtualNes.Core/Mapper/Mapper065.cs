﻿//////////////////////////////////////////////////////////////////////////
// Mapper065  Irem H3001                                                //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper065 : Mapper
	{
		BYTE patch;

		BYTE irq_enable;
		INT irq_counter;
		INT irq_latch;
		public Mapper065(NES parent) : base(parent)
		{
		}

		public override void Reset()
		{
			patch = 0;

			// Kaiketsu Yanchamaru 3(J)
			if (nes.rom.GetPROM_CRC() == 0xe30b7f64)
			{
				patch = 1;
			}

			SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);

			if (VROM_8K_SIZE != 0)
			{
				SetVROM_8K_Bank(0);
			}

			irq_enable = 0;
			irq_counter = 0;
		}

		//void Mapper065::Write(WORD addr, BYTE data)
		public override void Write(ushort addr, byte data)
		{
			switch (addr)
			{
				case 0x8000:
					SetPROM_8K_Bank(4, data);
					break;

				case 0x9000:
					if (patch == 0)
					{
						if ((data & 0x40) != 0) SetVRAM_Mirror(VRAM_VMIRROR);
						else SetVRAM_Mirror(VRAM_HMIRROR);
					}
					break;

				case 0x9001:
					if (patch != 0)
					{
						if ((data & 0x80) != 0) SetVRAM_Mirror(VRAM_HMIRROR);
						else SetVRAM_Mirror(VRAM_VMIRROR);
					}
					break;

				case 0x9003:
					if (patch == 0)
					{
						irq_enable = (byte)(data & 0x8);
						nes.cpu.ClrIRQ(IRQ_MAPPER);
					}
					break;
				case 0x9004:
					if (patch == 0)
					{
						irq_counter = irq_latch;
					}
					break;
				case 0x9005:
					if (patch != 0)
					{
						irq_counter = (BYTE)(data << 1);
						irq_enable = data;
						nes.cpu.ClrIRQ(IRQ_MAPPER);
					}
					else
					{
						irq_latch = (irq_latch & 0x00FF) | ((INT)data << 8);
					}
					break;

				case 0x9006:
					if (patch != 0)
					{
						irq_enable = 1;
					}
					else
					{
						irq_latch = (irq_latch & 0xFF00) | data;
					}
					break;

				case 0xB000:
				case 0xB001:
				case 0xB002:
				case 0xB003:
				case 0xB004:
				case 0xB005:
				case 0xB006:
				case 0xB007:
					SetVROM_1K_Bank((byte)(addr & 0x0007), data);
					break;

				case 0xA000:
					SetPROM_8K_Bank(5, data);
					break;
				case 0xC000:
					SetPROM_8K_Bank(6, data);
					break;
			}
		}

		//void Mapper065::HSync(INT scanline)
		public override void HSync(int scanline)
		{
			if (patch != 0)
			{
				if (irq_enable != 0)
				{
					if (irq_counter == 0)
					{
						//				nes.cpu.IRQ_NotPending();
						nes.cpu.SetIRQ(IRQ_MAPPER);
					}
					else
					{
						irq_counter--;
					}
				}
			}
		}

		//void Mapper065::Clock(INT cycles)
		public override void Clock(int cycles)
		{
			if (patch == 0)
			{
				if (irq_enable != 0)
				{
					if (irq_counter <= 0)
					{
						//				nes.cpu.IRQ_NotPending();
						nes.cpu.SetIRQ(IRQ_MAPPER);
					}
					else
					{
						irq_counter -= cycles;
					}
				}
			}
		}

		//void Mapper065::SaveState(LPBYTE p)
		public override void SaveState(byte[] p)
		{
			//p[0] = irq_enable;
			//*(INT*)&p[1] = irq_counter;
			//*(INT*)&p[5] = irq_latch;
		}

		//void Mapper065::LoadState(LPBYTE p)
		public override void LoadState(byte[] p)
		{
			//irq_enable = p[0];
			//irq_counter = *(INT*)&p[1];
			//irq_latch = *(INT*)&p[5];
		}


		public override bool IsStateSave()
		{
			return true;
		}
	}
}
