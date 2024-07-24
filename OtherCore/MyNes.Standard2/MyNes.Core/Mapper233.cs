using System.IO;

namespace MyNes.Core
{
    [BoardInfo("Unknown", 233)]
    internal class Mapper233 : Board
    {
    	private int title;

    	private int bank;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		bank = (title = 0);
    	}

    	internal override void SoftReset()
    	{
    		base.SoftReset();
    		bank = 0;
    		title ^= 32;
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		bank = data & 0x1F;
    		if ((data & 0x20) == 32)
    		{
    			Switch16KPRG(title | bank, PRGArea.Area8000);
    			Switch16KPRG(title | bank, PRGArea.AreaC000);
    		}
    		else
    		{
    			Switch32KPRG((title >> 1) | (bank >> 1), PRGArea.Area8000);
    		}
    		switch ((data >> 6) & 3)
    		{
    		case 0:
    			Switch01KNMT(128);
    			break;
    		case 1:
    			Switch01KNMTFromMirroring(Mirroring.Vert);
    			break;
    		case 2:
    			Switch01KNMTFromMirroring(Mirroring.Horz);
    			break;
    		case 3:
    			Switch01KNMTFromMirroring(Mirroring.OneScB);
    			break;
    		}
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(title);
    		stream.Write(bank);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		title = stream.ReadInt32();
    		bank = stream.ReadInt32();
    	}
    }
}
