using System;
using System.IO;

namespace MyNes.Core;

[BoardInfo("2-in-1 MMC3 Port 6000h", 47, true, true)]
internal class Mapper047 : Board
{
	private bool flag_c;

	private bool flag_p;

	private int address_8001;

	private int[] chr_reg;

	private int[] prg_reg;

	private bool irq_enabled;

	private byte irq_counter;

	private int old_irq_counter;

	private byte irq_reload;

	private bool irq_clear;

	private bool mmc3_alt_behavior;

	private int block;

	private int prg_and;

	private int prg_or;

	private int chr_and;

	private int chr_or;

	internal override void HardReset()
	{
		base.HardReset();
		flag_c = (flag_p = false);
		address_8001 = 0;
		prg_and = 15;
		prg_or = 0;
		chr_and = 127;
		chr_or = 0;
		prg_reg = new int[4];
		prg_reg[0] = 0;
		prg_reg[1] = 1;
		prg_reg[2] = PRG_ROM_08KB_Mask - 1;
		prg_reg[3] = PRG_ROM_08KB_Mask;
		SetupPRG();
		chr_reg = new int[6];
		for (int i = 0; i < 6; i++)
		{
			chr_reg[i] = 0;
		}
		irq_enabled = false;
		irq_counter = 0;
		irq_reload = byte.MaxValue;
		old_irq_counter = 0;
		irq_clear = false;
		if (IsGameFoundOnDB)
		{
			switch (GameCartInfo.chip_type[0].ToLower())
			{
			case "mmc3a":
				mmc3_alt_behavior = true;
				Console.WriteLine("Chip= MMC3 A, MMC3 IQR mode switched to RevA");
				break;
			case "mmc3b":
				mmc3_alt_behavior = false;
				Console.WriteLine("Chip= MMC3 B, MMC3 IQR mode switched to RevB");
				break;
			case "mmc3c":
				mmc3_alt_behavior = false;
				Console.WriteLine("Chip= MMC3 C, MMC3 IQR mode switched to RevB");
				break;
			}
		}
	}

	internal override void WriteSRM(ref ushort address, ref byte data)
	{
		if (PRG_RAM_ENABLED[PRG_AREA_BLK_INDEX[0]] && PRG_RAM_WRITABLE[PRG_AREA_BLK_INDEX[0]])
		{
			block = data & 1;
			prg_or = block << 4;
			chr_or = block << 7;
			SetupCHR();
			SetupPRG();
		}
	}

	internal override void WritePRG(ref ushort address, ref byte data)
	{
		switch (address & 0xE001)
		{
		case 32768:
			address_8001 = data & 7;
			flag_c = (data & 0x80) != 0;
			flag_p = (data & 0x40) != 0;
			SetupCHR();
			SetupPRG();
			break;
		case 32769:
			switch (address_8001)
			{
			case 0:
			case 1:
			case 2:
			case 3:
			case 4:
			case 5:
				chr_reg[address_8001] = data;
				SetupCHR();
				break;
			case 6:
			case 7:
				prg_reg[address_8001 - 6] = data & PRG_ROM_08KB_Mask;
				SetupPRG();
				break;
			}
			break;
		case 40960:
			if (NMT_DEFAULT_MIRROR != Mirroring.Full)
			{
				Switch01KNMTFromMirroring(((data & 1) == 1) ? Mirroring.Horz : Mirroring.Vert);
			}
			break;
		case 40961:
			TogglePRGRAMEnable((data & 0x80) != 0);
			TogglePRGRAMWritableEnable((data & 0x40) == 0);
			break;
		case 49152:
			irq_reload = data;
			break;
		case 49153:
			if (mmc3_alt_behavior)
			{
				irq_clear = true;
			}
			irq_counter = 0;
			break;
		case 57344:
			irq_enabled = false;
			NesEmu.IRQFlags &= -9;
			break;
		case 57345:
			irq_enabled = true;
			break;
		}
	}

	private void SetupCHR()
	{
		if (!flag_c)
		{
			Switch02KCHR(((chr_reg[0] & chr_and) | chr_or) >> 1, CHRArea.Area0000);
			Switch02KCHR(((chr_reg[1] & chr_and) | chr_or) >> 1, CHRArea.Area0800);
			Switch01KCHR((chr_reg[2] & chr_and) | chr_or, CHRArea.Area1000);
			Switch01KCHR((chr_reg[3] & chr_and) | chr_or, CHRArea.Area1400);
			Switch01KCHR((chr_reg[4] & chr_and) | chr_or, CHRArea.Area1800);
			Switch01KCHR((chr_reg[5] & chr_and) | chr_or, CHRArea.Area1C00);
		}
		else
		{
			Switch02KCHR(((chr_reg[0] & chr_and) | chr_or) >> 1, CHRArea.Area1000);
			Switch02KCHR(((chr_reg[1] & chr_and) | chr_or) >> 1, CHRArea.Area1800);
			Switch01KCHR((chr_reg[2] & chr_and) | chr_or, CHRArea.Area0000);
			Switch01KCHR((chr_reg[3] & chr_and) | chr_or, CHRArea.Area0400);
			Switch01KCHR((chr_reg[4] & chr_and) | chr_or, CHRArea.Area0800);
			Switch01KCHR((chr_reg[5] & chr_and) | chr_or, CHRArea.Area0C00);
		}
	}

	private void SetupPRG()
	{
		Switch08KPRG((prg_reg[flag_p ? 2 : 0] & prg_and) | prg_or, PRGArea.Area8000);
		Switch08KPRG((prg_reg[1] & prg_and) | prg_or, PRGArea.AreaA000);
		Switch08KPRG((prg_reg[(!flag_p) ? 2 : 0] & prg_and) | prg_or, PRGArea.AreaC000);
		Switch08KPRG((prg_reg[3] & prg_and) | prg_or, PRGArea.AreaE000);
	}

	internal override void OnPPUA12RaisingEdge()
	{
		old_irq_counter = irq_counter;
		if (irq_counter == 0 || irq_clear)
		{
			irq_counter = irq_reload;
		}
		else
		{
			irq_counter--;
		}
		if ((!mmc3_alt_behavior || old_irq_counter != 0 || irq_clear) && irq_counter == 0 && irq_enabled)
		{
			NesEmu.IRQFlags |= 8;
		}
		irq_clear = false;
	}

	internal override void WriteStateData(ref BinaryWriter stream)
	{
		base.WriteStateData(ref stream);
		stream.Write(flag_c);
		stream.Write(flag_p);
		stream.Write(address_8001);
		stream.Write(block);
		for (int i = 0; i < chr_reg.Length; i++)
		{
			stream.Write(chr_reg[i]);
		}
		for (int j = 0; j < prg_reg.Length; j++)
		{
			stream.Write(prg_reg[j]);
		}
		stream.Write(irq_counter);
		stream.Write(old_irq_counter);
		stream.Write(irq_reload);
		stream.Write(irq_clear);
		stream.Write(prg_and);
		stream.Write(prg_or);
		stream.Write(chr_and);
		stream.Write(chr_or);
		stream.Write(irq_enabled);
	}

	internal override void ReadStateData(ref BinaryReader stream)
	{
		base.ReadStateData(ref stream);
		flag_c = stream.ReadBoolean();
		flag_p = stream.ReadBoolean();
		address_8001 = stream.ReadInt32();
		block = stream.ReadInt32();
		for (int i = 0; i < chr_reg.Length; i++)
		{
			chr_reg[i] = stream.ReadInt32();
		}
		for (int j = 0; j < prg_reg.Length; j++)
		{
			prg_reg[j] = stream.ReadInt32();
		}
		irq_counter = stream.ReadByte();
		old_irq_counter = stream.ReadInt32();
		irq_reload = stream.ReadByte();
		irq_clear = stream.ReadBoolean();
		prg_and = stream.ReadInt32();
		prg_or = stream.ReadInt32();
		chr_and = stream.ReadInt32();
		chr_or = stream.ReadInt32();
		irq_enabled = stream.ReadBoolean();
	}
}
