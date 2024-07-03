namespace MyNes.Core;

[BoardInfo("150-in-1", 202)]
[HassIssues]
internal class Mapper202 : Board
{
	internal override string Issues => MNInterfaceLanguage.IssueMapper202;

	internal override void WritePRG(ref ushort address, ref byte data)
	{
		Switch01KNMTFromMirroring(((address & 1) == 1) ? Mirroring.Horz : Mirroring.Vert);
		Switch08KCHR((address >> 1) & 7);
		if ((address & 0xC) == 12)
		{
			Switch32KPRG(3, PRGArea.Area8000);
			return;
		}
		Switch16KPRG((address >> 1) & 7, PRGArea.Area8000);
		Switch16KPRG((address >> 1) & 7, PRGArea.AreaC000);
	}
}
