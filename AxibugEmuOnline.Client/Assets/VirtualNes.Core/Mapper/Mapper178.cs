using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper178 : Mapper
	{
		BYTE[] reg = new byte[3];
		BYTE banknum;
		public Mapper178(NES parent) : base(parent)
		{
		}

		public override void Reset()
		{
			reg[0] = 0;
			reg[1] = 0;
			reg[2] = 0;
			banknum = 0;
			SetBank_CPU();
		}

		//void Mapper178::WriteLow(WORD addr, BYTE data)
		public override void WriteLow(ushort addr, byte data)
		{
			if (addr == 0x4800)
			{
				if ((data & 0x01) != 0) SetVRAM_Mirror(VRAM_HMIRROR);
				else SetVRAM_Mirror(VRAM_VMIRROR);
			}
			else if (addr == 0x4801)
			{
				reg[0] = (byte)((data >> 1) & 0x0f);
				SetBank_CPU();
			}
			else if (addr == 0x4802)
			{
				reg[1] = (byte)((data << 2) & 0x0f);
				//			SetBank_CPU();
			}
			else if (addr == 0x4803)
			{
				//unknown
			}
			else if (addr >= 0x6000)
			{
				CPU_MEM_BANK[addr >> 13][addr & 0x1FFF] = data;
			}
		}

		//void Write(WORD addr, BYTE data)
		public override void Write(ushort addr, byte data)
		{
			//		SetPROM_32K_Bank( data );
		}

		void SetBank_CPU()
		{
			banknum = (byte)((reg[0] + reg[1]) & 0x0f);
			SetPROM_32K_Bank(banknum);
		}

		public override bool IsStateSave()
		{
			return true;
		}
	}
}
