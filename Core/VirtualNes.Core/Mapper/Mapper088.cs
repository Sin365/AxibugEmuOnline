﻿using static VirtualNes.MMU;
using BYTE = System.Byte;


namespace VirtualNes.Core
{
    public class Mapper088 : Mapper
    {
        BYTE reg;
        BYTE patch;
        public Mapper088(NES parent) : base(parent)
        {
        }

        public override void Reset()

        {
            SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);

            if (VROM_1K_SIZE >= 8)
            {
                SetVROM_8K_Bank(0);
            }
            patch = 0;

            uint crc = nes.rom.GetPROM_CRC();
            if (crc == 0xc1b6b2a6)
            { // Devil Man(J)
                patch = 1;
                nes.SetRenderMethod(EnumRenderMethod.POST_RENDER);
            }
            if (crc == 0xd9803a35)
            {   // Quinty(J)
                nes.SetRenderMethod(EnumRenderMethod.POST_RENDER);
            }
        }

        //void Mapper088::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            switch (addr)
            {
                case 0x8000:
                    reg = data;
                    if (patch != 0)
                    {
                        if ((data & 0x40) != 0) SetVRAM_Mirror(VRAM_MIRROR4H);
                        else SetVRAM_Mirror(VRAM_MIRROR4L);
                    }
                    break;
                case 0x8001:
                    switch (reg & 0x07)
                    {
                        case 0:
                            SetVROM_2K_Bank(0, data >> 1);
                            break;
                        case 1:
                            SetVROM_2K_Bank(2, data >> 1);
                            break;
                        case 2:
                            SetVROM_1K_Bank(4, data + 0x40);
                            break;
                        case 3:
                            SetVROM_1K_Bank(5, data + 0x40);
                            break;
                        case 4:
                            SetVROM_1K_Bank(6, data + 0x40);
                            break;
                        case 5:
                            SetVROM_1K_Bank(7, data + 0x40);
                            break;
                        case 6:
                            SetPROM_8K_Bank(4, data);
                            break;
                        case 7:
                            SetPROM_8K_Bank(5, data);
                            break;
                    }
                    break;
                case 0xC000:
                    if (data != 0) SetVRAM_Mirror(VRAM_MIRROR4H);
                    else SetVRAM_Mirror(VRAM_MIRROR4L);
                    break;
            }
        }

        //void Mapper088::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            p[0] = reg;
        }

        //void Mapper088::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            reg = p[0];
        }

        public override bool IsStateSave()
        {
            return true;
        }
    }
}
