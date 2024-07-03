using System.IO;

namespace MyNes.Core;

[BoardInfo("VRC3", 73)]
internal class Mapper073 : Board
{
	private bool irq_mode_8;

	private bool irq_enable;

	private bool irq_enable_on_ak;

	private int irq_reload;

	private int irq_counter;

	internal override void HardReset()
	{
		base.HardReset();
		Switch16KPRG(PRG_ROM_16KB_Mask, PRGArea.AreaC000);
		irq_mode_8 = false;
		irq_enable = false;
		irq_enable_on_ak = false;
		irq_reload = 0;
		irq_counter = 0;
	}

	internal override void WritePRG(ref ushort address, ref byte data)
	{
		switch (address & 0xF000)
		{
		case 32768:
			irq_reload = (irq_reload & 0xFFF0) | (data & 0xF);
			break;
		case 36864:
			irq_reload = (irq_reload & 0xFF0F) | ((data & 0xF) << 4);
			break;
		case 40960:
			irq_reload = (irq_reload & 0xF0FF) | ((data & 0xF) << 8);
			break;
		case 45056:
			irq_reload = (irq_reload & 0xFFF) | ((data & 0xF) << 12);
			break;
		case 49152:
			irq_mode_8 = (data & 4) == 4;
			irq_enable = (data & 2) == 2;
			irq_enable_on_ak = (data & 1) == 1;
			if (irq_enable)
			{
				irq_counter = irq_reload;
			}
			NesEmu.IRQFlags &= -9;
			break;
		case 53248:
			irq_enable = irq_enable_on_ak;
			NesEmu.IRQFlags &= -9;
			break;
		case 61440:
			Switch16KPRG(data & 0xF, PRGArea.Area8000);
			break;
		}
	}

	internal override void OnCPUClock()
	{
		if (!irq_enable)
		{
			return;
		}
		if (irq_mode_8)
		{
			irq_counter = (irq_counter & 0xFF00) | (byte)((irq_counter & 0xFF) + 1);
			if ((byte)(irq_counter & 0xFF) == byte.MaxValue)
			{
				NesEmu.IRQFlags |= 8;
				irq_counter = (irq_counter & 0xFF00) | (irq_reload & 0xFF);
			}
		}
		else
		{
			irq_counter++;
			if (irq_counter == 65535)
			{
				NesEmu.IRQFlags |= 8;
				irq_counter = irq_reload;
			}
		}
	}

	internal override void WriteStateData(ref BinaryWriter stream)
	{
		base.WriteStateData(ref stream);
		stream.Write(irq_mode_8);
		stream.Write(irq_enable);
		stream.Write(irq_enable_on_ak);
		stream.Write(irq_reload);
		stream.Write(irq_counter);
	}

	internal override void ReadStateData(ref BinaryReader stream)
	{
		base.ReadStateData(ref stream);
		irq_mode_8 = stream.ReadBoolean();
		irq_enable = stream.ReadBoolean();
		irq_enable_on_ak = stream.ReadBoolean();
		irq_reload = stream.ReadInt32();
		irq_counter = stream.ReadInt32();
	}
}
