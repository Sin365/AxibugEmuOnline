﻿//////////////////////////////////////////////////////////////////////////
// Mapper095  Namcot 106M (Dragon Buster)                               //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using BYTE = System.Byte;


namespace VirtualNes.Core
{
    public class Mapper095 : Mapper
    {
        BYTE reg;
        BYTE prg0, prg1;
        BYTE chr01, chr23, chr4, chr5, chr6, chr7;
        public Mapper095(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            reg = 0x00;
            prg0 = 0;
            prg1 = 1;
            SetBank_CPU();

            if (VROM_1K_SIZE != 0)
            {
                chr01 = 0;
                chr23 = 2;
                chr4 = 4;
                chr5 = 5;
                chr6 = 6;
                chr7 = 7;
            }
            else
            {
                chr01 = chr23 = chr4 = chr5 = chr6 = chr7 = 0;
            }

            SetBank_PPU();

            nes.SetRenderMethod(EnumRenderMethod.POST_RENDER);
        }

        //void Mapper095::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            switch (addr & 0xE001)
            {
                case 0x8000:
                    reg = data;
                    SetBank_CPU();
                    SetBank_PPU();
                    break;
                case 0x8001:
                    if (reg <= 0x05)
                    {
                        if ((data & 0x20) != 0) SetVRAM_Mirror(VRAM_MIRROR4H);
                        else SetVRAM_Mirror(VRAM_MIRROR4L);
                        data &= 0x1F;
                    }

                    switch (reg & 0x07)
                    {
                        case 0x00:
                            if (VROM_1K_SIZE != 0)
                            {
                                chr01 = (byte)(data & 0xFE);
                                SetBank_PPU();
                            }
                            break;
                        case 0x01:
                            if (VROM_1K_SIZE != 0)
                            {
                                chr23 = (byte)(data & 0xFE);
                                SetBank_PPU();
                            }
                            break;
                        case 0x02:
                            if (VROM_1K_SIZE != 0)
                            {
                                chr4 = data;
                                SetBank_PPU();
                            }
                            break;
                        case 0x03:
                            if (VROM_1K_SIZE != 0)
                            {
                                chr5 = data;
                                SetBank_PPU();
                            }
                            break;
                        case 0x04:
                            if (VROM_1K_SIZE != 0)
                            {
                                chr6 = data;
                                SetBank_PPU();
                            }
                            break;
                        case 0x05:
                            if (VROM_1K_SIZE != 0)
                            {
                                chr7 = data;
                                SetBank_PPU();
                            }
                            break;
                        case 0x06:
                            prg0 = data;
                            SetBank_CPU();
                            break;
                        case 0x07:
                            prg1 = data;
                            SetBank_CPU();
                            break;
                    }
                    break;
            }
        }

        void SetBank_CPU()
        {
            if ((reg & 0x40) != 0)
            {
                SetPROM_32K_Bank(PROM_8K_SIZE - 2, prg1, prg0, PROM_8K_SIZE - 1);
            }
            else
            {
                SetPROM_32K_Bank(prg0, prg1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);
            }
        }

        void SetBank_PPU()
        {
            if (VROM_1K_SIZE != 0)
            {
                if ((reg & 0x80) != 0)
                {
                    SetVROM_8K_Bank(chr4, chr5, chr6, chr7,
                             chr01, chr01 + 1, chr23, chr23 + 1);
                }
                else
                {
                    SetVROM_8K_Bank(chr01, chr01 + 1, chr23, chr23 + 1,
                             chr4, chr5, chr6, chr7);
                }
            }
        }

        //void Mapper095::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            p[0] = reg;
            p[1] = prg0;
            p[2] = prg1;
            p[3] = chr01;
            p[4] = chr23;
            p[5] = chr4;
            p[6] = chr5;
            p[7] = chr6;
            p[8] = chr7;
        }

        //void Mapper095::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            reg = p[0];
            prg0 = p[1];
            prg1 = p[2];
            chr01 = p[3];
            chr23 = p[4];
            chr4 = p[5];
            chr5 = p[6];
            chr6 = p[7];
            chr7 = p[8];
        }


        public override bool IsStateSave()
        {
            return true;
        }
    }
}
