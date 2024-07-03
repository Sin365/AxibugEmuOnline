namespace MyNes.Core
{
    [BoardInfo("Unknown", 230)]
    [HassIssues]
    internal class Mapper230 : Board
    {
    	private bool contraMode = false;

    	internal override string Issues => MNInterfaceLanguage.IssueMapper230;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		contraMode = true;
    		Switch16KPRG(0, PRGArea.Area8000);
    		Switch16KPRG(7, PRGArea.AreaC000);
    	}

    	internal override void SoftReset()
    	{
    		base.SoftReset();
    		contraMode = !contraMode;
    		if (contraMode)
    		{
    			Switch16KPRG(0, PRGArea.Area8000);
    			Switch16KPRG(7, PRGArea.AreaC000);
    			Switch01KNMTFromMirroring(Mirroring.Vert);
    		}
    		else
    		{
    			Switch08KCHR(0);
    			Switch16KPRG(8, PRGArea.Area8000);
    			Switch16KPRG(39, PRGArea.AreaC000);
    		}
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		if (contraMode)
    		{
    			Switch16KPRG(data & 7, PRGArea.Area8000);
    			Switch16KPRG(7, PRGArea.AreaC000);
    			Switch01KNMTFromMirroring(Mirroring.Vert);
    			return;
    		}
    		Switch01KNMTFromMirroring(((data & 0x40) == 64) ? Mirroring.Vert : Mirroring.Horz);
    		if ((data & 0x20) == 32)
    		{
    			Switch16KPRG((data & 0x1F) + 8, PRGArea.Area8000);
    			Switch16KPRG((data & 0x1F) + 8, PRGArea.AreaC000);
    		}
    		else
    		{
    			Switch32KPRG(((data & 0x1F) >> 1) + 4, PRGArea.Area8000);
    		}
    	}
    }
}
