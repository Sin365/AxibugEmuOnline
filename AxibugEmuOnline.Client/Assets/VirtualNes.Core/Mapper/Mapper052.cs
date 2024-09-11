//////////////////////////////////////////////////////////////////////////
// Mapper052  Konami VRC2 type B                                        //
//////////////////////////////////////////////////////////////////////////
using VirtualNes.Core.Debug;
using static VirtualNes.MMU;
using BYTE = System.Byte;
using INT = System.Int32;

namespace VirtualNes.Core
{
    public class Mapper052 : Mapper
    {

        BYTE[] reg = new byte[9];
        BYTE irq_enable;
        BYTE irq_counter;
        BYTE irq_latch;
        INT irq_clock;
        public Mapper052(NES parent) : base(parent) { }

        public override void Reset()
        {
            for (byte i = 0; i < 8; i++)
            {
                reg[i] = i;
            }
            reg[8] = 0;

            irq_enable = 0;
            irq_counter = 0;
            irq_latch = 0;
            irq_clock = 0;

            SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);
            SetVROM_8K_Bank(0);

            nes.SetRenderMethod(EnumRenderMethod.PRE_ALL_RENDER);
            //	nes->SetRenderMethod( NES::POST_RENDER );
            //	nes->SetRenderMethod( NES::POST_ALL_RENDER );

        }

        public override void Write(ushort addr, byte data)
        {
            if (addr >= 0xf000) Debuger.Log($"MPRWR A={addr & 0xFFFF}   D={data & 0xFF} L={nes.GetScanline()} CYC={nes.cpu.GetTotalCycles()}\n");
            if (addr >= 0xf000) Debuger.Log($"MPRWR A={addr & 0xFFFF} RAM={RAM[0x1c0] & 0xFF} L={nes.GetScanline()} CYC={nes.cpu.GetTotalCycles()}\n");
            switch (addr & 0xFFFF)
            {
                case 0x8000:
                    if (reg[8] != 0) SetPROM_8K_Bank(6, data);
                    else SetPROM_8K_Bank(4, data);
                    break;
                case 0x9002:
                    reg[8] = (byte)(data & 0x02);
                    break;

                case 0x9004:
                    data &= 0x03;
                    if (data == 0) SetVRAM_Mirror(VRAM_VMIRROR);
                    else if (data == 1) SetVRAM_Mirror(VRAM_HMIRROR);
                    else if (data == 2) SetVRAM_Mirror(VRAM_MIRROR4L);
                    else SetVRAM_Mirror(VRAM_MIRROR4H);
                    break;

                case 0xA000:
                    SetPROM_8K_Bank(5, data);
                    break;

                case 0xB000:
                    reg[0] = (byte)((reg[0] & 0xF0) | (data & 0x0F));
                    SetVROM_1K_Bank(0, reg[0]);
                    break;
                case 0xB001:
                    reg[0] = (byte)((reg[0] & 0x0F) | ((data & 0x0F) << 4));
                    SetVROM_1K_Bank(0, reg[0]);
                    break;
                case 0xB002:
                    reg[1] = (byte)((reg[1] & 0xF0) | (data & 0x0F));
                    SetVROM_1K_Bank(1, reg[1]);
                    break;
                case 0xB003:
                    reg[1] = (byte)((reg[1] & 0x0F) | ((data & 0x0F) << 4));
                    SetVROM_1K_Bank(1, reg[1]);
                    break;

                case 0xC000:
                    reg[2] = (byte)((reg[2] & 0xF0) | (data & 0x0F));
                    SetVROM_1K_Bank(2, reg[2]);
                    break;
                case 0xC001:
                    reg[2] = (byte)((reg[2] & 0x0F) | ((data & 0x0F) << 4));
                    SetVROM_1K_Bank(2, reg[2]);
                    break;
                case 0xC002:
                    reg[3] = (byte)((reg[3] & 0xF0) | (data & 0x0F));
                    SetVROM_1K_Bank(3, reg[3]);
                    break;
                case 0xC003:
                    reg[3] = (byte)((reg[3] & 0x0F) | ((data & 0x0F) << 4));
                    SetVROM_1K_Bank(3, reg[3]);
                    break;

                case 0xD000:
                    reg[4] = (byte)((reg[4] & 0xF0) | (data & 0x0F));
                    SetVROM_1K_Bank(4, reg[4]);
                    break;
                case 0xD001:
                    reg[4] = (byte)((reg[4] & 0x0F) | ((data & 0x0F) << 4));
                    SetVROM_1K_Bank(4, reg[4]);
                    break;
                case 0xD002:
                    reg[5] = (byte)((reg[5] & 0xF0) | (data & 0x0F));
                    SetVROM_1K_Bank(5, reg[5]);
                    break;
                case 0xD003:
                    reg[5] = (byte)((reg[5] & 0x0F) | ((data & 0x0F) << 4));
                    SetVROM_1K_Bank(5, reg[5]);
                    break;

                case 0xE000:
                    reg[6] = (byte)((reg[6] & 0xF0) | (data & 0x0F));
                    SetVROM_1K_Bank(6, reg[6]);
                    break;
                case 0xE001:
                    reg[6] = (byte)((reg[6] & 0x0F) | ((data & 0x0F) << 4));
                    SetVROM_1K_Bank(6, reg[6]);
                    break;
                case 0xE002:
                    reg[7] = (byte)((reg[7] & 0xF0) | (data & 0x0F));
                    SetVROM_1K_Bank(7, reg[7]);
                    break;
                case 0xE003:
                    reg[7] = (byte)((reg[7] & 0x0F) | ((data & 0x0F) << 4));
                    SetVROM_1K_Bank(7, reg[7]);
                    break;

                case 0xF000:
                    break;
                case 0xF004:
                case 0xFF04:
                    RAM[0x7f8] = 1;
                    break;
                case 0xF008:
                case 0xFF08:
                    irq_enable = 1;
                    //			irq_latch = ((RAM[0x7f8]*2)+0x11)^0xFF;	//Akumajou Special - Boku Dracula-kun
                    irq_latch = (byte)(((RAM[0x1c0] * 2) + 0x11) ^ 0xFF);   //Teenage Mutant Ninja Turtles
                    irq_counter = irq_latch;
                    irq_clock = 0;
                    nes.cpu.ClrIRQ(CPU.IRQ_MAPPER);
                    break;
                case 0xF00C:
                    irq_enable = 0;
                    nes.cpu.ClrIRQ(CPU.IRQ_MAPPER);
                    break;
            }
        }

        public override void HSync(int scanline)
        {
            //
        }

        public override void Clock(int cycles)
        {
            if (irq_enable != null)
            {
                irq_clock += cycles * 3;
                while (irq_clock >= 341)
                {
                    irq_clock -= 341;
                    irq_counter++;
                    if (irq_counter == 0)
                    {
                        irq_counter = irq_latch;
                        nes.cpu.SetIRQ(CPU.IRQ_MAPPER);
                    }
                }
            }
        }

        public override void SaveState(byte[] p)
        {
            //
        }

        public override void LoadState(byte[] p)
        {
            //
        }


        public override bool IsStateSave()
        {
            return true;
        }
    }
}
