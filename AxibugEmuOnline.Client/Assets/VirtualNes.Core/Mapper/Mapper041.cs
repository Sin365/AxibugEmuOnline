//////////////////////////////////////////////////////////////////////////
// Mapper041  Caltron 6-in-1                                            //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper041 : Mapper
	{
		BYTE[] reg = new byte[2];
		public Mapper041(NES parent) : base(parent)
		{
		}


		public override void Reset()
		{
			reg[0] = reg[1] = 0;

			SetPROM_32K_Bank(0, 1, 2, 3);

			if (VROM_1K_SIZE != 0)
			{
				SetVROM_8K_Bank(0);
			}
		}

		//void Mapper041::WriteLow(WORD addr, BYTE data)
		public override void WriteLow(ushort addr, byte data)
		{
			if (addr >= 0x6000 && addr < 0x6800)
			{
				SetPROM_32K_Bank(addr & 0x07);
				reg[0] = (byte)(addr & 0x04);
				reg[1] &= 0x03;
				reg[1] |= (byte)((addr >> 1) & 0x0C);
				SetVROM_8K_Bank(reg[1]);
				if ((addr & 0x20) != 0) SetVRAM_Mirror(VRAM_HMIRROR);
				else SetVRAM_Mirror(VRAM_VMIRROR);
			}
		}

		//void Mapper041::Write(WORD addr, BYTE data)
		public override void Write(ushort addr, byte data)
		{
			if (reg[0] != 0)
			{
				reg[1] &= 0x0C;
				reg[1] |= (byte)(addr & 0x03);
				SetVROM_8K_Bank(reg[1]);
			}
		}

		//void Mapper041::SaveState(LPBYTE p)
		public override void SaveState(byte[] p)
		{
			p[0] = reg[0];
			p[1] = reg[1];
		}

		//void Mapper041::LoadState(LPBYTE p)
		public override void LoadState(byte[] p)
		{
			reg[0] = p[0];
			reg[1] = p[1];
		}


		public override bool IsStateSave()
		{
			return true;
		}
	}
}
