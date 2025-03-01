﻿//////////////////////////////////////////////////////////////////////////
// Mapper163  NanJing Games                                             //
//////////////////////////////////////////////////////////////////////////
using System;
using static VirtualNes.MMU;
using BYTE = System.Byte;
using INT = System.Int32;


namespace VirtualNes.Core
{
    public class Mapper163 : Mapper
    {
        //BYTE    strobe;
        //BYTE	security;
        //BYTE	trigger;
        //BYTE	rom_type;

        //BYTE	reg[2];
        BYTE[] reg = new byte[3];
        BYTE strobe, trigger;
        ushort security;
        BYTE rom_type;

        INT www, index;
        public Mapper163(NES parent) : base(parent)
        {
        }

        static Int32[] index_adjust = new Int32[] { -1, -1, -1, -1, 2, 4, 6, 8 };
        static Int16[] step_table = new Int16[] {
7,8,9,10,11,12,13,14,16,17,19,21,23,25,28,31,34,37,41,45,50,55,
60,66,73,80,88,97,107,118,130,143,157,173,190,209,230,253,279,
307,337,371,408,449,494,544,598,658,724,796,876,963,1060,
1166,1282,1411,1552,1707,1878,2066,2272,2499,2749,3024,3327,
3660,4026,4428,4871,5358,5894,6484,7132,7845,8630,9493,10442,
11487,12635,13899,15289,16818,18500,20350,22385,24623,27086,29794,32767
};

        BYTE adpcm_decoder(BYTE data)
        {
            int sb, delta;
            int cur_sample = 0;
            if ((data & 8) != 0) sb = 1; else sb = 0;
            data &= 7;
            delta = (step_table[index] * data) / 4 + step_table[index] / 8;
            if (sb == 1) delta = -delta;
            cur_sample += delta;
            if (cur_sample > 32767) cur_sample = 32767;
            else if (cur_sample < -32768) cur_sample = -32768;
            else cur_sample = cur_sample;
            index += index_adjust[data];
            if (index < 0) index = 0;
            if (index > 88) index = 88;
            return (byte)(cur_sample & 0xff);
        }

        //void Mapper163::Reset()
        public override void Reset()
        {

            index = 0;

            reg[1] = 0xFF;
            strobe = 1;
            security = trigger = reg[0] = 0x00;
            rom_type = 0;
            SetPROM_32K_Bank(15);

            //	jedi_table_init();

            uint crc = nes.rom.GetPROM_CRC();
            if (crc == 0xb6a10d5d)
            {   // Hu Lu Jin Gang (NJ039) (Ch) [dump]
                SetPROM_32K_Bank(0);
            }
            if (crc == 0xf52468e7       // San Guo Wu Shuang - Meng Jiang Zhuan (NJ047) (Ch) [dump]
             || crc == 0x696D98E3)
            {   // San Guo Zhi Lv Bu Zhuan (NJ040) (Ch) [dump]
                rom_type = 1;
            }
            if (crc == 0xEFF96E8A)
            {   // Mo Shou Shi Jie E Mo Lie Ren (NJ097) (Ch) [dump]
                rom_type = 2;
            }

            www = 0;
        }

        ///BYTE Mapper163::ReadLow(WORD addr)
        public override byte ReadLow(ushort addr)
        {
            //	DEBUGOUT( "ReadLow  - addr= %04x\n", addr );

            if ((addr >= 0x5000 && addr < 0x6000))
            {
                switch (addr & 0x7700)
                {
                    case 0x5100:
                        return (byte)security;
                        break;
                    case 0x5500:
                        if (trigger != 0)
                            return (byte)security;
                        else
                            return 0;
                        break;
                }
                return 4;
            }
            else if (addr >= 0x6000)
            {
                return CPU_MEM_BANK[addr >> 13][addr & 0x1FFF];
            }
            return base.ReadLow(addr);
        }

        //void Mapper163::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            //	if(addr==0x5200) DEBUGOUT( "WriteLow - addr= %04x ; dat= %03x\n", addr, data );

            if ((addr >= 0x4020 && addr < 0x6000))
            {
                if (addr == 0x5101)
                {
                    if (strobe != 0 && data == 0)
                    {
                        trigger ^= 1;
                        //				trigger ^= 0xFF;
                    }
                    strobe = data;
                }
                else if (addr == 0x5100 && data == 6)
                {
                    SetPROM_32K_Bank(3);
                }
                else
                {
                    switch (addr & 0x7300)
                    {
                        case 0x5000:
                            reg[1] = data;
                            SetPROM_32K_Bank((reg[1] & 0xF) | (reg[0] << 4));
                            if ((reg[1] & 0x80) == 0 && (nes.ppu.GetScanlineNo() < 128))
                                SetCRAM_8K_Bank(0);
                            if (rom_type == 1) SetCRAM_8K_Bank(0);
                            break;
                        case 0x5100:
                            reg[2] = data;
                            //						nes.apu.Write(0x4011,(decode(reg[0]&0xf)<<3));
                            break;
                        case 0x5200:
                            reg[0] = data;
                            SetPROM_32K_Bank((reg[1] & 0xF) | (reg[0] << 4));
                            break;
                        case 0x5300:
                            security = data;
                            break;
                    }
                }
            }
            else if (addr >= 0x6000)
            {
                CPU_MEM_BANK[addr >> 13][addr & 0x1FFF] = data;
                if ((addr >= 0x7900 && addr <= 0x79FF))
                {
                    //			WAVRAM[addr&0xFF] = data;
                    //			if(addr==0x79FF){
                    //				memcpy( YWRAM+(www*256), WAVRAM, 256);
                    //				www++;
                    //			}
                    //			nes.apu.Write(0x4011,(adpcm_decoder(data)));
                    nes.apu.Write(0x4011, data);
                }
            }
        }

        //void Mapper163::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            //	DEBUGOUT( "Write    - addr= %04x ; dat= %03x\n", addr, data );
        }

        //void Mapper163::HSync(int scanline)
        public override void HSync(int scanline)
        {
            if ((reg[1] & 0x80) != 0 && nes.ppu.IsDispON())
            {
                if (scanline == 127)
                {
                    SetCRAM_4K_Bank(0, 1);
                    SetCRAM_4K_Bank(4, 1);
                }
                if (rom_type == 1)
                {
                    if (scanline < 127)
                    {
                        SetCRAM_4K_Bank(0, 0);
                        SetCRAM_4K_Bank(4, 0);
                    }
                }
                else
                {
                    if (scanline == 239)
                    {
                        SetCRAM_4K_Bank(0, 0);
                        SetCRAM_4K_Bank(4, 0);
                        if (rom_type == 2) SetCRAM_4K_Bank(4, 1);
                    }
                }
            }
        }

        ///void Mapper163::PPU_Latch(WORD addr)
        //{
        //    if (DirectInput.m_Sw[DIK_PAUSE])
        //    {
        //        nes.Dump_YWRAM();
        //    }
        //}

        //void Mapper163::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            p[0] = reg[0];
            p[1] = reg[1];
        }

        //void Mapper163::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            reg[0] = p[0];
            reg[1] = p[1];
        }

    }
}
