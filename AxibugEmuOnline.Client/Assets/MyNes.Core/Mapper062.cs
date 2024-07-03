namespace MyNes.Core
{
    [BoardInfo("Super 700-in-1", 62)]
    internal class Mapper062 : Board
    {
    	private int prg_page;

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		prg_page = ((address & 0x3F00) >> 8) | (address & 0x40);
    		Switch08KCHR(((address & 0x1F) << 2) | (data & 3));
    		if ((address & 0x20) == 32)
    		{
    			Switch16KPRG(prg_page, PRGArea.Area8000);
    			Switch16KPRG(prg_page, PRGArea.AreaC000);
    		}
    		else
    		{
    			Switch32KPRG(prg_page >> 1, PRGArea.Area8000);
    		}
    		Switch01KNMTFromMirroring(((address & 0x80) == 128) ? Mirroring.Horz : Mirroring.Vert);
    	}
    }
}
