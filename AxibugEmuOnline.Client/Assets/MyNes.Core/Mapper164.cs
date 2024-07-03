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
    		if (address >= 20480 && (address & 0xF000) == 20480)
    		{
    			Switch32KPRG(data, PRGArea.Area8000);
    		}
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		if ((address & 0xF000) == 53248)
    		{
    			Switch32KPRG(data, PRGArea.Area8000);
    		}
    	}
    }
}
