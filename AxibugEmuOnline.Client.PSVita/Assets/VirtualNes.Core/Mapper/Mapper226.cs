//////////////////////////////////////////////////////////////////////////
// Mapper226  76-in-1                                                   //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
    public class Mapper226 : Mapper
    {
        BYTE[] reg = new byte[2];
        public Mapper226(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(0);

            reg[0] = 0;
            reg[1] = 0;
        }

        //void Mapper226::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            if ((addr & 0x001) != 0)
            {
                reg[1] = data;
            }
            else
            {
                reg[0] = data;
            }

            if ((reg[0] & 0x40) != 0)
            {
                SetVRAM_Mirror(VRAM_VMIRROR);
            }
            else
            {
                SetVRAM_Mirror(VRAM_HMIRROR);
            }

            BYTE bank = (byte)(((reg[0] & 0x1E) >> 1) | ((reg[0] & 0x80) >> 3) | ((reg[1] & 0x01) << 5));

            if ((reg[0] & 0x20) != 0)
            {
                if ((reg[0] & 0x01) != 0)
                {
                    SetPROM_8K_Bank(4, bank * 4 + 2);
                    SetPROM_8K_Bank(5, bank * 4 + 3);
                    SetPROM_8K_Bank(6, bank * 4 + 2);
                    SetPROM_8K_Bank(7, bank * 4 + 3);
                }
                else
                {
                    SetPROM_8K_Bank(4, bank * 4 + 0);
                    SetPROM_8K_Bank(5, bank * 4 + 1);
                    SetPROM_8K_Bank(6, bank * 4 + 0);
                    SetPROM_8K_Bank(7, bank * 4 + 1);
                }
            }
            else
            {
                SetPROM_8K_Bank(4, bank * 4 + 0);
                SetPROM_8K_Bank(5, bank * 4 + 1);
                SetPROM_8K_Bank(6, bank * 4 + 2);
                SetPROM_8K_Bank(7, bank * 4 + 3);
            }
        }

        public override bool IsStateSave()
        {
            return true;
        }

        //void Mapper226::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            p[0] = reg[0];
            p[1] = reg[1];
        }

        //void Mapper226::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            reg[0] = p[0];
            reg[1] = p[1];
        }

    }
}
