﻿using static VirtualNes.MMU;
using BYTE = System.Byte;
using INT = System.Int32;


namespace VirtualNes.Core
{
    public class Mapper251 : Mapper
    {
        BYTE[] reg = new BYTE[11];
        BYTE[] breg = new BYTE[4];
        public Mapper251(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);

            SetVRAM_Mirror(VRAM_VMIRROR);

            INT i;
            for (i = 0; i < 11; i++)
                reg[i] = 0;
            for (i = 0; i < 4; i++)
                breg[i] = 0;
        }

        //void Mapper251::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            if ((addr & 0xE001) == 0x6000)
            {
                if (reg[9] != null)
                {
                    breg[reg[10]++] = data;
                    if (reg[10] == 4)
                    {
                        reg[10] = 0;
                        SetBank();
                    }
                }
            }
        }

        //void Mapper251::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            switch (addr & 0xE001)
            {
                case 0x8000:
                    reg[8] = data;
                    SetBank();
                    break;
                case 0x8001:
                    reg[reg[8] & 0x07] = data;
                    SetBank();
                    break;
                case 0xA001:
                    if ((data & 0x80) != 0)
                    {
                        reg[9] = 1;
                        reg[10] = 0;
                    }
                    else
                    {
                        reg[9] = 0;
                    }
                    break;
            }
        }

        //void Mapper251::SetBank()
        public void SetBank()
        {
            INT[] chr = new INT[6];
            INT[] prg = new int[4];

            for (INT i = 0; i < 6; i++)
            {
                chr[i] = (reg[i] | (breg[1] << 4)) & ((breg[2] << 4) | 0x0F);
            }

            if ((reg[8] & 0x80) != 0)
            {
                SetVROM_8K_Bank(chr[2], chr[3], chr[4], chr[5], chr[0], chr[0] + 1, chr[1], chr[1] + 1);
            }
            else
            {
                SetVROM_8K_Bank(chr[0], chr[0] + 1, chr[1], chr[1] + 1, chr[2], chr[3], chr[4], chr[5]);
            }

            prg[0] = (reg[6] & ((breg[3] & 0x3F) ^ 0x3F)) | (breg[1]);
            prg[1] = (reg[7] & ((breg[3] & 0x3F) ^ 0x3F)) | (breg[1]);
            prg[2] = prg[3] = ((breg[3] & 0x3F) ^ 0x3F) | (breg[1]);
            prg[2] &= PROM_8K_SIZE - 1;

            if ((reg[8] & 0x40) != 0)
            {
                SetPROM_32K_Bank(prg[2], prg[1], prg[0], prg[3]);
            }
            else
            {
                SetPROM_32K_Bank(prg[0], prg[1], prg[2], prg[3]);
            }
        }
        public override bool IsStateSave()
        {
            return true;
        }

        //void Mapper251::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            INT i;

            for (i = 0; i < 11; i++)
            {
                p[i] = reg[i];
            }
            for (i = 0; i < 4; i++)
            {
                p[i + 11] = breg[i];
            }
        }

        //void Mapper251::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            INT i;
            for (i = 0; i < 11; i++)
            {
                reg[i] = p[i];
            }
            for (i = 0; i < 4; i++)
            {
                reg[i] = p[i + 11];
            }
        }
    }
}
