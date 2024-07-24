using System;
using System.IO;

namespace MyNes.Core
{
    [BoardInfo("MMC5", 5, 8, 16)]
    [WithExternalSound]
    [HassIssues]
    internal class Mapper005 : Board
    {
    	private int ram_protectA;

    	private int ram_protectB;

    	private int ExRAM_mode;

    	private int[] CHROffset_spr;

    	private int[] CHROffsetEX;

    	private int[] CHROffsetSP;

    	private int[] chrRegA;

    	private int[] chrRegB;

    	private int[] prgReg;

    	private bool useSRAMmirroring;

    	private int chr_high;

    	private int chr_mode;

    	private int prg_mode;

    	private bool chr_setB_last;

    	private byte temp_val;

    	private byte temp_fill;

    	private int lastAccessVRAM;

    	private int paletteNo;

    	private int shift;

    	private int EXtilenumber;

    	private byte multiplicand;

    	private byte multiplier;

    	private ushort product;

    	private bool split_enable;

    	private bool split_right;

    	private int split_tile;

    	private int split_yscroll;

    	private bool split_doit;

    	private int split_watch_tile;

    	private byte irq_line;

    	private byte irq_enable;

    	private int irq_pending;

    	private int irq_current_counter;

    	private int irq_current_inframe;

    	private MMC5Sqr snd_1;

    	private MMC5Sqr snd_2;

    	private MMC5Pcm snd_3;

    	private double[] audio_pulse_table;

    	private double[] audio_tnd_table;

    	internal override string Issues => MNInterfaceLanguage.IssueMapper5;

    	internal override void Initialize(IRom rom)
    	{
    		base.Initialize(rom);
    		snd_1 = new MMC5Sqr();
    		snd_2 = new MMC5Sqr();
    		snd_3 = new MMC5Pcm();
    		audio_pulse_table = new double[32];
    		for (int i = 0; i < 32; i++)
    		{
    			audio_pulse_table[i] = 95.52 / (8128.0 / (double)i + 100.0);
    		}
    		audio_tnd_table = new double[204];
    		for (int j = 0; j < 204; j++)
    		{
    			audio_tnd_table[j] = 163.67 / (24329.0 / (double)j + 100.0);
    		}
    	}

    	internal override void HardReset()
    	{
    		base.HardReset();
    		switch (SHA1.ToUpper())
    		{
    		case "37267833C984F176DB4B0BC9D45DABA0FFF45304":
    			useSRAMmirroring = true;
    			break;
    		case "800AEFE756E85A0A78CCB4DAE68EBBA5DF24BF41":
    			useSRAMmirroring = true;
    			break;
    		}
    		Console.WriteLine("MMC5: using PRG RAM mirroring = " + useSRAMmirroring);
    		CHROffset_spr = new int[8];
    		CHROffsetEX = new int[8];
    		CHROffsetSP = new int[8];
    		chrRegA = new int[8];
    		chrRegB = new int[4];
    		prgReg = new int[4];
    		prgReg[3] = PRG_ROM_08KB_Mask;
    		prg_mode = 3;
    		Switch08KPRG(PRG_ROM_08KB_Mask, PRGArea.Area8000);
    		Switch08KPRG(PRG_ROM_08KB_Mask, PRGArea.AreaA000);
    		Switch08KPRG(PRG_ROM_08KB_Mask, PRGArea.AreaC000);
    		Switch08KPRG(PRG_ROM_08KB_Mask, PRGArea.AreaE000);
    		Switch04kCHREX(0, 0);
    		Switch04kCHRSP(0, 0);
    		Switch08kCHR_spr(0);
    		TogglePRGRAMWritableEnable(enable: true);
    		TogglePRGRAMEnable(enable: true);
    		APUApplyChannelsSettings();
    		snd_1.HardReset();
    		snd_2.HardReset();
    		snd_3.HardReset();
    	}

    	internal override void SoftReset()
    	{
    		base.SoftReset();
    		snd_1.SoftReset();
    		snd_2.SoftReset();
    		snd_3.SoftReset();
    	}

    	internal override void WriteEX(ref ushort address, ref byte value)
    	{
    		if (address >= 23552)
    		{
    			if (ExRAM_mode == 2)
    			{
    				NMT_RAM[2][address & 0x3FF] = value;
    			}
    			else if (ExRAM_mode < 2)
    			{
    				if (irq_current_inframe == 64)
    				{
    					NMT_RAM[2][address & 0x3FF] = value;
    				}
    				else
    				{
    					NMT_RAM[2][address & 0x3FF] = 0;
    				}
    			}
    			return;
    		}
    		switch (address)
    		{
    		case 20480:
    			snd_1.Write0(ref value);
    			break;
    		case 20482:
    			snd_1.Write2(ref value);
    			break;
    		case 20483:
    			snd_1.Write3(ref value);
    			break;
    		case 20484:
    			snd_2.Write0(ref value);
    			break;
    		case 20486:
    			snd_2.Write2(ref value);
    			break;
    		case 20487:
    			snd_2.Write3(ref value);
    			break;
    		case 20496:
    			snd_3.Write5010(value);
    			break;
    		case 20497:
    			snd_3.Write5011(value);
    			break;
    		case 20501:
    			snd_1.WriteEnabled((value & 1) != 0);
    			snd_2.WriteEnabled((value & 2) != 0);
    			break;
    		case 20736:
    			prg_mode = value & 3;
    			break;
    		case 20737:
    			chr_mode = value & 3;
    			break;
    		case 20738:
    			ram_protectA = value & 3;
    			UpdateRamProtect();
    			break;
    		case 20739:
    			ram_protectB = value & 3;
    			UpdateRamProtect();
    			break;
    		case 20740:
    			ExRAM_mode = value & 3;
    			break;
    		case 20741:
    			Switch01KNMT(value);
    			break;
    		case 20755:
    			if (!useSRAMmirroring)
    			{
    				Switch08KPRG(value & 7, PRGArea.Area6000);
    			}
    			else
    			{
    				Switch08KPRG((value >> 2) & 1, PRGArea.Area6000);
    			}
    			break;
    		case 20756:
    			if (prg_mode == 3)
    			{
    				Toggle08KPRG_RAM((value & 0x80) == 0, PRGArea.Area8000);
    				Switch08KPRG(value & 0x7F, PRGArea.Area8000);
    			}
    			break;
    		case 20757:
    			switch (prg_mode)
    			{
    			case 1:
    				Toggle16KPRG_RAM((value & 0x80) == 0, PRGArea.Area8000);
    				Switch16KPRG((value & 0x7F) >> 1, PRGArea.Area8000);
    				break;
    			case 2:
    				Toggle16KPRG_RAM((value & 0x80) == 0, PRGArea.Area8000);
    				Switch16KPRG((value & 0x7F) >> 1, PRGArea.Area8000);
    				break;
    			case 3:
    				Toggle08KPRG_RAM((value & 0x80) == 0, PRGArea.AreaA000);
    				Switch08KPRG(value & 0x7F, PRGArea.AreaA000);
    				break;
    			}
    			break;
    		case 20758:
    		{
    			int num = prg_mode;
    			if ((uint)(num - 2) <= 1u)
    			{
    				Toggle08KPRG_RAM((value & 0x80) == 0, PRGArea.AreaC000);
    				Switch08KPRG(value & 0x7F, PRGArea.AreaC000);
    			}
    			break;
    		}
    		case 20759:
    			switch (prg_mode)
    			{
    			case 0:
    				Switch32KPRG((value & 0x7C) >> 2, PRGArea.Area8000);
    				break;
    			case 1:
    				Switch16KPRG((value & 0x7F) >> 1, PRGArea.AreaC000);
    				break;
    			case 2:
    				Switch08KPRG(value & 0x7F, PRGArea.AreaE000);
    				break;
    			case 3:
    				Switch08KPRG(value & 0x7F, PRGArea.AreaE000);
    				break;
    			}
    			break;
    		case 20768:
    			chr_setB_last = false;
    			if (chr_mode == 3)
    			{
    				Switch01kCHR_spr(value | chr_high, 0);
    			}
    			break;
    		case 20769:
    			chr_setB_last = false;
    			switch (chr_mode)
    			{
    			case 2:
    				Switch02kCHR_spr(value | chr_high, 0);
    				break;
    			case 3:
    				Switch01kCHR_spr(value | chr_high, 1024);
    				break;
    			}
    			break;
    		case 20770:
    			chr_setB_last = false;
    			if (chr_mode == 3)
    			{
    				Switch01kCHR_spr(value | chr_high, 2048);
    			}
    			break;
    		case 20771:
    			chr_setB_last = false;
    			switch (chr_mode)
    			{
    			case 1:
    				Switch04kCHR_spr(value | chr_high, 0);
    				break;
    			case 2:
    				Switch02kCHR_spr(value | chr_high, 2048);
    				break;
    			case 3:
    				Switch01kCHR_spr(value | chr_high, 3072);
    				break;
    			}
    			break;
    		case 20772:
    			chr_setB_last = false;
    			if (chr_mode == 3)
    			{
    				Switch01kCHR_spr(value | chr_high, 4096);
    			}
    			break;
    		case 20773:
    			chr_setB_last = false;
    			switch (chr_mode)
    			{
    			case 2:
    				Switch02kCHR_spr(value | chr_high, 4096);
    				break;
    			case 3:
    				Switch01kCHR_spr(value | chr_high, 5120);
    				break;
    			}
    			break;
    		case 20774:
    			chr_setB_last = false;
    			if (chr_mode == 3)
    			{
    				Switch01kCHR_spr(value | chr_high, 6144);
    			}
    			break;
    		case 20775:
    			chr_setB_last = false;
    			switch (chr_mode)
    			{
    			case 0:
    				Switch08kCHR_spr(value | chr_high);
    				break;
    			case 1:
    				Switch04kCHR_spr(value | chr_high, 4096);
    				break;
    			case 2:
    				Switch02kCHR_spr(value | chr_high, 6144);
    				break;
    			case 3:
    				Switch01kCHR_spr(value | chr_high, 7168);
    				break;
    			}
    			break;
    		case 20776:
    			chr_setB_last = true;
    			if (chr_mode == 3)
    			{
    				Switch01KCHR(value | chr_high, CHRArea.Area0000);
    				Switch01KCHR(value | chr_high, CHRArea.Area1000);
    			}
    			break;
    		case 20777:
    			chr_setB_last = true;
    			switch (chr_mode)
    			{
    			case 2:
    				Switch02KCHR(value | chr_high, CHRArea.Area0000);
    				Switch02KCHR(value | chr_high, CHRArea.Area1000);
    				break;
    			case 3:
    				Switch01KCHR(value | chr_high, CHRArea.Area0400);
    				Switch01KCHR(value | chr_high, CHRArea.Area1400);
    				break;
    			}
    			break;
    		case 20778:
    			chr_setB_last = true;
    			if (chr_mode == 3)
    			{
    				Switch01KCHR(value | chr_high, CHRArea.Area0800);
    				Switch01KCHR(value | chr_high, CHRArea.Area1800);
    			}
    			break;
    		case 20779:
    			chr_setB_last = true;
    			switch (chr_mode)
    			{
    			case 0:
    				Switch04kCHR_bkg(value | chr_high, 0);
    				Switch04kCHR_bkg(value | chr_high, 4096);
    				break;
    			case 1:
    				Switch04KCHR(value | chr_high, CHRArea.Area0000);
    				Switch04KCHR(value | chr_high, CHRArea.Area1000);
    				break;
    			case 2:
    				Switch02KCHR(value | chr_high, CHRArea.Area0800);
    				Switch02KCHR(value | chr_high, CHRArea.Area1800);
    				break;
    			case 3:
    				Switch01KCHR(value | chr_high, CHRArea.Area0C00);
    				Switch01KCHR(value | chr_high, CHRArea.Area1C00);
    				break;
    			}
    			break;
    		case 20784:
    			chr_high = (value & 3) << 8;
    			break;
    		case 20742:
    		{
    			for (int j = 0; j < 960; j++)
    			{
    				NMT_RAM[3][j] = value;
    			}
    			break;
    		}
    		case 20743:
    		{
    			for (int i = 960; i < 1024; i++)
    			{
    				temp_fill = (byte)((uint)(2 << (value & 3)) | (value & 3u));
    				temp_fill |= (byte)((temp_fill & 0xF) << 4);
    				NMT_RAM[3][i] = temp_fill;
    			}
    			break;
    		}
    		case 20992:
    			split_tile = value & 0x1F;
    			split_enable = (value & 0x80) == 128;
    			split_right = (value & 0x40) == 64;
    			break;
    		case 20993:
    			split_yscroll = value;
    			break;
    		case 20994:
    			Switch04kCHRSP(value, address & 0);
    			Switch04kCHRSP(value, address & 0x1000);
    			break;
    		case 20995:
    			irq_line = value;
    			break;
    		case 20996:
    			irq_enable = value;
    			break;
    		case 20997:
    			multiplicand = value;
    			product = (ushort)(multiplicand * multiplier);
    			break;
    		case 20998:
    			multiplier = value;
    			product = (ushort)(multiplicand * multiplier);
    			break;
    		}
    	}

    	internal override void ReadEX(ref ushort address, out byte data)
    	{
    		if (address >= 23552 && ExRAM_mode >= 2)
    		{
    			data = NMT_RAM[2][address & 0x3FF];
    			return;
    		}
    		switch (address)
    		{
    		case 20496:
    			data = snd_3.Read5010();
    			break;
    		case 20996:
    			data = (byte)(irq_current_inframe | irq_pending);
    			irq_pending = 0;
    			NesEmu.IRQFlags &= -9;
    			break;
    		case 20997:
    			data = (byte)(product & 0xFFu);
    			break;
    		case 20998:
    			data = (byte)((product & 0xFF00) >> 8);
    			break;
    		case 20501:
    			data = (byte)((snd_1.ReadEnable() ? 1u : 0u) | (snd_2.ReadEnable() ? 2u : 0u));
    			data = 0;
    			break;
    		default:
    			data = 0;
    			break;
    		}
    	}

    	internal override void ReadCHR(ref ushort address, out byte data)
    	{
    		if (!NesEmu.ppu_is_sprfetch && split_enable && ExRAM_mode < 2)
    		{
    			split_watch_tile = address & 0x3F;
    			if (!split_right)
    			{
    				split_doit = split_watch_tile < split_tile;
    			}
    			else
    			{
    				split_doit = split_watch_tile >= split_tile;
    			}
    			_ = split_doit;
    		}
    		if (ExRAM_mode == 1)
    		{
    			if (!NesEmu.ppu_is_sprfetch)
    			{
    				EXtilenumber = NMT_RAM[2][lastAccessVRAM] & 0x3F;
    				Switch04kCHREX(EXtilenumber | chr_high, address & 0x1000);
    				data = CHR_ROM[CHROffsetEX[(address >> 10) & 7]][address & 0x3FF];
    			}
    			else
    			{
    				data = CHR_ROM[CHROffset_spr[(address >> 10) & 7]][address & 0x3FF];
    			}
    		}
    		else if (NesEmu.ppu_reg_2000_Sprite_size == 16)
    		{
    			if (!NesEmu.ppu_is_sprfetch)
    			{
    				data = CHR_ROM[CHR_AREA_BLK_INDEX[(address >> 10) & 7]][address & 0x3FF];
    			}
    			else
    			{
    				data = CHR_ROM[CHROffset_spr[(address >> 10) & 7]][address & 0x3FF];
    			}
    		}
    		else if (chr_setB_last)
    		{
    			data = CHR_ROM[CHR_AREA_BLK_INDEX[(address >> 10) & 7]][address & 0x3FF];
    		}
    		else
    		{
    			data = CHR_ROM[CHROffset_spr[(address >> 10) & 7]][address & 0x3FF];
    		}
    	}

    	internal override void ReadNMT(ref ushort address, out byte data)
    	{
    		_ = split_doit;
    		if (ExRAM_mode == 1)
    		{
    			if ((address & 0x3FF) <= 959)
    			{
    				lastAccessVRAM = address & 0x3FF;
    			}
    			else
    			{
    				paletteNo = NMT_RAM[2][lastAccessVRAM] & 0xC0;
    				shift = ((lastAccessVRAM >> 4) & 4) | (lastAccessVRAM & 2);
    				switch (shift)
    				{
    				case 0:
    					data = (byte)(paletteNo >> 6);
    					return;
    				case 2:
    					data = (byte)(paletteNo >> 4);
    					return;
    				case 4:
    					data = (byte)(paletteNo >> 2);
    					return;
    				case 6:
    					data = (byte)paletteNo;
    					return;
    				}
    			}
    		}
    		data = NMT_RAM[NMT_AREA_BLK_INDEX[(address >> 10) & 3]][address & 0x3FF];
    	}

    	internal override void WriteNMT(ref ushort address, ref byte value)
    	{
    		if (ExRAM_mode == 1 && (address & 0x3FF) <= 959)
    		{
    			lastAccessVRAM = address & 0x3FF;
    		}
    		NMT_RAM[NMT_AREA_BLK_INDEX[(address >> 10) & 3]][address & 0x3FF] = value;
    	}

    	private void UpdateRamProtect()
    	{
    		TogglePRGRAMWritableEnable(ram_protectA == 2 && ram_protectB == 1);
    	}

    	private void Switch04kCHR_bkg(int index, int where)
    	{
    		int num = (where >> 10) & 7;
    		index <<= 2;
    		CHR_AREA_BLK_INDEX[num] = index & CHR_ROM_01KB_Mask;
    		num++;
    		index++;
    		CHR_AREA_BLK_INDEX[num] = index & CHR_ROM_01KB_Mask;
    		num++;
    		index++;
    		CHR_AREA_BLK_INDEX[num] = index & CHR_ROM_01KB_Mask;
    		num++;
    		index++;
    		CHR_AREA_BLK_INDEX[num] = index & CHR_ROM_01KB_Mask;
    	}

    	private void Switch01kCHR_spr(int index, int where)
    	{
    		CHROffset_spr[(where >> 10) & 7] = index & CHR_ROM_01KB_Mask;
    	}

    	private void Switch02kCHR_spr(int index, int where)
    	{
    		int num = (where >> 10) & 7;
    		index <<= 1;
    		CHROffset_spr[num] = index & CHR_ROM_01KB_Mask;
    		index++;
    		CHROffset_spr[num + 1] = index & CHR_ROM_01KB_Mask;
    	}

    	private void Switch04kCHR_spr(int index, int where)
    	{
    		int num = (where >> 10) & 7;
    		index <<= 2;
    		CHROffset_spr[num] = index & CHR_ROM_01KB_Mask;
    		num++;
    		index++;
    		CHROffset_spr[num] = index & CHR_ROM_01KB_Mask;
    		num++;
    		index++;
    		CHROffset_spr[num] = index & CHR_ROM_01KB_Mask;
    		num++;
    		index++;
    		CHROffset_spr[num] = index & CHR_ROM_01KB_Mask;
    	}

    	private void Switch08kCHR_spr(int index)
    	{
    		index <<= 3;
    		CHROffset_spr[0] = index & CHR_ROM_01KB_Mask;
    		index++;
    		CHROffset_spr[1] = index & CHR_ROM_01KB_Mask;
    		index++;
    		CHROffset_spr[2] = index & CHR_ROM_01KB_Mask;
    		index++;
    		CHROffset_spr[3] = index & CHR_ROM_01KB_Mask;
    		index++;
    		CHROffset_spr[4] = index & CHR_ROM_01KB_Mask;
    		index++;
    		CHROffset_spr[5] = index & CHR_ROM_01KB_Mask;
    		index++;
    		CHROffset_spr[6] = index & CHR_ROM_01KB_Mask;
    		index++;
    		CHROffset_spr[7] = index & CHR_ROM_01KB_Mask;
    	}

    	private void Switch04kCHREX(int index, int where)
    	{
    		int num = (where >> 10) & 7;
    		index <<= 2;
    		CHROffsetEX[num] = index & CHR_ROM_01KB_Mask;
    		num++;
    		index++;
    		CHROffsetEX[num] = index & CHR_ROM_01KB_Mask;
    		num++;
    		index++;
    		CHROffsetEX[num] = index & CHR_ROM_01KB_Mask;
    		num++;
    		index++;
    		CHROffsetEX[num] = index & CHR_ROM_01KB_Mask;
    	}

    	private void Switch04kCHRSP(int index, int where)
    	{
    		int num = (where >> 10) & 7;
    		index <<= 2;
    		CHROffsetSP[num] = index & CHR_ROM_01KB_Mask;
    		num++;
    		index++;
    		CHROffsetSP[num] = index & CHR_ROM_01KB_Mask;
    		num++;
    		index++;
    		CHROffsetSP[num] = index & CHR_ROM_01KB_Mask;
    		num++;
    		index++;
    		CHROffsetSP[num] = index & CHR_ROM_01KB_Mask;
    	}

    	internal override void OnPPUScanlineTick()
    	{
    		irq_current_inframe = ((NesEmu.IsInRender() && NesEmu.IsRenderingOn()) ? 64 : 0);
    		if (irq_current_inframe == 0)
    		{
    			irq_current_inframe = 64;
    			irq_current_counter = 0;
    			irq_pending = 0;
    			NesEmu.IRQFlags &= -9;
    			return;
    		}
    		irq_current_counter++;
    		if (irq_current_counter == irq_line)
    		{
    			irq_pending = 128;
    			if (irq_enable == 128)
    			{
    				NesEmu.IRQFlags |= 8;
    			}
    		}
    	}

    	internal override void OnAPUClock()
    	{
    		base.OnAPUClock();
    		snd_1.Clock();
    		snd_2.Clock();
    	}

    	internal override void OnAPUClockEnvelope()
    	{
    		base.OnAPUClockEnvelope();
    		snd_1.ClockLength();
    		snd_2.ClockLength();
    		snd_1.ClockEnvelope();
    		snd_2.ClockEnvelope();
    	}

    	internal override double APUGetSample()
    	{
    		return audio_pulse_table[snd_1.output + snd_2.output] + audio_tnd_table[snd_3.output];
    	}

    	internal override void APUApplyChannelsSettings()
    	{
    		base.APUApplyChannelsSettings();
    		snd_1.Outputable = MyNesMain.RendererSettings.Audio_ChannelEnabled_MMC5_SQ1;
    		snd_2.Outputable = MyNesMain.RendererSettings.Audio_ChannelEnabled_MMC5_SQ2;
    		snd_3.Outputable = MyNesMain.RendererSettings.Audio_ChannelEnabled_MMC5_PCM;
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(ram_protectA);
    		stream.Write(ram_protectB);
    		stream.Write(ExRAM_mode);
    		for (int i = 0; i < CHROffset_spr.Length; i++)
    		{
    			stream.Write(CHROffset_spr[i]);
    		}
    		for (int j = 0; j < CHROffsetEX.Length; j++)
    		{
    			stream.Write(CHROffsetEX[j]);
    		}
    		for (int k = 0; k < CHROffsetSP.Length; k++)
    		{
    			stream.Write(CHROffsetSP[k]);
    		}
    		for (int l = 0; l < chrRegA.Length; l++)
    		{
    			stream.Write(chrRegA[l]);
    		}
    		for (int m = 0; m < chrRegB.Length; m++)
    		{
    			stream.Write(chrRegB[m]);
    		}
    		for (int n = 0; n < prgReg.Length; n++)
    		{
    			stream.Write(prgReg[n]);
    		}
    		stream.Write(useSRAMmirroring);
    		stream.Write(chr_high);
    		stream.Write(chr_mode);
    		stream.Write(prg_mode);
    		stream.Write(chr_setB_last);
    		stream.Write(temp_val);
    		stream.Write(temp_fill);
    		stream.Write(lastAccessVRAM);
    		stream.Write(paletteNo);
    		stream.Write(shift);
    		stream.Write(EXtilenumber);
    		stream.Write(multiplicand);
    		stream.Write(multiplier);
    		stream.Write(product);
    		stream.Write(split_enable);
    		stream.Write(split_right);
    		stream.Write(split_tile);
    		stream.Write(split_yscroll);
    		stream.Write(split_doit);
    		stream.Write(split_watch_tile);
    		stream.Write(irq_line);
    		stream.Write(irq_enable);
    		stream.Write(irq_pending);
    		stream.Write(irq_current_counter);
    		stream.Write(irq_current_inframe);
    		snd_1.WriteStateData(ref stream);
    		snd_2.WriteStateData(ref stream);
    		snd_3.SaveState(ref stream);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		ram_protectA = stream.ReadInt32();
    		ram_protectB = stream.ReadInt32();
    		ExRAM_mode = stream.ReadInt32();
    		for (int i = 0; i < CHROffset_spr.Length; i++)
    		{
    			CHROffset_spr[i] = stream.ReadInt32();
    		}
    		for (int j = 0; j < CHROffsetEX.Length; j++)
    		{
    			CHROffsetEX[j] = stream.ReadInt32();
    		}
    		for (int k = 0; k < CHROffsetSP.Length; k++)
    		{
    			CHROffsetSP[k] = stream.ReadInt32();
    		}
    		for (int l = 0; l < chrRegA.Length; l++)
    		{
    			chrRegA[l] = stream.ReadInt32();
    		}
    		for (int m = 0; m < chrRegB.Length; m++)
    		{
    			chrRegB[m] = stream.ReadInt32();
    		}
    		for (int n = 0; n < prgReg.Length; n++)
    		{
    			prgReg[n] = stream.ReadInt32();
    		}
    		useSRAMmirroring = stream.ReadBoolean();
    		chr_high = stream.ReadInt32();
    		chr_mode = stream.ReadInt32();
    		prg_mode = stream.ReadInt32();
    		chr_setB_last = stream.ReadBoolean();
    		temp_val = stream.ReadByte();
    		temp_fill = stream.ReadByte();
    		lastAccessVRAM = stream.ReadInt32();
    		paletteNo = stream.ReadInt32();
    		shift = stream.ReadInt32();
    		EXtilenumber = stream.ReadInt32();
    		multiplicand = stream.ReadByte();
    		multiplier = stream.ReadByte();
    		product = stream.ReadUInt16();
    		split_enable = stream.ReadBoolean();
    		split_right = stream.ReadBoolean();
    		split_tile = stream.ReadInt32();
    		split_yscroll = stream.ReadInt32();
    		split_doit = stream.ReadBoolean();
    		split_watch_tile = stream.ReadInt32();
    		irq_line = stream.ReadByte();
    		irq_enable = stream.ReadByte();
    		irq_pending = stream.ReadInt32();
    		irq_current_counter = stream.ReadInt32();
    		irq_current_inframe = stream.ReadInt32();
    		snd_1.ReadStateData(ref stream);
    		snd_2.ReadStateData(ref stream);
    		snd_3.LoadState(ref stream);
    	}
    }
}
