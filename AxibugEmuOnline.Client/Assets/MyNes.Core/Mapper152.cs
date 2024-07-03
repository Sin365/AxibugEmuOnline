namespace MyNes.Core;

[BoardInfo("Unknown", 152)]
internal class Mapper152 : Board
{
	internal override void HardReset()
	{
		base.HardReset();
		Switch16KPRG(PRG_ROM_16KB_Mask, PRGArea.AreaC000);
	}

	internal override void WritePRG(ref ushort address, ref byte data)
	{
		Switch08KCHR(data & 0xF);
		Switch16KPRG((data >> 4) & 7, PRGArea.Area8000);
		Switch01KNMTFromMirroring(((data & 0x80) == 128) ? Mirroring.OneScB : Mirroring.OneScA);
	}
}
