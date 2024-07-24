namespace MyNes.Core
{
    [BoardInfo("Irem 74HC161/32", 78)]
    internal class Mapper078 : Board
    {
    	private bool mirroring_mode_single;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch16KPRG(PRG_ROM_16KB_Mask, PRGArea.AreaC000);
    		mirroring_mode_single = false;
    		if (base.BoardType == "JALECO-JF-16")
    		{
    			mirroring_mode_single = true;
    		}
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		Switch08KCHR((data >> 4) & 0xF);
    		Switch16KPRG(data & 7, PRGArea.Area8000);
    		if (mirroring_mode_single)
    		{
    			Switch01KNMTFromMirroring(((data & 8) == 8) ? Mirroring.OneScB : Mirroring.OneScA);
    		}
    		else
    		{
    			Switch01KNMTFromMirroring(((data & 8) == 8) ? Mirroring.Vert : Mirroring.Horz);
    		}
    	}
    }
}
