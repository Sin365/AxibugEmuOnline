//////////////////////////////////////////////////////////////////////////
// Mapper058                                                            //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;


namespace VirtualNes.Core
{
    public class Mapper058 : Mapper
    {
        public Mapper058(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(0, 1, 0, 1);
            if (VROM_1K_SIZE != 0)
            {
                SetVROM_8K_Bank(0);
            }
        }

        //void Mapper058::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            if ((addr & 0x40) != 0)
            {
                SetPROM_16K_Bank(4, addr & 0x07);
                SetPROM_16K_Bank(6, addr & 0x07);
            }
            else
            {
                SetPROM_32K_Bank((addr & 0x06) >> 1);
            }

            if (VROM_1K_SIZE != 0)
            {
                SetVROM_8K_Bank((addr & 0x38) >> 3);
            }

            if ((data & 0x02) != 0) SetVRAM_Mirror(VRAM_VMIRROR);
            else SetVRAM_Mirror(VRAM_HMIRROR);
        }



    }
}
