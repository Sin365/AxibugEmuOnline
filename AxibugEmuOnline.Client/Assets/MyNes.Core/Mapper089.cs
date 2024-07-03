namespace MyNes.Core;

[BoardInfo("Sunsoft Early", 89)]
internal class Mapper089 : Board
{
	internal override void HardReset()
	{
		base.HardReset();
		Switch16KPRG(PRG_ROM_16KB_Mask, PRGArea.AreaC000);
	}

	internal override void WritePRG(ref ushort address, ref byte data)
	{
		Switch08KCHR((data & 7) | ((data >> 4) & 8));
		Switch16KPRG((data >> 4) & 7, PRGArea.Area8000);
		Switch01KNMTFromMirroring(((data & 8) == 8) ? Mirroring.OneScB : Mirroring.OneScA);
	}
}
