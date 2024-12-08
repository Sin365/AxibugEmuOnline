//////////////////////////////////////////////////////////////////////////
// Mapper007  AOROM/AMROM                                               //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using BYTE = System.Byte;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper007 : Mapper
    {

		BYTE patch;
		public Mapper007(NES parent) : base(parent) { }

        public override void Reset()
		{
			patch = 0;

			SetPROM_32K_Bank(0);
			SetVRAM_Mirror(VRAM_MIRROR4L);

			uint crc = nes.rom.GetPROM_CRC();
			if (crc == 0x3c9fe649)
			{   // WWF Wrestlemania Challenge(U)
				SetVRAM_Mirror(VRAM_VMIRROR);
				patch = 1;
			}
			if (crc == 0x09874777)
			{   // Marble Madness(U)
				nes.SetRenderMethod( EnumRenderMethod.TILE_RENDER);
			}

			if (crc == 0x279710DC       // Battletoads (U)
			 || crc == 0xCEB65B06)
			{   // Battletoads Double Dragon (U)
				nes.SetRenderMethod( EnumRenderMethod.PRE_ALL_RENDER);
				//::memset(WRAM, 0, sizeof(WRAM));
				MemoryUtility.ZEROMEMORY(WRAM, WRAM.Length);
			}
		}

		//void Mapper007::Write(WORD addr, BYTE data)
		public override void Write(ushort addr, byte data)
		{
			SetPROM_32K_Bank(data & 0x07);

			if (patch!=0)
			{
				if ((data & 0x10)!=0) SetVRAM_Mirror(VRAM_MIRROR4H);
				else SetVRAM_Mirror(VRAM_MIRROR4L);
			}
		}

	}
}
