using System.IO;

namespace MyNes.Core
{
    [BoardInfo("Taito TC0190/TC0350", 33)]
    [HassIssues]
    internal class Mapper033 : Board
    {
    	private bool MODE;

    	private bool irq_enabled;

    	private byte irq_counter;

    	private int old_irq_counter;

    	private byte irq_reload;

    	private bool irq_clear;

    	private bool mmc3_alt_behavior;

    	internal override string Issues => MNInterfaceLanguage.IssueMapper33;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch16KPRG(PRG_ROM_16KB_Mask, PRGArea.AreaC000);
    		MODE = true;
    		if (IsGameFoundOnDB)
    		{
    			foreach (string chip in base.Chips)
    			{
    				if (chip.Contains("TC0190"))
    				{
    					MODE = false;
    					ppuA12TogglesOnRaisingEdge = true;
    					enabled_ppuA12ToggleTimer = true;
    					break;
    				}
    			}
    		}
    		irq_enabled = false;
    		irq_counter = 0;
    		irq_reload = byte.MaxValue;
    		old_irq_counter = 0;
    		mmc3_alt_behavior = false;
    		irq_clear = false;
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		if (!MODE)
    		{
    			switch (address & 0xE003)
    			{
    			case 32768:
    				Switch08KPRG(data, PRGArea.Area8000);
    				break;
    			case 32769:
    				Switch08KPRG(data, PRGArea.AreaA000);
    				break;
    			case 32770:
    				Switch02KCHR(data, CHRArea.Area0000);
    				break;
    			case 32771:
    				Switch02KCHR(data, CHRArea.Area0800);
    				break;
    			case 40960:
    				Switch01KCHR(data, CHRArea.Area1000);
    				break;
    			case 40961:
    				Switch01KCHR(data, CHRArea.Area1400);
    				break;
    			case 40962:
    				Switch01KCHR(data, CHRArea.Area1800);
    				break;
    			case 40963:
    				Switch01KCHR(data, CHRArea.Area1C00);
    				break;
    			case 49152:
    				irq_reload = (byte)(data ^ 0xFFu);
    				break;
    			case 49153:
    				if (mmc3_alt_behavior)
    				{
    					irq_clear = true;
    				}
    				irq_counter = 0;
    				break;
    			case 49154:
    				irq_enabled = false;
    				NesEmu.IRQFlags &= -9;
    				break;
    			case 49155:
    				irq_enabled = true;
    				break;
    			case 57344:
    				Switch01KNMTFromMirroring(((data & 0x40) == 64) ? Mirroring.Horz : Mirroring.Vert);
    				break;
    			}
    		}
    		else
    		{
    			switch (address & 0xA003)
    			{
    			case 32768:
    				Switch01KNMTFromMirroring(((data & 0x40) == 64) ? Mirroring.Horz : Mirroring.Vert);
    				Switch08KPRG(data & 0x3F, PRGArea.Area8000);
    				break;
    			case 32769:
    				Switch08KPRG(data & 0x3F, PRGArea.AreaA000);
    				break;
    			case 32770:
    				Switch02KCHR(data, CHRArea.Area0000);
    				break;
    			case 32771:
    				Switch02KCHR(data, CHRArea.Area0800);
    				break;
    			case 40960:
    				Switch01KCHR(data, CHRArea.Area1000);
    				break;
    			case 40961:
    				Switch01KCHR(data, CHRArea.Area1400);
    				break;
    			case 40962:
    				Switch01KCHR(data, CHRArea.Area1800);
    				break;
    			case 40963:
    				Switch01KCHR(data, CHRArea.Area1C00);
    				break;
    			}
    		}
    	}

    	internal override void OnPPUA12RaisingEdge()
    	{
    		if (!MODE)
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
    			if ((!mmc3_alt_behavior || old_irq_counter != 0 || irq_clear) && irq_counter == 0 && irq_enabled)
    			{
    				NesEmu.IRQFlags |= 8;
    			}
    			irq_clear = false;
    		}
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
