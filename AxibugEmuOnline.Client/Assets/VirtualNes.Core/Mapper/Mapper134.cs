//////////////////////////////////////////////////////////////////////////
// Mapper133  SACHEN CHEN                                               //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
    public class Mapper134 : Mapper
    {
        BYTE cmd, prg, chr;
        public Mapper134(NES parent) : base(parent)
        {
        }

        public override void Reset()

        {
            SetPROM_32K_Bank(0);
            //	SetPROM_16K_Bank( 6, 0 );
            //	SetPROM_16K_Bank( 6, 1 );
            SetVROM_8K_Bank(0);
        }

        //void Mapper134::WriteLow(WORD addr, BYTE data)
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
                            prg = 0;
                            chr = 3;
                            break;
                        case 4:
                            chr &= 0x3;
                            chr |= (byte)((data & 0x07) << 2);
                            break;
                        case 5:
                            prg = (byte)(data & 0x07);
                            break;
                        case 6:
                            chr &= 0x1C;
                            chr |= (byte)(data & 0x3);
                            break;
                        case 7:
                            if ((data & 0x01) != 0) SetVRAM_Mirror(VRAM_HMIRROR);
                            else SetVRAM_Mirror(VRAM_VMIRROR);
                            break;
                    }
                    break;
            }
            SetPROM_32K_Bank(prg);
            //	SetPROM_16K_Bank( 4, (prg<<1)|0 );
            //	SetPROM_16K_Bank( 6, (prg<<1)|1 ); 
            SetVROM_8K_Bank(chr);
            CPU_MEM_BANK[addr >> 13][addr & 0x1FFF] = data;
        }

        //void Mapper134::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            p[0] = cmd;
            p[1] = prg;
            p[2] = chr;
        }

        //void Mapper134::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            cmd = p[0];
            prg = p[1];
            chr = p[2];
        }

    }
}
