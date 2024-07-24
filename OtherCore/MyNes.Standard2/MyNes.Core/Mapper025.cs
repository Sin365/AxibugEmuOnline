using System.IO;

namespace MyNes.Core
{
    [BoardInfo("VRC4", 25)]
    internal class Mapper025 : Board
    {
    	private bool prg_mode;

    	private byte prg_reg0;

    	private int[] chr_Reg;

    	private int irq_reload;

    	private int irq_counter;

    	private int prescaler;

    	private bool irq_mode_cycle;

    	private bool irq_enable;

    	private bool irq_enable_on_ak;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch16KPRG(PRG_ROM_16KB_Mask, PRGArea.AreaC000);
    		prescaler = 341;
    		chr_Reg = new int[8];
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		switch (address)
    		{
    		case 32768:
    		case 32769:
    		case 32770:
    		case 32771:
    		case 32772:
    		case 32776:
    		case 32780:
    			prg_reg0 = data;
    			Switch08KPRG(prg_mode ? (PRG_ROM_08KB_Mask - 1) : (prg_reg0 & 0x1F), PRGArea.Area8000);
    			Switch08KPRG(prg_mode ? (prg_reg0 & 0x1F) : (PRG_ROM_08KB_Mask - 1), PRGArea.AreaC000);
    			break;
    		case 36864:
    		case 36866:
    		case 36872:
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
    		case 36865:
    		case 36867:
    		case 36868:
    		case 36876:
    			prg_mode = (data & 2) == 2;
    			Switch08KPRG(prg_mode ? (PRG_ROM_08KB_Mask - 1) : (prg_reg0 & 0x1F), PRGArea.Area8000);
    			Switch08KPRG(prg_mode ? (prg_reg0 & 0x1F) : (PRG_ROM_08KB_Mask - 1), PRGArea.AreaC000);
    			break;
    		case 40960:
    		case 40961:
    		case 40962:
    		case 40963:
    		case 40964:
    		case 40968:
    		case 40972:
    			Switch08KPRG(data & 0x1F, PRGArea.AreaA000);
    			break;
    		case 45056:
    			chr_Reg[0] = (chr_Reg[0] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_Reg[0], CHRArea.Area0000);
    			break;
    		case 45058:
    		case 45064:
    			chr_Reg[0] = (chr_Reg[0] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_Reg[0], CHRArea.Area0000);
    			break;
    		case 45057:
    		case 45060:
    			chr_Reg[1] = (chr_Reg[1] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_Reg[1], CHRArea.Area0400);
    			break;
    		case 45059:
    		case 45068:
    			chr_Reg[1] = (chr_Reg[1] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_Reg[1], CHRArea.Area0400);
    			break;
    		case 49152:
    			chr_Reg[2] = (chr_Reg[2] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_Reg[2], CHRArea.Area0800);
    			break;
    		case 49154:
    		case 49160:
    			chr_Reg[2] = (chr_Reg[2] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_Reg[2], CHRArea.Area0800);
    			break;
    		case 49153:
    		case 49156:
    			chr_Reg[3] = (chr_Reg[3] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_Reg[3], CHRArea.Area0C00);
    			break;
    		case 49155:
    		case 49164:
    			chr_Reg[3] = (chr_Reg[3] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_Reg[3], CHRArea.Area0C00);
    			break;
    		case 53248:
    			chr_Reg[4] = (chr_Reg[4] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_Reg[4], CHRArea.Area1000);
    			break;
    		case 53250:
    		case 53256:
    			chr_Reg[4] = (chr_Reg[4] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_Reg[4], CHRArea.Area1000);
    			break;
    		case 53249:
    		case 53252:
    			chr_Reg[5] = (chr_Reg[5] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_Reg[5], CHRArea.Area1400);
    			break;
    		case 53251:
    		case 53260:
    			chr_Reg[5] = (chr_Reg[5] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_Reg[5], CHRArea.Area1400);
    			break;
    		case 57344:
    			chr_Reg[6] = (chr_Reg[6] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_Reg[6], CHRArea.Area1800);
    			break;
    		case 57346:
    		case 57352:
    			chr_Reg[6] = (chr_Reg[6] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_Reg[6], CHRArea.Area1800);
    			break;
    		case 57345:
    		case 57348:
    			chr_Reg[7] = (chr_Reg[7] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_Reg[7], CHRArea.Area1C00);
    			break;
    		case 57347:
    		case 57356:
    			chr_Reg[7] = (chr_Reg[7] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_Reg[7], CHRArea.Area1C00);
    			break;
    		case 61440:
    			irq_reload = (irq_reload & 0xF0) | (data & 0xF);
    			break;
    		case 61442:
    		case 61448:
    			irq_reload = (irq_reload & 0xF) | ((data & 0xF) << 4);
    			break;
    		case 61441:
    		case 61444:
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
    		case 61443:
    		case 61452:
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
    		stream.Write(prg_mode);
    		stream.Write(prg_reg0);
    		for (int i = 0; i < chr_Reg.Length; i++)
    		{
    			stream.Write(chr_Reg[i]);
    		}
    		stream.Write(irq_reload);
    		stream.Write(irq_counter);
    		stream.Write(prescaler);
    		stream.Write(irq_mode_cycle);
    		stream.Write(irq_enable);
    		stream.Write(irq_enable_on_ak);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		prg_mode = stream.ReadBoolean();
    		prg_reg0 = stream.ReadByte();
    		for (int i = 0; i < chr_Reg.Length; i++)
    		{
    			chr_Reg[i] = stream.ReadInt32();
    		}
    		irq_reload = stream.ReadInt32();
    		irq_counter = stream.ReadInt32();
    		prescaler = stream.ReadInt32();
    		irq_mode_cycle = stream.ReadBoolean();
    		irq_enable = stream.ReadBoolean();
    		irq_enable_on_ak = stream.ReadBoolean();
    	}
    }
}
