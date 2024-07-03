using System.IO;

namespace MyNes.Core
{
    [BoardInfo("Asder", 112)]
    internal class Mapper112 : Board
    {
    	private int address_A000;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch16KPRG(PRG_ROM_16KB_Mask, PRGArea.AreaC000);
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		switch (address & 0xE001)
    		{
    		case 32768:
    			address_A000 = data & 7;
    			break;
    		case 40960:
    			switch (address_A000)
    			{
    			case 0:
    				Switch08KPRG(data, PRGArea.Area8000);
    				break;
    			case 1:
    				Switch08KPRG(data, PRGArea.AreaA000);
    				break;
    			case 2:
    				Switch02KCHR(data >> 1, CHRArea.Area0000);
    				break;
    			case 3:
    				Switch02KCHR(data >> 1, CHRArea.Area0800);
    				break;
    			case 4:
    				Switch01KCHR(data, CHRArea.Area1000);
    				break;
    			case 5:
    				Switch01KCHR(data, CHRArea.Area1400);
    				break;
    			case 6:
    				Switch01KCHR(data, CHRArea.Area1800);
    				break;
    			case 7:
    				Switch01KCHR(data, CHRArea.Area1C00);
    				break;
    			}
    			break;
    		case 57344:
    			Switch01KNMTFromMirroring(((data & 1) == 1) ? Mirroring.Horz : Mirroring.Vert);
    			break;
    		}
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(address_A000);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		address_A000 = stream.ReadInt32();
    	}
    }
}
