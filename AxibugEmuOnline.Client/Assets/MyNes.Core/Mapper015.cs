namespace MyNes.Core;

[BoardInfo("100-in-1 Contra Function 16", 15)]
internal class Mapper015 : Board
{
	private int temp;

	internal override void WritePRG(ref ushort address, ref byte data)
	{
		switch (address & 3)
		{
		case 0:
			Switch16KPRG(data & 0x3F, PRGArea.Area8000);
			Switch16KPRG((data & 0x3F) | 1, PRGArea.AreaC000);
			break;
		case 1:
			Switch16KPRG(data & 0x3F, PRGArea.Area8000);
			Switch16KPRG(PRG_ROM_16KB_Mask, PRGArea.AreaC000);
			break;
		case 2:
			temp = data << 1;
			temp = ((data & 0x3F) << 1) | ((data >> 7) & 1);
			Switch08KPRG(temp, PRGArea.Area8000);
			Switch08KPRG(temp, PRGArea.AreaA000);
			Switch08KPRG(temp, PRGArea.AreaC000);
			Switch08KPRG(temp, PRGArea.AreaE000);
			break;
		case 3:
			Switch16KPRG(data & 0x3F, PRGArea.Area8000);
			Switch16KPRG(data & 0x3F, PRGArea.AreaC000);
			break;
		}
		Switch01KNMTFromMirroring(((data & 0x40) == 64) ? Mirroring.Horz : Mirroring.Vert);
	}
}
