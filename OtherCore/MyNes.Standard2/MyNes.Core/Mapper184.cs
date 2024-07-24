namespace MyNes.Core
{
    [BoardInfo("Sunsoft", 184)]
    internal class Mapper184 : Board
    {
    	internal override void WriteSRM(ref ushort address, ref byte data)
    	{
    		Switch04KCHR(data & 7, CHRArea.Area0000);
    		Switch04KCHR((data >> 4) & 7, CHRArea.Area1000);
    	}
    }
}
