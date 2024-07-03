namespace MyNes.Core
{
    [BoardInfo("MMC4", 10)]
    internal class Mapper010 : MMC2
    {
    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch16KPRG(0, PRGArea.Area8000);
    		Switch16KPRG(PRG_ROM_16KB_Mask, PRGArea.AreaC000);
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		if ((address & 0xF000) == 40960)
    		{
    			Switch16KPRG(data, PRGArea.Area8000);
    		}
    		else
    		{
    			base.WritePRG(ref address, ref data);
    		}
    	}
    }
}
