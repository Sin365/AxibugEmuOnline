namespace MyNes.Core
{
    [BoardInfo("FFE F3xxx", 8)]
    [HassIssues]
    internal class Mapper008 : FFE
    {
    	internal override string Issues => MNInterfaceLanguage.IssueMapper8;

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		Switch32KPRG((data >> 4) & 3, PRGArea.Area8000);
    		Switch08KCHR(data & 3);
    	}
    }
}
