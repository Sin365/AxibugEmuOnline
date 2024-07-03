namespace MyNes.Core;

[BoardInfo("64-in-1", 204)]
internal class Mapper204 : Board
{
	internal override void WritePRG(ref ushort address, ref byte data)
	{
		Switch01KNMTFromMirroring(((address & 0x10) == 16) ? Mirroring.Horz : Mirroring.Vert);
		Switch08KCHR(address & 7);
		if ((address & 6) == 6)
		{
			Switch32KPRG(3, PRGArea.Area8000);
			return;
		}
		Switch16KPRG(address & 7, PRGArea.Area8000);
		Switch16KPRG(address & 7, PRGArea.AreaC000);
	}
}
