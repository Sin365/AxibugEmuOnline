namespace MyNes.Core
{
    [BoardInfo("Unknown", 203)]
    [HassIssues]
    internal class Mapper203 : Board
    {
    	internal override string Issues => MNInterfaceLanguage.IssueMapper203;

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		Switch08KCHR(data & 3);
    		Switch16KPRG(data & 0x3F, PRGArea.Area8000);
    		Switch16KPRG(data & 0x3F, PRGArea.AreaC000);
    	}
    }
}
