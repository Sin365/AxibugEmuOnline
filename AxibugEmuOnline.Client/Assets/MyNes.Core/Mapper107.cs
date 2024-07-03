namespace MyNes.Core
{
    internal class Mapper107 : Board
    {
    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		Switch32KPRG(data >> 1, PRGArea.Area8000);
    		Switch08KCHR(data);
    	}
    }
}
