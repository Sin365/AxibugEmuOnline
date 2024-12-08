//////////////////////////////////////////////////////////////////////////
// Mapper023  Konami VRC2 type B                                        //
//////////////////////////////////////////////////////////////////////////
using static VirtualNes.MMU;
using static VirtualNes.Core.CPU;
using INT = System.Int32;
using BYTE = System.Byte;
//using Codice.CM.Client.Differences;

namespace VirtualNes.Core
{
	public class Mapper023 : Mapper
	{

		ushort addrmask;

		BYTE[] reg = new byte[9];
		BYTE irq_enable;
		BYTE irq_counter;
		BYTE irq_latch;
		INT irq_clock;
		public Mapper023(NES parent) : base(parent)
		{
		}


		public override void Reset()
		{
			addrmask = 0xFFFF;

			for (byte i = 0; i < 8; i++)
			{
				reg[i] = i;
			}
			reg[8] = 0;

			irq_enable = 0;
			irq_counter = 0;
			irq_latch = 0;
			irq_clock = 0;

			reg[9] = 1;

			SetPROM_32K_Bank(0, 1, PROM_8K_SIZE - 2, PROM_8K_SIZE - 1);
			SetVROM_8K_Bank(0);

			//	nes.SetRenderMethod( NES::POST_RENDER );

			uint crc = nes.rom.GetPROM_CRC();

			if (crc == 0x93794634       // Akumajou Special Boku Dracula Kun(J)
			 || crc == 0xc7829dae       // Akumajou Special Boku Dracula Kun(T-Eng)
			 || crc == 0xf82dc02f)
			{   // Akumajou Special Boku Dracula Kun(T-Eng v1.02)
				addrmask = 0xF00C;
				nes.SetRenderMethod(EnumRenderMethod.POST_ALL_RENDER);
			}
			if (crc == 0xdd53c4ae)
			{   // Tiny Toon Adventures(J)
				nes.SetRenderMethod(EnumRenderMethod.POST_ALL_RENDER);
			}
		}

		//void Mapper023::Write(WORD addr, BYTE data)
		public override void Write(ushort addr, byte data)
		{
			//DEBUGOUT( "MPRWR A=%04X D=%02X L=%3d CYC=%d\n", addr&0xFFFF, data&0xFF, nes.GetScanline(), nes.cpu.GetTotalCycles() );
			switch (addr & addrmask)
			{
				case 0x8000:
				case 0x8004:
				case 0x8008:
				case 0x800C:
					if (reg[8] != 0)
					{
						SetPROM_8K_Bank(6, data);
					}
					else
					{
						SetPROM_8K_Bank(4, data);
					}
					break;

				case 0x9000:
					if (data != 0xFF)
					{
						data &= 0x03;
						if (data == 0) SetVRAM_Mirror(VRAM_VMIRROR);
						else if (data == 1) SetVRAM_Mirror(VRAM_HMIRROR);
						else if (data == 2) SetVRAM_Mirror(VRAM_MIRROR4L);
						else SetVRAM_Mirror(VRAM_MIRROR4H);
					}
					break;

				case 0x9008:
					reg[8] = (byte)(data & 0x02);
					break;

				case 0xA000:
				case 0xA004:
				case 0xA008:
				case 0xA00C:
					SetPROM_8K_Bank(5, data);
					break;

				case 0xB000:
					reg[0] = (byte)((reg[0] & 0xF0) | (data & 0x0F));
					SetVROM_1K_Bank(0, reg[0]);
					break;
				case 0xB001:
				case 0xB004:
					reg[0] = ((byte)((reg[0] & 0x0F) | ((data & 0x0F) << 4)));
					SetVROM_1K_Bank(0, reg[0]);
					break;

				case 0xB002:
				case 0xB008:
					reg[1] = (byte)((reg[1] & 0xF0) | (data & 0x0F));
					SetVROM_1K_Bank(1, reg[1]);
					break;

				case 0xB003:
				case 0xB00C:
					reg[1] = (byte)((reg[1] & 0x0F) | ((data & 0x0F) << 4));
					SetVROM_1K_Bank(1, reg[1]);
					break;

				case 0xC000:
					reg[2] = (byte)((reg[2] & 0xF0) | (data & 0x0F));
					SetVROM_1K_Bank(2, reg[2]);
					break;

				case 0xC001:
				case 0xC004:
					reg[2] = (byte)((reg[2] & 0x0F) | ((data & 0x0F) << 4));
					SetVROM_1K_Bank(2, reg[2]);
					break;

				case 0xC002:
				case 0xC008:
					reg[3] = (byte)((reg[3] & 0xF0) | (data & 0x0F));
					SetVROM_1K_Bank(3, reg[3]);
					break;

				case 0xC003:
				case 0xC00C:
					reg[3] = (byte)((reg[3] & 0x0F) | ((data & 0x0F) << 4));
					SetVROM_1K_Bank(3, reg[3]);
					break;

				case 0xD000:
					reg[4] = (byte)((reg[4] & 0xF0) | (data & 0x0F));
					SetVROM_1K_Bank(4, reg[4]);
					break;

				case 0xD001:
				case 0xD004:
					reg[4] = (byte)((reg[4] & 0x0F) | ((data & 0x0F) << 4));
					SetVROM_1K_Bank(4, reg[4]);
					break;

				case 0xD002:
				case 0xD008:
					reg[5] = (byte)((reg[5] & 0xF0) | (data & 0x0F));
					SetVROM_1K_Bank(5, reg[5]);
					break;

				case 0xD003:
				case 0xD00C:
					reg[5] = (byte)((reg[5] & 0x0F) | ((data & 0x0F) << 4));
					SetVROM_1K_Bank(5, reg[5]);
					break;

				case 0xE000:
					reg[6] = (byte)((reg[6] & 0xF0) | (data & 0x0F));
					SetVROM_1K_Bank(6, reg[6]);
					break;

				case 0xE001:
				case 0xE004:
					reg[6] = (byte)((reg[6] & 0x0F) | ((data & 0x0F) << 4));
					SetVROM_1K_Bank(6, reg[6]);
					break;

				case 0xE002:
				case 0xE008:
					reg[7] = (byte)((reg[7] & 0xF0) | (data & 0x0F));
					SetVROM_1K_Bank(7, reg[7]);
					break;

				case 0xE003:
				case 0xE00C:
					reg[7] = ((byte)((reg[7] & 0x0F) | ((data & 0x0F) << 4)));
					SetVROM_1K_Bank(7, reg[7]);
					break;

				case 0xF000:
					irq_latch = (byte)((irq_latch & 0xF0) | (data & 0x0F));
					nes.cpu.ClrIRQ(IRQ_MAPPER);
					break;
				case 0xF004:
					irq_latch = (byte)((irq_latch & 0x0F) | ((data & 0x0F) << 4));
					nes.cpu.ClrIRQ(IRQ_MAPPER);
					break;

				case 0xF008:
					irq_enable = (byte)(data & 0x03);
					irq_counter = irq_latch;
					irq_clock = 0;
					nes.cpu.ClrIRQ(IRQ_MAPPER);
					break;

				case 0xF00C:
					irq_enable = (byte)((irq_enable & 0x01) * 3);
					nes.cpu.ClrIRQ(IRQ_MAPPER);
					break;
			}
		}

		//void Mapper023::Clock(INT cycles)
		public override void Clock(int cycles)
		{
			if ((irq_enable & 0x02) != 0)
			{
				irq_clock += cycles * 3;
				while (irq_clock >= 341)
				{
					irq_clock -= 341;
					irq_counter++;
					if (irq_counter == 0)
					{
						irq_counter = irq_latch;
						nes.cpu.SetIRQ(IRQ_MAPPER);
					}
				}
			}
		}

		//void Mapper023::SaveState(LPBYTE p)
		public override void SaveState(byte[] p)
		{
			//for (INT i = 0; i < 9; i++)
			//{
			//	p[i] = reg[i];
			//}
			//p[9] = irq_enable;
			//p[10] = irq_counter;
			//p[11] = irq_latch;
			//*(INT*)&p[12] = irq_clock;
		}

		//void Mapper023::LoadState(LPBYTE p)
		public override void LoadState(byte[] p)
		{
			//for (INT i = 0; i < 9; i++)
			//{
			//	reg[i] = p[i];
			//}
			//irq_enable = p[9];
			//irq_counter = p[10];
			//irq_latch = p[11];
			//irq_clock = *(INT*)&p[12];
		}


		public override bool IsStateSave()
		{
			return true;
		}
	}
}
