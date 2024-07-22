using System.IO;
using Unity.IL2CPP.CompilerServices;

namespace MyNes.Core
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [WithExternalSound]
    internal abstract class Namcot106 : Board
    {
    	private int irq_counter;

    	private bool irq_enable;

    	private bool disables_chr_ram_A;

    	private bool disables_chr_ram_B;

    	private bool enable_mirroring_switch;

    	private bool enable_N106_sound;

    	private int temp_nmt;

    	private Namcot106Chnl[] sound_channels;

    	private byte soundReg;

    	public int enabledChannels;

    	private int enabledChannels1;

    	private int channelIndex;

    	private byte temp_val;

    	private byte temp_i;

    	public byte[] EXRAM;

    	private bool[] sound_channels_enable;

    	private double soundOut;

    	private int sound_out_div;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		EXRAM = new byte[128];
    		Switch08KPRG(PRG_ROM_08KB_Mask, PRGArea.AreaE000);
    		enable_mirroring_switch = (enable_N106_sound = base.MapperNumber == 19);
    		switch (SHA1.ToUpper())
    		{
    		case "97E7E61EECB73CB1EA0C15AE51E65EA56301A685":
    		case "3D554F55411AB2DDD1A87E7583E643970DB784F3":
    		case "7FA51058307DB50825C2D3A3A98C0DA554BC3C92":
    		case "1C476C795CFC17E987C22FFD6F09BAF1396ED2C9":
    			enable_mirroring_switch = false;
    			enable_N106_sound = false;
    			break;
    		}
    		if (enable_N106_sound)
    		{
    			sound_channels = new Namcot106Chnl[8];
    			sound_channels_enable = new bool[8];
    			for (int i = 0; i < 8; i++)
    			{
    				sound_channels[i] = new Namcot106Chnl(this);
    				sound_channels[i].HardReset();
    			}
    			soundReg = 0;
    			enabledChannels = 0;
    			channelIndex = 0;
    			APUApplyChannelsSettings();
    		}
    	}

    	internal override void WriteEX(ref ushort address, ref byte data)
    	{
    		switch (address & 0xF800)
    		{
    		case 18432:
    			if (soundReg >= 64)
    			{
    				switch (soundReg & 0x7F)
    				{
    				case 64:
    					sound_channels[0].WriteA(ref data);
    					break;
    				case 66:
    					sound_channels[0].WriteB(ref data);
    					break;
    				case 68:
    					sound_channels[0].WriteC(ref data);
    					break;
    				case 70:
    					sound_channels[0].WriteD(ref data);
    					break;
    				case 71:
    					sound_channels[0].WriteE(ref data);
    					break;
    				case 72:
    					sound_channels[1].WriteA(ref data);
    					break;
    				case 74:
    					sound_channels[1].WriteB(ref data);
    					break;
    				case 76:
    					sound_channels[1].WriteC(ref data);
    					break;
    				case 78:
    					sound_channels[1].WriteD(ref data);
    					break;
    				case 79:
    					sound_channels[1].WriteE(ref data);
    					break;
    				case 80:
    					sound_channels[2].WriteA(ref data);
    					break;
    				case 82:
    					sound_channels[2].WriteB(ref data);
    					break;
    				case 84:
    					sound_channels[2].WriteC(ref data);
    					break;
    				case 86:
    					sound_channels[2].WriteD(ref data);
    					break;
    				case 87:
    					sound_channels[2].WriteE(ref data);
    					break;
    				case 88:
    					sound_channels[3].WriteA(ref data);
    					break;
    				case 90:
    					sound_channels[3].WriteB(ref data);
    					break;
    				case 92:
    					sound_channels[3].WriteC(ref data);
    					break;
    				case 94:
    					sound_channels[3].WriteD(ref data);
    					break;
    				case 95:
    					sound_channels[3].WriteE(ref data);
    					break;
    				case 96:
    					sound_channels[4].WriteA(ref data);
    					break;
    				case 98:
    					sound_channels[4].WriteB(ref data);
    					break;
    				case 100:
    					sound_channels[4].WriteC(ref data);
    					break;
    				case 102:
    					sound_channels[4].WriteD(ref data);
    					break;
    				case 103:
    					sound_channels[4].WriteE(ref data);
    					break;
    				case 104:
    					sound_channels[5].WriteA(ref data);
    					break;
    				case 106:
    					sound_channels[5].WriteB(ref data);
    					break;
    				case 108:
    					sound_channels[5].WriteC(ref data);
    					break;
    				case 110:
    					sound_channels[5].WriteD(ref data);
    					break;
    				case 111:
    					sound_channels[5].WriteE(ref data);
    					break;
    				case 112:
    					sound_channels[6].WriteA(ref data);
    					break;
    				case 114:
    					sound_channels[6].WriteB(ref data);
    					break;
    				case 116:
    					sound_channels[6].WriteC(ref data);
    					break;
    				case 118:
    					sound_channels[6].WriteD(ref data);
    					break;
    				case 119:
    					sound_channels[6].WriteE(ref data);
    					break;
    				case 120:
    					sound_channels[7].WriteA(ref data);
    					break;
    				case 122:
    					sound_channels[7].WriteB(ref data);
    					break;
    				case 124:
    					sound_channels[7].WriteC(ref data);
    					break;
    				case 126:
    					sound_channels[7].WriteD(ref data);
    					break;
    				case 127:
    					sound_channels[7].WriteE(ref data);
    					enabledChannels = (data & 0x70) >> 4;
    					channelIndex = 0;
    					enabledChannels1 = enabledChannels + 1;
    					temp_i = 7;
    					while (temp_i >= 0 && enabledChannels1 > 0)
    					{
    						sound_channels[temp_i].Enabled = true;
    						enabledChannels1--;
    						temp_i--;
    					}
    					break;
    				}
    			}
    			EXRAM[soundReg & 0x7F] = data;
    			if ((soundReg & 0x80) == 128)
    			{
    				soundReg = (byte)(((uint)(soundReg + 1) & 0x7Fu) | 0x80u);
    			}
    			break;
    		case 20480:
    			NesEmu.IRQFlags &= -9;
    			irq_counter = (irq_counter & 0x7F00) | data;
    			break;
    		case 22528:
    			NesEmu.IRQFlags &= -9;
    			irq_counter = (irq_counter & 0xFF) | ((data & 0x7F) << 8);
    			irq_enable = (data & 0x80) == 128;
    			break;
    		}
    	}

    	internal override void ReadEX(ref ushort address, out byte value)
    	{
    		switch (address & 0xF800)
    		{
    		case 18432:
    			value = EXRAM[soundReg & 0x7F];
    			if ((soundReg & 0x80) == 128)
    			{
    				soundReg = (byte)(((uint)(soundReg + 1) & 0x7Fu) | 0x80u);
    			}
    			break;
    		case 20480:
    			NesEmu.IRQFlags &= -9;
    			value = (byte)((uint)irq_counter & 0xFFu);
    			break;
    		case 22528:
    			NesEmu.IRQFlags &= -9;
    			value = (byte)((irq_enable ? 128u : 0u) | (uint)((irq_counter & 0x7F00) >> 8));
    			break;
    		default:
    			value = 0;
    			break;
    		}
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		switch (address & 0xF800)
    		{
    		case 32768:
    			if (!disables_chr_ram_A)
    			{
    				Toggle01KCHR_RAM(data >= 224, CHRArea.Area0000);
    				Switch01KCHR((data >= 224) ? (data - 224) : data, CHRArea.Area0000);
    			}
    			else
    			{
    				Toggle01KCHR_RAM(ram: false, CHRArea.Area0000);
    				Switch01KCHR(data, CHRArea.Area0000);
    			}
    			break;
    		case 34816:
    			if (!disables_chr_ram_A)
    			{
    				Toggle01KCHR_RAM(data >= 224, CHRArea.Area0400);
    				Switch01KCHR((data >= 224) ? (data - 224) : data, CHRArea.Area0400);
    			}
    			else
    			{
    				Toggle01KCHR_RAM(ram: false, CHRArea.Area0400);
    				Switch01KCHR(data, CHRArea.Area0400);
    			}
    			break;
    		case 36864:
    			if (!disables_chr_ram_A)
    			{
    				Toggle01KCHR_RAM(data >= 224, CHRArea.Area0800);
    				Switch01KCHR((data >= 224) ? (data - 224) : data, CHRArea.Area0800);
    			}
    			else
    			{
    				Toggle01KCHR_RAM(ram: false, CHRArea.Area0800);
    				Switch01KCHR(data, CHRArea.Area0800);
    			}
    			break;
    		case 38912:
    			if (!disables_chr_ram_A)
    			{
    				Toggle01KCHR_RAM(data >= 224, CHRArea.Area0C00);
    				Switch01KCHR((data >= 224) ? (data - 224) : data, CHRArea.Area0C00);
    			}
    			else
    			{
    				Toggle01KCHR_RAM(ram: false, CHRArea.Area0C00);
    				Switch01KCHR(data, CHRArea.Area0C00);
    			}
    			break;
    		case 40960:
    			if (!disables_chr_ram_B)
    			{
    				Toggle01KCHR_RAM(data >= 224, CHRArea.Area1000);
    				Switch01KCHR((data >= 224) ? (data - 224) : data, CHRArea.Area1000);
    			}
    			else
    			{
    				Toggle01KCHR_RAM(ram: false, CHRArea.Area1000);
    				Switch01KCHR(data, CHRArea.Area1000);
    			}
    			break;
    		case 43008:
    			if (!disables_chr_ram_B)
    			{
    				Toggle01KCHR_RAM(data >= 224, CHRArea.Area1400);
    				Switch01KCHR((data >= 224) ? (data - 224) : data, CHRArea.Area1400);
    			}
    			else
    			{
    				Toggle01KCHR_RAM(ram: false, CHRArea.Area1400);
    				Switch01KCHR(data, CHRArea.Area1400);
    			}
    			break;
    		case 45056:
    			if (!disables_chr_ram_B)
    			{
    				Toggle01KCHR_RAM(data >= 224, CHRArea.Area1800);
    				Switch01KCHR((data >= 224) ? (data - 224) : data, CHRArea.Area1800);
    			}
    			else
    			{
    				Toggle01KCHR_RAM(ram: false, CHRArea.Area1800);
    				Switch01KCHR(data, CHRArea.Area1800);
    			}
    			break;
    		case 47104:
    			if (!disables_chr_ram_B)
    			{
    				Toggle01KCHR_RAM(data >= 224, CHRArea.Area1C00);
    				Switch01KCHR((data >= 224) ? (data - 224) : data, CHRArea.Area1C00);
    			}
    			else
    			{
    				Toggle01KCHR_RAM(ram: false, CHRArea.Area1C00);
    				Switch01KCHR(data, CHRArea.Area1C00);
    			}
    			break;
    		case 49152:
    			if (enable_mirroring_switch)
    			{
    				NMT_AREA_BLK_INDEX[0] = data;
    			}
    			break;
    		case 51200:
    			if (enable_mirroring_switch)
    			{
    				NMT_AREA_BLK_INDEX[1] = data;
    			}
    			break;
    		case 53248:
    			if (enable_mirroring_switch)
    			{
    				NMT_AREA_BLK_INDEX[2] = data;
    			}
    			break;
    		case 55296:
    			if (enable_mirroring_switch)
    			{
    				NMT_AREA_BLK_INDEX[3] = data;
    			}
    			break;
    		case 57344:
    			Switch08KPRG(data & 0x3F, PRGArea.Area8000);
    			break;
    		case 59392:
    			Switch08KPRG(data & 0x3F, PRGArea.AreaA000);
    			disables_chr_ram_A = (data & 0x40) == 64;
    			disables_chr_ram_B = (data & 0x80) == 128;
    			break;
    		case 61440:
    			Switch08KPRG(data & 0x3F, PRGArea.AreaC000);
    			break;
    		case 63488:
    			soundReg = data;
    			break;
    		}
    	}

    	internal override void ReadNMT(ref ushort address, out byte data)
    	{
    		if (enable_mirroring_switch)
    		{
    			temp_nmt = NMT_AREA_BLK_INDEX[(address >> 10) & 3];
    			if (temp_nmt >= 224)
    			{
    				data = NMT_RAM[(temp_nmt - 224) & 1][address & 0x3FF];
    			}
    			else
    			{
    				data = CHR_ROM[temp_nmt][address & 0x3FF];
    			}
    		}
    		else
    		{
    			base.ReadNMT(ref address, out data);
    		}
    	}

    	internal override void WriteNMT(ref ushort address, ref byte data)
    	{
    		if (enable_mirroring_switch)
    		{
    			temp_nmt = NMT_AREA_BLK_INDEX[(address >> 10) & 3];
    			if (temp_nmt >= 224)
    			{
    				NMT_RAM[(temp_nmt - 224) & 1][address & 0x3FF] = data;
    			}
    		}
    		else
    		{
    			base.WriteNMT(ref address, ref data);
    		}
    	}

    	internal override void OnCPUClock()
    	{
    		if (irq_enable)
    		{
    			if (irq_counter == 32767)
    			{
    				NesEmu.IRQFlags |= 8;
    				irq_counter = 0;
    			}
    			else
    			{
    				irq_counter++;
    			}
    		}
    	}

    	internal override void OnAPUClockSingle()
    	{
    		if (sound_channels != null)
    		{
    			for (int i = 0; i < sound_channels.Length; i++)
    			{
    				sound_channels[i].ClockSingle();
    			}
    		}
    	}

    	internal override double APUGetSample()
    	{
    		soundOut = 0.0;
    		sound_out_div = 0;
    		if (enabledChannels > 0)
    		{
    			for (int i = 0; i < sound_channels.Length; i++)
    			{
    				if (sound_channels[i].Enabled && sound_channels_enable[i])
    				{
    					if (sound_channels[i].clocks > 0)
    					{
    						sound_channels[i].output = sound_channels[i].output_av / sound_channels[i].clocks;
    					}
    					sound_channels[i].clocks = (sound_channels[i].output_av = 0);
    					soundOut += sound_channels[i].output;
    					sound_out_div++;
    				}
    			}
    			soundOut = soundOut / 8.0 / 225.0;
    		}
    		return soundOut;
    	}

    	internal override void APUApplyChannelsSettings()
    	{
    		base.APUApplyChannelsSettings();
    		sound_channels_enable[0] = MyNesMain.RendererSettings.Audio_ChannelEnabled_NMT1;
    		sound_channels_enable[1] = MyNesMain.RendererSettings.Audio_ChannelEnabled_NMT2;
    		sound_channels_enable[2] = MyNesMain.RendererSettings.Audio_ChannelEnabled_NMT3;
    		sound_channels_enable[3] = MyNesMain.RendererSettings.Audio_ChannelEnabled_NMT4;
    		sound_channels_enable[4] = MyNesMain.RendererSettings.Audio_ChannelEnabled_NMT5;
    		sound_channels_enable[5] = MyNesMain.RendererSettings.Audio_ChannelEnabled_NMT6;
    		sound_channels_enable[6] = MyNesMain.RendererSettings.Audio_ChannelEnabled_NMT7;
    		sound_channels_enable[7] = MyNesMain.RendererSettings.Audio_ChannelEnabled_NMT8;
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(irq_counter);
    		stream.Write(irq_enable);
    		stream.Write(disables_chr_ram_A);
    		stream.Write(disables_chr_ram_B);
    		stream.Write(enable_mirroring_switch);
    		stream.Write(enable_N106_sound);
    		stream.Write(temp_nmt);
    		if (enable_N106_sound)
    		{
    			for (int i = 0; i < sound_channels.Length; i++)
    			{
    				sound_channels[i].SaveState(stream);
    			}
    		}
    		stream.Write(soundReg);
    		stream.Write(enabledChannels);
    		stream.Write(enabledChannels1);
    		stream.Write(channelIndex);
    		stream.Write(temp_val);
    		stream.Write(temp_i);
    		stream.Write(EXRAM);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		irq_counter = stream.ReadInt32();
    		irq_enable = stream.ReadBoolean();
    		disables_chr_ram_A = stream.ReadBoolean();
    		disables_chr_ram_B = stream.ReadBoolean();
    		enable_mirroring_switch = stream.ReadBoolean();
    		enable_N106_sound = stream.ReadBoolean();
    		temp_nmt = stream.ReadInt32();
    		if (enable_N106_sound)
    		{
    			for (int i = 0; i < sound_channels.Length; i++)
    			{
    				sound_channels[i].LoadState(stream);
    			}
    		}
    		soundReg = stream.ReadByte();
    		enabledChannels = stream.ReadInt32();
    		enabledChannels1 = stream.ReadInt32();
    		channelIndex = stream.ReadInt32();
    		temp_val = stream.ReadByte();
    		temp_i = stream.ReadByte();
    		stream.Read(EXRAM, 0, EXRAM.Length);
    	}
    }
}
