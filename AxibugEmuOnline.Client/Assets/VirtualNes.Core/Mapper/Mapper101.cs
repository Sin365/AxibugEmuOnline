//////////////////////////////////////////////////////////////////////////
// Mapper101  Jaleco(Urusei Yatsura)                                    //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;


namespace VirtualNes.Core
{
    public class Mapper101 : Mapper
    {
        public Mapper101(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(0, 1, 2, 3);
            SetVROM_8K_Bank(0);
        }

        //void Mapper101::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            if (addr >= 0x6000)
            {
                SetVROM_8K_Bank(data & 0x03);
            }
        }

        //void Mapper101::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            SetVROM_8K_Bank(data & 0x03);
        }


    }
}
