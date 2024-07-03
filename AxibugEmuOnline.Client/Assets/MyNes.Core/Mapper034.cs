namespace MyNes.Core
{
    [BoardInfo("BxROM/NINA-001", 34)]
    internal class Mapper034 : Board
    {
    	private bool BxROM;

    	private byte writeData;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		BxROM = true;
    		if (base.BoardType.Contains("NINA"))
    		{
    			BxROM = false;
    		}
    	}

    	internal override void WriteSRM(ref ushort address, ref byte data)
    	{
    		base.WriteSRM(ref address, ref data);
    		if (!BxROM)
    		{
    			switch (address)
    			{
    			case 32765:
    				Switch32KPRG(data, PRGArea.Area8000);
    				break;
    			case 32766:
    				Switch04KCHR(data, CHRArea.Area0000);
    				break;
    			case 32767:
    				Switch04KCHR(data, CHRArea.Area1000);
    				break;
    			}
    		}
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		if (BxROM)
    		{
    			ReadPRG(ref address, out writeData);
    			writeData &= data;
    			Switch32KPRG(writeData, PRGArea.Area8000);
    		}
    	}
    }
}
