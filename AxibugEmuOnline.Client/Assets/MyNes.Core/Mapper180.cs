namespace MyNes.Core
{
    [BoardInfo("Nihon Bussan", 180)]
    [HassIssues]
    internal class Mapper180 : Board
    {
    	internal override string Issues => MNInterfaceLanguage.IssueMapper180;

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		Switch16KPRG(data & 7, PRGArea.AreaC000);
    	}
    }
}
