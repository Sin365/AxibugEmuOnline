using System.IO;

namespace MyNes.Core
{
    [BoardInfo("C&E", 245, true, true)]
    [HassIssues]
    internal class Mapper245 : Board
    {
    	private bool flag_c;

    	private bool flag_p;

    	private int address_8001;

    	private int[] chr_reg;

    	private int[] prg_reg;

    	private int prg_or;

    	private bool irq_enabled;

    	private byte irq_counter;

    	private int old_irq_counter;

    	private byte irq_reload;

    	private bool irq_clear;

    	private bool mmc3_alt_behavior;

    	internal override string Issues => MNInterfaceLanguage.IssueMapper245;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		flag_c = (flag_p = false);
    		address_8001 = 0;
    		prg_reg = new int[4];
    		prg_or = 0;
    		prg_reg[0] = 0;
    		prg_reg[1] = 1;
    		prg_reg[2] = PRG_ROM_08KB_Mask - 1;
    		prg_reg[3] = PRG_ROM_08KB_Mask;
    		chr_reg = new int[6];
    		for (int i = 0; i < 6; i++)
    		{
    			chr_reg[i] = 0;
    		}
    		SetupPRG();
    		SetupCHR();
    		irq_enabled = false;
    		irq_counter = 0;
    		irq_reload = byte.MaxValue;
    		old_irq_counter = 0;
    		irq_clear = false;
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		switch (address & 0xE001)
    		{
    		case 32768:
    			address_8001 = data & 7;
    			flag_c = (data & 0x80) != 0;
    			flag_p = (data & 0x40) != 0;
    			SetupCHR();
    			SetupPRG();
    			break;
    		case 32769:
    			switch (address_8001)
    			{
    			case 0:
    			case 1:
    			case 2:
    			case 3:
    			case 4:
    			case 5:
    				chr_reg[address_8001] = data;
    				SetupCHR();
    				break;
    			case 6:
    				prg_reg[0] = data & 0x3F;
    				SetupPRG();
    				break;
    			case 7:
    				prg_reg[1] = data & 0x3F;
    				SetupPRG();
    				break;
    			}
    			break;
    		case 40960:
    			if (NMT_DEFAULT_MIRROR != Mirroring.Full)
    			{
    				Switch01KNMTFromMirroring(((data & 1) == 1) ? Mirroring.Horz : Mirroring.Vert);
    			}
    			break;
    		case 40961:
    			TogglePRGRAMEnable((data & 0x80) != 0);
    			TogglePRGRAMWritableEnable((data & 0x40) == 0);
    			break;
    		case 49152:
    			irq_reload = data;
    			break;
    		case 49153:
    			if (mmc3_alt_behavior)
    			{
    				irq_clear = true;
    			}
    			irq_counter = 0;
    			break;
    		case 57344:
    			irq_enabled = false;
    			NesEmu.IRQFlags &= -9;
    			break;
    		case 57345:
    			irq_enabled = true;
    			break;
    		}
    	}

    	private void SetupCHR()
    	{
    		if (CHR_ROM_01KB_Count > 0)
    		{
    			if (!flag_c)
    			{
    				Switch02KCHR(chr_reg[0] >> 1, CHRArea.Area0000);
    				Switch02KCHR(chr_reg[1] >> 1, CHRArea.Area0800);
    				Switch01KCHR(chr_reg[2], CHRArea.Area1000);
    				Switch01KCHR(chr_reg[3], CHRArea.Area1400);
    				Switch01KCHR(chr_reg[4], CHRArea.Area1800);
    				Switch01KCHR(chr_reg[5], CHRArea.Area1C00);
    			}
    			else
    			{
    				Switch02KCHR(chr_reg[0] >> 1, CHRArea.Area1000);
    				Switch02KCHR(chr_reg[1] >> 1, CHRArea.Area1800);
    				Switch01KCHR(chr_reg[2], CHRArea.Area0000);
    				Switch01KCHR(chr_reg[3], CHRArea.Area0400);
    				Switch01KCHR(chr_reg[4], CHRArea.Area0800);
    				Switch01KCHR(chr_reg[5], CHRArea.Area0C00);
    			}
    		}
    		else if (!flag_c)
    		{
    			Switch04KCHR(0, CHRArea.Area0000);
    			Switch04KCHR(1, CHRArea.Area1000);
    		}
    		else
    		{
    			Switch04KCHR(1, CHRArea.Area0000);
    			Switch04KCHR(0, CHRArea.Area1000);
    		}
    	}

    	private void SetupPRG()
    	{
    		prg_or = (chr_reg[0] & 2) << 5;
    		Switch08KPRG(prg_reg[flag_p ? 2 : 0] | prg_or, PRGArea.Area8000);
    		Switch08KPRG(prg_reg[1] | prg_or, PRGArea.AreaA000);
    		Switch08KPRG(prg_reg[(!flag_p) ? 2 : 0] | prg_or, PRGArea.AreaC000);
    		Switch08KPRG(prg_reg[3] | prg_or, PRGArea.AreaE000);
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
    		if ((!mmc3_alt_behavior || old_irq_counter != 0 || irq_clear) && irq_counter == 0 && irq_enabled)
    		{
    			NesEmu.IRQFlags |= 8;
    		}
    		irq_clear = false;
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(flag_c);
    		stream.Write(flag_p);
    		stream.Write(address_8001);
    		for (int i = 0; i < chr_reg.Length; i++)
    		{
    			stream.Write(chr_reg[i]);
    		}
    		for (int j = 0; j < prg_reg.Length; j++)
    		{
    			stream.Write(prg_reg[j]);
    		}
    		stream.Write(irq_enabled);
    		stream.Write(irq_counter);
    		stream.Write(old_irq_counter);
    		stream.Write(irq_reload);
    		stream.Write(irq_clear);
    		stream.Write(prg_or);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		flag_c = stream.ReadBoolean();
    		flag_p = stream.ReadBoolean();
    		address_8001 = stream.ReadInt32();
    		for (int i = 0; i < chr_reg.Length; i++)
    		{
    			chr_reg[i] = stream.ReadInt32();
    		}
    		for (int j = 0; j < prg_reg.Length; j++)
    		{
    			prg_reg[j] = stream.ReadInt32();
    		}
    		irq_enabled = stream.ReadBoolean();
    		irq_counter = stream.ReadByte();
    		old_irq_counter = stream.ReadInt32();
    		irq_reload = stream.ReadByte();
    		irq_clear = stream.ReadBoolean();
    		prg_or = stream.ReadInt32();
    	}
    }
}
