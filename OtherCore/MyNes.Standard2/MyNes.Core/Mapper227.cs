using System.IO;

namespace MyNes.Core
{
    [BoardInfo("Unknown", 227)]
    internal class Mapper227 : Board
    {
    	private bool flag_o;

    	private bool flag_s;

    	private bool flag_l;

    	private int prg_reg;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch16KPRG(0, PRGArea.AreaC000);
    		flag_o = false;
    		flag_s = false;
    		flag_l = false;
    		prg_reg = 0;
    		ToggleCHRRAMWritableEnable(enable: true);
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		flag_s = (address & 1) == 1;
    		flag_o = (address & 0x80) == 128;
    		flag_l = (address & 0x200) == 512;
    		prg_reg = ((address >> 2) & 0x1F) | ((address >> 3) & 0x20);
    		Switch01KNMTFromMirroring(((address & 2) == 2) ? Mirroring.Horz : Mirroring.Vert);
    		ToggleCHRRAMWritableEnable(!flag_o);
    		if (flag_o)
    		{
    			if (!flag_s)
    			{
    				Switch16KPRG(prg_reg, PRGArea.Area8000);
    				Switch16KPRG(prg_reg, PRGArea.AreaC000);
    			}
    			else
    			{
    				Switch32KPRG(prg_reg >> 1, PRGArea.Area8000);
    			}
    		}
    		else if (!flag_l)
    		{
    			if (!flag_s)
    			{
    				Switch16KPRG(prg_reg, PRGArea.Area8000);
    				Switch16KPRG(prg_reg & 0x38, PRGArea.AreaC000);
    			}
    			else
    			{
    				Switch16KPRG(prg_reg & 0x3E, PRGArea.Area8000);
    				Switch16KPRG(prg_reg & 0x38, PRGArea.AreaC000);
    			}
    		}
    		else if (!flag_s)
    		{
    			Switch16KPRG(prg_reg, PRGArea.Area8000);
    			Switch16KPRG(prg_reg | 7, PRGArea.AreaC000);
    		}
    		else
    		{
    			Switch16KPRG(prg_reg & 0x3E, PRGArea.Area8000);
    			Switch16KPRG(prg_reg | 7, PRGArea.AreaC000);
    		}
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(flag_o);
    		stream.Write(flag_s);
    		stream.Write(flag_l);
    		stream.Write(prg_reg);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		flag_o = stream.ReadBoolean();
    		flag_s = stream.ReadBoolean();
    		flag_l = stream.ReadBoolean();
    		prg_reg = stream.ReadInt32();
    	}
    }
}
