namespace sGBA;

public class Gba
{
	public ArmCore Cpu { get; private set; }
	public GbaMemory Memory { get; private set; }
	public GbaVideo Video { get; private set; }
	public GbaIo Io { get; private set; }
	public GbaDmaController Dma { get; private set; }
	public GbaTimerController Timers { get; private set; }
	public GbaBios Bios { get; private set; }
	public GbaAudio Audio { get; private set; }
	public GbaSavedata Savedata { get; private set; }
	public GbaCartridgeHardware Hardware { get; private set; }

	public bool IsRunning { get; set; }
	public int CyclesThisFrame { get; set; }
	public long FrameCounter { get; set; }
	public long TotalCycles { get; set; }
	public ushort KeysActive { get; set; }
	public ushort KeysLast { get; set; } = 0x400;
	public bool AllowOpposingDirections { get; set; } = true;
	public bool HaltPending { get; set; }

	public Gba()
	{
		Memory = new GbaMemory( this );
		Cpu = new ArmCore( this );
		Video = new GbaVideo( this );
		Io = new GbaIo( this );
		Dma = new GbaDmaController( this );
		Timers = new GbaTimerController( this );
		Bios = new GbaBios( this );
		Audio = new GbaAudio( this );
		Savedata = new GbaSavedata( this );
		Hardware = new GbaCartridgeHardware( this );
	}

	public void LoadRom( byte[] romData )
	{
		Memory.LoadRom( romData );
		Savedata.ForceType( romData );
		Hardware.InitRtc( romData );
	}

	public void Reset()
	{
		Memory.Reset();
		Cpu.Reset();
		Video.Reset();
		Io.Reset();
		Dma.Reset();
		Timers.Reset();
		Audio.Reset();
		Savedata.Reset();
		Hardware.Reset();
		Memory.InstallHleBios();
		Cpu.SkipBios();

		Io.ApplySkipBiosState();

		KeysLast = 0x400;
		HaltPending = false;

		CyclesThisFrame = 0;
		FrameCounter = 0;
		TotalCycles = 0;
		IsRunning = true;
		_frameInProgress = false;
		_frameLine = 0;
		_framePhase = 0;
	}

	public void RunFrame()
	{
		if ( !IsRunning ) return;

		Audio.BeginFrame();
		Io.TestKeypadIrq();

		long frameBase = Cpu.Cycles;

		for ( int line = 0; line < GbaConstants.VideoVerticalTotalPixels; line++ )
		{
			long lineBase = frameBase + (long)line * GbaConstants.VideoHorizontalLength;
			RunCpuTo( lineBase + GbaConstants.VideoHDrawLength );
			Video.StartHBlank();
			RunCpuTo( lineBase + GbaConstants.VideoHorizontalLength );
			Video.StartHDraw();
		}

		long frameEnd = frameBase + GbaConstants.VideoTotalLength;
		if ( Cpu.Cycles < frameEnd )
			Cpu.Cycles = frameEnd;

		FrameCounter++;
		TotalCycles += GbaConstants.VideoTotalLength;
	}

	private bool _frameInProgress;
	private long _frameBase;
	private int _frameLine;
	private int _framePhase;

	public bool FrameInProgress => _frameInProgress;

	public bool StepFrame()
	{
		if ( !IsRunning ) return false;
		if ( Io.LockstepBlocked ) return false;

		if ( !_frameInProgress )
		{
			Audio.BeginFrame();
			Io.TestKeypadIrq();
			_frameBase = Cpu.Cycles;
			_frameLine = 0;
			_framePhase = 0;
			_frameInProgress = true;
		}

		while ( _frameLine < GbaConstants.VideoVerticalTotalPixels )
		{
			long lineBase = _frameBase + (long)_frameLine * GbaConstants.VideoHorizontalLength;

			if ( _framePhase == 0 )
			{
				long target = lineBase + GbaConstants.VideoHDrawLength;
				RunCpuTo( target );
				if ( Cpu.Cycles < target )
					return false;
				Video.StartHBlank();
				_framePhase = 1;
			}

			long lineEnd = lineBase + GbaConstants.VideoHorizontalLength;
			RunCpuTo( lineEnd );
			if ( Cpu.Cycles < lineEnd )
				return false;
			Video.StartHDraw();
			_framePhase = 0;
			_frameLine++;
		}

		long frameEnd = _frameBase + GbaConstants.VideoTotalLength;
		if ( Cpu.Cycles < frameEnd )
			Cpu.Cycles = frameEnd;

		FrameCounter++;
		TotalCycles += GbaConstants.VideoTotalLength;
		_frameInProgress = false;
		return true;
	}

	public void ProcessEvents( long startCycle, long endCycle )
	{
		if ( endCycle < startCycle )
			return;

		if ( startCycle == endCycle )
		{
			if ( HasDueEvents( startCycle ) )
			{
				Cpu.Cycles = startCycle;
				ProcessPendingEvents();
			}
			return;
		}

		long targetCycle = endCycle;
		Cpu.Cycles = startCycle;

		if ( !HasDueEvents( startCycle ) )
		{
			long nextEvent = GetNextEvent( long.MaxValue );
			if ( nextEvent > targetCycle )
			{
				Audio.Tick( (int)(targetCycle - startCycle) );
				Cpu.Cycles = targetCycle;
				return;
			}
		}

		while ( Cpu.Cycles < targetCycle )
		{
			long nextEvent = GetNextEvent( targetCycle );

			if ( nextEvent > Cpu.Cycles )
			{
				long currentCycle = Cpu.Cycles;
				Cpu.Cycles = nextEvent;
				Audio.Tick( (int)(nextEvent - currentCycle) );
			}

			if ( !ProcessPendingEvents() )
				break;
		}

		if ( HasDueEvents( Cpu.Cycles ) )
			ProcessPendingEvents();

		Cpu.Cycles = targetCycle;
	}

	private bool HasDueEvents( long cycle )
	{
		return Io.NextIrqEvent <= cycle || Timers.NextGlobalEvent <= cycle || Io.NextSioEvent <= cycle || Io.NextDriverEvent <= cycle;
	}

	private long GetNextEvent( long defaultCycle )
	{
		long nextEvent = defaultCycle;
		if ( Io.NextIrqEvent < nextEvent )
			nextEvent = Io.NextIrqEvent;
		if ( Io.NextSioEvent < nextEvent )
			nextEvent = Io.NextSioEvent;
		if ( Io.NextDriverEvent < nextEvent )
			nextEvent = Io.NextDriverEvent;
		if ( Timers.NextGlobalEvent < nextEvent )
			nextEvent = Timers.NextGlobalEvent;
		return nextEvent;
	}

	private bool ProcessPendingEvents()
	{
		bool processedAny = false;
		Io.BeginEventProcessing();

		try
		{
			while ( true )
			{
				bool processedEvent = false;

				if ( Io.NextIrqEvent <= Cpu.Cycles )
				{
					Io.ProcessIrqEvent();
					processedEvent = true;
				}

				if ( Timers.NextGlobalEvent <= Cpu.Cycles )
				{
					Timers.Tick( 0 );
					processedEvent = true;
				}

				if ( Io.NextSioEvent <= Cpu.Cycles )
				{
					Io.FinishSioTransfer();
					processedEvent = true;
				}

				if ( Io.NextDriverEvent <= Cpu.Cycles )
				{
					Io.ProcessDriverEvent();
					processedEvent = true;
				}

				if ( !processedEvent )
					return processedAny;

				processedAny = true;
			}
		}
		finally
		{
			Io.EndEventProcessing();
		}
	}

	private void RunCpuTo( long target )
	{
		while ( Cpu.Cycles < target )
		{
			if ( Io.LockstepBlocked )
			{
				if ( Io.NextDriverEvent <= Cpu.Cycles )
					Io.ProcessDriverEvent();
				if ( Io.LockstepBlocked )
					return;
			}

			if ( Dma.ActiveDma >= 0 )
			{
				ProcessDma( target );
				continue;
			}

			if ( Cpu.Halted )
			{
				ProcessHalt( target );
				continue;
			}

			Cpu.Run( target );
		}
	}

	private void ProcessDma( long target )
	{
		while ( Dma.ActiveDma >= 0 && Cpu.Cycles < target )
		{
			var ch = Dma.Channels[Dma.ActiveDma];

			if ( ch.When > Cpu.Cycles )
			{
				long runTo = Math.Min( ch.When, target );
				if ( Cpu.Halted )
					AdvanceClock( (int)(runTo - Cpu.Cycles) );
				else
					Cpu.Run( runTo );
				continue;
			}

			int unitCost = Dma.ServiceUnit();
			AdvanceClock( unitCost );
		}
	}

	private void ProcessHalt( long target )
	{
		while ( Cpu.Cycles < target && Cpu.Halted )
		{
			if ( Io.LockstepBlocked )
				return;

			long nextEvent = GetNextEvent( target );
			if ( Dma.ActiveDma >= 0 )
				nextEvent = Math.Min( nextEvent, Dma.Channels[Dma.ActiveDma].When );

			int chunk = Math.Max( 1, (int)(nextEvent - Cpu.Cycles) );
			AdvanceClock( chunk );

			if ( Dma.ActiveDma >= 0 && Dma.Channels[Dma.ActiveDma].When <= Cpu.Cycles )
				ProcessDma( target );
		}
	}

	private void AdvanceClock( int cycles )
	{
		long startCycle = Cpu.Cycles;
		Cpu.Cycles += cycles;
		ProcessEvents( startCycle, Cpu.Cycles );
	}

	public void SetKeyState( GbaKey key, bool pressed )
	{
		if ( pressed )
			KeysActive |= (ushort)key;
		else
			KeysActive &= (ushort)~(ushort)key;
		Io.TestKeypadIrq();
	}
}

[Flags]
public enum GbaKey : ushort
{
	A = 1 << 0,
	B = 1 << 1,
	Select = 1 << 2,
	Start = 1 << 3,
	Right = 1 << 4,
	Left = 1 << 5,
	Up = 1 << 6,
	Down = 1 << 7,
	R = 1 << 8,
	L = 1 << 9,
}
