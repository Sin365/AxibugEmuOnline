namespace MyNes.Core
{
    [BoardInfo("20-in-1", 61)]
    internal class Mapper061 : Board
    {
    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		if ((address & 0x10) == 0)
    		{
    			Switch32KPRG(address & 0xF, PRGArea.Area8000);
    		}
    		else
    		{
    			Switch16KPRG(((address & 0xF) << 1) | ((address & 0x20) >> 5), PRGArea.Area8000);
    			Switch16KPRG(((address & 0xF) << 1) | ((address & 0x20) >> 5), PRGArea.AreaC000);
    		}
    		Switch01KNMTFromMirroring(((address & 0x80) == 128) ? Mirroring.Horz : Mirroring.Vert);
    	}
    }
}
