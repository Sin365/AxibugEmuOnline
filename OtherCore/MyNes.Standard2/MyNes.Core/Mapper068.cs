using System.IO;

namespace MyNes.Core
{
    [BoardInfo("Sunsoft 4", 68)]
    internal class Mapper068 : Board
    {
    	private bool flag_r;

    	private bool flag_m;

    	private int nt_reg0;

    	private int nt_reg1;

    	private int temp;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch16KPRG(PRG_ROM_16KB_Mask, PRGArea.AreaC000);
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		switch (address & 0xF000)
    		{
    		case 32768:
    			Switch02KCHR(data, CHRArea.Area0000);
    			break;
    		case 36864:
    			Switch02KCHR(data, CHRArea.Area0800);
    			break;
    		case 40960:
    			Switch02KCHR(data, CHRArea.Area1000);
    			break;
    		case 45056:
    			Switch02KCHR(data, CHRArea.Area1800);
    			break;
    		case 49152:
    			nt_reg0 = (data & 0x7F) | 0x80;
    			break;
    		case 53248:
    			nt_reg1 = (data & 0x7F) | 0x80;
    			break;
    		case 57344:
    			flag_r = (data & 0x10) == 16;
    			flag_m = (data & 1) == 1;
    			Switch01KNMTFromMirroring(flag_m ? Mirroring.Horz : Mirroring.Vert);
    			break;
    		case 61440:
    			Switch16KPRG(data, PRGArea.Area8000);
    			break;
    		}
    	}

    	internal override void ReadNMT(ref ushort address, out byte data)
    	{
    		if (!flag_r)
    		{
    			data = NMT_RAM[NMT_AREA_BLK_INDEX[(address >> 10) & 3]][address & 0x3FF];
    			return;
    		}
    		switch ((address >> 10) & 3)
    		{
    		case 0:
    			data = CHR_ROM[nt_reg0][address & 0x3FF];
    			break;
    		case 1:
    			data = CHR_ROM[flag_m ? nt_reg0 : nt_reg1][address & 0x3FF];
    			break;
    		case 2:
    			data = CHR_ROM[flag_m ? nt_reg1 : nt_reg0][address & 0x3FF];
    			break;
    		case 3:
    			data = CHR_ROM[nt_reg1][address & 0x3FF];
    			break;
    		default:
    			data = 0;
    			break;
    		}
    	}

    	internal override void WriteNMT(ref ushort address, ref byte data)
    	{
    		if (!flag_r)
    		{
    			base.WriteNMT(ref address, ref data);
    		}
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(flag_r);
    		stream.Write(flag_m);
    		stream.Write(nt_reg0);
    		stream.Write(nt_reg1);
    		stream.Write(temp);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		flag_r = stream.ReadBoolean();
    		flag_m = stream.ReadBoolean();
    		nt_reg0 = stream.ReadInt32();
    		nt_reg1 = stream.ReadInt32();
    		temp = stream.ReadInt32();
    	}
    }
}
