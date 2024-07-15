using System.IO;

namespace MyNes.Core
{
    [BoardInfo("11-in-1", 51)]
    internal class Mapper051 : Board
    {
    	private int bank;

    	private int mode = 1;

    	private int offset;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		bank = 0;
    		mode = 1;
    		offset = 0;
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		switch (address & 0xE000)
    		{
    		case 32768:
    		case 57344:
    			bank = data & 0xF;
    			UpdateBanks();
    			break;
    		case 49152:
    			bank = data & 0xF;
    			mode = ((data >> 3) & 2) | (mode & 1);
    			UpdateBanks();
    			break;
    		}
    	}

    	internal override void WriteSRM(ref ushort address, ref byte data)
    	{
    		mode = ((data >> 3) & 2) | ((data >> 1) & 1);
    		UpdateBanks();
    	}

    	private void UpdateBanks()
    	{
    		offset = 0;
    		if ((mode & 1) == 1)
    		{
    			Switch32KPRG(bank, PRGArea.Area8000);
    			offset = 35;
    		}
    		else
    		{
    			Switch08KPRG((bank << 1) | (mode >> 1), PRGArea.Area8000);
    			Switch08KPRG((bank << 1) | 7, PRGArea.Area8000);
    			offset = 47;
    		}
    		Switch08KPRG(offset | (bank << 2), PRGArea.Area6000);
    		Switch01KNMTFromMirroring((mode == 3) ? Mirroring.Horz : Mirroring.Vert);
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(bank);
    		stream.Write(mode);
    		stream.Write(offset);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		bank = stream.ReadInt32();
    		mode = stream.ReadInt32();
    		offset = stream.ReadInt32();
    	}
    }
}
