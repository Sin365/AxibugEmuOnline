namespace MyNes.Core;

[BoardInfo("GxROM", 66)]
internal class Mapper066 : Board
{
	internal override void WritePRG(ref ushort address, ref byte data)
	{
		Switch32KPRG((data >> 4) & 3, PRGArea.Area8000);
		Switch08KCHR(data & 3);
	}
}
