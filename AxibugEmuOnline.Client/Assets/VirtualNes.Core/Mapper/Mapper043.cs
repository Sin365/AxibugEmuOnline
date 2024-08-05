//////////////////////////////////////////////////////////////////////////
// Mapper043  SMB2J                                                     //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper043 : Mapper
	{
		BYTE irq_enable;
		INT irq_counter;
		public Mapper043(NES parent) : base(parent)
		{
		}


		public override void Reset()
		{
			irq_enable = 0xFF;
			irq_counter = 0;

			SetPROM_8K_Bank(3, 2);
			SetPROM_32K_Bank(1, 0, 4, 9);

			if (VROM_1K_SIZE != 0)
			{
				SetVROM_8K_Bank(0);
			}
		}

		//BYTE Mapper043::ReadLow(WORD addr)
		public override byte ReadLow(ushort addr)
		{
			if (0x5000 <= addr && addr < 0x6000)
			{
				byte[] pPtr = nes.rom.GetPROM();
				return pPtr[0x2000 * 8 + 0x1000 + (addr - 0x5000)];
			}
			return (BYTE)(addr >> 8);
		}

		//void Mapper043::ExWrite(WORD addr, BYTE data)
		public override void ExWrite(ushort addr, byte data)
		{
			if ((addr & 0xF0FF) == 0x4022)
			{
				switch (data & 0x07)
				{
					case 0x00:
					case 0x02:
					case 0x03:
					case 0x04:
						SetPROM_8K_Bank(6, 4);
						break;
					case 0x01:
						SetPROM_8K_Bank(6, 3);
						break;
					case 0x05:
						SetPROM_8K_Bank(6, 7);
						break;
					case 0x06:
						SetPROM_8K_Bank(6, 5);
						break;
					case 0x07:
						SetPROM_8K_Bank(6, 6);
						break;
				}
			}
		}

		//void Mapper043::WriteLow(WORD addr, BYTE data)
		public override void WriteLow(ushort addr, byte data)
		{
			if ((addr & 0xF0FF) == 0x4022)
			{
				ExWrite(addr, data);
			}
		}

		//void Mapper043::Write(WORD addr, BYTE data)
		public override void Write(ushort addr, byte data)
		{
			if (addr == 0x8122)
			{
				if ((data & 0x03) != 0)
				{
					irq_enable = 1;
				}
				else
				{
					irq_counter = 0;
					irq_enable = 0;
				}
				nes.cpu.ClrIRQ(IRQ_MAPPER);
			}
		}

		//void Mapper043::HSync(INT scanline)
		public override void HSync(int scanline)
		{
			nes.cpu.ClrIRQ(IRQ_MAPPER);
			if (irq_enable != 0)
			{
				irq_counter += 341;
				if (irq_counter >= 12288)
				{
					irq_counter = 0;
					//			nes.cpu.IRQ();
					nes.cpu.SetIRQ(IRQ_MAPPER);
				}
			}
		}

		//void Mapper043::SaveState(LPBYTE p)
		public override void SaveState(byte[] p)
		{
			//p[0] = irq_enable;
			//*(INT*)&p[1] = irq_counter;
		}

		//void Mapper043::LoadState(LPBYTE p)
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
