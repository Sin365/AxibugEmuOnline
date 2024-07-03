using System.IO;

namespace MyNes.Core;

[BoardInfo("Unknown", 206)]
internal class Mapper206 : Board
{
	private bool flag_c;

	private bool flag_p;

	private int address_8001;

	private int[] chr_reg;

	private int[] prg_reg;

	internal override void HardReset()
	{
		base.HardReset();
		flag_c = (flag_p = false);
		address_8001 = 0;
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
		}
	}

	private void SetupCHR()
	{
		if (!flag_c)
		{
			Switch02KCHR(chr_reg[0] >> 1, CHRArea.Area0000);
			Switch02KCHR(chr_reg[1] >> 1, CHRArea.Area0800);
			Switch01KCHR(chr_reg[2], CHRArea.Area1000);
			Switch01KCHR(chr_reg[3], CHRArea.Area1400);
			Switch01KCHR(chr_reg[4], CHRArea.Area1800);
			Switch01KCHR(chr_reg[5], CHRArea.Area1C00);
		}
		else
		{
			Switch02KCHR(chr_reg[0] >> 1, CHRArea.Area1000);
			Switch02KCHR(chr_reg[1] >> 1, CHRArea.Area1800);
			Switch01KCHR(chr_reg[2], CHRArea.Area0000);
			Switch01KCHR(chr_reg[3], CHRArea.Area0400);
			Switch01KCHR(chr_reg[4], CHRArea.Area0800);
			Switch01KCHR(chr_reg[5], CHRArea.Area0C00);
		}
	}

	private void SetupPRG()
	{
		Switch08KPRG(prg_reg[flag_p ? 2 : 0], PRGArea.Area8000);
		Switch08KPRG(prg_reg[1], PRGArea.AreaA000);
		Switch08KPRG(prg_reg[(!flag_p) ? 2 : 0], PRGArea.AreaC000);
		Switch08KPRG(prg_reg[3], PRGArea.AreaE000);
	}

	internal override void WriteStateData(ref BinaryWriter stream)
	{
		base.WriteStateData(ref stream);
		stream.Write(flag_c);
		stream.Write(flag_p);
		stream.Write(address_8001);
		for (int i = 0; i < chr_reg.Length; i++)
		{
			stream.Write(chr_reg[i]);
		}
		for (int j = 0; j < prg_reg.Length; j++)
		{
			stream.Write(prg_reg[j]);
		}
	}

	internal override void ReadStateData(ref BinaryReader stream)
	{
		base.ReadStateData(ref stream);
		flag_c = stream.ReadBoolean();
		flag_p = stream.ReadBoolean();
		address_8001 = stream.ReadInt32();
		for (int i = 0; i < chr_reg.Length; i++)
		{
			chr_reg[i] = stream.ReadInt32();
		}
		for (int j = 0; j < prg_reg.Length; j++)
		{
			prg_reg[j] = stream.ReadInt32();
		}
	}
}
