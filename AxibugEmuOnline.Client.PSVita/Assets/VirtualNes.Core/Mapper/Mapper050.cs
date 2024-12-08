//////////////////////////////////////////////////////////////////////////
// Mapper050  SMB2J                                                     //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper050 : Mapper
	{
		BYTE irq_enable;
		public Mapper050(NES parent) : base(parent)
		{
		}

		public override void Reset()
		{
			irq_enable = 0;
			SetPROM_8K_Bank(3, 15);
			SetPROM_8K_Bank(4, 8);
			SetPROM_8K_Bank(5, 9);
			SetPROM_8K_Bank(6, 0);
			SetPROM_8K_Bank(7, 11);
			if (VROM_1K_SIZE != 0)
			{
				SetVROM_8K_Bank(0);
			}
		}

		//void Mapper050::ExWrite(WORD addr, BYTE data)
		public override void ExWrite(ushort addr, byte data)
		{
			if ((addr & 0xE060) == 0x4020)
			{
				if ((addr & 0x0100) != 0)
				{
					irq_enable = (byte)(data & 0x01);
					nes.cpu.ClrIRQ(IRQ_MAPPER);
				}
				else
				{
					SetPROM_8K_Bank(6, (data & 0x08) | ((data & 0x01) << 2) | ((data & 0x06) >> 1));
				}
			}
		}

		//void Mapper050::WriteLow(WORD addr, BYTE data)
		public override void WriteLow(ushort addr, byte data)
		{
			if ((addr & 0xE060) == 0x4020)
			{
				if ((addr & 0x0100) != 0)
				{
					irq_enable = (byte)(data & 0x01);
					nes.cpu.ClrIRQ(IRQ_MAPPER);
				}
				else
				{
					SetPROM_8K_Bank(6, (data & 0x08) | ((data & 0x01) << 2) | ((data & 0x06) >> 1));
				}
			}
		}

		//void Mapper050::HSync(INT scanline)
		public override void HSync(int scanline)
		{
			if (irq_enable != 0)
			{
				if (scanline == 21)
				{
					//			nes.cpu.IRQ();
					nes.cpu.SetIRQ(IRQ_MAPPER);
				}
			}
		}

		//void Mapper050::SaveState(LPBYTE p)
		public override void SaveState(byte[] p)
		{
			p[0] = irq_enable;
		}

		//void Mapper050::LoadState(LPBYTE p)
		public override void LoadState(byte[] p)
		{
			irq_enable = p[0];
		}


		public override bool IsStateSave()
		{
			return true;
		}

	}
}
