namespace MyNes.Core
{
    [BoardInfo("74161/32", 93)]
    internal class Mapper093 : Board
    {
    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch16KPRG(PRG_ROM_16KB_Mask, PRGArea.AreaC000);
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		Switch16KPRG((data >> 4) & 0xF, PRGArea.Area8000);
    		Switch01KNMTFromMirroring(((data & 1) == 1) ? Mirroring.Horz : Mirroring.Vert);
    	}
    }
}
