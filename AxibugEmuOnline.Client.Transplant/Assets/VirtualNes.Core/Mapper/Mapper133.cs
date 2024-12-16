//////////////////////////////////////////////////////////////////////////
// Mapper133  SACHEN CHEN                                               //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;


namespace VirtualNes.Core
{
    public class Mapper133 : Mapper
    {
        public Mapper133(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(0);
            SetVROM_8K_Bank(0);
        }

        //void Mapper133::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            if (addr == 0x4120)
            {
                SetPROM_32K_Bank((data & 0x04) >> 2);
                SetVROM_8K_Bank(data & 0x03);
            }
            CPU_MEM_BANK[addr >> 13][addr & 0x1FFF] = data;
        }


    }
}
