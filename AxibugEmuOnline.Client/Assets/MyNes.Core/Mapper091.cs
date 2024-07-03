using System.IO;

namespace MyNes.Core
{
    [BoardInfo("HK-SF3", 91, true, true)]
    internal class Mapper091 : Board
    {
    	private bool irq_enabled;

    	private byte irq_counter;

    	private int old_irq_counter;

    	private byte irq_reload;

    	private bool irq_clear;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch16KPRG(PRG_ROM_16KB_Mask, PRGArea.AreaC000);
    	}

    	internal override void WriteSRM(ref ushort address, ref byte data)
    	{
    		switch (address & 0x7003)
    		{
    		case 24576:
    			Switch02KCHR(data, CHRArea.Area0000);
    			break;
    		case 24577:
    			Switch02KCHR(data, CHRArea.Area0800);
    			break;
    		case 24578:
    			Switch02KCHR(data, CHRArea.Area1000);
    			break;
    		case 24579:
    			Switch02KCHR(data, CHRArea.Area1800);
    			break;
    		case 28672:
    			Switch08KPRG(data & 0xF, PRGArea.Area8000);
    			break;
    		case 28673:
    			Switch08KPRG(data & 0xF, PRGArea.AreaA000);
    			break;
    		case 28674:
    			irq_enabled = false;
    			NesEmu.IRQFlags &= -9;
    			break;
    		case 28675:
    			irq_enabled = true;
    			irq_reload = 7;
    			irq_counter = 0;
    			break;
    		}
    	}

    	internal override void OnPPUA12RaisingEdge()
    	{
    		old_irq_counter = irq_counter;
    		if (irq_counter == 0 || irq_clear)
    		{
    			irq_counter = irq_reload;
    		}
    		else
    		{
    			irq_counter--;
    		}
    		if ((old_irq_counter != 0 || irq_clear) && irq_counter == 0 && irq_enabled)
    		{
    			NesEmu.IRQFlags |= 8;
    		}
    		irq_clear = false;
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(irq_enabled);
    		stream.Write(irq_counter);
    		stream.Write(old_irq_counter);
    		stream.Write(irq_reload);
    		stream.Write(irq_clear);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		irq_enabled = stream.ReadBoolean();
    		irq_counter = stream.ReadByte();
    		old_irq_counter = stream.ReadInt32();
    		irq_reload = stream.ReadByte();
    		irq_clear = stream.ReadBoolean();
    	}
    }
}
