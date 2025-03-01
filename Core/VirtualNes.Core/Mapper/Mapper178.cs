﻿//////////////////////////////////////////////////////////////////////////
// Mapper178  Education / WaiXing_FS305                                 //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using BYTE = System.Byte;


namespace VirtualNes.Core
{
    public class Mapper178 : Mapper
    {
        BYTE[] reg = new byte[3];
        BYTE banknum;
        BYTE OP_rom;
        public Mapper178(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            reg[0] = 0;
            reg[1] = 0;
            reg[2] = 0;
            banknum = 0;
            SetBank_CPU();
            OP_rom = 0;

            uint crc = nes.rom.GetPROM_CRC();
            if (crc == 0x925926BC       //[ES-XXXX] Kou Dai Zuan Shi Zhi Chong Wu Xiao Jing Ling 2 (C)
             || crc == 0xB0B13DBD)      //[ES-XXXX] Chong Wu Fei Cui Zhi Chong Wu Xiao Jing Ling IV (C)
            {
                OP_rom = 1;
            }
        }

        //void Mapper178::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            //	if( addr >= 0x4000 && addr <= 0x5FFF ) DEBUGOUT("Address=%04X Data=%02X\n", addr&0xFFFF, data&0xFF );
            if (addr == 0x4800)
            {
                if ((data & 0x01) != 0) SetVRAM_Mirror(VRAM_HMIRROR);
                else SetVRAM_Mirror(VRAM_VMIRROR);
            }
            else if (addr == 0x4801)
            {
                reg[0] = (byte)((data >> 1) & 0x0F);
                if (OP_rom != 0) reg[0] = (byte)(data << 2);
                SetBank_CPU();
            }
            else if (addr == 0x4802)
            {
                reg[1] = (byte)(data << 2);
                if (OP_rom != 0) reg[1] = data;
                SetBank_CPU();
            }
            else if (addr == 0x4803)
            {
                //unknown
                reg[2] = data;
            }
            else if (addr >= 0x6000)
            {
                CPU_MEM_BANK[addr >> 13][addr & 0x1FFF] = data;
            }
        }

        void SetBank_CPU()
        {
            banknum = (byte)(reg[0] | reg[1]);
            //	DEBUGOUT("Bank=%02X\n", banknum&0xFF );
            SetPROM_32K_Bank(banknum);
        }
    }
}
