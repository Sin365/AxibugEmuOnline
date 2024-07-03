using System.IO;

namespace MyNes.Core;

[BoardInfo("Namco 109", 76)]
internal class Mapper076 : Board
{
	private int address_8001;

	private bool prg_a;

	private byte prg_reg;

	internal override void HardReset()
	{
		base.HardReset();
		Switch08KPRG(PRG_ROM_08KB_Mask - 1, PRGArea.AreaC000);
		Switch08KPRG(PRG_ROM_08KB_Mask, PRGArea.AreaE000);
		address_8001 = 0;
		prg_a = false;
	}

	internal override void WritePRG(ref ushort address, ref byte data)
	{
		switch (address & 0xE001)
		{
		case 32768:
			address_8001 = data & 7;
			prg_a = (data & 0x40) == 64;
			Switch08KPRG(prg_reg, prg_a ? PRGArea.AreaC000 : PRGArea.Area8000);
			break;
		case 32769:
			switch (address_8001)
			{
			case 2:
				Switch02KCHR(data, CHRArea.Area0000);
				break;
			case 3:
				Switch02KCHR(data, CHRArea.Area0800);
				break;
			case 4:
				Switch02KCHR(data, CHRArea.Area1000);
				break;
			case 5:
				Switch02KCHR(data, CHRArea.Area1800);
				break;
			case 6:
				Switch08KPRG(prg_reg = data, prg_a ? PRGArea.AreaC000 : PRGArea.Area8000);
				break;
			case 7:
				Switch08KPRG(data, PRGArea.AreaA000);
				break;
			}
			break;
		case 40960:
			Switch01KNMTFromMirroring(((data & 1) == 1) ? Mirroring.Horz : Mirroring.Vert);
			break;
		}
	}

	internal override void WriteStateData(ref BinaryWriter stream)
	{
		base.WriteStateData(ref stream);
		stream.Write(address_8001);
		stream.Write(prg_a);
		stream.Write(prg_reg);
	}

	internal override void ReadStateData(ref BinaryReader stream)
	{
		base.ReadStateData(ref stream);
		address_8001 = stream.ReadInt32();
		prg_a = stream.ReadBoolean();
		prg_reg = stream.ReadByte();
	}
}
