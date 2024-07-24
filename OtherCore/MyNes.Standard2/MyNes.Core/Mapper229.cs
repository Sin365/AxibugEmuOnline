namespace MyNes.Core
{
    [BoardInfo("Unknown", 229)]
    [HassIssues]
    internal class Mapper229 : Board
    {
    	internal override string Issues => MNInterfaceLanguage.IssueMapper229;

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		Switch01KNMTFromMirroring(((address & 0x20) == 32) ? Mirroring.Horz : Mirroring.Vert);
    		Switch16KPRG(((address & 0x1E) == 30) ? (address & 0x1F) : 0, PRGArea.Area8000);
    		Switch16KPRG(((address & 0x1E) != 30) ? 1 : (address & 0x1F), PRGArea.AreaC000);
    		Switch08KCHR(address);
    	}
    }
}
