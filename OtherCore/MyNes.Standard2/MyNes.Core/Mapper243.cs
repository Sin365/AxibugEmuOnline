using System.IO;

namespace MyNes.Core
{
    [BoardInfo("Sachen Poker", 243)]
    [HassIssues]
    internal class Mapper243 : Board
    {
    	private int addr;

    	private int chr_reg;

    	internal override string Issues => MNInterfaceLanguage.IssueMapper243;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		addr = (chr_reg = 0);
    	}

    	internal override void WriteEX(ref ushort address, ref byte data)
    	{
    		if (address >= 20480 || address < 16416)
    		{
    			return;
    		}
    		switch (address & 0x4101)
    		{
    		case 16640:
    			addr = data & 7;
    			break;
    		case 16641:
    			switch (addr)
    			{
    			case 2:
    				chr_reg = ((data << 3) & 8) | (chr_reg & 7);
    				Switch08KCHR(chr_reg);
    				break;
    			case 4:
    				chr_reg = (data & 1) | (chr_reg & 0xE);
    				Switch08KCHR(chr_reg);
    				break;
    			case 5:
    				Switch32KPRG(data & 7, PRGArea.Area8000);
    				break;
    			case 6:
    				chr_reg = ((data & 3) << 1) | (chr_reg & 9);
    				Switch08KCHR(chr_reg);
    				break;
    			case 7:
    				switch ((data >> 1) & 3)
    				{
    				case 0:
    					Switch01KNMTFromMirroring(Mirroring.Horz);
    					break;
    				case 1:
    					Switch01KNMTFromMirroring(Mirroring.Vert);
    					break;
    				case 2:
    					Switch01KNMT(14);
    					break;
    				case 3:
    					Switch01KNMTFromMirroring(Mirroring.OneScB);
    					break;
    				}
    				break;
    			case 3:
    				break;
    			}
    			break;
    		}
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(addr);
    		stream.Write(chr_reg);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		addr = stream.ReadInt32();
    		chr_reg = stream.ReadInt32();
    	}
    }
}
