﻿//////////////////////////////////////////////////////////////////////////
// Mapper222                                                            //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;


namespace VirtualNes.Core
{
    public class Mapper222 : Mapper
    {
        public Mapper222(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);
            if (VROM_1K_SIZE != 0)
            {
                SetVROM_8K_Bank(0);
            }
            SetVRAM_Mirror(VRAM_VMIRROR);
        }

        //void Mapper222::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            switch (addr & 0xF003)
            {
                case 0x8000:
                    SetPROM_8K_Bank(4, data);
                    break;
                case 0xA000:
                    SetPROM_8K_Bank(5, data);
                    break;
                case 0xB000:
                    SetVROM_1K_Bank(0, data);
                    break;
                case 0xB002:
                    SetVROM_1K_Bank(1, data);
                    break;
                case 0xC000:
                    SetVROM_1K_Bank(2, data);
                    break;
                case 0xC002:
                    SetVROM_1K_Bank(3, data);
                    break;
                case 0xD000:
                    SetVROM_1K_Bank(4, data);
                    break;
                case 0xD002:
                    SetVROM_1K_Bank(5, data);
                    break;
                case 0xE000:
                    SetVROM_1K_Bank(6, data);
                    break;
                case 0xE002:
                    SetVROM_1K_Bank(7, data);
                    break;
            }
        }

        public override void LoadState(byte[] p)
        {
            //
        }

        public override void SaveState(byte[] p)
        {
            //
        }



    }
}
