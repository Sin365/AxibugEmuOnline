//////////////////////////////////////////////////////////////////////////
// Mapper013  CPROM                                                     //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper013 : Mapper
	{

		public Mapper013(NES parent) : base(parent) { }

		public override void Reset()
		{
			SetPROM_32K_Bank(0, 1, 2, 3);
			SetCRAM_4K_Bank(0, 0);
			SetCRAM_4K_Bank(4, 0);
		}

		//void Mapper013::Write(WORD addr, BYTE data)
		public override void Write(ushort addr, byte data)
		{
			SetPROM_32K_Bank((data & 0x30) >> 4);
			SetCRAM_4K_Bank(4, data & 0x03);
		}


	}
}
