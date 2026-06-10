namespace sGBA;

public partial class GbaVideo
{
	public Gba Gba { get; }

	public int VCount;
	public int Dot;

	public ushort DispCnt;
	public ushort DispStat;
	public ushort[] BgCnt = new ushort[4];
	public short[] BgHOfs = new short[4];
	public short[] BgVOfs = new short[4];
	public short[] BgPA = new short[2];
	public short[] BgPB = new short[2];
	public short[] BgPC = new short[2];
	public short[] BgPD = new short[2];
	public int[] BgX = new int[2];
	public int[] BgY = new int[2];
	public int[] BgRefX = new int[2];
	public int[] BgRefY = new int[2];

	public ushort BldCnt;
	public ushort BldAlpha;
	public ushort BldY;

	private ushort _win0H;
	private ushort _win0V;
	private ushort _win1H;
	private ushort _win1V;

	public ushort Win0H
	{
		get => _win0H;
		set => _win0H = CleanWindowRange( value, GbaConstants.ScreenWidth );
	}

	public ushort Win0V
	{
		get => _win0V;
		set => _win0V = CleanWindowRange( value, GbaConstants.ScreenHeight );
	}

	public ushort Win1H
	{
		get => _win1H;
		set => _win1H = CleanWindowRange( value, GbaConstants.ScreenWidth );
	}

	public ushort Win1V
	{
		get => _win1V;
		set => _win1V = CleanWindowRange( value, GbaConstants.ScreenHeight );
	}

	public ushort WinIn, WinOut;
	public ushort Mosaic;

	internal int _firstAffine = -1;
	internal int _lastDrawnY = -1;
	internal int[] _enabledAtY = [int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue];
	internal bool[] _wasFullyEnabled = new bool[4];
	internal uint[] _oldCharBase = new uint[2];
	internal int[] _oldCharBaseFirstY = new int[2];

	internal bool _oamDirty = true;
	internal int _oamBatchOffset;
	internal int _oamMax;

	public GbaVideo( Gba gba )
	{
		Gba = gba;
	}

	private static ushort CleanWindowRange( ushort value, int limit )
	{
		int start = value >> 8;
		int end = value & 0xFF;

		if ( start > limit && start > end )
			start = 0;

		if ( end > limit )
		{
			end = limit;
			if ( start > limit )
				start = limit;
		}

		return (ushort)(end | (start << 8));
	}

	public void Reset()
	{
		VCount = 0;
		Dot = 0;
		DispCnt = 0;
		DispStat = 0;
		Array.Clear( BgCnt );
		Array.Clear( BgHOfs );
		Array.Clear( BgVOfs );
		BgPA[0] = 0x100; BgPA[1] = 0x100;
		Array.Clear( BgPB );
		Array.Clear( BgPC );
		BgPD[0] = 0x100; BgPD[1] = 0x100;
		Array.Clear( BgX );
		Array.Clear( BgY );
		Array.Clear( BgRefX );
		Array.Clear( BgRefY );
		_win0H = 0;
		_win0V = 0;
		_win1H = 0;
		_win1V = 0;

		_firstAffine = -1;
		_lastDrawnY = -1;
		for ( int i = 0; i < 4; i++ ) _enabledAtY[i] = int.MaxValue;
		Array.Clear( _wasFullyEnabled );
		_oldCharBase[0] = 0; _oldCharBase[1] = 0;
		_oldCharBaseFirstY[0] = 0; _oldCharBaseFirstY[1] = 0;

		_oamDirty = true;
		_oamBatchOffset = 0;
		_oamMax = 0;
	}

	public void WriteDispCnt( ushort value )
	{
		value &= 0xFFF7;
		ushort oldVal = DispCnt;
		DispCnt = value;

		for ( int i = 0; i < 4; i++ )
		{
			bool wasEnabled = (oldVal & (0x100 << i)) != 0;
			bool isEnabled = (value & (0x100 << i)) != 0;

			if ( !isEnabled )
			{
				if ( _enabledAtY[i] < int.MaxValue )
					_wasFullyEnabled[i] = _lastDrawnY >= 0 && _enabledAtY[i] <= _lastDrawnY;
				_enabledAtY[i] = int.MaxValue;
			}
			else if ( _enabledAtY[i] == int.MaxValue && isEnabled )
			{
				if ( _lastDrawnY < 0 )
				{
					_enabledAtY[i] = 0;
				}
				else if ( _wasFullyEnabled[i] )
				{
					_enabledAtY[i] = 0;
				}
				else
				{
					int mode = value & 7;
					_enabledAtY[i] = mode > 2 ? _lastDrawnY + 3 : _lastDrawnY + 4;
				}
				_wasFullyEnabled[i] = false;
			}
		}
	}

	public void WriteBgCnt( int bg, ushort value )
	{
		ushort oldVal = BgCnt[bg];
		BgCnt[bg] = value;

		if ( bg >= 2 )
		{
			int idx = bg - 2;
			uint oldCB = (uint)((oldVal >> 2) & 3) * 0x4000u;
			uint newCB = (uint)((value >> 2) & 3) * 0x4000u;
			if ( oldCB != newCB )
			{
				_oldCharBase[idx] = oldCB;
				_oldCharBaseFirstY[idx] = VCount;
			}
		}
	}

	public void StartHBlank()
	{
		if ( VCount < GbaConstants.VisibleLines )
		{
			int bgMode = DispCnt & 7;
			if ( bgMode != 0 )
			{
				if ( _firstAffine < 0 )
					_firstAffine = VCount;
			}
			else
			{
				_firstAffine = -1;
			}

			_lastDrawnY = VCount;
			CaptureScanline( VCount );
		}

		DispStat |= 0x0002;

		if ( VCount < GbaConstants.VisibleLines )
			Gba.Dma.OnHBlank();

		if ( VCount >= 2 && VCount < GbaConstants.VisibleLines + 2 )
			Gba.Dma.OnDisplayStart();

		if ( (DispStat & 0x0010) != 0 )
			Gba.Io.RaiseIrq( GbaIrq.HBlank, -6 );

		if ( VCount < GbaConstants.VisibleLines )
		{
			int affMode = DispCnt & 7;
			if ( affMode >= 1 )
			{
				if ( _enabledAtY[2] <= VCount )
				{
					BgX[0] += BgPB[0];
					BgY[0] += BgPD[0];
				}
				if ( _enabledAtY[3] <= VCount )
				{
					BgX[1] += BgPB[1];
					BgY[1] += BgPD[1];
				}
			}
		}
	}

	public void StartHDraw()
	{
		DispStat &= unchecked((ushort)~0x0002);
		VCount++;

		if ( VCount == GbaConstants.VisibleLines )
		{
			DispStat |= 0x0001;
			if ( (DispStat & 0x0008) != 0 )
				Gba.Io.RaiseIrq( GbaIrq.VBlank );
			Gba.Dma.OnVBlank();
			Gba.Io.WirelessAdapter.FrameUpdate();

			SnapshotVram();
			CommitFrame();

			_oamDirty = true;
			_oamBatchOffset = 0;
			_oamMax = 0;

			BgX[0] = BgRefX[0];
			BgY[0] = BgRefY[0];
			BgX[1] = BgRefX[1];
			BgY[1] = BgRefY[1];

			_firstAffine = -1;
			_lastDrawnY = -1;
			for ( int i = 0; i < 4; i++ )
			{
				if ( _enabledAtY[i] < int.MaxValue )
					_enabledAtY[i] = 0;
			}
			_oldCharBase[0] = (uint)((BgCnt[2] >> 2) & 3) * 0x4000u;
			_oldCharBase[1] = (uint)((BgCnt[3] >> 2) & 3) * 0x4000u;
			_oldCharBaseFirstY[0] = 0;
			_oldCharBaseFirstY[1] = 0;
		}
		else if ( VCount == GbaConstants.VideoVerticalTotalPixels )
		{
			VCount = 0;
		}

		if ( VCount == GbaConstants.VideoVerticalTotalPixels - 1 )
		{
			DispStat &= unchecked((ushort)~0x0001);
		}

		int lyc = (DispStat >> 8) & 0xFF;
		if ( VCount == lyc )
		{
			DispStat |= 0x0004;
			if ( (DispStat & 0x0020) != 0 )
				Gba.Io.RaiseIrq( GbaIrq.VCounter );
		}
		else
		{
			DispStat &= unchecked((ushort)~0x0004);
		}
	}
}
