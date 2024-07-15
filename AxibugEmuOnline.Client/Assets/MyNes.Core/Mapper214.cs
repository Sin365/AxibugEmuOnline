namespace MyNes.Core
{
    [BoardInfo("X-in-1", 214)]
    internal class Mapper214 : Board
    {
    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		Switch16KPRG(address >> 2, PRGArea.Area8000);
    		Switch16KPRG(address >> 2, PRGArea.AreaC000);
    		Switch08KCHR(address);
    	}
    }
}
