using System.IO;

namespace MyNes.Core
{
    [BoardInfo("Irem G-101", 32)]
    internal class Mapper032 : Board
    {
    	private bool prg_mode;

    	private byte prg_reg0;

    	private bool enable_mirroring_switch;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		enable_mirroring_switch = true;
    		if (SHA1 == "7E4180432726A433C46BA2206D9E13B32761C11E")
    		{
    			enable_mirroring_switch = false;
    			Switch01KNMTFromMirroring(Mirroring.OneScA);
    		}
    		Switch08KPRG(PRG_ROM_08KB_Mask, PRGArea.AreaE000);
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		switch (address & 0xF007)
    		{
    		case 32768:
    		case 32769:
    		case 32770:
    		case 32771:
    		case 32772:
    		case 32773:
    		case 32774:
    		case 32775:
    			prg_reg0 = data;
    			Switch08KPRG((!prg_mode) ? prg_reg0 : 0, PRGArea.Area8000);
    			Switch08KPRG(prg_mode ? prg_reg0 : (PRG_ROM_08KB_Mask - 1), PRGArea.AreaC000);
    			break;
    		case 36864:
    		case 36865:
    		case 36866:
    		case 36867:
    		case 36868:
    		case 36869:
    		case 36870:
    		case 36871:
    			prg_mode = (data & 2) == 2;
    			Switch08KPRG((!prg_mode) ? prg_reg0 : 0, PRGArea.Area8000);
    			Switch08KPRG(prg_mode ? prg_reg0 : (PRG_ROM_08KB_Mask - 1), PRGArea.AreaC000);
    			if (enable_mirroring_switch)
    			{
    				Switch01KNMTFromMirroring(((data & 1) == 1) ? Mirroring.Horz : Mirroring.Vert);
    			}
    			break;
    		case 40960:
    		case 40961:
    		case 40962:
    		case 40963:
    		case 40964:
    		case 40965:
    		case 40966:
    		case 40967:
    			Switch08KPRG(data, PRGArea.AreaA000);
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

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(prg_mode);
    		stream.Write(prg_reg0);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		prg_mode = stream.ReadBoolean();
    		prg_reg0 = stream.ReadByte();
    	}
    }
}
