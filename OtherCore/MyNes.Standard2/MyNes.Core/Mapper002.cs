namespace MyNes.Core
{
    [BoardInfo("UxROM", 2)]
    internal class Mapper002 : Board
    {
    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch16KPRG(PRG_ROM_16KB_Mask, PRGArea.AreaC000);
    	}

    	internal override void WritePRG(ref ushort addr, ref byte val)
    	{
    		Switch16KPRG(val, PRGArea.Area8000);
    	}
    }
}
