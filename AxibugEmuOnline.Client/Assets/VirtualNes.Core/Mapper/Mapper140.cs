//////////////////////////////////////////////////////////////////////////
// Mapper140                                                            //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;


namespace VirtualNes.Core
{
    public class Mapper140 : Mapper
    {
        public Mapper140(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(0);
            if (VROM_1K_SIZE != 0)
            {
                SetVROM_8K_Bank(0);
            }
        }

        //void Mapper140::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            SetPROM_32K_Bank((data & 0xF0) >> 4);
            SetVROM_8K_Bank(data & 0x0F);
        }

    }
}
