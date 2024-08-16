//////////////////////////////////////////////////////////////////////////
// Mapper069  SunSoft FME-7                                             //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.Core.CPU;
using static VirtualNes.MMU;
using BYTE = System.Byte;
using INT = System.Int32;


namespace VirtualNes.Core
{
    public class Mapper069 : Mapper
    {
        BYTE patch;

        BYTE reg;
        BYTE irq_enable;
        INT irq_counter;
        public Mapper069(NES parent) : base(parent)
        {
        }

        public override void Reset()

        {
            reg = 0;
            irq_enable = 0;
            irq_counter = 0;

            SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);

            if (VROM_1K_SIZE != 0)
            {
                SetVROM_8K_Bank(0);
            }

            nes.apu.SelectExSound(32);
            nes.SetIrqType(NES.IRQMETHOD.IRQ_CLOCK);
            patch = 0;

            uint crc = nes.rom.GetPROM_CRC();

            if (crc == 0xfeac6916)
            {   // Honoo no Toukyuuji - Dodge Danpei 2(J)
                //		nes.SetIrqType( NES::IRQ_HSYNC );
                nes.SetRenderMethod(EnumRenderMethod.TILE_RENDER);
            }

            if (crc == 0xad28aef6)
            {   // Dynamite Batman(J) / Dynamite Batman - Return of the Joker(U)
                patch = 1;
            }
        }

        //void Mapper069::Write(WORD addr, BYTE data)
        public override void Write(ushort addr, byte data)
        {
            switch (addr & 0xE000)
            {
                case 0x8000:
                    reg = data;
                    break;

                case 0xA000:
                    switch (reg & 0x0F)
                    {
                        case 0x00:
                        case 0x01:
                        case 0x02:
                        case 0x03:
                        case 0x04:
                        case 0x05:
                        case 0x06:
                        case 0x07:
                            SetVROM_1K_Bank((byte)(reg & 0x07), data);
                            break;
                        case 0x08:
                            if (patch == 0 && (data & 0x40) == 0)
                            {
                                SetPROM_8K_Bank(3, data);
                            }
                            break;
                        case 0x09:
                            SetPROM_8K_Bank(4, data);
                            break;
                        case 0x0A:
                            SetPROM_8K_Bank(5, data);
                            break;
                        case 0x0B:
                            SetPROM_8K_Bank(6, data);
                            break;

                        case 0x0C:
                            data &= 0x03;
                            if (data == 0) SetVRAM_Mirror(VRAM_VMIRROR);
                            else if (data == 1) SetVRAM_Mirror(VRAM_HMIRROR);
                            else if (data == 2) SetVRAM_Mirror(VRAM_MIRROR4L);
                            else SetVRAM_Mirror(VRAM_MIRROR4H);
                            break;

                        case 0x0D:
                            irq_enable = data;
                            nes.cpu.ClrIRQ(IRQ_MAPPER);
                            break;

                        case 0x0E:
                            irq_counter = (irq_counter & 0xFF00) | data;
                            nes.cpu.ClrIRQ(IRQ_MAPPER);
                            break;

                        case 0x0F:
                            irq_counter = (irq_counter & 0x00FF) | (data << 8);
                            nes.cpu.ClrIRQ(IRQ_MAPPER);
                            break;
                    }
                    break;

                case 0xC000:
                case 0xE000:
                    nes.apu.ExWrite(addr, data);
                    break;
            }
        }

        //void Mapper069::Clock(INT cycles)
        public override void Clock(int cycles)
        {
            //if (irq_enable && (nes.GetIrqType() == NES::IRQ_CLOCK))
            if (irq_enable != 0 && (nes.GetIrqType() == (int)NES.IRQMETHOD.IRQ_HSYNC))
            {
                irq_counter -= cycles;
                if (irq_counter <= 0)
                {
                    nes.cpu.SetIRQ(IRQ_MAPPER);
                    irq_enable = 0;
                    irq_counter = 0xFFFF;
                }
            }
        }

        //void Mapper069::HSync(INT scanline)
        public override void HSync(int scanline)
        {
            if (irq_enable != 0 && (nes.GetIrqType() == (int)NES.IRQMETHOD.IRQ_HSYNC))
            {
                irq_counter -= 114;
                if (irq_counter <= 0)
                {
                    nes.cpu.SetIRQ(IRQ_MAPPER);
                    irq_enable = 0;
                    irq_counter = 0xFFFF;
                }
            }
        }

        //void Mapper069::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            //p[0] = reg;
            //p[1] = irq_enable;
            //*(INT*)&p[2] = irq_counter;
        }

        //void Mapper069::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            //reg = p[0];
            //irq_enable = p[1];
            //irq_counter = *(INT*)&p[2];
        }
        public override bool IsStateSave()
        {
            return true;
        }
    }
}
