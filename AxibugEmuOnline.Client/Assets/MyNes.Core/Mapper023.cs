using System.IO;

namespace MyNes.Core
{
    [BoardInfo("VRC2", 23)]
    internal class Mapper023 : Board
    {
    	private int[] chr_Reg;

    	private byte security;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch16KPRG(PRG_ROM_16KB_Mask, PRGArea.AreaC000);
    		chr_Reg = new int[8];
    		security = 0;
    	}

    	internal override void WriteSRM(ref ushort address, ref byte data)
    	{
    		if (address == 24576)
    		{
    			security = (byte)(data & 1u);
    		}
    	}

    	internal override void ReadSRM(ref ushort address, out byte data)
    	{
    		if (address == 24576)
    		{
    			data = security;
    		}
    		else
    		{
    			data = 0;
    		}
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		switch (address)
    		{
    		case 32768:
    		case 32769:
    		case 32770:
    		case 32771:
    			Switch08KPRG(data & 0xF, PRGArea.Area8000);
    			break;
    		case 36864:
    		case 36865:
    		case 36866:
    		case 36867:
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
    		case 40960:
    		case 40961:
    		case 40962:
    		case 40963:
    			Switch08KPRG(data & 0xF, PRGArea.AreaA000);
    			break;
    		case 45056:
    			chr_Reg[0] = (chr_Reg[0] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_Reg[0], CHRArea.Area0000);
    			break;
    		case 45057:
    			chr_Reg[0] = (chr_Reg[0] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_Reg[0], CHRArea.Area0000);
    			break;
    		case 45058:
    			chr_Reg[1] = (chr_Reg[1] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_Reg[1], CHRArea.Area0400);
    			break;
    		case 45059:
    			chr_Reg[1] = (chr_Reg[1] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_Reg[1], CHRArea.Area0400);
    			break;
    		case 49152:
    			chr_Reg[2] = (chr_Reg[2] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_Reg[2], CHRArea.Area0800);
    			break;
    		case 49153:
    			chr_Reg[2] = (chr_Reg[2] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_Reg[2], CHRArea.Area0800);
    			break;
    		case 49154:
    			chr_Reg[3] = (chr_Reg[3] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_Reg[3], CHRArea.Area0C00);
    			break;
    		case 49155:
    			chr_Reg[3] = (chr_Reg[3] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_Reg[3], CHRArea.Area0C00);
    			break;
    		case 53248:
    			chr_Reg[4] = (chr_Reg[4] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_Reg[4], CHRArea.Area1000);
    			break;
    		case 53249:
    			chr_Reg[4] = (chr_Reg[4] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_Reg[4], CHRArea.Area1000);
    			break;
    		case 53250:
    			chr_Reg[5] = (chr_Reg[5] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_Reg[5], CHRArea.Area1400);
    			break;
    		case 53251:
    			chr_Reg[5] = (chr_Reg[5] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_Reg[5], CHRArea.Area1400);
    			break;
    		case 57344:
    			chr_Reg[6] = (chr_Reg[6] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_Reg[6], CHRArea.Area1800);
    			break;
    		case 57345:
    			chr_Reg[6] = (chr_Reg[6] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_Reg[6], CHRArea.Area1800);
    			break;
    		case 57346:
    			chr_Reg[7] = (chr_Reg[7] & 0xF0) | (data & 0xF);
    			Switch01KCHR(chr_Reg[7], CHRArea.Area1C00);
    			break;
    		case 57347:
    			chr_Reg[7] = (chr_Reg[7] & 0xF) | ((data & 0xF) << 4);
    			Switch01KCHR(chr_Reg[7], CHRArea.Area1C00);
    			break;
    		}
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		for (int i = 0; i < chr_Reg.Length; i++)
    		{
    			stream.Write(chr_Reg[i]);
    		}
    		stream.Write(security);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		for (int i = 0; i < chr_Reg.Length; i++)
    		{
    			chr_Reg[i] = stream.ReadInt32();
    		}
    		security = stream.ReadByte();
    	}
    }
}
