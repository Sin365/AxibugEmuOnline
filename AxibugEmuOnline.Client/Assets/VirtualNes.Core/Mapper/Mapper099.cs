﻿//////////////////////////////////////////////////////////////////////////
// Mapper099  VS-Unisystem                                              //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.Core.CPU;
using static VirtualNes.MMU;
using BYTE = System.Byte;


namespace VirtualNes.Core
{
    public class Mapper099 : Mapper
    {
        BYTE coin;
        public Mapper099(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            // set CPU bank pointers
            if (PROM_8K_SIZE > 2)
            {
                SetPROM_32K_Bank(0, 1, 2, 3);
            }
            else if (PROM_8K_SIZE > 1)
            {
                SetPROM_32K_Bank(0, 1, 0, 1);
            }
            else
            {
                SetPROM_32K_Bank(0, 0, 0, 0);
            }

            // set VROM bank
            if (VROM_1K_SIZE != 0)
            {
                SetVROM_8K_Bank(0);
            }

            coin = 0;
        }

        //BYTE Mapper099::ExRead(WORD addr)
        public override byte ExRead(ushort addr)
        {
            if (addr == 0x4020)
            {
                return coin;
            }

            return (byte)(addr >> 8);
        }

        //void Mapper099::ExWrite(WORD addr, BYTE data)
        public override void ExWrite(ushort addr, byte data)
        {
            if (addr == 0x4016)
            {
                if ((data & 0x04) != 0)
                {
                    SetVROM_8K_Bank(1);
                }
                else
                {
                    SetVROM_8K_Bank(0);
                }

                if (nes.rom.GetPROM_CRC() == 0xC99EC059)
                {   // VS Raid on Bungeling Bay(J)
                    if ((data & 0x02) != 0)
                    {
                        nes.cpu.SetIRQ(IRQ_MAPPER);
                    }
                    else
                    {
                        nes.cpu.ClrIRQ(IRQ_MAPPER);
                    }
                }
            }

            if (addr == 0x4020)
            {
                coin = data;
            }
        }


    }
}
