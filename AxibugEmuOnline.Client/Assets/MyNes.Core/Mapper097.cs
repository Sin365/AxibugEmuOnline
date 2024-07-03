namespace MyNes.Core
{
    [BoardInfo("Irem - PRG HI", 97)]
    internal class Mapper097 : Board
    {
    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch16KPRG(PRG_ROM_16KB_Mask, PRGArea.Area8000);
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		Switch16KPRG(data & 0xF, PRGArea.AreaC000);
    		switch ((address >> 6) & 3)
    		{
    		case 0:
    			Switch01KNMTFromMirroring(Mirroring.OneScA);
    			break;
    		case 1:
    			Switch01KNMTFromMirroring(Mirroring.Horz);
    			break;
    		case 2:
    			Switch01KNMTFromMirroring(Mirroring.Vert);
    			break;
    		case 3:
    			Switch01KNMTFromMirroring(Mirroring.OneScB);
    			break;
    		}
    	}
    }
}
