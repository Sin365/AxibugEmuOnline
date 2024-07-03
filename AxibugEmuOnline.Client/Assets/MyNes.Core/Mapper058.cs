namespace MyNes.Core
{
    [BoardInfo("68-in-1 (Game Star)", 58)]
    [HassIssues]
    internal class Mapper058 : Board
    {
    	internal override string Issues => MNInterfaceLanguage.IssueMapper58;

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		Switch08KCHR((address >> 3) & 7);
    		if ((address & 0x40) == 0)
    		{
    			Switch32KPRG((address & 7) >> 1, PRGArea.Area8000);
    		}
    		else
    		{
    			Switch16KPRG(address & 7, PRGArea.Area8000);
    			Switch16KPRG(address & 7, PRGArea.AreaC000);
    		}
    		Switch01KNMTFromMirroring(((address & 0x80) == 128) ? Mirroring.Horz : Mirroring.Vert);
    	}
    }
}
