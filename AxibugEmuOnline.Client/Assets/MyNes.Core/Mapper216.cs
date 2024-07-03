namespace MyNes.Core
{
    [BoardInfo("Unknown", 216)]
    internal class Mapper216 : Board
    {
    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		Switch32KPRG(address, PRGArea.Area8000);
    		Switch08KCHR(address >> 1);
    	}
    }
}
