namespace MyNes.Core;

[BoardInfo("Jaleco/Konami", 87)]
internal class Mapper087 : Board
{
	internal override void WriteSRM(ref ushort address, ref byte data)
	{
		Switch08KCHR(((data & 2) >> 1) | ((data & 1) << 1));
	}
}
