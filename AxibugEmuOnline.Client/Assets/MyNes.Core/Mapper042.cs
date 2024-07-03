using System.IO;

namespace MyNes.Core
{
    [BoardInfo("Mario Baby", 42)]
    internal class Mapper042 : Board
    {
    	private int SRAM_PRG_Page;

    	private bool irqEnable;

    	private int irqCounter;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch32KPRG(PRG_ROM_32KB_Mask, PRGArea.Area8000);
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		if (address == 32768)
    		{
    			Switch08KCHR(data);
    			return;
    		}
    		if (address == 61440)
    		{
    			SRAM_PRG_Page = data << 13;
    			return;
    		}
    		switch (address & 0xE003)
    		{
    		case 57344:
    			Switch08KPRG(data, PRGArea.Area6000);
    			break;
    		case 57345:
    			if ((data & 8) == 8)
    			{
    				Switch01KNMTFromMirroring(Mirroring.Horz);
    			}
    			else
    			{
    				Switch01KNMTFromMirroring(Mirroring.Vert);
    			}
    			break;
    		case 57346:
    			irqEnable = (data & 2) == 2;
    			if (!irqEnable)
    			{
    				irqCounter = 0;
    			}
    			NesEmu.IRQFlags &= -9;
    			break;
    		}
    	}

    	internal override void OnCPUClock()
    	{
    		if (!irqEnable)
    		{
    			return;
    		}
    		int num = irqCounter++;
    		if ((irqCounter & 0x6000) != (num & 0x6000))
    		{
    			if ((irqCounter & 0x6000) == 24576)
    			{
    				NesEmu.IRQFlags |= 8;
    			}
    			else
    			{
    				NesEmu.IRQFlags &= -9;
    			}
    		}
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(SRAM_PRG_Page);
    		stream.Write(irqEnable);
    		stream.Write(irqCounter);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		SRAM_PRG_Page = stream.ReadInt32();
    		irqEnable = stream.ReadBoolean();
    		irqCounter = stream.ReadInt32();
    	}
    }
}
