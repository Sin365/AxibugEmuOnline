using System.IO;

namespace MyNes.Core;

[BoardInfo("FDS-Port - Alt. Levels", 50)]
internal class Mapper050 : Board
{
	private int prg_page;

	private int irq_counter;

	private bool irq_enabled;

	internal override void HardReset()
	{
		base.HardReset();
		Switch08KPRG(15, PRGArea.Area6000);
		Switch08KPRG(8, PRGArea.Area8000);
		Switch08KPRG(9, PRGArea.AreaA000);
		Switch08KPRG(11, PRGArea.AreaE000);
	}

	internal override void WriteEX(ref ushort address, ref byte data)
	{
		switch (address & 0x4120)
		{
		case 16416:
			prg_page = (data & 8) | ((data & 1) << 2) | ((data >> 1) & 3);
			Switch08KPRG(prg_page, PRGArea.AreaC000);
			break;
		case 16672:
			irq_enabled = (data & 1) == 1;
			if (!irq_enabled)
			{
				irq_counter = 0;
				NesEmu.IRQFlags &= -9;
			}
			break;
		}
	}

	internal override void OnCPUClock()
	{
		if (irq_enabled)
		{
			irq_counter++;
			if (irq_counter == 4096)
			{
				NesEmu.IRQFlags |= 8;
				irq_counter = 0;
			}
		}
	}

	internal override void WriteStateData(ref BinaryWriter stream)
	{
		base.WriteStateData(ref stream);
		stream.Write(prg_page);
		stream.Write(irq_counter);
		stream.Write(irq_enabled);
	}

	internal override void ReadStateData(ref BinaryReader stream)
	{
		base.ReadStateData(ref stream);
		prg_page = stream.ReadInt32();
		irq_counter = stream.ReadInt32();
		irq_enabled = stream.ReadBoolean();
	}
}
