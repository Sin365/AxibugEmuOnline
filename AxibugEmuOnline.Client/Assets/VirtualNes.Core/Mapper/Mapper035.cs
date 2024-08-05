//////////////////////////////////////////////////////////////////////////
// Mapper035                                                            //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper035 : Mapper
	{

        BYTE[] reg = new byte[8];
        BYTE[] chr = new byte[8];
        ushort IRQCount, IRQa;
        public Mapper035(NES parent) : base(parent)
		{
		}


		public override void Reset()
        {
            for (int i = 0; i < 8; i++)
                reg[i] = chr[i] = 0;

            IRQCount = IRQa = 0;

            //SetPROM_32K_Bank( 0, 1, PROM_8K_SIZE-2, PROM_8K_SIZE-1 );

            Sync();
            //setprg8r(0x10,0x6000,0);
            SetPROM_8K_Bank(7, PROM_8K_SIZE - 1);
        }

        void Sync()
        {
            int i;
            SetPROM_8K_Bank(4, reg[0]);
            SetPROM_8K_Bank(5, reg[1]);
            SetPROM_8K_Bank(6, reg[2]);
            for (i = 0; i < 8; i++)
                SetVROM_1K_Bank((byte)i, chr[i]);
            SetVRAM_Mirror(reg[3] ^ 1);
        }

        //void Mapper035::WriteLow(WORD addr, BYTE data)
        public override void WriteLow(ushort addr, byte data)
        {
            if (addr >= 0x6000 && addr <= 0x7FFF)
            {
                XRAM[addr - 0x6000] = data;
            }
            else
            {
                base.WriteLow(addr, data);
            }
        }
        //BYTE Mapper035::ReadLow(WORD addr)
        public override byte ReadLow(ushort addr)
        {
            if (addr >= 0x6000 && addr <= 0x7FFF)
            {
                return XRAM[addr - 0x6000];
            }
            else
            {
                return base.ReadLow(addr);
            }
        }

        //void Mapper035::Write(WORD A, BYTE V)
        public override void Write(ushort A, byte V)
        {
            switch (A)
            {
                case 0x8000: reg[0] = V; break;
                case 0x8001: reg[1] = V; break;
                case 0x8002: reg[2] = V; break;
                case 0x9000: chr[0] = V; break;
                case 0x9001: chr[1] = V; break;
                case 0x9002: chr[2] = V; break;
                case 0x9003: chr[3] = V; break;
                case 0x9004: chr[4] = V; break;
                case 0x9005: chr[5] = V; break;
                case 0x9006: chr[6] = V; break;
                case 0x9007: chr[7] = V; break;
                case 0xC002:
                    IRQa = 0;
                    nes.cpu.ClrIRQ(IRQ_MAPPER); break;
                case 0xC005: IRQCount = V; break;
                case 0xC003: IRQa = 1; break;
                case 0xD001: reg[3] = V; break;
            }
            Sync();
        }

        //void Mapper035::HSync(INT scanline)
        public override void HSync(int scanline)
        {
            if ((scanline >= 0 && scanline <= 239))
            {
                if (nes.ppu.IsDispON())
                {
                    if (IRQa!=0)
                    {
                        IRQCount--;
                        if (IRQCount == 0)
                        {
                            nes.cpu.SetIRQ(IRQ_MAPPER);
                            IRQa = 0;
                        }
                    }
                }
            }
        }

        public override bool IsStateSave()
        {
			 return true;
        }
    }
}
