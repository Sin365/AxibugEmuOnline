using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
using Codice.CM.Client.Differences;
using System.Runtime.ConstrainedExecution;

namespace VirtualNes.Core
{
	public class Mapper176 : Mapper
	{
		BYTE prg, chr;
		public Mapper176(NES parent) : base(parent)
		{
		}

		public override void Reset()
		{
			//prg = ~0;
			prg = (~0);
			chr = 0;
			Sync();
		}

		void Sync()
		{
			//setprg8r(0x10,0x6000,0);
			SetPROM_32K_Bank(prg >> 1);
			SetVROM_8K_Bank(chr);
		}

		//void Mapper176::WriteLow(WORD addr, BYTE data)
		public override void WriteLow(ushort addr, byte data)
		{
			switch (addr)
			{
				case 0x5ff1:
					prg = data; Sync();
					break;
				case 0x5ff2:
					chr = data; Sync();
					break;
				default:
					break;
			}
			if (addr >= 0x6000)
			{
				CPU_MEM_BANK[addr >> 13][addr & 0x1FFF] = data;
			}
		}

		public override bool IsStateSave()
		{
			return true;
		}
	}
}
