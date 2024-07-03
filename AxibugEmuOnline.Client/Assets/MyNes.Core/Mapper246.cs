namespace MyNes.Core;

[BoardInfo("Unknown", 246)]
internal class Mapper246 : Board
{
	internal override void HardReset()
	{
		base.HardReset();
		Switch08KPRG(255, PRGArea.AreaE000);
	}

	internal override void WriteSRM(ref ushort address, ref byte data)
	{
		if (address < 26624)
		{
			switch (address)
			{
			case 24576:
				Switch08KPRG(data, PRGArea.Area8000);
				break;
			case 24577:
				Switch08KPRG(data, PRGArea.AreaA000);
				break;
			case 24578:
				Switch08KPRG(data, PRGArea.AreaC000);
				break;
			case 24579:
				Switch08KPRG(data, PRGArea.AreaE000);
				break;
			case 24580:
				Switch02KCHR(data, CHRArea.Area0000);
				break;
			case 24581:
				Switch02KCHR(data, CHRArea.Area0800);
				break;
			case 24582:
				Switch02KCHR(data, CHRArea.Area1000);
				break;
			case 24583:
				Switch02KCHR(data, CHRArea.Area1800);
				break;
			}
		}
		else
		{
			base.WriteSRM(ref address, ref data);
		}
	}
}
