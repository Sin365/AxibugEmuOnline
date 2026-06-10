namespace sGBA;

public class GbaTimerController
{
	public Gba Gba { get; }
	public GbaTimer[] Channels = new GbaTimer[4];
	public long NextGlobalEvent = long.MaxValue;

	private static readonly int[] PrescaleBits = [0, 6, 8, 10];

	public GbaTimerController( Gba gba )
	{
		Gba = gba;
		for ( int i = 0; i < 4; i++ )
			Channels[i] = new GbaTimer( i );
	}

	public void Reset()
	{
		for ( int i = 0; i < 4; i++ )
			Channels[i].Reset();
		NextGlobalEvent = long.MaxValue;
	}

	public void WriteControl( int idx, ushort value )
	{
		value &= 0x00C7;
		var c = Channels[idx];
		bool wasEnabled = c.Enabled;
		bool wasCountUp = c.CountUp;
		int oldPrescaleBits = c.PrescaleBits;

		if ( wasEnabled && !wasCountUp )
		{
			SyncCounter( idx );
		}

		c.Control = value;
		c.Enabled = (value & 0x80) != 0;
		c.CountUp = idx > 0 && (value & 0x04) != 0;
		c.DoIrq = (value & 0x40) != 0;
		c.PrescaleBits = value & 3;

		bool reschedule = false;

		if ( wasEnabled != c.Enabled )
		{
			reschedule = true;
			if ( c.Enabled )
			{
				c.Counter = c.Reload;
			}
		}
		else if ( wasCountUp != c.CountUp )
		{
			reschedule = true;
		}
		else if ( c.PrescaleBits != oldPrescaleBits )
		{
			reschedule = true;
		}

		if ( reschedule )
		{
			c.NextOverflowCycle = long.MaxValue;
			if ( c.Enabled && !c.CountUp )
			{
				int bits = PrescaleBits[c.PrescaleBits];
				long tickMask = (1L << bits) - 1;
				c.LastEvent = Gba.Cpu.InstructionStartCycles & ~tickMask;
				ScheduleOverflow( c );
			}
			RecalcGlobalEvent();
		}
	}

	public void RecalcGlobalEvent()
	{
		long min = long.MaxValue;
		for ( int i = 0; i < 4; i++ )
		{
			var ch = Channels[i];
			if ( ch.Enabled && !ch.CountUp && ch.NextOverflowCycle < min )
				min = ch.NextOverflowCycle;
		}
		NextGlobalEvent = min;
	}

	private void ScheduleOverflow( GbaTimer c )
	{
		int bits = PrescaleBits[c.PrescaleBits];
		long tickMask = (1L << bits) - 1;
		long ticksToOverflow = (long)(0x10000 - c.Counter) << bits;
		c.NextOverflowCycle = (c.LastEvent & ~tickMask) + ticksToOverflow;
	}

	private void SyncCounter( int idx )
	{
		var c = Channels[idx];
		if ( !c.Enabled || c.CountUp ) return;

		int bits = PrescaleBits[c.PrescaleBits];
		long tickMask = (1L << bits) - 1;
		long currentCycle = Gba.Cpu.InstructionStartCycles & ~tickMask;
		long ticks = (currentCycle - c.LastEvent) >> bits;
		c.LastEvent = currentCycle;

		long total = c.Counter + ticks;
		int reload = c.Reload;
		int range = 0x10000 - reload;

		while ( total >= 0x10000 )
		{
			if ( range <= 0 ) { total = reload; break; }
			total -= range;
		}

		c.Counter = (ushort)total;
	}

	public ushort GetCounter( int idx )
	{
		var c = Channels[idx];
		if ( !c.Enabled || c.CountUp ) return c.Counter;

		int bits = PrescaleBits[c.PrescaleBits];
		long tickMask = (1L << bits) - 1;
		long adjustedCycle = (Gba.Cpu.InstructionStartCycles - 2) & ~tickMask;
		long ticks = (adjustedCycle - c.LastEvent) >> bits;

		int result = c.Counter + (int)ticks;

		int reload = c.Reload;
		int range = 0x10000 - reload;
		while ( result >= 0x10000 )
		{
			if ( range <= 0 ) { result = reload; break; }
			result -= range;
		}

		return (ushort)(result & 0xFFFF);
	}

	public void Tick( int cycles )
	{
		long currentCycle = Gba.Cpu.Cycles;

		if ( currentCycle < NextGlobalEvent )
			return;

		for ( int i = 0; i < 4; i++ )
		{
			var c = Channels[i];
			if ( !c.Enabled || c.CountUp ) continue;

			while ( currentCycle >= c.NextOverflowCycle )
			{
				long overflowCycle = c.NextOverflowCycle;
				c.Counter = c.Reload;
				c.LastEvent = overflowCycle;

				int bits = PrescaleBits[c.PrescaleBits];
				long ticksToOverflow = (long)(0x10000 - c.Counter) << bits;
				if ( ticksToOverflow <= 0 ) ticksToOverflow = 1;
				c.NextOverflowCycle = overflowCycle + ticksToOverflow;

				if ( c.DoIrq )
				{
					int late = (int)(currentCycle - overflowCycle);
					Gba.Io.RaiseIrq( (GbaIrq)(1 << (3 + i)), late );
				}

				if ( i <= 1 )
				{
					Gba.Audio.OnTimerOverflow( i );
				}

				if ( i < 3 )
				{
					var next = Channels[i + 1];
					if ( next.Enabled && next.CountUp )
					{
						int cascadeLate = (int)(currentCycle - overflowCycle);
						IncrementCascade( i + 1, cascadeLate );
					}
				}
			}
		}

		RecalcGlobalEvent();
	}

	private void IncrementCascade( int idx, int late )
	{
		var c = Channels[idx];
		c.Counter++;
		if ( c.Counter == 0 )
		{
			c.Counter = c.Reload;

			if ( c.DoIrq )
			{
				Gba.Io.RaiseIrq( (GbaIrq)(1 << (3 + idx)), late );
			}

			if ( idx <= 1 )
			{
				Gba.Audio.OnTimerOverflow( idx );
			}

			if ( idx < 3 )
			{
				var next = Channels[idx + 1];
				if ( next.Enabled && next.CountUp )
				{
					IncrementCascade( idx + 1, late );
				}
			}
		}
	}
}

public class GbaTimer
{
	public int Index;
	public ushort Reload;
	public ushort Counter;
	public ushort Control;

	public bool Enabled;
	public bool CountUp;
	public bool DoIrq;
	public int PrescaleBits;

	public long LastEvent;
	public long NextOverflowCycle = long.MaxValue;

	public GbaTimer( int index )
	{
		Index = index;
	}

	public void Reset()
	{
		Reload = Counter = Control = 0;
		Enabled = CountUp = DoIrq = false;
		PrescaleBits = 0;
		LastEvent = 0;
		NextOverflowCycle = long.MaxValue;
	}
}
