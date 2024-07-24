namespace MyNes.Core
{
    [BoardInfo("Irem H-3001", 65)]
    internal class Mapper065 : Board
    {
    	private bool irq_enable;

    	private int irq_reload;

    	private int irq_counter;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch08KPRG(0, PRGArea.Area8000);
    		Switch08KPRG(1, PRGArea.AreaA000);
    		Switch08KPRG(254, PRGArea.AreaC000);
    		Switch08KPRG(PRG_ROM_08KB_Mask, PRGArea.AreaE000);
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		switch (address)
    		{
    		case 32768:
    			Switch08KPRG(data, PRGArea.Area8000);
    			break;
    		case 36865:
    			Switch01KNMTFromMirroring(((data & 0x80) == 128) ? Mirroring.Horz : Mirroring.Vert);
    			break;
    		case 36867:
    			irq_enable = (data & 0x80) == 128;
    			NesEmu.IRQFlags &= -9;
    			break;
    		case 36868:
    			irq_counter = irq_reload;
    			NesEmu.IRQFlags &= -9;
    			break;
    		case 36869:
    			irq_reload = (irq_reload & 0xFF) | (data << 8);
    			break;
    		case 36870:
    			irq_reload = (irq_reload & 0xFF00) | data;
    			break;
    		case 40960:
    			Switch08KPRG(data, PRGArea.AreaA000);
    			break;
    		case 49152:
    			Switch08KPRG(data, PRGArea.AreaC000);
    			break;
    		case 45056:
    			Switch01KCHR(data, CHRArea.Area0000);
    			break;
    		case 45057:
    			Switch01KCHR(data, CHRArea.Area0400);
    			break;
    		case 45058:
    			Switch01KCHR(data, CHRArea.Area0800);
    			break;
    		case 45059:
    			Switch01KCHR(data, CHRArea.Area0C00);
    			break;
    		case 45060:
    			Switch01KCHR(data, CHRArea.Area1000);
    			break;
    		case 45061:
    			Switch01KCHR(data, CHRArea.Area1400);
    			break;
    		case 45062:
    			Switch01KCHR(data, CHRArea.Area1800);
    			break;
    		case 45063:
    			Switch01KCHR(data, CHRArea.Area1C00);
    			break;
    		}
    	}

    	internal override void OnCPUClock()
    	{
    		if (irq_enable)
    		{
    			if (irq_counter > 0)
    			{
    				irq_counter--;
    			}
    			else if (irq_counter == 0)
    			{
    				irq_counter = -1;
    				NesEmu.IRQFlags |= 8;
    			}
    		}
    	}
    }
}
