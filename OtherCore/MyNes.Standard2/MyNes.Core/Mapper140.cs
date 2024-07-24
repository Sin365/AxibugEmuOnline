namespace MyNes.Core
{
    [BoardInfo("Unknown", 140)]
    internal class Mapper140 : Board
    {
    	internal override void WriteSRM(ref ushort address, ref byte data)
    	{
    		Switch08KCHR(data & 0xF);
    		Switch32KPRG((data >> 4) & 3, PRGArea.Area8000);
    	}
    }
}
