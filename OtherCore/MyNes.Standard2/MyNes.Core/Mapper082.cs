using System.IO;

namespace MyNes.Core
{
    [BoardInfo("Taito X1-17 ", 82)]
    internal class Mapper082 : Board
    {
    	private bool chr_mode;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch08KPRG(PRG_ROM_08KB_Mask, PRGArea.AreaE000);
    	}

    	internal override void WriteSRM(ref ushort address, ref byte data)
    	{
    		switch (address)
    		{
    		case 32496:
    			Switch02KCHR(data >> 1, chr_mode ? CHRArea.Area1000 : CHRArea.Area0000);
    			break;
    		case 32497:
    			Switch02KCHR(data >> 1, chr_mode ? CHRArea.Area1800 : CHRArea.Area0800);
    			break;
    		case 32498:
    			Switch01KCHR(data, (!chr_mode) ? CHRArea.Area1000 : CHRArea.Area0000);
    			break;
    		case 32499:
    			Switch01KCHR(data, chr_mode ? CHRArea.Area0400 : CHRArea.Area1400);
    			break;
    		case 32500:
    			Switch01KCHR(data, chr_mode ? CHRArea.Area0800 : CHRArea.Area1800);
    			break;
    		case 32501:
    			Switch01KCHR(data, chr_mode ? CHRArea.Area0C00 : CHRArea.Area1C00);
    			break;
    		case 32502:
    			Switch01KNMTFromMirroring(((data & 1) == 1) ? Mirroring.Vert : Mirroring.Horz);
    			chr_mode = (data & 2) == 2;
    			break;
    		case 32506:
    			Switch08KPRG(data >> 2, PRGArea.Area8000);
    			break;
    		case 32507:
    			Switch08KPRG(data >> 2, PRGArea.AreaA000);
    			break;
    		case 32508:
    			Switch08KPRG(data >> 2, PRGArea.AreaC000);
    			break;
    		case 32503:
    		case 32504:
    		case 32505:
    			break;
    		}
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(chr_mode);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		chr_mode = stream.ReadBoolean();
    	}
    }
}
