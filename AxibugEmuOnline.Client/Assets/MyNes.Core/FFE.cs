using System.IO;

namespace MyNes.Core
{
    internal abstract class FFE : Board
    {
    	protected bool irqEnable;

    	protected int irqCounter;

    	internal override void WriteEX(ref ushort address, ref byte data)
    	{
    		switch (address)
    		{
    		case 17665:
    			irqEnable = false;
    			NesEmu.IRQFlags &= -9;
    			break;
    		case 17666:
    			irqCounter = (irqCounter & 0xFF00) | data;
    			break;
    		case 17667:
    			irqEnable = true;
    			irqCounter = (irqCounter & 0xFF) | (data << 8);
    			break;
    		}
    	}

    	internal override void OnCPUClock()
    	{
    		if (irqEnable)
    		{
    			irqCounter++;
    			if (irqCounter >= 65535)
    			{
    				irqCounter = 0;
    				NesEmu.IRQFlags |= 8;
    			}
    		}
    	}

    	internal override void WriteStateData(ref BinaryWriter bin)
    	{
    		base.WriteStateData(ref bin);
    		bin.Write(irqEnable);
    		bin.Write(irqCounter);
    	}

    	internal override void ReadStateData(ref BinaryReader bin)
    	{
    		base.ReadStateData(ref bin);
    		irqEnable = bin.ReadBoolean();
    		irqCounter = bin.ReadInt32();
    	}
    }
}
