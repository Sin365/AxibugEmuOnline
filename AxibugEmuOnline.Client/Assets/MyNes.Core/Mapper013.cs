namespace MyNes.Core
{
    [BoardInfo("CPROM", 13, 1, 16)]
    internal class Mapper013 : Board
    {
    	private byte writeData;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		Toggle08KCHR_RAM(ram: true);
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		ReadPRG(ref address, out writeData);
    		writeData &= data;
    		Switch04KCHR(writeData & 3, CHRArea.Area1000);
    	}
    }
}
