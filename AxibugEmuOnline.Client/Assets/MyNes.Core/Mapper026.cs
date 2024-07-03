using System.IO;

namespace MyNes.Core
{
    [BoardInfo("VRC6", 26)]
    [WithExternalSound]
    internal class Mapper026 : Board
    {
    	private int irq_reload;

    	private int irq_counter;

    	private int prescaler;

    	private bool irq_mode_cycle;

    	private bool irq_enable;

    	private bool irq_enable_on_ak;

    	private VRC6Pulse snd_1;

    	private VRC6Pulse snd_2;

    	private VRC6Sawtooth snd_3;

    	private double[] audio_pulse_table;

    	private double[] audio_tnd_table;

    	internal override void Initialize(IRom rom)
    	{
    		base.Initialize(rom);
    		snd_1 = new VRC6Pulse();
    		snd_2 = new VRC6Pulse();
    		snd_3 = new VRC6Sawtooth();
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
    		APUApplyChannelsSettings();
    		snd_1.HardReset();
    		snd_2.HardReset();
    		snd_3.HardReset();
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		switch (address)
    		{
    		case 32768:
    		case 32769:
    		case 32770:
    		case 32771:
    			Switch16KPRG(data, PRGArea.Area8000);
    			break;
    		case 36864:
    			snd_1.Write0(ref data);
    			break;
    		case 36866:
    			snd_1.Write1(ref data);
    			break;
    		case 36865:
    			snd_1.Write2(ref data);
    			break;
    		case 40960:
    			snd_2.Write0(ref data);
    			break;
    		case 40962:
    			snd_2.Write1(ref data);
    			break;
    		case 40961:
    			snd_2.Write2(ref data);
    			break;
    		case 45056:
    			snd_3.Write0(ref data);
    			break;
    		case 45058:
    			snd_3.Write1(ref data);
    			break;
    		case 45057:
    			snd_3.Write2(ref data);
    			break;
    		case 45059:
    			switch ((data & 0xC) >> 2)
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
    		case 49152:
    		case 49153:
    		case 49154:
    		case 49155:
    			Switch08KPRG(data, PRGArea.AreaC000);
    			break;
    		case 53248:
    			Switch01KCHR(data, CHRArea.Area0000);
    			break;
    		case 53250:
    			Switch01KCHR(data, CHRArea.Area0400);
    			break;
    		case 53249:
    			Switch01KCHR(data, CHRArea.Area0800);
    			break;
    		case 53251:
    			Switch01KCHR(data, CHRArea.Area0C00);
    			break;
    		case 57344:
    			Switch01KCHR(data, CHRArea.Area1000);
    			break;
    		case 57346:
    			Switch01KCHR(data, CHRArea.Area1400);
    			break;
    		case 57345:
    			Switch01KCHR(data, CHRArea.Area1800);
    			break;
    		case 57347:
    			Switch01KCHR(data, CHRArea.Area1C00);
    			break;
    		case 61440:
    			irq_reload = data;
    			break;
    		case 61442:
    			irq_mode_cycle = (data & 4) == 4;
    			irq_enable = (data & 2) == 2;
    			irq_enable_on_ak = (data & 1) == 1;
    			if (irq_enable)
    			{
    				irq_counter = irq_reload;
    				prescaler = 341;
    			}
    			NesEmu.IRQFlags &= -9;
    			break;
    		case 61441:
    			NesEmu.IRQFlags &= -9;
    			irq_enable = irq_enable_on_ak;
    			break;
    		}
    	}

    	internal override void OnCPUClock()
    	{
    		if (!irq_enable)
    		{
    			return;
    		}
    		if (!irq_mode_cycle)
    		{
    			if (prescaler > 0)
    			{
    				prescaler -= 3;
    				return;
    			}
    			prescaler = 341;
    			irq_counter++;
    			if (irq_counter == 255)
    			{
    				NesEmu.IRQFlags |= 8;
    				irq_counter = irq_reload;
    			}
    		}
    		else
    		{
    			irq_counter++;
    			if (irq_counter == 255)
    			{
    				NesEmu.IRQFlags |= 8;
    				irq_counter = irq_reload;
    			}
    		}
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
    		snd_1.Outputable = MyNesMain.RendererSettings.Audio_ChannelEnabled_VRC6_SQ1;
    		snd_2.Outputable = MyNesMain.RendererSettings.Audio_ChannelEnabled_VRC6_SQ2;
    		snd_3.Outputable = MyNesMain.RendererSettings.Audio_ChannelEnabled_VRC6_SAW;
    	}

    	internal override double APUGetSample()
    	{
    		return audio_pulse_table[snd_1.output + snd_2.output] + audio_tnd_table[snd_3.output];
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(irq_reload);
    		stream.Write(irq_counter);
    		stream.Write(prescaler);
    		stream.Write(irq_mode_cycle);
    		stream.Write(irq_enable);
    		stream.Write(irq_enable_on_ak);
    		snd_1.SaveState(ref stream);
    		snd_2.SaveState(ref stream);
    		snd_3.SaveState(ref stream);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		irq_reload = stream.ReadInt32();
    		irq_counter = stream.ReadInt32();
    		prescaler = stream.ReadInt32();
    		irq_mode_cycle = stream.ReadBoolean();
    		irq_enable = stream.ReadBoolean();
    		irq_enable_on_ak = stream.ReadBoolean();
    		snd_1.LoadState(ref stream);
    		snd_2.LoadState(ref stream);
    		snd_3.LoadState(ref stream);
    	}
    }
}
