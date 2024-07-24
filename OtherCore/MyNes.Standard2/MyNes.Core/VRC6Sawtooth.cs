using System.IO;

namespace MyNes.Core
{
    internal class VRC6Sawtooth
    {
    	private byte AccumRate;

    	private int accumClock;

    	private byte accumulationRegister;

    	private int frequency;

    	private int freqTimer;

    	private int cycles;

    	private bool enabled;

    	internal int output;

    	internal bool Outputable;

    	internal void HardReset()
    	{
    	}

    	private void UpdateFrequency()
    	{
    		freqTimer = (frequency + 1) * 2;
    	}

    	internal void Write0(ref byte data)
    	{
    		AccumRate = (byte)(data & 0x3Fu);
    	}

    	internal void Write1(ref byte data)
    	{
    		frequency = (frequency & 0xF00) | data;
    		UpdateFrequency();
    	}

    	internal void Write2(ref byte data)
    	{
    		frequency = (frequency & 0xFF) | ((data & 0xF) << 8);
    		enabled = (data & 0x80) == 128;
    		UpdateFrequency();
    	}

    	internal void ClockSingle()
    	{
    		if (--cycles > 0)
    		{
    			return;
    		}
    		cycles = freqTimer;
    		if (enabled)
    		{
    			accumClock++;
    			switch (++accumClock)
    			{
    			case 2:
    			case 4:
    			case 6:
    			case 8:
    			case 10:
    			case 12:
    				accumulationRegister += AccumRate;
    				break;
    			case 14:
    				accumulationRegister = 0;
    				accumClock = 0;
    				break;
    			}
    			if (Outputable)
    			{
    				output = (accumulationRegister >> 3) & 0x1F;
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
    		stream.Write(AccumRate);
    		stream.Write(accumClock);
    		stream.Write(accumulationRegister);
    		stream.Write(frequency);
    		stream.Write(freqTimer);
    		stream.Write(cycles);
    		stream.Write(enabled);
    	}

    	internal void LoadState(ref BinaryReader stream)
    	{
    		AccumRate = stream.ReadByte();
    		accumClock = stream.ReadInt32();
    		accumulationRegister = stream.ReadByte();
    		frequency = stream.ReadInt32();
    		freqTimer = stream.ReadInt32();
    		cycles = stream.ReadInt32();
    		enabled = stream.ReadBoolean();
    	}
    }
}
