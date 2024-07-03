using System.IO;

namespace MyNes.Core
{
    internal abstract class MMC2 : Board
    {
    	private byte chr_reg0A;

    	private byte chr_reg0B;

    	private byte chr_reg1A;

    	private byte chr_reg1B;

    	private byte latch_a = 254;

    	private byte latch_b = 254;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch08KPRG(PRG_ROM_08KB_Mask - 2, PRGArea.AreaA000);
    		Switch08KPRG(PRG_ROM_08KB_Mask - 1, PRGArea.AreaC000);
    		Switch08KPRG(PRG_ROM_08KB_Mask, PRGArea.AreaE000);
    		chr_reg0B = 4;
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		switch (address & 0xF000)
    		{
    		case 40960:
    			Switch08KPRG(data, PRGArea.Area8000);
    			break;
    		case 45056:
    			chr_reg0A = data;
    			if (latch_a == 253)
    			{
    				Switch04KCHR(chr_reg0A, CHRArea.Area0000);
    			}
    			break;
    		case 49152:
    			chr_reg0B = data;
    			if (latch_a == 254)
    			{
    				Switch04KCHR(chr_reg0B, CHRArea.Area0000);
    			}
    			break;
    		case 53248:
    			chr_reg1A = data;
    			if (latch_b == 253)
    			{
    				Switch04KCHR(chr_reg1A, CHRArea.Area1000);
    			}
    			break;
    		case 57344:
    			chr_reg1B = data;
    			if (latch_b == 254)
    			{
    				Switch04KCHR(chr_reg1B, CHRArea.Area1000);
    			}
    			break;
    		case 61440:
    			Switch01KNMTFromMirroring(((data & 1) == 1) ? Mirroring.Horz : Mirroring.Vert);
    			break;
    		}
    	}

    	internal override void ReadCHR(ref ushort address, out byte data)
    	{
    		if ((address & 0x1FF0) == 4048 && latch_a != 253)
    		{
    			latch_a = 253;
    			Switch04KCHR(chr_reg0A, CHRArea.Area0000);
    		}
    		else if ((address & 0x1FF0) == 4064 && latch_a != 254)
    		{
    			latch_a = 254;
    			Switch04KCHR(chr_reg0B, CHRArea.Area0000);
    		}
    		else if ((address & 0x1FF0) == 8144 && latch_b != 253)
    		{
    			latch_b = 253;
    			Switch04KCHR(chr_reg1A, CHRArea.Area1000);
    		}
    		else if ((address & 0x1FF0) == 8160 && latch_b != 254)
    		{
    			latch_b = 254;
    			Switch04KCHR(chr_reg1B, CHRArea.Area1000);
    		}
    		base.ReadCHR(ref address, out data);
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(chr_reg0A);
    		stream.Write(chr_reg0B);
    		stream.Write(chr_reg1A);
    		stream.Write(chr_reg1B);
    		stream.Write(latch_a);
    		stream.Write(latch_b);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		chr_reg0A = stream.ReadByte();
    		chr_reg0B = stream.ReadByte();
    		chr_reg1A = stream.ReadByte();
    		chr_reg1B = stream.ReadByte();
    		latch_a = stream.ReadByte();
    		latch_b = stream.ReadByte();
    	}
    }
}
