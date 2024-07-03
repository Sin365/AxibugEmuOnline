namespace MyNes.Core
{
    [BoardInfo("Unknown", 231)]
    internal class Mapper231 : Board
    {
    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		Switch16KPRG(address & 0x1E, PRGArea.Area8000);
    		Switch16KPRG((address & 0x1E) | ((address >> 5) & 1), PRGArea.AreaC000);
    		Switch01KNMTFromMirroring(((address & 0x80) == 128) ? Mirroring.Horz : Mirroring.Vert);
    	}
    }
}
