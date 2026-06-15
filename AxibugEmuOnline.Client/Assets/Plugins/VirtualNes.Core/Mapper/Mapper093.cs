//////////////////////////////////////////////////////////////////////////
// Mapper093  SunSoft (Fantasy Zone)                                    //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;


namespace VirtualNes.Core
{
    public class Mapper093 : Mapper
    {
        public Mapper093(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);
            if (VROM_8K_SIZE != 0)
            {
                SetVROM_8K_Bank(0);
            }
        }

        //void Mapper093::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            if (addr == 0x6000)
            {
                SetPROM_16K_Bank(4, data);
            }
        }

    }
}
