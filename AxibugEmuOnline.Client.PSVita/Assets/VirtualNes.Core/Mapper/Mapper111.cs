//////////////////////////////////////////////////////////////////////////
// Mapper111  Nintendo MMC1 Hack                                        //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using System;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
    public class Mapper111 : Mapper
    {

        ushort last_addr;

        BYTE patch;
        BYTE wram_patch;
        BYTE wram_bank;
        BYTE wram_count;

        BYTE[] reg = new byte[4];
        BYTE shift, regbuf;

        public Mapper111(NES parent) : base(parent)
        {
        }

        public override void Reset()
        {
            reg[0] = 0x0C;      // D3=1,D2=1
            reg[1] = reg[2] = reg[3] = 0;
            shift = regbuf = 0;

            patch = 0;
            wram_patch = 0;

            if (PROM_16K_SIZE < 32)
            {
                SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);
            }
            else
            {
                // For 512K/1M byte Cartridge
                SetPROM_16K_Bank(4, 0);
                SetPROM_16K_Bank(6, 16 - 1);

                patch = 1;
            }

            if (VROM_8K_SIZE != 0)
            {
                SetVROM_8K_Bank(0);
            }

            // Ninja Ryukenden(J)
            nes.SetRenderMethod(EnumRenderMethod.POST_ALL_RENDER);
        }

        //void Mapper111::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            //	DEBUGOUT( "MMC1 %04X=%02X\n", addr&0xFFFF,data&0xFF );

            if ((data & 0x80) != 0)
            {
                shift = regbuf = 0;
                reg[0] |= 0x0C;     // D3=1,D2=1 残りはリセットされない
                return;
            }

            addr = (ushort)((addr & 0x7FFF) >> 13);
            reg[addr] = data;

            //	DEBUGOUT( "MMC1 %d=%02X\n", addr&0xFFFF,regbuf&0xFF );

            if (patch != 1)
            {
                // For Normal Cartridge
                switch (addr)
                {
                    case 0:
                        if ((reg[0] & 0x02) != 0)
                        {
                            if ((reg[0] & 0x01) != 0) SetVRAM_Mirror(VRAM_HMIRROR);
                            else SetVRAM_Mirror(VRAM_VMIRROR);
                        }
                        else
                        {
                            if ((reg[0] & 0x01) != 0) SetVRAM_Mirror(VRAM_MIRROR4H);
                            else SetVRAM_Mirror(VRAM_MIRROR4L);
                        }
                        break;
                    case 1:
                        // Register #1
                        if (VROM_1K_SIZE != 0)
                        {
                            if ((reg[0] & 0x10) != 0)
                            {
                                // CHR 4K bank lower($0000-$0FFF)
                                SetVROM_4K_Bank(0, reg[1]);
                                // CHR 4K bank higher($1000-$1FFF)
                                SetVROM_4K_Bank(4, reg[2]);
                            }
                            else
                            {
                                // CHR 8K bank($0000-$1FFF)
                                SetVROM_8K_Bank(reg[1] >> 1);
                            }
                        }
                        else
                        {
                            // For Romancia
                            if ((reg[0] & 0x10) != 0)
                            {
                                SetCRAM_4K_Bank(0, reg[1]);
                            }
                        }
                        break;
                    case 2:
                        // Register #2
                        if (VROM_1K_SIZE != 0)
                        {
                            if ((reg[0] & 0x10) != 0)
                            {
                                // CHR 4K bank lower($0000-$0FFF)
                                SetVROM_4K_Bank(0, reg[1]);
                                // CHR 4K bank higher($1000-$1FFF)
                                SetVROM_4K_Bank(4, reg[2]);
                            }
                            else
                            {
                                // CHR 8K bank($0000-$1FFF)
                                SetVROM_8K_Bank(reg[1] >> 1);
                            }
                        }
                        else
                        {
                            // For Romancia
                            if ((reg[0] & 0x10) != 0)
                            {
                                SetCRAM_4K_Bank(4, reg[2]);
                            }
                        }
                        break;
                    case 3:
                        if ((reg[0] & 0x08) == 0)
                        {
                            // PRG 32K bank ($8000-$FFFF)
                            SetPROM_32K_Bank(reg[3] >> 1);
                        }
                        else
                        {
                            if ((reg[0] & 0x04) != 0)
                            {
                                // PRG 16K bank ($8000-$BFFF)
                                SetPROM_16K_Bank(4, reg[3]);
                                SetPROM_16K_Bank(6, PROM_16K_SIZE - 1);
                            }
                            else
                            {
                                // PRG 16K bank ($C000-$FFFF)
                                SetPROM_16K_Bank(6, reg[3]);
                                SetPROM_16K_Bank(4, 0);
                            }
                        }
                        break;
                }
            }
            else
            {
                // For 512K/1M byte Cartridge
                INT PROM_BASE = 0;
                if (PROM_16K_SIZE >= 32)
                {
                    PROM_BASE = reg[1] & 0x10;
                }

                // Register #0
                if (addr == 0)
                {
                    if ((reg[0] & 0x02) != 0)
                    {
                        if ((reg[0] & 0x01) != 0) SetVRAM_Mirror(VRAM_HMIRROR);
                        else SetVRAM_Mirror(VRAM_VMIRROR);
                    }
                    else
                    {
                        if ((reg[0] & 0x01) != 0) SetVRAM_Mirror(VRAM_MIRROR4H);
                        else SetVRAM_Mirror(VRAM_MIRROR4L);
                    }
                }
                // Register #1
                if (VROM_1K_SIZE != 0)
                {
                    if ((reg[0] & 0x10) != 0)
                    {
                        // CHR 4K bank lower($0000-$0FFF)
                        SetVROM_4K_Bank(0, reg[1]);
                    }
                    else
                    {
                        // CHR 8K bank($0000-$1FFF)
                        SetVROM_8K_Bank(reg[1] >> 1);
                    }
                }
                else
                {
                    // For Romancia
                    if ((reg[0] & 0x10) != 0)
                    {
                        SetCRAM_4K_Bank(0, reg[1]);
                    }
                }
                // Register #2
                if (VROM_1K_SIZE != 0)
                {
                    if ((reg[0] & 0x10) != 0)
                    {
                        // CHR 4K bank higher($1000-$1FFF)
                        SetVROM_4K_Bank(4, reg[2]);
                    }
                }
                else
                {
                    // For Romancia
                    if ((reg[0] & 0x10) != 0)
                    {
                        SetCRAM_4K_Bank(4, reg[2]);
                    }
                }
                // Register #3
                if ((reg[0] & 0x08) == 0)
                {
                    // PRG 32K bank ($8000-$FFFF)
                    SetPROM_32K_Bank((reg[3] & (0xF + PROM_BASE)) >> 1);
                }
                else
                {
                    if ((reg[0] & 0x04) != 0)
                    {
                        // PRG 16K bank ($8000-$BFFF)
                        SetPROM_16K_Bank(4, PROM_BASE + (reg[3] & 0x0F));
                        if (PROM_16K_SIZE >= 32) SetPROM_16K_Bank(6, PROM_BASE + 16 - 1);
                    }
                    else
                    {
                        // PRG 16K bank ($C000-$FFFF)
                        SetPROM_16K_Bank(6, PROM_BASE + (reg[3] & 0x0F));
                        if (PROM_16K_SIZE >= 32) SetPROM_16K_Bank(4, PROM_BASE);
                    }
                }
            }
        }

        //void Mapper111::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            p[0] = reg[0];
            p[1] = reg[1];
            p[2] = reg[2];
            p[3] = reg[3];
            p[4] = shift;
            p[5] = regbuf;

            p[6] = wram_bank;
            p[7] = wram_count;
        }

        //void Mapper111::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            reg[0] = p[0];
            reg[1] = p[1];
            reg[2] = p[2];
            reg[3] = p[3];
            shift = p[4];
            regbuf = p[5];

            wram_bank = p[6];
            wram_count = p[7];
        }


        public override bool IsStateSave()
        {
            return true;
        }
    }
}
