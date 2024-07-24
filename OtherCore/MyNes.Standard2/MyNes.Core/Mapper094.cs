namespace MyNes.Core
{
    [BoardInfo("74161/32", 94)]
    internal class Mapper094 : Board
    {
    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch16KPRG(PRG_ROM_16KB_Mask, PRGArea.AreaC000);
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		Switch16KPRG((data >> 2) & 7, PRGArea.Area8000);
    	}
    }
}
