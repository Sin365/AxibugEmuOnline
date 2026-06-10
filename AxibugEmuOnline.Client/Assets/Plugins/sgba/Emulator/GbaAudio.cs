using System.IO;

namespace sGBA;

public partial class GbaAudio
{
	public Gba Gba { get; }

	public const int SampleRate = 32768;
	public const int CyclesPerSample = GbaConstants.Arm7TdmiFrequency / SampleRate;
	public const int SamplesPerFrame = (GbaConstants.VideoTotalLength + CyclesPerSample - 1) / CyclesPerSample;
	public short[] OutputBuffer { get; set; } = new short[SamplesPerFrame * 2];
	public int SamplesWritten { get; set; }

	private long _nextSampleCycle;
	private long _nextFrameSeqCycle;

	private const int TimingFactor = 4;
	private const int FrameCycles = 0x2000;
	private const int CyclesPerFrameSeq = TimingFactor * FrameCycles;
	private int _frameSeqStep;

	private long _totalCycles;

	public bool Enable;

	public ushort Sound1CntL, Sound1CntH, Sound1CntX;
	public ushort Sound2CntL, Sound2CntH;
	public ushort Sound3CntL, Sound3CntH, Sound3CntX;
	public ushort Sound4CntL, Sound4CntH;
	public ushort SoundCntL, SoundCntH, SoundCntX;
	public ushort SoundBias;

	private int _volumeRight;
	private int _volumeLeft;
	private bool _psgCh1Right, _psgCh2Right, _psgCh3Right, _psgCh4Right;
	private bool _psgCh1Left, _psgCh2Left, _psgCh3Left, _psgCh4Left;

	private int _psgVolume;
	private bool _volumeChA;
	private bool _volumeChB;
	private bool _chARight;
	private bool _chALeft;
	private bool _chATimer;
	private bool _chBRight;
	private bool _chBLeft;
	private bool _chBTimer;

	public byte[] WaveRam = new byte[32];

	private struct FifoState
	{
		public uint[] Buffer;
		public int Write, Read;
		public uint Internal;
		public int Remaining;
		public sbyte Sample;
	}

	private FifoState _fifoA;
	private FifoState _fifoB;

	private bool _ch1Playing;
	private int _ch1Frequency;
	private int _ch1Length;
	private bool _ch1Stop;
	private int _ch1DutyIndex;
	private int _ch1Duty;
	private int _ch1Sample;
	private long _ch1LastUpdate;

	private int _ch1EnvVolume;
	private int _ch1EnvStepTime;
	private bool _ch1EnvDirection;
	private int _ch1EnvInitVolume;
	private int _ch1EnvDead;
	private int _ch1EnvNextStep;

	private int _ch1SweepShift;
	private bool _ch1SweepDirection;
	private int _ch1SweepTime;
	private int _ch1SweepStep;
	private bool _ch1SweepEnable;
	private bool _ch1SweepOccurred;
	private int _ch1SweepRealFreq;

	private bool _ch2Playing;
	private int _ch2Frequency;
	private int _ch2Length;
	private bool _ch2Stop;
	private int _ch2DutyIndex;
	private int _ch2Duty;
	private int _ch2Sample;
	private long _ch2LastUpdate;

	private int _ch2EnvVolume;
	private int _ch2EnvStepTime;
	private bool _ch2EnvDirection;
	private int _ch2EnvInitVolume;
	private int _ch2EnvDead;
	private int _ch2EnvNextStep;

	private bool _ch3Playing;
	private bool _ch3Enable;
	private bool _ch3Size;
	private bool _ch3Bank;
	private int _ch3Volume;
	private int _ch3Rate;
	private int _ch3Length;
	private bool _ch3Stop;
	private int _ch3Window;
	private int _ch3Sample;
	private long _ch3NextUpdate;

	private bool _ch4Playing;
	private int _ch4Ratio;
	private int _ch4Frequency;
	private bool _ch4Power;
	private int _ch4Length;
	private bool _ch4Stop;
	private uint _ch4Lfsr;
	private int _ch4Sample;
	private long _ch4LastEvent;

	private int _ch4EnvVolume;
	private int _ch4EnvStepTime;
	private bool _ch4EnvDirection;
	private int _ch4EnvInitVolume;
	private int _ch4EnvDead;
	private int _ch4EnvNextStep;

	private static readonly int[] DutyTable =
	[
		0, 0, 0, 0, 0, 0, 0, 1,
		1, 0, 0, 0, 0, 0, 0, 1,
		1, 0, 0, 0, 0, 1, 1, 1,
		0, 1, 1, 1, 1, 1, 1, 0,
	];

	public GbaAudio( Gba gba )
	{
		Gba = gba;
		SoundBias = 0x0200;
		_fifoA.Buffer = new uint[8];
		_fifoB.Buffer = new uint[8];
	}

	public void Reset()
	{
		SamplesWritten = 0;
		_frameSeqStep = 0;
		_totalCycles = 0;
		_nextSampleCycle = CyclesPerSample;
		_nextFrameSeqCycle = 0;

		Sound1CntL = Sound1CntH = Sound1CntX = 0;
		Sound2CntL = Sound2CntH = 0;
		Sound3CntL = Sound3CntH = Sound3CntX = 0;
		Sound4CntL = Sound4CntH = 0;
		SoundCntL = SoundCntH = SoundCntX = 0;
		SoundBias = 0x0200;

		Enable = false;
		_psgVolume = 0;
		_volumeChA = false;
		_volumeChB = false;
		_chARight = _chALeft = false;
		_chATimer = false;
		_chBRight = _chBLeft = false;
		_chBTimer = false;
		_volumeRight = _volumeLeft = 0;
		_psgCh1Right = _psgCh2Right = _psgCh3Right = _psgCh4Right = false;
		_psgCh1Left = _psgCh2Left = _psgCh3Left = _psgCh4Left = false;

		Array.Clear( WaveRam );

		ResetFifo( true, true );
		ResetFifo( false, true );

		_ch1Playing = _ch2Playing = _ch3Playing = _ch4Playing = false;
		_ch1EnvDead = _ch2EnvDead = _ch4EnvDead = 2;
		_ch1SweepTime = 8;
		_ch1EnvVolume = _ch2EnvVolume = _ch4EnvVolume = 0;
		_ch1EnvInitVolume = _ch2EnvInitVolume = _ch4EnvInitVolume = 0;
		_ch1EnvStepTime = _ch2EnvStepTime = _ch4EnvStepTime = 0;
		_ch1EnvDirection = _ch2EnvDirection = _ch4EnvDirection = false;
		_ch1EnvNextStep = _ch2EnvNextStep = _ch4EnvNextStep = 0;
		_ch1Frequency = _ch2Frequency = 0;
		_ch1Length = _ch2Length = _ch3Length = _ch4Length = 0;
		_ch1Stop = _ch2Stop = _ch3Stop = _ch4Stop = false;
		_ch1DutyIndex = _ch2DutyIndex = 0;
		_ch1Duty = _ch2Duty = 0;
		_ch1Sample = _ch2Sample = _ch3Sample = _ch4Sample = 0;
		_ch1LastUpdate = _ch2LastUpdate = 0;
		_ch3NextUpdate = 0;
		_ch4LastEvent = 0;
		_ch1SweepShift = 0;
		_ch1SweepDirection = false;
		_ch1SweepStep = 0;
		_ch1SweepEnable = false;
		_ch1SweepOccurred = false;
		_ch1SweepRealFreq = 0;
		_ch3Enable = false;
		_ch3Size = false;
		_ch3Bank = false;
		_ch3Volume = 0;
		_ch3Rate = 0;
		_ch3Window = 0;
		_ch4Ratio = 0;
		_ch4Frequency = 0;
		_ch4Power = false;
		_ch4Lfsr = 0;
	}

	public void ResetFifo( bool isA, bool clearLatched )
	{
		ref var fifo = ref isA ? ref _fifoA : ref _fifoB;
		Array.Clear( fifo.Buffer );
		fifo.Write = fifo.Read = 0;
		fifo.Internal = 0;
		fifo.Remaining = 0;
		if ( clearLatched ) fifo.Sample = 0;
	}

	public void BeginFrame()
	{
		SamplesWritten = 0;
	}

	public void WriteFifo( bool isA, uint value )
	{
		ref var fifo = ref isA ? ref _fifoA : ref _fifoB;
		fifo.Buffer[fifo.Write] = value;
		fifo.Write = (fifo.Write + 1) & 7;
	}

	public void OnTimerOverflow( int timer )
	{
		if ( !Enable ) return;

		if ( (_chALeft || _chARight) && (_chATimer ? 1 : 0) == timer )
			SampleFifo( ref _fifoA, 1 );

		if ( (_chBLeft || _chBRight) && (_chBTimer ? 1 : 0) == timer )
			SampleFifo( ref _fifoB, 2 );
	}

	private void SampleFifo( ref FifoState fifo, int dmaChannel )
	{
		int size = FifoSize( ref fifo );

		if ( 8 - size > 4 )
		{
			Gba.Dma.OnFifo( dmaChannel );
		}

		if ( fifo.Remaining == 0 && size > 0 )
		{
			fifo.Internal = fifo.Buffer[fifo.Read];
			fifo.Remaining = 4;
			fifo.Read = (fifo.Read + 1) & 7;
		}

		if ( fifo.Remaining > 0 )
		{
			fifo.Sample = (sbyte)(fifo.Internal & 0xFF);
			fifo.Internal >>= 8;
			fifo.Remaining--;
		}
	}

	private static int FifoSize( ref FifoState fifo )
	{
		return fifo.Write >= fifo.Read
			? fifo.Write - fifo.Read
			: 8 - fifo.Read + fifo.Write;
	}

	public void Tick( int cycles )
	{
		TickTo( Gba.Cpu.Cycles );
	}

	public void FlushSamples()
	{
		TickTo( Gba.Cpu.Cycles );
	}

	private void TickTo( long target )
	{
		if ( target <= _totalCycles )
			return;

		while ( true )
		{
			long nextEvent = Math.Min( _nextSampleCycle, _nextFrameSeqCycle );

			if ( nextEvent > target )
				break;

			_totalCycles = nextEvent;

			if ( _nextSampleCycle == nextEvent )
				RunSampleEvent();

			if ( _nextFrameSeqCycle == nextEvent )
				RunFrameSequencerEvent();
		}

		_totalCycles = target;
	}

	private void RunSampleEvent()
	{
		_nextSampleCycle += CyclesPerSample;
		WriteSample();
	}

	private void RunFrameSequencerEvent()
	{
		_nextFrameSeqCycle += CyclesPerFrameSeq;
		ClockFrameSequencer();
	}

	private void WriteSample()
	{
		if ( SamplesWritten >= SamplesPerFrame )
			return;

		short left;
		short right;
		if ( Enable )
			MixSample( out left, out right );
		else
		{
			left = 0;
			right = 0;
		}

		OutputBuffer[SamplesWritten * 2] = left;
		OutputBuffer[SamplesWritten * 2 + 1] = right;
		SamplesWritten++;
	}

	public void Serialize( BinaryWriter w )
	{
		w.Write( _nextSampleCycle );
		w.Write( _nextFrameSeqCycle );
		w.Write( _frameSeqStep );
		w.Write( _totalCycles );

		w.Write( _volumeRight ); w.Write( _volumeLeft );
		w.Write( _psgCh1Right ); w.Write( _psgCh2Right ); w.Write( _psgCh3Right ); w.Write( _psgCh4Right );
		w.Write( _psgCh1Left ); w.Write( _psgCh2Left ); w.Write( _psgCh3Left ); w.Write( _psgCh4Left );
		w.Write( _psgVolume );
		w.Write( _volumeChA ); w.Write( _volumeChB );
		w.Write( _chARight ); w.Write( _chALeft ); w.Write( _chATimer );
		w.Write( _chBRight ); w.Write( _chBLeft ); w.Write( _chBTimer );

		WriteFifo( w, ref _fifoA );
		WriteFifo( w, ref _fifoB );

		w.Write( _ch1Playing ); w.Write( _ch1Frequency ); w.Write( _ch1Length ); w.Write( _ch1Stop );
		w.Write( _ch1DutyIndex ); w.Write( _ch1Duty ); w.Write( _ch1Sample ); w.Write( _ch1LastUpdate );
		w.Write( _ch1EnvVolume ); w.Write( _ch1EnvStepTime ); w.Write( _ch1EnvDirection );
		w.Write( _ch1EnvInitVolume ); w.Write( _ch1EnvDead ); w.Write( _ch1EnvNextStep );
		w.Write( _ch1SweepShift ); w.Write( _ch1SweepDirection ); w.Write( _ch1SweepTime );
		w.Write( _ch1SweepStep ); w.Write( _ch1SweepEnable ); w.Write( _ch1SweepOccurred ); w.Write( _ch1SweepRealFreq );

		w.Write( _ch2Playing ); w.Write( _ch2Frequency ); w.Write( _ch2Length ); w.Write( _ch2Stop );
		w.Write( _ch2DutyIndex ); w.Write( _ch2Duty ); w.Write( _ch2Sample ); w.Write( _ch2LastUpdate );
		w.Write( _ch2EnvVolume ); w.Write( _ch2EnvStepTime ); w.Write( _ch2EnvDirection );
		w.Write( _ch2EnvInitVolume ); w.Write( _ch2EnvDead ); w.Write( _ch2EnvNextStep );

		w.Write( _ch3Playing ); w.Write( _ch3Enable ); w.Write( _ch3Size ); w.Write( _ch3Bank );
		w.Write( _ch3Volume ); w.Write( _ch3Rate ); w.Write( _ch3Length ); w.Write( _ch3Stop );
		w.Write( _ch3Window ); w.Write( _ch3Sample ); w.Write( _ch3NextUpdate );

		w.Write( _ch4Playing ); w.Write( _ch4Ratio ); w.Write( _ch4Frequency ); w.Write( _ch4Power );
		w.Write( _ch4Length ); w.Write( _ch4Stop ); w.Write( _ch4Lfsr ); w.Write( _ch4Sample ); w.Write( _ch4LastEvent );
		w.Write( _ch4EnvVolume ); w.Write( _ch4EnvStepTime ); w.Write( _ch4EnvDirection );
		w.Write( _ch4EnvInitVolume ); w.Write( _ch4EnvDead ); w.Write( _ch4EnvNextStep );
	}

	public void Deserialize( BinaryReader r )
	{
		_nextSampleCycle = r.ReadInt64();
		_nextFrameSeqCycle = r.ReadInt64();
		_frameSeqStep = r.ReadInt32();
		_totalCycles = r.ReadInt64();

		_volumeRight = r.ReadInt32(); _volumeLeft = r.ReadInt32();
		_psgCh1Right = r.ReadBoolean(); _psgCh2Right = r.ReadBoolean(); _psgCh3Right = r.ReadBoolean(); _psgCh4Right = r.ReadBoolean();
		_psgCh1Left = r.ReadBoolean(); _psgCh2Left = r.ReadBoolean(); _psgCh3Left = r.ReadBoolean(); _psgCh4Left = r.ReadBoolean();
		_psgVolume = r.ReadInt32();
		_volumeChA = r.ReadBoolean(); _volumeChB = r.ReadBoolean();
		_chARight = r.ReadBoolean(); _chALeft = r.ReadBoolean(); _chATimer = r.ReadBoolean();
		_chBRight = r.ReadBoolean(); _chBLeft = r.ReadBoolean(); _chBTimer = r.ReadBoolean();

		ReadFifo( r, ref _fifoA );
		ReadFifo( r, ref _fifoB );

		_ch1Playing = r.ReadBoolean(); _ch1Frequency = r.ReadInt32(); _ch1Length = r.ReadInt32(); _ch1Stop = r.ReadBoolean();
		_ch1DutyIndex = r.ReadInt32(); _ch1Duty = r.ReadInt32(); _ch1Sample = r.ReadInt32(); _ch1LastUpdate = r.ReadInt64();
		_ch1EnvVolume = r.ReadInt32(); _ch1EnvStepTime = r.ReadInt32(); _ch1EnvDirection = r.ReadBoolean();
		_ch1EnvInitVolume = r.ReadInt32(); _ch1EnvDead = r.ReadInt32(); _ch1EnvNextStep = r.ReadInt32();
		_ch1SweepShift = r.ReadInt32(); _ch1SweepDirection = r.ReadBoolean(); _ch1SweepTime = r.ReadInt32();
		_ch1SweepStep = r.ReadInt32(); _ch1SweepEnable = r.ReadBoolean(); _ch1SweepOccurred = r.ReadBoolean(); _ch1SweepRealFreq = r.ReadInt32();

		_ch2Playing = r.ReadBoolean(); _ch2Frequency = r.ReadInt32(); _ch2Length = r.ReadInt32(); _ch2Stop = r.ReadBoolean();
		_ch2DutyIndex = r.ReadInt32(); _ch2Duty = r.ReadInt32(); _ch2Sample = r.ReadInt32(); _ch2LastUpdate = r.ReadInt64();
		_ch2EnvVolume = r.ReadInt32(); _ch2EnvStepTime = r.ReadInt32(); _ch2EnvDirection = r.ReadBoolean();
		_ch2EnvInitVolume = r.ReadInt32(); _ch2EnvDead = r.ReadInt32(); _ch2EnvNextStep = r.ReadInt32();

		_ch3Playing = r.ReadBoolean(); _ch3Enable = r.ReadBoolean(); _ch3Size = r.ReadBoolean(); _ch3Bank = r.ReadBoolean();
		_ch3Volume = r.ReadInt32(); _ch3Rate = r.ReadInt32(); _ch3Length = r.ReadInt32(); _ch3Stop = r.ReadBoolean();
		_ch3Window = r.ReadInt32(); _ch3Sample = r.ReadInt32(); _ch3NextUpdate = r.ReadInt64();

		_ch4Playing = r.ReadBoolean(); _ch4Ratio = r.ReadInt32(); _ch4Frequency = r.ReadInt32(); _ch4Power = r.ReadBoolean();
		_ch4Length = r.ReadInt32(); _ch4Stop = r.ReadBoolean(); _ch4Lfsr = r.ReadUInt32(); _ch4Sample = r.ReadInt32(); _ch4LastEvent = r.ReadInt64();
		_ch4EnvVolume = r.ReadInt32(); _ch4EnvStepTime = r.ReadInt32(); _ch4EnvDirection = r.ReadBoolean();
		_ch4EnvInitVolume = r.ReadInt32(); _ch4EnvDead = r.ReadInt32(); _ch4EnvNextStep = r.ReadInt32();
	}

	private static void WriteFifo( BinaryWriter w, ref FifoState fifo )
	{
		for ( int i = 0; i < 8; i++ ) w.Write( fifo.Buffer[i] );
		w.Write( fifo.Write ); w.Write( fifo.Read );
		w.Write( fifo.Internal ); w.Write( fifo.Remaining );
		w.Write( fifo.Sample );
	}

	private static void ReadFifo( BinaryReader r, ref FifoState fifo )
	{
		for ( int i = 0; i < 8; i++ ) fifo.Buffer[i] = r.ReadUInt32();
		fifo.Write = r.ReadInt32(); fifo.Read = r.ReadInt32();
		fifo.Internal = r.ReadUInt32(); fifo.Remaining = r.ReadInt32();
		fifo.Sample = r.ReadSByte();
	}
}
