using System.IO;

namespace MyNes.Core;

[BoardInfo("NAMCOT-3453", 154)]
[HassIssues]
internal class Mapper154 : Board
{
	private int address_8001;

	internal override string Issues => MNInterfaceLanguage.IssueMapper154;

	internal override void HardReset()
	{
		base.HardReset();
		address_8001 = 0;
		Switch16KPRG(PRG_ROM_16KB_Mask, PRGArea.AreaC000);
	}

	internal override void WritePRG(ref ushort address, ref byte data)
	{
		switch (address & 0x8001)
		{
		case 32768:
			address_8001 = data & 7;
			Switch01KNMTFromMirroring(((data & 0x40) == 64) ? Mirroring.OneScB : Mirroring.OneScA);
			break;
		case 32769:
			switch (address_8001)
			{
			case 0:
				Switch02KCHR((data & 0x3F) >> 1, CHRArea.Area0000);
				break;
			case 1:
				Switch02KCHR((data & 0x3F) >> 1, CHRArea.Area0800);
				break;
			case 2:
				Switch01KCHR(data | 0x40, CHRArea.Area1000);
				break;
			case 3:
				Switch01KCHR(data | 0x40, CHRArea.Area1400);
				break;
			case 4:
				Switch01KCHR(data | 0x40, CHRArea.Area1800);
				break;
			case 5:
				Switch01KCHR(data | 0x40, CHRArea.Area1C00);
				break;
			case 6:
				Switch08KPRG(data, PRGArea.Area8000);
				break;
			case 7:
				Switch08KPRG(data, PRGArea.AreaA000);
				break;
			}
			break;
		}
	}

	internal override void WriteStateData(ref BinaryWriter stream)
	{
		base.WriteStateData(ref stream);
		stream.Write(address_8001);
	}

	internal override void ReadStateData(ref BinaryReader stream)
	{
		base.ReadStateData(ref stream);
		address_8001 = stream.ReadInt32();
	}
}
