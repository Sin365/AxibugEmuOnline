namespace MyNes.Core;

[BoardInfo("Jaleco Early Mapper 2", 86)]
internal class Mapper086 : Board
{
	internal override void WriteSRM(ref ushort address, ref byte data)
	{
		if (address < 28672)
		{
			Switch32KPRG((data >> 4) & 3, PRGArea.Area8000);
			Switch08KCHR((data & 7) | ((data >> 4) & 4));
		}
	}
}
