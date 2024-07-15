namespace MyNes.Core
{
    [BoardInfo("Super HIK 300-in-1 (1994)", 212)]
    internal class Mapper212 : Board
    {
    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		Switch08KCHR(address & 7);
    		Switch01KNMTFromMirroring(((address & 8) == 8) ? Mirroring.Horz : Mirroring.Vert);
    		if ((address & 0x4000) == 16384)
    		{
    			Switch32KPRG((address >> 1) & 3, PRGArea.Area8000);
    			return;
    		}
    		Switch16KPRG(address & 7, PRGArea.Area8000);
    		Switch16KPRG(address & 7, PRGArea.AreaC000);
    	}
    }
}
