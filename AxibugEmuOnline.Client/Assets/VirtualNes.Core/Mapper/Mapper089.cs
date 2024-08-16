//////////////////////////////////////////////////////////////////////////
// Mapper089  SunSoft (水戸黄門)                                        //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;


namespace VirtualNes.Core
{
    public class Mapper089 : Mapper
    {
        public Mapper089(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);
            SetVROM_8K_Bank(0);
        }

        //void Mapper089::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            if ((addr & 0xFF00) == 0xC000)
            {
                SetPROM_16K_Bank(4, (data & 0x70) >> 4);

                SetVROM_8K_Bank(((data & 0x80) >> 4) | (data & 0x07));

                if ((data & 0x08) != 0) SetVRAM_Mirror(VRAM_MIRROR4H);
                else SetVRAM_Mirror(VRAM_MIRROR4L);
            }
        }


    }
}
