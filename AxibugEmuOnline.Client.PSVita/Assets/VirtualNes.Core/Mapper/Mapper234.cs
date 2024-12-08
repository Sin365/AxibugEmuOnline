//////////////////////////////////////////////////////////////////////////
// Mapper234  Maxi-15                                                   //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{

    public class Mapper234 : Mapper
    {
        BYTE[] reg = new byte[2];
        public Mapper234(NES parent) : base(parent)
        {
        }

        public override void Reset()

        {
            SetPROM_32K_Bank(0, 1, 2, 3);

            reg[0] = 0;
            reg[1] = 0;
        }

        //void Mapper234::Read(WORD addr, BYTE data)
        public override void Read(ushort addr, byte data)
        {
            if (addr >= 0xFF80 && addr <= 0xFF9F)
            {
                if (reg[0] != 0)
                {
                    reg[0] = data;
                    SetBank();
                }
            }

            if (addr >= 0xFFE8 && addr <= 0xFFF7)
            {
                reg[1] = data;
                SetBank();
            }
        }

        //void Mapper234::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            if (addr >= 0xFF80 && addr <= 0xFF9F)
            {
                if (reg[0] == 0)
                {
                    reg[0] = data;
                    SetBank();
                }
            }

            if (addr >= 0xFFE8 && addr <= 0xFFF7)
            {
                reg[1] = data;
                SetBank();
            }
        }

        void SetBank()
        {
            if ((reg[0] & 0x80) != 0)
            {
                SetVRAM_Mirror(VRAM_HMIRROR);
            }
            else
            {
                SetVRAM_Mirror(VRAM_VMIRROR);
            }
            if ((reg[0] & 0x40) != 0)
            {
                SetPROM_32K_Bank((reg[0] & 0x0E) | (reg[1] & 0x01));
                SetVROM_8K_Bank(((reg[0] & 0x0E) << 2) | ((reg[1] >> 4) & 0x07));
            }
            else
            {
                SetPROM_32K_Bank(reg[0] & 0x0F);
                SetVROM_8K_Bank(((reg[0] & 0x0F) << 2) | ((reg[1] >> 4) & 0x03));
            }
        }
        public override bool IsStateSave()
        {
            return true;
        }
        //void Mapper234::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            p[0] = reg[0];
            p[1] = reg[1];
        }

        //void Mapper234::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            reg[0] = p[0];
            reg[1] = p[1];
        }
    }
}
