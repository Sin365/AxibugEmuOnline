//////////////////////////////////////////////////////////////////////////
// Mapper164  Pocket Monster Gold                                       //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
//using Codice.CM.Client.Differences;
using VirtualNes.Core.Debug;

namespace VirtualNes.Core
{
    public class Mapper164 : Mapper
    {
        BYTE reg5000;
        BYTE reg5100;
        BYTE a3, p_mode;
        public Mapper164(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            reg5000 = 0;
            reg5100 = 0;
            SetBank_CPU();
            SetBank_PPU();
            nes.ppu.SetExtLatchMode(true);
        }

        //void Mapper164::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            if (addr == 0x5000)
            {
                p_mode = (byte)(data >> 7);
                reg5000 = data;
                SetBank_CPU();
                SetBank_PPU();
            }
            else if (addr == 0x5100)
            {
                reg5100 = data;
                SetBank_CPU();
                SetBank_PPU();
            }
            else if (addr >= 0x6000)
            {
                CPU_MEM_BANK[addr >> 13][addr & 0x1FFF] = data;
            }
            else
            {
                Debuger.Log($"write to {addr:X4}:{data:X2}");
            }

        }


        void SetBank_CPU()
        {
            int mode, @base, bank;

            @base = (reg5100 & 1) << 5;
            mode = (reg5000 >> 4) & 0x07;

            switch (mode)
            {
                case 0:
                case 2:
                case 4:
                case 6:             /* NORMAL MODE */
                    bank = (reg5000 & 0x0f);
                    bank += (reg5000 & 0x20) >> 1;
                    SetPROM_16K_Bank(4, bank + @base);
                    SetPROM_16K_Bank(6, @base + 0x1f);
                    Debuger.Log($"-- normal mode: mode={mode}, bank={bank} --");
                    break;
                case 1:
                case 3:             /* REG MODE */
                    Debuger.Log("-- reg mode --");
                    break;
                case 5:             /* 32K MODE */
                    bank = (reg5000 & 0x0f);
                    SetPROM_32K_Bank(bank + (@base >> 1));
                    //			DEBUGOUT("-- 32K MODE: bank=%02x --\n", bank);
                    break;
                case 7:             /* HALF MODE */
                    bank = (reg5000 & 0x0f);
                    bank += (bank & 0x08) << 1;
                    SetPROM_16K_Bank(4, bank + @base);
                    bank = (bank & 0x10) + 0x0f;
                    SetPROM_16K_Bank(6, @base + 0x1f);
                    Debuger.Log("-- half mode --");
                    break;
                default:
                    break;
            };

        }

        void SetBank_PPU()
        {
            SetCRAM_8K_Bank(0);
        }


        //void Mapper164::PPU_ExtLatchX(INT x)
        public override void PPU_ExtLatchX(int x)
        {
            a3 = (byte)((x & 1) << 3);
        }

        //void Mapper164::PPU_ExtLatch(WORD ntbladr, BYTE& chr_l, BYTE& chr_h, BYTE& attr )
        public override void PPU_ExtLatch(ushort ntbladr, ref byte chr_l, ref byte chr_h, ref byte attr)
        {
            INT loopy_v = nes.ppu.GetPPUADDR();
            INT loopy_y = nes.ppu.GetTILEY();
            INT tileofs = (PPUREG[0] & PPU.PPU_BGTBL_BIT) << 8;
            INT attradr = 0x23C0 + (loopy_v & 0x0C00) + ((loopy_v & 0x0380) >> 4);
            INT attrsft = (ntbladr & 0x0040) >> 4;
            Span<byte> pNTBL = PPU_MEM_BANK[ntbladr >> 10];

            INT ntbl_x = ntbladr & 0x001F;
            INT tileadr;

            attradr &= 0x3FF;
            attr = (byte)(((pNTBL[attradr + (ntbl_x >> 2)] >> ((ntbl_x & 2) + attrsft)) & 3) << 2);
            tileadr = tileofs + pNTBL[ntbladr & 0x03FF] * 0x10 + loopy_y;

            if (p_mode != 0)
            {
                tileadr = (tileadr & 0xfff7) | a3;
                chr_l = chr_h = PPU_MEM_BANK[tileadr >> 10][tileadr & 0x03FF];
            }
            else
            {
                chr_l = PPU_MEM_BANK[tileadr >> 10][tileadr & 0x03FF];
                chr_h = PPU_MEM_BANK[tileadr >> 10][(tileadr & 0x03FF) + 8];
            }

        }

        //void Mapper164::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            p[0] = reg5000;
            p[1] = reg5100;
            p[2] = a3;
            p[3] = p_mode;
        }

        //void Mapper164::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            reg5000 = p[0];
            reg5100 = p[1];
            a3 = p[2];
            p_mode = p[3];
        }

        public override bool IsStateSave()
        {
            return true;
        }
    }
}
