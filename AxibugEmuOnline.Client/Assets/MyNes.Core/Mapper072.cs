using System.IO;

namespace MyNes.Core
{
    [BoardInfo("Jaleco Early Mapper 0", 72)]
    internal class Mapper072 : Board
    {
    	private byte writeData;

    	private int chr_reg;

    	private int prg_reg;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch16KPRG(PRG_ROM_16KB_Mask, PRGArea.AreaC000);
    		writeData = 0;
    		chr_reg = (prg_reg = 0);
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		switch ((data >> 6) & 3)
    		{
    		case 0:
    			Switch08KCHR(chr_reg);
    			Switch16KPRG(prg_reg, PRGArea.Area8000);
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
