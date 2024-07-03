using System.IO;

namespace MyNes.Core
{
    [BoardInfo("Unknown", 225)]
    internal class Mapper225 : Board
    {
    	private byte[] RAM;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		RAM = new byte[4];
    	}

    	internal override void WriteEX(ref ushort address, ref byte data)
    	{
    		if (address >= 22528)
    		{
    			RAM[address & 3] = (byte)(data & 0xFu);
    		}
    	}

    	internal override void ReadEX(ref ushort address, out byte value)
    	{
    		if (address >= 22528)
    		{
    			value = RAM[address & 3];
    		}
    		else
    		{
    			value = 0;
    		}
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		Switch08KCHR(address & 0x3F);
    		if ((address & 0x1000) == 4096)
    		{
    			Switch16KPRG((address >> 6) & 0x3F, PRGArea.Area8000);
    			Switch16KPRG((address >> 6) & 0x3F, PRGArea.AreaC000);
    		}
    		else
    		{
    			Switch32KPRG(((address >> 6) & 0x3F) >> 1, PRGArea.Area8000);
    		}
    		Switch01KNMTFromMirroring(((address & 0x2000) == 8192) ? Mirroring.Horz : Mirroring.Vert);
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(RAM);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		stream.Read(RAM, 0, RAM.Length);
    	}
    }
}
