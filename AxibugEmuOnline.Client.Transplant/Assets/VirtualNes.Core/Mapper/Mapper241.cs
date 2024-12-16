//////////////////////////////////////////////////////////////////////////
// Mapper241  Fon Serm Bon                                              //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;


namespace VirtualNes.Core
{
    public class Mapper241 : Mapper
    {
        public Mapper241(NES parent) : base(parent)
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

        //void Mapper241::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            if (addr == 0x8000)
            {
                SetPROM_32K_Bank(data);
            }
        }

    }
}
