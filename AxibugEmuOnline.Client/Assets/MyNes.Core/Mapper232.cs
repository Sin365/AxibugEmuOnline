using System.IO;

namespace MyNes.Core
{
    [BoardInfo("Unknown", 232)]
    internal class Mapper232 : Board
    {
    	private int prg_reg;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch16KPRG(3, PRGArea.AreaC000);
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		if (address < 49152)
    		{
    			prg_reg = ((data & 0x18) >> 1) | (prg_reg & 3);
    		}
    		else
    		{
    			prg_reg = (prg_reg & 0xC) | (data & 3);
    		}
    		Switch16KPRG(prg_reg, PRGArea.Area8000);
    		Switch16KPRG(3 | (prg_reg & 0xC), PRGArea.AreaC000);
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(prg_reg);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		prg_reg = stream.ReadInt32();
    	}
    }
}
