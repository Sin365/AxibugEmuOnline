using System.IO;

namespace MyNes.Core
{
    internal class Namcot106Chnl
    {
    	private Namcot106 namcot;

    	private int freqTimer;

    	private int frequency;

    	public int output;

    	public int output_av;

    	public int clocks;

    	private int cycles;

    	private int InstrumentLength;

    	private byte InstrumentAddress;

    	private int startPoint;

    	private int endPoint;

    	private int readPoint;

    	public int volume;

    	private bool freez;

    	public bool Enabled { get; set; }

    	public Namcot106Chnl(Namcot106 namcot)
    	{
    		this.namcot = namcot;
    	}

    	public void HardReset()
    	{
    	}

    	private void UpdateFrequency()
    	{
    		if (frequency > 0)
    		{
    			freqTimer = 983040 * (namcot.enabledChannels + 1) / frequency;
    			freez = false;
    		}
    		else
    		{
    			freez = true;
    			output_av = 0;
    		}
    	}

    	private void UpdatePlaybackParameters()
    	{
    		startPoint = InstrumentAddress;
    		endPoint = InstrumentAddress + 4 * (8 - InstrumentLength);
    		readPoint = InstrumentAddress;
    	}

    	public void WriteA(ref byte data)
    	{
    		frequency = (frequency & 0xFFFF00) | data;
    		UpdateFrequency();
    	}

    	public void WriteB(ref byte data)
    	{
    		frequency = (frequency & 0xFF00FF) | (data << 8);
    		UpdateFrequency();
    	}

    	public void WriteC(ref byte data)
    	{
    		frequency = (frequency & 0xFFFF) | ((data & 3) << 12);
    		InstrumentLength = (data >> 2) & 7;
    		UpdateFrequency();
    		UpdatePlaybackParameters();
    	}

    	public void WriteD(ref byte data)
    	{
    		InstrumentAddress = data;
    		UpdatePlaybackParameters();
    	}

    	public void WriteE(ref byte data)
    	{
    		volume = data & 0xF;
    	}

    	public void ClockSingle()
    	{
    		if (freez || --cycles > 0)
    		{
    			return;
    		}
    		cycles = freqTimer;
    		if (readPoint >= startPoint && readPoint <= endPoint)
    		{
    			if (Enabled && !freez)
    			{
    				if ((readPoint & 1) == 0)
    				{
    					output_av += (namcot.EXRAM[readPoint] & 0xF) * volume;
    				}
    				else
    				{
    					output_av += ((namcot.EXRAM[readPoint] >> 4) & 0xF) * volume;
    				}
    			}
    			readPoint++;
    		}
    		else
    		{
    			readPoint = startPoint;
    		}
    		clocks++;
    	}

    	public void SaveState(BinaryWriter stream)
    	{
    		stream.Write(freqTimer);
    		stream.Write(frequency);
    		stream.Write(output);
    		stream.Write(cycles);
    		stream.Write(InstrumentLength);
    		stream.Write(InstrumentAddress);
    		stream.Write(startPoint);
    		stream.Write(endPoint);
    		stream.Write(readPoint);
    		stream.Write(volume);
    		stream.Write(freez);
    	}

    	public void LoadState(BinaryReader stream)
    	{
    		freqTimer = stream.ReadInt32();
    		frequency = stream.ReadInt32();
    		output = stream.ReadByte();
    		cycles = stream.ReadInt32();
    		InstrumentLength = stream.ReadInt32();
    		InstrumentAddress = stream.ReadByte();
    		startPoint = stream.ReadInt32();
    		endPoint = stream.ReadInt32();
    		readPoint = stream.ReadInt32();
    		volume = stream.ReadInt32();
    		freez = stream.ReadBoolean();
    	}
    }
}
