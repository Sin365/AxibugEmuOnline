//////////////////////////////////////////////////////////////////////////
// Mapper008  FFE F3xxx                                                 //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;


namespace VirtualNes.Core
{
    public class Mapper008 : Mapper
    {

        public Mapper008(NES parent) : base(parent) { }

        public override void Reset()
        {
            SetPROM_32K_Bank(0, 1, 2, 3);
            SetVROM_8K_Bank(0);
        }

        //void Mapper008::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            SetPROM_16K_Bank(4, (data & 0xF8) >> 3);
            SetVROM_8K_Bank(data & 0x07);
        }


    }
}
