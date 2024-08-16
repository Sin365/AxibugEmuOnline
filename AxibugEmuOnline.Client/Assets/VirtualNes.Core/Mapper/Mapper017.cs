using static VirtualNes.Core.CPU;
using static VirtualNes.MMU;
using BYTE = System.Byte;
using INT = System.Int32;


namespace VirtualNes.Core
{
    public class Mapper017 : Mapper
    {
        BYTE irq_enable;
        INT irq_counter;
        INT irq_latch;
        public Mapper017(NES parent) : base(parent)
        {
        }


        public override void Reset()
        {
            SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);

            if (VROM_1K_SIZE != 0)
            {
                SetVROM_8K_Bank(0);
            }

            irq_enable = 0;
            irq_counter = 0;
            irq_latch = 0;
        }

        //void Mapper017::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            switch (addr)
            {
                case 0x42FE:
                    if ((data & 0x10) != 0) SetVRAM_Mirror(VRAM_MIRROR4H);
                    else SetVRAM_Mirror(VRAM_MIRROR4L);
                    break;
                case 0x42FF:
                    if ((data & 0x10) != 0) SetVRAM_Mirror(VRAM_HMIRROR);
                    else SetVRAM_Mirror(VRAM_VMIRROR);
                    break;

                case 0x4501:
                    irq_enable = 0;
                    nes.cpu.ClrIRQ(IRQ_MAPPER);
                    break;
                case 0x4502:
                    irq_latch = (irq_latch & 0xFF00) | data;
                    break;
                case 0x4503:
                    irq_latch = (irq_latch & 0x00FF) | ((INT)data << 8);
                    irq_counter = irq_latch;
                    irq_enable = 0xFF;
                    break;

                case 0x4504:
                case 0x4505:
                case 0x4506:
                case 0x4507:
                    SetPROM_8K_Bank((byte)(addr & 0x07), data);
                    break;

                case 0x4510:
                case 0x4511:
                case 0x4512:
                case 0x4513:
                case 0x4514:
                case 0x4515:
                case 0x4516:
                case 0x4517:
                    SetVROM_1K_Bank((byte)(addr & 0x07), data);
                    break;

                default:
                    base.WriteLow(addr, data);
                    break;
            }
        }

        //void Mapper017::HSync(INT scanline)
        public override void HSync(int scanline)
        {
            if (irq_enable != 0)
            {
                if (irq_counter >= 0xFFFF - 113)
                {
                    nes.cpu.SetIRQ(IRQ_MAPPER);
                    //			nes.cpu.IRQ();
                    //			irq_counter = 0;
                    //			irq_enable = 0;
                    irq_counter &= 0xFFFF;
                }
                else
                {
                    irq_counter += 113;
                }
            }
        }

        //void Mapper017::SaveState(LPBYTE p)
        public override void SaveState(byte[] p)
        {
            //p[0] = irq_enable;
            //*(INT*)&p[1] = irq_counter;
            //*(INT*)&p[5] = irq_latch;
        }

        //void Mapper017::LoadState(LPBYTE p)
        public override void LoadState(byte[] p)
        {
            //irq_enable = p[0];
            //irq_counter = *(INT*)&p[1];
            //irq_latch = *(INT*)&p[5];
        }


        public override bool IsStateSave()
        {
            return true;
        }

    }
}
