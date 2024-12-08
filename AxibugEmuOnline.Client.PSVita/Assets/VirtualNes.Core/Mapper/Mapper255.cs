//using Codice.CM.Client.Differences;
using System;

namespace VirtualNes.Core
{
    public class Mapper255 : Mapper
    {
        byte[] reg = new byte[4];
        public Mapper255(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            MMU.SetPROM_32K_Bank(0);
            MMU.SetVROM_8K_Bank(0);
            MMU.SetVRAM_Mirror(MMU.VRAM_VMIRROR);

            reg[0] = 0;
            reg[1] = 0;
            reg[2] = 0;
            reg[3] = 0;
        }

        //BYTE Mapper255::ReadLow(WORD addr, BYTE data)
        //{
        //    if (addr >= 0x5800)
        //    {
        //        return reg[addr & 0x0003] & 0x0F;
        //    }
        //    else
        //    {
        //        return addr >> 8;
        //    }
        //}
        public override byte ReadLow(ushort addr)
        {
            if (addr >= 0x5800)
            {
                return (byte)(reg[addr & 0x0003] & 0x0F);
            }
            else
            {
                return (byte)(addr >> 8);
            }
        }


        public override void WriteLow(ushort addr, byte data)
        {
            if (addr >= 0x5800)
            {
                reg[addr & 0x0003] = (byte)(data & 0x0F);
            }
        }
        public override void Write(ushort addr, byte data)
        {
            byte prg = (byte)((addr & 0x0F80) >> 7);
            int chr =  (byte)((addr & 0x003F));
            int bank = (byte)((addr & 0x4000) >> 14);

            if ((addr & 0x2000) != 0 )
            {
                MMU.SetVRAM_Mirror(MMU.VRAM_HMIRROR);
            }
            else
            {
                MMU.SetVRAM_Mirror(MMU.VRAM_VMIRROR);
            }

            if ((addr & 0x1000) !=0)
            {
                if ((addr & 0x0040) != 0)
                {
                    MMU.SetPROM_8K_Bank(4, 0x80 * bank + prg * 4 + 2);
                    MMU.SetPROM_8K_Bank(5, 0x80 * bank + prg * 4 + 3);
                    MMU.SetPROM_8K_Bank(6, 0x80 * bank + prg * 4 + 2);
                    MMU.SetPROM_8K_Bank(7, 0x80 * bank + prg * 4 + 3);
                }
                else
                {
                    MMU.SetPROM_8K_Bank(4, 0x80 * bank + prg * 4 + 0);
                    MMU.SetPROM_8K_Bank(5, 0x80 * bank + prg * 4 + 1);
                    MMU.SetPROM_8K_Bank(6, 0x80 * bank + prg * 4 + 0);
                    MMU.SetPROM_8K_Bank(7, 0x80 * bank + prg * 4 + 1);
                }
            }
            else
            {
                MMU.SetPROM_8K_Bank(4, 0x80 * bank + prg * 4 + 0);
                MMU.SetPROM_8K_Bank(5, 0x80 * bank + prg * 4 + 1);
                MMU.SetPROM_8K_Bank(6, 0x80 * bank + prg * 4 + 2);
                MMU.SetPROM_8K_Bank(7, 0x80 * bank + prg * 4 + 3);
            }

            MMU.SetVROM_1K_Bank(0, 0x200 * bank + chr * 8 + 0);
            MMU.SetVROM_1K_Bank(1, 0x200 * bank + chr * 8 + 1);
            MMU.SetVROM_1K_Bank(2, 0x200 * bank + chr * 8 + 2);
            MMU.SetVROM_1K_Bank(3, 0x200 * bank + chr * 8 + 3);
            MMU.SetVROM_1K_Bank(4, 0x200 * bank + chr * 8 + 4);
            MMU.SetVROM_1K_Bank(5, 0x200 * bank + chr * 8 + 5);
            MMU.SetVROM_1K_Bank(6, 0x200 * bank + chr * 8 + 6);
            MMU.SetVROM_1K_Bank(7, 0x200 * bank + chr * 8 + 7);
        }

        // For state save
        public override bool IsStateSave()
        {
            return true;
        }

        
        public override void SaveState(byte[] p)
        {
            p[0] = reg[0];
            p[1] = reg[1];
            p[2] = reg[2];
            p[3] = reg[3];
        }

        public override void LoadState(byte[] p)
        {
            reg[0] = p[0];
            reg[1] = p[1];
            reg[2] = p[2];
            reg[3] = p[3];
        }
    }
}
