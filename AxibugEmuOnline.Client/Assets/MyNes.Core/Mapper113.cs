namespace MyNes.Core;

[BoardInfo("Sachen/Hacker/Nina", 113)]
internal class Mapper113 : Board
{
	internal override void WriteEX(ref ushort address, ref byte data)
	{
		if ((address & 0x4100) == 16640)
		{
			Switch08KCHR((data & 7) | ((data & 0x40) >> 3));
			Switch32KPRG((data >> 3) & 7, PRGArea.Area8000);
			Switch01KNMTFromMirroring(((data & 0x80) == 128) ? Mirroring.Vert : Mirroring.Horz);
		}
	}
}
