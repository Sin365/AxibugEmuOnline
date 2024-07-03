using System.IO;

namespace MyNes.Core;

internal class MMC5Pcm
{
	internal byte output;

	internal bool Outputable;

	private bool readMode;

	private bool PCMIRQenable;

	private bool irqTrip;

	internal void HardReset()
	{
		output = 0;
		readMode = false;
		PCMIRQenable = false;
		irqTrip = false;
	}

	internal void SoftReset()
	{
		HardReset();
	}

	internal void Write5010(byte data)
	{
		readMode = (data & 1) == 1;
		PCMIRQenable = (data & 0x80) == 128;
		if (PCMIRQenable && irqTrip)
		{
			NesEmu.IRQFlags |= 8;
		}
	}

	internal byte Read5010()
	{
		byte result = (byte)((irqTrip & PCMIRQenable) ? 128 : 0);
		irqTrip = false;
		NesEmu.IRQFlags &= -9;
		return result;
	}

	internal void Write5011(byte data)
	{
		if (readMode)
		{
			return;
		}
		if (data == 0)
		{
			irqTrip = true;
		}
		else
		{
			irqTrip = false;
			if (Outputable)
			{
				output = data;
			}
		}
		if (PCMIRQenable && irqTrip)
		{
			NesEmu.IRQFlags |= 8;
		}
	}

	internal void SaveState(ref BinaryWriter stream)
	{
		stream.Write(readMode);
		stream.Write(PCMIRQenable);
		stream.Write(irqTrip);
	}

	internal void LoadState(ref BinaryReader stream)
	{
		readMode = stream.ReadBoolean();
		PCMIRQenable = stream.ReadBoolean();
		irqTrip = stream.ReadBoolean();
	}
}
