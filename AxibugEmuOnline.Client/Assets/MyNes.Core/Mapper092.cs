using System.IO;

namespace MyNes.Core
{
    [BoardInfo("Jaleco Early Mapper 1", 92)]
    internal class Mapper092 : Board
    {
    	private int chr_reg;

    	private int prg_reg;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch16KPRG(0, PRGArea.Area8000);
    		chr_reg = (prg_reg = 0);
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		switch ((data >> 6) & 3)
    		{
    		case 0:
    			Switch08KCHR(chr_reg);
    			Switch16KPRG(prg_reg, PRGArea.AreaC000);
    			break;
    		case 1:
    			chr_reg = data & 0xF;
    			break;
    		case 2:
    			prg_reg = data & 0xF;
    			break;
    		}
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(chr_reg);
    		stream.Write(prg_reg);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		chr_reg = stream.ReadInt32();
    		prg_reg = stream.ReadInt32();
    	}
    }
}
