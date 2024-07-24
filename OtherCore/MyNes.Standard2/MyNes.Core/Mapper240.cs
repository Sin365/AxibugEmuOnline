namespace MyNes.Core
{
    [BoardInfo("Unknown", 240)]
    internal class Mapper240 : Board
    {
    	internal override void WriteEX(ref ushort address, ref byte data)
    	{
    		Switch32KPRG((data >> 4) & 0xF, PRGArea.Area8000);
    		Switch08KCHR(data & 0xF);
    	}
    }
}
