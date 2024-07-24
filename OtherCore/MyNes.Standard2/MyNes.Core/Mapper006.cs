namespace MyNes.Core
{
    [BoardInfo("FFE F4xxx", 6)]
    [HassIssues]
    internal class Mapper006 : FFE
    {
    	internal override string Issues => MNInterfaceLanguage.IssueMapper6;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch16KPRG(7, PRGArea.AreaC000);
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		Switch08KCHR(data & 3);
    		Switch16KPRG((data >> 2) & 0xF, PRGArea.Area8000);
    	}
    }
}
