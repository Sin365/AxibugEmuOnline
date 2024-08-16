//////////////////////////////////////////////////////////////////////////
// Mapper097  Irem 74161                                                //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;


namespace VirtualNes.Core
{
    public class Mapper097 : Mapper
    {
        public Mapper097(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(PROM_8K_SIZE - 2, PROM_8K_SIZE - 1, 0, 1);

            if (VROM_8K_SIZE != 0)
            {
                SetVROM_8K_Bank(0);
            }
        }

        //void Mapper097::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            if (addr < 0xC000)
            {
                SetPROM_16K_Bank(6, data & 0x0F);

                if ((data & 0x80) != 0) SetVRAM_Mirror(VRAM_VMIRROR);
                else SetVRAM_Mirror(VRAM_HMIRROR);
            }
        }


    }
}
