using System.IO;

namespace MyNes.Core
{
    [BoardInfo("Unknown", 207)]
    [HassIssues]
    internal class Mapper207 : Board
    {
    	private int mirroring0;

    	private int mirroring1;

    	internal override string Issues => MNInterfaceLanguage.IssueMapper207;

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
    			Switch02KCHR(data & 0x3F, CHRArea.Area0000);
    			mirroring0 = (data >> 7) & 1;
    			break;
    		case 32497:
    			Switch02KCHR(data & 0x3F, CHRArea.Area0800);
    			mirroring1 = (data >> 7) & 1;
    			break;
    		case 32498:
    			Switch01KCHR(data, CHRArea.Area1000);
    			break;
    		case 32499:
    			Switch01KCHR(data, CHRArea.Area1400);
    			break;
    		case 32500:
    			Switch01KCHR(data, CHRArea.Area1800);
    			break;
    		case 32501:
    			Switch01KCHR(data, CHRArea.Area1C00);
    			break;
    		case 32506:
    		case 32507:
    			Switch08KPRG(data, PRGArea.Area8000);
    			break;
    		case 32508:
    		case 32509:
    			Switch08KPRG(data, PRGArea.AreaA000);
    			break;
    		case 32510:
    		case 32511:
    			Switch08KPRG(data, PRGArea.AreaC000);
    			break;
    		case 32502:
    		case 32503:
    		case 32504:
    		case 32505:
    			break;
    		}
    	}

    	internal override void ReadNMT(ref ushort address, out byte data)
    	{
    		switch ((address >> 10) & 3)
    		{
    		case 0:
    		case 1:
    			data = NMT_RAM[mirroring0][address & 0x3FF];
    			break;
    		case 2:
    		case 3:
    			data = NMT_RAM[mirroring1][address & 0x3FF];
    			break;
    		default:
    			data = 0;
    			break;
    		}
    	}

    	internal override void WriteNMT(ref ushort address, ref byte data)
    	{
    		switch ((address >> 10) & 3)
    		{
    		case 0:
    		case 1:
    			NMT_RAM[mirroring0][address & 0x3FF] = data;
    			break;
    		case 2:
    		case 3:
    			NMT_RAM[mirroring1][address & 0x3FF] = data;
    			break;
    		}
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(mirroring0);
    		stream.Write(mirroring1);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		mirroring0 = stream.ReadInt32();
    		mirroring1 = stream.ReadInt32();
    	}
    }
}
