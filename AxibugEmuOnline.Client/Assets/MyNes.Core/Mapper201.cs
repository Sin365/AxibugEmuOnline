namespace MyNes.Core
{
    [BoardInfo("Unknown", 201)]
    internal class Mapper201 : Board
    {
    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		Switch08KCHR(address & 0xFF);
    		Switch32KPRG(address & 0xFF, PRGArea.Area8000);
    	}
    }
}
