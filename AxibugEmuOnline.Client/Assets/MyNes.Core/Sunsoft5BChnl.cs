using System.IO;

namespace MyNes.Core;

internal class Sunsoft5BChnl
{
	internal bool Enabled;

	internal byte Volume;

	private int dutyStep;

	private int freqTimer;

	private int frequency;

	private int cycles;

	internal int output;

	internal bool Outputable;

	internal void HardReset()
	{
	}

	internal void SoftReset()
	{
	}

	internal void Write0(ref byte data)
	{
		frequency = (frequency & 0xF00) | data;
		freqTimer = (frequency + 1) * 2;
	}

	internal void Write1(ref byte data)
	{
		frequency = (frequency & 0xFF) | ((data & 0xF) << 8);
		freqTimer = (frequency + 1) * 2;
	}

	internal void ClockSingle()
	{
		if (--cycles > 0)
		{
			return;
		}
		cycles = freqTimer;
		dutyStep = (dutyStep + 1) & 0x1F;
		if (dutyStep <= 15)
		{
			if (Enabled)
			{
				if (Outputable)
				{
					output = Volume;
				}
				else
				{
					output = 0;
				}
			}
			else
			{
				output = 0;
			}
		}
		else
		{
			output = 0;
		}
	}

	internal void SaveState(ref BinaryWriter stream)
	{
		stream.Write(Enabled);
		stream.Write(Volume);
		stream.Write(dutyStep);
		stream.Write(freqTimer);
		stream.Write(frequency);
		stream.Write(cycles);
	}

	internal void LoadState(ref BinaryReader stream)
	{
		Enabled = stream.ReadBoolean();
		Volume = stream.ReadByte();
		dutyStep = stream.ReadInt32();
		freqTimer = stream.ReadInt32();
		frequency = stream.ReadInt32();
		cycles = stream.ReadInt32();
	}
}
