//////////////////////////////////////////////////////////////////////////
// Mapper175  15-in-1 (Kaiser)                                          //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper175 : Mapper
	{
		BYTE reg_dat;
		public Mapper175(NES parent) : base(parent)
		{
		}

		public override void Reset()
        {
            SetPROM_16K_Bank(4, 0);
            SetPROM_16K_Bank(6, 0);
            reg_dat = 0;

            if (VROM_1K_SIZE != 0)
            {
                SetVROM_8K_Bank(0);
            }
        }

        //BYTE Mapper175::Read(WORD addr)
        public override void Read(ushort addr, byte data)
        {
            if (addr == 0xFFFC)
            {
                SetPROM_16K_Bank(4, reg_dat & 0x0F);
                SetPROM_8K_Bank(6, (reg_dat & 0x0F) * 2);
            }
        }

        //void Mapper175::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            switch (addr)
            {
                case 0x8000:
                    if ((data & 0x04)!=0)
                    {
                        SetVRAM_Mirror(VRAM_HMIRROR);
                    }
                    else
                    {
                        SetVRAM_Mirror(VRAM_VMIRROR);
                    }
                    break;
                case 0xA000:
                    reg_dat = data;
                    SetPROM_8K_Bank(7, (reg_dat & 0x0F) * 2 + 1);
                    SetVROM_8K_Bank(reg_dat & 0x0F);
                    break;
            }
        }

        public override bool IsStateSave()
		{
			return true;
		}

	}
}
