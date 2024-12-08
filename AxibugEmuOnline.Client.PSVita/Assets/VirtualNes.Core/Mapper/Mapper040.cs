//////////////////////////////////////////////////////////////////////////
// Mapper040  SMB2J                                                     //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper040 : Mapper
	{
		BYTE irq_enable;
		INT irq_line;
		public Mapper040(NES parent) : base(parent)
		{
		}


		public override void Reset()
		{
			irq_enable = 0;
			irq_line = 0;

			SetPROM_8K_Bank(3, 6);
			SetPROM_32K_Bank(4, 5, 0, 7);

			if (VROM_1K_SIZE != 0)
			{
				SetVROM_8K_Bank(0);
			}
		}

		//void Mapper040::Write(WORD addr, BYTE data)
		public override void Write(ushort addr, byte data)
		{
			switch (addr & 0xE000)
			{
				case 0x8000:
					irq_enable = 0;
					nes.cpu.ClrIRQ(IRQ_MAPPER);
					break;
				case 0xA000:
					irq_enable = 0xFF;
					irq_line = 37;
					nes.cpu.ClrIRQ(IRQ_MAPPER);
					break;
				case 0xC000:
					break;
				case 0xE000:
					SetPROM_8K_Bank(6, data & 0x07);
					break;
			}
		}

		//void Mapper040::HSync(INT scanline)
		public override void HSync(int scanline)
		{
			if (irq_enable != 0)
			{
				if (--irq_line <= 0)
				{
					//			nes.cpu.IRQ();
					nes.cpu.SetIRQ(IRQ_MAPPER);
				}
			}
		}

		//void Mapper040::SaveState(LPBYTE p)
		public override void SaveState(byte[] p)
		{
			//p[0] = irq_enable;
			//*(INT*)&p[1] = irq_line;
		}

		//void Mapper040::LoadState(LPBYTE p)
		public override void LoadState(byte[] p)
		{
			//irq_enable = p[0];
			//irq_line = *(INT*)&p[1];
		}

		public override bool IsStateSave()
		{
			return true;
		}

	}
}
