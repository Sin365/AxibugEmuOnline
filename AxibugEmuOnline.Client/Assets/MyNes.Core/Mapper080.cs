namespace MyNes.Core;

[BoardInfo("Taito X-005", 80)]
internal class Mapper080 : Board
{
	internal override void HardReset()
	{
		base.HardReset();
		Switch08KPRG(PRG_ROM_08KB_Mask, PRGArea.AreaE000);
	}

	internal override void WriteSRM(ref ushort address, ref byte data)
	{
		switch (address)
		{
		case 32496:
			Switch02KCHR(data >> 1, CHRArea.Area0000);
			break;
		case 32497:
			Switch02KCHR(data >> 1, CHRArea.Area0800);
			break;
		case 32498:
			Switch01KCHR(data, CHRArea.Area1000);
			break;
		case 32499:
			Switch01KCHR(data, CHRArea.Area1400);
			break;
		case 32500:
			Switch01KCHR(data, CHRArea.Area1800);
			break;
		case 32501:
			Switch01KCHR(data, CHRArea.Area1C00);
			break;
		case 32502:
			Switch01KNMTFromMirroring(((data & 1) == 1) ? Mirroring.Vert : Mirroring.Horz);
			break;
		case 32506:
		case 32507:
			Switch08KPRG(data, PRGArea.Area8000);
			break;
		case 32508:
		case 32509:
			Switch08KPRG(data, PRGArea.AreaA000);
			break;
		case 32510:
		case 32511:
			Switch08KPRG(data, PRGArea.AreaC000);
			break;
		case 32503:
		case 32504:
		case 32505:
			break;
		}
	}
}
