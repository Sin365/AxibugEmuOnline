//////////////////////////////////////////////////////////////////////////
// Mapper135  SACHEN CHEN                                               //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
    public class Mapper135 : Mapper
    {
        BYTE cmd;
        BYTE chr0l, chr1l, chr0h, chr1h, chrch;
        public Mapper135(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            cmd = 0;
            chr0l = chr1l = chr0h = chr1h = chrch = 0;

            SetPROM_32K_Bank(0);
            SetBank_PPU();
        }

        //void Mapper135::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            switch (addr & 0x4101)
            {
                case 0x4100:
                    cmd = (byte)(data & 0x07);
                    break;
                case 0x4101:
                    switch (cmd)
                    {
                        case 0:
                            chr0l = (byte)(data & 0x07);
                            SetBank_PPU();
                            break;
                        case 1:
                            chr0h = (byte)(data & 0x07);
                            SetBank_PPU();
                            break;
                        case 2:
                            chr1l = (byte)(data & 0x07);
                            SetBank_PPU();
                            break;
                        case 3:
                            chr1h = (byte)(data & 0x07);
                            SetBank_PPU();
                            break;
                        case 4:
                            chrch = (byte)(data & 0x07);
                            SetBank_PPU();
                            break;
                        case 5:
                            SetPROM_32K_Bank((byte)(data & 0x07));
                            break;
                        case 6:
                            break;
                        case 7:
                            switch ((data >> 1) & 0x03)
                            {
                                case 0: SetVRAM_Mirror(VRAM_MIRROR4L); break;
                                case 1: SetVRAM_Mirror(VRAM_HMIRROR); break;
                                case 2: SetVRAM_Mirror(VRAM_VMIRROR); break;
                                case 3: SetVRAM_Mirror(VRAM_MIRROR4L); break;
                            }
                            break;
                    }
                    break;
            }

            CPU_MEM_BANK[addr >> 13][addr & 0x1FFF] = data;
        }

        void SetBank_PPU()
        {
            SetVROM_2K_Bank(0, 0 | (chr0l << 1) | (chrch << 4));
            SetVROM_2K_Bank(2, 1 | (chr0h << 1) | (chrch << 4));
            SetVROM_2K_Bank(4, 0 | (chr1l << 1) | (chrch << 4));
            SetVROM_2K_Bank(6, 1 | (chr1h << 1) | (chrch << 4));
        }

        //void Mapper135::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            p[0] = cmd;
            p[1] = chr0l;
            p[2] = chr0h;
            p[3] = chr1l;
            p[4] = chr1h;
            p[5] = chrch;
        }

        //void Mapper135::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            cmd = p[0];
            chr0l = p[1];
            chr0h = p[2];
            chr0l = p[3];
            chr0h = p[4];
            chrch = p[5];
        }

    }
}
