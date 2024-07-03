using System.IO;

namespace MyNes.Core
{
    [BoardInfo("74161/32", 96, 1, 32)]
    internal class Mapper096 : Board
    {
    	private int flag_c;

    	internal override string Issues => MNInterfaceLanguage.IssueMapper96;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		flag_c = 0;
    		Switch04KCHR(3, CHRArea.Area1000);
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		Switch32KPRG(data & 3, PRGArea.Area8000);
    		flag_c = (((data & 4) == 4) ? 1 : 0);
    		Switch04KCHR(3, CHRArea.Area1000);
    	}

    	internal override void OnPPUAddressUpdate(ref ushort address)
    	{
    		if ((address & 0x3FF) < 960 && (address & 0x1000) == 0)
    		{
    			Switch04KCHR(((address & 0x300) >> 8) | flag_c, CHRArea.Area0000);
    		}
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(flag_c);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		flag_c = stream.ReadInt32();
    	}
    }
}
