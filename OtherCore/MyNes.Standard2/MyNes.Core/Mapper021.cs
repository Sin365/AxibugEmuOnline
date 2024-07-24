using System.IO;

namespace MyNes.Core
{
    [BoardInfo("VRC4", 21)]
    internal class Mapper021 : Board
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
    		case 32770:
    		case 32772:
    		case 32774:
    		case 32832:
    		case 32896:
    		case 32960:
    			prg_reg0 = data;
    			Switch08KPRG(prg_mode ? (PRG_ROM_08KB_Mask - 1) : (prg_reg0 & 0x1F), PRGArea.Area8000);
    			Switch08KPRG(prg_mode ? (prg_reg0 & 0x1F) : (PRG_ROM_08KB_Mask - 1), PRGArea.AreaC000);
    			break;
    		case 36864:
    		case 36866:
    		case 36928:
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
    		case 36868:
    		case 36870:
    		case 36992:
    		case 37056:
    			prg_mode = (data & 2) == 2;
    			Switch08KPRG(prg_mode ? (PRG_ROM_08KB_Mask - 1) : (prg_reg0 & 0x1F), PRGArea.Area8000);
    			Switch08KPRG(prg_mode ? (prg_reg0 & 0x1F) : (PRG_ROM_08KB_Mask - 1), PRGArea.AreaC000);
    			break;
    		case 40960:
    		case 40962:
    		case 40964:
    		case 40966:
    		case 41024:
    		case 41088:
    		case 41152:
    			Switch08KPRG(data & 0x1F, PRGArea.AreaA000);
    			break;
    		case 45056:
    			chr_Reg[0] = (chr_Reg[0] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_Reg[0], CHRArea.Area0000);
    			break;
    		case 45058:
    		case 45120:
    			chr_Reg[0] = (chr_Reg[0] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_Reg[0], CHRArea.Area0000);
    			break;
    		case 45060:
    		case 45184:
    			chr_Reg[1] = (chr_Reg[1] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_Reg[1], CHRArea.Area0400);
    			break;
    		case 45062:
    		case 45248:
    			chr_Reg[1] = (chr_Reg[1] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_Reg[1], CHRArea.Area0400);
    			break;
    		case 49152:
    			chr_Reg[2] = (chr_Reg[2] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_Reg[2], CHRArea.Area0800);
    			break;
    		case 49154:
    		case 49216:
    			chr_Reg[2] = (chr_Reg[2] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_Reg[2], CHRArea.Area0800);
    			break;
    		case 49156:
    		case 49280:
    			chr_Reg[3] = (chr_Reg[3] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_Reg[3], CHRArea.Area0C00);
    			break;
    		case 49158:
    		case 49344:
    			chr_Reg[3] = (chr_Reg[3] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_Reg[3], CHRArea.Area0C00);
    			break;
    		case 53248:
    			chr_Reg[4] = (chr_Reg[4] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_Reg[4], CHRArea.Area1000);
    			break;
    		case 53250:
    		case 53312:
    			chr_Reg[4] = (chr_Reg[4] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_Reg[4], CHRArea.Area1000);
    			break;
    		case 53252:
    		case 53376:
    			chr_Reg[5] = (chr_Reg[5] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_Reg[5], CHRArea.Area1400);
    			break;
    		case 53254:
    		case 53440:
    			chr_Reg[5] = (chr_Reg[5] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_Reg[5], CHRArea.Area1400);
    			break;
    		case 57344:
    			chr_Reg[6] = (chr_Reg[6] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_Reg[6], CHRArea.Area1800);
    			break;
    		case 57346:
    		case 57408:
    			chr_Reg[6] = (chr_Reg[6] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_Reg[6], CHRArea.Area1800);
    			break;
    		case 57348:
    		case 57472:
    			chr_Reg[7] = (chr_Reg[7] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_Reg[7], CHRArea.Area1C00);
    			break;
    		case 57350:
    		case 57536:
    			chr_Reg[7] = (chr_Reg[7] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_Reg[7], CHRArea.Area1C00);
    			break;
    		case 61440:
    			irq_reload = (irq_reload & 0xF0) | (data & 0xF);
    			break;
    		case 61442:
    		case 61504:
    			irq_reload = (irq_reload & 0xF) | ((data & 0xF) << 4);
    			break;
    		case 61444:
    		case 61568:
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
    		case 61446:
    		case 61632:
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
