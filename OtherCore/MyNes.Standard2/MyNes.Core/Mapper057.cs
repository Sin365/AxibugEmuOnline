using System.IO;

namespace MyNes.Core
{
    [BoardInfo("6-in-1 (SuperGK)", 57)]
    internal class Mapper057 : Board
    {
    	private int chr_aaa;

    	private int chr_bbb;

    	private int chr_hhh;

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		switch (address & 0x8800)
    		{
    		case 32768:
    			chr_aaa = data & 7;
    			chr_hhh = (data & 0x40) >> 3;
    			break;
    		case 34816:
    			chr_bbb = data & 7;
    			if ((data & 0x10) == 16)
    			{
    				Switch32KPRG((data & 0xE0) >> 6, PRGArea.Area8000);
    			}
    			else
    			{
    				Switch16KPRG((data & 0xE0) >> 5, PRGArea.Area8000);
    				Switch16KPRG((data & 0xE0) >> 5, PRGArea.AreaC000);
    			}
    			Switch01KNMTFromMirroring(((data & 8) == 8) ? Mirroring.Horz : Mirroring.Vert);
    			break;
    		}
    		Switch08KCHR(chr_hhh | (chr_aaa | chr_bbb));
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(chr_aaa);
    		stream.Write(chr_bbb);
    		stream.Write(chr_hhh);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		chr_aaa = stream.ReadInt32();
    		chr_bbb = stream.ReadInt32();
    		chr_hhh = stream.ReadInt32();
    	}
    }
}
