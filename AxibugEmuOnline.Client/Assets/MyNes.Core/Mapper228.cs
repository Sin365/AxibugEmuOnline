using System.IO;

namespace MyNes.Core
{
    [BoardInfo("Unknown", 228)]
    [HassIssues]
    internal class Mapper228 : Board
    {
    	private byte[] RAM;

    	private int bank;

    	internal override string Issues => MNInterfaceLanguage.IssueMapper228;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		RAM = new byte[4];
    	}

    	internal override void WriteEX(ref ushort address, ref byte data)
    	{
    		RAM[address & 3] = (byte)(data & 0xFu);
    	}

    	internal override void ReadEX(ref ushort address, out byte data)
    	{
    		data = RAM[address & 3];
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		Switch08KCHR(((address & 0xF) << 2) | (data & 3));
    		Switch01KNMTFromMirroring(((address & 0x2000) == 8192) ? Mirroring.Horz : Mirroring.Vert);
    		bank = ((address >> 7) & 0x1F) + ((address >> 7) & (address >> 8) & 0x10);
    		if ((address & 0x20) == 32)
    		{
    			Switch16KPRG((bank << 2) | ((address >> 5) & 2), PRGArea.Area8000);
    			Switch16KPRG((bank << 2) | ((address >> 5) & 2), PRGArea.AreaC000);
    		}
    		else
    		{
    			Switch32KPRG(bank, PRGArea.Area8000);
    		}
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(RAM);
    		stream.Write(bank);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		stream.Read(RAM, 0, RAM.Length);
    		bank = stream.ReadInt32();
    	}
    }
}
