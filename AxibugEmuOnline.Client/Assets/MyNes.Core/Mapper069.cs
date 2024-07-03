using System.IO;

namespace MyNes.Core;

[BoardInfo("FME-7/Sunsoft 5B", 69)]
[WithExternalSound]
internal class Mapper069 : Board
{
	private int address_A000;

	private int address_E000;

	private int irq_counter;

	private bool irq_count_enabled;

	private bool irq_trigger_enabled;

	private Sunsoft5BChnl snd_1;

	private Sunsoft5BChnl snd_2;

	private Sunsoft5BChnl snd_3;

	private double[] audio_pulse_table;

	private double[] audio_tnd_table;

	internal override void Initialize(IRom rom)
	{
		base.Initialize(rom);
		snd_1 = new Sunsoft5BChnl();
		snd_2 = new Sunsoft5BChnl();
		snd_3 = new Sunsoft5BChnl();
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
		Switch08KPRG(PRG_ROM_08KB_Mask, PRGArea.AreaE000);
		address_A000 = 0;
		irq_counter = 65535;
		irq_count_enabled = false;
		irq_trigger_enabled = false;
		APUApplyChannelsSettings();
		snd_1.HardReset();
		snd_2.HardReset();
		snd_3.HardReset();
	}

	internal override void WritePRG(ref ushort address, ref byte data)
	{
		switch (address & 0xE000)
		{
		case 32768:
			address_A000 = data & 0xF;
			break;
		case 40960:
			switch (address_A000)
			{
			case 0:
				Switch01KCHR(data, CHRArea.Area0000);
				break;
			case 1:
				Switch01KCHR(data, CHRArea.Area0400);
				break;
			case 2:
				Switch01KCHR(data, CHRArea.Area0800);
				break;
			case 3:
				Switch01KCHR(data, CHRArea.Area0C00);
				break;
			case 4:
				Switch01KCHR(data, CHRArea.Area1000);
				break;
			case 5:
				Switch01KCHR(data, CHRArea.Area1400);
				break;
			case 6:
				Switch01KCHR(data, CHRArea.Area1800);
				break;
			case 7:
				Switch01KCHR(data, CHRArea.Area1C00);
				break;
			case 8:
				TogglePRGRAMEnable((data & 0x80) == 128);
				if ((data & 0x40u) != 0)
				{
					Toggle08KPRG_RAM(ram: true, PRGArea.Area6000);
					Switch08KPRG(data & 0x3F & PRG_RAM_08KB_Mask, PRGArea.Area6000);
				}
				else
				{
					Toggle08KPRG_RAM(ram: false, PRGArea.Area6000);
					Switch08KPRG(data & 0x3F, PRGArea.Area6000);
				}
				break;
			case 9:
				Switch08KPRG(data, PRGArea.Area8000);
				break;
			case 10:
				Switch08KPRG(data, PRGArea.AreaA000);
				break;
			case 11:
				Switch08KPRG(data, PRGArea.AreaC000);
				break;
			case 12:
				switch (data & 3)
				{
				case 0:
					Switch01KNMTFromMirroring(Mirroring.Vert);
					break;
				case 1:
					Switch01KNMTFromMirroring(Mirroring.Horz);
					break;
				case 2:
					Switch01KNMTFromMirroring(Mirroring.OneScA);
					break;
				case 3:
					Switch01KNMTFromMirroring(Mirroring.OneScB);
					break;
				}
				break;
			case 13:
				irq_count_enabled = (data & 0x80) == 128;
				irq_trigger_enabled = (data & 1) == 1;
				if (!irq_trigger_enabled)
				{
					NesEmu.IRQFlags &= -9;
				}
				break;
			case 14:
				irq_counter = (irq_counter & 0xFF00) | data;
				break;
			case 15:
				irq_counter = (irq_counter & 0xFF) | (data << 8);
				break;
			}
			break;
		case 49152:
			address_E000 = data & 0xF;
			break;
		case 57344:
			switch (address_E000)
			{
			case 0:
				snd_1.Write0(ref data);
				break;
			case 1:
				snd_1.Write1(ref data);
				break;
			case 2:
				snd_2.Write0(ref data);
				break;
			case 3:
				snd_2.Write1(ref data);
				break;
			case 4:
				snd_3.Write0(ref data);
				break;
			case 5:
				snd_3.Write1(ref data);
				break;
			case 7:
				snd_1.Enabled = (data & 1) == 0;
				snd_2.Enabled = (data & 2) == 0;
				snd_3.Enabled = (data & 4) == 0;
				break;
			case 8:
				snd_1.Volume = (byte)(data & 0xFu);
				break;
			case 9:
				snd_2.Volume = (byte)(data & 0xFu);
				break;
			case 10:
				snd_3.Volume = (byte)(data & 0xFu);
				break;
			case 6:
				break;
			}
			break;
		}
	}

	internal override void OnCPUClock()
	{
		if (!irq_count_enabled)
		{
			return;
		}
		irq_counter--;
		if (irq_counter <= 0)
		{
			irq_counter = 65535;
			if (irq_trigger_enabled)
			{
				NesEmu.IRQFlags |= 8;
			}
		}
	}

	internal override double APUGetSample()
	{
		return audio_pulse_table[snd_1.output + snd_2.output] + audio_tnd_table[snd_3.output];
	}

	internal override void OnAPUClockSingle()
	{
		base.OnAPUClockSingle();
		snd_1.ClockSingle();
		snd_2.ClockSingle();
		snd_3.ClockSingle();
	}

	internal override void APUApplyChannelsSettings()
	{
		base.APUApplyChannelsSettings();
		snd_1.Outputable = MyNesMain.RendererSettings.Audio_ChannelEnabled_SUN1;
		snd_2.Outputable = MyNesMain.RendererSettings.Audio_ChannelEnabled_SUN2;
		snd_3.Outputable = MyNesMain.RendererSettings.Audio_ChannelEnabled_SUN3;
	}

	internal override void WriteStateData(ref BinaryWriter stream)
	{
		base.WriteStateData(ref stream);
		stream.Write(address_A000);
		stream.Write(address_E000);
		stream.Write(irq_counter);
		stream.Write(irq_count_enabled);
		stream.Write(irq_trigger_enabled);
		snd_1.SaveState(ref stream);
		snd_2.SaveState(ref stream);
		snd_3.SaveState(ref stream);
	}

	internal override void ReadStateData(ref BinaryReader stream)
	{
		base.ReadStateData(ref stream);
		address_A000 = stream.ReadInt32();
		address_E000 = stream.ReadInt32();
		irq_counter = stream.ReadInt32();
		irq_count_enabled = stream.ReadBoolean();
		irq_trigger_enabled = stream.ReadBoolean();
		snd_1.LoadState(ref stream);
		snd_2.LoadState(ref stream);
		snd_3.LoadState(ref stream);
	}
}
