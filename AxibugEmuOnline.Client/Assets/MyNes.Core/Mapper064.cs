using System.IO;

namespace MyNes.Core
{
    [BoardInfo("Tengen RAMBO-1", 64, true, true)]
    internal class Mapper064 : Board
    {
    	private bool flag_c;

    	private bool flag_p;

    	private bool flag_k;

    	private int address_8001;

    	private int[] chr_reg;

    	private int[] prg_reg;

    	private bool irq_enabled;

    	private byte irq_counter;

    	private byte irq_reload;

    	private bool irq_mode;

    	private bool irq_clear;

    	private int irq_prescaler;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		flag_c = (flag_p = (flag_k = false));
    		address_8001 = 0;
    		prg_reg = new int[3];
    		prg_reg[0] = 0;
    		prg_reg[1] = 1;
    		prg_reg[2] = 2;
    		Switch08KPRG(PRG_ROM_08KB_Mask, PRGArea.AreaE000);
    		SetupPRG();
    		chr_reg = new int[8];
    		for (int i = 0; i < 8; i++)
    		{
    			chr_reg[i] = i;
    		}
    		SetupCHR();
    		irq_enabled = false;
    		irq_counter = 0;
    		irq_prescaler = 0;
    		irq_mode = false;
    		irq_reload = byte.MaxValue;
    		irq_clear = false;
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		switch (address & 0xE001)
    		{
    		case 32768:
    			address_8001 = data & 0xF;
    			flag_c = (data & 0x80) != 0;
    			flag_p = (data & 0x40) != 0;
    			flag_k = (data & 0x20) != 0;
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
    			case 7:
    				prg_reg[address_8001 - 6] = data;
    				SetupPRG();
    				break;
    			case 8:
    				chr_reg[6] = data;
    				SetupCHR();
    				break;
    			case 9:
    				chr_reg[7] = data;
    				SetupCHR();
    				break;
    			case 15:
    				prg_reg[2] = data;
    				SetupPRG();
    				break;
    			case 10:
    			case 11:
    			case 12:
    			case 13:
    			case 14:
    				break;
    			}
    			break;
    		case 40960:
    			if (NMT_DEFAULT_MIRROR != Mirroring.Full)
    			{
    				Switch01KNMTFromMirroring(((data & 1) == 1) ? Mirroring.Horz : Mirroring.Vert);
    			}
    			break;
    		case 49152:
    			irq_reload = data;
    			break;
    		case 49153:
    			irq_mode = (data & 1) == 1;
    			irq_clear = true;
    			irq_prescaler = 0;
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
    		if (!flag_c)
    		{
    			if (!flag_k)
    			{
    				Switch02KCHR(chr_reg[0] >> 1, CHRArea.Area0000);
    				Switch02KCHR(chr_reg[1] >> 1, CHRArea.Area0800);
    			}
    			else
    			{
    				Switch01KCHR(chr_reg[0], CHRArea.Area0000);
    				Switch01KCHR(chr_reg[6], CHRArea.Area0400);
    				Switch01KCHR(chr_reg[1], CHRArea.Area0800);
    				Switch01KCHR(chr_reg[7], CHRArea.Area0C00);
    			}
    			Switch01KCHR(chr_reg[2], CHRArea.Area1000);
    			Switch01KCHR(chr_reg[3], CHRArea.Area1400);
    			Switch01KCHR(chr_reg[4], CHRArea.Area1800);
    			Switch01KCHR(chr_reg[5], CHRArea.Area1C00);
    		}
    		else
    		{
    			if (!flag_k)
    			{
    				Switch02KCHR(chr_reg[0] >> 1, CHRArea.Area1000);
    				Switch02KCHR(chr_reg[1] >> 1, CHRArea.Area1800);
    			}
    			else
    			{
    				Switch01KCHR(chr_reg[0], CHRArea.Area1000);
    				Switch01KCHR(chr_reg[6], CHRArea.Area1400);
    				Switch01KCHR(chr_reg[1], CHRArea.Area1800);
    				Switch01KCHR(chr_reg[7], CHRArea.Area1C00);
    			}
    			Switch01KCHR(chr_reg[2], CHRArea.Area0000);
    			Switch01KCHR(chr_reg[3], CHRArea.Area0400);
    			Switch01KCHR(chr_reg[4], CHRArea.Area0800);
    			Switch01KCHR(chr_reg[5], CHRArea.Area0C00);
    		}
    	}

    	private void SetupPRG()
    	{
    		Switch08KPRG(prg_reg[flag_p ? 2 : 0], PRGArea.Area8000);
    		Switch08KPRG(prg_reg[(!flag_p) ? 1u : 0u], PRGArea.AreaA000);
    		Switch08KPRG(prg_reg[flag_p ? 1 : 2], PRGArea.AreaC000);
    	}

    	internal override void OnPPUA12RaisingEdge()
    	{
    		ClockIRQ();
    	}

    	internal override void OnCPUClock()
    	{
    		if (irq_mode)
    		{
    			irq_prescaler++;
    			if (irq_prescaler == 4)
    			{
    				irq_prescaler = 0;
    				ClockIRQ();
    			}
    		}
    	}

    	private void ClockIRQ()
    	{
    		if (irq_clear)
    		{
    			irq_counter = (byte)(irq_reload + 1);
    			irq_clear = false;
    		}
    		else if (irq_counter == 0)
    		{
    			irq_counter = irq_reload;
    		}
    		else if (--irq_counter == 0 && irq_enabled)
    		{
    			NesEmu.IRQFlags |= 8;
    		}
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
    		stream.Write(irq_reload);
    		stream.Write(irq_clear);
    		stream.Write(irq_prescaler);
    		stream.Write(irq_mode);
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
    		irq_reload = stream.ReadByte();
    		irq_clear = stream.ReadBoolean();
    		irq_prescaler = stream.ReadInt32();
    		irq_mode = stream.ReadBoolean();
    	}
    }
}
