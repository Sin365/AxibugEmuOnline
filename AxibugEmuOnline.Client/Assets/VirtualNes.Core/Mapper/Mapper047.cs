﻿//////////////////////////////////////////////////////////////////////////
// Mapper047  NES-QJ                                                    //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper047 : Mapper
	{
		BYTE[] reg = new byte[8];
		BYTE patch;
		BYTE bank;
		BYTE prg0, prg1;
		BYTE chr01, chr23, chr4, chr5, chr6, chr7;
		BYTE irq_enable;
		BYTE irq_counter;
		BYTE irq_latch;
		public Mapper047(NES parent) : base(parent)
		{
		}

		public override void Reset()
		{
			patch = 0;

			if (nes.rom.GetPROM_CRC() == 0x7eef434c)
			{
				patch = 1;
			}

			for (INT i = 0; i < 8; i++)
			{
				reg[i] = 0;
			}

			bank = 0;
			prg0 = 0;
			prg1 = 1;

			// set VROM banks
			if (VROM_1K_SIZE != 0)
			{
				chr01 = 0;
				chr23 = 2;
				chr4 = 4;
				chr5 = 5;
				chr6 = 6;
				chr7 = 7;
			}
			else
			{
				chr01 = chr23 = chr4 = chr5 = chr6 = chr7 = 0;
			}

			SetBank_CPU();
			SetBank_PPU();

			irq_enable = 0;
			irq_counter = 0;
			irq_latch = 0;
		}

		//void Mapper047::WriteLow(WORD addr, BYTE data)
		public override void WriteLow(ushort addr, byte data)
		{
			if (addr == 0x6000)
			{
				if (patch != 0)
				{
					bank = (byte)((data & 0x06) >> 1);
				}
				else
				{
					bank = (byte)((data & 0x01) << 1);
				}
				SetBank_CPU();
				SetBank_PPU();
			}
		}

		//void Mapper047::Write(WORD addr, BYTE data)
		public override void Write(ushort addr, byte data)
		{
			switch (addr & 0xE001)
			{
				case 0x8000:
					reg[0] = data;
					SetBank_CPU();
					SetBank_PPU();
					break;
				case 0x8001:
					reg[1] = data;
					switch (reg[0] & 0x07)
					{
						case 0x00:
							chr01 = (byte)(data & 0xFE);
							SetBank_PPU();
							break;
						case 0x01:
							chr23 = (byte)(data & 0xFE);
							SetBank_PPU();
							break;
						case 0x02:
							chr4 = data;
							SetBank_PPU();
							break;
						case 0x03:
							chr5 = data;
							SetBank_PPU();
							break;
						case 0x04:
							chr6 = data;
							SetBank_PPU();
							break;
						case 0x05:
							chr7 = data;
							SetBank_PPU();
							break;
						case 0x06:
							prg0 = data;
							SetBank_CPU();
							break;
						case 0x07:
							prg1 = data;
							SetBank_CPU();
							break;
					}
					break;
				case 0xA000:
					reg[2] = data;
					if ((data & 0x01) != 0) SetVRAM_Mirror(VRAM_HMIRROR);
					else SetVRAM_Mirror(VRAM_VMIRROR);
					break;
				case 0xA001:
					reg[3] = data;
					break;
				case 0xC000:
					reg[4] = data;
					irq_counter = data;
					break;
				case 0xC001:
					reg[5] = data;
					irq_latch = data;
					break;
				case 0xE000:
					reg[6] = data;
					irq_enable = 0;
					nes.cpu.ClrIRQ(IRQ_MAPPER);
					break;
				case 0xE001:
					reg[7] = data;
					irq_enable = 1;
					break;
			}
		}

		//void Mapper047::HSync(INT scanline)
		public override void HSync(int scanline)
		{
			if ((scanline >= 0 && scanline <= 239))
			{
				if (nes.ppu.IsDispON())
				{
					if (irq_enable != 0)
					{
						if ((--irq_counter) == 0)
						{
							irq_counter = irq_latch;
							//					nes.cpu.IRQ();
							nes.cpu.SetIRQ(IRQ_MAPPER);
						}
					}
				}
			}
		}

		void SetBank_CPU()
		{
			if ((reg[0] & 0x40) != 0)
			{
				SetPROM_8K_Bank(4, bank * 8 + ((patch != 0 && bank != 2) ? 6 : 14));
				SetPROM_8K_Bank(5, bank * 8 + prg1);
				SetPROM_8K_Bank(6, bank * 8 + prg0);
				SetPROM_8K_Bank(7, bank * 8 + ((patch != 0 && bank != 2) ? 7 : 15));
			}
			else
			{
				SetPROM_8K_Bank(4, bank * 8 + prg0);
				SetPROM_8K_Bank(5, bank * 8 + prg1);
				SetPROM_8K_Bank(6, bank * 8 + ((patch != 0 && bank != 2) ? 6 : 14));
				SetPROM_8K_Bank(7, bank * 8 + ((patch != 0 && bank != 2) ? 7 : 15));
			}
		}

		void SetBank_PPU()
		{
			if (VROM_1K_SIZE != 0)
			{
				if ((reg[0] & 0x80) != 0)
				{
					SetVROM_1K_Bank(0, (bank & 0x02) * 64 + chr4);
					SetVROM_1K_Bank(1, (bank & 0x02) * 64 + chr5);
					SetVROM_1K_Bank(2, (bank & 0x02) * 64 + chr6);
					SetVROM_1K_Bank(3, (bank & 0x02) * 64 + chr7);
					SetVROM_1K_Bank(4, (bank & 0x02) * 64 + chr01 + 0);
					SetVROM_1K_Bank(5, (bank & 0x02) * 64 + chr01 + 1);
					SetVROM_1K_Bank(6, (bank & 0x02) * 64 + chr23 + 0);
					SetVROM_1K_Bank(7, (bank & 0x02) * 64 + chr23 + 1);
				}
				else
				{
					SetVROM_1K_Bank(0, (bank & 0x02) * 64 + chr01 + 0);
					SetVROM_1K_Bank(1, (bank & 0x02) * 64 + chr01 + 1);
					SetVROM_1K_Bank(2, (bank & 0x02) * 64 + chr23 + 0);
					SetVROM_1K_Bank(3, (bank & 0x02) * 64 + chr23 + 1);
					SetVROM_1K_Bank(4, (bank & 0x02) * 64 + chr4);
					SetVROM_1K_Bank(5, (bank & 0x02) * 64 + chr5);
					SetVROM_1K_Bank(6, (bank & 0x02) * 64 + chr6);
					SetVROM_1K_Bank(7, (bank & 0x02) * 64 + chr7);
				}
			}
		}

		//void Mapper047::SaveState(LPBYTE p)
		public override void SaveState(byte[] p)
		{
			for (INT i = 0; i < 8; i++)
			{
				p[i] = reg[i];
			}
			p[8] = prg0;
			p[9] = prg1;
			p[10] = chr01;
			p[11] = chr23;
			p[12] = chr4;
			p[13] = chr5;
			p[14] = chr6;
			p[15] = chr7;
			p[16] = irq_enable;
			p[17] = irq_counter;
			p[18] = irq_latch;
			p[19] = bank;
		}

		//void Mapper047::LoadState(LPBYTE p)
		public override void LoadState(byte[] p)
		{
			for (INT i = 0; i < 8; i++)
			{
				reg[i] = p[i];
			}
			prg0 = p[8];
			prg1 = p[9];
			chr01 = p[10];
			chr23 = p[11];
			chr4 = p[12];
			chr5 = p[13];
			chr6 = p[14];
			chr7 = p[15];
			irq_enable = p[16];
			irq_counter = p[17];
			irq_latch = p[18];
			bank = p[19];
		}




	}
}