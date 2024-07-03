namespace MyNes.Core;

[BoardInfo("Sachen", 133)]
internal class Mapper133 : Board
{
	internal override void WriteEX(ref ushort address, ref byte data)
	{
		Switch08KCHR(data & 3);
		Switch32KPRG(data >> 2, PRGArea.Area8000);
	}
}
