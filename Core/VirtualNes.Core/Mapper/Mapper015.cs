﻿//////////////////////////////////////////////////////////////////////////
// Mapper015  100-in-1 chip                                             //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;


namespace VirtualNes.Core
{
    public class Mapper015 : Mapper
    {

        public Mapper015(NES parent) : base(parent) { }

        public override void Reset()
        {
            SetPROM_32K_Bank(0, 1, 2, 3);
        }

        //void Mapper015::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            switch (addr)
            {
                case 0x8000:
                    if ((data & 0x80) != 0)
                    {
                        SetPROM_8K_Bank(4, (data & 0x3F) * 2 + 1);
                        SetPROM_8K_Bank(5, (data & 0x3F) * 2 + 0);
                        SetPROM_8K_Bank(6, (data & 0x3F) * 2 + 3);
                        SetPROM_8K_Bank(7, (data & 0x3F) * 2 + 2);
                    }
                    else
                    {
                        SetPROM_8K_Bank(4, (data & 0x3F) * 2 + 0);
                        SetPROM_8K_Bank(5, (data & 0x3F) * 2 + 1);
                        SetPROM_8K_Bank(6, (data & 0x3F) * 2 + 2);
                        SetPROM_8K_Bank(7, (data & 0x3F) * 2 + 3);
                    }
                    if ((data & 0x40) != 0)
                        SetVRAM_Mirror(VRAM_HMIRROR);
                    else SetVRAM_Mirror(VRAM_VMIRROR);
                    break;
                case 0x8001:
                    if ((data & 0x80) != 0)
                    {
                        SetPROM_8K_Bank(6, (data & 0x3F) * 2 + 1);
                        SetPROM_8K_Bank(7, (data & 0x3F) * 2 + 0);
                    }
                    else
                    {
                        SetPROM_8K_Bank(6, (data & 0x3F) * 2 + 0);
                        SetPROM_8K_Bank(7, (data & 0x3F) * 2 + 1);
                    }
                    break;
                case 0x8002:
                    if ((data & 0x80) != 0)
                    {
                        SetPROM_8K_Bank(4, (data & 0x3F) * 2 + 1);
                        SetPROM_8K_Bank(5, (data & 0x3F) * 2 + 1);
                        SetPROM_8K_Bank(6, (data & 0x3F) * 2 + 1);
                        SetPROM_8K_Bank(7, (data & 0x3F) * 2 + 1);
                    }
                    else
                    {
                        SetPROM_8K_Bank(4, (data & 0x3F) * 2 + 0);
                        SetPROM_8K_Bank(5, (data & 0x3F) * 2 + 0);
                        SetPROM_8K_Bank(6, (data & 0x3F) * 2 + 0);
                        SetPROM_8K_Bank(7, (data & 0x3F) * 2 + 0);
                    }
                    break;
                case 0x8003:
                    if ((data & 0x80) != 0)
                    {
                        SetPROM_8K_Bank(6, (data & 0x3F) * 2 + 1);
                        SetPROM_8K_Bank(7, (data & 0x3F) * 2 + 0);
                    }
                    else
                    {
                        SetPROM_8K_Bank(6, (data & 0x3F) * 2 + 0);
                        SetPROM_8K_Bank(7, (data & 0x3F) * 2 + 1);
                    }
                    if ((data & 0x40) != 0) SetVRAM_Mirror(VRAM_HMIRROR);
                    else SetVRAM_Mirror(VRAM_VMIRROR);
                    break;
            }
        }


    }
}
