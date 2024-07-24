using System.IO;

namespace MyNes.Core
{
    [BoardInfo("15-in-1 Color Dreams", 46)]
    internal class Mapper046 : Board
    {
    	private int prg_reg;

    	private int chr_reg;

    	internal override void WriteSRM(ref ushort address, ref byte data)
    	{
    		prg_reg = (prg_reg & 1) | ((data << 1) & 0x1E);
    		chr_reg = (chr_reg & 7) | ((data >> 1) & 0x78);
    		Switch08KCHR(chr_reg);
    		Switch32KPRG(prg_reg, PRGArea.Area8000);
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		prg_reg = (data & 1) | (prg_reg & 0x1E);
    		chr_reg = ((data >> 4) & 7) | (chr_reg & 0x78);
    		Switch08KCHR(chr_reg);
    		Switch32KPRG(prg_reg, PRGArea.Area8000);
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(prg_reg);
    		stream.Write(chr_reg);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		prg_reg = stream.ReadInt32();
    		chr_reg = stream.ReadInt32();
    	}
    }
}
