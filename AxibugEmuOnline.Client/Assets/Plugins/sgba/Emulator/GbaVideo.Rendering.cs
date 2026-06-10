using System.Runtime.InteropServices;
using System.Threading;
using Sandbox.Rendering;

namespace sGBA;

public partial class GbaVideo
{
	[StructLayout( LayoutKind.Sequential )]
	public struct ScanlineState
	{
		public uint DispCntMosaic;
		public uint BgCnt01;
		public uint BgCnt23;
		public uint BgOffset0, BgOffset1, BgOffset2, BgOffset3;
		public int Bg2PA, Bg2PC;
		public int Bg2X, Bg2Y;
		public int Bg3PA, Bg3PC;
		public int Bg3X, Bg3Y;
		public uint BldCntAlpha;
		public uint BldYWin0H;
		public uint Win0VWin1H;
		public uint Win1VWinIn;
		public uint WinOutPad;
		public int FirstAffine;
		public uint EnabledAtYMask;
		public uint OamState;
	}

	[StructLayout( LayoutKind.Sequential )]
	public struct GpuSprite
	{
		public int X, Y;
		public int Width, Height;
		public int RenderWidth, RenderHeight;
		public uint CharBase;
		public uint Tile;
		public uint Stride;
		public uint Palette;
		public uint Flags;
		public int PA, PB, PC, PD;
		public int Cycles;
	}

	private readonly struct FrameSource
	{
		public FrameSource( Texture texture, Vector4 size )
		{
			Texture = texture;
			Size = size;
		}

		public Texture Texture { get; }
		public Vector4 Size { get; }
	}

	public int GpuScale { get; private set; }
	public Texture OutputTexture { get; private set; }
	public CommandList RenderCommandList { get; private set; }
	public bool GpuReady { get; private set; }
	public bool ReproduceClassicFeel { get; private set; } = true;

	private int _scaledWidth;
	private int _scaledHeight;
	private const int HistoryFrameCount = 7;
	private const float ResponseTimeStrength = 0.333f;

	private GpuBuffer<ScanlineState> _gpuScanlines;
	private GpuBuffer<GpuSprite> _gpuSprites;
	private GpuBuffer<uint> _gpuVram;
	private GpuBuffer<uint> _gpuPalette;

	private Texture _bg0Tex, _bg1Tex, _bg2Tex, _bg3Tex;
	private Texture _objColorTex, _objFlagsTex, _windowTex;
	private Texture _nativeFrameTex, _classicResponseTex, _classicLcdTex;
	private Texture[] _suspendPreviewTex;
	private int _previewWriteSlot;
	private Texture[] _originalHistoryTex;

	private ComputeShader _csBgMode0, _csBgMode2, _csBgMode3, _csBgMode4, _csBgMode5;
	private ComputeShader _csObj, _csWindow, _csFinalize;
	private ComputeShader _csResponseTime, _csLcdGridV2, _csGbaColor;

	private ScanlineState[][] _scanlineFrames;
	private uint[][] _paletteFrames;
	private GpuSprite[][] _spriteFrames;
	private int[] _frameOamTotal;
	private uint[][] _vramFrames;

	private const int FrameSlotCount = 3;
	private int _writeSlot;
	private int _readSlot = -1;
	private int _readySlot = -1;
	private int _frameReady;
	private bool _hasFrame;

	private const int MaxOamBatches = 8;
	private const int MaxOamEntries = 128 * MaxOamBatches;

	private uint[][] _frameOldCharBase;
	private int[][] _frameOldCharBaseFirstY;
	private int _historyHead = -1;
	private int _historyValidCount;
	private int _frameCount;

	public void SetReproduceClassicFeel( bool reproduceClassicFeel )
	{
		ReproduceClassicFeel = reproduceClassicFeel;
		ResetOriginalHistory();
	}

	private void ResetOriginalHistory()
	{
		_historyHead = -1;
		_historyValidCount = 0;
		_frameCount = 0;
	}

	public void InitGpu( int scale = 1 )
	{
		DisposeGpu();
		CreateFrameSnapshots();
		CreateGpuResources( scale );
	}

	public void DisposeGpu()
	{
		DisposeGpuResources();
		ClearFrameSnapshots();
	}

	private void CreateFrameSnapshots()
	{
		_scanlineFrames = new ScanlineState[FrameSlotCount][];
		_paletteFrames = new uint[FrameSlotCount][];
		_spriteFrames = new GpuSprite[FrameSlotCount][];
		_frameOamTotal = new int[FrameSlotCount];
		_vramFrames = new uint[FrameSlotCount][];
		_frameOldCharBase = new uint[FrameSlotCount][];
		_frameOldCharBaseFirstY = new int[FrameSlotCount][];
		for ( int i = 0; i < FrameSlotCount; i++ )
		{
			_scanlineFrames[i] = new ScanlineState[GbaConstants.VisibleLines];
			_paletteFrames[i] = new uint[256 * GbaConstants.VisibleLines];
			_spriteFrames[i] = new GpuSprite[MaxOamEntries];
			_vramFrames[i] = new uint[96 * 1024 / 4];
			_frameOldCharBase[i] = new uint[2];
			_frameOldCharBaseFirstY[i] = new int[2];
		}

		_writeSlot = 0;
		_readySlot = 1;
		_readSlot = 2;
		_frameReady = 0;
		_hasFrame = false;
	}

	private void ClearFrameSnapshots()
	{
		_scanlineFrames = null;
		_paletteFrames = null;
		_spriteFrames = null;
		_frameOamTotal = null;
		_vramFrames = null;
		_frameOldCharBase = null;
		_frameOldCharBaseFirstY = null;
		_writeSlot = 0;
		_readySlot = -1;
		_readSlot = -1;
		_frameReady = 0;
		_hasFrame = false;
	}

	private void CreateGpuResources( int scale )
	{
		GpuScale = Math.Max( 1, scale );
		_scaledWidth = GbaConstants.ScreenWidth * GpuScale;
		_scaledHeight = GbaConstants.ScreenHeight * GpuScale;

		_gpuScanlines = new GpuBuffer<ScanlineState>( GbaConstants.VisibleLines );
		_gpuSprites = new GpuBuffer<GpuSprite>( MaxOamEntries );
		_gpuVram = new GpuBuffer<uint>( 96 * 1024 / 4 );
		_gpuPalette = new GpuBuffer<uint>( 256 * GbaConstants.VisibleLines );

		_bg0Tex = CreateNativeColorRT( gpuOnly: true );
		_bg1Tex = CreateNativeColorRT( gpuOnly: true );
		_bg2Tex = CreateNativeColorRT( gpuOnly: true );
		_bg3Tex = CreateNativeColorRT( gpuOnly: true );
		_objColorTex = CreateNativeColorRT( gpuOnly: true );
		_objFlagsTex = CreateNativeUintRT( gpuOnly: true );
		_windowTex = CreateNativeUintRT( gpuOnly: true );
		_nativeFrameTex = CreateNativeColorRT( ImageFormat.RGBA16161616F );
		_classicResponseTex = CreateNativeColorRT( ImageFormat.RGBA16161616F, gpuOnly: true );
		_classicLcdTex = CreateScaledColorRT( ImageFormat.RGBA16161616F, gpuOnly: true );
		_originalHistoryTex = new Texture[HistoryFrameCount];
		for ( int i = 0; i < HistoryFrameCount; i++ )
			_originalHistoryTex[i] = CreateNativeColorRT( ImageFormat.RGBA16161616F, gpuOnly: true );

		OutputTexture = CreateScaledColorRT( ImageFormat.RGBA8888, gpuOnly: true );
		_suspendPreviewTex = new Texture[2];
		_suspendPreviewTex[0] = CreateNativeColorRT( ImageFormat.RGBA8888 );
		_suspendPreviewTex[1] = CreateNativeColorRT( ImageFormat.RGBA8888 );
		_previewWriteSlot = 0;

		_csBgMode0 = new ComputeShader( "shaders/gba_bg_mode0.shader" );
		_csBgMode2 = new ComputeShader( "shaders/gba_bg_mode2.shader" );
		_csBgMode3 = new ComputeShader( "shaders/gba_bg_mode3.shader" );
		_csBgMode4 = new ComputeShader( "shaders/gba_bg_mode4.shader" );
		_csBgMode5 = new ComputeShader( "shaders/gba_bg_mode5.shader" );
		_csObj = new ComputeShader( "shaders/gba_obj.shader" );
		_csWindow = new ComputeShader( "shaders/gba_window.shader" );
		_csFinalize = new ComputeShader( "shaders/gba_finalize.shader" );
		_csResponseTime = new ComputeShader( "shaders/postprocess/motionblur/response_time.shader" );
		_csLcdGridV2 = new ComputeShader( "shaders/postprocess/handheld/lcd_cgwg/lcd_grid_v2.shader" );
		_csGbaColor = new ComputeShader( "shaders/postprocess/handheld/color/gba_color.shader" );

		ResetOriginalHistory();
		RenderCommandList = new CommandList( "GBA PPU" );
		GpuReady = true;
	}

	private void DisposeGpuResources()
	{
		GpuReady = false;
		_gpuScanlines?.Dispose();
		_gpuSprites?.Dispose();
		_gpuVram?.Dispose();
		_gpuPalette?.Dispose();
		_bg0Tex?.Dispose();
		_bg1Tex?.Dispose();
		_bg2Tex?.Dispose();
		_bg3Tex?.Dispose();
		_objColorTex?.Dispose();
		_objFlagsTex?.Dispose();
		_windowTex?.Dispose();
		_nativeFrameTex?.Dispose();
		_classicResponseTex?.Dispose();
		_classicLcdTex?.Dispose();
		if ( _originalHistoryTex != null )
		{
			for ( int i = 0; i < _originalHistoryTex.Length; i++ )
				_originalHistoryTex[i]?.Dispose();
		}
		OutputTexture?.Dispose();
		if ( _suspendPreviewTex != null )
		{
			for ( int i = 0; i < _suspendPreviewTex.Length; i++ )
				_suspendPreviewTex[i]?.Dispose();
		}
		_bg0Tex = null;
		_bg1Tex = null;
		_bg2Tex = null;
		_bg3Tex = null;
		_objColorTex = null;
		_objFlagsTex = null;
		_windowTex = null;
		_nativeFrameTex = null;
		_classicResponseTex = null;
		_classicLcdTex = null;
		_originalHistoryTex = null;
		OutputTexture = null;
		_suspendPreviewTex = null;
		_previewWriteSlot = 0;
		RenderCommandList = null;
	}

	private Texture CreateRenderTarget( int width, int height, ImageFormat format, bool gpuOnly = false )
	{
		TextureBuilder builder = Texture.CreateRenderTarget()
			.WithSize( width, height )
			.WithFormat( format )
			.WithUAVBinding();

		if ( gpuOnly )
			builder = builder.WithGPUOnlyUsage();

		return builder.Create();
	}

	private Texture CreateNativeColorRT( ImageFormat format = ImageFormat.RGBA8888, bool gpuOnly = false ) => CreateRenderTarget( GbaConstants.ScreenWidth, GbaConstants.ScreenHeight, format, gpuOnly );

	private Texture CreateScaledColorRT( ImageFormat format = ImageFormat.RGBA8888, bool gpuOnly = false ) => CreateRenderTarget( _scaledWidth, _scaledHeight, format, gpuOnly );

	private Texture CreateNativeUintRT( bool gpuOnly = false ) => CreateRenderTarget( GbaConstants.ScreenWidth, GbaConstants.ScreenHeight, ImageFormat.R32_UINT, gpuOnly );

	private void CaptureScanline( int y )
	{
		if ( !GpuReady ) return;

		var scanlines = _scanlineFrames[_writeSlot];
		ref var s = ref scanlines[y];

		s.DispCntMosaic = DispCnt | ((uint)Mosaic << 16);
		s.BgCnt01 = BgCnt[0] | ((uint)BgCnt[1] << 16);
		s.BgCnt23 = BgCnt[2] | ((uint)BgCnt[3] << 16);

		s.BgOffset0 = PackOffset( BgHOfs[0], BgVOfs[0] );
		s.BgOffset1 = PackOffset( BgHOfs[1], BgVOfs[1] );
		s.BgOffset2 = PackOffset( BgHOfs[2], BgVOfs[2] );
		s.BgOffset3 = PackOffset( BgHOfs[3], BgVOfs[3] );

		s.Bg2PA = BgPA[0];
		s.Bg2PC = BgPC[0];
		s.Bg2X = BgX[0];
		s.Bg2Y = BgY[0];
		s.Bg3PA = BgPA[1];
		s.Bg3PC = BgPC[1];
		s.Bg3X = BgX[1];
		s.Bg3Y = BgY[1];

		s.BldCntAlpha = BldCnt | ((uint)BldAlpha << 16);
		s.BldYWin0H = BldY | ((uint)Win0H << 16);
		s.Win0VWin1H = Win0V | ((uint)Win1H << 16);
		s.Win1VWinIn = Win1V | ((uint)WinIn << 16);
		s.WinOutPad = WinOut;

		s.FirstAffine = _firstAffine;

		uint enabledMask = 0;
		for ( int i = 0; i < 4; i++ )
		{
			if ( (DispCnt & (0x100 << i)) != 0 && y >= _enabledAtY[i] )
				enabledMask |= (1u << i);
		}
		s.EnabledAtYMask = enabledMask;

		var ram = Gba.Memory.PaletteRam;
		var palDst = _paletteFrames[_writeSlot];
		int baseIdx = y * 256;
		Buffer.BlockCopy( ram, 0, palDst, baseIdx * 4, 1024 );

		if ( _oamDirty )
		{
			int newOffset = _oamBatchOffset + _oamMax;
			if ( newOffset + 128 <= MaxOamEntries )
			{
				_oamBatchOffset = newOffset;
				_oamMax = CleanOam( newOffset );
			}
			_oamDirty = false;
		}
		s.OamState = (uint)_oamBatchOffset | ((uint)_oamMax << 16);
	}

	private int CleanOam( int offset )
	{
		if ( !GpuReady ) return 0;

		var sprites = _spriteFrames[_writeSlot];
		byte[] oam = Gba.Memory.Oam;
		bool mapping1D = (DispCnt & 0x40) != 0;
		int bgMode = DispCnt & 7;
		int count = 0;

		for ( int i = 0; i < 128 && count < 128; i++ )
		{
			int off = i * 8;
			ushort attr0 = (ushort)(oam[off] | (oam[off + 1] << 8));
			ushort attr1 = (ushort)(oam[off + 2] | (oam[off + 3] << 8));
			ushort attr2 = (ushort)(oam[off + 4] | (oam[off + 5] << 8));

			int objMode = (attr0 >> 10) & 3;
			if ( objMode == 3 ) continue;

			bool isAffine = (attr0 & 0x100) != 0;
			bool doubleSize = isAffine && (attr0 & 0x200) != 0;
			if ( !isAffine && (attr0 & 0x200) != 0 ) continue;

			int shape = (attr0 >> 14) & 3;
			int sizeParam = (attr1 >> 14) & 3;
			GetSpriteSize( shape, sizeParam, out int w, out int h );

			int sprY = attr0 & 0xFF;
			if ( sprY >= 160 ) sprY -= 256;
			int sprX = attr1 & 0x1FF;
			if ( sprX >= 240 ) sprX -= 512;

			int tileNum = attr2 & 0x3FF;
			bool is8bpp = (attr0 & 0x2000) != 0;

			if ( bgMode >= 3 && tileNum < 512 ) continue;

			int align = is8bpp && !mapping1D ? 1 : 0;
			uint charBase = (uint)((0x10000 >> 1) + ((tileNum & ~align) * 0x10));
			uint tile = 0;
			if ( !mapping1D )
			{
				if ( is8bpp )
					tile = (charBase >> 5) & 0xFu;
				else
					tile = (charBase >> 4) & 0x1Fu;
				charBase &= ~0x1FFu;
			}
			uint stride = mapping1D ? (uint)(w >> 3) : (uint)(0x20 >> (is8bpp ? 1 : 0));

			ref var spr = ref sprites[offset + count];
			spr.X = sprX;
			spr.Y = sprY;
			spr.Width = w;
			spr.Height = h;
			spr.RenderWidth = doubleSize ? w * 2 : w;
			spr.RenderHeight = doubleSize ? h * 2 : h;
			spr.CharBase = charBase;
			spr.Tile = tile;
			spr.Stride = stride;
			spr.Palette = (uint)((attr2 >> 12) & 0xF);

			bool flipH = !isAffine && (attr1 & 0x1000) != 0;
			bool flipV = !isAffine && (attr1 & 0x2000) != 0;
			bool isMosaic = (attr0 & 0x1000) != 0;
			int priority = (attr2 >> 10) & 3;

			spr.Flags = 0;
			if ( is8bpp ) spr.Flags |= 1u;
			if ( flipH ) spr.Flags |= 2u;
			if ( flipV ) spr.Flags |= 4u;
			if ( isAffine ) spr.Flags |= 8u;
			if ( doubleSize ) spr.Flags |= 16u;
			if ( isMosaic ) spr.Flags |= 64u;
			if ( objMode == 1 ) spr.Flags |= 128u;
			if ( objMode == 2 ) spr.Flags |= 256u;
			spr.Flags |= (uint)(priority << 10);

			int cycles;
			if ( isAffine )
			{
				int renderW = doubleSize ? w * 2 : w;
				cycles = 8 + renderW * 2;
				if ( sprX < 0 )
					cycles += sprX;
			}
			else
			{
				cycles = w - 2;
				if ( sprX < 0 )
				{
					if ( sprX + w < 0 ) continue;
					cycles += sprX >> 1;
				}
			}
			spr.Cycles = cycles;

			if ( isAffine )
			{
				int affIdx = (attr1 >> 9) & 0x1F;
				int paOff = affIdx * 32 + 6;
				int pbOff = affIdx * 32 + 14;
				int pcOff = affIdx * 32 + 22;
				int pdOff = affIdx * 32 + 30;
				spr.PA = (short)(oam[paOff] | (oam[paOff + 1] << 8));
				spr.PB = (short)(oam[pbOff] | (oam[pbOff + 1] << 8));
				spr.PC = (short)(oam[pcOff] | (oam[pcOff + 1] << 8));
				spr.PD = (short)(oam[pdOff] | (oam[pdOff + 1] << 8));
			}
			else
			{
				spr.PA = flipH ? -256 : 256;
				spr.PB = 0;
				spr.PC = 0;
				spr.PD = flipV ? -256 : 256;
			}

			count++;
		}

		return count;
	}

	private void SnapshotVram()
	{
		if ( !GpuReady ) return;
		Buffer.BlockCopy( Gba.Memory.Vram, 0, _vramFrames[_writeSlot], 0, Gba.Memory.Vram.Length );
	}

	private void CommitFrame()
	{
		if ( !GpuReady ) return;
		Array.Copy( _oldCharBase, _frameOldCharBase[_writeSlot], 2 );
		Array.Copy( _oldCharBaseFirstY, _frameOldCharBaseFirstY[_writeSlot], 2 );
		_frameOamTotal[_writeSlot] = _oamBatchOffset + _oamMax;
		_writeSlot = Interlocked.Exchange( ref _readySlot, _writeSlot );
		Interlocked.Exchange( ref _frameReady, 1 );
	}

	private void DispatchWindowPass( CommandList cmd, Vector3 circle0, Vector3 circle1 )
	{
		cmd.Attributes.Set( "OutputMask", _windowTex );
		cmd.Attributes.Set( "Circle0", circle0 );
		cmd.Attributes.Set( "Circle1", circle1 );
		cmd.DispatchCompute( _csWindow, GbaConstants.ScreenWidth, GbaConstants.ScreenHeight, 1 );
		cmd.UavBarrier( _windowTex );
	}

	private void DispatchObjPass( CommandList cmd )
	{
		cmd.Attributes.Set( "Sprites", _gpuSprites );
		cmd.Attributes.Set( "OutputColor", _objColorTex );
		cmd.Attributes.Set( "OutputFlags", _objFlagsTex );
		cmd.Attributes.Set( "WindowTex", _windowTex );
		cmd.DispatchCompute( _csObj, GbaConstants.ScreenWidth, GbaConstants.ScreenHeight, 1 );

		cmd.UavBarrier( _objColorTex );
		cmd.UavBarrier( _objFlagsTex );
		cmd.UavBarrier( _windowTex );
	}

	private void DispatchOutputPasses( CommandList cmd )
	{
		Vector4 nativeSize = CreateSizeVector( GbaConstants.ScreenWidth, GbaConstants.ScreenHeight );
		Vector4 scaledSize = CreateSizeVector( _scaledWidth, _scaledHeight );

		FrameSource displaySource = new( _nativeFrameTex, nativeSize );
		FrameSource previewSource = displaySource;

		if ( ReproduceClassicFeel )
		{
			FrameSource responseSource = DispatchClassicResponsePass( cmd, displaySource, nativeSize );
			displaySource = DispatchClassicLcdPass( cmd, responseSource, scaledSize );
			previewSource = responseSource;
			_frameCount++;
		}

		DispatchGbaColorPass( cmd, displaySource, OutputTexture, scaledSize, _scaledWidth, _scaledHeight );
		cmd.UavBarrier( OutputTexture );

		Texture preview = _suspendPreviewTex[_previewWriteSlot];
		DispatchGbaColorPass( cmd, previewSource, preview, nativeSize, GbaConstants.ScreenWidth, GbaConstants.ScreenHeight );
		cmd.UavBarrier( preview );

		_previewWriteSlot ^= 1;
	}

	private FrameSource DispatchClassicResponsePass( CommandList cmd, FrameSource source, Vector4 nativeSize )
	{
		cmd.Attributes.Set( "SourceSize", source.Size );
		cmd.Attributes.Set( "OriginalSize", nativeSize );
		cmd.Attributes.Set( "OutputSize", nativeSize );
		cmd.Attributes.Set( "Source", source.Texture );
		BindOriginalHistoryTextures( cmd, source.Texture );
		cmd.Attributes.Set( "OutputTex", _classicResponseTex );
		cmd.Attributes.Set( "response_time", ResponseTimeStrength );
		cmd.DispatchCompute( _csResponseTime, GbaConstants.ScreenWidth, GbaConstants.ScreenHeight, 1 );
		cmd.UavBarrier( _classicResponseTex );

		return new FrameSource( _classicResponseTex, nativeSize );
	}

	private FrameSource DispatchClassicLcdPass( CommandList cmd, FrameSource source, Vector4 scaledSize )
	{
		cmd.Attributes.Set( "SourceSize", source.Size );
		cmd.Attributes.Set( "OriginalSize", CreateSizeVector( GbaConstants.ScreenWidth, GbaConstants.ScreenHeight ) );
		cmd.Attributes.Set( "OutputSize", scaledSize );
		cmd.Attributes.Set( "Source", source.Texture );
		cmd.Attributes.Set( "OutputTex", _classicLcdTex );
		cmd.Attributes.Set( "RSUBPIX_R", 1.0f );
		cmd.Attributes.Set( "RSUBPIX_G", 0.0f );
		cmd.Attributes.Set( "RSUBPIX_B", 0.0f );
		cmd.Attributes.Set( "GSUBPIX_R", 0.0f );
		cmd.Attributes.Set( "GSUBPIX_G", 1.0f );
		cmd.Attributes.Set( "GSUBPIX_B", 0.0f );
		cmd.Attributes.Set( "BSUBPIX_R", 0.0f );
		cmd.Attributes.Set( "BSUBPIX_G", 0.0f );
		cmd.Attributes.Set( "BSUBPIX_B", 1.0f );
		cmd.Attributes.Set( "gain", 1.0f );
		cmd.Attributes.Set( "gamma", 3.0f );
		cmd.Attributes.Set( "blacklevel", 0.05f );
		cmd.Attributes.Set( "ambient", 0.0f );
		cmd.Attributes.Set( "BGR", 0.0f );
		cmd.DispatchCompute( _csLcdGridV2, _scaledWidth, _scaledHeight, 1 );
		cmd.UavBarrier( _classicLcdTex );

		return new FrameSource( _classicLcdTex, scaledSize );
	}

	private void DispatchGbaColorPass( CommandList cmd, FrameSource source, Texture output, Vector4 outputSize, int dispatchWidth, int dispatchHeight )
	{
		cmd.Attributes.Set( "SourceSize", source.Size );
		cmd.Attributes.Set( "OriginalSize", CreateSizeVector( GbaConstants.ScreenWidth, GbaConstants.ScreenHeight ) );
		cmd.Attributes.Set( "OutputSize", outputSize );
		cmd.Attributes.Set( "Source", source.Texture );
		cmd.Attributes.Set( "OutputTex", output );
		cmd.Attributes.Set( "mode", 1.0f );
		cmd.Attributes.Set( "darken_screen", 0.8f );
		cmd.DispatchCompute( _csGbaColor, dispatchWidth, dispatchHeight, 1 );
	}

	private void UpdateOriginalHistory()
	{
		if ( !ReproduceClassicFeel || _frameCount <= 0 || _nativeFrameTex == null || _originalHistoryTex == null )
			return;

		int nextHistoryHead = (_historyHead + 1) % HistoryFrameCount;
		Texture historyTexture = _originalHistoryTex[nextHistoryHead];
		if ( historyTexture == null )
			return;

		Graphics.CopyTexture( _nativeFrameTex, historyTexture );
		_historyHead = nextHistoryHead;
		if ( _historyValidCount < HistoryFrameCount )
			_historyValidCount++;
	}

	private void BindOriginalHistoryTextures( CommandList cmd, Texture currentSource )
	{
		cmd.Attributes.Set( "OriginalHistory1", GetOriginalHistoryTexture( 1, currentSource ) );
		cmd.Attributes.Set( "OriginalHistory2", GetOriginalHistoryTexture( 2, currentSource ) );
		cmd.Attributes.Set( "OriginalHistory3", GetOriginalHistoryTexture( 3, currentSource ) );
		cmd.Attributes.Set( "OriginalHistory4", GetOriginalHistoryTexture( 4, currentSource ) );
		cmd.Attributes.Set( "OriginalHistory5", GetOriginalHistoryTexture( 5, currentSource ) );
		cmd.Attributes.Set( "OriginalHistory6", GetOriginalHistoryTexture( 6, currentSource ) );
		cmd.Attributes.Set( "OriginalHistory7", GetOriginalHistoryTexture( 7, currentSource ) );
	}

	private Texture GetOriginalHistoryTexture( int age, Texture currentSource )
	{
		if ( _originalHistoryTex == null || _historyHead < 0 || age > _historyValidCount )
			return currentSource;

		int index = _historyHead - age + 1;
		while ( index < 0 )
			index += HistoryFrameCount;

		return _originalHistoryTex[index];
	}

	private static Vector4 CreateSizeVector( int width, int height ) => new Vector4( width, height, 1.0f / width, 1.0f / height );

	public bool UploadAndBuildCommandList()
	{
		if ( !GpuReady || RenderCommandList == null || OutputTexture == null )
			return false;

		if ( _scanlineFrames == null || _paletteFrames == null || _spriteFrames == null || _frameOamTotal == null || _vramFrames == null )
			return false;

		if ( _gpuScanlines == null || _gpuSprites == null || _gpuVram == null || _gpuPalette == null )
			return false;

		if ( Interlocked.Exchange( ref _frameReady, 0 ) == 1 )
		{
			_readSlot = Interlocked.Exchange( ref _readySlot, _readSlot );
			_hasFrame = true;
		}

		if ( !_hasFrame ) return false;

		int slot = _readSlot;
		if ( slot < 0 ) return false;

		var scanlines = _scanlineFrames[slot];

		_gpuScanlines.SetData( scanlines );
		int totalSprites = _frameOamTotal[slot];
		_gpuSprites.SetData( _spriteFrames[slot].AsSpan( 0, Math.Max( 1, totalSprites ) ) );
		_gpuVram.SetData( _vramFrames[slot] );
		_gpuPalette.SetData( _paletteFrames[slot] );

		uint modesMask = 0;
		for ( int y = 0; y < GbaConstants.VisibleLines; y++ )
			modesMask |= 1u << (int)(scanlines[y].DispCntMosaic & 7);

		DetectCircle( scanlines, 0, out var circle0 );
		DetectCircle( scanlines, 1, out var circle1 );

		var cmd = RenderCommandList;
		UpdateOriginalHistory();
		cmd.Reset();

		cmd.Attributes.Set( "ScanlineStates", _gpuScanlines );
		cmd.Attributes.Set( "Vram", _gpuVram );
		cmd.Attributes.Set( "Palette", _gpuPalette );
		cmd.Attributes.Set( "Scale", 1 );

		DispatchWindowPass( cmd, circle0, circle1 );
		DispatchObjPass( cmd );

		cmd.Attributes.Set( "OldCharBase2", new Vector2( _frameOldCharBase[slot][0], _frameOldCharBaseFirstY[slot][0] ) );
		cmd.Attributes.Set( "OldCharBase3", new Vector2( _frameOldCharBase[slot][1], _frameOldCharBaseFirstY[slot][1] ) );

		Texture[] bgTex = [_bg0Tex, _bg1Tex, _bg2Tex, _bg3Tex];
		for ( int bg = 0; bg < 4; bg++ )
		{
			cmd.Attributes.Set( "BgIndex", bg );
			cmd.Attributes.Set( "OutputTex", bgTex[bg] );

			var shaders = GetBgShaders( bg, modesMask );
			bool first = true;
			foreach ( var shader in shaders )
			{
				cmd.Attributes.Set( "IsBasePass", first ? 1 : 0 );
				cmd.DispatchCompute( shader, GbaConstants.ScreenWidth, GbaConstants.ScreenHeight, 1 );
				if ( shaders.Count > 1 )
					cmd.UavBarrier( bgTex[bg] );
				first = false;
			}
		}

		cmd.UavBarrier( _bg0Tex );
		cmd.UavBarrier( _bg1Tex );
		cmd.UavBarrier( _bg2Tex );
		cmd.UavBarrier( _bg3Tex );

		cmd.Attributes.Set( "Bg0Tex", _bg0Tex );
		cmd.Attributes.Set( "Bg1Tex", _bg1Tex );
		cmd.Attributes.Set( "Bg2Tex", _bg2Tex );
		cmd.Attributes.Set( "Bg3Tex", _bg3Tex );
		cmd.Attributes.Set( "ObjColorTex", _objColorTex );
		cmd.Attributes.Set( "ObjFlagsTex", _objFlagsTex );
		cmd.Attributes.Set( "WindowTex", _windowTex );
		cmd.Attributes.Set( "OutputTex", _nativeFrameTex );
		cmd.DispatchCompute( _csFinalize, GbaConstants.ScreenWidth, GbaConstants.ScreenHeight, 1 );
		cmd.UavBarrier( _nativeFrameTex );
		DispatchOutputPasses( cmd );

		return true;
	}

	private List<ComputeShader> GetBgShaders( int bg, uint modesMask )
	{
		var result = new List<ComputeShader>( 2 );

		switch ( bg )
		{
			case 0:
			case 1:
				if ( (modesMask & 0b11) != 0 )
					result.Add( _csBgMode0 );
				break;

			case 2:
				if ( (modesMask & 0b001) != 0 ) result.Add( _csBgMode0 );
				if ( (modesMask & 0b110) != 0 ) result.Add( _csBgMode2 );
				if ( (modesMask & 0b001000) != 0 ) result.Add( _csBgMode3 );
				if ( (modesMask & 0b010000) != 0 ) result.Add( _csBgMode4 );
				if ( (modesMask & 0b100000) != 0 ) result.Add( _csBgMode5 );
				break;

			case 3:
				if ( (modesMask & 0b001) != 0 ) result.Add( _csBgMode0 );
				if ( (modesMask & 0b100) != 0 ) result.Add( _csBgMode2 );
				break;
		}

		return result;
	}

	private void DetectCircle( ScanlineState[] states, int window, out Vector3 result )
	{
		result = Vector3.Zero;
		if ( GpuScale < 2 ) return;

		int firstY = 0;
		int lastStartX = 0;
		int lastEndX = 0;
		int startX = 0;
		int endX = 0;

		int circleFirstY = -1;
		float centerX = -1;
		float centerY = -1;
		float radius = 0;
		bool invalid = false;

		for ( int y = firstY; y < GbaConstants.VisibleLines; y++ )
		{
			ref var s = ref states[y];

			uint winH, winV;
			if ( window == 0 )
			{
				winH = (s.BldYWin0H >> 16) & 0xFFFFu;
				winV = s.Win0VWin1H & 0xFFFFu;
			}
			else
			{
				winH = (s.Win0VWin1H >> 16) & 0xFFFFu;
				winV = s.Win1VWinIn & 0xFFFFu;
			}

			lastStartX = startX;
			lastEndX = endX;
			startX = (int)((winH >> 8) & 0xFF);
			endX = (int)(winH & 0xFF);
			int startY = (int)((winV >> 8) & 0xFF);
			int endY = (int)(winV & 0xFF);

			if ( startX == endX || y < startY || y >= endY )
			{
				if ( circleFirstY >= 0 )
				{
					centerY = (circleFirstY + y) / 2.0f;
					circleFirstY = -1;
				}
				continue;
			}
			if ( lastEndX - lastStartX <= 0 ) continue;

			if ( startX >= 240 ) { invalid = true; break; }

			int startDiff = lastStartX - startX;
			int endDiff = endX - lastEndX;
			if ( startDiff - endDiff < -1 || startDiff - endDiff > 1 )
			{
				invalid = true; break;
			}

			if ( startX < lastStartX )
			{
				centerX = (startX + endX) / 2.0f;
				if ( radius > 0 ) { invalid = true; break; }
			}
			else if ( startX > lastStartX && radius <= 0 )
			{
				radius = (lastEndX - lastStartX) / 2.0f;
			}

			if ( circleFirstY < 0 && y - 1 >= startY && y - 1 < endY )
			{
				circleFirstY = y - 1;
			}
		}

		if ( radius <= 0 ) invalid = true;
		if ( centerX < 0 ) invalid = true;
		if ( centerY < 0 ) invalid = true;

		for ( int y = firstY; y < GbaConstants.VisibleLines && !invalid; y++ )
		{
			ref var s = ref states[y];
			uint winH, winV;
			if ( window == 0 )
			{
				winH = (s.BldYWin0H >> 16) & 0xFFFFu;
				winV = s.Win0VWin1H & 0xFFFFu;
			}
			else
			{
				winH = (s.Win0VWin1H >> 16) & 0xFFFFu;
				winV = s.Win1VWinIn & 0xFFFFu;
			}

			int sx = (int)((winH >> 8) & 0xFF);
			int ex = (int)(winH & 0xFF);
			int sy = (int)((winV >> 8) & 0xFF);
			int ey = (int)(winV & 0xFF);
			bool xActive = sx < ex;
			bool yActive = y >= sy && y < ey;

			if ( xActive && yActive )
			{
				if ( centerY - y > radius ) { invalid = true; break; }
				if ( y - centerY > radius ) { invalid = true; break; }

				float cosine = MathF.Abs( y - centerY );
				float sine = MathF.Sqrt( radius * radius - cosine * cosine );
				if ( MathF.Abs( centerX - sine - sx ) <= 1 && MathF.Abs( centerX + sine - ex ) <= 1 )
					continue;

				if ( radius >= cosine + 1 )
				{
					float sine2 = MathF.Sqrt( radius * radius - (cosine + 1) * (cosine + 1) );
					if ( MathF.Abs( centerX - sine2 - sx ) <= 1 && MathF.Abs( centerX + sine2 - ex ) <= 1 )
						continue;
				}
				invalid = true;
			}
			else if ( centerY - y < radius && y - centerY < radius )
			{
				invalid = true;
			}
		}

		if ( !invalid )
			result = new Vector3( centerX, centerY, radius - 0.499f );
	}

	public byte[] CaptureScreenshot()
	{
		if ( _suspendPreviewTex == null ) return null;

		var pixels = _suspendPreviewTex[_previewWriteSlot].GetPixels();
		var result = new byte[GbaConstants.ScreenWidth * GbaConstants.ScreenHeight * 4];

		for ( int i = 0; i < pixels.Length && i * 4 + 3 < result.Length; i++ )
		{
			result[i * 4] = pixels[i].r;
			result[i * 4 + 1] = pixels[i].g;
			result[i * 4 + 2] = pixels[i].b;
			result[i * 4 + 3] = pixels[i].a;
		}

		return result;
	}

	public void CaptureScreenshotAsync( Action<byte[]> callback )
	{
		if ( _suspendPreviewTex == null )
		{
			callback?.Invoke( null );
			return;
		}

		_suspendPreviewTex[_previewWriteSlot].GetPixelsAsync<byte>( span =>
		{
			int expected = GbaConstants.ScreenWidth * GbaConstants.ScreenHeight * 4;
			var result = new byte[expected];
			int n = Math.Min( span.Length, expected );
			span.Slice( 0, n ).CopyTo( result );
			callback?.Invoke( result );
		}, ImageFormat.RGBA8888, (0, 0, GbaConstants.ScreenWidth, GbaConstants.ScreenHeight) );
	}

	private static uint PackOffset( short h, short v )
	{
		return (uint)(h & 0x1FF) | (uint)((v & 0x1FF) << 16);
	}

	private static void GetSpriteSize( int shape, int size, out int w, out int h )
	{
		switch ( shape )
		{
			case 0:
				w = h = 8 << size;
				break;
			case 1:
				(w, h) = size switch
				{
					0 => (16, 8),
					1 => (32, 8),
					2 => (32, 16),
					_ => (64, 32),
				};
				break;
			case 2:
				(w, h) = size switch
				{
					0 => (8, 16),
					1 => (8, 32),
					2 => (16, 32),
					_ => (32, 64),
				};
				break;
			default:
				w = h = 8;
				break;
		}
	}
}
