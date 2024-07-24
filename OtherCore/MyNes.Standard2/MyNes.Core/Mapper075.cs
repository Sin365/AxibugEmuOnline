using System.IO;

namespace MyNes.Core
{
    [BoardInfo("VRC1", 75)]
    internal class Mapper075 : Board
    {
    	private int chr0_reg;

    	private int chr1_reg;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch08KPRG(PRG_ROM_08KB_Mask, PRGArea.AreaE000);
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		switch (address & 0xF000)
    		{
    		case 32768:
    			Switch08KPRG(data & 0xF, PRGArea.Area8000);
    			break;
    		case 40960:
    			Switch08KPRG(data & 0xF, PRGArea.AreaA000);
    			break;
    		case 49152:
    			Switch08KPRG(data & 0xF, PRGArea.AreaC000);
    			break;
    		case 36864:
    			Switch01KNMTFromMirroring(((data & 1) == 1) ? Mirroring.Horz : Mirroring.Vert);
    			chr0_reg = (chr0_reg & 0xF) | ((data & 2) << 3);
    			Switch04KCHR(chr0_reg, CHRArea.Area0000);
    			chr1_reg = (chr1_reg & 0xF) | ((data & 4) << 2);
    			Switch04KCHR(chr1_reg, CHRArea.Area1000);
    			break;
    		case 57344:
    			chr0_reg = (chr0_reg & 0x10) | (data & 0xF);
    			Switch04KCHR(chr0_reg, CHRArea.Area0000);
    			break;
    		case 61440:
    			chr1_reg = (chr1_reg & 0x10) | (data & 0xF);
    			Switch04KCHR(chr1_reg, CHRArea.Area1000);
    			break;
    		}
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(chr0_reg);
    		stream.Write(chr1_reg);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		chr0_reg = stream.ReadInt32();
    		chr1_reg = stream.ReadInt32();
    	}
    }
}
