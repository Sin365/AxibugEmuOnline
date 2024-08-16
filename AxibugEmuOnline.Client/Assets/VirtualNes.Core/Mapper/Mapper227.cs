//////////////////////////////////////////////////////////////////////////
// Mapper227  1200-in-1                                                 //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using BYTE = System.Byte;


namespace VirtualNes.Core
{
    public class Mapper227 : Mapper
    {
        public Mapper227(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(0, 1, 0, 1);
        }

        //void Mapper227::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            BYTE bank = (byte)(((addr & 0x0100) >> 4) | ((addr & 0x0078) >> 3));

            if ((addr & 0x0001) != 0)
            {
                SetPROM_32K_Bank(bank);
            }
            else
            {
                if ((addr & 0x0004) != 0)
                {
                    SetPROM_8K_Bank(4, bank * 4 + 2);
                    SetPROM_8K_Bank(5, bank * 4 + 3);
                    SetPROM_8K_Bank(6, bank * 4 + 2);
                    SetPROM_8K_Bank(7, bank * 4 + 3);
                }
                else
                {
                    SetPROM_8K_Bank(4, bank * 4 + 0);
                    SetPROM_8K_Bank(5, bank * 4 + 1);
                    SetPROM_8K_Bank(6, bank * 4 + 0);
                    SetPROM_8K_Bank(7, bank * 4 + 1);
                }
            }

            if (!((addr & 0x0080) != 0))
            {
                if ((addr & 0x0200) != 0)
                {
                    SetPROM_8K_Bank(6, (bank & 0x1C) * 4 + 14);
                    SetPROM_8K_Bank(7, (bank & 0x1C) * 4 + 15);
                }
                else
                {
                    SetPROM_8K_Bank(6, (bank & 0x1C) * 4 + 0);
                    SetPROM_8K_Bank(7, (bank & 0x1C) * 4 + 1);
                }
            }
            if ((addr & 0x0002) != 0)
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
