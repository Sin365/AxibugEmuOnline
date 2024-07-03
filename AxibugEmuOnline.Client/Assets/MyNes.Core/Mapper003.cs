namespace MyNes.Core;

[BoardInfo("CNROM", 3)]
internal class Mapper003 : Board
{
	private byte data_temp;

	internal override void WritePRG(ref ushort address, ref byte data)
	{
		ReadPRG(ref address, out data_temp);
		data_temp &= data;
		Switch08KCHR(data_temp);
	}
}
