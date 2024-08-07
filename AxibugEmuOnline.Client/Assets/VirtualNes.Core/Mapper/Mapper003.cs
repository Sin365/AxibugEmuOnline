//////////////////////////////////////////////////////////////////////////
// Mapper003 CNROM                                                      //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
    public class Mapper003 : Mapper
    {

        public Mapper003(NES parent) : base(parent) { }

        public override void Reset()
        {
            switch (PROM_16K_SIZE)
            {
                case 1: // 16K only
                    SetPROM_16K_Bank(4, 0);
                    SetPROM_16K_Bank(6, 0);
                    break;
                case 2: // 32K
                    SetPROM_32K_Bank(0);
                    break;
            }
            //	nes.SetRenderMethod( NES::TILE_RENDER );
            uint crc = nes.rom.GetPROM_CRC();

            if (crc == 0x2b72fe7e)
            {   // Ganso Saiyuuki - Super Monkey Dai Bouken(J)
                nes.SetRenderMethod(EnumRenderMethod.TILE_RENDER);
                nes.ppu.SetExtNameTableMode(true);
            }

            //	if( crc == 0xE44D95B5 ) {	// ひみつｗ
            //	}
        }

#if FALSE//0
void	Mapper003::WriteLow( WORD addr, BYTE data )
{
	if( patch ) {
		Mapper::WriteLow( addr, data );
	} else {
		if( nes.rom.IsSAVERAM() ) {
			Mapper::WriteLow( addr, data );
		} else {
			if( addr >= 0x4800 ) {
				SetVROM_8K_Bank( data & 0x03 );
			}
		}
	}
}
#endif

        //void Mapper003::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            SetVROM_8K_Bank(data);
        }

    }
}
