﻿//////////////////////////////////////////////////////////////////////////
// Mapper165   Fire Emblem Chinese version                                                         //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using BYTE = System.Byte;
using INT = System.Int32;


namespace VirtualNes.Core
{
    public class Mapper165 : Mapper
    {
        BYTE[] reg = new byte[8];
        BYTE prg0, prg1;
        BYTE chr0, chr1, chr2, chr3;
        BYTE we_sram;
        BYTE latch;
        public Mapper165(NES parent) : base(parent)
        {
        }

        public override void Reset()

        {
            for (INT i = 0; i < 8; i++)
            {
                reg[i] = 0x00;
            }
            prg0 = 0;
            prg1 = 1;
            SetBank_CPU();

            chr0 = 0;
            chr1 = 0;
            chr2 = 4;
            chr3 = 4;
            latch = 0xFD;
            SetBank_PPU();

            we_sram = 0;    // Disable

            nes.ppu.SetChrLatchMode(true);
        }

        //void Mapper165::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {

            switch (addr & 0xE001)
            {
                case 0x8000:
                    reg[0] = data;
                    SetBank_CPU();
                    SetBank_PPU();
                    break;
                case 0x8001:
                    reg[1] = data;

                    switch (reg[0] & 0x07)
                    {
                        case 0x00:
                            chr0 = (byte)(data & 0xFC);
                            if (latch == 0xFD)
                                SetBank_PPU();
                            break;
                        case 0x01:
                            chr1 = (byte)(data & 0xFC);
                            if (latch == 0xFE)
                                SetBank_PPU();
                            break;

                        case 0x02:
                            chr2 = (byte)(data & 0xFC);
                            if (latch == 0xFD)
                                SetBank_PPU();
                            break;
                        case 0x04:
                            chr3 = (byte)(data & 0xFC);
                            if (latch == 0xFE)
                                SetBank_PPU();
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
                case 0xA000:
                    reg[2] = data;
                    if ((data & 0x01) != 0)
                    {
                        SetVRAM_Mirror(VRAM_HMIRROR);
                    }
                    else
                    {
                        SetVRAM_Mirror(VRAM_VMIRROR);
                    }
                    break;
                case 0xA001:
                    reg[3] = data;
                    break;
                default:
                    break;
            }

        }

        void SetBank_CPU()
        {
            SetPROM_32K_Bank(prg0, prg1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);
        }

        void SetBank_PPU()
        {
            if (latch == 0xFD)
            {
                SetBank_PPUSUB(0, chr0);
                SetBank_PPUSUB(4, chr2);
            }
            else
            {
                SetBank_PPUSUB(0, chr1);
                SetBank_PPUSUB(4, chr3);
            }
        }

        void SetBank_PPUSUB(int bank, int page)
        {
            if (page == 0)
            {
                SetCRAM_4K_Bank((byte)bank, page >> 2);
            }
            else
            {
                SetVROM_4K_Bank((byte)bank, page >> 2);
            }
        }

        //void Mapper165::PPU_ChrLatch(WORD addr)
        public override void PPU_ChrLatch(ushort addr)
        {
            ushort mask = (ushort)(addr & 0x1FF0);

            if (mask == 0x1FD0)
            {
                latch = 0xFD;
                SetBank_PPU();
            }
            else if (mask == 0x1FE0)
            {
                latch = 0xFE;
                SetBank_PPU();
            }

        }

        //void Mapper165::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            for (INT i = 0; i < 8; i++)
            {
                p[i] = reg[i];
            }
            p[8] = prg0;
            p[9] = prg1;
            p[10] = chr0;
            p[11] = chr1;
            p[12] = chr2;
            p[13] = chr3;
            p[14] = latch;
        }

        //void Mapper165::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            for (INT i = 0; i < 8; i++)
            {
                reg[i] = p[i];
            }
            prg0 = p[8];
            prg1 = p[9];
            chr0 = p[10];
            chr1 = p[11];
            chr2 = p[12];
            chr3 = p[13];
            latch = p[14];
        }

    }
}
