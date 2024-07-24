using System.IO;

namespace MyNes.Core
{
    [BoardInfo("Sunsoft 3", 67)]
    internal class Mapper067 : Board
    {
    	private bool irq_enabled;

    	private int irq_counter;

    	private bool odd;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch16KPRG(PRG_ROM_16KB_Mask, PRGArea.AreaC000);
    		irq_enabled = false;
    		irq_counter = 65535;
    		odd = false;
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		switch (address & 0xF800)
    		{
    		case 34816:
    			Switch02KCHR(data, CHRArea.Area0000);
    			break;
    		case 38912:
    			Switch02KCHR(data, CHRArea.Area0800);
    			break;
    		case 43008:
    			Switch02KCHR(data, CHRArea.Area1000);
    			break;
    		case 47104:
    			Switch02KCHR(data, CHRArea.Area1800);
    			break;
    		case 51200:
    			if (!odd)
    			{
    				irq_counter = (irq_counter & 0xFF) | (data << 8);
    			}
    			else
    			{
    				irq_counter = (irq_counter & 0xFF00) | data;
    			}
    			odd = !odd;
    			break;
    		case 55296:
    			irq_enabled = (data & 0x10) == 16;
    			odd = false;
    			NesEmu.IRQFlags &= -9;
    			break;
    		case 59392:
    			switch (data & 3)
    			{
    			case 0:
    				Switch01KNMTFromMirroring(Mirroring.Vert);
    				break;
    			case 1:
    				Switch01KNMTFromMirroring(Mirroring.Horz);
    				break;
    			case 2:
    				Switch01KNMTFromMirroring(Mirroring.OneScA);
    				break;
    			case 3:
    				Switch01KNMTFromMirroring(Mirroring.OneScB);
    				break;
    			}
    			break;
    		case 63488:
    			Switch16KPRG(data, PRGArea.Area8000);
    			break;
    		}
    	}

    	internal override void OnCPUClock()
    	{
    		if (irq_enabled)
    		{
    			irq_counter--;
    			if (irq_counter == 0)
    			{
    				irq_counter = 65535;
    				NesEmu.IRQFlags |= 8;
    				irq_enabled = false;
    			}
    		}
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(irq_enabled);
    		stream.Write(irq_counter);
    		stream.Write(odd);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		irq_enabled = stream.ReadBoolean();
    		irq_counter = stream.ReadInt32();
    		odd = stream.ReadBoolean();
    	}
    }
}
