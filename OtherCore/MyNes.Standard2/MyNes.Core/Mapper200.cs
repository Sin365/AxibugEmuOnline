namespace MyNes.Core
{
    [BoardInfo("Unknown", 200)]
    internal class Mapper200 : Board
    {
    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		Switch08KCHR(address & 7);
    		Switch16KPRG(address & 7, PRGArea.Area8000);
    		Switch16KPRG(address & 7, PRGArea.AreaC000);
    		Switch01KNMTFromMirroring(((address & 8) == 8) ? Mirroring.Horz : Mirroring.Vert);
    	}
    }
}
