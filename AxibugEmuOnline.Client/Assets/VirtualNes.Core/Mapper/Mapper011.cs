//////////////////////////////////////////////////////////////////////////
// Mapper011  Color Dreams                                              //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;


namespace VirtualNes.Core
{
    public class Mapper011 : Mapper
    {

        public Mapper011(NES parent) : base(parent) { }

        public override void Reset()
        {
            SetPROM_32K_Bank(0);

            if (VROM_1K_SIZE != 0)
            {
                SetVROM_8K_Bank(0);
                //		SetVROM_8K_Bank( 1 );
            }
            SetVRAM_Mirror(VRAM_VMIRROR);
        }

        //void Mapper011::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            //DEBUGOUT("WR A:%04X D:%02X\n", addr, data);
            SetPROM_32K_Bank(data);
            if (VROM_1K_SIZE != 0)
            {
                SetVROM_8K_Bank(data >> 4);
            }
        }


    }
}
