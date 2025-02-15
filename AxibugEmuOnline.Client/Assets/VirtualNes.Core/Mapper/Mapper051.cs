﻿//////////////////////////////////////////////////////////////////////////
// Mapper051  11-in-1                                                   //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;


namespace VirtualNes.Core
{
    public class Mapper051 : Mapper
    {
        int mode, bank;
        public Mapper051(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            bank = 0;
            mode = 1;

            SetBank_CPU();
            SetCRAM_8K_Bank(0);
        }

        //void Mapper051::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            if (addr >= 0x6000)
            {
                mode = ((data & 0x10) >> 3) | ((data & 0x02) >> 1);
                SetBank_CPU();
            }
        }

        //void Mapper051::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            bank = (data & 0x0f) << 2;
            if (0xC000 <= addr && addr <= 0xDFFF)
            {
                mode = (mode & 0x01) | ((data & 0x10) >> 3);
            }
            SetBank_CPU();
        }

        void SetBank_CPU()
        {
            switch (mode)
            {
                case 0:
                    SetVRAM_Mirror(VRAM_VMIRROR);
                    SetPROM_8K_Bank(3, (bank | 0x2c | 3));
                    SetPROM_8K_Bank(4, (bank | 0x00 | 0));
                    SetPROM_8K_Bank(5, (bank | 0x00 | 1));
                    SetPROM_8K_Bank(6, (bank | 0x0c | 2));
                    SetPROM_8K_Bank(7, (bank | 0x0c | 3));
                    break;
                case 1:
                    SetVRAM_Mirror(VRAM_VMIRROR);
                    SetPROM_8K_Bank(3, (bank | 0x20 | 3));
                    SetPROM_8K_Bank(4, (bank | 0x00 | 0));
                    SetPROM_8K_Bank(5, (bank | 0x00 | 1));
                    SetPROM_8K_Bank(6, (bank | 0x00 | 2));
                    SetPROM_8K_Bank(7, (bank | 0x00 | 3));
                    break;
                case 2:
                    SetVRAM_Mirror(VRAM_VMIRROR);
                    SetPROM_8K_Bank(3, (bank | 0x2e | 3));
                    SetPROM_8K_Bank(4, (bank | 0x02 | 0));
                    SetPROM_8K_Bank(5, (bank | 0x02 | 1));
                    SetPROM_8K_Bank(6, (bank | 0x0e | 2));
                    SetPROM_8K_Bank(7, (bank | 0x0e | 3));
                    break;
                case 3:
                    SetVRAM_Mirror(VRAM_HMIRROR);
                    SetPROM_8K_Bank(3, (bank | 0x20 | 3));
                    SetPROM_8K_Bank(4, (bank | 0x00 | 0));
                    SetPROM_8K_Bank(5, (bank | 0x00 | 1));
                    SetPROM_8K_Bank(6, (bank | 0x00 | 2));
                    SetPROM_8K_Bank(7, (bank | 0x00 | 3));
                    break;
            }
        }

        //void Mapper051::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            p[0] = (byte)mode;
            p[1] = (byte)bank;
        }

        //void Mapper051::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            mode = p[0];
            bank = p[1];
        }


        public override bool IsStateSave()
        {
            return true;
        }
    }
}
