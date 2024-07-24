using System;
using System.IO;
using System.Text;

namespace MyNes.Core
{
    public class WaveRecorder
    {
    	private string _fileName;

    	private Stream STR;

    	private bool _IsRecording;

    	private int SIZE;

    	private int NoOfSamples;

    	private int _Time;

    	private int TimeSamples;

    	private short channels;

    	private short bitsPerSample;

    	private int Frequency;

    	public int Time => _Time;

    	public bool IsRecording => _IsRecording;

    	public void Record(string FilePath, short channels, short bitsPerSample, int Frequency)
    	{
    		_fileName = FilePath;
    		this.channels = channels;
    		this.bitsPerSample = bitsPerSample;
    		this.Frequency = Frequency;
    		_Time = 0;
    		STR = new FileStream(FilePath, FileMode.Create);
    		ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
    		STR.Write(aSCIIEncoding.GetBytes("RIFF"), 0, 4);
    		STR.WriteByte(0);
    		STR.WriteByte(0);
    		STR.WriteByte(0);
    		STR.WriteByte(0);
    		STR.Write(aSCIIEncoding.GetBytes("WAVE"), 0, 4);
    		STR.Write(aSCIIEncoding.GetBytes("fmt "), 0, 4);
    		STR.WriteByte(16);
    		STR.WriteByte(0);
    		STR.WriteByte(0);
    		STR.WriteByte(0);
    		STR.WriteByte(1);
    		STR.WriteByte(0);
    		STR.WriteByte((byte)((uint)channels & 0xFFu));
    		STR.WriteByte((byte)((channels & 0xFF00) >> 8));
    		STR.WriteByte((byte)((uint)Frequency & 0xFFu));
    		STR.WriteByte((byte)((Frequency & 0xFF00) >> 8));
    		STR.WriteByte((byte)((Frequency & 0xFF0000) >> 16));
    		STR.WriteByte((byte)((Frequency & 0xFF000000u) >> 24));
    		int num = Frequency * channels * (bitsPerSample / 8);
    		STR.WriteByte((byte)((uint)num & 0xFFu));
    		STR.WriteByte((byte)((num & 0xFF00) >> 8));
    		STR.WriteByte((byte)((num & 0xFF0000) >> 16));
    		STR.WriteByte((byte)((num & 0xFF000000u) >> 24));
    		short num2 = (short)(channels * (bitsPerSample / 8));
    		STR.WriteByte((byte)((uint)num2 & 0xFFu));
    		STR.WriteByte((byte)((num2 & 0xFF00) >> 8));
    		STR.WriteByte((byte)((uint)bitsPerSample & 0xFFu));
    		STR.WriteByte((byte)((bitsPerSample & 0xFF00) >> 8));
    		STR.Write(aSCIIEncoding.GetBytes("data"), 0, 4);
    		STR.WriteByte(0);
    		STR.WriteByte(0);
    		STR.WriteByte(0);
    		STR.WriteByte(0);
    		_IsRecording = true;
    	}

    	public void AddBuffer(ref byte[] buffer)
    	{
    		for (int i = 0; i < buffer.Length; i++)
    		{
    			switch (channels)
    			{
    			case 1:
    				switch (bitsPerSample)
    				{
    				case 8:
    					STR.WriteByte(buffer[i]);
    					NoOfSamples++;
    					TimeSamples++;
    					if (TimeSamples >= Frequency)
    					{
    						_Time++;
    						TimeSamples = 0;
    					}
    					break;
    				case 16:
    					STR.WriteByte(buffer[i]);
    					i++;
    					STR.WriteByte(buffer[i]);
    					NoOfSamples++;
    					TimeSamples++;
    					if (TimeSamples >= Frequency)
    					{
    						_Time++;
    						TimeSamples = 0;
    					}
    					break;
    				case 32:
    					STR.WriteByte(buffer[i]);
    					i++;
    					STR.WriteByte(buffer[i]);
    					i++;
    					STR.WriteByte(buffer[i]);
    					i++;
    					STR.WriteByte(buffer[i]);
    					NoOfSamples++;
    					TimeSamples++;
    					if (TimeSamples >= Frequency)
    					{
    						_Time++;
    						TimeSamples = 0;
    					}
    					break;
    				}
    				break;
    			case 2:
    				switch (bitsPerSample)
    				{
    				case 8:
    					STR.WriteByte(buffer[i]);
    					STR.WriteByte(buffer[i]);
    					NoOfSamples++;
    					TimeSamples++;
    					if (TimeSamples >= Frequency)
    					{
    						_Time++;
    						TimeSamples = 0;
    					}
    					break;
    				case 16:
    					STR.WriteByte(buffer[i]);
    					STR.WriteByte(buffer[i]);
    					i++;
    					STR.WriteByte(buffer[i]);
    					STR.WriteByte(buffer[i]);
    					NoOfSamples++;
    					TimeSamples++;
    					if (TimeSamples >= Frequency)
    					{
    						_Time++;
    						TimeSamples = 0;
    					}
    					break;
    				case 32:
    					STR.WriteByte(buffer[i]);
    					i++;
    					STR.WriteByte(buffer[i]);
    					STR.WriteByte(buffer[i]);
    					i++;
    					STR.WriteByte(buffer[i]);
    					STR.WriteByte(buffer[i]);
    					i++;
    					STR.WriteByte(buffer[i]);
    					STR.WriteByte(buffer[i]);
    					i++;
    					STR.WriteByte(buffer[i]);
    					NoOfSamples++;
    					TimeSamples++;
    					if (TimeSamples >= Frequency)
    					{
    						_Time++;
    						TimeSamples = 0;
    					}
    					break;
    				}
    				break;
    			}
    		}
    	}

    	public void AddSample(int Sample)
    	{
    		if (!_IsRecording)
    		{
    			return;
    		}
    		switch (channels)
    		{
    		case 1:
    			switch (bitsPerSample)
    			{
    			case 8:
    				AddSample_mono_08(Sample);
    				break;
    			case 16:
    				AddSample_mono_16(Sample);
    				break;
    			case 32:
    				AddSample_mono_32(Sample);
    				break;
    			}
    			break;
    		case 2:
    			switch (bitsPerSample)
    			{
    			case 8:
    				AddSample_stereo_08(Sample);
    				break;
    			case 16:
    				AddSample_stereo_16(Sample);
    				break;
    			case 32:
    				AddSample_stereo_32(Sample);
    				break;
    			}
    			break;
    		}
    		NoOfSamples++;
    		TimeSamples++;
    		if (TimeSamples >= Frequency)
    		{
    			_Time++;
    			TimeSamples = 0;
    		}
    	}

    	private void AddSample_mono_08(int Sample)
    	{
    		STR.WriteByte((byte)((uint)Sample & 0xFFu));
    	}

    	private void AddSample_mono_16(int Sample)
    	{
    		STR.WriteByte((byte)((Sample & 0xFF00) >> 8));
    		STR.WriteByte((byte)((uint)Sample & 0xFFu));
    	}

    	private void AddSample_mono_32(int Sample)
    	{
    		STR.WriteByte((byte)((Sample & 0xFF000000u) >> 24));
    		STR.WriteByte((byte)((Sample & 0xFF0000) >> 16));
    		STR.WriteByte((byte)((Sample & 0xFF00) >> 8));
    		STR.WriteByte((byte)((uint)Sample & 0xFFu));
    	}

    	private void AddSample_stereo_08(int Sample)
    	{
    		STR.WriteByte((byte)((uint)Sample & 0xFFu));
    		STR.WriteByte((byte)((uint)Sample & 0xFFu));
    	}

    	private void AddSample_stereo_16(int Sample)
    	{
    		STR.WriteByte((byte)((Sample & 0xFF00) >> 8));
    		STR.WriteByte((byte)((uint)Sample & 0xFFu));
    		STR.WriteByte((byte)((Sample & 0xFF00) >> 8));
    		STR.WriteByte((byte)((uint)Sample & 0xFFu));
    	}

    	private void AddSample_stereo_32(int Sample)
    	{
    		STR.WriteByte((byte)((Sample & 0xFF000000u) >> 24));
    		STR.WriteByte((byte)((Sample & 0xFF0000) >> 16));
    		STR.WriteByte((byte)((Sample & 0xFF00) >> 8));
    		STR.WriteByte((byte)((uint)Sample & 0xFFu));
    		STR.WriteByte((byte)((Sample & 0xFF000000u) >> 24));
    		STR.WriteByte((byte)((Sample & 0xFF0000) >> 16));
    		STR.WriteByte((byte)((Sample & 0xFF00) >> 8));
    		STR.WriteByte((byte)((uint)Sample & 0xFFu));
    	}

    	public void Stop()
    	{
    		if (_IsRecording & (STR != null))
    		{
    			NoOfSamples *= channels * (bitsPerSample / 8);
    			SIZE = NoOfSamples + 36;
    			byte[] array = new byte[4];
    			byte[] array2 = new byte[4];
    			array = BitConverter.GetBytes(SIZE);
    			if (!BitConverter.IsLittleEndian)
    			{
    				Array.Reverse(array);
    			}
    			array2 = BitConverter.GetBytes(NoOfSamples);
    			if (!BitConverter.IsLittleEndian)
    			{
    				Array.Reverse(array2);
    			}
    			_IsRecording = false;
    			STR.Position = 4L;
    			STR.Write(array, 0, 4);
    			STR.Position = 40L;
    			STR.Write(array2, 0, 4);
    			STR.Close();
    			MyNesMain.VideoProvider.WriteInfoNotification("Sound file saved at " + Path.GetFileName(_fileName), instant: false);
    		}
    	}
    }
}
