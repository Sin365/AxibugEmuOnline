namespace MyNes.Core
{
    [BoardInfo("Color Dreams", 11)]
    internal class Mapper011 : Board
    {
    	private byte writeData;

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		ReadPRG(ref address, out writeData);
    		writeData &= data;
    		Switch32KPRG(writeData & 3, PRGArea.Area8000);
    		Switch08KCHR((writeData >> 4) & 0xF);
    	}
    }
}
