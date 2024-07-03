namespace MyNes.Core;

[BoardInfo("Camerica", 71)]
internal class Mapper071 : Board
{
	private bool fireHawk;

	internal override void HardReset()
	{
		base.HardReset();
		Switch16KPRG(PRG_ROM_16KB_Mask, PRGArea.AreaC000);
		fireHawk = SHA1.ToUpper() == "334781C830F135CF30A33E392D8AAA4AFDC223F9";
	}

	internal override void WritePRG(ref ushort addr, ref byte val)
	{
		if (addr < 40960)
		{
			if (fireHawk)
			{
				Switch01KNMTFromMirroring(((val & 0x10) == 16) ? Mirroring.OneScB : Mirroring.OneScA);
			}
		}
		else if (addr >= 49152)
		{
			Switch16KPRG(val, PRGArea.Area8000);
		}
	}
}
