//////////////////////////////////////////////////////////////////////////
// Mapper107  Magic Dragon Mapper                                       //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;

namespace VirtualNes.Core
{
    public class Mapper107 : Mapper
    {
        public Mapper107(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);
            SetVROM_8K_Bank(0);
        }

        //void Mapper107::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            SetPROM_32K_Bank((data >> 1) & 0x03);
            SetVROM_8K_Bank(data & 0x07);
        }


    }
}
