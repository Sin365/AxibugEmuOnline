namespace MyNes.Core
{
    [BoardInfo("Unknown", 164)]
    internal class Mapper164 : Board
    {
    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch32KPRG(255, PRGArea.Area8000);
    	}

    	internal override void WriteEX(ref ushort address, ref byte data)
    	{
    		if (address >= 20480)
    		{
    			int num = address & 0xF000;
    			int num2 = num;
    			if (num2 == 20480)
    			{
    				Switch32KPRG(data, PRGArea.Area8000);
    			}
    		}
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		int num = address & 0xF000;
    		int num2 = num;
    		if (num2 == 53248)
    		{
    			Switch32KPRG(data, PRGArea.Area8000);
    		}
    	}
    }
}
