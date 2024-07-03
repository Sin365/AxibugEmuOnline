namespace MyNes.Core
{
    [BoardInfo("Bandai", 70)]
    internal class Mapper070 : Board
    {
    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch16KPRG(PRG_ROM_08KB_Mask, PRGArea.AreaC000);
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		Switch16KPRG((data >> 4) & 0xF, PRGArea.Area8000);
    		Switch08KCHR(data & 0xF);
    	}
    }
}
