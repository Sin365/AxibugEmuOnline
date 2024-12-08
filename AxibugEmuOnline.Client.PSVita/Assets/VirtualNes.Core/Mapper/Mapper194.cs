////////////////////////////////////////////
// Mapper194 迷宮寺院ダババ                                             //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
    public class Mapper194 : Mapper
    {
        public Mapper194(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            SetPROM_32K_Bank(PROM_32K_SIZE - 1);
        }

        //void Mapper194::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            SetPROM_8K_Bank(3, data);
        }

    }
}
