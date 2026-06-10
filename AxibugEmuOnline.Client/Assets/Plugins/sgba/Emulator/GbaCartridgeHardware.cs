using System.IO;

namespace sGBA;

public class GbaCartridgeHardware
{
	public Gba Gba { get; }
	public bool HasRtc { get; set; }

	private byte _writeLatch;
	private byte _pinState;
	private byte _direction;
	private bool _readWrite;

	private int _rtcBytesRemaining;
	private int _rtcBitsRead;
	private int _rtcBits;
	private bool _rtcCommandActive;
	private bool _rtcSckEdge;
	private bool _rtcSioOutput;
	private int _rtcCommand;
	private byte _rtcControl;
	private byte[] _rtcTime = new byte[7];

	private static readonly int[] RtcBytes = [0, 0, 7, 0, 1, 0, 3, 0];

	public GbaCartridgeHardware( Gba gba )
	{
		Gba = gba;
	}

	public void Reset()
	{
		_writeLatch = 0;
		_pinState = 0;
		_direction = 0;
		_readWrite = false;

		_rtcBytesRemaining = 0;
		_rtcBitsRead = 0;
		_rtcBits = 0;
		_rtcCommandActive = false;
		_rtcSckEdge = true;
		_rtcSioOutput = true;
		_rtcCommand = 0;
		_rtcControl = 0x40;
		Array.Clear( _rtcTime );
	}

	public void GpioWrite( uint address, ushort value )
	{
		uint reg = address & 0xFF;

		switch ( reg )
		{
			case 0xC4:
				_writeLatch = (byte)(value & 0xF);
				_pinState &= (byte)~_direction;
				_pinState |= (byte)(_writeLatch & _direction);
				ReadPins();
				break;

			case 0xC6:
				_direction = (byte)(value & 0xF);
				_pinState &= (byte)~_direction;
				_pinState |= (byte)(_writeLatch & _direction);
				ReadPins();
				break;

			case 0xC8:
				_readWrite = (value & 1) != 0;
				break;
		}

		UpdateRomMirror();
	}

	public ushort GpioRead( uint address )
	{
		if ( !_readWrite )
			return 0;

		uint reg = address & 0xFF;
		return reg switch
		{
			0xC4 => _pinState,
			0xC6 => _direction,
			0xC8 => (ushort)(_readWrite ? 1 : 0),
			_ => 0
		};
	}

	private void UpdateRomMirror()
	{
		var rom = Gba.Memory.Rom;
		if ( rom.Length < 0xCA )
			return;

		if ( _readWrite )
		{
			rom[0xC4] = _pinState;
			rom[0xC5] = 0;
			rom[0xC6] = _direction;
			rom[0xC7] = 0;
			rom[0xC8] = (byte)(_readWrite ? 1 : 0);
			rom[0xC9] = 0;
		}
		else
		{
			rom[0xC4] = 0;
			rom[0xC5] = 0;
			rom[0xC6] = 0;
			rom[0xC7] = 0;
			rom[0xC8] = 0;
			rom[0xC9] = 0;
		}
	}

	private void ReadPins()
	{
		if ( HasRtc )
			RtcReadPins();
	}

	private void OutputPins( int pins )
	{
		_pinState &= _direction;
		_pinState |= (byte)(pins & ~_direction & 0xF);

		if ( _readWrite )
		{
			var rom = Gba.Memory.Rom;
			if ( rom.Length > 0xC5 )
			{
				rom[0xC4] = _pinState;
				rom[0xC5] = 0;
			}
		}
	}
	private void RtcReadPins()
	{
		OutputPins( _pinState & 2 );

		if ( (_pinState & 4) == 0 )
		{
			_rtcBitsRead = 0;
			_rtcBytesRemaining = 0;
			_rtcCommandActive = false;
			_rtcCommand = 0;
			_rtcSckEdge = true;
			_rtcSioOutput = true;
			OutputPins( 2 );
			return;
		}

		if ( !_rtcCommandActive )
		{
			OutputPins( 2 );

			if ( (_pinState & 1) == 0 )
			{
				_rtcBits &= ~(1 << _rtcBitsRead);
				_rtcBits |= ((_pinState >> 1) & 1) << _rtcBitsRead;
			}

			if ( !_rtcSckEdge && (_pinState & 1) != 0 )
			{
				_rtcBitsRead++;
				if ( _rtcBitsRead == 8 )
				{
					RtcBeginCommand();
				}
			}
		}
		else if ( (_rtcCommand & 0x80) == 0 )
		{
			OutputPins( 2 );

			if ( (_pinState & 1) == 0 )
			{
				_rtcBits &= ~(1 << _rtcBitsRead);
				_rtcBits |= ((_pinState >> 1) & 1) << _rtcBitsRead;
			}

			if ( !_rtcSckEdge && (_pinState & 1) != 0 )
			{
				if ( (((_rtcBits >> _rtcBitsRead) & 1) ^ ((_pinState >> 1) & 1)) != 0 )
				{
					_rtcBits &= ~(1 << _rtcBitsRead);
				}

				_rtcBitsRead++;
				if ( _rtcBitsRead == 8 )
				{
					RtcProcessByte();
				}
			}
		}
		else
		{
			if ( _rtcSckEdge && (_pinState & 1) == 0 )
			{
				_rtcSioOutput = RtcOutput();
				_rtcBitsRead++;
				if ( _rtcBitsRead == 8 )
				{
					_rtcBytesRemaining--;
					if ( _rtcBytesRemaining <= 0 )
					{
						_rtcBytesRemaining = RtcBytes[(_rtcCommand >> 4) & 7];
					}
					_rtcBitsRead = 0;
				}
			}
			OutputPins( _rtcSioOutput ? 2 : 0 );
		}

		_rtcSckEdge = (_pinState & 1) != 0;
	}

	private void RtcBeginCommand()
	{
		int command = _rtcBits;
		int magic = command & 0xF;

		if ( magic == 0x06 )
		{
			_rtcCommand = command;
			int cmd = (command >> 4) & 7;
			_rtcBytesRemaining = RtcBytes[cmd];
			_rtcCommandActive = true;

			switch ( cmd )
			{
				case 0:
					_rtcControl = 0;
					break;
				case 2:
				case 6:
					RtcUpdateClock();
					break;
				case 3:
				case 4:
					break;
			}
		}

		_rtcBits = 0;
		_rtcBitsRead = 0;
	}

	private void RtcProcessByte()
	{
		int cmd = (_rtcCommand >> 4) & 7;

		switch ( cmd )
		{
			case 4:
				_rtcControl = (byte)_rtcBits;
				break;
		}

		_rtcBits = 0;
		_rtcBitsRead = 0;
		_rtcBytesRemaining--;
		if ( _rtcBytesRemaining <= 0 )
		{
			_rtcBytesRemaining = RtcBytes[cmd];
		}
	}

	private bool RtcOutput()
	{
		byte outputByte = 0xFF;
		int cmd = (_rtcCommand >> 4) & 7;

		switch ( cmd )
		{
			case 4:
				outputByte = _rtcControl;
				break;
			case 2:
			case 6:
				outputByte = _rtcTime[7 - _rtcBytesRemaining];
				break;
		}

		return ((outputByte >> _rtcBitsRead) & 1) != 0;
	}

	private void RtcUpdateClock()
	{
		var now = DateTimeOffset.Now;
		_rtcTime[0] = RtcBcd( now.Year % 100 );
		_rtcTime[1] = RtcBcd( now.Month );
		_rtcTime[2] = RtcBcd( now.Day );
		_rtcTime[3] = RtcBcd( (int)now.DayOfWeek );

		bool hour24 = (_rtcControl & 0x40) != 0;
		if ( hour24 )
		{
			_rtcTime[4] = RtcBcd( now.Hour );
		}
		else
		{
			_rtcTime[4] = RtcBcd( now.Hour % 12 );
		}

		_rtcTime[5] = RtcBcd( now.Minute );
		_rtcTime[6] = RtcBcd( now.Second );
	}

	private static byte RtcBcd( int value )
	{
		return (byte)((value % 10) + ((value / 10 % 10) << 4));
	}

	public void InitRtc( byte[] rom )
	{
		if ( rom.Length < 0xB4 )
			return;

		string gameCode = System.Text.Encoding.ASCII.GetString( rom, 0xAC, 4 );
		if ( gameCode.StartsWith( "AXV", StringComparison.Ordinal ) || // Pokemon Ruby
			 gameCode.StartsWith( "AXP", StringComparison.Ordinal ) || // Pokemon Sapphire
			 gameCode.StartsWith( "BPE", StringComparison.Ordinal ) || // Pokemon Emerald
			 gameCode.StartsWith( "BLJ", StringComparison.Ordinal ) || // Legendz - Yomigaeru Shiren no Shima
			 gameCode.StartsWith( "BLV", StringComparison.Ordinal ) || // Legendz - Sign of Nekuromu
			 gameCode.StartsWith( "BR4", StringComparison.Ordinal ) || // RockMan EXE 4.5
			 gameCode.StartsWith( "BKA", StringComparison.Ordinal ) || // Sennen Kazoku
			 gameCode.StartsWith( "U3I", StringComparison.Ordinal ) || // Boktai
			 gameCode.StartsWith( "U32", StringComparison.Ordinal ) || // Boktai 2
			 gameCode.StartsWith( "U33", StringComparison.Ordinal ) )  // Boktai 3
		{
			HasRtc = true;
		}
	}

	public void Serialize( BinaryWriter w )
	{
		w.Write( _writeLatch );
		w.Write( _pinState );
		w.Write( _direction );
		w.Write( _readWrite );
		w.Write( _rtcBytesRemaining );
		w.Write( _rtcBitsRead );
		w.Write( _rtcBits );
		w.Write( _rtcCommandActive );
		w.Write( _rtcSckEdge );
		w.Write( _rtcSioOutput );
		w.Write( _rtcCommand );
		w.Write( _rtcControl );
		w.Write( _rtcTime );
	}

	public void Deserialize( BinaryReader r )
	{
		_writeLatch = r.ReadByte();
		_pinState = r.ReadByte();
		_direction = r.ReadByte();
		_readWrite = r.ReadBoolean();
		_rtcBytesRemaining = r.ReadInt32();
		_rtcBitsRead = r.ReadInt32();
		_rtcBits = r.ReadInt32();
		_rtcCommandActive = r.ReadBoolean();
		_rtcSckEdge = r.ReadBoolean();
		_rtcSioOutput = r.ReadBoolean();
		_rtcCommand = r.ReadInt32();
		_rtcControl = r.ReadByte();
		r.Read( _rtcTime );
	}
}
