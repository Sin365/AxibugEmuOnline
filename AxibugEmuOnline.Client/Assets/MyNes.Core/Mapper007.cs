namespace MyNes.Core
{
    [BoardInfo("AxROM", 7)]
    internal class Mapper007 : Board
    {
    	internal override void WritePRG(ref ushort addr, ref byte val)
    	{
    		Switch01KNMTFromMirroring(((val & 0x10) == 16) ? Mirroring.OneScB : Mirroring.OneScA);
    		Switch32KPRG(val & 7, PRGArea.Area8000);
    	}
    }
}
