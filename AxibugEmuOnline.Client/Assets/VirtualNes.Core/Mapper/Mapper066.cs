//////////////////////////////////////////////////////////////////////////
// Mapper066  Bandai 74161                                              //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;


namespace VirtualNes.Core
{
    public class Mapper066 : Mapper
    {
        public Mapper066(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);
            SetVROM_8K_Bank(0);

            //	if( nes->rom->GetPROM_CRC() == 0xe30552db ) {	// Paris-Dakar Rally Special
            //		nes->SetFrameIRQmode( FALSE );
            //	}
        }

        //void Mapper066::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            if (addr >= 0x6000)
            {
                SetPROM_32K_Bank((data & 0xF0) >> 4);
                SetVROM_8K_Bank(data & 0x0F);
            }
        }

        //void Mapper066::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            SetPROM_32K_Bank((data & 0xF0) >> 4);
            SetVROM_8K_Bank(data & 0x0F);
        }


    }
}
