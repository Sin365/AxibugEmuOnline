//////////////////////////////////////////////////////////////////////////
// Mapper242  Wai Xing Zhan Shi                                         //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;


namespace VirtualNes.Core
{
    public class Mapper242 : Mapper
    {
        public Mapper242(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(0);
        }

        //void Mapper242::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            if ((addr & 0x01) != 0)
            {
                SetPROM_32K_Bank((addr & 0xF8) >> 3);
            }
        }

    }
}
