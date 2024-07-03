using System.IO;

namespace MyNes.Core
{
    [BoardInfo("Unknown", 60)]
    [HassIssues]
    internal class Mapper060 : Board
    {
    	private int latch = 0;

    	private byte menu = 0;

    	internal override string Issues => MNInterfaceLanguage.IssueMapper60;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		latch = 0;
    		menu = 0;
    	}

    	internal override void SoftReset()
    	{
    		base.SoftReset();
    		latch = 0;
    		menu = (byte)((uint)(menu + 1) & 3u);
    		Switch08KCHR(menu);
    		Switch16KPRG(menu, PRGArea.Area8000);
    		Switch16KPRG(menu, PRGArea.AreaC000);
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		latch = address & 0x100;
    		Switch01KNMTFromMirroring(((address & 8) == 8) ? Mirroring.Horz : Mirroring.Vert);
    		Switch16KPRG((address >> 4) & ~((~address >> 7) & 1), PRGArea.Area8000);
    		Switch16KPRG((address >> 4) | ((~address >> 7) & 1), PRGArea.AreaC000);
    		Switch08KCHR(address);
    	}

    	internal override void ReadPRG(ref ushort address, out byte data)
    	{
    		if (latch == 0)
    		{
    			base.ReadPRG(ref address, out data);
    		}
    		else
    		{
    			data = menu;
    		}
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(latch);
    		stream.Write(menu);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		latch = stream.ReadInt32();
    		menu = stream.ReadByte();
    	}
    }
}
