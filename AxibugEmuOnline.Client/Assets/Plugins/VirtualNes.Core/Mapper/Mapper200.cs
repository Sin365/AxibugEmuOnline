//////////////////////////////////////////////
// Mapper200  1200-in-1                                                 //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;


namespace VirtualNes.Core
{
    public class Mapper200 : Mapper
    {
        public Mapper200(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            //	SetPROM_32K_Bank( 0, 1, PROM_8K_SIZE-2, PROM_8K_SIZE-1 );
            SetPROM_16K_Bank(4, 0);
            SetPROM_16K_Bank(6, 0);

            if (VROM_1K_SIZE != 0)
            {
                SetVROM_8K_Bank(0);
            }
        }

        //void Mapper200::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            SetPROM_16K_Bank(4, addr & 0x07);
            SetPROM_16K_Bank(6, addr & 0x07);
            SetVROM_8K_Bank(addr & 0x07);

            if ((addr & 0x01) != 0)
            {
                SetVRAM_Mirror(VRAM_VMIRROR);
            }
            else
            {
                SetVRAM_Mirror(VRAM_HMIRROR);
            }
        }
    }
}
