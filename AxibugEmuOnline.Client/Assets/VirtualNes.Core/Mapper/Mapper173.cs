//////////////////////////////////////////////////////////////////////////
// Mapper173          Subor                                             //
//////////////////////////////////////////////////////////////////////////
using VirtualNes.Core.Debug;
using static VirtualNes.MMU;
using BYTE = System.Byte;
using INT = System.Int32;
namespace VirtualNes.Core
{
    public class Mapper173 : Mapper
    {

        BYTE[] reg = new BYTE[10];

        INT irq_counter, irq_latch;
        BYTE irq_enable, irq_repeat;
        BYTE irq_occur;
        public Mapper173(NES parent) : base(parent) { }

        public override bool IsStateSave()
        {
            return true;
        }
        public override void Reset()
        {
            //	nes.ppu.SetExtLatchMode( TRUE );
            for (INT i = 0; i < 11; i++) reg[i] = 0x00;

            irq_enable = irq_repeat = 0;
            irq_counter = irq_latch = 0;
            irq_occur = 0;

            SetPROM_32K_Bank(0);
            nes.SetVideoMode(2 != 0);
        }

        public override byte ExRead(ushort addr)
        {
            Debuger.Log($"ExRead   - addr= {addr}\n");

            return 0x00;

            switch (addr)
            {
                case 0x4026:
                    //
                    break;
                case 0x4033:
                    //D7: 
                    //D6: 
                    //D5: 
                    //D4: 
                    //D3: 
                    //D2: 
                    //D1: 
                    //D0: 
                    //
                    break;
                case 0x4204:    //FDC主状态寄存器(STATUS)
                                //
                    break;
                case 0x4205:    //FDC数据寄存器(DATA)(读???)
                                //
                    break;
            }
        }

        public override void ExWrite(ushort addr, byte data)
        {
            Debuger.Log($"ExWrite  - addr= {addr} ; dat= {data}\n");
            switch (addr)
            {
                case 0x4020:
                    reg[0] = data;
                    break;
                case 0x4022:
                    reg[1] = data;
                    break;
                case 0x4023:
                    reg[2] = data;
                    break;
                case 0x4026:
                    reg[3] = data;
                    break;
                case 0x4031:
                    reg[4] = data;
                    break;
                case 0x4032:
                    reg[5] = data;

                    irq_repeat = (byte)(data & 0x01);
                    irq_enable = (byte)(data & 0x02);
                    irq_occur = 0;
                    if (irq_enable!= null)
                    {
                        irq_counter = irq_latch;
                    }
                    else
                    {
                        nes.cpu.ClrIRQ(CPU.IRQ_MAPPER);
                    }
                    break;
                case 0x4034:
                    reg[6] = data;

                    irq_latch = (irq_latch & 0xFF00) | data;
                    break;
                case 0x4035:
                    reg[7] = data;

                    irq_latch = (irq_latch & 0x00FF) | ((ushort)data << 8);
                    break;
                case 0x4040:
                    SetPROM_4K_Bank(0x8000, data & 0x7F);
                    break;
                case 0x4041:
                    SetPROM_4K_Bank(0x9000, data & 0x7F);
                    break;
                case 0x4042:
                    SetPROM_4K_Bank(0xa000, data & 0x7F);
                    break;
                case 0x4043:
                    SetPROM_4K_Bank(0xb000, data & 0x7F);
                    break;
                case 0x4044:
                    SetPROM_4K_Bank(0xc000, data & 0x7F);
                    break;
                case 0x4045:
                    SetPROM_4K_Bank(0xd000, data & 0x7F);
                    break;
                case 0x4046:
                    SetPROM_4K_Bank(0xe000, data & 0x7F);
                    break;
                case 0x4047:
                    SetPROM_4K_Bank(0xf000, data & 0x7F);
                    break;

                case 0x4205:    //FDC数据寄存器(DATA)(写???)
                                //
                    break;
            }
        }

        public override byte ReadLow(ushort addr)
        {
            //	DEBUGOUT( "ReadLow  - addr= %04x\n", addr );

            return CPU_MEM_BANK[addr >> 13][addr & 0x1FFF];
        }

        public override void WriteLow(ushort addr, byte data)
        {
            //	DEBUGOUT( "WriteLow - addr= %04x ; dat= %03x\n", addr, data );

            CPU_MEM_BANK[addr >> 13][addr & 0x1FFF] = data;
        }

        public override void Write(ushort addr, byte data)
        {
            //	DEBUGOUT( "Write    - addr= %04x ; dat= %03x\n", addr, data );
        }


        public override void HSync(int scanline)
        {
        //	if( (scanline >= 0 && scanline <= 239) ) {
        //		if( nes.ppu.IsDispON() ) {
        //			if( irq_enable ) {
        //				irq_enable = 0;
        ///				nes.cpu.SetIRQ( IRQ_MAPPER );
        //			}
        //		}
        //	}
    }

        public override void Clock(int cycles)
        {

            if (irq_enable!= 0)
            {
                irq_counter -= cycles;
                if (irq_counter <= 0)
                {
                    ////			irq_counter &= 0xFFFF;
                    irq_counter += irq_latch;

                    if (irq_occur == 0)
                    {
                        irq_occur = 0xFF;
                        if (irq_repeat == 0)
                        {
                            irq_enable = 0;
                        }
                        nes.cpu.SetIRQ(CPU.IRQ_MAPPER);
                    }
                }
            }
        }

        public override void PPU_Latch(ushort addr)
        {
            //
        }

        public override void PPU_ExtLatch(ushort ntbladr, ref byte chr_l, ref byte chr_h, ref byte attr)
        {
            INT loopy_v = nes.ppu.GetPPUADDR();
            INT loopy_y = nes.ppu.GetTILEY();
            INT tileofs = (PPUREG[0] & PPU.PPU_BGTBL_BIT) << 8;
            INT attradr = 0x23C0 + (loopy_v & 0x0C00) + ((loopy_v & 0x0380) >> 4);
            INT attrsft = (ntbladr & 0x0040) >> 4;
            ArrayRef<byte> pNTBL = PPU_MEM_BANK[ntbladr >> 10];
            INT ntbl_x = ntbladr & 0x001F;
            INT tileadr, ntb;

            ntb = (ntbladr >> 10) & 3;

            if (ntb == 2)
                tileofs |= 0x1000;
            //	else if(ntb && PPU_SW)
            tileofs |= 0x1000;
            //	else
            tileofs |= 0x0000;

            attradr &= 0x3FF;
            attr = (byte)(((pNTBL[attradr + (ntbl_x >> 2)] >> ((ntbl_x & 2) + attrsft)) & 3) << 2);
            tileadr = tileofs + pNTBL[ntbladr & 0x03FF] * 0x10 + loopy_y;

            chr_l = PPU_MEM_BANK[tileadr >> 10][tileadr & 0x03FF];
            chr_h = PPU_MEM_BANK[tileadr >> 10][(tileadr & 0x03FF) + 8];
        }

        public override void SaveState(byte[] p)
        {
            //
        }

        public override void LoadState(byte[] p)
        {
            //
        }
    }
}
