//////////////////////////////////////////////////////////////////////////
// Mapper232  Quattro Games                                             //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
    public class Mapper232 : Mapper
    {
        BYTE[] reg = new byte[2];
        public Mapper232(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);

            reg[0] = 0x0C;
            reg[1] = 0x00;
        }

        //void Mapper232::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            if (addr >= 0x6000)
            {
                Write(addr, data);
            }
        }

        //void Mapper232::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            //	if( addr == 0x9000 ) {
            //		reg[0] = (data & 0x18)>>1;
            //	} else if( addr >= 0xA000 && addr <= 0xFFFF ) {
            //		reg[1] = data & 0x03;
            //	}
            if (addr <= 0x9FFF)
            {
                reg[0] = (byte)((data & 0x18) >> 1);
            }
            else
            {
                reg[1] = (byte)(data & 0x03);
            }

            SetPROM_8K_Bank(4, (reg[0] | reg[1]) * 2 + 0);
            SetPROM_8K_Bank(5, (reg[0] | reg[1]) * 2 + 1);
            SetPROM_8K_Bank(6, (reg[0] | 0x03) * 2 + 0);
            SetPROM_8K_Bank(7, (reg[0] | 0x03) * 2 + 1);
        }

        public override bool IsStateSave()
        {
            return true;
        }



        //void Mapper232::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            p[0] = reg[0];
            p[1] = reg[1];
        }

        //void Mapper232::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            reg[0] = p[0];
            reg[1] = p[1];
        }
    }
}
