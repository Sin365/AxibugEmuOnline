using System.IO;

namespace MyNes.Core
{
    internal class VRC6Pulse
    {
    	private int dutyForm;

    	private int dutyStep;

    	private bool enabled = true;

    	internal bool Outputable;

    	private bool mode;

    	private byte volume;

    	private int freqTimer;

    	private int frequency;

    	private int cycles;

    	internal int output;

    	internal void HardReset()
    	{
    		dutyForm = 0;
    		dutyStep = 15;
    		enabled = true;
    		mode = false;
    		output = 0;
    	}

    	internal void Write0(ref byte data)
    	{
    		mode = (data & 0x80) == 128;
    		dutyForm = (data & 0x70) >> 4;
    		volume = (byte)(data & 0xFu);
    	}

    	internal void Write1(ref byte data)
    	{
    		frequency = (frequency & 0xF00) | data;
    	}

    	internal void Write2(ref byte data)
    	{
    		frequency = (frequency & 0xFF) | ((data & 0xF) << 8);
    		enabled = (data & 0x80) == 128;
    	}

    	internal void ClockSingle()
    	{
    		if (--cycles > 0)
    		{
    			return;
    		}
    		cycles = (frequency << 1) + 2;
    		if (!enabled)
    		{
    			return;
    		}
    		if (mode)
    		{
    			output = volume;
    			return;
    		}
    		dutyStep--;
    		if (dutyStep < 0)
    		{
    			dutyStep = 15;
    		}
    		if (dutyStep <= dutyForm)
    		{
    			if (Outputable)
    			{
    				output = volume;
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
    		stream.Write(dutyForm);
    		stream.Write(dutyStep);
    		stream.Write(enabled);
    		stream.Write(mode);
    		stream.Write(volume);
    		stream.Write(freqTimer);
    		stream.Write(frequency);
    		stream.Write(cycles);
    	}

    	internal void LoadState(ref BinaryReader stream)
    	{
    		dutyForm = stream.ReadInt32();
    		dutyStep = stream.ReadInt32();
    		enabled = stream.ReadBoolean();
    		mode = stream.ReadBoolean();
    		volume = stream.ReadByte();
    		freqTimer = stream.ReadInt32();
    		frequency = stream.ReadInt32();
    		cycles = stream.ReadInt32();
    	}
    }
}
