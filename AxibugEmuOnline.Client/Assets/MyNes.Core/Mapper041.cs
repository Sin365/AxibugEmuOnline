using System.IO;

namespace MyNes.Core
{
    [BoardInfo("Caltron 6-in-1", 41)]
    internal class Mapper041 : Board
    {
    	private bool enableReg = false;

    	private int vromReg = 0;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		vromReg = 0;
    		enableReg = true;
    	}

    	internal override void WriteSRM(ref ushort address, ref byte data)
    	{
    		if (address <= 26623)
    		{
    			Switch32KPRG(address & 7, PRGArea.Area8000);
    			enableReg = (address & 4) == 4;
    			vromReg = (vromReg & 3) | ((address >> 1) & 0xC);
    			Switch08KCHR(vromReg);
    			Switch01KNMTFromMirroring(((address & 0x20) == 32) ? Mirroring.Horz : Mirroring.Vert);
    		}
    		else
    		{
    			base.WriteSRM(ref address, ref data);
    		}
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		if (enableReg)
    		{
    			vromReg = (vromReg & 0xC) | (data & 3);
    			Switch08KCHR(vromReg);
    		}
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(enableReg);
    		stream.Write(vromReg);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		enableReg = stream.ReadBoolean();
    		vromReg = stream.ReadInt32();
    	}
    }
}
