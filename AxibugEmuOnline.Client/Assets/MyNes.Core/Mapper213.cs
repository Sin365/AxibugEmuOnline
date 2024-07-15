namespace MyNes.Core
{
    [BoardInfo("9999999-in-1", 213)]
    internal class Mapper213 : Board
    {
    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		Switch08KCHR((address >> 3) & 7);
    		Switch32KPRG((address >> 1) & 3, PRGArea.Area8000);
    	}
    }
}
