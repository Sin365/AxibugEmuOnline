namespace MyNes.Core
{
    [BoardInfo("Unknown", 242)]
    internal class Mapper242 : Board
    {
    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		Switch32KPRG((address >> 3) & 0xF, PRGArea.Area8000);
    		Switch01KNMTFromMirroring(((address & 2) == 2) ? Mirroring.Horz : Mirroring.Vert);
    	}
    }
}
