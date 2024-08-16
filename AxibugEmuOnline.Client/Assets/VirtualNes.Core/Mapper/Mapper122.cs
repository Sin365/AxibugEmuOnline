//////////////////////////////////////////////////////////////////////////
// Mapper122/184  SunSoft-1                                             //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;


namespace VirtualNes.Core
{
    public class Mapper122 : Mapper
    {
        public Mapper122(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(0, 1, 2, 3);
        }

        //void Mapper122::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            if (addr == 0x6000)
            {
                SetVROM_4K_Bank(0, data & 0x07);
                SetVROM_4K_Bank(4, (data & 0x70) >> 4);
            }
        }


    }
}
