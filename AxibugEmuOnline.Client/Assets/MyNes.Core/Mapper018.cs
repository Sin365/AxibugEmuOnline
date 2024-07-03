using System.IO;

namespace MyNes.Core
{
    [BoardInfo("Jaleco SS8806", 18)]
    internal class Mapper018 : Board
    {
    	private int[] prg_reg;

    	private int[] chr_reg;

    	private int irqRelaod;

    	private int irqCounter;

    	private bool irqEnable;

    	private int irqMask;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch08KPRG(PRG_ROM_08KB_Mask, PRGArea.AreaE000);
    		prg_reg = new int[3];
    		chr_reg = new int[8];
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		switch (address & 0xF003)
    		{
    		case 32768:
    			prg_reg[0] = (prg_reg[0] & 0xF0) | (data & 0xF);
    			Switch08KPRG(prg_reg[0], PRGArea.Area8000);
    			break;
    		case 32769:
    			prg_reg[0] = (prg_reg[0] & 0xF) | ((data & 0xF) << 4);
    			Switch08KPRG(prg_reg[0], PRGArea.Area8000);
    			break;
    		case 32770:
    			prg_reg[1] = (prg_reg[1] & 0xF0) | (data & 0xF);
    			Switch08KPRG(prg_reg[1], PRGArea.AreaA000);
    			break;
    		case 32771:
    			prg_reg[1] = (prg_reg[1] & 0xF) | ((data & 0xF) << 4);
    			Switch08KPRG(prg_reg[1], PRGArea.AreaA000);
    			break;
    		case 36864:
    			prg_reg[2] = (prg_reg[2] & 0xF0) | (data & 0xF);
    			Switch08KPRG(prg_reg[2], PRGArea.AreaC000);
    			break;
    		case 36865:
    			prg_reg[2] = (prg_reg[2] & 0xF) | ((data & 0xF) << 4);
    			Switch08KPRG(prg_reg[2], PRGArea.AreaC000);
    			break;
    		case 40960:
    			chr_reg[0] = (chr_reg[0] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_reg[0], CHRArea.Area0000);
    			break;
    		case 40961:
    			chr_reg[0] = (chr_reg[0] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_reg[0], CHRArea.Area0000);
    			break;
    		case 40962:
    			chr_reg[1] = (chr_reg[1] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_reg[1], CHRArea.Area0400);
    			break;
    		case 40963:
    			chr_reg[1] = (chr_reg[1] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_reg[1], CHRArea.Area0400);
    			break;
    		case 45056:
    			chr_reg[2] = (chr_reg[2] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_reg[2], CHRArea.Area0800);
    			break;
    		case 45057:
    			chr_reg[2] = (chr_reg[2] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_reg[2], CHRArea.Area0800);
    			break;
    		case 45058:
    			chr_reg[3] = (chr_reg[3] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_reg[3], CHRArea.Area0C00);
    			break;
    		case 45059:
    			chr_reg[3] = (chr_reg[3] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_reg[3], CHRArea.Area0C00);
    			break;
    		case 49152:
    			chr_reg[4] = (chr_reg[4] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_reg[4], CHRArea.Area1000);
    			break;
    		case 49153:
    			chr_reg[4] = (chr_reg[4] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_reg[4], CHRArea.Area1000);
    			break;
    		case 49154:
    			chr_reg[5] = (chr_reg[5] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_reg[5], CHRArea.Area1400);
    			break;
    		case 49155:
    			chr_reg[5] = (chr_reg[5] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_reg[5], CHRArea.Area1400);
    			break;
    		case 53248:
    			chr_reg[6] = (chr_reg[6] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_reg[6], CHRArea.Area1800);
    			break;
    		case 53249:
    			chr_reg[6] = (chr_reg[6] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_reg[6], CHRArea.Area1800);
    			break;
    		case 53250:
    			chr_reg[7] = (chr_reg[7] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_reg[7], CHRArea.Area1C00);
    			break;
    		case 53251:
    			chr_reg[7] = (chr_reg[7] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_reg[7], CHRArea.Area1C00);
    			break;
    		case 57344:
    			irqRelaod = (irqRelaod & 0xFFF0) | (data & 0xF);
    			break;
    		case 57345:
    			irqRelaod = (irqRelaod & 0xFF0F) | ((data & 0xF) << 4);
    			break;
    		case 57346:
    			irqRelaod = (irqRelaod & 0xF0FF) | ((data & 0xF) << 8);
    			break;
    		case 57347:
    			irqRelaod = (irqRelaod & 0xFFF) | ((data & 0xF) << 12);
    			break;
    		case 61440:
    			irqCounter = irqRelaod;
    			NesEmu.IRQFlags &= -9;
    			break;
    		case 61441:
    			irqEnable = (data & 1) == 1;
    			if ((data & 8) == 8)
    			{
    				irqMask = 15;
    			}
    			else if ((data & 4) == 4)
    			{
    				irqMask = 255;
    			}
    			else if ((data & 2) == 2)
    			{
    				irqMask = 4095;
    			}
    			else
    			{
    				irqMask = 65535;
    			}
    			NesEmu.IRQFlags &= -9;
    			break;
    		case 61442:
    			switch (data & 3)
    			{
    			case 0:
    				Switch01KNMTFromMirroring(Mirroring.Horz);
    				break;
    			case 1:
    				Switch01KNMTFromMirroring(Mirroring.Vert);
    				break;
    			case 2:
    				Switch01KNMTFromMirroring(Mirroring.OneScA);
    				break;
    			case 3:
    				Switch01KNMTFromMirroring(Mirroring.OneScB);
    				break;
    			}
    			break;
    		}
    	}

    	internal override void OnCPUClock()
    	{
    		if (irqEnable && (irqCounter & irqMask) > 0 && (--irqCounter & irqMask) == 0)
    		{
    			irqEnable = false;
    			NesEmu.IRQFlags |= 8;
    		}
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		for (int i = 0; i < prg_reg.Length; i++)
    		{
    			stream.Write(prg_reg[i]);
    		}
    		for (int j = 0; j < chr_reg.Length; j++)
    		{
    			stream.Write(chr_reg[j]);
    		}
    		stream.Write(irqRelaod);
    		stream.Write(irqCounter);
    		stream.Write(irqEnable);
    		stream.Write(irqMask);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		for (int i = 0; i < prg_reg.Length; i++)
    		{
    			prg_reg[i] = stream.ReadInt32();
    		}
    		for (int j = 0; j < chr_reg.Length; j++)
    		{
    			chr_reg[j] = stream.ReadInt32();
    		}
    		irqRelaod = stream.ReadInt32();
    		irqCounter = stream.ReadInt32();
    		irqEnable = stream.ReadBoolean();
    		irqMask = stream.ReadInt32();
    	}
    }
}
