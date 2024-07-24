using System.IO;

namespace MyNes.Core
{
    [BoardInfo("VRC7", 85)]
    [HassIssues]
    internal class Mapper085 : Board
    {
    	private int irq_reload;

    	private int irq_counter;

    	private int prescaler;

    	private bool irq_mode_cycle;

    	private bool irq_enable;

    	private bool irq_enable_on_ak;

    	internal override string Issues => MNInterfaceLanguage.IssueMapper85;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch08KPRG(PRG_ROM_08KB_Mask, PRGArea.AreaE000);
    		irq_reload = 0;
    		prescaler = 341;
    		irq_counter = 0;
    		irq_mode_cycle = false;
    		irq_enable = false;
    		irq_enable_on_ak = false;
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		switch (address)
    		{
    		case 32768:
    			Switch08KPRG(data, PRGArea.Area8000);
    			break;
    		case 32776:
    		case 32784:
    			Switch08KPRG(data, PRGArea.AreaA000);
    			break;
    		case 36864:
    			Switch08KPRG(data, PRGArea.AreaC000);
    			break;
    		case 40960:
    			Switch01KCHR(data, CHRArea.Area0000);
    			break;
    		case 40968:
    		case 40976:
    			Switch01KCHR(data, CHRArea.Area0400);
    			break;
    		case 45056:
    			Switch01KCHR(data, CHRArea.Area0800);
    			break;
    		case 45064:
    		case 45072:
    			Switch01KCHR(data, CHRArea.Area0C00);
    			break;
    		case 49152:
    			Switch01KCHR(data, CHRArea.Area1000);
    			break;
    		case 49160:
    		case 49168:
    			Switch01KCHR(data, CHRArea.Area1400);
    			break;
    		case 53248:
    			Switch01KCHR(data, CHRArea.Area1800);
    			break;
    		case 53256:
    		case 53264:
    			Switch01KCHR(data, CHRArea.Area1C00);
    			break;
    		case 57344:
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
    		case 57352:
    		case 57360:
    			irq_reload = data;
    			break;
    		case 61440:
    			irq_mode_cycle = (data & 4) == 4;
    			irq_enable = (data & 2) == 2;
    			irq_enable_on_ak = (data & 1) == 1;
    			if (irq_enable)
    			{
    				irq_counter = irq_reload;
    				prescaler = 341;
    			}
    			NesEmu.IRQFlags &= -9;
    			break;
    		case 61448:
    		case 61456:
    			NesEmu.IRQFlags &= -9;
    			irq_enable = irq_enable_on_ak;
    			break;
    		}
    	}

    	internal override void OnCPUClock()
    	{
    		if (!irq_enable)
    		{
    			return;
    		}
    		if (!irq_mode_cycle)
    		{
    			if (prescaler > 0)
    			{
    				prescaler -= 3;
    				return;
    			}
    			prescaler = 341;
    			irq_counter++;
    			if (irq_counter == 255)
    			{
    				NesEmu.IRQFlags |= 8;
    				irq_counter = irq_reload;
    			}
    		}
    		else
    		{
    			irq_counter++;
    			if (irq_counter == 255)
    			{
    				NesEmu.IRQFlags |= 8;
    				irq_counter = irq_reload;
    			}
    		}
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(prescaler);
    		stream.Write(irq_counter);
    		stream.Write(irq_mode_cycle);
    		stream.Write(irq_reload);
    		stream.Write(irq_enable);
    		stream.Write(irq_enable_on_ak);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		prescaler = stream.ReadInt32();
    		irq_counter = stream.ReadInt32();
    		irq_mode_cycle = stream.ReadBoolean();
    		irq_reload = stream.ReadInt32();
    		irq_enable = stream.ReadBoolean();
    		irq_enable_on_ak = stream.ReadBoolean();
    	}
    }
}
