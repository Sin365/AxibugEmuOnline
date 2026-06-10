using System.IO;

namespace sGBA;

public class GbaIo
{
	public Gba Gba { get; }

	public ushort IE;
	public ushort IF;
	public ushort IME;
	public ushort WaitCnt;
	public ushort KeyCnt;
	public ushort Rcnt = 0x8000;
	public byte PostFlg;

	private const int IrqDelayBase = 7;
	private long _irqFireCycle = long.MaxValue;
	private int _irqRearmDelay;

	private ushort _fifoALatch;
	private ushort _fifoBLatch;
	private ushort _sioCnt;
	private readonly ushort[] _sioRegs = new ushort[30];
	private int _sioMode = SioModeGpio;
	private long _sioCompletionCycle = long.MaxValue;

	private const int SioModeNormal8 = 0;
	private const int SioModeNormal32 = 1;
	private const int SioModeMulti = 2;
	private const int SioModeUart = 3;
	private const int SioModeGpio = 8;
	private const int SioModeJoybus = 12;

	private static readonly int[,] SioCyclesPerTransferMulti =
	{
		{ 31976, 63427, 94884, 125829 },
		{ 8378, 16241, 24104, 31457 },
		{ 5750, 10998, 16241, 20972 },
		{ 3140, 5755, 8376, 10486 },
	};

	private const int WirelessPollCycles = 4096;

	public long NextSioEvent => _sioCompletionCycle;

	public GbaWirelessAdapter WirelessAdapter { get; }

	public IGbaSioDriver SioDriver { get; private set; }

	public GbaSioMode SioMode => (GbaSioMode)_sioMode;

	public ushort SioCnt { get => _sioCnt; set => _sioCnt = value; }

	public ushort GetSioReg( int index ) => _sioRegs[index];

	public void ScheduleSioCompletion( long absoluteCycle )
	{
		_sioCompletionCycle = absoluteCycle;
		_sioPollingWireless = false;
	}

	private long _driverEventCycle = long.MaxValue;
	internal Action<int> DriverEventCallback;

	public bool LockstepBlocked { get; set; }

	public long NextDriverEvent => _driverEventCycle;
	public bool IsDriverEventScheduled => _driverEventCycle != long.MaxValue;

	public void ScheduleDriverEventIn( long relativeCycles )
	{
		if ( relativeCycles < 0 ) relativeCycles = 0;
		_driverEventCycle = Gba.Cpu.Cycles + relativeCycles;
	}

	public void DescheduleDriverEvent()
	{
		_driverEventCycle = long.MaxValue;
	}

	public void ProcessDriverEvent()
	{
		if ( _driverEventCycle == long.MaxValue ) return;
		if ( Gba.Cpu.Cycles < _driverEventCycle ) return;

		int cyclesLate = (int)(Gba.Cpu.Cycles - _driverEventCycle);
		_driverEventCycle = long.MaxValue;
		DriverEventCallback?.Invoke( cyclesLate );
	}

	private bool _sioPollingWireless;

	public GbaIo( Gba gba )
	{
		Gba = gba;
		WirelessAdapter = new GbaWirelessAdapter( gba );
	}

	public void SetSioDriver( IGbaSioDriver driver )
	{
		if ( SioDriver != null )
			SioDriver.Deinit();
		SioDriver = driver;
		if ( driver != null )
		{
			driver.Sio = this;
			if ( !driver.Init() )
			{
				driver.Deinit();
				SioDriver = null;
			}
		}
	}

	public void Reset()
	{
		IE = 0;
		IF = 0;
		IME = 0;
		WaitCnt = 0;
		KeyCnt = 0;
		Rcnt = 0x8000;
		_sioCnt = 0;
		Array.Clear( _sioRegs );
		_sioMode = SioModeGpio;
		_sioCompletionCycle = long.MaxValue;
		_sioPollingWireless = false;
		WirelessAdapter.Reset();
		SioDriver?.Reset();
		PostFlg = 0;
		_irqFireCycle = long.MaxValue;
		_irqRearmDelay = 0;
		_driverEventCycle = long.MaxValue;
		LockstepBlocked = false;
		InitializeRegisters();
	}

	private void InitializeRegisters()
	{
		ushort[] io = Gba.Memory.Io;
		Array.Clear( io );
		io[0x000 >> 1] = 0x0080;
		Gba.Video.DispCnt = 0x0080;
		io[0x134 >> 1] = 0x8000;
		io[0x130 >> 1] = 0x03FF;
		io[0x088 >> 1] = 0x0200;
		io[0x020 >> 1] = 0x0100;
		io[0x026 >> 1] = 0x0100;
		io[0x030 >> 1] = 0x0100;
		io[0x036 >> 1] = 0x0100;
		io[0x300 >> 1] = PostFlg;
	}

	public void ApplySkipBiosState()
	{
		PostFlg = 1;
		Gba.Video.VCount = 0x7E;

		ushort[] io = Gba.Memory.Io;
		io[0x006 >> 1] = 0x007E;
		io[0x300 >> 1] = PostFlg;
	}

	public void RaiseIrq( GbaIrq irq, int cyclesLate = 0 )
	{
		IF |= (ushort)irq;

		ushort matched = (ushort)(IE & (ushort)irq);
		if ( matched != 0 )
		{
			ushort biosIF = Gba.Memory.Load16( 0x03007FF8 );
			biosIF |= matched;
			Gba.Memory.Store16( 0x03007FF8, biosIF );
		}

		if ( (IE & IF) != 0 && _irqFireCycle == long.MaxValue )
		{
			_irqFireCycle = Gba.Cpu.Cycles + _irqRearmDelay - cyclesLate + IrqDelayBase;
		}
	}

	public long NextIrqEvent => _irqFireCycle;

	public void BeginEventProcessing()
	{
		_irqRearmDelay = 0;
	}

	public void ProcessIrqEvent()
	{
		if ( _irqFireCycle == long.MaxValue ) return;

		if ( Gba.Cpu.Cycles >= _irqFireCycle )
		{
			_irqFireCycle = long.MaxValue;
			Gba.Cpu.Halted = false;
			if ( IME != 0 && (IE & IF) != 0 && !Gba.Cpu.IrqDisable )
			{
				Gba.Cpu.IrqPending = true;
				_irqRearmDelay = 2;
			}
		}
	}

	public void EndEventProcessing()
	{
		_irqRearmDelay = 0;
	}

	public void TestIrq( int cyclesLate = 0 )
	{
		if ( (IE & IF) != 0 && _irqFireCycle == long.MaxValue )
		{
			_irqFireCycle = Gba.Cpu.InstructionStartCycles + IrqDelayBase - cyclesLate;
		}
	}

	public int SioTransferCyclesForLockstep( int connected ) => SioTransferCycles( connected );

	private int SioTransferCycles( int connected = 0 )
	{
		const int CpuFreq = 16777216;
		if ( connected < 0 ) connected = 0;
		if ( connected > 3 ) connected = 3;
		switch ( _sioMode )
		{
			case SioModeMulti:
				return SioCyclesPerTransferMulti[_sioCnt & 3, connected];
			case SioModeNormal8:
				return 8 * CpuFreq / (((_sioCnt & 0x0001) != 0 ? 2048 : 256) * 1024);
			case SioModeNormal32:
				return 32 * CpuFreq / (((_sioCnt & 0x0001) != 0 ? 2048 : 256) * 1024);
			default:
				return 0;
		}
	}

	public void FinishSioTransfer()
	{
		if ( _sioCompletionCycle == long.MaxValue ) return;
		if ( Gba.Cpu.Cycles < _sioCompletionCycle ) return;

		int cyclesLate = (int)(Gba.Cpu.Cycles - _sioCompletionCycle);
		long scheduled = _sioCompletionCycle;
		_sioCompletionCycle = long.MaxValue;

		if ( SioDriver != null && !_sioPollingWireless )
		{
			FinishSioTransferDriver( cyclesLate );
			return;
		}

		if ( _sioPollingWireless )
		{
			_sioPollingWireless = false;
			long elapsed = Math.Max( WirelessPollCycles, Gba.Cpu.Cycles - (scheduled - WirelessPollCycles) );
			bool raise = WirelessAdapter.Update( (uint)elapsed, _sioCnt, out bool wroteSio, out ushort newSioCnt, out ushort newDataLo, out ushort newDataHi );
			if ( wroteSio )
			{
				_sioRegs[0] = newDataLo;
				_sioRegs[1] = newDataHi;
				_sioCnt = newSioCnt;
			}
			ScheduleWirelessPoll();
			if ( raise )
				RaiseIrq( GbaIrq.Sio, cyclesLate );
			return;
		}

		switch ( _sioMode )
		{
			case SioModeMulti:
				_sioRegs[0] = 0;
				_sioRegs[1] = 0;
				_sioRegs[2] = 0;
				_sioRegs[3] = 0;
				_sioCnt &= unchecked((ushort)~0x0080);
				_sioCnt &= unchecked((ushort)~0x0030);
				Rcnt |= 0x0001;
				if ( (_sioCnt & 0x4000) != 0 )
					RaiseIrq( GbaIrq.Sio, cyclesLate );
				break;
			case SioModeNormal8:
				_sioCnt &= unchecked((ushort)~0x0080);
				_sioRegs[5] = 0;
				if ( (_sioCnt & 0x4000) != 0 )
					RaiseIrq( GbaIrq.Sio, cyclesLate );
				break;
			case SioModeNormal32:
				_sioCnt &= unchecked((ushort)~0x0080);
				if ( !WirelessAdapter.IsActive )
				{
					_sioRegs[0] = 0;
					_sioRegs[1] = 0;
				}
				_sioCnt |= 0x0004;
				if ( (_sioCnt & 0x4000) != 0 )
					RaiseIrq( GbaIrq.Sio, cyclesLate );
				ScheduleWirelessPoll();
				break;
		}
	}

	private readonly ushort[] _sioFinishMulti = new ushort[4];

	private void FinishSioTransferDriver( int cyclesLate )
	{
		switch ( _sioMode )
		{
			case SioModeMulti:
				_sioFinishMulti[0] = 0xFFFF;
				_sioFinishMulti[1] = 0xFFFF;
				_sioFinishMulti[2] = 0xFFFF;
				_sioFinishMulti[3] = 0xFFFF;
				SioDriver.FinishMultiplayer( _sioFinishMulti );
				_sioRegs[0] = _sioFinishMulti[0];
				_sioRegs[1] = _sioFinishMulti[1];
				_sioRegs[2] = _sioFinishMulti[2];
				_sioRegs[3] = _sioFinishMulti[3];
				_sioCnt &= unchecked((ushort)~0x0080);
				_sioCnt = (ushort)((_sioCnt & ~0x0030) | ((SioDriver.DeviceId() & 3) << 4));
				Rcnt |= 0x0001;
				if ( (_sioCnt & 0x4000) != 0 )
					RaiseIrq( GbaIrq.Sio, cyclesLate );
				break;
			case SioModeNormal8:
				{
					byte data = SioDriver.FinishNormal8();
					_sioCnt &= unchecked((ushort)~0x0080);
					_sioRegs[5] = data;
					if ( (_sioCnt & 0x4000) != 0 )
						RaiseIrq( GbaIrq.Sio, cyclesLate );
				}
				break;
			case SioModeNormal32:
				{
					uint data = SioDriver.FinishNormal32();
					_sioCnt &= unchecked((ushort)~0x0080);
					_sioRegs[0] = (ushort)(data & 0xFFFF);
					_sioRegs[1] = (ushort)(data >> 16);
					if ( (_sioCnt & 0x4000) != 0 )
						RaiseIrq( GbaIrq.Sio, cyclesLate );
				}
				break;
		}
	}

	private void ScheduleWirelessPoll()
	{
		if ( _sioCompletionCycle != long.MaxValue || !WirelessAdapter.IsActive )
			return;

		var state = WirelessAdapter.SpiState;
		if ( state != GbaWirelessAdapter.ComState.WaitEvent && state != GbaWirelessAdapter.ComState.WaitResponse )
			return;

		_sioCompletionCycle = Gba.Cpu.Cycles + WirelessPollCycles;
		_sioPollingWireless = true;
	}

	public void TestKeypadIrq()
	{
		ushort keysLast = Gba.KeysLast;
		ushort keysActive = Gba.KeysActive;

		ushort keyCnt = KeyCnt;
		if ( (keyCnt & 0x4000) == 0 )
			return;

		Gba.KeysLast = keysActive;
		bool isAnd = (keyCnt & 0x8000) != 0;
		keyCnt &= 0x03FF;

		if ( isAnd && keyCnt == (keysActive & keyCnt) )
		{
			if ( keysLast == keysActive )
				return;
			RaiseIrq( GbaIrq.Keypad );
		}
		else if ( !isAnd && (keysActive & keyCnt) != 0 )
		{
			RaiseIrq( GbaIrq.Keypad );
		}
		else
		{
			Gba.KeysLast = 0x400;
		}
	}

	private static bool IsReadConstantRegister( uint offset ) => offset switch
	{
		0x008 or 0x00A or 0x00C or 0x00E or
		0x048 or 0x04A or 0x050 or 0x052 or
		0x060 or 0x062 or 0x064 or 0x068 or 0x06C or 0x070 or 0x072 or 0x074 or 0x078 or 0x07C or 0x080 or 0x082 or
		0x102 or 0x106 or 0x10A or 0x10E or
		0x130 or 0x132 or 0x200 => true,
		_ => false
	};

	private ushort ReadKeyInputRegister()
	{
		ushort keysActive = Gba.KeysActive;
		if ( !Gba.AllowOpposingDirections )
		{
			ushort leftRight = (ushort)(keysActive & 0x0030);
			ushort upDown = (ushort)(keysActive & 0x00C0);
			keysActive &= 0x030F;
			if ( leftRight != 0x0030 )
				keysActive |= leftRight;
			if ( upDown != 0x00C0 )
				keysActive |= upDown;
		}

		ushort keyInput = (ushort)(0x03FF ^ keysActive);
		Gba.Memory.Io[0x130 >> 1] = keyInput;
		return keyInput;
	}

	private void SwitchSioMode()
	{
		int combined = ((Rcnt & 0xC000) | (_sioCnt & 0x3000)) >> 12;
		int newMode = combined < 8 ? combined & 3 : combined & 0xC;
		if ( newMode != _sioMode )
		{
			_sioMode = newMode;
			SioDriver?.SetMode( (GbaSioMode)newMode );
			if ( newMode == SioModeMulti )
			{
				int id = SioDriver?.DeviceId() ?? 0;
				if ( id != 0 )
					Rcnt |= 0x0004;
				else
					Rcnt &= unchecked((ushort)~0x0004);
			}
		}
	}

	private void WriteRcnt( ushort value )
	{
		ushort oldRcnt = Rcnt;
		Rcnt = (ushort)((Rcnt & 0x1FF) | (value & 0xC000));
		SwitchSioMode();
		if ( SioDriver != null )
		{
			ushort driven = SioDriver.WriteRcnt( value );
			if ( _sioMode == SioModeGpio )
				Rcnt = (ushort)((driven & 0x01FF) | (Rcnt & 0xC000));
			else
				Rcnt = (ushort)((driven & 0x01F0) | (Rcnt & 0xC00F));
		}
		else if ( _sioMode == SioModeGpio )
			Rcnt = (ushort)((Rcnt & 0xC000) | (value & 0x1FF));
		else
			Rcnt = (ushort)((Rcnt & 0xC00F) | (value & 0x1F0));

		if ( _sioMode == SioModeGpio && (value & 0x20) != 0 && (oldRcnt & 0x02) == 0 )
			WirelessAdapter.Reset();
	}

	private void WriteSioCnt( ushort value )
	{
		ushort oldSioCnt = _sioCnt;
		value &= 0x7FFF;
		if ( ((value ^ _sioCnt) & 0x3000) != 0 )
		{
			_sioCnt = (ushort)(value & 0x3000);
			SwitchSioMode();
		}

		if ( SioDriver != null )
		{
			WriteSioCntDriver( value );
			return;
		}

		switch ( _sioMode )
		{
			case SioModeMulti:
				value &= 0xFF83;
				value |= 0x0004;
				value |= (ushort)(_sioCnt & 0x00FC);
				Rcnt |= 0x0001;
				if ( (value & 0x0080) != 0 && (_sioCnt & 0x0080) == 0 )
				{
					_sioRegs[0] = 0xFFFF;
					_sioRegs[1] = 0xFFFF;
					_sioRegs[2] = 0xFFFF;
					_sioRegs[3] = 0xFFFF;
					Rcnt &= unchecked((ushort)~0x0001);
					_sioCompletionCycle = Gba.Cpu.Cycles + SioTransferCycles();
				}
				value |= 0x0008;
				break;
			case SioModeNormal8:
			case SioModeNormal32:
				value = (ushort)((value & 0x7F8B) | (oldSioCnt & 0x0004));
				if ( (value & 0x0081) == 0x0081 && _sioCompletionCycle == long.MaxValue )
				{
					uint sent = (uint)_sioRegs[0] | ((uint)_sioRegs[1] << 16);
					uint reply = WirelessAdapter.Transfer( sent );
					_sioRegs[0] = (ushort)(reply & 0xFFFF);
					_sioRegs[1] = (ushort)(reply >> 16);
					_sioCompletionCycle = Gba.Cpu.Cycles + SioTransferCycles();
					_sioPollingWireless = false;
				}

				bool soRose = (value & 0x0008) != 0 && (oldSioCnt & 0x0008) == 0;
				bool soFell = (value & 0x0008) == 0 && (oldSioCnt & 0x0008) != 0;
				if ( (value & 0x1) != 0 )
				{
					if ( soRose ) value &= unchecked((ushort)~0x0004);
				}
				else
				{
					if ( soRose ) value |= 0x0004;
					if ( soFell ) value &= unchecked((ushort)~0x0004);
				}

				if ( _sioMode == SioModeNormal32 )
					ScheduleWirelessPoll();
				break;
		}

		_sioCnt = value;
	}

	private void WriteSioCntDriver( ushort value )
	{
		int id = 0;
		int connected = 0;
		bool handled = SioDriver.HandlesMode( (GbaSioMode)_sioMode );
		if ( handled )
		{
			id = SioDriver.DeviceId();
			connected = SioDriver.ConnectedDevices();
		}

		switch ( _sioMode )
		{
			case SioModeMulti:
				value &= 0xFF83;
				if ( id != 0 || connected == 0 )
					value |= 0x0004;
				else
					value &= unchecked((ushort)~0x0004);
				value = (ushort)((value & ~0x0030) | ((id & 3) << 4));
				value |= (ushort)(_sioCnt & 0x00FC);
				Rcnt |= 0x0001;

				if ( (value & 0x0080) != 0 && (_sioCnt & 0x0080) == 0 )
				{
					if ( id == 0 )
					{
						_sioRegs[0] = 0xFFFF;
						_sioRegs[1] = 0xFFFF;
						_sioRegs[2] = 0xFFFF;
						_sioRegs[3] = 0xFFFF;
						Rcnt &= unchecked((ushort)~0x0001);
						StartTransfer( connected );
					}
				}
				break;
			case SioModeNormal8:
			case SioModeNormal32:
				if ( (value & 0x0001) != 0 )
					Rcnt |= 0x0001;
				if ( (value & 0x0080) != 0 && (_sioCnt & 0x0080) == 0 )
					StartTransfer( connected );
				break;
		}

		if ( handled )
		{
			value = SioDriver.WriteSioCnt( value );
		}
		else
		{
			switch ( _sioMode )
			{
				case SioModeNormal8:
				case SioModeNormal32:
					value |= 0x0004;
					break;
				case SioModeMulti:
					value |= 0x0008;
					break;
			}
		}

		_sioCnt = value;
	}

	private void StartTransfer( int connected )
	{
		if ( SioDriver != null && !SioDriver.Start() )
			return;
		_sioCompletionCycle = Gba.Cpu.Cycles + SioTransferCycles( connected );
		_sioPollingWireless = false;
	}

	private void WriteSioRegister( uint offset, ushort value )
	{
		int index = (int)((offset - 0x120) >> 1);
		bool handled = true;

		switch ( _sioMode )
		{
			case SioModeJoybus:
				switch ( offset )
				{
					case 0x12A:
					case 0x154:
					case 0x156:
						break;
					case 0x140:
						value = (ushort)((value & 0x0040) | (_sioRegs[16] & ~(value & 0x7) & ~0x0040));
						break;
					case 0x158:
						value = (ushort)((value & 0x0030) | (_sioRegs[28] & ~0x30));
						break;
					default: handled = false; break;
				}
				break;
			case SioModeNormal8:
				switch ( offset )
				{
					case 0x12A: break;
					case 0x140:
						value = (ushort)((value & 0x0040) | (_sioRegs[16] & ~(value & 0x7) & ~0x0040));
						break;
					default: handled = false; break;
				}
				break;
			case SioModeNormal32:
				switch ( offset )
				{
					case 0x120:
					case 0x122:
					case 0x12A:
						break;
					case 0x140:
						value = (ushort)((value & 0x0040) | (_sioRegs[16] & ~(value & 0x7) & ~0x0040));
						break;
					default: handled = false; break;
				}
				break;
			case SioModeMulti:
				switch ( offset )
				{
					case 0x12A: break;
					case 0x140:
						value = (ushort)((value & 0x0040) | (_sioRegs[16] & ~(value & 0x7) & ~0x0040));
						break;
					default: handled = false; break;
				}
				break;
			case SioModeUart:
				switch ( offset )
				{
					case 0x12A: break;
					case 0x140:
						value = (ushort)((value & 0x0040) | (_sioRegs[16] & ~(value & 0x7) & ~0x0040));
						break;
					default: handled = false; break;
				}
				break;
			default:
				handled = false;
				break;
		}

		if ( !handled )
			value = _sioRegs[index];

		_sioRegs[index] = value;
	}

	private ushort ReadWriteOnlyRegister( uint offset )
	{
		GbaLog.Write( LogCategory.GBAIO, LogLevel.GameError, "Read from write-only I/O register: {0:X3}", offset );
		return (ushort)Gba.Cpu.LoadBadValue();
	}

	private ushort ReadUnusedRegister( uint offset )
	{
		GbaLog.Write( LogCategory.GBAIO, LogLevel.GameError, "Read from unused I/O register: {0:X3}", offset );
		return (ushort)Gba.Cpu.LoadBadValue();
	}

	public ushort Read16( uint offset )
	{
		if ( !IsReadConstantRegister( offset ) )
			Gba.HaltPending = false;

		switch ( offset )
		{
			case 0x000: return Gba.Video.DispCnt;
			case 0x002: return 0;
			case 0x004: return Gba.Video.DispStat;
			case 0x006: return (ushort)Gba.Video.VCount;
			case 0x008: return (ushort)(Gba.Video.BgCnt[0] & 0xDFFF);
			case 0x00A: return (ushort)(Gba.Video.BgCnt[1] & 0xDFFF);
			case 0x00C: return Gba.Video.BgCnt[2];
			case 0x00E: return Gba.Video.BgCnt[3];

			case 0x010:
			case 0x012:
			case 0x014:
			case 0x016:
			case 0x018:
			case 0x01A:
			case 0x01C:
			case 0x01E:
			case 0x020:
			case 0x022:
			case 0x024:
			case 0x026:
			case 0x028:
			case 0x02A:
			case 0x02C:
			case 0x02E:
			case 0x030:
			case 0x032:
			case 0x034:
			case 0x036:
			case 0x038:
			case 0x03A:
			case 0x03C:
			case 0x03E:
			case 0x040:
			case 0x042:
			case 0x044:
			case 0x046:
			case 0x04C:
			case 0x054:
				return ReadWriteOnlyRegister( offset );

			case 0x048: return Gba.Video.WinIn;
			case 0x04A: return Gba.Video.WinOut;

			case 0x050: return Gba.Video.BldCnt;
			case 0x052: return Gba.Video.BldAlpha;

			case 0x060:
			case 0x062:
			case 0x064:
			case 0x068:
			case 0x06C:
			case 0x070:
			case 0x072:
			case 0x074:
			case 0x078:
			case 0x07C:
			case 0x080:
			case 0x082:
			case 0x084:
			case 0x088:
				return Gba.Audio.ReadRegister( offset );
			case 0x066:
			case 0x06A:
			case 0x06E:
			case 0x076:
			case 0x07A:
			case 0x07E:
			case 0x086:
			case 0x08A:
				return 0;
			case 0x090:
			case 0x092:
			case 0x094:
			case 0x096:
			case 0x098:
			case 0x09A:
			case 0x09C:
			case 0x09E:
				{
					int readBank;
					if ( (Gba.Audio.SoundCntX & 0x80) == 0 )
						readBank = 1;
					else
						readBank = (Gba.Audio.Sound3CntL & 0x40) != 0 ? 0 : 1;
					int waveOff = readBank * 16 + (int)(offset - 0x090);
					return (ushort)(Gba.Audio.WaveRam[waveOff] | (Gba.Audio.WaveRam[waveOff + 1] << 8));
				}

			case 0x0A0:
			case 0x0A2:
			case 0x0A4:
			case 0x0A6:
			case 0x0B0:
			case 0x0B2:
			case 0x0B4:
			case 0x0B6:
			case 0x0BC:
			case 0x0BE:
			case 0x0C0:
			case 0x0C2:
			case 0x0C8:
			case 0x0CA:
			case 0x0CC:
			case 0x0CE:
			case 0x0D4:
			case 0x0D6:
			case 0x0D8:
			case 0x0DA:
				return ReadWriteOnlyRegister( offset );

			case 0x0B8: return 0;
			case 0x0BA: return (ushort)(Gba.Dma.Channels[0].Reg & 0xF7E0);
			case 0x0C4: return 0;
			case 0x0C6: return (ushort)(Gba.Dma.Channels[1].Reg & 0xF7E0);
			case 0x0D0: return 0;
			case 0x0D2: return (ushort)(Gba.Dma.Channels[2].Reg & 0xF7E0);
			case 0x0DC: return 0;
			case 0x0DE: return (ushort)(Gba.Dma.Channels[3].Reg & 0xFFE0);

			case 0x100: return Gba.Timers.GetCounter( 0 );
			case 0x102: return Gba.Timers.Channels[0].Control;
			case 0x104: return Gba.Timers.GetCounter( 1 );
			case 0x106: return Gba.Timers.Channels[1].Control;
			case 0x108: return Gba.Timers.GetCounter( 2 );
			case 0x10A: return Gba.Timers.Channels[2].Control;
			case 0x10C: return Gba.Timers.GetCounter( 3 );
			case 0x10E: return Gba.Timers.Channels[3].Control;

			case 0x120:
			case 0x122:
			case 0x124:
			case 0x126:
				return _sioRegs[(int)((offset - 0x120) >> 1)];
			case 0x128: return _sioCnt;
			case 0x12A:
				return _sioRegs[5];
			case 0x12C:
			case 0x12E:
				return 0;
			case 0x130:
				return ReadKeyInputRegister();
			case 0x132: return KeyCnt;
			case 0x134:
				return Rcnt;
			case 0x136:
			case 0x138:
			case 0x13A:
			case 0x13C:
			case 0x13E:
				return 0;
			case 0x140:
				return _sioRegs[16];
			case 0x142:
				return 0;
			case 0x150:
			case 0x152:
				_sioRegs[28] &= unchecked((ushort)~0x0002);
				return _sioRegs[(int)((offset - 0x120) >> 1)];
			case 0x154:
			case 0x156:
			case 0x158:
				return _sioRegs[(int)((offset - 0x120) >> 1)];
			case 0x15A:
				return 0;

			case 0x200: return IE;
			case 0x202: return IF;
			case 0x204: return WaitCnt;
			case 0x206: return 0;
			case 0x208: return IME;
			case 0x20A: return 0;
			case 0x300: return PostFlg;
			case 0x302: return 0;

			default:
				return ReadUnusedRegister( offset );
		}
	}

	public void Write8( uint offset, byte value )
	{
		switch ( offset )
		{
			case 0x062:
			case 0x063:
			case 0x064:
			case 0x065:
			case 0x068:
			case 0x069:
			case 0x06C:
			case 0x06D:
			case 0x072:
			case 0x073:
			case 0x074:
			case 0x075:
			case 0x078:
			case 0x079:
			case 0x07C:
			case 0x07D:
				{
					uint halfOffset = offset & ~1u;
					bool highByte = (offset & 1) != 0;
					Gba.Audio.WriteRegisterByte( halfOffset, highByte, value );
					Gba.Memory.Io[halfOffset >> 1] = Gba.Audio.ReadRegister( halfOffset );
					return;
				}
		}

		uint halfOffset2 = offset & ~1u;
		ushort[] io = Gba.Memory.Io;
		ushort current = io[halfOffset2 >> 1];
		if ( (offset & 1) == 0 )
			current = (ushort)((current & 0xFF00) | value);
		else
			current = (ushort)((current & 0x00FF) | (value << 8));
		Write16( halfOffset2, current );
	}

	public void Write16( uint offset, ushort value )
	{
		ushort[] io = Gba.Memory.Io;
		int ioIndex = (int)(offset >> 1);
		bool writeIo = ioIndex >= 0 && ioIndex < io.Length;

		switch ( offset )
		{
			case 0x000:
				Gba.Video.WriteDispCnt( value );
				value = Gba.Video.DispCnt;
				break;
			case 0x004:
				{
					ushort dispstat = (ushort)((Gba.Video.DispStat & 0x7) | (value & 0xFFF8));
					int lyc = (dispstat >> 8) & 0xFF;
					if ( Gba.Video.VCount == lyc )
					{
						if ( (dispstat & 0x0020) != 0 && (dispstat & 0x0004) == 0 )
							RaiseIrq( GbaIrq.VCounter );
						dispstat |= 0x0004;
					}
					else
					{
						dispstat &= unchecked((ushort)~0x0004);
					}
					Gba.Video.DispStat = dispstat;
					value = dispstat;
					break;
				}
			case 0x006:
				writeIo = false;
				break;
			case 0x008:
				value &= 0xDFFF;
				Gba.Video.WriteBgCnt( 0, value );
				break;
			case 0x00A:
				value &= 0xDFFF;
				Gba.Video.WriteBgCnt( 1, value );
				break;
			case 0x00C: Gba.Video.WriteBgCnt( 2, value ); break;
			case 0x00E: Gba.Video.WriteBgCnt( 3, value ); break;

			case 0x010: value &= 0x01FF; Gba.Video.BgHOfs[0] = (short)value; break;
			case 0x012: value &= 0x01FF; Gba.Video.BgVOfs[0] = (short)value; break;
			case 0x014: value &= 0x01FF; Gba.Video.BgHOfs[1] = (short)value; break;
			case 0x016: value &= 0x01FF; Gba.Video.BgVOfs[1] = (short)value; break;
			case 0x018: value &= 0x01FF; Gba.Video.BgHOfs[2] = (short)value; break;
			case 0x01A: value &= 0x01FF; Gba.Video.BgVOfs[2] = (short)value; break;
			case 0x01C: value &= 0x01FF; Gba.Video.BgHOfs[3] = (short)value; break;
			case 0x01E: value &= 0x01FF; Gba.Video.BgVOfs[3] = (short)value; break;

			case 0x020: Gba.Video.BgPA[0] = (short)value; break;
			case 0x022: Gba.Video.BgPB[0] = (short)value; break;
			case 0x024: Gba.Video.BgPC[0] = (short)value; break;
			case 0x026: Gba.Video.BgPD[0] = (short)value; break;
			case 0x028:
				Gba.Video.BgRefX[0] = (Gba.Video.BgRefX[0] & unchecked((int)0xFFFF0000)) | value;
				Gba.Video.BgX[0] = Gba.Video.BgRefX[0];
				break;
			case 0x02A:
				Gba.Video.BgRefX[0] = (int)((uint)(Gba.Video.BgRefX[0] & 0x0000FFFF) | ((uint)value << 16));
				Gba.Video.BgRefX[0] <<= 4; Gba.Video.BgRefX[0] >>= 4;
				Gba.Video.BgX[0] = Gba.Video.BgRefX[0];
				break;
			case 0x02C:
				Gba.Video.BgRefY[0] = (Gba.Video.BgRefY[0] & unchecked((int)0xFFFF0000)) | value;
				Gba.Video.BgY[0] = Gba.Video.BgRefY[0];
				break;
			case 0x02E:
				Gba.Video.BgRefY[0] = (int)((uint)(Gba.Video.BgRefY[0] & 0x0000FFFF) | ((uint)value << 16));
				Gba.Video.BgRefY[0] <<= 4; Gba.Video.BgRefY[0] >>= 4;
				Gba.Video.BgY[0] = Gba.Video.BgRefY[0];
				break;

			case 0x030: Gba.Video.BgPA[1] = (short)value; break;
			case 0x032: Gba.Video.BgPB[1] = (short)value; break;
			case 0x034: Gba.Video.BgPC[1] = (short)value; break;
			case 0x036: Gba.Video.BgPD[1] = (short)value; break;
			case 0x038:
				Gba.Video.BgRefX[1] = (Gba.Video.BgRefX[1] & unchecked((int)0xFFFF0000)) | value;
				Gba.Video.BgX[1] = Gba.Video.BgRefX[1];
				break;
			case 0x03A:
				Gba.Video.BgRefX[1] = (int)((uint)(Gba.Video.BgRefX[1] & 0x0000FFFF) | ((uint)value << 16));
				Gba.Video.BgRefX[1] <<= 4; Gba.Video.BgRefX[1] >>= 4;
				Gba.Video.BgX[1] = Gba.Video.BgRefX[1];
				break;
			case 0x03C:
				Gba.Video.BgRefY[1] = (Gba.Video.BgRefY[1] & unchecked((int)0xFFFF0000)) | value;
				Gba.Video.BgY[1] = Gba.Video.BgRefY[1];
				break;
			case 0x03E:
				Gba.Video.BgRefY[1] = (int)((uint)(Gba.Video.BgRefY[1] & 0x0000FFFF) | ((uint)value << 16));
				Gba.Video.BgRefY[1] <<= 4; Gba.Video.BgRefY[1] >>= 4;
				Gba.Video.BgY[1] = Gba.Video.BgRefY[1];
				break;

			case 0x040: Gba.Video.Win0H = value; value = Gba.Video.Win0H; break;
			case 0x042: Gba.Video.Win1H = value; value = Gba.Video.Win1H; break;
			case 0x044: Gba.Video.Win0V = value; value = Gba.Video.Win0V; break;
			case 0x046: Gba.Video.Win1V = value; value = Gba.Video.Win1V; break;
			case 0x048: value &= 0x3F3F; Gba.Video.WinIn = value; break;
			case 0x04A: value &= 0x3F3F; Gba.Video.WinOut = value; break;

			case 0x04C: Gba.Video.Mosaic = value; break;

			case 0x050: value &= 0x3FFF; Gba.Video.BldCnt = value; break;
			case 0x052: value &= 0x1F1F; Gba.Video.BldAlpha = value; break;
			case 0x054: value &= 0x001F; Gba.Video.BldY = value; break;

			case 0x060:
			case 0x062:
			case 0x064:
			case 0x066:
			case 0x068:
			case 0x06A:
			case 0x06C:
			case 0x06E:
			case 0x070:
			case 0x072:
			case 0x074:
			case 0x076:
			case 0x078:
			case 0x07A:
			case 0x07C:
			case 0x07E:
			case 0x080:
			case 0x082:
			case 0x084:
			case 0x088:
				if ( offset <= 0x080 && !Gba.Audio.Enable )
				{
					writeIo = false;
					break;
				}
				if ( offset == 0x088 )
					value &= 0xC3FE;
				Gba.Audio.WriteRegister( offset, value );
				value = Gba.Audio.ReadRegister( offset );
				break;
			case 0x090:
			case 0x092:
			case 0x094:
			case 0x096:
			case 0x098:
			case 0x09A:
			case 0x09C:
			case 0x09E:
				{
					int writeBank;
					if ( (Gba.Audio.SoundCntX & 0x80) == 0 )
						writeBank = 1;
					else
						writeBank = (Gba.Audio.Sound3CntL & 0x40) != 0 ? 0 : 1;
					int waveOff = writeBank * 16 + (int)(offset - 0x090);
					Gba.Audio.WaveRam[waveOff] = (byte)value;
					Gba.Audio.WaveRam[waveOff + 1] = (byte)(value >> 8);
					break;
				}
			case 0x0A0:
				_fifoALatch = value;
				break;
			case 0x0A2:
				Gba.Audio.WriteFifo( true, (uint)(_fifoALatch | (value << 16)) );
				break;
			case 0x0A4:
				_fifoBLatch = value;
				break;
			case 0x0A6:
				Gba.Audio.WriteFifo( false, (uint)(_fifoBLatch | (value << 16)) );
				break;

			case >= 0x0B0 and <= 0x0DE:
				{
					int ch = (int)(offset - 0x0B0) / 12;
					switch ( (offset - 0x0B0) % 12 )
					{
						case 0: Gba.Dma.Channels[ch].SrcLow = value; break;
						case 2: Gba.Dma.Channels[ch].SrcHigh = value; break;
						case 4: Gba.Dma.Channels[ch].DstLow = value; break;
						case 6: Gba.Dma.Channels[ch].DstHigh = value; break;
						case 8: Gba.Dma.Channels[ch].CntLo = value; break;
						case 10: Gba.Dma.WriteControl( ch, value ); break;
					}
					break;
				}

			case 0x100: Gba.Timers.Channels[0].Reload = value; break;
			case 0x102: Gba.Timers.WriteControl( 0, value ); break;
			case 0x104: Gba.Timers.Channels[1].Reload = value; break;
			case 0x106: Gba.Timers.WriteControl( 1, value ); break;
			case 0x108: Gba.Timers.Channels[2].Reload = value; break;
			case 0x10A: Gba.Timers.WriteControl( 2, value ); break;
			case 0x10C: Gba.Timers.Channels[3].Reload = value; break;
			case 0x10E: Gba.Timers.WriteControl( 3, value ); break;

			case 0x120:
			case 0x122:
			case 0x12A:
				WriteSioRegister( offset, value );
				value = _sioRegs[(int)((offset - 0x120) >> 1)];
				break;
			case 0x124:
			case 0x126:
				_sioRegs[(int)((offset - 0x120) >> 1)] = value;
				break;
			case 0x12C:
			case 0x12E:
				writeIo = false;
				break;
			case 0x128:
				WriteSioCnt( value );
				value = _sioCnt;
				break;

			case 0x130:
				writeIo = false;
				break;
			case 0x132:
				value &= 0xC3FF;
				if ( Gba.KeysLast < 0x400 )
					Gba.KeysLast &= (ushort)(KeyCnt | ~value);
				KeyCnt = value;
				TestKeypadIrq();
				break;

			case 0x134:
				WriteRcnt( (ushort)(value & 0xC1FF) );
				value = Rcnt;
				break;
			case 0x136:
			case 0x138:
			case 0x13A:
			case 0x13C:
			case 0x13E:
				writeIo = false;
				break;
			case 0x140:
				WriteSioRegister( offset, value );
				value = _sioRegs[16];
				break;
			case 0x142:
				writeIo = false;
				break;
			case 0x150:
			case 0x152:
				WriteSioRegister( offset, value );
				value = _sioRegs[(int)((offset - 0x120) >> 1)];
				break;
			case 0x154:
			case 0x156:
				_sioRegs[28] |= 0x0008;
				WriteSioRegister( offset, value );
				value = _sioRegs[(int)((offset - 0x120) >> 1)];
				break;
			case 0x158:
				WriteSioRegister( offset, value );
				value = _sioRegs[28];
				break;

			case 0x200: IE = value; TestIrq( 1 ); break;
			case 0x202:
				IF &= (ushort)~value;
				TestIrq( 1 );
				value = IF;
				break;
			case 0x204:
				value &= 0x5FFF;
				WaitCnt = value;
				Gba.Memory.AdjustWaitstates( value );
				break;
			case 0x208:
				value = (ushort)(value & 1);
				IME = value;
				TestIrq( 1 );
				break;
			case 0x20A:
			case 0x206:
			case 0x302:
				writeIo = false;
				break;
			case 0x300:
				if ( Gba.Cpu.Gprs[15] >= GbaConstants.BiosSize )
				{
					GbaLog.Write( LogCategory.GBAIO, LogLevel.GameError, "Write to BIOS-only I/O register: {0:X3}", offset );
					writeIo = false;
					break;
				}

				if ( PostFlg != 0 )
				{
					Gba.Cpu.Halted = true;
					value &= 0x7FFF;
				}

				PostFlg = (byte)value;
				value = PostFlg;
				break;
			default:
				GbaLog.Write( LogCategory.GBAIO, LogLevel.GameError, "Write to unused I/O register: {0:X3}", offset );
				writeIo = false;
				break;
		}

		if ( writeIo )
			io[ioIndex] = value;
	}

	public void Serialize( BinaryWriter w )
	{
		w.Write( _irqFireCycle );
		w.Write( _fifoALatch );
		w.Write( _fifoBLatch );
		w.Write( _sioCnt );
		w.Write( _sioMode );
		w.Write( _sioCompletionCycle );
		for ( int i = 0; i < _sioRegs.Length; i++ )
			w.Write( _sioRegs[i] );
		w.Write( Gba.KeysLast );
	}

	public void Deserialize( BinaryReader r )
	{
		_irqFireCycle = r.ReadInt64();
		_fifoALatch = r.ReadUInt16();
		_fifoBLatch = r.ReadUInt16();
		_sioCnt = r.ReadUInt16();
		_sioMode = r.ReadInt32();
		_sioCompletionCycle = r.ReadInt64();
		for ( int i = 0; i < _sioRegs.Length; i++ )
			_sioRegs[i] = r.ReadUInt16();
		Gba.KeysLast = r.ReadUInt16();
	}
}

[Flags]
public enum GbaIrq : ushort
{
	VBlank = 1 << 0,
	HBlank = 1 << 1,
	VCounter = 1 << 2,
	Timer0 = 1 << 3,
	Timer1 = 1 << 4,
	Timer2 = 1 << 5,
	Timer3 = 1 << 6,
	Sio = 1 << 7,
	Dma0 = 1 << 8,
	Dma1 = 1 << 9,
	Dma2 = 1 << 10,
	Dma3 = 1 << 11,
	Keypad = 1 << 12,
	GamePak = 1 << 13,
}
