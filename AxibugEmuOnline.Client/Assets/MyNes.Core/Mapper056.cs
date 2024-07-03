namespace MyNes.Core;

[BoardInfo("Pirate SMB3", 56)]
internal class Mapper056 : Board
{
	private int irqCounter;

	private int irqLatch;

	private bool irqEnabled;

	private int irqControl;

	private int switchControl;

	internal override string Issues => MNInterfaceLanguage.IssueMapper56;

	internal override void HardReset()
	{
		base.HardReset();
		Switch08KPRG(PRG_ROM_08KB_Mask, PRGArea.AreaE000);
		irqLatch = 0;
		irqCounter = 0;
		irqControl = 0;
		irqEnabled = false;
	}

	internal override void WritePRG(ref ushort address, ref byte data)
	{
		if (address < 61440)
		{
			switch (address & 0xE000)
			{
			case 32768:
				irqLatch = (irqLatch & 0xFFF0) | (data & 0xF);
				break;
			case 36864:
				irqLatch = (irqLatch & 0xFF0F) | ((data & 0xF) << 4);
				break;
			case 40960:
				irqLatch = (irqLatch & 0xF0FF) | ((data & 0xF) << 8);
				break;
			case 45056:
				irqLatch = (irqLatch & 0xFFF) | ((data & 0xF) << 12);
				break;
			case 49152:
				irqControl = data & 5;
				irqEnabled = (data & 2) == 2;
				if (irqEnabled)
				{
					irqCounter = irqLatch;
				}
				NesEmu.IRQFlags &= -9;
				break;
			case 53248:
				irqEnabled = (irqControl & 1) == 1;
				NesEmu.IRQFlags &= -9;
				break;
			case 57344:
				switchControl = data;
				break;
			}
			return;
		}
		int num = (switchControl & 0xF) - 1;
		if (num < 3)
		{
			Switch08KPRG((data & 0xF) | (PRG_AREA_BLK_INDEX[(num >> 13) + 1] & 0x10), (PRGArea)num);
		}
		switch (address & 0xC00)
		{
		case 0:
			address &= 3;
			if (address < 3)
			{
				Switch08KPRG((data & 0xF) | (PRG_AREA_BLK_INDEX[(num >> 13) + 1] & 0x10), (PRGArea)address);
			}
			break;
		case 2048:
			Switch01KNMTFromMirroring(((data & 1) == 1) ? Mirroring.Vert : Mirroring.Horz);
			break;
		case 3072:
			Switch01KCHR(data, (CHRArea)(address & 7u));
			break;
		}
	}

	internal override void OnCPUClock()
	{
		if (irqEnabled && irqCounter++ == 65535)
		{
			irqCounter = irqLatch;
			NesEmu.IRQFlags |= 8;
		}
	}
}
