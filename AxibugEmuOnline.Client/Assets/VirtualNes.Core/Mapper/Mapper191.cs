//////////////////////////////////////////////////////////////////////////
// Mapper191 SACHEN Super Cartridge Xin1 (Ver.1-9)                      //
//           SACHEN Q-BOY Support                                       //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
    public class Mapper191 : Mapper
    {
        BYTE[] reg = new BYTE[8];
        BYTE prg0, prg1;
        BYTE chr0, chr1, chr2, chr3;
        BYTE highbank;
        public Mapper191(NES parent) : base(parent)
        {
        }

        public override void Reset()

        {
            for (INT i = 0; i < 8; i++)
            {
                reg[i] = 0x00;
            }

            prg0 = 0;
            //	prg1 = 1;
            SetBank_CPU();

            chr0 = 0;
            chr1 = 0;
            chr2 = 0;
            chr3 = 0;
            highbank = 0;
            SetBank_PPU();
        }

        //void Mapper191::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            switch (addr)
            {
                case 0x4100:
                    reg[0] = data;
                    break;
                case 0x4101:
                    reg[1] = data;
                    switch (reg[0])
                    {
                        case 0:
                            chr0 = (byte)(data & 7);
                            SetBank_PPU();
                            break;
                        case 1:
                            chr1 = (byte)(data & 7);
                            SetBank_PPU();
                            break;
                        case 2:
                            chr2 = (byte)(data & 7);
                            SetBank_PPU();
                            break;
                        case 3:
                            chr3 = (byte)(data & 7);
                            SetBank_PPU();
                            break;
                        case 4:
                            highbank = (byte)(data & 7);
                            SetBank_PPU();
                            break;
                        case 5:
                            prg0 = (byte)(data & 7);
                            SetBank_CPU();
                            break;
                        case 7:
                            if ((data & 0x02) != 0) SetVRAM_Mirror(VRAM_HMIRROR);
                            else SetVRAM_Mirror(VRAM_VMIRROR);
                            break;
                    }
                    break;
            }
        }

        void SetBank_CPU()
        {
            SetPROM_32K_Bank(prg0);
        }

        void SetBank_PPU()
        {
            if (VROM_1K_SIZE != 0)
            {
                SetVROM_1K_Bank(0, (((highbank << 3) + chr0) << 2) + 0);
                SetVROM_1K_Bank(1, (((highbank << 3) + chr0) << 2) + 1);
                SetVROM_1K_Bank(2, (((highbank << 3) + chr1) << 2) + 2);
                SetVROM_1K_Bank(3, (((highbank << 3) + chr1) << 2) + 3);
                SetVROM_1K_Bank(4, (((highbank << 3) + chr2) << 2) + 0);
                SetVROM_1K_Bank(5, (((highbank << 3) + chr2) << 2) + 1);
                SetVROM_1K_Bank(6, (((highbank << 3) + chr3) << 2) + 2);
                SetVROM_1K_Bank(7, (((highbank << 3) + chr3) << 2) + 3);
            }
        }

        public override bool IsStateSave()
        {
            return true;
        }



        //void Mapper191::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            p[0] = prg0;
            p[1] = chr0;
            p[2] = chr1;
            p[3] = chr2;
            p[4] = chr3;
            p[5] = highbank;
        }

        //void Mapper191::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            prg0 = p[0];
            chr0 = p[1];
            chr1 = p[2];
            chr2 = p[3];
            chr3 = p[4];
            highbank = p[5];
        }
    }
}
