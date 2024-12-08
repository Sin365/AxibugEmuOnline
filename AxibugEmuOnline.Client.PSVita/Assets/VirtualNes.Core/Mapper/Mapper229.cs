//////////////////////////////////////////////////////////////////////////
// Mapper229  31-in-1                                                   //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
    public class Mapper229 : Mapper
    {
        public Mapper229(NES parent) : base(parent)
        {
        }

		public override void Reset()
		{
            SetPROM_32K_Bank(0);
            SetVROM_8K_Bank(0);
        }

        //void Mapper229::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            if ((addr & 0x001E) != 0)
            {
                BYTE prg = (byte)(addr & 0x001F);

                SetPROM_8K_Bank(4, prg * 2 + 0);
                SetPROM_8K_Bank(5, prg * 2 + 1);
                SetPROM_8K_Bank(6, prg * 2 + 0);
                SetPROM_8K_Bank(7, prg * 2 + 1);

                SetVROM_8K_Bank(addr & 0x0FFF);
            }
            else
            {
                SetPROM_32K_Bank(0);
                SetVROM_8K_Bank(0);
            }

            if ((addr & 0x0020) != 0)
            {
                SetVRAM_Mirror(VRAM_HMIRROR);
            }
            else
            {
                SetVRAM_Mirror(VRAM_VMIRROR);
            }
        }

    }
}
