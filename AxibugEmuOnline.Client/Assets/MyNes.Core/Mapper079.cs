namespace MyNes.Core
{
    [BoardInfo("AVE Nina-3", 79)]
    internal class Mapper079 : Board
    {
    	internal override void WriteEX(ref ushort address, ref byte data)
    	{
    		if ((address ^ 0x4100) == 0)
    		{
    			Switch32KPRG((data >> 3) & 7, PRGArea.Area8000);
    			Switch08KCHR((data & 7) | ((data >> 3) & 8));
    		}
    	}
    }
}
