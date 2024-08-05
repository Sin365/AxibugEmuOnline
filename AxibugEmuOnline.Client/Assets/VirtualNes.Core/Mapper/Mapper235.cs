//////////////////////////////////////////////////////////////////////////
// Mapper235  150-in-1                                                  //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
    public class Mapper235 : Mapper
    {
        public Mapper235(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            for (INT i = 0; i < 0x2000; i++)
            {
                DRAM[i] = 0xFF;
            }

            SetPROM_32K_Bank(0);
        }

        //void Mapper235::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            {
                BYTE prg = (byte)(((addr & 0x0300) >> 3) | (addr & 0x001F));
                BYTE bus = 0;

                if (PROM_8K_SIZE == 64 * 2)
                {
                    // 100-in-1
                    switch (addr & 0x0300)
                    {
                        case 0x0000: break;
                        case 0x0100: bus = 1; break;
                        case 0x0200: bus = 1; break;
                        case 0x0300: bus = 1; break;
                    }
                }
                else if (PROM_8K_SIZE == 128 * 2)
                {
                    // 150-in-1
                    switch (addr & 0x0300)
                    {
                        case 0x0000: break;
                        case 0x0100: bus = 1; break;
                        case 0x0200: prg = (byte)((prg & 0x1F) | 0x20); break;
                        case 0x0300: bus = 1; break;
                    }
                }
                else if (PROM_8K_SIZE == 192 * 2)
                {
                    // 150-in-1
                    switch (addr & 0x0300)
                    {
                        case 0x0000: break;
                        case 0x0100: bus = 1; break;
                        case 0x0200: prg = (byte)((prg & 0x1F) | 0x20); break;
                        case 0x0300: prg = (byte)((prg & 0x1F) | 0x40); break;
                    }
                }
                else if (PROM_8K_SIZE == 256 * 2)
                {
                }

                if ((addr & 0x0800) != 0)
                {
                    if ((addr & 0x1000) != 0)
                    {
                        SetPROM_8K_Bank(4, prg * 4 + 2);
                        SetPROM_8K_Bank(5, prg * 4 + 3);
                        SetPROM_8K_Bank(6, prg * 4 + 2);
                        SetPROM_8K_Bank(7, prg * 4 + 3);
                    }
                    else
                    {
                        SetPROM_8K_Bank(4, prg * 4 + 0);
                        SetPROM_8K_Bank(5, prg * 4 + 1);
                        SetPROM_8K_Bank(6, prg * 4 + 0);
                        SetPROM_8K_Bank(7, prg * 4 + 1);
                    }
                }
                else
                {
                    SetPROM_32K_Bank(prg);
                }

                if (bus != 0)
                {
                    SetPROM_Bank(4, DRAM, BANKTYPE_ROM);
                    SetPROM_Bank(5, DRAM, BANKTYPE_ROM);
                    SetPROM_Bank(6, DRAM, BANKTYPE_ROM);
                    SetPROM_Bank(7, DRAM, BANKTYPE_ROM);
                }

                if ((addr & 0x0400) != 0)
                {
                    SetVRAM_Mirror(VRAM_MIRROR4L);
                }
                else if ((addr & 0x2000) != 0)
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
}